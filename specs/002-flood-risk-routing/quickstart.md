# Quickstart validation — spec 002 (hướng dẫn sau khi triển khai)

**Chưa có implementation spec002**. Các đường dẫn/command bên dưới là hợp đồng
cho bước tasks/implement tương lai, không phải lệnh đã chạy thành công. Không deploy
hoặc tạo hạ tầng trong bước plan. Tái dùng [001 quickstart](../001-map-location-skeleton/quickstart.md).

## Prerequisites và gates

1. Nền tảng001: map/GPS/manual/zone/HTTPS và auth/history khi dùng đã nghiệm thu;
   dataset/vùng nhóm cung cấp, chưa có thì BLOCKED, không thay bằng data giả.
2. .NET10 SDK/global.json001, Node lockfile001, Python3.12 và Docker Engine chạy
   Linux containers; `docker info` thành công, Testcontainers PostGIS sẵn sàng.
3. Rule config+manifest, flood dataset+manifest, support zone, OSM PBF/profile/digest
   đã nhóm kiểm; scripts không tự tải ảnh/dữ liệu thật không rõ quyền.
4. Trước weather LIVE: quyết định privacy FR019/ADR002 đã chốt; phải triển khai và
   kiểm thử notice temporary-processing-v2/consent/redaction, xác nhận điều kiện
   commercial/quota. Chưa đạt thì không bật LIVE; mock/mô phỏng không báo đạt US1.
5. P2: ảnh có nhãn/quyền sử dụng, model revision, worker token, quota CPU/GPU;
   thiếu thì hoãn US6 và ghi “chưa làm” kèm lý do.

## Local sequence (dự kiến)

```powershell
# Từ root, sau khi các script/file này được implement.
docker info
npm ci
python -m venv .venv-data
.\.venv-data\Scripts\python -m pip install -r scripts/flood-data/requirements.lock
.\.venv-data\Scripts\python scripts/flood-data/validate.py --manifest assets/flood-history/manifest.json
docker compose -f infra/compose/compose.yml --profile routing up -d --build
# migration/zone+flood seed job theo001 rồi chờ health ready
dotnet restore backend/FloodPlatform.sln --locked-mode
dotnet test backend/FloodPlatform.sln --no-restore
.\.venv-data\Scripts\python -m pytest scripts/flood-data/tests
npm --prefix apps/web run typecheck
npm --prefix apps/web run test:e2e
```

Compose mở rộng file001, nếu implementation chọn tên file001 khác thì dùng đúng tên
có thật, không tạo hai Compose mâu thuẫn. Profile routing: DB/API/web001+OSRM, không
AI. Profile vision chỉ thêm worker khi P2 làm. Không dùng compose fallback giả OSRM
để báo tuyến thật đạt. HTTP localhost đủ dev, điện thoại dùng HTTPS hợp lệ001.

OSRM graph preparation trước runtime (Docker image pin từ spike, không latest):

```text
osmium extract --polygon demo-buffered.geojson --strategy complete_ways source.osm.pbf -o demo.osm.pbf
osrm-extract -p /opt/car.lua /data/demo.osm.pbf
osrm-partition /data/demo.osrm
osrm-customize /data/demo.osrm
osrm-routed --algorithm mld /data/demo.osrm
```

Lưu PBF hash/license/ngày, profile hash, graph manifest và peak RSS. Technical buffer
không mở vùng phục vụ. Không dump URL OSRM tọa độ thật vào command transcript/logs.

## Environment additions (không phải secrets mẫu thật)

| Key | Nơi | Quy tắc |
|---|---|---|
| Risk__RulesPath / ManifestPath | API | bundle đã validate/hash, không override weight rời version |
| FloodHistory__ManifestPath | seed job | nguồn nhóm, immutable |
| Weather__LiveEnabled | API | false cho tới khi notice v2/consent/redaction và spike C được kiểm chứng |
| Weather__BaseUri | API | allowlist Open-Meteo, không client chọn |
| Weather__ApiKey | secret store nếu paid | không PUBLIC/env web, query redact |
| Routing__OsrmBaseUri | API private | không public browser, không gửi GPS background |
| Evidence__Enabled | API/web capability | false nếu US6 hoãn |
| Evidence__WorkerUri/ServiceToken/SigningKey | backend secrets | không commit, no logs |
| NEXT_PUBLIC_API_BASE_URL | Vercel | reuse001 public host, không secret |

Consent/scenario không env/sessionStorage/localStorage/cookie; mỗi tab RAM. Telemetry
OTLP/CORS/DB/auth dùng001; không cấu hình lại trong spec002.

## Trình tự validation

