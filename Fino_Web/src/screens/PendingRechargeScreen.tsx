import { useMemo, useState, type ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  RefreshCw, Search as SearchIcon, SlidersHorizontal, ShieldCheck, Send,
  CheckCircle2, Undo2, FileClock, Clock, AlertTriangle, Timer, Split, ChevronDown, RotateCcw,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { loadSession } from '@/lib/storage';
import { inr, dateTime } from '@/lib/format';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { Modal } from '@/components/ui/Modal';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };
const ADMIN_ROLES = [1, 2, 3, 6]; // admin / super-dist / dist / support see the user + provider columns
const PAGE_SIZE = 25;

interface Filters {
  serviceId: string;
  operatorId: string;
  userId: string;
  search: string;
}
const EMPTY: Filters = { serviceId: '0', operatorId: '0', userId: '', search: '' };

interface PendingResult {
  rows: Row[];
  count: number;
}

// Endpoints (all already implemented in Fino_API TransactionProcessingController).
const EP = {
  list: 'TransactionReport/SelectPendingTransactionWithFilter',
  statusCheckAll: 'TransactionProcessing/StatusCheckAll',   // batch: re-enquire provider STATUS
  successAll: 'TransactionProcessing/SuccessAll',           // batch: force SUCCESS
  refundAll: 'TransactionProcessing/RefundAll',             // batch: refund wallet
  resendAll: 'TransactionProcessing/ResendAll',             // batch: re-dispatch
  statusCheckOne: 'TransactionProcessing/GetRechargeTransctionStatus', // single: with status check
  resendOne: 'TransactionProcessing/Resend',                // single: without status check (re-dispatch)
  shiftOne: 'TransactionProcessing/ShiftRecharge',          // single: re-dispatch via a chosen provider
  shiftAll: 'TransactionProcessing/ShiftAll',               // batch: re-dispatch via a chosen provider
};

const rowId = (r: Row) => Number(getField(r, 'TransactionMasterID', 'TransactionID', 'ID') ?? 0);

/** "2d 4h" / "3h 20m" / "12m" — how long it has been pending. */
function pendingAge(dateStr: string): string {
  const t = new Date(dateStr).getTime();
  if (!Number.isFinite(t)) return '—';
  const mins = Math.max(Math.floor((Date.now() - t) / 60_000), 0);
  const d = Math.floor(mins / 1440);
  const h = Math.floor((mins % 1440) / 60);
  const m = mins % 60;
  if (d > 0) return `${d}d ${h}h`;
  if (h > 0) return `${h}h ${m}m`;
  return `${m}m`;
}

/** Older than 30 min is worth highlighting — the scheduler should have settled it by then. */
function isStale(dateStr: string): boolean {
  const t = new Date(dateStr).getTime();
  return Number.isFinite(t) && Date.now() - t > 30 * 60_000;
}

async function fetchPending(a: Filters, page: number): Promise<PendingResult> {
  const res = await apiPost<unknown>(EP.list, {
    TakeCount: PAGE_SIZE,
    SkipCount: page * PAGE_SIZE,
    Search: a.search,
    SortColumnName: '',
    SortDirection: '',
    FromDate: '',
    ToDate: '',
    Status: 'PENDING',
    ReportBy: 'Inline',
    UserID: Number(a.userId || 0),
    ServiceID: Number(a.serviceId || 0),
    OperatorID: Number(a.operatorId || 0),
    ProviderID: 0,
  });
  if (!res.ok) return { rows: [], count: 0 };
  const d = res.data as Record<string, unknown> | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Record<string, unknown>, 'list', 'List', 'Data', 'Items');
  const rows = Array.isArray(listRaw) ? (listRaw as Row[]) : [];
  const count = Array.isArray(d) ? rows.length : Number(getField(d as Record<string, unknown>, 'Count', 'count') ?? rows.length);
  return { rows, count };
}

