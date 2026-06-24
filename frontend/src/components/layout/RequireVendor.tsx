import { Navigate, Outlet, useLocation } from 'react-router-dom';

export default function RequireVendor() {
  const location = useLocation();
  const token = localStorage.getItem('vendorToken');
  const refreshToken = localStorage.getItem('vendorRefreshToken');
  const user = localStorage.getItem('vendorUser');

  if (!token || !refreshToken || !user) {
    return <Navigate to="/vendor/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}
