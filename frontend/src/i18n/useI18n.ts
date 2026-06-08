import { useAppContext } from '../contexts/AppContext';
import { translateKey, translateText } from './translations';
import { useLocation } from 'react-router-dom';

export function useI18n() {
  const { uiLanguage, adminUiLanguage } = useAppContext();
  const location = useLocation();

  const activeLanguage = location.pathname.startsWith('/admin')
    ? adminUiLanguage
    : uiLanguage;

  return {
    uiLanguage: activeLanguage,
    t: (key: string) => translateKey(key, activeLanguage),
    tx: (text: string) => translateText(text, activeLanguage)
  };
}