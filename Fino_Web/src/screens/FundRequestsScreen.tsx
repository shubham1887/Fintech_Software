import { useMemo, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format } from 'date-fns';
import {
  RefreshCw, SlidersHorizontal, ChevronDown, Search as SearchIcon, RotateCcw,
  Check, X,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { apiDate, inr } from '@/lib/format';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useAuth } from '@/features/auth/AuthContext';
import { imageUrl } from '@/api/catalog.api';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { Modal } from '@/components/ui/Modal';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

/** Statuses that still need a decision — the only ones this queue shows. */
const PENDING_STATUSES = ['PROCESS', 'PENDING', 'INITIATED'];
const isPending = (row: Row) =>
  PENDING_STATUSES.includes(String(getField(row, 'Status') ?? '').toUpperCase());

const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };

interface Filters {
  fromDate: string;
  toDate: string;
  userId: string;
  modeId: string;
  search: string;
}
const emptyFilters = (): Filters => ({ fromDate: '', toDate: apiDate(new Date()), userId: '', modeId: '0', search: '' });

/** Just the date part, e.g. "2026-07-07" — the API sends a full ".fffffff" timestamp we don't want. */
function ymd(v: unknown): string {
  const s = String(v ?? '');
  const m = s.match(/^(\d{4}-\d{2}-\d{2})/);
  if (m) return m[1];
  const d = new Date(s);
  return Number.isFinite(d.getTime()) && d.getFullYear() > 1 ? format(d, 'yyyy-MM-dd') : '—';
}

/**
 * Fund requests — pending fund-load requests awaiting approval. A collapsible filter (From/To
 * date, user, mode, free text) narrows the list; a KPI strip shows the count and pending amount.
 * Accept / Reject both open a modal that requires an admin remark; on Accept the approver may
 * also correct a Mode / Sub-Mode the requester picked wrongly.
 * Action = Fund/AcceptReject {FundTransactionID, Status, Amount, SenderRemark, ModeID, SubModeID}.
 */
