using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace LibraryLending.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var testConfiguration = new Dictionary<string, string?>
            {
                ["ConnectionStrings:LibraryLending"] = connectionString,
                ["DatabaseInitialization:ApplyMigrationsOnStartup"] = "false",
                ["DatabaseInitialization:SeedOnStartup"] = "false",
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning"
            };

            configurationBuilder.AddInMemoryCollection(testConfiguration);
        });
    }

    public HttpClient CreateApiClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
}
