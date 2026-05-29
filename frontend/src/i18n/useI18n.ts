import { useAppContext } from '../contexts/AppContext';
import { translateKey, translateText } from './translations';

export function useI18n() {
  const { uiLanguage } = useAppContext();

  return {
    uiLanguage,
    t: (key: string) => translateKey(key, uiLanguage),
    tx: (text: string) => translateText(text, uiLanguage)
  };
}