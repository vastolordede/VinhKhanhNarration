import SimpleResourcePage, { getCurrentAdminId } from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function NarrationManagementScreen() {
  return (
    <SimpleResourcePage
      config={{
        title: 'Narration Management',
        description: 'Quản lý nội dung thuyết minh gốc cho Place / Dish / General.',
        endpoint: endpoints.narrationContents,
        createEndpoint: endpoints.narrationContentsAutoTranslate,

        preparePayload: (payload, editing) => {
          if (!editing && !payload.createdBy) {
            const adminId = getCurrentAdminId();
            if (!adminId) throw new Error('Missing current admin id');
            payload.createdBy = adminId;
          }

          return payload;
        },

        validate: (form) => {
          const errors: Record<string, string> = {};
          const contentTypeId = Number(form.contentTypeId);

          if (contentTypeId === 1) {
            if (!form.placeId) errors.placeId = 'Place is required for Place Narration.';
            if (form.dishId) errors.dishId = 'Dish must be empty for Place Narration.';
          }

          if (contentTypeId === 2) {
            if (!form.dishId) errors.dishId = 'Dish is required for Dish Narration.';
            if (form.placeId) errors.placeId = 'Place must be empty for Dish Narration.';
          }

          if (contentTypeId === 3) {
            if (form.placeId) errors.placeId = 'Place must be empty for General Narration.';
            if (form.dishId) errors.dishId = 'Dish must be empty for General Narration.';
          }

          return errors;
        },

        fields: [
          { name: 'title', label: 'Title', required: true },
          { name: 'originalText', label: 'Original Text', type: 'textarea', required: true },
          {
            name: 'contentTypeId',
            label: 'Content Type',
            type: 'select',
            optionEndpoint: endpoints.contentTypes,
            optionValueKey: 'id',
            optionLabelKey: 'name',
            required: true
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
        ],

        columns: [
          { key: 'narrationId', label: 'Id' },
          { key: 'title', label: 'Title' },
          { key: 'contentTypeId', label: 'Type' },
          { key: 'placeId', label: 'Place' },
          { key: 'dishId', label: 'Dish' },
          { key: 'isActive', label: 'Active', render: (r) => (r.isActive ? 'Yes' : 'No') }
        ]
      }}
    />
  );
}