import { useMemo, useState, type ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  RefreshCw, Search as SearchIcon, SlidersHorizontal, ShieldCheck, Send,
  CheckCircle2, Undo2, FileClock, Clock, AlertTriangle, Timer, Split, ChevronDown, RotateCcw,
  PauseCircle, PlayCircle, Lock,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { loadSession } from '@/lib/storage';
import { inr, dateTime } from '@/lib/format';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { ReportPager, operatorsForService } from '@/components/report';
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
import { HELD_MARKER, type PendingFamily } from './pendingFamilies';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };
const ADMIN_ROLES = [1, 2, 3, 6]; // admin / super-dist / dist / support see the user + provider columns
const ADMIN_ONLY = [1, 6];        // the hold switch parks live money — admin / support only
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

interface HoldState {
  IsOn?: boolean;
  Remark?: string;
  HeldCount?: number;
  HeldAmount?: number;
}

const rowId = (r: Row) => Number(getField(r, 'TransactionMasterID', 'TransactionID', 'ID') ?? 0);

/** A transfer parked by the global hold — debited, but deliberately never sent upstream. */
const isHeld = (r: Row) => String(getField(r, 'Description', 'Remark') ?? '').includes(HELD_MARKER);

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

async function fetchPending(
  endpoint: string,
  a: Filters,
  page: number,
  listPayload?: Record<string, unknown>,
): Promise<PendingResult> {
  const res = await apiPost<unknown>(endpoint, {
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
    // Last, so a generic queue's pinned ServiceType wins over the defaults above.
    ...(listPayload ?? {}),
  });
  if (!res.ok) return { rows: [], count: 0 };
  const d = res.data as Record<string, unknown> | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Record<string, unknown>, 'list', 'List', 'Data', 'Items');
  const rows = Array.isArray(listRaw) ? (listRaw as Row[]) : [];
  const count = Array.isArray(d) ? rows.length : Number(getField(d as Record<string, unknown>, 'Count', 'count') ?? rows.length);
  return { rows, count };
}

/**
 * The pending-transaction workbench, shared by Pending Recharge and Pending DMT.
 *
 * Both queues answer the same question — which transactions has the provider not given a final
 * answer for, and what do we do about each — so they are one screen driven by a `PendingFamily`
 * (endpoints, identifying columns, capabilities). Before this, only recharge had the bulk
 * status-check / resend / success / refund tooling and DMT was a read-only report, which meant a
 * stuck transfer could only be settled one transaction at a time from the search screen.
 */
