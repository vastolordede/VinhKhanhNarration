import { Button } from './Button';
import { useI18n } from '../../i18n/useI18n';

export function PaginationBar({
  page,
  pageSize,
  totalItems,
  totalPages,
  onPageChange
}: {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  const { tx } = useI18n();

  const from = totalItems === 0 ? 0 : (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalItems);

  return (
    <div className="mt-4 flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-600">
      <div>
        {tx('Showing')} {from} - {to} / {totalItems}
      </div>

      <div className="flex items-center gap-2">
        <Button
          type="button"
          variant="secondary"
          className="px-3 py-2"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          {tx('Previous')}
        </Button>

        <span className="px-2 font-semibold text-slate-700">
          {tx('Page')} {page} / {Math.max(totalPages, 1)}
        </span>

        <Button
          type="button"
          variant="secondary"
          className="px-3 py-2"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          {tx('Next')}
        </Button>
      </div>
    </div>
  );
}