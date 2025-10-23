using Masarif.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<MasarifDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddCors(o =>
    o.AddPolicy("ng-dev", p =>
        p.WithOrigins(
            "http://localhost:4200",
            "http://127.0.0.1:4200",
            "https://localhost:4200",
            "https://127.0.0.1:4200"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
    )
);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors("ng-dev");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
