import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import PublicShell from './components/layout/PublicShell';
import AdminShell from './components/layout/AdminShell';
import SplashScreen from './features/public/SplashScreen';
import LanguageSelectionScreen from './features/public/LanguageSelectionScreen';
import MapExploreScreen from './features/public/MapExploreScreen';
import NarrationPlayerScreen from './features/public/NarrationPlayerScreen';
import SettingsScreen from './features/public/SettingsScreen';
import GuestAccessScreen from './features/public/GuestAccessScreen';
import AdminLoginScreen from './features/admin/AdminLoginScreen';
import AdminDashboardScreen from './features/admin/AdminDashboardScreen';
import LookupManagementScreen from './features/admin/LookupManagementScreen';
import LanguageManagementScreen from './features/admin/LanguageManagementScreen';
import PlacesManagementScreen from './features/admin/PlacesManagementScreen';
import DishesManagementScreen from './features/admin/DishesManagementScreen';
import NarrationManagementScreen from './features/admin/NarrationManagementScreen';
import TranslationManagementScreen from './features/admin/TranslationManagementScreen';
import AudioManagementScreen from './features/admin/AudioManagementScreen';
import FeedbackManagementScreen from './features/admin/FeedbackManagementScreen';
import ListeningHistoriesScreen from './features/admin/ListeningHistoriesScreen';
import GeofenceEventsScreen from './features/admin/GeofenceEventsScreen';
import VendorNarrationScreen from './features/vendor/VendorNarrationScreen';
import VendorLoginScreen from './features/vendor/VendorLoginScreen';
import VendorRegisterScreen from './features/vendor/VendorRegisterScreen';
import VendorDashboardScreen from './features/vendor/VendorDashboardScreen';
import VendorSubscriptionScreen from './features/vendor/VendorSubscriptionScreen';
import VendorMockPaymentScreen from './features/vendor/VendorMockPaymentScreen';
import VendorNotificationsScreen from './features/vendor/VendorNotificationsScreen';
import VendorShell from './components/layout/VendorShell';
import RequireVendor from './components/layout/RequireVendor';
import VendorManagementScreen from './features/admin/VendorManagementScreen';
import RequireAdmin from './components/layout/RequireAdmin';
import VendorCatalogScreen from './features/vendor/VendorCatalogScreen';
import AuditLogScreen from './features/admin/AuditLogScreen';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<SplashScreen />} />

        <Route path="/app" element={<PublicShell />}>
          <Route index element={<Navigate to="map" replace />} />
          <Route path="language" element={<LanguageSelectionScreen />} />
          <Route path="map" element={<MapExploreScreen />} />
          <Route path="listen" element={<NarrationPlayerScreen />} />
          <Route path="access" element={<GuestAccessScreen />} />
          <Route path="settings" element={<SettingsScreen />} />
        </Route>

        <Route path="/vendor/login" element={<VendorLoginScreen />} />
        <Route path="/vendor/register" element={<VendorRegisterScreen />} />
        <Route element={<RequireVendor />}>
          <Route path="/vendor" element={<VendorShell />}>
            <Route index element={<VendorDashboardScreen />} />
            <Route path="catalog" element={<VendorCatalogScreen />} />
            <Route path="narrations" element={<VendorNarrationScreen />} />
            <Route path="subscription" element={<VendorSubscriptionScreen />} />
            <Route path="mock-payment" element={<VendorMockPaymentScreen />} />
            <Route path="notifications" element={<VendorNotificationsScreen />} />
          </Route>
        </Route>

        <Route path="/admin/login" element={<AdminLoginScreen />} />
        <Route element={<RequireAdmin />}>
          <Route path="/admin" element={<AdminShell />}>
            <Route index element={<AdminDashboardScreen />} />
            <Route path="lookups" element={<LookupManagementScreen />} />
            <Route path="languages" element={<LanguageManagementScreen />} />
            <Route path="places" element={<PlacesManagementScreen />} />
            <Route path="dishes" element={<DishesManagementScreen />} />
            <Route path="vendors" element={<VendorManagementScreen />} />
            <Route path="narrations" element={<NarrationManagementScreen />} />
            <Route path="translations" element={<TranslationManagementScreen />} />
            <Route path="audio" element={<AudioManagementScreen />} />
            <Route path="feedbacks" element={<FeedbackManagementScreen />} />
            <Route path="listening-histories" element={<ListeningHistoriesScreen />} />
            <Route path="geofence-events" element={<GeofenceEventsScreen />} />
            <Route path="audit-logs" element={<AuditLogScreen />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
