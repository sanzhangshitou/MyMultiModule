namespace MyApp.Core.DTOs.Product;

/// <summary>
/// 商品分类DTO（支持树形结构）
/// </summary>
public class ProductCategoryDto
{
    /// <summary>分类ID</summary>
    public long Id { get; set; }

    /// <summary>分类名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>父级分类ID，0 为顶级</summary>
    public long ParentId { get; set; }

    /// <summary>排序值</summary>
    public int SortOrder { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>子分类列表</summary>
    public List<ProductCategoryDto>? Children { get; set; }
}
