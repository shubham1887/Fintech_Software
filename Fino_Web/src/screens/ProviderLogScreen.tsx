import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { RefreshCw, Eye } from 'lucide-react';
import { subDays } from 'date-fns';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { dateTime, apiDate } from '@/lib/format';
import { useClientSearch } from '@/lib/useClientSearch';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { TableToolbar } from '@/components/ui/TableToolbar';
import { Modal } from '@/components/ui/Modal';
import { exportToCsv } from '@/lib/csv';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');
const PAGE = 25;

/** Distinct, sorted, non-empty values of a field across the rows — for the filter dropdowns. */
function distinct(rows: Row[], key: string): string[] {
  const set = new Set<string>();
  for (const r of rows) {
    const v = str(getField(r, key)).trim();
    if (v) set.add(v);
  }
  return Array.from(set).sort((a, b) => a.localeCompare(b));
}

/**
 * Provider Log — provider request/response records rendered as a clean table (user, provider,
 * service, through, IP, time). Raw ids are hidden and the long Request / Response payloads are
 * moved into a details dialog opened from the row's View button. Filter by Provider, Service type
 * and date range.
 */
export function ProviderLogScreen({ def }: CustomScreenProps) {
  const [page, setPage] = useState(0);
  const [provider, setProvider] = useState('');
  const [service, setService] = useState('');
  const [fromDate, setFromDate] = useState(apiDate(subDays(new Date(), 7)));
  const [toDate, setToDate] = useState(apiDate(new Date()));
  const [detail, setDetail] = useState<Row | null>(null);

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['provider-log'],
    queryFn: () => fetchList('ProviderRequestResponseLog/GetAll', {}),
  });

  const providers = useMemo(() => distinct(rows, 'ProviderName'), [rows]);
  const services = useMemo(() => distinct(rows, 'ServiceName'), [rows]);

  // Provider + Service type + date-range filtering happens client-side over the loaded log.
  const scoped = useMemo(() => {
    const from = new Date(fromDate + 'T00:00:00').getTime();
    const to = new Date(toDate + 'T23:59:59').getTime();
    return rows.filter((r) => {
      if (provider && str(getField(r, 'ProviderName')) !== provider) return false;
      if (service && str(getField(r, 'ServiceName')) !== service) return false;
      const raw = getField(r, 'DateTime');
      const t = raw ? new Date(String(raw)).getTime() : NaN;
      return Number.isNaN(t) ? true : t >= from && t <= to;
    });
  }, [rows, provider, service, fromDate, toDate]);

  const { query, setQuery, filtered } = useClientSearch(scoped);

  const paged = useMemo(() => filtered.slice(page * PAGE, page * PAGE + PAGE), [filtered, page]);
  const total = filtered.length;
  const to = Math.min((page + 1) * PAGE, total);

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
      {/* Filters — Provider, Service type, date range */}
      <div className="mb-4 flex flex-wrap items-end gap-2">
        <div className="w-52">
          <Select label="Provider" value={provider} onChange={(e) => { setProvider(e.target.value); setPage(0); }}>
            <option value="">All providers</option>
            {providers.map((p) => <option key={p} value={p}>{p}</option>)}
          </Select>
        </div>
        <div className="w-52">
          <Autocomplete
            label="Service type"
            value={service}
            options={[
              { value: '', label: 'All services' },
              ...services.map((s) => ({ value: s, label: s })),
            ]}
            onChange={(v) => { setService(String(v)); setPage(0); }}
          />
        </div>
        <div className="w-40">
          <Input label="From" type="date" max={toDate} value={fromDate} onChange={(e) => { setFromDate(e.target.value); setPage(0); }} />
        </div>
        <div className="w-40">
          <Input label="To" type="date" value={toDate} onChange={(e) => { setToDate(e.target.value); setPage(0); }} />
        </div>
        {(provider || service) && (
          <Button variant="outline" size="sm" onClick={() => { setProvider(''); setService(''); setPage(0); }}>Clear</Button>
        )}
      </div>

      <TableToolbar query={query} onQuery={(v) => { setQuery(v); setPage(0); }} count={total} onExport={() => exportToCsv(filtered, 'Provider Log')} />

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="w-12 px-4 py-3 text-right">#</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Provider</th>
                <th className="px-4 py-3">Service</th>
                <th className="px-4 py-3">Through</th>
                <th className="px-4 py-3">IP</th>
                <th className="px-4 py-3">Date / Time</th>
                <th className="px-4 py-3 text-right">Details</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 8 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 8 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[120px]" /></td>
                    ))}
                  </tr>
                ))
              ) : paged.length === 0 ? (
                <tr>
                  <td colSpan={8}>
                    <EmptyState title="No provider logs" description="Nothing matches your search, or no provider calls have been logged yet." />
                  </td>
                </tr>
              ) : (
                paged.map((r, i) => {
                  const serial = page * PAGE + i + 1;
                  return (
                    <tr key={i} className="border-b border-slate-100 transition-colors last:border-0 even:bg-slate-50/40 hover:bg-brand-50/40">
                      <td className="px-4 py-3 text-right tabular-nums text-slate-400">{serial}</td>
                      <td className="px-4 py-3 font-medium text-slate-800">{str(getField(r, 'UserName')) || <Dash />}</td>
                      <td className="px-4 py-3 font-medium text-slate-800">{str(getField(r, 'ProviderName')) || <Dash />}</td>
                      <td className="px-4 py-3 text-slate-600">{str(getField(r, 'ServiceName')) || <Dash />}</td>
                      <td className="px-4 py-3 text-slate-600">{str(getField(r, 'Through')) || <Dash />}</td>
                      <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-500">{str(getField(r, 'IPAddress')) || <Dash />}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-500">{dateTime(str(getField(r, 'DateTime')))}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-right">
                        <Button variant="ghost" size="sm" onClick={() => setDetail(r)}>
                          <Eye className="size-4" /> View
                        </Button>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
        {!isFetching && total > 0 && (
          <div className="flex items-center justify-between border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
            <span>{page * PAGE + 1}–{to} of {total}</span>
            {total > PAGE && (
              <div className="flex gap-2">
                <Button variant="outline" size="sm" disabled={page === 0} onClick={() => setPage((p) => p - 1)}>Prev</Button>
                <Button variant="outline" size="sm" disabled={to >= total} onClick={() => setPage((p) => p + 1)}>Next</Button>
              </div>
            )}
          </div>
        )}
      </div>

      <Modal open={Boolean(detail)} onClose={() => setDetail(null)} title="Provider request / response" size="lg">
        {detail && (
          <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-sm">
              <Meta label="User" value={str(getField(detail, 'UserName'))} />
              <Meta label="Provider" value={str(getField(detail, 'ProviderName'))} />
              <Meta label="Service" value={str(getField(detail, 'ServiceName'))} />
              <Meta label="Through" value={str(getField(detail, 'Through'))} />
              <Meta label="IP" value={str(getField(detail, 'IPAddress'))} />
              <Meta label="Date" value={dateTime(str(getField(detail, 'DateTime')))} />
            </div>
            <LogBlock label="Request Log" value={str(getField(detail, 'Request')) || '—'} />
            <LogBlock label="Response Log" value={str(getField(detail, 'Response')) || '—'} />
          </div>
        )}
      </Modal>
    </>
  );
}

function Dash() {
  return <span className="text-slate-300">—</span>;
}

function Meta({ label, value }: { label: string; value: string }) {
  if (!value) return null;
  return (
    <span>
      <span className="text-slate-400">{label}:</span> <span className="font-medium text-slate-800">{value}</span>
    </span>
  );
}

function LogBlock({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</p>
      <pre className="max-h-56 overflow-auto whitespace-pre-wrap break-all rounded-lg bg-slate-50 p-3 text-xs leading-relaxed text-slate-700">
        {value}
      </pre>
    </div>
  );
}
