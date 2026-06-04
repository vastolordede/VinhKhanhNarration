import { FormEvent, ReactNode, useEffect, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { createItem, getList, patchItem, updateItem } from '../../api/crud';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { DataTable } from '../../components/ui/DataTable';
import { Input } from '../../components/ui/Input';
import { Textarea } from '../../components/ui/Textarea';
import { PageHeader } from '../../components/layout/PageHeader';
import { useI18n } from '../../i18n/useI18n';
import { StatusBadge } from '../../components/ui/StatusBadge';

export type FieldConfig = {
  name: string;
  label: string;
  type?: 'text' | 'number' | 'textarea' | 'checkbox';
  placeholder?: string;
  disabled?: boolean;
};

export type ResourceConfig = {
  title: string;
  description?: string;
  endpoint: string;
  idKey?: string;
  fields: FieldConfig[];
  columns: { key: string; label: string; render?: (row: any) => ReactNode }[];
  softDelete?: boolean;

  extraFormActions?: (
    form: Record<string, any>,
    setForm: Dispatch<SetStateAction<Record<string, any>>>
  ) => ReactNode;

  extraFormActionsAfterField?: string;
};

export default function SimpleResourcePage({ config }: { config: ResourceConfig }) {
  const [items, setItems] = useState<any[]>([]);
  const [editing, setEditing] = useState<any | null>(null);
  const [form, setForm] = useState<Record<string, any>>({});
  const [error, setError] = useState<string | null>(null);

  const { tx } = useI18n();

  async function load() {
    try {
      const data = await getList<any>(config.endpoint);
      setItems(data);
    } catch {
      setError('Không tải được dữ liệu. Kiểm tra backend endpoint.');
    }
  }

  useEffect(() => {
    load();
  }, [config.endpoint]);

  function getRowId(row: any) {
    if (config.idKey && row[config.idKey] !== undefined) return row[config.idKey];

    const key = Object.keys(row).find((k) => k.toLowerCase().endsWith('id'));
    return key ? row[key] : undefined;
  }

 function startEdit(row: any) {
  setEditing(row);
  setForm(row);
}

  function resetForm() {
    setEditing(null);
    setForm({});
    setError(null);
  }

  async function submit(e: FormEvent) {
  e.preventDefault();
  setError(null);

  if (editing) {
    const confirmed = window.confirm(
      tx('Are you sure you want to save these changes?')
    );

    if (!confirmed) return;
  }

  try {
    if (editing) {
      await updateItem(`${config.endpoint}/${getRowId(editing)}`, form);
    } else {
      await createItem(config.endpoint, form);
    }

    resetForm();
    await load();
  } catch {
    setError('Không lưu được dữ liệu. Kiểm tra dữ liệu nhập hoặc API.');
  }
}

  async function deactivate(row: any) {
  const confirmed = window.confirm(
    tx('Are you sure you want to hide this record?')
  );

  if (!confirmed) return;

  try {
    await patchItem(`${config.endpoint}/${getRowId(row)}/deactivate`);
    await load();
  } catch {
    setError('Không deactivate được dữ liệu.');
  }
}
function renderCellValue(value: ReactNode): ReactNode {
  if (value === null || value === undefined || value === '') {
    return '';
  }

  if (typeof value === 'boolean') {
    return <StatusBadge active={value} />;
  }

  if (typeof value === 'string') {
    const normalized = value.trim().toLowerCase();

    if (normalized === 'yes' || normalized === 'có' || normalized === 'true') {
      return <StatusBadge active={true} trueText="Yes" falseText="No" />;
    }

    if (normalized === 'no' || normalized === 'không' || normalized === 'false') {
      return <StatusBadge active={false} trueText="Yes" falseText="No" />;
    }

    if (normalized === 'on' || normalized === 'bật') {
      return <StatusBadge active={true} trueText="On" falseText="Off" />;
    }

    if (normalized === 'off' || normalized === 'tắt') {
      return <StatusBadge active={false} trueText="On" falseText="Off" />;
    }

    return tx(value);
  }

  return value;
}
  function renderField(field: FieldConfig) {
    return (
      <label key={field.name} className="block">
        <span className="mb-1 block text-sm font-semibold text-slate-700">
          {tx(field.label)}
        </span>

        {field.type === 'textarea' ? (
          <Textarea
            value={form[field.name] ?? ''}
            onChange={(e) => setForm({ ...form, [field.name]: e.target.value })}
            placeholder={field.placeholder ? tx(field.placeholder) : undefined}
            rows={4}
            disabled={field.disabled}
          />
        ) : field.type === 'checkbox' ? (
          <input
            type="checkbox"
            checked={Boolean(form[field.name])}
            onChange={(e) => setForm({ ...form, [field.name]: e.target.checked })}
            className="h-5 w-5"
            disabled={field.disabled}
          />
        ) : (
          <Input
            type={field.type === 'number' ? 'number' : 'text'}
            value={form[field.name] ?? ''}
            onChange={(e) =>
              setForm({
                ...form,
                [field.name]: field.type === 'number' ? Number(e.target.value) : e.target.value
              })
            }
            placeholder={field.placeholder ? tx(field.placeholder) : undefined}
            disabled={field.disabled}
          />
        )}
      </label>
    );
  }

 const rows = items.map((item) => [
  ...config.columns.map((col) => {
    const value = col.render ? col.render(item) : item[col.key];

    return renderCellValue(value);
  }),

  <div className="flex gap-2" key="actions">
      <Button variant="secondary" className="px-3 py-2" onClick={() => startEdit(item)}>
        {tx('Sửa')}
      </Button>

      {config.softDelete !== false && (
        <Button variant="danger" className="px-3 py-2" onClick={() => deactivate(item)}>
          {tx('Ẩn')}
        </Button>
      )}
    </div>
  ]);

  const extraFormActions = config.extraFormActions?.(form, setForm);
  const hasPlacedExtraActions =
    Boolean(config.extraFormActionsAfterField) &&
    config.fields.some((field) => field.name === config.extraFormActionsAfterField);

  return (
    <div>
      <PageHeader title={config.title} description={config.description} />

      <div className="grid gap-5 xl:grid-cols-[420px_1fr]">
        <Card>
          <h2 className="mb-4 font-bold text-slate-900">
            {tx(editing ? 'Cập nhật dữ liệu' : 'Thêm dữ liệu')}
          </h2>

          <form className="space-y-3" onSubmit={submit}>
            {config.fields.map((field) => (
              <div key={field.name} className="space-y-2">
                {renderField(field)}

                {config.extraFormActionsAfterField === field.name && extraFormActions}
              </div>
            ))}

            {!hasPlacedExtraActions && extraFormActions}

            {error && <p className="text-sm text-rose-600">{tx(error)}</p>}

            <div className="flex gap-2">
              <Button type="submit">
                {tx(editing ? 'Lưu thay đổi' : 'Tạo mới')}
              </Button>

              {editing && (
                <Button type="button" variant="secondary" onClick={resetForm}>
                  {tx('Hủy')}
                </Button>
              )}
            </div>
          </form>
        </Card>

       <DataTable
  headers={[...config.columns.map((c) => c.label), 'Thao tác']}
  rows={rows}
/>
      </div>
    </div>
  );
}