import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: '../../tests/e2e',
  outputDir: './test-results',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 15_000,
  expect: { timeout: 5_000 },
  reporter: [['list'], ['json', { outputFile: process.env.HARNESS_REPORT_FILE ?? 'test-reports/results.json' }]],
  use: {
    baseURL: 'http://127.0.0.1:4173',
    viewport: { width: 360, height: 780 },
    hasTouch: true,
    locale: 'vi-VN',
    permissions: [],
    screenshot: 'only-on-failure',
    trace: 'off'
  },
  projects: [
    { name: 'chromium-mobile', use: { browserName: 'chromium', isMobile: true } },
    { name: 'webkit-mobile', use: { browserName: 'webkit', isMobile: true } }
  ],
  webServer: {
    command: 'npm run harness:serve',
    url: 'http://127.0.0.1:4173/__harness/health',
    reuseExistingServer: false,
    timeout: 15_000
  }
});
