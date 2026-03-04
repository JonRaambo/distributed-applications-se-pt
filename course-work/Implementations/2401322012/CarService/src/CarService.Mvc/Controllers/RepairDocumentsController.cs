using CarService.Mvc.Models;
using CarService.Mvc.Services;
using CarService.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Mvc.Controllers;

public class RepairDocumentsController : Controller
{
    private readonly ApiClient _api;
    public RepairDocumentsController(ApiClient api) => _api = api;

    private bool HasToken() => !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("jwt"));

    public async Task<IActionResult> Index(int repairId, string? fileName, int page = 1, int pageSize = 10)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var url = $"api/repairs/{repairId}/documents?fileName={Uri.EscapeDataString(fileName ?? "")}&page={page}&pageSize={pageSize}";
        var data = await _api.GetAsync<PagedResultVm<RepairDocument>>(url) ?? new PagedResultVm<RepairDocument>();

        ViewBag.RepairId = repairId;
        ViewBag.FileName = fileName;
        return View(data);
    }
        [HttpPost]
        public async Task<IActionResult> Upload(int repairId, IFormFile file, string? description)
        {
            if (!HasToken()) return RedirectToAction("Login", "Account");

            if (file is null || file.Length == 0)
                return RedirectToAction(nameof(Index), new { repairId });

            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "file", file.FileName);

            if (!string.IsNullOrWhiteSpace(description))
                form.Add(new StringContent(description), "description");

            await _api.PostMultipartAsync($"api/repairs/{repairId}/documents", form);
            return RedirectToAction(nameof(Index), new { repairId });
        }
    [HttpGet]
    public async Task<IActionResult> Preview(int repairId, int documentId)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        // 1) взимаме meta, за да знаем ContentType
        var meta = await _api.GetAsync<CarService.Mvc.Models.RepairDocument>(
            $"api/repairs/{repairId}/documents/{documentId}");

        if (meta is null) return NotFound();

        // 2) дърпаме файла от API download endpoint-а с JWT
        var resp = await _api.GetRawAsync($"api/repairs/{repairId}/documents/{documentId}/download");
        if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode);

        var bytes = await resp.Content.ReadAsByteArrayAsync();

        var ct = string.IsNullOrWhiteSpace(meta.ContentType)
            ? "application/octet-stream"
            : meta.ContentType;

        return File(bytes, ct);
    }
    public async Task<IActionResult> Delete(int repairId, int documentId)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        await _api.DeleteAsync($"api/repairs/{repairId}/documents/{documentId}");
        return RedirectToAction(nameof(Index), new { repairId });
    }

    // Browser-friendly download (MVC proxies API call and adds Bearer token)
    public async Task<IActionResult> Download(int repairId, int documentId)
    {
        if (!HasToken()) return RedirectToAction("Login", "Account");

        var resp = await _api.GetRawAsync($"api/repairs/{repairId}/documents/{documentId}/download");
        if (!resp.IsSuccessStatusCode) return NotFound();

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = resp.Content.Headers.ContentDisposition?.FileNameStar
            ?? resp.Content.Headers.ContentDisposition?.FileName
            ?? $"document_{documentId}";

        fileName = fileName.Trim('"');
        return File(bytes, contentType, fileName);
    }
}
