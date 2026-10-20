# ADR-002: Công bố retention Open-Meteo trong notice đồng ý

**Ngày**: 2026-10-20. **Trạng thái**: Chấp thuận ở mức quyết định sản phẩm/tài liệu.
Quyết định của người dùng: “Cho phép sửa cam kết/notice để nói rõ dữ liệu gửi
Open-Meteo và thời hạn lưu của họ.” Thay quyết định giữ gate đóng trước đó;
không phải xác nhận implementation/spike đã đạt.

## Bối cảnh

FR019 và ngoại lệ001 ban đầu nói mọi bên chỉ dùng tạm, không lưu. Tài liệu
[Open-Meteo terms/privacy](https://open-meteo.com/en/terms) được kiểm2026-10-20
nêu log có thể chứa tọa độ, giữ90ngày; gói trả phí cũng có usage URL logs và giữ số
liệu tổng hợp sau khi xoá log chi tiết. Không thể hứa zero-retention thay provider.

## Quyết định được chấp thuận

Sửa FR019/SC011 và ngoại lệ FR008/SC008/US5 của001, đồng bộ plan/contracts:
notice `temporary-processing-v2` nêu chính xác phạm vi gửi điểm đến ứng dụng,
rounded cell0,01độ đến Open-Meteo và thời hạn log90ngày của bên nhận. Không chuyển
IP thiết bị/tài khoản/token; provider vẫn nhận request URL và kết nối của máy chủ.
Không nói rounding là ẩn danh tuyệt đối hoặc paid tự động không log.

Ứng dụng không lưu vị trí chính xác mặc định và không ghi tọa độ vào log; history
consent, hard delete, cutoff30ngày và backup commitments001 giữ nguyên. RAM weather
cell cache<=60phút không gắn người dùng được công bố. Consent chỉ RAM tab, explicit
click, revoke abort/drop late, GPS tracking không tự gửi; bản notice cũ phải đồng ý lại.
Thu hồi hoặc xoá history không xoá tức thì log provider. Mô phỏng không gọi provider.

## Đánh đổi

Giữ Open-Meteo/stack đã chốt và nói rõ retention thay vì chờ thoả thuận zero-retention
chưa có. Người dùng có thể không đồng ý và tiếp tục map/localGPS. Đổi nguồn/selfhost
hoặc mua gói không nằm trong quyết định này; điều kiện free/commercial cần spike.
Đây là ngoại lệ phạm vi nhà cung cấp được phê duyệt, không nới quy tắc log ứng dụng.

## Cổng triển khai

Blocker **quyết định** FR019 đã RESOLVED, đủ lập tasks. Trước LIVE phải triển khai
notice v2 và reject version cũ, test consent/redaction/rounding/no identity, hoàn tất
spikeC về điều khoản/quota/latency. Giữ Weather__LiveEnabled=false cho tới khi kiểm
chứng; không báo US1 đạt vì đã có quyết định trên giấy. Không tạo task ở lần sửa này.
