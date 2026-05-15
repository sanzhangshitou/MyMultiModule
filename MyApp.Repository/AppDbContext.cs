using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;

namespace MyApp.Repository;

/// <summary>
/// EF Core 数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSku> ProductSkus => Set<ProductSku>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- 全局软删除过滤器 ----
        modelBuilder.Entity<ProductCategory>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<ProductSku>().HasQueryFilter(s => !s.IsDeleted);

        // ---- ProductCategory ----
        modelBuilder.Entity<ProductCategory>(e =>
        {
            e.HasIndex(c => c.ParentId);
            e.HasIndex(c => c.SortOrder);
            e.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Product ----
        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.CategoryId);
            e.HasIndex(p => p.Status);
            e.HasIndex(p => p.SalesCount);
            e.HasIndex(p => p.CreatedAt);
            e.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(p => p.Skus)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.Images).HasColumnType("json");
            e.Property(p => p.Specs).HasColumnType("json");
        });

        // ---- ProductSku ----
        modelBuilder.Entity<ProductSku>(e =>
        {
            e.HasIndex(s => s.ProductId);
            e.HasIndex(s => s.SkuCode);
            e.HasIndex(s => s.Price);
            e.HasIndex(s => s.Stock);
            e.Property(s => s.SpecValues).HasColumnType("json");
            e.Property(s => s.Price).HasColumnType("decimal(10,2)");
            e.Property(s => s.MarketPrice).HasColumnType("decimal(10,2)");
        });

        // ---- ProductImage ----
        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasIndex(i => i.ProductId);
            e.HasIndex(i => i.SkuId);
        });
    }
}
