import { endpoints } from './endpoints';
import { getApiError, http, unwrap } from './http';
import {
  AdminVendorListItemDTO,
  PaymentOrderDTO,
  VendorDashboardDTO,
  VendorLoginResponseDTO,
  VendorNotificationDTO,
  VendorRenewalRequestDTO
} from '../types';

export async function vendorLogin(
  email: string,
  password: string
): Promise<VendorLoginResponseDTO> {
  try {
    const response = await http.post(endpoints.vendorLogin, { email, password });
    return unwrap<VendorLoginResponseDTO>(response);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function vendorRegister(formData: FormData): Promise<number> {
  try {
    const response = await http.post(endpoints.vendorRegister, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return unwrap<number>(response);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function getVendorDashboard(): Promise<VendorDashboardDTO> {
  const response = await http.get(endpoints.vendorDashboard);
  return unwrap<VendorDashboardDTO>(response);
}

export async function submitVendorRenewal(formData: FormData): Promise<number> {
  try {
    const response = await http.post(endpoints.vendorRenewal, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return unwrap<number>(response);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function confirmMockPayment(
  orderCode: string
): Promise<VendorDashboardDTO> {
  try {
    const response = await http.post(
      `${endpoints.vendorMockPayments}/${encodeURIComponent(orderCode)}/confirm`
    );
    return unwrap<VendorDashboardDTO>(response);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function getVendorNotifications(): Promise<VendorNotificationDTO[]> {
  const response = await http.get(endpoints.vendorNotifications);
  return unwrap<VendorNotificationDTO[]>(response);
}

export async function markVendorNotificationRead(
  notificationId: number
): Promise<void> {
  await http.patch(`${endpoints.vendorNotifications}/${notificationId}/read`);
}

export async function getAdminVendors(): Promise<AdminVendorListItemDTO[]> {
  const response = await http.get(endpoints.adminVendors);
  return unwrap<AdminVendorListItemDTO[]>(response);
}

export async function reviewVendorRegistration(
  vendorUserId: number,
  approved: boolean,
  reason?: string,
  amount = 500000
): Promise<PaymentOrderDTO> {
  const response = await http.patch(
    `${endpoints.adminVendors}/${vendorUserId}/review`,
    { approved, reason: reason || null, amount }
  );
  return unwrap<PaymentOrderDTO>(response);
}

export async function getPendingRenewals(): Promise<VendorRenewalRequestDTO[]> {
  const response = await http.get(`${endpoints.adminVendorRenewals}/pending`);
  return unwrap<VendorRenewalRequestDTO[]>(response);
}

export async function reviewVendorRenewal(
  requestId: number,
  approved: boolean,
  reason?: string,
  amount = 500000
): Promise<PaymentOrderDTO> {
  const response = await http.patch(
    `${endpoints.adminVendorRenewals}/${requestId}/review`,
    { approved, reason: reason || null, amount }
  );
  return unwrap<PaymentOrderDTO>(response);
}

export async function getVendorCatalog() {
  const response = await http.get(endpoints.vendorCatalog);
  return unwrap<import('../types').VendorCatalogDTO>(response);
}

export async function saveVendorPlace(place: import('../types').PlaceDTO) {
  const response = await http.put(`${endpoints.vendorCatalog}/place`, place);
  return unwrap<import('../types').PlaceDTO>(response);
}

export async function createVendorDish(
  request: import('../types').VendorDishRequestDTO
): Promise<number> {
  const response = await http.post(`${endpoints.vendorCatalog}/dishes`, request);
  return unwrap<number>(response);
}

export async function updateVendorDish(
  dishId: number,
  request: import('../types').VendorDishRequestDTO
): Promise<boolean> {
  const response = await http.put(
    `${endpoints.vendorCatalog}/dishes/${dishId}`,
    request
  );
  return unwrap<boolean>(response);
}

export async function setVendorDishActive(
  dishId: number,
  active: boolean
): Promise<boolean> {
  const response = await http.patch(
    `${endpoints.vendorCatalog}/dishes/${dishId}/${active ? 'restore' : 'deactivate'}`
  );
  return unwrap<boolean>(response);
}

export async function removeVendorMenuItem(dishId: number): Promise<boolean> {
  const response = await http.delete(
    `${endpoints.vendorCatalog}/menu/${dishId}`
  );
  return unwrap<boolean>(response);
}

export async function updateVendorProfile(payload: {
  ownerName: string;
  shopName: string;
  phone: string;
}) {
  const response = await http.put(endpoints.vendorProfile, payload);
  return unwrap<import('../types').VendorAuthUserDTO>(response);
}
