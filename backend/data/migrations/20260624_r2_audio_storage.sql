BEGIN;

ALTER TABLE public.audio_files
    ADD COLUMN IF NOT EXISTS storage_key varchar(1024);

CREATE INDEX IF NOT EXISTS idx_audio_files_storage_key
    ON public.audio_files (storage_key)
    WHERE storage_key IS NOT NULL;

COMMENT ON COLUMN public.audio_files.storage_key IS
    'Provider-specific private object key. For R2 this is the bucket object key; signed URLs are never stored.';

COMMIT;
