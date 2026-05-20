import { act, renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useSpeechSynthesis } from './useSpeechSynthesis';

describe('useSpeechSynthesis', () => {
  const speakMock = vi.fn();
  const cancelMock = vi.fn();
  const pauseMock = vi.fn();
  const resumeMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    Object.defineProperty(window, 'speechSynthesis', {
      writable: true,
      value: {
        speak: speakMock,
        cancel: cancelMock,
        pause: pauseMock,
        resume: resumeMock,
        getVoices: vi.fn(() => [])
      }
    });

    Object.defineProperty(window, 'SpeechSynthesisUtterance', {
      writable: true,
      value: vi.fn().mockImplementation((text: string) => ({
        text,
        lang: '',
        rate: 1,
        pitch: 1,
        onstart: null,
        onend: null,
        onerror: null
      }))
    });
  });

  it('should support speech synthesis when browser API exists', () => {
    const { result } = renderHook(() => useSpeechSynthesis('vi'));

    expect(result.current.supported).toBe(true);
  });

  it('should call speechSynthesis.speak when speak is called', () => {
    const { result } = renderHook(() => useSpeechSynthesis('vi'));

    act(() => {
      result.current.speak('Xin chào Vĩnh Khánh');
    });

    expect(cancelMock).toHaveBeenCalled();
    expect(speakMock).toHaveBeenCalledTimes(1);
  });

  it('should call cancel when stop is called', () => {
    const { result } = renderHook(() => useSpeechSynthesis('vi'));

    act(() => {
      result.current.stop();
    });

    expect(cancelMock).toHaveBeenCalled();
  });

  it('should call pause and resume', () => {
    const { result } = renderHook(() => useSpeechSynthesis('vi'));

    act(() => {
      result.current.pause();
    });

    act(() => {
      result.current.resume();
    });

    expect(pauseMock).toHaveBeenCalled();
    expect(resumeMock).toHaveBeenCalled();
  });
});