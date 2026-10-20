# Feature Specification: Mưa, rủi ro ngập và tuyến đường tham khảo

**Feature Branch**: Nhánh hiện tại `main`; không có hook tạo nhánh.

**Created**: 2026-10-20

**Status**: Draft — đã có plan và tasks; quyết định riêng tư/notice v2 đã chốt, chưa triển khai.

**Input**: Mở rộng spec 001 cho người đi xe máy, ô tô và shipper trong vùng nội thành
TP.HCM, dùng ẩn danh: mưa, lịch sử ngập, rủi ro có giải thích, mô phỏng mưa,
tuyến tránh nguy cơ cao và bằng chứng ảnh có fallback nhập tay.
Đây là thông tin tham khảo, không phải cảnh báo chính thức và không bảo đảm an toàn.

## Clarifications

### Session 2026-10-20

- Q: Người dùng đồng ý xử lý tạm vị trí theo cách nào và thu hồi ra sao? → A: Chọn A. Đồng ý một lần trong phiên tab, chỉ giữ trong bộ nhớ; tải lại/đóng tab thì hết hiệu lực. Áp dụng cho GPS và điểm chọn tay, người ẩn danh và đã đăng nhập, độc lập với đồng ý lưu lịch sử. Chỉ gửi khi bấm thao tác yêu cầu, kể cả khi theo dõi GPS đang bật; cập nhật GPS không tự gửi. Thông báo nêu điểm được gửi và bên nhận; phần cam kết không lưu của nhà cung cấp được thay bằng quyết định bổ sung sau plan bên dưới. Có nút thu hồi: dừng gửi mới, huỷ yêu cầu đang chờ và bỏ kết quả trả muộn; dữ liệu đã gửi chỉ xử lý tạm. Ghi ngoại lệ tại spec 001 FR-008, SC-008, giữ nguyên cam kết lưu/xoá lịch sử.

- Q: Ai được bật mưa mô phỏng và bật/tắt bằng cách nào? → A: Chọn A. Bất kỳ người dùng nào mở mục “Demo mô phỏng”, chọn một kịch bản mưa định sẵn có tên rõ ràng và bấm bật; không nhập số liệu tự do. Chỉ thay đầu vào mưa; lịch sử ngập, vùng hỗ trợ và bản đồ giữ nguyên là dữ liệu thật. Mọi màn hình có dải thông báo cố định “Đang dùng mưa mô phỏng: <tên kịch bản>” kèm nút tắt; mọi kết quả bị ảnh hưởng có nhãn “MÔ PHỎNG”. Trạng thái chỉ trong bộ nhớ tab, tải lại/đóng tab trở về chế độ thật; máy chủ không lưu trạng thái mô phỏng và không áp dụng cho yêu cầu người dùng khác.

- Q: Khi hết thời gian, US6 có được hoãn và US5 có bắt buộc để nghiệm thu MVP không? → A: Chọn A. US1–US5 bắt buộc đạt; US6 được hoãn nếu hết thời gian, phải ghi “chưa làm” kèm lý do. Các tiêu chí riêng về ảnh không được báo đạt khi hoãn US6.

- Q: Có chốt dự báo 3 giờ, bán kính 1 km và bốn mức rủi ro cộng trạng thái không đủ dữ liệu không? → A: Chọn A. Dự báo theo giờ trong 3 giờ tới; lịch sử trong bán kính 1 km; bốn mức thấp, trung bình, cao, rất cao và trạng thái riêng không đủ dữ liệu. Các giá trị đặt ở cấu hình, công bố như một phần của bộ quy tắc có phiên bản; thay đổi giá trị phải đổi phiên bản quy tắc. Khoảng cách tính theo mét trên mặt đất; điểm đúng 1 km thuộc phạm vi.

- Q: “Tránh ngập” nên ưu tiên tuyến theo thứ tự nào? → A: Chọn A. Chỉ xếp hạng tuyến có thời gian không vượt ngưỡng so với tuyến nhanh nhất; ngưỡng công bố trong plan và thuộc bộ quy tắc có phiên bản. Trong nhóm này, ưu tiên tổng chiều dài đoạn cao/rất cao ít nhất, rồi rủi ro tổng thấp hơn, rồi chiều dài thiếu dữ liệu ít hơn, cuối cùng thời gian ngắn hơn. Thiếu dữ liệu không là nguy cơ bằng không. Không có tuyến ít nguy cơ trong ngưỡng thì hiển thị tuyến nhanh nhất kèm cảnh báo rõ. Mỗi tuyến hiển thị chiều dài cao/rất cao, chiều dài thiếu dữ liệu, rủi ro tổng và thời gian để giải thích; giữ mọi cảnh báo và lưu ý tham khảo.

