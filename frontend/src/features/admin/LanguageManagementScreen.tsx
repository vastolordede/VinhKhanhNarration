import { useEffect, useMemo, useState } from 'react';
import { endpoints } from '../../api/endpoints';
import { getList } from '../../api/crud';
import { Card } from '../../components/ui/Card';
import { LanguageDTO } from '../../types';
import { StatusBadge } from '../../components/ui/StatusBadge';
import { useI18n } from '../../i18n/useI18n';

export default function LanguageManagementScreen() {
  const [languages, setLanguages] = useState<LanguageDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const { tx } = useI18n();

  useEffect(() => {
    setLoading(true);
    setError(null);

    getList<LanguageDTO>(endpoints.languages)
      .then(setLanguages)
      .catch(() => setError('Không tải được dữ liệu. Kiểm tra backend endpoint.'))
      .finally(() => setLoading(false));
  }, []);

  const sortedLanguages = useMemo(() => {
    return [...languages].sort((a, b) => {
      const aCode = a.languageCode?.toLowerCase() ?? '';
      const bCode = b.languageCode?.toLowerCase() ?? '';

      const order = ['vi', 'en', 'ja', 'ko', 'zh'];
      const aIndex = order.indexOf(aCode);
      const bIndex = order.indexOf(bCode);

      if (aIndex !== -1 && bIndex !== -1) return aIndex - bIndex;
      if (aIndex !== -1) return -1;
      if (bIndex !== -1) return 1;

      return aCode.localeCompare(bCode);
    });
  }, [languages]);

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">
          {tx('Language Management')}
        </h1>
      </div>

      <Card>
        {loading && (
          <p className="text-sm text-slate-500">
            {tx('Đang tải dữ liệu...')}
          </p>
        )}

        {error && (
          <p className="text-sm text-rose-600">
            {tx(error)}
          </p>
        )}

        {!loading && !error && sortedLanguages.length === 0 && (
          <p className="text-sm text-slate-500">
            {tx('Chưa có dữ liệu')}
          </p>
        )}

        {!loading && !error && sortedLanguages.length > 0 && (
          <div className="overflow-hidden rounded-2xl border border-slate-100">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 text-slate-500">
                <tr>
                  <th className="px-4 py-3 font-semibold">
                    {tx('Language Id')}
                  </th>

                  <th className="px-4 py-3 font-semibold">
                    {tx('Language Code')}
                  </th>

                  <th className="px-4 py-3 font-semibold">
                    {tx('Language Name')}
                  </th>

                  <th className="px-4 py-3 font-semibold">
                    {tx('Default')}
                  </th>

                  <th className="px-4 py-3 font-semibold">
                    {tx('Active')}
                  </th>
                </tr>
              </thead>

              <tbody className="divide-y divide-slate-100">
                {sortedLanguages.map((language) => {
                  const isDefault = Boolean((language as any).isDefault);

                  return (
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
                        <StatusBadge active={isDefault} />
                      </td>

                      <td className="px-4 py-3 text-slate-700">
                        <StatusBadge active={language.isActive} />
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
}