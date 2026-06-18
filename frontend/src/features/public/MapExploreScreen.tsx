import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { DivIcon, latLngBounds } from 'leaflet';
import { MapContainer, Marker, TileLayer, useMap } from 'react-leaflet';
import { LocateFixed, Volume2 } from 'lucide-react';
import {
  getActivePlaces,
  getPlaceDishes,
  resolveDishNarration,
  resolvePlaceNarration
} from '../../api/publicApi';
import { BottomSheet } from '../../components/ui/BottomSheet';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { useGeofenceWatcher } from '../../hooks/useGeofenceWatcher';
import { useGeolocation } from '../../hooks/useGeolocation';
import { NarrationResolveResultDTO, PlaceDTO, PlaceDishDTO } from '../../types';
import { useI18n } from '../../i18n/useI18n';
import FeedbackModal from './FeedbackModal';

const defaultLat = Number(import.meta.env.VITE_DEFAULT_MAP_LAT || 10.7569);
const defaultLng = Number(import.meta.env.VITE_DEFAULT_MAP_LNG || 106.7057);
const defaultZoom = Number(import.meta.env.VITE_DEFAULT_MAP_ZOOM || 16);

const placeIcon = new DivIcon({
  className: '',
  html: '<div class="marker-place"></div>',
  iconSize: [28, 28],
  iconAnchor: [14, 14]
});

const userIcon = new DivIcon({
  className: '',
  html: '<div class="marker-user"></div>',
  iconSize: [22, 22],
  iconAnchor: [11, 11]
});

function MapAutoFocus({
  places,
  selectedPlace,
  userPosition
}: {
  places: Array<PlaceDTO & { latitude: number; longitude: number }>;
  selectedPlace: PlaceDTO | null;
  userPosition?: { latitude: number; longitude: number };
}) {
  const map = useMap();

  useEffect(() => {
    if (userPosition) {
      map.setView([userPosition.latitude, userPosition.longitude], 17, {
        animate: true
      });
      return;
    }

    if (
      selectedPlace?.latitude !== null &&
      selectedPlace?.latitude !== undefined &&
      selectedPlace?.longitude !== null &&
      selectedPlace?.longitude !== undefined
    ) {
      map.setView([selectedPlace.latitude, selectedPlace.longitude], 17, {
        animate: true
      });
      return;
    }

    if (places.length === 1) {
      map.setView([places[0].latitude, places[0].longitude], 17, {
        animate: true
      });
      return;
    }

    if (places.length > 1) {
      const bounds = latLngBounds(
        places.map((place) => [place.latitude, place.longitude])
      );

      map.fitBounds(bounds, {
        paddingTopLeft: [40, 140],
        paddingBottomRight: [40, 120],
        maxZoom: 17,
        animate: true
      });
    }
  }, [map, places, selectedPlace, userPosition]);

  return null;
}

function narrationErrorMessage(message: string, t: (key: string) => string) {
  if (message.includes('CONTENT_LANGUAGE_NOT_AVAILABLE')) {
    return t('public.map.contentLanguageUnavailable');
  }
  if (message.includes('AUDIO_NOT_READY')) {
    return t('public.map.audioNotReady');
  }
  if (message.includes('NARRATION_NOT_AVAILABLE')) {
    return t('public.map.narrationUnavailable');
  }
  return message || t('public.map.narrationUnavailable');
}

