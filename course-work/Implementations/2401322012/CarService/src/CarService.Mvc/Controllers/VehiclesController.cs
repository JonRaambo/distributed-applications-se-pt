using CarService.Mvc.Models;
using CarService.Mvc.Services;
using CarService.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Mvc.Controllers;

public class VehiclesController : Controller
{
    private readonly ApiClient _api;
    public VehiclesController(ApiClient api) => _api = api;

    private bool HasToken() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("jwt"));

    public async Task<IActionResult> Index(string? plateNumber, string? brand, int? customerId, int page = 1, int pageSize = 10)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        string url;
        if (!string.IsNullOrWhiteSpace(plateNumber) || !string.IsNullOrWhiteSpace(brand) || customerId.HasValue)
        {
            url = $"api/vehicles/search?plateNumber={Uri.EscapeDataString(plateNumber ?? "")}&brand={Uri.EscapeDataString(brand ?? "")}&customerId={customerId}&page={page}&pageSize={pageSize}";
        }
        else
        {
            url = $"api/vehicles?page={page}&pageSize={pageSize}";
        }

        var data = await _api.GetAsync<PagedResultVm<Vehicle>>(url) ?? new PagedResultVm<Vehicle>();

        var allCustomers = new List<Customer>();
        var cPage = 1;
        const int cPageSize = 100;

        while (true)
        {
            var pageData = await _api.GetAsync<PagedResultVm<Customer>>($"api/customers?page={cPage}&pageSize={cPageSize}");
            if (pageData?.Items == null || pageData.Items.Count == 0) break;

            allCustomers.AddRange(pageData.Items);

            if (allCustomers.Count >= pageData.TotalCount) break;
            cPage++;
        }

        ViewBag.PlateNumber = plateNumber;
        ViewBag.Brand = brand;
        ViewBag.CustomerId = customerId;
        ViewBag.Customers = allCustomers;

        return View(data);
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var allCustomers = new List<Customer>();
        var page = 1;
        const int pageSize = 100;

        while (true)
        {
            var data = await _api.GetAsync<PagedResultVm<Customer>>($"api/customers?page={page}&pageSize={pageSize}");
            if (data?.Items == null || data.Items.Count == 0) break;

            allCustomers.AddRange(data.Items);

            if (allCustomers.Count >= data.TotalCount) break;
            page++;
        }

        ViewBag.Customers = allCustomers;

        return View(new Vehicle());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Vehicle model)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        var resp = await _api.PostAsync("api/vehicles", model);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Грешка при запис (провери CustomerId / PlateNumber)." );
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var model = await _api.GetAsync<Vehicle>($"api/vehicles/{id}");
        if (model is null) return NotFound();

         var allCustomers = new List<Customer>();
        var page = 1;
        const int pageSize = 100;

        while (true)
        {
            var data = await _api.GetAsync<PagedResultVm<Customer>>($"api/customers?page={page}&pageSize={pageSize}");
            if (data?.Items == null || data.Items.Count == 0) break;

            allCustomers.AddRange(data.Items);

            if (allCustomers.Count >= data.TotalCount) break;
            page++;
        }

        ViewBag.Customers = allCustomers;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Vehicle model)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var resp = await _api.PutAsync($"api/vehicles/{id}", model);
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

        var model = await _api.GetAsync<Vehicle>($"api/vehicles/{id}");
        if (model is null) return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var resp = await _api.DeleteAsync($"api/vehicles/{id}");
        if (!resp.IsSuccessStatusCode) return BadRequest();
        return RedirectToAction(nameof(Index));
    }

}
