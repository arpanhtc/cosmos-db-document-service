using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.DTOs.Response;
using CosmosDbManager.Application.Interfaces;
using CosmosDbManager.Application.Mappings;
using CosmosDbManager.Domain.Exceptions;
using CosmosDbManager.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CosmosDbManager.Application.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly ICosmosRepository _repository;
    private readonly IValidator<InsertDocumentRequest> _insertValidator;
    private readonly IValidator<UpsertDocumentRequest> _upsertValidator;
    private readonly IValidator<GetDocumentRequest> _getValidator;
    private readonly IValidator<PatchDocumentRequest> _patchValidator;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        ICosmosRepository repository,
        IValidator<InsertDocumentRequest> insertValidator,
        IValidator<UpsertDocumentRequest> upsertValidator,
        IValidator<GetDocumentRequest> getValidator,
        IValidator<PatchDocumentRequest> patchValidator,
        ILogger<DocumentService> logger)
    {
        _repository = repository;
        _insertValidator = insertValidator;
        _upsertValidator = upsertValidator;
        _getValidator = getValidator;
        _patchValidator = patchValidator;
        _logger = logger;
    }

    public async Task<OperationResult<DocumentResponse>> InsertAsync(InsertDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await _insertValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ValidationFailure(validationResult.Errors.Select(x => x.ErrorMessage));
        }

        try
        {
            var configuration = DocumentMapper.ToCosmosConfiguration(request.Configuration!);
            var document = DocumentMapper.ToDomainDocument(request.Id, request.PartitionKeyValue, request.JsonPayload);

            var created = await _repository.InsertAsync(configuration, document, ct);
            _logger.LogInformation("Insert completed for document {DocumentId}.", request.Id);

            return OperationResult<DocumentResponse>.Success(DocumentMapper.ToDocumentResponse(created));
        }
        catch (Exception exception)
        {
            return HandleException(exception, "Insert");
        }
    }

    public async Task<OperationResult<DocumentResponse>> UpsertAsync(UpsertDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await _upsertValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ValidationFailure(validationResult.Errors.Select(x => x.ErrorMessage));
        }

        try
        {
            var configuration = DocumentMapper.ToCosmosConfiguration(request.Configuration!);
            var document = DocumentMapper.ToDomainDocument(request.Id, request.PartitionKeyValue, request.JsonPayload);

            var upserted = await _repository.UpsertAsync(configuration, document, ct);
            _logger.LogInformation("Upsert completed for document {DocumentId}.", request.Id);

            return OperationResult<DocumentResponse>.Success(DocumentMapper.ToDocumentResponse(upserted));
        }
        catch (Exception exception)
        {
            return HandleException(exception, "Upsert");
        }
    }

    public async Task<OperationResult<DocumentResponse>> GetAsync(GetDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await _getValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ValidationFailure(validationResult.Errors.Select(x => x.ErrorMessage));
        }

        try
        {
            var configuration = DocumentMapper.ToCosmosConfiguration(request.Configuration!);
            var found = await _repository.GetAsync(configuration, request.Id, request.PartitionKeyValue, ct);
            _logger.LogInformation("Get completed for document {DocumentId}.", request.Id);

            return OperationResult<DocumentResponse>.Success(DocumentMapper.ToDocumentResponse(found));
        }
        catch (Exception exception)
        {
            return HandleException(exception, "Get");
        }
    }

    public async Task<OperationResult<DocumentResponse>> PatchAsync(PatchDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await _patchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ValidationFailure(validationResult.Errors.Select(x => x.ErrorMessage));
        }

        try
        {
            var configuration = DocumentMapper.ToCosmosConfiguration(request.Configuration!);
            var operations = DocumentMapper.ToPatchOperations(request.Operations);
            var patched = await _repository.PatchAsync(configuration, request.Id, request.PartitionKeyValue, operations, ct);
            _logger.LogInformation(
                "Patch completed for document {DocumentId} with {OperationCount} operations.",
                request.Id,
                request.Operations.Count);

            return OperationResult<DocumentResponse>.Success(DocumentMapper.ToDocumentResponse(patched));
        }
        catch (Exception exception)
        {
            return HandleException(exception, "Patch");
        }
    }

    private static OperationResult<DocumentResponse> ValidationFailure(IEnumerable<string> errors)
    {
        return OperationResult<DocumentResponse>.Failure(
            $"Validation failed: {string.Join(" ", errors)}",
            "VALIDATION_ERROR");
    }

    private OperationResult<DocumentResponse> HandleException(Exception exception, string operationName)
    {
        _logger.LogError(exception, "Document operation {OperationName} failed.", operationName);

        return exception switch
        {
            DocumentNotFoundException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "DOCUMENT_NOT_FOUND"),
            DocumentConflictException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "DOCUMENT_CONFLICT"),
            InvalidConfigurationException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "INVALID_CONFIGURATION"),
            CosmosRateLimitException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "COSMOS_RATE_LIMIT"),
            CosmosServiceUnavailableException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "COSMOS_SERVICE_UNAVAILABLE"),
            CosmosDbException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "COSMOS_DB_ERROR"),
            ArgumentException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "VALIDATION_ERROR"),
            FormatException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "VALIDATION_ERROR"),
            JsonException ex => OperationResult<DocumentResponse>.Failure(ex.Message, "VALIDATION_ERROR"),
            _ => OperationResult<DocumentResponse>.Failure(
                "An unexpected error occurred while processing the request.",
                "UNEXPECTED_ERROR")
        };
    }
}
