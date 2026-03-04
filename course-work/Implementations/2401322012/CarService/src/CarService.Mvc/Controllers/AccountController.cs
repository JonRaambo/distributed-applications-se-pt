using CarService.Mvc.Services;
using CarService.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;
    public AccountController(ApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var resp = await _api.PostAsync("api/auth/login", new { vm.Username, vm.Password });
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Грешно потребителско име/парола.");
            return View(vm);
        }

        var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        if (data?.Token is null)
        {
            ModelState.AddModelError("", "Неуспешен логин.");
            return View(vm);
        }

        HttpContext.Session.SetString("jwt", data.Token);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("jwt");
        return RedirectToAction(nameof(Login));
    }

    private sealed class LoginResponse
    {
        public string? Token { get; set; }
        public int ExpiresInMinutes { get; set; }
    }
}
