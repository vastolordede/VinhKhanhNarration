export function LoadingState({ label = 'Đang tải dữ liệu...' }: { label?: string }) {
  return <div className="rounded-3xl bg-white p-6 text-center text-sm text-slate-500 shadow-sm">{label}</div>;
}
