using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Core.Entities;

/// <summary>
/// 商品SKU实体（多规格库存）
/// </summary>
[Table("product_sku")]
public class ProductSku
{
    /// <summary>主键</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>所属商品ID</summary>
    public long ProductId { get; set; }

    /// <summary>SKU编码</summary>
    [MaxLength(100)]
    public string? SkuCode { get; set; }

    /// <summary>规格值组合 JSON，如 {"颜色":"红色","尺寸":"XL"}</summary>
    [Column(TypeName = "json")]
    public string? SpecValues { get; set; }

    /// <summary>销售价格</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// <summary>市场价格（划线价）</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? MarketPrice { get; set; }

    /// <summary>库存数量</summary>
    public int Stock { get; set; }

    /// <summary>SKU图片URL（可覆盖商品主图）</summary>
    [MaxLength(500)]
    public string? Image { get; set; }

    /// <summary>是否启用该规格</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>排序值</summary>
    public int SortOrder { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>软删除标记</summary>
    public bool IsDeleted { get; set; }

    /// <summary>所属商品（导航属性）</summary>
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}
