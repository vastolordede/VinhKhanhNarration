BEGIN;

-- =========================================================
-- Fixed supported narration languages
-- Do not delete existing rows because translations may reference them.
-- Unsupported rows will be deactivated instead.
-- =========================================================

INSERT INTO languages (
    language_code,
    language_name,
    is_default,
    is_active
)
VALUES
    ('vi', 'Tiếng Việt', TRUE, TRUE),
    ('en', 'English', FALSE, TRUE),
    ('ja', '日本語', FALSE, TRUE),
    ('ko', '한국어', FALSE, TRUE),
    ('zh', '中文', FALSE, TRUE)
ON CONFLICT (language_code) DO UPDATE
SET
    language_name = EXCLUDED.language_name,
    is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP;

-- Only Vietnamese is the default narration language.
UPDATE languages
SET
    is_default = CASE WHEN LOWER(language_code) = 'vi' THEN TRUE ELSE FALSE END,
    updated_at = CURRENT_TIMESTAMP
WHERE LOWER(language_code) IN ('vi', 'en', 'ja', 'ko', 'zh');

-- Deactivate unsupported languages such as the accidental "string" row.
UPDATE languages
SET
    is_active = FALSE,
    is_default = FALSE,
    updated_at = CURRENT_TIMESTAMP
WHERE LOWER(language_code) NOT IN ('vi', 'en', 'ja', 'ko', 'zh');

SELECT
    language_id,
    language_code,
    language_name,
    is_default,
    is_active
FROM languages
ORDER BY
    CASE LOWER(language_code)
        WHEN 'vi' THEN 1
        WHEN 'en' THEN 2
        WHEN 'ja' THEN 3
        WHEN 'ko' THEN 4
        WHEN 'zh' THEN 5
        ELSE 99
    END,
    language_id;

COMMIT;