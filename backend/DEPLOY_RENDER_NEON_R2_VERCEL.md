# Deploy Vĩnh Khánh Narration: Render + Neon + Vercel + Cloudflare R2

## Kiến trúc

- **Vercel**: React/Vite frontend.
- **Render Docker Web Service**: ASP.NET Core 8, Python và Edge TTS.
- **Neon**: PostgreSQL metadata.
- **Cloudflare R2**: file MP3 private.

File MP3 không được lưu trong Neon. Bảng `audio_files` chỉ lưu `storage_key` và metadata. Khi khách có Access Pass gọi endpoint audio, backend tạo presigned URL R2 ngắn hạn rồi redirect trình duyệt tới file MP3.

## 1. Chạy migration Neon

Chạy các migration cũ theo thứ tự của dự án, sau đó chạy thêm:

```text
backend/data/migrations/20260624_r2_audio_storage.sql
```

Migration thêm cột:

```text
audio_files.storage_key
```

Nên dùng **direct connection string** của Neon khi chạy migration. Backend Render có thể dùng pooled connection string.

## 2. Tạo Cloudflare R2

1. Vào Cloudflare Dashboard → R2 Object Storage.
2. Tạo bucket private, ví dụ `vinhkhanh-audio`.
3. Tạo R2 API token có quyền đọc và ghi object trong bucket.
4. Lưu lại:
   - Account ID
   - Access Key ID
   - Secret Access Key
   - S3 endpoint
5. Không bật public bucket.

### CORS bucket

Mở bucket → Settings → CORS Policy và dùng file:

```text
backend/deploy/r2-cors.example.json
```

Thay `YOUR-FRONTEND.vercel.app` bằng domain Vercel thật. CORS cần thiết vì màn hình Admin tải audio qua presigned URL trong trình duyệt.

## 3. Deploy backend trên Render

Có thể dùng Blueprint `backend/render.yaml` (chọn custom Blueprint path trên Render) hoặc tạo **Docker Web Service** thủ công.

Thiết lập thủ công:

- Root Directory: `backend`
- Dockerfile Path: `./Dockerfile`
- Health Check Path: `/health`

Dockerfile đã cài Python, tạo virtual environment `/opt/edge-tts` và cài `edge-tts` trong image.

### Render environment variables

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000

DATABASE_URL=<NEON_POOLED_CONNECTION_STRING>

Jwt__SecretKey=<RANDOM_SECRET_AT_LEAST_32_CHARACTERS>
Jwt__Issuer=VinhKhanhNarration
Jwt__Audience=VinhKhanhNarrationClient

FRONTEND_URL=https://YOUR-FRONTEND.vercel.app
CORS_ALLOWED_ORIGINS=https://YOUR-FRONTEND.vercel.app

Translation__Provider=GoogleFree

Speech__Provider=EdgeTTS
EdgeTts__PythonPath=/opt/edge-tts/bin/python
EdgeTts__ScriptPath=/app/tools/edge_tts_synthesize.py
EdgeTts__Rate=+0%
EdgeTts__Volume=+0%
EdgeTts__Pitch=+0Hz
EdgeTts__TimeoutSeconds=120

Storage__Provider=R2
R2__AccountId=<CLOUDFLARE_ACCOUNT_ID>
R2__AccessKeyId=<R2_ACCESS_KEY_ID>
R2__SecretAccessKey=<R2_SECRET_ACCESS_KEY>
R2__BucketName=vinhkhanh-audio
R2__Endpoint=https://<ACCOUNT_ID>.r2.cloudflarestorage.com
R2__Prefix=audio
R2__SignedUrlMinutes=5

Payment__Provider=Mock
GuestAccess__Price=50000
GuestAccess__DurationHours=24
GuestAccess__OrderExpiryMinutes=30
Lifecycle__IntervalMinutes=15
Lifecycle__GeofenceRetentionDays=90
```

Không thêm dấu ngoặc kép quanh giá trị trên Render.

## 4. Deploy frontend trên Vercel

- Root Directory: `frontend`
- Framework Preset: Vite
- Build Command: `npm run build`
- Output Directory: `dist`

Environment variable:

```env
VITE_API_BASE_URL=https://YOUR-BACKEND.onrender.com
```

Sau khi đổi environment variable phải redeploy frontend.

## 5. QR Mock cố định

Ảnh QR vẫn là file cố định:

```text
frontend/public/mock-payment-qr.svg
```

Nếu muốn khách quét QR để mở ứng dụng thật, tạo lại file này một lần để nó chứa:

```text
https://YOUR-FRONTEND.vercel.app/app/access
```

QR không chứa tài khoản ngân hàng, VietQR hoặc order code. Mỗi lần khách mở trang vẫn tạo một Mock Payment Order mới theo luồng hiện tại.

Tạo lại QR cố định sau khi biết domain Vercel:

```powershell
cd frontend
py -m pip install -r scripts/requirements-qr.txt
py scripts/generate_fixed_mock_qr.py https://YOUR-FRONTEND.vercel.app/app/access
```

Commit file `frontend/public/mock-payment-qr.svg` vừa được tạo rồi redeploy Vercel.

## 6. Audio cũ tạo ở local

Các record cũ chỉ có URL dạng `/generated-audio/...` không tồn tại trên Render sau deploy. Sau khi migration và cấu hình R2:

1. Vào Admin → Âm thanh.
2. Bấm **Thử lại** hoặc Admin → Thuyết minh → **Đồng bộ dịch & audio**.
3. Audio mới sẽ được upload lên R2 và có `storage_key`.

Kiểm tra:

```sql
SELECT
    audio_id,
    translation_id,
    provider,
    status,
    storage_key,
    audio_url,
    error_message
FROM public.audio_files
ORDER BY audio_id DESC;
```

Record production đúng có dạng:

```text
provider = EdgeTTS
status = Ready
storage_key = audio/2026/06/<uuid>-narration-....mp3
audio_url = NULL
```

## 7. Test production

1. Mở `https://YOUR-BACKEND.onrender.com/health`.
2. Đăng nhập Admin trên Vercel.
3. Tạo hoặc duyệt narration mới.
4. Xác nhận 5 bản dịch và 5 audio Ready.
5. Kiểm tra bucket R2 có 5 MP3.
6. Mua Guest Access Pass bằng Mock Payment.
7. Đổi ngôn ngữ và nghe audio.
8. Trong Network, endpoint `/api/public/audio/...` phải trả redirect tới presigned R2 URL.

## Bảo mật

- Bucket R2 giữ private.
- Không lưu presigned URL trong Neon.
- Không commit `.env`.
- Không đưa R2 Secret Access Key vào Vercel/frontend.
- Chỉ Render backend được giữ R2 credentials.