export function PendingRechargeScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const userTypeId = Number(loadSession()?.user?.UserTypeID ?? 0);
  const isAdmin = ADMIN_ROLES.includes(userTypeId);

  const [draft, setDraft] = useState<Filters>(EMPTY);
  const [applied, setApplied] = useState<Filters>(EMPTY);
  const [page, setPage] = useState(0);
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [busy, setBusy] = useState<string | null>(null); // which bulk action is running
  const [rowBusy, setRowBusy] = useState<number | null>(null);
  const [logId, setLogId] = useState<number | null>(null);
  // Success/Refund move money — show a confirm modal first.
  const [confirm, setConfirm] = useState<{ action: string; endpoint: string; verb: string; danger?: boolean } | null>(null);
  // Shift needs a provider chosen first. `ids` is the set of rows the shift will apply to.
  const [shift, setShift] = useState<{ ids: number[] } | null>(null);
  const [shiftProviderId, setShiftProviderId] = useState('');
  const [showFilters, setShowFilters] = useState(false); // filter panel stays collapsed until clicked

  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: services = [] } = useQuery({
    queryKey: ['svc-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });
  const { data: allOperators = [] } = useQuery({
    queryKey: ['op-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Operator/GetAll', LIST_PAGE),
  });
  const { data: users = [] } = useQuery({
    enabled: isAdmin, queryKey: ['user-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', USER_PAGE),
  });
  const { data: providers = [] } = useQuery({
    enabled: isAdmin, queryKey: ['prov-opts'], staleTime: 300_000,
    queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAGE),
  });

  const { data, isFetching, refetch, dataUpdatedAt } = useQuery({
    queryKey: ['pending-recharge', applied, page],
    queryFn: () => fetchPending(applied, page),
    refetchInterval: 60_000, // scheduler settles every 5 min; the page keeps itself fresh
  });
  const rows = data?.rows ?? [];
  const total = data?.count ?? 0;

  const operatorOptions = useMemo(
    () => allOperators.filter((o) => String(getField(o, 'ServiceID') ?? '') === String(draft.serviceId)),
    [allOperators, draft.serviceId],
  );

  // Searchable option lists (with a leading "All …" entry that maps to the all-value).
  const serviceOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '0', label: 'All services' },
    ...services.map((s, i) => ({ value: String(getField(s, 'ServiceID') ?? i), label: String(getField(s, 'ServiceName', 'Name') ?? '') })),
  ], [services]);
  const operatorOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '0', label: 'All operators' },
    ...operatorOptions.map((o, i) => ({ value: String(getField(o, 'OperatorID') ?? i), label: String(getField(o, 'OperatorName', 'Name') ?? '') })),
  ], [operatorOptions]);
  const userOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '', label: 'All users' },
    ...users.map((u, i) => ({ value: String(getField(u, 'UserMasterID', 'UserID') ?? i), label: String(getField(u, 'FirmName', 'PersonName', 'Name') ?? '') })),
  ], [users]);

  const pendingAmount = useMemo(
    () => rows.reduce((sum, r) => sum + (Number(getField(r, 'Amount')) || 0), 0),
    [rows],
  );

  const pageIds = rows.map(rowId).filter((id) => id > 0);
  const allChecked = pageIds.length > 0 && pageIds.every((id) => selected.has(id));
  const selectedIds = pageIds.filter((id) => selected.has(id));

  const { data: logs = [], isFetching: logsLoading } = useQuery({
    queryKey: ['status-logs', logId],
    queryFn: () => fetchList('ProviderRequestResponseLog/GetByTransaction', { TransactionID: logId }),
    enabled: logId !== null,
  });

  const runSearch = () => { setPage(0); setSelected(new Set()); setApplied(draft); };
  const resetFilters = () => { setDraft(EMPTY); setApplied(EMPTY); setPage(0); setSelected(new Set()); };

  const toggleAll = () => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (allChecked) pageIds.forEach((id) => next.delete(id));
      else pageIds.forEach((id) => next.add(id));
      return next;
    });
  };
  const toggleOne = (id: number) => {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  };

  const afterMutation = async () => {
    setSelected(new Set());
    await qc.invalidateQueries({ queryKey: ['pending-recharge'] });
  };

  // Bulk action over the selected rows (or the whole page if none are ticked).
  async function runBulk(action: string, endpoint: string, verb: string) {
    const ids = (selectedIds.length > 0 ? selectedIds : pageIds);
    if (ids.length === 0) { toast.info('No pending recharges.'); return; }
    setBusy(action);
    try {
      const before = total;
      const res = await apiPost<unknown>(endpoint, ids.map((id) => ({ TransactionID: id })));
      if (!res.ok) { toast.error(res.message || `${verb} failed.`); return; }
      const after = await fetchPending(applied, page);
      const settled = Math.max(before - after.count, 0);
      toast.success(
        settled > 0
          ? `${verb}: probed ${ids.length}, ${settled} settled/cleared.`
          : `${verb}: probed ${ids.length} — still pending (provider has not given a final status).`,
      );
      await afterMutation();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : `${verb} failed.`);
    } finally {
      setBusy(null);
    }
  }

  // Single-row action (with / without status check).
  async function runRow(id: number, endpoint: string, verb: string) {
    setRowBusy(id);
    try {
      const res = await apiPost<unknown>(endpoint, { ID: id });
      if (!res.ok) { toast.error(res.message || `${verb} failed.`); return; }
      toast.success(`Txn ${id}: ${verb} done.`);
      await afterMutation();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : `${verb} failed.`);
    } finally {
      setRowBusy(null);
    }
  }

  // Shift the chosen rows onto a specific provider. One id → single endpoint, many → batch.
  async function runShift() {
    const ids = shift?.ids ?? [];
    const providerId = Number(shiftProviderId || 0);
    if (ids.length === 0 || providerId <= 0) return;
    setBusy('shift');
    try {
      const res = ids.length === 1
        ? await apiPost<unknown>(EP.shiftOne, { TransactionID: ids[0], ProviderID: providerId })
        : await apiPost<unknown>(EP.shiftAll, ids.map((id) => ({ TransactionID: id, ProviderID: providerId })));
      if (!res.ok) { toast.error(res.message || 'Shift failed.'); return; }
      const name = providerName(providerId);
      toast.success(`Shifted ${ids.length} recharge(s) to ${name}.`);
      setShift(null);
      setShiftProviderId('');
      await afterMutation();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Shift failed.');
    } finally {
      setBusy(null);
    }
  }

  const providerName = (id: number) =>
    String(getField(providers.find((p) => Number(getField(p, 'ProviderID')) === id) ?? {}, 'ProviderName') ?? id);

  const actionCount = selectedIds.length > 0 ? selectedIds.length : pageIds.length;
  const from = total === 0 ? 0 : page * PAGE_SIZE + 1;
  const to = Math.min((page + 1) * PAGE_SIZE, total);

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <div className="flex items-center gap-3">
            <span className="hidden items-center gap-1.5 rounded-full bg-brand-50 px-3 py-1 text-xs font-medium text-brand-700 ring-1 ring-inset ring-brand-500/20 sm:inline-flex">
              <Timer className="size-3.5" />
              Auto status-check: every 5 min
            </span>
            <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
              <RefreshCw className="size-4" />
              Refresh
            </Button>
          </div>
        }
      />

      {/* One unified work panel — slim KPI strip, inline filters, table, pager */}
      <Card className="overflow-hidden">
        {/* Metric strip + filter toggle */}
        <div className="flex flex-wrap items-center justify-between gap-x-6 gap-y-3 border-b border-slate-100 px-5 py-3.5">
          <div className="flex items-center gap-5">
            <Metric label="Pending" value={String(total)} />
            <span className="h-8 w-px bg-slate-200/80" />
            <Metric label="This page" value={String(rows.length)} />
            <span className="h-8 w-px bg-slate-200/80" />
            <Metric label="Amount" value={inr(pendingAmount)} />
            <span className="h-8 w-px bg-slate-200/80" />
            <Metric label="Selected" value={String(selectedIds.length)} accent />
          </div>
          <div className="flex items-center gap-3">
            <span className="hidden text-[11px] text-slate-400 md:inline">
              {dataUpdatedAt ? `Updated ${new Date(dataUpdatedAt).toLocaleTimeString()}` : 'Loading…'}
            </span>
            <button
              type="button"
              onClick={() => setShowFilters((s) => !s)}
              aria-expanded={showFilters}
              className={cn(
                'inline-flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors',
                showFilters
                  ? 'border-brand-200 bg-brand-50 text-brand-700'
                  : 'border-slate-200 text-slate-600 hover:bg-slate-50',
              )}
            >
              <SlidersHorizontal className="size-3.5" />
              Filters
              <ChevronDown className={cn('size-3.5 transition-transform', showFilters && 'rotate-180')} />
            </button>
          </div>
        </div>

        {/* Collapsible filter drawer */}
        {showFilters && (
          <form
            onSubmit={(e) => { e.preventDefault(); runSearch(); }}
            className="grid grid-cols-1 gap-4 border-b border-slate-100 bg-slate-50/60 px-5 py-4 sm:grid-cols-2 lg:grid-cols-4"
          >
            <Autocomplete
              label="Service"
              options={serviceOpts}
              value={draft.serviceId}
              onChange={(v) => upd({ serviceId: String(v || '0'), operatorId: '0' })}
            />
            <Autocomplete
              label="Operator"
              options={operatorOpts}
              value={draft.operatorId}
              onChange={(v) => upd({ operatorId: String(v || '0') })}
            />
            {isAdmin && (
              <Autocomplete
                label="Entity / user"
                options={userOpts}
                value={draft.userId}
                onChange={(v) => upd({ userId: String(v) })}
              />
            )}
            <Input
              label="Consumer number"
              placeholder="Mobile / account no."
              value={draft.search}
              onChange={(e) => upd({ search: e.target.value })}
            />
            <div className="flex items-end gap-2">
              <Button type="submit" loading={isFetching} className="flex-1">
                <SearchIcon className="size-4" />
                Search
              </Button>
              <Button type="button" variant="outline" onClick={resetFilters}>
                <RotateCcw className="size-4" />
                Reset
              </Button>
            </div>
          </form>
        )}

        {/* Bulk action bar */}
        <div className="flex flex-wrap items-center gap-2 border-b border-slate-100 px-5 py-3">
          <span className="mr-1 text-xs text-slate-500">
            {selectedIds.length > 0 ? `${selectedIds.length} selected` : 'Acts on all rows on this page'}
          </span>
          <Button size="sm" variant="primary" disabled={actionCount === 0} loading={busy === 'check'}
            onClick={() => runBulk('check', EP.statusCheckAll, 'Status check')}>
            <ShieldCheck className="size-4" />
            Check status ({actionCount})
          </Button>
          <Button size="sm" variant="outline" disabled={actionCount === 0} loading={busy === 'resend'}
            onClick={() => runBulk('resend', EP.resendAll, 'Resend')}>
            <Send className="size-4" />
            Resend
          </Button>
          {isAdmin && (
            <Button size="sm" variant="outline" disabled={actionCount === 0} loading={busy === 'shift'}
              onClick={() => { setShiftProviderId(''); setShift({ ids: selectedIds.length > 0 ? selectedIds : pageIds }); }}>
              <Split className="size-4" />
              Shift
            </Button>
          )}
          <Button size="sm" variant="outline" disabled={actionCount === 0} loading={busy === 'success'}
            onClick={() => setConfirm({ action: 'success', endpoint: EP.successAll, verb: 'Mark success' })}>
            <CheckCircle2 className="size-4 text-success-600" />
            Success
          </Button>
          <Button size="sm" variant="danger" disabled={actionCount === 0} loading={busy === 'refund'}
            onClick={() => setConfirm({ action: 'refund', endpoint: EP.refundAll, verb: 'Refund', danger: true })}>
            <Undo2 className="size-4" />
            Refund
          </Button>
        </div>

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="w-10 px-3 py-3">
                  <input type="checkbox" checked={allChecked} onChange={toggleAll} className="size-4 accent-brand-600" aria-label="Select all" />
                </th>
                {['#', 'Date', 'Pending since', 'Name', 'Number', 'Operator', 'Amount', 'Status', ...(isAdmin ? ['Provider', 'Request ID'] : []), 'Actions'].map((h) => (
                  <th key={h} className={cn('whitespace-nowrap px-4 py-3', h === 'Amount' ? 'text-right' : 'text-left')}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 6 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: isAdmin ? 12 : 10 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[110px]" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={isAdmin ? 12 : 10}>
                    <EmptyState title="No pending recharges" description="All recharges have settled, or nothing matches the filters. The scheduler also checks these every 5 minutes." />
                  </td>
                </tr>
              ) : (
                rows.map((row, i) => {
                  const id = rowId(row);
                  return (
                    <tr key={id} className={cn('border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60', selected.has(id) && 'bg-brand-50/40')}>
                      <td className="px-3 py-3">
                        <input type="checkbox" checked={selected.has(id)} onChange={() => toggleOne(id)} className="size-4 accent-brand-600" aria-label={`Select ${id}`} />
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-500">{page * PAGE_SIZE + i + 1}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{dateTime(String(getField(row, 'DOC', 'DateTime', 'Date') ?? ''))}</td>
                      <td className="whitespace-nowrap px-4 py-3">
                        {(() => {
                          const doc = String(getField(row, 'DOC', 'DateTime', 'Date') ?? '');
                          return (
                            <span className={cn(
                              'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium tabular-nums',
                              isStale(doc) ? 'bg-warning-50 text-warning-600 ring-1 ring-inset ring-warning-500/20' : 'bg-slate-100 text-slate-500',
                            )}>
                              <Clock className="size-3" />
                              {pendingAge(doc)}
                            </span>
                          );
                        })()}
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-700">{String(getField(row, 'FirmName', 'Name') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 font-medium text-slate-800">{String(getField(row, 'ConsumerNumber') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{String(getField(row, 'OperatorName') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-right tabular-nums font-medium text-slate-800">{inr(Number(getField(row, 'Amount')) || 0)}</td>
                      <td className="whitespace-nowrap px-4 py-3"><StatusBadge status={String(getField(row, 'Status') ?? 'PENDING')} /></td>
                      {isAdmin && (
                        <>
                          <td className="whitespace-nowrap px-4 py-3 text-slate-600">{String(getField(row, 'ProviderName') || '—')}</td>
                          <td className="whitespace-nowrap px-4 py-3 text-xs text-slate-400">{String(getField(row, 'ProviderRequestID') ?? '—')}</td>
                        </>
                      )}
                      <td className="whitespace-nowrap px-4 py-3">
                        <div className="flex items-center gap-1">
                          <IconBtn title="Status check (enquire with the provider)" busy={rowBusy === id} onClick={() => runRow(id, EP.statusCheckOne, 'Status check')}>
                            <ShieldCheck className="size-4" />
                          </IconBtn>
                          <IconBtn title="Resend (dispatch again)" busy={rowBusy === id} onClick={() => runRow(id, EP.resendOne, 'Resend')}>
                            <Send className="size-4" />
                          </IconBtn>
                          {isAdmin && (
                            <IconBtn title="Shift (send through another provider)" busy={rowBusy === id} onClick={() => { setShiftProviderId(''); setShift({ ids: [id] }); }}>
                              <Split className="size-4" />
                            </IconBtn>
                          )}
                          <IconBtn title="View log" onClick={() => setLogId(id)}>
                            <FileClock className="size-4" />
                          </IconBtn>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        <div className="flex items-center justify-between gap-3 border-t border-slate-100 px-5 py-3 text-xs text-slate-500">
          <span className="tabular-nums">{from}–{to} of {total}</span>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" disabled={page === 0 || isFetching} onClick={() => { setSelected(new Set()); setPage((p) => p - 1); }}>Prev</Button>
            <Button variant="outline" size="sm" disabled={to >= total || isFetching} onClick={() => { setSelected(new Set()); setPage((p) => p + 1); }}>Next</Button>
          </div>
        </div>
      </Card>

      {/* Confirm — Success/Refund change wallet balances, so require a one-step confirm */}
      <Modal open={confirm !== null} onClose={() => setConfirm(null)} title={confirm ? `${confirm.verb} — confirm` : ''}>
        {confirm && (
          <div className="space-y-4">
            <div className={cn(
              'flex items-start gap-3 rounded-lg p-3 text-sm',
              confirm.danger ? 'bg-danger-50 text-danger-600' : 'bg-warning-50 text-warning-600',
            )}>
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <p>
                {confirm.action === 'refund'
                  ? `The amount for ${actionCount} transaction(s) will be returned to the wallet. This action cannot be undone.`
                  : `${actionCount} transaction(s) will be marked SUCCESS manually — without confirming with the provider. Are you sure the recharge actually completed at the provider?`}
              </p>
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={() => setConfirm(null)}>Cancel</Button>
              <Button
                variant={confirm.danger ? 'danger' : 'primary'}
                size="sm"
                onClick={() => { const c = confirm; setConfirm(null); void runBulk(c.action, c.endpoint, c.verb); }}
              >
                Yes, {confirm.verb.toLowerCase()} ({actionCount})
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Shift — pick the provider to re-dispatch through. */}
      <Modal open={shift !== null} onClose={() => setShift(null)} title="Shift recharge">
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            {shift?.ids.length === 1
              ? `Transaction ${shift.ids[0]} will be sent again through the provider you choose, instead of the one the switching sequence picked.`
              : `${shift?.ids.length ?? 0} recharges will be sent again through the provider you choose, instead of the one the switching sequence picked.`}
          </p>
          <Select label="Provider" value={shiftProviderId} onChange={(e) => setShiftProviderId(e.target.value)}>
            <option value="">— choose a provider —</option>
            {providers.map((p, i) => {
              const id = String(getField(p, 'ProviderID') ?? i);
              return <option key={id} value={id}>{String(getField(p, 'ProviderName', 'Name') ?? id)}</option>;
            })}
          </Select>
          <div className="flex justify-end gap-2">
            <Button variant="outline" size="sm" onClick={() => setShift(null)}>Cancel</Button>
            <Button size="sm" loading={busy === 'shift'} disabled={!shiftProviderId} onClick={() => void runShift()}>
              <Split className="size-4" />
              Shift ({shift?.ids.length ?? 0})
            </Button>
          </div>
        </div>
      </Modal>

      <Modal open={logId !== null} onClose={() => setLogId(null)} title={`Status check log · Txn ${logId ?? ''}`} size="xl">
        {logsLoading ? (
          <div className="space-y-2">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-16 rounded-lg" />)}</div>
        ) : logs.length === 0 ? (
          <EmptyState title="No status checks yet" description="This transaction has not been re-checked against the provider. The scheduler checks pending recharges every 5 minutes." />
        ) : (
          <div className="overflow-hidden rounded-lg border border-slate-200">
            <div className="overflow-x-auto">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="w-10 px-3 py-2.5 text-left">#</th>
                    <th className="whitespace-nowrap px-3 py-2.5 text-left">Provider</th>
                    <th className="whitespace-nowrap px-3 py-2.5 text-left">Date</th>
                    <th className="px-3 py-2.5 text-left">Request</th>
                    <th className="px-3 py-2.5 text-left">Response</th>
                  </tr>
                </thead>
                <tbody>
                  {logs.map((log, i) => (
                    <tr key={i} className="border-b border-slate-100 align-top last:border-0">
                      <td className="px-3 py-2.5 tabular-nums text-slate-400">{i + 1}</td>
                      <td className="whitespace-nowrap px-3 py-2.5 font-medium text-slate-700">{String(getField(log, 'Through') || '—')}</td>
                      <td className="whitespace-nowrap px-3 py-2.5 text-xs text-slate-500">{dateTime(String(getField(log, 'DateTime') ?? ''))}</td>
                      <td className="w-1/2 px-3 py-2.5 text-xs text-slate-500"><span className="block break-all">{String(getField(log, 'Request') || '—')}</span></td>
                      <td className="w-1/2 px-3 py-2.5 text-xs text-slate-500"><span className="block break-all">{String(getField(log, 'Response') || '—')}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}

/** Slim inline KPI — small uppercase label over a tabular figure; no box. */
function Metric({ label, value, accent }: { label: string; value: string; accent?: boolean }) {
  return (
    <div className="flex flex-col leading-tight">
      <span className="text-[10px] font-medium uppercase tracking-wide text-slate-400">{label}</span>
      <span className={cn('text-sm font-semibold tabular-nums', accent ? 'text-brand-700' : 'text-slate-800')}>{value}</span>
    </div>
  );
}

function IconBtn({ children, title, onClick, busy }: { children: ReactNode; title: string; onClick: () => void; busy?: boolean }) {
  return (
    <button
      type="button"
      title={title}
      disabled={busy}
      onClick={onClick}
      className={cn(
        'grid size-8 place-items-center rounded-lg text-slate-500 transition-colors hover:bg-brand-50 hover:text-brand-700 disabled:opacity-40',
        busy && 'animate-pulse',
      )}
    >
      {children}
    </button>
  );
}
