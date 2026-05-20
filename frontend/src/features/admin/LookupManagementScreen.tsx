import { useState } from 'react';
import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';

const configs = [
  ['Place Types', endpoints.placeTypes],
  ['Content Types', endpoints.contentTypes],
  ['Target Types', endpoints.targetTypes],
  ['Translation Sources', endpoints.translationSources],
  ['Trigger Modes', endpoints.triggerModes],
  ['Geofence Event Types', endpoints.geofenceEventTypes],
  ['Geofence Event Statuses', endpoints.geofenceEventStatuses]
] as const;

export default function LookupManagementScreen() {
  const [active, setActive] = useState(0);
  const [title, endpoint] = configs[active];
  return (
    <div>
      <div className="mb-4 flex flex-wrap gap-2">
        {configs.map(([label], index) => <Button key={label} variant={index === active ? 'primary' : 'secondary'} onClick={() => setActive(index)}>{label}</Button>)}
      </div>
      <SimpleResourcePage config={{
        title,
        description: 'CRUD bảng danh mục lookup để không hard-code constant trong database.',
        endpoint,
        idKey: Object.keys({})[0] || 'id',
        fields: [
          { name: 'code', label: 'Code' },
          { name: 'name', label: 'Name' },
          { name: 'description', label: 'Description', type: 'textarea' },
          { name: 'isActive', label: 'Active', type: 'checkbox' }
        ],
        columns: [
          { key: 'code', label: 'Code' },
          { key: 'name', label: 'Name' },
          { key: 'description', label: 'Description' },
          { key: 'isActive', label: 'Active', render: (r) => r.isActive ? 'Yes' : 'No' }
        ]
      }} />
    </div>
  );
}
