# =========================
# Build .NET 8 application
# Render Root Directory: backend
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VinhKhanhNarration.Api.csproj ./
RUN dotnet restore VinhKhanhNarration.Api.csproj

COPY . ./
RUN dotnet publish VinhKhanhNarration.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================
# Runtime: ASP.NET Core + Python Edge TTS
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        python3 \
        python3-venv \
        python3-pip \
        ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./
COPY --from=build /src/tools ./tools

RUN python3 -m venv /opt/edge-tts \
    && /opt/edge-tts/bin/python -m pip install --no-cache-dir --upgrade pip \
    && /opt/edge-tts/bin/python -m pip install --no-cache-dir \
        -r /app/tools/requirements-edge-tts.txt

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV Speech__Provider=EdgeTTS
ENV EdgeTts__PythonPath=/opt/edge-tts/bin/python
ENV EdgeTts__ScriptPath=/app/tools/edge_tts_synthesize.py

EXPOSE 10000

ENTRYPOINT ["dotnet", "VinhKhanhNarration.Api.dll"]
