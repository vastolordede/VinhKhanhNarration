import { describe, expect, it } from 'vitest';
import type { GuestAccessStatusDTO } from '../types';
import { hasUsableAccessPass } from './accessPolicy';

const activeStatus: GuestAccessStatusDTO = {
  hasActivePass: true,
  hoursRemaining: 24,
  pendingPayment: null,
  accessPass: {
    accessPassId: 1,
    guestSessionId: 'guest-1',
    guestPaymentOrderId: 2,
    startsAt: '2026-06-19T00:00:00Z',
    expiresAt: '2026-06-20T00:00:00Z',
    status: 'Active',
    createdAt: '2026-06-19T00:00:00Z',
    updatedAt: '2026-06-19T00:00:00Z'
  }
};

describe('hasUsableAccessPass', () => {
  it('accepts an active, unexpired pass', () => {
    expect(hasUsableAccessPass(activeStatus, new Date('2026-06-19T12:00:00Z'))).toBe(true);
  });

  it('rejects an expired pass even when the cached flag is true', () => {
    expect(hasUsableAccessPass(activeStatus, new Date('2026-06-20T00:00:01Z'))).toBe(false);
  });

  it('rejects a missing pass', () => {
    expect(hasUsableAccessPass(null)).toBe(false);
  });
});