**Quyết định bổ sung sau plan — 2026-10-20**: Người dùng chấp thuận sửa cam kết/notice để công bố dữ liệu gửi Open-Meteo và thời hạn log của họ. Cam kết không lưu/không log tọa độ áp dụng cho ứng dụng; Open-Meteo có thể giữ log yêu cầu chứa tọa độ làm tròn tối đa 90 ngày theo chính sách công bố. Thu hồi hoặc xoá lịch sử ứng dụng không xoá tức thì log nhà cung cấp. Quyết định này thay phần cam kết không lưu áp cho nhà cung cấp trong câu trả lời đầu, giữ nguyên cơ chế consent và cam kết lưu/xoá lịch sử spec 001. Notice mới là `temporary-processing-v2`, xem [privacy-notice.md](privacy-notice.md).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Xem mưa tại vị trí (Priority: P1)

Người dùng xem mưa hiện tại và dự báo vài giờ tới tại vị trí hoặc điểm đã chọn,
kèm nguồn, thời điểm và trạng thái dữ liệu cũ.

**Why this priority**: Cung cấp thông tin trực tiếp về mưa, là đầu vào cho rủi ro.

**Independent Test**: Dùng điểm cố định và nguồn mưa kiểm thử; không cần chức năng tuyến, ảnh hoặc rủi ro.

**Acceptance Scenarios**:

1. **Given** điểm trong vùng, nguồn sẵn sàng, **When** yêu cầu xem mưa,
   **Then** trong 3 giây thấy lượng mưa, đơn vị, dự báo theo giờ trong 3 giờ tới,
   nguồn và thời điểm dữ liệu theo bộ quy tắc có phiên bản.
2. **Given** nguồn lỗi, còn dữ liệu cũ, **When** xem mưa,
   **Then** thấy thông báo lỗi, nhãn dữ liệu cũ và thời điểm gốc.
3. **Given** nguồn lỗi và không có dữ liệu cũ, **When** xem mưa,
   **Then** thấy chưa có dữ liệu; không dùng mưa bằng không thay cho dữ liệu thiếu.

### User Story 2 - Xem lịch sử ngập quanh vị trí (Priority: P1)

Người dùng xem điểm ngập lịch sử trong một bán kính, kèm mức độ, thời gian ghi nhận,
nguồn và độ tin cậy của từng điểm.

**Why this priority**: Cung cấp bằng chứng địa phương có thể truy vết.

**Independent Test**: Dùng bộ dữ liệu nhóm cung cấp và điểm cố định; không cần mưa, tuyến hoặc ảnh.

**Acceptance Scenarios**:

1. **Given** các điểm trong/ngoài bán kính, **When** xem lịch sử,
   **Then** chỉ thấy điểm cách không quá 1.000 mét trên mặt đất, gồm điểm đúng 1.000 mét,
   cùng bán kính đang dùng và phiên bản quy tắc.
2. **Given** điểm chưa kiểm chứng, **When** xem chi tiết,
   **Then** có nhãn chưa kiểm chứng, mức độ, thời gian, nguồn và độ tin cậy;
   không trình bày như dữ liệu chính thức.
3. **Given** không có điểm trong bán kính, **When** xem lịch sử,
   **Then** báo không có dữ liệu trong phạm vi, không kết luận không có nguy cơ.

### User Story 3 - Điểm rủi ro có giải thích (Priority: P1)

Người dùng xem mức rủi ro và giải thích tiếng Việt: đóng góp của mưa hiện tại,
dự báo, lịch sử gần đó và bằng chứng hiện trường nếu có, cùng độ tin cậy.

**Why this priority**: Là giá trị cốt lõi và phải giải thích được theo constitution.

**Independent Test**: Dùng đầu vào đã biết, đối chiếu quy tắc công bố mà không cần nguồn trực tiếp, tuyến hoặc ảnh.

**Acceptance Scenarios**:

1. **Given** đủ dữ liệu, **When** đánh giá điểm, **Then** trong 3 giây thấy mức rủi ro,
   độ tin cậy, phân rã yếu tố, nguồn, thời điểm và phiên bản quy tắc.
2. **Given** thiếu/cũ một nguồn, **When** đánh giá, **Then** nêu rõ nguồn thiếu/cũ,
   giảm độ tin cậy theo quy tắc và không diễn giải thiếu dữ liệu là không có nguy cơ.
3. **Given** không đủ dữ liệu theo quy tắc, **When** đánh giá,
   **Then** báo không đủ dữ liệu để kết luận cùng lý do, không gán mức thấp mặc định.
4. **Given** cùng đầu vào và phiên bản quy tắc, **When** đánh giá lại,
   **Then** mức rủi ro và phân rã đóng góp không đổi.

### User Story 4 - Mưa mô phỏng cho demo (Priority: P1)

Bất kỳ người dùng nào mở mục “Demo mô phỏng”, chọn kịch bản mưa định sẵn có tên rõ
ràng và bấm bật để minh họa khi thời tiết thật không mưa; không cho nhập số liệu
tự do. Chỉ thay đầu vào mưa; lịch sử ngập, vùng hỗ trợ và bản đồ giữ nguyên là dữ
liệu thật. Người xem luôn phân biệt được mô phỏng với dữ liệu thật.

**Why this priority**: Giúp demo trực tiếp tái hiện được và trung thực về dữ liệu.

