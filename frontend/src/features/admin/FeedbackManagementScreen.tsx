import { useEffect, useState } from 'react';
import { getPagedList, patchItem } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';
import { DataTable } from '../../components/ui/DataTable';
import { PaginationBar } from '../../components/ui/PaginationBar';
import { PageHeader } from '../../components/layout/PageHeader';
import { FeedbackDTO, PagedResult } from '../../types';
import { StatusBadge } from '../../components/ui/StatusBadge';
import { useI18n } from '../../i18n/useI18n';

const PAGE_SIZE = 20;

export default function FeedbackManagementScreen() {
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<PagedResult<FeedbackDTO>>({
    items: [],
    page: 1,
    pageSize: PAGE_SIZE,
    totalItems: 0,
    totalPages: 0
  });

  const { tx } = useI18n();

  async function load(targetPage = page) {
    const data = await getPagedList<FeedbackDTO>(
      endpoints.adminFeedbacks,
      targetPage,
      PAGE_SIZE
    );

    setResult(data);
    setPage(data.page);
  }

  useEffect(() => {
    load(page);
  }, [page]);

  async function approve(id?: number) {
    if (!id) return;
    await patchItem(`${endpoints.adminFeedbacks}/${id}/approve`);
    await load(page);
  }

  async function reject(id?: number) {
    if (!id) return;
    await patchItem(`${endpoints.adminFeedbacks}/${id}/reject`);
    await load(page);
  }

  return (
    <div>
      <PageHeader
        title="Feedback Management"
        description="Duyệt hoặc từ chối feedback của khách anonymous."
      />

      <DataTable
        headers={[
          'Id',
          'Guest',
          'Target',
          'Rating',
          'Comment',
          'Approved',
          'Actions'
        ]}
        rows={result.items.map((feedback) => [
          feedback.feedbackId,
          feedback.guestSessionId || '',
          `P:${feedback.placeId ?? '-'} D:${feedback.dishId ?? '-'} N:${feedback.narrationId ?? '-'}`,
          feedback.rating,
          feedback.comment || '',
          <StatusBadge key="approved" active={feedback.isApproved === true} />,
          <div className="flex gap-2" key="actions">
            <Button
              className="px-3 py-2"
              onClick={() => approve(feedback.feedbackId)}
            >
              {tx('Approve')}
            </Button>

            <Button
              className="px-3 py-2"
              variant="danger"
              onClick={() => reject(feedback.feedbackId)}
            >
              {tx('Reject')}
            </Button>
          </div>
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