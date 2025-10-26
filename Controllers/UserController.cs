using Masarif.Api.Auth;
using Masarif.Api.Data;
using Masarif.Api.Dtos;
using Masarif.Api.Mapping;
using Masarif.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Masarif.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(MasarifDbContext db, IPasswordHasher<AppUser> hasher) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserReadDto>> Me(CancellationToken ct)
    {
        var id = User.TryGetUserId();
        if (id is null)
            return Unauthorized();

        var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.Value, ct);
        return u is null ? NotFound() : Ok(u.ToReadDto());
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] SelfUpdateDto dto, CancellationToken ct)
    {
        var id = User.TryGetUserId();
        if (id is null)
            return Unauthorized();

        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id.Value, ct);
        if (u is null)
            return NotFound();

        string? newHash = null;
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            newHash = hasher.HashPassword(u, dto.NewPassword);
            u.TokenVersion += 1;
        }

        u.ApplySelf(dto, newHash);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteMe(CancellationToken ct)
    {
        var id = User.TryGetUserId();
        if (id is null)
            return Unauthorized();

        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id.Value, ct);
        if (u is null)
            return NotFound();

        db.Users.Remove(u);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = db.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt);
        Response.Headers["X-Total-Count"] = (await q.CountAsync(ct)).ToString();

        var items = await q.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => u.ToReadDto())
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserReadDto>> GetById(int id, CancellationToken ct)
    {
        var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return u is null ? NotFound() : Ok(u.ToReadDto());
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserReadDto>> Create(
        [FromBody] UserCreateDto dto,
        CancellationToken ct
    )
    {
        var exists = await db.Users.AnyAsync(
            u => u.UserName == dto.UserName || u.Email == dto.Email,
            ct
        );
        if (exists)
            return Conflict(new { message = "Username or email already in use." });

        var u = new AppUser
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim(),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? Roles.User : dto.Role,
        };
        u.PasswordHash = hasher.HashPassword(u, dto.Password);

        db.Users.Add(u);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = u.Id }, u.ToReadDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UserUpdateDto dto,
        CancellationToken ct
    )
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u is null)
            return NotFound();

        u.Apply(dto);
        u.TokenVersion += 1;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u is null)
            return NotFound();

        db.Users.Remove(u);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
