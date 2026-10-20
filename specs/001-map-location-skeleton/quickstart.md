# Quickstart và hướng dẫn xác minh

Đây là runbook cho implementation sắp tạo, KHÔNG phải lệnh đã chạy thành công.
Hiện repo chỉ có Spec Kit và tài liệu thiết kế; các path mã nguồn/compose/workflow
bên dưới phải được speckit-tasks rồi implementation tạo trước khi chạy.

## Prerequisites và cấu hình

.NET10 SDK, Node24 LTS/npm, Docker Engine/Compose v2 (Windows: Docker Desktop/WSL2),
GitHub repo để chạy Actions, tài khoản Vercel/Azure có quyền provisioning.
Tạo local secrets không commit; .env.example chỉ tên biến, giá trị mẫu không dùng thật.

| Biến | Nơi giữ | Ghi chú |
|---|---|---|
| ConnectionStrings__App | user-secrets/dev; Azure Key Vault reference | TLS VerifyFull production; không lộ web |
| ConnectionStrings__Hangfire | như App | cùng server/DB, schema hangfire; role tối thiểu |
| DemoAccounts__Json | secret | array tài khoản demo; không hardcode, không log |
| Jwt__PrivateKeyPem | secret | RSA private key đủ mạnh; cấu hình issuer/audience/kid |
| Jwt__Issuer, Jwt__Audience | API config | đúng contract validation, không đoán từ Host client |
| Cors__AllowedOrigins__0 | API config | origin Vercel production chính xác, không dấu slash cuối |
| NEXT_PUBLIC_API_BASE_URL | Vercel/dev public env | HTTPS production; bundle lúc build |
| NEXT_PUBLIC_TILE_URL | Vercel/dev public env | URL có điều khoản/attribution thích hợp; public token nếu provider yêu cầu |
| OTEL_EXPORTER_OTLP_ENDPOINT | API optional | không cấu hình thì không dựng collector/Grafana |
| AZURE_CLIENT_ID/TENANT_ID/SUBSCRIPTION_ID | GitHub env | OIDC, RBAC scope app |
| VERCEL_TOKEN/ORG_ID/PROJECT_ID | GitHub env secrets/config | không chuyển token cho bundle |

Production secret dùng Key Vault reference/managed identity, local user-secrets hoặc
Compose env file gitignored. GitHub Environment tách preview/prod. Nếu thiếu seed secret,
chỉ map anonymous khả dụng; operator nhận lỗi thiếu cấu hình account, không mật khẩu mặc định.

## Chạy local (sau implementation)

Từ repo root:

```powershell
docker compose -f infra/compose.yaml up --build -d
docker compose -f infra/compose.yaml ps
docker compose -f infra/compose.yaml logs --tail 100 api
```

Expected: DB healthy; job init extensions/migrations/seed thành công một lần;
API ready; frontend http://localhost:3000, API http://localhost:8080.
Public HTTPS dùng cho điện thoại; HTTP LAN không đủ điều kiện geolocation.
Không chạy down -v để reset khi cần giữ bằng chứng kiểm privacy.

## Kiểm tra tự động

```powershell
dotnet test backend/FloodPlatform.sln --configuration Release
npm --prefix apps/web ci
npm --prefix apps/web run lint
npm --prefix apps/web run typecheck
npm --prefix apps/web run build
npm --prefix apps/web exec playwright install --with-deps
npm --prefix apps/web run test:e2e
```

Scripts test:e2e và Playwright config phải trỏ root tests/e2e, webServer frontend/API
local và fixture seed dùng riêng cho tests; tests khởi DB container thật khi cần.
Expected: xUnit unit+integration không skip PostGIS; E2E Chromium/WebKit đều đạt.
Không dùng tài khoản/DB production cho test xoá. Lưu report nhưng loại GPS/token.

Kiểm OpenAPI sau triển khai: generated /openapi/v1.json đối chiếu
[contract](contracts/openapi.json) về path/schema/security/status; Scalar ở /scalar
chỉ dev hoặc được bảo vệ operator. OpenAPI thiết kế dùng server relative để tránh
đưa URL production giả; health public không expose connection string.

## Kịch bản nghiệm thu

1. Mở HTTPS trên Chrome Android/Safari iPhone >=360px; map TP.HCM, zone boundary,
   attribution và banner không che nút. Ghi10 cold loads theo mạng spec.
2. Chọn lấy GPS, grant quyền; marker+accuracy+time. Mốc GPS10s đo từ thao tác lấy
   vị trí khi bản đồ đã mở, loại thời gian trả lời quyền, theo US2/SC-002 đã đồng bộ. Bật tracking, đi bộ30–50m,
   đo update <=1s sau callback. Tắt -> không update đến muộn. GPS test mô phỏng
   không được báo là đo ngoài trời.
3. Deny quyền -> tiếng Việt, hướng dẫn cấp lại, không lặp prompt; chọn manual
   trong2 thao tác, không accuracy giả; tracking off, không lưu manual.
