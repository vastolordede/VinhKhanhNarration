const fs = require('fs');

const API_KEY = process.env.GOOGLE_MAPS_API_KEY;

if (!API_KEY) {
  console.error('Missing GOOGLE_MAPS_API_KEY environment variable.');
  process.exit(1);
}

// Chép dữ liệu từ query DBeaver vào đây.
// Chỉ cần place_id + address.
const places = [
  {
    place_id: 1,
    place_name: 'Quán Ốc Vĩnh Khánh',
    address: '40 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 2,
    place_name: 'Ốc Đêm Vĩnh Khánh',
    address: '22 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 3,
    place_name: 'Hải Sản Vĩnh Khánh 58',
    address: '58 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 4,
    place_name: 'Sò Điệp Góc Phố',
    address: '72 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 5,
    place_name: 'Quán Nướng Vĩnh Khánh 88',
    address: '88 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 6,
    place_name: 'Bạch Tuộc Nướng 102',
    address: '102 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 7,
    place_name: 'Lẩu Thái Vĩnh Khánh 118',
    address: '118 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 8,
    place_name: 'Cơm Tấm Đêm 136',
    address: '136 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 9,
    place_name: 'Bún Mắm Miền Tây 154',
    address: '154 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 10,
    place_name: 'Bún Đậu Vĩnh Khánh 168',
    address: '168 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 11,
    place_name: 'Ăn Vặt Vĩnh Khánh 182',
    address: '182 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 12,
    place_name: 'Chân Gà Sả Tắc 198',
    address: '198 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 13,
    place_name: 'Lẩu Bò Sa Tế 210',
    address: '210 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 14,
    place_name: 'Mì Xào Hải Sản 226',
    address: '226 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 15,
    place_name: 'Trà Tắc Vĩnh Khánh 240',
    address: '240 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 16,
    place_name: 'Chè Khúc Bạch 252',
    address: '252 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 17,
    place_name: 'Gỏi Cuốn Healthy 268',
    address: '268 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 18,
    place_name: 'Hàu Nướng Phô Mai 282',
    address: '282 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 19,
    place_name: 'Quán Cua Đồng 300',
    address: '300 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 20,
    place_name: 'Xiên Que Vĩnh Khánh 318',
    address: '318 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 21,
    place_name: 'Cổng Phố Ẩm Thực Vĩnh Khánh',
    address: 'Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 22,
    place_name: 'Khu Check-in Ánh Đèn Vĩnh Khánh',
    address: '188 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 23,
    place_name: 'Khu Hải Sản Tập Trung',
    address: '60 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  },
  {
    place_id: 24,
    place_name: 'Khu Ăn Khuya Vĩnh Khánh',
    address: '230 Đ. Vĩnh Khánh, Khánh Hội, Hồ Chí Minh, Việt Nam'
  }
];

function sqlEscape(value) {
  return String(value).replace(/'/g, "''");
}

async function geocodeAddress(place) {
  const params = new URLSearchParams({
    address: place.address,
    region: 'vn',
    language: 'vi',
    key: API_KEY
  });

  const url = `https://maps.googleapis.com/maps/api/geocode/json?${params.toString()}`;

  const response = await fetch(url);
  const data = await response.json();

  if (data.status !== 'OK' || !data.results?.length) {
    return {
      place_id: place.place_id,
      address: place.address,
      ok: false,
      status: data.status,
      error_message: data.error_message || null
    };
  }

  const best = data.results[0];
  const location = best.geometry.location;

  return {
    place_id: place.place_id,
    address: place.address,
    ok: true,
    formatted_address: best.formatted_address,
    place_id_google: best.place_id,
    lat: location.lat,
    lng: location.lng,
    location_type: best.geometry.location_type
  };
}

async function main() {
  const results = [];

  for (const place of places) {
    console.log(`Geocoding place_id=${place.place_id}: ${place.address}`);

    const result = await geocodeAddress(place);
    results.push(result);

    // tránh gọi quá nhanh
    await new Promise((resolve) => setTimeout(resolve, 200));
  }

  fs.writeFileSync(
    'tools/geocode_results.json',
    JSON.stringify(results, null, 2),
    'utf8'
  );

  const okResults = results.filter((r) => r.ok);

  const sql = [
    '-- Generated from Google Geocoding API',
    '-- Review before running in DBeaver',
    'BEGIN;',
    '',
    ...okResults.map(
      (r) => `UPDATE places
SET
    latitude = ${r.lat},
    longitude = ${r.lng},
    updated_at = NOW()
WHERE place_id = ${r.place_id}
  AND address = '${sqlEscape(r.address)}';`
    ),
    '',
    'COMMIT;',
    ''
  ].join('\n\n');

  fs.writeFileSync(
    'tools/seed_demo_vinhkhanh_google_coordinates.sql',
    sql,
    'utf8'
  );

  const failed = results.filter((r) => !r.ok);

  console.log('');
  console.log(`Done. Success: ${okResults.length}, Failed: ${failed.length}`);
  console.log('Generated: tools/geocode_results.json');
  console.log('Generated: tools/seed_demo_vinhkhanh_google_coordinates.sql');

  if (failed.length) {
    console.log('');
    console.log('Failed addresses:');
    console.table(failed);
  }
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});