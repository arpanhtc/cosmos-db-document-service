using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CosmosDbManager.Web.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var requestId = Activity.Current?.Id ?? context.TraceIdentifier;
            _logger.LogError(exception, "Unhandled exception for {Path} with request id {RequestId}.", context.Request.Path, requestId);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();

            if (context.Request.Path.StartsWithSegments("/api") || WantsJson(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = "Please try again or contact support if the problem persists.",
                    Instance = context.Request.Path
                });
                return;
            }

            context.Response.Redirect($"/Home/Error?requestId={Uri.EscapeDataString(requestId)}");
        }
    }

    private static bool WantsJson(HttpRequest request)
    {
        return request.Headers.Accept.Count > 0
            && request.Headers.Accept.Any(value => !string.IsNullOrWhiteSpace(value) && value.Contains("application/json", StringComparison.OrdinalIgnoreCase));
    }
}
