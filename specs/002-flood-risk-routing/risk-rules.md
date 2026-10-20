# Risk rules v1 — thiết kế có phiên bản, chưa hiệu chuẩn thực địa

Nguồn chuẩn máy đọc: [contracts/risk-rules-v1.json](contracts/risk-rules-v1.json).
Manifest SHA256 kiểm exact UTF-8 bytes; bản này là artifact thiết kế trong spec,
implementation copy sang `assets/risk-rules/` bằng pipeline kiểm checksum, không sửa
hai bản độc lập. Mọi thay đổi giá trị hoặc thuật toán đổi version, checksum mới và
golden cases. Không override trọng số qua env mà giữ version cũ. Rule snapshot,
zone/dataset/scenario version cố định trong toàn request, mismatch409.

## Đầu vào và chuẩn hoá

Hàm clamp(x)=min(1,max(0,x)). Tất cả tính double hữu hạn, cấm NaN/Infinity/âm.

| Yếu tố | Chuẩn hoá [0,1] | Trọng số |
|---|---|---|
| C: mưa hiện tại | clamp(currentRainMm * 3600 / intervalSeconds / 20) | .40 |
| F: mưa dự báo | clamp(max(hourlyRainMm[3 khung giờ trọn tiếp theo])/20) | .30 |
| H: lịch sử gần | max(severity * reliability * max(0,1-distanceMeters/1000)) của điểm trong bán kính | .30 |

current.rain là mm trong interval nguồn trả, hiển thị rate mm/h và interval, không
nhầm mm15phút với mm1giờ. F lấy tối đa ba lượng mưa mỗi giờ, không tổng ba giờ rồi
nhầm thành một giờ. Thiếu bất kỳ một giờ trong3giờ thì Fmissing; UI vẫn hiển thị giờ
có dữ liệu với nhãn thiếu. Điểm ngoài1000m không tham gia; đúng1000m vẫn có trong
kết quả lịch sử nhưng đóng góp distance factor=0 (không đồng nghĩa dữ liệu thiếu).

Severity lịch sử low=.25, medium=.5, high=.75, severe=1. Trường severity thiếu là
unknown, không gán low. Reliability = declaredConfidence [0,1] * verificationFactor,
verified=1, unverified=.5, unknown=.25; UI hiển thị cả hai. Không có trùng lặp thì dùng
max nên thêm bản sao điểm không tự tăng risk. Dataset phải có metadata vùng đã khảo
sát và coverageComplete; H=0 chỉ khi dataset xác nhận coverage đủ, có0điểm phù hợp.
Dataset thiếu/coverage không đủ hoặc điểm gần có severity không biết thì Hmissing;
không dùng không có điểm để khẳng định không có nguy cơ.

Thời gian lịch sử là evidence lịch sử, không gọi là ngập hiện tại. Ngày tương lai,
nguồn/license/coordinate sai là reject; importedAt không thay observedAt. Dataset
reviewBy mặc định180ngày từ reviewedAt; quá hạn H có q=.5 và nhãn cũ, không tự sửa
ngày để tăng tin cậy. Không thể coi lịch sử là kiểm chứng tình trạng đường hôm nay.

## Missing, stale và confidence

Một yếu tố thiếu có `value=null, contribution=null, reason`, tuyệt đối không value0.
Với tập có dữ liệu A: `Sknown = 100 * sum(w_i*x_i)` trên A; `U=100*sum(w_i)` trên
nguồn thiếu. U là **dự phòng bất định**, không tuyên bố nguy cơ quan sát bằng100.
Báo khoảng `[Sknown, min(100,Sknown+U)]`; điểm dùng phân loại bảo thủ là đầu trên,
hiển thị riêng dự phòng bất định trong explanation để tổng đóng góp tái hiện được.
Không chia lại trọng số theo nguồn còn lại. Tính confidence `100*sum(w_i*q_i)`;
qfresh=1, qstale=.5, qmissing=0. q chỉ đổi confidence, không nhân giảm điểm nguy cơ.
Với lịch sử q áp theo dataset review freshness; reliability điểm đã nằm trong H.

Chỉ có mức rủi ro khi tổng trọng số nguồn có dữ liệu >=.60 và confidence>=30.
Không đủ thì `status=insufficient_data, score=null, level=null`, vẫn trả phân rã,
interval/nguồn/reasons/confidence. Bốn mức khi đủ: [0,25) thấp, [25,50) trung bình,
[50,75) cao, [75,100] rất cao. Status insufficient_data không phải mức thứ năm.
Đổi dữ liệu thiếu sang dữ liệu có thể giảm dự phòng bất định; không mô tả đó là
vi phạm đơn điệu. Thuộc tính đơn điệu chỉ so cùng availability/freshness/rule,
tăng C/F/H hoặc bằng chứng dương không làm giảm score hoặc mức.

