using System.Text.Json;
using MyApp.Core.DTOs.Common;
using MyApp.Core.DTOs.Product;
using MyApp.Core.Entities;
using MyApp.Core.Enums;
using MyApp.Core.Interfaces;

namespace MyApp.Service;

/// <summary>
/// 商品服务实现（核心业务逻辑）
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IProductCategoryRepository _categoryRepo;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProductService(IProductRepository repo, IProductCategoryRepository categoryRepo)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
    }

    /// <summary>管理后台-分页查询商品列表（含所有状态）</summary>
    public async Task<PagedResult<ProductDto>> GetAdminListAsync(ProductListRequest request)
    {
        var result = await _repo.GetPagedListAsync(request);
        var dtos = new List<ProductDto>();

        foreach (var p in result.List)
            dtos.Add(await BuildProductDtoAsync(p));

        return new PagedResult<ProductDto>
        {
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize,
            List = dtos
        };
    }

    /// <summary>管理后台-获取商品详情</summary>
    public async Task<ProductDetailDto?> GetAdminDetailAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return null;
        return await BuildDetailDtoAsync(product);
    }

    /// <summary>新增商品，初始状态为草稿，同时创建SKU</summary>
    public async Task<ProductDetailDto> CreateAsync(ProductCreateInput input)
    {
        var product = new Product
        {
            CategoryId = input.CategoryId,
            Name = input.Name,
            Subtitle = input.Subtitle,
            Description = input.Description,
            MainImage = input.MainImage,
            Images = SerializeJson(input.Images),
            Specs = SerializeJson(input.Specs),
            Unit = input.Unit,
            SortOrder = input.SortOrder,
            Status = ProductStatus.Draft
        };

        await _repo.InsertAsync(product);

        if (input.Skus.Count > 0)
        {
            var skus = input.Skus.Select((s, i) => new ProductSku
            {
                ProductId = product.Id,
                SkuCode = s.SkuCode ?? $"{product.Id}_{i + 1}",
                SpecValues = SerializeJson(s.SpecValues),
                Price = s.Price,
                MarketPrice = s.MarketPrice,
                Stock = s.Stock,
                Image = s.Image,
                SortOrder = s.SortOrder
            }).ToList();

            await _repo.InsertSkuBatchAsync(skus);
        }

        return await GetAdminDetailAsync(product.Id)
            ?? throw new InvalidOperationException("创建后查询失败");
    }

    /// <summary>修改商品，整体替换SKU（先删后增）</summary>
    public async Task<ProductDetailDto> UpdateAsync(long id, ProductUpdateInput input)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("商品不存在");

        product.CategoryId = input.CategoryId;
        product.Name = input.Name;
        product.Subtitle = input.Subtitle;
        product.Description = input.Description;
        product.MainImage = input.MainImage;
        product.Images = SerializeJson(input.Images);
        product.Specs = SerializeJson(input.Specs);
        product.Unit = input.Unit;
        product.SortOrder = input.SortOrder;

        await _repo.UpdateAsync(product);
        await _repo.DeleteSkusByProductIdAsync(product.Id);

        if (input.Skus.Count > 0)
        {
            var skus = input.Skus.Select((s, i) => new ProductSku
            {
                ProductId = product.Id,
                SkuCode = s.SkuCode ?? $"{product.Id}_{i + 1}",
                SpecValues = SerializeJson(s.SpecValues),
                Price = s.Price,
                MarketPrice = s.MarketPrice,
                Stock = s.Stock,
                Image = s.Image,
                SortOrder = s.SortOrder
            }).ToList();

            await _repo.InsertSkuBatchAsync(skus);
        }

        return await GetAdminDetailAsync(product.Id)
            ?? throw new InvalidOperationException("更新后查询失败");
    }

    /// <summary>软删除商品</summary>
    public async Task DeleteAsync(long id)
    {
        await _repo.DeleteAsync(id);
    }

    /// <summary>
    /// 上架商品。只有草稿或下架状态的商品才能上架。
    /// 必须有至少一个SKU。
    /// </summary>
    public async Task ShelveAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("商品不存在");

        if (product.Status != ProductStatus.OffShelf && product.Status != ProductStatus.Draft)
            throw new InvalidOperationException("只有下架或草稿状态的商品才能上架");

        var skus = await _repo.GetSkusAsync(id);
        if (skus.Count == 0)
            throw new InvalidOperationException("商品没有SKU，无法上架");

        await _repo.UpdateStatusAsync(id, ProductStatus.OnSale);
    }

    /// <summary>下架商品。只有上架状态的商品才能下架。</summary>
    public async Task UnshelveAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("商品不存在");

        if (product.Status != ProductStatus.OnSale)
            throw new InvalidOperationException("只有上架状态的商品才能下架");

        await _repo.UpdateStatusAsync(id, ProductStatus.OffShelf);
    }

    /// <summary>启用商品（恢复为草稿状态）。只有禁用状态的商品才能启用。</summary>
    public async Task EnableAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("商品不存在");

        if (product.Status != ProductStatus.Disabled)
            throw new InvalidOperationException("只有禁用状态的商品才能启用");

        await _repo.UpdateStatusAsync(id, ProductStatus.Draft);
    }

    /// <summary>禁用商品。任何状态均可禁用。</summary>
    public async Task DisableAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("商品不存在");

        await _repo.UpdateStatusAsync(id, ProductStatus.Disabled);
    }

    /// <summary>前台-分页查询商品列表（仅上架商品）</summary>
    public async Task<PagedResult<ProductDto>> GetFrontListAsync(ProductListRequest request)
    {
        request.Status = (int)ProductStatus.OnSale;
        return await GetAdminListAsync(request);
    }

    /// <summary>前台-获取商品详情（仅上架商品）</summary>
    public async Task<ProductDetailDto?> GetFrontDetailAsync(long id)
    {
        var detail = await GetAdminDetailAsync(id);
        if (detail is null || detail.Status != ProductStatus.OnSale) return null;
        return detail;
    }

    /// <summary>构建列表DTO，填充SKU价格范围与库存汇总</summary>
    private async Task<ProductDto> BuildProductDtoAsync(Product p)
    {
        var skus = await _repo.GetSkusAsync(p.Id);
        return new ProductDto
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? "",
            Name = p.Name,
            Subtitle = p.Subtitle,
            MainImage = p.MainImage,
            Status = p.Status,
            Unit = p.Unit,
            MinPrice = skus.Count > 0 ? skus.Min(s => s.Price) : 0,
            MaxPrice = skus.Count > 0 ? skus.Max(s => s.Price) : 0,
            TotalStock = skus.Sum(s => s.Stock),
            SalesCount = p.SalesCount,
            SortOrder = p.SortOrder,
            CreatedAt = p.CreatedAt
        };
    }

    /// <summary>构建详情DTO，反序列化JSON字段，组装SKU列表</summary>
    private async Task<ProductDetailDto> BuildDetailDtoAsync(Product p)
    {
        var skus = await _repo.GetSkusAsync(p.Id);
        return new ProductDetailDto
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? "",
            Name = p.Name,
            Subtitle = p.Subtitle,
            Description = p.Description,
            MainImage = p.MainImage,
            Images = DeserializeJson<List<string>>(p.Images),
            Status = p.Status,
            Specs = DeserializeJson<List<ProductSpecDto>>(p.Specs),
            Unit = p.Unit,
            MinPrice = skus.Count > 0 ? skus.Min(s => s.Price) : 0,
            MaxPrice = skus.Count > 0 ? skus.Max(s => s.Price) : 0,
            TotalStock = skus.Sum(s => s.Stock),
            SalesCount = p.SalesCount,
            SortOrder = p.SortOrder,
            CreatedAt = p.CreatedAt,
            Skus = skus.Select(s => new SkuDto
            {
                Id = s.Id,
                SkuCode = s.SkuCode,
                SpecValues = DeserializeJson<Dictionary<string, string>>(s.SpecValues),
                Price = s.Price,
                MarketPrice = s.MarketPrice,
                Stock = s.Stock,
                Image = s.Image,
                IsEnabled = s.IsEnabled,
                SortOrder = s.SortOrder
            }).ToList()
        };
    }

    /// <summary>序列化对象为JSON字符串</summary>
    private static string? SerializeJson<T>(T? obj)
    {
        return obj is null ? null : JsonSerializer.Serialize(obj, _jsonOptions);
    }

    /// <summary>反序列化JSON字符串为对象</summary>
    private static T? DeserializeJson<T>(string? json)
    {
        return json is null ? default : JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }
}
