BEGIN;

-- =========================================================
-- 1. Refresh tokens for Vendor accounts
-- =========================================================
CREATE TABLE IF NOT EXISTS public.vendor_refresh_tokens (
    refresh_token_id bigserial PRIMARY KEY,
    vendor_user_id bigint NOT NULL,
    token_hash varchar(255) NOT NULL UNIQUE,
    expires_at timestamp without time zone NOT NULL,
    revoked_at timestamp without time zone,
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_ip varchar(100),
    revoked_by_ip varchar(100),
    replaced_by_token_hash varchar(255),
    CONSTRAINT fk_vendor_refresh_tokens_vendor
        FOREIGN KEY (vendor_user_id) REFERENCES public.vendor_users(vendor_user_id)
        ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_vendor_refresh_tokens_vendor_active
    ON public.vendor_refresh_tokens(vendor_user_id, expires_at DESC)
    WHERE revoked_at IS NULL;

-- =========================================================
-- 2. Guest mock payment and 24-hour access pass
-- =========================================================
CREATE TABLE IF NOT EXISTS public.guest_payment_orders (
    guest_payment_order_id bigserial PRIMARY KEY,
    guest_session_id varchar(100) NOT NULL,
    order_code varchar(90) NOT NULL UNIQUE,
    amount numeric(18,2) NOT NULL DEFAULT 50000,
    status varchar(30) NOT NULL DEFAULT 'Pending',
    provider varchar(30) NOT NULL DEFAULT 'Mock',
    payment_url text NOT NULL,
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_at timestamp without time zone NOT NULL,
    paid_at timestamp without time zone,
    CONSTRAINT fk_guest_payment_orders_session
        FOREIGN KEY (guest_session_id) REFERENCES public.guest_sessions(guest_session_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT chk_guest_payment_orders_status CHECK (
        status IN ('Pending','Paid','Cancelled','Expired')
    )
);

CREATE TABLE IF NOT EXISTS public.guest_access_passes (
    access_pass_id bigserial PRIMARY KEY,
    guest_session_id varchar(100) NOT NULL,
    guest_payment_order_id bigint NOT NULL UNIQUE,
    starts_at timestamp without time zone NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'Active',
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_guest_access_passes_session
        FOREIGN KEY (guest_session_id) REFERENCES public.guest_sessions(guest_session_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_guest_access_passes_payment
        FOREIGN KEY (guest_payment_order_id) REFERENCES public.guest_payment_orders(guest_payment_order_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_guest_access_passes_status CHECK (
        status IN ('Active','Expired','Revoked')
    )
);

CREATE INDEX IF NOT EXISTS idx_guest_payment_orders_session
    ON public.guest_payment_orders(guest_session_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_guest_access_passes_session
    ON public.guest_access_passes(guest_session_id, expires_at DESC);
CREATE UNIQUE INDEX IF NOT EXISTS uq_vendor_subscriptions_payment_order_id
    ON public.vendor_subscriptions(payment_order_id);

-- =========================================================
-- 3. Vendor ownership for dishes
-- =========================================================
ALTER TABLE public.dishes
    ADD COLUMN IF NOT EXISTS owner_vendor_id bigint;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_dishes_owner_vendor'
    ) THEN
        ALTER TABLE public.dishes
            ADD CONSTRAINT fk_dishes_owner_vendor
            FOREIGN KEY (owner_vendor_id) REFERENCES public.vendor_users(vendor_user_id)
            ON UPDATE CASCADE ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_dishes_owner_vendor
    ON public.dishes(owner_vendor_id, is_active, dish_id);

-- One active ownership link per stall/vendor in this project scope.
CREATE UNIQUE INDEX IF NOT EXISTS uq_vendor_users_place_id
    ON public.vendor_users(place_id)
    WHERE place_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_places_owner_vendor_id
    ON public.places(owner_vendor_id)
    WHERE owner_vendor_id IS NOT NULL;

-- Synchronize the two ownership columns for existing rows where possible.
UPDATE public.places p
SET owner_vendor_id = vu.vendor_user_id
FROM public.vendor_users vu
WHERE vu.place_id = p.place_id
  AND p.owner_vendor_id IS NULL;

UPDATE public.vendor_users vu
SET place_id = p.place_id,
    updated_at = CURRENT_TIMESTAMP
FROM public.places p
WHERE p.owner_vendor_id = vu.vendor_user_id
  AND vu.place_id IS NULL;

-- =========================================================
-- 4. Audit log
-- =========================================================
CREATE TABLE IF NOT EXISTS public.audit_logs (
    audit_log_id bigserial PRIMARY KEY,
    actor_type varchar(20) NOT NULL,
    actor_id bigint,
    guest_session_id varchar(100),
    action varchar(120) NOT NULL,
    entity_type varchar(80),
    entity_id bigint,
    details jsonb,
    ip_address varchar(100),
    user_agent varchar(500),
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_audit_logs_actor_type CHECK (
        actor_type IN ('Admin','Vendor','Guest','System')
    )
);

CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at
    ON public.audit_logs(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_actor
    ON public.audit_logs(actor_type, actor_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_entity
    ON public.audit_logs(entity_type, entity_id, created_at DESC);

-- =========================================================
-- 5. Expire stale rows once during migration
-- =========================================================
UPDATE public.payment_orders
SET status = 'Expired'
WHERE status = 'Pending' AND expires_at <= CURRENT_TIMESTAMP;

UPDATE public.guest_payment_orders
SET status = 'Expired'
WHERE status = 'Pending' AND expires_at <= CURRENT_TIMESTAMP;

UPDATE public.vendor_subscriptions
SET status = 'Expired', updated_at = CURRENT_TIMESTAMP
WHERE status = 'Active' AND expires_at <= CURRENT_TIMESTAMP;

UPDATE public.guest_access_passes
SET status = 'Expired', updated_at = CURRENT_TIMESTAMP
WHERE status = 'Active' AND expires_at <= CURRENT_TIMESTAMP;

COMMIT;
