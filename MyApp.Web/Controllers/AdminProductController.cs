using Microsoft.AspNetCore.Mvc;
using MyApp.Core.DTOs.Common;
using MyApp.Core.DTOs.Product;
using MyApp.Core.Interfaces;

namespace MyApp.Web.Controllers;

/// <summary>
/// 管理后台-商品管理接口
/// 提供商品的完整CRUD、上下架、启用/禁用等管理功能
/// </summary>
[ApiController]
[Route("api/admin/products")]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _service;

    public AdminProductsController(IProductService service)
    {
        _service = service;
    }

    /// <summary>获取商品列表（管理后台），支持分页、搜索、筛选、排序</summary>
    [HttpGet]
    public async Task<PagedResult<ProductDto>> GetListAsync([FromQuery] ProductListRequest request)
    {
        return await _service.GetAdminListAsync(request);
    }

    /// <summary>获取商品详情（管理后台），包含SKU列表</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDetailDto>> GetDetailAsync(long id)
    {
        var detail = await _service.GetAdminDetailAsync(id);
        return detail is null ? NotFound() : Ok(detail);
    }

    /// <summary>新增商品（含SKU），初始状态为草稿</summary>
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> CreateAsync([FromBody] ProductCreateInput input)
    {
        var detail = await _service.CreateAsync(input);
        return CreatedAtAction(nameof(GetDetailAsync), new { id = detail.Id }, detail);
    }

    /// <summary>修改商品（整体替换SKU）</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProductDetailDto>> UpdateAsync(long id, [FromBody] ProductUpdateInput input)
    {
        var detail = await _service.UpdateAsync(id, input);
        return Ok(detail);
    }

    /// <summary>删除商品（软删除）</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>上架商品（草稿或下架 → 上架）</summary>
    [HttpPut("{id:long}/shelve")]
    public async Task<IActionResult> ShelveAsync(long id)
    {
        await _service.ShelveAsync(id);
        return Ok();
    }

    /// <summary>下架商品（上架 → 下架）</summary>
    [HttpPut("{id:long}/unshelve")]
    public async Task<IActionResult> UnshelveAsync(long id)
    {
        await _service.UnshelveAsync(id);
        return Ok();
    }

    /// <summary>启用商品（禁用 → 草稿）</summary>
    [HttpPut("{id:long}/enable")]
    public async Task<IActionResult> EnableAsync(long id)
    {
        await _service.EnableAsync(id);
        return Ok();
    }

    /// <summary>禁用商品（任意状态 → 禁用）</summary>
    [HttpPut("{id:long}/disable")]
    public async Task<IActionResult> DisableAsync(long id)
    {
        await _service.DisableAsync(id);
        return Ok();
    }
}
