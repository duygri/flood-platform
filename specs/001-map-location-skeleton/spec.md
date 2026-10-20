# Feature Specification: Nền tảng bản đồ và vị trí TP.HCM

**Feature Branch**: Không tạo nhánh; dự án chưa có Git và không có hook tạo nhánh.

**Created**: 2026-10-19

**Status**: Draft — đã có plan và tasks

**Input**: Walking skeleton cho ứng dụng cảnh báo ngập và dẫn đường an toàn tại TP.HCM:
web di động, bản đồ, vị trí và độ chính xác, theo dõi bật/tắt, chọn điểm thủ công,
sử dụng ẩn danh, đồng ý trước khi lưu vị trí, HTTPS công khai và kiểm tra vận hành.

## Clarifications

### Session 2026-10-19

- Quyết định đã chốt: dùng ẩn danh ngay; đăng nhập email + mật khẩu là tuỳ chọn để lưu lịch sử; mặc định không lưu, tự xoá sau 30 ngày; chỉ hỗ trợ TP.HCM; mobile web, tiếng Việt trước; không mở rộng sang thời tiết, rủi ro, tìm đường hoặc AI ảnh.
- Q: Khi bật lưu lịch sử, lưu dữ liệu nào và xoá bằng cách nào? → A: Chọn A. Chỉ tài khoản đã đăng nhập và bật lưu mới lưu tọa độ GPS, độ chính xác, thời điểm; không lưu điểm thủ công. Có “Xoá toàn bộ lịch sử”; tắt lưu chỉ dừng ghi mới. Mỗi bản ghi tự xoá sau 30 ngày kể từ lúc ghi nhận.
- Q: Khi mất GPS hoặc mất mạng, tiếp tục theo dõi và lưu lịch sử thế nào? → A: Chọn A. Mất GPS giữ điểm cuối, đánh dấu cũ sau 30 giây, không ghi vị trí cũ thành điểm mới. Mất mạng giữ phần bản đồ đã tải, GPS tiếp tục cập nhật nếu có; không giữ điểm để gửi bù. Phục hồi tiếp tục nếu theo dõi và lưu vẫn bật. Marker đổi màu và banner thông báo khi vị trí cũ hoặc mất mạng; ghi log mất GPS, mất mạng và phục hồi, không kèm tọa độ. Gửi bù điểm ngoài phạm vi MVP.

- Q: Trong MVP, người dùng tự tạo tài khoản hay dùng tài khoản demo có sẵn? → A: Chọn A. Chỉ đăng nhập/đăng xuất tài khoản demo; seed từ biến môi trường hoặc secret, không hardcode. Dùng ASP.NET Core Identity với bảng người dùng chuẩn để thêm tự đăng ký mà không đổi thiết kế dữ liệu. Tự đăng ký nằm trong roadmap sau MVP; chưa có xác minh email hoặc quên mật khẩu.

- Q: Vùng TP.HCM được hỗ trợ trong MVP dùng ranh giới nào? → A: Chọn A. Đa giác demo nội thành phải bao trọn mọi điểm ngập seed và tuyến demo; lưu GeoJSON có phiên bản trong repo, nạp vào PostGIS khi khởi tạo. Ngoài vùng không lưu lịch sử và hiện “chưa hỗ trợ”. Kiểm thử điểm trong, ngoài và sát biên. Mở rộng toàn TP.HCM sau thi.

- Q: Điểm đúng trên biên đa giác được coi là trong hay ngoài vùng? → A: Chọn A. Điểm trên biên thuộc vùng; xét tọa độ điểm, không mở rộng theo vòng sai số GPS. Triển khai dùng ST_Covers hoặc tương đương, không dùng ST_Contains để xét vùng vì loại điểm trên biên. Kiểm thử điểm trên biên bắt buộc. GPS dao động sát biên có thể làm trạng thái nhảy; làm mượt sau MVP.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Mở bản đồ TP.HCM (Priority: P1)

Người đi xe máy, ô tô và shipper mở ứng dụng trên điện thoại và thấy ngay bản đồ
khu vực TP.HCM để xác nhận ứng dụng hoạt động.

**Why this priority**: Đây là nền tảng của mọi luồng demo và tính năng về sau.

