import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { DivIcon } from 'leaflet';
import { MapContainer, Marker, TileLayer, useMap } from 'react-leaflet';
import { LocateFixed, Volume2 } from 'lucide-react';
import { getActivePlaces, getPlaceDishes, resolvePlaceNarration } from '../../api/publicApi';
import { BottomSheet } from '../../components/ui/BottomSheet';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { useAppContext } from '../../contexts/AppContext';
import { useGeofenceWatcher } from '../../hooks/useGeofenceWatcher';
import { useGeolocation } from '../../hooks/useGeolocation';
import { PlaceDTO, PlaceDishDTO } from '../../types';
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

function MapFocus({ lat, lng }: { lat?: number; lng?: number }) {
  const map = useMap();
  useEffect(() => {
    if (lat === undefined || lng === undefined) return;

map.setView([lat, lng], 17);
  }, [lat, lng, map]);
  return null;
}

export default function MapExploreScreen() {
  const [places, setPlaces] = useState<PlaceDTO[]>([]);
  const [selectedPlace, setSelectedPlace] = useState<PlaceDTO | null>(null);
  const [menu, setMenu] = useState<PlaceDishDTO[]>([]);
  const [feedbackOpen, setFeedbackOpen] = useState(false);
  const [status, setStatus] = useState('Mặc định hiển thị khu vực phố ẩm thực Vĩnh Khánh.');
  const { language, setCurrentNarration, trackingEnabled, setTrackingEnabled } = useAppContext();
  const navigate = useNavigate();
  const geo = useGeolocation();
  const { handlePosition } = useGeofenceWatcher((narration) => {
    setCurrentNarration(narration);
    navigate('/app/listen');
  });

  useEffect(() => { getActivePlaces().then(setPlaces); }, []);
  useEffect(() => { if (geo.position) handlePosition(geo.position); }, [geo.position, handlePosition]);

  const openPlace = useCallback(async (place: PlaceDTO) => {
    setSelectedPlace(place);
    try { setMenu(await getPlaceDishes(place.placeId)); } catch { setMenu([]); }
  }, []);

  async function listenPlace() {
    if (!selectedPlace || !language) return;
    const result = await resolvePlaceNarration(selectedPlace.placeId, language.languageId);
    setCurrentNarration(result);
    navigate('/app/listen');
  }

  function requestLocation() {
    geo.requestCurrentPosition();
    setStatus('Đã xin quyền vị trí. Nếu trình duyệt cho phép, bản đồ sẽ focus theo vị trí của bạn.');
  }

  function toggleTracking() {
    const next = !trackingEnabled;
    setTrackingEnabled(next);
    if (next) {
      geo.startWatching(handlePosition);
      setStatus('Đang theo dõi vị trí và gửi geofence check theo thời gian thực.');
    } else {
      geo.stopWatching();
      setStatus('Đã tắt theo dõi vị trí.');
    }
  }

  const validPlaces = useMemo(
  () =>
    places.filter(
      (p): p is PlaceDTO & { latitude: number; longitude: number } =>
        p.latitude !== null &&
        p.latitude !== undefined &&
        p.longitude !== null &&
        p.longitude !== undefined
    ),
  [places]
);
const focusLat =
  geo.position?.latitude ??
  selectedPlace?.latitude ??
  validPlaces[0]?.latitude ??
  undefined;

const focusLng =
  geo.position?.longitude ??
  selectedPlace?.longitude ??
  validPlaces[0]?.longitude ??
  undefined;
  return (
    <div className="relative h-screen bg-slate-100 pb-20">
      <MapContainer center={[defaultLat, defaultLng]} zoom={defaultZoom} className="h-full w-full">
        <TileLayer attribution="&copy; OpenStreetMap" url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        {validPlaces.map((place) => (
          <Marker key={place.placeId} position={[Number(place.latitude), Number(place.longitude)]} icon={placeIcon} eventHandlers={{ click: () => openPlace(place) }} />
        ))}
        {geo.position && <Marker position={[geo.position.latitude, geo.position.longitude]} icon={userIcon} />}
        <MapFocus lat={focusLat} lng={focusLng} />
      </MapContainer>

      <div className="pointer-events-none absolute left-4 right-4 top-4 z-[700] space-y-3">
        <Card className="pointer-events-auto">
          <div className="flex items-center justify-between gap-3">
            <div>
              <p className="font-bold text-slate-900">Bản đồ phố ẩm thực</p>
              <p className="text-xs text-slate-500">{status}</p>
              <p className="text-xs font-semibold text-teal-700">
  Places loaded: {validPlaces.length}
</p>
            </div>
            <Button variant="secondary" onClick={requestLocation}><LocateFixed size={18} /></Button>
          </div>
          <Button onClick={toggleTracking} className="mt-3 w-full" variant={trackingEnabled ? 'danger' : 'primary'}>
            {trackingEnabled ? 'Tắt theo dõi vị trí' : 'Bật theo dõi vị trí'}
          </Button>
        </Card>
      </div>

      <BottomSheet open={!!selectedPlace} onClose={() => setSelectedPlace(null)}>
        {selectedPlace && (
          <div className="space-y-4">
            <div className="flex items-start justify-between gap-3 pr-10">
              <div>
                <h2 className="text-xl font-bold text-slate-900">{selectedPlace.placeName}</h2>
                <p className="text-sm text-slate-500">{selectedPlace.address || 'Chưa có địa chỉ'}</p>
              </div>
              <Button onClick={listenPlace} className="shrink-0"><Volume2 size={18} /></Button>
            </div>
            {selectedPlace.imageUrl && <img src={selectedPlace.imageUrl} alt={selectedPlace.placeName} className="h-44 w-full rounded-3xl object-cover" />}
            <p className="text-sm leading-6 text-slate-700">{selectedPlace.description || 'Chưa có mô tả cho địa điểm này.'}</p>
            <div className="rounded-2xl bg-slate-50 p-4 text-sm text-slate-600">
              <p><b>Giờ mở cửa:</b> {selectedPlace.openingHours || 'Chưa cập nhật'}</p>
              <p><b>Bán kính geofence:</b> {selectedPlace.triggerRadiusMeters}m</p>
            </div>
            <div>
              <h3 className="mb-2 font-bold text-slate-900">Món ăn / bảng giá</h3>
              <div className="space-y-2">
                {menu.length === 0 && <p className="text-sm text-slate-500">Chưa có món ăn cho quán này.</p>}
                {menu.map((item) => (
                  <div key={item.placeDishId} className="rounded-2xl border border-slate-100 p-3 text-sm">
                    <p className="font-semibold">Món #{item.dishId} {item.isRecommended ? '• Gợi ý' : ''}</p>
                    <p className="text-slate-500">Giá: {item.price ? `${item.price.toLocaleString()} VND` : 'Chưa cập nhật'}</p>
                    {item.note && <p className="text-slate-500">{item.note}</p>}
                  </div>
                ))}
              </div>
            </div>
            <Button variant="secondary" onClick={() => setFeedbackOpen(true)} className="w-full">Gửi feedback cho quán</Button>
          </div>
        )}
      </BottomSheet>

      <FeedbackModal open={feedbackOpen} onClose={() => setFeedbackOpen(false)} placeId={selectedPlace?.placeId} />
    </div>
  );
}
