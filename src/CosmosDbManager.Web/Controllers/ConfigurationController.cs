using CosmosDbManager.Web.Extensions;
using CosmosDbManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CosmosDbManager.Web.Controllers;

[Route("configuration")]
public sealed class ConfigurationController : Controller
{
    private const string SessionKey = "CosmosDb.Configuration";

    [HttpGet("")]
    public IActionResult Index()
    {
        return View(ConfigurationViewModel.FromDto(HttpContext.Session.GetObject<CosmosDbManager.Application.DTOs.Request.CosmosConfigurationDto>(SessionKey)));
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ConfigurationViewModel model)
    {
        HttpContext.Session.SetObject(SessionKey, model.ToDto());
        TempData["StatusMessage"] = "Configuration saved to session.";
        return RedirectToAction(nameof(Index));
    }
}
