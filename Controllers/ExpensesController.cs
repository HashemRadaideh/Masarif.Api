using Masarif.Api.Data;
using Masarif.Api.Contracts;
using Masarif.Api.Mapping;
using Masarif.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Masarif.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController(MasarifDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseReadDto>>> GetAll(
        [FromQuery] string? category,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        IQueryable<Expense> q = db.Expenses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(e => e.Category == category);

        if (from.HasValue)
            q = q.Where(e => e.ExpenseDate >= from.Value);

        if (to.HasValue)
            q = q.Where(e => e.ExpenseDate <= to.Value);

        var result = await q
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => e.ToReadDto())
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseReadDto>> GetById(int id)
    {
        var e = await db.Expenses.FindAsync(id);
        return e is null ? NotFound() : Ok(e.ToReadDto());
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseReadDto>> Create([FromBody] ExpenseCreateDto dto)
    {
        var e = dto.ToEntity();
        db.Expenses.Add(e);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = e.Id }, e.ToReadDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExpenseUpdateDto dto)
    {
        var e = await db.Expenses.FindAsync(id);
        if (e is null) return NotFound();

        e.Apply(dto);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await db.Expenses.FindAsync(id);
        if (e is null) return NotFound();

        db.Expenses.Remove(e);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
