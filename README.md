<!-- ========================================================= -->

<!-- FILE 1: VinhKhanhNarration/README.md                     -->

<!-- ========================================================= -->

# VinhKhanhNarration

**VinhKhanhNarration** là hệ thống thuyết minh thông minh đa ngôn ngữ cho khu phố ẩm thực Vĩnh Khánh. Dự án hỗ trợ khách tham quan khám phá địa điểm, món ăn và nội dung thuyết minh thông qua bản đồ, QR code, định vị geofence và trình phát thuyết minh đa ngôn ngữ.

Hệ thống gồm hai phần chính:

```text
backend/    ASP.NET Core RESTful API
frontend/   React + Vite + TypeScript mobile web app
tests/      Backend automated tests
```

Backend xử lý nghiệp vụ, xác thực admin, kết nối PostgreSQL và cung cấp RESTful API. Frontend gồm public mobile web app cho khách tham quan và admin web app để quản lý dữ liệu hệ thống.

---

## 1. Mục tiêu dự án

Dự án được xây dựng nhằm hỗ trợ trải nghiệm khám phá phố ẩm thực Vĩnh Khánh theo hướng hiện đại và cá nhân hóa hơn.

Các mục tiêu chính:

```text
- Cung cấp thuyết minh đa ngôn ngữ cho địa điểm và món ăn.
- Cho phép khách sử dụng bản đồ, QR code hoặc vị trí hiện tại để mở nội dung phù hợp.
- Hỗ trợ phát thuyết minh bằng audio file hoặc Web Speech API.
- Ghi nhận lịch sử nghe và feedback của khách.
- Cung cấp trang quản trị để quản lý dữ liệu địa điểm, món ăn, bản dịch, QR code và feedback.
```

---

## 2. Chức năng chính

### Public app

```text
- Khởi tạo phiên khách anonymous.
- Chọn ngôn ngữ nghe.
- Xem bản đồ khu vực Vĩnh Khánh.
- Xem địa điểm, món ăn và nội dung gợi ý.
- Quét QR code hoặc nhập QR code thủ công.
- Phát thuyết minh theo ngôn ngữ đã chọn.
- Dùng audio file nếu có, hoặc fallback sang Web Speech API.
- Gửi feedback sau khi trải nghiệm.
- Bật theo dõi vị trí để kiểm tra geofence và gợi ý nội dung phù hợp.
```

### Admin app

```text
- Đăng nhập admin.
- Quản lý dashboard.
- Quản lý bảng danh mục / lookup.
- Quản lý ngôn ngữ.
- Quản lý địa điểm / POI / geofence.
- Quản lý danh mục món ăn và món ăn.
- Quản lý nội dung thuyết minh.
- Quản lý bản dịch đa ngôn ngữ.
- Quản lý audio file.
- Quản lý QR code và tải QR PNG miễn phí từ frontend.
- Quản lý feedback.
- Xem lịch sử nghe.
- Xem sự kiện geofence.
```

---

## 3. Công nghệ sử dụng

### Backend

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

### Frontend

```text
- React
- Vite
- TypeScript
- TailwindCSS
- React Router DOM
- Axios
- Leaflet / React Leaflet
- html5-qrcode
- qrcode
- Web Speech API
- Vitest
- React Testing Library
```

### Database

```text
- PostgreSQL
- Foreign key constraints
- CHECK constraints
- Trigger validation cho QR code và narration target logic
```

---

## 4. Cấu trúc project

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
│   ├── appsettings.Development.json
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

## 5. Yêu cầu cài đặt

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

## 6. Clone project

```bash
git clone <repository-url>
cd VinhKhanhNarration
```

---

## 7. Hướng dẫn cấu hình

Project có 3 README chính:

```text
README.md              Tổng quan project
backend/README.md      Hướng dẫn cấu hình và chạy backend
frontend/README.md     Hướng dẫn cấu hình và chạy frontend
```

Cấu hình database, file `.env`, port backend, Swagger và test backend được mô tả chi tiết trong:

```text
backend/README.md
```

Cấu hình Vite, TailwindCSS, API base URL, test frontend và các màn public/admin được mô tả chi tiết trong:

```text
frontend/README.md
```

---

## 8. Chạy nhanh toàn bộ project

### Terminal 1: chạy backend

```bash
cd backend
dotnet restore
dotnet run
```

Backend mặc định chạy tại:

```text
http://localhost:5151
```

Swagger:

```text
http://localhost:5151/swagger
```

### Terminal 2: chạy frontend

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

## 9. Tài khoản admin mẫu

Nếu database đã được seed tài khoản admin mẫu, có thể đăng nhập bằng:

```text
Email: admin@vinhkhanh.local
Password: Admin@123
```

Lưu ý: mật khẩu thực tế phụ thuộc vào dữ liệu seed hoặc dữ liệu hiện tại trong database.

---

## 10. Validation và bảo vệ dữ liệu

Project sử dụng nhiều lớp kiểm tra dữ liệu:

```text
Frontend validation:
- Required field.
- Field error dưới input.
- Border đỏ khi lỗi.
- Tự focus vào field lỗi đầu tiên.
- Success message khi tạo hoặc cập nhật thành công.

Backend validation:
- Validate nghiệp vụ trong BUS layer.
- Trả lỗi rõ ràng dạng message và fieldErrors.

Database validation:
- Foreign key constraints.
- CHECK constraints.
- Trigger chặn sai logic QR code và narration target.
```

Một số logic quan trọng được kiểm tra ở nhiều lớp:

```text
Narration:
- Place Narration phải có placeId và không có dishId.
- Dish Narration phải có dishId và không có placeId.
- General Narration không được có placeId hoặc dishId.

QR Code:
- Place QR phải trỏ tới placeId.
- Dish QR phải trỏ tới dishId.
- Narration QR phải trỏ tới narrationId.
```

---

## 11. File không nên commit

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

## 12. Ghi chú demo

Khi demo, nên kiểm tra các flow chính:

```text
1. Admin tạo nội dung thuyết minh.
2. Admin tạo QR code và tải QR PNG.
3. Public nhập hoặc scan QR để mở đúng narration.
4. Public gửi feedback.
5. Admin xem và duyệt feedback.
6. Public bật vị trí để kiểm tra geofence.
```

---

## 13. Trạng thái hiện tại

Project hiện đã hoàn thiện các chức năng chính cho demo:

```text
- Public mobile web app.
- Admin management web app.
- Đa ngôn ngữ public.
- Tách riêng admin UI language.
- QR code flow.
- Feedback flow.
- Validation FE / BE / DB.
- QR PNG download miễn phí trên frontend.
```
