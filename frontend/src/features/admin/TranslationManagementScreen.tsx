import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function TranslationManagementScreen() {
  return <SimpleResourcePage config={{ title: 'Translation Management', description: 'Bản dịch theo từng ngôn ngữ. Nếu chưa có audio file, FE sẽ dùng TTS đọc translatedText.', endpoint: endpoints.narrationTranslations, fields: [
    { name: 'narrationId', label: 'Narration Id', type: 'number' },
    { name: 'languageId', label: 'Language Id', type: 'number' },
    { name: 'translatedTitle', label: 'Translated Title' },
    { name: 'translatedText', label: 'Translated Text', type: 'textarea' },
    { name: 'translationSourceId', label: 'Translation Source Id', type: 'number' },
    { name: 'reviewedBy', label: 'Reviewed By', type: 'number' },
    { name: 'isReviewed', label: 'Reviewed', type: 'checkbox' }
  ], columns: [
    { key: 'translationId', label: 'Id' }, { key: 'narrationId', label: 'Narration' }, { key: 'languageId', label: 'Language' }, { key: 'translatedTitle', label: 'Title' }, { key: 'isReviewed', label: 'Reviewed', render: r => r.isReviewed ? 'Yes' : 'No' }
  ] }} />;
}
