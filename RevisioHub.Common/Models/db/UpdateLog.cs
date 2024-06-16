using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisioHub.Common.Models.db;
public class UpdateLog
{
    [Key]
    public int Id { get; set; }
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;
    [ForeignKey(nameof(ServiceHostId))]
    public ServiceHost ServiceHost { get; set; } = null!;
    public int ServiceHostId { get; set; }
    public string Log { get; set; } = string.Empty;
    public string OriginalVersion { get; set; } = string.Empty;
    public string NewVersion { get; set; } = string.Empty;

}
