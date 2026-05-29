import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getActiveLanguages, updateGuestLanguage } from '../../api/publicApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { useI18n } from '../../i18n/useI18n';
import { LanguageDTO } from '../../types';

export default function LanguageSelectionScreen() {
  const [languages, setLanguages] = useState<LanguageDTO[]>([]);
  const [loading, setLoading] = useState(true);

  const { guestSession, setLanguage } = useAppContext();
  const navigate = useNavigate();
  const { t } = useI18n();

  useEffect(() => {
    getActiveLanguages()
      .then(setLanguages)
      .finally(() => setLoading(false));
  }, []);

  async function chooseLanguage(language: LanguageDTO) {
    if (guestSession?.guestSessionId) {
      await updateGuestLanguage(guestSession.guestSessionId, language.languageId);
    }

    // This changes narration/listening language only.
    // It must not change UI language.
    setLanguage(language);

    navigate('/app/map');
  }

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-10">
      <h1 className="text-2xl font-bold text-slate-900">
        {t('public.languageSelection.title')}
      </h1>

      <p className="mt-2 text-sm text-slate-500">
        {t('public.languageSelection.description')}
      </p>

      <div className="mt-6 space-y-3">
        {loading && (
          <Card>
            {t('public.languageSelection.loading')}
          </Card>
        )}

        {!loading && languages.length === 0 && (
          <Card>
            {t('public.languageSelection.empty')}
          </Card>
        )}

        {languages.map((language) => (
          <Card
            key={language.languageId}
            className="flex items-center justify-between gap-4"
          >
            <div>
              <p className="font-semibold text-slate-900">
                {language.languageName}
              </p>

              <p className="text-xs uppercase text-slate-400">
                {language.languageCode}
              </p>
            </div>

            <Button onClick={() => chooseLanguage(language)}>
              {t('public.languageSelection.choose')}
            </Button>
          </Card>
        ))}
      </div>
    </div>
  );
}