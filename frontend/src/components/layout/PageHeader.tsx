import { ReactNode } from 'react';
import { useI18n } from '../../i18n/useI18n';

export function PageHeader({
  title,
  description,
  action,
  actions
}: {
  title: string;
  description?: string;
  action?: ReactNode;
  actions?: ReactNode;
}) {
  const { tx } = useI18n();

  return (
    <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">{tx(title)}</h1>

        {description && (
          <p className="mt-1 text-sm text-slate-500">{tx(description)}</p>
        )}
      </div>

      {actions ?? action}
    </div>
  );
}