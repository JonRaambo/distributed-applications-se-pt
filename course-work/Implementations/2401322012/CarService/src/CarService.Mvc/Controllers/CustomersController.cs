using CarService.Mvc.Models;
using CarService.Mvc.Services;
using CarService.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Mvc.Controllers;

public class CustomersController : Controller
{
    private readonly ApiClient _api;
    public CustomersController(ApiClient api) => _api = api;

    private bool HasToken() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("jwt"));

    public async Task<IActionResult> Index(string? phone, string? lastName, int page = 1, int pageSize = 10)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        string url;
        if (!string.IsNullOrWhiteSpace(phone) || !string.IsNullOrWhiteSpace(lastName))
            url = $"api/customers/search?phone={Uri.EscapeDataString(phone ?? "")}&lastName={Uri.EscapeDataString(lastName ?? "")}&page={page}&pageSize={pageSize}";
        else
            url = $"api/customers?page={page}&pageSize={pageSize}";

        var data = await _api.GetAsync<PagedResultVm<Customer>>(url) ?? new PagedResultVm<Customer>();
        ViewBag.Phone = phone;
        ViewBag.LastName = lastName;
        return View(data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");
        return View(new Customer());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer model)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        var resp = await _api.PostAsync("api/customers", model);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Грешка при запис.");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var model = await _api.GetAsync<Customer>($"api/customers/{id}");
        if (model is null) return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Customer model)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var resp = await _api.PutAsync($"api/customers/{id}", model);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Грешка при обновяване.");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var model = await _api.GetAsync<Customer>($"api/customers/{id}");
        if (model is null) return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var resp = await _api.DeleteAsync($"api/customers/{id}");
        if (!resp.IsSuccessStatusCode) return BadRequest();
        return RedirectToAction(nameof(Index));
    }
}