**Independent Test**: Mở địa chỉ công khai khi chưa cấp quyền vị trí; bản đồ vẫn dùng được.

**Acceptance Scenarios**:

1. **Given** điện thoại có kết nối 4G theo điều kiện đo trong Assumptions,
   **When** mở địa chỉ HTTPS công khai, **Then** bản đồ TP.HCM dùng được trong 5 giây.
2. **Given** màn hình rộng 360px, **When** kéo và thu phóng bản đồ,
   **Then** thao tác được bằng cảm ứng, nút chính nhìn thấy và không có cuộn ngang trang.
3. **Given** không đăng nhập, **When** mở ứng dụng, **Then** dùng bản đồ ngay ở chế độ ẩn danh.

### User Story 2 - Xem vị trí và độ chính xác (Priority: P1)

Người dùng chủ động cho phép lấy vị trí để thấy vị trí hiện tại và độ chính xác.

**Why this priority**: Đây là giá trị cốt lõi của walking skeleton và demo trực tiếp.

**Independent Test**: Cấp một vị trí có độ chính xác đã biết; xác nhận marker,
vòng độ chính xác và số mét hiển thị tương ứng mà không cần bật theo dõi liên tục.

**Acceptance Scenarios**:

1. **Given** bản đồ đang mở, **When** người dùng chọn lấy vị trí và cấp quyền,
   **Then** bản đồ đưa vị trí vào vùng nhìn thấy, có marker, vòng độ chính xác và số mét.
2. **Given** bản đồ đã mở, quyền đã cấp và thiết bị trả vị trí trong 5 giây,
   **When** người dùng bấm lấy vị trí, **Then** marker và độ chính xác xuất hiện
   trong 10 giây kể từ thao tác bấm; không tính thời gian trả lời hộp thoại quyền.
3. **Given** vị trí có sai số lớn, **When** hiển thị, **Then** độ chính xác thực tế
   được thể hiện, không hiển thị vị trí đó như một điểm chắc chắn tuyệt đối.

### User Story 3 - Bật và tắt theo dõi (Priority: P1)

Người dùng bật theo dõi khi di chuyển và có thể dừng bất cứ lúc nào.

**Why this priority**: Chứng minh nền tảng theo dõi và quyền kiểm soát riêng tư.

**Independent Test**: Cấp chuỗi vị trí, bật/tắt theo dõi; kiểm tra marker và trạng thái.

**Acceptance Scenarios**:

1. **Given** quyền vị trí đã cấp, **When** bật theo dõi và nhận vị trí mới,
   **Then** marker, độ chính xác và thời điểm cập nhật thay đổi theo vị trí mới.
2. **Given** đang theo dõi, **When** tắt, **Then** trạng thái chuyển sang đã tắt,
   ngừng yêu cầu vị trí liên tục và không áp dụng các cập nhật đến muộn.
3. **Given** đã tắt, **When** bật lại, **Then** chỉ bắt đầu lại theo yêu cầu người dùng.
4. **Given** theo dõi đang bật ở trang hiển thị, **When** đi bộ 30–50m ngoài trời,
   **Then** marker cập nhật theo vị trí được thiết bị cung cấp, trong sai số được báo.

### User Story 4 - Không có GPS vẫn dùng được (Priority: P1)

Người dùng nhận hướng dẫn rõ ràng khi từ chối quyền hoặc không lấy được vị trí,
và chọn điểm thủ công để tiếp tục dùng bản đồ.

**Why this priority**: Luồng demo phải tiếp tục được khi vị trí không khả dụng.

**Independent Test**: Từ chối quyền hoặc mô phỏng không có tín hiệu, rồi chọn điểm.

**Acceptance Scenarios**:

1. **Given** yêu cầu vị trí, **When** quyền bị từ chối, **Then** hiện lý do,
   hướng dẫn cấp lại quyền và thao tác chọn điểm; không tự lặp hộp thoại xin quyền.
2. **Given** chưa có vị trí, **When** quá 10 giây sau khi cấp quyền mà chưa nhận được,
   **Then** hiện trạng thái không lấy được vị trí, nút thử lại và chọn điểm thủ công.
3. **Given** chế độ chọn thủ công, **When** chạm bản đồ, **Then** đánh dấu điểm,
   ghi rõ là điểm chọn thủ công và không gán vòng độ chính xác GPS cho điểm này.
