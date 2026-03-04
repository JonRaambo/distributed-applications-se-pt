using System.ComponentModel.DataAnnotations;

namespace CarService.Mvc.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, StringLength(20)]
    public string Phone { get; set; } = "";

    [StringLength(80), EmailAddress]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
