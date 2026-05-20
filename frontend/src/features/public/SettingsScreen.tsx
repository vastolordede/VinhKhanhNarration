import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';

export default function SettingsScreen() {
  const { guestSession, language, trackingEnabled, setTrackingEnabled, setLanguage } = useAppContext();
  const navigate = useNavigate();
  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-8">
      <h1 className="text-2xl font-bold text-slate-900">Cài đặt</h1>
      <div className="mt-5 space-y-4">
        <Card>
          <p className="text-sm text-slate-500">Guest Session</p>
          <p className="mt-1 break-all font-semibold text-slate-900">{guestSession?.guestSessionId || 'Chưa có'}</p>
        </Card>
        <Card>
          <p className="text-sm text-slate-500">Ngôn ngữ hiện tại</p>
          <p className="mt-1 font-semibold text-slate-900">{language?.languageName || 'Chưa chọn'}</p>
          <Button className="mt-4 w-full" variant="secondary" onClick={() => navigate('/app/language')}>Đổi ngôn ngữ</Button>
        </Card>
        <Card>
          <p className="font-semibold text-slate-900">Theo dõi vị trí</p>
          <p className="mt-1 text-sm text-slate-500">Có thể bật/tắt trực tiếp ở màn hình bản đồ.</p>
          <Button className="mt-4 w-full" variant={trackingEnabled ? 'danger' : 'primary'} onClick={() => setTrackingEnabled(!trackingEnabled)}>
            {trackingEnabled ? 'Tắt theo dõi' : 'Bật theo dõi'}
          </Button>
        </Card>
      </div>
    </div>
  );
}
