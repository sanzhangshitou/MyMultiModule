using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Core.Entities;

/// <summary>
/// 商品分类实体
/// </summary>
[Table("product_category")]
public class ProductCategory
{
    /// <summary>主键</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>分类名称</summary>
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>父级分类ID，0 表示顶级分类</summary>
    public long ParentId { get; set; }

    /// <summary>排序值，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>软删除标记</summary>
    public bool IsDeleted { get; set; }

    /// <summary>子分类列表</summary>
    [InverseProperty(nameof(Parent))]
    public List<ProductCategory>? Children { get; set; }

    /// <summary>父级分类</summary>
    [ForeignKey(nameof(ParentId))]
    public ProductCategory? Parent { get; set; }
}
