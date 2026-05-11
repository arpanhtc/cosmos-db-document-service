using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.DTOs.Response;
using CosmosDbManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CosmosDbManager.Web.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentApiController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentApiController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("insert")]
    public async Task<IActionResult> Insert([FromBody] InsertDocumentRequest request, CancellationToken ct)
    {
        return ToActionResult(await _documentService.InsertAsync(request, ct));
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertDocumentRequest request, CancellationToken ct)
    {
        return ToActionResult(await _documentService.UpsertAsync(request, ct));
    }

    [HttpPost("get")]
    public async Task<IActionResult> Get([FromBody] GetDocumentRequest request, CancellationToken ct)
    {
        return ToActionResult(await _documentService.GetAsync(request, ct));
    }

    [HttpPost("patch")]
    public async Task<IActionResult> Patch([FromBody] PatchDocumentRequest request, CancellationToken ct)
    {
        return ToActionResult(await _documentService.PatchAsync(request, ct));
    }

    private static IActionResult ToActionResult(OperationResult<DocumentResponse> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        return result.ErrorCode switch
        {
            "VALIDATION_ERROR" => new BadRequestObjectResult(result),
            "DOCUMENT_NOT_FOUND" => new NotFoundObjectResult(result),
            "DOCUMENT_CONFLICT" => new ConflictObjectResult(result),
            "INVALID_CONFIGURATION" => new BadRequestObjectResult(result),
            "COSMOS_RATE_LIMIT" => new ObjectResult(result) { StatusCode = StatusCodes.Status429TooManyRequests },
            "COSMOS_SERVICE_UNAVAILABLE" => new ObjectResult(result) { StatusCode = StatusCodes.Status503ServiceUnavailable },
            _ => new ObjectResult(result) { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }
}
