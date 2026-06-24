import { useEffect, useState } from 'react';
import { getPagedList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { DataTable } from '../../components/ui/DataTable';
import { PaginationBar } from '../../components/ui/PaginationBar';
import { PageHeader } from '../../components/layout/PageHeader';
import { GeofenceEventDTO, PagedResult } from '../../types';

const PAGE_SIZE = 20;

function formatDate(value?: string | null) {
  if (!value) return '-';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString('vi-VN');
}

export default function GeofenceEventsScreen() {
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<PagedResult<GeofenceEventDTO>>({
    items: [],
    page: 1,
    pageSize: PAGE_SIZE,
    totalItems: 0,
    totalPages: 0
  });

  useEffect(() => {
    let mounted = true;

    getPagedList<GeofenceEventDTO>(
      endpoints.adminGeofenceEvents,
      page,
      PAGE_SIZE
    ).then((data) => {
      if (!mounted) return;
      setResult(data);
      if (data.page !== page) setPage(data.page);
    });

    return () => {
      mounted = false;
    };
  }, [page]);

  return (
    <div>
      <PageHeader
        title="Geofence Log"
        description="Nhật ký chỉ đọc phục vụ theo dõi kỹ thuật. Hệ thống tự dọn log quá thời hạn lưu trữ."
      />

      <DataTable
        headers={[
          'Id',
          'Guest Session',
          'Access Pass',
          'Địa điểm',
          'Thuyết minh',
          'Loại',
          'Trạng thái',
          'Khoảng cách',
          'Thời điểm phát hiện'
        ]}
        rows={result.items.map((item) => [
          item.eventId,
          item.guestSessionId,
          item.accessPassId ?? '-',
          item.placeId,
          item.narrationId ?? '-',
          item.eventTypeId,
          item.eventStatusId,
          item.distanceMeters == null ? '-' : `${item.distanceMeters} m`,
          formatDate(item.detectedAt)
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
