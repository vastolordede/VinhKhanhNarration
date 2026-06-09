<!-- ========================================================= -->

<!-- FILE 3: VinhKhanhNarration/frontend/README.md            -->

<!-- ========================================================= -->

# VinhKhanhNarration Frontend

Frontend React + TypeScript cho đồ án:

**Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**

Frontend gồm hai phần:

```text
Public mobile web app:
- Dành cho khách tham quan.
- Sử dụng bản đồ, QR code, geofence và narration player.

Admin web app:
- Dành cho người quản trị.
- Quản lý dữ liệu địa điểm, món ăn, thuyết minh, bản dịch, QR code, feedback và lịch sử.
```

---

## 1. Công nghệ

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

---

## 2. Cài đặt dependencies

Sau khi clone project, vào thư mục frontend:

```bash
cd frontend
npm install
```

Lệnh `npm install` sẽ đọc `package.json` và cài toàn bộ thư viện cần thiết.

Một số thư viện chính:

```text
react
react-dom
react-router-dom
axios
leaflet
react-leaflet
html5-qrcode
qrcode
lucide-react
```

Một số thư viện dev/test:

```text
vite
typescript
@vitejs/plugin-react
tailwindcss
postcss
autoprefixer
vitest
jsdom
@testing-library/react
@testing-library/jest-dom
@testing-library/user-event
@vitest/coverage-v8
```

Nếu cài thiếu thư viện QR, chạy:

```bash
npm install qrcode
npm install -D @types/qrcode
```

---

## 3. Scripts

Các script chính trong `package.json`:

```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "preview": "vite preview",
    "test": "vitest",
    "test:run": "vitest run",
    "test:coverage": "vitest run --coverage"
  }
}
```

---

## 4. Lưu ý VS Code với TailwindCSS

Nếu VS Code báo:

```text
Unknown at rule @tailwind
```

đây thường là cảnh báo của CSS linter, không phải lỗi chạy app.

Có thể tạo file:

```text
.vscode/settings.json
```

ở root project với nội dung:

```json
{
  "css.lint.unknownAtRules": "ignore",
  "scss.lint.unknownAtRules": "ignore",
  "less.lint.unknownAtRules": "ignore",
  "files.associations": {
    "*.css": "tailwindcss"
  }
}
```

Nên cài thêm extension VS Code:

```text
Tailwind CSS IntelliSense
PostCSS Language Support
```

---

## 5. Scope màn hình

### Public Mobile Web

```text
1. Splash / Init Session
2. Language Selection
3. Map Explore
4. Place Detail Bottom Sheet
5. Narration Player
6. QR Scanner
7. Feedback Modal
8. Settings / Change Language
```

### Admin Web

```text
1. Admin Login
2. Admin Dashboard
3. Lookup Management
4. Language Management
5. Places / POI Management
6. Dishes Management
7. Narration Management
8. Translation Management
9. Audio Management
10. QR Code Management
11. Feedback Management
12. Listening Histories
13. Geofence Events
```

---

## 6. Cấu trúc frontend

```text
frontend/
│
├── src/
│   ├── api/
│   ├── components/
│   ├── contexts/
│   ├── features/
│   │   ├── admin/
│   │   └── public/
│   ├── hooks/
│   ├── i18n/
│   ├── test/
│   ├── types/
│   ├── utils/
│   ├── App.tsx
│   ├── main.tsx
│   └── styles.css
│
├── .env.example
├── package.json
├── postcss.config.js
├── tailwind.config.js
├── tsconfig.json
├── tsconfig.app.json
├── vite.config.ts
├── vitest.config.ts
└── README.md
```

---

## 7. Cấu hình `.env`

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

Nếu backend chạy port khác, sửa:

```env
VITE_API_BASE_URL=http://localhost:5151
```

---

## 8. Chạy frontend

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

## 9. Backend cần chạy trước

Frontend cần backend đang chạy tại:

```text
http://localhost:5151
```

Kiểm tra backend bằng Swagger:

```text
http://localhost:5151/swagger
```

Nếu frontend báo lỗi gọi API, kiểm tra:

```text
- Backend đã chạy chưa.
- VITE_API_BASE_URL trong frontend/.env đúng chưa.
- Backend đã bật CORS cho http://localhost:5173 chưa.
```

---

## 10. Public app flow

Luồng chính của khách:

```text
Mở web app
-> tạo GuestSession
-> chọn ngôn ngữ
-> vào bản đồ
-> bấm marker địa điểm
-> xem thông tin địa điểm và món ăn
-> bấm nghe thuyết minh
-> mở Narration Player
-> nếu backend có audioUrl thì phát audio file
-> nếu không có audioUrl thì dùng Web Speech API đọc translatedText
-> ghi ListeningHistory
-> khách có thể gửi Feedback
```

