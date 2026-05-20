import { useCallback, useEffect, useMemo, useState } from 'react';

export function useSpeechSynthesis(languageCode?: string) {
  const [speaking, setSpeaking] = useState(false);
  const [paused, setPaused] = useState(false);
  const supported = typeof window !== 'undefined' && 'speechSynthesis' in window;

  const lang = useMemo(() => {
    if (!languageCode) return 'vi-VN';
    const lower = languageCode.toLowerCase();
    if (lower.startsWith('en')) return 'en-US';
    if (lower.startsWith('ko')) return 'ko-KR';
    if (lower.startsWith('ja')) return 'ja-JP';
    if (lower.startsWith('zh')) return 'zh-CN';
    return 'vi-VN';
  }, [languageCode]);

  const stop = useCallback(() => {
    if (!supported) return;
    window.speechSynthesis.cancel();
    setSpeaking(false);
    setPaused(false);
  }, [supported]);

  const speak = useCallback(
    (text: string) => {
      if (!supported || !text.trim()) return;
      window.speechSynthesis.cancel();
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = lang;
      utterance.rate = 0.95;
      utterance.pitch = 1;
      utterance.onstart = () => {
        setSpeaking(true);
        setPaused(false);
      };
      utterance.onend = () => {
        setSpeaking(false);
        setPaused(false);
      };
      utterance.onerror = () => {
        setSpeaking(false);
        setPaused(false);
      };
      window.speechSynthesis.speak(utterance);
    },
    [supported, lang]
  );

  const pause = useCallback(() => {
    if (!supported) return;
    window.speechSynthesis.pause();
    setPaused(true);
  }, [supported]);

  const resume = useCallback(() => {
    if (!supported) return;
    window.speechSynthesis.resume();
    setPaused(false);
  }, [supported]);

  useEffect(() => stop, [stop]);

  return { supported, speaking, paused, speak, pause, resume, stop };
}
