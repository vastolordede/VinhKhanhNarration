# Patch 20260624 — Edge TTS Pipeline + Fixed Mock QR

## Mục tiêu

1. Thay Azure Speech mặc định bằng Python `edge-tts` để tạo MP3 cho 5 ngôn ngữ đang bật trong database: `vi`, `en`, `ja`, `ko`, `zh`.
2. Thay Azure Translator mặc định bằng `GoogleFree` để pipeline demo không cần Azure key.
3. Giữ nguyên toàn bộ luồng tạo order, xác nhận Mock Payment, tạo Guest Session/Access Pass và kích hoạt Vendor subscription.
4. Chỉ thay cách hiển thị QR: Guest và Vendor luôn dùng cùng một ảnh `frontend/public/mock-payment-qr.svg`.

## Cài Edge TTS

Mở PowerShell tại thư mục `backend`:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\setup_edge_tts.ps1
```

Script sẽ tạo:

```text
backend/.venv-tts/
```

và cài phiên bản được khóa trong:

```text
backend/tools/requirements-edge-tts.txt
```

## Kiểm tra 5 giọng

```powershell
.\.venv-tts\Scripts\python.exe .\tools\test_edge_tts_5_languages.py
```

Các file thử được tạo trong:

```text
backend/wwwroot/generated-audio/edge-tts-test/
```

## Chạy pipeline

```powershell
dotnet build
dotnet run
```

Sau đó:

1. Vendor tạo narration cho sạp hoặc món.
2. Admin duyệt narration nguồn.
3. Hệ thống tự dịch sang các ngôn ngữ đang bật.
4. Backend gọi Python Edge TTS cho từng `default_voice_id` của bảng `languages`.
5. MP3 được lưu trong `backend/wwwroot/generated-audio`.
6. `audio_files` chuyển sang `Ready` và narration tự publish khi toàn bộ ngôn ngữ hoàn tất.

Các audio cũ đang `Failed` có thể bấm **Thử lại** tại `/admin/audio`.

## Provider mặc định

```text
Translation:Provider = GoogleFree
Speech:Provider = EdgeTTS
```

Có thể quay lại Azure bằng cách đặt:

```env
Translation__Provider=AzureTranslator
Speech__Provider=AzureSpeech
```

và cung cấp key tương ứng.

## Fixed Mock QR

Ảnh dùng chung:

```text
frontend/public/mock-payment-qr.svg
```

Muốn đổi hình QR về sau chỉ cần thay đúng file trên, giữ nguyên tên file.

QR không chứa `orderCode`, tài khoản ngân hàng hoặc cổng thanh toán thật. Mỗi order vẫn có mã riêng và nút xác nhận vẫn gọi API Mock hiện tại.

## Database

Patch này không thay đổi schema và không có migration mới.