export function PendingTxnScreen({ def, family }: CustomScreenProps & { family: PendingFamily }) {
  const toast = useToast();
  const qc = useQueryClient();
  const userTypeId = Number(loadSession()?.user?.UserTypeID ?? 0);
  const isAdmin = ADMIN_ROLES.includes(userTypeId);
  const canHold = family.supportsHold && ADMIN_ONLY.includes(userTypeId);
  const EP = family.endpoints;

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
  const [holdDialog, setHoldDialog] = useState<null | { turningOn: boolean }>(null);
  const [holdRemark, setHoldRemark] = useState('');

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
    enabled: isAdmin && family.supportsShift, queryKey: ['prov-opts'], staleTime: 300_000,
    queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAGE),
  });

  // State of the global transfer hold (DMT only): whether new transfers are being parked, and how
  // many are waiting. Polled alongside the queue so the banner cannot go stale while someone works.
  const { data: hold } = useQuery<HoldState>({
    enabled: canHold,
    queryKey: ['dmt-hold'],
    queryFn: async () => {
      const res = await apiPost<HoldState>('TransactionHold/GetDMT', {});
      return res.ok && res.data ? res.data : {};
    },
    refetchInterval: 60_000,
  });
  const holdOn = Boolean(hold?.IsOn);

  const { data, isFetching, refetch, dataUpdatedAt } = useQuery({
    queryKey: ['pending-txn', family.key, applied, page],
    queryFn: () => fetchPending(EP.list, applied, page, family.listPayload),
    refetchInterval: 60_000, // the scheduler settles in the background; the page keeps itself fresh
  });
  const rows = data?.rows ?? [];
  const total = data?.count ?? 0;

  const operatorOptions = useMemo(
    () => operatorsForService(allOperators, draft.serviceId),
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
  // Held transfers need Release, not Resend — they have never been sent. The button only appears
  // when there is actually something parked in the current selection (or on the page).
  const heldIds = useMemo(
    () => (family.supportsHold ? rows.filter(isHeld).map(rowId).filter((id) => id > 0) : []),
    [rows, family.supportsHold],
  );
  const heldTargets = selectedIds.length > 0 ? heldIds.filter((id) => selectedIds.includes(id)) : heldIds;

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
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const afterMutation = async () => {
    setSelected(new Set());
    await qc.invalidateQueries({ queryKey: ['pending-txn', family.key] });
    if (canHold) await qc.invalidateQueries({ queryKey: ['dmt-hold'] });
  };

  // Bulk action over the selected rows (or the whole page if none are ticked).
  async function runBulk(action: string, endpoint: string, verb: string) {
    const ids = (selectedIds.length > 0 ? selectedIds : pageIds);
    if (ids.length === 0) { toast.info(`No pending ${family.nounPlural}.`); return; }
    setBusy(action);
    try {
      const before = total;
      const res = await apiPost<unknown>(endpoint, ids.map((id) => ({ TransactionID: id })));
      if (!res.ok) { toast.error(res.message || `${verb} failed.`); return; }
      const after = await fetchPending(EP.list, applied, page);
      const settled = Math.max(before - after.count, 0);
      toast.success(
        settled > 0
          ? `${verb}: probed ${ids.length}, ${settled} settled/cleared.`
          : `${verb}: probed ${ids.length} — still pending (the provider has not given a final status).`,
      );
      await afterMutation();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : `${verb} failed.`);
    } finally {
      setBusy(null);
    }
  }

  // Single-row action. The two families' single-row endpoints disagree on the id field name
  // (TxnSelectByIDRequest.ID vs TxnDMTResendRequest.TransactionID); both are sent because an
  // unknown field is ignored on deserialisation, which keeps one call path for both queues.
  async function runRow(id: number, endpoint: string, verb: string) {
    setRowBusy(id);
    try {
      const res = await apiPost<unknown>(endpoint, { ID: id, TransactionID: id });
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
    if (ids.length === 0 || providerId <= 0 || !EP.shiftOne || !EP.shiftAll) return;
    setBusy('shift');
    try {
      const res = ids.length === 1
        ? await apiPost<unknown>(EP.shiftOne, { TransactionID: ids[0], ProviderID: providerId })
        : await apiPost<unknown>(EP.shiftAll, ids.map((id) => ({ TransactionID: id, ProviderID: providerId })));
      if (!res.ok) { toast.error(res.message || 'Shift failed.'); return; }
      toast.success(`Shifted ${ids.length} ${ids.length === 1 ? family.noun : family.nounPlural} to ${providerName(providerId)}.`);
      setShift(null);
      setShiftProviderId('');
      await afterMutation();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Shift failed.');
    } finally {
      setBusy(null);
    }
  }

  // Send specific parked transfers now. Works while the hold is still on, so one urgent transfer
  // can be let through without lifting the hold for everyone.
  async function runRelease(ids: number[]) {
    if (!EP.release || ids.length === 0) return;
    setBusy('release');
    try {
      const res = await apiPost<unknown>(EP.release, ids.map((id) => ({ TransactionID: id })));
      if (!res.ok) { toast.error(res.message || 'Release failed.'); return; }
      toast.success(res.message || `Released ${ids.length} ${ids.length === 1 ? family.noun : family.nounPlural}.`);
      await afterMutation();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Release failed.');
    } finally {
      setBusy(null);
    }
  }

  // Flip the global hold. Turning it OFF also releases everything it parked — the API queues that
  // as a background job, so this returns immediately and the banner count drains as they dispatch.
  async function runHold(turningOn: boolean) {
    setBusy('hold');
    try {
      const res = await apiPost<HoldState>('TransactionHold/SetDMT', { IsOn: turningOn, Remark: holdRemark.trim() });
      if (!res.ok) { toast.error(res.message || 'Could not change the hold.'); return; }
      const parked = Number(res.data?.HeldCount ?? 0);
      toast.success(
        turningOn
          ? 'Transfers are on hold. New transfers will be accepted and kept pending until you resume.'
          : parked > 0
            ? `Transfers resumed. Releasing ${parked} held ${parked === 1 ? family.noun : family.nounPlural} in the background.`
            : 'Transfers resumed.',
      );
      setHoldDialog(null);
      setHoldRemark('');
      await afterMutation();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not change the hold.');
    } finally {
      setBusy(null);
    }
  }

  const providerName = (id: number) =>
    String(getField(providers.find((p) => Number(getField(p, 'ProviderID')) === id) ?? {}, 'ProviderName') ?? id);

  const actionCount = selectedIds.length > 0 ? selectedIds.length : pageIds.length;
  const showShift = isAdmin && family.supportsShift;

  /**
   * The providers worth offering in the shift dialog.
   *
   * Inactive providers are dropped - one that is switched off cannot serve anything, so listing it
   * only invites a shift that will be refused. When a SINGLE row is being shifted, the provider it
   * is already stuck on is shown but not selectable: re-sending to the same provider is the one
   * choice guaranteed to leave it exactly where it is.
   *
   * The list is NOT narrowed to the operator's configured routes. That would need the whole route
   * table (hundreds of rows) in the browser, and getting it wrong would HIDE a provider that would
   * have worked - a worse failure than showing one that is refused, which the dispatch names
   * clearly ("The selected provider is not configured for this operator") and the batch now
   * reports per row.
   */
  const shiftOptions = useMemo(() => {
    const currentId = shift?.ids.length === 1
      ? Number(getField(rows.find((r) => rowId(r) === shift.ids[0]) ?? {}, 'ProviderID') ?? 0)
      : 0;
    return providers
      .filter((p) => getField(p, 'IsActive') !== false)
      .map((p, i) => {
        const id = Number(getField(p, 'ProviderID') ?? i);
        return {
          id: String(id),
          name: String(getField(p, 'ProviderName', 'Name') ?? id),
          isCurrent: currentId > 0 && id === currentId,
        };
      });
  }, [providers, shift, rows]);
  const colCount = 5 + family.columns.length + (isAdmin ? 2 : 0);

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
            {canHold && (
              <Button
                variant={holdOn ? 'primary' : 'outline'}
                size="sm"
                loading={busy === 'hold'}
                onClick={() => { setHoldRemark(''); setHoldDialog({ turningOn: !holdOn }); }}
              >
                {holdOn ? <PlayCircle className="size-4" /> : <PauseCircle className="size-4" />}
                {holdOn ? 'Resume transfers' : 'Hold transfers'}
              </Button>
            )}
            <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
              <RefreshCw className="size-4" />
              Refresh
            </Button>
          </div>
        }
      />

      {/* Hold banner — while the switch is on, nothing here is stuck; it is being parked on purpose. */}
      {canHold && holdOn && (
        <div className="mb-4 flex flex-wrap items-start gap-3 rounded-card border border-warning-500/30 bg-warning-50 px-5 py-3.5 text-sm text-warning-700">
          <Lock className="mt-0.5 size-4 shrink-0" />
          <div className="flex-1">
            <p className="font-semibold">Transfers are on hold.</p>
            <p className="mt-0.5">
              New transfers are still accepted, debited and commissioned — they are simply not sent to the bank yet, and
              sit here as pending. {Number(hold?.HeldCount ?? 0) > 0 && (
                <>
                  <span className="font-semibold tabular-nums">{hold?.HeldCount}</span>{' '}
                  {Number(hold?.HeldCount) === 1 ? 'transfer is' : 'transfers are'} waiting
                  {Number(hold?.HeldAmount ?? 0) > 0 && <> ({inr(Number(hold?.HeldAmount))})</>}.{' '}
                </>
              )}
              Resuming sends every waiting transfer through the normal sequence.
            </p>
            {Boolean(hold?.Remark) && <p className="mt-1 text-xs text-warning-600">Reason: {hold?.Remark}</p>}
          </div>
          <Button variant="primary" size="sm" loading={busy === 'hold'} onClick={() => { setHoldRemark(''); setHoldDialog({ turningOn: false }); }}>
            <PlayCircle className="size-4" />
            Resume now
          </Button>
        </div>
      )}

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
              label={family.searchLabel}
              placeholder={family.searchPlaceholder}
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
          {EP.release && heldTargets.length > 0 && (
            <Button size="sm" variant="outline" loading={busy === 'release'} onClick={() => void runRelease(heldTargets)}>
              <PlayCircle className="size-4 text-success-600" />
              Release held ({heldTargets.length})
            </Button>
          )}
          {showShift && (
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
                {['#', 'Date', 'Pending since'].map((h) => (
                  <th key={h} className="whitespace-nowrap px-4 py-3 text-left">{h}</th>
                ))}
                {family.columns.map((c) => (
                  <th key={c.label} className={cn('whitespace-nowrap px-4 py-3', c.type === 'money' ? 'text-right' : 'text-left')}>{c.label}</th>
                ))}
                <th className="whitespace-nowrap px-4 py-3 text-left">Status</th>
                {isAdmin && <th className="whitespace-nowrap px-4 py-3 text-left">Provider</th>}
                {isAdmin && <th className="whitespace-nowrap px-4 py-3 text-left">Request ID</th>}
                <th className="whitespace-nowrap px-4 py-3 text-left">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 6 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: colCount }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[110px]" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={colCount}>
                    <EmptyState
                      title={`No pending ${family.nounPlural}`}
                      description={`Everything has settled, or nothing matches the filters. The scheduler also re-checks these every 5 minutes.`}
                    />
                  </td>
                </tr>
              ) : (
                rows.map((row, i) => {
                  const id = rowId(row);
                  const doc = String(getField(row, 'DOC', 'DateTime', 'Date') ?? '');
                  const held = family.supportsHold && isHeld(row);
                  return (
                    <tr key={id} className={cn('border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60', selected.has(id) && 'bg-brand-50/40')}>
                      <td className="px-3 py-3">
                        <input type="checkbox" checked={selected.has(id)} onChange={() => toggleOne(id)} className="size-4 accent-brand-600" aria-label={`Select ${id}`} />
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-500">{page * PAGE_SIZE + i + 1}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{dateTime(doc)}</td>
                      <td className="whitespace-nowrap px-4 py-3">
                        <span className={cn(
                          'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium tabular-nums',
                          // A held transfer is not late — it is waiting on purpose, so it never
                          // gets the "this has been stuck too long" warning colour.
                          !held && isStale(doc) ? 'bg-warning-50 text-warning-600 ring-1 ring-inset ring-warning-500/20' : 'bg-slate-100 text-slate-500',
                        )}>
                          <Clock className="size-3" />
                          {pendingAge(doc)}
                        </span>
                      </td>
                      {family.columns.map((c) => {
                        const raw = getField(row, ...c.keys);
                        if (c.type === 'money') {
                          return (
                            <td key={c.label} className="whitespace-nowrap px-4 py-3 text-right font-medium tabular-nums text-slate-800">
                              {inr(Number(raw) || 0)}
                            </td>
                          );
                        }
                        const text = String(raw ?? '').trim();
                        return (
                          <td key={c.label} className={cn('whitespace-nowrap px-4 py-3', c.type === 'bold' ? 'font-medium text-slate-800' : 'text-slate-600')}>
                            {text || <span className="text-slate-300">—</span>}
                          </td>
                        );
                      })}
                      <td className="whitespace-nowrap px-4 py-3">
                        {held ? (
                          <span className="inline-flex items-center gap-1 rounded-full bg-warning-50 px-2 py-0.5 text-xs font-medium text-warning-700 ring-1 ring-inset ring-warning-500/20">
                            <Lock className="size-3" />
                            On hold
                          </span>
                        ) : (
                          <StatusBadge status={String(getField(row, 'Status') ?? 'PENDING')} />
                        )}
                      </td>
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
                          <IconBtn
                            title={held ? 'Release now — send this transfer to the bank' : 'Resend (dispatch again)'}
                            busy={rowBusy === id || busy === 'release'}
                            onClick={() => (held && EP.release ? void runRelease([id]) : void runRow(id, EP.resendOne, 'Resend'))}
                          >
                            <Send className="size-4" />
                          </IconBtn>
                          {showShift && (
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

      </Card>

      {/* Same pager as every report, so paging feels identical wherever you are. */}
      <ReportPager
        page={page}
        pageSize={PAGE_SIZE}
        total={total}
        loading={isFetching}
        onPage={(p) => { setSelected(new Set()); setPage(p); }}
      />

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
                  : `${actionCount} transaction(s) will be marked SUCCESS manually — without confirming with the provider. Are you sure ${actionCount === 1 ? 'it' : 'they'} actually completed?`}
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

      {/* Hold / resume — the switch parks live customer money, so it always asks first. */}
      <Modal
        open={holdDialog !== null}
        onClose={() => setHoldDialog(null)}
        title={holdDialog?.turningOn ? 'Hold transfers' : 'Resume transfers'}
      >
        {holdDialog && (
          <div className="space-y-4">
            <p className={cn('rounded-lg p-3 text-sm', holdDialog.turningOn ? 'bg-warning-50 text-warning-700' : 'bg-slate-50 text-slate-600')}>
              {holdDialog.turningOn ? (
                <>
                  New transfers will still be accepted from retailers — validated, debited and commissioned exactly as
                  usual — but they will <b>not</b> be sent to the bank. They stay pending here until you resume.
                  Nothing already in flight is affected.
                </>
              ) : (
                <>
                  Every transfer parked by the hold will be sent through the normal sequence, in the background.
                  {Number(hold?.HeldCount ?? 0) > 0 && (
                    <> <b className="tabular-nums">{hold?.HeldCount}</b> waiting ({inr(Number(hold?.HeldAmount ?? 0))}).</>
                  )}
                </>
              )}
            </p>
            <Input
              label="Reason"
              placeholder={holdDialog.turningOn ? 'e.g. bank outage, reconciliation window' : 'e.g. bank confirmed service restored'}
              value={holdRemark}
              onChange={(e) => setHoldRemark(e.target.value)}
            />
            <div className="flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={() => setHoldDialog(null)}>Cancel</Button>
              <Button
                size="sm"
                variant={holdDialog.turningOn ? 'danger' : 'primary'}
                loading={busy === 'hold'}
                onClick={() => void runHold(holdDialog.turningOn)}
              >
                {holdDialog.turningOn ? <PauseCircle className="size-4" /> : <PlayCircle className="size-4" />}
                {holdDialog.turningOn ? 'Hold transfers' : 'Resume transfers'}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Shift — pick the provider to re-dispatch through. */}
      <Modal open={shift !== null} onClose={() => setShift(null)} title={`Shift ${family.noun}`}>
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            {shift?.ids.length === 1
              ? `Transaction ${shift.ids[0]} will be sent again through the provider you choose, instead of the one the switching sequence picked.`
              : `${shift?.ids.length ?? 0} ${family.nounPlural} will be sent again through the provider you choose, instead of the one the switching sequence picked.`}
          </p>
          <Select label="Provider" value={shiftProviderId} onChange={(e) => setShiftProviderId(e.target.value)}>
            <option value="">— choose a provider —</option>
            {shiftOptions.map((p) => (
              <option key={p.id} value={p.id} disabled={p.isCurrent}>
                {p.name}{p.isCurrent ? ' — already stuck here' : ''}
              </option>
            ))}
          </Select>
          <p className="text-xs text-slate-500">
            The provider must have an active route for the operator on each row. Any that do not
            are reported back rather than silently skipped.
          </p>
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
          <EmptyState
            title="No status checks yet"
            description={`This transaction has not been re-checked against the provider. The scheduler checks pending ${family.nounPlural} every 5 minutes.`}
          />
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
