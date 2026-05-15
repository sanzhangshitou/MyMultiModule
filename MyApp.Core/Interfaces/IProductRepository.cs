using MyApp.Core.DTOs.Common;
using MyApp.Core.DTOs.Product;
using MyApp.Core.Entities;
using MyApp.Core.Enums;

namespace MyApp.Core.Interfaces;

/// <summary>
/// 商品仓储接口
/// </summary>
public interface IProductRepository
{
    /// <summary>分页查询商品列表</summary>
    Task<PagedResult<Product>> GetPagedListAsync(ProductListRequest request);

    /// <summary>根据ID获取商品（含SKU）</summary>
    Task<Product?> GetByIdAsync(long id);

    /// <summary>新增商品</summary>
    Task<int> InsertAsync(Product product);

    /// <summary>修改商品基本信息</summary>
    Task<int> UpdateAsync(Product product);

    /// <summary>软删除商品</summary>
    Task<int> DeleteAsync(long id);

    /// <summary>更新商品状态</summary>
    Task<int> UpdateStatusAsync(long id, ProductStatus status);

    /// <summary>获取商品的所有SKU</summary>
    Task<List<ProductSku>> GetSkusAsync(long productId);

    /// <summary>新增单个SKU</summary>
    Task<int> InsertSkuAsync(ProductSku sku);

    /// <summary>批量新增SKU</summary>
    Task<int> InsertSkuBatchAsync(List<ProductSku> skus);

    /// <summary>批量更新SKU</summary>
    Task<int> UpdateSkuBatchAsync(List<ProductSku> skus);

    /// <summary>软删除商品下所有SKU</summary>
    Task<int> DeleteSkusByProductIdAsync(long productId);
}
