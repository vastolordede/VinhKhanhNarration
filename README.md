<!-- ========================================================= -->
<!-- FILE 1: VinhKhanhNarration/README.md                     -->
<!-- ========================================================= -->

# VinhKhanhNarration

Dự án **Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**.

Hệ thống được thiết kế theo hướng RESTful API. Backend chịu trách nhiệm xử lý nghiệp vụ, kết nối cơ sở dữ liệu PostgreSQL và cung cấp API cho frontend hoặc các client khác sử dụng.

Frontend hiện tại đang trong quá trình phát triển.

---

## 1. Tổng quan dự án

Mục tiêu của hệ thống là hỗ trợ khách tham quan phố ẩm thực Vĩnh Khánh có thể nghe thuyết minh tự động bằng nhiều ngôn ngữ thông qua QR code hoặc geofence.

Các chức năng chính của hệ thống gồm:

```text
- Quản lý tài khoản nội bộ
- Quản lý ngôn ngữ
- Quản lý danh mục hệ thống
- Quản lý địa điểm / POI
- Quản lý món ăn
- Quản lý nội dung thuyết minh
- Quản lý bản dịch đa ngôn ngữ
- Quản lý file audio
- Quản lý QR code
- Tạo phiên khách anonymous
- Ghi nhận lịch sử nghe
- Gửi và duyệt feedback
- Kiểm tra geofence và tự động phát thuyết minh theo vị trí
```

---

## 2. Cấu trúc thư mục

Cấu trúc tổng thể của project:

```text
VinhKhanhNarration/
│
├── backend/
│   ├── BUS/
│   ├── Controllers/
│   ├── DAO/
│   ├── Database/
│   ├── DTO/
│   ├── Properties/
│   ├── Swagger/
│   ├── Utils/
│   ├── data/
│   ├── .env
│   ├── .env.example
│   ├── appsettings.json
│   ├── Program.cs
│   ├── README.md
│   └── VinhKhanhNarration.Api.csproj
│
├── frontend/
│   └── README.md
│
└── README.md
```

Trong đó:

```text
backend/
- Chứa RESTful API viết bằng ASP.NET Core.
- Xử lý nghiệp vụ chính của hệ thống.
- Kết nối PostgreSQL.
- Cung cấp Swagger để test API.

frontend/
- Hiện tại đang trong quá trình phát triển.
- Chi tiết sẽ được cập nhật sau trong README của frontend.
```

---

## 3. Công nghệ sử dụng

Backend sử dụng:

```text
- ASP.NET Core Web API
- PostgreSQL
- Npgsql
- Swagger / Swashbuckle
- BCrypt.Net-Next
- RESTful API
- OOP 3 lớp: DTO / DAO / BUS
```

Frontend:

```text
- Hiện tại đang trong quá trình phát triển.
```

---

## 4. Cài đặt PostgreSQL trên máy

Trước khi cấu hình backend, cần cài PostgreSQL trên máy.

Thông tin cần chuẩn bị:

```text
Host: localhost
Port: 5432
Database: vinh_khanh_narration_db
Username: postgres
Password: mật khẩu PostgreSQL của máy bạn
```

Khi cài PostgreSQL, cần ghi nhớ mật khẩu của user `postgres`.

Ví dụ:

```text
Username: postgres
Password: 123456
```

Mật khẩu này sẽ được dùng để cấu hình file `.env` trong backend.

---

## 5. Tạo database

Sau khi cài PostgreSQL, tạo database với tên:

```text
vinh_khanh_narration_db
```

Lệnh SQL tạo database:

```sql
CREATE DATABASE vinh_khanh_narration_db;
```

Sau khi tạo database, mở đúng database `vinh_khanh_narration_db` trong công cụ quản lý PostgreSQL như DBeaver hoặc pgAdmin, sau đó chạy script tạo bảng trong thư mục backend.

Script database nằm trong:

```text
backend/data/
```

---

## 6. Clone project

Sau khi setup PostgreSQL, clone project về máy.

Cấu trúc sau khi clone nên là:

```text
VinhKhanhNarration/
├── backend/
└── frontend/
```

Nếu clone về mà folder backend có tên khác, có thể đổi tên folder backend lại thành:

```text
backend
```

Tên folder có thể thay đổi, nhưng nên giữ là `backend` để dễ quản lý và đúng với tài liệu cấu hình.

---

## 7. Cấu hình backend

Sau khi clone project và tạo database xong, chuyển sang đọc hướng dẫn cấu hình backend tại:

```text
backend/README.md
```

Backend cần cấu hình các thông tin chính sau:

```text
- DB_HOST
- DB_PORT
- DB_NAME
- DB_USER
- DB_PASSWORD
- ASPNETCORE_ENVIRONMENT
- ASPNETCORE_URLS
```

Các thông tin này được đặt trong file:

```text
backend/.env
```

Không nên ghi trực tiếp connection string vào source code.

Không nên đẩy file `.env` thật lên GitHub vì file này có chứa mật khẩu database.

---

## 8. Cấu hình frontend

Frontend hiện tại đang trong quá trình phát triển.

Khi frontend được bổ sung, hướng dẫn cấu hình sẽ được cập nhật trong:

```text
frontend/README.md
```

---

## 9. Ghi chú về database

Database hiện tại dùng PostgreSQL và bao gồm các nhóm bảng chính:

```text
admin_users

languages

place_types
content_types
target_types
translation_sources
trigger_modes
geofence_event_types
geofence_event_statuses

places
dish_categories
dishes
place_dishes

narration_contents
narration_translations
audio_files

qr_codes

guest_sessions
guest_poi_states
geofence_events

listening_histories
feedbacks
```

Một số điểm thiết kế quan trọng:

```text
- Tài khoản nội bộ dùng bảng admin_users.
- Role nội bộ được fix cứng: Admin, ContentManager, Translator, Reviewer.
- Khách không cần account riêng.
- Khách sử dụng guest_session_id.
- Các loại dữ liệu như place type, content type, target type, translation source, trigger mode, event type, event status được tách thành bảng lookup để dễ CRUD và mở rộng.
- Geofence dùng places như POI.
- QR code có thể trỏ tới địa điểm, món ăn hoặc nội dung thuyết minh.
```

---

## 10. Ghi chú phát triển

Dự án hiện tại đang tập trung vào backend RESTful API.

Frontend sẽ được phát triển sau.

Backend được thiết kế độc lập để sau này có thể kết nối với nhiều loại client khác nhau, ví dụ:

```text
- Web frontend
- Mobile app
- Admin dashboard
- Desktop app
```

Miễn client gọi được REST API thì không cần thay đổi kiến trúc backend.






