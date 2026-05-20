import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function QRCodeManagementScreen() {
  return <SimpleResourcePage config={{ title: 'QR Code Management', description: 'QR có thể trỏ tới Place, Dish hoặc Narration.', endpoint: endpoints.qrCodes, fields: [
    { name: 'qrCodeValue', label: 'QR Code Value' },
    { name: 'qrCodeImageUrl', label: 'QR Image URL' },
    { name: 'targetTypeId', label: 'Target Type Id', type: 'number' },
    { name: 'placeId', label: 'Place Id', type: 'number' },
    { name: 'dishId', label: 'Dish Id', type: 'number' },
    { name: 'narrationId', label: 'Narration Id', type: 'number' },
    { name: 'isActive', label: 'Active', type: 'checkbox' }
  ], columns: [
    { key: 'qrCodeId', label: 'Id' }, { key: 'qrCodeValue', label: 'Value' }, { key: 'targetTypeId', label: 'Target Type' }, { key: 'placeId', label: 'Place' }, { key: 'dishId', label: 'Dish' }, { key: 'narrationId', label: 'Narration' }, { key: 'isActive', label: 'Active', render: r => r.isActive ? 'Yes' : 'No' }
  ] }} />;
}
