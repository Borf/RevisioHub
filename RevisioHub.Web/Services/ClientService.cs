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

namespace RevisioHub.Web.Services;

[Authorize]
public class ClientService : Hub
{
    public static ObservableCollection<string> Users { get; set; } = new();
    private IServiceProvider serviceProvider;
    private ServiceStatusService serviceStatus;
    public ClientService(IServiceProvider serviceProvider, ServiceStatusService serviceStatus)
    {
        this.serviceProvider = serviceProvider;
        this.serviceStatus = serviceStatus;
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

        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("HostInfo", host);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Client " + Context.UserIdentifier + " disconnected");
        Users.Remove(Context.UserIdentifier!);
        return base.OnDisconnectedAsync(exception);
    }

    public void Status(int serviceHostId, string status)
    {
        serviceStatus[serviceHostId] = status;
        Console.WriteLine("Status for " + serviceHostId + " is now " + status);
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