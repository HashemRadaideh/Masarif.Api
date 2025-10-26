using Masarif.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Masarif.Api.Data;

public sealed class MasarifDbContext(DbContextOptions<MasarifDbContext> options)
    : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>(u =>
        {
            u.ToTable("users");
            u.HasKey(x => x.Id);
            u.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            u.Property(x => x.Email).HasMaxLength(200).IsRequired();
            u.Property(x => x.PasswordHash).IsRequired();
            u.Property(x => x.Role).HasMaxLength(20).IsRequired();
            u.HasIndex(x => x.UserName).IsUnique();
            u.HasIndex(x => x.Email).IsUnique();
        });

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

            e.Property(p => p.UserId).IsRequired();
            e.HasOne(p => p.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
