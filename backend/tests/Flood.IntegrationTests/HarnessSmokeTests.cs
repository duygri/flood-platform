namespace Flood.IntegrationTests;

public sealed class HarnessSmokeTests
{
    [Fact]
    [Trait("Category", "Harness")]
    public void Test_harness_runs_on_net10_with_repository_context()
    {
        Assert.Equal(10, Environment.Version.Major);
        Assert.True(File.Exists(Path.Combine(TestRepository.Root, "specs",
            "001-map-location-skeleton", "tasks.md")));
    }
}

internal static class TestRepository
{
    public static string Root
    {
        get
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
                 directory is not null; directory = directory.Parent)
                if (Directory.Exists(Path.Combine(directory.FullName, ".specify")))
                    return directory.FullName;
            throw new InvalidOperationException("Cannot find the repository .specify directory.");
        }
    }
}