4. **Given** đang theo dõi, **When** chọn điểm thủ công, **Then** tắt theo dõi;
   điểm thủ công không bị ghi đè cho tới khi người dùng chủ động lấy vị trí lại.

### User Story 5 - Ẩn danh và lịch sử có đồng ý (Priority: P1)

Người dùng dùng bản đồ ngay ở chế độ ẩn danh; đăng nhập email + mật khẩu là
lựa chọn để lưu và quản lý lịch sử vị trí của chính mình. MVP chỉ có đăng nhập/
đăng xuất bằng tài khoản demo cấp sẵn, chưa có tự đăng ký.

**Why this priority**: Giữ luồng truy cập nhanh và quyền kiểm soát dữ liệu nhạy cảm.

**Independent Test**: Dùng ẩn danh; sau đó đăng nhập, bật lưu, ghi vị trí và xoá lịch sử.

**Acceptance Scenarios**:

1. **Given** chưa đăng nhập hoặc chưa bật lưu, **When** dùng GPS,
   **Then** không gửi tọa độ GPS lên máy chủ ứng dụng hoặc lưu bền vững.
2. **Given** đã đăng nhập, **When** bật rõ ràng việc lưu,
   **Then** chỉ lưu tọa độ GPS, độ chính xác và thời điểm gắn với tài khoản;
   điểm thủ công không được lưu. Quyền GPS không thay cho đồng ý lưu.
3. **Given** đang lưu, **When** tắt lưu, **Then** dừng ghi mới, giữ bản ghi cũ
   tới khi người dùng xoá hoặc hết hạn; bản ghi đến muộn sau khi tắt không được ghi.
4. **Given** có lịch sử, **When** chọn “Xoá toàn bộ lịch sử” và thao tác thành công,
   **Then** mọi bản ghi hiện có của tài khoản không còn xem hoặc truy xuất được;
   nếu thất bại phải báo rõ, không báo xoá thành công.
5. **Given** bản ghi đã đủ 30 ngày kể từ thời điểm ghi nhận,
   **When** truy cập lịch sử qua giao diện hoặc API, **Then** bản ghi không còn
   truy xuất được ngay tại mốc hết hạn. Xoá vật lý do tác vụ nền thực hiện, mục tiêu
   trong 1 phút khi hệ thống hoạt động bình thường; có thể chậm hơn khi hệ thống lỗi
   và được cảnh báo khi trễ quá 5 phút. Bản sao lưu của nhà cung cấp không bị xoá tức thì.
6. **Given** hai tài khoản khác nhau, **When** xem hoặc xoá lịch sử,
   **Then** mỗi người chỉ truy cập và xoá được dữ liệu của chính mình.
7. **Given** tải lại trang, **When** mở bản đồ, **Then** theo dõi mặc định tắt;
   lịch sử đã lưu không được trình bày như vị trí GPS hiện tại.
8. **Given** tài khoản demo đã được cấp, **When** đăng nhập đúng email và mật khẩu,
   **Then** truy cập được lịch sử riêng; thông tin sai nhận lỗi dễ hiểu và không có phiên đăng nhập.
9. **Given** đã đăng nhập, bật lưu và kết nối được máy chủ, **When** đăng xuất,
   **Then** phiên bị thu hồi, dừng lưu và phiên cũ không còn truy cập được lịch sử;
   bản đồ ẩn danh vẫn dùng được.
10. **Given** đã đăng nhập nhưng không kết nối được máy chủ, **When** đăng xuất,
    **Then** thiết bị xoá phiên cục bộ, dừng lưu và hiển thị thông báo phiên trên
    máy chủ sẽ hết hạn trong tối đa 15 phút; không báo đã thu hồi phiên máy chủ.
    Đây là giới hạn đã biết; bản đồ ẩn danh vẫn dùng được trong giới hạn dữ liệu đã tải.
    Truy vết kiểm thử và cài đặt: T027, T037.

### User Story 6 - Kiểm tra hệ thống và tra cứu lỗi (Priority: P1)

Người vận hành kiểm tra được trạng thái và tìm được lỗi khi demo gặp sự cố.

