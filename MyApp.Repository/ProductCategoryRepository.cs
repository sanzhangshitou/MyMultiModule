using Microsoft.EntityFrameworkCore;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Repository;

/// <summary>
/// 商品分类仓储实现（EF Core）
/// </summary>
public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly AppDbContext _db;

    public ProductCategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>获取所有未删除的分类，按排序值升序</summary>
    public async Task<List<ProductCategory>> GetAllAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>根据ID获取分类</summary>
    public async Task<ProductCategory?> GetByIdAsync(long id)
    {
        return await _db.Categories.FindAsync(id);
    }

    /// <summary>新增分类</summary>
    public async Task<int> InsertAsync(ProductCategory category)
    {
        _db.Categories.Add(category);
        return await _db.SaveChangesAsync();
    }

    /// <summary>修改分类</summary>
    public async Task<int> UpdateAsync(ProductCategory category)
    {
        _db.Entry(category).State = EntityState.Modified;
        _db.Entry(category).Property(c => c.CreatedAt).IsModified = false;
        return await _db.SaveChangesAsync();
    }

    /// <summary>软删除分类</summary>
    public async Task<int> DeleteAsync(long id)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity is null) return 0;
        entity.IsDeleted = true;
        return await _db.SaveChangesAsync();
    }
}
