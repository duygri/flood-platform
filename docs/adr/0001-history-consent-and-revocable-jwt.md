# Consent, hard delete và JWT có thể thu hồi

Dùng Identity chuẩn để giữ đường nâng cấp đăng ký, nhưng phát JWT ngắn hạn trong RAM
cho hai domain Vercel/Azure thay cookie cross-site dễ bị chặn. Vì logout phải vô hiệu
phiên cũ, lưu sessions trong PostgreSQL và kiểm mỗi request; không dùng JWT stateless.
Đánh đổi là một lần đọc DB và đăng nhập lại sau reload, chấp nhận cho demo ít người.

History thuộc người dùng, không thuộc phiên. Consent và write_generation có hàng khoá
chung cho record/off/delete/logout để request đến muộn không tái tạo dữ liệu đã xoá.
Giữ Identity schema chuẩn, thêm bảng app riêng; không thêm distributed lock/cache.
Hangfire cùng API/DB xoá thật mỗi phút; đọc loại hết hạn ngay30 ngày. Đây không phải
cam kết xoá vật lý tức thì khi DB down hoặc xoá mọi backup. Sau restore MVP scrub
mọi history/reset consent/revoke sessions trước mở truy cập, chấp nhận mất lịch sử.

**Status**: Thiết kế được đề xuất trong plan ngày2026-10-19, chưa triển khai hoặc kiểm thử.