**Why this priority**: Có thể chẩn đoán sự cố trực tiếp thay vì chỉ thấy trang không hoạt động.

**Independent Test**: Mở trang trạng thái; tạo lỗi có kiểm soát và tra cứu bằng mã đối chiếu.

**Acceptance Scenarios**:

1. **Given** hệ thống hoạt động, **When** mở trang kiểm tra, **Then** thấy trạng thái
   hoạt động và thời điểm kiểm tra; khi thành phần bắt buộc lỗi phải báo chưa sẵn sàng.
2. **Given** lỗi ứng dụng có thể xử lý, **When** thao tác thất bại, **Then** người dùng
   thấy thông báo dễ hiểu và hướng khắc phục; người vận hành tìm thấy sự kiện tương ứng.
3. **Given** trang trạng thái công khai, **When** truy cập, **Then** không thấy
   thông tin bí mật hoặc chi tiết chẩn đoán nội bộ.

### Edge Cases

- Quyền bị thu hồi lúc theo dõi: dừng cập nhật, báo lỗi và cho chọn thủ công.
- Thiết bị không hỗ trợ vị trí: bản đồ và chọn điểm thủ công vẫn hoạt động.
- Mất GPS: giữ marker cuối và thời điểm; sau 30 giây không có vị trí mới, đổi màu marker và hiện banner vị trí cũ. Không lưu điểm cuối thành bản ghi mới; khi GPS phục hồi, tiếp tục nếu theo dõi vẫn bật.
- Khi trang quay lại từ nền: thể hiện tuổi dữ liệu; chỉ tiếp tục nếu theo dõi vẫn bật
  và quyền còn hiệu lực. Theo dõi khi đóng trang hoặc chạy nền không được bảo đảm.
- Mất mạng: giữ phần bản đồ đã tải, đổi màu marker nếu đang có marker và hiện banner mất mạng; GPS tiếp tục cập nhật nếu có. Không lưu hàng đợi hoặc gửi bù điểm của khoảng mất mạng. Nếu chưa tải được bản đồ, báo lỗi và cho thử lại. Khi phục hồi, chỉ lưu các vị trí mới nếu theo dõi và lưu vẫn bật; trạng thái cũ vẫn giữ nếu GPS chưa phục hồi.
- Vị trí GPS hoặc điểm chọn ngoài vùng TP.HCM: hiện “chưa hỗ trợ”, không lưu điểm ngoài vùng; cho chọn lại điểm trong vùng.
- Tọa độ không hợp lệ hoặc độ chính xác không hợp lệ: không thay marker hợp lệ;
  báo trạng thái dữ liệu không hợp lệ và cho thử lại/chọn thủ công.
- Rủi ro đã biết: GPS dao động sát biên có thể làm trạng thái trong/ngoài nhảy
  qua lại; MVP đánh giá từng tọa độ đúng quy tắc, chỉ ghi điểm trong hoặc trên biên.
  Làm mượt trạng thái được hoãn sau MVP, không có khoảng đệm ngoài vùng.
- Bật/tắt nhanh hoặc cập nhật đến muộn: trạng thái tắt và điểm thủ công được ưu tiên.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hiển thị bản đồ tương tác với trung tâm mặc định tại TP.HCM, hỗ trợ
  kéo và thu phóng bằng cảm ứng (US1).
- **FR-002**: Cung cấp thao tác chủ động yêu cầu vị trí; hiển thị marker, vòng và
  giá trị độ chính xác theo mét cùng thời điểm cập nhật (US2).
- **FR-003**: Cung cấp bật/tắt theo dõi, mặc định tắt khi mở mới hoặc tải lại;
  chỉ cập nhật liên tục khi người dùng bật và quyền còn hiệu lực (US3).
- **FR-004**: Dừng nhận vị trí liên tục khi tắt và bỏ qua cập nhật đến muộn (US3).
- **FR-005**: Phân biệt lỗi quyền, không hỗ trợ, hết thời gian chờ và mất tín hiệu;
  hiển thị thông báo tiếng Việt, hướng dẫn, thử lại khi phù hợp và chọn thủ công (US4).
- **FR-006**: Cho chọn và thay điểm thủ công; phân biệt điểm này với GPS,
  dừng theo dõi khi chọn và chỉ quay lại GPS theo thao tác người dùng (US4).
