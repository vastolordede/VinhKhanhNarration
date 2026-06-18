import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { Bell, CreditCard, FileText, LayoutDashboard, LogOut } from 'lucide-react';

const navItems = [
  { to: '/vendor', label: 'Tổng quan', icon: LayoutDashboard },
  { to: '/vendor/narrations', label: 'Thuyết minh', icon: FileText },
  { to: '/vendor/subscription', label: 'Gia hạn', icon: CreditCard },
  { to: '/vendor/notifications', label: 'Thông báo', icon: Bell }
];

export default function VendorShell() {
  const navigate = useNavigate();
  const raw = localStorage.getItem('vendorUser');
  const vendor = raw ? JSON.parse(raw) : null;

  function logout() {
    localStorage.removeItem('vendorToken');
    localStorage.removeItem('vendorTokenExpiresAt');
    localStorage.removeItem('vendorUser');
    navigate('/vendor/login');
  }

  return (
    <div className="min-h-screen bg-slate-50 lg:flex">
      <aside className="w-full border-b border-slate-200 bg-white p-4 lg:min-h-screen lg:w-72 lg:border-b-0 lg:border-r">
        <div className="rounded-3xl bg-teal-700 p-4 text-white">
          <p className="text-sm opacity-80">Vendor Portal</p>
          <h1 className="text-xl font-bold">{vendor?.shopName || 'Vĩnh Khánh'}</h1>
          <p className="mt-1 text-xs opacity-80">{vendor?.accountStatus}</p>
        </div>

        <nav className="mt-4 grid grid-cols-2 gap-2 lg:block lg:space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === '/vendor'}
                className={({ isActive }) =>
                  `flex items-center gap-2 rounded-2xl px-3 py-2.5 text-sm font-semibold ${
                    isActive
                      ? 'bg-teal-50 text-teal-700'
                      : 'text-slate-600 hover:bg-slate-50'
                  }`
                }
              >
                <Icon size={18} />
                {item.label}
              </NavLink>
            );
          })}
        </nav>

        <button
          onClick={logout}
          className="mt-4 flex w-full items-center gap-2 rounded-2xl px-3 py-2.5 text-sm font-semibold text-rose-600 hover:bg-rose-50"
        >
          <LogOut size={18} /> Đăng xuất
        </button>
      </aside>

      <main className="min-w-0 flex-1 p-4 sm:p-6 lg:p-8">
        <div className="mx-auto max-w-screen-xl">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
