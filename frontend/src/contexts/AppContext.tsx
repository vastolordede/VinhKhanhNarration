import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useMemo,
  useState
} from 'react';
import {
  GuestAccessStatusDTO,
  GuestSessionDTO,
  LanguageDTO,
  NarrationResolveResultDTO
} from '../types';
import { normalizeUiLanguage, UiLanguage } from '../i18n/translations';

export type AdminUiLanguage = Extract<UiLanguage, 'vi' | 'en'>;

type AppContextValue = {
  guestSession: GuestSessionDTO | null;
  setGuestSession: (session: GuestSessionDTO | null) => void;

  language: LanguageDTO | null;
  setLanguage: (language: LanguageDTO | null) => void;

  accessStatus: GuestAccessStatusDTO | null;
  setAccessStatus: (status: GuestAccessStatusDTO | null) => void;

  uiLanguage: UiLanguage;
  setUiLanguage: (language: UiLanguage) => void;

  adminUiLanguage: AdminUiLanguage;
  setAdminUiLanguage: (language: AdminUiLanguage) => void;

  currentNarration: NarrationResolveResultDTO | null;
  setCurrentNarration: (narration: NarrationResolveResultDTO | null) => void;

  trackingEnabled: boolean;
  setTrackingEnabled: (enabled: boolean) => void;
};

const AppContext = createContext<AppContextValue | null>(null);

const GUEST_SESSION_KEY = 'guestSession';
const GUEST_ACCESS_STATUS_KEY = 'guestAccessStatus';

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

function normalizeAdminUiLanguage(value: unknown): AdminUiLanguage {
  return value === 'en' ? 'en' : 'vi';
}

export function AppProvider({ children }: { children: ReactNode }) {
  const [guestSession, setGuestSessionState] = useState<GuestSessionDTO | null>(() =>
    readJson<GuestSessionDTO>(GUEST_SESSION_KEY)
  );

  const [language, setLanguageState] = useState<LanguageDTO | null>(() =>
    readJson<LanguageDTO>('language')
  );

  // Cache trạng thái pass để mobile không mất quyền hiển thị chỉ vì một request
  // kiểm tra tạm thời thất bại. Backend vẫn kiểm tra pass ở mọi protected API.
  const [accessStatus, setAccessStatusState] = useState<GuestAccessStatusDTO | null>(() =>
    readJson<GuestAccessStatusDTO>(GUEST_ACCESS_STATUS_KEY)
  );

  const [uiLanguage, setUiLanguageState] = useState<UiLanguage>(() => {
    const savedUiLanguage = localStorage.getItem('uiLanguage');
    if (savedUiLanguage) return normalizeUiLanguage(savedUiLanguage);

    const savedContentLanguage = readJson<LanguageDTO>('language');
    return normalizeUiLanguage(savedContentLanguage?.languageCode);
  });

  const [adminUiLanguage, setAdminUiLanguageState] = useState<AdminUiLanguage>(() =>
    normalizeAdminUiLanguage(localStorage.getItem('adminUiLanguage'))
  );

  const [currentNarration, setCurrentNarrationState] =
    useState<NarrationResolveResultDTO | null>(() => {
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

  const setAccessStatus = useCallback((status: GuestAccessStatusDTO | null) => {
    setAccessStatusState(status);

    if (status) {
      localStorage.setItem(GUEST_ACCESS_STATUS_KEY, JSON.stringify(status));
    } else {
      localStorage.removeItem(GUEST_ACCESS_STATUS_KEY);
    }
  }, []);

  const setGuestSession = useCallback((session: GuestSessionDTO | null) => {
    setGuestSessionState(session);

    if (session) {
      localStorage.setItem(GUEST_SESSION_KEY, JSON.stringify(session));
      return;
    }

    localStorage.removeItem(GUEST_SESSION_KEY);
    localStorage.removeItem(GUEST_ACCESS_STATUS_KEY);
    setAccessStatusState(null);
  }, []);

  const setUiLanguage = useCallback((lang: UiLanguage) => {
    setUiLanguageState(lang);
    localStorage.setItem('uiLanguage', lang);
  }, []);

  const setLanguage = useCallback((lang: LanguageDTO | null) => {
    setLanguageState(lang);
    setCurrentNarrationState(null);
    sessionStorage.removeItem('currentNarration');

    if (lang) {
      localStorage.setItem('language', JSON.stringify(lang));
    } else {
      localStorage.removeItem('language');
    }
  }, []);

  const setAdminUiLanguage = useCallback((lang: AdminUiLanguage) => {
    const normalized = normalizeAdminUiLanguage(lang);
    setAdminUiLanguageState(normalized);
    localStorage.setItem('adminUiLanguage', normalized);
  }, []);

  const setCurrentNarration = useCallback(
    (narration: NarrationResolveResultDTO | null) => {
      setCurrentNarrationState(narration);

      if (narration) {
        sessionStorage.setItem('currentNarration', JSON.stringify(narration));
      } else {
        sessionStorage.removeItem('currentNarration');
      }
    },
    []
  );

  const value = useMemo(
    () => ({
      guestSession,
      setGuestSession,
      language,
      setLanguage,
      accessStatus,
      setAccessStatus,
      uiLanguage,
      setUiLanguage,
      adminUiLanguage,
      setAdminUiLanguage,
      currentNarration,
      setCurrentNarration,
      trackingEnabled,
      setTrackingEnabled
    }),
    [
      guestSession,
      setGuestSession,
      language,
      setLanguage,
      accessStatus,
      setAccessStatus,
      uiLanguage,
      setUiLanguage,
      adminUiLanguage,
      setAdminUiLanguage,
      currentNarration,
      setCurrentNarration,
      trackingEnabled
    ]
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useAppContext() {
  const context = useContext(AppContext);
  if (!context) throw new Error('useAppContext must be used inside AppProvider');
  return context;
}
