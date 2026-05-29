-- =========================================================
-- FULL DEMO SEED DATA FOR DEPLOY-LIKE USER EXPERIENCE
-- Project: VinhKhanhNarration
-- Run order:
--   1) schema.sql
--   2) seed_minimal.sql
--   3) this file
--
-- Notes:
-- - This file does NOT replace schema.sql or seed_minimal.sql.
-- - It is intentionally larger than seed_demo_vinhkhanh_extended.sql.
-- - All demo addresses follow the same pattern for geocoding consistency:
--   "<number> Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam"
-- - Coordinates are pre-filled so the map can render immediately even before geocoding.
-- =========================================================

BEGIN;


-- =========================================================
-- 0. Ensure required base data exists
-- =========================================================

INSERT INTO languages (language_code, language_name, is_default, is_active)
VALUES
('vi', 'Tiếng Việt', TRUE, TRUE),
('en', 'English', FALSE, TRUE)
ON CONFLICT (language_code) DO UPDATE
SET language_name = EXCLUDED.language_name,
    is_active = TRUE;

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

INSERT INTO place_types (code, name, description, is_active)
VALUES
('Restaurant', 'Restaurant', 'Nhà hàng / quán ăn', TRUE),
('FoodStall', 'Food Stall', 'Gian hàng / quầy bán đồ ăn', TRUE),
('Area', 'Area', 'Khu vực tham quan', TRUE),
('CheckInPoint', 'Check-in Point', 'Điểm check-in', TRUE),
('Other', 'Other', 'Loại khác', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;

INSERT INTO content_types (code, name, description, is_active)
VALUES
('Place', 'Place Narration', 'Thuyết minh cho địa điểm', TRUE),
('Dish', 'Dish Narration', 'Thuyết minh cho món ăn', TRUE),
('General', 'General Narration', 'Thuyết minh chung', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;

INSERT INTO target_types (code, name, description, is_active)
VALUES
('Place', 'Place Target', 'QR trỏ tới địa điểm', TRUE),
('Dish', 'Dish Target', 'QR trỏ tới món ăn', TRUE),
('Narration', 'Narration Target', 'QR trỏ tới bài thuyết minh', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;

INSERT INTO translation_sources (code, name, description, is_active)
VALUES
('Manual', 'Manual', 'Dịch thủ công', TRUE),
('AI', 'AI', 'Dịch bằng AI', TRUE),
('GoogleTranslate', 'Google Translate', 'Dịch bằng Google Translate', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;

INSERT INTO trigger_modes (code, name, description, is_active)
VALUES
('Enter', 'Enter', 'Kích hoạt khi đi vào vùng POI', TRUE),
('Near', 'Near', 'Kích hoạt khi đến gần POI', TRUE),
('Both', 'Both', 'Kích hoạt cả hai trường hợp', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;

INSERT INTO geofence_event_types (code, name, description, is_active)
VALUES
('Enter', 'Enter', 'Đi vào vùng POI', TRUE),
('Near', 'Near', 'Đến gần POI', TRUE),
('Exit', 'Exit', 'Rời khỏi POI', TRUE)
ON CONFLICT (code) DO UPDATE
SET name = EXCLUDED.name,
    description = EXCLUDED.description,
    is_active = TRUE;

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


-- =========================================================
-- 1. Rich dish categories
-- =========================================================

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Hải sản', 'Các món ốc, nghêu, sò, tôm, mực và hải sản đặc trưng của phố ẩm thực Vĩnh Khánh.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Hải sản');

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Món nướng', 'Các món nướng than, xiên que, bò, gà và hải sản nướng.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Món nướng');

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Lẩu', 'Các món lẩu phù hợp nhóm bạn, gia đình và khách du lịch.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Lẩu');

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Cơm - bún - mì', 'Các món ăn no phổ biến cho bữa tối và ăn khuya.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Cơm - bún - mì');

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Ăn vặt', 'Các món ăn nhẹ, khai vị và món đường phố dễ gọi.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Ăn vặt');

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Đồ uống', 'Nước giải khát, trà, sinh tố và đồ uống dùng kèm.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Đồ uống');

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Tráng miệng', 'Chè, bánh flan, rau câu và món ngọt sau bữa ăn.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Tráng miệng');

INSERT INTO dish_categories (category_name, description, is_active)
SELECT 'Món chay / Healthy', 'Một số lựa chọn nhẹ bụng, ít dầu mỡ hoặc phù hợp ăn chay.', TRUE
WHERE NOT EXISTS (SELECT 1 FROM dish_categories WHERE category_name = 'Món chay / Healthy');


-- =========================================================
-- 2. Rich dish menu
-- =========================================================

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Ốc len xào dừa',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Ốc len béo thơm nước cốt dừa, vị ngọt mặn hài hòa.',
    '',
    85000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Ốc len xào dừa');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Ốc hương rang muối',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Ốc hương rang muối ớt, thơm và đậm vị.',
    '',
    120000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Ốc hương rang muối');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Nghêu hấp sả',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Nghêu hấp sả nóng, nước dùng thơm nhẹ.',
    '',
    65000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Nghêu hấp sả');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Sò điệp nướng mỡ hành',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Sò điệp nướng mỡ hành và đậu phộng.',
    '',
    95000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Sò điệp nướng mỡ hành');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Mực nướng sa tế',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Mực nướng sa tế cay nhẹ, phù hợp dùng chung.',
    '',
    135000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Mực nướng sa tế');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Tôm rang me',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Tôm rang sốt me chua ngọt.',
    '',
    145000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Tôm rang me');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Càng ghẹ rang muối',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Càng ghẹ rang muối đậm vị, hợp ăn nhóm.',
    '',
    160000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Càng ghẹ rang muối');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Hàu nướng phô mai',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Hải sản'),
    'Hàu nướng phủ phô mai béo nhẹ.',
    '',
    110000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Hàu nướng phô mai');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Bò nướng lá lốt',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món nướng'),
    'Bò cuốn lá lốt nướng than, ăn cùng rau sống.',
    '',
    70000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Bò nướng lá lốt');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Ba chỉ bò nướng',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món nướng'),
    'Ba chỉ bò ướp sốt, nướng tại bàn hoặc nướng sẵn.',
    '',
    129000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Ba chỉ bò nướng');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Gà xiên nướng',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món nướng'),
    'Xiên gà nướng thơm, dễ gọi cho nhóm nhỏ.',
    '',
    35000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Gà xiên nướng');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Bạch tuộc nướng',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món nướng'),
    'Bạch tuộc nướng giòn dai, sốt cay nhẹ.',
    '',
    140000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Bạch tuộc nướng');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Sườn nướng mật ong',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món nướng'),
    'Sườn nướng mật ong mềm, vị ngọt nhẹ.',
    '',
    125000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Sườn nướng mật ong');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Lẩu Thái hải sản',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Lẩu'),
    'Lẩu Thái vị chua cay, dùng kèm hải sản và rau.',
    '',
    220000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Lẩu Thái hải sản');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Lẩu cua đồng',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Lẩu'),
    'Lẩu cua đồng vị thanh, hợp nhóm 3-4 người.',
    '',
    210000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Lẩu cua đồng');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Lẩu bò sa tế',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Lẩu'),
    'Lẩu bò sa tế cay thơm, nước dùng đậm.',
    '',
    240000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Lẩu bò sa tế');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Lẩu cá kèo',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Lẩu'),
    'Lẩu cá kèo miền Nam ăn cùng rau đắng.',
    '',
    230000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Lẩu cá kèo');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Cơm tấm sườn bì chả',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Cơm - bún - mì'),
    'Cơm tấm sườn, bì, chả và nước mắm pha.',
    '',
    65000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Cơm tấm sườn bì chả');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Bún mắm',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Cơm - bún - mì'),
    'Bún mắm đậm vị miền Tây, nhiều topping.',
    '',
    75000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Bún mắm');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Mì xào hải sản',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Cơm - bún - mì'),
    'Mì xào cùng tôm, mực và rau.',
    '',
    80000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Mì xào hải sản');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Bún đậu mẹt nhỏ',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Cơm - bún - mì'),
    'Bún đậu, đậu chiên, chả cốm và rau ăn kèm.',
    '',
    89000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Bún đậu mẹt nhỏ');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Bánh tráng nướng',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Ăn vặt'),
    'Bánh tráng nướng trứng, hành, xúc xích và phô mai.',
    '',
    35000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Bánh tráng nướng');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Khoai tây lắc phô mai',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Ăn vặt'),
    'Khoai tây chiên lắc phô mai.',
    '',
    45000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Khoai tây lắc phô mai');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Gỏi xoài khô mực',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Ăn vặt'),
    'Gỏi xoài chua ngọt ăn cùng khô mực.',
    '',
    65000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Gỏi xoài khô mực');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Chân gà sả tắc',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Ăn vặt'),
    'Chân gà ngâm sả tắc giòn, vị chua cay.',
    '',
    70000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Chân gà sả tắc');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Trà tắc',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Đồ uống'),
    'Trà tắc mát, dễ uống cùng món cay.',
    '',
    20000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Trà tắc');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Nước sâm',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Đồ uống'),
    'Nước sâm thanh mát.',
    '',
    18000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Nước sâm');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Sinh tố bơ',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Đồ uống'),
    'Sinh tố bơ béo nhẹ.',
    '',
    45000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Sinh tố bơ');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Trà đào cam sả',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Đồ uống'),
    'Trà đào cam sả thơm mát.',
    '',
    39000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Trà đào cam sả');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Chè khúc bạch',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Tráng miệng'),
    'Chè khúc bạch mát, vị ngọt nhẹ.',
    '',
    42000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Chè khúc bạch');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Bánh flan caramel',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Tráng miệng'),
    'Bánh flan mềm mịn, sốt caramel.',
    '',
    28000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Bánh flan caramel');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Rau câu dừa',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Tráng miệng'),
    'Rau câu dừa mát, phù hợp sau bữa ăn.',
    '',
    30000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Rau câu dừa');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Gỏi cuốn chay',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món chay / Healthy'),
    'Gỏi cuốn rau củ, bún và nước chấm nhẹ.',
    '',
    45000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Gỏi cuốn chay');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Salad rau củ',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món chay / Healthy'),
    'Salad rau củ tươi, ít dầu mỡ.',
    '',
    59000,
    FALSE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Salad rau củ');

INSERT INTO dishes (dish_name, category_id, description, image_url, average_price, is_signature_dish, is_active)
SELECT
    'Đậu hũ nấm sốt tiêu',
    (SELECT category_id FROM dish_categories WHERE category_name = 'Món chay / Healthy'),
    'Đậu hũ và nấm sốt tiêu, dùng nóng.',
    '',
    72000,
    TRUE,
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM dishes WHERE dish_name = 'Đậu hũ nấm sốt tiêu');


-- =========================================================
-- 3. Demo places on Đ. Vĩnh Khánh
-- =========================================================

DROP TABLE IF EXISTS demo_vinhkhanh_places;
CREATE TEMP TABLE demo_vinhkhanh_places (
    place_name TEXT,
    house_no INTEGER,
    place_type_code TEXT,
    latitude NUMERIC(10,7),
    longitude NUMERIC(10,7),
    opening_hours TEXT,
    description TEXT,
    radius INTEGER,
    priority INTEGER,
    trigger_mode_code TEXT,
    slug TEXT
) ON COMMIT DROP;

INSERT INTO demo_vinhkhanh_places (
    place_name, house_no, place_type_code, latitude, longitude,
    opening_hours, description, radius, priority, trigger_mode_code, slug
)
VALUES

('Quán Ốc Vĩnh Khánh', 40, 'Restaurant', 10.7614345, 106.7027862, '17:00 - 23:30', 'Quán ốc nổi bật tại phố ẩm thực Vĩnh Khánh, phù hợp cho khách muốn trải nghiệm không khí ăn uống đường phố.', 80, 10, 'Both', 'QUAN_OC_VINH_KHANH'),
('Ốc Đêm Vĩnh Khánh', 22, 'Restaurant', 10.7621500, 106.7018200, '16:30 - 00:30', 'Quán ốc buổi tối với nhiều món xào, hấp và nướng, không khí nhộn nhịp.', 75, 9, 'Both', 'OC_DEM_VINH_KHANH'),
('Hải Sản Vĩnh Khánh 58', 58, 'Restaurant', 10.7610800, 106.7031200, '16:00 - 23:45', 'Quán hải sản bình dân, nổi bật với tôm, mực và các món nghêu sò.', 85, 9, 'Both', 'HAI_SAN_VINH_KHANH_58'),
('Sò Điệp Góc Phố', 72, 'FoodStall', 10.7607800, 106.7035200, '17:00 - 23:00', 'Quầy hải sản nhỏ chuyên sò điệp nướng mỡ hành và món ăn nhanh.', 60, 7, 'Near', 'SO_DIEP_GOC_PHO'),
('Quán Nướng Vĩnh Khánh 88', 88, 'Restaurant', 10.7604800, 106.7038500, '17:30 - 23:30', 'Điểm nướng than dành cho nhóm bạn, có nhiều món bò, gà và hải sản nướng.', 75, 8, 'Both', 'QUAN_NUONG_VINH_KHANH_88'),
('Bạch Tuộc Nướng 102', 102, 'FoodStall', 10.7602500, 106.7041400, '17:00 - 23:30', 'Quầy bạch tuộc nướng sa tế, phục vụ nhanh, phù hợp ăn vặt buổi tối.', 55, 7, 'Near', 'BACH_TUOC_NUONG_102'),
('Lẩu Thái Vĩnh Khánh 118', 118, 'Restaurant', 10.7600300, 106.7044200, '16:30 - 23:45', 'Quán lẩu Thái hải sản với nước dùng chua cay và không gian phù hợp nhóm.', 80, 8, 'Both', 'LAU_THAI_VINH_KHANH_118'),
('Cơm Tấm Đêm 136', 136, 'Restaurant', 10.7598300, 106.7047000, '18:00 - 01:00', 'Quán cơm tấm mở tối, phù hợp khách muốn ăn no trước khi dạo phố.', 65, 6, 'Enter', 'C_M_TAM_DEM_136'),
('Bún Mắm Miền Tây 154', 154, 'Restaurant', 10.7596500, 106.7049300, '15:30 - 22:30', 'Quán bún mắm miền Tây đậm vị, nhiều topping hải sản và rau ăn kèm.', 65, 7, 'Both', 'BUN_MAM_MIEN_TAY_154'),
('Bún Đậu Vĩnh Khánh 168', 168, 'FoodStall', 10.7594500, 106.7052000, '15:00 - 22:30', 'Quầy bún đậu mẹt nhỏ, phù hợp ăn nhanh hoặc đi nhóm ít người.', 55, 6, 'Near', 'BUN_DAU_VINH_KHANH_168'),
('Ăn Vặt Vĩnh Khánh 182', 182, 'FoodStall', 10.7592300, 106.7054600, '15:00 - 23:00', 'Khu ăn vặt có bánh tráng nướng, khoai lắc và nhiều món nhẹ.', 55, 6, 'Near', 'AN_VAT_VINH_KHANH_182'),
('Chân Gà Sả Tắc 198', 198, 'FoodStall', 10.7590200, 106.7057200, '16:00 - 23:30', 'Quầy chân gà sả tắc và gỏi xoài, vị chua cay dễ ăn.', 50, 6, 'Near', 'CHAN_GA_SA_TAC_198'),
('Lẩu Bò Sa Tế 210', 210, 'Restaurant', 10.7588400, 106.7059600, '17:00 - 00:00', 'Quán lẩu bò sa tế đậm vị, thích hợp ăn tối và ăn khuya.', 80, 8, 'Both', 'LAU_BO_SA_TE_210'),
('Mì Xào Hải Sản 226', 226, 'FoodStall', 10.7586500, 106.7062300, '16:30 - 23:00', 'Quầy mì xào hải sản phục vụ nhanh, giá dễ tiếp cận.', 55, 5, 'Near', 'MI_XAO_HAI_SAN_226'),
('Trà Tắc Vĩnh Khánh 240', 240, 'FoodStall', 10.7584500, 106.7064800, '14:00 - 00:30', 'Quầy nước giải khát, phù hợp dừng chân giữa tuyến tham quan.', 45, 5, 'Near', 'TRA_TAC_VINH_KHANH_240'),
('Chè Khúc Bạch 252', 252, 'FoodStall', 10.7582500, 106.7067400, '14:30 - 23:30', 'Quán chè và tráng miệng nhẹ sau bữa ăn.', 45, 5, 'Near', 'CHE_KHUC_BACH_252'),
('Gỏi Cuốn Healthy 268', 268, 'FoodStall', 10.7580400, 106.7070000, '10:00 - 21:30', 'Lựa chọn nhẹ bụng với gỏi cuốn, salad và món chay đơn giản.', 50, 5, 'Enter', 'GOI_CUON_HEALTHY_268'),
('Hàu Nướng Phô Mai 282', 282, 'FoodStall', 10.7578200, 106.7072600, '17:00 - 23:30', 'Quầy hàu nướng phô mai và các món hải sản nướng nhanh.', 55, 6, 'Near', 'HAU_NUONG_PHO_MAI_282'),
('Quán Cua Đồng 300', 300, 'Restaurant', 10.7576000, 106.7075200, '16:00 - 22:30', 'Quán lẩu cua đồng và món dân dã, phù hợp khách muốn món vị thanh.', 75, 6, 'Both', 'QUAN_CUA_DONG_300'),
('Xiên Que Vĩnh Khánh 318', 318, 'FoodStall', 10.7573800, 106.7077800, '15:00 - 23:30', 'Quầy xiên que nướng và đồ ăn vặt cho khách đi dạo.', 50, 5, 'Near', 'XIEN_QUE_VINH_KHANH_318'),
('Cổng Phố Ẩm Thực Vĩnh Khánh', 1, 'CheckInPoint', 10.7624000, 106.7015200, '08:00 - 23:59', 'Điểm bắt đầu tuyến trải nghiệm phố ẩm thực Vĩnh Khánh.', 90, 10, 'Enter', 'CONG_PHO_AM_THUC_VINH_KHANH'),
('Khu Check-in Ánh Đèn Vĩnh Khánh', 188, 'CheckInPoint', 10.7591600, 106.7055500, '18:00 - 23:30', 'Vị trí check-in khi phố lên đèn, gần nhiều quán ăn vặt.', 70, 8, 'Near', 'KHU_CHECK_IN_ANH_DEN_VINH_KHANH'),
('Khu Hải Sản Tập Trung', 60, 'Area', 10.7609700, 106.7032300, '16:00 - 23:45', 'Khu vực tập trung nhiều quán hải sản và món ốc.', 100, 9, 'Both', 'KHU_HAI_SAN_TAP_TRUNG'),
('Khu Ăn Khuya Vĩnh Khánh', 230, 'Area', 10.7585600, 106.7063500, '18:00 - 01:00', 'Khu vực có nhiều món ăn no và ăn khuya trên tuyến Vĩnh Khánh.', 100, 8, 'Both', 'KHU_AN_KHUYA_VINH_KHANH');


INSERT INTO places (
    place_name, place_type_id, address, description, latitude, longitude,
    opening_hours, image_url, is_poi, is_geofence_enabled,
    trigger_radius_meters, priority, trigger_mode_id,
    debounce_seconds, cooldown_seconds, is_active
)
SELECT
    dp.place_name,
    pt.place_type_id,
    CASE
        WHEN dp.house_no = 1 THEN 'Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
        ELSE dp.house_no || ' Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
    END,
    dp.description,
    dp.latitude,
    dp.longitude,
    dp.opening_hours,
    '',
    TRUE,
    TRUE,
    dp.radius,
    dp.priority,
    tm.trigger_mode_id,
    10,
    300,
    TRUE
FROM demo_vinhkhanh_places dp
JOIN place_types pt ON pt.code = dp.place_type_code
JOIN trigger_modes tm ON tm.code = dp.trigger_mode_code
WHERE NOT EXISTS (
    SELECT 1 FROM places p WHERE p.place_name = dp.place_name
);

UPDATE places p
SET address = CASE
        WHEN dp.house_no = 1 THEN 'Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
        ELSE dp.house_no || ' Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
    END,
    description = dp.description,
    latitude = dp.latitude,
    longitude = dp.longitude,
    opening_hours = dp.opening_hours,
    is_poi = TRUE,
    is_geofence_enabled = TRUE,
    trigger_radius_meters = dp.radius,
    priority = dp.priority,
    place_type_id = pt.place_type_id,
    trigger_mode_id = tm.trigger_mode_id,
    debounce_seconds = 10,
    cooldown_seconds = 300,
    is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP
FROM demo_vinhkhanh_places dp
JOIN place_types pt ON pt.code = dp.place_type_code
JOIN trigger_modes tm ON tm.code = dp.trigger_mode_code
WHERE p.place_name = dp.place_name;


-- =========================================================
-- 4. Menu items for each place
-- =========================================================

DROP TABLE IF EXISTS demo_vinhkhanh_place_dishes;
CREATE TEMP TABLE demo_vinhkhanh_place_dishes (
    place_name TEXT,
    dish_name TEXT,
    price NUMERIC(12,2),
    is_recommended BOOLEAN,
    note TEXT
) ON COMMIT DROP;

INSERT INTO demo_vinhkhanh_place_dishes (place_name, dish_name, price, is_recommended, note)
VALUES

('Quán Ốc Vĩnh Khánh', 'Ốc len xào dừa', 85000, TRUE, 'Món nên thử khi ghé quán'),
('Quán Ốc Vĩnh Khánh', 'Ốc hương rang muối', 120000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Ốc Vĩnh Khánh', 'Nghêu hấp sả', 65000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Ốc Vĩnh Khánh', 'Sò điệp nướng mỡ hành', 95000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Ốc Vĩnh Khánh', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Ốc Đêm Vĩnh Khánh', 'Ốc hương rang muối', 120000, TRUE, 'Món nên thử khi ghé quán'),
('Ốc Đêm Vĩnh Khánh', 'Nghêu hấp sả', 65000, FALSE, 'Món dùng kèm phổ biến'),
('Ốc Đêm Vĩnh Khánh', 'Càng ghẹ rang muối', 160000, FALSE, 'Món dùng kèm phổ biến'),
('Ốc Đêm Vĩnh Khánh', 'Chân gà sả tắc', 70000, FALSE, 'Món dùng kèm phổ biến'),
('Ốc Đêm Vĩnh Khánh', 'Nước sâm', 18000, FALSE, 'Món dùng kèm phổ biến'),
('Hải Sản Vĩnh Khánh 58', 'Tôm rang me', 145000, TRUE, 'Món nên thử khi ghé quán'),
('Hải Sản Vĩnh Khánh 58', 'Mực nướng sa tế', 135000, FALSE, 'Món dùng kèm phổ biến'),
('Hải Sản Vĩnh Khánh 58', 'Càng ghẹ rang muối', 160000, FALSE, 'Món dùng kèm phổ biến'),
('Hải Sản Vĩnh Khánh 58', 'Hàu nướng phô mai', 110000, FALSE, 'Món dùng kèm phổ biến'),
('Hải Sản Vĩnh Khánh 58', 'Trà đào cam sả', 39000, FALSE, 'Món dùng kèm phổ biến'),
('Sò Điệp Góc Phố', 'Sò điệp nướng mỡ hành', 95000, TRUE, 'Món nên thử khi ghé quán'),
('Sò Điệp Góc Phố', 'Hàu nướng phô mai', 110000, FALSE, 'Món dùng kèm phổ biến'),
('Sò Điệp Góc Phố', 'Bánh tráng nướng', 35000, FALSE, 'Món dùng kèm phổ biến'),
('Sò Điệp Góc Phố', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Nướng Vĩnh Khánh 88', 'Ba chỉ bò nướng', 129000, TRUE, 'Món nên thử khi ghé quán'),
('Quán Nướng Vĩnh Khánh 88', 'Bò nướng lá lốt', 70000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Nướng Vĩnh Khánh 88', 'Sườn nướng mật ong', 125000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Nướng Vĩnh Khánh 88', 'Bạch tuộc nướng', 140000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Nướng Vĩnh Khánh 88', 'Trà đào cam sả', 39000, FALSE, 'Món dùng kèm phổ biến'),
('Bạch Tuộc Nướng 102', 'Bạch tuộc nướng', 140000, TRUE, 'Món nên thử khi ghé quán'),
('Bạch Tuộc Nướng 102', 'Mực nướng sa tế', 135000, FALSE, 'Món dùng kèm phổ biến'),
('Bạch Tuộc Nướng 102', 'Gỏi xoài khô mực', 65000, FALSE, 'Món dùng kèm phổ biến'),
('Bạch Tuộc Nướng 102', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Lẩu Thái Vĩnh Khánh 118', 'Lẩu Thái hải sản', 220000, TRUE, 'Món nên thử khi ghé quán'),
('Lẩu Thái Vĩnh Khánh 118', 'Tôm rang me', 145000, FALSE, 'Món dùng kèm phổ biến'),
('Lẩu Thái Vĩnh Khánh 118', 'Mực nướng sa tế', 135000, FALSE, 'Món dùng kèm phổ biến'),
('Lẩu Thái Vĩnh Khánh 118', 'Nghêu hấp sả', 65000, FALSE, 'Món dùng kèm phổ biến'),
('Lẩu Thái Vĩnh Khánh 118', 'Nước sâm', 18000, FALSE, 'Món dùng kèm phổ biến'),
('Cơm Tấm Đêm 136', 'Cơm tấm sườn bì chả', 65000, TRUE, 'Món nên thử khi ghé quán'),
('Cơm Tấm Đêm 136', 'Gà xiên nướng', 35000, FALSE, 'Món dùng kèm phổ biến'),
('Cơm Tấm Đêm 136', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Cơm Tấm Đêm 136', 'Bánh flan caramel', 28000, FALSE, 'Món dùng kèm phổ biến'),
('Bún Mắm Miền Tây 154', 'Bún mắm', 75000, TRUE, 'Món nên thử khi ghé quán'),
('Bún Mắm Miền Tây 154', 'Nghêu hấp sả', 65000, FALSE, 'Món dùng kèm phổ biến'),
('Bún Mắm Miền Tây 154', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Bún Mắm Miền Tây 154', 'Rau câu dừa', 30000, FALSE, 'Món dùng kèm phổ biến'),
('Bún Đậu Vĩnh Khánh 168', 'Bún đậu mẹt nhỏ', 89000, TRUE, 'Món nên thử khi ghé quán'),
('Bún Đậu Vĩnh Khánh 168', 'Chân gà sả tắc', 70000, FALSE, 'Món dùng kèm phổ biến'),
('Bún Đậu Vĩnh Khánh 168', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Bún Đậu Vĩnh Khánh 168', 'Khoai tây lắc phô mai', 45000, FALSE, 'Món dùng kèm phổ biến'),
('Ăn Vặt Vĩnh Khánh 182', 'Bánh tráng nướng', 35000, TRUE, 'Món nên thử khi ghé quán'),
('Ăn Vặt Vĩnh Khánh 182', 'Khoai tây lắc phô mai', 45000, FALSE, 'Món dùng kèm phổ biến'),
('Ăn Vặt Vĩnh Khánh 182', 'Gỏi xoài khô mực', 65000, FALSE, 'Món dùng kèm phổ biến'),
('Ăn Vặt Vĩnh Khánh 182', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Chân Gà Sả Tắc 198', 'Chân gà sả tắc', 70000, TRUE, 'Món nên thử khi ghé quán'),
('Chân Gà Sả Tắc 198', 'Gỏi xoài khô mực', 65000, FALSE, 'Món dùng kèm phổ biến'),
('Chân Gà Sả Tắc 198', 'Bánh tráng nướng', 35000, FALSE, 'Món dùng kèm phổ biến'),
('Chân Gà Sả Tắc 198', 'Trà đào cam sả', 39000, FALSE, 'Món dùng kèm phổ biến'),
('Lẩu Bò Sa Tế 210', 'Lẩu bò sa tế', 240000, TRUE, 'Món nên thử khi ghé quán'),
('Lẩu Bò Sa Tế 210', 'Ba chỉ bò nướng', 129000, TRUE, 'Món nên thử khi ghé quán'),
('Lẩu Bò Sa Tế 210', 'Bò nướng lá lốt', 70000, FALSE, 'Món dùng kèm phổ biến'),
('Lẩu Bò Sa Tế 210', 'Nước sâm', 18000, FALSE, 'Món dùng kèm phổ biến'),
('Mì Xào Hải Sản 226', 'Mì xào hải sản', 80000, TRUE, 'Món nên thử khi ghé quán'),
('Mì Xào Hải Sản 226', 'Tôm rang me', 145000, FALSE, 'Món dùng kèm phổ biến'),
('Mì Xào Hải Sản 226', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Mì Xào Hải Sản 226', 'Bánh flan caramel', 28000, FALSE, 'Món dùng kèm phổ biến'),
('Trà Tắc Vĩnh Khánh 240', 'Trà tắc', 20000, TRUE, 'Món nên thử khi ghé quán'),
('Trà Tắc Vĩnh Khánh 240', 'Nước sâm', 18000, FALSE, 'Món dùng kèm phổ biến'),
('Trà Tắc Vĩnh Khánh 240', 'Trà đào cam sả', 39000, FALSE, 'Món dùng kèm phổ biến'),
('Trà Tắc Vĩnh Khánh 240', 'Sinh tố bơ', 45000, FALSE, 'Món dùng kèm phổ biến'),
('Chè Khúc Bạch 252', 'Chè khúc bạch', 42000, TRUE, 'Món nên thử khi ghé quán'),
('Chè Khúc Bạch 252', 'Bánh flan caramel', 28000, FALSE, 'Món dùng kèm phổ biến'),
('Chè Khúc Bạch 252', 'Rau câu dừa', 30000, FALSE, 'Món dùng kèm phổ biến'),
('Chè Khúc Bạch 252', 'Sinh tố bơ', 45000, FALSE, 'Món dùng kèm phổ biến'),
('Gỏi Cuốn Healthy 268', 'Gỏi cuốn chay', 45000, TRUE, 'Món nên thử khi ghé quán'),
('Gỏi Cuốn Healthy 268', 'Salad rau củ', 59000, FALSE, 'Món dùng kèm phổ biến'),
('Gỏi Cuốn Healthy 268', 'Đậu hũ nấm sốt tiêu', 72000, TRUE, 'Món nên thử khi ghé quán'),
('Gỏi Cuốn Healthy 268', 'Nước sâm', 18000, FALSE, 'Món dùng kèm phổ biến'),
('Hàu Nướng Phô Mai 282', 'Hàu nướng phô mai', 110000, TRUE, 'Món nên thử khi ghé quán'),
('Hàu Nướng Phô Mai 282', 'Sò điệp nướng mỡ hành', 95000, FALSE, 'Món dùng kèm phổ biến'),
('Hàu Nướng Phô Mai 282', 'Mực nướng sa tế', 135000, FALSE, 'Món dùng kèm phổ biến'),
('Hàu Nướng Phô Mai 282', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Cua Đồng 300', 'Lẩu cua đồng', 210000, TRUE, 'Món nên thử khi ghé quán'),
('Quán Cua Đồng 300', 'Gỏi cuốn chay', 45000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Cua Đồng 300', 'Nước sâm', 18000, FALSE, 'Món dùng kèm phổ biến'),
('Quán Cua Đồng 300', 'Rau câu dừa', 30000, FALSE, 'Món dùng kèm phổ biến'),
('Xiên Que Vĩnh Khánh 318', 'Gà xiên nướng', 35000, TRUE, 'Món nên thử khi ghé quán'),
('Xiên Que Vĩnh Khánh 318', 'Bánh tráng nướng', 35000, FALSE, 'Món dùng kèm phổ biến'),
('Xiên Que Vĩnh Khánh 318', 'Khoai tây lắc phô mai', 45000, FALSE, 'Món dùng kèm phổ biến'),
('Xiên Que Vĩnh Khánh 318', 'Trà tắc', 20000, FALSE, 'Món dùng kèm phổ biến'),
('Cổng Phố Ẩm Thực Vĩnh Khánh', 'Trà tắc', 20000, TRUE, 'Món nên thử khi ghé quán'),
('Cổng Phố Ẩm Thực Vĩnh Khánh', 'Bánh tráng nướng', 35000, FALSE, 'Món dùng kèm phổ biến'),
('Cổng Phố Ẩm Thực Vĩnh Khánh', 'Nước sâm', 18000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Check-in Ánh Đèn Vĩnh Khánh', 'Bánh tráng nướng', 35000, TRUE, 'Món nên thử khi ghé quán'),
('Khu Check-in Ánh Đèn Vĩnh Khánh', 'Chân gà sả tắc', 70000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Check-in Ánh Đèn Vĩnh Khánh', 'Trà đào cam sả', 39000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Hải Sản Tập Trung', 'Ốc len xào dừa', 85000, TRUE, 'Món nên thử khi ghé quán'),
('Khu Hải Sản Tập Trung', 'Sò điệp nướng mỡ hành', 95000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Hải Sản Tập Trung', 'Hàu nướng phô mai', 110000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Hải Sản Tập Trung', 'Mực nướng sa tế', 135000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Ăn Khuya Vĩnh Khánh', 'Cơm tấm sườn bì chả', 65000, TRUE, 'Món nên thử khi ghé quán'),
('Khu Ăn Khuya Vĩnh Khánh', 'Lẩu bò sa tế', 240000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Ăn Khuya Vĩnh Khánh', 'Mì xào hải sản', 80000, FALSE, 'Món dùng kèm phổ biến'),
('Khu Ăn Khuya Vĩnh Khánh', 'Chè khúc bạch', 42000, TRUE, 'Món nên thử khi ghé quán');


INSERT INTO place_dishes (place_id, dish_id, price, is_recommended, note)
SELECT p.place_id, d.dish_id, dpd.price, dpd.is_recommended, dpd.note
FROM demo_vinhkhanh_place_dishes dpd
JOIN places p ON p.place_name = dpd.place_name
JOIN dishes d ON d.dish_name = dpd.dish_name
ON CONFLICT (place_id, dish_id) DO UPDATE
SET price = EXCLUDED.price,
    is_recommended = EXCLUDED.is_recommended,
    note = EXCLUDED.note,
    updated_at = CURRENT_TIMESTAMP;


-- =========================================================
-- 5. Place narration contents, translations and fallback TTS audio rows
-- =========================================================

INSERT INTO narration_contents (title, original_text, content_type_id, place_id, dish_id, created_by, is_active)
SELECT
    'Khám phá ' || dp.place_name,
    dp.description || ' Địa chỉ: ' ||
    CASE
        WHEN dp.house_no = 1 THEN 'Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
        ELSE dp.house_no || ' Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
    END || '. Đây là một điểm dừng trong tuyến trải nghiệm phố ẩm thực Vĩnh Khánh.',
    (SELECT content_type_id FROM content_types WHERE code = 'Place'),
    p.place_id,
    NULL,
    (SELECT admin_id FROM admin_users WHERE email = 'admin@vinhkhanh.local'),
    TRUE
FROM demo_vinhkhanh_places dp
JOIN places p ON p.place_name = dp.place_name
WHERE NOT EXISTS (
    SELECT 1 FROM narration_contents nc
    WHERE nc.place_id = p.place_id
      AND nc.title = 'Khám phá ' || dp.place_name
);

UPDATE narration_contents nc
SET original_text = dp.description || ' Địa chỉ: ' ||
    CASE
        WHEN dp.house_no = 1 THEN 'Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
        ELSE dp.house_no || ' Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
    END || '. Đây là một điểm dừng trong tuyến trải nghiệm phố ẩm thực Vĩnh Khánh.',
    is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP
FROM demo_vinhkhanh_places dp
JOIN places p ON p.place_name = dp.place_name
WHERE nc.place_id = p.place_id
  AND nc.title = 'Khám phá ' || dp.place_name;

INSERT INTO narration_translations (
    narration_id, language_id, translated_title, translated_text,
    translation_source_id, reviewed_by, is_reviewed
)
SELECT
    nc.narration_id,
    l.language_id,
    CASE
        WHEN l.language_code = 'vi' THEN nc.title
        ELSE 'Discover ' || dp.place_name
    END,
    CASE
        WHEN l.language_code = 'vi' THEN
            dp.place_name || ' nằm tại ' ||
            CASE
                WHEN dp.house_no = 1 THEN 'Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
                ELSE dp.house_no || ' Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
            END || '. ' || dp.description || ' Gợi ý: hãy mở bản đồ, kiểm tra khoảng cách và chọn món nổi bật trước khi ghé quán.'
        ELSE
            dp.place_name || ' is located at ' ||
            CASE
                WHEN dp.house_no = 1 THEN 'Vinh Khanh Street, Khanh Hoi, Ho Chi Minh City, Vietnam'
                ELSE dp.house_no || ' Vinh Khanh Street, Khanh Hoi, Ho Chi Minh City, Vietnam'
            END || '. ' ||
            CASE dp.place_type_code
                WHEN 'Restaurant' THEN 'This restaurant is a suggested stop on the Vinh Khanh food street route.'
                WHEN 'FoodStall' THEN 'This food stall is suitable for a quick street-food stop.'
                WHEN 'CheckInPoint' THEN 'This check-in point helps visitors orient themselves before continuing the route.'
                ELSE 'This area is part of the Vinh Khanh food street experience.'
            END || ' You can use the map, QR code, or narration feature to explore it.'
    END,
    (SELECT translation_source_id FROM translation_sources WHERE code = 'Manual'),
    (SELECT admin_id FROM admin_users WHERE email = 'admin@vinhkhanh.local'),
    TRUE
FROM demo_vinhkhanh_places dp
JOIN places p ON p.place_name = dp.place_name
JOIN narration_contents nc ON nc.place_id = p.place_id AND nc.title = 'Khám phá ' || dp.place_name
CROSS JOIN languages l
WHERE l.language_code IN ('vi', 'en')
ON CONFLICT (narration_id, language_id) DO UPDATE
SET translated_title = EXCLUDED.translated_title,
    translated_text = EXCLUDED.translated_text,
    translation_source_id = EXCLUDED.translation_source_id,
    reviewed_by = EXCLUDED.reviewed_by,
    is_reviewed = TRUE,
    updated_at = CURRENT_TIMESTAMP;

INSERT INTO audio_files (
    translation_id, audio_url, voice_name, voice_gender,
    duration_seconds, file_format, generated_by, is_active
)
SELECT
    nt.translation_id,
    '',
    CASE WHEN l.language_code = 'vi' THEN 'vi-VN Browser TTS' ELSE 'en-US Browser TTS' END,
    'Neutral',
    45,
    'mp3',
    'TTS',
    TRUE
FROM narration_translations nt
JOIN languages l ON l.language_id = nt.language_id
JOIN narration_contents nc ON nc.narration_id = nt.narration_id
JOIN places p ON p.place_id = nc.place_id
JOIN demo_vinhkhanh_places dp ON dp.place_name = p.place_name
WHERE NOT EXISTS (
    SELECT 1 FROM audio_files af WHERE af.translation_id = nt.translation_id
);

INSERT INTO qr_codes (qr_code_value, qr_code_image_url, target_type_id, place_id, dish_id, narration_id, is_active)
SELECT
    'QR_PLACE_' || dp.slug,
    '',
    (SELECT target_type_id FROM target_types WHERE code = 'Place'),
    p.place_id,
    NULL,
    NULL,
    TRUE
FROM demo_vinhkhanh_places dp
JOIN places p ON p.place_name = dp.place_name
ON CONFLICT (qr_code_value) DO UPDATE
SET place_id = EXCLUDED.place_id,
    dish_id = NULL,
    narration_id = NULL,
    target_type_id = EXCLUDED.target_type_id,
    is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP;

INSERT INTO qr_codes (qr_code_value, qr_code_image_url, target_type_id, place_id, dish_id, narration_id, is_active)
SELECT
    'QR_NARRATION_' || dp.slug,
    '',
    (SELECT target_type_id FROM target_types WHERE code = 'Narration'),
    NULL,
    NULL,
    nc.narration_id,
    TRUE
FROM demo_vinhkhanh_places dp
JOIN places p ON p.place_name = dp.place_name
JOIN narration_contents nc ON nc.place_id = p.place_id AND nc.title = 'Khám phá ' || dp.place_name
ON CONFLICT (qr_code_value) DO UPDATE
SET place_id = NULL,
    dish_id = NULL,
    narration_id = EXCLUDED.narration_id,
    target_type_id = EXCLUDED.target_type_id,
    is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP;


-- =========================================================
-- 6. Dish narrations and dish QR codes for richer QR testing
-- =========================================================

DROP TABLE IF EXISTS demo_vinhkhanh_dish_narrations;
CREATE TEMP TABLE demo_vinhkhanh_dish_narrations (
    dish_name TEXT,
    vi_text TEXT,
    en_text TEXT,
    slug TEXT
) ON COMMIT DROP;

INSERT INTO demo_vinhkhanh_dish_narrations (dish_name, vi_text, en_text, slug)
VALUES

('Ốc len xào dừa', 'Ốc len xào dừa là món nổi bật trong nhóm hải sản tại phố ẩm thực Vĩnh Khánh. Ốc len béo thơm nước cốt dừa. vị ngọt mặn hài hòa. Giá tham khảo khoảng 85.000 VND.', 'Ốc len xào dừa is a highlighted dish in the Hải sản group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 85,000 VND.', 'OC_LEN_XAO_DUA'),
('Ốc hương rang muối', 'Ốc hương rang muối là món nổi bật trong nhóm hải sản tại phố ẩm thực Vĩnh Khánh. Ốc hương rang muối ớt. thơm và đậm vị. Giá tham khảo khoảng 120.000 VND.', 'Ốc hương rang muối is a highlighted dish in the Hải sản group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 120,000 VND.', 'OC_HU_NG_RANG_MUOI'),
('Sò điệp nướng mỡ hành', 'Sò điệp nướng mỡ hành là món nổi bật trong nhóm hải sản tại phố ẩm thực Vĩnh Khánh. Sò điệp nướng mỡ hành và đậu phộng. Giá tham khảo khoảng 95.000 VND.', 'Sò điệp nướng mỡ hành is a highlighted dish in the Hải sản group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 95,000 VND.', 'SO_DIEP_NUONG_MO_HANH'),
('Mực nướng sa tế', 'Mực nướng sa tế là món nổi bật trong nhóm hải sản tại phố ẩm thực Vĩnh Khánh. Mực nướng sa tế cay nhẹ. phù hợp dùng chung. Giá tham khảo khoảng 135.000 VND.', 'Mực nướng sa tế is a highlighted dish in the Hải sản group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 135,000 VND.', 'MUC_NUONG_SA_TE'),
('Càng ghẹ rang muối', 'Càng ghẹ rang muối là món nổi bật trong nhóm hải sản tại phố ẩm thực Vĩnh Khánh. Càng ghẹ rang muối đậm vị. hợp ăn nhóm. Giá tham khảo khoảng 160.000 VND.', 'Càng ghẹ rang muối is a highlighted dish in the Hải sản group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 160,000 VND.', 'CANG_GHE_RANG_MUOI'),
('Ba chỉ bò nướng', 'Ba chỉ bò nướng là món nổi bật trong nhóm món nướng tại phố ẩm thực Vĩnh Khánh. Ba chỉ bò ướp sốt. nướng tại bàn hoặc nướng sẵn. Giá tham khảo khoảng 129.000 VND.', 'Ba chỉ bò nướng is a highlighted dish in the Món nướng group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 129,000 VND.', 'BA_CHI_BO_NUONG'),
('Bạch tuộc nướng', 'Bạch tuộc nướng là món nổi bật trong nhóm món nướng tại phố ẩm thực Vĩnh Khánh. Bạch tuộc nướng giòn dai. sốt cay nhẹ. Giá tham khảo khoảng 140.000 VND.', 'Bạch tuộc nướng is a highlighted dish in the Món nướng group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 140,000 VND.', 'BACH_TUOC_NUONG'),
('Lẩu Thái hải sản', 'Lẩu Thái hải sản là món nổi bật trong nhóm lẩu tại phố ẩm thực Vĩnh Khánh. Lẩu Thái vị chua cay. dùng kèm hải sản và rau. Giá tham khảo khoảng 220.000 VND.', 'Lẩu Thái hải sản is a highlighted dish in the Lẩu group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 220,000 VND.', 'LAU_THAI_HAI_SAN'),
('Lẩu bò sa tế', 'Lẩu bò sa tế là món nổi bật trong nhóm lẩu tại phố ẩm thực Vĩnh Khánh. Lẩu bò sa tế cay thơm. nước dùng đậm. Giá tham khảo khoảng 240.000 VND.', 'Lẩu bò sa tế is a highlighted dish in the Lẩu group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 240,000 VND.', 'LAU_BO_SA_TE'),
('Cơm tấm sườn bì chả', 'Cơm tấm sườn bì chả là món nổi bật trong nhóm cơm - bún - mì tại phố ẩm thực Vĩnh Khánh. Cơm tấm sườn. bì. chả và nước mắm pha. Giá tham khảo khoảng 65.000 VND.', 'Cơm tấm sườn bì chả is a highlighted dish in the Cơm - bún - mì group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 65,000 VND.', 'C_M_TAM_SUON_BI_CHA'),
('Bún đậu mẹt nhỏ', 'Bún đậu mẹt nhỏ là món nổi bật trong nhóm cơm - bún - mì tại phố ẩm thực Vĩnh Khánh. Bún đậu. đậu chiên. chả cốm và rau ăn kèm. Giá tham khảo khoảng 89.000 VND.', 'Bún đậu mẹt nhỏ is a highlighted dish in the Cơm - bún - mì group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 89,000 VND.', 'BUN_DAU_MET_NHO'),
('Gỏi xoài khô mực', 'Gỏi xoài khô mực là món nổi bật trong nhóm ăn vặt tại phố ẩm thực Vĩnh Khánh. Gỏi xoài chua ngọt ăn cùng khô mực. Giá tham khảo khoảng 65.000 VND.', 'Gỏi xoài khô mực is a highlighted dish in the Ăn vặt group on Vinh Khanh food street. It is suitable for visitors who want to try a local street-food flavor. Estimated price: 65,000 VND.', 'GOI_XOAI_KHO_MUC');


INSERT INTO narration_contents (title, original_text, content_type_id, place_id, dish_id, created_by, is_active)
SELECT
    'Giới thiệu món ' || dnd.dish_name,
    dnd.vi_text,
    (SELECT content_type_id FROM content_types WHERE code = 'Dish'),
    NULL,
    d.dish_id,
    (SELECT admin_id FROM admin_users WHERE email = 'admin@vinhkhanh.local'),
    TRUE
FROM demo_vinhkhanh_dish_narrations dnd
JOIN dishes d ON d.dish_name = dnd.dish_name
WHERE NOT EXISTS (
    SELECT 1 FROM narration_contents nc
    WHERE nc.dish_id = d.dish_id
      AND nc.title = 'Giới thiệu món ' || dnd.dish_name
);

INSERT INTO narration_translations (
    narration_id, language_id, translated_title, translated_text,
    translation_source_id, reviewed_by, is_reviewed
)
SELECT
    nc.narration_id,
    l.language_id,
    CASE WHEN l.language_code = 'vi' THEN nc.title ELSE 'Dish introduction: ' || dnd.dish_name END,
    CASE WHEN l.language_code = 'vi' THEN dnd.vi_text ELSE dnd.en_text END,
    (SELECT translation_source_id FROM translation_sources WHERE code = 'Manual'),
    (SELECT admin_id FROM admin_users WHERE email = 'admin@vinhkhanh.local'),
    TRUE
FROM demo_vinhkhanh_dish_narrations dnd
JOIN dishes d ON d.dish_name = dnd.dish_name
JOIN narration_contents nc ON nc.dish_id = d.dish_id AND nc.title = 'Giới thiệu món ' || dnd.dish_name
CROSS JOIN languages l
WHERE l.language_code IN ('vi', 'en')
ON CONFLICT (narration_id, language_id) DO UPDATE
SET translated_title = EXCLUDED.translated_title,
    translated_text = EXCLUDED.translated_text,
    translation_source_id = EXCLUDED.translation_source_id,
    reviewed_by = EXCLUDED.reviewed_by,
    is_reviewed = TRUE,
    updated_at = CURRENT_TIMESTAMP;

INSERT INTO audio_files (translation_id, audio_url, voice_name, voice_gender, duration_seconds, file_format, generated_by, is_active)
SELECT
    nt.translation_id,
    '',
    CASE WHEN l.language_code = 'vi' THEN 'vi-VN Browser TTS' ELSE 'en-US Browser TTS' END,
    'Neutral',
    35,
    'mp3',
    'TTS',
    TRUE
FROM narration_translations nt
JOIN languages l ON l.language_id = nt.language_id
JOIN narration_contents nc ON nc.narration_id = nt.narration_id
JOIN dishes d ON d.dish_id = nc.dish_id
JOIN demo_vinhkhanh_dish_narrations dnd ON dnd.dish_name = d.dish_name
WHERE NOT EXISTS (
    SELECT 1 FROM audio_files af WHERE af.translation_id = nt.translation_id
);

INSERT INTO qr_codes (qr_code_value, qr_code_image_url, target_type_id, place_id, dish_id, narration_id, is_active)
SELECT
    'QR_DISH_' || dnd.slug,
    '',
    (SELECT target_type_id FROM target_types WHERE code = 'Dish'),
    NULL,
    d.dish_id,
    NULL,
    TRUE
FROM demo_vinhkhanh_dish_narrations dnd
JOIN dishes d ON d.dish_name = dnd.dish_name
ON CONFLICT (qr_code_value) DO UPDATE
SET place_id = NULL,
    dish_id = EXCLUDED.dish_id,
    narration_id = NULL,
    target_type_id = EXCLUDED.target_type_id,
    is_active = TRUE,
    updated_at = CURRENT_TIMESTAMP;


-- =========================================================
-- 7. Demo guest sessions, feedback, geofence events and listening histories
-- =========================================================

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-001-VI', (SELECT language_id FROM languages WHERE language_code = 'vi'), 'Chrome Android demo', '192.168.1.11', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-002-EN', (SELECT language_id FROM languages WHERE language_code = 'en'), 'Safari iPhone demo', '192.168.1.12', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-003-VI', (SELECT language_id FROM languages WHERE language_code = 'vi'), 'Chrome Desktop demo', '192.168.1.13', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-004-EN', (SELECT language_id FROM languages WHERE language_code = 'en'), 'Edge Desktop demo', '192.168.1.14', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-005-VI', (SELECT language_id FROM languages WHERE language_code = 'vi'), 'Mobile kiosk demo', '192.168.1.15', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-006-EN', (SELECT language_id FROM languages WHERE language_code = 'en'), 'Tablet demo', '192.168.1.16', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-007-VI', (SELECT language_id FROM languages WHERE language_code = 'vi'), 'QR test device', '192.168.1.17', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-008-EN', (SELECT language_id FROM languages WHERE language_code = 'en'), 'Geofence test device', '192.168.1.18', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-009-VI', (SELECT language_id FROM languages WHERE language_code = 'vi'), 'Manual playback test', '192.168.1.19', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;

INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
VALUES ('GUEST-DEMO-010-EN', (SELECT language_id FROM languages WHERE language_code = 'en'), 'Feedback test device', '192.168.1.20', TRUE)
ON CONFLICT (guest_session_id) DO UPDATE
SET preferred_language_id = EXCLUDED.preferred_language_id,
    device_info = EXCLUDED.device_info,
    ip_address = EXCLUDED.ip_address,
    last_seen_at = CURRENT_TIMESTAMP,
    is_active = TRUE;


-- Demo guest POI states

INSERT INTO guest_poi_states (
    guest_session_id, place_id, is_inside, last_entered_at, last_exited_at,
    last_triggered_at, cooldown_until, last_distance_meters
)
SELECT
    gs.guest_session_id,
    p.place_id,
    (row_number() OVER () % 2 = 0),
    CURRENT_TIMESTAMP - ((row_number() OVER ())::text || ' minutes')::interval,
    CURRENT_TIMESTAMP - (((row_number() OVER ()) + 8)::text || ' minutes')::interval,
    CURRENT_TIMESTAMP - (((row_number() OVER ()) + 3)::text || ' minutes')::interval,
    CURRENT_TIMESTAMP + (((row_number() OVER ()) + 2)::text || ' minutes')::interval,
    20 + (row_number() OVER ()) * 3
FROM guest_sessions gs
JOIN demo_vinhkhanh_places dp ON TRUE
JOIN places p ON p.place_name = dp.place_name
WHERE gs.guest_session_id LIKE 'GUEST-DEMO-%'
  AND dp.priority >= 8
ON CONFLICT (guest_session_id, place_id) DO UPDATE
SET is_inside = EXCLUDED.is_inside,
    last_entered_at = EXCLUDED.last_entered_at,
    last_exited_at = EXCLUDED.last_exited_at,
    last_triggered_at = EXCLUDED.last_triggered_at,
    cooldown_until = EXCLUDED.cooldown_until,
    last_distance_meters = EXCLUDED.last_distance_meters,
    updated_at = CURRENT_TIMESTAMP;


-- Demo feedbacks

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-001-VI', p.place_id, NULL, NULL, 4, 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-01]', FALSE
FROM places p
WHERE p.place_name = 'Quán Ốc Vĩnh Khánh'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-001-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-01]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-002-EN', p.place_id, NULL, NULL, 5, 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-02]', TRUE
FROM places p
WHERE p.place_name = 'Ốc Đêm Vĩnh Khánh'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-002-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-02]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-003-VI', p.place_id, NULL, NULL, 5, 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-03]', TRUE
FROM places p
WHERE p.place_name = 'Hải Sản Vĩnh Khánh 58'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-003-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-03]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-004-EN', p.place_id, NULL, NULL, 5, 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-04]', FALSE
FROM places p
WHERE p.place_name = 'Sò Điệp Góc Phố'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-004-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-04]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-005-VI', p.place_id, NULL, NULL, 4, 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-05]', TRUE
FROM places p
WHERE p.place_name = 'Quán Nướng Vĩnh Khánh 88'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-005-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-05]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-006-EN', p.place_id, NULL, NULL, 5, 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-06]', TRUE
FROM places p
WHERE p.place_name = 'Bạch Tuộc Nướng 102'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-006-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-06]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-007-VI', p.place_id, NULL, NULL, 5, 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-07]', FALSE
FROM places p
WHERE p.place_name = 'Lẩu Thái Vĩnh Khánh 118'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-007-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-07]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-008-EN', p.place_id, NULL, NULL, 5, 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-08]', TRUE
FROM places p
WHERE p.place_name = 'Cơm Tấm Đêm 136'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-008-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-08]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-009-VI', p.place_id, NULL, NULL, 4, 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-09]', TRUE
FROM places p
WHERE p.place_name = 'Bún Mắm Miền Tây 154'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-009-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-09]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-010-EN', p.place_id, NULL, NULL, 5, 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-10]', FALSE
FROM places p
WHERE p.place_name = 'Bún Đậu Vĩnh Khánh 168'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-010-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-10]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-001-VI', p.place_id, NULL, NULL, 5, 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-11]', TRUE
FROM places p
WHERE p.place_name = 'Ăn Vặt Vĩnh Khánh 182'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-001-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-11]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-002-EN', p.place_id, NULL, NULL, 5, 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-12]', TRUE
FROM places p
WHERE p.place_name = 'Chân Gà Sả Tắc 198'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-002-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-12]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-003-VI', p.place_id, NULL, NULL, 4, 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-13]', FALSE
FROM places p
WHERE p.place_name = 'Lẩu Bò Sa Tế 210'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-003-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-13]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-004-EN', p.place_id, NULL, NULL, 5, 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-14]', TRUE
FROM places p
WHERE p.place_name = 'Mì Xào Hải Sản 226'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-004-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-14]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-005-VI', p.place_id, NULL, NULL, 5, 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-15]', TRUE
FROM places p
WHERE p.place_name = 'Trà Tắc Vĩnh Khánh 240'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-005-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-15]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-006-EN', p.place_id, NULL, NULL, 5, 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-16]', FALSE
FROM places p
WHERE p.place_name = 'Chè Khúc Bạch 252'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-006-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Không khí đông vui, món ăn lên nhanh và phù hợp đi nhóm. [demo-16]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-007-VI', p.place_id, NULL, NULL, 4, 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-17]', TRUE
FROM places p
WHERE p.place_name = 'Gỏi Cuốn Healthy 268'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-007-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Địa điểm dễ tìm trên đường Vĩnh Khánh, món đặc trưng khá ổn. [demo-17]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-008-EN', p.place_id, NULL, NULL, 5, 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-18]', TRUE
FROM places p
WHERE p.place_name = 'Hàu Nướng Phô Mai 282'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-008-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Bản đồ và QR hữu ích, nghe thuyết minh trước khi vào quán rất tiện. [demo-18]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-009-VI', p.place_id, NULL, NULL, 5, 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-19]', FALSE
FROM places p
WHERE p.place_name = 'Quán Cua Đồng 300'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-009-VI'
        AND f.place_id = p.place_id
        AND f.comment = 'Giá hợp lý, nên bổ sung thêm hình ảnh món ăn trong ứng dụng. [demo-19]'
  );

INSERT INTO feedbacks (guest_session_id, place_id, dish_id, narration_id, rating, comment, is_approved)
SELECT 'GUEST-DEMO-010-EN', p.place_id, NULL, NULL, 5, 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-20]', TRUE
FROM places p
WHERE p.place_name = 'Xiên Que Vĩnh Khánh 318'
  AND NOT EXISTS (
      SELECT 1 FROM feedbacks f
      WHERE f.guest_session_id = 'GUEST-DEMO-010-EN'
        AND f.place_id = p.place_id
        AND f.comment = 'Phù hợp khách du lịch muốn biết nhanh món nên thử. [demo-20]'
  );


