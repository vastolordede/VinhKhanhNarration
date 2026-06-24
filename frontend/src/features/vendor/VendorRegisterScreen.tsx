import { FormEvent, useState } from 'react';
import { Link } from 'react-router-dom';
import { vendorRegister } from '../../api/vendorApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';

export default function VendorRegisterScreen() {
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setMessage(null);
    try {
      const form = new FormData(event.currentTarget);
      const id = await vendorRegister(form);
      setMessage(`Đã gửi hồ sơ Vendor #${id}. Hãy đăng nhập để theo dõi trạng thái duyệt và thanh toán.`);
      event.currentTarget.reset();
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Không gửi được hồ sơ.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="min-h-screen bg-slate-100 p-5">
      <Card className="mx-auto max-w-2xl">
        <h1 className="text-2xl font-bold text-slate-900">Đăng ký Vendor</h1>
        <p className="mt-2 text-sm text-slate-500">Lần đầu cần giấy phép kinh doanh và giấy an toàn thực phẩm.</p>
        <form onSubmit={submit} className="mt-6 grid gap-4 sm:grid-cols-2">
          <Input name="OwnerName" required placeholder="Tên chủ sạp" />
          <Input name="ShopName" required placeholder="Tên sạp" />
          <Input name="Email" type="email" required placeholder="Email" />
          <Input name="Phone" placeholder="Số điện thoại" />
          <Input name="Password" type="password" required minLength={8} placeholder="Mật khẩu" />
          <p className="rounded-2xl bg-slate-50 px-4 py-3 text-sm text-slate-600">
            Sau khi hồ sơ được duyệt và kích hoạt gói, bạn sẽ khai báo sạp và món ăn trong trang quản lý Vendor.
          </p>
          <label className="text-sm font-semibold text-slate-700">
            Giấy phép kinh doanh
            <input name="BusinessLicense" type="file" accept=".pdf,.jpg,.jpeg,.png,.webp" required className="mt-2 block w-full text-sm" />
          </label>
          <label className="text-sm font-semibold text-slate-700">
            Giấy an toàn thực phẩm
            <input name="FoodSafetyCertificate" type="file" accept=".pdf,.jpg,.jpeg,.png,.webp" required className="mt-2 block w-full text-sm" />
          </label>
          <label className="text-sm font-semibold text-slate-700 sm:col-span-2">
            Ngày hết hạn giấy an toàn thực phẩm
            <Input name="FoodSafetyExpiresAt" type="date" required className="mt-2" />
          </label>
          {message && <p className="sm:col-span-2 rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700">{message}</p>}
          {error && <p className="sm:col-span-2 rounded-xl bg-rose-50 p-3 text-sm text-rose-600">{error}</p>}
          <Button className="sm:col-span-2" disabled={busy}>{busy ? 'Đang gửi...' : 'Gửi hồ sơ đăng ký'}</Button>
        </form>
        <p className="mt-4 text-center text-sm"><Link to="/vendor/login" className="font-semibold text-teal-700">Quay lại đăng nhập</Link></p>
      </Card>
    </div>
  );
}
