<!--
Sync Impact Report (temporary; remove before committing)
Version: unratified scaffold -> 1.0.0 (initial adoption).
Modified principles: all template slots replaced with six project principles below.
Added sections: Ràng buộc vận hành; Quy trình và cổng chất lượng; Governance rules.
Removed: template examples and placeholders only.
Dependent templates: unchanged; read constitution at runtime.
Deferred TODOs: none.
-->
# Cảnh báo ngập lụt và dẫn đường an toàn Constitution

## Core Principles

### I. MVP chạy được và demo trực tiếp được

Ưu tiên cao nhất là MVP chạy được từ đầu đến cuối và trình diễn trực tiếp được.
Đặc tả phải xác định luồng demo chính và tiêu chí nghiệm thu quan sát được.
Chỉ thêm tính năng phụ sau khi luồng này hoạt động và vượt qua kiểm thử bắt buộc.
Mỗi hạng mục phải đóng góp rõ ràng cho MVP hoặc được đưa vào phạm vi sau MVP.
Mục đích là chứng minh giá trị sản phẩm trong thời gian hackathon.

### II. Mọi điểm rủi ro ngập phải giải thích được

Mỗi điểm rủi ro ngập phải truy vết được tới công thức, trọng số, dữ liệu đầu vào
và nguồn dữ liệu. Phải ghi phiên bản quy tắc, thời điểm dữ liệu, cách chuẩn hóa,
ngưỡng phân loại và xử lý dữ liệu thiếu để tái hiện kết quả.
Dữ liệu mô phỏng cho demo phải được nhận diện rõ. Nếu AI tham gia đánh giá,
phải ghi vai trò và ảnh hưởng của AI; kết quả vẫn phải giải thích được.

### III. Hạ tầng chỉ thêm khi có lý do bảo vệ được

Mặc định chọn cách triển khai đơn giản nhất đáp ứng MVP. Mọi queue, cache hoặc
service bổ sung phải có lý do ghi trước khi triển khai: vấn đề cụ thể, bằng chứng
hoặc ràng buộc, phương án đơn giản đã cân nhắc, chi phí và điều kiện đánh giá lại.
Nhóm phải giải thích được lựa chọn trước giám khảo. AI Worker tách riêng là
ranh giới bắt buộc theo nguyên tắc IV; hạ tầng hỗ trợ nó vẫn phải qua đánh giá này.

### IV. Modular Monolith, Vertical Slice và AI Worker có fallback

Ứng dụng lõi phải dùng Modular Monolith, có ranh giới module rõ ràng và tổ chức
chức năng theo Vertical Slice. Giao tiếp qua hợp đồng; không truy cập trực tiếp
chi tiết nội bộ của module khác. AI Worker phải chạy thành tiến trình riêng,
có hợp đồng đầu vào và đầu ra rõ ràng.
Mọi lời gọi worker phải có timeout hữu hạn và fallback xác định trong plan.
Khi worker lỗi, không khả dụng hoặc chậm quá hạn, API lõi và luồng demo phải
hoàn tất bằng fallback. Phản hồi phải thể hiện chế độ suy giảm và lý do.
Fallback phải kiểm thử được và giữ khả năng giải thích kết quả rủi ro.

### V. Kiểm thử Risk Engine, API lõi và luồng demo

Risk Engine và API lõi phải có kiểm thử tự động: xUnit cho thành phần .NET,
pytest cho thành phần Python tương ứng. Risk Engine phải kiểm thử công thức,
trọng số, giá trị biên, dữ liệu thiếu và tính tái hiện kết quả.
API lõi phải kiểm thử hợp đồng, kiểm tra đầu vào và phản hồi lỗi.
Phải kiểm thử fallback khi AI Worker lỗi hoặc chậm.
Luồng demo chính phải có kiểm thử Playwright từ đầu đến cuối.
Kiểm thử liên quan phải chạy đạt trước khi thay đổi được coi là hoàn thành;
báo cáo phải nêu lệnh chạy và kết quả thực tế.

### VI. Logging, xử lý lỗi nhất quán và quyết định có lý do

Phía .NET phải dùng Serilog cho structured logging. AI Worker phải ghi log có
cấu trúc và dùng correlation ID để đối chiếu với ứng dụng lõi.
Phải ghi lỗi, timeout, việc dùng fallback và phiên bản quy tắc tính rủi ro.
API lõi phải có hợp đồng lỗi thống nhất gồm mã lỗi, thông điệp và correlation ID;
chi tiết chẩn đoán phải nằm trong log thay vì phản hồi cho người dùng.
Mọi quyết định kiến trúc phải ghi bối cảnh, lựa chọn, lý do và hệ quả trong plan
hoặc nhật ký quyết định. Đánh đổi quan trọng phải có ADR.

## Ràng buộc vận hành

- Plan phải xác định nguồn dữ liệu, độ mới dữ liệu và cơ chế truy vết kết quả rủi ro.
- Plan phải đặt timeout cụ thể, mô tả fallback và cách thể hiện trạng thái suy giảm.
- Hướng dẫn demo phải nêu điều kiện khởi chạy, dữ liệu và cách tái hiện luồng chính.
- Dẫn đường phải thể hiện cơ sở đánh giá rủi ro và giới hạn dữ liệu;
  không khẳng định an toàn tuyệt đối khi dữ liệu thiếu hoặc đã cũ.

## Quy trình và cổng chất lượng

1. Đặc tả xác định phạm vi MVP, luồng demo và tiêu chí nghiệm thu trước khi lập plan.
2. Plan kiểm tra từng nguyên tắc; ghi ranh giới module, hợp đồng worker, cách tính
   rủi ro, chiến lược kiểm thử và lý do cho hạ tầng được đề xuất.
3. Tasks phải bao gồm kiểm thử, logging, fallback và tài liệu quyết định cần thiết.
4. Trước khi hoàn thành, chạy kiểm thử liên quan và trình diễn luồng chính;
   ghi kết quả thực tế và giới hạn chưa xác minh.
5. Review phải kiểm tra khả năng giải thích rủi ro, fallback khi AI lỗi hoặc chậm,
   mức độ cần thiết của hạ tầng và tính nhất quán với constitution.

## Governance

Constitution là chuẩn quản trị cho đặc tả, plan, tasks và review của dự án.
Khi tài liệu triển khai mâu thuẫn, phải sửa tài liệu hoặc đề xuất sửa constitution
trước khi triển khai phần mâu thuẫn.

Sửa đổi phải ghi lý do, nguyên tắc bị ảnh hưởng, tác động và phương án chuyển đổi
nếu cần; người phụ trách dự án phải chấp thuận. Áp dụng semantic versioning:
MAJOR khi loại bỏ hoặc định nghĩa lại nguyên tắc gây mất tương thích; MINOR khi
thêm nguyên tắc hoặc mở rộng nghĩa vụ; PATCH khi làm rõ mà không đổi nghĩa vụ.
Cập nhật ngày sửa đổi khi nội dung thay đổi.

Mỗi plan và review phải ghi kết quả kiểm tra tuân thủ. Ngoại lệ phải ghi phạm vi,
lý do, người chấp thuận và điều kiện kết thúc. Ngoại lệ thay đổi nguyên tắc bắt buộc
phải qua quy trình sửa constitution.

**Version**: 1.0.0 | **Ratified**: 2026-10-19 | **Last Amended**: 2026-10-19
