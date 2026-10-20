# T003 browser harness (RED only)

The test server in `harness/server.mjs` binds to loopback and serves an explicitly
empty harness page, a readiness URL and a synthetic raster. It is NOT Next.js,
the T005 map shell, or a deployable application. No login, GPS, map, cloud service
or real flood data is implemented here.

From the repository root, after `npm ci`:

```powershell
npm --prefix apps/web run typecheck
npm exec -- playwright install chromium webkit
npm --prefix apps/web run test:harness
npm --prefix apps/web run test:red
```

`test:harness` must PASS first: browser JavaScript, local HTTP readiness and a real
PNG fixture work. `test:red` exercises the expected future map behavior and fails
on missing map/attribution, not server startup, package imports or browser absence.
Playwright uses one worker, no retries, 360x780 mobile/touch contexts, Vietnamese
locale, no granted GPS permission, Chromium and WebKit.

The four map cases cover anonymous HCM centering without GPS, attribution,
interactive panning/touch zoom, and Vietnamese mobile layout with >=44px controls.
OSM tile requests are intercepted with `fixtures/synthetic-raster.png`, a generated
256x256 grid with no geographic content; tests do not bulk-download public tiles.

For reliable camera assertions, the future T005 test build can connect the read-only
`window.__mapTestApi.snapshot()` seam to its ACTUAL MapLibre map (center [lng,lat],
zoom). This is not a fake map and must not implement application behavior inside
tests. It need not be exposed by production builds. The map region is named
`Bản đồ TP.HCM`; zoom control has the accessible Vietnamese name `Phóng to`.

Package/browser startup errors are harness defects to fix, not valid RED.
Screenshots live under `apps/web/test-results/`; JSON reports under
`apps/web/test-reports/` (both ignored). `HARNESS_REPORT_FILE` selects a report path
so the smoke and RED evidence can be retained separately.
No real-device acceptance, HTTPS deployment or T005 implementation is claimed.
