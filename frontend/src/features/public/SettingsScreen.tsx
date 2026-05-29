import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { useI18n } from '../../i18n/useI18n';

export default function SettingsScreen() {
  const {
  guestSession,
  language,
  trackingEnabled,
  setTrackingEnabled
} = useAppContext();

  const navigate = useNavigate();
  const { t } = useI18n();

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-8">
      <h1 className="text-2xl font-bold text-slate-900">
        {t('public.settings.title')}
      </h1>

      <div className="mt-5 space-y-4">
        <Card>
          <p className="text-sm text-slate-500">
            {t('public.settings.guestSession')}
          </p>

          <p className="mt-1 break-all font-semibold text-slate-900">
            {guestSession?.guestSessionId || t('public.settings.noSession')}
          </p>
        </Card>

        

        <Card>
          <p className="text-sm text-slate-500">
            {t('public.settings.narrationLanguage')}
          </p>

          <p className="mt-1 font-semibold text-slate-900">
            {language?.languageName || t('public.settings.noNarrationLanguage')}
          </p>

          <p className="mt-2 text-sm text-slate-500">
            {t('public.settings.narrationLanguageDescription')}
          </p>

          <Button
            className="mt-4 w-full"
            variant="secondary"
            onClick={() => navigate('/app/language')}
          >
            {t('public.settings.changeNarrationLanguage')}
          </Button>
        </Card>

        <Card>
          <p className="font-semibold text-slate-900">
            {t('public.settings.tracking')}
          </p>

          <p className="mt-1 text-sm text-slate-500">
            {t('public.settings.trackingDescription')}
          </p>

          <Button
            className="mt-4 w-full"
            variant={trackingEnabled ? 'danger' : 'primary'}
            onClick={() => setTrackingEnabled(!trackingEnabled)}
          >
            {trackingEnabled
              ? t('public.settings.disableTracking')
              : t('public.settings.enableTracking')}
          </Button>
        </Card>
      </div>
    </div>
  );
}