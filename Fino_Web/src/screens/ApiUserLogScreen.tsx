import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { dateTime, apiDate, inr } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { PageHeader } from '@/components/ui/PageHeader';
import { ReportActions, ReportFilterBar, ReportPager, operatorsForService } from '@/components/report';
import { Input } from '@/components/ui/Input';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { StatusBadge } from '@/components/ui/Badge';
import { LogTable, type LogTableColumn } from '@/components/ui/LogTable';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');
const PAGE = 25;
const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
// API partners only (UserTypeID 5) — the log is theirs, nobody else calls the developer API.
const PARTNER_PAGE = { PageNumber: 1, PageSize: 1000, TakeCount: 1000, SkipCount: 0, UserTypeID: 5 };

interface Filters {
  fromDate: string;
  toDate: string;
  userId: string | number;
  serviceId: string;
  operatorId: string;
  search: string;
}

/** Opens on today: a partner log is read as "what did they send us today", not as history. */
const defaults = (): Filters => ({
  fromDate: apiDate(new Date()),
  toDate: apiDate(new Date()),
  userId: '',
  serviceId: '0',
  operatorId: '0',
  search: '',
});

interface LogPage {
  Count: number;
  list: Row[];
}

async function fetchLogs(f: Filters, page: number): Promise<LogPage> {
  const res = await apiPost<LogPage>('APIUserInitialRequestResponseLog/Select', {
    FromDate: f.fromDate,
    ToDate: f.toDate,
    UserID: Number(f.userId || 0),
    ServiceID: Number(f.serviceId || 0),
    OperatorID: Number(f.operatorId || 0),
    Search: f.search.trim(),
    SkipCount: page * PAGE,
    TakeCount: PAGE,
  });
  if (!res.ok || !res.data) return { Count: 0, list: [] };
  const d = res.data as unknown as Row;
  const list = getField(d, 'list', 'List', 'data');
  return { Count: Number(getField(d, 'Count') ?? 0), list: Array.isArray(list) ? (list as Row[]) : [] };
}

/**
 * API Partner Logs — what each integrator sent to the developer API and what we answered.
 *
 * Replaces a raw dump of the APIUserInitialRequestResponseLog table that showed a log id and a
 * user id and could not be filtered. The server resolves the partner's name, the service and the
 * operator from the transaction the call created, and the filters are the ones a support desk
 * actually uses: a date (today by default), the partner, the service, the operator, and a free
 * text box over the consumer number, the partner's own request id and the payloads.
 */