---

## 11. QR flow

Public QR flow:

```text
Vào màn QR
-> quét QR bằng camera hoặc nhập QR code thủ công
-> frontend gửi QR Code Value lên backend
-> backend resolve QR
-> frontend mở đúng địa điểm / món ăn / narration tương ứng
```

Admin QR flow:

```text
Admin tạo QR Code
-> chọn Target Type
-> chọn Place / Dish / Narration tương ứng
-> bấm Download QR
-> frontend tự generate QR PNG từ QR Code Value
-> tải file PNG về máy để in hoặc demo
```

QR PNG được tạo trực tiếp ở frontend bằng thư viện `qrcode`, không cần dịch vụ trả phí và không cần lưu ảnh lên server.

---

## 12. Geofence realtime flow

Trong màn hình `Map Explore`:

```text
- Bản đồ mặc định focus ở khu vực phố ẩm thực Vĩnh Khánh.
- Nút lấy vị trí xin quyền geolocation.
- Khi bật theo dõi vị trí, frontend gửi vị trí lên backend theo interval.
- Backend kiểm tra POI/geofence.
- Nếu backend trả shouldPlay = true, frontend mở Narration Player.
```

---

## 13. Text-To-Speech

Project dùng Web Speech API để đọc text khi chưa có file audio thật.

Logic:

```text
Nếu backend trả audioUrl:
- Phát audio file.

Nếu backend không có audioUrl:
- Đọc translatedText bằng speechSynthesis.speak().
```

---

## 14. Đa ngôn ngữ

Public app hỗ trợ nhiều ngôn ngữ:

```text
vi
en
ja
ko
zh
```

Admin app tách riêng ngôn ngữ UI và chỉ dùng:

```text
vi
en
```

Lý do tách riêng:

```text
- Public language là ngôn ngữ nghe của khách.
- Admin UI language là ngôn ngữ giao diện quản trị.
- Không để public chọn Japanese/Korean/Chinese rồi làm admin bị đổi ngôn ngữ theo.
```

---

## 15. Admin UX

Admin form đã hỗ trợ:

```text
- Dropdown thay cho nhập ID thủ công.
- Required field.
- Field error hiển thị dưới input.
- Border đỏ ở field lỗi.
- Tự focus vào field lỗi đầu tiên.
- Success message khi tạo/sửa thành công.
- Field createdBy/reviewedBy được xử lý tự động khi phù hợp.
```

Một số field được đổi sang dropdown:

```text
contentTypeId
placeId
dishId
categoryId
placeTypeId
triggerModeId
narrationId
languageId
translationSourceId
translationId
targetTypeId
```

---

## 16. Test frontend

Cài dependencies:

```bash
cd frontend
npm install
```

Chạy test:

```bash
npm run test:run
```

Chạy watch mode:

```bash
npm test
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

## 17. Build frontend

```bash
npm run build
```

Nếu build thành công nhưng Vite cảnh báo bundle lớn hơn 500 kB, đây là warning tối ưu bundle, không phải lỗi build.

Có thể bỏ qua trong scope demo hiện tại.

---

## 18. Lỗi thường gặp

### Không gọi được backend

Kiểm tra:

```text
- Backend đã chạy chưa.
- http://localhost:5151/swagger có mở được không.
- frontend/.env có VITE_API_BASE_URL đúng chưa.
- Backend đã bật CORS chưa.
```

### Thiếu node_modules

Chạy:

```bash
cd frontend
npm install
```

### Test lỗi thiếu package

Chạy:

```bash
cd frontend
npm install
```

### TypeScript báo moduleResolution node10 deprecated

Trong `frontend/tsconfig.app.json`, dùng:

```json
"moduleResolution": "Bundler"
```

### Camera QR không chạy

Kiểm tra:

```text
- Trình duyệt có cấp quyền camera chưa.
- Đang chạy bằng localhost hoặc HTTPS.
- Nếu camera không dùng được, có thể nhập QR code thủ công.
```

---

## 19. File không nên commit

```text
node_modules/
dist/
.env
```

---

## 20. Checklist demo frontend

Trước khi demo, nên test:

```text
1. Public chọn ngôn ngữ.
2. Public xem map.
3. Public mở narration player.
4. Public quét hoặc nhập QR.
5. Public gửi feedback.
6. Admin đăng nhập.
7. Admin tạo narration.
8. Admin tạo QR code và tải QR PNG.
9. Admin xem feedback và approve/reject.
```