## Bằng chứng ảnh/nhập tay (P2)

Không evidence thì bonus=0, không làm thiếu core. `bonus=10*E`, score và hai đầu
interval cap100. Manual mức không rõ/không ngập=0, thấp=.25, vừa=.5, cao=1, ghi rõ tự
khai/chưa kiểm chứng; không phải nguồn chính thức. AI flood_signs E=.5, no_clear_signs
E=0, uncertain E=0 + reason uncertainty; caption không được chuyển thành mực nước
chính xác hoặc xác suất ngập đã hiệu chuẩn. Evidence không bao giờ trừ score.

Evidence token stateless HMAC hết hạn5phút, chứa E/source/model/issuedAt/expiry,
không chứa ảnh/toạ độ/user; giữ ở RAM tab, chỉ dùng với điểm người dùng đang đánh giá.
Khi đổi điểm client xoá evidence; không dùng evidence đơn điểm để chấm mọi đoạn
trên tuyến, vì không chứng minh được liên quan không gian. Routing v1 chỉ dùng ba
yếu tố core, công bố `evidenceApplied=false`; bằng chứng có tác dụng đánh giá điểm.
Nếu US6 hoãn, adapter evidence không gọi worker và không báo đạt SC004 hoặc phần ảnh.

## Weather và mô phỏng

Fresh fetchedAge<=5phút, current validAge<=30phút và hourly phủ đủ khoảng. fetchedAge
(5,60]phút có nhãn stale q=.5, validTime không tự cập nhật; validTime current>30phút
thì Cmissing dù fetch mới. Tuổi>60phút loại cache; giờ thiếu/invalid Fmissing. Cũ
không phải tươi sau khi mạng phục hồi nếu chưa fetch thành công. Cache chỉ rounded
weather-cell data, không exact point/user history, TTL không sliding. Notice v2 công
bố retention Open-Meteo theo ADR-002 đã được chấp thuận; không dùng stale vượt giới
hạn để né kiểm thử nguồn/consent. Không đổi rule version vì lần sửa này chỉ đổi notice,
không đổi giá trị hoặc công thức rủi ro.

Preset allowlist: demo-dry(C=0; ba giờ0/0/0mm), demo-rain(C rate10mm/h;
10/10/10mm), demo-heavy(C rate20mm/h;20/20/20mm). Mỗi preset có tên tiếng Việt và
version, thời gian tương đối request nhưng nhãn MÔ PHỎNG rõ; current synthetic
interval3600s. Chỉ mưa thay, H/zone/map giữ thật. Server nhận mode+scenarioID theo
request, không lưu state hoặc gọi Open-Meteo; request không chọn preset trong
mode simulation bị400. mode live không chấp nhận scenarioID để tránh cache bleed.

## Tuyến: tổng hợp và thứ tự

Densify geography<=50m; sample endpoint/midpoint, score đoạn=max các score mẫu.
Nếu bất kỳ mẫu insufficient_data thì cả đoạn missing, score=null, không đưa vào
known-score denominator. Độ dài geography mét, không NTS Length trên4326. Border
zone không buffer. Việc sampling là xấp xỉ cần công bố, không xác nhận mọi cm đường.

`routeRiskScore = sum(length*segmentScore)/sum(knownSegmentLength)`;
knownLength=0 thì score=null, aggregateStatus=insufficient_data. Partial coverage
thì aggregateStatus=partial, nêu coverage%, unknown length và cảnh báo; không gọi
điểm trung bình trên phần biết là điểm toàn tuyến đầy đủ. `missingMeters=sum(length
của đoạn missing)`, `highVeryHighMeters=sum(length đoạn đã có level cao/rất cao)`.
Mỗi đoạn chỉ đếm1lần; known+missing=total trong dung sai1cm do tính toán, không xoá
unknown khỏi tổng chiều dài. Uncertainty reserve ở mẫu đã đủ nguồn được giải thích,
không gán đoạn thiếu toàn bộ thành score0.

Fastest là min duration của candidate hợp lệ sau dedupe/ST_Covers, thời gian gốc
OSRM không hiệu chỉnh mưa (có lưu ý). Eligible khi `T<=min(1.5*Tfastest,Tfastest+600)`.
Đúng ngưỡng được tính, comparison thời gian dùng integer milliseconds, không round
phút trước so sánh. Sorted tuple `(highMeters, routeRiskScore or +infinity, missingMeters,
durationMs, stableCandidateId)`. Nếu score null sắp như +infinity chỉ trong comparator,
JSON không có Infinity. Risk và mét so bằng giá trị chưa round UI; tie numerical
<=1e-6 score và <=0.01m, để thứ tự xác định dùng lượng tử cố định decimal 6score/2m
trước comparator, không comparator epsilon không bắc cầu. Trọng số/tolerance versioned.

