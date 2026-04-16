using LibraryLending.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.PostgreSql;

namespace LibraryLending.IntegrationTests.Infrastructure;

public sealed class IntegrationTestEnvironment : IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("library_lending_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner? _respawner;

    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    public string ConnectionString => _databaseContainer.GetConnectionString();

    public HttpClient CreateClient()
    {
        EnsureInitialized();
        return Factory.CreateApiClient();
    }

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();

        Factory = new CustomWebApplicationFactory(ConnectionString);

        _ = Factory.CreateApiClient();

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        await InitializeRespawnerAsync(dbContext);
        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();
        await _databaseContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        EnsureInitialized();

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync();

        try
        {
            await _respawner!.ResetAsync(connection);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        EnsureInitialized();

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await action(dbContext);
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        EnsureInitialized();

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await action(dbContext);
    }

    private async Task InitializeRespawnerAsync(ApplicationDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync();

        try
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = [new Respawn.Graph.Table("__EFMigrationsHistory")]
            });
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private void EnsureInitialized()
    {
        if (Factory is null || _respawner is null)
        {
            throw new InvalidOperationException("The integration test environment has not been initialized yet.");
        }
    }
}
