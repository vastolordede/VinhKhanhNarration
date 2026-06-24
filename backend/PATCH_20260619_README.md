# VinhKhanhNarration – Full Missing Modules Patch (2026-06-19)

## Scope implemented

This patch completes the remaining Hoàng Phúc and Nguyễn Minh Tuấn scope and merges it with the existing Vendor mock-payment, six-month subscription, multilingual narration, translation and TTS modules.

- Guest mock payment and 24-hour Access Pass. Guest payment does **not** use QR; QR remains only in the Vendor mock-payment flow.
- Map, stall and menu browsing remain public. An active Access Pass is required for narration resolution, protected audio, geofence checks and listening-history writes.
- Admin and Vendor refresh-token rotation, reuse detection, logout/logout-all, password change and session revocation.
- One Vendor owns one stall; Vendor CRUD is limited to its own stall, dishes and menu assignments.
- Vendor narration can target only the Vendor's own stall or dishes. General narration and cross-Vendor identifiers are rejected by the backend.
- New registration cannot claim an existing public stall. The Vendor creates its stall only after document approval and subscription activation.
- Public filtering hides expired/inactive Vendor stalls, dishes, menu rows and narrations from lists, search, direct lookup and geofence resolution.
- Automatic lifecycle maintenance for subscriptions, pending orders, Guest Access Passes and expired/revoked refresh tokens.
- Audit logs for mutating HTTP requests and scheduler runs, with an Admin audit screen.
- Local audio and Vendor documents are no longer exposed by static-file middleware. Protected endpoints enforce Guest pass, Admin role or Vendor ownership.
- Frontend policy tests, backend utility tests and GitHub Actions CI.

## Database migration order

Run these migrations in order:

```text
20260618_multilingual_narration_tts.sql
20260618_vendor_admin_mock_payment.sql
20260619_complete_missing_modules.sql
```

The 2026-06-19 migration creates Vendor refresh tokens, Guest payment/pass tables, dish ownership, ownership indexes and audit logs.

## Required environment values

Keep real credentials only in `backend/.env`. Do not commit that file.

- PostgreSQL settings: `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `DB_SSLMODE`.
- For Neon/production, uncomment and replace `DATABASE_URL`; keep it commented for local PostgreSQL.
- `Jwt__SecretKey` must be at least 32 characters.
- `Translator__Key`, `Translator__Region` for real translation.
- `Speech__Key`, `Speech__Region` for real TTS.
- Optional: `GuestAccess__Price`, `GuestAccess__DurationHours`, `GuestAccess__OrderExpiryMinutes`.
- Optional: `Lifecycle__IntervalMinutes`.

## Smoke-test order

1. Start PostgreSQL and apply the three migrations in order.
2. Start backend and frontend.
3. Register a Vendor with both required documents.
4. Admin approves the Vendor; confirm the Vendor QR mock payment and verify the six-month subscription.
5. Vendor creates its stall, owned dishes and menu.
6. Verify the Vendor cannot submit another Vendor's `placeId` or `dishId`.
7. Vendor creates a narration and sends it for review.
8. Admin approves the source narration; verify translation/TTS and automatic publish.
9. Create a Guest session and confirm a Guest mock payment without QR; verify the 24-hour pass.
10. Verify map/menu work without a pass, while narration/audio/geofence require an active pass.
11. Expire a subscription/pass in the database and verify scheduler status changes and public hiding.
12. Verify Vendor/Admin hide, soft-delete, restore and notification flows.
13. Verify the Admin Audit Log records write operations and lifecycle maintenance.

## Validation notes

- Clean frontend install completed with `npm ci`.
- Frontend tests passed: 2 files, 7 tests.
- Frontend production build passed (`tsc -b` and Vite build).
- Vite reports a non-blocking warning because the main JavaScript chunk is above 500 kB.
- JSON, XML and YAML parsing passed.
- C# lexical delimiter scan passed across 76 source/test files.
- ASP.NET route scan found 117 routes and no duplicate verb/route pair.
- `git diff --check` passed.
- The sandbox does not contain the .NET SDK or a running PostgreSQL instance, so backend compilation/tests and migration execution were not claimed as locally passed. The included CI runs the clean .NET 8 build and tests.
