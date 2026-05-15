namespace MyApp.Core.DTOs.Product;

/// <summary>
/// SKU入参（创建/更新时使用）
/// </summary>
public class SkuInput
{
    /// <summary>SKU编码</summary>
    public string? SkuCode { get; set; }

    /// <summary>规格值组合，如 {"颜色":"红色","尺寸":"XL"}</summary>
    public Dictionary<string, string>? SpecValues { get; set; }

    /// <summary>销售价格</summary>
    public decimal Price { get; set; }

    /// <summary>市场价格（划线价）</summary>
    public decimal? MarketPrice { get; set; }

    /// <summary>库存数量</summary>
    public int Stock { get; set; }

    /// <summary>SKU图片URL</summary>
    public string? Image { get; set; }

    /// <summary>排序值</summary>
    public int SortOrder { get; set; }
}
