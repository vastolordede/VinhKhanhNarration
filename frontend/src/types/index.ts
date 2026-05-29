export type ID = number;

export interface ApiResponse<T = unknown> {
  success?: boolean;
  message?: string;
  data?: T;
}

export interface LanguageDTO {
  languageId: ID;
  languageCode: string;
  languageName: string;
  isDefault: boolean;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface LookupDTO {
  id?: ID;
  placeTypeId?: ID;
  contentTypeId?: ID;
  targetTypeId?: ID;
  translationSourceId?: ID;
  triggerModeId?: ID;
  eventTypeId?: ID;
  eventStatusId?: ID;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface PlaceDTO {
  placeId: ID;
  placeName: string;
  placeTypeId: ID;
  address?: string;
  description?: string;
  latitude?: number | null;
  longitude?: number | null;
  openingHours?: string;
  imageUrl?: string;
  isPoi: boolean;
  isGeofenceEnabled: boolean;
  triggerRadiusMeters: number;
  priority: number;
  triggerModeId: ID;
  debounceSeconds: number;
  cooldownSeconds: number;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface DishCategoryDTO {
  categoryId: ID;
  categoryName: string;
  description?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface DishDTO {
  dishId: ID;
  dishName: string;
  categoryId: ID;
  description?: string;
  imageUrl?: string;
  averagePrice?: number | null;
  isSignatureDish: boolean;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface PlaceDishDTO {
  placeDishId: ID;
  placeId: ID;
  dishId: ID;
  price?: number | null;
  isRecommended: boolean;
  note?: string;
  createdAt?: string;
  updatedAt?: string;
  place?: PlaceDTO;
  dish?: DishDTO;
}

export interface NarrationContentDTO {
  narrationId: ID;
  title: string;
  originalText: string;
  contentTypeId: ID;
  placeId?: ID | null;
  dishId?: ID | null;
  createdBy: ID;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface NarrationTranslationDTO {
  translationId: ID;
  narrationId: ID;
  languageId: ID;
  translatedTitle: string;
  translatedText: string;
  translationSourceId: ID;
  reviewedBy?: ID | null;
  isReviewed: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface AudioFileDTO {
  audioId: ID;
  translationId: ID;
  audioUrl?: string | null;
  voiceName?: string | null;
  voiceGender?: string | null;
  durationSeconds?: number | null;
  fileFormat?: string;
  generatedBy?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface QRCodeDTO {
  qrCodeId: ID;
  qrCodeValue: string;
  qrCodeImageUrl?: string;
  targetTypeId: ID;
  placeId?: ID | null;
  dishId?: ID | null;
  narrationId?: ID | null;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface GuestSessionDTO {
  guestSessionId: string;
  preferredLanguageId?: ID | null;
  deviceInfo?: string;
  ipAddress?: string;
  createdAt?: string;
  lastSeenAt?: string;
  isActive: boolean;
}

export interface QRScanResultDTO {
  placeId?: ID | null;
  dishId?: ID | null;
  narrationId: ID;
  translationId?: ID | null;
  audioId?: ID | null;
  title: string;
  text: string;
  audioUrl?: string | null;
}

export interface NarrationResolveResultDTO extends QRScanResultDTO {
  useTts: boolean;
  source: 'place' | 'dish' | 'narration' | 'qr' | 'geofence' | 'manual';
}

export interface GeofenceCheckResultDTO {
  shouldPlay: boolean;
  reason: string;
  placeId?: ID | null;
  narrationId?: ID | null;
  translationId?: ID | null;
  audioId?: ID | null;
  audioUrl?: string | null;
  title?: string | null;
  text?: string | null;
  distanceMeters?: number | null;
}

export interface FeedbackDTO {
  feedbackId?: ID;
  guestSessionId?: string | null;
  placeId?: ID | null;
  dishId?: ID | null;
  narrationId?: ID | null;
  rating: number;
  comment?: string;
  isApproved?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface ListeningHistoryDTO {
  historyId?: ID;
  guestSessionId?: string | null;
  narrationId: ID;
  languageId: ID;
  audioId?: ID | null;
  qrCodeId?: ID | null;
  geofenceEventId?: ID | null;
  triggerSource: 'QR' | 'Geofence' | 'Manual';
  playbackStatus: 'Played' | 'Skipped' | 'Stopped' | 'Completed';
  listenedAt?: string;
  deviceInfo?: string;
  ipAddress?: string;
  listenDurationSeconds?: number | null;
}

export interface GeofenceEventDTO {
  eventId: ID;
  guestSessionId: string;
  placeId: ID;
  narrationId?: ID | null;
  eventTypeId: ID;
  eventStatusId: ID;
  userLatitude?: number;
  userLongitude?: number;
  distanceMeters?: number;
  detectedAt?: string;
  processedAt?: string;
  note?: string;
}
