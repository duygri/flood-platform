# API contract notes

[OpenAPI 3.0.3](openapi.json) có10 paths/13 operations. Đây là contract thiết kế,
chưa có API hoạt động. Scalar dùng OpenAPI sinh từ implementation, có check drift CI.

| Method | Path | Quyền / kết quả chính |
|---|---|---|
| GET | /health/live | Public200 process alive |
| GET | /health/ready | Public200 ready hoặc503, chỉ status và checkedAt |
| POST | /api/v1/auth/login | Public, demo credentials;200 JWT/session/settings |
| POST | /api/v1/auth/logout | Bearer;204 sau revoke sid, savingOff và tăng generation |
| GET | /api/v1/auth/session | Bearer;200 user/session/settings |
| GET, PUT | /api/v1/me/history-settings | Bearer; GET cài đặt, PUT consent với expected generation |
| POST | /api/v1/me/location-history | Bearer+consent;201 created/200 duplicate hợp lệ |
| GET | /api/v1/me/location-history | Bearer;50 mặc định/max100, cursor, loại expired |
| DELETE | /api/v1/me/location-history | Bearer; body expected generation;204 hard delete + X-Write-Generation |
| GET | /api/v1/support-zone | Public GeoJSON Feature + version/checksum/ETag,304 khi phù hợp |
| POST | /api/v1/support-zone/check-point | Public, không lưu, body point/version;200 supported true/false |
| POST | /api/v1/client-events | Public rate-limited;202 chuyển trạng thái không tọa độ |

## Invariants

- Không endpoint nhận userId để chọn chủ dữ liệu; đọc từ sub đã xác thực.
- JWT RAM15 phút, không refresh; private key không lộ client. Logout phải revoke
  server-side, không chỉ xoá token hoặc gọi cookie SignOut. Thiếu/expired/revoked401.
- Settings PUT và DELETE yêu cầu generation. Toggle/login/logout/delete tăng generation;
  POST point cũ409; server lock/check/insert cùng transaction. DELETE có body được
  fetch gửi tường minh; OpenAPI/client generator phải kiểm hỗ trợ body DELETE.
- Mọi authenticated response no-store; thêm Cache-Control no-store cho login dù public.
- Ngoài vùng khi check trả200 supported=false, message “chưa hỗ trợ”; cố ghi history422.
  Zone version sai409; reload zone rồi đợi fix mới, không replay fix bị reject.
- GeoJSON tọa độ [longitude,latitude], Point4326. Polygon kiểm ST_Covers gồm cạnh/đỉnh.
  Anonymous GPS chỉ check local, không POST point tới server trước consent.
- source chỉ gps: client phải chặn manual. Browser không chứng minh nguồn GPS hoặc
  chống giả tọa độ; tính năng anti-spoofing không có trong scope.
- Duplicate sampleId chỉ trả200 nếu payload khớp và session/consent/generation vẫn hợp lệ;
  payload khác409; sau delete không resurrect sample cũ. Không tự retry upload.
- Delete204 nghĩa transaction đã commit DELETE thật. Saving state giữ nguyên,
  điểm mới với generation mới có thể lưu tiếp. Delete không làm sạch backup ngay.
- GPS quá15s hoặc tương lai quá5s422; finite accuracy0..100000m; invalid coordinates400.
  Hết30 ngày tính theo recordedAt; list không trả, cleanup physical theo SLA của plan.
- Error gồm type/title/status/detail/code/traceId; body application/problem+json;
  readiness503 là health JSON có chủ đích. Không stack/SQL/token/GPS trong errors.
- Stable error codes: validation_failed, invalid_credentials, account_locked,
  session_invalid, saving_disabled, generation_conflict, zone_version_conflict,
  outside_support_zone, stale_location, future_location, duplicate_sample_conflict,
  rate_limited, dependency_unavailable. Title/detail tiếng Việt, code không dịch.

## Rate limit và kích thước

Single API instance MVP: login5/phút/IP cộng Identity lockout5 lần sai/5 phút;
record12/phút/user; check-point60/phút/IP; events30 batches/phút/IP, body<=16KB,
max100 events/batch; các endpoint user khác60/phút/user, body<=16KB.
429 có Retry-After; không log raw IP dài hạn hoặc PII trong metric labels.
Lưu lượng thật vượt mục tiêu cần load test và distributed limiter sau ADR, không
thêm Redis chỉ để làm rate limit demo. Giới hạn tổng reverse-proxy bổ sung ở deployment.

## Client telemetry

Allowlist eventId/event/occurredAt, reject unknown properties. Không nhận message tự
soạn/URL/coordinates/email để tránh kênh log rò dữ liệu. RAM ring100 events và24h;
close/reload mất log chưa gửi là giới hạn demo đã ghi, không persist GPS hoặc token.
Event batch endpoint không phải nơi chuyển lịch sử offline; tuyệt đối không point replay.

## Mở rộng

Không expose /register /forgot-password /routing /risk /weather /ai hoặc endpoint
xoá một điểm trong phiên bản này. Không public Hangfire Dashboard; Scalar dev/protected.
