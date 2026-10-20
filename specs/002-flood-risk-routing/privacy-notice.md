# Notice đồng ý xử lý vị trí — temporary-processing-v2

**Trạng thái**: Nội dung sản phẩm đã được người dùng chấp thuận về phạm vi ngày
2026-10-20; chưa triển khai giao diện. Dùng nội dung này cho cả GPS/điểm chọn tay,
người ẩn danh/đã đăng nhập. Không phải đồng ý lưu lịch sử.

## Nội dung hiển thị trước nút đồng ý

**Cho phép xử lý điểm bạn yêu cầu trong tab này?**

- Điểm sẽ gửi đến máy chủ ứng dụng: **<điểm GPS hoặc điểm chọn tay đang xem>**.
  Khi tìm tuyến, nêu cả điểm đi/đến; thao tác tìm tuyến cho phép xử lý các điểm mẫu
  dọc ứng viên để đánh giá mưa/rủi ro, không tự gửi các vị trí GPS đang di chuyển.
- Để lấy thời tiết thật, máy chủ ứng dụng gửi tọa độ **làm tròn theo ô 0,01 độ** đến
  **Open-Meteo** cho điểm yêu cầu hoặc ô mưa dọc tuyến. Không chuyển tài khoản,
  token đăng nhập hoặc IP thiết bị của bạn đến Open-Meteo. Họ vẫn nhận thông tin
  kết nối của máy chủ gọi và URL yêu cầu; làm tròn không bảo đảm ẩn danh tuyệt đối.
- Ứng dụng chỉ xử lý vị trí chính xác tạm thời, không ghi vị trí vào log hoặc tự lưu
  lịch sử. Dữ liệu thời tiết ô làm tròn có thể được giữ trong RAM tối đa60phút,
  không gắn tài khoản. Lưu lịch sử vị trí vẫn cần lựa chọn riêng của spec001.
- **Open-Meteo có thể lưu log yêu cầu chứa tọa độ làm tròn tối đa90ngày theo chính
  sách công bố.** Với dịch vụ trả phí, sau khi xoá log chi tiết họ có thể giữ số liệu
  sử dụng tổng hợp. Đây là chính sách bên nhận, không phải lịch sử vị trí ứng dụng.
  Xem [chính sách Open-Meteo](https://open-meteo.com/en/terms).
- Bạn chỉ đồng ý một lần trong phiên tab; trạng thái chỉ ở bộ nhớ, tải lại/đóng tab
  thì mất. **Chỉ khi bạn bấm yêu cầu mới gửi điểm**; theo dõi GPS bật không tự gửi.
  Trước mỗi thao tác luôn thấy điểm sắp gửi. Mô phỏng mưa không gọi Open-Meteo.
- Nút **Thu hồi đồng ý** dừng gửi mới, huỷ yêu cầu đang chờ và bỏ kết quả trả muộn.
  Thu hồi hoặc xoá toàn bộ lịch sử trong ứng dụng **không xoá tức thì log đã có tại
  Open-Meteo** và không lấy lại được dữ liệu đã truyền.

Nút: **Đồng ý cho tab này** / **Chưa đồng ý**. Không chọn sẵn đồng ý, không gắn với
đăng nhập hoặc bật lưu lịch sử. Chưa đồng ý vẫn dùng được bản đồ và vị trí tại thiết bị.

## Cách dùng và kiểm chứng

Client giữ consentNoticeVersion=temporary-processing-v2 trong RAM cùng consent;
không lưu lựa chọn vào localStorage/sessionStorage/cookie/DB. API chỉ nhận version
notice hiện hành; client gửi bản v1/thiếu hoặc consent=false thì từ chối trước mọi
outbound request, báo consent_notice_outdated hoặc consent_required, yêu cầu hiển
thị notice v2/đồng ý lại. Cùng code400 cho version cũ,403 cho chưa đồng ý; không đổi
ruleVersion vì notice không thay công thức rủi ro.

Kiểm thử notice đủ nội dung, link hoạt động, điểm/OD hiện đúng, rounded cell request,
không device identity, chỉ click mới gửi, revoke/reload, cả hai trạng thái tài khoản.
Mock outbound để kiểm phiên mô phỏng gọi Open-Meteo0lần; fake sink/exporter không có
tọa độ/ảnh. Không kiểm thử bằng cách đòi log Open-Meteo bị xoá ngay, vì không có cam
kết đó. Điều kiện commercial/quota/latency vẫn cần spike, không tự mua dịch vụ.
