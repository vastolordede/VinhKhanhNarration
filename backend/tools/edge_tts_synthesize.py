#!/usr/bin/env python3
"""Generate an MP3 file with edge-tts.

This script is intentionally small because it is invoked by the ASP.NET backend.
It receives source text through a UTF-8 file instead of command-line text so long
narrations and special characters remain safe across Windows and Linux.
"""

from __future__ import annotations

import argparse
import asyncio
import json
import sys
from pathlib import Path

import edge_tts


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate narration MP3 with edge-tts.")
    parser.add_argument("--input-file", required=True, help="UTF-8 text input path.")
    parser.add_argument("--output-file", required=True, help="MP3 output path.")
    parser.add_argument("--voice", required=True, help="Edge voice short name.")
    parser.add_argument("--rate", default="+0%")
    parser.add_argument("--volume", default="+0%")
    parser.add_argument("--pitch", default="+0Hz")
    return parser.parse_args()


async def run(args: argparse.Namespace) -> None:
    input_path = Path(args.input_file).expanduser().resolve()
    output_path = Path(args.output_file).expanduser().resolve()

    if not input_path.is_file():
        raise FileNotFoundError(f"Input file not found: {input_path}")

    text = input_path.read_text(encoding="utf-8").strip()
    if not text:
        raise ValueError("TTS input text is empty.")

    output_path.parent.mkdir(parents=True, exist_ok=True)

    communicate = edge_tts.Communicate(
        text=text,
        voice=args.voice,
        rate=args.rate,
        volume=args.volume,
        pitch=args.pitch,
    )
    await communicate.save(str(output_path))

    if not output_path.is_file() or output_path.stat().st_size == 0:
        raise RuntimeError("edge-tts did not create a valid MP3 file.")

    print(
        json.dumps(
            {
                "ok": True,
                "voice": args.voice,
                "outputFile": str(output_path),
                "sizeBytes": output_path.stat().st_size,
            },
            ensure_ascii=False,
        )
    )


def main() -> int:
    args = parse_args()

    try:
        asyncio.run(run(args))
        return 0
    except Exception as error:  # noqa: BLE001 - CLI boundary must return a clear error.
        print(f"edge-tts failed: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
