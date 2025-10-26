using Masarif.Api.Auth;
using Masarif.Api.Data;
using Masarif.Api.Dtos;
using Masarif.Api.Mapping;
using Masarif.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Masarif.Api.Controllers;

public sealed record ExpensesQuery(
    string? Category,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 50,
    int? UserId = null
);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController(MasarifDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseReadDto>>> GetAll(
        [FromQuery] ExpensesQuery q,
        CancellationToken ct
    )
    {
        IQueryable<Expense> query = db.Expenses.AsNoTracking();

        var currentUserId = User.TryGetUserId();
        if (currentUserId is null)
            return Unauthorized();

        if (!User.IsAdmin())
        {
            query = query.Where(e => e.UserId == currentUserId.Value);
        }
        else if (q.UserId is int forUser)
        {
            query = query.Where(e => e.UserId == forUser);
        }
        else
        {
            // Admin without specific UserId filter - show all expenses
            // Keep the query as is (no additional filtering)
        }

        if (!string.IsNullOrWhiteSpace(q.Category))
            query = query.Where(e => e.Category == q.Category);

        if (q.From.HasValue)
            query = query.Where(e => e.ExpenseDate >= q.From.Value);
        if (q.To.HasValue)
            query = query.Where(e => e.ExpenseDate <= q.To.Value);

        query = query.OrderByDescending(e => e.ExpenseDate);

        var page = Math.Max(1, q.Page);
        var size = Math.Clamp(q.PageSize, 1, 500);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(e => e.ToReadDto())
            .ToListAsync(ct);

        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseReadDto>> GetById(int id, CancellationToken ct)
    {
        var e = await db.Expenses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null)
            return NotFound();

        if (!User.IsAdmin() && e.UserId != User.TryGetUserId())
            return Forbid();
        return Ok(e.ToReadDto());
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseReadDto>> Create(
        [FromBody] ExpenseCreateDto dto,
        CancellationToken ct
    )
    {
        var uid = User.TryGetUserId();
        if (uid is null)
            return Unauthorized();

        var e = dto.ToEntity();
        e.UserId = uid.Value;

        db.Expenses.Add(e);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = e.Id }, e.ToReadDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ExpenseUpdateDto dto,
        CancellationToken ct
    )
    {
        var e = await db.Expenses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null)
            return NotFound();

        if (!User.IsAdmin() && e.UserId != User.TryGetUserId())
            return Forbid();

        e.Apply(dto);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var e = await db.Expenses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null)
            return NotFound();

        if (!User.IsAdmin() && e.UserId != User.TryGetUserId())
            return Forbid();

        db.Expenses.Remove(e);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
