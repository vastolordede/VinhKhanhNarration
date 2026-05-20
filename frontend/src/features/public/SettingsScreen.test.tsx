import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import SettingsScreen from './SettingsScreen';
import { renderWithApp, seedGuestSession, seedLanguage } from '../../test/renderWithApp';

const navigateMock = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return { ...actual, useNavigate: () => navigateMock };
});

describe('SettingsScreen', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    seedGuestSession();
    seedLanguage();
  });

  it('should show guest session and language', () => {
    renderWithApp(<SettingsScreen />, ['/app/settings']);

    expect(screen.getByText('GUEST-TEST-001')).toBeInTheDocument();
    expect(screen.getByText('Tiếng Việt')).toBeInTheDocument();
  });

  it('should navigate to language screen when clicking change language', async () => {
    const user = userEvent.setup();
    renderWithApp(<SettingsScreen />, ['/app/settings']);

    await user.click(screen.getByRole('button', { name: /đổi ngôn ngữ/i }));

    expect(navigateMock).toHaveBeenCalledWith('/app/language');
  });

  it('should toggle tracking button label', async () => {
    const user = userEvent.setup();
    renderWithApp(<SettingsScreen />, ['/app/settings']);

    await user.click(screen.getByRole('button', { name: /bật theo dõi/i }));

    expect(screen.getByRole('button', { name: /tắt theo dõi/i })).toBeInTheDocument();
  });
});
