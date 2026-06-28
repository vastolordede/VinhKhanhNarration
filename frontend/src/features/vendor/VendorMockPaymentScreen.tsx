import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { confirmMockPayment, getVendorDashboard } from '../../api/vendorApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { PaymentOrderDTO } from '../../types';

export default function VendorMockPaymentScreen() {
  const [params] = useSearchParams();
  const orderCode = params.get('orderCode') || '';
  const [order, setOrder] = useState<PaymentOrderDTO | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getVendorDashboard().then((d) => setOrder(d.pendingPayment || null)).catch((e) => setError(e.message));
  }, []);

  async function pay() {
    setError(null);
    try {
      const result = await confirmMockPayment(orderCode);
      localStorage.setItem('vendorUser', JSON.stringify(result.vendor));
      setMessage(`Thanh toán demo thành công. Gói có hiệu lực đến ${result.subscription ? new Date(result.subscription.expiresAt).toLocaleString('vi-VN') : ''}.`);
      setOrder(null);
    } catch (e) { setError(e instanceof Error ? e.message : 'Thanh toán thất bại.'); }
  }

  return (
    <Card className="mx-auto max-w-lg text-center">
      <h1 className="text-2xl font-bold">Thanh toán demo</h1>
      {order && <>
        <p className="mt-3 font-semibold">{order.orderCode}</p>
        <p className="text-slate-600">{order.amount.toLocaleString('vi-VN')} VND · {order.purpose}</p>
        <img
          className="mx-auto mt-4 h-64 w-64"
          src="/mock-payment-qr.svg"
          alt="QR thanh toán demo cố định"
        />
        <p className="mt-2 text-xs leading-5 text-slate-500">
          Đây là ảnh QR Mock cố định dùng chung cho mọi đơn. Mã đơn và luồng xác nhận
          vẫn được xử lý riêng, không có tiền thật được gửi hoặc nhận.
        </p>
        <Button className="mt-4 w-full" onClick={() => void pay()}>Xác nhận đã thanh toán (Demo)</Button>
      </>}
      {message && <p className="mt-4 rounded-xl bg-emerald-50 p-3 text-emerald-700">{message}</p>}
      {error && <p className="mt-4 rounded-xl bg-rose-50 p-3 text-rose-600">{error}</p>}
      <Link className="mt-4 inline-block font-semibold text-teal-700" to="/vendor">Về tổng quan</Link>
    </Card>
  );
}
