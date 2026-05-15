# MyMultiModule

基于 .NET 10 多模块分层架构的商城系统 — ASP.NET Core Web API + EF Core + MySQL + Redis。

- 多 SKU 商品管理（规格定义、库存、价格）
- 分类树（自引用、递归构建、Redis 缓存）
- 管理后台 + 商城前台接口分离
- 软删除、JSON 字段、全局查询过滤器

## 项目结构

```
MyMultiModule/
├── MyApp.Web/                # 启动层 — 控制器、DI、中间件
│   ├── Controllers/          # Home / AdminProducts / AdminCategories / Products / Categories
│   ├── Program.cs            # 应用入口
│   └── appsettings.json      # MySQL + Redis 连接字符串
├── MyApp.Service/            # 业务逻辑层
│   ├── ProductService.cs     # 商品 CRUD、状态校验、SKU 组装
│   └── ProductCategoryService.cs  # 分类树构建 + Redis 缓存
├── MyApp.Repository/         # 数据访问层
│   ├── AppDbContext.cs       # EF Core DbContext（索引、过滤器、关系映射）
│   ├── ProductRepository.cs
│   └── ProductCategoryRepository.cs
├── MyApp.Core/               # 核心层（不依赖 ORM 实现）
│   ├── Entities/             # Product / ProductCategory / ProductSku / ProductImage
│   ├── DTOs/Product/         # 入参/出参 DTO（9 个）
│   ├── DTOs/Common/          # PagedResult<T> 通用分页
│   ├── Enums/                # ProductStatus
│   └── Interfaces/           # 仓储接口 + 服务接口
├── MyApp.Common/             # 公共工具（预留）
├── database/schema.sql       # 建库脚本 + 初始分类数据
└── MyApp.slnx
```

## 分层依赖

```
Web → Service → Repository → Core → Common
```

| 层 | 职责 | 项目引用 |
|----|------|----------|
| Web | HTTP 路由、参数绑定、JSON 响应 | Service, Core, Common |
| Service | 业务编排、状态校验、Redis 缓存 | Repository, Core, Common |
| Repository | EF Core 查询、实体映射 | Core, Common |
| Core | 实体、DTO、枚举、接口 | Common |
| Common | 共享工具类 | — |

## 技术栈

| 组件 | 说明 |
|------|------|
| .NET 10.0 | Nullable & ImplicitUsings enabled |
| EF Core 9.0.5 | ORM |
| Pomelo.EntityFrameworkCore.MySql 9.0 | MySQL 驱动 |
| EFCore.NamingConventions 9.0 | `CamelCase` → `snake_case` 自动映射 |
| StackExchangeRedis 9.0.5 | Redis 分布式缓存 |
| Microsoft.AspNetCore.OpenApi 10.0.8 | `/openapi/v1.json` |

## 快速开始

```bash
# 初始化数据库
mysql -u root -p < database/schema.sql

# 构建
dotnet build MyApp.slnx

# 启动
dotnet run --project MyApp.Web/MyApp.Web.csproj

# 格式化代码
dotnet format MyApp.slnx
```

| 地址 | 说明 |
|------|------|
| `http://localhost:5119` | 首页（API 概览 JSON） |
| `http://localhost:5119/openapi/v1.json` | OpenAPI 文档 |
| `http://localhost:5119/api/products` | 前台商品列表 |
| `http://localhost:5119/api/admin/products` | 后台商品管理 |

### 连接字符串

`MyApp.Web/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "Default": "Server=127.0.0.1;Port=3366;Database=my_mall;User=root;Password=mysql123456;",
    "Redis": "127.0.0.1:6699,password=redis123456,defaultDatabase=6"
  }
}
```

## 数据库

| 表 | 说明 | JSON 列 |
|----|------|---------|
| `product_category` | 分类树（parent_id 自引用） | — |
| `product` | 商品主表 | specs, images |
| `product_sku` | 多规格 SKU | spec_values |
| `product_image` | 商品图片（商品级/SKU 级） | — |

所有表均含 `created_at`、`updated_at`、`is_deleted`。`AppDbContext` 通过 `HasQueryFilter(e => !e.IsDeleted)` 全局排除软删除记录。

