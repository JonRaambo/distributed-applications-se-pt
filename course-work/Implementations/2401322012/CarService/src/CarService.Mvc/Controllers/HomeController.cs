using Microsoft.AspNetCore.Mvc;

namespace CarService.Mvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
