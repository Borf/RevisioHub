using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevisioHub.Client;
using RevisioHub.Common.Models;
using RevisioHub.Common.Models.db;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Schema;

public class Program
{
    public static Host HostInfo { get; set; } = new();
    public static Configuration Configuration { get; set; } = new();
    public static CancellationTokenSource QuitTokenSource { get; set; } = new();
    public static HubConnection connection { get; set; } = null!;
    private static async Task Main(string[] args)
    {
        var rawConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
        rawConfig.Bind(Program.Configuration);

        Console.WriteLine("Revisiohub client");
        Console.WriteLine("-----------------");
        connection = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            })
            .WithKeepAliveInterval(TimeSpan.FromSeconds(1))
            .ConfigureLogging(options =>
            {
                options.AddConsole();
                options.SetMinimumLevel(LogLevel.Warning);
            })
            .WithUrl(Configuration.EndPoint, options =>
    {
            options.AccessTokenProvider = () => Task.FromResult<string?>(Configuration.Jwt);
        })
            .Build();
        Console.WriteLine("Built Connection");

        connection.Closed += async (error) =>
        {
            Console.WriteLine("Disconnected from hub...");
            await Task.Delay(new Random().Next(0, 5) * 1000);
            Console.WriteLine("Reconnecting");
            await connection.StartAsync();
        };

        connection.On<Host>("HostInfo", OnHostInfo);
        connection.On("Pong", () => Console.WriteLine("Pong received"));
        connection.On<RunConfig>("RunScript", OnRunScript);
        connection.On("Restart", OnRestart);
        connection.On<int>("Log", OnLog);
        connection.On<int, int>("StartUpdate", StartUpdate);


        Console.WriteLine("Connecting to server");
        await connection.StartAsync();
        Console.WriteLine("Connected to server");

        await Task.Delay(TimeSpan.FromSeconds(1));
        _ = Task.Run(WatchDog);

        await Task.Delay(-1);
    }

    private static async Task OnLog(int serviceHostId)
    {
        var service = HostInfo.ServiceHosts.First(h => h.Id == serviceHostId);
        var logScript = service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Logs && sc.HostType == HostInfo.HostType) ?? service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Logs && sc.HostType == HostType.Generic);
        string log = await RunScript(service, logScript!);
        await connection.SendAsync("OnLog", serviceHostId, log);
    }


    private static async Task StartUpdate(int serviceHostId, int LogId)
    {
        Console.WriteLine("Updating service...");
        var service = HostInfo.ServiceHosts.First(h => h.Id == serviceHostId);
        var updateScript = service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Update && sc.HostType == HostInfo.HostType) ?? service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Update && sc.HostType == HostType.Generic);
        await RunScript(service, updateScript!, connection, LogId, "UpdateUpdate");
        Console.WriteLine("Done Updating service...");
        var versionScript = service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Version && sc.HostType == HostInfo.HostType) ?? service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Version && sc.HostType == HostType.Generic);
        var version = await RunScript(service, versionScript!);
        await connection.SendAsync("UpdateVersion", LogId, version);

    }


    private static void OnRestart()
    {
        Environment.Exit(0);
    }

    private static async Task WatchDog()
    {
        Console.WriteLine("Starting watchdog");
        int versionCheck = 0;
        while(!QuitTokenSource.Token.IsCancellationRequested)
        {
            foreach(var service in HostInfo.ServiceHosts)
            {
                try
                {
                    var statusScript = service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Status && sc.HostType == HostInfo.HostType) ?? service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Status && sc.HostType == HostType.Generic);
                    if (statusScript == null)
                        continue;
                    //Console.WriteLine("getting status for " + service.Service.Name);
                    string status = await RunScript(service, statusScript);
                    //Console.WriteLine("Status: " + status);
                    await connection.SendAsync("Status", service.Id, new ServiceStatus(status));
                }catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                if(versionCheck == 0)
                {
                    try
                    {
                        var versionScript = service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Version && sc.HostType == HostInfo.HostType) ?? service.Service.ServiceScripts.FirstOrDefault(sc => sc.ScriptType == ScriptType.Version && sc.HostType == HostType.Generic);
                        if (versionScript == null)
                            continue;
                        //Console.WriteLine("getting version for " + service.Service.Name);
                        string version = await RunScript(service, versionScript);
                        await connection.SendAsync("Version", service.Id, version);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }
            }
            versionCheck = (versionCheck + 1) % 30;
            await Task.Delay(TimeSpan.FromSeconds(10), QuitTokenSource.Token);
        }
    }

    private static async Task OnHostInfo(Host hostInfo)
    {
        Console.WriteLine("Got host info: " + hostInfo.Name);
        HostInfo = hostInfo;
        await Task.Yield();
    }

    private static async Task OnRunScript(RunConfig runConfig)
    {
        string scriptFile = Path.Combine(runConfig.WorkingDirectory, "script");
        string command = runConfig.Command;
        if (HostInfo.HostType == HostType.WindowsService || HostInfo.HostType == HostType.WindowsConsole)
        {
            scriptFile = Path.Combine(runConfig.WorkingDirectory, "script.bat");
            command = "@echo off\n" + command;
        }
        await File.WriteAllTextAsync(scriptFile, command);
        if (HostInfo.HostType == HostType.Linux || HostInfo.HostType == HostType.DockerLinux)
            await Process.Start("chmod", "777 " + scriptFile).WaitForExitAsync();

        var psi = new ProcessStartInfo
        {
            FileName = scriptFile,
            WorkingDirectory = runConfig.WorkingDirectory,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        foreach (var var in runConfig.EnvironmentVariables)
            psi.Environment[var.Key] = var.Value;

        if (HostInfo.HostType == HostType.Linux || HostInfo.HostType == HostType.DockerLinux)
        {
            psi.FileName = "/bin/bash";
            psi.Arguments = scriptFile;
        }
        var process = Process.Start(psi);
    }


    private static async Task<string> RunScript(ServiceHost service, ServiceScript script)
    {
        string scriptFile = Path.Combine(service.WorkingDirectory, script.ScriptType.ToString());
        string command = script.Script;
        if (HostInfo.HostType == HostType.WindowsService || HostInfo.HostType == HostType.WindowsConsole)
        {
            scriptFile = Path.Combine(service.WorkingDirectory, script.ScriptType.ToString() + ".bat");
            command = "@echo off\n" + command;
        }

        await File.WriteAllTextAsync(scriptFile, command);
        if (HostInfo.HostType == HostType.Linux || HostInfo.HostType == HostType.DockerLinux)
            await Process.Start("chmod", "777 " + scriptFile).WaitForExitAsync();
        
        var psi = new ProcessStartInfo
        {
            FileName = scriptFile,
            WorkingDirectory = service.WorkingDirectory,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        foreach (var var in service.EnvironmentVariables)
            psi.Environment[var.Variable] = var.Value;

        if (HostInfo.HostType == HostType.Linux || HostInfo.HostType == HostType.DockerLinux)
        {
            psi.FileName = "/bin/bash";
            psi.Arguments = scriptFile;
        }



        var process = Process.Start(psi);
        if (process == null)
            return "Error";
        string output = "";
        while (!process.StandardOutput.EndOfStream)
            output += process.StandardOutput.ReadLine() + "\n";
        if (output.EndsWith("\n"))
            output = output.Substring(0, output.Length - 1);
        await process!.WaitForExitAsync();
        return output;
    }


    private static async Task RunScript(ServiceHost service, ServiceScript script, HubConnection connection, int id, string callback)
    {
        string scriptFile = Path.Combine(service.WorkingDirectory, script.ScriptType.ToString());
        string command = script.Script;
        if (HostInfo.HostType == HostType.WindowsService || HostInfo.HostType == HostType.WindowsConsole)
        {
            scriptFile = Path.Combine(service.WorkingDirectory, script.ScriptType.ToString() + ".bat");
            command = "@echo off\n" + command;
        }

        await File.WriteAllTextAsync(scriptFile, command);
        if (HostInfo.HostType == HostType.Linux || HostInfo.HostType == HostType.DockerLinux)
            await Process.Start("chmod", "777 " + scriptFile).WaitForExitAsync();

        var psi = new ProcessStartInfo
        {
            FileName = scriptFile,
            WorkingDirectory = service.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        foreach (var var in service.EnvironmentVariables)
            psi.Environment[var.Variable] = var.Value;

        if (HostInfo.HostType == HostType.Linux || HostInfo.HostType == HostType.DockerLinux)
        {
            psi.FileName = "/bin/bash";
            psi.Arguments = scriptFile;
        }



        var process = Process.Start(psi);
        if (process == null)
            return;
        string output = "";
        while (!process.StandardOutput.EndOfStream)
        {
            while (process.StandardOutput.Peek() != -1)
                output += process.StandardOutput.ReadLine() + "\n";
            while (process.StandardError.Peek() != -1)
                output += process.StandardError.ReadLine() + "\n";
            await connection.SendAsync(callback, id, output);
            await Task.Delay(1);
        }
        if (output.EndsWith("\n"))
            output = output.Substring(0, output.Length - 1);
        await connection.SendAsync(callback, id, output);
        await process!.WaitForExitAsync();
        return;
    }
}