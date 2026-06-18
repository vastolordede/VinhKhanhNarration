import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function LanguageManagementScreen() {
  return (
    <SimpleResourcePage
      config={{
        title: 'Language Management',
        description:
          'Bật Active + Content + Translation + TTS để ngôn ngữ được tự dịch và tự tạo audio sau khi duyệt nội dung nguồn.',
        endpoint: endpoints.languages,
        fields: [
          { name: 'languageCode', label: 'Language Code', required: true },
          { name: 'languageName', label: 'Language Name', required: true },
          { name: 'locale', label: 'Locale', required: true },
          { name: 'nativeName', label: 'Native Name', required: true },
          { name: 'defaultVoiceId', label: 'Default TTS Voice', required: true },
          { name: 'isDefault', label: 'Default', type: 'checkbox' },
          { name: 'isUiEnabled', label: 'UI Enabled', type: 'checkbox' },
          { name: 'isContentEnabled', label: 'Content Enabled', type: 'checkbox' },
          {
            name: 'isTranslationSupported',
            label: 'Translation Supported',
            type: 'checkbox'
          },
          { name: 'isTtsSupported', label: 'TTS Supported', type: 'checkbox' },
          { name: 'isActive', label: 'Active', type: 'checkbox' }
        ],
        columns: [
          { key: 'languageId', label: 'Id' },
          { key: 'languageCode', label: 'Code' },
          { key: 'languageName', label: 'Name' },
          { key: 'locale', label: 'Locale' },
          { key: 'defaultVoiceId', label: 'Default Voice' },
          {
            key: 'isContentEnabled',
            label: 'Content',
            render: (row) => (row.isContentEnabled ? 'Yes' : 'No')
          },
          {
            key: 'isTranslationSupported',
            label: 'Translation',
            render: (row) => (row.isTranslationSupported ? 'Yes' : 'No')
          },
          {
            key: 'isTtsSupported',
            label: 'TTS',
            render: (row) => (row.isTtsSupported ? 'Yes' : 'No')
          },
          {
            key: 'isActive',
            label: 'Active',
            render: (row) => (row.isActive ? 'Yes' : 'No')
          }
        ]
      }}
    />
  );
}
