# Research — spec 002

**Ngày kiểm tra nguồn**: 2026-10-20. Đây là nghiên cứu tài liệu và quyết định thiết kế,
không phải kết quả latency/RAM/quota. Mọi spike có trạng thái NOT RUN hoặc BLOCKED,
không có benchmark sản phẩm đã đạt. Không còn quyết định thiết kế ngầm; cổng riêng tư
đã được người dùng chấp thuận bằng cập nhật cam kết/notice v2; còn kiểm thử thực thi.

## R1 — Open-Meteo: sử dụng và privacy gate

**Decision**: Giữ Open-Meteo/.NET HttpClient. Người dùng đã chấp thuận sửa FR019
và notice để phân biệt không lưu của ứng dụng với retention nhà cung cấp. Notice v2
phải công bố tọa độ làm tròn 0,01 độ, bên nhận, thời hạn log và giới hạn thu hồi.
Không còn blocker thiết kế zero-retention; LIVE chỉ bật sau kiểm thử notice/consent/
redaction và xác nhận điều kiện sử dụng/quota, không tự đổi stack.

**Evidence**: Free chỉ non-commercial, dưới10.000/day,5.000/hour,600/min; quảng bá
sản phẩm thương mại cần kiểm điều kiện hoặc paid. Data CC-BY4.0 cần attribution.
Provider có thể giữ log tọa độ90ngày; paid cũng có URL/usage logs, sau90ngày giữ số
liệu tổng hợp. Không khẳng định mọi dữ liệu billing bị xoá trong90ngày. Nguồn:
[Terms & privacy](https://open-meteo.com/en/terms), [Pricing](https://open-meteo.com/en/pricing).

**Rationale**: Nói rõ trách nhiệm từng bên, không hứa zero-retention thay provider.
Rounding không bảo đảm ẩn danh. Thu hồi/xoá lịch sử ứng dụng không xoá tức thì log
đã truyền; không chuyển IP thiết bị/tài khoản/token sang provider. Ứng dụng vẫn
không lưu lịch sử mặc định hoặc log tọa độ, cache weather ô công khai chỉ RAM.

**Alternatives considered**: Giữ cam kết không lưu cho mọi bên và chờ zero-retention
(không chọn theo quyết định người dùng); coi free/rounding là không lưu (không đúng);
đổi nguồn/selfhost (không chọn, ngoài stack). Không biến chấp thuận sửa notice thành
chứng nhận spike/implementation đã đạt.

## R2 — Weather semantics, freshness và cache

**Decision**: current.rain theo interval, hourly.rain ba giờ trọn tiếp theo; UI nói
nguồn mô hình, không đo tại trạm. URL upstream cố định, rounded grid0.01độ;
HTTP1.5s tổng, không retry, RAM weather cache bounded128cells,5min fresh/60min max.
Tuổi current validTime>30min là thiếu, forecast thiếu giờ là Fmissing. Provider
fetchedAt khác validTime; không bịa thời điểm model run. Latency chưa đo.

**Evidence**: Current dùng dữ liệu mô hình15phút; hourly có đơn vị mm và hourly.time,
current.interval cho phép chuyển rate. Phạm vi dữ liệu do model quyết định, không
suy ra đo chính xác trên từng đoạn đường. [Forecast API](https://open-meteo.com/en/docs).

**Rationale**: Giảm call/PII gửi bên ngoài nhưng vẫn giữ nhãn sai số/nguồn; RAM không
cần Redis cho20người. `RiskResponse.weatherSnapshot` trả nguyên snapshot thời tiết
đã dùng, gồm thời điểm, nguồn, current/ba giờ dự báo, chất lượng và mode/scenario.
Sau kết quả risk, UI hiển thị mưa và vẽ Recharts chỉ từ snapshot này. Endpoint mưa
riêng chỉ phục vụ US1 khi chưa tính risk; không ghép response mưa độc lập vào risk.
Không dùng mã snapshot để lưu/tra cứu tại máy chủ. Cache weather cell hiện có vẫn
được dùng khi lấy dữ liệu, nhưng hết hạn cache không đổi snapshot đã gắn với response.
Test gọi US1 lấy W1, để cache hết hạn rồi tính risk bằng W2: biểu đồ sau risk phải
dùng W2; W1 trả muộn hoặc cache đổi tiếp cũng không được ghi đè.

Routing áp dụng cùng nguyên tắc: `RouteResponse.weatherSnapshots` chứa các snapshot
ô mưa thật sự đã dùng trong request; mỗi segment có `weatherSnapshotIndexes` trỏ
vào mảng ngay trong response đó. Đây chỉ là vị trí phần tử của payload, không phải
mã tra cứu hoặc state snapshot ở máy chủ. UI chi tiết tuyến/đoạn lấy mưa từ payload
tuyến, không gọi endpoint mưa để dựng lại bằng dữ liệu mới. Biểu đồ điểm risk vẫn
dùng RiskResponse của điểm đó, không tự đổi sang ô mưa của tuyến.
**Alternatives**: gọi mỗi GPSfix hoặc mỗi50msegment gây thừa call, trái consent/SLA;
lưu request cacheDB tạo lịch sử vị trí ngoài scope; background refresh không cần.

## R3 — Đo khoảng cách và vùng

**Decision**: ST_DWithin geography spheroid mét, inclusive1000m với tolerance1mm chỉ
roundoff; GiST index cùng type query. ST_Covers geometry cho cả LineString và zone,
không ST_Contains, không snap/buffer vùng. Segmentize geography mét, max50m, kiểm lại
full geometry trước segmentation. [ST_DWithin](https://postgis.net/docs/ST_DWithin.html),
[ST_Project](https://postgis.net/docs/ST_Project.html),
[ST_Covers](https://postgis.net/docs/ST_Covers.html),
[ST_Segmentize](https://postgis.net/docs/ST_Segmentize.html).

**Rationale**: geometry4326.Length tính độ; endpoint trong không bảo đảm toàn tuyến trong.
**Alternatives**: EPSG3857 khoảng cách không phải mét mặt đất chính xác; strict floating
point cutoff thiếu test border; nới vùng theo GPS accuracy đổi contract001.

## R4 — OSRM graph và alternatives

**Decision**: Linux VM private cùng vùng Azure; Docker MLD extract→partition→customize→routed,
car.lua checksum/PBF checksum. Request alternatives=2, geometryfull; có thể chỉ1tuyến,
rerank<=3 ứng viên. Không global flood shortest-path. CPU/RAM4vCPU8GiB preprocessing,
2vCPU4GiB runtime là điểm bắt đầu spike, không kết luận đủ RAM.

**Evidence**: alternatives tìm tối đa số yêu cầu, không đảm bảo có; profile ở preprocessing,
đổi `driving` URL không biến car thành xe máy. Nguồn:
[OSRM README](https://github.com/Project-OSRM/osrm-backend),
[OSRM API](https://project-osrm.org/docs/v5.24.0/api/),
[HTTP docs](https://github.com/Project-OSRM/osrm-backend/blob/master/docs/http.md),
[Osmium extract](https://docs.osmcode.org/osmium/latest/osmium-extract.html).

OSM graph/map dùng ODbL, giữ attribution OpenStreetMap contributors và link license
trên màn hình tuyến; khi phân phối dataset/graph phải rà soát nghĩa vụ ODbL riêng,
không trộn dữ liệu nhóm chưa rõ quyền vào OSM. [OSM copyright](https://www.openstreetmap.org/copyright).

**Rationale**: Giữ đúng stack, không public demo dependency, route phải thật.
**Alternatives**: OSRM public demo cho production (không SLA/quota/riêng tư đủ);
exclude arbitrary flood polygons (không phải chức năng Route API); fake straight line
(không đáp ứng). RAM wiki planet cũ không suy ra RAM TP.HCM; spike bắt buộc.

## R5 — Florence-2 và nơi chạy P2

**Decision**: Model base0.23B, prompt cố định/caption mapping bảo thủ; CPU FP32 và GPU
FP16 phải benchmark riêng. CPU Azure ContainerApps và GPU T4 ContainerApps là hai
phương án cùng Docker/FastAPI; chọn qua chất lượng/latency/quota/budget. Không nêu
đơn giá Azure giả định; spike chụp quote vùng/ngày và chi phí minReplica/idle.

**Evidence**: Florence là model vision tổng quát, MIT, không flood classifier hiệu
chuẩn; Transformers có native support. GPU khả dụng phụ thuộc region/quota, cold
start phải đo. [Model card](https://huggingface.co/microsoft/Florence-2-base),
[Transformers Florence2](https://huggingface.co/docs/transformers/model_doc/florence2),
[Azure GPU](https://learn.microsoft.com/en-us/azure/container-apps/gpu-serverless-overview),
[Workload profiles](https://learn.microsoft.com/en-us/azure/container-apps/workload-profiles-overview).

**Alternatives considered**: HF dedicated custom endpoint có thể chạy nhưng thêm
nhà vận hành dữ liệu và billing, không chọn mặc định; CPU đạt15s chưa chứng minh;
GPU không quota thì BLOCKED, không đổi model. US6 có thể hoãn thay vì kéo core.

## R6 — Ảnh tạm, deadline và abuse

**Decision**: Raw JPEG/PNG stream bounded5MiB, decoded12MP, RAM-only, không URL hoặc
multipart spool; worker1concurrent, queue0, deadline12s,API14s,UI fallback15s.
finally dispose, process terminate khi infer quá hạn; no body/access debug dumps.

**Evidence**: Buffered uploads có thể ghi tempfile, cần tránh mặc định có spool.
[FastAPI request files](https://fastapi.tiangolo.com/tutorial/request-files/),
[ASP.NET uploads](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0),
[Pillow image safeguards](https://pillow.readthedocs.io/en/stable/reference/Image.html).

**Rationale**: Đảm bảo không ảnh trên disk trong thành công/lỗi/cancel, không chỉ
xoá cuối happy path. Timeout thread không đồng nghĩa PyTorch dừng, dùng process
có thể chấm dứt. Không thể bảo đảm zeroization vật lý RAM tuyệt đối.
**Alternatives**: object storage (trái scope), async queue (vỡ SLA, thêm state),
confidence token score như xác suất ngập (không hiệu chuẩn).

## R7 — API, observability và seams

**Decision**: Built-in MinimalAPI validation/rate limiter, reused ProblemDetails/CORS001,
POST cho point query; redaction HttpClient/OTel/SQL/proxy trước sink, không chỉ Serilog.
IP limiter một instance demo, global quota provider; không coi CORS chống bot.
[ASP.NET rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0),
[HTTP logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/?view=aspnetcore-10.0).

**Rationale**: OSRM URL và OpenMeteo query có thể làm lộ tọa độ qua auto-instrumentation;
POST public không tự bảo vệ outbound log. Không cần thêm service/hạ tầng để giảm rủi ro.
**Alternatives**: log request đầy đủ để debug (trái privacy), mock DB chứng minh geography
(không kiểm SQL thật), tách mỗi core thành microservice (không cần cho MVP).

## Quyết định số học của plan và các điểm phải chứng minh

risk-rules.md cụ thể hóa FR007/012, không phải validation khoa học. Điểm risk có
dự phòng bất định tách khỏi contributions; missing không là0; partial route score
trên known length, allunknown=null. Ngưỡng thời gian1.5×fastest và+600s (lấy min),
max50msegment/12weathercells/3000segments là lựa chọn thiết kế có phiên bản cần đo.
Không có nguồn khoa học nào trong research khẳng định các trọng số này dự báo ngập
chính xác. Nhóm duyệt golden cases và data quality trước release.

Spike A/B/C định nghĩa tại plan, artifacts tại docs/validation/002. Đóng gate cần
báo cáo thực nghiệm; thiếu Docker/dữ liệu/quota/credential ghi BLOCKED. Trạng thái
provider privacy đã RESOLVED ở mức quyết định/notice; không gộp thành Pass thực thi
cho latency/quota/consent tests chưa chạy.
