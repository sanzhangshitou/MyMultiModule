# MyMultiModule

基于 .NET 10 多模块分层架构的商城系统 — ASP.NET Core Web API + EF Core + MySQL。支持多 SKU 商品管理、分类树、前后台接口分离。

## 项目结构

```
MyMultiModule/
├── MyApp.Web/                         # 启动层 — 控制器、DI、中间件
│   ├── Controllers/
│   │   ├── HomeController.cs                   # GET / 返回 API 概览
│   │   ├── AdminProductsController.cs           # 后台商品 CRUD + 上下架
│   │   ├── AdminCategoriesController.cs         # 后台分类 CRUD
│   │   ├── ProductsController.cs                # 前台商品列表/详情
│   │   └── CategoriesController.cs              # 前台分类树
│   ├── Program.cs                               # 入口：注册 EF Core + DI
│   └── appsettings.json                         # 连接字符串
├── MyApp.Service/                      # 业务逻辑层
│   ├── ProductService.cs                        # 状态校验、JSON序列化、SKU组装
│   └── ProductCategoryService.cs                # 递归构建分类树
├── MyApp.Repository/                   # 数据访问层
│   ├── AppDbContext.cs                          # DbContext、索引、软删除过滤器
│   ├── ProductRepository.cs                     # 分页/排序/子查询价格排序
│   └── ProductCategoryRepository.cs             # 分类查询
├── MyApp.Core/                         # 核心层（不依赖ORM实现）
│   ├── Entities/
│   │   ├── Product.cs / ProductCategory.cs / ProductSku.cs / ProductImage.cs
│   ├── DTOs/Product/
│   │   ├── ProductDto.cs / ProductDetailDto.cs / ProductCreateInput.cs
│   │   ├── ProductUpdateInput.cs / ProductListRequest.cs / ProductSpecDto.cs
│   │   ├── ProductCategoryDto.cs / SkuDto.cs / SkuInput.cs
│   ├── DTOs/Common/PagedResult.cs
│   ├── Enums/ProductStatus.cs
│   └── Interfaces/
│       ├── IProductRepository.cs / IProductCategoryRepository.cs
│       └── IProductService.cs / IProductCategoryService.cs
├── MyApp.Common/                       # 公共工具（预留）
├── database/
│   └── schema.sql                      # 建库脚本 + 初始分类数据
└── MyApp.slnx
```

## 分层依赖

```
Web → Service → Repository → Core → Common
```

| 层 | 职责 | 项目引用 |
|----|------|----------|
| Web | HTTP 路由、参数绑定、返回 JSON | Service, Core, Common |
| Service | 业务编排、状态校验 | Repository, Core, Common |
| Repository | EF Core 查询、实体映射 | Core, Common |
| Core | 实体、DTO、枚举、接口 | Common |
| Common | 共享工具类 | — |

## 技术栈

| 组件 | 说明 |
|------|------|
| .NET 10.0 | Nullable enabled, ImplicitUsings enabled |
| EF Core 9.0.5 | ORM |
| Pomelo.EntityFrameworkCore.MySql 9.0 | MySQL 驱动 |
| EFCore.NamingConventions 9.0 | CamelCase → snake_case 自动映射 |
| Microsoft.AspNetCore.OpenApi 10.0.8 | OpenAPI 文档 |

## 快速开始

```bash
# 1. 初始化数据库
mysql -u root -p < database/schema.sql

# 2. 构建
dotnet build MyApp.slnx

# 3. 启动
dotnet run --project MyApp.Web/MyApp.Web.csproj
```

| 地址 | 说明 |
|------|------|
| `http://localhost:5119` | 首页（JSON） |
| `http://localhost:5119/api/products` | 前台商品列表 |
| `http://localhost:5119/api/admin/products` | 后台商品管理 |

连接字符串在 `MyApp.Web/appsettings.json`：

```json
"ConnectionStrings": {
  "Default": "Server=127.0.0.1;Port=3366;Database=my_mall;User=root;Password=mysql123456;"
}
```

## 数据库

| 表 | 说明 |
|----|------|
| `product_category` | 分类树（parent_id 自引用） |
| `product` | 商品主表（name, category_id, status, specs(JSON), images(JSON)） |
| `product_sku` | 多规格 SKU（spec_values(JSON), price, stock） |
| `product_image` | 商品图片（product_id, sku_id 可为空） |

所有表均含 `created_at`、`updated_at`、`is_deleted`（软删除）。`AppDbContext` 全局过滤器自动排除 `is_deleted = 1` 的记录。

列名由 C# `CamelCase` → DB `snake_case` 自动映射（`CategoryId` → `category_id`）。

