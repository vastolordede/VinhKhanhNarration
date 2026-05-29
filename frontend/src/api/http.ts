import axios, { AxiosError, AxiosInstance } from 'axios';
import { normalizeUiLanguage } from '../i18n/translations';

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5151';

export const http: AxiosInstance = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
});

http.interceptors.request.use((config) => {
  const token = localStorage.getItem('adminToken');

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  const savedUiLanguage = localStorage.getItem('uiLanguage');
  const savedLanguage = localStorage.getItem('language');

  let languageCode = savedUiLanguage;

  if (!languageCode && savedLanguage) {
    try {
      languageCode = JSON.parse(savedLanguage)?.languageCode;
    } catch {
      languageCode = null;
    }
  }

  config.headers['Accept-Language'] = normalizeUiLanguage(languageCode);

  return config;
});

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