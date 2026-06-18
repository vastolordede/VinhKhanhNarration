import { FormEvent, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getVendorDashboard, submitVendorRenewal } from '../../api/vendorApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { VendorDashboardDTO } from '../../types';

export default function VendorSubscriptionScreen() {
  const [dashboard, setDashboard] = useState<VendorDashboardDTO | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() { setDashboard(await getVendorDashboard()); }
  useEffect(() => { void load().catch((e) => setError(e.message)); }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    try {
      const id = await submitVendorRenewal(new FormData(event.currentTarget));
      setMessage(`Đã gửi yêu cầu gia hạn #${id}. Admin sẽ kiểm tra giấy an toàn thực phẩm.`);
      event.currentTarget.reset();
      await load();
    } catch (e) { setError(e instanceof Error ? e.message : 'Không gửi được.'); }
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Gói 6 tháng và gia hạn</h1>
      {dashboard?.pendingPayment && (
        <Card>
          <p className="font-bold">Có mã thanh toán đang chờ</p>
          <p className="mt-1 text-sm">{dashboard.pendingPayment.orderCode} · {dashboard.pendingPayment.amount.toLocaleString('vi-VN')} VND</p>
          <Link className="mt-3 inline-block font-semibold text-teal-700" to={`/vendor/mock-payment?orderCode=${encodeURIComponent(dashboard.pendingPayment.orderCode)}`}>Thanh toán demo</Link>
        </Card>
      )}
      <Card>
        <h2 className="font-bold">Gửi hồ sơ gia hạn</h2>
        <p className="mt-1 text-sm text-slate-500">Gia hạn chỉ cần giấy an toàn thực phẩm còn hiệu lực.</p>
        <form onSubmit={submit} className="mt-4 space-y-4">
          <Input name="FoodSafetyExpiresAt" type="date" required />
          <input name="FoodSafetyCertificate" type="file" accept=".pdf,.jpg,.jpeg,.png,.webp" required className="block w-full text-sm" />
          {message && <p className="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700">{message}</p>}
          {error && <p className="rounded-xl bg-rose-50 p-3 text-sm text-rose-600">{error}</p>}
          <Button>Gửi hồ sơ gia hạn</Button>
        </form>
      </Card>
    </div>
  );
}