列名由 `EFCore.NamingConventions.UseSnakeCaseNamingConvention()` 自动转换：

| C# 属性 | 数据库列 |
|----------|----------|
| `CategoryId` | `category_id` |
| `SkuCode` | `sku_code` |
| `IsEnabled` | `is_enabled` |
| `SalesCount` | `sales_count` |
| `MainImage` | `main_image` |

## 商品状态机

```
Draft(0) ── shelve ──→ OnSale(1) ── unshelve ──→ OffShelf(2)
   ↑                       │                        │
   └── enable ──── Disabled(3) ←──── disable ←─────┘
```

| 状态 | 值 | 说明 |
|------|----|------|
| Draft | 0 | 草稿，不在前台展示 |
| OnSale | 1 | 上架，前台可见 |
| OffShelf | 2 | 下架，可重新上架 |
| Disabled | 3 | 禁用，启用后回到 Draft |

**状态转换校验：**

| 操作 | 路由后缀 | 前置条件 | 目标状态 |
|------|---------|----------|---------|
| 上架 | `/shelve` | Draft 或 OffShelf | OnSale |
| 下架 | `/unshelve` | OnSale | OffShelf |
| 启用 | `/enable` | Disabled | Draft |
| 禁用 | `/disable` | 任意状态 | Disabled |

上架额外要求：至少存在 1 个 SKU。

## API 接口

### 首页

```
GET /                           →  { name, version, endpoints[] }
```

### 管理后台 `/api/admin/`

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/admin/products` | 商品列表 |
| GET | `/api/admin/products/{id}` | 商品详情（含 SKU） |
| POST | `/api/admin/products` | 新增商品（含 SKU） |
| PUT | `/api/admin/products/{id}` | 修改商品（替换所有 SKU） |
| DELETE | `/api/admin/products/{id}` | 软删除 |
| PUT | `/api/admin/products/{id}/shelve` | 上架 |
| PUT | `/api/admin/products/{id}/unshelve` | 下架 |
| PUT | `/api/admin/products/{id}/enable` | 启用 |
| PUT | `/api/admin/products/{id}/disable` | 禁用 |
| GET | `/api/admin/categories` | 分类树（Redis 缓存） |
| POST | `/api/admin/categories` | 新增分类（失效缓存） |
| PUT | `/api/admin/categories/{id}` | 修改分类（失效缓存） |
| DELETE | `/api/admin/categories/{id}` | 删除分类（失效缓存） |

**列表查询参数**（`GET /api/admin/products`）：

| 参数 | 类型 | 默认 | 说明 |
|------|------|------|------|
| `keyword` | string | — | 商品名称模糊搜索 |
| `categoryId` | long | — | 分类筛选 |
| `status` | int | — | 0:Draft 1:OnSale 2:OffShelf 3:Disabled |
| `page` | int | 1 | 页码 |
| `pageSize` | int | 20 | 每页条数 |
| `sortField` | string | — | `price` / `sales` / `created` |
| `sortDesc` | bool | false | 是否降序 |

**新增商品请求：**

```json
{
  "categoryId": 6,
  "name": "智能手机 X1",
  "subtitle": "旗舰新品",
  "description": "<p>详情HTML</p>",
  "mainImage": "/img/phone.jpg",
  "images": ["/img/p1.jpg", "/img/p2.jpg"],
  "specs": [
    { "name": "颜色", "values": ["星空黑", "极光白"] },
    { "name": "存储", "values": ["128GB", "256GB"] }
  ],
  "unit": "部",
  "sortOrder": 1,
  "skus": [
    {
      "skuCode": "X1-BLK-128",
      "specValues": { "颜色": "星空黑", "存储": "128GB" },
      "price": 3999.00, "marketPrice": 4599.00,
      "stock": 100, "sortOrder": 1
    },
    {
      "skuCode": "X1-WHT-256",
      "specValues": { "颜色": "极光白", "存储": "256GB" },
      "price": 4599.00, "marketPrice": 5199.00,
      "stock": 50, "sortOrder": 2
    }
  ]
}
```

**列表响应：**

```json
{
  "total": 100, "page": 1, "pageSize": 20,
  "list": [
    {
      "id": 1, "categoryId": 6, "categoryName": "手机",
      "name": "智能手机 X1", "subtitle": "旗舰新品",
      "mainImage": "/img/phone.jpg", "status": 1, "unit": "部",
      "minPrice": 3999.00, "maxPrice": 4599.00,
      "totalStock": 150, "salesCount": 0, "sortOrder": 1,
      "createdAt": "2026-05-15T10:00:00"
    }
  ]
}
```

**分类树响应：**

```json
[
  { "id": 1, "name": "服装", "parentId": 0,
    "children": [
      { "id": 4, "name": "男装", "parentId": 1, "children": [] }
    ]
  }
]
```

### 商城前台 `/api/`

仅返回 `status = 1 (OnSale)` 的商品。

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/products` | 商品列表（参数同后台） |
| GET | `/api/products/{id}` | 商品详情 |
| GET | `/api/categories` | 分类树（Redis 缓存） |

