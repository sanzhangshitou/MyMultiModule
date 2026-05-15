using MyApp.Core.Enums;

namespace MyApp.Core.DTOs.Product;

/// <summary>
/// 商品列表项DTO（用于列表展示）
/// </summary>
public class ProductDto
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

    /// <summary>商品主图URL</summary>
    public string? MainImage { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatus Status { get; set; }

    /// <summary>商品单位</summary>
    public string? Unit { get; set; }

    /// <summary>最低售价（取所有SKU中最低价）</summary>
    public decimal MinPrice { get; set; }

    /// <summary>最高售价（取所有SKU中最高价）</summary>
    public decimal MaxPrice { get; set; }

    /// <summary>总库存（所有SKU库存之和）</summary>
    public int TotalStock { get; set; }

    /// <summary>累计销量</summary>
    public int SalesCount { get; set; }

    /// <summary>排序值</summary>
    public int SortOrder { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
