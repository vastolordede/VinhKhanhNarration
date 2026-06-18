import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getVendorDashboard } from '../../api/vendorApi';
import { Card } from '../../components/ui/Card';
import { VendorDashboardDTO } from '../../types';

export default function VendorDashboardScreen() {
  const [data, setData] = useState<VendorDashboardDTO | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getVendorDashboard().then((value) => {
      setData(value);
      localStorage.setItem('vendorUser', JSON.stringify(value.vendor));
    }).catch((e) => setError(e instanceof Error ? e.message : 'Không tải được dữ liệu.'));
  }, []);

  if (error) return <Card><p className="text-rose-600">{error}</p></Card>;
  if (!data) return <Card><p>Đang tải...</p></Card>;

  return (
    <div className="space-y-4">
      {data.shouldWarnExpiry && (
        <div className="rounded-2xl bg-red-600 px-4 py-3 font-semibold text-white">
          Gói còn {data.daysRemaining} ngày. <Link className="underline" to="/vendor/subscription">Gia hạn ngay</Link>
        </div>
      )}
      <div>
        <h1 className="text-2xl font-bold">Xin chào, {data.vendor.ownerName}</h1>
        <p className="text-slate-500">{data.vendor.shopName}</p>
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        <Card><p className="text-sm text-slate-500">Trạng thái</p><p className="mt-1 text-xl font-bold">{data.vendor.accountStatus}</p></Card>
        <Card><p className="text-sm text-slate-500">Thời hạn</p><p className="mt-1 text-xl font-bold">{data.subscription ? new Date(data.subscription.expiresAt).toLocaleDateString('vi-VN') : 'Chưa kích hoạt'}</p></Card>
        <Card><p className="text-sm text-slate-500">Thông báo chưa đọc</p><p className="mt-1 text-xl font-bold">{data.unreadNotifications}</p></Card>
      </div>
      {!data.canManageContent && (
        <Card>
          <p className="font-semibold text-amber-700">Bạn chưa thể quản lý nội dung.</p>
          <p className="mt-2 text-sm text-slate-600">Hãy chờ duyệt hồ sơ, thanh toán hoặc gia hạn tài khoản.</p>
          {data.pendingPayment && <Link className="mt-3 inline-block font-semibold text-teal-700" to={`/vendor/mock-payment?orderCode=${encodeURIComponent(data.pendingPayment.orderCode)}`}>Mở thanh toán demo</Link>}
        </Card>
      )}
    </div>
  );
}
