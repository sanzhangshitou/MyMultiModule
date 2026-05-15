using Microsoft.AspNetCore.Mvc;
using MyApp.Core.DTOs.Product;
using MyApp.Core.Interfaces;

namespace MyApp.Web.Controllers;

/// <summary>
/// 商城前台-分类接口
/// 提供分类树和分类详情查询
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IProductCategoryService _service;

    public CategoriesController(IProductCategoryService service)
    {
        _service = service;
    }

    /// <summary>获取分类树（前台使用）</summary>
    [HttpGet]
    public async Task<List<ProductCategoryDto>> GetTree()
    {
        return await _service.GetTreeAsync();
    }

    /// <summary>获取分类详情</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductCategoryDto>> GetById(long id)
    {
        var cat = await _service.GetByIdAsync(id);
        return cat is null ? NotFound() : Ok(cat);
    }
}
