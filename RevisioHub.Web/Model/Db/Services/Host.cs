using System.ComponentModel.DataAnnotations;

namespace RevisioHub.Web.Model.Db.Services;

public class Host
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public HostType HostType { get; set; } = HostType.Generic; //TODO: should this move to ServiceHost ?

    public List<ServiceHost> ServiceHosts { get; set; } = new();
}
