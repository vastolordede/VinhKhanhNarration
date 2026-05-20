import { TextareaHTMLAttributes } from 'react';

export function Textarea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea {...props} className={`w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none focus:border-teal-600 ${props.className ?? ''}`} />;
}
