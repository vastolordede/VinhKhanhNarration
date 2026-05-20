import { Outlet, NavLink } from 'react-router-dom';
import { Map, QrCode, Settings } from 'lucide-react';

const navItems = [
  { to: '/app/map', label: 'Bản đồ', icon: Map },
  { to: '/app/qr', label: 'QR', icon: QrCode },
  { to: '/app/settings', label: 'Cài đặt', icon: Settings }
];

export default function PublicShell() {
  return (
    <div className="mobile-shell relative overflow-hidden">
      <main className="min-h-screen pb-20">
        <Outlet />
      </main>
      <nav className="fixed inset-x-0 bottom-0 z-[800] mx-auto max-w-[520px] border-t border-slate-200 bg-white/95 px-4 py-2 backdrop-blur safe-bottom">
        <div className="grid grid-cols-3 gap-2">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `flex flex-col items-center rounded-2xl px-3 py-2 text-xs font-semibold ${isActive ? 'bg-teal-50 text-teal-700' : 'text-slate-500'}`
                }
              >
                <Icon size={20} />
                <span>{item.label}</span>
              </NavLink>
            );
          })}
        </div>
      </nav>
    </div>
  );
}
