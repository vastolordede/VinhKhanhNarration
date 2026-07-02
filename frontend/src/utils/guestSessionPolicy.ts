import axios from 'axios';

type ApiErrorBody = {
  message?: string;
  title?: string;
};

/**
 * Chỉ xóa Guest Session khi backend xác nhận chắc chắn session không còn hợp lệ.
 * Lỗi mạng, Render cold start, timeout hoặc HTTP 5xx không được xóa localStorage.
 */
export function isDefinitiveGuestSessionFailure(error: unknown): boolean {
  if (!axios.isAxiosError<ApiErrorBody>(error)) return false;

  const status = error.response?.status;
  if (status === 401 || status === 403 || status === 404 || status === 410) {
    return true;
  }

  // Backend hiện tại có một số endpoint gom lỗi nghiệp vụ về HTTP 400.
  // Chỉ xem 400 là session hỏng khi message nói rõ về session.
  if (status === 400) {
    const message = `${error.response?.data?.message ?? ''} ${
      error.response?.data?.title ?? ''
    }`.toLowerCase();

    return (
      message.includes('guest session không tồn tại') ||
      message.includes('guest session đã hết hạn') ||
      message.includes('guest session is expired') ||
      message.includes('guest session does not exist')
    );
  }

  return false;
}
