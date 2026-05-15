# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build MyApp.slnx                              # build
dotnet run --project MyApp.Web/MyApp.Web.csproj      # run (http://localhost:5119)
dotnet watch --project MyApp.Web/MyApp.Web.csproj    # hot reload
dotnet format MyApp.slnx                              # auto-fix code style
dotnet format MyApp.slnx --verify-no-changes          # check only
mysql -u root -p < database/schema.sql               # init database
```

## Architecture

Clean Architecture 5-layer: `Web → Service → Repository → Core → Common`.

- **Web** — MVC Controllers (not Minimal API). All DI registration is inline in `Program.cs` — no extension methods.
- **Service** — Business logic, JSON serialization of spec/image fields, state validation, Redis caching. References `IDistributedCache` for caching.
- **Repository** — EF Core via `AppDbContext`. Soft-delete filters are global (`HasQueryFilter`). All repository interfaces defined in Core layer.
- **Core** — Entities use `[Table]`/`[Key]`/`[ForeignKey]` DataAnnotations only. No ORM implementation dependency. EF Core Abstractions package for attributes.
- **Common** — Currently empty, reserved for shared utilities.

### Key naming rules (enforced by `.editorconfig`)

- Private fields: `_camelCase` prefix
- Async methods: must have `Async` suffix
- File-scoped namespaces
- Indent: 4 spaces (.cs), 2 spaces (.json/.sql/.csproj)

### ORM: EF Core + Pomelo MySQL

- Table/column naming: `EFCore.NamingConventions.UseSnakeCaseNamingConvention()` — `CategoryId` → `category_id`
- JSON columns: `product.Images`, `product.Specs`, `product_sku.SpecValues` — explicitly mapped as `json` type in both entity `[Column(TypeName)]` and DbContext `HasColumnType("json")`
- Soft delete: all tables have `is_deleted` (BIT), excluded globally via `HasQueryFilter(e => !e.IsDeleted)`
- Price columns: `decimal(10,2)` — both entity `[Column(TypeName)]` and fluent API

### Product status machine

```
Draft(0) → shelve → OnSale(1) → unshelve → OffShelf(2)
   ↑                     │                       │
   └── enable ←── Disabled(3) ←──── disable ←───┘
```

Status transitions enforced in `ProductService`:
- `ShelveAsync`: Draft/OffShelf → OnSale (requires ≥1 SKU)
- `UnshelveAsync`: OnSale → OffShelf
- `EnableAsync`: Disabled → Draft
- `DisableAsync`: any → Disabled

### Redis caching (ProductCategoryService)

Cache-Aside pattern for category tree:
- **Read**: check `categories:tree` → hit: return; miss: query DB, build tree, set cache (1h absolute), return
- **Write** (create/update/delete): perform DB change → `RemoveAsync("categories:tree")`
- Redis config: connection string `Redis` in appsettings, database 6 (`defaultDatabase=6`)

### Repository patterns

- Query with soft-delete: already filtered globally, no need to add `!IsDeleted` in LINQ
- Pagination: `Skip((page-1)*pageSize).Take(pageSize)`
- Sort by price: subquery `_db.ProductSkus.Where(s => s.ProductId == p.Id).Min(s => (decimal?)s.Price) ?? 0`
- Batch updates: use `ExecuteUpdateAsync` for performance (status changes, soft-delete SKUs)
- Single entity update: `FindAsync` then set fields, `SaveChangesAsync`
- Protecting read-only fields when updating: `_db.Entry(entity).Property(x => x.CreatedAt).IsModified = false`
- SKU replacement: service calls `DeleteSkusByProductIdAsync` (soft-delete all) then `InsertSkuBatchAsync` (add new)

### Adding a new feature

1. **Core**: add entity/DTO/interface. Entities use `[Table]`/`[Key]`/`[ForeignKey]` only — no EF Core fluent API in Core.
2. **Repository**: implement interface. Register entity in `AppDbContext.OnModelCreating` (indexes, relationships, query filters).
3. **Service**: implement business logic. Inject `IDistributedCache` if caching needed.
4. **Web**: add Controller, register `AddScoped<I, Impl>` in `Program.cs`.
