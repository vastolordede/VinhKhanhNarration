import { FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { vendorLogin } from '../../api/vendorApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';

export default function VendorLoginScreen() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const navigate = useNavigate();

  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const result = await vendorLogin(email, password);
      localStorage.setItem('vendorToken', result.accessToken);
      localStorage.setItem('vendorTokenExpiresAt', result.accessTokenExpiresAt);
      localStorage.setItem('vendorRefreshToken', result.refreshToken);
      localStorage.setItem('vendorRefreshTokenExpiresAt', result.refreshTokenExpiresAt);
      localStorage.setItem('vendorUser', JSON.stringify(result.vendor));
      navigate('/vendor');
    } catch (loginError) {
      setError(loginError instanceof Error ? loginError.message : 'Đăng nhập thất bại.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 p-5">
      <Card className="w-full max-w-md">
        <h1 className="text-2xl font-bold text-slate-900">Vendor Login</h1>
        <p className="mt-2 text-sm text-slate-500">Đăng nhập để quản lý sạp và nội dung.</p>
        <form onSubmit={submit} className="mt-6 space-y-4">
          <Input type="email" required placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
          <Input type="password" required placeholder="Mật khẩu" value={password} onChange={(e) => setPassword(e.target.value)} />
          {error && <p className="rounded-xl bg-rose-50 p-3 text-sm text-rose-600">{error}</p>}
          <Button className="w-full" disabled={busy}>{busy ? 'Đang đăng nhập...' : 'Đăng nhập'}</Button>
        </form>
        <p className="mt-4 text-center text-sm text-slate-500">
          Chưa có tài khoản? <Link className="font-semibold text-teal-700" to="/vendor/register">Đăng ký Vendor</Link>
        </p>
      </Card>
    </div>
  );
}