- **FR-007**: Dùng ẩn danh ngay; cho đăng nhập email + mật khẩu để sử dụng lịch sử.
  Chỉ có tài khoản demo cấp sẵn và thao tác đăng nhập/đăng xuất trong MVP.
  Đăng nhập không tự bật lưu. Khi kết nối được máy chủ, đăng xuất thu hồi phiên,
  dừng lưu và phiên cũ không còn truy cập được lịch sử. Khi không kết nối được máy chủ,
  thiết bị xoá phiên cục bộ, dừng lưu và thông báo phiên máy chủ sẽ hết hạn trong
  tối đa 15 phút; đây là giới hạn đã biết, không được báo đã thu hồi phiên máy chủ.
  Lịch sử chỉ được xem và xoá bởi chủ tài khoản (US5; truy vết T027, T037).
- **FR-008**: Mặc định chỉ xử lý GPS trong bộ nhớ phiên. Chỉ khi đăng nhập và bật lưu
  rõ ràng mới gửi/lưu tọa độ GPS, độ chính xác, thời điểm gắn với tài khoản.
  Không lưu điểm thủ công hoặc tọa độ trong log/đo lường lỗi. Đủ 30 ngày kể từ lúc
  ghi nhận, bản ghi không còn truy xuất được qua giao diện hoặc API ngay tại mốc hết hạn.
  Tác vụ nền xoá vật lý với mục tiêu trong 1 phút khi hệ thống hoạt động bình thường;
  có thể chậm hơn khi hệ thống lỗi, cảnh báo khi trễ quá 5 phút. Bản sao lưu của nhà
  cung cấp không bị xoá tức thì. Có “Xoá toàn bộ lịch sử”; tắt lưu dừng ghi mới,
  không xoá bản ghi cũ. Không ghi cập nhật đến muộn sau khi tắt lưu (US5).
- **FR-009**: Cung cấp địa chỉ HTTPS công khai mở được trên điện thoại (US1, US2).
- **FR-010**: Có trang kiểm tra trạng thái và thời điểm kiểm tra, phân biệt hoạt động
  với chưa sẵn sàng khi phụ thuộc bắt buộc lỗi; không lộ bí mật (US6).
- **FR-011**: Lỗi ứng dụng phải có log cơ bản, mã đối chiếu khi áp dụng và thông báo
  dễ hiểu; lỗi vị trí phải được ghi theo loại mà không kèm tọa độ chính xác (US4, US6).
- **FR-012**: Phải nêu rõ dữ liệu gửi tới bên cung cấp bản đồ; không gửi GPS như
  định danh/theo dõi. Yêu cầu tải vùng bản đồ có thể tiết lộ vùng đang xem (US5).
- **FR-013**: Dữ liệu cũ và vị trí sai số lớn phải được nhận diện; vị trí và điểm chọn
  không được diễn giải thành kết luận an toàn hay mức rủi ro ngập (US2, US4).

- **FR-014**: Chỉ hỗ trợ vị trí và điểm chọn thuộc đa giác demo nội thành TP.HCM;
  hiển thị ranh giới vùng hỗ trợ. Đa giác phải bao trọn mọi điểm ngập trong bộ dữ
  liệu seed và toàn bộ tuyến demo. Ngoài vùng hiện “chưa hỗ trợ”, không lưu lịch sử
  và cho chọn lại trong vùng. Điểm đúng trên biên được tính là trong vùng; xét tọa độ
  GPS hoặc điểm thủ công, không mở rộng theo vòng sai số. Kiểm thử điểm trong,
  ngoài, sát biên hai phía và đúng trên biên (US2, US4).

- **FR-015**: Khi mất GPS, giữ điểm cuối và thời điểm, đánh dấu cũ sau 30 giây;
  không ghi lại điểm cũ như điểm mới. Mất mạng giữ phần bản đồ đã tải, tiếp tục
  GPS nếu có nhưng không xếp hàng hoặc gửi bù điểm. Khi phục hồi, tiếp tục theo
  trạng thái theo dõi và đồng ý lưu hiện hành; tắt theo dõi/lưu trong sự cố phải
  được tôn trọng. Vị trí cũ hoặc mất mạng phải có marker đổi màu (nếu có marker)
  và banner chữ tiếng Việt; không chỉ dùng màu để biểu đạt trạng thái (US3, US4).
