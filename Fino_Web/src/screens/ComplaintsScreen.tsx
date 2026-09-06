import { useMemo, useState, type ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { subDays, format } from 'date-fns';
import {
  Check, X, MoreVertical, RefreshCw, SlidersHorizontal, ChevronDown,
  RotateCcw, Download,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { apiDate, inr, dateTime } from '@/lib/format';
import { exportToCsv } from '@/lib/csv';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { Modal } from '@/components/ui/Modal';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { cn } from '@/lib/cn';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
type Bucket = 'pending' | 'accept' | 'reject';

const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };
const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };

/**
 * A complaint has exactly three states: pending (nobody decided yet), accept, or reject.
 * Legacy rows may carry SUCCESS/FAILED — fold those into accept/reject so the count is honest.
 */
function bucketOf(row: Row): Bucket {
  const s = String(getField(row, 'ComplaintStatus') ?? '').toUpperCase().trim();
  if (s === 'ACCEPT' || s === 'ACCEPTED' || s === 'SUCCESS') return 'accept';
  if (s === 'REJECT' || s === 'REJECTED' || s === 'FAILED' || s === 'FAIL') return 'reject';
  return 'pending';
}

/** Just the date part, e.g. "2026-07-07" — the API sends a full ".fffffff" timestamp we don't want. */
function ymd(v: unknown): string {
  const s = String(v ?? '');
  const m = s.match(/^(\d{4}-\d{2}-\d{2})/);
  if (m) return m[1];
  const d = new Date(s);
  return Number.isFinite(d.getTime()) && d.getFullYear() > 1 ? format(d, 'yyyy-MM-dd') : '—';
}

/** Columns shown in the table (and exported). */
const COLUMNS: { key: string; label: string }[] = [
  { key: 'DateTime', label: 'Date' },
  { key: 'UserName', label: 'User' },
  { key: 'UserMobile', label: 'Mobile' },
  { key: 'ServiceName', label: 'Service' },
  { key: 'OperatorName', label: 'Operator' },
  { key: 'ProviderName', label: 'Provider' },
  { key: 'ConsumerNumber', label: 'Number' },
  { key: 'Amount', label: 'Amount' },
  { key: 'ComplaintType', label: 'Type' },
  { key: 'CustomerRemark', label: 'Customer Remark' },
  { key: 'AdminRemark', label: 'Admin Remark' },
  { key: 'ComplaintStatus', label: 'Status' },
];

/**
 * Complaints — records the decision (accept / reject) with a required admin remark
 * (ComplaintMaster/Resolve). It does not move money; settle the underlying transaction from
 * Pending Recharge or Search if it still needs it.
 */
