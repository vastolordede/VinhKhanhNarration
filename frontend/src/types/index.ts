export type ID = number;

export interface ApiResponse<T = unknown> {
  success?: boolean;
  message?: string;
  data?: T;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface LanguageDTO {
  languageId: ID;
  languageCode: string;
  languageName: string;
  locale?: string | null;
  nativeName?: string | null;
  isDefault: boolean;
  isUiEnabled: boolean;
  isContentEnabled: boolean;
  isTranslationSupported: boolean;
  isTtsSupported: boolean;
  defaultVoiceId?: string | null;
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

export type NarrationWorkflowStatus =
  | 'Draft'
  | 'PendingReview'
  | 'Approved'
  | 'Rejected'
  | 'Published'
  | 'HiddenByVendor'
  | 'HiddenByAdmin'
  | 'DeletedByVendor'
  | 'DeletedByAdmin';

export type TranslationStatus =
  | 'Pending'
  | 'Generating'
  | 'PendingReview'
  | 'Approved'
  | 'Rejected'
  | 'Failed'
  | 'Outdated';

export type AudioStatus =
  | 'Pending'
  | 'Generating'
  | 'Ready'
  | 'Failed'
  | 'Outdated';

export interface NarrationContentDTO {
  narrationId: ID;
  title: string;
  originalText: string;
  contentTypeId: ID;
  placeId?: ID | null;
  dishId?: ID | null;
  sourceLanguageId?: ID | null;
  createdBy?: ID | null;
  submittedByVendorId?: ID | null;
  workflowStatus: NarrationWorkflowStatus;
  rejectionReason?: string | null;
  submittedAt?: string | null;
  reviewedBy?: ID | null;
  reviewedAt?: string | null;
  publishedAt?: string | null;
  previousWorkflowStatus?: string | null;
  moderationReason?: string | null;
  moderationByAdminId?: ID | null;
  hiddenAt?: string | null;
  deletedAt?: string | null;
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
  provider?: string | null;
  status: TranslationStatus;
  errorMessage?: string | null;
  reviewedBy?: ID | null;
  reviewedAt?: string | null;
  isReviewed: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface AudioFileDTO {
  audioId: ID;
  translationId: ID;
  audioUrl?: string | null;
  provider?: string | null;
  voiceName?: string | null;
  voiceGender?: string | null;
  durationSeconds?: number | null;
  fileFormat: string;
  generatedBy: string;
  status: AudioStatus;
  errorMessage?: string | null;
  sourceTextHash?: string | null;
  generatedAt?: string | null;
  publishedAt?: string | null;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface PublicNarrationResultDTO {
  placeId?: ID | null;
  dishId?: ID | null;
  narrationId: ID;
  translationId: ID;
  audioId: ID;
  title: string;
  text: string;
  audioUrl: string;
  languageCode: string;
  locale: string;
}

export interface NarrationResolveResultDTO extends PublicNarrationResultDTO {
  geofenceEventId?: ID | null;
  source: 'place' | 'dish' | 'narration' | 'geofence' | 'manual';
}

export interface VendorNarrationRequestDTO {
  vendorUserId?: ID;
  sourceLanguageId: ID;
  title: string;
  originalText: string;
  contentTypeId: ID;
  placeId?: ID | null;
  dishId?: ID | null;
}

export interface VendorNarrationStatusDTO {
  narration: NarrationContentDTO;
  translations: NarrationTranslationDTO[];
  audioFiles: AudioFileDTO[];
}

export interface ReviewNarrationRequestDTO {
  adminId?: ID;
  approved: boolean;
  rejectionReason?: string | null;
}

export interface AutoProcessNarrationResultDTO {
  narrationId: ID;
  translationsCreated: number;
  translationsUpdated: number;
  audioTargetCount: number;
  audioReady: number;
  published: boolean;
  errors: string[];
}

export interface ReviewNarrationResultDTO {
  narrationId: ID;
  approved: boolean;
  processing?: AutoProcessNarrationResultDTO | null;
}

export interface ReviewTranslationRequestDTO {
  adminId: ID;
  approved: boolean;
  rejectionReason?: string | null;
}

export interface GenerateTranslationsResultDTO {
  narrationId: ID;
  created: number;
  updated: number;
  errors: string[];
}

export interface GenerateAudioRequestDTO {
  adminId: ID;
  voiceName?: string | null;
  publishAfterGenerate?: boolean;
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

export interface GeofenceCheckResultDTO {
  shouldPlay: boolean;
  reason: string;
  placeId?: ID | null;
  narrationId?: ID | null;
  translationId?: ID | null;
  audioId?: ID | null;
  geofenceEventId?: ID | null;
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
  geofenceEventId?: ID | null;
  triggerSource: 'Geofence' | 'Manual';
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


export type VendorAccountStatus =
  | 'PendingReview'
  | 'PendingPayment'
  | 'Active'
  | 'ExpiringSoon'
  | 'Expired'
  | 'Rejected'
  | 'Suspended';

export interface VendorAuthUserDTO {
  vendorUserId: ID;
  ownerName: string;
  shopName: string;
  email: string;
  accountStatus: VendorAccountStatus;
  placeId?: ID | null;
}

export interface VendorLoginResponseDTO {
  accessToken: string;
  accessTokenExpiresAt: string;
  vendor: VendorAuthUserDTO;
}

export interface VendorDocumentDTO {
  documentId: ID;
  vendorUserId: ID;
  documentType: 'BusinessLicense' | 'FoodSafety';
  fileName: string;
  fileUrl: string;
  expiresAt?: string | null;
  verificationStatus: string;
  reviewReason?: string | null;
  createdAt: string;
}

export interface VendorSubscriptionDTO {
  subscriptionId: ID;
  vendorUserId: ID;
  paymentOrderId: ID;
  startsAt: string;
  expiresAt: string;
  status: string;
}

export interface PaymentOrderDTO {
  paymentOrderId: ID;
  vendorUserId: ID;
  renewalRequestId?: ID | null;
  purpose: 'Registration' | 'Renewal';
  orderCode: string;
  amount: number;
  status: string;
  provider: string;
  paymentUrl: string;
  qrImageUrl: string;
  createdAt: string;
  expiresAt: string;
  paidAt?: string | null;
}

export interface VendorNotificationDTO {
  notificationId: ID;
  vendorUserId: ID;
  notificationType: string;
  title: string;
  message: string;
  entityType?: string | null;
  entityId?: ID | null;
  isRead: boolean;
  createdAt: string;
  readAt?: string | null;
}

export interface VendorDashboardDTO {
  vendor: VendorAuthUserDTO;
  subscription?: VendorSubscriptionDTO | null;
  pendingPayment?: PaymentOrderDTO | null;
  unreadNotifications: number;
  daysRemaining: number;
  canManageContent: boolean;
  shouldWarnExpiry: boolean;
}

export interface AdminVendorListItemDTO {
  vendorUserId: ID;
  ownerName: string;
  shopName: string;
  email: string;
  phone?: string | null;
  placeId?: ID | null;
  accountStatus: VendorAccountStatus;
  reviewReason?: string | null;
  createdAt: string;
  documents: VendorDocumentDTO[];
  subscription?: VendorSubscriptionDTO | null;
  pendingPayment?: PaymentOrderDTO | null;
}

export interface VendorRenewalRequestDTO {
  renewalRequestId: ID;
  vendorUserId: ID;
  foodSafetyDocumentId: ID;
  status: string;
  reviewReason?: string | null;
  createdAt: string;
}
