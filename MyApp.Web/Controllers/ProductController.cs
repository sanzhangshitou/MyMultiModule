using Microsoft.AspNetCore.Mvc;
using MyApp.Core.DTOs.Common;
using MyApp.Core.DTOs.Product;
using MyApp.Core.Interfaces;

namespace MyApp.Web.Controllers;

/// <summary>
/// 商城前台-商品接口（App/Web端使用）
/// 仅返回上架状态的商品
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    /// <summary>获取前台商品列表（仅上架商品），支持分页、搜索、分类筛选、排序</summary>
    [HttpGet]
    public async Task<PagedResult<ProductDto>> GetList([FromQuery] ProductListRequest request)
    {
        return await _service.GetFrontListAsync(request);
    }

    /// <summary>获取前台商品详情（仅上架商品），包含SKU列表</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDetailDto>> GetDetail(long id)
    {
        var detail = await _service.GetFrontDetailAsync(id);
        return detail is null ? NotFound() : Ok(detail);
    }
}
