import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';

// Test-only empty page, NOT T005's Next.js/map implementation. Bind loopback only.
const page = `<!doctype html><html lang="vi"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Test harness — chưa có bản đồ</title></head><body>
<main data-testid="harness-shell">Harness chạy được. Giao diện bản đồ T005 chưa được cài đặt.</main>
</body></html>`;

const server = createServer(async (request, response) => {
  const path = new URL(request.url ?? '/', 'http://127.0.0.1').pathname;
  response.setHeader('Cache-Control', 'no-store');
  if (path === '/__harness/health') {
    response.writeHead(200, { 'Content-Type': 'application/json' });
    response.end(JSON.stringify({ harness: 'ready', implementation: 'not-started' }));
  } else if (path === '/') {
    response.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
    response.end(page);
  } else if (path === '/__harness/raster.png') {
    try {
      response.writeHead(200, { 'Content-Type': 'image/png' });
      response.end(await readFile(new URL('../fixtures/synthetic-raster.png', import.meta.url)));
    } catch {
      response.statusCode = 500;
      response.end('Synthetic raster fixture unavailable');
    }
  } else {
    response.writeHead(404);
    response.end();
  }
});

server.listen(4173, '127.0.0.1', () => console.log('T003 test-only harness: http://127.0.0.1:4173'));
for (const signal of ['SIGTERM', 'SIGINT']) process.on(signal, () => server.close());
