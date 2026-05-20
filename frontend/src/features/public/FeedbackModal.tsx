import { useState } from 'react';
import { submitFeedback } from '../../api/publicApi';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import { Textarea } from '../../components/ui/Textarea';
import { useAppContext } from '../../contexts/AppContext';

export default function FeedbackModal({ open, onClose, placeId, dishId, narrationId }: { open: boolean; onClose: () => void; placeId?: number | null; dishId?: number | null; narrationId?: number | null }) {
  const { guestSession } = useAppContext();
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [saving, setSaving] = useState(false);

  async function submit() {
    setSaving(true);
    await submitFeedback({ guestSessionId: guestSession?.guestSessionId, placeId, dishId, narrationId, rating, comment, isApproved: false });
    setSaving(false);
    setComment('');
    onClose();
  }

  return (
    <Modal open={open} title="Gửi đánh giá" onClose={onClose}>
      <div className="space-y-4">
        <div className="flex gap-2">
          {[1, 2, 3, 4, 5].map((star) => (
            <button key={star} onClick={() => setRating(star)} className={`h-10 w-10 rounded-full text-lg ${star <= rating ? 'bg-amber-100 text-amber-600' : 'bg-slate-100 text-slate-400'}`}>★</button>
          ))}
        </div>
        <Textarea rows={4} placeholder="Cảm nhận của bạn..." value={comment} onChange={(e) => setComment(e.target.value)} />
        <Button onClick={submit} disabled={saving} className="w-full">{saving ? 'Đang gửi...' : 'Gửi feedback'}</Button>
      </div>
    </Modal>
  );
}
