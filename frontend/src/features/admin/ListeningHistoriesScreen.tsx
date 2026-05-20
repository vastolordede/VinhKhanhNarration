import { useEffect, useState } from 'react';
import { getList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { DataTable } from '../../components/ui/DataTable';
import { PageHeader } from '../../components/layout/PageHeader';
import { ListeningHistoryDTO } from '../../types';

export default function ListeningHistoriesScreen() {
  const [items, setItems] = useState<ListeningHistoryDTO[]>([]);
  useEffect(() => { getList<ListeningHistoryDTO>(endpoints.adminListeningHistories).then(setItems); }, []);
  return <div>
    <PageHeader title="Listening Histories" description="Theo dõi lượt nghe theo QR / Geofence / Manual." />
    <DataTable headers={['Id','Guest','Narration','Language','Source','Status','Duration','Time']} rows={items.map(x => [
      x.historyId, x.guestSessionId || '', x.narrationId, x.languageId, x.triggerSource, x.playbackStatus, x.listenDurationSeconds ?? '', x.listenedAt || ''
    ])} />
  </div>;
}
