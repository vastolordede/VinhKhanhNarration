import { useEffect, useState } from 'react';
import { getPagedList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { DataTable } from '../../components/ui/DataTable';
import { PaginationBar } from '../../components/ui/PaginationBar';
import { PageHeader } from '../../components/layout/PageHeader';
import { ListeningHistoryDTO, PagedResult } from '../../types';

const PAGE_SIZE = 20;

export default function ListeningHistoriesScreen() {
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<PagedResult<ListeningHistoryDTO>>({
    items: [],
    page: 1,
    pageSize: PAGE_SIZE,
    totalItems: 0,
    totalPages: 0
  });

  async function load(targetPage = page) {
    const data = await getPagedList<ListeningHistoryDTO>(
      endpoints.adminListeningHistories,
      targetPage,
      PAGE_SIZE
    );

    setResult(data);
    setPage(data.page);
  }

  useEffect(() => {
    load(page);
  }, [page]);

  return (
    <div>
      <PageHeader
        title="Listening Histories"
        description="Theo dõi lượt nghe theo QR / Geofence / Manual."
      />

      <DataTable
        headers={[
          'Id',
          'Guest',
          'Narration',
          'Language',
          'Source',
          'Status',
          'Duration',
          'Time'
        ]}
        rows={result.items.map((item) => [
          item.historyId,
          item.guestSessionId || '',
          item.narrationId,
          item.languageId,
          item.triggerSource,
          item.playbackStatus,
          item.listenDurationSeconds ?? '',
          item.listenedAt || ''
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