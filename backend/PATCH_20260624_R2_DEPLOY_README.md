# Patch: R2 audio storage and production deployment

Patch này chuyển audio production từ filesystem tạm của Render sang Cloudflare R2, đồng thời giữ LocalAudioStorage cho local development.

## Thay đổi chính

- Thêm `R2AudioStorage` sử dụng S3-compatible API.
- MP3 được upload private vào R2.
- Neon chỉ lưu `storage_key`, không lưu binary MP3 hoặc presigned URL.
- Guest/Admin/Vendor vẫn gọi endpoint backend có kiểm tra quyền.
- Backend tạo presigned GET URL ngắn hạn rồi redirect tới R2.
- Dockerfile Render cài Python + `edge-tts` cùng ASP.NET Core.
- Thêm forwarded headers để URL tạo trên Render dùng HTTPS.
- Thêm `/health` cho Render health check.
- Thêm Blueprint `render.yaml`.
- Thêm hướng dẫn deploy Render + Neon + Vercel + R2.
- Thêm script tạo lại QR Mock cố định theo URL Vercel.

## Migration bắt buộc

```text
backend/data/migrations/20260624_r2_audio_storage.sql
```

## Local

Giữ:

```env
Storage__Provider=Local
```

## Render

Đặt:

```env
Storage__Provider=R2
```

và cấu hình đầy đủ `R2__...` theo file:

```text
backend/DEPLOY_RENDER_NEON_R2_VERCEL.md
```

## Audio local cũ

File local cũ không có trên Render. Sau khi deploy, dùng Admin → Thuyết minh → Đồng bộ dịch & audio hoặc Admin → Âm thanh → Thử lại để sinh lại và upload lên R2.
