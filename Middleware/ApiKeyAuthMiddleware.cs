namespace RemindPe.Middleware;

/// <summary>
/// Simple API key authentication middleware for single-tenant merchant application.
/// Protects /api/* endpoints. Skips health checks, static files, and Swagger.
/// The API key is configured via "ApiKey" in appsettings or REMINDPE_API_KEY environment variable.
/// When no key is configured (development), authentication is bypassed with a warning.
/// </summary>
public class ApiKeyAuthMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthMiddleware> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip authentication for non-API routes (static files, health, swagger)
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var configuredKey = _configuration["ApiKey"]
                            ?? Environment.GetEnvironmentVariable("REMINDPE_API_KEY");

        // In development with no key configured, allow requests with a warning
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            if (_environment.IsDevelopment())
            {
                await _next(context);
                return;
            }

            // In production, refuse to start without an API key
            _logger.LogError("API key is not configured. Set 'ApiKey' in appsettings or REMINDPE_API_KEY environment variable.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Service misconfigured. Contact administrator.\"}");
            return;
        }

        // Validate the API key from the request header
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey)
            || string.IsNullOrWhiteSpace(extractedApiKey))
        {
            _logger.LogWarning("API request without API key from {RemoteIp}: {Method} {Path}",
                context.Connection.RemoteIpAddress, context.Request.Method, path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"API key is required. Provide X-Api-Key header.\"}");
            return;
        }

        // Constant-time comparison to prevent timing attacks
        if (!CryptographicEquals(configuredKey, extractedApiKey.ToString()))
        {
            _logger.LogWarning("Invalid API key attempt from {RemoteIp}: {Method} {Path}",
                context.Connection.RemoteIpAddress, context.Request.Method, path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Invalid API key.\"}");
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Constant-time string comparison to prevent timing side-channel attacks.
    /// </summary>
    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }
}
