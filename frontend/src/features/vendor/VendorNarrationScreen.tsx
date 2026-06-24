import { FormEvent, useEffect, useMemo, useState } from 'react';
import { getList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { getVendorCatalog } from '../../api/vendorApi';
import {
  createVendorNarration,
  getVendorNarrations,
  getVendorNarrationStatus,
  moderateVendorNarration,
  updateVendorNarration
} from '../../api/narrationApi';
import { AuthenticatedAudio } from '../../components/media/AuthenticatedAudio';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { Modal } from '../../components/ui/Modal';
import { Textarea } from '../../components/ui/Textarea';
import {
  DishDTO,
  LanguageDTO,
  LookupDTO,
  NarrationContentDTO,
  PlaceDTO,
  VendorNarrationRequestDTO,
  VendorNarrationStatusDTO
} from '../../types';

const emptyForm = {
  narrationId: 0,
  title: '',
  originalText: '',
  sourceLanguageId: 0,
  contentTypeId: 0,
  placeId: null as number | null,
  dishId: null as number | null
};

export default function VendorNarrationScreen() {
  const [items, setItems] = useState<NarrationContentDTO[]>([]);
  const [languages, setLanguages] = useState<LanguageDTO[]>([]);
  const [contentTypes, setContentTypes] = useState<LookupDTO[]>([]);
  const [places, setPlaces] = useState<PlaceDTO[]>([]);
  const [dishes, setDishes] = useState<DishDTO[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [modalOpen, setModalOpen] = useState(false);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [statusById, setStatusById] = useState<Record<number, VendorNarrationStatusDTO>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [narrations, languageList, typeList, catalog] = await Promise.all([
        getVendorNarrations(),
        getList<LanguageDTO>(`${endpoints.languages}/active`),
        getList<LookupDTO>(`${endpoints.contentTypes}/active`),
        getVendorCatalog()
      ]);
      const activePlace = catalog.place?.isActive ? catalog.place : null;
      const activeDishes = catalog.dishes.filter((x) => x.isActive);
      const vendorTypes = typeList.filter(
        (x) => x.isActive && (
          (x.code === 'Place' && activePlace != null) ||
          (x.code === 'Dish' && activeDishes.length > 0)
        )
      );
      setItems(narrations);
      setLanguages(languageList.filter((x) => x.isActive && x.isContentEnabled));
      setContentTypes(vendorTypes);
      setPlaces(activePlace ? [activePlace] : []);
      setDishes(activeDishes);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Không tải được dữ liệu.');
    }
  }

  useEffect(() => { void load(); }, []);

  const typeMap = useMemo(() => new Map(contentTypes.map((x) => [x.id, x])), [contentTypes]);

  function openCreate() {
    const language = languages.find((x) => x.isDefault) ?? languages[0];
    setForm({
      ...emptyForm,
      sourceLanguageId: language?.languageId ?? 0,
      contentTypeId: contentTypes[0]?.id ?? 0
    });
    setModalOpen(true);
  }

  function openEdit(item: NarrationContentDTO) {
    setForm({
      narrationId: item.narrationId,
      title: item.title,
      originalText: item.originalText,
      sourceLanguageId: item.sourceLanguageId ?? 0,
      contentTypeId: item.contentTypeId,
      placeId: item.placeId ?? null,
      dishId: item.dishId ?? null
    });
    setModalOpen(true);
  }

  async function save(event: FormEvent) {
    event.preventDefault();
    const typeCode = typeMap.get(form.contentTypeId)?.code;
    const payload: VendorNarrationRequestDTO = {
      sourceLanguageId: form.sourceLanguageId,
      title: form.title.trim(),
      originalText: form.originalText.trim(),
      contentTypeId: form.contentTypeId,
      placeId: typeCode === 'Place' ? form.placeId : null,
      dishId: typeCode === 'Dish' ? form.dishId : null
    };

    setError(null);
    try {
      if (form.narrationId) {
        await updateVendorNarration(form.narrationId, payload);
        setMessage('Đã cập nhật và gửi lại cho Admin duyệt.');
      } else {
        await createVendorNarration(payload);
        setMessage('Đã tạo và gửi cho Admin duyệt.');
      }
      setModalOpen(false);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Không lưu được.');
    }
  }

  async function moderate(item: NarrationContentDTO, action: 'hide' | 'delete' | 'restore') {
    if (!window.confirm(`Xác nhận ${action} nội dung này?`)) return;
    setBusyId(item.narrationId);
    setError(null);
    try {
      await moderateVendorNarration(item.narrationId, action);
      setMessage('Đã cập nhật trạng thái nội dung.');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Không thực hiện được.');
    } finally {
      setBusyId(null);
    }
  }

  async function toggleStatus(item: NarrationContentDTO) {
    if (expandedId === item.narrationId) return setExpandedId(null);
    setExpandedId(item.narrationId);
    if (statusById[item.narrationId]) return;
    setBusyId(item.narrationId);
    try {
      const status = await getVendorNarrationStatus(item.narrationId);
      setStatusById((current) => ({ ...current, [item.narrationId]: status }));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Không tải được trạng thái.');
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div>
      <div className="mb-5 flex items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold">Nội dung thuyết minh</h1>
          <p className="text-sm text-slate-500">Mỗi lần thêm hoặc sửa sẽ tự chuyển sang chờ Admin duyệt.</p>
        </div>
        <Button onClick={openCreate} disabled={contentTypes.length === 0 || languages.length === 0}>Thêm nội dung</Button>
      </div>

      {message && <p className="mb-4 rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700">{message}</p>}
      {error && <p className="mb-4 rounded-xl bg-rose-50 p-3 text-sm text-rose-600">{error}</p>}
      {!error && contentTypes.length === 0 && (
        <p className="mb-4 rounded-xl bg-amber-50 p-3 text-sm text-amber-700">
          Hãy khai báo sạp hoặc ít nhất một món ăn đang hoạt động trước khi tạo nội dung thuyết minh.
        </p>
      )}

      <div className="space-y-3">
        {items.map((item) => {
          const vendorHidden = item.workflowStatus === 'HiddenByVendor';
          const vendorDeleted = item.workflowStatus === 'DeletedByVendor';
          const adminLocked = item.workflowStatus === 'HiddenByAdmin' || item.workflowStatus === 'DeletedByAdmin';
          const editable = item.workflowStatus !== 'PendingReview';
          return (
            <Card key={item.narrationId}>
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <p className="text-xs font-bold uppercase text-teal-700">#{item.narrationId} · {item.workflowStatus}</p>
                  <h2 className="mt-1 text-lg font-bold">{item.title}</h2>
                  <p className="mt-2 line-clamp-3 text-sm text-slate-600">{item.originalText}</p>
                  {(item.rejectionReason || item.moderationReason) && (
                    <p className="mt-2 rounded-xl bg-rose-50 p-2 text-sm font-semibold text-rose-600">
                      Lý do: {item.rejectionReason || item.moderationReason}
                    </p>
                  )}
                </div>
                <div className="flex flex-wrap gap-2">
                  <Button variant="secondary" onClick={() => void toggleStatus(item)}>Dịch & audio</Button>
                  {editable && !vendorHidden && !vendorDeleted && (
                    <Button variant="secondary" onClick={() => openEdit(item)}>{adminLocked ? 'Sửa và gửi lại' : 'Sửa'}</Button>
                  )}
                  {!vendorHidden && !vendorDeleted && !adminLocked && (
                    <Button variant="secondary" disabled={busyId === item.narrationId} onClick={() => void moderate(item, 'hide')}>Ẩn</Button>
                  )}
                  {!vendorDeleted && !adminLocked && (
                    <Button variant="secondary" disabled={busyId === item.narrationId} onClick={() => void moderate(item, 'delete')}>Xóa</Button>
                  )}
                  {(vendorHidden || vendorDeleted) && (
                    <Button disabled={busyId === item.narrationId} onClick={() => void moderate(item, 'restore')}>Khôi phục</Button>
                  )}
                </div>
              </div>

              {expandedId === item.narrationId && (
                <div className="mt-4 space-y-2 border-t pt-4">
                  {statusById[item.narrationId]?.translations.map((translation) => {
                    const audio = statusById[item.narrationId].audioFiles
                      .filter((x) => x.translationId === translation.translationId)
                      .sort((a, b) => b.audioId - a.audioId)[0];
                    return (
                      <div key={translation.translationId} className="rounded-xl bg-slate-50 p-3 text-sm">
                        <p className="font-semibold">Language #{translation.languageId} · Text {translation.status} · Audio {audio?.status || 'Chưa tạo'}</p>
                        <p className="mt-1 text-slate-600">{translation.translatedTitle}</p>
                        {audio?.audioUrl && audio.status === 'Ready' && <AuthenticatedAudio audioId={audio.audioId} className="mt-2 w-full" />}
                      </div>
                    );
                  })}
                  {!statusById[item.narrationId]?.translations.length && <p className="text-sm text-slate-500">Chưa có bản dịch.</p>}
                </div>
              )}
            </Card>
          );
        })}
      </div>

      <Modal open={modalOpen} title={form.narrationId ? 'Sửa và gửi duyệt' : 'Thêm và gửi duyệt'} onClose={() => setModalOpen(false)}>
        <form onSubmit={save} className="space-y-3">
          <Input required placeholder="Tiêu đề" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
          <Textarea required rows={8} placeholder="Nội dung nguồn" value={form.originalText} onChange={(e) => setForm({ ...form, originalText: e.target.value })} />
          <select className="w-full rounded-2xl border px-4 py-3 text-sm" value={form.sourceLanguageId} onChange={(e) => setForm({ ...form, sourceLanguageId: Number(e.target.value) })} required>
            {languages.map((item) => <option key={item.languageId} value={item.languageId}>{item.languageName}</option>)}
          </select>
          <select className="w-full rounded-2xl border px-4 py-3 text-sm" value={form.contentTypeId} onChange={(e) => setForm({ ...form, contentTypeId: Number(e.target.value), placeId: null, dishId: null })} required>
            {contentTypes.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}
          </select>
          {typeMap.get(form.contentTypeId)?.code === 'Place' && (
            <select className="w-full rounded-2xl border px-4 py-3 text-sm" value={form.placeId ?? ''} onChange={(e) => setForm({ ...form, placeId: e.target.value ? Number(e.target.value) : null })} required>
              <option value="">Chọn địa điểm</option>
              {places.map((item) => <option key={item.placeId} value={item.placeId}>{item.placeName}</option>)}
            </select>
          )}
          {typeMap.get(form.contentTypeId)?.code === 'Dish' && (
            <select className="w-full rounded-2xl border px-4 py-3 text-sm" value={form.dishId ?? ''} onChange={(e) => setForm({ ...form, dishId: e.target.value ? Number(e.target.value) : null })} required>
              <option value="">Chọn món ăn</option>
              {dishes.map((item) => <option key={item.dishId} value={item.dishId}>{item.dishName}</option>)}
            </select>
          )}
          <Button className="w-full">Lưu và gửi Admin duyệt</Button>
        </form>
      </Modal>
    </div>
  );
}
