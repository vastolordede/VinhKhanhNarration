import { ReactElement } from 'react';
import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppProvider } from '../contexts/AppContext';

export function renderWithApp(ui: ReactElement, initialEntries: string[] = ['/app/map']) {
  return render(
    <MemoryRouter initialEntries={initialEntries}>
      <AppProvider>{ui}</AppProvider>
    </MemoryRouter>
  );
}

export function seedGuestSession() {
  localStorage.setItem(
    'guestSession',
    JSON.stringify({
      guestSessionId: 'GUEST-TEST-001',
      preferredLanguageId: 1,
      deviceInfo: 'vitest',
      isActive: true
    })
  );
}

export function seedLanguage() {
  localStorage.setItem(
    'language',
    JSON.stringify({
      languageId: 1,
      languageCode: 'vi',
      languageName: 'Tiếng Việt',
      isDefault: true,
      isActive: true
    })
  );
}

export function seedCurrentNarration() {
  sessionStorage.setItem(
    'currentNarration',
    JSON.stringify({
      placeId: 1,
      dishId: null,
      narrationId: 10,
      translationId: 20,
      audioId: 30,
      title: 'Giới thiệu Quán Ốc Vĩnh Khánh',
      text: 'Đây là nội dung thuyết minh dùng cho test.',
      audioUrl: null,
      useTts: true,
      source: 'place'
    })
  );
}
