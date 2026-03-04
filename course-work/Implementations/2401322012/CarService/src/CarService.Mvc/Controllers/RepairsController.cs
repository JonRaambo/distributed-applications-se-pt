using CarService.Mvc.Models;
using CarService.Mvc.Services;
using CarService.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Mvc.Controllers;

public class RepairsController : Controller
{
    private readonly ApiClient _api;
    public RepairsController(ApiClient api) => _api = api;

    private bool HasToken() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("jwt"));

    private async Task<List<Vehicle>> LoadAllVehicles()
    {
        var allVehicles = new List<Vehicle>();
        var page = 1;
        const int pageSize = 100;

        while (true)
        {
            var data = await _api.GetAsync<PagedResultVm<Vehicle>>($"api/vehicles?page={page}&pageSize={pageSize}");
            if (data?.Items == null || data.Items.Count == 0) break;

            allVehicles.AddRange(data.Items);

            if (allVehicles.Count >= data.TotalCount) break;
            page++;
        }

        return allVehicles;
    }

    public async Task<IActionResult> Index(
        RepairStatus? status,
        string? plateNumber,
        bool? isPaid,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int page = 1,
        int pageSize = 10)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        // Load ALL vehicles (for mapping PlateNumber->VehicleId + showing plate in table)
        var allVehicles = await LoadAllVehicles();

        // Convert plateNumber -> vehicleId (for API search)
        int? vehicleId = null;
        if (!string.IsNullOrWhiteSpace(plateNumber))
        {
            var match = allVehicles.FirstOrDefault(v =>
                !string.IsNullOrWhiteSpace(v.PlateNumber) &&
                v.PlateNumber.Contains(plateNumber, StringComparison.OrdinalIgnoreCase));

            vehicleId = match?.Id;
        }

        string url;
        if (status.HasValue || !string.IsNullOrWhiteSpace(plateNumber) || isPaid.HasValue || startDateFrom.HasValue || startDateTo.HasValue)
        {
            url = $"api/repairs/search?status={status}&vehicleId={vehicleId}&isPaid={isPaid}&startDateFrom={startDateFrom:O}&startDateTo={startDateTo:O}&page={page}&pageSize={pageSize}";
        }
        else
        {
            url = $"api/repairs?page={page}&pageSize={pageSize}";
        }

        var data = await _api.GetAsync<PagedResultVm<Repair>>(url) ?? new PagedResultVm<Repair>();

        ViewBag.Status = status;
        ViewBag.PlateNumber = plateNumber;
        ViewBag.IsPaid = isPaid;
        ViewBag.StartDateFrom = startDateFrom?.ToString("yyyy-MM-dd");
        ViewBag.StartDateTo = startDateTo?.ToString("yyyy-MM-dd");
        ViewBag.Vehicles = allVehicles;

        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var vehicles = await LoadAllVehicles();
        ViewBag.Vehicles = vehicles;

        return View(new Repair { StartDate = DateTime.UtcNow });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Repair model)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = await LoadAllVehicles();
            return View(model);
        }

        var resp = await _api.PostAsync("api/repairs", model);
        if (!resp.IsSuccessStatusCode)
        {
            ViewBag.Vehicles = await LoadAllVehicles();
            ModelState.AddModelError("", "Грешка при запис (провери Vehicle).");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var model = await _api.GetAsync<Repair>($"api/repairs/{id}");
        if (model is null) return NotFound();

        ViewBag.Vehicles = await LoadAllVehicles();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Repair model)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = await LoadAllVehicles();
            return View(model);
        }

        var resp = await _api.PutAsync($"api/repairs/{id}", model);
        if (!resp.IsSuccessStatusCode)
        {
            ViewBag.Vehicles = await LoadAllVehicles();
            ModelState.AddModelError("", "Грешка при обновяване.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var model = await _api.GetAsync<Repair>($"api/repairs/{id}");
        if (model is null) return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var resp = await _api.DeleteAsync($"api/repairs/{id}");
        if (!resp.IsSuccessStatusCode) return BadRequest();
        return RedirectToAction(nameof(Index));
    }
}