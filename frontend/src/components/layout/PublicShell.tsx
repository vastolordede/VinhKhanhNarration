import { useEffect } from 'react';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Map, Settings, TicketCheck } from 'lucide-react';
import { getGuestAccessStatus } from '../../api/publicApi';
import { useAppContext } from '../../contexts/AppContext';
import { useI18n } from '../../i18n/useI18n';
import { isDefinitiveGuestSessionFailure } from '../../utils/guestSessionPolicy';

const navItems = [
  { to: '/app/map', labelKey: 'public.nav.map', icon: Map },
  { to: '/app/access', labelKey: 'Access Pass', icon: TicketCheck },
  { to: '/app/settings', labelKey: 'public.nav.settings', icon: Settings }
];

export default function PublicShell() {
  const { t } = useI18n();
  const location = useLocation();
  const navigate = useNavigate();
  const {
    guestSession,
    setGuestSession,
    setAccessStatus,
    setCurrentNarration,
    setTrackingEnabled
  } = useAppContext();

  useEffect(() => {
    const guestSessionId = guestSession?.guestSessionId;
    if (!guestSessionId) return;
    const activeGuestSessionId = guestSessionId;

    let cancelled = false;

    function clearInvalidSession() {
      setGuestSession(null);
      setAccessStatus(null);
      setCurrentNarration(null);
      setTrackingEnabled(false);

      if (location.pathname === '/app/listen') {
        navigate('/app/access', { replace: true });
      }
    }

    async function verifyAccess() {
      try {
        const status = await getGuestAccessStatus(activeGuestSessionId);
        if (cancelled) return;

        setAccessStatus(status);
        if (!status.hasActivePass || !status.guestSession?.isActive) {
          clearInvalidSession();
        }
      } catch (error) {
        if (cancelled) return;

        if (isDefinitiveGuestSessionFailure(error)) {
          clearInvalidSession();
          return;
        }

        // Giữ session/cache khi chỉ là lỗi mạng, timeout, cold start hoặc 5xx.
        // Protected API ở backend vẫn là lớp xác thực cuối cùng.
        console.warn('Guest access verification temporarily failed:', error);
      }
    }

    void verifyAccess();
    const timer = window.setInterval(() => void verifyAccess(), 60_000);

    return () => {
      cancelled = true;
      window.clearInterval(timer);
    };
  }, [
    guestSession?.guestSessionId,
    location.pathname,
    navigate,
    setAccessStatus,
    setCurrentNarration,
    setGuestSession,
    setTrackingEnabled
  ]);

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
                      isActive
                        ? 'bg-teal-50 text-teal-700'
                        : 'text-slate-500'
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
