using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RevisioHub.Common.Models.db;

public class ServiceHost
{
    [Key]
    public int Id { get; set; }

    public int HostId { get; set; }
    [ForeignKey(nameof(HostId))]
    public Host Host { get; set; } = null!;

    public int ServiceId { get; set; }
    [ForeignKey(nameof(ServiceId))]
    public Service Service { get; set; } = null!;
    public string WorkingDirectory { get; set; } = string.Empty;
    public List<EnvironmentVariable> EnvironmentVariables { get; set; } = new();
    public string EndPoint { get; set; } = string.Empty;
}
