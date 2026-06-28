from __future__ import annotations

import argparse
from pathlib import Path

import qrcode
from qrcode.image.svg import SvgPathImage


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate the one shared mock-payment QR image."
    )
    parser.add_argument(
        "url",
        help="Public Guest Access URL, for example https://app.vercel.app/app/access",
    )
    parser.add_argument(
        "--output",
        default=str(Path(__file__).resolve().parents[1] / "public" / "mock-payment-qr.svg"),
    )
    args = parser.parse_args()

    url = args.url.strip()
    if not url.startswith(("http://", "https://")):
        raise SystemExit("The QR target must be an http:// or https:// URL.")

    output = Path(args.output).resolve()
    output.parent.mkdir(parents=True, exist_ok=True)

    qr = qrcode.QRCode(
        version=None,
        error_correction=qrcode.constants.ERROR_CORRECT_M,
        box_size=10,
        border=4,
    )
    qr.add_data(url)
    qr.make(fit=True)

    image = qr.make_image(image_factory=SvgPathImage)
    image.save(output)

    print(f"Fixed mock QR written to: {output}")
    print(f"Target URL: {url}")
    print("This QR opens the mock app only; it does not contain bank payment data.")


if __name__ == "__main__":
    main()
