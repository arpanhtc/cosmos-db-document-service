using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.Interfaces;
using CosmosDbManager.Application.Services;
using CosmosDbManager.Application.Validators;
using CosmosDbManager.Domain.Entities;
using CosmosDbManager.Domain.Exceptions;
using CosmosDbManager.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CosmosDbManager.Application.Tests.Application.Services;

public sealed class DocumentServiceTests
{
    private readonly Mock<ICosmosRepository> _repository = new();
    private readonly IDocumentService _service;

    public DocumentServiceTests()
    {
        _service = new DocumentService(
            _repository.Object,
            new InsertDocumentValidator(),
            new UpsertDocumentValidator(),
            new GetDocumentValidator(),
            new PatchDocumentValidator(),
            NullLogger<DocumentService>.Instance);
    }

    [Fact]
    public async Task InsertAsync_WithInvalidPayload_ShouldReturnValidationError()
    {
        var request = CreateInsertRequest();
        request.JsonPayload = """{"id":"doc-1","tenantId":""";

        var result = await _service.InsertAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        _repository.Verify(
            x => x.InsertAsync(It.IsAny<CosmosDbManager.Domain.ValueObjects.CosmosConfiguration>(), It.IsAny<CosmosDocument>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenNotFound_ShouldReturnDocumentNotFoundCode()
    {
        var request = CreateGetRequest();

        _repository
            .Setup(x => x.GetAsync(It.IsAny<CosmosDbManager.Domain.ValueObjects.CosmosConfiguration>(), request.Id, request.PartitionKeyValue, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DocumentNotFoundException(request.Id, request.Configuration!.ContainerName));

        var result = await _service.GetAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("DOCUMENT_NOT_FOUND");
    }

    [Fact]
    public async Task UpsertAsync_WithValidRequest_ShouldReturnSuccess()
    {
        var request = CreateUpsertRequest();
        var domainDocument = CosmosDocumentFrom(request.Id, request.PartitionKeyValue, request.JsonPayload);

        _repository
            .Setup(x => x.UpsertAsync(It.IsAny<CosmosDbManager.Domain.ValueObjects.CosmosConfiguration>(), It.IsAny<CosmosDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainDocument);

        var result = await _service.UpsertAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(request.Id);
    }

    [Fact]
    public async Task PatchAsync_WhenRepositoryThrowsRateLimit_ShouldReturnRateLimitCode()
    {
        var request = CreatePatchRequest();

        _repository
            .Setup(x => x.PatchAsync(It.IsAny<CosmosDbManager.Domain.ValueObjects.CosmosConfiguration>(), request.Id, request.PartitionKeyValue, It.IsAny<IEnumerable<CosmosDbManager.Domain.ValueObjects.PatchOperation>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosRateLimitException("Rate limited."));

        var result = await _service.PatchAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("COSMOS_RATE_LIMIT");
    }

    private static InsertDocumentRequest CreateInsertRequest()
    {
        return new InsertDocumentRequest
        {
            Configuration = CreateConfiguration(),
            Id = "doc-1",
            PartitionKeyValue = "tenant-1",
            JsonPayload = """{"id":"doc-1","tenantId":"tenant-1","status":"active"}"""
        };
    }

    private static UpsertDocumentRequest CreateUpsertRequest()
    {
        return new UpsertDocumentRequest
        {
            Configuration = CreateConfiguration(),
            Id = "doc-1",
            PartitionKeyValue = "tenant-1",
            JsonPayload = """{"id":"doc-1","tenantId":"tenant-1","status":"active"}"""
        };
    }

    private static GetDocumentRequest CreateGetRequest()
    {
        return new GetDocumentRequest
        {
            Configuration = CreateConfiguration(),
            Id = "doc-1",
            PartitionKeyValue = "tenant-1"
        };
    }

    private static PatchDocumentRequest CreatePatchRequest()
    {
        return new PatchDocumentRequest
        {
            Configuration = CreateConfiguration(),
            Id = "doc-1",
            PartitionKeyValue = "tenant-1",
            Operations =
            [
                new PatchOperationDto
                {
                    OperationType = "Set",
                    Path = "/status",
                    Value = "active"
                }
            ]
        };
    }

    private static CosmosConfigurationDto CreateConfiguration()
    {
        return new CosmosConfigurationDto
        {
            ConnectionString = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
            DatabaseName = "db-1",
            ContainerName = "users-1",
            PartitionKey = "/tenantId"
        };
    }

    private static CosmosDocument CosmosDocumentFrom(string id, string partitionKeyValue, string jsonPayload)
    {
        using var json = System.Text.Json.JsonDocument.Parse(jsonPayload);
        return new CosmosDocument(id, partitionKeyValue, json.RootElement, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
    }
}
