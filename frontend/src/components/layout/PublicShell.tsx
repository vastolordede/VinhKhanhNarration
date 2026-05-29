import { Outlet, NavLink } from 'react-router-dom';
import { Map, QrCode, Settings } from 'lucide-react';
import { LanguageSwitcher } from '../common/LanguageSwitcher';
import { useI18n } from '../../i18n/useI18n';

const navItems = [
  { to: '/app/map', label: 'Bản đồ', icon: Map },
  { to: '/app/qr', label: 'QR', icon: QrCode },
  { to: '/app/settings', label: 'Cài đặt', icon: Settings }
];

export default function PublicShell() {
  const { tx } = useI18n();

  return (
    <div className="public-page-bg">
      <div className="mobile-shell responsive-public-shell relative overflow-hidden">
        <div className="fixed right-4 top-4 z-[850]">
          {/* UI language only. This does NOT change narration/listening language. */}
          <LanguageSwitcher compact />
        </div>

        <main className="min-h-screen pb-20">
          <Outlet />
        </main>

        <nav className="fixed inset-x-0 bottom-0 z-[800] mx-auto w-full max-w-[430px] border-t border-slate-200 bg-white/95 px-4 py-2 backdrop-blur safe-bottom sm:max-w-[520px] md:max-w-[768px] lg:max-w-[1024px]">
          <div className="grid grid-cols-3 gap-2">
            {navItems.map((item) => {
              const Icon = item.icon;

              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  className={({ isActive }) =>
                    `flex flex-col items-center rounded-2xl px-3 py-2 text-xs font-semibold ${
                      isActive ? 'bg-teal-50 text-teal-700' : 'text-slate-500'
                    }`
                  }
                >
                  <Icon size={20} />
                  <span>{tx(item.label)}</span>
                </NavLink>
              );
            })}
          </div>
        </nav>
      </div>
    </div>
  );
}