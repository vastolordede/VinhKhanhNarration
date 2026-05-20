




<!-- ========================================================= -->
<!-- FILE 2: VinhKhanhNarration/backend/README.md             -->
<!-- ========================================================= -->

# VinhKhanhNarration Backend

Backend RESTful API C# ASP.NET Core cho đồ án:

**Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**

Backend cung cấp API cho frontend và các client khác sử dụng.

---

## 1. Kiến trúc backend

```text
Controller / REST API Layer
    -> BUS Layer
        -> DAO Layer
            -> DTO Layer
                -> PostgreSQL
```

```text
Controller:
- Nhận request từ frontend
- Gọi BUS xử lý nghiệp vụ
- Trả response dạng JSON

BUS:
- Xử lý business logic
- Validate dữ liệu
- Điều phối nhiều DAO nếu cần

DAO:
- Truy vấn PostgreSQL
- Thực hiện INSERT, UPDATE, DELETE, SELECT

DTO:
- Object truyền dữ liệu giữa các layer
```

---

## 2. Công nghệ

```text
- ASP.NET Core Web API
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

Sau đó chạy script tạo bảng trong:

```text
backend/data/
```

Nếu chỉ muốn chạy demo giao diện nhanh, chạy thêm seed tối thiểu nếu có:

```text
backend/data/seed_minimal.sql
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

---

## 6. Chạy backend

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

## 7. Swagger

Sau khi chạy backend, mở:

```text
http://localhost:5151/swagger
```

Swagger giúp kiểm tra nhanh các API như:

```text
GET  /api/languages/active
GET  /api/places/active
POST /api/public/guest-sessions
POST /api/public/qr/resolve
POST /api/public/geofence/check
POST /api/public/feedbacks
```

---

## 8. CORS cho frontend

Frontend chạy ở:

```text
http://localhost:5173
```

Trong `Program.cs`, CORS cần cho phép origin này.

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

Và sau `var app = builder.Build();`, trước `app.MapControllers();` cần có:

```csharp
app.UseCors("FrontendDev");
```

---

## 9. Lưu ý cho integration test

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

## 10. Test backend

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
- PostgreSQL đang chạy
- backend/.env đúng
- database đã tạo
- seed data tối thiểu đã chạy
```

---

## 11. API chính

```text
Auth:
POST /api/auth/login

Languages:
GET /api/languages/active

Places:
GET /api/places/active
GET /api/places/{id}

Dishes:
GET /api/dishes/active
GET /api/place-dishes/place/{placeId}

Narrations:
GET /api/narration-contents/place/{placeId}
GET /api/narration-translations/narration/{narrationId}/language/{languageId}
GET /api/audio-files/playable?narrationId=&languageId=

Guest:
POST  /api/public/guest-sessions
PATCH /api/public/guest-sessions/{guestSessionId}/language

QR:
POST /api/public/qr/resolve

Geofence:
POST /api/public/geofence/check

Listening:
POST /api/public/listening-histories

Feedback:
POST  /api/public/feedbacks
GET   /api/admin/feedbacks
PATCH /api/admin/feedbacks/{feedbackId}/approve
PATCH /api/admin/feedbacks/{feedbackId}/reject
```

---

## 12. Lỗi thường gặp

### Frontend báo CORS

Kiểm tra:

```text
Program.cs đã AddCors chưa
Program.cs đã app.UseCors("FrontendDev") chưa
UseCors có đặt trước MapControllers không
Frontend có chạy đúng http://localhost:5173 không
```

### Backend không kết nối DB

Kiểm tra:

```text
PostgreSQL đã chạy chưa
backend/.env đúng chưa
DB_NAME, DB_USER, DB_PASSWORD đúng chưa
Database vinh_khanh_narration_db đã tạo chưa
```

### Test backend lỗi Program inaccessible

Kiểm tra cuối `Program.cs` có:

```csharp
public partial class Program { }
```




