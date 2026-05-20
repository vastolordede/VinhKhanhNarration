import { useEffect, useState } from 'react';
import { getList, patchItem } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';
import { DataTable } from '../../components/ui/DataTable';
import { PageHeader } from '../../components/layout/PageHeader';
import { GeofenceEventDTO } from '../../types';

export default function GeofenceEventsScreen() {
  const [items, setItems] = useState<GeofenceEventDTO[]>([]);
  async function load() { setItems(await getList<GeofenceEventDTO>(endpoints.adminGeofenceEvents)); }
  useEffect(() => { load(); }, []);
  async function markProcessed(eventId: number) { await patchItem(`${endpoints.adminGeofenceEvents}/${eventId}/status`, {}); await load(); }
  return <div>
    <PageHeader title="Geofence Events" description="Log thời gian thực khi khách vào/gần/rời POI." />
    <DataTable headers={['Id','Guest','Place','Narration','Type','Status','Distance','Detected','Actions']} rows={items.map(x => [
      x.eventId, x.guestSessionId, x.placeId, x.narrationId ?? '', x.eventTypeId, x.eventStatusId, x.distanceMeters ?? '', x.detectedAt || '',
      <Button key="a" className="px-3 py-2" variant="secondary" onClick={() => markProcessed(x.eventId)}>Update status</Button>
    ])} />
  </div>;
}
