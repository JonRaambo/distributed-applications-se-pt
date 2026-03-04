using System.ComponentModel.DataAnnotations;

namespace CarService.Mvc.ViewModels;

public class LoginVm
{
    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, StringLength(100)]
    public string Password { get; set; } = "";
}
