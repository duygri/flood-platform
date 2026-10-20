using System.Security.Cryptography;
using System.Text.Json;

namespace Flood.IntegrationTests.SupportArea;

// Deliberately around (0,0). Never use these synthetic coordinates as HCM flood data (T001).
internal sealed class SyntheticDemoBundle : IDisposable
{
    public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "flood-harness-" + Guid.NewGuid());
    public string ManifestPath => Path.Combine(DirectoryPath, "manifest.json");

    public SyntheticDemoBundle(string scenario)
    {
        Directory.CreateDirectory(DirectoryPath);
        var polygon = scenario == "invalid_polygon"
            ? "[[[0,0],[4,4],[0,4],[4,0],[0,0]]]"
            : "[[[0,0],[4,0],[4,1],[1,1],[1,4],[0,4],[0,0]]]";
        var route = scenario == "route_not_covered" ? "[[0.5,3],[3,0.5]]" : "[[0.5,0.5],[0.5,3]]";
        var point = scenario == "point_not_covered" ? "[8,8]" : "[0.5,0.5]";
        File.WriteAllText(Path.Combine(DirectoryPath, "zone.geojson"),
            "{\"type\":\"Feature\",\"properties\":{\"version\":\"synthetic-v1\"},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":" + polygon + "}}");
        File.WriteAllText(Path.Combine(DirectoryPath, "flood-points.geojson"),
            "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"id\":\"synthetic-point\",\"properties\":{},\"geometry\":{\"type\":\"Point\",\"coordinates\":" + point + "}}]}");
        File.WriteAllText(Path.Combine(DirectoryPath, "demo-routes.geojson"),
            "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"id\":\"synthetic-route\",\"properties\":{},\"geometry\":{\"type\":\"LineString\",\"coordinates\":" + route + "}}]}");
        var files = new[] { "zone.geojson", "flood-points.geojson", "demo-routes.geojson" }
            .Select(name => new { path = name, sha256 = Convert.ToHexStringLower(
                SHA256.HashData(File.ReadAllBytes(Path.Combine(DirectoryPath, name)))) }).ToArray();
        File.WriteAllText(ManifestPath, JsonSerializer.Serialize(new
        {
            version = "synthetic-v1", source = "SYNTHETIC TEST DATA, NOT REAL FLOOD DATA",
            srid = scenario == "wrong_srid" ? 3857 : 4326,
            existingVersionSha256 = scenario == "version_conflict" ? new string('0', 64) : null,
            files
        }));
        if (scenario == "checksum_mismatch") File.AppendAllText(Path.Combine(DirectoryPath, "zone.geojson"), " ");
        if (scenario == "missing_fixture") File.Delete(Path.Combine(DirectoryPath, "demo-routes.geojson"));
    }

    public void Dispose() => Directory.Delete(DirectoryPath, true);
}
