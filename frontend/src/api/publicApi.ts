import { endpoints } from './endpoints';
import { getApiError, http, unwrap } from './http';
import {
  FeedbackDTO,
  GeofenceCheckResultDTO,
  GuestSessionDTO,
  LanguageDTO,
  ListeningHistoryDTO,
  NarrationResolveResultDTO,
  PlaceDTO,
  PlaceDishDTO,
  PublicNarrationResultDTO
} from '../types';

export async function createGuestSession(
  deviceInfo?: string
): Promise<GuestSessionDTO> {
  const response = await http.post(endpoints.guestSessions, { deviceInfo });
  return unwrap<GuestSessionDTO>(response);
}

export async function getActiveLanguages(): Promise<LanguageDTO[]> {
  const response = await http.get(`${endpoints.languages}/active`);
  return unwrap<LanguageDTO[]>(response).filter(
    (language) => language.isActive && language.isContentEnabled
  );
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

function toResolveResult(
  result: PublicNarrationResultDTO,
  source: NarrationResolveResultDTO['source']
): NarrationResolveResultDTO {
  return {
    ...result,
    source
  };
}

export async function resolvePlaceNarration(
  placeId: number,
  languageId: number
): Promise<NarrationResolveResultDTO> {
  try {
    const response = await http.get(
      `${endpoints.publicNarrations}/place/${placeId}`,
      { params: { languageId } }
    );

    return toResolveResult(
      unwrap<PublicNarrationResultDTO>(response),
      'place'
    );
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function resolveDishNarration(
  dishId: number,
  languageId: number
): Promise<NarrationResolveResultDTO> {
  try {
    const response = await http.get(
      `${endpoints.publicNarrations}/dish/${dishId}`,
      { params: { languageId } }
    );

    return toResolveResult(
      unwrap<PublicNarrationResultDTO>(response),
      'dish'
    );
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function resolveNarration(
  narrationId: number,
  languageId: number
): Promise<NarrationResolveResultDTO> {
  try {
    const response = await http.get(
      `${endpoints.publicNarrations}/${narrationId}`,
      { params: { languageId } }
    );

    return toResolveResult(
      unwrap<PublicNarrationResultDTO>(response),
      'narration'
    );
  } catch (error) {
    throw new Error(getApiError(error));
  }
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

export async function createListeningHistory(
  payload: ListeningHistoryDTO
): Promise<number> {
  const response = await http.post(endpoints.listeningHistories, payload);
  return unwrap<number>(response);
}

export async function updateListeningStatus(
  historyId: number,
  status: ListeningHistoryDTO['playbackStatus']
): Promise<void> {
  await http.patch(`${endpoints.listeningHistories}/${historyId}/status`, {
    status
  });
}

export async function updateListeningDuration(
  historyId: number,
  seconds: number
): Promise<void> {
  await http.patch(`${endpoints.listeningHistories}/${historyId}/duration`, {
    seconds: Math.max(0, Math.floor(seconds))
  });
}

export async function submitFeedback(payload: FeedbackDTO): Promise<void> {
  await http.post(endpoints.feedbacks, payload);
}
