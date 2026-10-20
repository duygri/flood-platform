# T002/T003 — harness và bằng chứng RED

**Ngày**: 2026-10-20 (Asia/Saigon).
**Phạm vi được yêu cầu**: chỉ T002 và T003, dừng trước T004/T005.

## Kết quả

| Kiểm tra | Kết quả thực tế | Phân loại |
|---|---|---|
| .NET restore locked/build | Thành công, build 0 lỗi/0 cảnh báo | Harness chạy được |
| xUnit Category=Harness | 1 PASS | Smoke .NET10/runtime/repo |
| xUnit Category=ValidatorRed | 6 FAIL do `validator_not_implemented`, thông điệp RED(T002) | RED hợp lệ: chưa cài validator T004 |
| xUnit Category=PostGIS | 3 SKIPPED, lý do BLOCKED(T002) | Không có Docker engine, KHÔNG tính là RED/PASS |
| npm ci --ignore-scripts | Thành công với root package-lock.json | Cài lại harness từ lockfile |
| TypeScript typecheck | Thành công, không lỗi | Không dùng lỗi biên dịch làm RED |
| Playwright harness smoke | 4 PASS: 2 Chromium + 2 WebKit | Server, browser JavaScript, PNG local hoạt động |
| Playwright map behavior | 8 FAIL: 4 Chromium + 4 WebKit | RED(T003): thiếu map/attribution T005, không do server/browser |

**T003 đã tick** vì tiêu chí harness + tests RED đạt. **T002 chưa tick**: đã có
harness/6 RED hợp lệ nhưng chưa chạy được phần Testcontainers/ST_Covers thật.
**T001 giữ nguyên chưa tick**, không tạo assets hoặc bịa dữ liệu nhóm.

Đây là kết quả test-first có chủ ý, không phải ứng dụng map đã hoạt động hoặc suite
đã GREEN. Không triển khai validator/map để làm test qua.

## BLOCKED và giới hạn

- Docker CLI không có trong PATH hoặc đường dẫn Docker Desktop tiêu chuẩn, named
  pipe docker_engine không khả dụng, WSL chưa được cài. Không tự cài Docker/WSL,
  không dùng DB khác để thay bằng chứng PostGIS. T002 chờ một Docker engine sử dụng
  được, sau đó chạy lại Category=PostGIS; không báo ST_Covers đã được xác minh.
- Dữ liệu demo thật T001 do nhóm cung cấp, nằm ngoài hai task được phép. Synthetic
  fixture quanh (0,0) chỉ dùng test, không thay GeoJSON TP.HCM/điểm ngập/tuyến demo.
- Checklist requirements giữ nguyên 14/16; CHK001/CHK016 chưa tick vì các quyết định
  công nghệ đã được người dùng chốt. Không tự sửa checklist.
- Repo hiện chưa có Git; làm trong workspace hiện có, không git init/worktree/commit.
- Không deploy, không tạo service/cloud infrastructure, không thực hiện T004/T005
  hoặc task nào khác, không đưa secret vào mã nguồn.

## File tạo/sửa (không tính dependency/build/test output đã ignore)

### Cấu hình chung

- `.gitignore`, `.dockerignore`: loại dependency, output và secret khỏi source/context.
- `global.json`: pin SDK10.0.113, rollForward latestPatch.
- `package.json`, `package-lock.json`: npm workspace private, toolchain test ở root;
  Playwright1.64.1, TypeScript5.9.3, @types/node24.19.2.

### T002

- `backend/FloodPlatform.sln`
- `backend/tests/Flood.IntegrationTests/Flood.IntegrationTests.csproj`
- `backend/tests/Flood.IntegrationTests/packages.lock.json`
- `backend/tests/Flood.IntegrationTests/HarnessSmokeTests.cs`
- `backend/tests/Flood.IntegrationTests/Fixtures/PostgisFixture.cs`
- `backend/tests/Flood.IntegrationTests/SupportArea/SyntheticDemoBundle.cs`
- `backend/tests/Flood.IntegrationTests/SupportArea/ZoneValidatorDriver.cs`
- `backend/tests/Flood.IntegrationTests/SupportArea/ZoneDataTests.cs`
- `backend/tests/Flood.IntegrationTests/SupportArea/ZoneCoverageTests.cs`
- `backend/tests/Flood.IntegrationTests/README.md`

Template UnitTest1.cs được tạo trong bootstrap rồi xoá/thay bằng tests có ý nghĩa.
Npgsql10.0.3/Testcontainers.PostgreSql4.16.0/xUnit2.9.3 được pin trong project/lockfile.
Container password ngẫu nhiên mỗi lần chạy, không phải secret thật/hardcode.

### T003

