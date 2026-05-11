using CosmosDbManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CosmosDbManager.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? requestId = null)
    {
        return View(new ErrorViewModel
        {
            RequestId = requestId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
