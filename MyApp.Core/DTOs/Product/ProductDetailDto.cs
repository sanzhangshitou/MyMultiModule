using MyApp.Core.Enums;

namespace MyApp.Core.DTOs.Product;

/// <summary>
/// 商品详情DTO（包含完整信息和SKU列表）
/// </summary>
public class ProductDetailDto
{
    /// <summary>商品ID</summary>
    public long Id { get; set; }

    /// <summary>所属分类ID</summary>
    public long CategoryId { get; set; }

    /// <summary>所属分类名称</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>商品名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>商品副标题</summary>
    public string? Subtitle { get; set; }

    /// <summary>商品详情（富文本）</summary>
    public string? Description { get; set; }

    /// <summary>商品主图URL</summary>
    public string? MainImage { get; set; }

    /// <summary>商品轮播图URL列表</summary>
    public List<string>? Images { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatus Status { get; set; }

    /// <summary>规格定义列表</summary>
    public List<ProductSpecDto>? Specs { get; set; }

    /// <summary>商品单位</summary>
    public string? Unit { get; set; }

    /// <summary>最低售价</summary>
    public decimal MinPrice { get; set; }

    /// <summary>最高售价</summary>
    public decimal MaxPrice { get; set; }

    /// <summary>总库存</summary>
    public int TotalStock { get; set; }

    /// <summary>累计销量</summary>
    public int SalesCount { get; set; }

    /// <summary>排序值</summary>
    public int SortOrder { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>SKU列表</summary>
    public List<SkuDto> Skus { get; set; } = [];
}
