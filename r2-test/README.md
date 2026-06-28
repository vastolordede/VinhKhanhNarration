# R2 Smoke Test

Đặt thư mục này trong project, ví dụ:

```text
D:\School\VinhKhanhNarration\r2-test
```

Đảm bảo `backend/.env` đã có:

```env
R2__AccountId=...
R2__AccessKeyId=...
R2__SecretAccessKey=...
R2__BucketName=vinhkhanhnarration
R2__Endpoint=https://<ACCOUNT_ID>.r2.cloudflarestorage.com
```

Chạy:

```powershell
powershell -ExecutionPolicy Bypass -File .\run_r2_smoke_test.ps1
```

Kết quả thành công:

```text
[OK] Kết nối bucket thành công.
[OK] Upload thành công.
[OK] Download thành công.
[OK] Tạo presigned URL thành công.
R2 SMOKE TEST PASSED
```

Object test được tự xóa nên bucket có thể vẫn hiển thị 0 B.
