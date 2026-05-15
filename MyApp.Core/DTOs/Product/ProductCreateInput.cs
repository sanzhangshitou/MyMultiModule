namespace MyApp.Core.DTOs.Product;

/// <summary>
/// 新增商品入参
/// </summary>
public class ProductCreateInput
{
    /// <summary>所属分类ID</summary>
    public long CategoryId { get; set; }

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

    /// <summary>规格定义列表</summary>
    public List<ProductSpecDto>? Specs { get; set; }

    /// <summary>商品单位</summary>
    public string? Unit { get; set; }

    /// <summary>排序值</summary>
    public int SortOrder { get; set; }

    /// <summary>SKU列表</summary>
    public List<SkuInput> Skus { get; set; } = [];
}
