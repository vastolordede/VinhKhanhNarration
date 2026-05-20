import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function PlacesManagementScreen() {
  return <SimpleResourcePage config={{ title: 'Places / POI Management', description: 'Quản lý địa điểm, tọa độ, bán kính, priority, debounce và cooldown.', endpoint: endpoints.places, fields: [
    { name: 'placeName', label: 'Place Name' },
    { name: 'placeTypeId', label: 'Place Type Id', type: 'number' },
    { name: 'address', label: 'Address' },
    { name: 'description', label: 'Description', type: 'textarea' },
    { name: 'latitude', label: 'Latitude', type: 'number' },
    { name: 'longitude', label: 'Longitude', type: 'number' },
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
  ], columns: [
    { key: 'placeId', label: 'Id' }, { key: 'placeName', label: 'Name' }, { key: 'address', label: 'Address' },
    { key: 'latitude', label: 'Lat' }, { key: 'longitude', label: 'Lng' }, { key: 'isGeofenceEnabled', label: 'Geofence', render: r => r.isGeofenceEnabled ? 'On' : 'Off' }
  ] }} />;
}
