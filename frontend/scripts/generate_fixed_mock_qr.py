from __future__ import annotations

import argparse
from pathlib import Path
from urllib.parse import urlparse

import qrcode
from qrcode.image.svg import SvgPathImage


FRONTEND_ROOT = Path(__file__).resolve().parents[1]

DEFAULT_URL = (
    "https://vinh-khanh-narration.vercel.app/app/access"
)

DEFAULT_OUTPUT = (
    FRONTEND_ROOT
    / "public"
    / "mock-payment-qr.svg"
)


def validate_url(value: str) -> str:
    url = value.strip()
    parsed = urlparse(url)

    if parsed.scheme not in {"http", "https"}:
        raise argparse.ArgumentTypeError(
            "URL phải bắt đầu bằng http:// hoặc https://"
        )

    if not parsed.netloc:
        raise argparse.ArgumentTypeError(
            "URL không có domain hợp lệ."
        )

    return url.rstrip("/")


def main() -> None:
    parser = argparse.ArgumentParser(
        description=(
            "Tạo một QR cố định mở trang Guest Access. "
            "QR này không chứa thông tin thanh toán thật."
        )
    )

    parser.add_argument(
        "--url",
        type=validate_url,
        default=DEFAULT_URL,
        help=(
            "URL trang Guest Access. "
            f"Mặc định: {DEFAULT_URL}"
        ),
    )

    parser.add_argument(
        "--output",
        type=Path,
        default=DEFAULT_OUTPUT,
        help=(
            "Đường dẫn file SVG được tạo. "
            f"Mặc định: {DEFAULT_OUTPUT}"
        ),
    )

    args = parser.parse_args()

    output_path = args.output.resolve()
    output_path.parent.mkdir(
        parents=True,
        exist_ok=True,
    )

    qr = qrcode.QRCode(
        version=None,
        error_correction=(
            qrcode.constants.ERROR_CORRECT_M
        ),
        box_size=12,
        border=4,
    )

    qr.add_data(args.url)
    qr.make(fit=True)

    image = qr.make_image(
        image_factory=SvgPathImage
    )

    image.save(output_path)

    if not output_path.exists():
        raise RuntimeError(
            "Không tạo được file QR."
        )

    print()
    print("Đã tạo QR Mock cố định thành công.")
    print(f"Target URL : {args.url}")
    print(f"Output     : {output_path}")
    print(f"Size       : {output_path.stat().st_size} bytes")
    print()
    print(
        "QR chỉ mở trang Guest Access, "
        "không chứa tài khoản ngân hàng "
        "và không nhận tiền thật."
    )


if __name__ == "__main__":
    main()