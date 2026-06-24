# Dashboard + legacy content cleanup patch

## Frontend

- Removes the "Revenue calculation rule" card.
- Restores the horizontal overview chart.
- Keeps the revenue doughnut chart.
- Keeps and expands the time activity chart with Listening, Geofence, Feedback and paid Guest Sessions.

## Database

Run after the previous `20260624_guest_session_lifecycle_cleanup.sql` migration:

```text
backend/data/migrations/20260624_remove_legacy_content_data.sql
```

This cleanup deletes old local/mock:

- Places / POI
- Dishes
- Place menus
- Narration contents
- Narration translations
- Audio records
- Listening, geofence, POI-state and feedback logs tied to old content

It preserves Admin accounts, languages, lookup/master tables, Vendor accounts/payments and Guest payment/session/access-pass history.
