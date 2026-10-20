# Cảnh báo ngập và bản đồ demo

Thuật ngữ nghiệp vụ cho nền tảng bản đồ/vị trí; không định nghĩa kiến trúc triển khai.

## Language

**Vùng hỗ trợ**:
Khu vực demo nội thành mà ứng dụng phục vụ; điểm trên ranh giới cũng thuộc vùng.
_Avoid_: Toàn TP.HCM, vùng an toàn

**Vị trí GPS**:
Điểm do thiết bị trả về cùng độ chính xác và thời điểm xác định.
_Avoid_: Vị trí chắc chắn, điểm thủ công

**Điểm thủ công**:
Điểm người dùng chủ động chọn trên bản đồ, không phải xác nhận họ đang ở đó.
_Avoid_: Vị trí GPS

**Theo dõi vị trí**:
Việc cập nhật vị trí mới khi người dùng bật theo dõi trong trang đang hiển thị.
_Avoid_: Lưu lịch sử

**Đồng ý lưu lịch sử**:
Lựa chọn riêng của người đã đăng nhập cho phép lưu các vị trí GPS mới đủ điều kiện.
_Avoid_: Quyền GPS, đăng nhập, bật theo dõi

**Lịch sử vị trí**:
Các vị trí GPS được lưu với sự đồng ý, thuộc một người dùng, tự hết hạn sau30 ngày.
_Avoid_: Lịch sử điểm chọn, dữ liệu ngập

**Vị trí cũ**:
Vị trí cuối đã quá30 giây không có lần xác định mới; không phải vị trí hiện tại đã xác nhận.
_Avoid_: Vị trí đang theo dõi chính xác

**Điểm ngập seed**:
Điểm tham chiếu trong dữ liệu demo để kiểm tra vùng bao phủ, chưa phải kết luận ngập hiện tại.
_Avoid_: Cảnh báo ngập thời gian thực
