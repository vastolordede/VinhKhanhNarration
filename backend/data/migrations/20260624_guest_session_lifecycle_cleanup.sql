BEGIN;

-- ============================================================
-- 1. Paid Guest Session lifecycle
--    One successful Guest payment creates exactly one new session.
--    The price is copied to the session as a historical snapshot.
-- ============================================================

ALTER TABLE public.guest_payment_orders
    ALTER COLUMN guest_session_id DROP NOT NULL;

-- Remove the legacy order -> session FK. The canonical relation is now
-- guest_sessions.guest_payment_order_id -> guest_payment_orders. This avoids
-- a circular FK while retaining guest_session_id as a convenient lookup field.
ALTER TABLE public.guest_payment_orders
    DROP CONSTRAINT IF EXISTS fk_guest_payment_orders_session;

ALTER TABLE public.guest_payment_orders
    ADD COLUMN IF NOT EXISTS preferred_language_id bigint,
    ADD COLUMN IF NOT EXISTS device_info varchar(255),
    ADD COLUMN IF NOT EXISTS ip_address varchar(50);

ALTER TABLE public.guest_payment_orders
    DROP CONSTRAINT IF EXISTS fk_guest_payment_orders_language;
ALTER TABLE public.guest_payment_orders
    ADD CONSTRAINT fk_guest_payment_orders_language
    FOREIGN KEY (preferred_language_id)
    REFERENCES public.languages(language_id)
    ON UPDATE CASCADE
    ON DELETE SET NULL;

ALTER TABLE public.guest_sessions
    ADD COLUMN IF NOT EXISTS guest_payment_order_id bigint,
    ADD COLUMN IF NOT EXISTS access_price numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS access_started_at timestamp without time zone,
    ADD COLUMN IF NOT EXISTS access_expires_at timestamp without time zone,
    ADD COLUMN IF NOT EXISTS deactivated_at timestamp without time zone;

ALTER TABLE public.listening_histories
    ADD COLUMN IF NOT EXISTS access_pass_id bigint;

ALTER TABLE public.geofence_events
    ADD COLUMN IF NOT EXISTS access_pass_id bigint;

-- Existing legacy sessions are not billable sessions.
UPDATE public.guest_sessions
SET access_price = 0
WHERE guest_payment_order_id IS NULL;

-- Remove constraints safely before recreating them.
ALTER TABLE public.guest_sessions
    DROP CONSTRAINT IF EXISTS fk_guest_sessions_payment_order;
ALTER TABLE public.listening_histories
    DROP CONSTRAINT IF EXISTS fk_listening_histories_access_pass;
ALTER TABLE public.geofence_events
    DROP CONSTRAINT IF EXISTS fk_geofence_events_access_pass;

ALTER TABLE public.guest_sessions
    ADD CONSTRAINT fk_guest_sessions_payment_order
    FOREIGN KEY (guest_payment_order_id)
    REFERENCES public.guest_payment_orders(guest_payment_order_id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'chk_guest_sessions_access_price'
    ) THEN
        ALTER TABLE public.guest_sessions
            ADD CONSTRAINT chk_guest_sessions_access_price
            CHECK (access_price >= 0);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'chk_guest_sessions_access_window'
    ) THEN
        ALTER TABLE public.guest_sessions
            ADD CONSTRAINT chk_guest_sessions_access_window
            CHECK (
                access_started_at IS NULL
                OR access_expires_at IS NULL
                OR access_expires_at > access_started_at
            );
    END IF;
END $$;

ALTER TABLE public.listening_histories
    ADD CONSTRAINT fk_listening_histories_access_pass
    FOREIGN KEY (access_pass_id)
    REFERENCES public.guest_access_passes(access_pass_id)
    ON UPDATE CASCADE
    ON DELETE SET NULL;

ALTER TABLE public.geofence_events
    ADD CONSTRAINT fk_geofence_events_access_pass
    FOREIGN KEY (access_pass_id)
    REFERENCES public.guest_access_passes(access_pass_id)
    ON UPDATE CASCADE
    ON DELETE SET NULL;

CREATE INDEX IF NOT EXISTS ix_guest_sessions_access_started_at
    ON public.guest_sessions(access_started_at);
CREATE INDEX IF NOT EXISTS ix_guest_sessions_access_expires_at
    ON public.guest_sessions(access_expires_at);
CREATE INDEX IF NOT EXISTS ix_guest_payment_orders_paid_at
    ON public.guest_payment_orders(paid_at)
    WHERE status = 'Paid';
CREATE INDEX IF NOT EXISTS ix_payment_orders_paid_at
    ON public.payment_orders(paid_at)
    WHERE status = 'Paid';
CREATE INDEX IF NOT EXISTS ix_listening_histories_listened_at
    ON public.listening_histories(listened_at);
CREATE INDEX IF NOT EXISTS ix_geofence_events_detected_at
    ON public.geofence_events(detected_at);

-- ============================================================
-- 2. Local-only cleanup of legacy/test runtime data
--    Preserve Admin accounts, valid languages and master lookups.
-- ============================================================

TRUNCATE TABLE
    public.listening_histories,
    public.guest_poi_states,
    public.geofence_events,
    public.feedbacks,
    public.guest_access_passes,
    public.guest_payment_orders,
    public.guest_sessions,
    public.audio_files,
    public.admin_refresh_tokens,
    public.vendor_refresh_tokens,
    public.audit_logs
RESTART IDENTITY;

CREATE UNIQUE INDEX IF NOT EXISTS ux_guest_sessions_payment_order
    ON public.guest_sessions(guest_payment_order_id)
    WHERE guest_payment_order_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ux_guest_payment_orders_session
    ON public.guest_payment_orders(guest_session_id)
    WHERE guest_session_id IS NOT NULL;

-- Remove known placeholder language while retaining vi/en/ja/ko/zh.
DELETE FROM public.narration_translations
WHERE language_id IN (
    SELECT language_id
    FROM public.languages
    WHERE language_code = 'string'
       OR language_name = 'string'
);

UPDATE public.narration_contents
SET source_language_id = (
    SELECT language_id
    FROM public.languages
    WHERE language_code = 'vi'
    ORDER BY language_id
    LIMIT 1
)
WHERE source_language_id IN (
    SELECT language_id
    FROM public.languages
    WHERE language_code = 'string'
       OR language_name = 'string'
);

DELETE FROM public.languages
WHERE language_code = 'string'
   OR language_name = 'string';

-- Remove only clearly named local test narrations.
DELETE FROM public.narration_contents
WHERE lower(title) IN (
    'test sau update',
    'bài test auto translate từ admin'
);

-- Keep sequences consistent after targeted cleanup.
SELECT setval(
    pg_get_serial_sequence('public.languages', 'language_id'),
    COALESCE((SELECT MAX(language_id) FROM public.languages), 1),
    true
);

COMMIT;
