using System.Security.Claims;
using System.Text;
using Masarif.Api.Auth;
using Masarif.Api.Data;
using Masarif.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

const string CorsPolicy = "ng-dev";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddDbContextPool<MasarifDbContext>(o =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    o.UseSqlServer(cs, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null));
});

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o =>
    o.AddPolicy(
        CorsPolicy,
        p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    )
);

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
builder.Services.AddSingleton(jwtOptions);

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        };

        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var userIdClaim = ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    var db = ctx.HttpContext.RequestServices.GetRequiredService<MasarifDbContext>();
                    var user = await db.Users.FindAsync(userId);
                    if (user != null)
                    {
                        var tokenVersionClaim = ctx.Principal?.FindFirst("ver");
                        if (
                            tokenVersionClaim != null
                            && int.TryParse(tokenVersionClaim.Value, out var tokenVersion)
                        )
                        {
                            if (tokenVersion != user.TokenVersion)
                            {
                                ctx.Fail("Token has been invalidated");
                                return;
                            }
                        }
                    }
                }
            },

            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("[JWT] Authentication failed: " + ctx.Exception.Message);
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MasarifDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Users.AnyAsync())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        var admin = new AppUser
        {
            UserName = "admin",
            Email = "admin@local",
            Role = Roles.Admin,
            CreatedAt = DateTime.UtcNow,
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@123!");
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
