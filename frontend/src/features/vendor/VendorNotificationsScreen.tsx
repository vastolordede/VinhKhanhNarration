import { useEffect, useState } from 'react';
import { getVendorNotifications, markVendorNotificationRead } from '../../api/vendorApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { VendorNotificationDTO } from '../../types';

export default function VendorNotificationsScreen() {
  const [items, setItems] = useState<VendorNotificationDTO[]>([]);
  const [error, setError] = useState<string | null>(null);

  async function load() { setItems(await getVendorNotifications()); }
  useEffect(() => { void load().catch((e) => setError(e.message)); }, []);

  async function read(id: number) {
    await markVendorNotificationRead(id);
    await load();
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Thông báo</h1>
      {error && <p className="mb-4 text-rose-600">{error}</p>}
      <div className="space-y-3">
        {items.map((item) => (
          <Card key={item.notificationId} className={item.isRead ? 'opacity-70' : ''}>
            <div className="flex items-start justify-between gap-3">
              <div>
                <p className="font-bold">{item.title}</p>
                <p className="mt-1 text-sm text-slate-600">{item.message}</p>
                <p className="mt-2 text-xs text-slate-400">{new Date(item.createdAt).toLocaleString('vi-VN')}</p>
              </div>
              {!item.isRead && <Button variant="secondary" onClick={() => void read(item.notificationId)}>Đã đọc</Button>}
            </div>
          </Card>
        ))}
        {!items.length && <Card><p>Chưa có thông báo.</p></Card>}
      </div>
    </div>
  );
}
