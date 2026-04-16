using LibraryLending.Api.Extensions;
using LibraryLending.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions =
        ActivityTrackingOptions.SpanId |
        ActivityTrackingOptions.TraceId |
        ActivityTrackingOptions.ParentId |
        ActivityTrackingOptions.Tags |
        ActivityTrackingOptions.Baggage;
});

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    options.UseUtcTimestamp = true;
});

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

builder.Services
    .AddApiServices(builder.Configuration);

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseApiPipeline();
app.MapApiEndpoints();

app.Run();

public partial class Program;
