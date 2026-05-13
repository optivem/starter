using Microsoft.EntityFrameworkCore;
using MyCompany.MyShop.Backend.Core.Entities;

namespace MyCompany.MyShop.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Coupon> Coupons { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Country).HasDefaultValueSql("'US'");
            entity.Property(e => e.BasePrice).HasDefaultValueSql("0");
            entity.Property(e => e.DiscountRate).HasDefaultValueSql("0");
            entity.Property(e => e.DiscountAmount).HasDefaultValueSql("0");
            entity.Property(e => e.SubtotalPrice).HasDefaultValueSql("0");
            entity.Property(e => e.TaxRate).HasDefaultValueSql("0");
            entity.Property(e => e.TaxAmount).HasDefaultValueSql("0");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.UsedCount).HasDefaultValueSql("0");
        });
    }
}
