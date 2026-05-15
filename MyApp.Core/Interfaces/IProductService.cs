using MyApp.Core.DTOs.Common;
using MyApp.Core.DTOs.Product;

namespace MyApp.Core.Interfaces;

/// <summary>
/// 商品服务接口
/// </summary>
public interface IProductService
{
    /// <summary>管理后台-分页查询商品列表</summary>
    Task<PagedResult<ProductDto>> GetAdminListAsync(ProductListRequest request);

    /// <summary>管理后台-获取商品详情</summary>
    Task<ProductDetailDto?> GetAdminDetailAsync(long id);

    /// <summary>新增商品（含SKU），初始状态为草稿</summary>
    Task<ProductDetailDto> CreateAsync(ProductCreateInput input);

    /// <summary>修改商品（整体替换SKU）</summary>
    Task<ProductDetailDto> UpdateAsync(long id, ProductUpdateInput input);

    /// <summary>软删除商品</summary>
    Task DeleteAsync(long id);

    /// <summary>上架商品</summary>
    Task ShelveAsync(long id);

    /// <summary>下架商品</summary>
    Task UnshelveAsync(long id);

    /// <summary>启用商品（恢复为草稿状态）</summary>
    Task EnableAsync(long id);

    /// <summary>禁用商品</summary>
    Task DisableAsync(long id);

    /// <summary>前台-分页查询商品列表（仅上架商品）</summary>
    Task<PagedResult<ProductDto>> GetFrontListAsync(ProductListRequest request);

    /// <summary>前台-获取商品详情（仅上架商品）</summary>
    Task<ProductDetailDto?> GetFrontDetailAsync(long id);
}
