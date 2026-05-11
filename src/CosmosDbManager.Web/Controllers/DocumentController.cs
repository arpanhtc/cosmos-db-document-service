using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.DTOs.Response;
using CosmosDbManager.Application.Interfaces;
using CosmosDbManager.Web.Extensions;
using CosmosDbManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CosmosDbManager.Web.Controllers;

[Route("document")]
public sealed class DocumentController : Controller
{
    private const string SessionKey = "CosmosDb.Configuration";
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("insert")]
    public IActionResult Insert()
    {
        return View(new InsertDocumentViewModel
        {
            Configuration = LoadConfiguration()
        });
    }

    [HttpPost("insert")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Insert(InsertDocumentViewModel model, CancellationToken ct)
    {
        PersistConfiguration(model.Configuration);
        model.Result = OperationResultViewModel.From(await _documentService.InsertAsync(ToRequest(model), ct));
        return View(model);
    }

    [HttpGet("upsert")]
    public IActionResult Upsert()
    {
        return View(new UpsertDocumentViewModel
        {
            Configuration = LoadConfiguration()
        });
    }

    [HttpPost("upsert")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(UpsertDocumentViewModel model, CancellationToken ct)
    {
        PersistConfiguration(model.Configuration);
        model.Result = OperationResultViewModel.From(await _documentService.UpsertAsync(ToRequest(model), ct));
        return View(model);
    }

    [HttpGet("get")]
    public IActionResult Get()
    {
        return View(new GetDocumentViewModel
        {
            Configuration = LoadConfiguration()
        });
    }

    [HttpPost("get")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Get(GetDocumentViewModel model, CancellationToken ct)
    {
        PersistConfiguration(model.Configuration);
        model.Result = OperationResultViewModel.From(await _documentService.GetAsync(ToRequest(model), ct));
        return View(model);
    }

    [HttpGet("patch")]
    public IActionResult Patch()
    {
        return View(new PatchDocumentViewModel
        {
            Configuration = LoadConfiguration()
        });
    }

    [HttpPost("patch")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Patch(PatchDocumentViewModel model, CancellationToken ct)
    {
        model.Operations ??= [new PatchOperationViewModel()];
        PersistConfiguration(model.Configuration);
        model.Result = OperationResultViewModel.From(await _documentService.PatchAsync(ToRequest(model), ct));
        return View(model);
    }

    private static InsertDocumentRequest ToRequest(InsertDocumentViewModel model)
    {
        return new InsertDocumentRequest
        {
            Configuration = model.Configuration.ToDto(),
            Id = model.Id,
            PartitionKeyValue = model.PartitionKeyValue,
            JsonPayload = model.JsonPayload
        };
    }

    private static UpsertDocumentRequest ToRequest(UpsertDocumentViewModel model)
    {
        return new UpsertDocumentRequest
        {
            Configuration = model.Configuration.ToDto(),
            Id = model.Id,
            PartitionKeyValue = model.PartitionKeyValue,
            JsonPayload = model.JsonPayload
        };
    }

    private static GetDocumentRequest ToRequest(GetDocumentViewModel model)
    {
        return new GetDocumentRequest
        {
            Configuration = model.Configuration.ToDto(),
            Id = model.Id,
            PartitionKeyValue = model.PartitionKeyValue
        };
    }

    private static PatchDocumentRequest ToRequest(PatchDocumentViewModel model)
    {
        return new PatchDocumentRequest
        {
            Configuration = model.Configuration.ToDto(),
            Id = model.Id,
            PartitionKeyValue = model.PartitionKeyValue,
            Operations = model.Operations.Select(operation => new PatchOperationDto
            {
                OperationType = operation.OperationType,
                Path = operation.Path,
                Value = string.IsNullOrWhiteSpace(operation.Value) ? null : operation.Value
            }).ToList()
        };
    }

    private ConfigurationViewModel LoadConfiguration()
    {
        return ConfigurationViewModel.FromDto(HttpContext.Session.GetObject<CosmosConfigurationDto>(SessionKey));
    }

    private void PersistConfiguration(ConfigurationViewModel configuration)
    {
        HttpContext.Session.SetObject(SessionKey, configuration.ToDto());
    }
}
