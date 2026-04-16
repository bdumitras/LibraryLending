using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Infrastructure.Persistence;
using LibraryLending.Infrastructure.Persistence.Repositories;
using LibraryLending.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryLending.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "LibraryLending";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not found. Configure it under ConnectionStrings:{ConnectionStringName}.");
        }

        services.Configure<DatabaseInitializationOptions>(
            configuration.GetSection(DatabaseInitializationOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AssemblyReference).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure();
                });

            options.EnableDetailedErrors();
        });

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();

        return services;
    }
}
