BEGIN;

ALTER TABLE public.languages
    ADD COLUMN IF NOT EXISTS locale varchar(20),
    ADD COLUMN IF NOT EXISTS native_name varchar(100),
    ADD COLUMN IF NOT EXISTS is_ui_enabled boolean NOT NULL DEFAULT true,
    ADD COLUMN IF NOT EXISTS is_content_enabled boolean NOT NULL DEFAULT true,
    ADD COLUMN IF NOT EXISTS is_translation_supported boolean NOT NULL DEFAULT true,
    ADD COLUMN IF NOT EXISTS is_tts_supported boolean NOT NULL DEFAULT true,
    ADD COLUMN IF NOT EXISTS default_voice_id varchar(150);

UPDATE public.languages
SET locale = CASE lower(language_code)
        WHEN 'vi' THEN 'vi-VN'
        WHEN 'en' THEN 'en-US'
        WHEN 'ja' THEN 'ja-JP'
        WHEN 'ko' THEN 'ko-KR'
        WHEN 'zh' THEN 'zh-CN'
        ELSE language_code
    END,
    native_name = CASE lower(language_code)
        WHEN 'vi' THEN 'Tiếng Việt'
        WHEN 'en' THEN 'English'
        WHEN 'ja' THEN '日本語'
        WHEN 'ko' THEN '한국어'
        WHEN 'zh' THEN '中文'
        ELSE language_name
    END,
    default_voice_id = CASE lower(language_code)
        WHEN 'vi' THEN 'vi-VN-HoaiMyNeural'
        WHEN 'en' THEN 'en-US-JennyNeural'
        WHEN 'ja' THEN 'ja-JP-NanamiNeural'
        WHEN 'ko' THEN 'ko-KR-SunHiNeural'
        WHEN 'zh' THEN 'zh-CN-XiaoxiaoNeural'
        ELSE default_voice_id
    END
WHERE locale IS NULL OR native_name IS NULL OR default_voice_id IS NULL;

ALTER TABLE public.narration_contents
    ALTER COLUMN created_by DROP NOT NULL,
    ADD COLUMN IF NOT EXISTS source_language_id bigint,
    ADD COLUMN IF NOT EXISTS workflow_status varchar(30) NOT NULL DEFAULT 'Draft',
    ADD COLUMN IF NOT EXISTS rejection_reason text,
    ADD COLUMN IF NOT EXISTS submitted_by_vendor_id bigint,
    ADD COLUMN IF NOT EXISTS submitted_at timestamp without time zone,
    ADD COLUMN IF NOT EXISTS reviewed_by bigint,
    ADD COLUMN IF NOT EXISTS reviewed_at timestamp without time zone,
    ADD COLUMN IF NOT EXISTS published_at timestamp without time zone;

UPDATE public.narration_contents
SET source_language_id = (
        SELECT language_id FROM public.languages
        WHERE lower(language_code) = 'vi'
        ORDER BY language_id LIMIT 1
    )
WHERE source_language_id IS NULL;

UPDATE public.narration_contents
SET workflow_status = CASE WHEN is_active THEN 'Published' ELSE 'Draft' END
WHERE workflow_status IS NULL OR workflow_status = 'Draft';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'chk_narration_contents_workflow_status'
    ) THEN
        ALTER TABLE public.narration_contents
            ADD CONSTRAINT chk_narration_contents_workflow_status
            CHECK (workflow_status IN ('Draft','PendingReview','Approved','Rejected','Published'));
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_narration_contents_source_language'
    ) THEN
        ALTER TABLE public.narration_contents
            ADD CONSTRAINT fk_narration_contents_source_language
            FOREIGN KEY (source_language_id) REFERENCES public.languages(language_id)
            ON UPDATE CASCADE ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_narration_contents_reviewed_by'
    ) THEN
        ALTER TABLE public.narration_contents
            ADD CONSTRAINT fk_narration_contents_reviewed_by
            FOREIGN KEY (reviewed_by) REFERENCES public.admin_users(admin_id)
            ON UPDATE CASCADE ON DELETE SET NULL;
    END IF;