## 商品状态机

```
                  上架              下架
Draft(0) ────────────→ OnSale(1) ────────────→ OffShelf(2)
   ↑                       │                        │
   │                       │                        │
   └── 启用 ←── Disabled(3) ←── 禁用 ←─────────────┘
```

| 状态 | 值 | 说明 |
|------|----|------|
| Draft | 0 | 草稿，不在前台展示 |
| OnSale | 1 | 上架，前台可见 |
| OffShelf | 2 | 下架，可重新上架 |
| Disabled | 3 | 禁用，启用后回 Draft |

状态转换校验：

- 上架：仅 Draft / OffShelf → OnSale，必须有至少 1 个 SKU
- 下架：仅 OnSale → OffShelf
- 启用：仅 Disabled → Draft
- 禁用：任意状态 → Disabled

## API 接口

### 首页

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/` | 返回 API 名称、版本、接口列表 |

### 管理后台 `/api/admin/`

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/admin/products` | 商品列表 |
| GET | `/api/admin/products/{id}` | 商品详情 |
| POST | `/api/admin/products` | 新增商品 |
| PUT | `/api/admin/products/{id}` | 修改商品 |
| DELETE | `/api/admin/products/{id}` | 软删除 |
| PUT | `/api/admin/products/{id}/shelve` | 上架 |
| PUT | `/api/admin/products/{id}/unshelve` | 下架 |
| PUT | `/api/admin/products/{id}/enable` | 启用 |
| PUT | `/api/admin/products/{id}/disable` | 禁用 |
| GET | `/api/admin/categories` | 分类树 |
| POST | `/api/admin/categories` | 新增分类 |
| PUT | `/api/admin/categories/{id}` | 修改分类 |
| DELETE | `/api/admin/categories/{id}` | 删除分类 |

商品列表查询参数：

| 参数 | 类型 | 说明 |
|------|------|------|
| keyword | string | 商品名称模糊搜索 |
| categoryId | long | 分类筛选 |
| status | int | 0:草稿 1:上架 2:下架 3:禁用 |
| page | int | 页码，默认 1 |
| pageSize | int | 每页条数，默认 20 |
| sortField | string | price / sales / created |
| sortDesc | bool | 是否降序 |

新增商品示例：

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
      "price": 3999.00,
      "marketPrice": 4599.00,
      "stock": 100,
      "sortOrder": 1
    },
    {
      "skuCode": "X1-WHT-256",
      "specValues": { "颜色": "极光白", "存储": "256GB" },
      "price": 4599.00,
      "marketPrice": 5199.00,
      "stock": 50,
      "sortOrder": 2
    }
  ]
}
```

列表响应：

```json
{
  "total": 100,
  "page": 1,
  "pageSize": 20,
  "list": [
    {
      "id": 1,
      "categoryId": 6,
      "categoryName": "手机",
      "name": "智能手机 X1",
      "subtitle": "旗舰新品",
      "mainImage": "/img/phone.jpg",
      "status": 1,
      "unit": "部",
      "minPrice": 3999.00,
      "maxPrice": 4599.00,
      "totalStock": 150,
      "salesCount": 0,
      "sortOrder": 1,
      "createdAt": "2026-05-15T10:00:00"
    }
  ]
}
```

分类树响应：

```json
[
  {
    "id": 1,
    "name": "服装",
    "parentId": 0,
    "children": [
      { "id": 4, "name": "男装", "parentId": 1, "children": [] },
      { "id": 5, "name": "女装", "parentId": 1, "children": [] }
    ]
  },
  {
    "id": 2,
    "name": "电子产品",
    "parentId": 0,
    "children": [
      { "id": 6, "name": "手机", "parentId": 2, "children": [] },
      { "id": 7, "name": "电脑", "parentId": 2, "children": [] }
    ]
  }
]
```

### 商城前台 `/api/`

仅返回上架状态商品。

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/products` | 商品列表 |
| GET | `/api/products/{id}` | 商品详情 |
| GET | `/api/categories` | 分类树 |

## 多 SKU 模型

```
Product (1) → (N) ProductSku

product.specs (JSON) — 规格定义
  [{"Name":"颜色","Values":["红","蓝"]}, {"Name":"尺寸","Values":["S","M"]}]

product_sku.spec_values (JSON) — 规格值组合
  {"颜色":"红", "尺寸":"M"}
```

列表接口的 `minPrice` / `maxPrice` / `totalStock` 由该商品所有 SKU 实时聚合：

```
minPrice   = MIN(sku.price)
maxPrice   = MAX(sku.price)  
totalStock = SUM(sku.stock)
```

按价格排序时使用 SKU 最低价排序。
