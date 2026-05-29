import { Outlet, NavLink } from 'react-router-dom';
import { Map, QrCode, Settings } from 'lucide-react';
import { useI18n } from '../../i18n/useI18n';

const navItems = [
  { to: '/app/map', labelKey: 'public.nav.map', icon: Map },
  { to: '/app/qr', labelKey: 'public.nav.qr', icon: QrCode },
  { to: '/app/settings', labelKey: 'public.nav.settings', icon: Settings }
];

export default function PublicShell() {
  const { t } = useI18n();

  return (
    <div className="public-page-bg">
      <div className="mobile-shell responsive-public-shell relative overflow-hidden">
        

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
                  <span>{t(item.labelKey)}</span>
                </NavLink>
              );
            })}
          </div>
        </nav>
      </div>
    </div>
  );
}