
<!-- ========================================================= -->
<!-- FILE 3: VinhKhanhNarration/frontend/README.md            -->
<!-- ========================================================= -->

# VinhKhanhNarration Frontend

Frontend React + TailwindCSS cho đồ án:

**Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**

Frontend chạy dạng mobile web app và gọi backend RESTful API.

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
- Web Speech API
- Vitest
- React Testing Library
```

---
---

## 2. Cài đặt thư viện frontend

Sau khi clone project, vào thư mục frontend và cài dependencies:

```bash
cd frontend
npm install
```

Lệnh `npm install` sẽ tự đọc file `package.json` và cài toàn bộ thư viện cần thiết.

Nếu cần cài lại đầy đủ các thư viện chính của frontend, dùng các lệnh sau:

```bash
npm install react react-dom react-router-dom axios leaflet react-leaflet html5-qrcode lucide-react
npm install -D vite typescript @vitejs/plugin-react tailwindcss postcss autoprefixer
npm install -D @types/react @types/react-dom @types/leaflet
```

Cài thư viện test frontend:

```bash
npm install -D vitest jsdom @testing-library/react @testing-library/jest-dom @testing-library/user-event @vitest/coverage-v8
```

Nếu project chưa có Tailwind config thì chạy:

```bash
npx tailwindcss init -p
```

Nếu đã có hai file sau thì không cần chạy lại lệnh trên:

```text
tailwind.config.js
postcss.config.js
```

Scripts cần có trong `package.json`:

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

## 3. Lưu ý VS Code với TailwindCSS

Nếu VS Code báo lỗi:

```text
Unknown at rule @tailwind
```

thì đây thường là cảnh báo của CSS linter, không phải lỗi chạy app.

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
## 2. Scope màn hình

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
9. Admin Login
10. Admin Dashboard
11. Lookup Management
12. Language Management
13. Places / POI Management
14. Dishes Management
15. Narration Management
16. Translation Management
17. Audio Management
18. QR Code Management
19. Feedback Management
20. Listening Histories
21. Geofence Events
```

---

## 3. Cấu trúc frontend

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

## 4. Cấu hình `.env`

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

## 5. Chạy frontend

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

## 6. Backend cần chạy trước

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
- Backend đã chạy chưa
- VITE_API_BASE_URL trong frontend/.env đúng chưa
- Backend đã bật CORS cho http://localhost:5173 chưa
```

---

## 7. Test frontend

Cài dependencies:

```bash
cd frontend
npm install
```

Chạy test:

```bash
npm run test:run
```

Chạy test dạng watch mode:

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

## 8. Luồng public chính

```text
Mở web app
-> tạo GuestSession
-> chọn ngôn ngữ
-> vào bản đồ
-> bấm marker quán
-> mở bottom sheet chi tiết quán
-> bấm nút Nghe
-> mở Narration Player
-> nếu backend có audioUrl thì phát audio file
-> nếu không có audioUrl thì dùng Web Speech API đọc translatedText
-> ghi ListeningHistory
-> khách có thể gửi Feedback
```

---

## 9. Geofence realtime flow

Trong màn hình `Map Explore`:

```text
- Bản đồ mặc định focus ở phố ẩm thực Vĩnh Khánh.
- Nút lấy vị trí xin quyền geolocation.
- Khi bật theo dõi vị trí, frontend gửi vị trí lên backend theo interval.
- Backend trả shouldPlay = true thì frontend mở Narration Player.
```

---

## 10. Text-To-Speech

Project dùng Web Speech API để đọc text khi chưa có file audio thật.

Logic:

```text
Nếu backend trả audioUrl:
- Phát audio file.

Nếu backend không có audioUrl:
- Đọc translatedText bằng speechSynthesis.speak().
```

---

## 11. Lỗi thường gặp

### Không gọi được backend

Kiểm tra:

```text
Backend đã chạy chưa
http://localhost:5151/swagger có mở được không
frontend/.env có VITE_API_BASE_URL đúng chưa
Backend đã bật CORS chưa
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

---

## 12. File không nên commit

```text
node_modules/
dist/
.env
``` 