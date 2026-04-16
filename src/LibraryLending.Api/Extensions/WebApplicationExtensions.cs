using LibraryLending.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LibraryLending.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DocumentTitle = "Library Lending API";
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Lending API v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    status = report.Status.ToString(),
                    service = "LibraryLending.Api",
                    utcTime = DateTime.UtcNow,
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        duration = entry.Value.Duration.TotalMilliseconds
                    })
                };

                await context.Response.WriteAsJsonAsync(payload);
            }
        });

        app.MapControllers();

        return app;
    }
}
