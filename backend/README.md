<!-- ========================================================= -->
<!-- FILE 2: VinhKhanhNarration/backend/README.md             -->
<!-- ========================================================= -->

# VinhKhanhNarration Backend

RESTful API C# ASP.NET Core cho đồ án:

**Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**

Backend cung cấp API cho các chức năng:

```text
- Quản lý tài khoản nội bộ
- Quản lý ngôn ngữ
- Quản lý bảng danh mục lookup
- Quản lý địa điểm / POI
- Quản lý món ăn
- Quản lý nội dung thuyết minh
- Quản lý bản dịch
- Quản lý file audio
- Quản lý QR code
- Tạo phiên khách anonymous
- Ghi nhận lịch sử nghe
- Gửi và duyệt feedback
- Kiểm tra geofence và tự động phát thuyết minh theo vị trí
```

---
## 1. Kiến trúc backend

Backend được thiết kế theo mô hình OOP nhiều lớp:

```text
Controller / REST API Layer
    -> BUS Layer
        -> DAO Layer
            -> DTO Layer
                -> PostgreSQL
```

Ý nghĩa từng lớp:

```text
Controller:
- Nhận request từ client.
- Gọi BUS xử lý nghiệp vụ.
- Trả response JSON.

BUS:
- Chứa business logic.
- Validate dữ liệu.
- Điều phối nhiều DAO nếu cần.

DAO:
- Làm việc trực tiếp với PostgreSQL.
- Thực hiện INSERT, UPDATE, DELETE, SELECT.

DTO:
- Object chứa dữ liệu.
- Dùng để truyền dữ liệu giữa các layer.
```

---

## 2. Công nghệ sử dụng

```text
- ASP.NET Core Web API
- PostgreSQL
- Npgsql
- Swagger / Swashbuckle
- BCrypt.Net-Next
- RESTful API
- OOP 3 lớp: DTO / DAO / BUS
```

---

## 3. Cấu trúc thư mục backend

```text
backend/
│
├── BUS/
│   └── Chứa các class xử lý nghiệp vụ.
│
├── Controllers/
│   └── Chứa các REST API controller.
│
├── DAO/
│   └── Chứa các class truy vấn PostgreSQL.
│
├── Database/
│   └── Chứa DbConnectionFactory.
│
├── DTO/
│   └── Chứa các class data transfer object.
│
├── Properties/
│   └── Chứa launchSettings.json.
│
├── Swagger/
│   └── Chứa cấu hình Swagger.
│
├── Utils/
│   └── Chứa các helper như EnvLoader, PasswordHasher, GeoDistanceCalculator.
│
├── data/
│   └── Chứa script database.
│
├── .env
├── .env.example
├── .gitignore
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── README.md
└── VinhKhanhNarration.Api.csproj
```

---

## 4. Cấu hình database bằng `.env`

Backend không hard-code connection string trong source code.

Database được đọc từ file:

```text
backend/.env
```

Nội dung mẫu:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=vinh_khanh_narration_db
DB_USER=postgres
DB_PASSWORD=your_postgres_password

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5151
```

Trong đó:

```text
DB_HOST:
- Host PostgreSQL.
- Thường là localhost.

DB_PORT:
- Port PostgreSQL.
- Mặc định thường là 5432.

DB_NAME:
- Tên database của project.
- Mặc định là vinh_khanh_narration_db.

DB_USER:
- User PostgreSQL.
- Thường là postgres.

DB_PASSWORD:
- Mật khẩu PostgreSQL trên máy của người chạy project.

ASPNETCORE_ENVIRONMENT:
- Môi trường chạy backend.
- Development dùng cho môi trường phát triển.

ASPNETCORE_URLS:
- URL backend sẽ lắng nghe.
- Ví dụ http://localhost:5151.
```

Ví dụ nếu thông tin PostgreSQL trong DBeaver là:

```text
Host: localhost
Port: 5432
Database: vinh_khanh_narration_db
Username: postgres
Password: 123456
```

thì file `.env` sẽ là:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=vinh_khanh_narration_db
DB_USER=postgres
DB_PASSWORD=123456

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5151
```

---

## 5. File `.env.example`

File `.env.example` dùng làm mẫu cấu hình.

Vị trí:

```text
backend/.env.example
```

Nội dung:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=vinh_khanh_narration_db
DB_USER=postgres
DB_PASSWORD=your_password_here

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5151
```

Lưu ý:

```text
.env.example có thể đưa lên GitHub.
.env thật không nên đưa lên GitHub vì có chứa mật khẩu database.
```

---

## 6. File `.gitignore`

File `.gitignore` nên có:

```gitignore
bin/
obj/

.env

.vs/
.vscode/

