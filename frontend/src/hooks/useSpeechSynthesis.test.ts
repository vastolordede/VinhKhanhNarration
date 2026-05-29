import { useCallback, useEffect, useMemo, useRef, useState } from 'react';

type SpeechConfig = {
  lang: string;
  rate: number;
  pitch: number;
};

function getSpeechConfig(languageCode?: string): SpeechConfig {
  const lower = languageCode?.toLowerCase() ?? 'vi';

  if (lower.startsWith('en')) {
    return {
      lang: 'en-US',
      rate: 0.95,
      pitch: 1
    };
  }

  if (lower.startsWith('ja')) {
    return {
      lang: 'ja-JP',
      rate: 0.9,
      pitch: 1
    };
  }

  if (lower.startsWith('ko')) {
    return {
      lang: 'ko-KR',
      rate: 0.9,
      pitch: 1
    };
  }

  if (lower.startsWith('zh')) {
    return {
      lang: 'zh-CN',
      rate: 0.9,
      pitch: 1
    };
  }

  return {
    lang: 'vi-VN',
    rate: 0.9,
    pitch: 1
  };
}

function getLanguageBase(lang: string) {
  return lang.split('-')[0].toLowerCase();
}

function scoreVoice(voice: SpeechSynthesisVoice, targetLang: string) {
  const voiceLang = voice.lang.toLowerCase();
  const voiceName = voice.name.toLowerCase();
  const target = targetLang.toLowerCase();
  const base = getLanguageBase(targetLang);

  let score = 0;

  if (voiceLang === target) score += 1000;
  else if (voiceLang.startsWith(base)) score += 700;

  if (base === 'vi' && (voiceName.includes('vietnam') || voiceName.includes('việt') || voiceName.includes('viet'))) {
    score += 100;
  }

  if (base === 'en' && voiceName.includes('english')) {
    score += 100;
  }

  if (base === 'ja' && (voiceName.includes('japanese') || voiceName.includes('日本'))) {
    score += 100;
  }

  if (base === 'ko' && (voiceName.includes('korean') || voiceName.includes('한국'))) {
    score += 100;
  }

  if (base === 'zh' && (voiceName.includes('chinese') || voiceName.includes('mandarin') || voiceName.includes('中文'))) {
    score += 100;
  }

  if (voice.localService) score += 10;
  if (voice.default) score += 5;

  return score;
}

function findBestVoice(voices: SpeechSynthesisVoice[], targetLang: string) {
  if (voices.length === 0) return null;

  const ranked = voices
    .map((voice) => ({
      voice,
      score: scoreVoice(voice, targetLang)
    }))
    .filter((item) => item.score > 0)
    .sort((a, b) => b.score - a.score);

  return ranked[0]?.voice ?? null;
}

function splitTextIntoChunks(text: string, maxLength = 220) {
  const normalized = text.replace(/\s+/g, ' ').trim();

  if (!normalized) return [];

  const sentences = normalized.match(/[^.!?。！？]+[.!?。！？]?/g) ?? [normalized];
  const chunks: string[] = [];
  let current = '';

  for (const sentence of sentences) {
    const cleanSentence = sentence.trim();

    if (!cleanSentence) continue;

    if ((current + ' ' + cleanSentence).trim().length <= maxLength) {
      current = (current + ' ' + cleanSentence).trim();
      continue;
    }

    if (current) {
      chunks.push(current);
      current = '';
    }

    if (cleanSentence.length <= maxLength) {
      current = cleanSentence;
      continue;
    }

    for (let i = 0; i < cleanSentence.length; i += maxLength) {
      chunks.push(cleanSentence.slice(i, i + maxLength).trim());
    }
  }

  if (current) {
    chunks.push(current);
  }

  return chunks;
}

export function useSpeechSynthesis(languageCode?: string) {
  const [speaking, setSpeaking] = useState(false);
  const [paused, setPaused] = useState(false);
  const [voices, setVoices] = useState<SpeechSynthesisVoice[]>([]);

  const queueRef = useRef<string[]>([]);
  const speechTokenRef = useRef(0);

  const supported =
    typeof window !== 'undefined' &&
    'speechSynthesis' in window &&
    'SpeechSynthesisUtterance' in window;

  const speechConfig = useMemo(
    () => getSpeechConfig(languageCode),
    [languageCode]
  );

  const selectedVoice = useMemo(
    () => findBestVoice(voices, speechConfig.lang),
    [voices, speechConfig.lang]
  );

  useEffect(() => {
    if (!supported) return;

    const synth = window.speechSynthesis;

    function loadVoices() {
      setVoices(synth.getVoices());
    }

    loadVoices();

    synth.addEventListener?.('voiceschanged', loadVoices);
    synth.onvoiceschanged = loadVoices;

    return () => {
      synth.removeEventListener?.('voiceschanged', loadVoices);

      if (synth.onvoiceschanged === loadVoices) {
        synth.onvoiceschanged = null;
      }
    };
  }, [supported]);

  const stop = useCallback(() => {
    if (!supported) return;

    speechTokenRef.current += 1;
    queueRef.current = [];

    window.speechSynthesis.cancel();

    setSpeaking(false);
    setPaused(false);
  }, [supported]);

  const speak = useCallback(
    (text: string) => {
      if (!supported || !text.trim()) return;

      window.speechSynthesis.cancel();

      const chunks = splitTextIntoChunks(text);
      const token = speechTokenRef.current + 1;

      speechTokenRef.current = token;
      queueRef.current = [...chunks];

      function speakNextChunk() {
        if (speechTokenRef.current !== token) return;

        const nextText = queueRef.current.shift();

        if (!nextText) {
          setSpeaking(false);
          setPaused(false);
          return;
        }

        const utterance = new SpeechSynthesisUtterance(nextText);

        utterance.lang = speechConfig.lang;
        utterance.rate = speechConfig.rate;
        utterance.pitch = speechConfig.pitch;

        if (selectedVoice) {
          utterance.voice = selectedVoice;
        }

        utterance.onstart = () => {
          setSpeaking(true);
          setPaused(false);
        };

        utterance.onend = () => {
          speakNextChunk();
        };

        utterance.onerror = () => {
          setSpeaking(false);
          setPaused(false);
        };

        window.speechSynthesis.speak(utterance);
      }

      speakNextChunk();
    },
    [supported, speechConfig, selectedVoice]
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

  return {
    supported,
    speaking,
    paused,
    lang: speechConfig.lang,
    voice: selectedVoice,
    voices,
    speak,
    pause,
    resume,
    stop
  };
}