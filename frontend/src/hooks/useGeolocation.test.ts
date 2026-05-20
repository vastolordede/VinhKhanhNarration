import { act, renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useGeolocation } from './useGeolocation';

describe('useGeolocation', () => {
  const getCurrentPositionMock = vi.fn();
  const watchPositionMock = vi.fn();
  const clearWatchMock = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();

    Object.defineProperty(window.navigator, 'geolocation', {
      writable: true,
      value: {
        getCurrentPosition: getCurrentPositionMock,
        watchPosition: watchPositionMock,
        clearWatch: clearWatchMock
      }
    });
  });

  it('should request current position successfully', () => {
    getCurrentPositionMock.mockImplementation((success) => {
      success({
        coords: {
          latitude: 10.7569,
          longitude: 106.7057,
          accuracy: 20
        }
      });
    });

    const { result } = renderHook(() => useGeolocation());

    act(() => {
      result.current.requestCurrentPosition();
    });

    expect(result.current.position?.latitude).toBe(10.7569);
    expect(result.current.position?.longitude).toBe(106.7057);
    expect(result.current.error).toBeNull();
  });

  it('should set error when geolocation fails', () => {
    getCurrentPositionMock.mockImplementation((_success, error) => {
      error({
        message: 'Permission denied'
      });
    });

    const { result } = renderHook(() => useGeolocation());

    act(() => {
      result.current.requestCurrentPosition();
    });

    expect(result.current.error).toBe('Permission denied');
  });

  it('should start watching position', () => {
    watchPositionMock.mockImplementation((success) => {
      success({
        coords: {
          latitude: 10.757,
          longitude: 106.706,
          accuracy: 15
        }
      });

      return 100;
    });

    const { result } = renderHook(() => useGeolocation());

    act(() => {
      result.current.startWatching();
    });

    expect(watchPositionMock).toHaveBeenCalled();
    expect(result.current.position?.latitude).toBe(10.757);
  });

  it('should stop watching position', () => {
    watchPositionMock.mockReturnValue(100);

    const { result } = renderHook(() => useGeolocation());

    act(() => {
      result.current.startWatching();
    });

    act(() => {
      result.current.stopWatching();
    });

    expect(clearWatchMock).toHaveBeenCalledWith(100);
  });
});