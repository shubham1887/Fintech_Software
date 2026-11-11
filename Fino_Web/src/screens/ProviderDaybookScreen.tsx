import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ChevronRight } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { loadSession } from '@/lib/storage';
import { inr, apiDate } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { ReportActions, ReportFilterBar, operatorsForService } from '@/components/report';
import { PageHeader } from '@/components/ui/PageHeader';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };
const ADMIN_ROLES = [1, 2, 3, 6]; // admin / super-dist / dist / support see the entity picker + margins
const ADMIN_ONLY = [1, 6];

type Row = Record<string, unknown>;

interface Filters {
  fromDate: string;
  toDate: string;
  serviceId: string;
  operatorId: string;
  providerId: string;
  userId: string | number;
}

function defaultFilters(): Filters {
  return {
    fromDate: apiDate(new Date()),
    toDate: apiDate(new Date()),
    serviceId: '0',
    operatorId: '0',
    providerId: '0',
    userId: '',
  };
}

const num = (r: Row, k: string) => Number(getField(r, k) ?? 0) || 0;

/** One provider, with its per-service/per-operator sale rows folded underneath. */
interface ProviderGroup {
  providerId: number;
  providerName: string;
  children: Row[];
  totalHits: number;
  totalAmount: number;
  successHits: number;
  successAmount: number;
  failedHits: number;
  pendingHits: number;
  // The chain's own payouts — what the retailer, distributor and super-distributor earned on this
  // provider's sale. The API has always returned them; the table simply never showed them, so the
  // daybook could tell you the admin's margin without showing the commission it was netted against.
  retailerCommission: number;
  distributerCommission: number;
  superDistributerCommission: number;
  adminCommission: number;
  providerCommission: number;
  profit: number;
}

async function fetchDaybook(f: Filters): Promise<Row[]> {
  const res = await apiPost<unknown>('DayBookReport/ProviderDaybook', {
    FromDate: f.fromDate,
    ToDate: f.toDate,
    ServiceID: Number(f.serviceId || 0),
    OperatorID: Number(f.operatorId || 0),
    ProviderID: Number(f.providerId || 0),
    UserID: Number(f.userId || 0),
    IsExcelDownload: false,
  });
  if (!res.ok) return [];
  const d = res.data as Record<string, unknown> | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Row, 'list', 'List', 'Data');
  return Array.isArray(listRaw) ? (listRaw as Row[]) : [];
}

/**
 * Provider Daybook — one row per provider (API), expandable into its per-service / per-operator
 * sale. Replaces the old flat table that repeated the provider name on every operator line.
 * Filters: date range, service (cascades operator), operator, provider and — for admins — entity.
 */
