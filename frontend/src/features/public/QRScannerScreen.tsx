import { ChangeEvent, useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Camera, ImagePlus, QrCode, Square } from 'lucide-react';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { resolveQr } from '../../api/publicApi';
import { useAppContext } from '../../contexts/AppContext';
import { useI18n } from '../../i18n/useI18n';

type QrErrorKey =
  | 'public.qr.needNarrationLanguage'
  | 'public.qr.resolveError'
  | 'public.qr.cameraInitError'
  | 'public.qr.cameraOpenError'
  | 'public.qr.imageScanError';

export default function QRScannerScreen() {
  const [manualValue, setManualValue] = useState('');
  const [errorKey, setErrorKey] = useState<QrErrorKey | null>(null);
  const [scannerReady, setScannerReady] = useState(false);
  const [scanning, setScanning] = useState(false);

  const readerId = 'qr-reader';
  const qrRef = useRef<any>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const resolvingRef = useRef(false);
  const scanningRef = useRef(false);

  const { language, guestSession, setCurrentNarration } = useAppContext();
  const navigate = useNavigate();
  const { t } = useI18n();

  async function safeStopScanner() {
    const qr = qrRef.current;

    if (!qr || !scanningRef.current) {
      return;
    }

    try {
      await qr.stop();
    } catch {
      // Ignore because html5-qrcode throws if scanner is already stopped or paused.
    } finally {
      scanningRef.current = false;
      setScanning(false);
    }
  }

  async function resolve(value: string) {
    const qrValue = value.trim();

    if (!qrValue) return;

    if (resolvingRef.current) return;
    resolvingRef.current = true;

    if (!language || !guestSession) {
      setErrorKey('public.qr.needNarrationLanguage');
      resolvingRef.current = false;
      return;
    }

    try {
      setErrorKey(null);

      await safeStopScanner();

      const result = await resolveQr(
        qrValue,
        language.languageId,
        guestSession.guestSessionId
      );

      setCurrentNarration(result);
      navigate('/app/listen');
    } catch {
      setErrorKey('public.qr.resolveError');
    } finally {
      resolvingRef.current = false;
    }
  }

  useEffect(() => {
    let mounted = true;

    async function initScanner() {
      try {
        const { Html5Qrcode } = await import('html5-qrcode');

        if (!mounted) return;

        qrRef.current = new Html5Qrcode(readerId);
        setScannerReady(true);
      } catch {
        if (mounted) {
          setErrorKey('public.qr.cameraInitError');
        }
      }
    }

    initScanner();

    return () => {
      mounted = false;

      const qr = qrRef.current;

      if (!qr) return;

      if (scanningRef.current) {
        qr.stop()
          .catch(() => undefined)
          .finally(() => {
            scanningRef.current = false;
            try {
              qr.clear();
            } catch {
              // Ignore clear errors.
            }
          });
      } else {
        try {
          qr.clear();
        } catch {
          // Ignore clear errors.
        }
      }
    };
  }, []);

  async function startCamera() {
    if (!qrRef.current || scanningRef.current) return;

    try {
      setErrorKey(null);

      await qrRef.current.start(
        { facingMode: 'environment' },
        {
          fps: 10,
          qrbox: { width: 250, height: 250 }
        },
        (decodedText: string) => {
          resolve(decodedText);
        },
        () => undefined
      );

      scanningRef.current = true;
      setScanning(true);
    } catch {
      scanningRef.current = false;
      setScanning(false);
      setErrorKey('public.qr.cameraOpenError');
    }
  }

  async function stopCamera() {
    await safeStopScanner();
  }

  async function scanImageFile(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (!file || !qrRef.current) return;

    try {
      setErrorKey(null);

      await safeStopScanner();

      const decodedText = await qrRef.current.scanFile(file, true);
      await resolve(decodedText);
    } catch {
      setErrorKey('public.qr.imageScanError');
    } finally {
      event.target.value = '';
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-8">
      <h1 className="text-2xl font-bold text-slate-900">
        {t('public.qr.title')}
      </h1>

      <p className="mt-2 text-sm text-slate-500">
        {t('public.qr.description')}
      </p>

      <Card className="mt-5">
        <div className="rounded-2xl border border-slate-200 bg-white p-4">
          <div
            id={readerId}
            className="overflow-hidden rounded-2xl bg-slate-50"
          />

          {!scanning && (
            <div className="flex min-h-[180px] flex-col items-center justify-center gap-3 text-center text-slate-500">
              <QrCode size={42} />

              <p className="text-sm font-semibold text-slate-700">
                {t('public.qr.scannerIdle')}
              </p>

              <p className="max-w-xs text-xs">
                {t('public.qr.scannerHint')}
              </p>
            </div>
          )}
        </div>

        <div className="mt-4 grid grid-cols-1 gap-2 sm:grid-cols-3">
          <Button
            type="button"
            onClick={startCamera}
            disabled={!scannerReady || scanning}
          >
            <Camera size={16} className="mr-2" />
            {t('public.qr.startCamera')}
          </Button>

          <Button
            type="button"
            variant="secondary"
            onClick={stopCamera}
            disabled={!scanning}
          >
            <Square size={16} className="mr-2" />
            {t('public.qr.stopCamera')}
          </Button>

          <Button
            type="button"
            variant="secondary"
            onClick={() => fileInputRef.current?.click()}
            disabled={!scannerReady}
          >
            <ImagePlus size={16} className="mr-2" />
            {t('public.qr.scanImage')}
          </Button>
        </div>

        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={scanImageFile}
        />

        {errorKey && (
          <p className="mt-3 text-sm text-rose-600">
            {t(errorKey)}
          </p>
        )}
      </Card>

      <Card className="mt-4 space-y-3">
        <p className="text-sm font-semibold">
          {t('public.qr.manual')}
        </p>

        <Input
          value={manualValue}
          onChange={(e) => setManualValue(e.target.value)}
          placeholder={t('public.qr.placeholder')}
        />

        <Button
          className="w-full"
          onClick={() => resolve(manualValue)}
          disabled={!manualValue.trim()}
        >
          {t('public.qr.resolve')}
        </Button>
      </Card>
    </div>
  );
}