import { expect, test } from '@playwright/test';
import { readFile } from 'node:fs/promises';

// Read-only test hook for the future T005 map adapter; never invent a fake map to pass.
declare global {
  interface Window {
    __mapTestApi?: { snapshot(): { center: [number, number]; zoom: number } };
    __gpsRequestCount?: number;
  }
}

test.beforeEach(async ({ page }) => {
  const raster = await readFile(new URL('./fixtures/synthetic-raster.png', import.meta.url));
  await page.route(/https?:\/\/(?:[abc]\.)?tile\.openstreetmap\.org\//, route =>
    route.fulfill({ status: 200, contentType: 'image/png', body: raster }));
  await page.addInitScript(() => {
    window.__gpsRequestCount = 0;
    for (const method of ['getCurrentPosition', 'watchPosition'] as const) {
      const original = navigator.geolocation[method].bind(navigator.geolocation);
      Object.defineProperty(navigator.geolocation, method, {
        configurable: true,
        value: (...args: unknown[]) => {
          window.__gpsRequestCount = (window.__gpsRequestCount ?? 0) + 1;
          return Reflect.apply(original, navigator.geolocation, args);
        }
      });
    }
  });
  await page.goto('/');
});

test('anonymous user sees the HCM map within five seconds without login or GPS permission', async ({ page }) => {
  expect(await page.evaluate(() => window.__gpsRequestCount)).toBe(0);
  await expect(page.getByRole('region', { name: 'Bản đồ TP.HCM' }),
    'RED(T003): T005 map implementation is absent; harness page loaded successfully').toBeVisible();
  const map = await page.evaluate(() => window.__mapTestApi?.snapshot());
  expect(map, 'T005 must connect the read-only map test adapter to the actual map').toBeDefined();
  expect(map!.center[0]).toBeGreaterThan(106.4);
  expect(map!.center[0]).toBeLessThan(107.0);
  expect(map!.center[1]).toBeGreaterThan(10.6);
  expect(map!.center[1]).toBeLessThan(11.0);
  expect(await page.evaluate(() => window.__gpsRequestCount)).toBe(0);
});

test('map keeps visible OpenStreetMap attribution', async ({ page }) => {
  await expect(page.getByRole('link', { name: /OpenStreetMap/ }),
    'RED(T003): no map/attribution has been implemented in T005').toBeVisible();
});

test('map supports pan and touch zoom on a mobile viewport', async ({ page }) => {
  const region = page.getByRole('region', { name: 'Bản đồ TP.HCM' });
  await expect(region, 'RED(T003): T005 interactive map is not implemented').toBeVisible();
  const before = await page.evaluate(() => window.__mapTestApi?.snapshot());
  expect(before).toBeDefined();
  const bounds = await region.boundingBox();
  expect(bounds).not.toBeNull();
  await page.mouse.move(bounds!.x + bounds!.width * 0.6, bounds!.y + bounds!.height * 0.5);
  await page.mouse.down();
  await page.mouse.move(bounds!.x + bounds!.width * 0.3, bounds!.y + bounds!.height * 0.5, { steps: 8 });
  await page.mouse.up();
  await expect.poll(() => page.evaluate(() => window.__mapTestApi?.snapshot().center)).not.toEqual(before!.center);
  await page.getByRole('button', { name: 'Phóng to', exact: true }).tap();
  await expect.poll(() => page.evaluate(() => window.__mapTestApi?.snapshot().zoom)).toBeGreaterThan(before!.zoom);
});

test('Vietnamese mobile map has no horizontal overflow and touch controls are at least 44px', async ({ page }) => {
  await expect(page.getByRole('region', { name: 'Bản đồ TP.HCM' }),
    'RED(T003): T005 mobile map layout is not implemented').toBeVisible();
  await expect(page.locator('html')).toHaveAttribute('lang', 'vi');
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  const controls = page.getByRole('button');
  expect(await controls.count()).toBeGreaterThan(0);
  for (const control of await controls.all()) {
    await expect(control).toHaveAccessibleName(/\S/);
    const bounds = await control.boundingBox();
    expect(bounds).not.toBeNull();
    expect(bounds!.width).toBeGreaterThanOrEqual(44);
    expect(bounds!.height).toBeGreaterThanOrEqual(44);
  }
});
