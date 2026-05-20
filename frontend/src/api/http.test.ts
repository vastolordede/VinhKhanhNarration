import { describe, expect, it } from 'vitest';
import { unwrap, getApiError } from './http';

describe('http helpers', () => {
  it('unwrap should return data property when ApiResponse wrapper is used', () => {
    const result = unwrap<{ name: string }>({
      data: {
        success: true,
        message: 'Success',
        data: { name: 'Vĩnh Khánh' }
      }
    });

    expect(result.name).toBe('Vĩnh Khánh');
  });

  it('unwrap should return body directly when response is not wrapped', () => {
    const result = unwrap<{ name: string }>({ data: { name: 'Direct' } });

    expect(result.name).toBe('Direct');
  });

  it('getApiError should fallback to error message', () => {
    const message = getApiError(new Error('Network error'));

    expect(message).toBe('Network error');
  });
});