| Bước | Hành động | Kết quả / SC |
|---|---|---|
|1| Mở map, bật theo dõi, di chuyển GPS giả trong test | Không request spatial mới khi không bấm; consent/history độc lập SC011 |
|2| Đồng ý notice v2: điểm/rounded cell/bên nhận/log90ngày/giới hạn thu hồi, bấm xem mưa LIVE sau gate thực thi | <=3s, source/model validTime/units SC001; mưa0 khác null |
|3| Từ chối/thu hồi consent, pending response đến muộn/reload tab | Không gửi mới, abort và bỏ stale response; hỏi đồng ý lại SC011 |
|4| Xem history tại fixtures dưới/đúng/trên1000m | inclusive boundary+tolerance1mm, nhãn nguồn/tin cậy SC007/010 |
|5| Chọn preset có tên, không có free-form rain | Banner fixed/nút tắt mọi screen, only rain override, weather outbound0 SC009 |
|6| Xem risk và chart | <=3s, 100% phân rã/version/confidence/source, R01–R15 SC002/006 |
|7| Chọn OD tuyến demo được nhóm cấp | <=5s, 1–3 candidates hợp lệ; bốn số liệu và lý do/rank Q01–Q11 SC003/005 |
|8| All-high/unknown/no-route/outside/backend503 | Cảnh báo/ProblemDetails đúng, map chức năng khác vẫn hoạt động SC008/010/012 |
|9| Tắt mô phỏng/reload và tab khác đang LIVE | Không bleed mode/cache, no fake freshness SC009 |
|10| P2 ảnh thành công/error/timeout/cancel/large/malformed | <=15s response hoặc manual, disk không có ảnh SC004/008/011 |

Các giá trị rules theo risk-rules.md; xUnit unit/SQL tests và pytest đều phải chạy
trước khi đánh dấu gate. Browser mocks dùng endpoint contracts, không fake green từ
mô hình chưa có. Endpoint worker tắt do US6 hoãn trả503 evidence_not_enabled,
UI không trình bày ảnh đang hoạt động; tests ảnh không được ghi pass.

## Spike và báo cáo

Hồi quy snapshot: gọi US1 nhận W1, làm cache hết hạn rồi gọi risk nhận W2. Sau risk,
UI mưa/biểu đồ phải khớp `RiskResponse.weatherSnapshot=W2`, không W1 hoặc lần fetch
khác; thử W1 trả muộn. Với route, làm cache đổi sau khi chấm tuyến rồi kiểm UI đoạn
vẫn đọc `RouteResponse.weatherSnapshots` và các chỉ số trong response, không gọi
lại nguồn để giải thích. Không mã snapshot hoặc kho lookup ở máy chủ.

Trước milestones tính năng: báo cáo Florence CPU/GPU, OSRM graph/RAM/E2E, OpenMeteo
latency/terms/privacy. Ghi machine/SKU/region/date/version/command, cold/warm,
p50/p95/max/raw duration, exceptions/quota, PASS/FAIL/BLOCKED. Dữ liệu cá nhân không
vào artifacts. Giá CPU/GPU lấy quote thật ngày đo, không suy từ cấu hình dự kiến.

Mưa/risk đo30lần mỗi trạng thái; tuyến30cặp×3lần/conditions cold/warm, concurrency1/5;
20người synthetic tổng. Chỉ dùng p95 để chẩn đoán; spec “trong Xgiây” cần báo cả max
và từng ca vượt, không tự đổi tiêu chí thành p95. Client performance.now() đầu click
và sau frame render đủ kết quả; network/server spans hỗ trợ, không thay phép đo UI.

## Production và demo

Theo pipeline001, bổ sung flood migration/import + rule/scenario graph manifest;
AppService API, Vercel web; private OSRM VM graph read-only, AI ContainerApps khiP2.
Chưa có quota/credentials/VNet/privateDNS hoặc dữ liệu thì BLOCKED. Health component
weather/OSRM/AI degraded không làm sập map. Smoke non-personal point, không log coords.

Diễn tập điện thoại Chrome Android/Safari iPhone, cả GPS và chọn tay, offline/deny/
API lỗi, hotspot/localCompose dự phòng. Mưa mô phỏng có banner đúng; nguồn thật lỗi
không dùng labelLIVE cho fixture. OSRM không chạy thì không báo đạt demo tuyến;
ảnh lỗi thì manual/hoãn. Ghi từng SC001–012 vào docs/validation/002/acceptance.md:
PASS/FAIL/BLOCKED/“chưa làm”, artifact/version/time và phần ảnh hoãn cụ thể.
US1–5 bắt buộc, US6 hoãn không thành đạt toàn spec. Không sửa trạng thái task001.

