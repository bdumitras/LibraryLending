using LibraryLending.Infrastructure.Persistence;
using LibraryLending.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryLending.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseInitializationOptions>>().Value;
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("LibraryLending.DatabaseInitialization");

        if (options.ApplyMigrationsOnStartup)
        {
            logger.LogInformation("Applying pending database migrations.");
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (options.SeedOnStartup)
        {
            logger.LogInformation("Running database seed.");
            await LibraryLendingDataSeeder.SeedAsync(dbContext, logger, cancellationToken);
        }
    }
}
