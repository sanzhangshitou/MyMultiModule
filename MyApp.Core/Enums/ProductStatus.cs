namespace MyApp.Core.Enums;

/// <summary>
/// 商品状态枚举
/// </summary>
public enum ProductStatus
{
    /// <summary>草稿（新建商品，未上架）</summary>
    Draft = 0,

    /// <summary>上架（前台可见，可售卖）</summary>
    OnSale = 1,

    /// <summary>下架（已从售卖状态撤下）</summary>
    OffShelf = 2,

    /// <summary>禁用（违规或不再使用）</summary>
    Disabled = 3
}
