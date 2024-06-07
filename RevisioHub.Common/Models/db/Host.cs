using RevisioHub.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace RevisioHub.Common.Models.db;

public class Host
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public HostType HostType { get; set; } = HostType.Generic; //TODO: should this move to ServiceHost ?
    public Architecture Architecture { get; set; } = Architecture.x64;
    public List<ServiceHost> ServiceHosts { get; set; } = new();
}
