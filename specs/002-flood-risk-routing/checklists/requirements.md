# Specification Quality Checklist: Mưa, rủi ro ngập và tuyến đường tham khảo

**Purpose**: Kiểm tra chất lượng trước clarify hoặc plan.
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] Không chứa lựa chọn ngôn ngữ, framework, nhà cung cấp hoặc thiết kế triển khai.
- [x] Tập trung vào giá trị người dùng và nhu cầu sản phẩm.
- [x] Viết cho bên liên quan không chuyên kỹ thuật.
- [x] Hoàn thành mọi mục bắt buộc của template.

## Requirement Completeness

- [x] Không còn dấu NEEDS CLARIFICATION.
- [x] Yêu cầu kiểm thử được, giả định ghi rõ.
- [x] Tiêu chí thành công đo được.
- [x] Tiêu chí thành công không phụ thuộc công nghệ.
- [x] Có kịch bản nghiệm thu từng story.
- [x] Xác định trường hợp biên, lỗi và thiếu dữ liệu.
- [x] Phạm vi và ưu tiên rõ.
- [x] Ghi nhận phụ thuộc và giả định.

## Feature Readiness

- [x] Mọi FR có kịch bản hoặc SC tương ứng.
- [x] Kịch bản bao phủ luồng chính và kiểm thử độc lập từng story.
- [x] Kết quả mong muốn cụ thể hóa bằng SC-001–SC-012.
- [x] Không lẫn thiết kế triển khai vào đặc tả.

## Notes

- Rà soát 16/16 mục; đây là kiểm tra tài liệu, không phải kết quả chạy sản phẩm.
- Giả định cần xác nhận ở clarify: dự báo ba giờ, bán kính 1 km, bốn mức rủi ro.
- Công thức/ngưỡng, độ mới nguồn và tổng hợp tuyến cần chốt trong plan trước triển khai/nghiệm thu.
- Spec 001 chưa triển khai hoàn chỉnh; đã ghi phụ thuộc, không sửa spec 001.
- Không có hook trước/sau specify vì `.specify/extensions.yml` không tồn tại.