export default function MapExploreScreen() {
  const [places, setPlaces] = useState<PlaceDTO[]>([]);
  const [selectedPlace, setSelectedPlace] = useState<PlaceDTO | null>(null);
  const [selectedNarration, setSelectedNarration] =
    useState<NarrationResolveResultDTO | null>(null);
  const [menu, setMenu] = useState<PlaceDishDTO[]>([]);
  const [feedbackOpen, setFeedbackOpen] = useState(false);
  const [statusKey, setStatusKey] = useState('public.map.defaultStatus');
  const [narrationError, setNarrationError] = useState<string | null>(null);
  const [loadingNarration, setLoadingNarration] = useState(false);

  const {
    language,
    setCurrentNarration,
    trackingEnabled,
    setTrackingEnabled
  } = useAppContext();

  const navigate = useNavigate();
  const geo = useGeolocation();
  const { t } = useI18n();

  const openPlayer = useCallback(
    (narration: NarrationResolveResultDTO) => {
      setCurrentNarration(narration);
      navigate('/app/listen');
    },
    [navigate, setCurrentNarration]
  );

  const { handlePosition } = useGeofenceWatcher(openPlayer);

  useEffect(() => {
    getActivePlaces()
      .then(setPlaces)
      .catch((error) => console.error('Load places failed:', error));
  }, []);

  useEffect(() => {
    if (geo.position) void handlePosition(geo.position);
  }, [geo.position, handlePosition]);

  const loadPlaceNarration = useCallback(
    async (placeId: number) => {
      setSelectedNarration(null);
      setNarrationError(null);

      if (!language) return;

      setLoadingNarration(true);
      try {
        setSelectedNarration(
          await resolvePlaceNarration(placeId, language.languageId)
        );
      } catch (error) {
        setNarrationError(
          narrationErrorMessage(
            error instanceof Error ? error.message : '',
            t
          )
        );
      } finally {
        setLoadingNarration(false);
      }
    },
    [language, t]
  );

  const openPlace = useCallback(
    async (place: PlaceDTO) => {
      setSelectedPlace(place);
      setMenu([]);
      setSelectedNarration(null);
      setNarrationError(null);

      const [menuResult] = await Promise.allSettled([
        getPlaceDishes(place.placeId),
        loadPlaceNarration(place.placeId)
      ]);

      setMenu(menuResult.status === 'fulfilled' ? menuResult.value : []);
    },
    [loadPlaceNarration]
  );

  async function listenPlace() {
    if (!selectedPlace || !language) return;

    if (selectedNarration) {
      openPlayer(selectedNarration);
      return;
    }

    await loadPlaceNarration(selectedPlace.placeId);
  }

  async function listenDish(dishId: number) {
    if (!language) return;

    setNarrationError(null);
    setLoadingNarration(true);

    try {
      openPlayer(await resolveDishNarration(dishId, language.languageId));
    } catch (error) {
      setNarrationError(
        narrationErrorMessage(
          error instanceof Error ? error.message : '',
          t
        )
      );
    } finally {
      setLoadingNarration(false);
    }
  }

  function requestLocation() {
    geo.requestCurrentPosition();
    setStatusKey('public.map.locationRequested');
  }

  function toggleTracking() {
    const next = !trackingEnabled;
    setTrackingEnabled(next);

    if (next) {
      geo.startWatching(handlePosition);
      setStatusKey('public.map.trackingStarted');
    } else {
      geo.stopWatching();
      setStatusKey('public.map.trackingStopped');
    }
  }

  const validPlaces = useMemo(
    () =>
      places.filter(
        (place): place is PlaceDTO & {
          latitude: number;
          longitude: number;
        } =>
          place.latitude !== null &&
          place.latitude !== undefined &&
          place.longitude !== null &&
          place.longitude !== undefined
      ),
    [places]
  );

  return (
    <div className="relative h-screen bg-slate-100 pb-20">
      <MapContainer
        center={[defaultLat, defaultLng]}
        zoom={defaultZoom}
        className="h-full w-full"
      >
        <TileLayer
          attribution="&copy; OpenStreetMap"
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        {validPlaces.map((place) => (
          <Marker
            key={place.placeId}
            position={[place.latitude, place.longitude]}
            icon={placeIcon}
            eventHandlers={{ click: () => void openPlace(place) }}
          />
        ))}

        {geo.position && (
          <Marker
            position={[geo.position.latitude, geo.position.longitude]}
            icon={userIcon}
          />
        )}

        <MapAutoFocus
          places={validPlaces}
          selectedPlace={selectedPlace}
          userPosition={geo.position ?? undefined}
        />
      </MapContainer>

      <div className="pointer-events-none absolute left-4 right-4 top-4 z-[700] space-y-3">
        <Card className="pointer-events-auto">
          <div className="flex items-center justify-between gap-3">
            <div>
              <p className="font-bold text-slate-900">
                {t('public.map.title')}
              </p>
              <p className="text-xs text-slate-500">{t(statusKey)}</p>
              <p className="text-xs font-semibold text-teal-700">
                {t('public.map.placesLoaded')}: {validPlaces.length}
              </p>
            </div>

            <Button variant="secondary" onClick={requestLocation}>
              <LocateFixed size={18} />
            </Button>
          </div>

          <Button
            onClick={toggleTracking}
            className="mt-3 w-full"
            variant={trackingEnabled ? 'danger' : 'primary'}
          >
            {trackingEnabled
              ? t('public.map.disableTracking')
              : t('public.map.enableTracking')}
          </Button>
        </Card>
      </div>

      <BottomSheet
        open={Boolean(selectedPlace)}
        onClose={() => {
          setSelectedPlace(null);
          setSelectedNarration(null);
          setNarrationError(null);
        }}
      >
        {selectedPlace && (
          <div className="space-y-4">
            <div className="flex items-start justify-between gap-3 pr-10">
              <div>
                <h2 className="text-xl font-bold text-slate-900">
                  {selectedNarration?.title || selectedPlace.placeName}
                </h2>
                <p className="text-sm text-slate-500">
                  {selectedPlace.address || t('public.map.noAddress')}
                </p>
              </div>

              <Button
                onClick={() => void listenPlace()}
                disabled={!language || loadingNarration || !selectedNarration}
                className="shrink-0"
              >
                <Volume2 size={18} />
              </Button>
            </div>

            {selectedPlace.imageUrl && (
              <img
                src={selectedPlace.imageUrl}
                alt={selectedPlace.placeName}
                className="h-44 w-full rounded-3xl object-cover"
              />
            )}

            <p className="text-sm leading-6 text-slate-700">
              {selectedNarration?.text ||
                selectedPlace.description ||
                t('public.map.noDescription')}
            </p>

            {loadingNarration && (
              <p className="rounded-2xl bg-slate-50 px-4 py-3 text-sm text-slate-600">
                {t('public.map.loadingNarration')}
              </p>
            )}

            {narrationError && (
              <p className="rounded-2xl bg-amber-50 px-4 py-3 text-sm font-semibold text-amber-700">
                {narrationError}
              </p>
            )}

            <div className="rounded-2xl bg-slate-50 p-4 text-sm text-slate-600">
              <p>
                <b>{t('public.map.openingHours')}:</b>{' '}
                {selectedPlace.openingHours || t('public.map.notUpdated')}
              </p>
              <p>
                <b>{t('public.map.geofenceRadius')}:</b>{' '}
                {selectedPlace.triggerRadiusMeters}m
              </p>
            </div>

            <div>
              <h3 className="mb-2 font-bold text-slate-900">
                {t('public.map.dishesTitle')}
              </h3>

              <div className="space-y-2">
                {menu.length === 0 && (
                  <p className="text-sm text-slate-500">
                    {t('public.map.noDishes')}
                  </p>
                )}

                {menu.map((item) => (
                  <div
                    key={item.placeDishId}
                    className="flex items-center justify-between gap-3 rounded-2xl border border-slate-100 p-3 text-sm"
                  >
                    <div>
                      <p className="font-semibold">
                        {item.dish?.dishName ||
                          `${t('public.map.dish')} #${item.dishId}`}{' '}
                        {item.isRecommended
                          ? `• ${t('public.map.recommended')}`
                          : ''}
                      </p>
                      <p className="text-slate-500">
                        {t('public.map.price')}:{' '}
                        {item.price
                          ? `${item.price.toLocaleString()} VND`
                          : t('public.map.notUpdated')}
                      </p>
                      {item.note && (
                        <p className="text-slate-500">{item.note}</p>
                      )}
                    </div>

                    <Button
                      variant="secondary"
                      onClick={() => void listenDish(item.dishId)}
                      disabled={!language || loadingNarration}
                    >
                      <Volume2 size={17} />
                    </Button>
                  </div>
                ))}
              </div>
            </div>

            <Button
              variant="secondary"
              onClick={() => setFeedbackOpen(true)}
              className="w-full"
            >
              {t('public.map.sendFeedback')}
            </Button>
          </div>
        )}
      </BottomSheet>

      <FeedbackModal
        open={feedbackOpen}
        onClose={() => setFeedbackOpen(false)}
        placeId={selectedPlace?.placeId}
      />
    </div>
  );
}
