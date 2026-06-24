import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getGuestAccessStatus } from '../../api/publicApi';
import { useAppContext } from '../../contexts/AppContext';

export default function SplashScreen() {
  const navigate = useNavigate();
  const {
    guestSession,
    setGuestSession,
    language,
    setAccessStatus,
    setCurrentNarration,
    setTrackingEnabled
  } = useAppContext();
  const [message, setMessage] = useState('Đang kiểm tra phiên Guest...');

  useEffect(() => {
    let mounted = true;

    async function init() {
      try {
        if (guestSession?.guestSessionId) {
          try {
            const status = await getGuestAccessStatus(guestSession.guestSessionId);
            if (!mounted) return;

            if (status.hasActivePass && status.guestSession?.isActive) {
              setGuestSession(status.guestSession);
              setAccessStatus(status);
            } else {
              setGuestSession(null);
              setAccessStatus(status);
              setCurrentNarration(null);
              setTrackingEnabled(false);
            }
          } catch {
            if (!mounted) return;
            setGuestSession(null);
            setAccessStatus(null);
            setCurrentNarration(null);
            setTrackingEnabled(false);
          }
        }

        if (!mounted) return;
        setMessage('Đã sẵn sàng');
        window.setTimeout(
          () => navigate(language ? '/app/map' : '/app/language', { replace: true }),
          300
        );
      } catch {
        if (mounted) setMessage('Không thể kết nối backend API.');
      }
    }

    void init();
    return () => {
      mounted = false;
    };
  }, [
    guestSession?.guestSessionId,
    language,
    navigate,
    setAccessStatus,
    setCurrentNarration,
    setGuestSession,
    setTrackingEnabled
  ]);

  return (
    <div className="mobile-shell flex min-h-screen items-center justify-center bg-gradient-to-br from-teal-800 to-slate-950 p-6 text-white">
      <div className="text-center">
        <div className="mx-auto mb-5 flex h-20 w-20 items-center justify-center rounded-[2rem] bg-white/10 text-3xl font-black backdrop-blur">
          VK
        </div>
        <h1 className="text-2xl font-bold">Vĩnh Khánh Narration</h1>
        <p className="mt-3 text-sm text-teal-50/80">
          Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh
        </p>
        <p className="mt-8 text-sm text-teal-50/70">{message}</p>
      </div>
    </div>
  );
}
