import type { GuestAccessStatusDTO } from '../types';

export function hasUsableAccessPass(
  status: GuestAccessStatusDTO | null | undefined,
  now = new Date()
): boolean {
  if (!status?.hasActivePass || status.accessPass?.status !== 'Active') return false;
  const expiresAt = Date.parse(status.accessPass.expiresAt);
  return Number.isFinite(expiresAt) && expiresAt > now.getTime();
}
