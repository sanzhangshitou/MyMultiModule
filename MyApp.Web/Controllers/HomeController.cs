using Microsoft.AspNetCore.Mvc;

namespace MyApp.Web.Controllers;

/// <summary>
/// 默认首页，返回 API 基本信息
/// </summary>
[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    /// <summary>API 首页，返回服务信息 JSON</summary>
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            name = "MyMultiModule API",
            version = "1.0.0",
            description = "多模块分层架构商城系统",
            endpoints = new[]
            {
                new { group = "管理后台-商品", path = "/api/admin/products", methods = "GET POST" },
                new { group = "管理后台-商品", path = "/api/admin/products/{id}", methods = "GET PUT DELETE" },
                new { group = "管理后台-商品", path = "/api/admin/products/{id}/shelve", methods = "PUT" },
                new { group = "管理后台-商品", path = "/api/admin/products/{id}/unshelve", methods = "PUT" },
                new { group = "管理后台-商品", path = "/api/admin/products/{id}/enable", methods = "PUT" },
                new { group = "管理后台-商品", path = "/api/admin/products/{id}/disable", methods = "PUT" },
                new { group = "管理后台-分类", path = "/api/admin/categories", methods = "GET POST" },
                new { group = "管理后台-分类", path = "/api/admin/categories/{id}", methods = "GET PUT DELETE" },
                new { group = "商城前台-商品", path = "/api/products", methods = "GET" },
                new { group = "商城前台-商品", path = "/api/products/{id}", methods = "GET" },
                new { group = "商城前台-分类", path = "/api/categories", methods = "GET" }
            }
        });
    }
}
