import { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import {
  getActiveLanguages,
  resolveNarration,
  updateGuestLanguage
} from '../../api/publicApi';
import { useAppContext } from '../../contexts/AppContext';
import { UiLanguage } from '../../i18n/translations';
import { useI18n } from '../../i18n/useI18n';
import { LanguageDTO } from '../../types';

type Props = {
  compact?: boolean;
};

type PublicLanguageOption = {
  code: UiLanguage;
  labelKey: string;
};

type AdminLanguageOption = {
  code: 'vi' | 'en';
  labelKey: string;
};

const publicOptions: PublicLanguageOption[] = [
  { code: 'vi', labelKey: 'language.vi' },
  { code: 'en', labelKey: 'language.en' },
  { code: 'ja', labelKey: 'language.ja' },
  { code: 'ko', labelKey: 'language.ko' },
  { code: 'zh', labelKey: 'language.zh' }
];

const adminOptions: AdminLanguageOption[] = [
  { code: 'vi', labelKey: 'language.vi' },
  { code: 'en', labelKey: 'language.en' }
];

/**
 * Chuẩn hóa các mã như:
 * vi, vi-VN, vi_VN → vi
 * en, en-US → en
 */
function normalizeLanguageCode(value?: string | null): string {
  return (value ?? '')
    .trim()
    .toLowerCase()
    .replace('_', '-')
    .split('-')[0];
}

function findContentLanguage(
  languages: LanguageDTO[],
  uiLanguage: UiLanguage
): LanguageDTO | undefined {
  const requestedCode = normalizeLanguageCode(uiLanguage);

  return languages.find((language) => {
    const languageCode = normalizeLanguageCode(
      language.languageCode
    );

    const localeCode = normalizeLanguageCode(
      language.locale
    );

    return (
      language.isActive &&
      language.isContentEnabled &&
      (
        languageCode === requestedCode ||
        localeCode === requestedCode
      )
    );
  });
}

export function LanguageSwitcher({
  compact = false
}: Props) {
  const {
    uiLanguage,
    setUiLanguage,

    adminUiLanguage,
    setAdminUiLanguage,

    guestSession,

    language,
    setLanguage,

    currentNarration,
    setCurrentNarration
  } = useAppContext();

  const { t } = useI18n();
  const location = useLocation();

  const [languages, setLanguages] = useState<
    LanguageDTO[]
  >([]);

  const [
    changingLanguage,
    setChangingLanguage
  ] = useState<UiLanguage | null>(null);

  const isAdminRoute =
    location.pathname.startsWith('/admin');

  const activeLanguage = isAdminRoute
    ? adminUiLanguage
    : uiLanguage;

  const options = isAdminRoute
    ? adminOptions
    : publicOptions;

  useEffect(() => {
    if (isAdminRoute) {
      return;
    }

    let cancelled = false;

    async function loadLanguages(): Promise<void> {
      try {
        const result = await getActiveLanguages();

        if (!cancelled) {
          setLanguages(result);
        }
      } catch (error) {
        console.error(
          'Load active languages failed:',
          error
        );
      }
    }

    void loadLanguages();

    return () => {
      cancelled = true;
    };
  }, [isAdminRoute]);

  async function changePublicLanguage(
    code: UiLanguage
  ): Promise<void> {
    if (changingLanguage) {
      return;
    }

    /*
     * Không gọi API lại khi cả ngôn ngữ giao diện
     * và ngôn ngữ narration đã đúng.
     */
    const currentContentCode =
      normalizeLanguageCode(
        language?.languageCode ?? language?.locale
      );

    if (
      uiLanguage === code &&
      currentContentCode ===
        normalizeLanguageCode(code)
    ) {
      return;
    }

    setChangingLanguage(code);

    /*
     * Lưu narration hiện tại trước khi setLanguage,
     * vì setLanguage sẽ chủ động xóa narration/audio cũ.
     */
    const previousNarration = currentNarration;

    try {
      let availableLanguages = languages;

      /*
       * Người dùng có thể bấm rất nhanh trước khi
       * useEffect tải xong danh sách ngôn ngữ.
       */
      if (availableLanguages.length === 0) {
        availableLanguages =
          await getActiveLanguages();

        setLanguages(availableLanguages);
      }

      const narrationLanguage =
        findContentLanguage(
          availableLanguages,
          code
        );

      if (!narrationLanguage) {
        console.error(
          `Narration language is not available: ${code}`
        );

        return;
      }

      /*
       * Đổi ngôn ngữ giao diện.
       */
      setUiLanguage(code);

      /*
       * Đồng bộ ngôn ngữ narration vào Guest Session.
       * Guest chưa thanh toán/chưa có session vẫn được
       * chọn ngôn ngữ local.
       */
      if (guestSession?.guestSessionId) {
        try {
          await updateGuestLanguage(
            guestSession.guestSessionId,
            narrationLanguage.languageId
          );
        } catch (error) {
          /*
           * Không chặn việc đổi ngôn ngữ local nếu API
           * cập nhật session tạm thời thất bại.
           */
          console.error(
            'Update guest narration language failed:',
            error
          );
        }
      }

      /*
       * setLanguage đồng thời xóa narration và audio cũ
       * khỏi context/sessionStorage.
       */
      setLanguage(narrationLanguage);

      /*
       * Nếu khách đang ở Player, tải lại đúng narration
       * và file audio của ngôn ngữ vừa chọn.
       */

if (
  previousNarration?.narrationId &&
  guestSession?.guestSessionId
) {
  try {
    const translatedNarration =
      await resolveNarration(
        previousNarration.narrationId,
        narrationLanguage.languageId,
        guestSession.guestSessionId
      );

    setCurrentNarration(
      translatedNarration
    );
  } catch (error) {
    console.error(
      'Reload narration in selected language failed:',
      error
    );

    setCurrentNarration(null);
  }
} else {
  /*
   * Chưa có Guest Session hoặc chưa mở narration,
   * chỉ cập nhật ngôn ngữ. Khi khách mở nội dung sau,
   * hệ thống sẽ lấy audio theo ngôn ngữ mới.
   */
  setCurrentNarration(null);
}


    } catch (error) {
      console.error(
        'Change public language failed:',
        error
      );
    } finally {
      setChangingLanguage(null);
    }
  }

  return (
    <div
      className={
        compact
          ? [
              'flex flex-wrap items-center gap-1',
              'rounded-2xl bg-white/90 p-1',
              'shadow-sm'
            ].join(' ')
          : 'rounded-2xl bg-slate-50 p-2'
      }
    >
      {!compact && (
        <p
          className={[
            'mb-2 text-xs font-semibold uppercase',
            'tracking-wide text-slate-500'
          ].join(' ')}
        >
          {t('language.switch')}
        </p>
      )}

      <div className="flex flex-wrap gap-1">
        {options.map((option) => {
          const active =
            activeLanguage === option.code;

          const changing =
            changingLanguage === option.code;

          const disabled =
            !isAdminRoute &&
            changingLanguage !== null;

          return (
            <button
              key={option.code}
              type="button"
              aria-pressed={active}
              disabled={disabled}
              onClick={() => {
                if (isAdminRoute) {
                  setAdminUiLanguage(
                    option.code as 'vi' | 'en'
                  );

                  return;
                }

                void changePublicLanguage(
                  option.code
                );
              }}
              className={[
                'rounded-full px-3 py-1.5',
                'text-xs font-bold transition',
                'disabled:cursor-wait',
                'disabled:opacity-60',
                active
                  ? 'bg-teal-700 text-white'
                  : [
                      'bg-white text-slate-600',
                      'hover:bg-slate-100'
                    ].join(' ')
              ].join(' ')}
            >
              {changing
                ? '...'
                : compact
                  ? option.code.toUpperCase()
                  : t(option.labelKey)}
            </button>
          );
        })}
      </div>
    </div>
  );
}

