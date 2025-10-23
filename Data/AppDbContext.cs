using Microsoft.EntityFrameworkCore;
using Masarif.Api.Models;

namespace Masarif.Api.Data;

public class MasarifDbContext(DbContextOptions<MasarifDbContext> options) : DbContext(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Expense>(e =>
        {
            e.Property(p => p.Title).HasMaxLength(150).IsRequired();
            e.Property(p => p.Category).HasMaxLength(100).IsRequired();
            e.Property(p => p.Amount).HasColumnType("decimal(10,2)");
            e.Property(p => p.ExpenseDate).IsRequired();
        });
    }
}