**Independent Test**: Kiểm tra bật/tắt, đầu vào và nhãn bằng kịch bản định trước;
không cần nguồn mưa thật hoặc tuyến đã triển khai.

**Acceptance Scenarios**:

1. **Given** chế độ thật và người dùng bất kỳ, **When** mở “Demo mô phỏng”, chọn
   kịch bản định sẵn và bấm bật, **Then** mọi màn hình có dải thông báo cố định
   “Đang dùng mưa mô phỏng: <tên kịch bản>” kèm nút tắt; không có nhập số liệu tự do.
2. **Given** mô phỏng đang bật, **When** xem mưa/rủi ro/tuyến,
   **Then** mọi kết quả bị ảnh hưởng có nhãn; từng nguồn thật/mô phỏng phân biệt được;
   không ghi mô phỏng vào lịch sử thật.
3. **Given** mô phỏng đang bật, **When** tắt, tải lại hoặc đóng tab,
   **Then** trở về chế độ thật, không dùng lại kết quả mô phỏng như dữ liệu thật;
   báo nguồn thật chưa sẵn sàng nếu cần.
4. **Given** hai phiên người dùng độc lập, **When** một phiên bật mô phỏng,
   **Then** chỉ đầu vào mưa của yêu cầu mô phỏng bị thay thế; lịch sử ngập, vùng hỗ
   trợ và bản đồ giữ nguyên; máy chủ không lưu trạng thái mô phỏng và không áp dụng
   mô phỏng cho yêu cầu của người dùng khác.

### User Story 5 - Tuyến đường tránh ngập (Priority: P2)

Người dùng chọn điểm đi/đến trong vùng, nhận một đến ba tuyến có rủi ro tổng,
đoạn nguy cơ cao và giải thích vì sao tuyến ít rủi ro hơn được ưu tiên.
Chỉ xếp hạng trong ngưỡng thời gian so với tuyến nhanh nhất theo quy tắc có phiên
bản; ưu tiên chiều dài cao/rất cao ít nhất, rồi rủi ro tổng, chiều dài thiếu dữ
liệu và cuối cùng thời gian, đều theo thứ tự tăng dần.

**Why this priority**: Chuyển đánh giá rủi ro thành hỗ trợ quyết định di chuyển, sau nhóm P1.

**Independent Test**: Dùng bộ tuyến và rủi ro đã biết, không cần thời tiết trực tiếp hoặc đánh giá ảnh.

**Acceptance Scenarios**:

1. **Given** có tuyến hợp lệ giữa hai điểm, **When** tìm tuyến,
   **Then** trong 5 giây thấy một đến ba tuyến cùng rủi ro tổng, đoạn nguy cơ cao,
   nguồn và lý do xếp hạng.
2. **Given** mọi tuyến có đoạn nguy cơ cao, **When** xem tuyến,
   **Then** thấy cảnh báo rõ, không tuyên bố tuyến nào an toàn tuyệt đối.
3. **Given** không có tuyến hoặc thành phần tìm đường lỗi, **When** tìm tuyến,
   **Then** thấy thông báo tương ứng; các chức năng độc lập còn sẵn sàng vẫn dùng được.
4. **Given** điểm đi/đến hoặc một đoạn tuyến ngoài vùng, **When** tìm/nhận tuyến,
   **Then** chỉ đề xuất tuyến nằm trọn vùng hoặc báo chưa có tuyến được hỗ trợ;
   không trình bày đoạn ngoài vùng như đã đánh giá đầy đủ.
5. **Given** các tuyến trong và vượt ngưỡng thời gian so với tuyến nhanh nhất,
   **When** xếp hạng, **Then** chỉ xếp hạng các tuyến trong ngưỡng theo thứ tự:
   chiều dài cao/rất cao, rủi ro tổng, chiều dài thiếu dữ liệu, thời gian;
   tiêu chí sau chỉ phân định khi các tiêu chí trước bằng nhau.
6. **Given** không có tuyến ít nguy cơ nào trong ngưỡng, **When** xem kết quả,
   **Then** thấy tuyến nhanh nhất kèm cảnh báo rõ không có phương án ít nguy cơ
   đáp ứng ngưỡng; không tuyên bố tuyến nhanh nhất an toàn.
7. **Given** tuyến có đoạn thiếu dữ liệu, **When** xem hoặc so sánh tuyến,
   **Then** thấy chiều dài cao/rất cao, chiều dài thiếu dữ liệu, rủi ro tổng và thời
   gian; thiếu dữ liệu không được coi là nguy cơ bằng không, và tuyến ít thiếu dữ
   liệu hơn đứng trước khi các tiêu chí rủi ro bằng nhau.

### User Story 6 - Bằng chứng ảnh hiện trường (Priority: P3)

Người dùng chụp/tải ảnh để nhận đánh giá dấu hiệu ngập và mô tả ngắn làm bằng chứng
bổ sung; lỗi hoặc quá hạn thì chuyển nhập tay mức ngập.

**Why this priority**: Bổ sung bằng chứng sau các luồng chính, không để AI chặn chúng.

