namespace LibraryLending.Infrastructure.Persistence.Seeding;

public sealed class DatabaseInitializationOptions
{
    public const string SectionName = "DatabaseInitialization";

    public bool ApplyMigrationsOnStartup { get; set; }

    public bool SeedOnStartup { get; set; }
}
