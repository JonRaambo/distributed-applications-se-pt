using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Api.Models;

public enum RepairStatus
{
    Created = 0,
    InProgress = 1,
    WaitingParts = 2,
    Completed = 3,
    Cancelled = 4
}

public class Repair
{
    public int Id { get; set; }

    [Required]
    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    [Required, StringLength(80)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public RepairStatus Status { get; set; } = RepairStatus.Created;

    [Required, Column(TypeName = "decimal(6,2)"), Range(0, 10000)]
    public decimal LaborHours { get; set; }

    [Required, Column(TypeName = "decimal(10,2)"), Range(0, 100000000)]
    public decimal PartsCost { get; set; }

    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsPaid { get; set; }

    public ICollection<RepairDocument> Documents { get; set; } = new List<RepairDocument>();
}
