using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using RevisioHub.Common.Models;
using RevisioHub.Web.Model.Db.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RevisioHub.Web.Services;

[Authorize]
public class ClientService : Hub
{
    public static ObservableCollection<string> Users { get; set; } = new();
    private IServiceProvider serviceProvider;
    private ServiceStatusService serviceStatus;
    private ServiceVersionService serviceVersion;
    private ServiceUpdateLogService serviceUpdateLog;
    private ServiceLogService serviceLog;

    private Dictionary<string, List<string>> Connections = new();

    public ClientService(IServiceProvider serviceProvider, ServiceStatusService serviceStatus, ServiceVersionService serviceVersion, ServiceLogService serviceLog, ServiceUpdateLogService serviceUpdateLog)
    {
        this.serviceProvider = serviceProvider;
        this.serviceStatus = serviceStatus;
        this.serviceVersion = serviceVersion;
        this.serviceLog = serviceLog;
        this.serviceUpdateLog = serviceUpdateLog;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Client " + Context.UserIdentifier + " connected");
        Users.Add(Context.UserIdentifier!);

        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<Context>();

        var host = context.Hosts
                .Include(h => h.ServiceHosts).ThenInclude(sh => sh.EnvironmentVariables)
                .Include(h => h.ServiceHosts).ThenInclude(sh => sh.Service).ThenInclude(s => s.ServiceScripts)
                .First(h => h.Name == Context.UserIdentifier);
        Console.WriteLine("Sending info on " + host.Name);

        if (!Connections.ContainsKey(Context.UserIdentifier!))
            Connections[Context.UserIdentifier!] = new();
        Connections[Context.UserIdentifier!].Add(Context.ConnectionId);

        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("HostInfo", host);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Client " + Context.UserIdentifier + " disconnected");

        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<Context>();

        var host = context.Hosts
                .Include(h => h.ServiceHosts).ThenInclude(sh => sh.EnvironmentVariables)
                .Include(h => h.ServiceHosts).ThenInclude(sh => sh.Service).ThenInclude(s => s.ServiceScripts)
                .First(h => h.Name == Context.UserIdentifier);

        foreach (var sh in host.ServiceHosts)
            serviceStatus[sh.Id] = new ServiceStatus() { Status = "Disconnected" };


        if (Connections.ContainsKey(Context.UserIdentifier!))
            if (Connections[Context.UserIdentifier!].Contains(Context.ConnectionId))
                Connections[Context.UserIdentifier!].Remove(Context.ConnectionId);

        Users.Remove(Context.UserIdentifier!);
        return base.OnDisconnectedAsync(exception);
    }

    public void Status(int serviceHostId, ServiceStatus status)
    {
        serviceStatus[serviceHostId] = status;
        Console.WriteLine("Status for " + serviceHostId + " is now " + status);
    }

    public void Version(int serviceHostId, string version)
    {
        serviceVersion[serviceHostId] = version;
        Console.WriteLine("Version is " + version);
    }

    public void OnLog(int serviceHostId, string data)
    {
        serviceLog[serviceHostId] = data;
    }

    public async void UpdateUpdate(int logId, string output)
    {
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<Context>();
        var log = context.UpdateLogs.Find(logId);
        log!.Log = output;
        await context.SaveChangesAsync();
        await serviceUpdateLog.UpdateLog(logId, output);
    }

    public async void UpdateVersion(int logId, string newVersion)
    {
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<Context>();
        var log = context.UpdateLogs.Find(logId);
        log.NewVersion = newVersion;
        await context.SaveChangesAsync();
        serviceVersion[log.ServiceHostId] = newVersion;
        await serviceUpdateLog.UpdateLog(logId, log.Log);
    }


    public List<string> GetConnectionIds(string user)
    {
        if (Connections.ContainsKey(user))
            return Connections[user];
        else
            return new();
    }

}


public static class ClientServiceExtensions
{
    public static async Task ClientConfigUpdated(this IHubContext<ClientService> clientService, string name, Context context)
    {
        var host = context.Hosts
        .Include(h => h.ServiceHosts).ThenInclude(sh => sh.EnvironmentVariables)
        .Include(h => h.ServiceHosts).ThenInclude(sh => sh.Service).ThenInclude(s => s.ServiceScripts)
        .First(h => h.Name == name);
        Console.WriteLine("Sending info on " + host.Name);
        await clientService.Clients.User(name).SendAsync("HostInfo", host);
    }

}