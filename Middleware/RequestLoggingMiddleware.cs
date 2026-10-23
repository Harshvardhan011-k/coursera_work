using System.Diagnostics;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Custom middleware that captures HTTP method, path, response status code,
/// and request processing time using structured logging.
/// Rethrows unexpected exceptions to avoid silently swallowing errors.
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
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                method,
                path,
                statusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "HTTP {Method} {Path} failed with unhandled exception after {ElapsedMilliseconds} ms",
                method,
                path,
                stopwatch.ElapsedMilliseconds);

            // Crucial: rethrow so the exception is not silently swallowed
            throw;
        }
    }
}

/// <summary>
/// Extension method used to register the RequestLoggingMiddleware cleanly in the pipeline.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
