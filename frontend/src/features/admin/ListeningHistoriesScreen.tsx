import { useEffect, useState } from 'react';
import { getPagedList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { DataTable } from '../../components/ui/DataTable';
import { PaginationBar } from '../../components/ui/PaginationBar';
import { PageHeader } from '../../components/layout/PageHeader';
import { ListeningHistoryDTO, PagedResult } from '../../types';

const PAGE_SIZE = 20;

function formatDate(value?: string | null) {
  if (!value) return '-';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString('vi-VN');
}

export default function ListeningHistoriesScreen() {
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<PagedResult<ListeningHistoryDTO>>({
    items: [],
    page: 1,
    pageSize: PAGE_SIZE,
    totalItems: 0,
    totalPages: 0
  });

  useEffect(() => {
    let mounted = true;

    getPagedList<ListeningHistoryDTO>(
      endpoints.adminListeningHistories,
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
        title="Listening Histories"
        description="Nhật ký lượt phát audio thực tế. Guest hết hạn bị chặn nghe nhưng log cũ vẫn được giữ để thống kê."
      />

      <DataTable
        headers={[
          'Id',
          'Guest Session',
          'Access Pass',
          'Thuyết minh',
          'Ngôn ngữ',
          'Nguồn',
          'Trạng thái',
          'Thời lượng',
          'Thời gian'
        ]}
        rows={result.items.map((item) => [
          item.historyId ?? '-',
          item.guestSessionId || '-',
          item.accessPassId ?? '-',
          item.narrationId,
          item.languageId,
          item.triggerSource,
          item.playbackStatus,
          item.listenDurationSeconds == null
            ? '-'
            : `${item.listenDurationSeconds}s`,
          formatDate(item.listenedAt)
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