4. GPS ngừng -> giữ marker, stale30s với đổi màu+banner. Offline -> giữ tile cũ,
   GPS còn có thể update; online lại không gửi bù, chờ fix mới. Verify transition logs
   không GPS. Không reload để giả rằng tile toàn vùng có offline support.
5. Anonymous và signed-in savingOff: inspect network tới API/storage/log xác nhận
   không truyền hoặc lưu GPS. Tile provider vẫn thấy vùng đang xem, nêu rõ trong UI.
6. Login bằng secret demo account, saving vẫn off. Bật lưu -> điểm GPS trong/trên biên
   được ghi; ngoài vùng hiển thị “chưa hỗ trợ”, 422 khi cố ghi; không gửi manual.
7. Xem history, delete-all, SQL COUNT=0. Saving vẫn on -> điểm mới sau đó có thể xuất
   hiện; không nhầm với dữ liệu cũ khôi phục. Race test POST/off/delete/logout.
8. Hai tài khoản không xem/xoá chéo; logout online -> token cũ401. Logout offline
   chỉ bảo đảm client dừng, thông báo server session chưa revoke cho tới expiry.
9. Seed record tuổi30 ngày bằng fixture test -> list không trả ở đúng cutoff;
   chạy Hangfire cleanup -> raw SQL expired count=0. Dừng job xác nhận log/metric
   backlog, restart catch-up. Kiểm uptime không phải bằng chứng hard-delete đúng hạn.
10. /health/live200, /health/ready200 khi DB/zone/schema/job sẵn; ngắt DB ->ready503,
    live còn200; frontend status không lộ exception. Kiểm correlation với log lỗi.

Ghi kết quả từng SC-001–SC-015, ngày, thiết bị, build SHA, zone version, điều kiện mạng,
lệnh test và artifact report. Chưa chạy thì ghi “chưa xác minh”, không ghi đạt.

## Provision và deploy production

1. Xác nhận region/quota/chi phí, phiên bản PostgreSQL17+PostGIS phù hợp subscription.
   Chọn Azure Flexible Server, allowlist postgis rồi enable extension bằng quyền quản trị.
2. Tạo App Service Linux Basic+ Always On, HTTPS Only; deployment container có runtime
   net10.0 pin; DB TLS VerifyFull, firewall chỉ nguồn API/migration. Có tài khoản runtime
   và migration riêng; không cấp CREATE EXTENSION cho runtime.
3. Tạo Vercel project apps/web, set public API/tile URL; origin chính xác trong API CORS.
   Đóng gói support GeoJSON cùng checksum/version từ nguồn authoritative, không copy tay.
4. Cấu hình GitHub OIDC với Azure và Vercel secrets; chạy CI đầy đủ, migration/seed,
   deploy API image SHA rồi frontend theo pipeline plan. Không dùng demo seed mật khẩu
   ở NEXT_PUBLIC. Không tự deploy khi chưa được yêu cầu thực thi.
5. Smoke public live/ready, anonymous map, login tài khoản test, record/delete lịch sử
   test và log scrub. Không claim public URL tồn tại trước bước provisioning này.
6. Rollback image chỉ nếu migrations tương thích; khi restore backup phải scrub toàn
   bộ location_history, reset consent/revoke sessions và cleanup trước mở API.

Supabase fallback: enable PostGIS, identity/app/hangfire schema không expose Data API;
dùng direct hoặc session connection hỗ trợ môi trường mạng, không transaction pooler
cho Hangfire. Vẫn Identity ASP.NET, không Supabase Auth; chạy lại spatial/job integration
trước switch, không ghép cả hai DB vào pipeline cùng lúc.

## Chuẩn bị lượt demo

- Chốt khu vực, điểm ngập seed/tuyến demo và GeoJSON có phiên bản trước nghiệm thu
  vùng. Chưa có fixture thật thì gate bao phủ chưa đạt; fixture tổng hợp chỉ dùng
  cho test hình học, không thay bằng chứng dữ liệu demo.
- Seed tối thiểu3 tài khoản demo từ secret, phân công riêng cho người trình bày và
  giám khảo; không dùng chung tài khoản đồng thời vì login mới thu hồi phiên trước.
- Nói trước token chỉ RAM: reload cần đăng nhập lại và không tự bật lưu. Kịch bản
  có bước đăng nhập lại bằng tài khoản được phân công sau thao tác reload.
- Chuẩn bị riêng kịch bản5–7 phút, bảng so sánh Google Maps/Waze và mô hình kinh doanh;
  đây là tài liệu dự thi ngoài spec này. Cần nguyên văn5 tiêu chí BTC để đối chiếu.

## Rehearsal dự phòng

Chuẩn bị hotspot, điện thoại GPS khác, demo point thủ công trong vùng, Compose đã build
và dữ liệu demo gắn nhãn. API lỗi vẫn cho map/GPS local và banner lịch sử unavailable.
Nếu cloud không truy cập được, trình diễn local phải nói rõ là local; không báo đã lưu/
xoá thành công khi chưa có xác nhận DB. Chưa có routing/risk/AI/thời tiết trong demo này.

