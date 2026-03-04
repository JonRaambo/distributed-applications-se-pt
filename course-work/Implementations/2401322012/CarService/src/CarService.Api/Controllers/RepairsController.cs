using CarService.Api.Data;
using CarService.Api.Dtos;
using CarService.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarService.Api.Controllers;

[ApiController]
[Route("api/repairs")]
[Authorize]
public class RepairsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RepairsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<Repair>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Repairs.AsNoTracking().OrderByDescending(x => x.Id);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<Repair> { Page = page, PageSize = pageSize, TotalCount = total, Items = items });
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<Repair>>> Search(
        [FromQuery] RepairStatus? status = null,
        [FromQuery] int? vehicleId = null,
        [FromQuery] bool? isPaid = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Repairs.AsNoTracking().AsQueryable();

        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (vehicleId.HasValue) q = q.Where(x => x.VehicleId == vehicleId.Value);
        if (isPaid.HasValue) q = q.Where(x => x.IsPaid == isPaid.Value);
        if (startDateFrom.HasValue) q = q.Where(x => x.StartDate >= startDateFrom.Value);
        if (startDateTo.HasValue) q = q.Where(x => x.StartDate <= startDateTo.Value);

        q = q.OrderByDescending(x => x.Id);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<Repair> { Page = page, PageSize = pageSize, TotalCount = total, Items = items });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Repair>> GetById(int id)
    {
        var item = await _db.Repairs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Repair>> Create(Repair model)
    {
        var vehicleExists = await _db.Vehicles.AnyAsync(v => v.Id == model.VehicleId);
        if (!vehicleExists) return BadRequest("VehicleId does not exist");

        _db.Repairs.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Repair model)
    {
        if (id != model.Id) return BadRequest("Id mismatch");

        var exists = await _db.Repairs.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        var vehicleExists = await _db.Vehicles.AnyAsync(v => v.Id == model.VehicleId);
        if (!vehicleExists) return BadRequest("VehicleId does not exist");

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Repairs.FindAsync(id);
        if (item is null) return NotFound();

        _db.Repairs.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
