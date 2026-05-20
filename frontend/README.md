# VinhKhanhNarration Frontend

React + TailwindCSS mobile web app cho đồ án **Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh**.

Frontend này được thiết kế để chạy cùng backend RESTful API ASP.NET Core.

## 1. Scope màn hình

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

## 2. Công nghệ

```text
React
Vite
TypeScript
TailwindCSS
React Router DOM
Axios
Leaflet / React Leaflet
html5-qrcode
Web Speech API cho text-to-speech
```

## 3. Cấu hình API

Tạo file `.env` ở thư mục frontend:

```env
VITE_API_BASE_URL=http://localhost:5151
VITE_DEFAULT_MAP_LAT=10.7569
VITE_DEFAULT_MAP_LNG=106.7057
VITE_DEFAULT_MAP_ZOOM=16
VITE_GEOFENCE_INTERVAL_MS=10000
```

Trong đó:

```text
VITE_API_BASE_URL: URL backend ASP.NET Core
VITE_DEFAULT_MAP_LAT/LNG: vị trí mặc định khi mở bản đồ
VITE_DEFAULT_MAP_ZOOM: zoom mặc định
VITE_GEOFENCE_INTERVAL_MS: khoảng thời gian gửi geofence check khi bật theo dõi vị trí
```

## 4. Luồng public chính

```text
Mở web app
→ tạo GuestSession
→ chọn ngôn ngữ
→ vào bản đồ
→ bấm marker quán
→ mở bottom sheet chi tiết quán
→ bấm nút Nghe cạnh tên quán
→ mở Narration Player
→ nếu có audioUrl thì phát audio file
→ nếu không có audioUrl thì dùng Web Speech API đọc translatedText
→ ghi ListeningHistory
→ khách có thể gửi Feedback
```

## 5. Geofence realtime flow

Trong màn hình `Map Explore`:

```text
- Mặc định bản đồ focus ở phố ẩm thực Vĩnh Khánh.
- Nút “Lấy vị trí” xin quyền geolocation và focus theo vị trí khách.
- Nút “Bật theo dõi vị trí” dùng watchPosition để theo dõi realtime.
- Mỗi VITE_GEOFENCE_INTERVAL_MS, frontend gửi vị trí lên backend.
- Backend trả shouldPlay = true thì frontend tự chuyển sang Narration Player.
```

## 6. TTS audio scope

Project dùng **Cách 1: client-side Text-To-Speech bằng Web Speech API**.

Logic:

```text
Nếu backend trả audioUrl:
  phát audio file.

Nếu backend không có audioUrl:
  đọc translatedText bằng speechSynthesis.speak().
```

Như vậy admin không bắt buộc phải upload audio thật cho mọi bài thuyết minh.

## 7. API backend đang được frontend gọi

```text
POST /api/public/guest-sessions
GET  /api/languages/active
PATCH /api/public/guest-sessions/{guestSessionId}/language

GET /api/places/active
GET /api/places/{id}
GET /api/place-dishes/place/{placeId}
GET /api/narration-contents/place/{placeId}
GET /api/narration-contents/dish/{dishId}
GET /api/narration-translations/narration/{narrationId}/language/{languageId}
GET /api/audio-files/playable?narrationId=&languageId=

POST /api/public/qr/resolve
POST /api/public/geofence/check
POST /api/public/listening-histories
POST /api/public/feedbacks
```

Admin pages gọi các endpoint CRUD tương ứng như:

```text
/api/places
/api/dishes
/api/narration-contents
/api/narration-translations
/api/audio-files
/api/qr-codes
/api/admin/feedbacks
/api/admin/listening-histories
/api/admin/geofence-events
```

## 8. Ghi chú

Frontend này tập trung đúng scope PoC:

```text
- Bản đồ là màn hình trung tâm.
- Địa điểm/quán ăn là marker trên bản đồ.
- Chi tiết quán là bottom sheet.
- Nút nghe dùng audioUrl hoặc TTS.
- Geofence realtime nằm ngay trong màn hình bản đồ.
- Admin web dùng các màn hình CRUD đơn giản để nhập dữ liệu.
```
