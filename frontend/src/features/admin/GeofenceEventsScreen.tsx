import { useEffect, useState } from 'react';
import { getPagedList, patchItem } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';
import { DataTable } from '../../components/ui/DataTable';
import { PaginationBar } from '../../components/ui/PaginationBar';
import { PageHeader } from '../../components/layout/PageHeader';
import { GeofenceEventDTO, PagedResult } from '../../types';

const PAGE_SIZE = 20;

export default function GeofenceEventsScreen() {
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<PagedResult<GeofenceEventDTO>>({
    items: [],
    page: 1,
    pageSize: PAGE_SIZE,
    totalItems: 0,
    totalPages: 0
  });

  async function load(targetPage = page) {
    const data = await getPagedList<GeofenceEventDTO>(
      endpoints.adminGeofenceEvents,
      targetPage,
      PAGE_SIZE
    );

    setResult(data);
    setPage(data.page);
  }

  useEffect(() => {
    load(page);
  }, [page]);

  async function markProcessed(eventId: number) {
    await patchItem(`${endpoints.adminGeofenceEvents}/${eventId}/status`, {});
    await load(page);
  }

  return (
    <div>
      <PageHeader
        title="Geofence Events"
        description="Log thời gian thực khi khách vào/gần/rời POI."
      />

      <DataTable
        headers={[
          'Id',
          'Guest',
          'Place',
          'Narration',
          'Type',
          'Status',
          'Distance',
          'Detected',
          'Actions'
        ]}
        rows={result.items.map((item) => [
          item.eventId,
          item.guestSessionId,
          item.placeId,
          item.narrationId ?? '',
          item.eventTypeId,
          item.eventStatusId,
          item.distanceMeters ?? '',
          item.detectedAt || '',
          <Button
            key="actions"
            className="px-3 py-2"
            variant="secondary"
            onClick={() => markProcessed(item.eventId)}
          >
            Update status
          </Button>
        ])}
      />

      <PaginationBar
        page={result.page}
        pageSize={result.pageSize}
        totalItems={result.totalItems}
        totalPages={result.totalPages}
        onPageChange={setPage}
      />
    </div>
  );
}