using System.ComponentModel.DataAnnotations;

namespace CarService.Api.Models;

public class RepairDocument
{
    public int Id { get; set; }

    [Required]
    public int RepairId { get; set; }

    public Repair? Repair { get; set; }

    [Required, StringLength(120)]
    public string FileName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public long FileSize { get; set; }

    [Required, StringLength(260)]
    public string StoredPath { get; set; } = string.Empty;

    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string? Description { get; set; }
}