**Điều kiện bàn giao**: Được hoãn US6 nếu hết thời gian; báo cáo phải ghi “chưa làm”
kèm lý do, không báo đạt các tiêu chí về ảnh. US1–US5 vẫn bắt buộc để nghiệm thu MVP.

**Independent Test**: Dùng ảnh kiểm thử và mô phỏng thành công/lỗi/quá hạn;
kiểm tra nhập tay mà không cần tìm đường.

**Acceptance Scenarios**:

1. **Given** ảnh xử lý được, **When** yêu cầu đánh giá, **Then** trong 15 giây thấy
   có/không có dấu hiệu ngập hoặc không xác định, cùng mô tả ngắn và nhãn bằng chứng bổ sung.
2. **Given** lỗi hoặc chưa có kết quả sau 15 giây, **When** chờ đánh giá,
   **Then** có nhập tay mức ngập và lý do chuyển chế độ; các chức năng khác không bị chặn.
3. **Given** bằng chứng ảnh hoặc nhập tay, **When** tính rủi ro,
   **Then** giải thích vai trò, nguồn và đóng góp của bằng chứng theo quy tắc công bố.
4. **Given** xử lý kết thúc, thất bại hoặc hết hạn, **When** kiểm tra lưu trữ,
   **Then** ảnh không được giữ lại sau xử lý theo mặc định.

### Edge Cases

- Ngoài vùng hiện “chưa hỗ trợ”; điểm đúng trên biên thuộc vùng theo spec 001.
- GPS cũ, bị từ chối hoặc mất tín hiệu: giữ hành vi spec 001, cho chọn điểm thủ công;
  kết quả gắn với điểm đang xem, không giả là vị trí hiện tại.
- Đổi điểm khi yêu cầu cũ chưa xong: không hiển thị kết quả cũ như kết quả của điểm mới.
- Chưa đồng ý, đã thu hồi hoặc tải lại tab: không gửi điểm cho xử lý tạm trước khi
  đồng ý lại. Theo dõi GPS chỉ cập nhật tại thiết bị, không tự phát sinh yêu cầu xử lý.
- Thu hồi khi yêu cầu đang chạy: dừng gửi mới, huỷ yêu cầu đang chờ và bỏ kết quả
  trả muộn; không tuyên bố thu hồi được dữ liệu đã truyền hoặc xoá ngay log Open-Meteo; log nhà cung cấp theo thời hạn đã công bố.
- Mất mạng: giữ phần bản đồ đã tải theo spec 001; báo nguồn không khả dụng;
  dữ liệu đã có chỉ dùng cùng thời điểm và nhãn cũ, không biến thành dữ liệu mới.
- Mưa bằng không khác thiếu dữ liệu; không có lịch sử khác không có nguy cơ.
- Bán kính lịch sử tính theo mét trên mặt đất: điểm đúng 1.000 mét được tính trong
  phạm vi, điểm xa hơn bị loại; không dùng khoảng cách theo độ tọa độ làm mét.
- Điểm lịch sử thiếu thuộc tính: hiện chưa rõ/chưa kiểm chứng, không tự bịa nguồn hoặc mức độ.
- Ảnh không đọc được/không đủ bằng chứng: báo rõ, cho nhập tay, không ép kết luận có/không ngập.
- Mô phỏng kết hợp bằng chứng thật: mỗi đầu vào giữ nhãn nguồn;
  kết quả phụ thuộc mưa mô phỏng vẫn mang nhãn “MÔ PHỎNG”.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Dùng ẩn danh cho mọi chức năng; chỉ trong vùng hỗ trợ có phiên bản của
  spec 001; điểm trên biên thuộc vùng, ngoài vùng hiện “chưa hỗ trợ”.
- **FR-002**: Hiển thị mưa hiện tại và dự báo theo giờ trong 3 giờ tới cho điểm đang xem, kèm đơn vị,
  nguồn, thời điểm dữ liệu; phân biệt thời điểm dữ liệu với thời điểm truy vấn (US1).
- **FR-003**: Nguồn mưa lỗi phải có thông báo; chỉ dùng dữ liệu cũ có nhãn cũ và
  thời điểm gốc; không có dữ liệu thì báo thiếu, không thay bằng mưa bằng không (US1).
- **FR-004**: Hiển thị điểm lịch sử trong bán kính 1 km quanh điểm đang xem,
  tính khoảng cách theo mét trên mặt đất và bao gồm điểm đúng 1.000 mét;
  gồm mức độ, thời gian, nguồn và độ tin cậy từng điểm (US2).
- **FR-005**: Gắn nhãn chưa kiểm chứng cho dữ liệu tương ứng, nêu thuộc tính thiếu;
  không trình bày dữ liệu nhóm cung cấp như xác nhận chính thức (US2).
- **FR-006**: Đánh giá rủi ro bằng quy tắc công bố; kết quả có mức rủi ro, độ tin cậy
  và giải thích tiếng Việt phân rã các yếu tố áp dụng hoặc bị thiếu. Có bốn mức
  thấp, trung bình, cao, rất cao; không đủ dữ liệu là trạng thái riêng, không phải mức thấp (US3).
