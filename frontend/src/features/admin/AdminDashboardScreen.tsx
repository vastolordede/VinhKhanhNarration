import { useEffect, useState } from 'react';
import { getList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { Card } from '../../components/ui/Card';
import { PageHeader } from '../../components/layout/PageHeader';

export default function AdminDashboardScreen() {
  const [stats, setStats] = useState<Record<string, number>>({});

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
    ['Địa điểm / POI', stats.places],
    ['Món ăn', stats.dishes],
    ['Thuyết minh', stats.narrations],
    ['Feedback', stats.feedbacks],
    ['Lượt nghe', stats.listening],
    ['Geofence events', stats.geofence]
  ];

  return (
    <div>
      <PageHeader title="Admin Dashboard" description="Tổng quan dữ liệu hệ thống." />
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {cards.map(([label, value]) => (
          <Card key={label as string}>
            <p className="text-sm text-slate-500">{label}</p>
            <p className="mt-2 text-3xl font-bold text-slate-900">{value as number}</p>
          </Card>
        ))}
      </div>
    </div>
  );
}
