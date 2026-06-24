import { useNavigate } from 'react-router-dom';
import { LanguageSwitcher } from '../../components/common/LanguageSwitcher';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { useI18n } from '../../i18n/useI18n';
import { hasUsableAccessPass } from '../../utils/accessPolicy';

export default function SettingsScreen() {
  const {
    guestSession,
    language,
    accessStatus,
    trackingEnabled,
    setTrackingEnabled
  } = useAppContext();

  const navigate = useNavigate();
  const { t } = useI18n();
  const hasAccessPass = hasUsableAccessPass(accessStatus);

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
          <p className="font-semibold text-slate-900">Access Pass 24 giờ</p>
          <p className={`mt-1 text-sm font-semibold ${
            hasAccessPass ? 'text-emerald-700' : 'text-amber-700'
          }`}>
            {hasAccessPass
              ? `Đang hoạt động • còn ${(accessStatus?.hoursRemaining ?? 0)} giờ`
              : 'Chưa có pass đang hoạt động'}
          </p>
          <Button
            className="mt-4 w-full"
            variant="secondary"
            onClick={() => navigate('/app/access')}
          >
            Quản lý Access Pass
          </Button>
        </Card>

        <Card>
          <p className="font-semibold text-slate-900">
            {t('public.settings.uiLanguage')}
          </p>

          <p className="mt-1 text-sm text-slate-500">
            {t('public.settings.uiLanguageDescription')}
          </p>

          <div className="mt-4">
            <LanguageSwitcher />
          </div>
        </Card>

        <Card>
          <p className="text-sm text-slate-500">
            {t('public.settings.narrationLanguage')}
          </p>

          <p className="mt-1 font-semibold text-slate-900">
            {language?.nativeName ||
              language?.languageName ||
              t('public.settings.noNarrationLanguage')}
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
            onClick={() => {
              if (!trackingEnabled && !hasAccessPass) {
                navigate('/app/access');
                return;
              }
              setTrackingEnabled(!trackingEnabled);
            }}
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
