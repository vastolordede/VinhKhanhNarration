import { endpoints } from './endpoints';
import { http, unwrap } from './http';
import {
  AudioFileDTO,
  FeedbackDTO,
  GeofenceCheckResultDTO,
  GuestSessionDTO,
  LanguageDTO,
  ListeningHistoryDTO,
  NarrationContentDTO,
  NarrationResolveResultDTO,
  NarrationTranslationDTO,
  PlaceDTO,
  PlaceDishDTO,
  QRScanResultDTO
} from '../types';

export async function createGuestSession(deviceInfo?: string): Promise<GuestSessionDTO> {
  const response = await http.post(endpoints.guestSessions, { deviceInfo });
  return unwrap<GuestSessionDTO>(response);
}

export async function getActiveLanguages(): Promise<LanguageDTO[]> {
  const response = await http.get(`${endpoints.languages}/active`);
  return unwrap<LanguageDTO[]>(response);
}

export async function updateGuestLanguage(
  guestSessionId: string,
  languageId: number
): Promise<void> {
  await http.patch(`${endpoints.guestSessions}/${guestSessionId}/language`, {
    languageId
  });
}

export async function getActivePlaces(): Promise<PlaceDTO[]> {
  const response = await http.get(`${endpoints.places}/active`);
  return unwrap<PlaceDTO[]>(response);
}

export async function getPlace(id: number): Promise<PlaceDTO> {
  const response = await http.get(`${endpoints.places}/${id}`);
  return unwrap<PlaceDTO>(response);
}

export async function getPlaceDishes(placeId: number): Promise<PlaceDishDTO[]> {
  const response = await http.get(`${endpoints.placeDishes}/place/${placeId}`);
  return unwrap<PlaceDishDTO[]>(response);
}

export async function getNarrationsByPlace(
  placeId: number
): Promise<NarrationContentDTO[]> {
  const response = await http.get(`${endpoints.narrationContents}/place/${placeId}`);
  return unwrap<NarrationContentDTO[]>(response);
}

export async function getNarrationsByDish(
  dishId: number
): Promise<NarrationContentDTO[]> {
  const response = await http.get(`${endpoints.narrationContents}/dish/${dishId}`);
  return unwrap<NarrationContentDTO[]>(response);
}

export async function getTranslation(
  narrationId: number,
  languageId: number
): Promise<NarrationTranslationDTO | null> {
  try {
    const response = await http.get(
      `${endpoints.narrationTranslations}/narration/${narrationId}/language/${languageId}`
    );

    return unwrap<NarrationTranslationDTO | null>(response) ?? null;
  } catch {
    return null;
  }
}

export async function getPlayableAudio(
  narrationId: number,
  languageId: number
): Promise<AudioFileDTO | null> {
  try {
    const response = await http.get(`${endpoints.audioFiles}/playable`, {
      params: {
        narrationId,
        languageId
      }
    });

    return unwrap<AudioFileDTO | null>(response) ?? null;
  } catch {
    return null;
  }
}

export async function resolvePlaceNarration(
  placeId: number,
  languageId: number
): Promise<NarrationResolveResultDTO> {
  const narrations = await getNarrationsByPlace(placeId);
  const narration = narrations.find((x) => x.isActive) ?? narrations[0];

  if (!narration) {
    throw new Error('Không tìm thấy nội dung thuyết minh cho địa điểm này.');
  }

  const translation = await getTranslation(narration.narrationId, languageId);
  const audio = await getPlayableAudio(narration.narrationId, languageId);

  return {
    placeId,
    dishId: null,
    narrationId: narration.narrationId,
    translationId: translation?.translationId ?? null,
    audioId: audio?.audioId ?? null,
    title: translation?.translatedTitle || narration.title,
    text: translation?.translatedText || narration.originalText,
    audioUrl: audio?.audioUrl ?? null,
    useTts: !audio?.audioUrl,
    source: 'place'
  };
}

export async function resolveDishNarration(
  dishId: number,
  languageId: number
): Promise<NarrationResolveResultDTO> {
  const narrations = await getNarrationsByDish(dishId);
  const narration = narrations.find((x) => x.isActive) ?? narrations[0];

  if (!narration) {
    throw new Error('Không tìm thấy nội dung thuyết minh cho món ăn này.');
  }

  const translation = await getTranslation(narration.narrationId, languageId);
  const audio = await getPlayableAudio(narration.narrationId, languageId);

  return {
    placeId: null,
    dishId,
    narrationId: narration.narrationId,
    translationId: translation?.translationId ?? null,
    audioId: audio?.audioId ?? null,
    title: translation?.translatedTitle || narration.title,
    text: translation?.translatedText || narration.originalText,
    audioUrl: audio?.audioUrl ?? null,
    useTts: !audio?.audioUrl,
    source: 'dish'
  };
}

export async function resolveQr(
  qrCodeValue: string,
  languageId: number,
  guestSessionId: string
): Promise<NarrationResolveResultDTO> {
  const response = await http.post(endpoints.qrResolve, {
    qrCodeValue,
    languageId,
    guestSessionId
  });

  const result = unwrap<QRScanResultDTO>(response);

  return {
    ...result,
    translationId: result.translationId ?? null,
    audioId: result.audioId ?? null,
    audioUrl: result.audioUrl ?? null,
    useTts: !result.audioUrl,
    source: 'qr'
  };
}

export async function checkGeofence(
  guestSessionId: string,
  latitude: number,
  longitude: number,
  languageId: number
): Promise<GeofenceCheckResultDTO> {
  const response = await http.post(endpoints.geofenceCheck, {
    guestSessionId,
    latitude,
    longitude,
    languageId
  });

  return unwrap<GeofenceCheckResultDTO>(response);
}

export async function submitListeningHistory(
  payload: ListeningHistoryDTO
): Promise<void> {
  await http.post(endpoints.listeningHistories, payload);
}

export async function submitFeedback(payload: FeedbackDTO): Promise<void> {
  await http.post(endpoints.feedbacks, payload);
}