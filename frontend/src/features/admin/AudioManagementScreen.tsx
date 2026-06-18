import { useEffect, useMemo, useState } from 'react';
import { getList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { getApiError } from '../../api/http';
import {
  generateAudio,
  getAudioFiles,
  getTranslations
} from '../../api/narrationApi';
import { PageHeader } from '../../components/layout/PageHeader';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import {
  AudioFileDTO,
  LanguageDTO,
  NarrationTranslationDTO
} from '../../types';
import { getCurrentAdminId } from './SimpleResourcePage';

export default function AudioManagementScreen() {
  const [translations, setTranslations] = useState<NarrationTranslationDTO[]>([]);
  const [audioFiles, setAudioFiles] = useState<AudioFileDTO[]>([]);
  const [languages, setLanguages] = useState<LanguageDTO[]>([]);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [translationList, audioList, languageList] = await Promise.all([
        getTranslations(),
        getAudioFiles(),
        getList<LanguageDTO>(endpoints.languages)
      ]);
      setTranslations(translationList);
      setAudioFiles(audioList);
      setLanguages(languageList);
    } catch (loadError) {
      setError(getApiError(loadError));
    }
  }

  useEffect(() => {
    void load();
  }, []);

  const languageMap = useMemo(
    () => new Map(languages.map((x) => [x.languageId, x])),
    [languages]
  );

  const latestAudioMap = useMemo(() => {
    const map = new Map<number, AudioFileDTO>();
    [...audioFiles]
      .sort((a, b) => b.audioId - a.audioId)
      .forEach((audio) => {
        if (!map.has(audio.translationId)) {
          map.set(audio.translationId, audio);
        }
      });
    return map;
  }, [audioFiles]);

  async function runGenerate(item: NarrationTranslationDTO) {
    const adminId = getCurrentAdminId();
    if (!adminId) return setError('Không tìm thấy Admin ID.');

    setBusyId(item.translationId);
    setError(null);
    setMessage(null);
    try {
      await generateAudio(item.translationId, {
        adminId,
        publishAfterGenerate: true
      });
      setMessage('Đã tạo và công khai audio. Có thể nghe thử ngay.');
      await load();
    } catch (generateError) {
      setError(getApiError(generateError));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Audio Management"
        description="Audio được tự tạo khi duyệt nội dung nguồn. Màn hình này dùng để theo dõi, nghe thử hoặc tạo lại khi lỗi."
      />

      {message && (
        <p className="mb-4 rounded-2xl bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-700">
          {message}
        </p>
      )}
      {error && (
        <p className="mb-4 rounded-2xl bg-rose-50 px-4 py-3 text-sm font-semibold text-rose-700">
          {error}
        </p>
      )}

      <div className="space-y-3">
        {translations.map((translation) => {
          const language = languageMap.get(translation.languageId);
          const audio = latestAudioMap.get(translation.translationId);
          const canGenerate =
            translation.status === 'Approved' && translation.isReviewed;

          return (
            <Card key={translation.translationId}>
              <div className="flex flex-wrap items-start justify-between gap-4">
                <div className="min-w-0 flex-1">
                  <p className="text-xs font-bold uppercase text-teal-700">
                    Translation #{translation.translationId} •{' '}
                    {language?.languageName || translation.languageId}
                  </p>
                  <h2 className="mt-1 font-bold text-slate-900">
                    {translation.translatedTitle}
                  </h2>
                  <p className="mt-2 text-sm text-slate-600">
                    Translation: {translation.status} • Audio:{' '}
                    {audio?.status || 'Chưa tạo'}
                  </p>
                  <p className="mt-1 text-xs text-slate-500">
                    Voice: {audio?.voiceName || language?.defaultVoiceId || '-'}
                    {' • '}Provider: {audio?.provider || '-'}
                  </p>
                  {audio?.errorMessage && (
                    <p className="mt-2 text-sm font-semibold text-rose-600">
                      {audio.errorMessage}
                    </p>
                  )}
                  {audio?.audioUrl && audio.status === 'Ready' && (
                    <audio
                      className="mt-3 w-full"
                      controls
                      preload="metadata"
                      src={audio.audioUrl}
                    />
                  )}
                </div>

                <Button
                  onClick={() => void runGenerate(translation)}
                  disabled={!canGenerate || busyId === translation.translationId}
                >
                  {audio?.status === 'Ready'
                    ? 'Tạo lại audio'
                    : audio?.status === 'Failed' || audio?.status === 'Outdated'
                      ? 'Thử lại'
                      : 'Tạo audio'}
                </Button>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
