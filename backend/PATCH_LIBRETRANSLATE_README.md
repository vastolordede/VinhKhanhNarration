# Patch chuyển Translation sang LibreTranslate tự host

Patch này chỉ thay đổi backend. Không chứa `.env` thật và không ghi đè secret.

## File được thêm/sửa

- `Services/LibreTranslateService.cs`: provider mới, timeout, retry và thông báo lỗi ngắn.
- `Program.cs`: đăng ký LibreTranslate qua DI và chọn provider bằng cấu hình.
- `appsettings.json`: LibreTranslate trở thành provider mặc định local.
- `.env.example`: bổ sung các biến cấu hình.
- `render.yaml`: đổi provider production sang LibreTranslate và chờ nhập URL service.
- `docker-compose.libretranslate.yml`: chạy LibreTranslate local bằng Docker.

## Sau khi apply

Chạy LibreTranslate local:

```powershell
docker compose -f backend/docker-compose.libretranslate.yml up -d
```

Kiểm tra:

```powershell
curl http://localhost:5000/health
```

Thêm vào `backend/.env` của máy local:

```env
Translation__Provider=LibreTranslate
Translation__LibreTranslateUrl=http://localhost:5000
Translation__LibreTranslateApiKey=
Translation__TimeoutSeconds=180
Translation__RetryAttempts=3
Translation__RetryBaseDelayMilliseconds=750
```

Build backend:

```powershell
cd backend
dotnet restore
dotnet build
```

> Production cần deploy một LibreTranslate service riêng rồi đặt
> `Translation__LibreTranslateUrl` của backend Render bằng URL service đó.
