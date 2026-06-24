import { useEffect, useMemo, useState } from 'react';
import {
  confirmGuestAccessPayment,
  createGuestAccessOrder,
  getGuestAccessStatus
} from '../../api/publicApi';
import { getApiError } from '../../api/http';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { GuestPaymentOrderDTO } from '../../types';

export default function GuestAccessScreen() {
  const {
    guestSession,
    setGuestSession,
    language,
    accessStatus,
    setAccessStatus,
    setTrackingEnabled,
    setCurrentNarration
  } = useAppContext();
  const [order, setOrder] = useState<GuestPaymentOrderDTO | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const expiresLabel = useMemo(() => {
    const value = accessStatus?.accessPass?.expiresAt;
    return value ? new Date(value).toLocaleString() : null;
  }, [accessStatus?.accessPass?.expiresAt]);

  useEffect(() => {
    const guestSessionId = guestSession?.guestSessionId;
    if (!guestSessionId) {
      setAccessStatus(null);
      return;
    }

    let mounted = true;
    getGuestAccessStatus(guestSessionId)
      .then((status) => {
        if (!mounted) return;
        setAccessStatus(status);
        if (!status.hasActivePass || !status.guestSession?.isActive) {
          setGuestSession(null);
          setCurrentNarration(null);
          setTrackingEnabled(false);
        }
      })
      .catch((loadError) => {
        if (!mounted) return;
        setGuestSession(null);
        setAccessStatus(null);
        setCurrentNarration(null);
        setTrackingEnabled(false);
        setError(getApiError(loadError));
      });

    return () => {
      mounted = false;
    };
  }, [
    guestSession?.guestSessionId,
    setAccessStatus,
    setCurrentNarration,
    setGuestSession,
    setTrackingEnabled
  ]);

  async function createOrder() {
    setBusy(true);
    setError(null);
    try {
      const created = await createGuestAccessOrder(
        language?.languageId ?? null,
        navigator.userAgent
      );
      setOrder(created);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setBusy(false);
    }
  }

  async function confirm() {
    if (!order) return;
    setBusy(true);
    setError(null);
    try {
      const status = await confirmGuestAccessPayment(order.orderCode);
      if (!status.guestSession?.guestSessionId || !status.hasActivePass) {
        throw new Error('Backend chưa tạo được Guest Session sau thanh toán.');
      }

      // Mỗi lần thanh toán thành công luôn thay bằng một Guest Session mới.
      setGuestSession(status.guestSession);
      setAccessStatus(status);
      setOrder(null);
      setTrackingEnabled(false);
      setCurrentNarration(null);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-8">
      <h1 className="text-2xl font-bold text-slate-900">Access Pass 24 giờ</h1>
      <p className="mt-2 text-sm text-slate-500">
        Bản đồ và menu được xem miễn phí. Mỗi lần thanh toán thành công tạo một
        Guest Session mới có thời hạn 24 giờ.
      </p>

      <div className="mt-5 space-y-4">
        {accessStatus?.hasActivePass && guestSession ? (
          <Card>
            <p className="text-sm font-semibold text-emerald-700">
              Pass đang hoạt động
            </p>
            <p className="mt-2 text-3xl font-black text-slate-900">
              {accessStatus.hoursRemaining} giờ
            </p>
            <p className="mt-1 text-sm text-slate-500">Hết hạn: {expiresLabel}</p>
            <p className="mt-2 break-all text-xs text-slate-400">
              Session: {guestSession.guestSessionId}
            </p>
          </Card>
        ) : (
          <Card>
            <p className="font-semibold text-slate-900">Chưa có Access Pass</p>
            <p className="mt-2 text-sm text-slate-500">
              Thanh toán bên dưới là mô phỏng. Guest Session chỉ được sinh sau
              khi xác nhận thanh toán thành công.
            </p>
            {!order && (
              <Button className="mt-4 w-full" disabled={busy} onClick={createOrder}>
                {busy ? 'Đang tạo mã...' : 'Tạo thanh toán mô phỏng'}
              </Button>
            )}
          </Card>
        )}

        {order && !accessStatus?.hasActivePass && (
          <Card>
            <div className="text-center">
              <p className="rounded-2xl bg-slate-100 px-4 py-5 font-mono text-sm font-bold text-slate-800">
                {order.orderCode}
              </p>
              <p className="mt-2 text-2xl font-black text-teal-700">
                {order.amount.toLocaleString('vi-VN')} VND
              </p>
              <p className="mt-1 text-xs text-slate-500">
                Mã hết hạn: {new Date(order.expiresAt).toLocaleString()}
              </p>
              <Button className="mt-4 w-full" disabled={busy} onClick={confirm}>
                {busy ? 'Đang xác nhận...' : 'Xác nhận đã thanh toán (Mock)'}
              </Button>
            </div>
          </Card>
        )}

        {error && (
          <p className="rounded-2xl bg-rose-50 p-4 text-sm font-semibold text-rose-700">
            {error}
          </p>
        )}
      </div>
    </div>
  );
}
