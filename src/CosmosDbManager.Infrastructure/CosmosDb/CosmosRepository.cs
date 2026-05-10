using System.Net;
using System.Text.Json;
using CosmosDbManager.Domain.Entities;
using CosmosDbManager.Domain.Enums;
using CosmosDbManager.Domain.Exceptions;
using CosmosDbManager.Domain.Interfaces;
using CosmosDbManager.Domain.ValueObjects;
using CosmosDbManager.Infrastructure.Configuration;
using CosmosDbManager.Infrastructure.Resilience;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using CosmosPatchOperation = Microsoft.Azure.Cosmos.PatchOperation;
using DomainPatchOperation = CosmosDbManager.Domain.ValueObjects.PatchOperation;

namespace CosmosDbManager.Infrastructure.CosmosDb;

public sealed class CosmosRepository : ICosmosRepository
{
    private readonly ICosmosConnectionFactory _connectionFactory;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly ILogger<CosmosRepository> _logger;

    public CosmosRepository(
        ICosmosConnectionFactory connectionFactory,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _retryPolicy = RetryPolicyHelper.CreateCosmosRetryPolicy(settings?.Value ?? throw new ArgumentNullException(nameof(settings)), logger);
    }

    public Task<CosmosDocument> GetAsync(
        CosmosConfiguration configuration,
        string id,
        string partitionKeyValue,
        CancellationToken ct = default)
    {
        ValidateConfiguration(configuration);

        return ExecuteAsync(async cancellationToken =>
        {
            var container = GetContainer(configuration);
            var response = await container.ReadItemAsync<CosmosDocumentRecord>(
                id,
                new PartitionKey(partitionKeyValue),
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }, configuration, id, cancellationToken: ct);
    }

    public Task<CosmosDocument> InsertAsync(
        CosmosConfiguration configuration,
        CosmosDocument document,
        CancellationToken ct = default)
    {
        ValidateConfiguration(configuration);
        ValidateDocument(document);

        return ExecuteAsync(async cancellationToken =>
        {
            var container = GetContainer(configuration);
            var now = DateTimeOffset.UtcNow;
            var record = new CosmosDocumentRecord
            {
                Id = document.Id,
                PartitionKeyValue = document.PartitionKeyValue,
                Payload = document.Payload.Clone(),
                CreatedAt = document.CreatedAt ?? now,
                UpdatedAt = now
            };

            var response = await container.CreateItemAsync(
                record,
                new PartitionKey(document.PartitionKeyValue),
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }, configuration, document.Id, cancellationToken: ct);
    }

    public Task<CosmosDocument> UpsertAsync(
        CosmosConfiguration configuration,
        CosmosDocument document,
        CancellationToken ct = default)
    {
        ValidateConfiguration(configuration);
        ValidateDocument(document);

        return ExecuteAsync(async cancellationToken =>
        {
            var container = GetContainer(configuration);
            var now = DateTimeOffset.UtcNow;
            var record = new CosmosDocumentRecord
            {
                Id = document.Id,
                PartitionKeyValue = document.PartitionKeyValue,
                Payload = document.Payload.Clone(),
                CreatedAt = document.CreatedAt ?? now,
                UpdatedAt = now
            };

            var response = await container.UpsertItemAsync(
                record,
                new PartitionKey(document.PartitionKeyValue),
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }, configuration, document.Id, cancellationToken: ct);
    }

    public Task<CosmosDocument> PatchAsync(
        CosmosConfiguration configuration,
        string id,
        string partitionKeyValue,
        IEnumerable<DomainPatchOperation> operations,
        CancellationToken ct = default)
    {
        ValidateConfiguration(configuration);

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(partitionKeyValue))
        {
            throw new ArgumentException("PartitionKeyValue is required.", nameof(partitionKeyValue));
        }

        var patchOperations = operations?.Select(MapToCosmosPatchOperation).ToArray()
            ?? throw new ArgumentNullException(nameof(operations));

        if (patchOperations.Length == 0)
        {
            throw new ArgumentException("At least one patch operation is required.", nameof(operations));
        }

        return ExecuteAsync(async cancellationToken =>
        {
            var container = GetContainer(configuration);
            var response = await container.PatchItemAsync<CosmosDocumentRecord>(
                id,
                new PartitionKey(partitionKeyValue),
                patchOperations,
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }, configuration, id, cancellationToken: ct);
    }

    private async Task<CosmosDocument> ExecuteAsync(
        Func<CancellationToken, Task<CosmosDocument>> operation,
        CosmosConfiguration configuration,
        string documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(operation, cancellationToken);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos operation failed for {DocumentId}.", documentId);
            throw TranslateException(ex, configuration, documentId);
        }
    }

    private Container GetContainer(CosmosConfiguration configuration)
    {
        var client = (CosmosClient)_connectionFactory.CreateClient(configuration);
        return client.GetContainer(configuration.DatabaseName, configuration.ContainerName);
    }

    private static CosmosDocument MapToDomain(CosmosDocumentRecord record)
    {
        return new CosmosDocument(
            record.Id,
            record.PartitionKeyValue,
            record.Payload.Clone(),
            record.CreatedAt,
            record.UpdatedAt);
    }

    private static CosmosPatchOperation MapToCosmosPatchOperation(DomainPatchOperation operation)
    {
        return operation.OperationType switch
        {
            CosmosDbManager.Domain.Enums.PatchOperationType.Add => CosmosPatchOperation.Add(operation.Path, operation.Value),
            CosmosDbManager.Domain.Enums.PatchOperationType.Set => CosmosPatchOperation.Set(operation.Path, operation.Value),
            CosmosDbManager.Domain.Enums.PatchOperationType.Replace => CosmosPatchOperation.Replace(operation.Path, operation.Value),
            CosmosDbManager.Domain.Enums.PatchOperationType.Remove => CosmosPatchOperation.Remove(operation.Path),
            CosmosDbManager.Domain.Enums.PatchOperationType.Increment => MapIncrementOperation(operation),
            _ => throw new ArgumentOutOfRangeException(nameof(operation.OperationType), operation.OperationType, "Unsupported patch operation type.")
        };
    }

    private static CosmosPatchOperation MapIncrementOperation(DomainPatchOperation operation)
    {
        if (operation.Value is null)
        {
            throw new ArgumentException("Increment operation requires a value.", nameof(operation));
        }

        return operation.Value switch
        {
            byte or sbyte or short or ushort or int or uint or long or ulong => CosmosPatchOperation.Increment(operation.Path, Convert.ToInt64(operation.Value, System.Globalization.CultureInfo.InvariantCulture)),
            float or double or decimal => CosmosPatchOperation.Increment(operation.Path, Convert.ToDouble(operation.Value, System.Globalization.CultureInfo.InvariantCulture)),
            string value when long.TryParse(value, out var longValue) => CosmosPatchOperation.Increment(operation.Path, longValue),
            string value when double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue) => CosmosPatchOperation.Increment(operation.Path, doubleValue),
            JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt64(out var longJsonValue) => CosmosPatchOperation.Increment(operation.Path, longJsonValue),
            JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Number => CosmosPatchOperation.Increment(operation.Path, jsonElement.GetDouble()),
            _ => throw new ArgumentException("Increment operation requires a numeric value.", nameof(operation))
        };
    }

    private static Exception TranslateException(CosmosException exception, CosmosConfiguration configuration, string documentId)
    {
        return exception.StatusCode switch
        {
            HttpStatusCode.NotFound => new DocumentNotFoundException(documentId, configuration.ContainerName),
            HttpStatusCode.Conflict => new DocumentConflictException(documentId, configuration.ContainerName),
            HttpStatusCode.TooManyRequests => new CosmosRateLimitException(exception.Message, exception, exception.RetryAfter),
            HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout => new CosmosServiceUnavailableException(exception.Message, exception),
            _ => new CosmosDbException(exception.Message, exception)
        };
    }

    private static void ValidateConfiguration(CosmosConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(configuration.ConnectionString)
            || string.IsNullOrWhiteSpace(configuration.DatabaseName)
            || string.IsNullOrWhiteSpace(configuration.ContainerName)
            || string.IsNullOrWhiteSpace(configuration.PartitionKey)
            || !configuration.PartitionKey.StartsWith('/'))
        {
            throw new InvalidConfigurationException("Invalid Cosmos DB configuration.");
        }
    }

    private static void ValidateDocument(CosmosDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(document.Id) || string.IsNullOrWhiteSpace(document.PartitionKeyValue))
        {
            throw new InvalidConfigurationException("Document is missing required fields.");
        }
    }

    private sealed record CosmosDocumentRecord
    {
        public string Id { get; init; } = string.Empty;

        public string PartitionKeyValue { get; init; } = string.Empty;

        public JsonElement Payload { get; init; }

        public DateTimeOffset? CreatedAt { get; init; }

        public DateTimeOffset? UpdatedAt { get; init; }
    }
}
