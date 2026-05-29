import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Pause, Play, Square } from 'lucide-react';
import { submitListeningHistory } from '../../api/publicApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { useSpeechSynthesis } from '../../hooks/useSpeechSynthesis';
import FeedbackModal from './FeedbackModal';
import { ListeningHistoryDTO } from '../../types';
import { useI18n } from '../../i18n/useI18n';

export default function NarrationPlayerScreen() {
  const { currentNarration, language, guestSession } = useAppContext();
  const navigate = useNavigate();
  const { t } = useI18n();
  const tts = useSpeechSynthesis(language?.languageCode);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const [playingAudio, setPlayingAudio] = useState(false);
  const [feedbackOpen, setFeedbackOpen] = useState(false);

  const sourceLabel = useMemo(() => {
  if (!currentNarration) return '';

  if (currentNarration.source === 'geofence') {
    return t('public.player.source.geofence');
  }

  if (currentNarration.source === 'qr') {
    return t('public.player.source.qr');
  }

  if (currentNarration.source === 'place') {
    return t('public.player.source.place');
  }

  if (currentNarration.source === 'dish') {
    return t('public.player.source.dish');
  }

  if (currentNarration.source === 'narration') {
    return t('public.player.source.narration');
  }

  return t('public.player.source.manual');
}, [currentNarration, t]);

 useEffect(() => {
  if (!currentNarration || !language || !guestSession?.guestSessionId) {
    return;
  }

  const payload: ListeningHistoryDTO = {
    guestSessionId: guestSession.guestSessionId,
    narrationId: currentNarration.narrationId,
    languageId: language.languageId,
    triggerSource:
      currentNarration.source === 'geofence'
        ? 'Geofence'
        : currentNarration.source === 'qr'
          ? 'QR'
          : 'Manual',
    playbackStatus: 'Played',
    deviceInfo: navigator.userAgent
  };

  if (currentNarration.audioId) {
    payload.audioId = currentNarration.audioId;
  }

  submitListeningHistory(payload).catch((error) => {
    console.error('Submit listening history failed:', error);
  });
}, [currentNarration, language, guestSession]);

  if (!currentNarration) {
    return (
      <div className="min-h-screen p-5 pt-10">
        <Card>
          <p className="font-semibold text-slate-900">
  {t('public.player.noNarration')}
</p>

<Button className="mt-4" onClick={() => navigate('/app/map')}>
  {t('public.player.backToMap')}
</Button>
        </Card>
      </div>
    );
  }

  function play() {
  if (!currentNarration) return;

  if (currentNarration.audioUrl) {
    audioRef.current?.play();
    setPlayingAudio(true);
  } else {
    tts.speak(currentNarration.text);
  }
}

function pause() {
  if (!currentNarration) return;

  if (currentNarration.audioUrl) {
    audioRef.current?.pause();
    setPlayingAudio(false);
  } else {
    tts.pause();
  }
}

function stop() {
  if (!currentNarration) return;

  if (currentNarration.audioUrl) {
    audioRef.current?.pause();

    if (audioRef.current) {
      audioRef.current.currentTime = 0;
    }

    setPlayingAudio(false);
  } else {
    tts.stop();
  }
}

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-6">
<button
  onClick={() => navigate(-1)}
  className="mb-4 flex items-center gap-2 text-sm font-semibold text-slate-600"
>
  <ArrowLeft size={18} />
  {t('public.player.back')}
</button>
      <Card className="space-y-5">
        <div>
<p className="text-xs font-bold uppercase tracking-wide text-teal-700">
  {sourceLabel} •{' '}
  {currentNarration.audioUrl
    ? t('public.player.audioFile')
    : t('public.player.browserTts')}
</p>
          <h1 className="mt-2 text-2xl font-bold text-slate-900">{currentNarration.title}</h1>
        </div>

        {currentNarration.audioUrl && (
          <audio ref={audioRef} src={currentNarration.audioUrl} controls className="w-full" onEnded={() => setPlayingAudio(false)} />
        )}

        <div className="grid grid-cols-3 gap-2">
          <Button onClick={play}>
  <Play size={18} />
  <span className="ml-1">{t('public.player.play')}</span>
</Button>

<Button variant="secondary" onClick={pause}>
  <Pause size={18} />
  <span className="ml-1">{t('public.player.pause')}</span>
</Button>

<Button variant="secondary" onClick={stop}>
  <Square size={18} />
  <span className="ml-1">{t('public.player.stop')}</span>
</Button>
        </div>

        <div className="rounded-3xl bg-slate-50 p-4">
          <p className="whitespace-pre-line text-sm leading-7 text-slate-700">{currentNarration.text}</p>
        </div>

       <Button
  variant="secondary"
  onClick={() => setFeedbackOpen(true)}
  className="w-full"
>
  {t('public.player.sendFeedback')}
</Button>
      </Card>
      <FeedbackModal open={feedbackOpen} onClose={() => setFeedbackOpen(false)} placeId={currentNarration.placeId} dishId={currentNarration.dishId} narrationId={currentNarration.narrationId} />
    </div>
  );
}
