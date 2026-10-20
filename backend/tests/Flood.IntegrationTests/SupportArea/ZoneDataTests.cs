namespace Flood.IntegrationTests.SupportArea;

public sealed class ZoneDataTests
{
    [Theory]
    [Trait("Category", "ValidatorRed")]
    [InlineData("valid", "valid")]
    [InlineData("checksum_mismatch", "checksum_mismatch")]
    [InlineData("version_conflict", "version_conflict")]
    [InlineData("invalid_polygon", "invalid_polygon")]
    [InlineData("wrong_srid", "wrong_srid")]
    [InlineData("missing_fixture", "missing_fixture")]
    public async Task Validator_reports_required_data_contract_outcome(string scenario, string expected)
    {
        using var bundle = new SyntheticDemoBundle(scenario);
        var result = await ZoneValidatorDriver.ValidateAsync(bundle.ManifestPath);
        Assert.True(result.Code == expected,
            $"Expected '{expected}', actual '{result.Code}'. {result.Detail}");
    }
}