export function FundRequestsScreen({ def }: CustomScreenProps) {
  const qc = useQueryClient();
  const toast = useToast();
  const { user } = useAuth();
  const currentUserId = Number(user?.UserID ?? 0);

  const [showFilters, setShowFilters] = useState(false);
  const [draft, setDraft] = useState<Filters>(emptyFilters);
  const [applied, setApplied] = useState<Filters>(emptyFilters);
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  // Accept / Reject modal state.
  const [target, setTarget] = useState<Row | null>(null);
  const [proofSrc, setProofSrc] = useState<string | null>(null);
  const [decision, setDecision] = useState<'Accept' | 'Reject'>('Accept');
  const [remark, setRemark] = useState('');
  const [modeId, setModeId] = useState('');
  const [subModeId, setSubModeId] = useState('');

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['fund-requests', applied.fromDate, applied.toDate, applied.search],
    queryFn: () =>
      fetchList('Fund/SelectRequestByFilter', {
        Status: '', FromDate: applied.fromDate, ToDate: applied.toDate,
        TakeCount: 500, SkipCount: 0, Search: applied.search,
      }),
  });

  const { data: modes = [] } = useQuery({
    queryKey: ['fund-modes'], staleTime: 300_000,
    queryFn: () => fetchList('Mode/GetActive', {}),
  });
  const { data: users = [] } = useQuery({
    queryKey: ['fund-users'], staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', USER_PAGE),
  });
  const { data: subModes = [] } = useQuery({
    queryKey: ['fund-submodes', modeId], enabled: Boolean(modeId),
    queryFn: () => fetchList('SubMode/GetByModeId', { ModeID: Number(modeId) }),
  });

  // This is the approval queue, so only undecided requests belong here, then the structured
  // filters (user + mode) narrow further client-side.
  const pendingRows = useMemo(() => {
    let rs = rows.filter(isPending);
    if (applied.userId)
      rs = rs.filter((r) =>
        String(getField(r, 'SenderID') ?? '') === applied.userId ||
        String(getField(r, 'ReceiverID') ?? '') === applied.userId);
    if (applied.modeId && applied.modeId !== '0')
      rs = rs.filter((r) => String(getField(r, 'ModeID') ?? '') === applied.modeId);
    return rs;
  }, [rows, applied]);

  const pendingAmount = useMemo(
    () => pendingRows.reduce((s, r) => s + (Number(getField(r, 'Amount')) || 0), 0),
    [pendingRows],
  );

  const userOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '', label: 'All users' },
    ...users.map((u, i) => {
      const sub = ['FirmName', 'MobileNumber'].map((k) => String(getField(u, k) ?? '').trim()).filter(Boolean).join(' · ');
      return { value: String(getField(u, 'UserMasterID', 'UserID') ?? i), label: String(getField(u, 'PersonName', 'FirmName', 'Name') ?? ''), sublabel: sub || undefined };
    }),
  ], [users]);

  const modeOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '0', label: 'All modes' },
    ...modes.map((m, i) => ({ value: String(getField(m, 'ModeID') ?? i), label: String(getField(m, 'ModeName', 'Name') ?? '') })),
  ], [modes]);

  const runSearch = () => setApplied(draft);
  const resetFilters = () => { const e = emptyFilters(); setDraft(e); setApplied(e); };

  const actMut = useMutation({
    mutationFn: (v: { id: unknown; status: 'Accept' | 'Reject'; amount: unknown; remark: string; modeId: number; subModeId: number }) =>
      apiPost('Fund/AcceptReject', {
        FundTransactionID: v.id, Status: v.status, Amount: v.amount, SenderRemark: v.remark,
        ModeID: v.modeId || null, SubModeID: v.subModeId || null,
      }),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success('Request updated.');
        close();
        qc.invalidateQueries({ queryKey: ['fund-requests'] });
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const open = (row: Row, d: 'Accept' | 'Reject') => {
    setTarget(row);
    setDecision(d);
    setRemark('');
    setModeId(String(getField(row, 'ModeID') ?? ''));
    setSubModeId(String(getField(row, 'SubModeID') ?? ''));
  };
  const close = () => { setTarget(null); setRemark(''); setModeId(''); setSubModeId(''); };

  const submit = () => {
    if (!target) return;
    if (!remark.trim()) { toast.error('An admin remark is required.'); return; }
    actMut.mutate({
      id: getField(target, 'FundTransactionID'),
      status: decision,
      amount: getField(target, 'Amount'),
      remark: remark.trim(),
      modeId: decision === 'Accept' ? Number(modeId) : 0,
      subModeId: decision === 'Accept' ? Number(subModeId) : 0,
    });
  };

  const columns = ['Action','#', 'Date', 'Sender', 'Receiver', 'Wallet', 'Mode', 'Sub mode', 'Amount', 'UTR / Receipt', 'Proof', 'Remark'];

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={`${pendingRows.length} fund-load request${pendingRows.length === 1 ? '' : 's'} awaiting your approval`}
        icon={pickIcon(def.title)}
        actions={
          <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
        }
      />

      <Card className="overflow-hidden">
        {/* KPI strip + filter toggle */}
        <div className="flex flex-wrap items-center justify-between gap-x-6 gap-y-3 border-b border-slate-100 px-5 py-3.5">
          <div className="flex items-center gap-5">
            <Metric label="Pending" value={String(pendingRows.length)} />
            <span className="h-8 w-px bg-slate-200/80" />
            <Metric label="Pending amount" value={inr(pendingAmount)} accent />
          </div>
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

        {/* Collapsible filter drawer */}
        {showFilters && (
          <form
            onSubmit={(e) => { e.preventDefault(); runSearch(); }}
            className="grid grid-cols-1 gap-4 border-b border-slate-100 bg-slate-50/60 px-5 py-4 sm:grid-cols-2 lg:grid-cols-3"
          >
            <Input label="From date" type="date" value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
            <Autocomplete label="User" options={userOpts} value={draft.userId} onChange={(v) => upd({ userId: String(v) })} />
            <Autocomplete label="Mode" options={modeOpts} value={draft.modeId} onChange={(v) => upd({ modeId: String(v || '0') })} />
            <Input label="Search" placeholder="Name / UTR / remark…" value={draft.search} onChange={(e) => upd({ search: e.target.value })} />
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

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                {columns.map((h) => (
                  <th key={h} className={cn('whitespace-nowrap px-4 py-3', h === 'Amount' ? 'text-right' : h === 'Action' ? 'text-right' : 'text-left')}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 5 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {columns.map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[110px]" /></td>
                    ))}
                  </tr>
                ))
              ) : pendingRows.length === 0 ? (
                <tr>
                  <td colSpan={columns.length}>
                    <EmptyState title="No pending requests" description="Nothing is awaiting approval for the current filter." />
                  </td>
                </tr>
              ) : (
                pendingRows.map((row, i) => {
                  const isApprover = Number(getField(row, 'RequestToID') ?? 0) === currentUserId;
                  const src = imageUrl(getField(row, 'ReceiptPath') as string);
                  return (
                    <tr key={String(getField(row, 'FundTransactionID') ?? i)} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      <td className="whitespace-nowrap px-4 py-3 text-right">
                        {isApprover ? (
                          <div className="flex items-center justify-end gap-2">
                            <ActionBtn tone="approve" onClick={() => open(row, 'Accept')}><Check className="size-3.5" /></ActionBtn>
                            <ActionBtn tone="reject" onClick={() => open(row, 'Reject')}><X className="size-3.5" /></ActionBtn>
                          </div>
                        ) : (
                          <StatusBadge status={String(getField(row, 'Status') ?? '')} />
                        )}
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-400">{i + 1}</td>
                      <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-600">{ymd(getField(row, 'DOC', 'DateTime', 'Date', 'TransactionDOC'))}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-700">{String(getField(row, 'SenderName') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-700">{String(getField(row, 'ReceiverName') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{String(getField(row, 'WalletName') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{String(getField(row, 'ModeName') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{String(getField(row, 'SubModeName') ?? '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-right font-semibold tabular-nums text-slate-800">{inr(Number(getField(row, 'Amount')) || 0)}</td>
                      <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-slate-600">{String(getField(row, 'UTRNumber') || '—')}</td>
                      <td className="px-4 py-3">
                        {src ? (
                          <button type="button" onClick={() => setProofSrc(src)} className="block">
                            <img src={src} alt="Proof" className="size-9 rounded-md border border-slate-200 object-cover transition hover:ring-2 hover:ring-brand-400" />
                          </button>
                        ) : <span className="text-slate-300">—</span>}
                      </td>
                      <td className="max-w-[220px] truncate px-4 py-3 text-slate-500" title={String(getField(row, 'Remark') ?? '')}>{String(getField(row, 'Remark') || '—')}</td>
                      
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {!isFetching && pendingRows.length > 0 && (
          <div className="border-t border-slate-100 bg-slate-50/40 px-5 py-2.5 text-xs text-slate-500">
            Showing <span className="font-semibold text-slate-700 tabular-nums">{pendingRows.length}</span> pending request{pendingRows.length === 1 ? '' : 's'}
          </div>
        )}
      </Card>

      {/* Proof preview */}
      <Modal open={proofSrc !== null} onClose={() => setProofSrc(null)} title="Payment proof">
        {proofSrc && (
          <a href={proofSrc} target="_blank" rel="noreferrer" className="block">
            <img src={proofSrc} alt="Payment proof" className="mx-auto max-h-[70vh] w-auto rounded-lg" />
          </a>
        )}
      </Modal>

      {/* Accept / Reject */}
      <Modal open={target !== null} onClose={close} title={decision === 'Reject' ? 'Reject fund request' : 'Approve fund request'}>
        <div className="space-y-4">
          <div className="rounded-lg bg-slate-50 p-3 text-sm">
            {target && (
              <div className="flex items-center justify-between gap-3">
                <span className="text-slate-600">
                  {String(getField(target, 'SenderName') || '—')} → {String(getField(target, 'ReceiverName') || '—')}
                </span>
                <span className="font-semibold text-slate-800">{inr(Number(getField(target, 'Amount')) || 0)}</span>
              </div>
            )}
          </div>

          {decision === 'Accept' && (
            <>
              <p className="rounded-lg bg-slate-50 p-3 text-xs text-slate-500">
                If the requester chose the wrong Mode or Sub-Mode, correct it here before approving.
              </p>
              <Select label="Mode" value={modeId} onChange={(e) => { setModeId(e.target.value); setSubModeId(''); }}>
                <option value="">— select —</option>
                {modes.map((m, i) => {
                  const id = String(getField(m, 'ModeID') ?? i);
                  return <option key={id} value={id}>{String(getField(m, 'ModeName') ?? id)}</option>;
                })}
              </Select>
              <Select label="Sub Mode" value={subModeId} onChange={(e) => setSubModeId(e.target.value)} disabled={!modeId}>
                <option value="">— select —</option>
                {subModes.map((s, i) => {
                  const id = String(getField(s, 'SubModeID') ?? i);
                  return <option key={id} value={id}>{String(getField(s, 'SubModeName') ?? id)}</option>;
                })}
              </Select>
            </>
          )}

          <Input
            label="Admin remark (required)"
            placeholder={decision === 'Reject' ? 'Reason for rejection' : 'Note for this approval'}
            value={remark}
            onChange={(e) => setRemark(e.target.value)}
          />

          <div className="flex justify-end gap-2">
            <Button variant="outline" size="sm" onClick={close}>Cancel</Button>
            <Button
              size="sm"
              variant={decision === 'Reject' ? 'danger' : 'primary'}
              loading={actMut.isPending}
              disabled={!remark.trim()}
              onClick={submit}
            >
              {decision === 'Reject' ? 'Reject' : 'Approve'}
            </Button>
          </div>
        </div>
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

/** Compact coloured pill button used for the row's Approve / Reject actions. */
function ActionBtn({ tone, onClick, children }: { tone: 'approve' | 'reject'; onClick: () => void; children: React.ReactNode }) {
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
