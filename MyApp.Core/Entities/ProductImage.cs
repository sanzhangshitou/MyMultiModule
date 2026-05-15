using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Core.Entities;

/// <summary>
/// 商品图片实体（支持按 SKU 关联）
/// </summary>
[Table("product_image")]
public class ProductImage
{
    /// <summary>主键</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>所属商品ID</summary>
    public long ProductId { get; set; }

    /// <summary>关联的SKU ID（可为空，为空则为商品级图片）</summary>
    public long? SkuId { get; set; }

    /// <summary>图片URL</summary>
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    /// <summary>排序值</summary>
    public int SortOrder { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}
