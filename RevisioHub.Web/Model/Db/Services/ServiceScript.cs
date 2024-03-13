using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RevisioHub.Web.Model.Db.Services;

public class ServiceScript
{
    [Key]
    public int Id { get; set; }
    public ScriptType ScriptType { get; set; } = ScriptType.Unknown;
    public HostType HostType { get; set; } = HostType.Generic;
    public int ServiceId { get; set; }
    [ForeignKey(nameof(ServiceId))]
    public Service Service { get; set; } = null!;

    public string Script { get; set; } = string.Empty;
}
