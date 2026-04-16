using LibraryLending.IntegrationTests.Infrastructure;

namespace LibraryLending.IntegrationTests;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class DatabaseInfrastructureTests : IntegrationTestBase
{
    public DatabaseInfrastructureTests(IntegrationTestEnvironment environment)
        : base(environment)
    {
    }

    [Fact]
    public async Task SeedBasicScenarioAsync_should_insert_expected_data()
    {
        var scenario = await SeedBasicScenarioAsync();

        var totalRows = await Environment.ExecuteDbContextAsync(dbContext =>
            TestDataSeeder.CountAllRowsAsync(dbContext));

        Assert.NotEqual(Guid.Empty, scenario.AvailableBookId);
        Assert.NotEqual(Guid.Empty, scenario.ActiveLoanId);
        Assert.Equal(8, totalRows);
    }

    [Fact]
    public async Task ResetDatabaseAsync_should_clear_seeded_data_between_tests()
    {
        var totalRows = await Environment.ExecuteDbContextAsync(dbContext =>
            TestDataSeeder.CountAllRowsAsync(dbContext));

        Assert.Equal(0, totalRows);
    }
}
