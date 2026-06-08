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
          { name: 'placeName', label: 'Place Name', required: true },
        {
  name: 'placeTypeId',
  label: 'Place Type',
  type: 'select',
  optionEndpoint: endpoints.placeTypes,
  optionValueKey: 'id',
  optionLabelKey: 'name'
  , required: true
},
          { name: 'address', label: 'Address' },
          { name: 'description', label: 'Description', type: 'textarea' },
         {
  name: 'latitude',
  label: 'Latitude',
  type: 'number',
  placeholder: 'Auto-fill from address or enter manually'
},
{
  name: 'longitude',
  label: 'Longitude',
  type: 'number',
  placeholder: 'Auto-fill from address or enter manually'
},
          { name: 'openingHours', label: 'Opening Hours' },
          { name: 'imageUrl', label: 'Image URL' },
          { name: 'isPoi', label: 'Is POI', type: 'checkbox' },
          { name: 'isGeofenceEnabled', label: 'Geofence Enabled', type: 'checkbox' },
          { name: 'triggerRadiusMeters', label: 'Radius Meters', type: 'number', required: true },
          { name: 'priority', label: 'Priority', type: 'number', required: true },
          {
  name: 'triggerModeId',
  label: 'Trigger Mode',
  type: 'select',
  optionEndpoint: endpoints.triggerModes,
  optionValueKey: 'id',
  optionLabelKey: 'name'
  , required: true
},
          { name: 'debounceSeconds', label: 'Debounce Seconds', type: 'number', required: true },
          { name: 'cooldownSeconds', label: 'Cooldown Seconds', type: 'number', required: true },
          { name: 'isActive', label: 'Active', type: 'checkbox' }
        ],
validate: (form) => {
  const errors: Record<string, string> = {};

  if ((form.isPoi || form.isGeofenceEnabled) && (!form.latitude || !form.longitude)) {
    errors.latitude = 'Latitude is required for POI/geofence places.';
    errors.longitude = 'Longitude is required for POI/geofence places.';
  }

  if (Number(form.triggerRadiusMeters) <= 0) {
    errors.triggerRadiusMeters = 'Radius Meters must be greater than 0.';
  }

  if (Number(form.priority) < 0) {
    errors.priority = 'Priority must be greater than or equal to 0.';
  }

  if (Number(form.debounceSeconds) < 0) {
    errors.debounceSeconds = 'Debounce Seconds must be greater than or equal to 0.';
  }

  if (Number(form.cooldownSeconds) < 0) {
    errors.cooldownSeconds = 'Cooldown Seconds must be greater than or equal to 0.';
  }

  return errors;
},
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