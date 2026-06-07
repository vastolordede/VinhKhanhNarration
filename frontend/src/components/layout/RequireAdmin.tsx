import { Navigate, Outlet, useLocation } from 'react-router-dom';

function hasAdminSession() {
  const accessToken = localStorage.getItem('adminToken');
  const refreshToken = localStorage.getItem('adminRefreshToken');
  const adminUser = localStorage.getItem('adminUser');

  return Boolean(accessToken && refreshToken && adminUser);
}

export default function RequireAdmin() {
  const location = useLocation();

  if (!hasAdminSession()) {
    return <Navigate to="/admin/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}