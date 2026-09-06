import { useEffect, useLayoutEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import { AlertTriangle, Download, MoreVertical, Printer, RefreshCw, Search as SearchIcon, Share2, SlidersHorizontal } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { loadSession } from '@/lib/storage';
import { inr, dateTime, dateYmd, apiDate } from '@/lib/format';
import { exportToCsv } from '@/lib/csv';
import { getField } from '@/lib/row';
import { printReceipt, downloadReceipt, shareReceipt, getReceiptData, statusColor, type ReceiptBrand } from '@/lib/receipt';
import { useBrand } from '@/theme/BrandProvider';
import { cn } from '@/lib/cn';
import { useIsMobileApp } from '@/lib/native';
import { MobileRecordList } from '@/components/ui/MobileRecordList';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { Modal } from '@/components/ui/Modal';
import { pickIcon } from '@/components/layout/iconMap';
import type { ReportColumn, TxnReportConfig } from './txnReportTypes';

const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };
const ADMIN_ROLES = [1, 2, 3, 6]; // admin / super-dist / dist / support see the user picker
const STATUS_SETS: Record<string, string[]> = {
  recharge: ['ALL', 'SUCCESS', 'PROCESS', 'FAILURE'],
  dmt: ['ALL', 'SUCCESS', 'PROCESS', 'FAILURE', 'REFUNDED'],
  simple: ['ALL', 'SUCCESS', 'FAILURE'],
};
const CRITERIA_OPTIONS = [
  { label: 'Transaction ID', value: 'TransactionID' },
  { label: 'Consumer / Account No.', value: 'ConsumerNumber' },
  { label: 'Entity Mobile', value: 'UserMobile' },
  { label: 'Transaction Ref. Id', value: 'ProviderRefNo' },
];

interface Filters {
  fromDate: string;
  toDate: string;
  status: string;
  complaint: string;
  serviceId: string;
  operatorId: string;
  providerId: string;
  walletId: string;
  userId: string | number;
  criteria: string;
  criteriaText: string;
  searchText: string;
  amountType: string;
  groupBy: string;
}

function defaultFilters(config?: TxnReportConfig): Filters {
  return {
    fromDate: apiDate(subDays(new Date(), 7)),
    toDate: apiDate(new Date()),
    status: 'ALL',
    complaint: 'ALL',
    serviceId: '0',
    operatorId: '0',
    providerId: '0',
    walletId: '0',
    userId: '',
    criteria: '',
    criteriaText: '',
    searchText: '',
    amountType: '',
    groupBy: config?.filters?.groupBy?.default ?? config?.filters?.groupBy?.options[0]?.value ?? '',
  };
}

interface TxnResult {
  rows: Record<string, unknown>[];
  count: number;
  summary: Record<string, unknown>;
  /**
   * The API's own explanation when it returned no rows ("No commission slab is assigned to you
   * yet", "You don't have permission…"). Far more useful in the empty state than a generic
   * "nothing found", so it is carried through rather than swallowed.
   */
  message: string;
}

async function fetchTxn(endpoint: string, a: Filters, page: number, pageSize: number): Promise<TxnResult> {
  const res = await apiPost<unknown>(endpoint, {
    TakeCount: pageSize,
    SkipCount: page * pageSize,
    Search: a.searchText,
    SortColumnName: '',
    SortDirection: '',
    FromDate: a.fromDate,
    ToDate: a.toDate,
    Status: a.status,
    Complaint: a.complaint,
    ReportBy: 'Inline',
    UserID: Number(a.userId || 0),
    ServiceID: Number(a.serviceId || 0),
    OperatorID: Number(a.operatorId || 0),
    ProviderID: Number(a.providerId || 0),
    WalletID: Number(a.walletId || 0),
    // Single-day reconciliation reports read `Date`; range reports ignore it. Sending the
    // range's end date keeps both kinds working from the one set of date inputs.
    Date: a.toDate,
    ExportType: 'Select Download',
    IsExcelDownload: false,
    IsPDFDownload: false,
    IsRechargeReport: false,
    Criteria: a.criteria,
    CriteriaText: a.criteriaText,
    GroupBy: a.groupBy,
    AmountCreditDebit: a.amountType,
    MinAmount: 0,
    MaxAmount: 0,
    Latitude: '',
    Longitude: '',
  });
  if (!res.ok) return { rows: [], count: 0, summary: {}, message: specificReason(res.message) };
  const d = res.data as Record<string, unknown> | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Record<string, unknown>, 'list', 'List', 'Data', 'Items');
  const rows = Array.isArray(listRaw) ? (listRaw as Record<string, unknown>[]) : [];
  const count = Array.isArray(d) ? rows.length : Number(getField(d as Record<string, unknown>, 'Count', 'count') ?? rows.length);
  const summary = (Array.isArray(d) ? {} : getField(d as Record<string, unknown>, 'HeaderSummary', 'headerSummary')) as Record<string, unknown>;
  return { rows, count, summary: summary && typeof summary === 'object' ? summary : {}, message: '' };
}

