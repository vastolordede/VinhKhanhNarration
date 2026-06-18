BEGIN;

CREATE TABLE IF NOT EXISTS public.vendor_users (
    vendor_user_id bigserial PRIMARY KEY,
    owner_name varchar(150) NOT NULL,
    shop_name varchar(180) NOT NULL,
    email varchar(180) NOT NULL UNIQUE,
    phone varchar(30),
    password_hash text NOT NULL,
    place_id bigint,
    account_status varchar(30) NOT NULL DEFAULT 'PendingReview',
    review_reason text,
    reviewed_by bigint,
    reviewed_at timestamp without time zone,
    is_active boolean NOT NULL DEFAULT false,
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_vendor_users_place
        FOREIGN KEY (place_id) REFERENCES public.places(place_id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT fk_vendor_users_reviewed_by
        FOREIGN KEY (reviewed_by) REFERENCES public.admin_users(admin_id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT chk_vendor_users_account_status CHECK (
        account_status IN (
            'PendingReview','PendingPayment','Active','ExpiringSoon',
            'Expired','Rejected','Suspended'
        )
    )
);

CREATE TABLE IF NOT EXISTS public.vendor_documents (
    document_id bigserial PRIMARY KEY,
    vendor_user_id bigint NOT NULL,
    document_type varchar(30) NOT NULL,
    file_name varchar(255) NOT NULL,
    file_url text NOT NULL,
    expires_at date,
    verification_status varchar(30) NOT NULL DEFAULT 'Pending',
    review_reason text,
    reviewed_by bigint,
    reviewed_at timestamp without time zone,
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_vendor_documents_vendor
        FOREIGN KEY (vendor_user_id) REFERENCES public.vendor_users(vendor_user_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_vendor_documents_reviewed_by
        FOREIGN KEY (reviewed_by) REFERENCES public.admin_users(admin_id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT chk_vendor_documents_type CHECK (
        document_type IN ('BusinessLicense','FoodSafety')
    ),
    CONSTRAINT chk_vendor_documents_status CHECK (
        verification_status IN ('Pending','Approved','Rejected','Superseded')
    )
);

CREATE TABLE IF NOT EXISTS public.vendor_renewal_requests (
    renewal_request_id bigserial PRIMARY KEY,
    vendor_user_id bigint NOT NULL,
    food_safety_document_id bigint NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'PendingReview',
    review_reason text,
    reviewed_by bigint,
    reviewed_at timestamp without time zone,
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_vendor_renewal_vendor
        FOREIGN KEY (vendor_user_id) REFERENCES public.vendor_users(vendor_user_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_vendor_renewal_document
        FOREIGN KEY (food_safety_document_id) REFERENCES public.vendor_documents(document_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_vendor_renewal_reviewed_by
        FOREIGN KEY (reviewed_by) REFERENCES public.admin_users(admin_id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT chk_vendor_renewal_status CHECK (
        status IN ('PendingReview','PendingPayment','Approved','Rejected','Paid')
    )
);

CREATE TABLE IF NOT EXISTS public.payment_orders (
    payment_order_id bigserial PRIMARY KEY,
    vendor_user_id bigint NOT NULL,
    renewal_request_id bigint,
    purpose varchar(30) NOT NULL,
    order_code varchar(80) NOT NULL UNIQUE,
    amount numeric(18,2) NOT NULL DEFAULT 0,
    status varchar(30) NOT NULL DEFAULT 'Pending',
    provider varchar(30) NOT NULL DEFAULT 'Mock',
    payment_url text NOT NULL,
    qr_image_url text NOT NULL,
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_at timestamp without time zone NOT NULL,
    paid_at timestamp without time zone,
    CONSTRAINT fk_payment_orders_vendor
        FOREIGN KEY (vendor_user_id) REFERENCES public.vendor_users(vendor_user_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_payment_orders_renewal
        FOREIGN KEY (renewal_request_id) REFERENCES public.vendor_renewal_requests(renewal_request_id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT chk_payment_orders_purpose CHECK (
        purpose IN ('Registration','Renewal')
    ),
    CONSTRAINT chk_payment_orders_status CHECK (
        status IN ('Pending','Paid','Cancelled','Expired')
    )
);

CREATE TABLE IF NOT EXISTS public.vendor_subscriptions (
    subscription_id bigserial PRIMARY KEY,
    vendor_user_id bigint NOT NULL,
    payment_order_id bigint NOT NULL,
    starts_at timestamp without time zone NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'Active',
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_vendor_subscriptions_vendor
        FOREIGN KEY (vendor_user_id) REFERENCES public.vendor_users(vendor_user_id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_vendor_subscriptions_payment
        FOREIGN KEY (payment_order_id) REFERENCES public.payment_orders(payment_order_id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT chk_vendor_subscriptions_status CHECK (
        status IN ('Active','Expired','Suspended')
    )
);

CREATE TABLE IF NOT EXISTS public.vendor_notifications (
    notification_id bigserial PRIMARY KEY,
    vendor_user_id bigint NOT NULL,
    notification_type varchar(50) NOT NULL,
    title varchar(200) NOT NULL,
    message text NOT NULL,
    entity_type varchar(50),
    entity_id bigint,
    is_read boolean NOT NULL DEFAULT false,
    created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    read_at timestamp without time zone,
    CONSTRAINT fk_vendor_notifications_vendor
        FOREIGN KEY (vendor_user_id) REFERENCES public.vendor_users(vendor_user_id)
        ON UPDATE CASCADE ON DELETE CASCADE
);

ALTER TABLE public.places
    ADD COLUMN IF NOT EXISTS owner_vendor_id bigint;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_places_owner_vendor'
    ) THEN
        ALTER TABLE public.places
            ADD CONSTRAINT fk_places_owner_vendor
            FOREIGN KEY (owner_vendor_id) REFERENCES public.vendor_users(vendor_user_id)
            ON UPDATE CASCADE ON DELETE SET NULL;
    END IF;
END $$;

ALTER TABLE public.narration_contents
    ADD COLUMN IF NOT EXISTS previous_workflow_status varchar(30),
    ADD COLUMN IF NOT EXISTS moderation_reason text,
    ADD COLUMN IF NOT EXISTS moderation_by_admin_id bigint,
    ADD COLUMN IF NOT EXISTS hidden_at timestamp without time zone,
    ADD COLUMN IF NOT EXISTS deleted_at timestamp without time zone;

ALTER TABLE public.narration_contents
    DROP CONSTRAINT IF EXISTS chk_narration_contents_workflow_status;

ALTER TABLE public.narration_contents
    ADD CONSTRAINT chk_narration_contents_workflow_status CHECK (
        workflow_status IN (
            'Draft','PendingReview','Approved','Rejected','Published',
            'HiddenByVendor','HiddenByAdmin','DeletedByVendor','DeletedByAdmin'
        )
    );

UPDATE public.narration_contents nc
SET submitted_by_vendor_id = NULL
WHERE submitted_by_vendor_id IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM public.vendor_users vu
      WHERE vu.vendor_user_id = nc.submitted_by_vendor_id
  );

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_narration_contents_vendor'
    ) THEN
        ALTER TABLE public.narration_contents
            ADD CONSTRAINT fk_narration_contents_vendor
            FOREIGN KEY (submitted_by_vendor_id) REFERENCES public.vendor_users(vendor_user_id)
            ON UPDATE CASCADE ON DELETE SET NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_narration_contents_moderation_admin'
    ) THEN
        ALTER TABLE public.narration_contents
            ADD CONSTRAINT fk_narration_contents_moderation_admin
            FOREIGN KEY (moderation_by_admin_id) REFERENCES public.admin_users(admin_id)
            ON UPDATE CASCADE ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_vendor_users_status
    ON public.vendor_users(account_status);
CREATE INDEX IF NOT EXISTS idx_vendor_documents_vendor
    ON public.vendor_documents(vendor_user_id, document_type, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_vendor_subscriptions_vendor
    ON public.vendor_subscriptions(vendor_user_id, expires_at DESC);
CREATE INDEX IF NOT EXISTS idx_payment_orders_vendor
    ON public.payment_orders(vendor_user_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_vendor_notifications_unread
    ON public.vendor_notifications(vendor_user_id, is_read, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_places_owner_vendor
    ON public.places(owner_vendor_id);

COMMIT;
