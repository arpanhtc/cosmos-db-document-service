using System.Collections.Concurrent;
using System.Text.Json;
using CosmosDbManager.Domain.Interfaces;
using CosmosDbManager.Domain.ValueObjects;
using CosmosDbManager.Infrastructure.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosDbManager.Infrastructure.CosmosDb;

public sealed class CosmosConnectionFactory : ICosmosConnectionFactory
{
    private readonly ConcurrentDictionary<string, CosmosClient> _clients = new(StringComparer.Ordinal);
    private readonly CosmosDbSettings _settings;

    public CosmosConnectionFactory(IOptions<CosmosDbSettings> settings)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public object CreateClient(CosmosConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return _clients.GetOrAdd(configuration.ConnectionString, _ => CreateCosmosClient(configuration.ConnectionString));
    }

    private CosmosClient CreateCosmosClient(string connectionString)
    {
        var options = new CosmosClientOptions
        {
            Serializer = new CosmosSystemTextJsonSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }),
            ConnectionMode = ParseConnectionMode(_settings.ConnectionMode),
            RequestTimeout = TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds),
            MaxRetryAttemptsOnRateLimitedRequests = _settings.MaxRetryAttempts,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(Math.Max(_settings.RequestTimeoutSeconds, _settings.MaxRetryAttempts))
        };

        return new CosmosClient(connectionString, options);
    }

    private static ConnectionMode ParseConnectionMode(string connectionMode)
    {
        return Enum.TryParse<ConnectionMode>(connectionMode, ignoreCase: true, out var mode)
            ? mode
            : ConnectionMode.Gateway;
    }
}
