import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Pause, Play, Square } from 'lucide-react';
import { submitListeningHistory } from '../../api/publicApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { useSpeechSynthesis } from '../../hooks/useSpeechSynthesis';
import FeedbackModal from './FeedbackModal';

export default function NarrationPlayerScreen() {
  const { currentNarration, language, guestSession } = useAppContext();
  const navigate = useNavigate();
  const tts = useSpeechSynthesis(language?.languageCode);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const [playingAudio, setPlayingAudio] = useState(false);
  const [feedbackOpen, setFeedbackOpen] = useState(false);

  const sourceLabel = useMemo(() => {
    if (!currentNarration) return '';
    if (currentNarration.source === 'geofence') return 'Tự động theo vị trí';
    if (currentNarration.source === 'qr') return 'Quét QR';
    if (currentNarration.source === 'place') return 'Địa điểm';
    if (currentNarration.source === 'dish') return 'Món ăn';
    return 'Thủ công';
  }, [currentNarration]);

  useEffect(() => {
    if (!currentNarration || !language) return;
    submitListeningHistory({
      guestSessionId: guestSession?.guestSessionId,
      narrationId: currentNarration.narrationId,
      languageId: language.languageId,
      audioId: currentNarration.audioId,
      qrCodeId: null,
      geofenceEventId: null,
      triggerSource: currentNarration.source === 'geofence' ? 'Geofence' : currentNarration.source === 'qr' ? 'QR' : 'Manual',
      playbackStatus: 'Played',
      deviceInfo: navigator.userAgent
    }).catch(() => undefined);
  }, [currentNarration, language, guestSession]);

  if (!currentNarration) {
    return (
      <div className="min-h-screen p-5 pt-10">
        <Card>
          <p className="font-semibold text-slate-900">Chưa có nội dung thuyết minh.</p>
          <Button className="mt-4" onClick={() => navigate('/app/map')}>Quay lại bản đồ</Button>
        </Card>
      </div>
    );
  }

  function play() {
    if (currentNarration?.audioUrl) {
      audioRef.current?.play();
      setPlayingAudio(true);
    } else {
      tts.speak(currentNarration.text);
    }
  }

  function pause() {
    if (currentNarration?.audioUrl) {
      audioRef.current?.pause();
      setPlayingAudio(false);
    } else {
      tts.pause();
    }
  }

  function stop() {
    if (currentNarration?.audioUrl) {
      audioRef.current?.pause();
      if (audioRef.current) audioRef.current.currentTime = 0;
      setPlayingAudio(false);
    } else {
      tts.stop();
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 p-5 pt-6">
      <button onClick={() => navigate(-1)} className="mb-4 flex items-center gap-2 text-sm font-semibold text-slate-600"><ArrowLeft size={18} /> Quay lại</button>
      <Card className="space-y-5">
        <div>
          <p className="text-xs font-bold uppercase tracking-wide text-teal-700">{sourceLabel} • {currentNarration.audioUrl ? 'Audio file' : 'TTS trình duyệt'}</p>
          <h1 className="mt-2 text-2xl font-bold text-slate-900">{currentNarration.title}</h1>
        </div>

        {currentNarration.audioUrl && (
          <audio ref={audioRef} src={currentNarration.audioUrl} controls className="w-full" onEnded={() => setPlayingAudio(false)} />
        )}

        <div className="grid grid-cols-3 gap-2">
          <Button onClick={play}><Play size={18} /> <span className="ml-1">Nghe</span></Button>
          <Button variant="secondary" onClick={pause}><Pause size={18} /> <span className="ml-1">Tạm dừng</span></Button>
          <Button variant="secondary" onClick={stop}><Square size={18} /> <span className="ml-1">Dừng</span></Button>
        </div>

        <div className="rounded-3xl bg-slate-50 p-4">
          <p className="whitespace-pre-line text-sm leading-7 text-slate-700">{currentNarration.text}</p>
        </div>

        <Button variant="secondary" onClick={() => setFeedbackOpen(true)} className="w-full">Gửi feedback</Button>
      </Card>
      <FeedbackModal open={feedbackOpen} onClose={() => setFeedbackOpen(false)} placeId={currentNarration.placeId} dishId={currentNarration.dishId} narrationId={currentNarration.narrationId} />
    </div>
  );
}
