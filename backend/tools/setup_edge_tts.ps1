$ErrorActionPreference = "Stop"

$BackendRoot = Split-Path -Parent $PSScriptRoot
Set-Location $BackendRoot

if (-not (Get-Command py -ErrorAction SilentlyContinue)) {
    throw "Không tìm thấy Python launcher 'py'. Hãy cài Python 3 trước."
}

py -m venv .venv-tts
& .\.venv-tts\Scripts\python.exe -m pip install --upgrade pip
& .\.venv-tts\Scripts\python.exe -m pip install -r .\tools\requirements-edge-tts.txt

Write-Host ""
Write-Host "Edge TTS đã được cài tại backend\.venv-tts" -ForegroundColor Green
Write-Host "Khởi động lại backend rồi bấm Thử lại ở trang Âm thanh." -ForegroundColor Cyan
