import { FormEvent, useEffect, useMemo, useState } from 'react';
import { getList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import {
  createVendorDish,
  getVendorCatalog,
  removeVendorMenuItem,
  saveVendorPlace,
  setVendorDishActive,
  updateVendorDish
} from '../../api/vendorApi';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { Textarea } from '../../components/ui/Textarea';
import {
  DishCategoryDTO,
  DishDTO,
  LookupDTO,
  PlaceDTO,
  VendorCatalogDTO,
  VendorDishRequestDTO
} from '../../types';

const emptyPlace: PlaceDTO = {
  placeId: 0,
  placeName: '',
  placeTypeId: 0,
  address: '',
  description: '',
  latitude: null,
  longitude: null,
  openingHours: '',
  imageUrl: '',
  isPoi: true,
  isGeofenceEnabled: true,
  triggerRadiusMeters: 50,
  priority: 0,
  triggerModeId: 0,
  debounceSeconds: 10,
  cooldownSeconds: 300,
  isActive: true
};

const emptyDish: VendorDishRequestDTO = {
  dishName: '',
  categoryId: 0,
  description: '',
  imageUrl: '',
  averagePrice: null,
  isSignatureDish: false,
  menuPrice: null,
  isRecommended: false,
  note: ''
};

export default function VendorCatalogScreen() {
  const [catalog, setCatalog] = useState<VendorCatalogDTO | null>(null);
  const [place, setPlace] = useState<PlaceDTO>(emptyPlace);
  const [dish, setDish] = useState<VendorDishRequestDTO>(emptyDish);
  const [editingDishId, setEditingDishId] = useState<number | null>(null);
  const [categories, setCategories] = useState<DishCategoryDTO[]>([]);
  const [placeTypes, setPlaceTypes] = useState<LookupDTO[]>([]);
  const [triggerModes, setTriggerModes] = useState<LookupDTO[]>([]);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const menuByDish = useMemo(
    () => new Map((catalog?.menu ?? []).map((item) => [item.dishId, item])),
    [catalog?.menu]
  );

  async function load() {
    const [catalogData, categoryData, placeTypeData, triggerModeData] =
      await Promise.all([
        getVendorCatalog(),
        getList<DishCategoryDTO>(`${endpoints.dishCategories}/active`),
        getList<LookupDTO>(`${endpoints.placeTypes}/active`),
        getList<LookupDTO>(`${endpoints.triggerModes}/active`)
      ]);
    setCatalog(catalogData);
    setPlace(catalogData.place ?? {
      ...emptyPlace,
      placeTypeId: placeTypeData[0]?.placeTypeId ?? placeTypeData[0]?.id ?? 0,
      triggerModeId: triggerModeData[0]?.triggerModeId ?? triggerModeData[0]?.id ?? 0
    });
    setCategories(categoryData);
    setPlaceTypes(placeTypeData);
    setTriggerModes(triggerModeData);
    setDish((current) => ({
      ...current,
      categoryId: current.categoryId || categoryData[0]?.categoryId || 0
    }));
  }

  useEffect(() => {
    void load().catch((loadError) =>
      setError(loadError instanceof Error ? loadError.message : 'Không tải được dữ liệu.')
    );
  }, []);

  async function savePlace(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setMessage(null);
    try {
      const saved = await saveVendorPlace(place);
      setPlace(saved);
      setMessage('Đã lưu thông tin sạp.');
      await load();
    } catch (saveError) {
      setError(saveError instanceof Error ? saveError.message : 'Không thể lưu sạp.');
    } finally {
      setBusy(false);
    }
  }

  async function saveDish(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setMessage(null);
    try {
      if (editingDishId) await updateVendorDish(editingDishId, dish);
      else await createVendorDish(dish);
      setDish({ ...emptyDish, categoryId: categories[0]?.categoryId || 0 });
      setEditingDishId(null);
      setMessage(editingDishId ? 'Đã cập nhật món.' : 'Đã thêm món vào menu.');
      await load();
    } catch (saveError) {
      setError(saveError instanceof Error ? saveError.message : 'Không thể lưu món.');
    } finally {
      setBusy(false);
    }
  }

  function editDish(item: DishDTO) {
    const menu = menuByDish.get(item.dishId);
    setEditingDishId(item.dishId);
    setDish({
      dishName: item.dishName,
      categoryId: item.categoryId,
      description: item.description ?? '',
      imageUrl: item.imageUrl ?? '',
      averagePrice: item.averagePrice ?? null,
      isSignatureDish: item.isSignatureDish,
      menuPrice: menu?.price ?? item.averagePrice ?? null,
      isRecommended: menu?.isRecommended ?? false,
      note: menu?.note ?? ''
    });
    window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
  }

  async function toggleDish(item: DishDTO) {
    await setVendorDishActive(item.dishId, !item.isActive);
    await load();
  }

  async function removeFromMenu(item: DishDTO) {
    await removeVendorMenuItem(item.dishId);
    await load();
  }

  const selectClass =
    'w-full rounded-2xl border border-slate-200 px-4 py-3 text-sm outline-none focus:border-teal-600';

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Sạp, món ăn và menu</h1>
        <p className="mt-1 text-sm text-slate-500">
          Vendor chỉ được quản lý dữ liệu thuộc sạp của mình.
        </p>
      </div>

      {error && <p className="rounded-2xl bg-rose-50 p-4 text-sm text-rose-700">{error}</p>}
      {message && <p className="rounded-2xl bg-emerald-50 p-4 text-sm text-emerald-700">{message}</p>}

      <Card>
        <h2 className="text-lg font-bold">Thông tin sạp</h2>
        <form className="mt-4 grid gap-3 md:grid-cols-2" onSubmit={savePlace}>
          <Input required placeholder="Tên sạp" value={place.placeName} onChange={(e) => setPlace({ ...place, placeName: e.target.value })} />
          <select className={selectClass} value={place.placeTypeId} onChange={(e) => setPlace({ ...place, placeTypeId: Number(e.target.value) })}>
            {placeTypes.map((item) => <option key={item.placeTypeId ?? item.id} value={item.placeTypeId ?? item.id}>{item.name}</option>)}
          </select>
          <Input required placeholder="Địa chỉ" value={place.address ?? ''} onChange={(e) => setPlace({ ...place, address: e.target.value })} />
          <Input placeholder="Giờ mở cửa" value={place.openingHours ?? ''} onChange={(e) => setPlace({ ...place, openingHours: e.target.value })} />
          <Input type="number" step="any" placeholder="Latitude" value={place.latitude ?? ''} onChange={(e) => setPlace({ ...place, latitude: e.target.value === '' ? null : Number(e.target.value) })} />
          <Input type="number" step="any" placeholder="Longitude" value={place.longitude ?? ''} onChange={(e) => setPlace({ ...place, longitude: e.target.value === '' ? null : Number(e.target.value) })} />
          <Input placeholder="URL hình ảnh" value={place.imageUrl ?? ''} onChange={(e) => setPlace({ ...place, imageUrl: e.target.value })} />
          <select className={selectClass} value={place.triggerModeId} onChange={(e) => setPlace({ ...place, triggerModeId: Number(e.target.value) })}>
            {triggerModes.map((item) => <option key={item.triggerModeId ?? item.id} value={item.triggerModeId ?? item.id}>{item.name}</option>)}
          </select>
          <Textarea className="md:col-span-2" placeholder="Mô tả sạp" value={place.description ?? ''} onChange={(e) => setPlace({ ...place, description: e.target.value })} />
          <div className="grid grid-cols-3 gap-3 md:col-span-2">
            <Input type="number" min="1" placeholder="Bán kính (m)" value={place.triggerRadiusMeters} onChange={(e) => setPlace({ ...place, triggerRadiusMeters: Number(e.target.value) })} />
            <Input type="number" min="0" placeholder="Debounce" value={place.debounceSeconds} onChange={(e) => setPlace({ ...place, debounceSeconds: Number(e.target.value) })} />
            <Input type="number" min="0" placeholder="Cooldown" value={place.cooldownSeconds} onChange={(e) => setPlace({ ...place, cooldownSeconds: Number(e.target.value) })} />
          </div>
          <Button className="md:col-span-2" disabled={busy}>Lưu thông tin sạp</Button>
        </form>
      </Card>

      <Card>
        <h2 className="text-lg font-bold">Danh sách món</h2>
        <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {(catalog?.dishes ?? []).map((item) => {
            const menu = menuByDish.get(item.dishId);
            return (
              <div key={item.dishId} className="rounded-2xl border border-slate-200 p-4">
                <div className="flex items-start justify-between gap-2">
                  <div>
                    <p className="font-bold">{item.dishName}</p>
                    <p className="text-xs text-slate-500">{item.isActive ? 'Đang hiển thị' : 'Đã ẩn'}</p>
                  </div>
                  {menu?.isRecommended && <span className="rounded-full bg-amber-100 px-2 py-1 text-xs font-bold text-amber-700">Đề xuất</span>}
                </div>
                <p className="mt-2 text-sm text-slate-600">{item.description || 'Chưa có mô tả'}</p>
                <p className="mt-2 font-semibold text-teal-700">{(menu?.price ?? item.averagePrice ?? 0).toLocaleString()} VND</p>
                <div className="mt-3 flex flex-wrap gap-2">
                  <Button variant="secondary" className="px-3 py-2" onClick={() => editDish(item)}>Sửa</Button>
                  <Button variant={item.isActive ? 'danger' : 'secondary'} className="px-3 py-2" onClick={() => void toggleDish(item)}>{item.isActive ? 'Ẩn' : 'Khôi phục'}</Button>
                  {menu && <Button variant="ghost" className="px-3 py-2" onClick={() => void removeFromMenu(item)}>Bỏ khỏi menu</Button>}
                </div>
              </div>
            );
          })}
        </div>
      </Card>

      <Card>
        <h2 className="text-lg font-bold">{editingDishId ? 'Cập nhật món' : 'Thêm món mới'}</h2>
        {!catalog?.place && <p className="mt-2 text-sm font-semibold text-amber-700">Hãy lưu thông tin sạp trước khi thêm món.</p>}
        <form className="mt-4 grid gap-3 md:grid-cols-2" onSubmit={saveDish}>
          <Input required placeholder="Tên món" value={dish.dishName} onChange={(e) => setDish({ ...dish, dishName: e.target.value })} />
          <select className={selectClass} value={dish.categoryId} onChange={(e) => setDish({ ...dish, categoryId: Number(e.target.value) })}>
            {categories.map((item) => <option key={item.categoryId} value={item.categoryId}>{item.categoryName}</option>)}
          </select>
          <Input type="number" min="0" placeholder="Giá trung bình" value={dish.averagePrice ?? ''} onChange={(e) => setDish({ ...dish, averagePrice: e.target.value === '' ? null : Number(e.target.value) })} />
          <Input type="number" min="0" placeholder="Giá trong menu" value={dish.menuPrice ?? ''} onChange={(e) => setDish({ ...dish, menuPrice: e.target.value === '' ? null : Number(e.target.value) })} />
          <Input placeholder="URL hình ảnh" value={dish.imageUrl ?? ''} onChange={(e) => setDish({ ...dish, imageUrl: e.target.value })} />
          <Input placeholder="Ghi chú menu" value={dish.note ?? ''} onChange={(e) => setDish({ ...dish, note: e.target.value })} />
          <Textarea className="md:col-span-2" placeholder="Mô tả món" value={dish.description ?? ''} onChange={(e) => setDish({ ...dish, description: e.target.value })} />
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={dish.isSignatureDish} onChange={(e) => setDish({ ...dish, isSignatureDish: e.target.checked })} /> Món đặc trưng</label>
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={dish.isRecommended} onChange={(e) => setDish({ ...dish, isRecommended: e.target.checked })} /> Đề xuất trong menu</label>
          <div className="flex gap-2 md:col-span-2">
            <Button className="flex-1" disabled={busy || !catalog?.place}>{editingDishId ? 'Cập nhật món' : 'Thêm món'}</Button>
            {editingDishId && <Button type="button" variant="secondary" onClick={() => { setEditingDishId(null); setDish({ ...emptyDish, categoryId: categories[0]?.categoryId || 0 }); }}>Hủy</Button>}
          </div>
        </form>
      </Card>
    </div>
  );
}
