using Masarif.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Masarif.Api.Data;

public sealed class MasarifDbContext(DbContextOptions<MasarifDbContext> options)
    : DbContext(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Expense>(e =>
        {
            e.ToTable("expenses");

            e.HasKey(p => p.Id);

            e.Property(p => p.Title).HasMaxLength(150).IsRequired();

            e.Property(p => p.Category).HasMaxLength(100).IsRequired();

            e.Property(p => p.Amount).HasPrecision(10, 2);

            e.Property(p => p.ExpenseDate).IsRequired();

            e.HasIndex(p => p.ExpenseDate);
            e.HasIndex(p => p.Category);
        });
    }
}
