import { useEffect, useState } from 'react';
import { getList, patchItem } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';
import { DataTable } from '../../components/ui/DataTable';
import { PageHeader } from '../../components/layout/PageHeader';
import { FeedbackDTO } from '../../types';

export default function FeedbackManagementScreen() {
  const [items, setItems] = useState<FeedbackDTO[]>([]);
  async function load() { setItems(await getList<FeedbackDTO>(endpoints.adminFeedbacks)); }
  useEffect(() => { load(); }, []);
  async function approve(id?: number) { if (!id) return; await patchItem(`${endpoints.adminFeedbacks}/${id}/approve`); await load(); }
  async function reject(id?: number) { if (!id) return; await patchItem(`${endpoints.adminFeedbacks}/${id}/reject`); await load(); }
  return <div>
    <PageHeader title="Feedback Management" description="Duyệt hoặc từ chối feedback của khách anonymous." />
    <DataTable headers={['Id','Guest','Target','Rating','Comment','Approved','Actions']} rows={items.map(f => [
      f.feedbackId, f.guestSessionId || '', `P:${f.placeId ?? '-'} D:${f.dishId ?? '-'} N:${f.narrationId ?? '-'}`, f.rating, f.comment || '', f.isApproved ? 'Yes' : 'No',
      <div className="flex gap-2" key="a"><Button className="px-3 py-2" onClick={() => approve(f.feedbackId)}>Approve</Button><Button className="px-3 py-2" variant="danger" onClick={() => reject(f.feedbackId)}>Reject</Button></div>
    ])} />
  </div>;
}