Tuyến ít nguy cơ hơn: tuple `(highMeters,score)` nhỏ hơn fastest; nếu hai tiêu chí
bằng nhau, thiếu ít hơn ưu tiên nhưng giải thích là “ít thiếu dữ liệu hơn”, không
gọi đã giảm nguy cơ quan sát. Nếu không có candidate cải thiện nguy cơ trong ngưỡng,
trả fastest đầu (hoặc chỉ fastest) + no_lower_risk_within_budget; liệt kê constraint.
Tất cả candidates cao thì all_routes_high; unknown bất kỳ thì incomplete_data;
không có hỗ trợ tuyến nếu tất cả geometry bị loại. Không bao giờ thay NoRoute bằng
đường thẳng. Chỉ hứa ưu tiên trong tập OSRM trả, không toàn graph, không cam kết an toàn.

## Bảng tình huống kỳ vọng — synthetic, không phải dữ liệu thật

C/F/H trong bảng đã chuẩn hoá, freshness=1 nếu không ghi khác; không evidence.
Các ca là oracle tính tay; implementation phải kiểm độc lập, không sinh expected
bằng chính code Risk Engine rồi gọi là test.

| Ca | C | F | H | Kỳ vọng score / mức / confidence |
|---|---:|---:|---:|---|
| R01 | 0 | 0 | 0 | 0 / thấp /100, chỉ khi history coverage đủ |
| R02 | .5 | .5 | 0 | 35 /trung bình/100 |
| R03 | .5 | .5 | .5 | 50 /cao/100 |
| R04 | 1 | 1 | 1 | 100 /rất cao/100 |
| R05 | .625 | 0 | 0 | 25 đúng ngưỡng trung bình |
| R06 | 1 | 1 | 1/6 | 75 đúng ngưỡng rất cao |
| R07 | 0 | 0 | null | interval[0,30], score30/trung bình/conf70; dự phòng30 có nhãn |
| R08 | 1 | 1 | null | interval[70,100], score100/rất cao/conf70 |
| R09 | null | null | 1 | score/level null, insufficient, confidence30 |
| R10 | .5 | .5 | .5 | tất cả stale: score50/cao/conf50 |
| R11 | .5 | .5 | .5 | manual E1: score60/cao, evidence bonus10, core confidence100 |
| R12 | .5 | .5 | .5 | AI uncertain: score50, bonus0, uncertainty reason |
| R13 | 0 | 0 | .5→0 | high severity1/reliability1 tại500m→1000m: score15→0; điểm1000m vẫn hiển thị |
| R14 | 0→1 | F cố định | H cố định | score không giảm với availability/q cố định, kể cả clamp100 |
| R15 | mưa thiếu | 3giờ có1giờ thiếu | lịch sử missing | insufficient, không score0 |

Test đúng mỗi cutoff và +/-1e-6; tolerance tính score1e-6, không sửa thresholds.
Golden cases weather rate interval900s5mm=>20mm/h=>C1; mưa0 hợp lệ C0 khác null.
History distance kiểm SQL thật, không lấy Euclidean lat/lon.

| Ca tuyến | Đầu vào | Kỳ vọng |
|---|---|---|
| Q01 | Fastest600s, alternative900s/901s | 900 thuộc ngưỡng;901 loại |
| Q02 | Fastest1800s, alt2400/2401 | 2400 thuộc min2700,2400;2401 loại |
| Q03 | A(high100m,score60,missing0,T600),B(high0,score70,missing0,T800) | B trước A dù score trung bình cao hơn |
| Q04 | high bằng, score A40 B30 | B trước |
| Q05 | high/score bằng, missing A100 B0, thời gian A500 B600 | B trước vì ít thiếu dữ liệu |
| Q06 | high/score/missing bằng | thời gian nhỏ trước, rồi stableID |
| Q07 | không candidate giảm tuple risk trong budget | fastest + cảnh báo, không gọi an toàn |
| Q08 | 1000m known score80 +1000m missing | routeScore80 trên phần biết, missing1000m, partial; không score40 |
| Q09 | toàn tuyến unknown | score null, missing=total, cảnh báo; không score0 |
| Q10 | endpoints trong nhưng giữa tuyến ngoài zone | discard toàn tuyến, không clip |
| Q11 | simulation concurrent với live | không trạng thái global/cache bleed, provider calls simulation=0 |
