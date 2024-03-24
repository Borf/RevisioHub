using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RevisioHub.Client;
using System.Diagnostics;

public class Program
{
    public static Configuration Configuration { get; set; } = new();

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
        HubConnection connection = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .WithKeepAliveInterval(TimeSpan.FromSeconds(1))
            .WithUrl(Configuration.EndPoint, options =>
    {
            options.AccessTokenProvider = () => Task.FromResult(Configuration.Jwt);
        })
            .Build();
        connection.Closed += async (error) =>
        {
            Console.WriteLine("Disconnected from hub...");
            await Task.Delay(new Random().Next(0, 5) * 1000);
            Console.WriteLine("Reconnecting");
            await connection.StartAsync();
        };

        connection.On("Pong", () => Console.WriteLine("Pong received"));
        connection.On<string, string>("RunScript", RunScript);


        await connection.StartAsync();
        Console.WriteLine("Connected to server");
        await Task.Delay(-1);
    }


    private static async Task RunScript(string workingDirectory, string script)
    {
        int lineCount = script.Split("\n").Length;
        if(lineCount == 1)
        {
            ProcessStartInfo psi = new()
            { 
                WorkingDirectory = workingDirectory,
                FileName = script.Split(" ")[0],
            };
            psi.CreateNoWindow = false;
            psi.UseShellExecute = true;

            var process = Process.Start(psi);
        }
    }

}