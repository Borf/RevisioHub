using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using RevisioHub.Web.Model.Db.Services;
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


    public override Task OnConnectedAsync()
    {
        Console.WriteLine("Client " + Context.UserIdentifier + " connected");
        Users.Add(Context.UserIdentifier!);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Client " + Context.UserIdentifier + " disconnected");
        Users.Remove(Context.UserIdentifier!);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
        => await Clients.All.SendAsync("ReceiveMessage", user, message);


    public async Task Ping()
    {
        Console.WriteLine("Ping");
        await this.Clients.Caller.SendAsync("Pong");
    }
}
