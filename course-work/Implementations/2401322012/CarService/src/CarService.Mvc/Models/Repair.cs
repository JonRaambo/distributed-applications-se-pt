using System.ComponentModel.DataAnnotations;

namespace CarService.Mvc.Models;

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

    [Required, StringLength(80)]
    public string Title { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public RepairStatus Status { get; set; } = RepairStatus.Created;

    [Required, Range(0, 10000)]
    public decimal LaborHours { get; set; }

    [Required, Range(0, 100000000)]
    public decimal PartsCost { get; set; }

    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsPaid { get; set; }
}
