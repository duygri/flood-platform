# ADR-003: Quy tắc có phiên bản và xếp lại ứng viên OSRM

**Ngày**: 2026-10-20. **Trạng thái**: Chấp nhận cho thiết kế, cần spike/golden tests trước release.

## Bối cảnh

MVP cần risk giải thích được và tuyến trong5s, không có dataset hiệu chuẩn thủy văn.
OSRM stack đã chốt sinh tuyến theo thời gian/profile, không có flood dynamic
polygon exclusion trực tiếp, alternatives không bảo đảm đủ3tuyến.

## Quyết định

Version/hash immutable heuristic config, pure .NET Risk với sources/timestamps,
missing=null và uncertainty reserve minh bạch. Không tuyên bố xác suất/độ chính xác
thực địa. Rerank1–3 candidate hợp lệ trong vùng bằng tuple đã clarify và ngưỡng
min(1.5Tfastest,Tfastest+600s), luôn công bố bốn số liệu và missing length.
“Cải thiện nguy cơ” so bộ ba highVeryHighMeters/routeRiskScore/missingMeters theo
thứ tự từ điển với fastest; chỉ không có cải thiện trong ngưỡng mới fallback fastest.
PostGIS kiểm toàn LineString bằng Covers; không clipping đường ngoài vùng.
Profile car.lua chỉ ô tô tham khảo, không giả tuyến xe máy. Route aggregate phần
known có nhãn partial; allunknown=null, không dùng0 cho missing.

## Đánh đổi và lựa chọn khác

Flood-aware shortest path toàn graph có thể tốt hơn tập ứng viên nhưng thêm tích
hợp/custom profile/graph update chưa đủ bằng chứng cho MVP. Tối ưu thời gian/risk
bằng một điểm trọng số không giữ thứ tự clarify, bị loại. Gọi weather mỗisegment
vỡ budget/quota, thay bằng batch cappedcells; nhãn chất lượng/unknown giữ nguyên.
Không đảm bảo tuyến ít nguy cơ nhất toàn thành phố hoặc phù hợp mọi phương tiện.

## Hệ quả và điều kiện xem lại

Cần graph/pipeline/VM riêng có spike RAM và budget5s; không có quota thì US5BLOCKED.
Ghi liệu thời tiết mô hình thưa hơn đường, sampling50m bỏ sót hotspot nhỏ. Sau khi có
bằng chứng ứng viên OSRM thường không có đường ít nguy cơ dù graph có, đề xuất
feature tối ưu toàn mạng với ADR mới. Không sửa rule dưới cùng version để làm demo đẹp.
