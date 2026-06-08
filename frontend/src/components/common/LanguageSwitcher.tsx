import { useAppContext } from '../../contexts/AppContext';
import { UiLanguage } from '../../i18n/translations';
import { useI18n } from '../../i18n/useI18n';
import { useLocation } from 'react-router-dom';

type Props = {
  compact?: boolean;
};

const options: { code: Extract<UiLanguage, 'vi' | 'en'>; labelKey: string }[] = [
  { code: 'vi', labelKey: 'language.vi' },
  { code: 'en', labelKey: 'language.en' }
];

export function LanguageSwitcher({ compact = false }: Props) {
  const { uiLanguage, setUiLanguage, adminUiLanguage, setAdminUiLanguage } = useAppContext();
  const { t } = useI18n();

  const location = useLocation();
const isAdminRoute = location.pathname.startsWith('/admin');
const activeLanguage = isAdminRoute ? adminUiLanguage : uiLanguage;

  return (
    <div
      className={
        compact
          ? 'flex items-center gap-1 rounded-full bg-white/90 p-1 shadow-sm'
          : 'rounded-2xl bg-slate-50 p-2'
      }
    >
      {!compact && (
        <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
          {t('language.switch')}
        </p>
      )}

      <div className="flex gap-1">
        {options.map((option) => {
          const active = activeLanguage === option.code;

          return (
            <button
              key={option.code}
              type="button"
              onClick={() => {
  if (isAdminRoute) setAdminUiLanguage(option.code);
  else setUiLanguage(option.code);
}}
              className={`rounded-full px-3 py-1.5 text-xs font-bold transition ${
                active ? 'bg-teal-700 text-white' : 'bg-white text-slate-600 hover:bg-slate-100'
              }`}
            >
              {compact ? option.code.toUpperCase() : t(option.labelKey)}
            </button>
          );
        })}
      </div>
    </div>
  );
}