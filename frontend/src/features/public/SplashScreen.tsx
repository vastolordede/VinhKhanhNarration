import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createGuestSession } from '../../api/publicApi';
import { useAppContext } from '../../contexts/AppContext';

export default function SplashScreen() {
  const navigate = useNavigate();
  const { guestSession, setGuestSession, language } = useAppContext();
  const [message, setMessage] = useState('Đang khởi tạo phiên khách...');

  useEffect(() => {
    async function init() {
      try {
        let session = guestSession;
        if (!session?.guestSessionId) {
          session = await createGuestSession(navigator.userAgent);
          setGuestSession(session);
        }
        setMessage('Đã sẵn sàng');
        setTimeout(() => navigate(language ? '/app/map' : '/app/language', { replace: true }), 400);
      } catch (error) {
        setMessage('Không thể khởi tạo phiên khách. Hãy kiểm tra backend API.');
      }
    }
    init();
  }, [guestSession, language, navigate, setGuestSession]);

  return (
    <div className="mobile-shell flex min-h-screen items-center justify-center bg-gradient-to-br from-teal-800 to-slate-950 p-6 text-white">
      <div className="text-center">
        <div className="mx-auto mb-5 flex h-20 w-20 items-center justify-center rounded-[2rem] bg-white/10 text-3xl font-black backdrop-blur">VK</div>
        <h1 className="text-2xl font-bold">Vĩnh Khánh Narration</h1>
        <p className="mt-3 text-sm text-teal-50/80">Thuyết minh tự động đa ngôn ngữ cho phố ẩm thực Vĩnh Khánh</p>
        <p className="mt-8 text-sm text-teal-50/70">{message}</p>
      </div>
    </div>
  );
}
