
from __future__ import annotations

import os
import sys
import time
from pathlib import Path

import boto3
from botocore.config import Config
from botocore.exceptions import BotoCoreError, ClientError

try:
    from dotenv import load_dotenv
except ImportError:
    load_dotenv = None


def load_env_file() -> None:
    """
    Tự tìm file .env ở các vị trí thường gặp.

    Ví dụ cấu trúc:
    VinhKhanhNarration/
    ├── backend/
    │   └── .env
    └── r2-test/
        └── r2_smoke_test.py
    """
    current_file_directory = Path(__file__).resolve().parent

    candidates = [
        Path.cwd() / ".env",
        Path.cwd() / "backend" / ".env",
        current_file_directory / ".env",
        current_file_directory.parent / ".env",
        current_file_directory.parent / "backend" / ".env",
    ]

    for env_path in candidates:
        if not env_path.exists():
            continue

        if load_dotenv is None:
            print(
                f"Đã tìm thấy {env_path}, "
                "nhưng chưa cài python-dotenv.",
                file=sys.stderr,
            )
            return

        load_dotenv(
            dotenv_path=env_path,
            override=False,
        )

        print(f"Đã đọc cấu hình từ: {env_path}")
        return

    print(
        "Không tìm thấy file .env. "
        "Script sẽ dùng biến môi trường của terminal."
    )


def first_env(*names: str) -> str:
    """
    Lấy giá trị đầu tiên tồn tại trong danh sách tên biến.

    Hỗ trợ cả:
    R2_ACCOUNT_ID
    và:
    R2__AccountId
    """
    for name in names:
        value = os.getenv(name, "").strip()

        if value:
            return value

    return ""


def require_env(
    label: str,
    *names: str,
) -> str:
    value = first_env(*names)

    if not value:
        expected_names = ", ".join(names)

        raise RuntimeError(
            f"Thiếu {label}. "
            f"Hãy cấu hình một trong các biến: "
            f"{expected_names}"
        )

    return value


def mask_secret(value: str) -> str:
    """
    Hiển thị một phần giá trị để kiểm tra script đọc đúng,
    nhưng không làm lộ toàn bộ credential.
    """
    if len(value) <= 8:
        return "*" * len(value)

    return (
        f"{value[:4]}...{value[-4:]}"
        f" (length={len(value)})"
    )


def main() -> int:
    load_env_file()

    try:
        account_id = require_env(
            "R2 Account ID",
            "R2_ACCOUNT_ID",
            "R2__AccountId",
        )

        access_key_id = require_env(
            "R2 Access Key ID",
            "R2_ACCESS_KEY_ID",
            "R2__AccessKeyId",
        )

        secret_access_key = require_env(
            "R2 Secret Access Key",
            "R2_SECRET_ACCESS_KEY",
            "R2__SecretAccessKey",
        )

        bucket_name = require_env(
            "R2 Bucket Name",
            "R2_BUCKET_NAME",
            "R2__BucketName",
        )

    except RuntimeError as error:
        print(
            f"[CONFIG ERROR] {error}",
            file=sys.stderr,
        )
        return 2

    endpoint = first_env(
        "R2_ENDPOINT",
        "R2__Endpoint",
    )

    if not endpoint:
        endpoint = (
            f"https://{account_id}"
            ".r2.cloudflarestorage.com"
        )

    endpoint = endpoint.rstrip("/")

    object_key = (
        "smoke-test/"
        f"vinhkhanh-{int(time.time())}.txt"
    )

    test_content = (
        b"VinhKhanhNarration R2 smoke test"
    )

    print()
    print("Cấu hình đang được kiểm tra:")
    print(
        f"- Account ID: "
        f"{mask_secret(account_id)}"
    )
    print(
        f"- Access Key: "
        f"{mask_secret(access_key_id)}"
    )
    print(
        f"- Secret Key: "
        f"{mask_secret(secret_access_key)}"
    )
    print(f"- Endpoint:   {endpoint}")
    print(f"- Bucket:     {bucket_name}")
    print(f"- Object key: {object_key}")
    print()

    client = boto3.client(
        "s3",
        endpoint_url=endpoint,
        region_name="auto",
        aws_access_key_id=access_key_id,
        aws_secret_access_key=secret_access_key,
        config=Config(
            signature_version="s3v4",
            retries={
                "max_attempts": 3,
                "mode": "standard",
            },
        ),
    )

    uploaded = False

    try:
        # Không dùng head_bucket vì một số R2 token
        # chỉ có quyền thao tác object.
        client.put_object(
            Bucket=bucket_name,
            Key=object_key,
            Body=test_content,
            ContentType=(
                "text/plain; charset=utf-8"
            ),
            Metadata={
                "project": (
                    "vinhkhanh-narration"
                ),
                "purpose": "smoke-test",
            },
        )

        uploaded = True

        print(
            "[OK] Upload object thành công."
        )

        response = client.get_object(
            Bucket=bucket_name,
            Key=object_key,
        )

        downloaded_content = (
            response["Body"].read()
        )

        if downloaded_content != test_content:
            raise RuntimeError(
                "Nội dung tải xuống không giống "
                "nội dung đã upload."
            )

        print(
            "[OK] Download object thành công."
        )

        presigned_url = (
            client.generate_presigned_url(
                ClientMethod="get_object",
                Params={
                    "Bucket": bucket_name,
                    "Key": object_key,
                },
                ExpiresIn=300,
            )
        )

        print(
            "[OK] Tạo presigned URL thành công."
        )

        print()
        print("R2 SMOKE TEST PASSED")
        print()
        print(
            "Presigned URL có hiệu lực 5 phút:"
        )
        print(presigned_url)

        return 0

    except ClientError as error:
        error_data = error.response.get(
            "Error",
            {},
        )

        response_metadata = (
            error.response.get(
                "ResponseMetadata",
                {},
            )
        )

        status_code = response_metadata.get(
            "HTTPStatusCode",
            "Unknown",
        )

        error_code = error_data.get(
            "Code",
            "Unknown",
        )

        error_message = error_data.get(
            "Message",
            str(error),
        )

        print()
        print(
            "[R2 ERROR] "
            f"HTTP {status_code} | "
            f"{error_code}: "
            f"{error_message}",
            file=sys.stderr,
        )

        print(
            """
Kiểm tra lại:

1. R2__AccessKeyId phải là Access Key ID.
2. R2__SecretAccessKey phải là Secret Access Key.
3. Không dùng Token Value thay cho Secret Access Key.
4. Token phải có quyền Object Read & Write.
5. Token phải được cấp cho đúng bucket.
6. R2__BucketName phải đúng chính xác tên bucket.
7. Account ID trong endpoint phải cùng tài khoản với token.
""",
            file=sys.stderr,
        )

        return 1

    except (
        BotoCoreError,
        RuntimeError,
    ) as error:
        print(
            f"[R2 ERROR] {error}",
            file=sys.stderr,
        )

        return 1

    finally:
        if uploaded:
            try:
                client.delete_object(
                    Bucket=bucket_name,
                    Key=object_key,
                )

                print(
                    "[CLEANUP] "
                    "Đã xóa object test."
                )

            except Exception as cleanup_error:
                print(
                    "[CLEANUP WARNING] "
                    "Không xóa được object test: "
                    f"{cleanup_error}",
                    file=sys.stderr,
                )


if __name__ == "__main__":
    raise SystemExit(main())

