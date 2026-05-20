import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { http, unwrap } from '../../api/http';
import { endpoints } from '../../api/endpoints';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';

export default function AdminLoginScreen() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      const response = await http.post(endpoints.authLogin, { email, password });
      const data = unwrap<{ token?: string; accessToken?: string }>(response);
      const token = data.token || data.accessToken || '';
      if (token) localStorage.setItem('adminToken', token);
      navigate('/admin');
    } catch {
      setError('Đăng nhập thất bại. Kiểm tra email/password hoặc backend API.');
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 p-5">
      <Card className="w-full max-w-md">
        <h1 className="text-2xl font-bold text-slate-900">Admin Login</h1>
        <p className="mt-2 text-sm text-slate-500">Đăng nhập tài khoản nội bộ để quản lý dữ liệu.</p>
        <form onSubmit={submit} className="mt-6 space-y-4">
          <Input type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
          <Input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
          {error && <p className="text-sm text-rose-600">{error}</p>}
          <Button className="w-full">Đăng nhập</Button>
        </form>
      </Card>
    </div>
  );
}
