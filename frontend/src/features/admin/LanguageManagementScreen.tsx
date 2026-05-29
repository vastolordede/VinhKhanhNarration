import { useEffect, useMemo, useState } from 'react';
import { endpoints } from '../../api/endpoints';
import { getList } from '../../api/crud';
import { Card } from '../../components/ui/Card';
import { LanguageDTO } from '../../types';

const supportedLanguages = [
  {
    code: 'vi',
    name: 'Tiếng Việt',
    systemLabel: 'Vietnamese',
    ttsLang: 'vi-VN',
    isDefault: true
  },
  {
    code: 'en',
    name: 'English',
    systemLabel: 'English',
    ttsLang: 'en-US',
    isDefault: false
  },
  {
    code: 'ja',
    name: '日本語',
    systemLabel: 'Japanese',
    ttsLang: 'ja-JP',
    isDefault: false
  },
  {
    code: 'ko',
    name: '한국어',
    systemLabel: 'Korean',
    ttsLang: 'ko-KR',
    isDefault: false
  },
  {
    code: 'zh',
    name: '中文',
    systemLabel: 'Chinese',
    ttsLang: 'zh-CN',
    isDefault: false
  }
];

export default function LanguageManagementScreen() {
  const [languages, setLanguages] = useState<LanguageDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getList<LanguageDTO>(endpoints.languages)
      .then(setLanguages)
      .catch(() => setError('Không tải được danh sách ngôn ngữ. Kiểm tra backend endpoint.'))
      .finally(() => setLoading(false));
  }, []);

  const languageByCode = useMemo(() => {
    return new Map(
      languages.map((language) => [
        language.languageCode.toLowerCase(),
        language
      ])
    );
  }, [languages]);

  const supportedCodes = useMemo(
    () => new Set(supportedLanguages.map((language) => language.code)),
    []
  );

  const unsupportedLanguages = useMemo(() => {
    return languages.filter(
      (language) => !supportedCodes.has(language.languageCode.toLowerCase())
    );
  }, [languages, supportedCodes]);

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">
          Quản lý ngôn ngữ
        </h1>

        <p className="mt-1 text-sm text-slate-500">
          Danh sách ngôn ngữ thuyết minh được cố định cho hệ thống. Admin không tạo mới ngôn ngữ tại màn hình này.
        </p>
      </div>

      <Card>
        <div className="mb-4">
          <h2 className="text-lg font-bold text-slate-900">
            Ngôn ngữ hệ thống hỗ trợ
          </h2>

          <p className="mt-1 text-sm text-slate-500">
            Các ngôn ngữ này sẽ được dùng cho bản dịch thuyết minh và lựa chọn ngôn ngữ nghe của khách.
          </p>
        </div>

        {loading && (
          <p className="text-sm text-slate-500">
            Đang tải dữ liệu...
          </p>
        )}

        {error && (
          <p className="text-sm text-rose-600">
            {error}
          </p>
        )}

        {!loading && !error && (
          <div className="overflow-hidden rounded-2xl border border-slate-100">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 text-slate-500">
                <tr>
                  <th className="px-4 py-3 font-semibold">Mã</th>
                  <th className="px-4 py-3 font-semibold">Tên hiển thị</th>
                  <th className="px-4 py-3 font-semibold">Nhãn hệ thống</th>
                  <th className="px-4 py-3 font-semibold">TTS lang</th>
                  <th className="px-4 py-3 font-semibold">Mặc định</th>
                  <th className="px-4 py-3 font-semibold">Trạng thái DB</th>
                </tr>
              </thead>

              <tbody className="divide-y divide-slate-100">
                {supportedLanguages.map((item) => {
                  const dbLanguage = languageByCode.get(item.code);
                  const exists = !!dbLanguage;
                  const active = dbLanguage?.isActive === true;

                  return (
                    <tr key={item.code} className="bg-white">
                      <td className="px-4 py-3 font-semibold text-slate-900">
                        {item.code}
                      </td>

                      <td className="px-4 py-3 text-slate-700">
                        {item.name}
                      </td>

                      <td className="px-4 py-3 text-slate-700">
                        {item.systemLabel}
                      </td>

                      <td className="px-4 py-3 text-slate-700">
                        {item.ttsLang}
                      </td>

                      <td className="px-4 py-3 text-slate-700">
                        {item.isDefault ? 'Có' : 'Không'}
                      </td>

                      <td className="px-4 py-3">
                        {exists && active && (
                          <span className="rounded-full bg-teal-50 px-3 py-1 text-xs font-bold text-teal-700">
                            Đang hoạt động
                          </span>
                        )}

                        {exists && !active && (
                          <span className="rounded-full bg-amber-50 px-3 py-1 text-xs font-bold text-amber-700">
                            Có trong DB nhưng đang tắt
                          </span>
                        )}

                        {!exists && (
                          <span className="rounded-full bg-rose-50 px-3 py-1 text-xs font-bold text-rose-700">
                            Chưa có trong DB
                          </span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {unsupportedLanguages.length > 0 && (
        <Card className="mt-5">
          <h2 className="text-lg font-bold text-slate-900">
            Ngôn ngữ không thuộc cấu hình cố định
          </h2>

          <p className="mt-1 text-sm text-slate-500">
            Các dòng này nên được tắt bằng file seed_supported_languages_fixed.sql. Không nên dùng cho app public.
          </p>

          <div className="mt-4 overflow-hidden rounded-2xl border border-slate-100">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 text-slate-500">
                <tr>
                  <th className="px-4 py-3 font-semibold">Id</th>
                  <th className="px-4 py-3 font-semibold">Mã</th>
                  <th className="px-4 py-3 font-semibold">Tên</th>
                  <th className="px-4 py-3 font-semibold">Hoạt động</th>
                </tr>
              </thead>

              <tbody className="divide-y divide-slate-100">
                {unsupportedLanguages.map((language) => (
                  <tr key={language.languageId} className="bg-white">
                    <td className="px-4 py-3 text-slate-700">
                      {language.languageId}
                    </td>

                    <td className="px-4 py-3 font-semibold text-slate-900">
                      {language.languageCode}
                    </td>

                    <td className="px-4 py-3 text-slate-700">
                      {language.languageName}
                    </td>

                    <td className="px-4 py-3 text-slate-700">
                      {language.isActive ? 'Có' : 'Không'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}
    </div>
  );
}