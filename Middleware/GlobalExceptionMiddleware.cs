using System.Net;
using System.Text.Json;
using RemindPe.Common;

namespace RemindPe.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate _next, ILogger<GlobalExceptionMiddleware> logger)
    {
        this._next = _next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException notFound => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                payload = ApiResponse<object>.Fail(notFound.Message)
            },
            ValidationException validation => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                payload = ApiResponse<object>.Fail(validation.Message, validation.Errors)
            },
            BusinessRuleException business => new
            {
                statusCode = (int)HttpStatusCode.UnprocessableEntity,
                payload = ApiResponse<object>.Fail(business.Message)
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                payload = ApiResponse<object>.Fail("An unexpected server error occurred. Please try again later.")
            }
        };

        context.Response.StatusCode = response.statusCode;

        var json = JsonSerializer.Serialize(response.payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
