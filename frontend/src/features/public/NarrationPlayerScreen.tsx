import { ChangeEvent, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Pause, Play, Square } from 'lucide-react';
import {
  createListeningHistory,
  updateListeningDuration,
  updateListeningStatus
} from '../../api/publicApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import FeedbackModal from './FeedbackModal';
import { ListeningHistoryDTO } from '../../types';
import { useI18n } from '../../i18n/useI18n';

function formatTime(value: number): string {
  if (!Number.isFinite(value) || value < 0) return '00:00';

  const totalSeconds = Math.floor(value);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
}

export default function NarrationPlayerScreen() {
  const { currentNarration, language, guestSession } = useAppContext();
  const navigate = useNavigate();
  const { t } = useI18n();

  const audioRef = useRef<HTMLAudioElement | null>(null);
  const historyIdRef = useRef<number | null>(null);
  const creatingHistoryRef = useRef(false);

  const [isPlaying, setIsPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [audioLoadFailed, setAudioLoadFailed] = useState(false);
  const [feedbackOpen, setFeedbackOpen] = useState(false);

  const audioAvailable = Boolean(
    currentNarration?.audioUrl?.trim() && !audioLoadFailed
  );

  const sourceLabel = useMemo(() => {
    if (!currentNarration) return '';
    return t(`public.player.source.${currentNarration.source}`);
  }, [currentNarration, t]);

  useEffect(() => {
    const audio = audioRef.current;

    historyIdRef.current = null;
    creatingHistoryRef.current = false;
    setIsPlaying(false);
    setCurrentTime(0);
    setDuration(0);
    setAudioLoadFailed(false);

    if (audio) {
      audio.pause();
      audio.currentTime = 0;
      audio.load();
    }
  }, [currentNarration?.narrationId, currentNarration?.audioUrl]);

  async function ensureListeningHistory(): Promise<number | null> {
    if (historyIdRef.current) return historyIdRef.current;

    if (
      creatingHistoryRef.current ||
      !currentNarration ||
      !language ||
      !guestSession?.guestSessionId
    ) {
      return null;
    }

    creatingHistoryRef.current = true;

    const payload: ListeningHistoryDTO = {
      guestSessionId: guestSession.guestSessionId,
      narrationId: currentNarration.narrationId,
      languageId: language.languageId,
      audioId: currentNarration.audioId,
      geofenceEventId: currentNarration.geofenceEventId ?? null,
      triggerSource:
        currentNarration.source === 'geofence' ? 'Geofence' : 'Manual',
      playbackStatus: 'Played',
      listenDurationSeconds: 0,
      deviceInfo: navigator.userAgent
    };

    try {
      const historyId = await createListeningHistory(payload);
      historyIdRef.current = historyId;
      return historyId;
    } catch (error) {
      console.error('Create listening history failed:', error);
      return null;
    } finally {
      creatingHistoryRef.current = false;
    }
  }

  async function finishHistory(
    status: ListeningHistoryDTO['playbackStatus'],
    reset: boolean
  ): Promise<void> {
    const historyId = historyIdRef.current;
    if (!historyId) return;

    const seconds = audioRef.current?.currentTime ?? currentTime;

    try {
      await Promise.all([
        updateListeningStatus(historyId, status),
        updateListeningDuration(historyId, seconds)
      ]);
    } catch (error) {
      console.error('Update listening history failed:', error);
    } finally {
      if (reset) historyIdRef.current = null;
    }
  }

  async function play(): Promise<void> {
    if (!audioAvailable || !audioRef.current) return;

    try {
      await audioRef.current.play();
    } catch (error) {
      console.error('Audio playback failed:', error);
      setAudioLoadFailed(true);
    }
  }

  function pause(): void {
    audioRef.current?.pause();
  }

  function stop(): void {
    const audio = audioRef.current;
    if (!audio) return;

    audio.pause();
    void finishHistory('Stopped', true);
    audio.currentTime = 0;
    setCurrentTime(0);
    setIsPlaying(false);
  }

  function seek(event: ChangeEvent<HTMLInputElement>): void {
    const audio = audioRef.current;
    if (!audio) return;

    const nextTime = Number(event.target.value);
    audio.currentTime = nextTime;
    setCurrentTime(nextTime);
  }

  function handlePlay(): void {
    setIsPlaying(true);
    void ensureListeningHistory();
  }

  function handlePause(): void {
    setIsPlaying(false);
  }

  function handleEnded(): void {
    setIsPlaying(false);
    setCurrentTime(duration);
    void finishHistory('Completed', true);
  }

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
            {audioAvailable
              ? t('public.player.audioFile')
              : t('public.player.audioUnavailable')}
          </p>
          <h1 className="mt-2 text-2xl font-bold text-slate-900">
            {currentNarration.title}
          </h1>
        </div>

        <audio
          ref={audioRef}
          src={currentNarration.audioUrl}
          preload="metadata"
          onLoadedMetadata={(event) => {
            setDuration(event.currentTarget.duration || 0);
            setAudioLoadFailed(false);
          }}
          onTimeUpdate={(event) => {
            setCurrentTime(event.currentTarget.currentTime);
          }}
          onPlay={handlePlay}
          onPause={handlePause}
          onEnded={handleEnded}
          onError={() => {
            setAudioLoadFailed(true);
            setIsPlaying(false);
          }}
        />

        {!audioAvailable && (
          <p className="rounded-2xl bg-amber-50 px-4 py-3 text-sm font-semibold text-amber-700">
            {audioLoadFailed
              ? t('public.player.audioLoadError')
              : t('public.player.audioUnavailableMessage')}
          </p>
        )}

        <div className="space-y-2">
          <input
            type="range"
            min={0}
            max={duration > 0 ? duration : 0}
            step={0.1}
            value={Math.min(currentTime, duration || 0)}
            onChange={seek}
            disabled={!audioAvailable || duration <= 0}
            aria-label={t('public.player.seek')}
            className="w-full accent-teal-700 disabled:opacity-50"
          />
          <div className="flex justify-between text-xs font-semibold text-slate-500">
            <span>{formatTime(currentTime)}</span>
            <span>{formatTime(duration)}</span>
          </div>
        </div>

        <div className="grid grid-cols-3 gap-2">
          <Button
            onClick={() => void play()}
            disabled={!audioAvailable || isPlaying}
          >
            <Play size={18} />
            <span className="ml-1">{t('public.player.play')}</span>
          </Button>

          <Button
            variant="secondary"
            onClick={pause}
            disabled={!audioAvailable || !isPlaying}
          >
            <Pause size={18} />
            <span className="ml-1">{t('public.player.pause')}</span>
          </Button>

          <Button
            variant="secondary"
            onClick={stop}
            disabled={!audioAvailable}
          >
            <Square size={18} />
            <span className="ml-1">{t('public.player.stop')}</span>
          </Button>
        </div>

        <div className="rounded-3xl bg-slate-50 p-4">
          <p className="whitespace-pre-line text-sm leading-7 text-slate-700">
            {currentNarration.text}
          </p>
        </div>

        <Button
          variant="secondary"
          onClick={() => setFeedbackOpen(true)}
          className="w-full"
        >
          {t('public.player.sendFeedback')}
        </Button>
      </Card>

      <FeedbackModal
        open={feedbackOpen}
        onClose={() => setFeedbackOpen(false)}
        narrationId={currentNarration.narrationId}
      />
    </div>
  );
}
