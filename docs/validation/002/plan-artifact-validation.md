# Kiểm tra artifact kế hoạch spec 002

**Ngày**: 2026-10-20. **Phạm vi**: tài liệu/contract, không implementation hoặc benchmark.

## Lệnh đã chạy và kết quả

- `.specify/scripts/powershell/setup-plan.ps1 -Json`: thành công, feature002 resolve,
  tạo template plan; nhánh Git thực tế vẫn main, không tạo branch hoặc commit.
- `npx --yes @redocly/cli@2.15.0 lint specs/002-flood-risk-routing/contracts/openapi.json specs/002-flood-risk-routing/contracts/worker-openapi.json --extends minimal`:
  exit0, cả hai API descriptions valid. CLI tải vào npm cache, không thêm dependency
  hoặc đổi package lockfile của dự án.
- Python static validation: mọi internal `$ref` resolve, operationId không trùng,
  public7/internal1; manifest SHA256 và byte count khớp config; local markdown links
  trong plan/research/rules/data/quickstart/contracts README tồn tại; không marker
  template NEEDS CLARIFICATION; không có tasks002.
- Python arithmetic check độc lập: các ví dụ R01–R06 và ngưỡng Q01/Q02 khớp số học;
  không phải test Risk Engine đã triển khai hoặc chứng nhận mô hình thực địa.
- `git diff --check`: không lỗi whitespace trong thay đổi tracked; kiểm whitespace
  file mới theo static scan. `.gitattributes` contracts giữ rule+manifest exactbytes.
- Không có `.specify/extensions.yml`: không hook trước/sau plan cần chạy.

## Chưa thực hiện / bị chặn

| Hạng mục | Trạng thái |
|---|---|
| Risk/route xUnit thực thi, PostGIS SQL/meters tests | NOT RUN, chưa implementation |
| pytest pipeline/worker và Playwright002 | NOT RUN, chưa implementation |
| Florence CPU/GPU latency/VRAM/quality | NOT RUN; thiếu input/engine/quota/credential phải ghi BLOCKED khi chạy spike |
| OSRM extract/RAM/graph và E2E5s | NOT RUN; thiếu PBF/zone/Docker/quota phải ghi BLOCKED |
| OpenMeteo latency/quota sampling | NOT RUN, chỉ nghiên cứu tài liệu; không gửi vị trí thật |
| Weather LIVE FR019 | Tại bước plan: BLOCKED vì chưa quyết định cam kết; đã được thay bằng quyết định RESOLVED ở cập nhật cuối báo cáo |
| Deploy/infra provisioning | NOT RUN, ngoài bước plan |
| US6 ảnh | Thiết kế P2 có thể hoãn; chưa được báo đạt |

Không thay spec001 hoặc spec002 trong bước plan; các chỉnh sửa001 hiện có trong
working tree là từ yêu cầu đồng bộ trước đó. Không thay checkbox/task IDs001,
không tạo tasks002. GLOSSARY bổ sung thuật ngữ, hai ADR mới ghi quyết định quan trọng.

## Kết luận gate

Artifact thiết kế đã kiểm tra; kế hoạch có cổng chặn rõ. Chưa đủ điều kiện gọi thời
tiết LIVE hoặc báo MVP hoàn thành. Trước khi thực thi đường LIVE cần đóng ADR002;
spike số đo/quota/dataset và phần nền tảng001 vẫn là gates bắt buộc. Không tự đổi stack.

## Cập nhật sau quyết định người dùng — 2026-10-20

Kết quả validation phía trên là lịch sử của bước plan. Blocker thiết kế FR019 đã
RESOLVED: người dùng cho phép notice công bố retention Open-Meteo90ngày. Các yêu
cầu/spec/plan/contracts và ADR002 được đồng bộ sang temporary-processing-v2.
Không thay số đo NOT RUN hoặc coi implementation đã đạt; gate LIVE cần notice,
consent, redaction, điều khoản/quota/latency spikeC thực sự được kiểm chứng.
Đủ chuyển sang tạo tasks, chưa tạo tasks trong lần cập nhật này.
