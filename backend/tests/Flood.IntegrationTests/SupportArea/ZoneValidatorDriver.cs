using System.Diagnostics;
using System.Text.Json;

namespace Flood.IntegrationTests.SupportArea;

internal sealed record ValidationObservation(string Code, string Detail);

// Test-only adapter for the future T004 CLI, not a validator or a fake success implementation.
internal static class ZoneValidatorDriver
{
    public static async Task<ValidationObservation> ValidateAsync(string manifest, string? connectionString = null)
    {
        var project = Path.Combine(TestRepository.Root, "backend", "tools", "Flood.DataValidation", "Flood.DataValidation.csproj");
        if (!File.Exists(project))
            return new("validator_not_implemented", "RED(T002): T004 Flood.DataValidation implementation is absent; harness compiled and executed.");

        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false, CreateNoWindow = true
        };
        foreach (var argument in new[] { "run", "--no-build", "--project", project, "--", "--manifest", manifest })
            start.ArgumentList.Add(argument);
        // Never put a database password on the command line or in failure output.
        if (connectionString is not null) start.Environment["VALIDATION_CONNECTION_STRING"] = connectionString;
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot launch the validator CLI.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(true); throw new TimeoutException("Validator CLI timed out."); }
        await error; // Drain, but do not echo arbitrary tool output containing configuration.
        using var document = JsonDocument.Parse(await output);
        return new(document.RootElement.GetProperty("code").GetString()!, "CLI exit code: " + process.ExitCode);
    }
}
