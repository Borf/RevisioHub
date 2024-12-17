using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RevisioHub.Common.Models.db;

public class ServiceEnvironmentVariable
{
    [Key]
    public int Id { get; set; }
    public int ServiceId { get; set; }
    [ForeignKey(nameof(ServiceId))]
    public Service Service { get; set; } = null!;
    public string Variable { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
