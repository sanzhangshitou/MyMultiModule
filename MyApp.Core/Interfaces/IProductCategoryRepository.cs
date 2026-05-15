using MyApp.Core.Entities;

namespace MyApp.Core.Interfaces;

/// <summary>
/// 商品分类仓储接口
/// </summary>
public interface IProductCategoryRepository
{
    /// <summary>获取所有未删除的分类</summary>
    Task<List<ProductCategory>> GetAllAsync();

    /// <summary>根据ID获取分类</summary>
    Task<ProductCategory?> GetByIdAsync(long id);

    /// <summary>新增分类</summary>
    Task<int> InsertAsync(ProductCategory category);

    /// <summary>修改分类</summary>
    Task<int> UpdateAsync(ProductCategory category);

    /// <summary>软删除分类</summary>
    Task<int> DeleteAsync(long id);
}
