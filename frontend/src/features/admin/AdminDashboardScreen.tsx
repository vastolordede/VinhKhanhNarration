import { useEffect, useState } from 'react';
import { getList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { Card } from '../../components/ui/Card';
import { PageHeader } from '../../components/layout/PageHeader';
import { useI18n } from '../../i18n/useI18n';

export default function AdminDashboardScreen() {
  const [stats, setStats] = useState<Record<string, number>>({});
  const { tx } = useI18n();

  useEffect(() => {
    async function load() {
      const entries = await Promise.allSettled([
        getList(endpoints.places),
        getList(endpoints.dishes),
        getList(endpoints.narrationContents),
        getList(endpoints.adminFeedbacks),
        getList(endpoints.adminListeningHistories),
        getList(endpoints.adminGeofenceEvents)
      ]);

      setStats({
        places: entries[0].status === 'fulfilled' ? entries[0].value.length : 0,
        dishes: entries[1].status === 'fulfilled' ? entries[1].value.length : 0,
        narrations: entries[2].status === 'fulfilled' ? entries[2].value.length : 0,
        feedbacks: entries[3].status === 'fulfilled' ? entries[3].value.length : 0,
        listening: entries[4].status === 'fulfilled' ? entries[4].value.length : 0,
        geofence: entries[5].status === 'fulfilled' ? entries[5].value.length : 0
      });
    }

    load();
  }, []);

  const cards = [
    { label: 'Places / POI', value: stats.places },
    { label: 'Dishes', value: stats.dishes },
    { label: 'Narrations', value: stats.narrations },
    { label: 'Feedback', value: stats.feedbacks },
    { label: 'Listening', value: stats.listening },
    { label: 'Geofence Events', value: stats.geofence }
  ];

  return (
    <div>
      <PageHeader title="Admin Dashboard" description="Tổng quan dữ liệu hệ thống." />

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {cards.map((card) => (
          <Card key={card.label}>
            <p className="text-sm text-slate-500">{tx(card.label)}</p>
            <p className="mt-2 text-3xl font-bold text-slate-900">{card.value ?? 0}</p>
          </Card>
        ))}
      </div>
    </div>
  );
}