- `apps/web/package.json`
- `apps/web/tsconfig.json`
- `apps/web/playwright.config.ts`
- `tests/e2e/harness/server.mjs` (server loopback TRỐNG chỉ để test, không phải T005)
- `tests/e2e/harness.spec.ts`
- `tests/e2e/map.spec.ts`
- `tests/e2e/fixtures/synthetic-raster.png` (grid PNG256x256 tự tạo, không dữ liệu địa lý)
- `tests/e2e/README.md`

Lockfile tạm apps/web/package-lock.json đã bỏ; root package-lock.json là nguồn duy nhất.

### Ghi nhận

- `specs/001-map-location-skeleton/tasks.md`: chỉ đổi checkbox T003 sang [x].
- `docs/validation/t002-t003-execution.md`: báo cáo này.

## Lệnh chính đã chạy

Các lệnh dưới đây tương đối từ repo root để chạy lại thuận tiện; lúc thực thi đã
dùng đường dẫn tuyệt đối tương ứng. Đây là danh mục build/setup/test, không liệt kê
mọi lệnh đọc file. Exit code của từng test được phân biệt, không dùng exit code
cuối của chuỗi lệnh để che các suite RED.

```powershell
# Preflight
dotnet --list-sdks
node --version
npm --version
git rev-parse --git-dir
wsl --list --quiet
# Kiểm tra Docker PATH/service/đường dẫn/named pipe: không khả dụng.

# Bootstrap T002
dotnet new sln --name FloodPlatform --format sln --output backend
dotnet new xunit --framework net10.0 --name Flood.IntegrationTests --output backend/tests/Flood.IntegrationTests --no-restore
dotnet sln backend/FloodPlatform.sln add backend/tests/Flood.IntegrationTests/Flood.IntegrationTests.csproj
dotnet add backend/tests/Flood.IntegrationTests/Flood.IntegrationTests.csproj package Testcontainers.PostgreSql
dotnet add backend/tests/Flood.IntegrationTests/Flood.IntegrationTests.csproj package Npgsql
dotnet restore backend/FloodPlatform.sln --locked-mode
dotnet build backend/FloodPlatform.sln --no-restore --verbosity minimal
dotnet test backend/FloodPlatform.sln --no-build --filter "Category=Harness" --logger "trx;LogFileName=harness-smoke.trx" --results-directory backend/test-results
dotnet test backend/FloodPlatform.sln --no-build --filter "Category=ValidatorRed" --logger "trx;LogFileName=validator-red.trx" --results-directory backend/test-results
dotnet test backend/FloodPlatform.sln --no-build --filter "Category=PostGIS" --logger "trx;LogFileName=postgis-blocked.trx" --results-directory backend/test-results

# Bootstrap T003 và sửa package resolution
npm view @playwright/test version
npm view typescript version
npm view @types/node version
npm --prefix apps/web install --save-dev --save-exact @types/node@24
npm install
npm dedupe
npm uninstall --workspace flood-map-web-harness --save-dev @playwright/test @types/node typescript
npm install --save-dev --save-exact @playwright/test@1.64.1 @types/node@24.19.2 typescript@5.9.3
npm exec -- playwright install chromium webkit
npm ci --ignore-scripts
npm --prefix apps/web run typecheck
$env:HARNESS_REPORT_FILE='test-reports/harness-smoke.json'
npm --prefix apps/web run test:harness
$env:HARNESS_REPORT_FILE='test-reports/map-red.json'
npm --prefix apps/web run test:red
```

SDK/package scaffolding ban đầu có lỗi TS package resolution vì tests ở root còn
dependencies ở apps/web. Đã điều tra và sửa bằng root npm workspace/toolchain;
typecheck và smoke sau sửa đạt. Lỗi đó KHÔNG được tính là RED. Một thử nghiệm dọn
dependency bằng shell bị policy chặn, không thực thi; đã dùng npm uninstall/install
để quản lý dependency an toàn thay thế. Không cần/xin quyền mở rộng công việc.

PNG fixture được tạo xác định bằng Node fs/zlib, không tải ảnh/tile bên ngoài.
Browser binaries cài vào Playwright cache của tài khoản; NuGet/npm vào cache/dependencies,
không phải service hạ tầng mới. dotnet lần đầu tự tạo development certificate mặc định;
không chạy trust certificate hoặc deploy HTTPS.

## Bằng chứng máy đọc được (output ignored)

- `backend/test-results/harness-smoke.trx`:1 PASS.
- `backend/test-results/validator-red.trx`:6 FAIL do thiếu cài đặt T004.
- `backend/test-results/postgis-blocked.trx`:3 SKIPPED/BLOCKED.
- `apps/web/test-reports/harness-smoke.json`:4 PASS.
- `apps/web/test-reports/map-red.json`:8 FAIL, mỗi lỗi có RED(T003)/missing element.

Các ca map chưa chạm được assertions pan/zoom/layout sau bước map-visible, vì map
chưa có. Đây là RED trước T005, không phải bằng chứng các tương tác đã đạt. WebKit
trong Playwright không phải nghiệm thu Safari iPhone thật; task điện thoại không được chạy.
