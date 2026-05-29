import { ReactNode } from 'react';
import { useI18n } from '../../i18n/useI18n';

export function DataTable({
  headers,
  rows
}: {
  headers: string[];
  rows: ReactNode[][];
}) {
  const { tx } = useI18n();

  function renderCell(cell: ReactNode) {
    if (typeof cell === 'string') {
      return tx(cell);
    }

    return cell;
  }

  return (
    <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
      <div className="overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead className="bg-slate-50 text-left text-slate-500">
            <tr>
              {headers.map((header) => (
                <th key={header} className="px-4 py-3 font-semibold">
                  {tx(header)}
                </th>
              ))}
            </tr>
          </thead>

          <tbody className="divide-y divide-slate-100">
            {rows.map((row, index) => (
              <tr key={index} className="text-slate-700">
                {row.map((cell, cellIndex) => (
                  <td key={cellIndex} className="px-4 py-3 align-top">
                    {renderCell(cell)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}