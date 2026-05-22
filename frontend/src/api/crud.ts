import { http, unwrap } from './http';
import { endpoints } from './endpoints';

export async function getList<T>(url: string): Promise<T[]> {
  const response = await http.get(url);
  return unwrap<T[]>(response);
}

export async function getOne<T>(url: string): Promise<T> {
  const response = await http.get(url);
  return unwrap<T>(response);
}

export async function createItem<T>(url: string, payload: unknown): Promise<T> {
  const response = await http.post(url, payload);
  return unwrap<T>(response);
}

export async function updateItem<T>(url: string, payload: unknown): Promise<T> {
  const response = await http.put(url, payload);
  return unwrap<T>(response);
}

export async function patchItem<T>(url: string, payload?: unknown): Promise<T> {
  const response = await http.patch(url, payload ?? {});
  return unwrap<T>(response);
}

export async function deleteItem<T>(url: string): Promise<T> {
  const response = await http.delete(url);
  return unwrap<T>(response);
}
export type GeocodingResult = {
  displayName: string;
  latitude: number;
  longitude: number;
};

export async function resolveAddress(address: string): Promise<GeocodingResult> {
  const response = await http.post(endpoints.geocodingResolve, {
    address
  });

  return unwrap<GeocodingResult>(response);
}
