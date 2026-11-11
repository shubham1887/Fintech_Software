import { useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import { AlertTriangle, Download, MoreVertical, Printer, Share2 } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { loadSession } from '@/lib/storage';
import { inr, dateTime, apiDate } from '@/lib/format';
import { useCsvExport } from '@/lib/exportAll';
import { getField } from '@/lib/row';
import { UserTypeCode } from '@/lib/roles';
import { isOpenComplaint } from '@/lib/complaints';
import { printReceipt, downloadReceipt, shareReceipt, getReceiptData, statusColor, type ReceiptBrand } from '@/lib/receipt';
import { useBrand } from '@/theme/BrandProvider';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { Modal } from '@/components/ui/Modal';
import {
  ReportActions, ReportFilterBar, ReportPager, ReportSummary, ReportTable,
  humanize, operatorsForService, summaryFromRecord, type ReportTableColumn,
} from '@/components/report';
import { pickIcon } from '@/components/layout/iconMap';
import { txnState } from '@/lib/txnStatus';
import type { TxnReportConfig } from './txnReportTypes';

const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };
const ADMIN_ROLES = [1, 2, 3, 6]; // admin / super-dist / dist / support see the user picker
// Admin and support own the data, so they read a row whole. For every other role a row is only
// ever the columns their own config left them — see `visibleKeys` below.
const FULL_ROW_ROLES = [1, 6];
const STATUS_SETS: Record<string, string[]> = {
  recharge: ['ALL', 'SUCCESS', 'PROCESS', 'FAILURE'],
  dmt: ['ALL', 'SUCCESS', 'PROCESS', 'FAILURE', 'REFUNDED'],
  simple: ['ALL', 'SUCCESS', 'FAILURE'],
  // Fund movements. PROCESS is a real, common state here — a fund or reverse request that is
  // raised and waiting on a decision — so 'simple' (SUCCESS / FAILURE only) would hide exactly
  // the rows somebody opens a fund report to chase.
  fund: ['ALL', 'SUCCESS', 'PROCESS', 'FAILURE'],
  // The hold report reuses the same dropdown — its two states are the directions a hold moves.
  hold: ['ALL', 'HOLD', 'RELEASE'],
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
    fromDate: apiDate(config?.defaultFromToday ? new Date() : subDays(new Date(), 7)),
    toDate: apiDate(new Date()),
    status: config?.defaultStatus ?? 'ALL',
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

async function fetchTxn(
  endpoint: string,
  a: Filters,
  skip: number,
  take: number,
  fixedPayload?: Record<string, unknown>,
  /**
   * Mark the request as an export.
   *
   * It no longer means "send me everything in one answer". Asking for that — a TakeCount of 0 —
   * is read by the transaction store as "use your default page", which is 50 rows: a 322-row
   * report downloaded as 50 and said so in a toast nobody expected to read. The export now names
   * the window it wants and walks the windows (see `fetchAllRows`), and the flag only tells the
   * service not to cap a window the caller did not name.
   */
  exportAll = false,
): Promise<TxnResult> {
  const res = await apiPost<unknown>(endpoint, {
    TakeCount: take,
    SkipCount: skip,
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
    IsExcelDownload: exportAll,
    PageNumber: Math.floor(skip / Math.max(1, take)) + 1,
    PageSize: take,
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
    // Last, so a screen that pins a field (e.g. the Reverse Requests queue pinning
    // Type=REVERSE_REQUEST) wins over the generic default above.
    ...(fixedPayload ?? {}),
  });
  if (!res.ok) return { rows: [], count: 0, summary: {}, message: specificReason(res.message) };
  const d = res.data as Record<string, unknown> | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Record<string, unknown>, 'list', 'List', 'Data', 'Items');
  const all = Array.isArray(listRaw) ? (listRaw as Record<string, unknown>[]) : [];
  const summary = (Array.isArray(d) ? {} : getField(d as Record<string, unknown>, 'HeaderSummary', 'headerSummary')) as Record<string, unknown>;

  // Rows per page is a promise to the reader: pick 20 and you get 20.
  //
  // Not every endpoint pages server-side. Several answer with the whole result set and a truthful
  // total, so the page has to be cut here. The test for which kind of reply this is used to be
  // "did it send more rows than I asked for", which gets the common case right and the edge cases
  // wrong: a 15-row result with a page size of 20 looks paged, and so does an endpoint that
  // honours TakeCount but ignores SkipCount — the second one then shows page 1 forever.
  //
  // The server's own count answers it properly. Fewer rows than the total means this IS a page of
  // a larger set, so it is passed through; rows equal to the total means the whole set arrived and
  // the page is cut from the offset the request named. Both readings agree with the pager, which
  // is driven by the same total.
  const reported = Array.isArray(d) ? all.length : Number(getField(d as Record<string, unknown>, 'Count', 'count') ?? all.length);
  const count = Math.max(reported, all.length);
  // An export asked for a window of its own and keeps the lot — no page is cut from it.
  const serverPaged = all.length < count;
  const rows = exportAll || serverPaged ? all : all.slice(skip, skip + take);

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
  const [statusCheckRow, setStatusCheckRow] = useState<Record<string, unknown> | null>(null);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const { exporting, runExport } = useCsvExport();

  // Who sees WHAT in the transaction log. The provider rows are our own side of the wire — the
  // upstream endpoint, the vendor id, our opening/closing balance and the margin — so they belong
  // to admin/support alone. An API user opens the same "Log" action and must be shown its OWN API
  // traffic instead: the request it posted to us, our reply, and the callbacks we pushed back.
  // The server enforces both (ApiAccessPolicy + the services); this only keeps the UI from asking
  // for rows it may not have and from drawing an empty provider panel at a role that never fills it.
  const isProviderLogViewer = userTypeId === UserTypeCode.Admin || userTypeId === UserTypeCode.SupportUser;
  const isApiUser = userTypeId === UserTypeCode.ApiUser;
  const showApiUserLog = isProviderLogViewer || isApiUser;

  // Provider status-check / dispatch logs for one transaction (item: "recharge status check log").
  const { data: statusLogs = [], isFetching: logsLoading } = useQuery({
    queryKey: ['status-logs', logTxnId],
    queryFn: () => fetchList('ProviderRequestResponseLog/GetByTransaction', { TransactionID: logTxnId }),
    enabled: logTxnId !== null && isProviderLogViewer,
  });
  // Provider callback logs for the same transaction (async provider callbacks — item #8).
  const { data: callbackLogs = [], isFetching: callbackLoading } = useQuery({
    queryKey: ['callback-logs', logTxnId],
    queryFn: () => fetchList('ProviderCallBackLog/GetByTransaction', { TransactionID: logTxnId }),
    enabled: logTxnId !== null && isProviderLogViewer,
  });
  // The API user's own exchange with us: what it sent, what we answered.
  const { data: apiUserLogs = [], isFetching: apiUserLoading } = useQuery({
    queryKey: ['api-user-logs', logTxnId],
    queryFn: () => fetchList('APIUserInitialRequestResponseLog/GetByTransaction', { TransactionMasterID: logTxnId }),
    enabled: logTxnId !== null && showApiUserLog,
  });
  // The callbacks we pushed to the API user for this transaction, with their server's reply.
  const { data: apiUserCallbacks = [], isFetching: apiUserCallbackLoading } = useQuery({
    queryKey: ['api-user-callback-logs', logTxnId],
    queryFn: () => fetchList('APIUserCallbackRequestResponseLog/GetByTransaction', { TransactionMasterID: logTxnId }),
    enabled: logTxnId !== null && showApiUserLog,
  });
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: services = [] } = useQuery({
    enabled: Boolean(f.service), queryKey: ['svc-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });
  // A report that is about ONE service opens on one rather than on "All services". Commission
  // Structure is the case: a rate card for every service at once is forty-odd rows of three
  // different products interleaved, and the first thing anyone did on that screen was pick a
  // service. Applied once, when the options first arrive, and only while the filter is still on
  // its default — so it never fights an operator who has already chosen something.
  useEffect(() => {
    if (!config.defaultFirstService || services.length === 0) return;
    const first = String(getField(services[0], 'ServiceID') ?? '');
    if (!first || first === '0') return;
    setDraft((d) => (d.serviceId === '0' ? { ...d, serviceId: first } : d));
    setApplied((a) => (a.serviceId === '0' ? { ...a, serviceId: first } : a));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [config.defaultFirstService, services]);

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

  const { data, isFetching, isPending, refetch } = useQuery({
    queryKey: ['txn', config.endpoint, config.fixedPayload, applied, page, pageSize],
    queryFn: () => fetchTxn(config.endpoint, applied, page * pageSize, pageSize, config.fixedPayload),
  });
  const rows = data?.rows ?? [];
  const total = data?.count ?? 0;
  const summary = data?.summary ?? {};
  const emptyReason = data?.message ?? '';

  // Export collects the WHOLE result set for the current filters, a window at a time, rather
  // than writing out the page on screen. Asking for "all of it" in one request is what this used
  // to do, and it is the one thing the transaction store quietly refuses: a take of 0 means "use
  // your default page" there, so a 322-row report came back as 50.
  const onExport = () => runExport({
    filename: title,
    columns: visibleKeys ?? undefined,
    onScreen: rows,
    fetchAll: (skip, take) =>
      fetchTxn(config.endpoint, applied, skip, take, config.fixedPayload, true)
        .then((r) => ({ rows: r.rows, count: r.count })),
  });

  const userOptions: AutocompleteOption[] = useMemo(
    () => users.map((u) => ({
      value: String(getField(u, 'UserMasterID', 'UserID') ?? ''),
      label: String(getField(u, 'FirmName', 'PersonName', 'Name') ?? '—'),
      sublabel: String(getField(u, 'MobileNumber', 'Mobile') ?? ''),
      // The tier, so a name in this list can be told apart from the same name a level up or down.
      badge: String(getField(u, 'UserType', 'UserTypeName', 'Role') ?? ''),
    })),
    [users],
  );
  const operatorOptions = useMemo(
    () => operatorsForService(allOperators, draft.serviceId),
    [allOperators, draft.serviceId],
  );
  const columns = useMemo(
    () => config.columns.filter((c) => !c.roles || c.roles.includes(userTypeId)),
    [config.columns, userTypeId],
  );
  // The config's report columns, expressed as the shared report table's columns. Money columns are
  // marked so the common totals footer picks them up without every report config repeating itself.
  const tableColumns = useMemo<ReportTableColumn[]>(
    () => columns.map((c) => ({
      key: c.key,
      label: c.label,
      // "#" has always meant a running serial in these reports, whatever the config says.
      type: (c.type === 'serial' || c.label === '#' ? 'serial' : c.type ?? 'text') as ReportTableColumn['type'],
      // Signed columns total too — a commission column's total is the net of what was earned
      // against what was charged, which is the figure anyone reconciling the page wants.
      total: c.type === 'money' || c.type === 'signedMoney' || c.type === 'plusMinusMoney',
      totalMode: c.totalMode,
    })),
    [columns],
  );
  /**
   * The fields this role may read off a row.
   *
   * The table drops the columns a role is not allowed to see, but "View details" and Export both
   * worked off the RAW row — every field the API sent, including the ones the column gate had just
   * removed. On Commission Structure that meant a retailer could open any row's kebab menu, or
   * export the page, and read what their distributor and super distributor earn on the same
   * transaction. Both now go through the same role-filtered column list the table uses.
   *
   * `null` = no restriction (admin / support), which keeps their detail view and export complete.
   */
  const visibleKeys = useMemo<string[] | null>(
    () => (FULL_ROW_ROLES.includes(userTypeId)
      ? null
      : columns.filter((c) => c.type !== 'serial' && c.key !== '_serial').map((c) => c.key)),
    [columns, userTypeId],
  );
  const detailEntries = useMemo<Array<[string, unknown]>>(() => {
    if (!detail) return [];
    if (visibleKeys === null) return Object.entries(detail);
    return columns
      .filter((c) => visibleKeys.includes(c.key))
      .map((c) => [c.label, detail[c.key]] as [string, unknown]);
  }, [detail, columns, visibleKeys]);
  const summaryItems = useMemo(() => summaryFromRecord(summary), [summary]);
  // The config's own labels are display-only stubs; the settle entry is a real, wired action.
  // "Status Check" is only offered where the report names an endpoint to ask the provider with —
  // listing it otherwise just reopened the log under a second name.
  const rowActionLabels = useMemo(() => {
    let labels = config.actions ?? [];
    if (!config.statusCheckEndpoint) labels = labels.filter((l) => !/status\s*check/i.test(l));
    return config.manualSettle ? [...labels, SETTLE_ACTION] : labels;
  }, [config.actions, config.manualSettle, config.statusCheckEndpoint]);

  // Searching used to give no feedback at all: a search that matched nothing looked identical to a
  // search that had not run yet, and the API's reason for an empty result ("no slab assigned",
  // "you don't have permission") was buried in the empty state. Every deliberate search now ends in
  // a toast — how many rows matched, or why there are none. Only a search the user actually
  // triggered announces itself; background refetches and paging stay quiet.
  const [searchSeq, setSearchSeq] = useState(0);
  const announcedSeq = useRef(0);
  const runSearch = () => { setPage(0); setApplied(draft); setSearchSeq((s) => s + 1); };
  const resetFilters = () => {
    const d = defaultFilters(config);
    setDraft(d); setApplied(d); setPage(0); setSearchSeq((s) => s + 1);
  };

  useEffect(() => {
    if (searchSeq === 0 || announcedSeq.current === searchSeq) return;
    if (isFetching || isPending) return;   // wait for this search to settle
    announcedSeq.current = searchSeq;

    if (total > 0) toast.success(`${total} ${total === 1 ? 'record' : 'records'} found.`);
    else if (emptyReason) toast.info(emptyReason);
    else toast.info('No record matched this search. Try a wider date range or different filters.');
  }, [searchSeq, isFetching, isPending, total, emptyReason, toast]);

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
          <ReportActions
            total={total}
            loading={isFetching}
            onRefresh={() => refetch()}
            onExport={onExport}
            exportLoading={exporting}
            exportDisabled={rows.length === 0 || exporting}
          />
        }
      />

      <ReportFilterBar onSearch={runSearch} onReset={resetFilters} loading={isFetching}>
            {f.search && (
              <Input
                label={f.search.label}
                placeholder={f.search.placeholder}
                value={draft.searchText}
                onChange={(e) => upd({ searchText: e.target.value })}
              />
            )}
            {!f.hideDates && (
              <>
                <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
                <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
              </>
            )}

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
                {/* The VALUE stays the machine token the API filters on; only the label is
                    humanised, so the dropdown reads the same words as the Status column
                    ("Pending", not "PROCESS"). */}
                {STATUS_SETS[f.status].map((s) => (
                  <option key={s} value={s}>{s === 'ALL' ? 'All' : txnState(s).label}</option>
                ))}
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
      </ReportFilterBar>

      <ReportSummary items={summaryItems} />

      <ReportTable
        columns={tableColumns}
        rows={rows}
        loading={isFetching}
        serialStart={page * pageSize + 1}
        emptyReason={emptyReason}
        rowLeading={(row) => (
          <RowActions
            row={row}
            onView={setDetail}
            actions={rowActionLabels}
            onAction={(label, r) => {
              if (label === SETTLE_ACTION) {
                const id = Number(getField(r, 'TransactionMasterID', 'TransactionID') ?? 0);
                if (id > 0) setSettleRow(r);
                else toast.error('No transaction id on this row.');
              } else if (/status\s*check/i.test(label)) {
                // Re-ask the PROVIDER — a different thing from reading the log of what was
                // already asked. Opens a confirm dialog; the call goes out on Submit.
                const id = Number(getField(r, 'TransactionMasterID', 'TransactionID') ?? 0);
                if (id > 0) setStatusCheckRow(r);
                else toast.error('No transaction id on this row.');
              } else if (/\blog\b/i.test(label)) {
                const id = Number(getField(r, 'TransactionMasterID', 'TransactionID') ?? 0);
                if (id > 0) setLogTxnId(id);
                else toast.error('No transaction id on this row.');
              } else if (/complaint/i.test(label)) {
                const id = Number(getField(r, 'TransactionMasterID', 'TransactionID') ?? 0);
                if (id > 0) setComplaintRow(r);
                else toast.error('No transaction id on this row.');
              } else if (/print|receipt/i.test(label)) {
                setReceiptRow(r);
              } else {
                toast.info(`${label} — this action is coming soon.`);
              }
            }}
          />
        )}
      />

      <ReportPager
        page={page}
        pageSize={pageSize}
        total={total}
        loading={isFetching}
        onPage={setPage}
        onPageSize={(n) => { setPageSize(n); setPage(0); }}
      />

      <Modal open={Boolean(detail)} onClose={() => setDetail(null)} title="Transaction details" size="lg">
        {detail && (
          <div className="grid grid-cols-1 gap-x-6 gap-y-0 sm:grid-cols-2">
            {detailEntries.map(([k, v]) => (
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

      <StatusCheckModal
        row={statusCheckRow}
        endpoint={config.statusCheckEndpoint ?? ''}
        onClose={() => setStatusCheckRow(null)}
        onChecked={() => void qc.invalidateQueries({ queryKey: ['txn', config.endpoint] })}
        onViewLog={(id) => { setStatusCheckRow(null); setLogTxnId(id); }}
      />

      <ReceiptModal row={receiptRow} brand={{ appName: brand.appName, logoUrl: brand.logoUrl }} onClose={() => setReceiptRow(null)} />

      <Modal open={logTxnId !== null} onClose={() => setLogTxnId(null)} title={`Transaction log · Txn ${logTxnId ?? ''}`} size="lg">
        <div className="space-y-5">
          {/* The API user's own traffic. For an API user this IS the log — it never sees our
              provider exchange — and admin/support get it as well, because it is what an
              integrator quotes when they open a ticket. */}
          {showApiUserLog && (
            <>
              <section>
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-slate-400">
                  {isApiUser ? 'Your API request & response' : 'API user request & response'}
                </h4>
                {apiUserLoading ? (
                  <div className="space-y-2">{Array.from({ length: 2 }).map((_, i) => <Skeleton key={i} className="h-16 rounded-lg" />)}</div>
                ) : apiUserLogs.length === 0 ? (
                  <EmptyState
                    title="No API request logged"
                    description={isApiUser
                      ? 'This transaction was not created through your API, so there is no request of yours to show.'
                      : 'This transaction was not created through the API, so there is no integrator request to show.'}
                  />
                ) : (
                  <div className="space-y-3">
                    {apiUserLogs.map((log, i) => (
                      <div key={i} className="rounded-lg border border-slate-200 p-3 text-sm">
                        <div className="mb-1 flex items-center justify-between">
                          <span className="font-semibold text-slate-700">{String(getField(log, 'APIUserRequestID') || 'API request')}</span>
                          <span className="text-xs text-slate-400">{dateTime(String(getField(log, 'DateTime') ?? ''))}</span>
                        </div>
                        <p className="break-all text-xs text-slate-500"><span className="font-medium text-slate-600">Request:</span> {String(getField(log, 'Request') || '—')}</p>
                        <p className="mt-1 break-all text-xs text-slate-500"><span className="font-medium text-slate-600">Response:</span> {String(getField(log, 'Response') || '—')}</p>
                      </div>
                    ))}
                  </div>
                )}
              </section>

              <section>
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-slate-400">
                  {isApiUser ? 'Callbacks we sent you' : 'Callbacks sent to the API user'}
                </h4>
                {apiUserCallbackLoading ? (
                  <div className="space-y-2">{Array.from({ length: 2 }).map((_, i) => <Skeleton key={i} className="h-14 rounded-lg" />)}</div>
                ) : apiUserCallbacks.length === 0 ? (
                  <EmptyState
                    title="No callback pushed yet"
                    description="A callback is pushed once the transaction reaches a final status. A transaction still in process has none."
                  />
                ) : (
                  <div className="space-y-3">
                    {apiUserCallbacks.map((log, i) => (
                      <div key={i} className="rounded-lg border border-slate-200 p-3 text-sm">
                        <div className="mb-1 flex items-center justify-between">
                          <span className="font-semibold text-slate-700">{String(getField(log, 'APIUserRequestID') || 'Callback')}</span>
                          <span className="text-xs text-slate-400">{dateTime(String(getField(log, 'DateTime') ?? ''))}</span>
                        </div>
                        <p className="break-all text-xs text-slate-500"><span className="font-medium text-slate-600">We sent:</span> {String(getField(log, 'Request') || '—')}</p>
                        <p className="mt-1 break-all text-xs text-slate-500"><span className="font-medium text-slate-600">Your reply:</span> {String(getField(log, 'Response') || '—')}</p>
                      </div>
                    ))}
                  </div>
                )}
              </section>
            </>
          )}

          {/* Provider request / status-check logs — admin/support only (see isProviderLogViewer). */}
          {isProviderLogViewer && (
            <>
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
            </>
          )}
        </div>
      </Modal>
    </>
  );
}

/**
 * Status check — re-ask the PROVIDER what became of one transaction.
 *
 * This used to open the transaction log, which is the record of what we asked BEFORE; nothing
 * went out to the provider at all, so "Status Check" and "Log" were two names for one panel and
 * a stuck transaction stayed stuck. Submit now posts to the report's own status endpoint
 * (`TransactionProcessing/Get…TransactionStatus`), which enquires upstream and settles the
 * transaction from the answer — wallet credit and commission reversal included on a failure.
 *
 * It is a confirm-then-send dialog rather than a one-click action because the reply can move
 * money, and because a provider's enquiry API is rate-limited: the operator should see which
 * transaction they are about to re-ask, and its current status, first.
 */
function StatusCheckModal({
  row, endpoint, onClose, onChecked, onViewLog,
}: {
  row: Record<string, unknown> | null;
  endpoint: string;
  onClose: () => void;
  onChecked: () => void;
  onViewLog: (id: number) => void;
}) {
  const toast = useToast();
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState<string | null>(null);

  const id = Number(getField(row ?? {}, 'TransactionMasterID', 'TransactionID') ?? 0);
  const current = String(getField(row ?? {}, 'Status') ?? '—');
  const amount = Number(getField(row ?? {}, 'Amount')) || 0;
  const number = String(getField(row ?? {}, 'ConsumerNumber') ?? '—');
  const reference = String(getField(row ?? {}, 'ReferenceNo', 'ProviderRefNo') ?? '') || '—';
  const state = txnState(current);

  // A fresh row means a fresh dialog: never show the previous transaction's answer.
  useEffect(() => { setResult(null); setBusy(false); }, [id]);

  async function submit() {
    if (id <= 0 || !endpoint) return;
    setBusy(true);
    try {
      const res = await apiPost<unknown>(endpoint, { ID: id, TransactionID: id });
      if (!res.ok) {
        // The provider's own words are the useful part — a refusal, "transaction not found",
        // "please try after 10 minutes" — so it is shown in the dialog, not flashed in a toast.
        setResult(res.message || 'The provider did not answer this enquiry.');
        toast.error(res.message || 'Status check failed.');
        return;
      }
      setResult(res.message || 'The provider has been asked. The status below is now up to date.');
      toast.success(`Txn ${id}: status checked.`);
      onChecked();
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Status check failed.';
      setResult(message);
      toast.error(message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal open={row !== null} onClose={onClose} title={`Status check · Txn ${id || ''}`} size="md">
      <div className="space-y-4">
        <p className="text-sm text-slate-600">
          This asks the provider what happened to this transaction and records their answer. If they
          report a final outcome, the transaction is settled accordingly — a failure refunds the
          wallet and reverses the commission.
        </p>

        <dl className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm">
          {[
            ['Transaction', id ? `#${id}` : '—'],
            ['Number', number],
            ['Amount', inr(amount)],
            ['Operator reference', reference],
            ['Current status', state.label],
          ].map(([k, v]) => (
            <div key={k} className="flex items-center justify-between gap-4 py-1">
              <dt className="text-slate-500">{k}</dt>
              <dd className="truncate font-medium text-slate-800">{v}</dd>
            </div>
          ))}
        </dl>

        {result && (
          <div className="rounded-lg border border-slate-200 bg-white p-3 text-sm text-slate-700">
            <p className="mb-1 text-xs font-semibold uppercase tracking-wider text-slate-400">Provider reply</p>
            {result}
          </div>
        )}

        <div className="flex justify-end gap-2">
          {id > 0 && (
            <Button type="button" variant="ghost" onClick={() => onViewLog(id)}>View log</Button>
          )}
          <Button type="button" variant="outline" onClick={onClose}>Close</Button>
          <Button type="button" loading={busy} disabled={id <= 0 || !endpoint} onClick={() => void submit()}>
            Submit
          </Button>
        </div>
      </div>
    </Modal>
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

  // Settling a transaction by hand is nearly always the answer to a complaint raised on it, so the
  // complaint is pulled up here — before the decision, not after. The backend closes any open
  // complaint from the same decision (refund ⇒ accepted, success ⇒ rejected); showing it first is
  // what stops two people working the same dispute from two screens.
  const { data: complaints = [], isFetching: complaintsLoading } = useQuery({
    queryKey: ['txn-complaints', id],
    queryFn: () => fetchList('ComplaintMaster/GetByTransaction', { TransactionMasterID: id }),
    enabled: row !== null && id > 0,
  });
  const openComplaint = complaints.find((c) => isOpenComplaint(String(getField(c, 'ComplaintStatus') ?? '')));
  const decidedComplaint = complaints.find((c) => !isOpenComplaint(String(getField(c, 'ComplaintStatus') ?? '')));

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
      toast.success(
        openComplaint
          ? `Txn ${id} marked ${status}. Its complaint was ${status === 'SUCCESS' ? 'rejected' : 'accepted'}.`
          : `Txn ${id} marked ${status}.`,
      );
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

        {/* Complaint on this transaction — checked before the decision is made, not after. */}
        {complaintsLoading ? (
          <Skeleton className="h-16 rounded-lg" />
        ) : openComplaint ? (
          <div className="rounded-lg border border-danger-200 bg-danger-50 p-3 text-sm text-danger-700">
            <p className="font-semibold">This transaction has an open complaint.</p>
            <p className="mt-1 text-danger-600">
              {String(getField(openComplaint, 'ComplaintType') || 'Complaint')} ·{' '}
              {String(getField(openComplaint, 'UserName') || '—')} ·{' '}
              {dateTime(String(getField(openComplaint, 'DateTime') ?? ''))}
            </p>
            {Boolean(getField(openComplaint, 'CustomerRemark')) && (
              <p className="mt-1 italic text-danger-600">"{String(getField(openComplaint, 'CustomerRemark'))}"</p>
            )}
            <p className="mt-2 text-xs text-danger-600">
              Settling here closes it too: marking the transaction <b>Failed</b> refunds the customer and records the
              complaint as <b>accepted</b>; marking it <b>Success</b> records the complaint as <b>rejected</b>.
            </p>
          </div>
        ) : decidedComplaint ? (
          <p className="rounded-lg bg-slate-50 p-3 text-sm text-slate-500">
            A complaint was raised on this transaction and already{' '}
            <span className="font-medium text-slate-700">{String(getField(decidedComplaint, 'ComplaintStatus') ?? '')}</span>.
          </p>
        ) : null}

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
