# Validation Report – 2026-06-19

## Passed locally

- Clean frontend dependency install: `npm ci`.
- Frontend tests: 2 test files and 7 tests passed.
- Frontend production build: `tsc -b` and Vite build passed.
- JSON, `.csproj` XML and GitHub Actions YAML parsing passed.
- C# lexical structural scan: 76 files passed balanced delimiter checks.
- ASP.NET route scan: 117 routes, no duplicate HTTP verb + route pair.
- Legacy QR source reference scan: no `QRCodeManagementScreen`, `QRScannerScreen`, `qrCodes` or `resolveQr` reference.
- Static-file bypass scan: `UseStaticFiles()` is absent; audio and Vendor documents use protected controllers.
- `git diff --check`: passed.
- Secret/package safety: generated patch excludes `.env`, `node_modules`, `dist`, `bin`, `obj`, `.git` and credential/key files.

## Non-blocking warning

- Vite reports the main JavaScript chunk is larger than 500 kB after minification. This is an optimization item, not a build failure.

## Environment-blocked checks

- `dotnet build` / `dotnet test`: the sandbox has no .NET SDK and outbound SDK download is unavailable.
- PostgreSQL migration execution: no PostgreSQL service is running in the sandbox.

## Clean-environment checks included in CI

The workflow `.github/workflows/ci.yml` runs:

- `dotnet restore`, `dotnet build`, `dotnet test` on .NET 8.
- `npm ci`, `npm run test:run`, `npm run build` on Node.js 22.
