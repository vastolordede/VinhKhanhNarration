import { FormEvent, ReactNode, useEffect, useMemo, useState } from 'react';
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
import { PaginationBar } from '../../components/ui/PaginationBar';

type OptionValueType = 'number' | 'string';

type FieldErrors = Record<string, string>;

export type FieldConfig = {
  name: string;
  label: string;
  type?: 'text' | 'number' | 'textarea' | 'checkbox' | 'select';
  placeholder?: string;
  disabled?: boolean;
  nullable?: boolean;
  required?: boolean;

  optionEndpoint?: string;
  optionValueKey?: string;
  optionLabelKey?: string;
  optionValueType?: OptionValueType;
  optionLabel?: (option: any) => string;
  emptyLabel?: string;
  includeInactiveOptions?: boolean;
};

export type ResourceConfig = {
  title: string;
  description?: string;
  endpoint: string;
  createEndpoint?: string;
  idKey?: string;
  fields: FieldConfig[];
  columns: { key: string; label: string; render?: (row: any) => ReactNode }[];
  softDelete?: boolean;
  preparePayload?: (payload: Record<string, any>, editing: any | null) => Record<string, any>;
  validate?: (form: Record<string, any>, editing: any | null) => FieldErrors;
  extraRowActions?: (row: any) => ReactNode;
  pageSize?: number;
disablePagination?: boolean;

  extraFormActions?: (
    form: Record<string, any>,
    setForm: Dispatch<SetStateAction<Record<string, any>>>
  ) => ReactNode;

  extraFormActionsAfterField?: string;
};

export function getCurrentAdminId(): number | null {
  try {
    const raw = localStorage.getItem('adminUser');

    if (raw) {
      const admin = JSON.parse(raw) as Record<string, any>;

      const adminId = Number(
        admin.adminId ??
          admin.AdminId ??
          admin.id ??
          admin.Id ??
          admin.admin_id
      );

      if (Number.isFinite(adminId) && adminId > 0) {
        return adminId;
      }
    }

    const token = localStorage.getItem('adminToken');

    if (token) {
      const payloadPart = token.split('.')[1];

      if (payloadPart) {
        const payload = JSON.parse(atob(payloadPart));

        const adminId = Number(
          payload.adminId ??
            payload.AdminId ??
            payload.nameid ??
            payload.sub
        );

        if (Number.isFinite(adminId) && adminId > 0) {
          return adminId;
        }
      }
    }

    return null;
  } catch {
    return null;
  }
}

