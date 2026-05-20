import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import NarrationPlayerScreen from './NarrationPlayerScreen';
import { renderWithApp, seedCurrentNarration, seedGuestSession, seedLanguage } from '../../test/renderWithApp';

const speakMock = vi.fn();
const pauseMock = vi.fn();
const stopMock = vi.fn();
const submitListeningHistoryMock = vi.fn();
const submitFeedbackMock = vi.fn();
const navigateMock = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return { ...actual, useNavigate: () => navigateMock };
});

vi.mock('../../hooks/useSpeechSynthesis', () => ({
  useSpeechSynthesis: () => ({
    supported: true,
    speaking: false,
    paused: false,
    speak: speakMock,
    pause: pauseMock,
    resume: vi.fn(),
    stop: stopMock
  })
}));

vi.mock('../../api/publicApi', () => ({
  submitListeningHistory: (payload: unknown) => submitListeningHistoryMock(payload),
  submitFeedback: (payload: unknown) => submitFeedbackMock(payload)
}));

describe('NarrationPlayerScreen', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    seedGuestSession();
    seedLanguage();
    seedCurrentNarration();
    submitListeningHistoryMock.mockResolvedValue(undefined);
    submitFeedbackMock.mockResolvedValue(undefined);
  });

  it('should render current narration from session storage', async () => {
    renderWithApp(<NarrationPlayerScreen />, ['/app/listen']);

    expect(screen.getByText('Giới thiệu Quán Ốc Vĩnh Khánh')).toBeInTheDocument();
    expect(screen.getByText(/Đây là nội dung thuyết minh/i)).toBeInTheDocument();

    await waitFor(() => {
      expect(submitListeningHistoryMock).toHaveBeenCalled();
    });
  });

  it('should use TTS when audioUrl is empty', async () => {
    const user = userEvent.setup();
    renderWithApp(<NarrationPlayerScreen />, ['/app/listen']);

    await user.click(screen.getByRole('button', { name: /nghe/i }));

    expect(speakMock).toHaveBeenCalledWith('Đây là nội dung thuyết minh dùng cho test.');
  });

  it('should pause and stop TTS', async () => {
    const user = userEvent.setup();
    renderWithApp(<NarrationPlayerScreen />, ['/app/listen']);

    await user.click(screen.getByRole('button', { name: /tạm dừng/i }));
    await user.click(screen.getByRole('button', { name: /dừng/i }));

    expect(pauseMock).toHaveBeenCalled();
    expect(stopMock).toHaveBeenCalled();
  });
});