export function ProviderDaybookScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const userTypeId = Number(loadSession()?.user?.UserTypeID ?? 0);
  const isAdmin = ADMIN_ONLY.includes(userTypeId);
  const showUser = ADMIN_ROLES.includes(userTypeId);

  const [draft, setDraft] = useState<Filters>(defaultFilters);
  const [applied, setApplied] = useState<Filters>(draft);
  const [open, setOpen] = useState<Set<number>>(new Set());
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: services = [] } = useQuery({
    queryKey: ['svc-opts'], staleTime: 300_000, queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });
  const { data: allOperators = [] } = useQuery({
    queryKey: ['op-opts'], staleTime: 300_000, queryFn: () => fetchList('Operator/GetAll', LIST_PAGE),
  });
  const { data: providers = [] } = useQuery({
    queryKey: ['prov-opts'], staleTime: 300_000, queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAGE),
  });
  const { data: users = [], isLoading: usersLoading } = useQuery({
    enabled: showUser, queryKey: ['user-opts'], staleTime: 300_000, queryFn: () => fetchList('Users/Select', USER_PAGE),
  });

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['provider-daybook', applied],
    queryFn: () => fetchDaybook(applied),
  });

  const operatorOptions = useMemo(
    () => operatorsForService(allOperators, draft.serviceId),
    [allOperators, draft.serviceId],
  );
  const userOptions: AutocompleteOption[] = useMemo(
    () => users.map((u) => ({
      value: String(getField(u, 'UserMasterID', 'UserID') ?? ''),
      label: String(getField(u, 'FirmName', 'PersonName', 'Name') ?? '—'),
      sublabel: String(getField(u, 'MobileNumber', 'Mobile') ?? ''),
    })),
    [users],
  );

  // Fold the flat provider+service+operator rows into one group per provider. The backend appends
  // a "Total" summary row with ProviderID 0 — drop it; we compute our own provider subtotals.
  const groups = useMemo<ProviderGroup[]>(() => {
    const map = new Map<number, ProviderGroup>();
    for (const r of rows) {
      const pid = num(r, 'ProviderID');
      if (pid <= 0) continue;
      let g = map.get(pid);
      if (!g) {
        g = {
          providerId: pid,
          providerName: String(getField(r, 'ProviderName') ?? pid),
          children: [], totalHits: 0, totalAmount: 0, successHits: 0, successAmount: 0,
          failedHits: 0, pendingHits: 0, retailerCommission: 0, distributerCommission: 0,
          superDistributerCommission: 0, adminCommission: 0, providerCommission: 0, profit: 0,
        };
        map.set(pid, g);
      }
      g.children.push(r);
      g.totalHits += num(r, 'TotalHits');
      g.totalAmount += num(r, 'TotalAmount');
      g.successHits += num(r, 'SuccessHits');
      g.successAmount += num(r, 'SuccessAmount');
      g.failedHits += num(r, 'FailedHits');
      g.pendingHits += num(r, 'PendingHits');
      g.retailerCommission += num(r, 'RetailerCommission');
      g.distributerCommission += num(r, 'DistributerCommission');
      g.superDistributerCommission += num(r, 'SuperDistributerCommission');
      g.adminCommission += num(r, 'AdminCommission');
      g.providerCommission += num(r, 'ProviderCommission');
      g.profit += num(r, 'Profit');
    }
    return [...map.values()].sort((a, b) => a.providerName.localeCompare(b.providerName));
  }, [rows]);

  const totals = useMemo(() => groups.reduce((acc, g) => ({
    totalHits: acc.totalHits + g.totalHits,
    successHits: acc.successHits + g.successHits,
    successAmount: acc.successAmount + g.successAmount,
    retailerCommission: acc.retailerCommission + g.retailerCommission,
    distributerCommission: acc.distributerCommission + g.distributerCommission,
    superDistributerCommission: acc.superDistributerCommission + g.superDistributerCommission,
    adminCommission: acc.adminCommission + g.adminCommission,
    providerCommission: acc.providerCommission + g.providerCommission,
    profit: acc.profit + g.profit,
  }), {
    totalHits: 0, successHits: 0, successAmount: 0, retailerCommission: 0,
    distributerCommission: 0, superDistributerCommission: 0, adminCommission: 0,
    providerCommission: 0, profit: 0,
  }), [groups]);

  const toggle = (pid: number) => setOpen((s) => {
    const next = new Set(s);
    if (next.has(pid)) next.delete(pid); else next.add(pid);
    return next;
  });

  const runSearch = () => setApplied(draft);
  const resetFilters = () => { const d = defaultFilters(); setDraft(d); setApplied(d); };

  // Failed / Pending columns appear only when the range actually has some: a daybook is what
  // the day earned, and two columns of zeros beside the money read as if they counted.
  const showFailed = groups.some((g) => g.failedHits > 0);
  const showPending = groups.some((g) => g.pendingHits > 0);
  // Provider / Success / S. Amt (+ Failed / Pending when present) + the commission block
  // (R / DS / Super DS / Provider / Net profit), admin-only.
  const colCount = 3 + (showFailed ? 1 : 0) + (showPending ? 1 : 0) + (isAdmin ? 5 : 0);

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <ReportActions
            total={groups.length}
            loading={isFetching}
            onRefresh={() => refetch()}
            onExport={() => downloadCsv(rows, def.title)}
            exportDisabled={rows.length === 0}
          />
        }
      />

      <ReportFilterBar onSearch={runSearch} onReset={resetFilters} loading={isFetching}>
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />

            <Autocomplete
              label="Service"
              value={draft.serviceId}
              options={[
                { value: '0', label: 'All services' },
                ...services.map((s, i) => ({ value: String(getField(s, 'ServiceID') ?? i), label: String(getField(s, 'ServiceName', 'Name') ?? '') })),
              ]}
              onChange={(v) => upd({ serviceId: String(v || '0'), operatorId: '0' })}
            />
            <Autocomplete
              label="Operator"
              value={draft.operatorId}
              options={[
                { value: '0', label: 'All operators' },
                ...operatorOptions.map((o, i) => ({ value: String(getField(o, 'OperatorID') ?? i), label: String(getField(o, 'OperatorName', 'Name') ?? '') })),
              ]}
              onChange={(v) => upd({ operatorId: String(v || '0') })}
            />
            <Select label="Provider" value={draft.providerId} onChange={(e) => upd({ providerId: e.target.value })}>
              <option value="0">All providers</option>
              {providers.map((p, i) => {
                const id = String(getField(p, 'ProviderID') ?? i);
                return <option key={id} value={id}>{String(getField(p, 'ProviderName', 'Name') ?? id)}</option>;
              })}
            </Select>
            {showUser && (
              <Autocomplete
                label="Entity / user"
                placeholder="Search by firm name…"
                options={userOptions}
                value={draft.userId}
                loading={usersLoading}
                onChange={(v) => upd({ userId: v })}
              />
            )}
      </ReportFilterBar>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3 text-left">Provider / Operator</th>
                <th className="px-4 py-3 text-right">Success</th>
                <th className="px-4 py-3 text-right">S. Amt</th>
                {showFailed && <th className="px-4 py-3 text-right">Failed</th>}
                {showPending && <th className="px-4 py-3 text-right">Pending</th>}
                {isAdmin && <th className="px-4 py-3 text-right" title="Retailer commission">R. Comm</th>}
                {isAdmin && <th className="px-4 py-3 text-right" title="Distributor commission">DS. Comm</th>}
                {isAdmin && <th className="px-4 py-3 text-right" title="Super-distributor commission">Super DS. Comm</th>}
                {isAdmin && <th className="px-4 py-3 text-right" title="Commission earned from the provider">Provider Comm</th>}
                {isAdmin && <th className="px-4 py-3 text-right" title="What the admin keeps: provider commission less every commission paid down the chain">Net Profit</th>}
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
              ) : groups.length === 0 ? (
                <tr>
                  <td colSpan={colCount}>
                    <EmptyState title="No provider sale found" description="Change the date range / filters and try again." />
                  </td>
                </tr>
              ) : (
                groups.map((g) => {
                  const expanded = open.has(g.providerId);
                  return (
                    <ProviderRows key={g.providerId} group={g} expanded={expanded} isAdmin={isAdmin} showFailed={showFailed} showPending={showPending} onToggle={() => toggle(g.providerId)} />
                  );
                })
              )}
            </tbody>
            {groups.length > 0 && !isFetching && (
              <tfoot>
                <tr className="border-t-2 border-slate-200 bg-slate-50/80 font-semibold text-slate-800">
                  <td className="px-4 py-3">Total</td>
                  <td className="px-4 py-3 text-right tabular-nums">{totals.successHits}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{inr(totals.successAmount)}</td>
                  {showFailed && <td className="px-4 py-3 text-right tabular-nums text-danger-600">{groups.reduce((s, g) => s + g.failedHits, 0)}</td>}
                  {showPending && <td className="px-4 py-3 text-right tabular-nums text-warning-600">{groups.reduce((s, g) => s + g.pendingHits, 0)}</td>}
                  {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(totals.retailerCommission)}</td>}
                  {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(totals.distributerCommission)}</td>}
                  {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(totals.superDistributerCommission)}</td>}
                  {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(totals.providerCommission)}</td>}
                  {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(totals.profit)}</td>}
                </tr>
              </tfoot>
            )}
          </table>
        </div>
      </div>
    </>
  );
}