export function ApiUserLogScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const [draft, setDraft] = useState<Filters>(defaults);
  const [applied, setApplied] = useState<Filters>(draft);
  const [page, setPage] = useState(0);
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: services = [] } = useQuery({
    queryKey: ['svc-opts'], staleTime: 300_000, queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });
  const { data: allOperators = [] } = useQuery({
    queryKey: ['op-opts'], staleTime: 300_000, queryFn: () => fetchList('Operator/GetAll', LIST_PAGE),
  });
  const { data: partners = [], isLoading: partnersLoading } = useQuery({
    queryKey: ['api-partner-opts'], staleTime: 300_000, queryFn: () => fetchList('Users/Select', PARTNER_PAGE),
  });

  const { data, isFetching, refetch } = useQuery({
    queryKey: ['api-user-log', applied, page],
    queryFn: () => fetchLogs(applied, page),
    placeholderData: (prev) => prev,
  });
  const rows = data?.list ?? [];
  const total = data?.Count ?? 0;

  const operatorOptions = useMemo(() => operatorsForService(allOperators, draft.serviceId), [allOperators, draft.serviceId]);
  const partnerOptions: AutocompleteOption[] = useMemo(
    () => partners.map((u) => ({
      value: String(getField(u, 'UserMasterID', 'UserID') ?? ''),
      label: String(getField(u, 'FirmName', 'PersonName', 'Name') ?? '—'),
      sublabel: String(getField(u, 'MobileNumber', 'Mobile') ?? ''),
    })),
    [partners],
  );

  const runSearch = () => { setPage(0); setApplied(draft); };
  const reset = () => { const d = defaults(); setDraft(d); setApplied(d); setPage(0); };

  const exportAll = async () => {
    // Every row the filters match, not the page on screen.
    const all = await fetchLogs({ ...applied }, 0);
    const pages = Math.ceil(all.Count / PAGE);
    const collected = [...all.list];
    for (let p = 1; p < pages; p++) collected.push(...(await fetchLogs(applied, p)).list);
    downloadCsv(collected.map((r) => ({
      DateTime: str(getField(r, 'DateTime')),
      Partner: str(getField(r, 'UserName')),
      PartnerMobile: str(getField(r, 'MobileNumber')),
      Service: str(getField(r, 'ServiceName')),
      Operator: str(getField(r, 'OperatorName')),
      ConsumerNumber: str(getField(r, 'ConsumerNumber')),
      PartnerRequestID: str(getField(r, 'APIUserRequestID')),
      TransactionID: str(getField(r, 'TransactionMasterID')),
      Amount: str(getField(r, 'Amount')),
      Status: str(getField(r, 'Status')),
      Request: str(getField(r, 'Request')),
      Response: str(getField(r, 'Response')),
    })), def.title);
  };

  const columns: LogTableColumn<Row>[] = [
    { label: 'Date / Time', render: (r) => <span className="whitespace-nowrap text-slate-500">{dateTime(str(getField(r, 'DateTime')))}</span> },
    {
      label: 'Partner',
      render: (r) => (
        <div>
          <div className="font-medium text-slate-800">{str(getField(r, 'UserName')) || <Dash />}</div>
          <div className="text-xs text-slate-400">{str(getField(r, 'MobileNumber'))}</div>
        </div>
      ),
    },
    { label: 'Service', render: (r) => str(getField(r, 'ServiceName')) || <Dash /> },
    { label: 'Operator', render: (r) => str(getField(r, 'OperatorName')) || <Dash /> },
    { label: 'Number', render: (r) => <span className="tabular-nums">{str(getField(r, 'ConsumerNumber')) || <Dash />}</span> },
    { label: 'Partner ref', render: (r) => <span className="font-mono text-xs">{str(getField(r, 'APIUserRequestID')) || <Dash />}</span> },
    {
      label: 'Txn #',
      className: 'text-right',
      render: (r) => { const id = Number(getField(r, 'TransactionMasterID') ?? 0); return id > 0 ? <span className="tabular-nums">{id}</span> : <Dash />; },
    },
    {
      label: 'Amount',
      className: 'text-right',
      render: (r) => { const a = Number(getField(r, 'Amount') ?? 0); return a > 0 ? <span className="tabular-nums">{inr(a)}</span> : <Dash />; },
    },
    { label: 'Status', render: (r) => { const s = str(getField(r, 'Status')); return s ? <StatusBadge status={s} /> : <Dash />; } },
  ];

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <ReportActions
            total={total}
            loading={isFetching}
            onRefresh={() => refetch()}
            onExport={() => void exportAll()}
            exportDisabled={total === 0}
          />
        }
      />

      <ReportFilterBar onSearch={runSearch} onReset={reset} loading={isFetching}>
        <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
        <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
        <Autocomplete
          label="API partner"
          placeholder="All partners"
          options={partnerOptions}
          value={draft.userId}
          loading={partnersLoading}
          onChange={(v) => upd({ userId: v })}
        />
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
        <Input
          label="Search"
          placeholder="Number, partner ref, txn id or payload text…"
          value={draft.search}
          onChange={(e) => upd({ search: e.target.value })}
        />
      </ReportFilterBar>

      <LogTable
        columns={columns}
        payloads={[
          { label: 'Partner request', value: (r) => str(getField(r, 'Request')) },
          { label: 'Our response', value: (r) => str(getField(r, 'Response')) },
        ]}
        rows={rows}
        loading={isFetching && rows.length === 0}
        rowKey={(r, i) => String(getField(r, 'APIUserInitialRequestResponseLogID') ?? `${page}-${i}`)}
        startIndex={page * PAGE + 1}
        emptyTitle="No API calls"
        emptyDescription="No partner called the developer API in this period, or nothing matches these filters."
      />

      {total > 0 && <ReportPager page={page} pageSize={PAGE} total={total} onPage={setPage} />}
    </>
  );
}

function Dash() {
  return <span className="text-slate-300">—</span>;
}
