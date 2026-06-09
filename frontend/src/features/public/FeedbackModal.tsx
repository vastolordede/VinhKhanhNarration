import { useState } from 'react';
import { submitFeedback } from '../../api/publicApi';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import { Textarea } from '../../components/ui/Textarea';
import { useAppContext } from '../../contexts/AppContext';
import { useI18n } from '../../i18n/useI18n';

export default function FeedbackModal({
  open,
  onClose,
  placeId,
  dishId,
  narrationId
}: {
  open: boolean;
  onClose: () => void;
  placeId?: number | null;
  dishId?: number | null;
  narrationId?: number | null;
}) {
  const { guestSession } = useAppContext();
  const { t } = useI18n();

  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    if (saving) return;

    setSaving(true);
    setMessage(null);
    setError(null);

    try {
      await submitFeedback({
        guestSessionId: guestSession?.guestSessionId,
        placeId,
        dishId,
        narrationId,
        rating,
        comment,
        isApproved: false
      });

      setComment('');
      setRating(5);
      setMessage(t('public.feedback.success'));

      setTimeout(() => {
        setMessage(null);
        onClose();
      }, 600);
    } catch {
      setError(t('public.feedback.error'));
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal open={open} title={t('public.feedback.title')} onClose={onClose}>
      <div className="space-y-4">
        <div className="flex gap-2">
          {[1, 2, 3, 4, 5].map((star) => (
            <button
              key={star}
              type="button"
              onClick={() => setRating(star)}
              className={`h-10 w-10 rounded-full text-lg ${
                star <= rating
                  ? 'bg-amber-100 text-amber-600'
                  : 'bg-slate-100 text-slate-400'
              }`}
            >
              ★
            </button>
          ))}
        </div>

        <Textarea
          rows={4}
          placeholder={t('public.feedback.placeholder')}
          value={comment}
          onChange={(e) => setComment(e.target.value)}
        />

        {message && (
          <p className="rounded-2xl bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-700">
            {message}
          </p>
        )}

        {error && (
          <p className="rounded-2xl bg-rose-50 px-4 py-3 text-sm font-semibold text-rose-600">
            {error}
          </p>
        )}

        <Button onClick={submit} disabled={saving} className="w-full">
          {saving ? t('public.feedback.sending') : t('public.feedback.submit')}
        </Button>
      </div>
    </Modal>
  );
}