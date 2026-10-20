# Contracts — spec 002

- `openapi.json`: 7 thao tác public, phần mở rộng API001, không triển khai endpoint ở bước plan.
- `worker-openapi.json`: một thao tác private P2, không public ingress.
- `risk-rules-v1.json`: config thiết kế có phiên bản; chưa là bộ quy tắc đã hiệu chuẩn.
- `manifest.json`: SHA256 exact bytes và kích thước config. `.gitattributes` giữ byte
  JSON rule/manifest qua checkout để không hỏng hash do CRLF.

## Public endpoints

| Method/path | Story | Điều kiện |
|---|---|---|
| GET /api/v1/risk-rules | US3 | metadata không point, version/hash/config; không secret |
| GET /api/v1/simulation-scenarios | US4 | catalog preset, không lưu lựa chọn |
| POST /api/v1/weather/at-point | US1 | explicit click/temporaryConsent, zone/ruleVersion; LIVE gate |
| POST /api/v1/flood-history/nearby | US2 | consent, geography1000m; dataset thật kể cả mode mô phỏng |
| POST /api/v1/risk/evaluate | US3 | consent, point, mode/scenario, optional evidence |
| POST /api/v1/routes/search | US5 | consent, OD, mode/scenario; car-reference, 1–3 ứng viên |
| POST /api/v1/field-evidence/images | US6 P2 | raw JPEG/PNG <=5MiB/12MP, headers consent; không URL/multipart |

Mọi chức năng anonymous; auth001 vẫn có thể dùng cho history001, không làm điều kiện
truy cập002. CORS/ProblemDetails001 dùng nguyên contract. Mọi dữ liệu exactpoint trong
POST body; no-store responses, không URL/body/OTel logs tọa độ/ảnh. JSON<=16KiB.
OpenAPI servers `.invalid` là placeholder an toàn, không deploy URL đã tồn tại.
Notice `temporary-processing-v2` theo [privacy-notice.md](../privacy-notice.md): không
lưu/log tọa độ của ứng dụng khác với log tọa độ làm tròn tối đa90ngày tại Open-Meteo;
thu hồi hoặc xoá history không xoá ngay log nhà cung cấp. Quyết định này đã chốt.

`mode=simulation` bắt buộc ID catalog khớp ruleVersion; nhánh này không gọi weather
upstream dù routing có nhiều điểm. mode live scenarioId=null. Không cookie/server
session simulation. Sai version409, ngoài vùng422 trước outbound, consent false403;
flag true không thay thông báo/đồng ý thực của người dùng. Version notice cũ/thiếu
trả400 consent_notice_outdated trước outbound; chưa đồng ý trả403 consent_required. Quyền consent nằm RAM tab,
reload/close mất, click phải có điểm đang gửi, revoke abort/epoch bỏ kết quả muộn.

Weather current là model rain, raw interval→mm/h. Hourly đúng3slot tương lai,
slot thiếu=null. Nguồn không có dữ liệu trả explicit unavailable; notice/consent
chưa được triển khai kiểm chứng hoặc provider chưa đủ điều kiện thì503 không được
coi là empty weather success. Không còn quyết định zero-retention chưa chốt. Risk insufficient_data
200domain, score/level null; explain known/missing/uncertainty reserve, confidence.
Route không đủ geometry hợp lệ trả422; router unavailable503; không tạo đường thẳng.

Manual evidence OR signed evidenceToken, không chấp nhận AI score/URL tùy ý; evidence
chỉ áp điểm hiện tại, routing v1 không dùng evidence. Input token không log; hết hạn
trả400 evidence_expired và cho nhập tay/chụp lại, không tự cache ảnh. P2 disabled503
`evidence_not_enabled`, UI ghi “chưa làm” trong báo cáo, không đạt tiêu chí ảnh.
Lỗi worker/upstream có thể trả200 assessment=manual_required với fallbackReason để
UI chuyển nhập tay, hoặc ProblemDetails nếu lỗi request; cả hai trong deadline UI15s.

Bốn số liệu tuyến bắt buộc: highVeryHighMeters, missingMeters, routeRiskScore,
durationSeconds; routeRiskScore=null khi không có đoạn known, partial có coverage
và cảnh báo. Xếp hạng/fallback theo risk-rules.md, không tất định đảm bảo3tuyến.

## Worker contract

.NET→private worker `POST /internal/v1/image-assessments`, raw body JPEG/PNG,
service bearer + bounded correlationID + deadline<=12000ms. Không tọa độ, userID,
imageURL hoặc custom prompt. Model warm preload, inference1, no queue; overload429.
API budget14s, UI15s: error/timeout đều mở manual fallback. Response modelVersion
revision pin, assessment enum, template tiếng Việt; API ký evidence token, worker
không giữ state. Buffer RAM/finally/no tempfiles phải được test ở success/error/
reject/cancel/timeout, và không capture body tại reverse proxy/APM/debug dumps.

## Validation

Cần validator OpenAPI3.1 cho cả hai file + internal `$ref` resolution, contract tests
mode/versions/body caps/null/partial, checksum exact bytes của rule bundle. Mọi thay
đổi config phải đổi version+manifest và golden cases. `assets/risk-rules/` khi thực
thi cần cùng chính sách Git bytes; copy có kiểm hash, không duy trì hai bản tự sửa.
