using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.DTOs.Response;

namespace CosmosDbManager.Application.Interfaces;

public interface IDocumentService
{
    Task<OperationResult<DocumentResponse>> InsertAsync(InsertDocumentRequest request, CancellationToken ct = default);

    Task<OperationResult<DocumentResponse>> UpsertAsync(UpsertDocumentRequest request, CancellationToken ct = default);

    Task<OperationResult<DocumentResponse>> GetAsync(GetDocumentRequest request, CancellationToken ct = default);

    Task<OperationResult<DocumentResponse>> PatchAsync(PatchDocumentRequest request, CancellationToken ct = default);
}
