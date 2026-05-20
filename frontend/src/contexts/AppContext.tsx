import { createContext, ReactNode, useContext, useMemo, useState } from 'react';
import { GuestSessionDTO, LanguageDTO, NarrationResolveResultDTO } from '../types';

type AppContextValue = {
  guestSession: GuestSessionDTO | null;
  setGuestSession: (session: GuestSessionDTO | null) => void;
  language: LanguageDTO | null;
  setLanguage: (language: LanguageDTO | null) => void;
  currentNarration: NarrationResolveResultDTO | null;
  setCurrentNarration: (narration: NarrationResolveResultDTO | null) => void;
  trackingEnabled: boolean;
  setTrackingEnabled: (enabled: boolean) => void;
};

const AppContext = createContext<AppContextValue | null>(null);

export function AppProvider({ children }: { children: ReactNode }) {
  const [guestSession, setGuestSessionState] = useState<GuestSessionDTO | null>(() => {
    const raw = localStorage.getItem('guestSession');
    return raw ? JSON.parse(raw) : null;
  });
  const [language, setLanguageState] = useState<LanguageDTO | null>(() => {
    const raw = localStorage.getItem('language');
    return raw ? JSON.parse(raw) : null;
  });
  const [currentNarration, setCurrentNarration] = useState<NarrationResolveResultDTO | null>(() => {
    const raw = sessionStorage.getItem('currentNarration');
    return raw ? JSON.parse(raw) : null;
  });
  const [trackingEnabled, setTrackingEnabled] = useState(false);

  const setGuestSession = (session: GuestSessionDTO | null) => {
    setGuestSessionState(session);
    if (session) localStorage.setItem('guestSession', JSON.stringify(session));
    else localStorage.removeItem('guestSession');
  };

  const setLanguage = (lang: LanguageDTO | null) => {
    setLanguageState(lang);
    if (lang) localStorage.setItem('language', JSON.stringify(lang));
    else localStorage.removeItem('language');
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
      currentNarration,
      setCurrentNarration: setNarration,
      trackingEnabled,
      setTrackingEnabled
    }),
    [guestSession, language, currentNarration, trackingEnabled]
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useAppContext() {
  const context = useContext(AppContext);
  if (!context) throw new Error('useAppContext must be used inside AppProvider');
  return context;
}
