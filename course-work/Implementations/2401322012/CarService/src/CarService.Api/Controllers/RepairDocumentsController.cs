using CarService.Api.Data;
using CarService.Api.Dtos;
using CarService.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarService.Api.Controllers;

[ApiController]
[Route("api/repairs/{repairId:int}/documents")]
[Authorize]
public class RepairDocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public RepairDocumentsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RepairDocument>>> List(
        int repairId,
        [FromQuery] string? fileName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.RepairDocuments.AsNoTracking().Where(d => d.RepairId == repairId);

        if (!string.IsNullOrWhiteSpace(fileName))
            q = q.Where(d => d.FileName.Contains(fileName));

        q = q.OrderByDescending(d => d.Id);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<RepairDocument> { Page = page, PageSize = pageSize, TotalCount = total, Items = items });
    }

    [HttpGet("search")]
    public Task<ActionResult<PagedResult<RepairDocument>>> Search(
        int repairId,
        [FromQuery] string? fileName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        => List(repairId, fileName, page, pageSize);

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<RepairDocument>> Upload(int repairId, [FromForm] IFormFile file, [FromForm] string? description = null)
    {
        if (file is null || file.Length == 0) return BadRequest("File is required");

        var repairExists = await _db.Repairs.AnyAsync(r => r.Id == repairId);
        if (!repairExists) return BadRequest("RepairId does not exist");

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads", "repairs", repairId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var ext = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid():N}{ext}";
        var storedPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var fs = System.IO.File.Create(storedPath))
        {
            await file.CopyToAsync(fs);
        }

        var doc = new RepairDocument
        {
            RepairId = repairId,
            FileName = Path.GetFileName(file.FileName),
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            FileSize = file.Length,
            StoredPath = storedPath,
            UploadedAt = DateTime.UtcNow,
            Description = description
        };

        _db.RepairDocuments.Add(doc);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMeta), new { repairId, documentId = doc.Id }, doc);
    }

    [HttpGet("{documentId:int}")]
    public async Task<ActionResult<RepairDocument>> GetMeta(int repairId, int documentId)
    {
        var doc = await _db.RepairDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.RepairId == repairId);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpGet("{documentId:int}/download")]
    public async Task<IActionResult> Download(int repairId, int documentId)
    {
        var doc = await _db.RepairDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == documentId && d.RepairId == repairId);
        if (doc is null) return NotFound();
        if (!System.IO.File.Exists(doc.StoredPath)) return NotFound("File missing on disk");

        var bytes = await System.IO.File.ReadAllBytesAsync(doc.StoredPath);
        return File(bytes, doc.ContentType, doc.FileName);
    }

    [HttpPut("{documentId:int}")]
    public async Task<IActionResult> Update(int repairId, int documentId, UpdateDocumentDto dto)
    {
        var doc = await _db.RepairDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.RepairId == repairId);
        if (doc is null) return NotFound();

        doc.Description = dto.Description;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{documentId:int}")]
    public async Task<IActionResult> Delete(int repairId, int documentId)
    {
        var doc = await _db.RepairDocuments.FirstOrDefaultAsync(d => d.Id == documentId && d.RepairId == repairId);
        if (doc is null) return NotFound();

        _db.RepairDocuments.Remove(doc);
        await _db.SaveChangesAsync();

        if (System.IO.File.Exists(doc.StoredPath))
            System.IO.File.Delete(doc.StoredPath);

        return NoContent();
    }
}