- **FR-016**: Ghi log các lần chuyển trạng thái mất GPS, mất mạng và phục hồi,
  gồm loại sự kiện và thời điểm, không kèm tọa độ hoặc mật khẩu. Khi không có
  mạng, log vẫn phải ghi được tại thiết bị; việc chuyển log vận hành về máy chủ
  nếu có phải tách biệt với lịch sử vị trí và không chứa điểm GPS (US4, US6).

- **FR-017**: Tài khoản demo phải được seed từ biến môi trường hoặc secret;
  email/mật khẩu demo không được hardcode trong mã nguồn hoặc ghi vào log.
  Seed lại không tạo tài khoản trùng. Thiếu cấu hình phải báo rõ cho người vận hành,
  không tạo mật khẩu mặc định ngầm; bản đồ ẩn danh vẫn phải dùng được (US5, US6).

### Non-functional Requirements

- **NFR-001**: Bản đồ dùng được trong 5 giây ở ít nhất 9/10 lần tải mới theo
  điều kiện đo; marker GPS và độ chính xác xuất hiện trong 10 giây từ thao tác bấm
  lấy vị trí khi bản đồ đã mở, quyền có sẵn và GPS trả trong 5 giây.
  Không tính thời gian trả lời hộp thoại quyền.
- **NFR-002**: Giao diện dùng được từ 360px; không cuộn ngang trang, nút thao tác
  có nhãn rõ và vùng chạm tối thiểu 44 × 44px; trạng thái không chỉ phân biệt bằng màu.
- **NFR-003**: Sau khi nhận một vị trí hợp lệ mới, giao diện phản ánh trong 1 giây;
  kéo/thu phóng và bật/tắt không làm trang mất phản hồi quá 1 giây trong luồng đo.
- **NFR-004**: Có kiểm thử tự động cho mở bản đồ, cấp/từ chối quyền, cập nhật,
  bật/tắt, chọn thủ công, timeout, trạng thái hệ thống, tài khoản, đồng ý lưu, xoá lịch sử, hết hạn 30 ngày, mất GPS/mạng và phục hồi. Demo GPS thực phải được
  xác minh trên điện thoại; vị trí mô phỏng trong kiểm thử không thay thế xác minh này.
- **NFR-005**: Tuân thủ constitution 1.0.0 về cấu trúc lõi, logging, lỗi, kiểm thử
  và ghi quyết định. Không tạo Risk Engine hoặc AI Worker trong feature này;
  nghĩa vụ cho chúng được áp dụng khi tính năng tương ứng được đặc tả.

- **NFR-006**: Ràng buộc kỹ thuật do người dùng chốt: dùng ASP.NET Core Identity
  và bảng người dùng chuẩn; bổ sung tự đăng ký sau MVP không cần thay thiết kế
  dữ liệu tài khoản. Plan phải tuân thủ lựa chọn này, không chọn cơ chế tài khoản khác.

- **NFR-007**: Ràng buộc kỹ thuật do người dùng chốt: vùng hỗ trợ được lưu trong
  file GeoJSON có phiên bản trong repo và nạp vào PostGIS khi khởi tạo.
  Plan phải ghi nguồn/phiên bản vùng và cách xác nhận bao phủ điểm seed, tuyến demo.
  Phép kiểm tra phải dùng ST_Covers hoặc tương đương để tính cả điểm trên biên;
  không dùng ST_Contains cho quyết định trong/ngoài vùng.

### Key Entities

- **Supported Area**: Đa giác demo nội thành có phiên bản, bao phủ điểm ngập seed và tuyến demo; là căn cứ xác định trong/ngoài vùng.
- **Map View**: Vùng xem, tâm và mức thu phóng hiện tại.
- **Location Fix**: Tọa độ, độ chính xác và thời điểm thiết bị xác định; chỉ trong phiên nếu chưa đồng ý lưu.
- **Location Session**: Nguồn GPS/thủ công, trạng thái quyền, trạng thái theo dõi,
  vị trí cuối và độ mới dữ liệu; không phải lịch sử hành trình.
