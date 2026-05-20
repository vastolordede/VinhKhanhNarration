import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { resolveQr } from '../../api/publicApi';
import { useAppContext } from '../../contexts/AppContext';

export default function QRScannerScreen() {
  const [manualValue, setManualValue] = useState('');
  const [error, setError] = useState<string | null>(null);
  const readerId = 'qr-reader';
  const scannerRef = useRef<any>(null);
  const { language, guestSession, setCurrentNarration } = useAppContext();
  const navigate = useNavigate();

  async function resolve(value: string) {
    if (!language || !guestSession) {
      setError('Cần chọn ngôn ngữ và tạo GuestSession trước.');
      return;
    }
    try {
      const result = await resolveQr(value, language.languageId, guestSession.guestSessionId);
      setCurrentNarration(result);
      navigate('/app/listen');
    } catch (e) {
      setError('Không đọc được QR hoặc backend chưa có dữ liệu QR tương ứng.');
    }
  }

  useEffect(() => {
    let mounted = true;
    async function startScanner() {
      try {
        const { Html5QrcodeScanner } = await import('html5-qrcode');
        if (!mounted) return;
        scannerRef.current = new Html5QrcodeScanner(readerId, { fps: 10, qrbox: { width: 250, height: 250 } }, false);
        scannerRef.current.render(
          (decodedText: string) => resolve(decodedText),
          () => undefined
        );
      } catch {
        setError('Không thể mở camera QR. Có thể dùng ô nhập mã QR bên dưới.');
      }
    }
    startScanner();
    return () => {
      mounted = false;
      scannerRef.current?.clear?.().catch?.(() => undefined);
    };
  }, []);

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-8">
      <h1 className="text-2xl font-bold text-slate-900">Quét QR</h1>
      <p className="mt-2 text-sm text-slate-500">Quét QR tại quán, món ăn hoặc điểm thuyết minh.</p>
      <Card className="mt-5">
        <div id={readerId} className="overflow-hidden rounded-2xl" />
        {error && <p className="mt-3 text-sm text-rose-600">{error}</p>}
      </Card>
      <Card className="mt-4 space-y-3">
        <p className="text-sm font-semibold">Nhập QR thủ công</p>
        <Input value={manualValue} onChange={(e) => setManualValue(e.target.value)} placeholder="QR value..." />
        <Button className="w-full" onClick={() => resolve(manualValue)} disabled={!manualValue.trim()}>Resolve QR</Button>
      </Card>
    </div>
  );
}
