import SimpleResourcePage, { getCurrentAdminId } from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function TranslationManagementScreen() {
  return <SimpleResourcePage config={{ title: 'Translation Management', description: 'Bản dịch theo từng ngôn ngữ. Nếu chưa có audio file, FE sẽ dùng TTS đọc translatedText.', endpoint: endpoints.narrationTranslations,
preparePayload: (payload) => {
  if (payload.isReviewed && !payload.reviewedBy) {
    payload.reviewedBy = getCurrentAdminId();
  }

  if (!payload.isReviewed) {
    payload.reviewedBy = null;
  }

  return payload;
},
fields: [
  {
    name: 'narrationId',
    label: 'Narration',
    type: 'select',
    optionEndpoint: endpoints.narrationContents,
    optionValueKey: 'narrationId',
    optionLabelKey: 'title',
    required: true
  },
  {
    name: 'languageId',
    label: 'Language',
    type: 'select',
    optionEndpoint: endpoints.languages,
    optionValueKey: 'languageId',
    optionLabel: (language) =>
      `${language.languageId} - ${language.languageName} (${language.languageCode})`,
    required: true
  },
  { name: 'translatedTitle', label: 'Translated Title', required: true },
  { name: 'translatedText', label: 'Translated Text', type: 'textarea', required: true },
  {
    name: 'translationSourceId',
    label: 'Translation Source',
    type: 'select',
    optionEndpoint: endpoints.translationSources,
    optionValueKey: 'id',
    optionLabelKey: 'name',
    required: true
  },
  { name: 'isReviewed', label: 'Reviewed', type: 'checkbox' }
], columns: [
    { key: 'translationId', label: 'Id' }, { key: 'narrationId', label: 'Narration' }, { key: 'languageId', label: 'Language' }, { key: 'translatedTitle', label: 'Title' }, { key: 'isReviewed', label: 'Reviewed', render: r => r.isReviewed ? 'Yes' : 'No' }
  ] }} />;
}
