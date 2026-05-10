using CosmosDbManager.Domain.Entities;
using CosmosDbManager.Domain.ValueObjects;

namespace CosmosDbManager.Domain.Interfaces;

public interface ICosmosRepository
{
    Task<CosmosDocument> GetAsync(
        CosmosConfiguration configuration,
        string id,
        string partitionKeyValue,
        CancellationToken ct = default);

    Task<CosmosDocument> InsertAsync(
        CosmosConfiguration configuration,
        CosmosDocument document,
        CancellationToken ct = default);

    Task<CosmosDocument> UpsertAsync(
        CosmosConfiguration configuration,
        CosmosDocument document,
        CancellationToken ct = default);

    Task<CosmosDocument> PatchAsync(
        CosmosConfiguration configuration,
        string id,
        string partitionKeyValue,
        IEnumerable<PatchOperation> operations,
        CancellationToken ct = default);
}
