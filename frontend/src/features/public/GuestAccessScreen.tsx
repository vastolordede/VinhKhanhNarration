import { useEffect, useMemo, useState } from 'react';
import { Copy, ExternalLink, ShieldCheck } from 'lucide-react';
import { useSearchParams } from 'react-router-dom';
import {
  confirmGuestAccessPayment,
  createGuestAccessOrder,
  getGuestAccessOrder,
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

  const [searchParams, setSearchParams] = useSearchParams();
  const requestedOrderCode = searchParams.get('orderCode')?.trim() ?? '';

  const [order, setOrder] = useState<GuestPaymentOrderDTO | null>(null);
  const [busy, setBusy] = useState(false);
  const [loadingOrder, setLoadingOrder] = useState(false);
  const [copied, setCopied] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const expiresLabel = useMemo(() => {
    const value = accessStatus?.accessPass?.expiresAt;
    return value ? new Date(value).toLocaleString('vi-VN') : null;
  }, [accessStatus?.accessPass?.expiresAt]);

  const paymentUrl = useMemo(() => {
    if (!order) return '';

    if (order.paymentUrl?.trim()) {
      return order.paymentUrl.trim();
    }

    return `${window.location.origin}/app/access?orderCode=${encodeURIComponent(
      order.orderCode
    )}`;
  }, [order]);

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

  useEffect(() => {
    if (!requestedOrderCode) return;

    let mounted = true;
    setLoadingOrder(true);
    setError(null);

    getGuestAccessOrder(requestedOrderCode)
      .then(async (loadedOrder) => {
        if (!mounted) return;
        setOrder(loadedOrder);

        if (loadedOrder.status === 'Paid' && loadedOrder.guestSessionId) {
          const status = await getGuestAccessStatus(loadedOrder.guestSessionId);
          if (!mounted) return;

          if (status.hasActivePass && status.guestSession?.guestSessionId) {
            setGuestSession(status.guestSession);
            setAccessStatus(status);
            setTrackingEnabled(false);
            setCurrentNarration(null);
            setOrder(null);
            setSearchParams({}, { replace: true });
          }
        }
      })
      .catch((loadError) => {
        if (!mounted) return;
        setOrder(null);
        setError(getApiError(loadError));
      })
      .finally(() => {
        if (mounted) setLoadingOrder(false);
      });

    return () => {
      mounted = false;
    };
  }, [
    requestedOrderCode,
    setAccessStatus,
    setCurrentNarration,
    setGuestSession,
    setSearchParams,
    setTrackingEnabled
  ]);

  async function createOrder() {
    setBusy(true);
    setCopied(false);
    setError(null);

    try {
      const created = await createGuestAccessOrder(
        language?.languageId ?? null,
        navigator.userAgent
      );

      setOrder(created);
      setSearchParams({ orderCode: created.orderCode }, { replace: true });
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setBusy(false);
    }
  }

  async function confirm() {
    if (!order || order.status !== 'Pending') return;

    setBusy(true);
    setError(null);

    try {
      const status = await confirmGuestAccessPayment(order.orderCode);
      if (!status.guestSession?.guestSessionId || !status.hasActivePass) {
        throw new Error('Backend chưa tạo được Guest Session sau thanh toán.');
      }

      // Đây là xác nhận mô phỏng. Không có cổng thanh toán hoặc giao dịch ngân hàng.
      setGuestSession(status.guestSession);
      setAccessStatus(status);
      setOrder(null);
      setSearchParams({}, { replace: true });
      setTrackingEnabled(false);
      setCurrentNarration(null);
    } catch (requestError) {
      setError(getApiError(requestError));
    } finally {
      setBusy(false);
    }
  }

  async function copyOrderCode() {
    if (!order) return;

    try {
      await navigator.clipboard.writeText(order.orderCode);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1800);
    } catch {
      setCopied(false);
    }
  }

  const isPending = order?.status === 'Pending';
  const isExpired = order?.status === 'Expired';

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-8">
      <h1 className="text-2xl font-bold text-slate-900">Access Pass 24 giờ</h1>
      <p className="mt-2 text-sm text-slate-500">
        Bản đồ và menu được xem miễn phí. Mỗi lần xác nhận thanh toán mô phỏng
        thành công sẽ tạo một Guest Session mới có thời hạn 24 giờ.
      </p>

      <div className="mt-5 space-y-4">
        <div className="rounded-2xl border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-900">
          <p className="font-bold">MÔ PHỎNG THANH TOÁN — KHÔNG CHUYỂN TIỀN</p>
          <p className="mt-1 text-xs leading-5">
            Đây là một ảnh QR Mock cố định dùng chung cho mọi đơn. QR không chứa
            tài khoản ngân hàng, không phải VietQR và không kết nối cổng thanh toán thật.
          </p>
        </div>

        {accessStatus?.hasActivePass && guestSession ? (
          <Card>
            <div className="flex items-start gap-3">
              <div className="rounded-2xl bg-emerald-50 p-3 text-emerald-700">
                <ShieldCheck className="h-6 w-6" />
              </div>
              <div>
                <p className="text-sm font-semibold text-emerald-700">
                  Pass đang hoạt động
                </p>
                <p className="mt-2 text-3xl font-black text-slate-900">
                  {accessStatus.hoursRemaining} giờ
                </p>
                <p className="mt-1 text-sm text-slate-500">
                  Hết hạn: {expiresLabel}
                </p>
                <p className="mt-2 break-all text-xs text-slate-400">
                  Session: {guestSession.guestSessionId}
                </p>
              </div>
            </div>
          </Card>
        ) : (
          <Card>
            <p className="font-semibold text-slate-900">Chưa có Access Pass</p>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Hệ thống sẽ tạo một đơn Mock mới nhưng luôn hiển thị cùng một ảnh QR.
              Guest Session chỉ được sinh sau khi bạn bấm xác nhận thanh toán mô phỏng.
            </p>

            {!order && !loadingOrder && (
              <Button className="mt-4 w-full" disabled={busy} onClick={createOrder}>
                {busy ? 'Đang tạo đơn...' : 'Tạo đơn thanh toán mô phỏng'}
              </Button>
            )}

            {loadingOrder && (
              <p className="mt-4 text-sm font-medium text-slate-500">
                Đang tải đơn thanh toán...
              </p>
            )}
          </Card>
        )}

        {order && !accessStatus?.hasActivePass && (
          <Card>
            <div className="text-center">
              <span className="inline-flex rounded-full bg-rose-100 px-3 py-1 text-xs font-extrabold tracking-wide text-rose-700">
                DEMO / MOCK PAYMENT
              </span>

              <h2 className="mt-3 text-lg font-bold text-slate-900">
                QR thanh toán mô phỏng
              </h2>

              {isPending && (
                <div className="mx-auto mt-4 w-fit rounded-3xl border-4 border-white bg-white p-4 shadow-sm ring-1 ring-slate-200">
                  <img
                    src="/mock-payment-qr.svg"
                    alt="QR thanh toán mô phỏng cố định"
                    className="h-[220px] w-[220px]"
                  />
                </div>
              )}

              <p className="mt-4 text-3xl font-black text-teal-700">
                {order.amount.toLocaleString('vi-VN')} VND
              </p>

              <div className="mx-auto mt-3 flex max-w-md items-center justify-between gap-2 rounded-2xl bg-slate-100 px-3 py-3 text-left">
                <div className="min-w-0">
                  <p className="text-xs text-slate-500">Mã đơn Mock</p>
                  <p className="truncate font-mono text-sm font-bold text-slate-800">
                    {order.orderCode}
                  </p>
                </div>
                <button
                  type="button"
                  onClick={copyOrderCode}
                  className="shrink-0 rounded-xl bg-white p-2 text-slate-600 shadow-sm hover:text-teal-700"
                  aria-label="Sao chép mã đơn"
                >
                  <Copy className="h-4 w-4" />
                </button>
              </div>

              {copied && (
                <p className="mt-2 text-xs font-semibold text-emerald-700">
                  Đã sao chép mã đơn.
                </p>
              )}

              <p className="mt-3 text-xs text-slate-500">
                Mã hết hạn: {new Date(order.expiresAt).toLocaleString('vi-VN')}
              </p>

              {paymentUrl && (
                <a
                  href={paymentUrl}
                  className="mt-3 inline-flex items-center gap-1 text-xs font-semibold text-teal-700 hover:underline"
                >
                  Mở trang Mock Payment
                  <ExternalLink className="h-3.5 w-3.5" />
                </a>
              )}

              {isExpired ? (
                <Button className="mt-4 w-full" disabled={busy} onClick={createOrder}>
                  Tạo đơn Mock mới
                </Button>
              ) : (
                <Button
                  className="mt-4 w-full"
                  disabled={busy || !isPending}
                  onClick={confirm}
                >
                  {busy
                    ? 'Đang xác nhận...'
                    : 'Xác nhận thanh toán mô phỏng'}
                </Button>
              )}

              <p className="mt-3 text-xs leading-5 text-rose-600">
                Nút xác nhận chỉ đổi trạng thái đơn Mock trong database. Không có
                tiền thật được gửi hoặc nhận.
              </p>
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
