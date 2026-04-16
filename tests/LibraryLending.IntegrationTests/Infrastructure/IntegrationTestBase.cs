using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LibraryLending.Infrastructure.Persistence;

namespace LibraryLending.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollectionNames.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    protected IntegrationTestBase(IntegrationTestEnvironment environment)
    {
        Environment = environment;
        Client = environment.CreateClient();
    }

    protected IntegrationTestEnvironment Environment { get; }

    protected HttpClient Client { get; }

    public virtual Task InitializeAsync()
    {
        return Environment.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task<T?> ReadAsJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    protected async Task<T> EnsureSuccessAndReadAsJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);

        if (payload is null)
        {
            throw new InvalidOperationException($"Response body for {typeof(T).Name} was null.");
        }

        return payload;
    }

    protected Task SeedAsync(Func<ApplicationDbContext, Task> seedAction)
    {
        return Environment.ExecuteDbContextAsync(async dbContext =>
        {
            await seedAction(dbContext);
            await dbContext.SaveChangesAsync();
        });
    }

    protected Task<T> SeedAsync<T>(Func<ApplicationDbContext, Task<T>> seedAction)
    {
        return Environment.ExecuteDbContextAsync(async dbContext =>
        {
            var result = await seedAction(dbContext);
            await dbContext.SaveChangesAsync();
            return result;
        });
    }

    protected async Task<SeededLibraryScenario> SeedBasicScenarioAsync(CancellationToken cancellationToken = default)
    {
        return await SeedAsync(dbContext => TestDataSeeder.SeedBasicScenarioAsync(dbContext, cancellationToken));
    }
}
