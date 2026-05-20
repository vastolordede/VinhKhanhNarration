import SimpleResourcePage from './SimpleResourcePage';
import { endpoints } from '../../api/endpoints';

export default function LanguageManagementScreen() {
  return <SimpleResourcePage config={{ title: 'Language Management', endpoint: endpoints.languages, fields: [
    { name: 'languageCode', label: 'Language Code' },
    { name: 'languageName', label: 'Language Name' },
    { name: 'isDefault', label: 'Default', type: 'checkbox' },
    { name: 'isActive', label: 'Active', type: 'checkbox' }
  ], columns: [
    { key: 'languageCode', label: 'Code' },
    { key: 'languageName', label: 'Name' },
    { key: 'isDefault', label: 'Default', render: r => r.isDefault ? 'Yes' : 'No' },
    { key: 'isActive', label: 'Active', render: r => r.isActive ? 'Yes' : 'No' }
  ] }} />;
}
