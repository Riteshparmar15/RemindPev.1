namespace RemindPe.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("D");
        }

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        using (_logger.BeginScope("CorrelationId:{CorrelationId}", correlationId))
        {
            await _next(context);
        }
    }
}
