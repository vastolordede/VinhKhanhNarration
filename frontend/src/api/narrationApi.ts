import { endpoints } from './endpoints';
import { getApiError, http, unwrap } from './http';
import {
  AudioFileDTO,
  AutoProcessNarrationResultDTO,
  GenerateAudioRequestDTO,
  GenerateTranslationsResultDTO,
  NarrationContentDTO,
  NarrationTranslationDTO,
  ReviewNarrationRequestDTO,
  ReviewNarrationResultDTO,
  ReviewTranslationRequestDTO,
  VendorNarrationRequestDTO,
  VendorNarrationStatusDTO
} from '../types';

export async function getNarrations(): Promise<NarrationContentDTO[]> {
  const response = await http.get(endpoints.narrationContents);
  return unwrap<NarrationContentDTO[]>(response);
}

export async function createNarration(
  payload: NarrationContentDTO
): Promise<number> {
  const response = await http.post(endpoints.narrationContents, payload);
  return unwrap<number>(response);
}

export async function updateNarration(
  narrationId: number,
  payload: NarrationContentDTO
): Promise<void> {
  await http.put(`${endpoints.narrationContents}/${narrationId}`, payload);
}

export async function reviewNarration(
  narrationId: number,
  payload: ReviewNarrationRequestDTO
): Promise<ReviewNarrationResultDTO> {
  const response = await http.patch(
    `${endpoints.narrationContents}/${narrationId}/review`,
    payload
  );
  return unwrap<ReviewNarrationResultDTO>(response);
}

export async function processNarrationPipeline(
  narrationId: number,
  adminId: number
): Promise<AutoProcessNarrationResultDTO> {
  const response = await http.post(
    `${endpoints.narrationContents}/${narrationId}/process-pipeline`,
    { adminId }
  );
  return unwrap<AutoProcessNarrationResultDTO>(response);
}

export async function generateTranslations(
  narrationId: number,
  sourceLanguageCode?: string
): Promise<GenerateTranslationsResultDTO> {
  const response = await http.post(
    `${endpoints.narrationContents}/${narrationId}/generate-translations`,
    { sourceLanguageCode: sourceLanguageCode || null }
  );
  return unwrap<GenerateTranslationsResultDTO>(response);
}

export async function publishNarration(
  narrationId: number,
  adminId: number
): Promise<void> {
  await http.patch(
    `${endpoints.narrationContents}/${narrationId}/publish`,
    { adminId }
  );
}

export async function getTranslations(): Promise<NarrationTranslationDTO[]> {
  const response = await http.get(endpoints.narrationTranslations);
  return unwrap<NarrationTranslationDTO[]>(response);
}

export async function getTranslationsByNarration(
  narrationId: number
): Promise<NarrationTranslationDTO[]> {
  const response = await http.get(
    `${endpoints.narrationTranslations}/narration/${narrationId}`
  );
  return unwrap<NarrationTranslationDTO[]>(response);
}

export async function updateTranslation(
  translationId: number,
  payload: NarrationTranslationDTO
): Promise<void> {
  await http.put(
    `${endpoints.narrationTranslations}/${translationId}`,
    payload
  );
}

export async function reviewTranslation(
  translationId: number,
  payload: ReviewTranslationRequestDTO
): Promise<void> {
  await http.patch(
    `${endpoints.narrationTranslations}/${translationId}/review`,
    payload
  );
}

export async function getAudioFiles(): Promise<AudioFileDTO[]> {
  const response = await http.get(endpoints.audioFiles);
  return unwrap<AudioFileDTO[]>(response);
}

export async function generateAudio(
  translationId: number,
  payload: GenerateAudioRequestDTO
): Promise<AudioFileDTO> {
  const response = await http.post(
    `${endpoints.audioFiles}/generate/${translationId}`,
    payload
  );
  return unwrap<AudioFileDTO>(response);
}

export async function getVendorNarrations(): Promise<NarrationContentDTO[]> {
  const response = await http.get(endpoints.vendorNarrations);
  return unwrap<NarrationContentDTO[]>(response);
}

export async function createVendorNarration(
  payload: VendorNarrationRequestDTO
): Promise<number> {
  try {
    const response = await http.post(endpoints.vendorNarrations, payload);
    return unwrap<number>(response);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function updateVendorNarration(
  narrationId: number,
  payload: VendorNarrationRequestDTO
): Promise<void> {
  try {
    await http.put(
      `${endpoints.vendorNarrations}/${narrationId}`,
      payload
    );
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function submitVendorNarration(
  narrationId: number
): Promise<void> {
  try {
    await http.patch(`${endpoints.vendorNarrations}/${narrationId}/submit`);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function getVendorNarrationStatus(
  narrationId: number
): Promise<VendorNarrationStatusDTO> {
  try {
    const response = await http.get(
      `${endpoints.vendorNarrations}/${narrationId}/status`
    );
    return unwrap<VendorNarrationStatusDTO>(response);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}


export async function moderateVendorNarration(
  narrationId: number,
  action: 'hide' | 'delete' | 'restore'
): Promise<void> {
  try {
    await http.patch(`${endpoints.vendorNarrations}/${narrationId}/${action}`);
  } catch (error) {
    throw new Error(getApiError(error));
  }
}

export async function moderateAdminNarration(
  narrationId: number,
  action: 'hide' | 'delete' | 'restore',
  reason?: string
): Promise<void> {
  try {
    await http.patch(`${endpoints.narrationContents}/${narrationId}/moderation`, {
      action,
      reason: reason || null
    });
  } catch (error) {
    throw new Error(getApiError(error));
  }
}
