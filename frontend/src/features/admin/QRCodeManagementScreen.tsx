import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function QRCodeManagementScreen() {
  return (
    <SimpleResourcePage
      config={{
        title: 'QR Code Management',
        description: 'QR có thể trỏ tới Place, Dish hoặc Narration.',
        endpoint: endpoints.qrCodes,

        validate: (form) => {
          const errors: Record<string, string> = {};
          const targetTypeId = Number(form.targetTypeId);

          if (targetTypeId === 1) {
            if (!form.placeId) errors.placeId = 'Place is required for Place QR.';
            if (form.dishId) errors.dishId = 'Dish must be empty for Place QR.';
            if (form.narrationId) errors.narrationId = 'Narration must be empty for Place QR.';
          }

          if (targetTypeId === 2) {
            if (!form.dishId) errors.dishId = 'Dish is required for Dish QR.';
            if (form.placeId) errors.placeId = 'Place must be empty for Dish QR.';
            if (form.narrationId) errors.narrationId = 'Narration must be empty for Dish QR.';
          }

          if (targetTypeId === 3) {
            if (!form.narrationId) errors.narrationId = 'Narration is required for Narration QR.';
            if (form.placeId) errors.placeId = 'Place must be empty for Narration QR.';
            if (form.dishId) errors.dishId = 'Dish must be empty for Narration QR.';
          }

          return errors;
        },

        fields: [
          { name: 'qrCodeValue', label: 'QR Code Value', required: true },
          { name: 'qrCodeImageUrl', label: 'QR Image URL' },
          {
            name: 'targetTypeId',
            label: 'Target Type',
            type: 'select',
            optionEndpoint: endpoints.targetTypes,
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
          {
            name: 'narrationId',
            label: 'Narration',
            type: 'select',
            optionEndpoint: endpoints.narrationContents,
            optionValueKey: 'narrationId',
            optionLabelKey: 'title',
            nullable: true,
            emptyLabel: 'Không chọn Narration'
          },
          { name: 'isActive', label: 'Active', type: 'checkbox' }
        ],

        columns: [
          { key: 'qrCodeId', label: 'Id' },
          { key: 'qrCodeValue', label: 'Value' },
          { key: 'targetTypeId', label: 'Target Type' },
          { key: 'placeId', label: 'Place' },
          { key: 'dishId', label: 'Dish' },
          { key: 'narrationId', label: 'Narration' },
          { key: 'isActive', label: 'Active', render: (r) => (r.isActive ? 'Yes' : 'No') }
        ]
      }}
    />
  );
}