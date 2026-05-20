import { useCallback, useRef, useState } from 'react';

export type GeoPoint = {
  latitude: number;
  longitude: number;
  accuracy?: number;
};

export function useGeolocation() {
  const [position, setPosition] = useState<GeoPoint | null>(null);
  const [error, setError] = useState<string | null>(null);
  const watchIdRef = useRef<number | null>(null);

  const requestCurrentPosition = useCallback(() => {
    if (!navigator.geolocation) {
      setError('Trình duyệt không hỗ trợ geolocation.');
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setError(null);
        setPosition({
          latitude: pos.coords.latitude,
          longitude: pos.coords.longitude,
          accuracy: pos.coords.accuracy
        });
      },
      (err) => setError(err.message),
      { enableHighAccuracy: true, timeout: 10000, maximumAge: 5000 }
    );
  }, []);

  const startWatching = useCallback((onChange?: (point: GeoPoint) => void) => {
    if (!navigator.geolocation) {
      setError('Trình duyệt không hỗ trợ geolocation.');
      return;
    }

    if (watchIdRef.current !== null) return;

    watchIdRef.current = navigator.geolocation.watchPosition(
      (pos) => {
        const point = {
          latitude: pos.coords.latitude,
          longitude: pos.coords.longitude,
          accuracy: pos.coords.accuracy
        };
        setError(null);
        setPosition(point);
        onChange?.(point);
      },
      (err) => setError(err.message),
      { enableHighAccuracy: true, timeout: 10000, maximumAge: 5000 }
    );
  }, []);

  const stopWatching = useCallback(() => {
    if (watchIdRef.current !== null) {
      navigator.geolocation.clearWatch(watchIdRef.current);
      watchIdRef.current = null;
    }
  }, []);

  return { position, error, requestCurrentPosition, startWatching, stopWatching };
}