## 多 SKU 模型

```
Product (1) ─── (N) ProductSku
```

- `product.specs`（JSON）定义该商品有哪些规格维度
- `product_sku.spec_values`（JSON）记录每个 SKU 的具体规格值组合
- 每个 SKU 有独立的价格、市场价、库存、图片

列表接口的价格和库存由 SKU 实时聚合：

| 字段 | 聚合方式 |
|------|----------|
| `minPrice` | MIN(sku.price) |
| `maxPrice` | MAX(sku.price) |
| `totalStock` | SUM(sku.stock) |

排序 `sortField=price` 按 SKU 最低价排序。

## Redis 缓存

### 分类树缓存

`ProductCategoryService` 对分类树做了 Redis 缓存：

```
读流程：GetTreeAsync()
  Redis GET "categories:tree"
    ├── 命中 → 直接返回
    └── 未命中 → 查库构建树 → Redis SET 回填 → 返回

写流程：CreateAsync / UpdateAsync / DeleteAsync
  执行数据库变更 → Redis DEL "categories:tree"
```

| 配置项 | 值 |
|--------|-----|
| Key | `categories:tree` |
| 序列化 | System.Text.Json（camelCase） |
| 过期时间 | 绝对过期 1 小时 |
| 失效策略 | 写时删除（Cache-Aside） |
| Redis 地址 | `127.0.0.1:6699`，密码 `redis123456`，DB 6 |

分类是典型的读多写少数据，使用 Cache-Aside 策略在保证一致性的同时减少数据库查询。

## 代码风格

```bash
dotnet format MyApp.slnx                  # 自动修复
dotnet format MyApp.slnx --verify-no-changes  # 仅检查
```

`.editorconfig` 主要规则：

- 缩进：4 空格（.cs），2 空格（.json/.sql/.csproj）
- 换行符：CRLF（.cs），LF（.sh）
- 私有字段：`_camelCase` 前缀
- async 方法：必须 `Async` 后缀
- 命名空间：文件范围声明（file-scoped）
- 优先使用 `var`、简化 `new()`、auto-property

## 架构要点

- **软删除**：所有实体通过 `is_deleted` 标记，`AppDbContext` 全局过滤器自动排除已删除行
- **仓储接口在 Core**：接口定义在 Core 层，实现细节在 Repository 层，Service 层只依赖接口
- **JSON 序列化**：`ProductService` 内部用 `System.Text.Json`（camelCase 策略），规格和轮播图直接以 JSON 存入 MySQL
- **SKU 更新策略**：编辑商品时先软删除全部旧 SKU 再批量插入新 SKU，避免逐个比对
- **状态更新**：批量操作用 `ExecuteUpdateAsync`（非追踪、高性能 SQL），单条操作用 `FindAsync` + SaveChanges
- **分类缓存**：Redis Cache-Aside 策略，读时缓存、写时删除，key = `categories:tree`
- **全局查询过滤器**：`HasQueryFilter(e => !e.IsDeleted)` 使得所有 LINQ 查询自动排除已删除记录
