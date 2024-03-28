using Microsoft.EntityFrameworkCore;
using RevisioHub.Common.Models.db;

namespace RevisioHub.Web.Model.Db.Services;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<Service> Services { get; set; }
    public DbSet<Common.Models.db.Host> Hosts { get; set; }
    public DbSet<ServiceHost> ServiceHosts { get; set; }
    public DbSet<ServiceScript> ServiceScripts { get; set; }
    public DbSet<EnvironmentVariable> EnvironmentVariables { get; set; }
}
