import * as QRCode from 'qrcode';
import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';

function safeFileName(value: string) {
  return value
    .trim()
    .replace(/[^a-zA-Z0-9-_]+/g, '_')
    .replace(/^_+|_+$/g, '')
    .slice(0, 80);
}

async function downloadQrImage(row: any) {
  const qrValue = String(row.qrCodeValue ?? '').trim();

  if (!qrValue) {
    window.alert('QR Code Value is empty.');
    return;
  }

  try {
    const dataUrl = await QRCode.toDataURL(qrValue, {
      width: 512,
      margin: 2,
      errorCorrectionLevel: 'M'
    });

    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = `${safeFileName(qrValue) || 'qr-code'}.png`;
    document.body.appendChild(link);
    link.click();
    link.remove();
  } catch {
    window.alert('Unable to generate QR image.');
  }
}

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
        ],

        extraRowActions: (row) => (
          <Button
            type="button"
            variant="secondary"
            className="px-3 py-2"
            onClick={() => downloadQrImage(row)}
          >
            Download QR
          </Button>
        )
      }}
    />
  );
}