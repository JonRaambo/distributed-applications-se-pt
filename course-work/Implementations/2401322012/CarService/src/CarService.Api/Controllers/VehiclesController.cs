using CarService.Api.Data;
using CarService.Api.Dtos;
using CarService.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarService.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly AppDbContext _db;
    public VehiclesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<Vehicle>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Vehicles.AsNoTracking().OrderByDescending(x => x.Id);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<Vehicle> { Page = page, PageSize = pageSize, TotalCount = total, Items = items });
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<Vehicle>>> Search(
        [FromQuery] string? plateNumber = null,
        [FromQuery] string? brand = null,
        [FromQuery] int? customerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Vehicles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(plateNumber))
            q = q.Where(x => x.PlateNumber.Contains(plateNumber));

        if (!string.IsNullOrWhiteSpace(brand))
            q = q.Where(x => x.Brand.Contains(brand));

        if (customerId.HasValue)
            q = q.Where(x => x.CustomerId == customerId.Value);

        q = q.OrderByDescending(x => x.Id);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<Vehicle> { Page = page, PageSize = pageSize, TotalCount = total, Items = items });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Vehicle>> GetById(int id)
    {
        var item = await _db.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Vehicle>> Create(Vehicle model)
    {
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == model.CustomerId);
        if (!customerExists) return BadRequest("CustomerId does not exist");

        _db.Vehicles.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Vehicle model)
    {
        if (id != model.Id) return BadRequest("Id mismatch");

        var exists = await _db.Vehicles.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == model.CustomerId);
        if (!customerExists) return BadRequest("CustomerId does not exist");

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Vehicles.FindAsync(id);
        if (item is null) return NotFound();

        _db.Vehicles.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
