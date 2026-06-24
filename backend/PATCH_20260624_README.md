# Patch 2026-06-24 — Guest Session lifecycle, log cleanup, audio fix

## Quy tắc nghiệp vụ sau patch

- Không còn tạo Guest Session miễn phí.
- Mỗi mock payment Guest được xác nhận thành công tạo **một Guest Session mới** và một Access Pass 24 giờ.
- Guest payment hiển thị QR có thể quét thật, nhưng QR chỉ chứa URL nội bộ của ứng dụng và tuyệt đối không chứa thông tin ngân hàng/VietQR.
- Giá mua được chụp lại tại `guest_sessions.access_price`. Dashboard cộng doanh thu Guest từ giá trên từng session, không lấy số lượt nghe để tính tiền.
- Guest mua lại sau khi hết hạn sẽ nhận session mới; session cũ không được tái kích hoạt.
- Hết hạn pass: session chuyển `is_active = false`; narration, audio, geofence và việc cập nhật listening history bị chặn.
- Listening history được giữ làm log read-only cho Admin.
- Geofence là log read-only và được scheduler xóa sau thời gian retention.
- Audio DTO đã được đồng bộ với schema để tránh lỗi `/api/audio-files` do mapper yêu cầu các cột moderation không tồn tại.

## Migration

Chạy sau migration `20260619_complete_missing_modules.sql`:

```text
backend/data/migrations/20260624_guest_session_lifecycle_cleanup.sql
```

> Migration này dành cho database local/test và có thao tác xóa dữ liệu runtime cũ. Hãy backup database trước khi chạy.

Migration sẽ xóa dữ liệu cũ trong:

- listening histories
- guest POI state
- geofence events
- feedback test/runtime
- Guest sessions, payment orders và Access Pass cũ
- audio records cũ
- refresh tokens và audit logs test

Migration giữ lại:

- tài khoản Admin
- các ngôn ngữ hợp lệ vi/en/ja/ko/zh
- lookup/master data
- places, dishes và narration mẫu, ngoại trừ một số narration có tên test rõ ràng

Sau migration, có thể xóa thủ công file audio cũ trong `backend/wwwroot/generated-audio/` nếu thư mục này tồn tại.

## Chạy kiểm tra

```powershell
cd backend
dotnet restore
dotnet build
dotnet test tests/VinhKhanhNarration.Api.Tests/VinhKhanhNarration.Api.Tests.csproj
dotnet run
```

```powershell
cd frontend
npm ci
npm test -- --run
npm run build
npm run dev
```

## Test nhanh Guest

1. Mở `/app/access` khi chưa có session.
2. Tạo mock payment và kiểm tra QR xuất hiện cùng nhãn `DEMO / MOCK PAYMENT`.
3. Quét QR hoặc mở URL nội bộ; xác nhận payment bằng nút Mock.
4. Kiểm tra response có `guestSession`, `accessPrice` và Access Pass 24 giờ.
5. Tạo payment lần hai: phải sinh `guestSessionId` khác.
6. Đặt `access_expires_at` và pass `expires_at` về thời gian quá khứ, chạy lại backend/scheduler.
7. Narration, audio và geofence phải trả 402; Admin vẫn xem được listening history đã ghi trước đó.

## Dashboard

Endpoint mới:

```http
GET /api/admin/dashboard/statistics?period=month
```

`period` nhận: `month`, `quarter`, `halfYear`, `year`, `all`.

- Guest revenue: tổng `guest_sessions.access_price` trong khoảng thời gian.
- Vendor revenue: tổng `payment_orders.amount` có trạng thái `Paid` trong khoảng thời gian.


## QR Mock Payment

- QR chỉ mã hóa `payment_url` dạng `/app/access?orderCode=...`.
- Endpoint đọc đơn: `GET /api/public/access/orders/{orderCode}`.
- Nút xác nhận gọi endpoint Mock hiện có và chỉ cập nhật database; không kết nối ngân hàng, VietQR, ví điện tử hoặc payment gateway.
- Để quét từ điện thoại khi chạy local, cấu hình `FRONTEND_URL` bằng IP LAN của máy tính, ví dụ `http://192.168.1.20:5173`.
- Frontend dùng dependency `qrcode.react` để render QR SVG.

### Chạy demo QR trên điện thoại cùng Wi-Fi

Ví dụ máy tính có IP LAN `192.168.1.20`:

```env
# backend/.env
ASPNETCORE_URLS=http://0.0.0.0:5151
CORS_ALLOWED_ORIGINS=http://localhost:5173,http://192.168.1.20:5173
FRONTEND_URL=http://192.168.1.20:5173

# frontend/.env
VITE_API_BASE_URL=http://192.168.1.20:5151
```

Điện thoại và máy tính phải ở cùng mạng. Không điền tài khoản ngân hàng, mã ngân hàng hoặc thông tin VietQR vào QR mock.
