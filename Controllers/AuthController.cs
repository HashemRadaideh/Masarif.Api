using Masarif.Api.Auth;
using Masarif.Api.Data;
using Masarif.Api.Dtos;
using Masarif.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Masarif.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    MasarifDbContext db,
    IPasswordHasher<AppUser> hasher,
    IJwtTokenService jwt
) : ControllerBase
{
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Signup(
        [FromBody] SignupRequest req,
        CancellationToken ct
    )
    {
        var exists = await db.Users.AnyAsync(
            u => u.UserName == req.UserName || u.Email == req.Email,
            ct
        );
        if (exists)
            return Conflict(new { message = "Username or email already in use." });

        var user = new AppUser
        {
            UserName = req.UserName.Trim(),
            Email = req.Email.Trim(),
            Role = Roles.User,
        };
        user.PasswordHash = hasher.HashPassword(user, req.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var (token, exp) = jwt.CreateAccessToken(user);
        return Ok(new AuthResponse(token, exp, user.Role));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest req,
        CancellationToken ct
    )
    {
        var identifier = req.UserNameOrEmail.Trim();
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.UserName == identifier || u.Email == identifier,
            ct
        );

        if (user is null)
            return Unauthorized();

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        var (token, exp) = jwt.CreateAccessToken(user);
        return Ok(new AuthResponse(token, exp, user.Role));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<LogoutResponse>> Logout(CancellationToken ct)
    {
        var id = User.TryGetUserId();
        if (id is null)
            return Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id.Value, ct);
        if (user is null)
            return Unauthorized();

        user.TokenVersion += 1;
        await db.SaveChangesAsync(ct);
        return Ok(new LogoutResponse(true));
    }
}
