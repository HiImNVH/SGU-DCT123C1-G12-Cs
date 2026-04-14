namespace TravelGuide.API.Middleware;

/// <summary>
/// Middleware log moi HTTP request va response
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("[log] - Request bat dau: {Method} {Path}", method, path);

        await _next(context);

        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        var statusCode = context.Response.StatusCode;

        if (statusCode >= 400)
        {
            _logger.LogWarning("[warn] - Request ket thuc voi loi: {Method} {Path} | Status={Status} | {Elapsed}ms",
                method, path, statusCode, elapsed);
        }
        else
        {
            _logger.LogInformation("[log] - Request hoan thanh: {Method} {Path} | Status={Status} | {Elapsed}ms",
                method, path, statusCode, elapsed);
        }
    }
}