export function ComplaintsScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();

  const [showFilters, setShowFilters] = useState(false);
  const [fromDate, setFromDate] = useState(apiDate(subDays(new Date(), 30)));
  const [toDate, setToDate] = useState(apiDate(new Date()));
  const [userId, setUserId] = useState('');
  const [serviceId, setServiceId] = useState('0');
  const [search, setSearch] = useState('');
  const [bucket, setBucket] = useState<Bucket>('pending');

  const [target, setTarget] = useState<Row | null>(null);
  const [outcome, setOutcome] = useState<'ACCEPT' | 'REJECT' | ''>('');
  const [remark, setRemark] = useState('');
  const [busy, setBusy] = useState(false);
  const [logTarget, setLogTarget] = useState<Row | null>(null);

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['complaints'],
    queryFn: () => fetchList('ComplaintMaster/GetAll', {}),
  });
  const { data: users = [] } = useQuery({
    queryKey: ['complaint-users'], staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', USER_PAGE),
  });
  const { data: services = [] } = useQuery({
    queryKey: ['complaint-services'], staleTime: 300_000,
    queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });

  const logTxnId = logTarget ? Number(getField(logTarget, 'TransactionMasterID') ?? 0) : 0;
  const { data: logs = [], isFetching: logsLoading } = useQuery({
    queryKey: ['complaint-provider-log', logTxnId],
    enabled: logTxnId > 0,
    queryFn: () => fetchList('ProviderRequestResponseLog/GetByTransaction', { TransactionID: logTxnId }),
  });

  // Structured filters (date + user + service) applied client-side over the full list.
  const baseRows = useMemo(() => {
    const from = new Date(fromDate + 'T00:00:00').getTime();
    const to = new Date(toDate + 'T23:59:59').getTime();
    return rows.filter((r) => {
      const raw = getField(r, 'DateTime', 'TransactionDate');
      const t = raw ? new Date(String(raw)).getTime() : NaN;
      if (!Number.isNaN(t) && (t < from || t > to)) return false;
      if (userId && String(getField(r, 'UserID', 'UserMasterID') ?? '') !== userId) return false;
      if (serviceId && serviceId !== '0' && String(getField(r, 'ServiceID') ?? '') !== serviceId) return false;
      return true;
    });
  }, [rows, fromDate, toDate, userId, serviceId]);

  const counts = useMemo(() => {
    const c = { pending: 0, accept: 0, reject: 0 };
    for (const r of baseRows) c[bucketOf(r)]++;
    return c;
  }, [baseRows]);

  // Bucket (tab) + free-text search.
  const shown = useMemo(() => {
    const q = search.trim().toLowerCase();
    return baseRows.filter((r) => {
      if (bucketOf(r) !== bucket) return false;
      if (!q) return true;
      return COLUMNS.some((c) => String(getField(r, c.key) ?? '').toLowerCase().includes(q));
    });
  }, [baseRows, bucket, search]);

  const userOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '', label: 'All users' },
    ...users.map((u, i) => {
      const sub = ['FirmName', 'MobileNumber'].map((k) => String(getField(u, k) ?? '').trim()).filter(Boolean).join(' · ');
      return { value: String(getField(u, 'UserMasterID', 'UserID') ?? i), label: String(getField(u, 'PersonName', 'FirmName', 'Name') ?? ''), sublabel: sub || undefined };
    }),
  ], [users]);
  const serviceOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '0', label: 'All services' },
    ...services.map((s, i) => ({ value: String(getField(s, 'ServiceID') ?? i), label: String(getField(s, 'ServiceName', 'Name') ?? '') })),
  ], [services]);

  const resetFilters = () => {
    setFromDate(apiDate(subDays(new Date(), 30)));
    setToDate(apiDate(new Date()));
    setUserId('');
    setServiceId('0');
    setSearch('');
  };

  const exportCsv = () =>
    exportToCsv(
      shown.map((r) =>
        Object.fromEntries(
          COLUMNS.map((c) => [
            c.label,
            c.key === 'DateTime' ? ymd(getField(r, 'DateTime', 'TransactionDate'))
              : c.key === 'Amount' ? Number(getField(r, 'Amount')) || 0
              : String(getField(r, c.key) ?? ''),
          ]),
        ),
      ),
      'Complaints',
    );

  const close = () => { setTarget(null); setOutcome(''); setRemark(''); };

  async function submit() {
    if (!target || !outcome) return;
    if (!remark.trim()) { toast.error('An admin remark is required.'); return; }
    setBusy(true);
    try {
      const res = await apiPost('ComplaintMaster/Resolve', {
        ComplaintMasterID: getField(target, 'ComplaintMasterID'),
        ComplaintStatus: outcome,
        AdminRemark: remark.trim(),
      });
      if (!res.ok) { toast.error(res.message || 'Could not update the complaint.'); return; }
      toast.success(`Complaint ${outcome === 'ACCEPT' ? 'accepted' : 'rejected'}.`);
      close();
      await qc.invalidateQueries({ queryKey: ['complaints'] });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not update the complaint.');
    } finally {
      setBusy(false);
    }
  }

  const TABS: { key: Bucket; label: string; n: number }[] = [
    { key: 'pending', label: 'Pending', n: counts.pending },
    { key: 'accept', label: 'Accepted', n: counts.accept },
    { key: 'reject', label: 'Rejected', n: counts.reject },
  ];

  const cell = (row: Row, key: string): ReactNode => {
    if (key === 'DateTime') return <span className="tabular-nums">{ymd(getField(row, 'DateTime', 'TransactionDate'))}</span>;
    if (key === 'Amount') return <span className="tabular-nums font-semibold text-slate-800">{inr(Number(getField(row, 'Amount')) || 0)}</span>;
    if (key === 'ComplaintStatus') return <StatusBadge status={String(getField(row, 'ComplaintStatus') ?? '')} />;
    const v = String(getField(row, key) ?? '').trim();
    if (!v) return <span className="text-slate-300">—</span>;
    if (key === 'CustomerRemark' || key === 'AdminRemark') return <span className="block max-w-[200px] truncate" title={v}>{v}</span>;
    return v;
  };

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
        }
      />

      <Card className="overflow-hidden">
        {/* Export + filter toggle */}
        <div className="flex flex-wrap items-center justify-end gap-x-6 gap-y-3 border-b border-slate-100 px-5 py-3.5">
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" onClick={exportCsv} disabled={shown.length === 0}>
              <Download className="size-4" />
              Export
            </Button>
            <button
              type="button"
              onClick={() => setShowFilters((s) => !s)}
              aria-expanded={showFilters}
              className={cn(
                'inline-flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors',
                showFilters ? 'border-brand-200 bg-brand-50 text-brand-700' : 'border-slate-200 text-slate-600 hover:bg-slate-50',
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
          <div className="grid grid-cols-1 gap-4 border-b border-slate-100 bg-slate-50/60 px-5 py-4 sm:grid-cols-2 lg:grid-cols-3">
            <Input label="From date" type="date" max={toDate} value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
            <Input label="To date" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
            <Autocomplete label="User" options={userOpts} value={userId} onChange={(v) => setUserId(String(v))} />
            <Autocomplete label="Service" options={serviceOpts} value={serviceId} onChange={(v) => setServiceId(String(v || '0'))} />
            <Input label="Search" placeholder="Number / remark / operator…" value={search} onChange={(e) => setSearch(e.target.value)} />
            <div className="flex items-end">
              <Button type="button" variant="outline" onClick={resetFilters}>
                <RotateCcw className="size-4" />
                Reset filters
              </Button>
            </div>
          </div>
        )}

        {/* Status tabs */}
        <div className="flex gap-1 border-b border-slate-200 px-5 pt-3">
          {TABS.map((t) => (
            <button
              key={t.key}
              type="button"
              onClick={() => setBucket(t.key)}
              className={cn(
                '-mb-px flex items-center gap-1.5 border-b-2 px-3.5 py-2 text-sm font-medium transition-colors',
                bucket === t.key ? 'border-brand-600 text-brand-700' : 'border-transparent text-slate-500 hover:text-slate-700',
              )}
            >
              {t.label}
              <span className={cn('rounded-full px-1.5 py-0.5 text-[11px] tabular-nums', bucket === t.key ? 'bg-brand-100 text-brand-700' : 'bg-slate-100 text-slate-500')}>{t.n}</span>
            </button>
          ))}
        </div>

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                {COLUMNS.map((c) => (
                  <th key={c.key} className={cn('whitespace-nowrap px-4 py-3', c.key === 'Amount' ? 'text-right' : 'text-left')}>{c.label}</th>
                ))}
                <th className="whitespace-nowrap px-4 py-3 text-right">Action</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 5 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: COLUMNS.length + 1 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[110px]" /></td>
                    ))}
                  </tr>
                ))
              ) : shown.length === 0 ? (
                <tr>
                  <td colSpan={COLUMNS.length + 1}>
                    <EmptyState title="No complaints" description="Nothing in this period matches — change the filters or tab." />
                  </td>
                </tr>
              ) : (
                shown.map((row, i) => {
                  const pending = bucketOf(row) === 'pending';
                  return (
                    <tr key={String(getField(row, 'ComplaintMasterID') ?? i)} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      {COLUMNS.map((c) => (
                        <td key={c.key} className={cn('whitespace-nowrap px-4 py-3 text-slate-600', c.key === 'Amount' && 'text-right')}>{cell(row, c.key)}</td>
                      ))}
                      <td className="whitespace-nowrap px-4 py-3 text-right">
                        <div className="flex items-center justify-end gap-2">
                          {pending && (
                            <>
                              <ActionBtn tone="approve" onClick={() => { setTarget(row); setOutcome('ACCEPT'); setRemark(''); }}><Check className="size-3.5" />Accept</ActionBtn>
                              <ActionBtn tone="reject" onClick={() => { setTarget(row); setOutcome('REJECT'); setRemark(''); }}><X className="size-3.5" />Reject</ActionBtn>
                            </>
                          )}
                          <IconBtn title="View request / response" onClick={() => setLogTarget(row)}>
                            <MoreVertical className="size-4" />
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

        {!isFetching && shown.length > 0 && (
          <div className="border-t border-slate-100 bg-slate-50/40 px-5 py-2.5 text-xs text-slate-500">
            Showing <span className="font-semibold text-slate-700 tabular-nums">{shown.length}</span> complaint{shown.length === 1 ? '' : 's'}
          </div>
        )}
      </Card>

      {/* Accept / Reject */}
      <Modal open={target !== null} onClose={close} title={outcome === 'REJECT' ? 'Reject complaint' : 'Accept complaint'}>
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            {target && (
              <>
                Complaint from <span className="font-medium text-slate-700">{String(getField(target, 'UserName') || '—')}</span>{' '}
                for {String(getField(target, 'OperatorName') || '—')} · {String(getField(target, 'ConsumerNumber') || '—')}.
              </>
            )}
          </p>
          <p className={cn('rounded-lg p-3 text-sm', outcome === 'ACCEPT' ? 'bg-amber-50 text-amber-700' : 'bg-slate-50 text-slate-500')}>
            {outcome === 'ACCEPT' ? (
              <>Accepting refunds the transaction amount to the user and debits back the commission
              credited to the distributor and super-distributor. This cannot be undone.</>
            ) : (
              <>This records the outcome of the complaint only. Rejecting moves no money.</>
            )}
          </p>
          <Input
            label="Admin remark (required)"
            placeholder="Why is this being accepted / rejected?"
            value={remark}
            onChange={(e) => setRemark(e.target.value)}
          />
          <div className="flex justify-end gap-2">
            <Button variant="outline" size="sm" onClick={close}>Cancel</Button>
            <Button
              size="sm"
              variant={outcome === 'REJECT' ? 'danger' : 'primary'}
              loading={busy}
              disabled={!remark.trim()}
              onClick={submit}
            >
              {outcome === 'REJECT' ? 'Reject' : 'Accept'}
            </Button>
          </div>
        </div>
      </Modal>

      {/* 3-dot: provider request / response of the complained transaction */}
      <Modal open={logTarget !== null} onClose={() => setLogTarget(null)} title="Transaction request / response">
        {logTarget && (
          <div className="space-y-4">
            <p className="text-sm text-slate-500">
              {String(getField(logTarget, 'UserName') || '—')} · {String(getField(logTarget, 'OperatorName') || '—')} ·{' '}
              {String(getField(logTarget, 'ConsumerNumber') || '—')} ·{' '}
              <span className="font-medium text-slate-700">{inr(Number(getField(logTarget, 'Amount')) || 0)}</span>
            </p>

            {logsLoading ? (
              <div className="space-y-3">{Array.from({ length: 2 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-lg" />)}</div>
            ) : logs.length === 0 ? (
              <p className="rounded-lg bg-slate-50 p-4 text-sm text-slate-500">
                No provider request/response was logged for this transaction.
              </p>
            ) : (
              <div className="max-h-[65vh] space-y-3 overflow-auto">
                {logs.map((log, i) => (
                  <div key={i} className="overflow-hidden rounded-lg border border-slate-200">
                    <div className="flex flex-wrap items-center gap-x-5 gap-y-1 border-b border-slate-100 bg-slate-50/70 px-3 py-2 text-xs">
                      {Boolean(getField(log, 'ProviderName')) && (
                        <span><span className="text-slate-400">Provider:</span> <span className="font-medium text-slate-700">{String(getField(log, 'ProviderName'))}</span></span>
                      )}
                      {Boolean(getField(log, 'Through')) && <span><span className="text-slate-400">Through:</span> {String(getField(log, 'Through'))}</span>}
                      <span className="ml-auto text-slate-400">{dateTime(String(getField(log, 'DateTime') ?? ''))}</span>
                    </div>
                    <div className="space-y-2 px-3 py-2.5">
                      <LogBlock label="Request" value={String(getField(log, 'Request') || '—')} />
                      <LogBlock label="Response" value={String(getField(log, 'Response') || '—')} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </Modal>
    </>
  );
}

/** Compact coloured pill button used for the row's Accept / Reject actions. */
function ActionBtn({ tone, onClick, children }: { tone: 'approve' | 'reject'; onClick: () => void; children: ReactNode }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'inline-flex items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-xs font-semibold transition-colors',
        tone === 'approve'
          ? 'border-success-200 bg-success-50 text-success-700 hover:bg-success-100'
          : 'border-danger-200 bg-danger-50 text-danger-600 hover:bg-danger-100',
      )}
    >
      {children}
    </button>
  );
}

function IconBtn({ children, title, onClick }: { children: ReactNode; title: string; onClick: () => void }) {
  return (
    <button
      type="button"
      title={title}
      onClick={onClick}
      className="grid size-8 place-items-center rounded-lg text-slate-500 transition-colors hover:bg-slate-100 hover:text-slate-700"
    >
      {children}
    </button>
  );
}

function LogBlock({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</p>
      <pre className="max-h-40 overflow-auto whitespace-pre-wrap break-all rounded-lg bg-slate-50 p-3 text-xs leading-relaxed text-slate-700">
        {value}
      </pre>
    </div>
  );
}