END $$;

ALTER TABLE public.narration_translations
    ADD COLUMN IF NOT EXISTS status varchar(30) NOT NULL DEFAULT 'PendingReview',
    ADD COLUMN IF NOT EXISTS provider varchar(50),
    ADD COLUMN IF NOT EXISTS error_message text,
    ADD COLUMN IF NOT EXISTS reviewed_at timestamp without time zone;

UPDATE public.narration_translations
SET status = CASE WHEN is_reviewed THEN 'Approved' ELSE 'PendingReview' END
WHERE status IS NULL OR status = 'PendingReview';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'chk_narration_translations_status'
    ) THEN
        ALTER TABLE public.narration_translations
            ADD CONSTRAINT chk_narration_translations_status
            CHECK (status IN ('Pending','Generating','PendingReview','Approved','Rejected','Failed','Outdated'));
    END IF;
END $$;

ALTER TABLE public.audio_files
    ALTER COLUMN audio_url DROP NOT NULL,
    ADD COLUMN IF NOT EXISTS provider varchar(50),
    ADD COLUMN IF NOT EXISTS status varchar(30) NOT NULL DEFAULT 'Pending',
    ADD COLUMN IF NOT EXISTS error_message text,
    ADD COLUMN IF NOT EXISTS source_text_hash varchar(64),
    ADD COLUMN IF NOT EXISTS generated_at timestamp without time zone,
    ADD COLUMN IF NOT EXISTS published_at timestamp without time zone;

UPDATE public.audio_files
SET status = CASE
        WHEN audio_url IS NOT NULL AND btrim(audio_url) <> '' THEN 'Ready'
        ELSE 'Pending'
    END,
    provider = COALESCE(provider, generated_by),
    generated_at = COALESCE(generated_at, created_at)
WHERE status IS NULL OR status = 'Pending';

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'chk_audio_files_status'
    ) THEN
        ALTER TABLE public.audio_files
            ADD CONSTRAINT chk_audio_files_status
            CHECK (status IN ('Pending','Generating','Ready','Failed','Outdated'));
    END IF;
END $$;

-- QR nghe nội dung bị loại bỏ. QR thanh toán của Vendor thuộc payment module khác.
ALTER TABLE public.listening_histories DROP CONSTRAINT IF EXISTS fk_listening_histories_qr;
ALTER TABLE public.listening_histories DROP CONSTRAINT IF EXISTS fk_listening_histories_qr_code;
ALTER TABLE public.listening_histories DROP CONSTRAINT IF EXISTS chk_listening_histories_trigger_source;
UPDATE public.listening_histories SET trigger_source = 'Manual' WHERE trigger_source = 'QR';
ALTER TABLE public.listening_histories ALTER COLUMN trigger_source SET DEFAULT 'Manual';
ALTER TABLE public.listening_histories DROP COLUMN IF EXISTS qr_code_id;
ALTER TABLE public.listening_histories
    ADD CONSTRAINT chk_listening_histories_trigger_source
    CHECK (trigger_source IN ('Geofence','Manual'));

DROP TABLE IF EXISTS public.qr_codes CASCADE;
DROP FUNCTION IF EXISTS public.fn_validate_qr_code_target() CASCADE;
DROP FUNCTION IF EXISTS public.validate_qr_target() CASCADE;

CREATE INDEX IF NOT EXISTS idx_narration_contents_workflow_status
    ON public.narration_contents(workflow_status);
CREATE INDEX IF NOT EXISTS idx_narration_contents_vendor
    ON public.narration_contents(submitted_by_vendor_id);
CREATE INDEX IF NOT EXISTS idx_narration_translations_status
    ON public.narration_translations(status);
CREATE INDEX IF NOT EXISTS idx_audio_files_status
    ON public.audio_files(status);

COMMIT;
