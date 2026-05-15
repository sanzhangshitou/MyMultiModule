# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run

```bash
# Build the entire solution
dotnet build MyApp.slnx

# Run the web application
dotnet run --project MyApp.Web/MyApp.Web.csproj

# Run with watch (hot reload)
dotnet watch --project MyApp.Web/MyApp.Web.csproj
```

The Web project serves HTTP on `http://localhost:5119` and HTTPS on `https://localhost:7037` (from `launchSettings.json`).

## Project Architecture

This is a **.NET 10** clean-architecture solution using the new `.slnx` format (XML-based solution files, replacing `.sln`).

```
MyApp.Web         → ASP.NET Core Web API (host, endpoints, DI wiring)
MyApp.Service     → Business logic / orchestration layer
MyApp.Repository  → Data access (SqlSugar ORM v5.1.4)
MyApp.Core        → Domain models, interfaces, business rules
MyApp.Common      → Shared utilities, base classes, constants
```

Dependency chain: **Web → Service → Repository → Core → Common**

Each project targets `net10.0` with `ImplicitUsings` and `Nullable` enabled. The solution uses the `<Project Sdk="Microsoft.NET.Sdk">` format for class libraries and `Microsoft.NET.Sdk.Web` for the Web project.

### Key packages

- `SqlSugar` 5.1.4.207 — ORM used by the Repository layer
- `Microsoft.AspNetCore.OpenApi` 10.0.8 — OpenAPI/Swagger support in the Web layer
