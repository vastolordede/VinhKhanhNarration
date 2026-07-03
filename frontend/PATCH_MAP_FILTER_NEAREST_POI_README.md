# Patch: Map filter + nearest POI distance

## Chức năng

- Thêm ô tìm kiếm theo tên sạp trên Public Map.
- Tìm kiếm không phân biệt chữ hoa/thường và dấu tiếng Việt.
- Chỉ hiển thị marker phù hợp với tên đang lọc.
- Khi chỉ còn một kết quả, bản đồ tự focus vào sạp đó.
- Khi trình duyệt đã cấp vị trí, tính POI gần nhất bằng công thức Haversine.
- Khoảng cách được tính từ GPS hiện tại của thiết bị đến POI gần nhất trong danh sách đang hiển thị.
- Nhấn vào thẻ POI gần nhất để mở chi tiết sạp.
- Chi tiết sạp hiển thị thêm khoảng cách từ vị trí hiện tại.
- Bổ sung bản dịch UI cho vi/en/ja/ko/zh.

## Phạm vi

Chỉ sửa frontend. Không cần migration, không thay đổi database, backend hoặc biến môi trường.

## Kiểm tra

1. Mở `/app/map`.
2. Nhập một phần tên sạp vào ô tìm kiếm.
3. Kiểm tra marker và số sạp hiển thị thay đổi.
4. Nhấn nút định vị và cấp quyền GPS.
5. Kiểm tra thẻ `POI gần nhất` và khoảng cách.
6. Nhấn thẻ POI gần nhất để mở bottom sheet.
