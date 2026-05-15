using Microsoft.EntityFrameworkCore;
using MyApp.Core.DTOs.Common;
using MyApp.Core.DTOs.Product;
using MyApp.Core.Entities;
using MyApp.Core.Enums;
using MyApp.Core.Interfaces;

namespace MyApp.Repository;

/// <summary>
/// 商品仓储实现（EF Core）
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>分页查询商品列表，支持关键词搜索、分类筛选、状态筛选和排序</summary>
    public async Task<PagedResult<Product>> GetPagedListAsync(ProductListRequest request)
    {
        IQueryable<Product> query = _db.Products.Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
            query = query.Where(p => p.Name.Contains(request.Keyword));
        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == (ProductStatus)request.Status.Value);

        var total = await query.CountAsync();

        query = (request.SortField?.ToLower()) switch
        {
            "price" => request.SortDesc
                ? query.OrderByDescending(p => _db.ProductSkus
                    .Where(s => s.ProductId == p.Id)
                    .Min(s => (decimal?)s.Price) ?? 0)
                : query.OrderBy(p => _db.ProductSkus
                    .Where(s => s.ProductId == p.Id)
                    .Min(s => (decimal?)s.Price) ?? 0),
            "sales" => request.SortDesc
                ? query.OrderByDescending(p => p.SalesCount)
                : query.OrderBy(p => p.SalesCount),
            "created" => request.SortDesc
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
        };

        var list = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            List = list
        };
    }

    /// <summary>根据ID获取商品，包含导航属性 Category 和 Skus</summary>
    public async Task<Product?> GetByIdAsync(long id)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Skus)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>新增商品</summary>
    public async Task<int> InsertAsync(Product product)
    {
        _db.Products.Add(product);
        return await _db.SaveChangesAsync();
    }

    /// <summary>修改商品基本信息（不含SKU）</summary>
    public async Task<int> UpdateAsync(Product product)
    {
        _db.Entry(product).State = EntityState.Modified;
        _db.Entry(product).Property(p => p.Status).IsModified = false;
        _db.Entry(product).Property(p => p.SalesCount).IsModified = false;
        _db.Entry(product).Property(p => p.CreatedAt).IsModified = false;
        return await _db.SaveChangesAsync();
    }

    /// <summary>软删除商品</summary>
    public async Task<int> DeleteAsync(long id)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity is null) return 0;
        entity.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }

    /// <summary>更新商品状态</summary>
    public async Task<int> UpdateStatusAsync(long id, ProductStatus status)
    {
        return await _db.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, status));
    }

    /// <summary>获取商品的所有SKU（按排序值升序）</summary>
    public async Task<List<ProductSku>> GetSkusAsync(long productId)
    {
        return await _db.ProductSkus
            .Where(s => s.ProductId == productId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }

    /// <summary>新增单个SKU</summary>
    public async Task<int> InsertSkuAsync(ProductSku sku)
    {
        _db.ProductSkus.Add(sku);
        return await _db.SaveChangesAsync();
    }

    /// <summary>批量新增SKU</summary>
    public async Task<int> InsertSkuBatchAsync(List<ProductSku> skus)
    {
        _db.ProductSkus.AddRange(skus);
        return await _db.SaveChangesAsync();
    }

    /// <summary>批量更新SKU（根据ID识别新增或更新）</summary>
    public async Task<int> UpdateSkuBatchAsync(List<ProductSku> skus)
    {
        _db.ProductSkus.UpdateRange(skus);
        return await _db.SaveChangesAsync();
    }

    /// <summary>软删除商品下的所有SKU</summary>
    public async Task<int> DeleteSkusByProductIdAsync(long productId)
    {
        return await _db.ProductSkus
            .Where(s => s.ProductId == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));
    }
}