-- Demo geofence events

INSERT INTO geofence_events (
    guest_session_id, place_id, narration_id, user_latitude, user_longitude,
    distance_meters, detected_at, processed_at, note, event_type_id, event_status_id
)
SELECT
    gs.guest_session_id,
    p.place_id,
    nc.narration_id,
    p.latitude + 0.00008,
    p.longitude + 0.00008,
    18 + (row_number() OVER ()) * 2,
    CURRENT_TIMESTAMP - ((row_number() OVER ())::text || ' minutes')::interval,
    CURRENT_TIMESTAMP - (((row_number() OVER ()) - 1)::text || ' minutes')::interval,
    'FULL_DEMO_GEOFENCE_' || dp.slug || '_' || gs.guest_session_id,
    getype.event_type_id,
    ges.event_status_id
FROM guest_sessions gs
JOIN demo_vinhkhanh_places dp ON dp.priority >= 7
JOIN places p ON p.place_name = dp.place_name
JOIN narration_contents nc ON nc.place_id = p.place_id AND nc.title = 'Khám phá ' || dp.place_name
JOIN geofence_event_types getype ON getype.code = CASE WHEN dp.trigger_mode_code = 'Near' THEN 'Near' ELSE 'Enter' END
JOIN geofence_event_statuses ges ON ges.code = CASE WHEN dp.priority >= 8 THEN 'Played' ELSE 'Detected' END
WHERE gs.guest_session_id IN ('GUEST-DEMO-001-VI', 'GUEST-DEMO-002-EN', 'GUEST-DEMO-008-EN')
  AND NOT EXISTS (
      SELECT 1 FROM geofence_events ge
      WHERE ge.note = 'FULL_DEMO_GEOFENCE_' || dp.slug || '_' || gs.guest_session_id
  );


