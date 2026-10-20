using Flood.IntegrationTests.Fixtures;

namespace Flood.IntegrationTests.SupportArea;

[Trait("Category", "PostGIS")]
public sealed class ZoneCoverageTests(PostgisFixture postgis) : IClassFixture<PostgisFixture>
{
    private const string Zone = "ST_GeomFromText('POLYGON((0 0,4 0,4 1,1 1,1 4,0 4,0 0))',4326)";

    [PostgisFact]
    public async Task Covers_includes_inside_edge_and_vertex_and_rejects_outside()
    {
        Assert.True(await postgis.ScalarBooleanAsync($"SELECT ST_Covers({Zone},ST_SetSRID(ST_Point(0.5,0.5),4326))"));
        Assert.True(await postgis.ScalarBooleanAsync($"SELECT ST_Covers({Zone},ST_SetSRID(ST_Point(0,0.5),4326))"));
        Assert.True(await postgis.ScalarBooleanAsync($"SELECT ST_Covers({Zone},ST_SetSRID(ST_Point(0,0),4326))"));
        Assert.False(await postgis.ScalarBooleanAsync($"SELECT ST_Covers({Zone},ST_SetSRID(ST_Point(-0.000001,0.5),4326))"));
    }

    [PostgisFact]
    public async Task Validator_rejects_any_uncovered_seed_point()
    {
        using var bundle = new SyntheticDemoBundle("point_not_covered");
        var result = await ZoneValidatorDriver.ValidateAsync(bundle.ManifestPath, postgis.ConnectionString);
        Assert.True(result.Code == "point_not_covered", $"Expected point_not_covered; actual {result.Code}. {result.Detail}");
    }

    [PostgisFact]
    public async Task Validator_checks_whole_route_not_only_covered_endpoints()
    {
        var route = "ST_GeomFromText('LINESTRING(0.5 3,3 0.5)',4326)";
        Assert.True(await postgis.ScalarBooleanAsync($"SELECT ST_Covers({Zone},ST_StartPoint({route})) AND ST_Covers({Zone},ST_EndPoint({route}))"));
        Assert.False(await postgis.ScalarBooleanAsync($"SELECT ST_Covers({Zone},{route})"));
        using var bundle = new SyntheticDemoBundle("route_not_covered");
        var result = await ZoneValidatorDriver.ValidateAsync(bundle.ManifestPath, postgis.ConnectionString);
        Assert.True(result.Code == "route_not_covered", $"Expected route_not_covered; actual {result.Code}. {result.Detail}");
    }
}
