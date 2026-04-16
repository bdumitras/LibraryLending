using LibraryLending.IntegrationTests.Infrastructure;

namespace LibraryLending.IntegrationTests;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class HealthEndpointTests : IntegrationTestBase
{
    public HealthEndpointTests(IntegrationTestEnvironment environment)
        : base(environment)
    {
    }

    [Fact]
    public async Task Get_health_should_return_ok()
    {
        var response = await Client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}
