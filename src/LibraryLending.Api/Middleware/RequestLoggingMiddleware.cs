using System.Diagnostics;

namespace LibraryLending.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["RequestMethod"] = context.Request.Method,
            ["RequestPath"] = context.Request.Path.Value
        });

        logger.LogDebug(
            "HTTP request started {Method} {Path}.",
            context.Request.Method,
            context.Request.Path.Value);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation(
                "HTTP request completed {Method} {Path} with status code {StatusCode} in {ElapsedMilliseconds} ms.",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
