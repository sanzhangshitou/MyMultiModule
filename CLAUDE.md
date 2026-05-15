# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build MyApp.slnx                              # 构建解决方案
dotnet run --project MyApp.Web/MyApp.Web.csproj      # 启动 (http://localhost:5119)
dotnet watch --project MyApp.Web/MyApp.Web.csproj    # 热重载
```

## Architecture

5 层 Clean Architecture，依赖方向：`Web → Service → Repository → Core → Common`

- **MyApp.Web** — 控制器、DI 注册、入口。使用 MVC Controller（非 Minimal API）。`Program.cs` 中手动注册所有仓储和服务
- **MyApp.Service** — 业务编排。`ProductService` 负责商品状态校验、JSON 序列化/反序列化（规格、轮播图）、SKU 组装；`ProductCategoryService` 递归构建分类树
- **MyApp.Repository** — EF Core 数据访问。`AppDbContext` 配置实体映射、索引、全局软删除过滤器（`HasQueryFilter(p => !p.IsDeleted)`）
- **MyApp.Core** — 实体（`[Table]`/`[Key]`/`[ForeignKey]` DataAnnotations）、DTO、枚举、仓储/服务接口。不依赖 ORM 实现，仅引用 `EF Core Abstractions`
- **MyApp.Common** — 预留公共工具层

### 关键技术决策

| 决策 | 选型 |
|------|------|
| ORM | EF Core 9.0.5 + Pomelo.EntityFrameworkCore.MySql 9.0 |
| 列名映射 | `EFCore.NamingConventions` 的 `UseSnakeCaseNamingConvention()` — `CategoryId` → `category_id` |
| 软删除 | 所有表 `is_deleted` 字段，DbContext 全局过滤器排除 |
| 规格/SKU | `product.specs` 和 `product_sku.spec_values` 为 MySQL JSON 列 |
| 多 SKU | 一个 Product 对应多个 ProductSku，列表价格/库存实时由 SKU 聚合 |
| 商品状态 | `Draft(0)` → `OnSale(1)` → `OffShelf(2)` / `Disabled(3)` |
| 前后台分离 | Admin 控制器 (`/api/admin/`) + 前台控制器 (`/api/`)，前台仅返回 `OnSale` 商品 |
| API 格式 | Controller 返回原生对象（非 `ActionResult<T>` 包装），ASP.NET 自动序列化为 JSON |

### 状态转换规则

```
Draft ──→ shelve ──→ OnSale ──→ unshelve ──→ OffShelf
  ↑                      │                       │
  └── enable ──── Disabled ←──── disable ←───────┘
```

- 上架：仅 Draft/OffShelf → OnSale，必须至少有 1 个 SKU
- 下架：仅 OnSale → OffShelf
- 启用：仅 Disabled → Draft
- 禁用：任意状态 → Disabled

## Database

数据库 `my_mall`，连接字符串在 `appsettings.json`：

```
Server=127.0.0.1;Port=3366;Database=my_mall;User=root;Password=mysql123456;
```

初始化：`mysql -u root -p < database/schema.sql`

### 表结构要点

- `product_category` — 自引用树形（`parent_id`）
- `product` — 商品主表，`specs`/`images` 为 JSON，`status` 存 int 枚举值
- `product_sku` — `spec_values` JSON 存规格值组合，`price`/`market_price` 为 `decimal(10,2)`
- `product_image` — 支持商品级（`sku_id` 为空）和 SKU 级图片

Index：product 表按 `category_id`/`status`/`sales_count`/`created_at` 建索引；product_sku 按 `product_id`/`sku_code`/`price`/`stock` 建索引

## Code patterns

### Repository 层

- 所有仓储实现 `repository_interface`（Core 层定义），构造函数注入 `AppDbContext`
- 按价格排序用子查询：`.OrderBy(p => _db.ProductSkus.Where(s => s.ProductId == p.Id).Min(s => (decimal?)s.Price) ?? 0)`
- 批量状态更新用 `ExecuteUpdateAsync`（非追踪高性能）：`.Where().ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true))`
- 修改实体时排除不可改字段：`_db.Entry(entity).Property(x => x.CreatedAt).IsModified = false`

### Service 层

- `ProductService` 用 `System.Text.Json` 序列化/反序列化 JSON 字段，camelCase 策略
- 创建/更新商品时先删全部 SKU（软删除）再批量插入新 SKU
- 详情 DTO 需同时加载 `Category` 和 `Skus` 导航属性

### 添加新功能的步骤

1. `MyApp.Core` — 添加实体 / DTO / 接口
2. `MyApp.Repository` — 实现仓储，在 `AppDbContext.OnModelCreating` 中配置映射和索引
3. `MyApp.Service` — 实现业务逻辑
4. `MyApp.Web` — 添加 Controller，在 `Program.cs` 注册 `AddScoped`
