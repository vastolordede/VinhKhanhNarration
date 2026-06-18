import { useCallback, useEffect, useRef } from 'react';
import { checkGeofence } from '../api/publicApi';
import { useAppContext } from '../contexts/AppContext';
import { GeoPoint } from './useGeolocation';
import { NarrationResolveResultDTO } from '../types';

const intervalMs = Number(
  import.meta.env.VITE_GEOFENCE_INTERVAL_MS || 10000
);

export function useGeofenceWatcher(
  onNarrationDetected: (narration: NarrationResolveResultDTO) => void
) {
  const { guestSession, language, trackingEnabled } = useAppContext();
  const lastSentRef = useRef(0);
  const requestRunningRef = useRef(false);

  const handlePosition = useCallback(
    async (point: GeoPoint) => {
      if (
        !trackingEnabled ||
        !guestSession?.guestSessionId ||
        !language?.languageId ||
        requestRunningRef.current
      ) {
        return;
      }

      const now = Date.now();
      if (now - lastSentRef.current < intervalMs) return;

      lastSentRef.current = now;
      requestRunningRef.current = true;

      try {
        const result = await checkGeofence(
          guestSession.guestSessionId,
          point.latitude,
          point.longitude,
          language.languageId
        );

        if (
          result.shouldPlay &&
          result.narrationId &&
          result.translationId &&
          result.audioId &&
          result.audioUrl?.trim() &&
          result.text?.trim()
        ) {
          onNarrationDetected({
            placeId: result.placeId ?? null,
            dishId: null,
            narrationId: result.narrationId,
            translationId: result.translationId,
            audioId: result.audioId,
            geofenceEventId: result.geofenceEventId ?? null,
            title: result.title?.trim() || 'Thuyết minh tự động',
            text: result.text,
            audioUrl: result.audioUrl,
            languageCode: language.languageCode,
            locale: language.locale || language.languageCode,
            source: 'geofence'
          });
        }
      } catch (error) {
        console.error('Geofence check failed:', error);
      } finally {
        requestRunningRef.current = false;
      }
    }, [
      guestSession?.guestSessionId,
      language,
      trackingEnabled,
      onNarrationDetected
    ]
  );

  useEffect(() => {
    if (!trackingEnabled) {
      lastSentRef.current = 0;
      requestRunningRef.current = false;
    }
  }, [trackingEnabled]);

  return { handlePosition };
}
