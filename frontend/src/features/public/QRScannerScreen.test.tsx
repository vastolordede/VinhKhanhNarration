import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import QRScannerScreen from './QRScannerScreen';
import { renderWithApp, seedGuestSession, seedLanguage } from '../../test/renderWithApp';

const resolveQrMock = vi.fn();
const navigateMock = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return { ...actual, useNavigate: () => navigateMock };
});

vi.mock('html5-qrcode', () => ({
  Html5QrcodeScanner: vi.fn().mockImplementation(() => ({
    render: vi.fn(),
    clear: vi.fn().mockResolvedValue(undefined)
  }))
}));

vi.mock('../../api/publicApi', () => ({
  resolveQr: (qrCodeValue: string, languageId: number, guestSessionId: string) => resolveQrMock(qrCodeValue, languageId, guestSessionId)
}));

describe('QRScannerScreen', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    seedGuestSession();
    seedLanguage();
    resolveQrMock.mockResolvedValue({
      placeId: 1,
      dishId: null,
      narrationId: 10,
      translationId: 20,
      audioId: 30,
      title: 'QR Narration',
      text: 'QR narration text',
      audioUrl: null,
      useTts: true,
      source: 'qr'
    });
  });

  it('should resolve QR code from manual input', async () => {
    const user = userEvent.setup();
    renderWithApp(<QRScannerScreen />, ['/app/qr']);

    await user.type(screen.getByPlaceholderText('QR value...'), 'QR_PLACE_OC_VINH_KHANH');
    await user.click(screen.getByRole('button', { name: /resolve qr/i }));

    await waitFor(() => {
      expect(resolveQrMock).toHaveBeenCalledWith('QR_PLACE_OC_VINH_KHANH', 1, 'GUEST-TEST-001');
      expect(navigateMock).toHaveBeenCalledWith('/app/listen');
    });
  });
});