- **Manual Point**: Tọa độ người dùng chọn, nhãn nguồn thủ công; không có độ chính xác GPS.
- **Account**: Danh tính đăng nhập bằng email + mật khẩu, sở hữu lịch sử riêng.
- **Location History Record**: Chủ tài khoản, tọa độ GPS, độ chính xác, thời điểm ghi nhận và hạn xoá sau 30 ngày.
- **Saving Consent**: Trạng thái đồng ý lưu, độc lập với quyền GPS và bật/tắt theo dõi.
- **System Status**: Trạng thái sẵn sàng, thời điểm và kết quả kiểm tra phụ thuộc bắt buộc.
- **Error Event**: Loại lỗi, thời điểm, mã đối chiếu và ngữ cảnh chẩn đoán đã loại tọa độ.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Trong 10 lần mở mới trên điện thoại theo điều kiện đo, ít nhất 9 lần
  bản đồ tương tác được trong 5 giây (FR-001, FR-009; NFR-001).
- **SC-002**: Khi bản đồ đã mở, quyền có sẵn và GPS cung cấp trong 5 giây, ít nhất
  9/10 lần marker cùng độ chính xác xuất hiện trong 10 giây từ thao tác bấm lấy
  vị trí; không tính thời gian trả lời hộp thoại quyền (FR-002).
- **SC-003**: Trong lần đi bộ 30–50m ngoài trời, marker phản ánh mỗi vị trí hợp lệ
  được nhận trong 1 giây; khi tắt, không áp dụng thêm cập nhật (FR-003, FR-004).
- **SC-004**: Các tình huống từ chối quyền, không hỗ trợ và quá hạn đều có hướng dẫn;
  người dùng chọn được điểm trong tối đa 2 thao tác từ lúc thấy lỗi (FR-005, FR-006).
- **SC-005**: Toàn bộ luồng demo thực hiện được trên màn hình 360px không bị che
  nút chính hoặc cuộn ngang; dữ liệu cũ được đánh dấu sau 30 giây (NFR-002, FR-013).
- **SC-006**: Khi hệ thống bình thường, người vận hành thấy trạng thái hoạt động
  trong 3 giây; khi mô phỏng phụ thuộc bắt buộc lỗi, thấy chưa sẵn sàng (FR-010).
- **SC-007**: Các trường hợp kiểm thử tự động tại NFR-004 đều đạt trước demo;
  báo cáo phân biệt kết quả tự động và kiểm tra điện thoại thực (FR-001–FR-011).
- **SC-008**: Kiểm tra ở chế độ ẩn danh và chưa bật lưu xác nhận không lưu bền vững
  hoặc gửi GPS lên máy chủ ứng dụng; mọi log không chứa tọa độ (FR-008, FR-011, FR-012).
- **SC-009**: Kiểm thử tài khoản và đồng ý xác nhận chỉ lưu ba trường vị trí đã chốt;
  điểm thủ công không được lưu; tắt lưu dừng ghi mới; xoá toàn bộ làm dữ liệu hiện có
  không còn truy xuất được. Đủ 30 ngày từ lúc ghi nhận, giao diện và API không còn
  cho truy xuất bản ghi ngay tại mốc hết hạn; tác vụ nền xoá vật lý với mục tiêu
  trong 1 phút khi hệ thống hoạt động bình thường, có thể chậm hơn khi hệ thống lỗi
  và cảnh báo khi trễ quá 5 phút. Không coi bản sao lưu của nhà cung cấp đã bị xoá
  tức thì. Tài khoản khác không thể xem/xoá dữ liệu (US5).
- **SC-010**: GPS và điểm thủ công ngoài vùng hỗ trợ đều hiện “chưa hỗ trợ”
  và không được ghi vào lịch sử. Kiểm thử phải có điểm trong, ngoài, sát biên ở cả
  hai phía và đúng trên đường biên; điểm trên biên phải được chấp nhận, điểm ngay
  ngoài biên phải bị từ chối lưu, kể cả khi vòng sai số giao vùng hỗ trợ (FR-014).
- **SC-011**: Kiểm thử mất GPS xác nhận điểm cuối giữ nguyên, marker đổi màu và
  banner vị trí cũ xuất hiện ở mốc 30 giây. Mất mạng có banner và marker đổi màu
  nếu có marker; GPS mới vẫn cập nhật trên phần bản đồ đã tải. Phục hồi không gửi
  bù điểm; chỉ tiếp tục theo dõi/lưu khi trạng thái tương ứng còn bật (FR-015).
