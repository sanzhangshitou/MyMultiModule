using Microsoft.AspNetCore.Mvc;
using MyApp.Core.DTOs.Product;
using MyApp.Core.Interfaces;

namespace MyApp.Web.Controllers;

/// <summary>
/// 管理后台-分类管理接口
/// 提供分类的CRUD操作
/// </summary>
[ApiController]
[Route("api/admin/categories")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IProductCategoryService _service;

    public AdminCategoriesController(IProductCategoryService service)
    {
        _service = service;
    }

    /// <summary>获取全部分类树</summary>
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

    /// <summary>新增分类</summary>
    [HttpPost]
    public async Task<ActionResult<ProductCategoryDto>> Create([FromBody] ProductCategoryDto input)
    {
        var cat = await _service.CreateAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
    }

    /// <summary>修改分类</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProductCategoryDto>> Update(long id, [FromBody] ProductCategoryDto input)
    {
        var cat = await _service.UpdateAsync(id, input);
        return Ok(cat);
    }

    /// <summary>删除分类</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