function ProviderRows({ group: g, expanded, isAdmin, showFailed, showPending, onToggle }: {
  group: ProviderGroup;
  expanded: boolean;
  isAdmin: boolean;
  showFailed: boolean;
  showPending: boolean;
  onToggle: () => void;
}) {
  return (
    <>
      <tr className="cursor-pointer border-b border-slate-100 transition-colors hover:bg-slate-50/60" onClick={onToggle}>
        <td className="px-4 py-3">
          <div className="flex items-center gap-2 font-medium text-slate-800">
            <ChevronRight className={cn('size-4 text-slate-400 transition-transform', expanded && 'rotate-90')} />
            {g.providerName}
            <span className="text-xs font-normal text-slate-400">({g.children.length})</span>
          </div>
        </td>
        <td className="px-4 py-3 text-right tabular-nums text-success-600">{g.successHits}</td>
        <td className="px-4 py-3 text-right tabular-nums">{inr(g.successAmount)}</td>
        {showFailed && <td className="px-4 py-3 text-right tabular-nums text-danger-600">{g.failedHits || ''}</td>}
        {showPending && <td className="px-4 py-3 text-right tabular-nums text-warning-600">{g.pendingHits || ''}</td>}
        {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(g.retailerCommission)}</td>}
        {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(g.distributerCommission)}</td>}
        {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(g.superDistributerCommission)}</td>}
        {isAdmin && <td className="px-4 py-3 text-right tabular-nums">{inr(g.providerCommission)}</td>}
        {isAdmin && <td className={cn('px-4 py-3 text-right font-semibold tabular-nums', g.profit < 0 ? 'text-danger-600' : 'text-success-700')}>{inr(g.profit)}</td>}
      </tr>
      {expanded && g.children.map((c, i) => (
        <tr key={i} className="border-b border-slate-100 bg-slate-50/40 text-slate-600 last:border-0">
          <td className="px-4 py-2 pl-11">
            <span className="font-medium text-slate-700">{String(getField(c, 'Operator') ?? '—')}</span>
            <span className="ml-2 text-xs text-slate-400">{String(getField(c, 'ServiceName') ?? '')}</span>
          </td>
          <td className="px-4 py-2 text-right tabular-nums text-success-600">{num(c, 'SuccessHits')}</td>
          <td className="px-4 py-2 text-right tabular-nums">{inr(num(c, 'SuccessAmount'))}</td>
          {showFailed && <td className="px-4 py-2 text-right tabular-nums text-danger-600">{num(c, 'FailedHits') || ''}</td>}
          {showPending && <td className="px-4 py-2 text-right tabular-nums text-warning-600">{num(c, 'PendingHits') || ''}</td>}
          {isAdmin && <td className="px-4 py-2 text-right tabular-nums">{inr(num(c, 'RetailerCommission'))}</td>}
          {isAdmin && <td className="px-4 py-2 text-right tabular-nums">{inr(num(c, 'DistributerCommission'))}</td>}
          {isAdmin && <td className="px-4 py-2 text-right tabular-nums">{inr(num(c, 'SuperDistributerCommission'))}</td>}
          {isAdmin && <td className="px-4 py-2 text-right tabular-nums">{inr(num(c, 'ProviderCommission'))}</td>}
          {isAdmin && <td className="px-4 py-2 text-right tabular-nums">{inr(num(c, 'Profit'))}</td>}
        </tr>
      ))}
    </>
  );
}