*.user
*.suo
*.cache
```

Mục đích:

```text
- Không đẩy file build.
- Không đẩy file .env chứa mật khẩu database.
- Không đẩy file cấu hình cá nhân của IDE.
```

---

## 7. Cấu hình `appsettings.json`

Backend hiện tại không cần lưu connection string trong `appsettings.json`.

File `appsettings.json` có thể để như sau:

```json
{
  "Jwt": {
    "SecretKey": "CHANGE_THIS_SECRET_KEY_FOR_DEVELOPMENT_ONLY",
    "Issuer": "VinhKhanhNarration",
    "Audience": "VinhKhanhNarrationClient",
    "ExpiresInMinutes": 120
  },
  "AllowedHosts": "*"
}
```

Connection string sẽ được tạo trong `DbConnectionFactory` từ biến môi trường đọc được từ `.env`.

---

## 8. Cấu hình `DbConnectionFactory`

File:

```text
backend/Database/DbConnectionFactory.cs
```

`DbConnectionFactory` có nhiệm vụ đọc các biến môi trường:

```text
DB_HOST
DB_PORT
DB_NAME
DB_USER
DB_PASSWORD
```

Sau đó tạo connection string cho Npgsql.

Không nên ghi trực tiếp thông tin database trong file này.

---

## 9. Database schema

Database cần đúng schema cuối cùng đã chốt.

Tên database:

```text
vinh_khanh_narration_db
```

Các nhóm bảng chính:

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

Các bảng lookup sau được dùng để thay thế giá trị fix cứng:

```text
place_types
content_types
target_types
translation_sources
trigger_modes
geofence_event_types
geofence_event_statuses
```

Riêng `admin_users.role` vẫn fix cứng với các giá trị:

```text
Admin
ContentManager
Translator
Reviewer
```

---

## 10. Module API chính

Backend gồm các nhóm controller chính:

```text
AuthController
AdminUsersController
LanguagesController

PlaceTypesController
ContentTypesController
TargetTypesController
TranslationSourcesController
TriggerModesController
GeofenceEventTypesController
GeofenceEventStatusesController

PlacesController
DishCategoriesController
DishesController
PlaceDishesController

NarrationContentsController
NarrationTranslationsController
AudioFilesController
QRCodesController

GuestSessionsController
GeofenceController
ListeningHistoriesController
FeedbacksController
```

---

## 11. Nhóm API quản trị

Nhóm API quản trị dùng cho admin nội bộ.

Ví dụ:

```text
POST   /api/auth/login

GET    /api/admin-users
POST   /api/admin-users
PUT    /api/admin-users/{id}
PATCH  /api/admin-users/{id}/deactivate
PATCH  /api/admin-users/{id}/restore

GET    /api/languages
POST   /api/languages
PUT    /api/languages/{id}
PATCH  /api/languages/{id}/set-default

GET    /api/places
POST   /api/places
PUT    /api/places/{id}
PATCH  /api/places/{id}/deactivate

GET    /api/dishes
POST   /api/dishes
PUT    /api/dishes/{id}

GET    /api/narration-contents
POST   /api/narration-contents
PUT    /api/narration-contents/{id}

GET    /api/narration-translations
POST   /api/narration-translations
PATCH  /api/narration-translations/{id}/review

GET    /api/audio-files
POST   /api/audio-files

GET    /api/qr-codes
POST   /api/qr-codes
```

---

## 12. Nhóm API public cho khách anonymous

Khách không cần account.

Luồng cơ bản:

```text
Khách mở app/web
-> tạo GuestSession
-> chọn ngôn ngữ
-> quét QR hoặc gửi vị trí geofence
-> hệ thống trả nội dung/audio
-> lưu lịch sử nghe
-> khách có thể gửi feedback
```

Các API public tiêu biểu:

```text
POST   /api/public/guest-sessions
PATCH  /api/public/guest-sessions/{guestSessionId}/language

POST   /api/public/qr/resolve

POST   /api/public/geofence/check

POST   /api/public/listening-histories
PATCH  /api/public/listening-histories/{historyId}/status
PATCH  /api/public/listening-histories/{historyId}/duration

