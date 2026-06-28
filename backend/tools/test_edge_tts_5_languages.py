#!/usr/bin/env python3
"""Create five small MP3 samples using the voices configured in the database."""

from __future__ import annotations

import asyncio
from pathlib import Path

import edge_tts


BACKEND_ROOT = Path(__file__).resolve().parents[1]
OUTPUT_DIR = BACKEND_ROOT / "wwwroot" / "generated-audio" / "edge-tts-test"

SAMPLES = {
    "vi": (
        "vi-VN-HoaiMyNeural",
        "Xin chào. Đây là hệ thống thuyết minh phố ẩm thực Vĩnh Khánh.",
    ),
    "en": (
        "en-US-JennyNeural",
        "Hello. This is the narration system for Vinh Khanh food street.",
    ),
    "ja": (
        "ja-JP-NanamiNeural",
        "こんにちは。これはヴィンカイン飲食街の音声案内システムです。",
    ),
    "ko": (
        "ko-KR-SunHiNeural",
        "안녕하세요. 빈카인 음식 거리의 음성 안내 시스템입니다.",
    ),
    "zh": (
        "zh-CN-XiaoxiaoNeural",
        "您好。这是永庆美食街的语音导览系统。",
    ),
}


async def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    for code, (voice, text) in SAMPLES.items():
        output = OUTPUT_DIR / f"test_{code}.mp3"
        await edge_tts.Communicate(text=text, voice=voice).save(str(output))
        print(f"[OK] {code}: {voice} -> {output}")


if __name__ == "__main__":
    asyncio.run(main())
