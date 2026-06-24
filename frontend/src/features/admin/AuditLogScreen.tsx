import { FormEvent, useEffect, useState } from 'react';
import { endpoints } from '../../api/endpoints';
import { http, unwrap } from '../../api/http';
import { PageHeader } from '../../components/layout/PageHeader';
import { Button } from '../../components/ui/Button';
import { DataTable } from '../../components/ui/DataTable';
import { Input } from '../../components/ui/Input';
import { PaginationBar } from '../../components/ui/PaginationBar';
import { AuditLogDTO, PagedResult } from '../../types';

const PAGE_SIZE = 25;

export default function AuditLogScreen() {
  const [page, setPage] = useState(1);
  const [actorType, setActorType] = useState('');
  const [action, setAction] = useState('');
  const [result, setResult] = useState<PagedResult<AuditLogDTO>>({
    items: [],
    page: 1,
    pageSize: PAGE_SIZE,
    totalItems: 0,
    totalPages: 0
  });

  async function load(targetPage = page) {
    const response = await http.get(endpoints.adminAuditLogs, {
      params: {
        page: targetPage,
        pageSize: PAGE_SIZE,
        actorType: actorType || undefined,
        action: action || undefined
      }
    });
    const data = unwrap<PagedResult<AuditLogDTO>>(response);
    setResult(data);
    setPage(data.page);
  }

  useEffect(() => {
    void load(page);
  }, [page]);

  function filter(event: FormEvent) {
    event.preventDefault();
    if (page === 1) void load(1);
    else setPage(1);
  }

  return (
    <div>
      <PageHeader
        title="Audit Log"
        description="Theo dõi các thao tác ghi dữ liệu của Admin, Vendor, Guest và scheduler."
      />

      <form onSubmit={filter} className="mb-4 grid gap-3 rounded-3xl bg-white p-4 shadow-sm md:grid-cols-[180px_1fr_auto]">
        <select
          className="rounded-2xl border border-slate-200 px-4 py-3 text-sm"
          value={actorType}
          onChange={(event) => setActorType(event.target.value)}
        >
          <option value="">Tất cả actor</option>
          <option value="Admin">Admin</option>
          <option value="Vendor">Vendor</option>
          <option value="Guest">Guest</option>
          <option value="System">System</option>
        </select>
        <Input
          placeholder="Lọc theo action hoặc API path"
          value={action}
          onChange={(event) => setAction(event.target.value)}
        />
        <Button>Lọc</Button>
      </form>

      <DataTable
        headers={['Id', 'Time', 'Actor', 'Action', 'Entity', 'Status / Trace', 'IP']}
        rows={result.items.map((item) => [
          item.auditLogId,
          new Date(item.createdAt).toLocaleString(),
          `${item.actorType}${item.actorId ? ` #${item.actorId}` : ''}${item.guestSessionId ? ` ${item.guestSessionId.slice(0, 8)}…` : ''}`,
          item.action,
          `${item.entityType || ''}${item.entityId ? ` #${item.entityId}` : ''}`,
          item.details ? JSON.stringify(item.details) : '',
          item.ipAddress || ''
        ])}
      />

      <PaginationBar
        page={result.page}
        pageSize={result.pageSize}
        totalItems={result.totalItems}
        totalPages={result.totalPages}
        onPageChange={setPage}
      />
    </div>
  );
}
