using System.Diagnostics;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Flood.IntegrationTests.Fixtures;

public static class DockerProbe
{
    public static bool Available { get; } = Check();

    private static bool Check()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("docker", "info --format {{.ServerVersion}}")
            {
                RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false, CreateNoWindow = true
            });
            if (process is null) return false;
            if (!process.WaitForExit(5000)) { process.Kill(true); return false; }
            return process.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception) { return false; }
    }
}

public sealed class PostgisFactAttribute : FactAttribute
{
    public PostgisFactAttribute()
    {
        if (!DockerProbe.Available)
            Skip = "BLOCKED(T002): Docker CLI/engine unavailable. Not a product RED; ST_Covers unverified.";
    }
}

public sealed class PostgisFixture : IAsyncLifetime
{
    private PostgreSqlContainer? container;

    public string ConnectionString => container?.GetConnectionString()
        ?? throw new InvalidOperationException("PostGIS is BLOCKED: no Docker engine.");

    public async Task InitializeAsync()
    {
        // Skipped facts must not initialize Testcontainers and turn an environment blocker into RED.
        if (!DockerProbe.Available) return;
        container = new PostgreSqlBuilder("postgis/postgis:17-3.5")
            .WithDatabase("synthetic_geometry_tests")
            .WithUsername("harness")
            .WithPassword(Guid.NewGuid().ToString("N"))
            .Build();
        await container.StartAsync();
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS postgis", connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ScalarBooleanAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (bool)(await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("Expected a Boolean PostGIS result."));
    }

    public async Task DisposeAsync()
    {
        if (container is not null) await container.DisposeAsync();
    }
}
