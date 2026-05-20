import { ReactNode } from 'react';
import { X } from 'lucide-react';

export function BottomSheet({ open, onClose, children }: { open: boolean; onClose: () => void; children: ReactNode }) {
  if (!open) return null;
  return (
    <div className="fixed inset-x-0 bottom-0 z-[900] mx-auto max-w-[520px] rounded-t-[2rem] bg-white shadow-2xl safe-bottom">
      <div className="flex items-center justify-center pt-3">
        <div className="h-1.5 w-12 rounded-full bg-slate-300" />
      </div>
      <button onClick={onClose} className="absolute right-4 top-4 rounded-full bg-slate-100 p-2 text-slate-600">
        <X size={18} />
      </button>
      <div className="max-h-[72vh] overflow-y-auto px-5 pb-6 pt-4">{children}</div>
    </div>
  );
}
