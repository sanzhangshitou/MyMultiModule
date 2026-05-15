using Microsoft.EntityFrameworkCore;
using MyApp.Core.Interfaces;
using MyApp.Repository;
using MyApp.Service;

/// <summary>
/// 应用入口，配置 DI 容器、EF Core、HTTP 管线
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// 注册 MVC 控制器和 OpenAPI 文档
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 注册 Redis 分布式缓存
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// 注册 EF Core DbContext（MySQL + snake_case 命名）
var connStr = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("未配置数据库连接字符串");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr))
           .UseSnakeCaseNamingConvention();
});

// 注册仓储层
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// 注册服务层
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
