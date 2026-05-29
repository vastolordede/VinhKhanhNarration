import { createContext, ReactNode, useContext, useMemo, useState } from 'react';
import { GuestSessionDTO, LanguageDTO, NarrationResolveResultDTO } from '../types';
import { normalizeUiLanguage, UiLanguage } from '../i18n/translations';

type AppContextValue = {
  guestSession: GuestSessionDTO | null;
  setGuestSession: (session: GuestSessionDTO | null) => void;

  language: LanguageDTO | null;
  setLanguage: (language: LanguageDTO | null) => void;

  uiLanguage: UiLanguage;
  setUiLanguage: (language: UiLanguage) => void;

  currentNarration: NarrationResolveResultDTO | null;
  setCurrentNarration: (narration: NarrationResolveResultDTO | null) => void;

  trackingEnabled: boolean;
  setTrackingEnabled: (enabled: boolean) => void;
};

const AppContext = createContext<AppContextValue | null>(null);

function readJson<T>(key: string): T | null {
  const raw = localStorage.getItem(key);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as T;
  } catch {
    localStorage.removeItem(key);
    return null;
  }
}

export function AppProvider({ children }: { children: ReactNode }) {
  const [guestSession, setGuestSessionState] = useState<GuestSessionDTO | null>(() =>
    readJson<GuestSessionDTO>('guestSession')
  );

  const [language, setLanguageState] = useState<LanguageDTO | null>(() =>
    readJson<LanguageDTO>('language')
  );

const [uiLanguage, setUiLanguageState] = useState<UiLanguage>(() => {
  const savedLanguage = readJson<LanguageDTO>('language');

  if (savedLanguage) {
    return normalizeUiLanguage(savedLanguage.languageCode);
  }

  const savedUiLanguage = localStorage.getItem('uiLanguage');
  return normalizeUiLanguage(savedUiLanguage);
});

  const [currentNarration, setCurrentNarration] = useState<NarrationResolveResultDTO | null>(() => {
    const raw = sessionStorage.getItem('currentNarration');
    if (!raw) return null;

    try {
      return JSON.parse(raw) as NarrationResolveResultDTO;
    } catch {
      sessionStorage.removeItem('currentNarration');
      return null;
    }
  });

  const [trackingEnabled, setTrackingEnabled] = useState(false);

  const setGuestSession = (session: GuestSessionDTO | null) => {
    setGuestSessionState(session);

    if (session) localStorage.setItem('guestSession', JSON.stringify(session));
    else localStorage.removeItem('guestSession');
  };

  const setUiLanguage = (lang: UiLanguage) => {
    setUiLanguageState(lang);
    localStorage.setItem('uiLanguage', lang);
  };

const setLanguage = (lang: LanguageDTO | null) => {
  setLanguageState(lang);

  if (lang) {
    localStorage.setItem('language', JSON.stringify(lang));

    // Public app rule:
    // selected narration language also controls public UI language.
    const nextUiLanguage = normalizeUiLanguage(lang.languageCode);

    setUiLanguageState(nextUiLanguage);
    localStorage.setItem('uiLanguage', nextUiLanguage);
  } else {
    localStorage.removeItem('language');
  }
};

  const setNarration = (narration: NarrationResolveResultDTO | null) => {
    setCurrentNarration(narration);

    if (narration) sessionStorage.setItem('currentNarration', JSON.stringify(narration));
    else sessionStorage.removeItem('currentNarration');
  };

  const value = useMemo(
    () => ({
      guestSession,
      setGuestSession,
      language,
      setLanguage,
      uiLanguage,
      setUiLanguage,
      currentNarration,
      setCurrentNarration: setNarration,
      trackingEnabled,
      setTrackingEnabled
    }),
    [guestSession, language, uiLanguage, currentNarration, trackingEnabled]
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useAppContext() {
  const context = useContext(AppContext);
  if (!context) throw new Error('useAppContext must be used inside AppProvider');
  return context;
}