/**
 * Generic "nothing matched" replies carry no information the user can act on, so they are
 * dropped and the empty state falls back to its own actionable hint. Anything more specific
 * (a missing slab, a permission refusal) is worth showing verbatim.
 */
const GENERIC_EMPTY = /^(data not found|no record(s)? found|no data found)\.?$/i;
function specificReason(message: string): string {
  const m = (message ?? '').trim();
  return GENERIC_EMPTY.test(m) ? '' : m;
}

function humanize(key: string) {
  return key.replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/_/g, ' ').replace(/\bI D\b/g, 'ID').trim();
}
const MONEYISH = /(amount|amt|balance|commission|charge|tds|gst|surcharge|opening|closing|credit|debit|sale|purchase|profit|hold)/i;

/** Label of the built-in manual-settle row action (config.manualSettle). */
const SETTLE_ACTION = 'Manual Success / Failed';

export function TxnReportScreen({ title, subtitle, config }: { title: string; subtitle?: string; config: TxnReportConfig }) {
  const toast = useToast();
  const qc = useQueryClient();
  const brand = useBrand();
  const userTypeId = Number(loadSession()?.user?.UserTypeID ?? 0);
  const currentUserId = Number(loadSession()?.user?.UserID ?? 0);
  const f = config.filters ?? {};
  const showUser = Boolean(f.user) && ADMIN_ROLES.includes(userTypeId);
  const hidden = Boolean(config.hideForRoles?.includes(userTypeId));

  const [draft, setDraft] = useState<Filters>(() => defaultFilters(config));
  const [applied, setApplied] = useState<Filters>(draft);
  const [detail, setDetail] = useState<Record<string, unknown> | null>(null);
  const [logTxnId, setLogTxnId] = useState<number | null>(null);
  const [settleRow, setSettleRow] = useState<Record<string, unknown> | null>(null);
  const [complaintRow, setComplaintRow] = useState<Record<string, unknown> | null>(null);
  const [receiptRow, setReceiptRow] = useState<Record<string, unknown> | null>(null);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);

  // Provider status-check / dispatch logs for one transaction (item: "recharge status check log").
  const { data: statusLogs = [], isFetching: logsLoading } = useQuery({
    queryKey: ['status-logs', logTxnId],
    queryFn: () => fetchList('ProviderRequestResponseLog/GetByTransaction', { TransactionID: logTxnId }),
    enabled: logTxnId !== null,
  });
  // Provider callback logs for the same transaction (async provider callbacks — item #8).
  const { data: callbackLogs = [], isFetching: callbackLoading } = useQuery({
    queryKey: ['callback-logs', logTxnId],
    queryFn: () => fetchList('ProviderCallBackLog/GetByTransaction', { TransactionID: logTxnId }),
    enabled: logTxnId !== null,
  });
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: services = [] } = useQuery({
    enabled: Boolean(f.service), queryKey: ['svc-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });
  const { data: allOperators = [] } = useQuery({
    enabled: Boolean(f.operator), queryKey: ['op-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Operator/GetAll', LIST_PAGE),
  });
  const { data: providers = [] } = useQuery({
    enabled: Boolean(f.provider), queryKey: ['prov-opts'], staleTime: 300_000,
    queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAGE),
  });
  const { data: wallets = [] } = useQuery({
    enabled: Boolean(f.wallet), queryKey: ['wallet-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Wallet/Select', {}),
  });
  const { data: users = [], isLoading: usersLoading } = useQuery({
    enabled: showUser, queryKey: ['user-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', USER_PAGE),
  });

  const { data, isFetching, refetch } = useQuery({
    queryKey: ['txn', config.endpoint, applied, page, pageSize],
    queryFn: () => fetchTxn(config.endpoint, applied, page, pageSize),
  });
  const rows = data?.rows ?? [];
  const total = data?.count ?? 0;
  const summary = data?.summary ?? {};
  const emptyReason = data?.message ?? '';

  const userOptions: AutocompleteOption[] = useMemo(
    () => users.map((u) => ({
      value: String(getField(u, 'UserMasterID', 'UserID') ?? ''),
      label: String(getField(u, 'FirmName', 'PersonName', 'Name') ?? '—'),
      sublabel: String(getField(u, 'MobileNumber', 'Mobile') ?? ''),
    })),
    [users],
  );
  const operatorOptions = useMemo(
    () => allOperators.filter((o) => String(getField(o, 'ServiceID') ?? '') === String(draft.serviceId)),
    [allOperators, draft.serviceId],
  );
  const columns = useMemo(
    () => config.columns.filter((c) => !c.roles || c.roles.includes(userTypeId)),
    [config.columns, userTypeId],
  );
  const summaryEntries = Object.entries(summary).filter(([, v]) => typeof v === 'number' || typeof v === 'string');
  // The config's own labels are display-only stubs; the settle entry is a real, wired action.
  const rowActionLabels = useMemo(
    () => (config.manualSettle ? [...(config.actions ?? []), SETTLE_ACTION] : config.actions),
    [config.actions, config.manualSettle],
  );

  const runSearch = () => { setPage(0); setApplied(draft); };

  if (hidden) {
    return (
      <>
        <PageHeader title={title} subtitle={subtitle} icon={pickIcon(title)} />
        <EmptyState
          title="Not applicable"
          description="This report does not apply to your role, so there is nothing to display here."
        />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={title}
        subtitle={subtitle}
        icon={pickIcon(title)}
        actions={
          <>
            <span className="text-sm text-slate-400">{total} rows</span>
            <Button variant="outline" size="sm" onClick={() => exportToCsv(rows, title)} disabled={rows.length === 0}>
              <Download className="size-4" />
              Export
            </Button>
            <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
              <RefreshCw className="size-4" />
            </Button>
          </>
        }
      />

      <Card className="mb-4">
        <CardBody>
          <div className="mb-3 flex items-center gap-2 text-sm font-medium text-slate-600">
            <SlidersHorizontal className="size-4 text-brand-600" />
            Filters
          </div>
          <form
            onSubmit={(e) => { e.preventDefault(); runSearch(); }}
            className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4"
          >
            {f.search && (
              <Input
                label={f.search.label}
                placeholder={f.search.placeholder}
                value={draft.searchText}
                onChange={(e) => upd({ searchText: e.target.value })}
              />
            )}
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />

            {f.service && (
              <Autocomplete
                label="Service"
                value={draft.serviceId}
                options={[
                  { value: '0', label: 'All services' },
                  ...services.map((s, i) => ({ value: String(getField(s, 'ServiceID') ?? i), label: String(getField(s, 'ServiceName', 'Name') ?? '') })),
                ]}
                onChange={(v) => upd({ serviceId: String(v || '0'), operatorId: '0' })}
              />
            )}
            {f.operator && (
              <Autocomplete
                label="Operator"
                value={draft.operatorId}
                options={[
                  { value: '0', label: 'All operators' },
                  ...operatorOptions.map((o, i) => ({ value: String(getField(o, 'OperatorID') ?? i), label: String(getField(o, 'OperatorName', 'Name') ?? '') })),
                ]}
                onChange={(v) => upd({ operatorId: String(v || '0') })}
              />
            )}
            {f.provider && (
              <Select label="Provider" value={draft.providerId} onChange={(e) => upd({ providerId: e.target.value })}>
                <option value="0">All providers</option>
                {providers.map((p, i) => {
                  const id = String(getField(p, 'ProviderID') ?? i);
                  return <option key={id} value={id}>{String(getField(p, 'ProviderName', 'Name') ?? id)}</option>;
                })}
              </Select>
            )}
            {f.wallet && (
              <Select label="Wallet" value={draft.walletId} onChange={(e) => upd({ walletId: e.target.value })}>
                <option value="0">All wallets</option>
                {wallets.map((w, i) => {
                  const id = String(getField(w, 'WalletID') ?? i);
                  return <option key={id} value={id}>{String(getField(w, 'Name', 'WalletName') ?? id)}</option>;
                })}
              </Select>
            )}
            {f.groupBy && (
              <Select label={f.groupBy.label} value={draft.groupBy} onChange={(e) => upd({ groupBy: e.target.value })}>
                {f.groupBy.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </Select>
            )}
            {f.status && (
              <Select label="Status" value={draft.status} onChange={(e) => upd({ status: e.target.value })}>
                {STATUS_SETS[f.status].map((s) => <option key={s} value={s}>{s}</option>)}
              </Select>
            )}
            {f.complaint && (
              <Select label="Complaint" value={draft.complaint} onChange={(e) => upd({ complaint: e.target.value })}>
                {['ALL', 'ACCEPT', 'REJECT', 'PROCESS'].map((s) => <option key={s} value={s}>{s}</option>)}
              </Select>
            )}
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
            {f.amountType && (
              <Select label="Credit / Debit" value={draft.amountType} onChange={(e) => upd({ amountType: e.target.value })}>
                <option value="">All</option>
                <option value="Credit">Credit only</option>
                <option value="Debit">Debit only</option>
              </Select>
            )}
            {f.criteria && (
              <Select label="Search by" value={draft.criteria} onChange={(e) => upd({ criteria: e.target.value, criteriaText: '' })}>
                <option value="">— none —</option>
                {CRITERIA_OPTIONS.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
              </Select>
            )}
            {f.criteria && draft.criteria && (
              <Input
                label={CRITERIA_OPTIONS.find((c) => c.value === draft.criteria)?.label ?? 'Value'}
                placeholder="Enter value"
                value={draft.criteriaText}
                onChange={(e) => upd({ criteriaText: e.target.value })}
              />
            )}

            <div className="flex items-end">
              <Button type="submit" loading={isFetching} className="w-full">
                <SearchIcon className="size-4" />
                Search
              </Button>
            </div>
          </form>
        </CardBody>
      </Card>

      {summaryEntries.length > 0 && (
        <div className="mb-4 grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
          {summaryEntries.slice(0, 12).map(([k, v]) => (
            <div key={k} className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
              <p className="truncate text-xs font-medium text-slate-500">{humanize(k)}</p>
              <p className="mt-0.5 font-display text-lg font-bold text-slate-900">
                {typeof v === 'number' && MONEYISH.test(k) ? inr(v) : String(v)}
              </p>
            </div>
          ))}
        </div>
      )}

      <ReportTable
        columns={columns}
        rows={rows}
        loading={isFetching}
        emptyReason={emptyReason}
        actions={rowActionLabels}
        onView={setDetail}
        onAction={(label, row) => {
          if (label === SETTLE_ACTION) {
            const id = Number(getField(row, 'TransactionMasterID', 'TransactionID') ?? 0);
            if (id > 0) setSettleRow(row);
            else toast.error('No transaction id on this row.');
          } else if (/\blog\b|status\s*check/i.test(label)) {
            const id = Number(getField(row, 'TransactionMasterID', 'TransactionID') ?? 0);
            if (id > 0) setLogTxnId(id);
            else toast.error('No transaction id on this row.');
          } else if (/complaint/i.test(label)) {
            const id = Number(getField(row, 'TransactionMasterID', 'TransactionID') ?? 0);
            if (id > 0) setComplaintRow(row);
            else toast.error('No transaction id on this row.');
          } else if (/print|receipt/i.test(label)) {
            setReceiptRow(row);
          } else {
            toast.info(`${label} — this action is coming soon.`);
          }
        }}
        page={page}
        pageSize={pageSize}
        total={total}
        onPage={setPage}
        onPageSize={(n) => { setPageSize(n); setPage(0); }}
      />

      <Modal open={Boolean(detail)} onClose={() => setDetail(null)} title="Transaction details" size="lg">
        {detail && (
          <div className="grid grid-cols-1 gap-x-6 gap-y-0 sm:grid-cols-2">
            {Object.entries(detail).map(([k, v]) => (
              <div key={k} className="flex justify-between gap-3 border-b border-slate-100 py-1.5 text-sm">
                <span className="shrink-0 text-slate-500">{humanize(k)}</span>
                <span className="truncate text-right font-medium text-slate-800">
                  {v === null || v === undefined || v === '' ? '—' : String(v)}
                </span>
              </div>
            ))}
          </div>
        )}
      </Modal>

      <ManualSettleModal
        row={settleRow}
        onClose={() => setSettleRow(null)}
        onSettled={() => {
          setSettleRow(null);
          void qc.invalidateQueries({ queryKey: ['txn', config.endpoint] });
        }}
      />

      <ComplaintModal
        row={complaintRow}
        currentUserId={currentUserId}
        onClose={() => setComplaintRow(null)}
        onRaised={() => {
          setComplaintRow(null);
          void qc.invalidateQueries({ queryKey: ['txn', config.endpoint] });
        }}
      />

      <ReceiptModal row={receiptRow} brand={{ appName: brand.appName, logoUrl: brand.logoUrl }} onClose={() => setReceiptRow(null)} />

      <Modal open={logTxnId !== null} onClose={() => setLogTxnId(null)} title={`Transaction log · Txn ${logTxnId ?? ''}`} size="lg">
        <div className="space-y-5">
          {/* Provider request / status-check logs */}
          <section>
            <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-slate-400">Provider requests & status checks</h4>
            {logsLoading ? (
              <div className="space-y-2">{Array.from({ length: 2 }).map((_, i) => <Skeleton key={i} className="h-16 rounded-lg" />)}</div>
            ) : statusLogs.length === 0 ? (
              <EmptyState title="No status checks yet" description="This transaction has not been re-checked against the provider. The scheduler runs every few minutes for pending recharges." />
            ) : (
              <div className="space-y-3">
                {statusLogs.map((log, i) => (
                  <div key={i} className="rounded-lg border border-slate-200 p-3 text-sm">
                    <div className="mb-1 flex items-center justify-between">
                      <span className="font-semibold text-slate-700">{String(getField(log, 'Through') || 'Provider call')}</span>
                      <span className="text-xs text-slate-400">{dateTime(String(getField(log, 'DateTime') ?? ''))}</span>
                    </div>
                    <p className="break-all text-xs text-slate-500"><span className="font-medium text-slate-600">Request:</span> {String(getField(log, 'Request') || '—')}</p>
                    <p className="mt-1 break-all text-xs text-slate-500"><span className="font-medium text-slate-600">Response:</span> {String(getField(log, 'Response') || '—')}</p>
                  </div>
                ))}
              </div>
            )}
          </section>

          {/* Provider callback logs (async callbacks the provider posted back for this txn) */}
          <section>
            <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-slate-400">Provider callbacks</h4>
            {callbackLoading ? (
              <div className="space-y-2">{Array.from({ length: 2 }).map((_, i) => <Skeleton key={i} className="h-14 rounded-lg" />)}</div>
            ) : callbackLogs.length === 0 ? (
              <EmptyState title="No callbacks received" description="No provider callback has been posted for this transaction yet." />
            ) : (
              <div className="space-y-3">
                {callbackLogs.map((log, i) => (
                  <div key={i} className="rounded-lg border border-slate-200 p-3 text-sm">
                    <div className="mb-1 flex items-center justify-between">
                      <span className="font-semibold text-slate-700">{String(getField(log, 'Type') || 'Callback')}</span>
                      <span className="text-xs text-slate-400">{dateTime(String(getField(log, 'DateTime') ?? ''))}</span>
                    </div>
                    <p className="break-all text-xs text-slate-500"><span className="font-medium text-slate-600">Reply:</span> {String(getField(log, 'ProviderReply') || '—')}</p>
                  </div>
                ))}
              </div>
            )}
          </section>
        </div>
      </Modal>
    </>
  );
}

/**
 * Manually settle one transaction — TransactionProcessing/UpdateStatusManually.
 * FAILED refunds the wallet and reverses every commission tier; SUCCESS only finalises the
 * status (the debit and commissions were already posted when the transaction went to PROCESS).
 */
function ManualSettleModal({
  row, onClose, onSettled,
}: {
  row: Record<string, unknown> | null;
  onClose: () => void;
  onSettled: () => void;
}) {
  const toast = useToast();
  const [status, setStatus] = useState('');
  const [txnId, setTxnId] = useState('');
  const [remark, setRemark] = useState('');
  const [busy, setBusy] = useState(false);

  const id = Number(getField(row ?? {}, 'TransactionMasterID', 'TransactionID') ?? 0);
  const current = String(getField(row ?? {}, 'Status') ?? '—');
  const amount = Number(getField(row ?? {}, 'Amount')) || 0;

  // Remount per row so the fields never carry over from a previous transaction.
  const reset = () => { setStatus(''); setTxnId(''); setRemark(''); };

  async function submit() {
    if (!status) { toast.error('Choose Success or Failed.'); return; }
    setBusy(true);
    try {
      const res = await apiPost('TransactionProcessing/UpdateStatusManually', {
        TransactionID: id,
        Status: status,
        TxnID: txnId,
        Remark: remark,
      });
      if (!res.ok) { toast.error(res.message || 'Manual update failed.'); return; }
      toast.success(`Txn ${id} marked ${status}.`);
      reset();
      onSettled();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Manual update failed.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal
      open={row !== null}
      onClose={() => { reset(); onClose(); }}
      title={`Manual update · Txn ${id || ''}`}
    >
      <div className="space-y-4">
        <div className="flex items-start gap-3 rounded-lg bg-warning-50 p-3 text-sm text-warning-600">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            This settles the transaction without asking the provider. Current status is{' '}
            <span className="font-semibold">{current}</span>, amount {inr(amount)}. Marking it Failed
            refunds the wallet and reverses all commission.
          </p>
        </div>

        <Select label="New status" value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">— choose —</option>
          <option value="SUCCESS">Success</option>
          <option value="FAILED">Failed (refund)</option>
        </Select>
        <Input
          label="Operator / reference no."
          placeholder="Optional — recorded against the transaction"
          value={txnId}
          onChange={(e) => setTxnId(e.target.value)}
        />
        <Input
          label="Remark"
          placeholder="Why is this being settled manually?"
          value={remark}
          onChange={(e) => setRemark(e.target.value)}
        />

        <div className="flex justify-end gap-2">
          <Button variant="outline" size="sm" onClick={() => { reset(); onClose(); }}>Cancel</Button>
          <Button
            variant={status === 'FAILED' ? 'danger' : 'primary'}
            size="sm"
            loading={busy}
            disabled={!status}
            onClick={submit}
          >
            {status === 'FAILED' ? 'Mark failed & refund' : 'Mark success'}
          </Button>
        </div>
      </div>
    </Modal>
  );
}

const COMPLAINT_TYPES = [
  'Amount debited, service failed',
  'Recharge / bill not received',
  'Wrong number recharged',
  'Double debit',
  'Other',
];

/**
 * Raise a complaint against one transaction — ComplaintMaster/Create. The backend also forwards it
 * to the provider when that provider has a COMPLAINT endpoint configured (Provider → Endpoints).
 */
function ComplaintModal({
  row, currentUserId, onClose, onRaised,
}: {
  row: Record<string, unknown> | null;
  currentUserId: number;
  onClose: () => void;
  onRaised: () => void;
}) {
  const toast = useToast();
  const [type, setType] = useState(COMPLAINT_TYPES[0]);
  const [remark, setRemark] = useState('');
  const [newNumber, setNewNumber] = useState('');
  const [busy, setBusy] = useState(false);

  const r = row ?? {};
  const id = Number(getField(r, 'TransactionMasterID', 'TransactionID') ?? 0);
  const consumer = String(getField(r, 'ConsumerNumber') ?? '');
  const amount = Number(getField(r, 'Amount')) || 0;
  const status = String(getField(r, 'Status') ?? '—');
  const isWrongNumber = /wrong number/i.test(type);

  const reset = () => { setType(COMPLAINT_TYPES[0]); setRemark(''); setNewNumber(''); };

  async function submit() {
    if (!remark.trim()) { toast.error('Please describe the issue.'); return; }
    if (amount <= 0) { toast.error('This transaction has no amount to dispute.'); return; }
    setBusy(true);
    try {
      const res = await apiPost('ComplaintMaster/Create', {
        UserID: Number(getField(r, 'UserID', 'UserMasterID') ?? 0) || currentUserId,
        TransactionMasterID: id,
        ServiceID: Number(getField(r, 'ServiceID') ?? 0),
        OperatorID: Number(getField(r, 'OperatorID') ?? 0),
        ConsumerNumber: consumer,
        Amount: amount,
        CustomerRemark: remark.trim(),
        AdminRemark: '',
        ComplaintStatus: 'Pending',
        TransactionMasterDOCInt: Number(getField(r, 'DOCInt') ?? 0),
        TransactionDate: String(getField(r, 'DOC') ?? ''),
        ComplaintType: type,
        NewConsumerNumber: isWrongNumber ? newNumber.trim() : '',
        ProviderID: Number(getField(r, 'ProviderID') ?? 0),
        ProviderRequestID: String(getField(r, 'ProviderRequestID', 'ProviderRefNo') ?? ''),
        ReferenceNo: String(getField(r, 'ReferenceNo') ?? ''),
      });
      if (!res.ok) { toast.error(res.message || 'Could not raise the complaint.'); return; }
      toast.success(`Complaint raised for Txn ${id}.`);
      reset();
      onRaised();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not raise the complaint.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal open={row !== null} onClose={() => { reset(); onClose(); }} title={`Raise complaint · Txn ${id || ''}`}>
      <div className="space-y-4">
        <div className="rounded-lg bg-slate-50 p-3 text-sm text-slate-600">
          <div className="flex justify-between"><span className="text-slate-400">Number</span><span className="font-medium">{consumer || '—'}</span></div>
          <div className="flex justify-between"><span className="text-slate-400">Amount</span><span className="font-medium tabular-nums">{inr(amount)}</span></div>
          <div className="flex justify-between"><span className="text-slate-400">Status</span><span className="font-medium">{status}</span></div>
        </div>

        <Select label="Complaint type" value={type} onChange={(e) => setType(e.target.value)}>
          {COMPLAINT_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
        </Select>

        {isWrongNumber && (
          <Input
            label="Correct number"
            placeholder="The number it should have gone to"
            value={newNumber}
            onChange={(e) => setNewNumber(e.target.value)}
          />
        )}

        <div>
          <label className="mb-1 block text-xs font-medium text-slate-500">Describe the issue</label>
          <textarea
            value={remark}
            onChange={(e) => setRemark(e.target.value)}
            rows={3}
            placeholder="What went wrong with this transaction?"
            className="w-full resize-y rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-brand-400 focus:ring-2 focus:ring-brand-500/15"
          />
        </div>

        <div className="flex justify-end gap-2">
          <Button variant="outline" size="sm" onClick={() => { reset(); onClose(); }}>Cancel</Button>
          <Button size="sm" loading={busy} disabled={!remark.trim()} onClick={submit}>Raise complaint</Button>
        </div>
      </div>
    </Modal>
  );
}

/**
 * Transaction receipt preview with Print / Download (PNG) / Share. The receipt is built dynamically
 * from the row (see lib/receipt.ts) so it works for every transaction report.
 */
function ReceiptModal({
  row, brand, onClose,
}: {
  row: Record<string, unknown> | null;
  brand: ReceiptBrand;
  onClose: () => void;
}) {
  const toast = useToast();
  const [busy, setBusy] = useState<'download' | 'share' | null>(null);
  const d = getReceiptData(row ?? {}, brand);

  const onDownload = async () => {
    if (!row) return;
    setBusy('download');
    try { if (!(await downloadReceipt(row, brand))) toast.error('Could not generate the receipt image.'); }
    finally { setBusy(null); }
  };

  const onShare = async () => {
    if (!row) return;
    setBusy('share');
    try {
      const res = await shareReceipt(row, brand);
      if (!res.ok && res.reason === 'unsupported') {
        toast.info('Sharing not supported here — downloading the image instead.');
        await downloadReceipt(row, brand);
      } else if (!res.ok && res.reason === 'error') {
        toast.error('Could not share the receipt.');
      }
      // 'cancelled' → user dismissed the share sheet; stay silent.
    } finally { setBusy(null); }
  };

  return (
    <Modal open={row !== null} onClose={onClose} title={`Receipt · Txn ${d.id}`}>
      <div className="space-y-4">
        <div className="mx-auto max-w-[340px] overflow-hidden rounded-xl border border-slate-200 bg-white">
          <div className="px-4 pb-2 pt-4 text-center">
            <div className="font-display text-lg font-extrabold text-slate-900">{d.appName}</div>
            <div className="text-[11px] text-slate-500">Transaction Receipt</div>
            <span className="mt-2 inline-block rounded-full px-3 py-0.5 text-xs font-bold text-white" style={{ backgroundColor: statusColor(d.status) }}>{d.status}</span>
          </div>
          <div>
            {d.lines.map((l) => (
              <div key={l.label} className="flex justify-between gap-3 border-b border-dashed border-slate-200 px-4 py-1.5 text-[12.5px] last:border-0">
                <span className="whitespace-nowrap text-slate-500">{l.label}</span>
                <span className="break-all text-right font-semibold text-slate-800">{l.value}</span>
              </div>
            ))}
          </div>
          <div className="px-4 py-3 text-center text-[10.5px] text-slate-400">System-generated receipt</div>
        </div>

        <div className="flex flex-wrap justify-end gap-2">
          <Button variant="outline" size="sm" onClick={() => { if (row && !printReceipt(row, brand)) toast.error('Allow pop-ups to print.'); }}>
            <Printer className="size-4" /> Print
          </Button>
          <Button variant="outline" size="sm" loading={busy === 'download'} onClick={onDownload}>
            <Download className="size-4" /> Download
          </Button>
          <Button size="sm" loading={busy === 'share'} onClick={onShare}>
            <Share2 className="size-4" /> Share
          </Button>
        </div>
      </div>
    </Modal>
  );
}

function formatCell(col: ReportColumn, value: unknown, serial?: number): ReactNode {
  // Any "#" column shows a running serial (1,2,3…) that continues across pages, instead of the
  // raw row id. The id itself stays in the row data for the row actions (Print/Log/etc.).
  if (col.type === 'serial' || col.label === '#') return <span className="tabular-nums text-slate-500">{serial}</span>;
  if (value === null || value === undefined || value === '') return <span className="text-slate-300">—</span>;
  switch (col.type) {
    case 'status': return <StatusBadge status={String(value)} />;
    case 'money': return <span className="tabular-nums">{inr(Number(value) || 0)}</span>;
    case 'number': return <span className="tabular-nums">{String(value)}</span>;
    case 'date': return dateYmd(String(value));
    case 'bold': return <span className="font-medium text-slate-800">{String(value)}</span>;
    case 'image':
      return <img src={String(value)} alt="" className="h-7 w-7 rounded object-contain" onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = 'none'; }} />;
    default:
      // A date-time value in an untyped column — format it rather than leak raw ISO.
      if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(value)) return dateYmd(value);
      return String(value);
  }
}

const PAGE_SIZES = [10, 20, 50, 100];

function ReportTable({
  columns, rows, loading, emptyReason, actions, onView, onAction, page, pageSize, total, onPage, onPageSize,
}: {
  columns: ReportColumn[];
  rows: Record<string, unknown>[];
  loading: boolean;
  /** The API's reason for returning nothing, shown instead of the generic empty message. */
  emptyReason?: string;
  actions?: string[];
  onView: (row: Record<string, unknown>) => void;
  onAction: (label: string, row: Record<string, unknown>) => void;
  page: number;
  pageSize: number;
  total: number;
  onPage: (p: number) => void;
  onPageSize: (n: number) => void;
}) {
  const colCount = columns.length + 1;
  const from = total === 0 ? 0 : page * pageSize + 1;
  const to = Math.min((page + 1) * pageSize, total);
  const isRight = (c: ReportColumn) => c.type === 'money' || c.type === 'number';
  const serialFor = (i: number) => page * pageSize + i + 1;
  const isMobile = useIsMobileApp();

  // Every txn report is a wide table; on a phone the columns that matter scroll off screen.
  // Same rows as cards, with the row's actions kept attached to it.
  const mobileBody = (
    <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
      {loading ? (
        <div className="space-y-3 p-4">
          {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-12 rounded-lg" />)}
        </div>
      ) : rows.length === 0 ? (
        <EmptyState
          title={emptyReason ? 'Nothing to show' : 'No records found'}
          description={emptyReason || 'Change your date range / filters and try again.'}
        />
      ) : (
        <MobileRecordList
          rows={rows as unknown[]}
          actions={(row) => (
            <RowActions row={row as Record<string, unknown>} onView={onView} actions={actions} onAction={onAction} />
          )}
          fields={columns.map((c) => ({
            key: c.key + c.label,
            label: c.label,
            render: (row: unknown) =>
              formatCell(
                c,
                getField(row as Record<string, unknown>, c.key),
                serialFor(rows.indexOf(row as Record<string, unknown>)),
              ),
          }))}
        />
      )}
    </div>
  );

  if (isMobile) return mobileBody;

  return (
    <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
              <th className="w-10 px-3 py-3" />
              {columns.map((c) => (
                <th key={c.key + c.label} className={cn('whitespace-nowrap px-4 py-3', isRight(c) ? 'text-right' : 'text-left')}>
                  {c.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              Array.from({ length: 8 }).map((_, r) => (
                <tr key={r} className="border-b border-slate-100 last:border-0">
                  {Array.from({ length: Math.max(colCount, 1) }).map((__, c) => (
                    <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[120px]" /></td>
                  ))}
                </tr>
              ))
            ) : rows.length === 0 ? (
              <tr>
                <td colSpan={Math.max(colCount, 1)}>
                  <EmptyState
                    title={emptyReason ? 'Nothing to show' : 'No records found'}
                    description={emptyReason || 'Change your date range / filters and try again.'}
                  />
                </td>
              </tr>
            ) : (
              rows.map((row, i) => (
                <tr key={i} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                  <td className="px-3 py-3">
                    <RowActions row={row} onView={onView} actions={actions} onAction={onAction} />
                  </td>
                  {columns.map((c) => (
                    <td key={c.key + c.label} className={cn('whitespace-nowrap px-4 py-3 text-slate-700', isRight(c) && 'text-right')}>
                      {formatCell(c, getField(row, c.key), serialFor(i))}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 px-4 py-3 text-sm text-slate-500">
        <div className="flex items-center gap-2">
          <span>Rows per page</span>
          <select
            className="rounded-lg border border-slate-200 bg-white px-2 py-1 text-sm text-slate-700 outline-none focus:border-brand-400 focus:ring-2 focus:ring-brand-500/15"
            value={pageSize}
            disabled={loading}
            onChange={(e) => onPageSize(Number(e.target.value))}
          >
            {PAGE_SIZES.map((n) => <option key={n} value={n}>{n}</option>)}
          </select>
        </div>
        <span>{from}–{to} of {total}</span>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" disabled={page === 0 || loading} onClick={() => onPage(page - 1)}>Prev</Button>
          <Button variant="outline" size="sm" disabled={to >= total || loading} onClick={() => onPage(page + 1)}>Next</Button>
        </div>
      </div>
    </div>
  );
}

function RowActions({
  row, onView, actions, onAction,
}: {
  row: Record<string, unknown>;
  onView: (row: Record<string, unknown>) => void;
  actions?: string[];
  onAction: (label: string, row: Record<string, unknown>) => void;
}) {
  const [open, setOpen] = useState(false);
  const btnRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const [pos, setPos] = useState<{ top: number; left: number } | null>(null);

  // Position the menu in a portal (fixed) from the trigger's rect, so the table's horizontal
  // scroll container can't clip it. Flip up / clamp so it stays on-screen.
  useLayoutEffect(() => {
    if (!open || !btnRef.current) return;
    const r = btnRef.current.getBoundingClientRect();
    const menuW = 176; // w-44
    const menuH = 8 + (1 + (actions?.length ?? 0)) * 32;
    let left = Math.max(8, r.right - menuW);
    if (left + menuW > window.innerWidth - 8) left = window.innerWidth - 8 - menuW;
    let top = r.bottom + 4;
    if (top + menuH > window.innerHeight - 8) top = Math.max(8, r.top - menuH - 4);
    setPos({ top, left });
  }, [open, actions]);

  // Close on outside click, and on scroll/resize (the fixed menu would otherwise drift).
  useEffect(() => {
    if (!open) return;
    const onDoc = (e: MouseEvent) => {
      if (menuRef.current?.contains(e.target as Node) || btnRef.current?.contains(e.target as Node)) return;
      setOpen(false);
    };
    const onMove = () => setOpen(false);
    document.addEventListener('mousedown', onDoc);
    window.addEventListener('scroll', onMove, true);
    window.addEventListener('resize', onMove);
    return () => {
      document.removeEventListener('mousedown', onDoc);
      window.removeEventListener('scroll', onMove, true);
      window.removeEventListener('resize', onMove);
    };
  }, [open]);

  return (
    <>
      <button
        ref={btnRef}
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
        className="grid size-7 cursor-pointer place-items-center rounded-lg text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600"
      >
        <MoreVertical className="size-4" />
      </button>
      {open && pos && createPortal(
        <div
          ref={menuRef}
          role="menu"
          style={{ position: 'fixed', top: pos.top, left: pos.left }}
          className="z-50 w-44 overflow-hidden rounded-lg border border-slate-200 bg-white py-1 shadow-lg"
        >
          <button
            type="button"
            role="menuitem"
            className="block w-full px-3 py-1.5 text-left text-sm font-medium text-brand-700 hover:bg-brand-50"
            onClick={() => { onView(row); setOpen(false); }}
          >
            View details
          </button>
          {actions?.map((a) => (
            <button
              key={a}
              type="button"
              role="menuitem"
              className="block w-full px-3 py-1.5 text-left text-sm text-slate-700 hover:bg-slate-50"
              onClick={() => { onAction(a, row); setOpen(false); }}
            >
              {a}
            </button>
          ))}
        </div>,
        document.body,
      )}
    </>
  );
}
