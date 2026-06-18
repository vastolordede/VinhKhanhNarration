import { useEffect, useState } from 'react';
import {
  getNarrations,
  moderateAdminNarration,
  processNarrationPipeline,
  reviewNarration
} from '../../api/narrationApi';
import { getApiError } from '../../api/http';
import { PageHeader } from '../../components/layout/PageHeader';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { NarrationContentDTO } from '../../types';

export default function NarrationManagementScreen() {
  const [items, setItems] = useState<NarrationContentDTO[]>([]);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setItems(await getNarrations());
  }

  useEffect(() => { void load().catch((e) => setError(getApiError(e))); }, []);

  async function run(id: number, action: () => Promise<void>) {
    setBusyId(id);
    setMessage(null);
    setError(null);
    try {
      await action();
      await load();
    } catch (e) {
      setError(getApiError(e));
    } finally {
      setBusyId(null);
    }
  }

  async function approve(item: NarrationContentDTO) {
    await run(item.narrationId, async () => {
      const result = await reviewNarration(item.narrationId, { approved: true });
      const process = result.processing;
      setMessage(process
        ? `Đã duyệt. ${process.audioReady}/${process.audioTargetCount} audio sẵn sàng.${process.errors.length ? ` Lỗi: ${process.errors.join(' | ')}` : ''}`
        : 'Đã duyệt nội dung.');
    });
  }

  async function reject(item: NarrationContentDTO) {
    const reason = window.prompt('Nhập lý do từ chối:')?.trim();
    if (!reason) return;
    await run(item.narrationId, async () => {
      await reviewNarration(item.narrationId, { approved: false, rejectionReason: reason });
      setMessage('Đã từ chối và gửi thông báo cho Vendor.');
    });
  }

  async function moderate(item: NarrationContentDTO, action: 'hide' | 'delete' | 'restore') {
    const reason = action === 'restore'
      ? window.prompt('Ghi chú khôi phục (có thể để trống):') || undefined
      : window.prompt(`Nhập lý do ${action === 'hide' ? 'ẩn' : 'xóa'}:`)?.trim();
    if (action !== 'restore' && !reason) return;

    await run(item.narrationId, async () => {
      await moderateAdminNarration(item.narrationId, action, reason);
      setMessage('Đã cập nhật trạng thái và thông báo cho Vendor.');
    });
  }

  async function retry(item: NarrationContentDTO) {
    await run(item.narrationId, async () => {
      const result = await processNarrationPipeline(item.narrationId, 0);
      setMessage(`Đồng bộ xong: ${result.audioReady}/${result.audioTargetCount} audio sẵn sàng.`);
    });
  }

  return (
    <div>
      <PageHeader
        title="Kiểm duyệt thuyết minh"
        description="Admin chỉ phê duyệt, từ chối, ẩn, xóa mềm và khôi phục. Vendor là bên duy nhất thêm hoặc sửa nội dung."
      />

      {message && <p className="mb-4 rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700">{message}</p>}
      {error && <p className="mb-4 rounded-xl bg-rose-50 p-3 text-sm text-rose-600">{error}</p>}

      <div className="space-y-3">
        {items.map((item) => {
          const adminHidden = item.workflowStatus === 'HiddenByAdmin';
          const adminDeleted = item.workflowStatus === 'DeletedByAdmin';
          return (
            <Card key={item.narrationId}>
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <p className="text-xs font-bold uppercase text-teal-700">#{item.narrationId} · {item.workflowStatus}</p>
                  <h2 className="mt-1 text-lg font-bold">{item.title}</h2>
                  <p className="mt-2 line-clamp-4 text-sm text-slate-600">{item.originalText}</p>
                  <p className="mt-2 text-xs text-slate-500">Vendor #{item.submittedByVendorId || '-'} · Place #{item.placeId || '-'} · Dish #{item.dishId || '-'}</p>
                  {(item.rejectionReason || item.moderationReason) && (
                    <p className="mt-2 rounded-xl bg-rose-50 p-2 text-sm font-semibold text-rose-600">Lý do: {item.rejectionReason || item.moderationReason}</p>
                  )}
                </div>

                <div className="flex flex-wrap gap-2">
                  {item.workflowStatus === 'PendingReview' && <>
                    <Button disabled={busyId === item.narrationId} onClick={() => void approve(item)}>Duyệt</Button>
                    <Button variant="secondary" disabled={busyId === item.narrationId} onClick={() => void reject(item)}>Từ chối</Button>
                  </>}
                  {(item.workflowStatus === 'Approved' || item.workflowStatus === 'Published') && (
                    <Button variant="secondary" disabled={busyId === item.narrationId} onClick={() => void retry(item)}>Đồng bộ dịch & audio</Button>
                  )}
                  {!adminHidden && !adminDeleted && (
                    <Button variant="secondary" disabled={busyId === item.narrationId} onClick={() => void moderate(item, 'hide')}>Ẩn</Button>
                  )}
                  {!adminDeleted && (
                    <Button variant="secondary" disabled={busyId === item.narrationId} onClick={() => void moderate(item, 'delete')}>Xóa mềm</Button>
                  )}
                  {(adminHidden || adminDeleted) && (
                    <Button disabled={busyId === item.narrationId} onClick={() => void moderate(item, 'restore')}>Khôi phục</Button>
                  )}
                </div>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