POST   /api/public/feedbacks
```

---

## 13. QR resolve flow

API:

```text
POST /api/public/qr/resolve
```

Luồng xử lý:

```text
Client gửi qrCodeValue, languageId, guestSessionId
-> hệ thống tìm QRCode
-> xác định QR trỏ tới Place, Dish hoặc Narration
-> tìm NarrationContent tương ứng
-> tìm NarrationTranslation theo languageId
-> tìm AudioFile active
-> trả về title, text, audioUrl
-> có thể ghi ListeningHistories
```

Kết quả trả về dạng:

```json
{
  "placeId": 1,
  "dishId": null,
  "narrationId": 10,
  "translationId": 25,
  "audioId": 30,
  "title": "Giới thiệu phố ẩm thực Vĩnh Khánh",
  "text": "Nội dung thuyết minh...",
  "audioUrl": "/audio/vinh-khanh-vi.mp3"
}
```

---

## 14. Geofence flow

API:

```text
POST /api/public/geofence/check
```

Luồng xử lý:

```text
Client gửi guestSessionId, latitude, longitude, languageId
-> hệ thống lấy danh sách POI đang bật geofence
-> tính khoảng cách từ khách đến từng POI
-> chọn POI gần nhất hoặc có priority cao nhất
-> kiểm tra bán kính TriggerRadiusMeters
-> kiểm tra DebounceSeconds
-> kiểm tra CooldownSeconds
-> tạo GeofenceEvents
-> cập nhật GuestPoiStates
-> nếu hợp lệ thì tìm NarrationContent
-> tìm bản dịch theo languageId
-> tìm AudioFile active
-> trả về audio cần phát
-> ghi ListeningHistories
```

Kết quả trả về dạng:

```json
{
  "shouldPlay": true,
  "reason": "Valid geofence trigger",
  "placeId": 1,
  "narrationId": 10,
  "translationId": 25,
  "audioId": 30,
  "audioUrl": "/audio/vinh-khanh-vi.mp3",
  "distanceMeters": 35.5
}
```

---

## 15. Feedback flow

API:

```text
POST /api/public/feedbacks
```

Khách có thể feedback cho một trong ba đối tượng:

```text
Place
Dish
Narration
```

Quy tắc:

```text
- Rating từ 1 đến 5.
- Một feedback chỉ nên gắn với một target chính.
- Admin không sửa nội dung comment của khách.
- Admin chỉ approve hoặc reject feedback.
```

API admin:

```text
GET   /api/admin/feedbacks
GET   /api/admin/feedbacks/pending
PATCH /api/admin/feedbacks/{feedbackId}/approve
PATCH /api/admin/feedbacks/{feedbackId}/reject
```

---

## 16. Lưu ý về soft delete

Các bảng chính có `is_active` sẽ dùng soft delete:

```text
admin_users
languages
lookup tables
places
dish_categories
dishes
narration_contents
audio_files
qr_codes
guest_sessions
```

Một số bảng trong schema hiện tại chưa có `is_active`, ví dụ:

```text
place_dishes
narration_translations
```

Với các bảng này, có hai hướng:

```text
Cách 1:
- Giữ nguyên schema hiện tại.
- Dùng xóa vật lý khi cần.

Cách 2:
- Bổ sung is_active để soft delete thống nhất hơn.
```

Khuyến nghị nếu muốn soft delete thống nhất:

```sql
ALTER TABLE place_dishes
ADD COLUMN is_active BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE narration_translations
ADD COLUMN is_active BOOLEAN NOT NULL DEFAULT TRUE;
```

---

## 17. Swagger

Swagger được cấu hình trong:

```text
backend/Swagger/
```

Và được bật trong:

```text
backend/Program.cs
```

Swagger dùng để kiểm tra và test API trong quá trình phát triển backend.

---

## 18. Ghi chú khi dùng VS Code

Nếu VS Code hiện thông báo tải `.NET 10.0` hoặc báo C# Dev Kit đang cài runtime, đó là thông báo của extension, không nhất thiết là lỗi project.

Nên kiểm tra backend bằng terminal thay vì chỉ dựa vào thông báo của extension.

---

## 19. Thứ tự kiểm tra API đề xuất

Sau khi backend và database đã được cấu hình, nên kiểm tra API theo thứ tự:

```text
1. Kiểm tra Swagger mở được
2. Kiểm tra Languages
3. Kiểm tra lookup tables
4. Tạo AdminUser
5. Login
6. Tạo PlaceType / TriggerMode nếu chưa có
7. Tạo Place / POI
8. Tạo DishCategory
9. Tạo Dish
10. Gán Dish vào Place
11. Tạo NarrationContent
12. Tạo NarrationTranslation
13. Tạo AudioFile
14. Tạo QRCode
15. Tạo GuestSession
16. Resolve QR
17. Gửi geofence check
18. Gửi feedback
19. Xem listening histories
20. Xem geofence events
```

---

## 20. Ghi chú phát triển tiếp

Frontend hiện chưa chốt, nên backend được thiết kế dưới dạng RESTful API độc lập.

Sau này frontend có thể là:

```text
- React web
- Mobile app
- C# desktop app
- Admin dashboard
```

Miễn là client gọi được REST API thì không cần đổi kiến trúc backend.