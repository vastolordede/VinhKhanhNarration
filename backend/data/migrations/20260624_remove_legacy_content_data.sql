BEGIN;

-- ============================================================
-- Local development cleanup
-- Remove all legacy/mock content so Places, Dishes and Narrations
-- can be entered again manually from the current application flow.
--
-- Preserved:
--   - admin_users
--   - languages
--   - lookup/master tables
--   - Vendor accounts, subscriptions and payment history
--   - Guest payment/session/access-pass history
-- ============================================================

-- Runtime logs tied to the old content cannot remain meaningful after
-- their Place/Narration/Audio records are removed.
DELETE FROM public.listening_histories;
DELETE FROM public.geofence_events;
DELETE FROM public.guest_poi_states;
DELETE FROM public.feedbacks;

-- Narration-generated data.
DELETE FROM public.audio_files;
DELETE FROM public.narration_translations;
DELETE FROM public.narration_contents;

-- Menu and catalog content.
DELETE FROM public.place_dishes;
DELETE FROM public.dishes;

-- Keep Vendor accounts, but detach any ownership link to a deleted Place.
UPDATE public.vendor_users
SET place_id = NULL
WHERE place_id IS NOT NULL;

DELETE FROM public.places;

-- Restart local content identities so manually entered data starts cleanly.
SELECT setval(pg_get_serial_sequence('public.listening_histories', 'history_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.geofence_events', 'event_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.feedbacks', 'feedback_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.audio_files', 'audio_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.narration_translations', 'translation_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.narration_contents', 'narration_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.place_dishes', 'place_dish_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.dishes', 'dish_id'), 1, false);
SELECT setval(pg_get_serial_sequence('public.places', 'place_id'), 1, false);

COMMIT;
