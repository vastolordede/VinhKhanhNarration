import SimpleResourcePage, { getCurrentAdminId } from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function NarrationManagementScreen() {
  return <SimpleResourcePage config={{ title: 'Narration Management', description: 'Quản lý nội dung thuyết minh gốc cho Place / Dish / General.', endpoint: endpoints.narrationContents,createEndpoint: endpoints.narrationContentsAutoTranslate,
preparePayload: (payload, editing) => {
  if (!editing && !payload.createdBy) {
    const adminId = getCurrentAdminId();
    if (!adminId) throw new Error('Missing current admin id');
    payload.createdBy = adminId;
  }

  return payload;
},
fields: [
    { name: 'title', label: 'Title' },
    { name: 'originalText', label: 'Original Text', type: 'textarea' },
    {
  name: 'contentTypeId',
  label: 'Content Type',
  type: 'select',
  optionEndpoint: endpoints.contentTypes,
  optionValueKey: 'id',
  optionLabelKey: 'name'
},
    {
  name: 'placeId',
  label: 'Place',
  type: 'select',
  optionEndpoint: endpoints.places,
  optionValueKey: 'placeId',
  optionLabelKey: 'placeName',
  nullable: true,
  emptyLabel: 'Không chọn Place'
},
   {
  name: 'dishId',
  label: 'Dish',
  type: 'select',
  optionEndpoint: endpoints.dishes,
  optionValueKey: 'dishId',
  optionLabelKey: 'dishName',
  nullable: true,
  emptyLabel: 'Không chọn Dish'
},
    { name: 'isActive', label: 'Active', type: 'checkbox' }
  ], columns: [
    { key: 'narrationId', label: 'Id' }, { key: 'title', label: 'Title' }, { key: 'contentTypeId', label: 'Type' }, { key: 'placeId', label: 'Place' }, { key: 'dishId', label: 'Dish' }, { key: 'isActive', label: 'Active', render: r => r.isActive ? 'Yes' : 'No' }
  ] }} />;
}
