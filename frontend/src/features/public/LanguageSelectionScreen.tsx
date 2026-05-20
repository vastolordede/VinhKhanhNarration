import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getActiveLanguages, updateGuestLanguage } from '../../api/publicApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { LanguageDTO } from '../../types';

export default function LanguageSelectionScreen() {
  const [languages, setLanguages] = useState<LanguageDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const { guestSession, setLanguage } = useAppContext();
  const navigate = useNavigate();

  useEffect(() => {
    getActiveLanguages().then(setLanguages).finally(() => setLoading(false));
  }, []);

  async function chooseLanguage(language: LanguageDTO) {
    if (guestSession?.guestSessionId) {
      await updateGuestLanguage(guestSession.guestSessionId, language.languageId);
    }
    setLanguage(language);
    navigate('/app/map');
  }

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-10">
      <h1 className="text-2xl font-bold text-slate-900">Chọn ngôn ngữ</h1>
      <p className="mt-2 text-sm text-slate-500">Ngôn ngữ này sẽ được dùng để đọc thuyết minh bằng TTS.</p>
      <div className="mt-6 space-y-3">
        {loading && <Card>Đang tải ngôn ngữ...</Card>}
        {languages.map((language) => (
          <Card key={language.languageId} className="flex items-center justify-between">
            <div>
              <p className="font-semibold text-slate-900">{language.languageName}</p>
              <p className="text-xs uppercase text-slate-400">{language.languageCode}</p>
            </div>
            <Button onClick={() => chooseLanguage(language)}>Chọn</Button>
          </Card>
        ))}
      </div>
    </div>
  );
}