-- Demo listening histories

INSERT INTO listening_histories (
    guest_session_id, narration_id, language_id, audio_id, qr_code_id,
    listened_at, device_info, ip_address, listen_duration_seconds,
    geofence_event_id, trigger_source, playback_status
)
SELECT
    gs.guest_session_id,
    nc.narration_id,
    l.language_id,
    af.audio_id,
    q.qr_code_id,
    CURRENT_TIMESTAMP - ((row_number() OVER ())::text || ' minutes')::interval,
    gs.device_info,
    gs.ip_address,
    20 + (row_number() OVER ()) * 2,
    NULL,
    CASE WHEN row_number() OVER () % 3 = 0 THEN 'Manual' ELSE 'QR' END,
    CASE WHEN row_number() OVER () % 5 = 0 THEN 'Completed' ELSE 'Played' END
FROM guest_sessions gs
JOIN languages l ON l.language_id = gs.preferred_language_id
JOIN demo_vinhkhanh_places dp ON dp.priority >= 6
JOIN places p ON p.place_name = dp.place_name
JOIN narration_contents nc ON nc.place_id = p.place_id AND nc.title = 'Khám phá ' || dp.place_name
JOIN narration_translations nt ON nt.narration_id = nc.narration_id AND nt.language_id = l.language_id
JOIN audio_files af ON af.translation_id = nt.translation_id
LEFT JOIN qr_codes q ON q.qr_code_value = 'QR_PLACE_' || dp.slug
WHERE gs.guest_session_id LIKE 'GUEST-DEMO-%'
  AND NOT EXISTS (
      SELECT 1 FROM listening_histories lh
      WHERE lh.guest_session_id = gs.guest_session_id
        AND lh.narration_id = nc.narration_id
        AND lh.language_id = l.language_id
        AND lh.device_info = gs.device_info
  );


-- =========================================================
-- 8. Quick verification counts
-- =========================================================

SELECT 'places' AS table_name, COUNT(*) AS total FROM places
UNION ALL SELECT 'dishes', COUNT(*) FROM dishes
UNION ALL SELECT 'place_dishes', COUNT(*) FROM place_dishes
UNION ALL SELECT 'narration_contents', COUNT(*) FROM narration_contents
UNION ALL SELECT 'narration_translations', COUNT(*) FROM narration_translations
UNION ALL SELECT 'qr_codes', COUNT(*) FROM qr_codes
UNION ALL SELECT 'guest_sessions', COUNT(*) FROM guest_sessions
UNION ALL SELECT 'feedbacks', COUNT(*) FROM feedbacks
UNION ALL SELECT 'geofence_events', COUNT(*) FROM geofence_events
UNION ALL SELECT 'listening_histories', COUNT(*) FROM listening_histories
ORDER BY table_name;

COMMIT;
