# Patch 2026-06-24 — Guest Session lifecycle, log cleanup, audio fix

## Quy tắc nghiệp vụ sau patch

- Không còn tạo Guest Session miễn phí.
- Mỗi mock payment Guest được xác nhận thành công tạo **một Guest Session mới** và một Access Pass 24 giờ.
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

Sau migration:

- đăng nhập lại Admin/Vendor vì refresh token test đã được xóa;
- có thể xóa thủ công file audio cũ trong `backend/wwwroot/generated-audio/` nếu thư mục này tồn tại;
- Translation/TTS thật vẫn cần cấu hình Azure key/region. Không có key thì audio mới sẽ ở trạng thái `Failed` với thông báo cấu hình, nhưng API danh sách không được trả 500.

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
2. Tạo mock payment.
3. Xác nhận payment.
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
