import { useCallback, useEffect, useRef } from 'react';
import { checkGeofence } from '../api/publicApi';
import { useAppContext } from '../contexts/AppContext';
import { GeoPoint } from './useGeolocation';
import { NarrationResolveResultDTO } from '../types';

const intervalMs = Number(import.meta.env.VITE_GEOFENCE_INTERVAL_MS || 10000);

export function useGeofenceWatcher(onNarrationDetected: (narration: NarrationResolveResultDTO) => void) {
  const { guestSession, language, trackingEnabled } = useAppContext();
  const lastSentRef = useRef<number>(0);

  const handlePosition = useCallback(
    async (point: GeoPoint) => {
      if (!trackingEnabled || !guestSession?.guestSessionId || !language?.languageId) return;
      const now = Date.now();
      if (now - lastSentRef.current < intervalMs) return;
      lastSentRef.current = now;

      const result = await checkGeofence(
        guestSession.guestSessionId,
        point.latitude,
        point.longitude,
        language.languageId
      );

      if (result.shouldPlay && result.narrationId && result.translationId) {
        onNarrationDetected({
          placeId: result.placeId ?? null,
          dishId: null,
          narrationId: result.narrationId,
          translationId: result.translationId,
          audioId: result.audioId ?? null,
          title: result.title || 'Thuyết minh tự động',
          text: result.text || result.reason || '',
          audioUrl: result.audioUrl ?? null,
          useTts: !result.audioUrl,
          source: 'geofence'
        });
      }
    },
    [guestSession, language, trackingEnabled, onNarrationDetected]
  );

  useEffect(() => {
    if (!trackingEnabled) lastSentRef.current = 0;
  }, [trackingEnabled]);

  return { handlePosition };
}
