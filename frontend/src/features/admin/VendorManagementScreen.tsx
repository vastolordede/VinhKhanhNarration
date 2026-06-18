import { useEffect, useState } from 'react';
import {
  getAdminVendors,
  getPendingRenewals,
  reviewVendorRegistration,
  reviewVendorRenewal
} from '../../api/vendorApi';
import { PageHeader } from '../../components/layout/PageHeader';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { AdminVendorListItemDTO, VendorRenewalRequestDTO } from '../../types';

const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5151';

export default function VendorManagementScreen() {
  const [vendors, setVendors] = useState<AdminVendorListItemDTO[]>([]);
  const [renewals, setRenewals] = useState<VendorRenewalRequestDTO[]>([]);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    const [vendorList, renewalList] = await Promise.all([
      getAdminVendors(),
      getPendingRenewals()
    ]);
    setVendors(vendorList);
    setRenewals(renewalList);
  }

  useEffect(() => { void load().catch((e) => setError(e.message)); }, []);

  async function reviewVendor(vendorId: number, approved: boolean) {
    const reason = approved ? undefined : window.prompt('Lý do từ chối:')?.trim();
    if (!approved && !reason) return;
    try {
      const order = await reviewVendorRegistration(vendorId, approved, reason);
      setMessage(approved ? `Đã duyệt và tạo mã thanh toán ${order.orderCode}.` : 'Đã từ chối hồ sơ.');
      await load();
    } catch (e) { setError(e instanceof Error ? e.message : 'Không duyệt được.'); }
  }

  async function reviewRenewalRequest(requestId: number, approved: boolean) {
    const reason = approved ? undefined : window.prompt('Lý do từ chối:')?.trim();
    if (!approved && !reason) return;
    try {
      const order = await reviewVendorRenewal(requestId, approved, reason);
      setMessage(approved ? `Đã duyệt gia hạn và tạo mã ${order.orderCode}.` : 'Đã từ chối gia hạn.');
      await load();
    } catch (e) { setError(e instanceof Error ? e.message : 'Không duyệt được.'); }
  }

  return (
    <div>
      <PageHeader title="Vendor & Subscription" description="Kiểm tra giấy phép, cấp mã thanh toán demo và quản lý gói 6 tháng." />
      {message && <p className="mb-4 rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700">{message}</p>}
      {error && <p className="mb-4 rounded-xl bg-rose-50 p-3 text-sm text-rose-600">{error}</p>}

      <h2 className="mb-3 text-lg font-bold">Hồ sơ Vendor</h2>
      <div className="space-y-3">
        {vendors.map((vendor) => (
          <Card key={vendor.vendorUserId}>
            <div className="flex flex-wrap justify-between gap-4">
              <div>
                <p className="text-xs font-bold uppercase text-teal-700">Vendor #{vendor.vendorUserId} · {vendor.accountStatus}</p>
                <h3 className="mt-1 text-lg font-bold">{vendor.shopName}</h3>
                <p className="text-sm text-slate-600">{vendor.ownerName} · {vendor.email} · {vendor.phone || '-'}</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  {vendor.documents.map((doc) => (
                    <a key={doc.documentId} href={`${apiBase}${doc.fileUrl}`} target="_blank" rel="noreferrer" className="rounded-xl bg-slate-100 px-3 py-2 text-sm font-semibold text-teal-700">
                      {doc.documentType} · {doc.verificationStatus}
                    </a>
                  ))}
                </div>
                {vendor.subscription && <p className="mt-2 text-sm">Hết hạn: {new Date(vendor.subscription.expiresAt).toLocaleString('vi-VN')}</p>}
                {vendor.pendingPayment && <p className="mt-2 text-sm font-semibold text-amber-700">Chờ thanh toán: {vendor.pendingPayment.orderCode}</p>}
              </div>
              {['PendingReview', 'Rejected'].includes(vendor.accountStatus) && (
                <div className="flex gap-2">
                  <Button onClick={() => void reviewVendor(vendor.vendorUserId, true)}>Duyệt hồ sơ</Button>
                  <Button variant="secondary" onClick={() => void reviewVendor(vendor.vendorUserId, false)}>Từ chối</Button>
                </div>
              )}
            </div>
          </Card>
        ))}
      </div>

      <h2 className="mb-3 mt-8 text-lg font-bold">Yêu cầu gia hạn chờ duyệt</h2>
      <div className="space-y-3">
        {renewals.map((item) => (
          <Card key={item.renewalRequestId}>
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="font-bold">Gia hạn #{item.renewalRequestId}</p>
                <p className="text-sm text-slate-500">Vendor #{item.vendorUserId} · Document #{item.foodSafetyDocumentId}</p>
              </div>
              <div className="flex gap-2">
                <Button onClick={() => void reviewRenewalRequest(item.renewalRequestId, true)}>Duyệt</Button>
                <Button variant="secondary" onClick={() => void reviewRenewalRequest(item.renewalRequestId, false)}>Từ chối</Button>
              </div>
            </div>
          </Card>
        ))}
        {!renewals.length && <Card><p>Không có yêu cầu gia hạn chờ duyệt.</p></Card>}
      </div>
    </div>
  );
}
