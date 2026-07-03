import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { DivIcon, latLngBounds } from 'leaflet';
import { MapContainer, Marker, TileLayer, useMap } from 'react-leaflet';
import { LocateFixed, Navigation, Search, Volume2, X } from 'lucide-react';
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
import { hasUsableAccessPass } from '../../utils/accessPolicy';
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

type MappablePlace = PlaceDTO & {
  latitude: number;
  longitude: number;
};

function normalizePlaceName(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLocaleLowerCase()
    .trim();
}

function toRadians(value: number) {
  return (value * Math.PI) / 180;
}

function calculateDistanceMeters(
  from: { latitude: number; longitude: number },
  to: { latitude: number; longitude: number }
) {
  const earthRadiusMeters = 6_371_000;
  const latitudeDelta = toRadians(to.latitude - from.latitude);
  const longitudeDelta = toRadians(to.longitude - from.longitude);
  const fromLatitude = toRadians(from.latitude);
  const toLatitude = toRadians(to.latitude);

  const haversine =
    Math.sin(latitudeDelta / 2) ** 2 +
    Math.cos(fromLatitude) *
      Math.cos(toLatitude) *
      Math.sin(longitudeDelta / 2) ** 2;

  return (
    2 *
    earthRadiusMeters *
    Math.atan2(Math.sqrt(haversine), Math.sqrt(1 - haversine))
  );
}

function formatDistance(distanceMeters: number) {
  if (distanceMeters < 1000) {
    return `${Math.max(1, Math.round(distanceMeters))} m`;
  }

  const digits = distanceMeters < 10_000 ? 1 : 0;
  return `${(distanceMeters / 1000).toFixed(digits)} km`;
}

