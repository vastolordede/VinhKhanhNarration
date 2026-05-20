import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function NarrationManagementScreen() {
  return <SimpleResourcePage config={{ title: 'Narration Management', description: 'Quản lý nội dung thuyết minh gốc cho Place / Dish / General.', endpoint: endpoints.narrationContents, fields: [
    { name: 'title', label: 'Title' },
    { name: 'originalText', label: 'Original Text', type: 'textarea' },
    { name: 'contentTypeId', label: 'Content Type Id', type: 'number' },
    { name: 'placeId', label: 'Place Id', type: 'number' },
    { name: 'dishId', label: 'Dish Id', type: 'number' },
    { name: 'createdBy', label: 'Created By Admin Id', type: 'number' },
    { name: 'isActive', label: 'Active', type: 'checkbox' }
  ], columns: [
    { key: 'narrationId', label: 'Id' }, { key: 'title', label: 'Title' }, { key: 'contentTypeId', label: 'Type' }, { key: 'placeId', label: 'Place' }, { key: 'dishId', label: 'Dish' }, { key: 'isActive', label: 'Active', render: r => r.isActive ? 'Yes' : 'No' }
  ] }} />;
}
