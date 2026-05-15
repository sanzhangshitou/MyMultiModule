# MyMultiModule

基于 .NET 10 的多模块分层架构项目，采用 ASP.NET Core Web API 构建。

## 项目结构

```
MyMultiModule/
├── MyApp.Web/           # 启动项目，ASP.NET Core Web API
├── MyApp.Service/       # 服务层，业务逻辑处理
├── MyApp.Repository/    # 数据访问层，数据库 CRUD
├── MyApp.Core/          # 核心层，实体模型与业务规则
└── MyApp.Common/        # 公共工具层，通用方法
```

## 模块依赖关系

```
Web ──→ Service ──→ Repository ──→ Core ──→ Common
  │                    │            │          │
  └────────────────────┴────────────┴──────────┘
```

| 模块 | 作用 | 依赖 |
|------|------|------|
| MyApp.Web | 启动项目，接收 HTTP 请求，对外提供 API | Service, Core, Common |
| MyApp.Service | 业务逻辑层，编排业务规则 | Repository, Core, Common |
| MyApp.Repository | 数据访问层，基于 SqlSugar ORM 操作数据库 | Core, Common |
| MyApp.Core | 核心层，定义实体类、接口、领域模型 | Common |
| MyApp.Common | 工具层，通用工具类（日志、加密、Excel 等） | — |

## 技术栈

- **框架**: .NET 10.0，启用 Nullable 与 ImplicitUsings
- **ORM**: SqlSugar 5.1.4
- **OpenAPI**: 内置 OpenAPI 文档支持

## 快速开始

```bash
# 构建整个解决方案
dotnet build MyApp.slnx

# 运行 Web 项目
dotnet run --project MyApp.Web/MyApp.Web.csproj

# 热重载模式
dotnet watch --project MyApp.Web/MyApp.Web.csproj
```

启动后访问 `http://localhost:5119` 或 `https://localhost:7037`。

## 添加新模块

```bash
# 创建新的类库项目
dotnet new classlib -n MyApp.NewModule

# 添加到解决方案
dotnet sln add MyApp.NewModule/MyApp.NewModule.csproj

# 按分层规则添加引用
dotnet add MyApp.NewModule reference MyApp.Common
```
