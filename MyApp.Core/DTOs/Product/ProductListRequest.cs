namespace MyApp.Core.DTOs.Product;

/// <summary>
/// 商品列表查询请求
/// </summary>
public class ProductListRequest
{
    /// <summary>搜索关键词（模糊匹配商品名称）</summary>
    public string? Keyword { get; set; }

    /// <summary>分类ID筛选</summary>
    public long? CategoryId { get; set; }

    /// <summary>状态筛选（0-草稿 1-上架 2-下架 3-禁用）</summary>
    public int? Status { get; set; }

    /// <summary>页码，从1开始</summary>
    public int Page { get; set; } = 1;

    /// <summary>每页条数</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>排序字段：price / sales / created</summary>
    public string? SortField { get; set; }

    /// <summary>是否降序排列</summary>
    public bool SortDesc { get; set; }
}