- **SC-012**: Mô phỏng mất GPS, mất mạng và phục hồi tạo được log chuyển trạng thái
  với thời điểm, không chứa tọa độ; sự kiện mất mạng ghi được khi đang offline (FR-016).

- **SC-013**: Seed với secret hợp lệ tạo tài khoản demo đăng nhập được; chạy lại
  không tạo trùng; thiếu secret không sinh mật khẩu mặc định. Kiểm tra mã nguồn
  và log không có thông tin đăng nhập demo được hardcode hoặc lộ ra (FR-017).
- **SC-014**: Kiểm thử đăng nhập đúng/sai và hai trường hợp đăng xuất. Khi kết nối
  được máy chủ, phiên bị thu hồi, phiên cũ không còn đọc lịch sử hoặc ghi vị trí.
  Khi không kết nối được máy chủ, thiết bị xoá phiên cục bộ và dừng lưu; thông báo
  phiên máy chủ sẽ hết hạn trong tối đa 15 phút, không báo đã thu hồi phiên máy chủ.
  Đây là giới hạn đã biết; vẫn dùng được bản đồ ẩn danh trong giới hạn kết nối/dữ liệu
  đã tải (FR-007; truy vết T027, T037).

- **SC-015**: Mọi điểm ngập seed và toàn bộ hình học tuyến demo đều nằm trong vùng
  hỗ trợ; khởi tạo nạp đúng phiên bản vùng đã lưu trong repo (FR-014, NFR-007).

## Assumptions

- Ẩn danh dùng ngay; đăng nhập email + mật khẩu và lịch sử có đồng ý thuộc phạm vi lần này. Giao diện ưu tiên mobile web và tiếng Việt.
- Phạm vi MVP là đa giác demo nội thành; mở rộng toàn TP.HCM sau thi. Điểm ngập seed
  và tuyến demo chỉ dùng xác định/kiểm thử vùng, không triển khai dữ liệu ngập hoặc tìm đường.
- Điều kiện đo: điện thoại có trình duyệt hiện hành, màn hình 360px trở lên,
  trang ở tiền cảnh; tải mới không tận dụng dữ liệu bản đồ của lần trước;
  mạng 4G ổn định với tải xuống tối thiểu 10 Mbps, tải lên 2 Mbps và độ trễ tối đa 100ms.
  Ghi thiết bị, trình duyệt và điều kiện mạng khi báo cáo.
- Mốc tải bản đồ tính từ mở link; mốc GPS 10 giây tính từ thao tác bấm lấy vị trí
  khi bản đồ đã mở, loại thời gian người dùng đọc/trả lời hộp thoại quyền.
  Không thể cam kết thời gian GPS khi thiết bị không có tín hiệu.
- Cần nhà cung cấp bản đồ cho phép dùng công khai, kết nối mạng và nơi phục vụ HTTPS.
  Sự cố bên ngoài phải hiển thị rõ; bản đồ offline không nằm trong phạm vi.
- Luồng demo: mở HTTPS → bản đồ TP.HCM → lấy GPS và độ chính xác → bật theo dõi,
  đi bộ → tắt → thử nhánh từ chối/timeout và chọn thủ công → kiểm tra trạng thái.
- Constitution vẫn là chuẩn; các nghĩa vụ về chấm điểm ngập/AI được hoãn tới
  feature có các khả năng đó, không phải ngoại lệ với nguyên tắc.

## Out of Scope

Thời tiết, dữ liệu ngập lịch sử, chấm điểm rủi ro, tìm đường, AI nhận diện ảnh,
thanh toán, thông báo đẩy, báo cáo cá nhân, tự đăng ký (roadmap sau MVP),
xác minh email, khôi phục mật khẩu và quản lý tài khoản nâng cao,
mở rộng vùng hỗ trợ ra toàn TP.HCM (sau thi), làm mượt trạng thái GPS sát biên (sau MVP), bản đồ offline đầy đủ, gửi bù điểm GPS thu được khi mất mạng và bảo đảm theo dõi khi trang ở nền/đóng.





