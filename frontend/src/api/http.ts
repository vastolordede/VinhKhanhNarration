import axios, {
  AxiosError,
  AxiosInstance,
  InternalAxiosRequestConfig
} from 'axios';
import { normalizeUiLanguage } from '../i18n/translations';

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5151';

type AuthKind = 'admin' | 'vendor' | null;
type RetryConfig = InternalAxiosRequestConfig & { _retry?: boolean };

export const http: AxiosInstance = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' }
});

const refreshClient = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' }
});

function authKindFor(url?: string): AuthKind {
  const path = url || '';
  if (path.startsWith('/api/public/')) return null;
  if (path.startsWith('/api/vendor') || path.startsWith('/api/vendor-auth')) {
    return 'vendor';
  }
  if (
    path.startsWith('/api/admin') ||
    path.startsWith('/api/auth') ||
    path.startsWith('/api/admin-users')
  ) {
    return 'admin';
  }
  if (window.location.pathname.startsWith('/vendor')) return 'vendor';
  if (window.location.pathname.startsWith('/admin')) return 'admin';
  return null;
}

function tokenKey(kind: Exclude<AuthKind, null>) {
  return kind === 'vendor' ? 'vendorToken' : 'adminToken';
}

function refreshKey(kind: Exclude<AuthKind, null>) {
  return kind === 'vendor' ? 'vendorRefreshToken' : 'adminRefreshToken';
}

function clearSession(kind: Exclude<AuthKind, null>) {
  const prefix = kind === 'vendor' ? 'vendor' : 'admin';
  ['Token', 'RefreshToken', 'TokenExpiresAt', 'RefreshTokenExpiresAt', 'User'].forEach(
    (suffix) => localStorage.removeItem(`${prefix}${suffix}`)
  );
}

function saveRefreshedSession(
  kind: Exclude<AuthKind, null>,
  data: {
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: string;
    refreshTokenExpiresAt: string;
    admin?: unknown;
    vendor?: unknown;
  }
) {
  const prefix = kind === 'vendor' ? 'vendor' : 'admin';
  localStorage.setItem(`${prefix}Token`, data.accessToken);
  localStorage.setItem(`${prefix}RefreshToken`, data.refreshToken);
  localStorage.setItem(`${prefix}TokenExpiresAt`, data.accessTokenExpiresAt);
  localStorage.setItem(
    `${prefix}RefreshTokenExpiresAt`,
    data.refreshTokenExpiresAt
  );
  const user = kind === 'vendor' ? data.vendor : data.admin;
  if (user) localStorage.setItem(`${prefix}User`, JSON.stringify(user));
}

http.interceptors.request.use((config) => {
  const kind = authKindFor(config.url);
  const token = kind ? localStorage.getItem(tokenKey(kind)) : null;
  if (token) config.headers.Authorization = `Bearer ${token}`;

  const isAdminRoute = kind === 'admin' || window.location.pathname.startsWith('/admin');
  const savedUiLanguage = isAdminRoute
    ? localStorage.getItem('adminUiLanguage')
    : localStorage.getItem('uiLanguage');
  const savedLanguage = localStorage.getItem('language');
  let languageCode = savedUiLanguage;
  if (!isAdminRoute && !languageCode && savedLanguage) {
    try {
      languageCode = JSON.parse(savedLanguage)?.languageCode;
    } catch {
      languageCode = null;
    }
  }
  config.headers['Accept-Language'] = isAdminRoute
    ? languageCode === 'en' ? 'en' : 'vi'
    : normalizeUiLanguage(languageCode);
  return config;
});

let adminRefreshPromise: Promise<string> | null = null;
let vendorRefreshPromise: Promise<string> | null = null;

async function refreshAccessToken(kind: Exclude<AuthKind, null>): Promise<string> {
  const existing = kind === 'admin' ? adminRefreshPromise : vendorRefreshPromise;
  if (existing) return existing;

  const promise = (async () => {
    const refreshToken = localStorage.getItem(refreshKey(kind));
    if (!refreshToken) throw new Error('Missing refresh token');
    const url = kind === 'vendor' ? '/api/vendor-auth/refresh' : '/api/auth/refresh';
    const response = await refreshClient.post(url, { refreshToken });
    const data = unwrap<{
      accessToken: string;
      refreshToken: string;
      accessTokenExpiresAt: string;
      refreshTokenExpiresAt: string;
      admin?: unknown;
      vendor?: unknown;
    }>(response);
    saveRefreshedSession(kind, data);
    return data.accessToken;
  })();

  if (kind === 'admin') adminRefreshPromise = promise;
  else vendorRefreshPromise = promise;

  try {
    return await promise;
  } finally {
    if (kind === 'admin') adminRefreshPromise = null;
    else vendorRefreshPromise = null;
  }
}

http.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config as RetryConfig | undefined;
    const kind = authKindFor(config?.url);
    const isRefreshCall = config?.url?.includes('/refresh');
    if (error.response?.status !== 401 || !config || config._retry || !kind || isRefreshCall) {
      return Promise.reject(error);
    }

    config._retry = true;
    try {
      const token = await refreshAccessToken(kind);
      config.headers.Authorization = `Bearer ${token}`;
      return http.request(config);
    } catch (refreshError) {
      clearSession(kind);
      const loginPath = kind === 'vendor' ? '/vendor/login' : '/admin/login';
      if (window.location.pathname !== loginPath) window.location.assign(loginPath);
      return Promise.reject(refreshError);
    }
  }
);

export function getApiError(error: unknown): string {
  const axiosError = error as AxiosError<{ message?: string; title?: string }>;
  return (
    axiosError.response?.data?.message ||
    axiosError.response?.data?.title ||
    axiosError.message ||
    'Unexpected API error'
  );
}

export function unwrap<T>(response: { data: unknown }): T {
  const body = response.data as { data?: T } | T;
  if (body && typeof body === 'object' && 'data' in body) {
    return (body as { data: T }).data;
  }
  return body as T;
}