- **FR-007**: Công bố công thức, trọng số, chuẩn hóa, ngưỡng phân loại, cách xử lý
  dữ liệu thiếu/cũ, vai trò bằng chứng và phiên bản quy tắc. Mỗi kết quả có đầu vào,
  nguồn, thời điểm, đóng góp để tái hiện mà không cần lưu lịch sử vị trí. Dự báo 3 giờ,
  bán kính 1 km, bốn mức rủi ro và trạng thái không đủ dữ liệu phải đặt ở cấu hình,
  công bố trong bộ quy tắc có phiên bản; thay đổi các giá trị phải đổi phiên bản quy tắc (US3).
- **FR-008**: Thiếu/cũ dữ liệu phải giảm độ tin cậy theo quy tắc và nêu lý do;
  không đủ dữ liệu thì báo chưa thể kết luận, không tạo mức chắc chắn giả (US3).
- **FR-009**: Bất kỳ người dùng nào được mở “Demo mô phỏng”, chọn kịch bản mưa định
  sẵn có tên rõ ràng và bấm bật; không cho nhập số liệu tự do. Khi bật, mọi màn hình
  phải có dải thông báo cố định “Đang dùng mưa mô phỏng: <tên kịch bản>” kèm nút tắt;
  mọi kết quả bị ảnh hưởng mang nhãn “MÔ PHỎNG”. Trạng thái chỉ ở bộ nhớ tab, tải lại
  hoặc đóng tab thì trở về chế độ thật; máy chủ không lưu trạng thái mô phỏng và
  không áp dụng mô phỏng cho yêu cầu của người dùng khác (US4).
- **FR-010**: Phân biệt nguồn thật/mô phỏng; không ghi mô phỏng vào lịch sử ngập thật
  hoặc lịch sử vị trí thật. Chỉ thay đầu vào mưa; lịch sử ngập, vùng hỗ trợ và bản đồ
  giữ nguyên là dữ liệu thật; tắt mô phỏng không dùng lại kết quả mô phỏng như thật (US4).
- **FR-011**: Cho chọn điểm đi/đến, trả một đến ba tuyến nằm trọn trong vùng nếu có;
  mỗi tuyến có rủi ro tổng, đoạn nguy cơ cao, nguồn và giải thích; hiển thị tổng
  chiều dài đi qua đoạn cao/rất cao, chiều dài thiếu dữ liệu và thời gian di chuyển (US5).
- **FR-012**: Chỉ xếp hạng tuyến có thời gian không vượt ngưỡng so với tuyến nhanh
  nhất; ngưỡng phải công bố trong plan, thuộc bộ quy tắc có phiên bản. Xếp tăng dần
  theo tổng chiều dài đoạn cao/rất cao, rồi rủi ro tổng, rồi chiều dài thiếu dữ liệu,
  cuối cùng thời gian; tiêu chí sau chỉ phân định khi các tiêu chí trước bằng nhau.
  Không có tuyến ít nguy cơ trong ngưỡng thì hiển thị tuyến nhanh nhất kèm cảnh báo
  rõ. Thiếu dữ liệu không được coi là nguy cơ bằng không. Giải thích dựa trên bốn
  số liệu hiển thị, nêu giới hạn dữ liệu và cảnh báo khi mọi tuyến có đoạn nguy cơ
  cao; giữ lưu ý tham khảo và không bảo đảm an toàn (US5).
- **FR-013**: Không có tuyến hoặc tìm đường lỗi phải báo rõ, không tạo tuyến giả;
  chức năng độc lập còn sẵn sàng vẫn dùng được (US5).
- **FR-014**: Cho chụp/tải ảnh, nhận đánh giá dấu hiệu ngập và mô tả ngắn;
  phân biệt không phát hiện dấu hiệu với không đủ bằng chứng (US6).
- **FR-015**: Xử lý ảnh lỗi/chậm phải có nhập tay mức ngập trong tối đa 15 giây
  từ thao tác gửi yêu cầu, kèm lý do và trạng thái suy giảm; không chặn luồng chính (US6).
- **FR-016**: Ảnh và bằng chứng nhập tay phục vụ đánh giá trong phiên; mặc định không giữ
  ảnh sau xử lý, không tự ghi vào lịch sử ngập hoặc báo cáo cộng đồng (US6).
- **FR-017**: Mọi kết quả rủi ro/tuyến có giải thích, nhãn nguồn, lưu ý tham khảo,
  không thay cảnh báo chính thức, không bảo đảm an toàn; không dùng lời khẳng định an toàn tuyệt đối.
- **FR-018**: Giữ cam kết riêng tư spec 001: không lưu vị trí mặc định, không tự bật lưu
  khi dùng tính năng mới; lịch sử chỉ cho người đăng nhập đã bật lưu, có xoá toàn bộ
  và hết hạn theo spec 001. Log/đo lường lỗi không chứa tọa độ hoặc ảnh.
