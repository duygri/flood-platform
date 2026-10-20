# T002 test harness (RED only)

This project contains tests and adapters only. It does not implement the T004
validator, T001 real demo data, API endpoints, or an application database.

Run from the repository root:

```powershell
dotnet restore backend/FloodPlatform.sln --locked-mode
dotnet build backend/FloodPlatform.sln --no-restore
dotnet test backend/FloodPlatform.sln --no-build --filter "Category=Harness"
dotnet test backend/FloodPlatform.sln --no-build --filter "Category=ValidatorRed"
dotnet test backend/FloodPlatform.sln --no-build --filter "Category=PostGIS"
```

- `Harness`: a real .NET 10 execution smoke test. Must PASS before calling anything RED.
- `ValidatorRed`: six data-contract cases. While T004 is absent the real CLI adapter
  returns `validator_not_implemented`; assertions expect actual validation outcomes
  and fail with an explicit missing-implementation message. No fake validator is
  provided to make the cases green.
- `PostGIS`: Testcontainers PostgreSQL 17/PostGIS 3.5 fixture, `ST_Covers` edge/vertex,
  uncovered point, and a whole LineString whose endpoints are inside but whose
  middle leaves a concave polygon. If `docker info` cannot reach an engine, these
  cases are SKIPPED with `BLOCKED(T002)`, never reported as product RED or PASS.

Synthetic geometry is deliberately around (0,0), marked as test data, and created
in a unique temporary directory that each test cleans up. It does not populate
`assets/` or satisfy the real-data release gate.

## Test adapter boundary for T004

The future validator is expected at
`backend/tools/Flood.DataValidation/Flood.DataValidation.csproj`. The test driver
invokes a built CLI with `--manifest <temporary manifest path>`; JSON stdout contains
`code`: `valid`, `checksum_mismatch`, `version_conflict`, `invalid_polygon`,
`wrong_srid`, `missing_fixture`, `point_not_covered`, or `route_not_covered`.
The manifest lists relative file paths and SHA-256 values, version, source and SRID.
The conflicting-version case includes `existingVersionSha256` as a test input.
The adapter is a test seam, not production code; T004 must settle its production
CLI format and adapt this driver if that format differs, while retaining the
behavior assertions. Do not implement T004 merely to finish this RED-only task.

Spatial tests pass the ephemeral container connection through
`VALIDATION_CONNECTION_STRING`, not CLI arguments. Logs never echo tool stderr or
the connection string. Container passwords are generated per run, not real secrets.

Current environment: Docker CLI/engine and WSL unavailable. The project compiles
and ValidatorRed executes, but the real PostGIS portion is BLOCKED. T002 must remain
unchecked until its PostGIS execution criterion can be verified.