function MapAutoFocus({
  places,
  selectedPlace,
  userPosition,
  filterActive
}: {
  places: MappablePlace[];
  selectedPlace: PlaceDTO | null;
  userPosition?: { latitude: number; longitude: number };
  filterActive: boolean;
}) {
  const map = useMap();

  useEffect(() => {
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

    if (filterActive && places.length === 1) {
      map.setView([places[0].latitude, places[0].longitude], 17, {
        animate: true
      });
      return;
    }

    if (filterActive && places.length > 1) {
      const bounds = latLngBounds(
        places.map((place) => [place.latitude, place.longitude])
      );

      map.fitBounds(bounds, {
        paddingTopLeft: [40, 210],
        paddingBottomRight: [40, 120],
        maxZoom: 17,
        animate: true
      });
      return;
    }

    if (userPosition) {
      map.setView([userPosition.latitude, userPosition.longitude], 17, {
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
        paddingTopLeft: [40, 210],
        paddingBottomRight: [40, 120],
        maxZoom: 17,
        animate: true
      });
    }
  }, [filterActive, map, places, selectedPlace, userPosition]);

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
  const [placeQuery, setPlaceQuery] = useState('');

  const {
    guestSession,
    language,
    accessStatus,
    setCurrentNarration,
    trackingEnabled,
    setTrackingEnabled
  } = useAppContext();

  const navigate = useNavigate();
  const geo = useGeolocation();
  const { t } = useI18n();
  const hasAccessPass = hasUsableAccessPass(accessStatus);

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

      if (!language || !guestSession?.guestSessionId || !hasAccessPass) return;

      setLoadingNarration(true);
      try {
        setSelectedNarration(
          await resolvePlaceNarration(
            placeId,
            language.languageId,
            guestSession.guestSessionId
          )
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
    [language, guestSession?.guestSessionId, hasAccessPass, t]
  );

  const openPlace = useCallback(
    async (place: PlaceDTO) => {
      setSelectedPlace(place);
      setMenu([]);
      setSelectedNarration(null);
      setNarrationError(null);

      const [menuResult] = await Promise.allSettled([
        getPlaceDishes(place.placeId)
      ]);
      if (hasAccessPass) void loadPlaceNarration(place.placeId);

      setMenu(menuResult.status === 'fulfilled' ? menuResult.value : []);
    },
    [loadPlaceNarration, hasAccessPass]
  );

  async function listenPlace() {
    if (!hasAccessPass) {
      navigate('/app/access');
      return;
    }
    if (!selectedPlace || !language || !guestSession?.guestSessionId) return;

    if (selectedNarration) {
      openPlayer(selectedNarration);
      return;
    }

    await loadPlaceNarration(selectedPlace.placeId);
  }

  async function listenDish(dishId: number) {
    if (!hasAccessPass) {
      navigate('/app/access');
      return;
    }
    if (!language || !guestSession?.guestSessionId) return;

    setNarrationError(null);
    setLoadingNarration(true);

    try {
      openPlayer(await resolveDishNarration(
        dishId,
        language.languageId,
        guestSession.guestSessionId
      ));
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
    if (!trackingEnabled && !hasAccessPass) {
      navigate('/app/access');
      return;
    }
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
        (place): place is MappablePlace =>
          place.latitude !== null &&
          place.latitude !== undefined &&
          place.longitude !== null &&
          place.longitude !== undefined
      ),
    [places]
  );

  const normalizedPlaceQuery = useMemo(
    () => normalizePlaceName(placeQuery),
    [placeQuery]
  );

  const filteredPlaces = useMemo(
    () =>
      normalizedPlaceQuery
        ? validPlaces.filter((place) =>
            normalizePlaceName(place.placeName).includes(normalizedPlaceQuery)
          )
        : validPlaces,
    [normalizedPlaceQuery, validPlaces]
  );

  const nearestPlace = useMemo(() => {
    if (!geo.position || filteredPlaces.length === 0) return null;

    return filteredPlaces.reduce<{
      place: MappablePlace;
      distanceMeters: number;
    } | null>((nearest, place) => {
      const distanceMeters = calculateDistanceMeters(geo.position!, place);

      if (!nearest || distanceMeters < nearest.distanceMeters) {
        return { place, distanceMeters };
      }

      return nearest;
    }, null);
  }, [filteredPlaces, geo.position]);

  const selectedPlaceDistance = useMemo(() => {
    if (
      !geo.position ||
      selectedPlace?.latitude === null ||
      selectedPlace?.latitude === undefined ||
      selectedPlace?.longitude === null ||
      selectedPlace?.longitude === undefined
    ) {
      return null;
    }

    return calculateDistanceMeters(geo.position, {
      latitude: selectedPlace.latitude,
      longitude: selectedPlace.longitude
    });
  }, [geo.position, selectedPlace]);

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

        {filteredPlaces.map((place) => (
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
          places={filteredPlaces}
          selectedPlace={selectedPlace}
          userPosition={geo.position ?? undefined}
          filterActive={Boolean(normalizedPlaceQuery)}
        />
      </MapContainer>

      <div className="pointer-events-none absolute left-4 right-4 top-4 z-[700] space-y-3">
        <Card className="pointer-events-auto">
          <div className="flex items-start justify-between gap-3">
            <div className="min-w-0">
              <p className="font-bold text-slate-900">
                {t('public.map.title')}
              </p>
              <p className="text-xs text-slate-500">{t(statusKey)}</p>
              <p className="text-xs font-semibold text-teal-700">
                {t('public.map.placesVisible')}: {filteredPlaces.length}/
                {validPlaces.length}
              </p>
              <button
                className={`mt-1 text-left text-xs font-bold ${
                  hasAccessPass ? 'text-emerald-600' : 'text-amber-600'
                }`}
                onClick={() => navigate('/app/access')}
              >
                {hasAccessPass
                  ? `Access Pass: ${(accessStatus?.hoursRemaining ?? 0)}h`
                  : 'Mua Access Pass để nghe'}
              </button>
            </div>

            <Button variant="secondary" onClick={requestLocation}>
              <LocateFixed size={18} />
            </Button>
          </div>

          <div className="relative mt-3">
            <Search
              size={17}
              className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            />
            <input
              type="text"
inputMode="search"
              value={placeQuery}
              onChange={(event) => setPlaceQuery(event.target.value)}
              placeholder={t('public.map.searchPlaceholder')}
              aria-label={t('public.map.searchPlaceholder')}
              className="h-11 w-full rounded-2xl border border-slate-200 bg-white pl-10 pr-10 text-sm text-slate-800 outline-none transition focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
            />
            {placeQuery && (
              <button
                type="button"
                onClick={() => setPlaceQuery('')}
                title={t('public.map.clearSearch')}
                aria-label={t('public.map.clearSearch')}
                className="absolute right-2 top-1/2 grid h-8 w-8 -translate-y-1/2 place-items-center rounded-full text-slate-500 hover:bg-slate-100"
              >
                <X size={16} />
              </button>
            )}
          </div>

          {normalizedPlaceQuery && filteredPlaces.length === 0 && (
            <p className="mt-2 text-xs font-semibold text-amber-700">
              {t('public.map.noSearchResults')}
            </p>
          )}

          {nearestPlace ? (
            <button
              type="button"
              onClick={() => void openPlace(nearestPlace.place)}
              className="mt-3 flex w-full items-center gap-3 rounded-2xl bg-teal-50 px-3 py-2 text-left transition hover:bg-teal-100"
            >
              <Navigation size={18} className="shrink-0 text-teal-700" />
              <span className="min-w-0">
                <span className="block truncate text-xs font-bold text-teal-900">
                  {t('public.map.nearestPoi')}: {nearestPlace.place.placeName}
                </span>
                <span className="block text-xs text-teal-700">
                  {t('public.map.distance')}: {formatDistance(nearestPlace.distanceMeters)}
                </span>
              </span>
            </button>
          ) : (
            <button
              type="button"
              onClick={requestLocation}
              className="mt-3 w-full rounded-2xl bg-slate-50 px-3 py-2 text-left text-xs text-slate-600"
            >
              {t('public.map.locationDistanceHint')}
            </button>
          )}

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
                  {selectedPlace.placeName}
                </h2>
                <p className="text-sm text-slate-500">
                  {selectedPlace.address || t('public.map.noAddress')}
                </p>
              </div>

              <Button
                onClick={() => void listenPlace()}
                disabled={!language || loadingNarration}
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
              {selectedPlaceDistance !== null && (
                <p>
                  <b>{t('public.map.distanceFromYou')}:</b>{' '}
                  {formatDistance(selectedPlaceDistance)}
                </p>
              )}
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
                        {item.dishName || item.dish?.dishName ||
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
