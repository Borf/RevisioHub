using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RevisioHub.Common.Models.db;

public class InstallationFile
{
    [Key]
    public int InstallationFileId { get; set; }
    public int ServiceId { get; set; }
    [ForeignKey(nameof(ServiceId))]
    public Service Service { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public byte[] Data { get; set; } = new byte[0];

    [NotMapped]
    public string StrData
    {
        get => System.Text.Encoding.UTF8.GetString(Data);
        set => Data = System.Text.Encoding.UTF8.GetBytes(value);
    }
}

public enum FileType
{
    Text,Binary
}