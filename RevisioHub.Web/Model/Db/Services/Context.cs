using Microsoft.EntityFrameworkCore;
using RevisioHub.Web.Model.Db.User;

namespace RevisioHub.Web.Model.Db.Services;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<Service> Services { get; set; }
    public DbSet<Host> Hosts { get; set; }
    public DbSet<ServiceHost> ServiceHosts { get; set; }
    public DbSet<ServiceScript> ServiceScripts { get; set; }
    public DbSet<EnvironmentVariable> EnvironmentVariables { get; set; }
}
