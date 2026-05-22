-- =========================================================
-- MINIMAL SEED DATA FOR FRONTEND DEMO
-- Project: VinhKhanhNarration
-- =========================================================

-- 1. Languages
INSERT INTO languages (language_code, language_name, is_default, is_active)
VALUES
('vi', 'Tiếng Việt', TRUE, TRUE),
('en', 'English', FALSE, TRUE)
ON CONFLICT (language_code) DO UPDATE
SET language_name = EXCLUDED.language_name,
    is_active = TRUE;


-- 2. Lookup: place_types
INSERT INTO place_types (code, name, description, is_active)
VALUES
('Restaurant', 'Restaurant', 'Nhà hàng / quán ăn', TRUE),
('Area', 'Area', 'Khu vực tham quan', TRUE),
('Other', 'Other', 'Loại khác', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;


-- 3. Lookup: content_types
INSERT INTO content_types (code, name, description, is_active)
VALUES
('Place', 'Place Narration', 'Thuyết minh cho địa điểm', TRUE),
('Dish', 'Dish Narration', 'Thuyết minh cho món ăn', TRUE),
('General', 'General Narration', 'Thuyết minh chung', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;


-- 4. Lookup: target_types
INSERT INTO target_types (code, name, description, is_active)
VALUES
('Place', 'Place Target', 'QR trỏ tới địa điểm', TRUE),
('Dish', 'Dish Target', 'QR trỏ tới món ăn', TRUE),
('Narration', 'Narration Target', 'QR trỏ tới bài thuyết minh', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;


-- 5. Lookup: translation_sources
INSERT INTO translation_sources (code, name, description, is_active)
VALUES
('Manual', 'Manual', 'Dịch thủ công', TRUE),
('AI', 'AI', 'Dịch bằng AI', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;


-- 6. Lookup: trigger_modes
INSERT INTO trigger_modes (code, name, description, is_active)
VALUES
('Enter', 'Enter', 'Kích hoạt khi đi vào vùng POI', TRUE),
('Near', 'Near', 'Kích hoạt khi đến gần POI', TRUE),
('Both', 'Both', 'Kích hoạt cả hai trường hợp', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;


-- 7. Lookup: geofence event types
INSERT INTO geofence_event_types (code, name, description, is_active)
VALUES
('Enter', 'Enter', 'Đi vào vùng POI', TRUE),
('Near', 'Near', 'Đến gần POI', TRUE),
('Exit', 'Exit', 'Rời khỏi POI', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;


-- 8. Lookup: geofence event statuses
INSERT INTO geofence_event_statuses (code, name, description, is_active)
VALUES
('Detected', 'Detected', 'Đã phát hiện', TRUE),
('IgnoredDebounce', 'Ignored Debounce', 'Bỏ qua do debounce', TRUE),
('IgnoredCooldown', 'Ignored Cooldown', 'Bỏ qua do cooldown', TRUE),
('Queued', 'Queued', 'Đã đưa vào hàng chờ', TRUE),
('Played', 'Played', 'Đã phát', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;


-- 9. Admin user giả để làm created_by
INSERT INTO admin_users (full_name, email, password_hash, role, is_active)
SELECT
    'System Admin',
    'admin@vinhkhanh.local',
    '$2a$10$YLcrI6L7gaaeolTeVRDTsOat/mWthdTayXTz/M.IG/EraGbMVobom',
    'Admin',
    TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM admin_users WHERE email = 'admin@vinhkhanh.local'
);


-- 10. Dish category
INSERT INTO dish_categories (category_name, description, is_active)
SELECT
    'Hải sản',
    'Các món hải sản phổ biến tại phố ẩm thực Vĩnh Khánh',
    TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM dish_categories WHERE category_name = 'Hải sản'
);


-- 11. Place / POI mẫu
INSERT INTO places (
    place_name,
    place_type_id,
    address,
    description,
    latitude,
    longitude,
    opening_hours,
    image_url,
    is_poi,
    is_geofence_enabled,
    trigger_radius_meters,
    priority,
    trigger_mode_id,
    debounce_seconds,
    cooldown_seconds,
    is_active
)
SELECT
    'Quán Ốc Vĩnh Khánh',
    (SELECT place_type_id FROM place_types WHERE code = 'Restaurant'),
    'Đường Vĩnh Khánh, Quận 4, TP.HCM',
    'Quán ốc nổi bật tại phố ẩm thực Vĩnh Khánh, phù hợp cho khách muốn trải nghiệm không khí ăn uống đường phố.',
    10.7569000,
    106.7057000,
    '17:00 - 23:30',
    '',
    TRUE,
    TRUE,
    80,
    10,
    (SELECT trigger_mode_id FROM trigger_modes WHERE code = 'Both'),
    10,
    300,
    TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM places WHERE place_name = 'Quán Ốc Vĩnh Khánh'
);


-- 12. Dish mẫu
INSERT INTO dishes (
    dish_name,
    category_id,
    description,
    image_url,
    average_price,
    is_signature_dish,
    is_active
)
SELECT
    'Ốc len xào dừa',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Ốc len xào dừa là món ăn có vị béo thơm từ nước cốt dừa, thường được nhiều khách chọn khi ăn ốc.',
    '',
    85000,
    TRUE,
    TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM dishes WHERE dish_name = 'Ốc len xào dừa'
);


-- 13. Gán món vào quán
INSERT INTO place_dishes (
    place_id,
    dish_id,
    price,
    is_recommended,
    note
)
SELECT
    (SELECT place_id FROM places WHERE place_name = 'Quán Ốc Vĩnh Khánh'),
    (SELECT dish_id FROM dishes WHERE dish_name = 'Ốc len xào dừa'),
    85000,
    TRUE,
    'Món nên thử khi ghé quán'
WHERE NOT EXISTS (
    SELECT 1 FROM place_dishes
    WHERE place_id = (SELECT place_id FROM places WHERE place_name = 'Quán Ốc Vĩnh Khánh')
      AND dish_id = (SELECT dish_id FROM dishes WHERE dish_name = 'Ốc len xào dừa')
);


-- 14. Narration content cho quán
INSERT INTO narration_contents (
    title,
    original_text,
    content_type_id,
    place_id,
    dish_id,
    created_by,
    is_active
)
SELECT
    'Giới thiệu Quán Ốc Vĩnh Khánh',
    'Quán Ốc Vĩnh Khánh là một điểm dừng chân tiêu biểu trong khu phố ẩm thực Vĩnh Khánh. Nơi đây nổi bật với các món ốc, hải sản và không khí ăn uống đường phố sôi động về đêm.',
    (SELECT content_type_id FROM content_types WHERE code = 'Place'),
    (SELECT place_id FROM places WHERE place_name = 'Quán Ốc Vĩnh Khánh'),
    NULL,
    (SELECT admin_id FROM admin_users WHERE email = 'admin@vinhkhanh.local'),
    TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM narration_contents
    WHERE title = 'Giới thiệu Quán Ốc Vĩnh Khánh'
);


-- 15. Translation tiếng Việt
INSERT INTO narration_translations (
    narration_id,
    language_id,
    translated_title,
    translated_text,
    translation_source_id,
    reviewed_by,
    is_reviewed
)
SELECT
    (SELECT narration_id FROM narration_contents WHERE title = 'Giới thiệu Quán Ốc Vĩnh Khánh'),
    (SELECT language_id FROM languages WHERE language_code = 'vi'),
    'Giới thiệu Quán Ốc Vĩnh Khánh',
    'Quán Ốc Vĩnh Khánh là một điểm dừng chân tiêu biểu trong khu phố ẩm thực Vĩnh Khánh. Nơi đây nổi bật với các món ốc, hải sản và không khí ăn uống đường phố sôi động về đêm.',
    (SELECT translation_source_id FROM translation_sources WHERE code = 'Manual'),
    (SELECT admin_id FROM admin_users WHERE email = 'admin@vinhkhanh.local'),
    TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM narration_translations
    WHERE narration_id = (SELECT narration_id FROM narration_contents WHERE title = 'Giới thiệu Quán Ốc Vĩnh Khánh')
      AND language_id = (SELECT language_id FROM languages WHERE language_code = 'vi')
);


-- 16. Audio file rỗng để frontend dùng Web Speech API
INSERT INTO audio_files (
    translation_id,
    audio_url,
    voice_name,
    voice_gender,
    duration_seconds,
    file_format,
    generated_by,
    is_active
)
SELECT
    (SELECT nt.translation_id
     FROM narration_translations nt
     JOIN narration_contents nc ON nc.narration_id = nt.narration_id
     JOIN languages l ON l.language_id = nt.language_id
     WHERE nc.title = 'Giới thiệu Quán Ốc Vĩnh Khánh'
       AND l.language_code = 'vi'),
    '',
    'Browser TTS',
    'Neutral',
    NULL,
    'mp3',
    'TTS',
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM audio_files af
    WHERE af.translation_id = (
        SELECT nt.translation_id
        FROM narration_translations nt
        JOIN narration_contents nc ON nc.narration_id = nt.narration_id
        JOIN languages l ON l.language_id = nt.language_id
        WHERE nc.title = 'Giới thiệu Quán Ốc Vĩnh Khánh'
          AND l.language_code = 'vi'
    )
);


