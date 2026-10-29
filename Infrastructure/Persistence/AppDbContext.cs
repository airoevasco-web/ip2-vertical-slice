namespace Infrastructure.Persistence;

using Application.Abstractions.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            entity.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(p => p.StockQuantity)
                .IsRequired();
        });
    }
}