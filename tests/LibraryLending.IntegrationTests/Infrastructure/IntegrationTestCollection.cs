namespace LibraryLending.IntegrationTests.Infrastructure;

public static class IntegrationTestCollectionNames
{
    public const string Name = "LibraryLending integration tests";
}

[CollectionDefinition(IntegrationTestCollectionNames.Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestEnvironment>
{
}
