using System.Net;
using System.Text.Json;

namespace TravelGuide.API.Middleware;

/// <summary>
/// Middleware xu ly loi toan cuc - bat moi exception, tra ve response chuan
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "[error] - Loi khong xu ly duoc: {Message} | Path: {Path}",
                ex.Message, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Du lieu dau vao khong hop le"),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Khong co quyen truy cap"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Khong tim thay du lieu"),
            _ => (HttpStatusCode.InternalServerError, "Loi he thong, vui long thu lai")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)statusCode
        });

        await context.Response.WriteAsync(response);
    }
}