-- 17. QR test
INSERT INTO qr_codes (
    qr_code_value,
    qr_code_image_url,
    target_type_id,
    place_id,
    dish_id,
    narration_id,
    is_active
)
SELECT
    'QR_PLACE_OC_VINH_KHANH',
    '',
    (SELECT target_type_id FROM target_types WHERE code = 'Place'),
    (SELECT place_id FROM places WHERE place_name = 'Quán Ốc Vĩnh Khánh'),
    NULL,
    NULL,
    TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM qr_codes WHERE qr_code_value = 'QR_PLACE_OC_VINH_KHANH'
);


-- 18. Check tối thiểu
SELECT 'languages' AS table_name, COUNT(*) AS total FROM languages
UNION ALL
SELECT 'place_types', COUNT(*) FROM place_types
UNION ALL
SELECT 'content_types', COUNT(*) FROM content_types
UNION ALL
SELECT 'target_types', COUNT(*) FROM target_types
UNION ALL
SELECT 'places', COUNT(*) FROM places
UNION ALL
SELECT 'dishes', COUNT(*) FROM dishes
UNION ALL
SELECT 'place_dishes', COUNT(*) FROM place_dishes
UNION ALL
SELECT 'narration_contents', COUNT(*) FROM narration_contents
UNION ALL
SELECT 'narration_translations', COUNT(*) FROM narration_translations
UNION ALL
SELECT 'audio_files', COUNT(*) FROM audio_files
UNION ALL
SELECT 'qr_codes', COUNT(*) FROM qr_codes;