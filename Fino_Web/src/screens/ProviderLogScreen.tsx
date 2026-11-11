import { useMemo, useState } from 'react';
import { useQuery, keepPreviousData } from '@tanstack/react-query';

import { subDays } from 'date-fns';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { dateTime, apiDate } from '@/lib/format';
import { useCsvExport } from '@/lib/exportAll';
import { PageHeader } from '@/components/ui/PageHeader';
import { ReportActions, ReportFilterBar, ReportPager } from '@/components/report';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { LogTable, type LogTableColumn } from '@/components/ui/LogTable';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');
const PAGE = 25;
const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
const USER_PAGE = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 };

interface Filters {
  fromDate: string;
  toDate: string;
  providerId: string;
  serviceId: string;
  userId: string;
  through: string;
  transactionId: string;
  search: string;
}

const defaults = (): Filters => ({
  fromDate: apiDate(subDays(new Date(), 7)),
  toDate: apiDate(new Date()),
  providerId: '0',
  serviceId: '0',
  userId: '',
  through: '',
  transactionId: '',
  search: '',
});

interface LogPage {
  rows: Row[];
  count: number;
  throughOptions: string[];
}

async function fetchLogs(f: Filters, skip: number, take: number, forExport = false): Promise<LogPage> {
  const res = await apiPost<Record<string, unknown>>('ProviderRequestResponseLog/Search', {
    FromDate: f.fromDate,
    ToDate: f.toDate,
    ProviderID: Number(f.providerId || 0),
    ServiceID: Number(f.serviceId || 0),
    UserID: Number(f.userId || 0),
    TransactionID: Number(f.transactionId || 0),
    Through: f.through,
    Search: f.search,
    SkipCount: skip,
    TakeCount: take,
    IsExcelDownload: forExport,
  });
  if (!res.ok || !res.data) return { rows: [], count: 0, throughOptions: [] };
  const list = getField(res.data, 'list', 'List');
  const opts = getField(res.data, 'ThroughOptions');
  return {
    rows: Array.isArray(list) ? (list as Row[]) : [],
    count: Number(getField(res.data, 'Count', 'count') ?? 0),
    throughOptions: Array.isArray(opts) ? (opts as string[]) : [],
  };
}

/**
 * Provider Log — every call we made to a provider and what it replied.
 *
 * Two things were wrong with it. The payloads — the whole point of the screen — lived behind a View
 * button, one dialog per row, so you could never scan or compare calls; they are now in the table
 * itself (see `LogTable`). And the filters ran in the browser over the entire log table, which was
 * downloaded in full on every visit: a log that grows all day made the screen slower every day, and
 * the filters only ever narrowed the slice that had already been fetched. Filtering, searching and
 * paging now happen server-side, so the page loads the 25 rows it shows and nothing else.
 */
