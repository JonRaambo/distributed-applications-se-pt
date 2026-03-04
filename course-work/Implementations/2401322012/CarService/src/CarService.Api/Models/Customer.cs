using System.ComponentModel.DataAnnotations;

namespace CarService.Api.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(80), EmailAddress]
    public string? Email { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
