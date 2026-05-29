import { NavLink, Outlet } from 'react-router-dom';
import {
  BarChart3,
  Database,
  FileAudio,
  Languages,
  MapPinned,
  MessageSquare,
  Navigation,
  QrCode,
  ScrollText,
  Utensils
} from 'lucide-react';
import { LanguageSwitcher } from '../common/LanguageSwitcher';
import { useI18n } from '../../i18n/useI18n';

const navItems = [
  { to: '/admin', label: 'Dashboard', icon: BarChart3 },
  { to: '/admin/lookups', label: 'Lookup', icon: Database },
  { to: '/admin/languages', label: 'Languages', icon: Languages },
  { to: '/admin/places', label: 'Places / POI', icon: MapPinned },
  { to: '/admin/dishes', label: 'Dishes', icon: Utensils },
  { to: '/admin/narrations', label: 'Narrations', icon: ScrollText },
  { to: '/admin/translations', label: 'Translations', icon: Languages },
  { to: '/admin/audio', label: 'Audio', icon: FileAudio },
  { to: '/admin/qr-codes', label: 'QR Codes', icon: QrCode },
  { to: '/admin/feedbacks', label: 'Feedbacks', icon: MessageSquare },
  { to: '/admin/listening-histories', label: 'Listening', icon: ScrollText },
  { to: '/admin/geofence-events', label: 'Geofence', icon: Navigation }
];

export default function AdminShell() {
  const { tx } = useI18n();

  return (
    <div className="admin-shell flex min-h-screen w-full bg-slate-50">
      <aside className="hidden min-h-screen w-72 shrink-0 border-r border-slate-200 bg-white p-4 lg:block">
        <div className="mb-4 rounded-3xl bg-teal-700 p-4 text-white">
          <p className="text-sm opacity-80">{tx('Admin')}</p>
          <h1 className="text-xl font-bold">Vĩnh Khánh Narration</h1>
        </div>

        <div className="mb-5">
          <LanguageSwitcher />
        </div>

        <nav className="space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;

            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === '/admin'}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-2xl px-3 py-2.5 text-sm font-semibold ${
                    isActive ? 'bg-teal-50 text-teal-700' : 'text-slate-600 hover:bg-slate-50'
                  }`
                }
              >
                <Icon size={18} />
                {tx(item.label)}
              </NavLink>
            );
          })}
        </nav>
      </aside>

      <main className="min-w-0 flex-1 px-4 py-4 sm:px-6 lg:px-8 xl:px-10 2xl:px-12">
        <div className="mx-auto w-full max-w-screen-2xl">
          <Outlet />
        </div>
      </main>
    </div>
  );
}