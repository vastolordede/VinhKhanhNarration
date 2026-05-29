import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';
import { resolveAddress } from '../../api/crud';
import { Button } from '../../components/ui/Button';

export default function PlacesManagementScreen() {
  return (
    <SimpleResourcePage
      config={{
        title: 'Places / POI Management',
        description: 'Quản lý địa điểm, tọa độ, bán kính, priority, debounce và cooldown.',
        endpoint: endpoints.places,

        extraFormActionsAfterField: 'address',

        extraFormActions: (form, setForm) => {
          async function handleResolveAddress() {
            const address = String(form.address ?? '').trim();

            if (!address) {
              alert('Vui lòng nhập địa chỉ trước.');
              return;
            }

            try {
            const result = await resolveAddress(address);

if (!result.isExactHouseNumber) {
  const shouldUseApproximate = window.confirm(
    `Nominatim chỉ tìm được tọa độ gần đúng cho địa chỉ này.

Địa chỉ trả về:
${result.displayName}

Tọa độ:
${result.latitude}, ${result.longitude}

Lý do:
${result.warning ?? 'Kết quả không khớp chính xác số nhà.'}

Bạn có muốn dùng tọa độ gần đúng này không?`
  );

  if (!shouldUseApproximate) {
    return;
  }
}

setForm((prev) => ({
  ...prev,
  latitude: result.latitude,
  longitude: result.longitude
}));

alert(
  `Đã lấy tọa độ:
${result.latitude}, ${result.longitude}

Địa chỉ trả về:
${result.displayName}

Độ khớp:
${result.matchQuality}`
);
            } catch {
              alert('Không tìm thấy tọa độ cho địa chỉ này.');
            }
          }

          return (
            <Button
              type="button"
              variant="secondary"
              onClick={handleResolveAddress}
              className="w-full"
            >
              Lấy tọa độ từ địa chỉ
            </Button>
          );
        },

        fields: [
          { name: 'placeName', label: 'Place Name' },
          { name: 'placeTypeId', label: 'Place Type Id', type: 'number' },
          { name: 'address', label: 'Address' },
          { name: 'description', label: 'Description', type: 'textarea' },
         {
  name: 'latitude',
  label: 'Latitude',
  type: 'number',
  placeholder: 'Tự động lấy từ địa chỉ hoặc nhập tay'
},
{
  name: 'longitude',
  label: 'Longitude',
  type: 'number',
  placeholder: 'Tự động lấy từ địa chỉ hoặc nhập tay'
},
          { name: 'openingHours', label: 'Opening Hours' },
          { name: 'imageUrl', label: 'Image URL' },
          { name: 'isPoi', label: 'Is POI', type: 'checkbox' },
          { name: 'isGeofenceEnabled', label: 'Geofence Enabled', type: 'checkbox' },
          { name: 'triggerRadiusMeters', label: 'Radius Meters', type: 'number' },
          { name: 'priority', label: 'Priority', type: 'number' },
          { name: 'triggerModeId', label: 'Trigger Mode Id', type: 'number' },
          { name: 'debounceSeconds', label: 'Debounce Seconds', type: 'number' },
          { name: 'cooldownSeconds', label: 'Cooldown Seconds', type: 'number' },
          { name: 'isActive', label: 'Active', type: 'checkbox' }
        ],

        columns: [
          { key: 'placeId', label: 'Id' },
          { key: 'placeName', label: 'Name' },
          { key: 'address', label: 'Address' },
          { key: 'latitude', label: 'Lat' },
          { key: 'longitude', label: 'Lng' },
          {
            key: 'isGeofenceEnabled',
            label: 'Geofence',
            render: (r) => (r.isGeofenceEnabled ? 'On' : 'Off')
          }
        ]
      }}
    />
  );
}