- **FR-019**: Đồng ý xử lý tạm vị trí một lần trong phiên tab, chỉ giữ trạng thái trong
  bộ nhớ tab, không lưu bền vững; tải lại hoặc đóng tab thì hết hiệu lực. Áp dụng cho
  điểm GPS và điểm chọn tay, người ẩn danh và đã đăng nhập, độc lập với đồng ý lưu
  lịch sử spec 001. Thông báo trước khi đồng ý phải nêu rõ điểm được gửi, bên nhận
  là máy chủ ứng dụng và Open-Meteo. Ứng dụng xử lý tạm, không lưu vị trí chính xác
  vào lịch sử nếu chưa bật lưu và không ghi tọa độ vào log. Khi dùng thời tiết thật,
  máy chủ chỉ gửi tọa độ làm tròn theo ô 0,01 độ đến Open-Meteo, không gửi tọa độ
  chính xác, tài khoản/token/IP thiết bị. Notice phải nêu Open-Meteo có thể giữ log
  yêu cầu chứa tọa độ làm tròn tối đa 90 ngày theo chính sách được liên kết, không
  hứa nhà cung cấp không lưu. Cache thời tiết ô làm tròn chỉ ở RAM tối đa 60 phút,
  không gắn người dùng. Phải đồng ý notice `temporary-processing-v2` trước khi gửi;
  mô phỏng không gọi Open-Meteo.
  Chỉ gửi điểm khi người dùng bấm thao tác yêu cầu; ngay cả khi đang theo dõi GPS,
  vị trí mới không tự gửi. Khi điểm thay đổi, thao tác yêu cầu phải thể hiện rõ điểm
  sắp gửi; không cần hỏi lại đồng ý trong cùng phiên tab còn hiệu lực. Có nút thu hồi:
  dừng gửi mới, huỷ yêu cầu đang chờ và bỏ kết quả trả muộn; thu hồi hoặc xoá lịch sử
  ứng dụng không xoá tức thì log đã có tại Open-Meteo. Ảnh chỉ gửi khi người dùng
  chủ động yêu cầu xử lý, không giữ sau xử lý.
- **FR-020**: Lỗi mưa, ảnh hoặc tìm đường không ngừng các chức năng độc lập;
  thông báo dễ hiểu và thể hiện suy giảm, không thay lỗi bằng kết quả giả.

### Non-Functional Requirements

- **NFR-001**: Web di động, tiếng Việt trước; kế thừa màn hình từ 360px và thiết bị của spec 001.
- **NFR-002**: Mưa hiển thị trong 3 giây khi nguồn sẵn sàng, rủi ro có giải thích trong
  3 giây, tuyến trong 5 giây. Đo từ thao tác xác nhận điểm/yêu cầu đến khi kết quả
  tương ứng hiển thị đầy đủ; ảnh có kết quả hoặc nhập tay trong 15 giây.
- **NFR-003**: Quy tắc rủi ro tái hiện được; kiểm thử công thức, trọng số, ngưỡng,
  dữ liệu thiếu/cũ, chiều thay đổi hợp lý; có kiểm thử fallback ảnh và demo chính.
- **NFR-004**: Ghi lỗi, timeout, fallback, phiên bản quy tắc và mã đối chiếu, không có
  tọa độ/ảnh; quyết định kiến trúc quan trọng ghi lý do theo constitution.

### Key Entities *(include if feature involves data)*

- **Điểm đánh giá**: Điểm GPS/chọn tay trong vùng, dùng tạm; không mặc nhiên là lịch sử vị trí.
- **Thông tin mưa**: Lượng, đơn vị, thời đoạn hiện tại/dự báo, nguồn, thời điểm, trạng thái cũ/mô phỏng.
- **Điểm ngập lịch sử**: Vị trí, mức độ, thời gian, nguồn, độ tin cậy, trạng thái kiểm chứng.
- **Bộ quy tắc rủi ro**: Phiên bản, công thức, trọng số, chuẩn hóa, ngưỡng, xử lý thiếu/cũ
  và bằng chứng; cấu hình công bố gồm dự báo 3 giờ, bán kính 1 km tính theo mét trên
  mặt đất (kể cả điểm đúng biên), bốn mức và trạng thái không đủ dữ liệu.
- **Kết quả rủi ro**: Mức, độ tin cậy, phân rã, đầu vào/nguồn/thời điểm, phiên bản và nhãn mô phỏng.
- **Kịch bản mưa mô phỏng**: Kịch bản định sẵn có tên rõ ràng và đầu vào mưa;
  không có số liệu nhập tự do. Trạng thái bật/tắt chỉ trong bộ nhớ tab.
- **Tuyến tham khảo**: Điểm đi/đến, các đoạn, rủi ro tổng, tổng chiều dài cao/rất cao,
  chiều dài thiếu dữ liệu, thời gian, nguồn, phiên bản quy tắc và giải thích xếp hạng
  trong ngưỡng thời gian so với tuyến nhanh nhất.
- **Bằng chứng hiện trường**: Ảnh tạm hoặc mức nhập tay, nguồn, mô tả, trạng thái xác định và đóng góp.

## Success Criteria *(mandatory)*

