BEGIN;

UPDATE public.languages
SET is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP
WHERE language_code IN ('vi', 'en', 'ja', 'ko', 'zh');

UPDATE public.audio_files
SET is_active = FALSE,
    updated_at = CURRENT_TIMESTAMP
WHERE audio_url IS NULL OR btrim(audio_url) = '';

INSERT INTO public.translation_sources (code, name, description, is_active)
VALUES ('AI', 'AI', 'Dịch tự động bằng API dịch', TRUE)
ON CONFLICT (code) DO UPDATE
SET is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP;

COMMIT;

SELECT
    nc.narration_id,
    nc.title,
    string_agg(l.language_code, ', ' ORDER BY l.language_code) AS missing_languages
FROM public.narration_contents nc
CROSS JOIN public.languages l
LEFT JOIN public.narration_translations nt
    ON nt.narration_id = nc.narration_id
   AND nt.language_id = l.language_id
WHERE nc.is_active = TRUE
  AND l.is_active = TRUE
  AND l.language_code IN ('vi', 'en', 'ja', 'ko', 'zh')
  AND nt.translation_id IS NULL
GROUP BY nc.narration_id, nc.title
ORDER BY nc.narration_id;