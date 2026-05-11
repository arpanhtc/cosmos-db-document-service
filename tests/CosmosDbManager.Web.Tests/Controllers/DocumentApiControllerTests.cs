using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.DTOs.Response;
using CosmosDbManager.Application.Interfaces;
using CosmosDbManager.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CosmosDbManager.Web.Tests.Controllers;

public sealed class DocumentApiControllerTests
{
    [Fact]
    public async Task Get_WhenServiceReturnsNotFound_ShouldReturn404()
    {
        var service = new Mock<IDocumentService>();
        service
            .Setup(x => x.GetAsync(It.IsAny<GetDocumentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<DocumentResponse>.Failure("Document not found.", "DOCUMENT_NOT_FOUND"));

        var controller = new DocumentApiController(service.Object);

        var result = await controller.Get(new GetDocumentRequest(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Insert_WhenServiceReturnsSuccess_ShouldReturn200()
    {
        var service = new Mock<IDocumentService>();
        service
            .Setup(x => x.InsertAsync(It.IsAny<InsertDocumentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<DocumentResponse>.Success(new DocumentResponse
            {
                Id = "doc-1",
                PartitionKeyValue = "tenant-1",
                JsonPayload = "{}"
            }));

        var controller = new DocumentApiController(service.Object);

        var result = await controller.Insert(new InsertDocumentRequest(), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }
}
