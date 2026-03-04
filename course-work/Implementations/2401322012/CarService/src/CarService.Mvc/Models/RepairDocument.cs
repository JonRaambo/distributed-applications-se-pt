using System.ComponentModel.DataAnnotations;

namespace CarService.Mvc.Models;

public class RepairDocument
{
    public int Id { get; set; }

    [Required]
    public int RepairId { get; set; }

    [Required, StringLength(120)]
    public string FileName { get; set; } = "";

    [Required, StringLength(80)]
    public string ContentType { get; set; } = "";

    public long FileSize { get; set; }

    [Required, StringLength(260)]
    public string StoredPath { get; set; } = "";

    public DateTime UploadedAt { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }
}
