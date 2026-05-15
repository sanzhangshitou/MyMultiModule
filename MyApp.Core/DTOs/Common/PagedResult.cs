namespace MyApp.Core.DTOs.Common;

/// <summary>
/// 通用分页结果
/// </summary>
/// <typeparam name="T">列表项类型</typeparam>
public class PagedResult<T>
{
    /// <summary>总记录数</summary>
    public int Total { get; set; }

    /// <summary>当前页码</summary>
    public int Page { get; set; }

    /// <summary>每页条数</summary>
    public int PageSize { get; set; }

    /// <summary>数据列表</summary>
    public List<T> List { get; set; } = [];
}
