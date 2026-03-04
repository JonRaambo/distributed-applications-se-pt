using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Api.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    [Required, StringLength(10)]
    public string PlateNumber { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Brand { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Model { get; set; } = string.Empty;

    [Required, Range(1950, 2100)]
    public int Year { get; set; }

    [StringLength(17)]
    public string? Vin { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    [Range(0, 20)]
    public decimal EngineVolume { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Repair> Repairs { get; set; } = new List<Repair>();
}