export function ProviderLogScreen({ def }: CustomScreenProps) {
  const [draft, setDraft] = useState<Filters>(defaults);
  const [applied, setApplied] = useState<Filters>(draft);
  const [page, setPage] = useState(0);
  const { exporting, runExport } = useCsvExport();

  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data, isFetching, refetch } = useQuery({
    queryKey: ['provider-log', applied, page],
    queryFn: () => fetchLogs(applied, page * PAGE, PAGE),
    placeholderData: keepPreviousData,
  });
  const rows = data?.rows ?? [];
  const total = data?.count ?? 0;
  const throughOptions = data?.throughOptions ?? [];

  const { data: providers = [] } = useQuery({
    queryKey: ['prov-opts'], staleTime: 300_000,
    queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAGE),
  });
  const { data: services = [] } = useQuery({
    queryKey: ['svc-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });
  const { data: users = [] } = useQuery({
    queryKey: ['user-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', USER_PAGE),
  });

  const serviceOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '0', label: 'All services' },
    ...services.map((s, i) => ({ value: String(getField(s, 'ServiceID') ?? i), label: str(getField(s, 'ServiceName', 'Name')) })),
  ], [services]);
  const userOpts = useMemo<AutocompleteOption[]>(() => [
    { value: '', label: 'All entities' },
    ...users.map((u, i) => ({
      value: String(getField(u, 'UserMasterID', 'UserID') ?? i),
      label: str(getField(u, 'FirmName', 'PersonName', 'Name')),
      sublabel: str(getField(u, 'MobileNumber', 'Mobile')) || undefined,
    })),
  ], [users]);

  const runSearch = () => { setPage(0); setApplied(draft); };
  const reset = () => { const d = defaults(); setDraft(d); setApplied(d); setPage(0); };


  const columns: LogTableColumn<Row>[] = [
    { label: 'Date / Time', render: (r) => <span className="whitespace-nowrap text-slate-500">{dateTime(str(getField(r, 'DateTime')))}</span> },
    { label: 'Provider', render: (r) => <span className="font-medium text-slate-800">{str(getField(r, 'ProviderName')) || <Dash />}</span> },
    { label: 'Entity', render: (r) => str(getField(r, 'UserName')) || <Dash /> },
    { label: 'Service', render: (r) => str(getField(r, 'ServiceName')) || <Dash /> },
    { label: 'Call type', render: (r) => str(getField(r, 'Through')) || <Dash /> },
    {
      label: 'Txn',
      render: (r) => {
        const id = Number(getField(r, 'TransactionID') ?? 0);
        return id > 0 ? <span className="tabular-nums text-slate-600">{id}</span> : <Dash />;
      },
    },
    { label: 'IP', render: (r) => <span className="tabular-nums text-slate-500">{str(getField(r, 'IPAddress')) || <Dash />}</span> },
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
            exportLoading={exporting}
            // The whole filtered log, not the 25 rows on screen — the payloads are the point of it.
            onExport={() => runExport({
              filename: 'Provider Log',
              onScreen: rows,
              fetchAll: (skip, take) => fetchLogs(applied, skip, take, true),
              batch: 500,
            })}
            exportDisabled={total === 0}
          />
        }
      />

      <ReportFilterBar onSearch={runSearch} onReset={reset} loading={isFetching}>
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
            <Select label="Provider" value={draft.providerId} onChange={(e) => upd({ providerId: e.target.value })}>
              <option value="0">All providers</option>
              {providers.map((p, i) => {
                const id = String(getField(p, 'ProviderID') ?? i);
                return <option key={id} value={id}>{str(getField(p, 'ProviderName', 'Name')) || id}</option>;
              })}
            </Select>
            <Autocomplete label="Service" options={serviceOpts} value={draft.serviceId} onChange={(v) => upd({ serviceId: String(v || '0') })} />
            <Autocomplete label="Entity / user" options={userOpts} value={draft.userId} onChange={(v) => upd({ userId: String(v) })} />
            <Select label="Call type" value={draft.through} onChange={(e) => upd({ through: e.target.value })}>
              <option value="">All call types</option>
              {throughOptions.map((t) => <option key={t} value={t}>{t}</option>)}
            </Select>
            <Input
              label="Transaction ID"
              placeholder="Exact id"
              inputMode="numeric"
              value={draft.transactionId}
              onChange={(e) => upd({ transactionId: e.target.value.replace(/\D/g, '') })}
            />
            <Input
              label="Search request / response"
              placeholder="Reference no., error text, IP…"
              value={draft.search}
              onChange={(e) => upd({ search: e.target.value })}
            />
      </ReportFilterBar>

      <LogTable
        columns={columns}
        payloads={[
          { label: 'Request', value: (r) => str(getField(r, 'Request')) },
          { label: 'Response', value: (r) => str(getField(r, 'Response')) },
        ]}
        rows={rows}
        loading={isFetching && rows.length === 0}
        rowKey={(r, i) => String(getField(r, 'ProviderRequestResponseLogID') ?? `${page}-${i}`)}
        startIndex={page * PAGE + 1}
        emptyTitle="No provider logs"
        emptyDescription="Nothing matches these filters, or no provider calls have been logged in this period."
      />

      {total > 0 && <ReportPager page={page} pageSize={PAGE} total={total} loading={isFetching} onPage={setPage} />}
    </>
  );
}

function Dash() {
  return <span className="text-slate-300">—</span>;
}