### Measurable Outcomes

Nghiệm thu MVP bắt buộc US1–US5 và các tiêu chí tương ứng đạt. Nếu hoãn US6 vì hết
thời gian, SC-004 và phần kiểm thử riêng về ảnh trong SC-008, SC-011 phải ghi
“chưa làm” kèm lý do, không báo đạt; các phần còn lại vẫn bắt buộc. Hoãn US6 không
có nghĩa hoàn thành toàn bộ spec, và không miễn các bảo vệ/fallback nếu triển khai ảnh.

- **SC-001**: Mọi ca nghiệm thu nguồn mưa sẵn sàng hiển thị mưa đầy đủ đơn vị, nguồn,
  thời điểm trong 3 giây từ thao tác yêu cầu (FR-002; NFR-002).
- **SC-002**: Mọi ca nghiệm thu rủi ro hiển thị kết quả hoặc trạng thái không đủ dữ liệu
  trong 3 giây; 100% có phân rã yếu tố, nguồn, thời điểm, độ tin cậy và phiên bản (FR-006–FR-008).
- **SC-003**: Mọi cặp điểm nghiệm thu có tuyến được hỗ trợ trả một đến ba tuyến đầy đủ
  giải thích và đoạn nguy cơ cao trong 5 giây, thứ tự đúng quy tắc. Kiểm thử tuyến
  đúng/vượt ngưỡng thời gian, thứ tự chiều dài cao/rất cao → rủi ro tổng → chiều dài
  thiếu dữ liệu → thời gian, cùng trường hợp không có tuyến ít nguy cơ trong ngưỡng
  phải hiển thị tuyến nhanh nhất kèm cảnh báo. Mọi tuyến hiển thị đủ bốn số liệu;
  thiếu dữ liệu không bị coi là nguy cơ bằng không (FR-011, FR-012).
- **SC-004**: Mọi ca ảnh thành công/lỗi/quá hạn đều có đánh giá hoặc nhập tay trong
  15 giây; lỗi ảnh không chặn chức năng khác (FR-014–FR-016).
- **SC-005**: Hoàn tất trên Chrome Android và Safari iPhone luồng ẩn danh: chọn điểm,
  bật mưa mô phỏng, xem rủi ro có giải thích, chọn tuyến tránh nguy cơ cao;
  100% màn hình/kết quả bị ảnh hưởng có nhãn “MÔ PHỎNG” (US4, US5).
- **SC-006**: Bộ tình huống kỳ vọng được nhóm chốt trước nghiệm thu bao phủ mưa tăng/giảm,
  lịch sử gần/xa, ngưỡng, thiếu/cũ dữ liệu. 100% khớp quy tắc công bố; cùng đầu vào
  cho cùng kết quả; tăng yếu tố nguy cơ khi các yếu tố khác giữ nguyên không làm giảm
  rủi ro trái quy tắc. Kiểm tra cấu hình công bố chứa dự báo 3 giờ, bán kính 1 km,
  bốn mức và trạng thái không đủ dữ liệu; mỗi thay đổi giá trị phải có phiên bản
  quy tắc khác và kết quả truy vết đúng phiên bản (FR-007; NFR-003).
- **SC-007**: 100% điểm lịch sử kiểm thử được lọc đúng bán kính, có mức độ/thời gian/nguồn/
  độ tin cậy hoặc nhãn thiếu; mọi điểm chưa kiểm chứng có nhãn. Các ca cách dưới,
  đúng và trên 1.000 mét trên mặt đất phải lần lượt được tính trong, trong và ngoài
  phạm vi (FR-004, FR-005).
- **SC-008**: Mọi ca nguồn mưa lỗi có/không có dữ liệu cũ, ảnh quá hạn và tìm đường lỗi
  có thông báo đúng, không tạo kết quả giả và giữ chức năng độc lập còn sẵn sàng (FR-003, FR-013, FR-020).
- **SC-009**: Mọi ca bật/tắt mô phỏng không ghi dữ liệu mô phỏng vào lịch sử thật,
  không dùng lại kết quả mô phỏng như thật sau khi tắt. Người ẩn danh và đã đăng nhập
  đều bật được kịch bản định sẵn có tên, không có nhập số liệu tự do; mọi màn hình
  khi bật có dải thông báo cố định đúng tên và nút tắt, mọi kết quả bị ảnh hưởng có
  nhãn “MÔ PHỎNG”. Chỉ mưa thay đổi, lịch sử ngập/vùng hỗ trợ/bản đồ giữ nguyên.
  Tải lại/đóng tab trở về chế độ thật; không lưu trạng thái mô phỏng trên máy chủ
  và không ảnh hưởng yêu cầu người dùng khác (FR-009, FR-010).
- **SC-010**: Các ca điểm trong/ngoài/trên biên đúng hành vi spec 001;
  không đề xuất tuyến vượt vùng như đã đánh giá đầy đủ (FR-001, FR-011).
