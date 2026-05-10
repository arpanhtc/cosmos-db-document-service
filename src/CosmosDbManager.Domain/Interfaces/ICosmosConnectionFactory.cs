using CosmosDbManager.Domain.ValueObjects;

namespace CosmosDbManager.Domain.Interfaces;

public interface ICosmosConnectionFactory
{
    object CreateClient(CosmosConfiguration configuration);
}