export default function SimpleResourcePage({ config }: { config: ResourceConfig }) {
  const [items, setItems] = useState<any[]>([]);
  const [editing, setEditing] = useState<any | null>(null);
  const [form, setForm] = useState<Record<string, any>>({});
  const [lookupOptions, setLookupOptions] = useState<Record<string, any[]>>({});
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [tablePage, setTablePage] = useState(1);

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
    setTablePage(1);
    load();
  }, [config.endpoint]);

  const optionSignature = useMemo(
    () =>
      config.fields
        .filter((field) => field.type === 'select' && field.optionEndpoint)
        .map((field) => `${field.name}:${field.optionEndpoint}`)
        .join('|'),
    [config.fields]
  );

  useEffect(() => {
    const selectFields = config.fields.filter(
      (field) => field.type === 'select' && field.optionEndpoint
    );

    if (!selectFields.length) {
      setLookupOptions({});
      return;
    }

    let cancelled = false;

    async function loadOptions() {
      try {
        const entries = await Promise.all(
          selectFields.map(async (field) => {
            const data = await getList<any>(field.optionEndpoint!);

            const activeData = field.includeInactiveOptions
              ? data
              : data.filter((item) => item.isActive !== false);

            return [field.name, activeData] as const;
          })
        );

        if (!cancelled) {
          setLookupOptions(Object.fromEntries(entries));
        }
      } catch {
        if (!cancelled) {
          setError('Không tải được dữ liệu dropdown. Kiểm tra backend endpoint lookup.');
        }
      }
    }

    loadOptions();

    return () => {
      cancelled = true;
    };
  }, [optionSignature, config.fields]);

  function getRowId(row: any) {
    if (config.idKey && row[config.idKey] !== undefined) {
      return row[config.idKey];
    }

    const key = Object.keys(row).find((k) => k.toLowerCase().endsWith('id'));
    return key ? row[key] : undefined;
  }

  function startEdit(row: any) {
    setEditing(row);
    setForm(row);
    setError(null);
    setSuccess(null);
    setFieldErrors({});
  }

  function resetForm() {
    setEditing(null);
    setForm({});
    setError(null);
    setSuccess(null);
    setFieldErrors({});
  }

  function normalizePayload(payload: Record<string, any>) {
    const next = { ...payload };

    for (const field of config.fields) {
      if (next[field.name] === '') {
        if (field.nullable) {
          next[field.name] = null;
        } else {
          delete next[field.name];
        }
      }
    }

    return next;
  }

  function isEmpty(value: any) {
    return value === undefined || value === null || value === '';
  }

  function focusField(fieldName: string) {
    setTimeout(() => {
      const element = document.querySelector(`[name="${fieldName}"]`) as HTMLElement | null;
      element?.focus();
      element?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 0);
  }

  function validateForm() {
    const errors: FieldErrors = {};

    for (const field of config.fields) {
      if (!field.required) continue;

      if (field.type === 'checkbox') continue;

      if (isEmpty(form[field.name])) {
        errors[field.name] = `${field.label} is required.`;
      }
    }

    const customErrors = config.validate?.(form, editing) ?? {};

    return {
      ...errors,
      ...customErrors
    };
  }

  function getBackendFieldErrors(err: any): FieldErrors | null {
    const data = err?.response?.data;

    const apiFieldErrors =
      data?.fieldErrors ||
      data?.data?.fieldErrors ||
      data?.errors;

    if (!apiFieldErrors || typeof apiFieldErrors !== 'object') {
      return null;
    }

    return apiFieldErrors as FieldErrors;
  }

  function getBackendMessage(err: any) {
    const data = err?.response?.data;

    return (
      data?.message ||
      data?.title ||
      data?.data?.message ||
      err?.message ||
      'Không lưu được dữ liệu. Kiểm tra dữ liệu nhập hoặc API.'
    );
  }

  async function submit(e: FormEvent) {
    e.preventDefault();

    setError(null);
    setSuccess(null);
    setFieldErrors({});

    const frontendErrors = validateForm();

    if (Object.keys(frontendErrors).length > 0) {
      setFieldErrors(frontendErrors);
      focusField(Object.keys(frontendErrors)[0]);
      return;
    }

    if (editing) {
      const confirmed = window.confirm(
        tx('Are you sure you want to save these changes?')
      );

      if (!confirmed) return;
    }

    try {
      const normalizedPayload = normalizePayload(form);

      const payload = config.preparePayload
        ? config.preparePayload(normalizedPayload, editing)
        : normalizedPayload;

      if (editing) {
        await updateItem(`${config.endpoint}/${getRowId(editing)}`, payload);
      } else {
        await createItem(config.createEndpoint ?? config.endpoint, payload);
      }

      setEditing(null);
      setForm({});
      setFieldErrors({});
      setError(null);
      setSuccess(editing ? 'Updated successfully.' : 'Created successfully.');

      await load();
    } catch (err: any) {
      console.error('Save failed:', err);

      const backendFieldErrors = getBackendFieldErrors(err);

      if (backendFieldErrors && Object.keys(backendFieldErrors).length > 0) {
        setFieldErrors(backendFieldErrors);
        focusField(Object.keys(backendFieldErrors)[0]);
      }

      setError(getBackendMessage(err));
    }
  }

  async function deactivate(row: any) {
    const confirmed = window.confirm(tx('Are you sure you want to hide this record?'));
    if (!confirmed) return;

    try {
      await patchItem(`${config.endpoint}/${getRowId(row)}/deactivate`);

      if (editing && getRowId(editing) === getRowId(row)) {
        resetForm();
      }

      setSuccess('Hidden successfully.');
      await load();
    } catch (err: any) {
      setError(getBackendMessage(err));
    }
  }

  async function restore(row: any) {
    const confirmed = window.confirm(tx('Are you sure you want to restore this record?'));
    if (!confirmed) return;

    try {
      await patchItem(`${config.endpoint}/${getRowId(row)}/restore`);

      if (editing && getRowId(editing) === getRowId(row)) {
        resetForm();
      }

      setSuccess('Restored successfully.');
      await load();
    } catch (err: any) {
      setError(getBackendMessage(err));
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

  function getOptionValue(field: FieldConfig, option: any) {
    const valueKey = field.optionValueKey ?? 'id';
    return option[valueKey];
  }

  function getOptionLabel(field: FieldConfig, option: any) {
    if (field.optionLabel) {
      return field.optionLabel(option);
    }

    const labelKey = field.optionLabelKey ?? 'name';

    const label =
      option[labelKey] ??
      option.name ??
      option.title ??
      option.placeName ??
      option.dishName ??
      option.categoryName ??
      option.languageName ??
      option.translatedTitle;

    const value = getOptionValue(field, option);

    return value !== undefined && label !== undefined
      ? `${value} - ${label}`
      : String(label ?? value ?? '');
  }

  function findSelectField(name: string) {
    return config.fields.find((field) => field.name === name && field.type === 'select');
  }

  function getLookupLabel(fieldName: string, value: any) {
    const field = findSelectField(fieldName);

    if (!field || value === null || value === undefined || value === '') {
      return value;
    }

    const option = (lookupOptions[fieldName] ?? []).find(
      (item) => String(getOptionValue(field, item)) === String(value)
    );

    return option ? getOptionLabel(field, option) : value;
  }

  function setFieldValue(field: FieldConfig, rawValue: string | boolean) {
    setError(null);
    setSuccess(null);

    if (fieldErrors[field.name]) {
      const nextErrors = { ...fieldErrors };
      delete nextErrors[field.name];
      setFieldErrors(nextErrors);
    }

    if (field.type === 'checkbox') {
      setForm({ ...form, [field.name]: Boolean(rawValue) });
      return;
    }

    if (typeof rawValue !== 'string') return;

    if (field.type === 'number') {
      setForm({
        ...form,
        [field.name]: rawValue === '' ? (field.nullable ? null : '') : Number(rawValue)
      });
      return;
    }

    if (field.type === 'select') {
      const valueType = field.optionValueType ?? 'number';

      setForm({
        ...form,
        [field.name]:
          rawValue === ''
            ? field.nullable
              ? null
              : ''
            : valueType === 'number'
              ? Number(rawValue)
              : rawValue
      });

      return;
    }

    setForm({ ...form, [field.name]: rawValue });
  }

  function getFieldClassName(fieldName: string) {
    return fieldErrors[fieldName]
      ? 'border-rose-400 focus:border-rose-500 focus:ring-4 focus:ring-rose-50'
      : '';
  }

  function renderField(field: FieldConfig) {
    const fieldError = fieldErrors[field.name];

    return (
      <label key={field.name} className="block">
        <span className="mb-1 block text-sm font-semibold text-slate-700">
          {tx(field.label)}
          {field.required && <span className="ml-1 text-rose-500">*</span>}
        </span>

        {field.type === 'textarea' ? (
          <Textarea
            name={field.name}
            value={form[field.name] ?? ''}
            onChange={(e) => setFieldValue(field, e.target.value)}
            placeholder={field.placeholder ? tx(field.placeholder) : undefined}
            rows={4}
            disabled={field.disabled}
            className={getFieldClassName(field.name)}
          />
        ) : field.type === 'checkbox' ? (
          <input
            name={field.name}
            type="checkbox"
            checked={Boolean(form[field.name])}
            onChange={(e) => setFieldValue(field, e.target.checked)}
            className="h-5 w-5"
            disabled={field.disabled}
          />
        ) : field.type === 'select' ? (
          <select
            name={field.name}
            value={form[field.name] ?? ''}
            onChange={(e) => setFieldValue(field, e.target.value)}
            disabled={field.disabled}
            className={`w-full rounded-2xl border bg-white px-4 py-3 text-sm outline-none transition focus:border-teal-500 focus:ring-4 focus:ring-teal-50 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500 ${
              fieldError ? 'border-rose-400 focus:border-rose-500 focus:ring-rose-50' : 'border-slate-200'
            }`}
          >
            <option value="">{tx(field.emptyLabel ?? 'Chọn dữ liệu')}</option>

            {(lookupOptions[field.name] ?? []).map((option) => {
              const value = getOptionValue(field, option);

              return (
                <option key={String(value)} value={String(value)}>
                  {getOptionLabel(field, option)}
                </option>
              );
            })}
          </select>
        ) : (
          <Input
            name={field.name}
            type={field.type === 'number' ? 'number' : 'text'}
            value={form[field.name] ?? ''}
            onChange={(e) => setFieldValue(field, e.target.value)}
            placeholder={field.placeholder ? tx(field.placeholder) : undefined}
            disabled={field.disabled}
            className={getFieldClassName(field.name)}
          />
        )}

        {fieldError && (
          <p className="mt-1 text-xs font-medium text-rose-600">
            {tx(fieldError)}
          </p>
        )}
      </label>
    );
  }
const pageSize = config.pageSize ?? 10;
const totalItems = items.length;
const totalPages = Math.ceil(totalItems / pageSize);
const shouldPaginate = !config.disablePagination && totalItems > pageSize;

const safeTablePage = Math.min(
  Math.max(tablePage, 1),
  Math.max(totalPages, 1)
);

const visibleItems = shouldPaginate
  ? items.slice((safeTablePage - 1) * pageSize, safeTablePage * pageSize)
  : items;
  const rows = visibleItems.map((item) => {
    const isInactive = item.isActive === false;
    const hasActiveStatus = typeof item.isActive === 'boolean';

    return [
      ...config.columns.map((col) => {
        const value = col.render ? col.render(item) : getLookupLabel(col.key, item[col.key]);

        return renderCellValue(value);
      }),

      <div className="flex gap-2" key="actions">
        <Button
          variant="secondary"
          className="px-3 py-2"
          onClick={() => startEdit(item)}
        >
          {tx('Edit')}
        </Button>

        {config.softDelete !== false && (
          <Button
            variant={hasActiveStatus && isInactive ? 'primary' : 'danger'}
            className="px-3 py-2"
            onClick={() => {
              if (hasActiveStatus && isInactive) {
                restore(item);
              } else {
                deactivate(item);
              }
            }}
          >
            {tx(hasActiveStatus && isInactive ? 'Restore' : 'Hide')}
          </Button>
        )}
        {config.extraRowActions?.(item)}
      </div>
    ];
  });

  const rowClassNames = visibleItems.map((item) =>
    item.isActive === false ? 'bg-slate-100 text-slate-400' : ''
  );

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

            {success && (
              <p className="rounded-2xl bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-700">
                {tx(success)}
              </p>
            )}

            {error && (
              <p className="rounded-2xl bg-rose-50 px-4 py-3 text-sm font-semibold text-rose-600">
                {tx(error)}
              </p>
            )}

            <div className="flex gap-2">
              <Button type="submit">
                {tx(editing ? 'Save Changes' : 'Create')}
              </Button>

              {editing && (
                <Button type="button" variant="secondary" onClick={resetForm}>
                  {tx('Hủy')}
                </Button>
              )}
            </div>
          </form>
        </Card>

        <div>
  <DataTable
    headers={[...config.columns.map((c) => tx(c.label)), tx('Actions')]}
    rows={rows}
    rowClassNames={rowClassNames}
  />

  {shouldPaginate && (
    <PaginationBar
      page={safeTablePage}
      pageSize={pageSize}
      totalItems={totalItems}
      totalPages={totalPages}
      onPageChange={setTablePage}
    />
  )}
</div>
      </div>
    </div>
  );
}