- **SC-011**: Kiểm tra luồng ẩn danh không tự lưu lịch sử hoặc gửi GPS nền;
  log không chứa tọa độ/ảnh; lưu trữ không giữ ảnh sau xử lý kết thúc, kể cả lỗi/quá hạn.
  Kiểm thử cả người ẩn danh/đã đăng nhập với GPS/điểm chọn tay: chỉ gửi sau đồng ý
  và bấm yêu cầu; notice v2 nêu đúng điểm, tọa độ làm tròn, bên nhận, log Open-Meteo
  tối đa 90 ngày và cache thời tiết RAM không gắn người dùng; kiểm tra ứng dụng
  không lưu vị trí chính xác hoặc ghi tọa độ vào log, không hứa xoá tức thì log
  nhà cung cấp khi thu hồi/xoá lịch sử. Bật theo
  dõi không tự gửi cập nhật. Trạng thái đồng ý chỉ ở bộ nhớ tab; tải lại/đóng tab
  phải đồng ý lại; thu hồi dừng gửi mới, huỷ yêu cầu đang chờ và bỏ kết quả muộn.
  Đồng ý xử lý tạm không bật lưu lịch sử (FR-016, FR-018, FR-019).
- **SC-012**: 100% kết quả rủi ro/tuyến nghiệm thu có lưu ý tham khảo, nguồn và giải thích;
  mọi tuyến nguy cơ cao có cảnh báo, không có cam kết an toàn tuyệt đối (FR-017).

## Assumptions

- Spec 001 là hợp đồng nền tảng, không đồng nghĩa đã triển khai xong. Hiện chỉ T003
  hoàn tất, T001 chờ dữ liệu, T002 chờ Docker. Nghiệm thu tích hợp cần phần nền tảng
  tương ứng hoạt động; không sửa hoặc đánh dấu task spec 001 trong bước này.
- Giữ đa giác, phiên bản và quy tắc trên biên spec 001. Nhóm cung cấp dữ liệu ngập
  có nguồn và độ tin cậy; không tự bịa dữ liệu thật. Dữ liệu thử/mô phỏng phải có nhãn.
- Đã chốt dự báo theo giờ trong ba giờ tới, bán kính lịch sử 1 km, tính theo mét
  trên mặt đất và bao gồm điểm đúng 1.000 mét; không phải quyết định nhà cung cấp.
- Đã chốt bốn mức: thấp, trung bình, cao, rất cao; không đủ dữ liệu là trạng thái riêng.
  Các giá trị này đặt ở cấu hình và công bố trong bộ quy tắc có phiên bản;
  thay đổi giá trị phải đổi phiên bản quy tắc.
  Công thức/trọng số/ngưỡng phải công bố trước nghiệm thu; không tự tuyên bố độ chính xác thực địa.
- Thời tiết từ dịch vụ công khai. Nguồn, phạm vi bao phủ, điều kiện sử dụng, độ mới
  chấp nhận được và quy tắc dùng dữ liệu cũ cần chốt ở plan.
- Thứ tự xếp hạng tuyến đã chốt tại FR-012. Ngưỡng thời gian so với tuyến nhanh nhất,
  cách tính rủi ro tổng và tiêu chí xác định tuyến ít nguy cơ phải được cụ thể hoá
  trong plan bằng bộ quy tắc có phiên bản và tình huống kỳ vọng; không tự gán ngưỡng
  tại bước clarify. Ảnh hưởng bằng chứng cũng cần chốt trong plan;
  thiếu dữ liệu không được coi là nguy cơ bằng không.
- Mô phỏng dùng kịch bản mưa định sẵn, chỉ trong bộ nhớ tab, tải lại/đóng tab trở về
  chế độ thật; chỉ thay mưa, không đổi lịch sử ngập, vùng hỗ trợ hoặc bản đồ.
  Máy chủ không lưu trạng thái mô phỏng, không áp dụng cho yêu cầu người dùng khác.
  Ảnh/nhập tay không tạo kho báo cáo cộng đồng.
- Đo hiệu năng với bộ điểm/tuyến/ảnh nghiệm thu và điều kiện mạng di động được ghi nhận;
  bắt đầu sau thao tác yêu cầu có đủ quyền xử lý, không tính thời gian người dùng cấp quyền/chọn ảnh.
- P1 gồm US1–US4, P2 là US5, P3 là US6; cả sáu thuộc scope. US1–US5 bắt buộc đạt
  để nghiệm thu MVP, kể cả demo tuyến SC-005. US6 có thể hoãn nếu hết thời gian,
  phải ghi “chưa làm” kèm lý do và không báo đạt tiêu chí ảnh; chỉ xong P1 chưa đạt MVP.
- Tuân thủ constitution II về giải thích, IV về fallback AI, I về demo, V về kiểm thử,
  VI về logging/quyết định có lý do. Công nghệ và kiến trúc dành cho bước plan.

### Ngoài phạm vi

Thanh toán, thông báo đẩy, báo cáo ngập cộng đồng, ứng dụng native, mở rộng toàn TP.HCM,
cảnh báo chính thức thay cơ quan chức năng và bản đồ offline.
Không tạo plan, tasks hoặc triển khai trong bước specify này.
