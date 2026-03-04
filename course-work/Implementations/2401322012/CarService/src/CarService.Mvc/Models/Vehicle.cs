using System.ComponentModel.DataAnnotations;

namespace CarService.Mvc.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required, StringLength(10)]
    public string PlateNumber { get; set; } = "";

    [Required, StringLength(30)]
    public string Brand { get; set; } = "";

    [Required, StringLength(30)]
    public string Model { get; set; } = "";

    [Required, Range(1950, 2100)]
    public int Year { get; set; }

    [StringLength(17)]
    public string? Vin { get; set; }

    [Range(0, 20)]
    public decimal EngineVolume { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
