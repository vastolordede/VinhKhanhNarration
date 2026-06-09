<!-- ========================================================= -->

<!-- FILE 2: VinhKhanhNarration/backend/README.md             -->

<!-- ========================================================= -->

# VinhKhanhNarration Backend

Backend RESTful API cho đồ án:

**Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**

Backend được xây dựng bằng **ASP.NET Core Web API** và kết nối với **PostgreSQL**. Backend cung cấp API cho public mobile web app và admin web app.

---

## 1. Kiến trúc backend

Backend được tổ chức theo mô hình nhiều lớp:

```text
Controller / REST API Layer
    -> BUS Layer
        -> DAO Layer
            -> PostgreSQL
```

Vai trò từng lớp:

```text
Controller:
- Nhận HTTP request từ frontend.
- Gọi BUS để xử lý nghiệp vụ.
- Trả response JSON cho client.

BUS:
- Xử lý business logic.
- Validate dữ liệu.
- Điều phối nhiều DAO khi cần.
- Trả lỗi nghiệp vụ rõ ràng cho Controller.

DAO:
- Truy vấn PostgreSQL.
- Thực hiện SELECT, INSERT, UPDATE, DELETE.

DTO:
- Object truyền dữ liệu giữa các layer.
- Chuẩn hóa request và response data.
```

---

## 2. Công nghệ

```text
- ASP.NET Core Web API
- C#
- PostgreSQL
- Npgsql
- Swagger / Swashbuckle
- BCrypt.Net-Next
- xUnit
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing
```

---

## 3. Cấu trúc backend

```text
backend/
│
├── BUS/
├── Controllers/
├── DAO/
├── Database/
├── DTO/
├── Properties/
├── Swagger/
├── Utils/
├── data/
├── .env.example
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── README.md
└── VinhKhanhNarration.Api.csproj
```

---

## 4. Cấu hình database bằng `.env`

Tạo file:

```text
backend/.env
```

Có thể copy từ:

```text
backend/.env.example
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

Không commit file `.env` lên GitHub.

---

## 5. Database

Tên database mặc định:

```text
vinh_khanh_narration_db
```

Tạo database:

```sql
CREATE DATABASE vinh_khanh_narration_db;
```

Sau đó mở đúng database `vinh_khanh_narration_db` trong DBeaver hoặc pgAdmin và chạy script trong thư mục:

```text
backend/data/
```

Tùy project hiện tại, script database có thể gồm:

```text
- Script tạo bảng.
- Script seed dữ liệu lookup.
- Script seed dữ liệu mẫu.
- Script trigger hoặc constraint bổ sung.
```

Các nhóm bảng chính:

```text
admin_users
admin_refresh_tokens

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

---

## 6. Dữ liệu lookup cần có

Để hệ thống chạy ổn, database cần có dữ liệu lookup cơ bản cho các bảng:

```text
languages
place_types
content_types
target_types
translation_sources
trigger_modes
geofence_event_types
geofence_event_statuses
```

Các bảng này được dùng cho dropdown trong admin và validation ở backend/database.

---

## 7. Chạy backend

Tại thư mục backend:

```bash
cd backend
dotnet restore
dotnet run
```

Backend mặc định chạy tại:

```text
http://localhost:5151
```

Swagger dùng để test API:

```text
http://localhost:5151/swagger
```

---

## 8. Swagger

Sau khi chạy backend, mở:

```text
http://localhost:5151/swagger
```

Swagger giúp kiểm tra nhanh các API như:

```text
GET  /api/languages/active
GET  /api/places/active
GET  /api/dishes/active
POST /api/public/guest-sessions
POST /api/public/qr/resolve
POST /api/public/geofence/check
POST /api/public/feedbacks
POST /api/public/listening-histories
```

---

## 9. CORS cho frontend

Frontend development server chạy tại:

```text
http://localhost:5173
```

Trong `Program.cs`, backend cần cho phép origin này.

Ví dụ:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

Sau `var app = builder.Build();`, trước `app.MapControllers();` cần có:

```csharp
app.UseCors("FrontendDev");
```

---

## 10. Public API chính

```text
Languages:
GET    /api/languages/active

Places:
GET    /api/places/active
GET    /api/places/{id}

Dishes:
GET    /api/dishes/active
GET    /api/place-dishes/place/{placeId}

Narrations:
GET    /api/narration-contents/place/{placeId}
GET    /api/narration-translations/narration/{narrationId}/language/{languageId}
GET    /api/audio-files/playable?narrationId=&languageId=

Guest Session:
POST   /api/public/guest-sessions
PATCH  /api/public/guest-sessions/{guestSessionId}/language

QR:
POST   /api/public/qr/resolve

Geofence:
POST   /api/public/geofence/check

Listening:
POST   /api/public/listening-histories

Feedback:
POST   /api/public/feedbacks
```

