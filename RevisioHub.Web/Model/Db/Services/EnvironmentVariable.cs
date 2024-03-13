using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RevisioHub.Web.Model.Db.Services;

public class EnvironmentVariable
{
    [Key]
    public int Id { get; set; }
    public int ServiceHostId { get; set; }
    [ForeignKey(nameof(ServiceHostId))]
    public ServiceHost ServiceHost { get; set; } = null!;
    public string Variable { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
