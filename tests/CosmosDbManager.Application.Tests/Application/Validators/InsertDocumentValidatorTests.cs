using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.Validators;
using FluentAssertions;

namespace CosmosDbManager.Application.Tests.Application.Validators;

public sealed class InsertDocumentValidatorTests
{
    private readonly InsertDocumentValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldBeValid()
    {
        var request = new InsertDocumentRequest
        {
            Configuration = new CosmosConfigurationDto
            {
                ConnectionString = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
                DatabaseName = "db-1",
                ContainerName = "users-1",
                PartitionKey = "/tenantId"
            },
            Id = "doc-1",
            PartitionKeyValue = "tenant-1",
            JsonPayload = """{"id":"doc-1","tenantId":"tenant-1"}"""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidJson_ShouldFail()
    {
        var request = new InsertDocumentRequest
        {
            Configuration = new CosmosConfigurationDto
            {
                ConnectionString = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
                DatabaseName = "db-1",
                ContainerName = "users-1",
                PartitionKey = "/tenantId"
            },
            Id = "doc-1",
            PartitionKeyValue = "tenant-1",
            JsonPayload = """{"id":"doc-1","tenantId":"""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.ErrorMessage).Should().Contain("JsonPayload must be valid JSON.");
    }
}
