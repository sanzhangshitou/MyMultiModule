using MyApp.Core.DTOs.Product;
using MyApp.Core.Entities;
using MyApp.Core.Interfaces;

namespace MyApp.Service;

/// <summary>
/// 商品分类服务实现
/// </summary>
public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _repo;

    public ProductCategoryService(IProductCategoryRepository repo)
    {
        _repo = repo;
    }

    /// <summary>获取分类树，从顶级开始递归构建</summary>
    public async Task<List<ProductCategoryDto>> GetTreeAsync()
    {
        var all = await _repo.GetAllAsync();
        return BuildTree(all, 0);
    }

    /// <summary>根据ID获取分类</summary>
    public async Task<ProductCategoryDto?> GetByIdAsync(long id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    /// <summary>新增分类</summary>
    public async Task<ProductCategoryDto> CreateAsync(ProductCategoryDto input)
    {
        var entity = new ProductCategory
        {
            Name = input.Name,
            ParentId = input.ParentId,
            SortOrder = input.SortOrder,
            IsEnabled = true
        };
        await _repo.InsertAsync(entity);
        return Map(entity);
    }

    /// <summary>修改分类</summary>
    public async Task<ProductCategoryDto> UpdateAsync(long id, ProductCategoryDto input)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("分类不存在");

        entity.Name = input.Name;
        entity.ParentId = input.ParentId;
        entity.SortOrder = input.SortOrder;
        entity.IsEnabled = input.IsEnabled;
        await _repo.UpdateAsync(entity);
        return Map(entity);
    }

    /// <summary>软删除分类</summary>
    public async Task DeleteAsync(long id)
    {
        await _repo.DeleteAsync(id);
    }

    /// <summary>实体转DTO</summary>
    private static ProductCategoryDto Map(ProductCategory entity)
    {
        return new ProductCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentId = entity.ParentId,
            SortOrder = entity.SortOrder,
            IsEnabled = entity.IsEnabled
        };
    }

    /// <summary>递归构建分类树</summary>
    private static List<ProductCategoryDto> BuildTree(List<ProductCategory> all, long parentId)
    {
        return all
            .Where(c => c.ParentId == parentId)
            .Select(c =>
            {
                var dto = Map(c);
                dto.Children = BuildTree(all, c.Id);
                return dto;
            })
            .ToList();
    }
}
