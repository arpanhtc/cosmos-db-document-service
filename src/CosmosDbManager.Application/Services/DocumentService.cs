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

    /// <summary>
    /// Creates a new document in Cosmos DB.
    /// </summary>
    public async Task<OperationResult<DocumentResponse>> InsertAsync(InsertDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await ValidateRequestAsync(_insertValidator, request, ct);
        if (validationResult != null)
        {
            return validationResult;
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

    /// <summary>
    /// Creates or updates a document in Cosmos DB.
    /// </summary>
    public async Task<OperationResult<DocumentResponse>> UpsertAsync(UpsertDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await ValidateRequestAsync(_upsertValidator, request, ct);
        if (validationResult != null)
        {
            return validationResult;
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

    /// <summary>
    /// Retrieves a document from Cosmos DB.
    /// </summary>
    public async Task<OperationResult<DocumentResponse>> GetAsync(GetDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await ValidateRequestAsync(_getValidator, request, ct);
        if (validationResult != null)
        {
            return validationResult;
        }

        try
        {
            var configuration = DocumentMapper.ToCosmosConfiguration(request.Configuration!);
            var document = await _repository.GetAsync(configuration, request.Id, request.PartitionKeyValue, ct);
            _logger.LogInformation("Get completed for document {DocumentId}.", request.Id);

            return OperationResult<DocumentResponse>.Success(DocumentMapper.ToDocumentResponse(document));
        }
        catch (Exception exception)
        {
            return HandleException(exception, "Get");
        }
    }

    /// <summary>
    /// Updates specific fields in a Cosmos DB document.
    /// </summary>
    public async Task<OperationResult<DocumentResponse>> PatchAsync(PatchDocumentRequest request, CancellationToken ct = default)
    {
        var validationResult = await ValidateRequestAsync(_patchValidator, request, ct);
        if (validationResult != null)
        {
            return validationResult;
        }

        try
        {
            var configuration = DocumentMapper.ToCosmosConfiguration(request.Configuration!);
            var operations = DocumentMapper.ToPatchOperations(request.Operations);
            var document = await _repository.PatchAsync(configuration, request.Id, request.PartitionKeyValue, operations, ct);
            _logger.LogInformation(
                "Patch completed for document {DocumentId} with {OperationCount} operations.",
                request.Id,
                request.Operations.Count);

            return OperationResult<DocumentResponse>.Success(DocumentMapper.ToDocumentResponse(document));
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

    /// <summary>
    /// Validates a request and returns a failure result if validation fails.
    /// </summary>
    /// <returns>Null if validation succeeds, otherwise a failure result.</returns>
    private async Task<OperationResult<DocumentResponse>?> ValidateRequestAsync<T>(
        IValidator<T> validator,
        T request,
        CancellationToken ct) where T : class
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ValidationFailure(validationResult.Errors.Select(x => x.ErrorMessage));
        }
        return null;
    }
}
