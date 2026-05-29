import { TextareaHTMLAttributes } from 'react';
import { useI18n } from '../../i18n/useI18n';

export function Textarea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  const { tx } = useI18n();

  return (
    <textarea
      {...props}
      placeholder={props.placeholder ? tx(props.placeholder) : undefined}
      className={`w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none focus:border-teal-600 ${props.className ?? ''}`}
    />
  );
}