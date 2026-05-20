import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import FeedbackModal from './FeedbackModal';
import { renderWithApp, seedGuestSession } from '../../test/renderWithApp';

const submitFeedbackMock = vi.fn();

vi.mock('../../api/publicApi', () => ({
  submitFeedback: (payload: unknown) => submitFeedbackMock(payload)
}));

describe('FeedbackModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    seedGuestSession();
    submitFeedbackMock.mockResolvedValue(undefined);
  });

  it('should render feedback modal when open', () => {
    renderWithApp(<FeedbackModal open={true} onClose={vi.fn()} placeId={1} />);

    expect(screen.getByText('Gửi đánh giá')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Cảm nhận của bạn...')).toBeInTheDocument();
  });

  it('should submit feedback and close modal', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderWithApp(<FeedbackModal open={true} onClose={onClose} placeId={1} />);

    await user.click(screen.getAllByText('★')[2]);
    await user.type(screen.getByPlaceholderText('Cảm nhận của bạn...'), 'Quán rất ngon');
    await user.click(screen.getByRole('button', { name: /gửi feedback/i }));

    await waitFor(() => {
      expect(submitFeedbackMock).toHaveBeenCalledWith(
        expect.objectContaining({
          guestSessionId: 'GUEST-TEST-001',
          placeId: 1,
          rating: 3,
          comment: 'Quán rất ngon'
        })
      );
      expect(onClose).toHaveBeenCalled();
    });
  });
});
