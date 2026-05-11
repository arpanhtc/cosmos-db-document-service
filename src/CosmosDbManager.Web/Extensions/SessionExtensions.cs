using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CosmosDbManager.Web.Extensions;

public static class SessionExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static void SetObject<T>(this ISession session, string key, T value)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        session.SetString(key, JsonSerializer.Serialize(value, Options));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var payload = session.GetString(key);
        return string.IsNullOrWhiteSpace(payload)
            ? default
            : JsonSerializer.Deserialize<T>(payload, Options);
    }
}