---

## 11. Admin API chính

```text
Auth:
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout

Feedback:
GET    /api/admin/feedbacks
PATCH  /api/admin/feedbacks/{feedbackId}/approve
PATCH  /api/admin/feedbacks/{feedbackId}/reject

CRUD Management:
GET/POST/PUT/PATCH cho các nhóm dữ liệu:
- lookup
- languages
- places
- dishes
- narration contents
- narration translations
- audio files
- qr codes
- listening histories
- geofence events
```

Tên endpoint cụ thể có thể kiểm tra trực tiếp trong Swagger.

---

## 12. Validation backend

Backend có validation ở BUS layer để bảo vệ nghiệp vụ trước khi ghi dữ liệu xuống database.

Một số validation quan trọng:

```text
Narration:
- Title không được rỗng.
- Original Text không được rỗng.
- Content Type bắt buộc hợp lệ.
- Created By Admin bắt buộc hợp lệ.
- Place Narration phải có placeId và không có dishId.
- Dish Narration phải có dishId và không có placeId.
- General Narration không được có placeId hoặc dishId.

Translation:
- Narration bắt buộc.
- Language bắt buộc.
- Translated Title bắt buộc.
- Translated Text bắt buộc.
- Translation Source bắt buộc.

Audio:
- Translation bắt buộc.
- Audio URL bắt buộc nếu tạo audio file.
- Duration không được âm.

QR Code:
- QR Code Value bắt buộc.
- Target Type bắt buộc.
- Place QR chỉ được trỏ tới placeId.
- Dish QR chỉ được trỏ tới dishId.
- Narration QR chỉ được trỏ tới narrationId.

Place / Dish:
- Tên địa điểm, tên món ăn và danh mục bắt buộc.
- Giá trị số như radius, priority, debounce, cooldown không được sai logic.
```

Khi validation lỗi, backend trả response rõ ràng dạng:

```json
{
  "success": false,
  "message": "Validation failed.",
  "fieldErrors": {
    "title": "Title is required.",
    "contentTypeId": "Content Type is required."
  }
}
```

Frontend có thể đọc `fieldErrors` để hiển thị lỗi ngay dưới input tương ứng.

---

## 13. Database validation

Ngoài backend validation, database cũng có lớp bảo vệ cuối cùng:

```text
- Foreign key constraints.
- CHECK constraints.
- Trigger validation.
```

Logic quan trọng được chặn ở database:

```text
Narration target:
- Place Narration requires place_id only.
- Dish Narration requires dish_id only.
- General Narration must not target place or dish.

QR target:
- Place QR requires place_id only.
- Dish QR requires dish_id only.
- Narration QR requires narration_id only.
```

Điều này giúp tránh dữ liệu sai kể cả khi request không đi qua frontend.

---

## 14. Lưu ý cho integration test

Cuối file:

```text
backend/Program.cs
```

cần có:

```csharp
public partial class Program { }
```

Ví dụ:

```csharp
app.MapControllers();

app.Run();

public partial class Program { }
```

Dòng này dùng cho `Microsoft.AspNetCore.Mvc.Testing`.

---

## 15. Test backend

Từ root project:

```bash
dotnet test tests/VinhKhanhNarration.Api.Tests/VinhKhanhNarration.Api.Tests.csproj
```

Backend test gồm:

```text
Unit tests:
- PasswordHasherTests
- SessionGeneratorTests
- GeoDistanceCalculatorTests

Integration tests:
- ApiReadSmokeTests
- AdminAndLogReadSmokeTests
- PublicFlowSmokeTests
```

Integration test cần:

```text
- PostgreSQL đang chạy.
- backend/.env đúng.
- Database đã tạo.
- Seed data tối thiểu đã chạy.
```

---

## 16. Lỗi thường gặp

### Frontend báo CORS

Kiểm tra:

```text
- Program.cs đã AddCors chưa.
- Program.cs đã app.UseCors("FrontendDev") chưa.
- UseCors có đặt trước MapControllers không.
- Frontend có chạy đúng http://localhost:5173 không.
```

### Backend không kết nối DB

Kiểm tra:

```text
- PostgreSQL đã chạy chưa.
- backend/.env đúng chưa.
- DB_NAME, DB_USER, DB_PASSWORD đúng chưa.
- Database vinh_khanh_narration_db đã tạo chưa.
```

### Login admin thất bại

Kiểm tra:

```text
- Email admin có tồn tại trong admin_users không.
- is_active có bằng TRUE không.
- password_hash có đúng bcrypt hash không.
- Backend đang kết nối đúng database chưa.
```

### Test backend lỗi Program inaccessible

Kiểm tra cuối `Program.cs` có:

```csharp
public partial class Program { }
```

---

## 17. File không nên commit

```text
.env
bin/
obj/
```
