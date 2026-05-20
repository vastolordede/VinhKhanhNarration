<!-- ========================================================= -->
<!-- FILE 1: VinhKhanhNarration/README.md                     -->
<!-- ========================================================= -->

# VinhKhanhNarration

Dự án **Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**.

Hệ thống gồm:

```text
backend/   ASP.NET Core RESTful API
frontend/  React + Vite + TailwindCSS mobile web app
tests/     Backend automated tests
```

Backend xử lý nghiệp vụ, kết nối PostgreSQL và cung cấp API.  
Frontend là web app chạy trên điện thoại, dùng bản đồ, QR code, geofence và Web Speech API để phát thuyết minh.

---

## 1. Chức năng chính

```text
- Quản lý tài khoản nội bộ
- Quản lý ngôn ngữ
- Quản lý danh mục hệ thống
- Quản lý địa điểm / POI
- Quản lý món ăn
- Quản lý nội dung thuyết minh
- Quản lý bản dịch đa ngôn ngữ
- Quản lý file audio hoặc dùng Text-To-Speech
- Quản lý QR code
- Tạo phiên khách anonymous
- Ghi nhận lịch sử nghe
- Gửi và duyệt feedback
- Kiểm tra geofence và tự động phát thuyết minh theo vị trí
```

---

## 2. Công nghệ sử dụng

```text
Backend:
- ASP.NET Core Web API
- PostgreSQL
- Npgsql
- Swagger / Swashbuckle
- BCrypt.Net-Next
- xUnit
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing

Frontend:
- React
- Vite
- TypeScript
- TailwindCSS
- React Router DOM
- Axios
- Leaflet / React Leaflet
- html5-qrcode
- Web Speech API
- Vitest
- React Testing Library
```

---

## 3. Cấu trúc project

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
│   ├── .env.example
│   ├── appsettings.json
│   ├── Program.cs
│   ├── README.md
│   └── VinhKhanhNarration.Api.csproj
│
├── frontend/
│   ├── src/
│   ├── .env.example
│   ├── package.json
│   ├── vite.config.ts
│   ├── vitest.config.ts
│   └── README.md
│
├── tests/
│   └── VinhKhanhNarration.Api.Tests/
│
└── README.md
```

---

## 4. Yêu cầu cài đặt

Trước khi chạy project, máy cần có:

```text
- .NET SDK
- Node.js LTS
- PostgreSQL
- DBeaver hoặc pgAdmin
```

Kiểm tra nhanh:

```bash
dotnet --version
node -v
npm -v
```

---

## 5. Clone project

```bash
git clone <repository-url>
cd VinhKhanhNarration
```

---

## 6. Setup PostgreSQL

Tạo database:

```sql
CREATE DATABASE vinh_khanh_narration_db;
```

Sau đó mở đúng database `vinh_khanh_narration_db` trong DBeaver hoặc pgAdmin.

Chạy script tạo bảng trong thư mục:

```text
backend/data/
```

Nếu chỉ cần dữ liệu mẫu tối thiểu để frontend hiển thị giao diện, chạy thêm file seed tối thiểu nếu có:

```text
backend/data/seed_minimal.sql
```

Database tối thiểu cần có dữ liệu cho:

```text
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
```

---

## 7. Cấu hình backend

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

## 8. Chạy backend

```bash
cd backend
dotnet restore
dotnet run
```

Backend mặc định chạy tại:

```text
http://localhost:5151
```

Swagger để test API:

```text
http://localhost:5151/swagger
```

---

## 9. Cấu hình frontend

Tạo file:

```text
frontend/.env
```

Có thể copy từ:

```text
frontend/.env.example
```

Nội dung mẫu:

```env
VITE_API_BASE_URL=http://localhost:5151
VITE_DEFAULT_MAP_LAT=10.7569
VITE_DEFAULT_MAP_LNG=106.7057
VITE_DEFAULT_MAP_ZOOM=16
VITE_GEOFENCE_INTERVAL_MS=10000
VITE_APP_NAME=VinhKhanhNarration
VITE_APP_ENV=development
```

---

## 10. Chạy frontend

Mở terminal mới:

```bash
cd frontend
npm install
npm run dev
```

Frontend mặc định chạy tại:

```text
http://localhost:5173
```

---

## 11. Chạy test backend

Từ root project:

```bash
dotnet test tests/VinhKhanhNarration.Api.Tests/VinhKhanhNarration.Api.Tests.csproj
```

Backend test gồm:

```text
Unit tests:
- PasswordHasher
- SessionGenerator
- GeoDistanceCalculator

Integration smoke tests:
- Languages API
- Places API
- Guest session API
- QR resolve API
- Geofence API
- Feedback API
- Listening histories API
```

Integration test cần PostgreSQL đang chạy và database đã có seed tối thiểu.

---

## 12. Chạy test frontend

```bash
cd frontend
npm install
npm run test:run
```

Frontend test gồm:

```text
- useSpeechSynthesis
- useGeolocation
- API helper
- LanguageSelectionScreen
- NarrationPlayerScreen
- FeedbackModal
- QRScannerScreen
- SettingsScreen
```

---

## 13. Chạy toàn bộ project khi demo

Terminal 1:

```bash
cd backend
dotnet run
```

Mở Swagger:

```text
http://localhost:5151/swagger
```

Terminal 2:

```bash
cd frontend
npm run dev
```

Mở frontend:

```text
http://localhost:5173
```

---

## 14. File không nên commit

```text
backend/.env
frontend/.env
backend/bin/
backend/obj/
frontend/node_modules/
frontend/dist/
.vs/
.vscode/
```

---

## 15. File hướng dẫn tạm có thể xóa

Nếu đã cấu hình xong, có thể xóa các file hướng dẫn tạm:

```text
PATCH_BACKEND_PROGRAM.txt
PATCH_BACKEND_PROGRAM.md
PATCH_FRONTEND_PACKAGE_JSON.md
README_TESTS.md
```

Chỉ cần giữ các README chính:

```text
README.md
backend/README.md
frontend/README.md
tests/README.md
```
