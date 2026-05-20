import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import PublicShell from './components/layout/PublicShell';
import AdminShell from './components/layout/AdminShell';
import SplashScreen from './features/public/SplashScreen';
import LanguageSelectionScreen from './features/public/LanguageSelectionScreen';
import MapExploreScreen from './features/public/MapExploreScreen';
import QRScannerScreen from './features/public/QRScannerScreen';
import NarrationPlayerScreen from './features/public/NarrationPlayerScreen';
import SettingsScreen from './features/public/SettingsScreen';
import AdminLoginScreen from './features/admin/AdminLoginScreen';
import AdminDashboardScreen from './features/admin/AdminDashboardScreen';
import LookupManagementScreen from './features/admin/LookupManagementScreen';
import LanguageManagementScreen from './features/admin/LanguageManagementScreen';
import PlacesManagementScreen from './features/admin/PlacesManagementScreen';
import DishesManagementScreen from './features/admin/DishesManagementScreen';
import NarrationManagementScreen from './features/admin/NarrationManagementScreen';
import TranslationManagementScreen from './features/admin/TranslationManagementScreen';
import AudioManagementScreen from './features/admin/AudioManagementScreen';
import QRCodeManagementScreen from './features/admin/QRCodeManagementScreen';
import FeedbackManagementScreen from './features/admin/FeedbackManagementScreen';
import ListeningHistoriesScreen from './features/admin/ListeningHistoriesScreen';
import GeofenceEventsScreen from './features/admin/GeofenceEventsScreen';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<SplashScreen />} />
        <Route path="/app" element={<PublicShell />}>
          <Route index element={<Navigate to="map" replace />} />
          <Route path="language" element={<LanguageSelectionScreen />} />
          <Route path="map" element={<MapExploreScreen />} />
          <Route path="qr" element={<QRScannerScreen />} />
          <Route path="listen" element={<NarrationPlayerScreen />} />
          <Route path="settings" element={<SettingsScreen />} />
        </Route>

        <Route path="/admin/login" element={<AdminLoginScreen />} />
        <Route path="/admin" element={<AdminShell />}>
          <Route index element={<AdminDashboardScreen />} />
          <Route path="lookups" element={<LookupManagementScreen />} />
          <Route path="languages" element={<LanguageManagementScreen />} />
          <Route path="places" element={<PlacesManagementScreen />} />
          <Route path="dishes" element={<DishesManagementScreen />} />
          <Route path="narrations" element={<NarrationManagementScreen />} />
          <Route path="translations" element={<TranslationManagementScreen />} />
          <Route path="audio" element={<AudioManagementScreen />} />
          <Route path="qr-codes" element={<QRCodeManagementScreen />} />
          <Route path="feedbacks" element={<FeedbackManagementScreen />} />
          <Route path="listening-histories" element={<ListeningHistoriesScreen />} />
          <Route path="geofence-events" element={<GeofenceEventsScreen />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
