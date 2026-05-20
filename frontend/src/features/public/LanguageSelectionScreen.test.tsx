import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import LanguageSelectionScreen from './LanguageSelectionScreen';
import { renderWithApp, seedGuestSession } from '../../test/renderWithApp';

const navigateMock = vi.fn();
const getActiveLanguagesMock = vi.fn();
const updateGuestLanguageMock = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return { ...actual, useNavigate: () => navigateMock };
});

vi.mock('../../api/publicApi', () => ({
  getActiveLanguages: () => getActiveLanguagesMock(),
  updateGuestLanguage: (guestSessionId: string, languageId: number) => updateGuestLanguageMock(guestSessionId, languageId)
}));

describe('LanguageSelectionScreen', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    seedGuestSession();
    getActiveLanguagesMock.mockResolvedValue([
      { languageId: 1, languageCode: 'vi', languageName: 'Tiếng Việt', isDefault: true, isActive: true },
      { languageId: 2, languageCode: 'en', languageName: 'English', isDefault: false, isActive: true }
    ]);
    updateGuestLanguageMock.mockResolvedValue(undefined);
  });

  it('should render active languages from API', async () => {
    renderWithApp(<LanguageSelectionScreen />, ['/app/language']);

    expect(screen.getByText('Đang tải ngôn ngữ...')).toBeInTheDocument();
    expect(await screen.findByText('Tiếng Việt')).toBeInTheDocument();
    expect(await screen.findByText('English')).toBeInTheDocument();
  });

  it('should update guest language and navigate to map when choosing language', async () => {
    const user = userEvent.setup();
    renderWithApp(<LanguageSelectionScreen />, ['/app/language']);

    await screen.findByText('Tiếng Việt');
    const chooseButtons = screen.getAllByRole('button', { name: /chọn/i });
    await user.click(chooseButtons[0]);

    await waitFor(() => {
      expect(updateGuestLanguageMock).toHaveBeenCalledWith('GUEST-TEST-001', 1);
      expect(navigateMock).toHaveBeenCalledWith('/app/map');
    });
  });
});
