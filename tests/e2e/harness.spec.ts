import { expect, test } from '@playwright/test';

test('harness server and browser execute without a product implementation', async ({ page, request }) => {
  const health = await request.get('/__harness/health');
  expect(health.status()).toBe(200);
  expect(await health.json()).toEqual({ harness: 'ready', implementation: 'not-started' });
  await page.goto('/');
  await expect(page.getByTestId('harness-shell')).toBeVisible();
  expect(await page.evaluate(() => 2 + 2)).toBe(4);
});

test('synthetic raster fixture is served locally, without OSM traffic', async ({ request }) => {
  const raster = await request.get('/__harness/raster.png');
  expect(raster.status()).toBe(200);
  expect(raster.headers()['content-type']).toBe('image/png');
  expect((await raster.body()).subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
});
