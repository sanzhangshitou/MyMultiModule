using MyApp.Core.DTOs.Product;

namespace MyApp.Core.Interfaces;

/// <summary>
/// 商品分类服务接口
/// </summary>
public interface IProductCategoryService
{
    /// <summary>获取分类树</summary>
    Task<List<ProductCategoryDto>> GetTreeAsync();

    /// <summary>根据ID获取分类</summary>
    Task<ProductCategoryDto?> GetByIdAsync(long id);

    /// <summary>新增分类</summary>
    Task<ProductCategoryDto> CreateAsync(ProductCategoryDto input);

    /// <summary>修改分类</summary>
    Task<ProductCategoryDto> UpdateAsync(long id, ProductCategoryDto input);

    /// <summary>删除分类</summary>
    Task DeleteAsync(long id);
}
