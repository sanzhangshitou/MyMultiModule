namespace MyApp.Core.DTOs.Product;

/// <summary>
/// 商品规格定义
/// </summary>
public class ProductSpecDto
{
    /// <summary>规格名称，如"颜色"、"尺寸"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>规格值列表，如 ["红色","蓝色","黑色"]</summary>
    public List<string> Values { get; set; } = [];
}
