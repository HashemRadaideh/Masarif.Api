using Masarif.Api.Data;
using Microsoft.EntityFrameworkCore;

const string CorsPolicy = "ng-dev";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddDbContextPool<MasarifDbContext>(o =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    o.UseSqlServer(
        cs,
        sql =>
        {
            sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        }
    );
});

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o =>
    o.AddPolicy(
        CorsPolicy,
        p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
