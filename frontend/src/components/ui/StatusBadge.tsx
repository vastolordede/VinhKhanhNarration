import { useI18n } from '../../i18n/useI18n';

type StatusBadgeProps = {
  active: boolean;
  trueText?: string;
  falseText?: string;
};

export function StatusBadge({
  active,
  trueText = 'Yes',
  falseText = 'No'
}: StatusBadgeProps) {
  const { tx } = useI18n();

  const label = active ? trueText : falseText;

  return (
    <span
      className={[
        'inline-flex min-w-[54px] items-center justify-center rounded-xl px-3 py-1 text-xs font-bold ring-1',
        active
          ? 'bg-emerald-100 text-emerald-700 ring-emerald-200'
          : 'bg-rose-100 text-rose-700 ring-rose-200'
      ].join(' ')}
    >
      {tx(label)}
    </span>
  );
}