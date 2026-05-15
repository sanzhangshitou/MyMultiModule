using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyApp.Core.Enums;

namespace MyApp.Core.Entities;

/// <summary>
/// 商品主表实体
/// </summary>
[Table("product")]
public class Product
{
    /// <summary>主键</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>所属分类ID</summary>
    public long CategoryId { get; set; }

    /// <summary>商品名称</summary>
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>商品副标题/简介</summary>
    [MaxLength(500)]
    public string? Subtitle { get; set; }

    /// <summary>商品详情（富文本）</summary>
    public string? Description { get; set; }

    /// <summary>商品主图URL</summary>
    [MaxLength(500)]
    public string? MainImage { get; set; }

    /// <summary>商品轮播图URL列表，JSON 数组格式</summary>
    [Column(TypeName = "json")]
    public string? Images { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    /// <summary>
    /// 规格定义 JSON，格式如 [{"Name":"颜色","Values":["红","蓝"]}]
    /// </summary>
    [Column(TypeName = "json")]
    public string? Specs { get; set; }

    /// <summary>商品单位，如"件"/"箱"/"kg"</summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>排序值，越小越靠前</summary>
    public int SortOrder { get; set; }

    /// <summary>累计销量</summary>
    public int SalesCount { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>软删除标记</summary>
    public bool IsDeleted { get; set; }

    /// <summary>所属分类（导航属性）</summary>
    [ForeignKey(nameof(CategoryId))]
    public ProductCategory? Category { get; set; }

    /// <summary>SKU列表（导航属性）</summary>
    [InverseProperty(nameof(ProductSku.Product))]
    public List<ProductSku>? Skus { get; set; }
}
