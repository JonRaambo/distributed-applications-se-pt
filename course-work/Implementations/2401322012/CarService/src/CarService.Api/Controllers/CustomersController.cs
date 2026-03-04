using CarService.Api.Data;
using CarService.Api.Dtos;
using CarService.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarService.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<Customer>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Customers.AsNoTracking().OrderByDescending(x => x.Id);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<Customer> { Page = page, PageSize = pageSize, TotalCount = total, Items = items });
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<Customer>>> Search(
        [FromQuery] string? phone = null,
        [FromQuery] string? lastName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(phone))
            q = q.Where(x => x.Phone.Contains(phone));

        if (!string.IsNullOrWhiteSpace(lastName))
            q = q.Where(x => x.LastName.Contains(lastName));

        q = q.OrderByDescending(x => x.Id);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<Customer> { Page = page, PageSize = pageSize, TotalCount = total, Items = items });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var item = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer model)
    {
        _db.Customers.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Customer model)
    {
        if (id != model.Id) return BadRequest("Id mismatch");

        var exists = await _db.Customers.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        _db.Entry(model).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Customers.FindAsync(id);
        if (item is null) return NotFound();

        _db.Customers.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
