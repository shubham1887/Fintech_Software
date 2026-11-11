import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { subDays, startOfMonth } from 'date-fns';
import { Globe, Smartphone, Cpu, MapPin } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { dateTime, apiDate } from '@/lib/format';
import { useCsvExport } from '@/lib/exportAll';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { ReportActions, ReportFilterBar } from '@/components/report';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

const PAGE_SIZE = 200;

interface Filters {
  fromDate: string;
  toDate: string;
  userId: number | '';
  through: string;
}

function defaultFilters(): Filters {
  return {
    fromDate: apiDate(subDays(new Date(), 30)),
    toDate: apiDate(new Date()),
    userId: '',
    through: '',
  };
}

/** Users/Select → dropdown options (id + person / firm / mobile for search). */
async function fetchUsers(): Promise<Row[]> {
  const res = await apiPost<unknown>('Users/Select', { PageNumber: 1, PageSize: 500, IsActive: true });
  if (!res.ok) return [];
  const d = res.data;
  if (Array.isArray(d)) return d as Row[];
  if (d && typeof d === 'object') {
    const arr = Object.values(d as Record<string, unknown>).find((v) => Array.isArray(v));
    if (arr) return arr as Row[];
  }
  return [];
}

/**
 * One window of the history. The endpoint pages by PageNumber/PageSize and sends no total, so the
 * export walks the windows until one comes back short rather than settling for the first.
 */
async function fetchHistory(f: Filters, skip = 0, take = PAGE_SIZE): Promise<Row[]> {
  const res = await apiPost<unknown>('LoginHistory/GetAll', {
    UserID: f.userId === '' ? 0 : f.userId,
    Through: f.through,
    FromDate: f.fromDate,
    ToDate: f.toDate,
    PageNumber: Math.floor(skip / Math.max(1, take)) + 1,
    PageSize: take,
  });
  // Empty set comes back as a business FAIL ("No record found") → clean empty state, not an error.
  if (!res.ok) return [];
  const d = res.data;
  if (Array.isArray(d)) return d as Row[];
  if (d && typeof d === 'object') {
    const arr = Object.values(d as Record<string, unknown>).find((v) => Array.isArray(v));
    if (arr) return arr as Row[];
  }
  return [];
}

const throughIcon = (t: string) => {
  const v = t.toLowerCase();
  if (v.includes('android') || v.includes('app') || v.includes('mobile')) return <Smartphone className="size-3.5" />;
  if (v.includes('api')) return <Cpu className="size-3.5" />;
  return <Globe className="size-3.5" />;
};

/**
 * Login History — successful sign-ins recorded at login (LoginHistory/GetAll), with
 * From / To date, a searchable user picker and a channel (Web / App / API) filter.
 * The user name is joined client-side from Users/Select since the history row carries
 * only a UserID.
 */
export function LoginHistoryScreen({ def }: CustomScreenProps) {
  const [draft, setDraft] = useState<Filters>(defaultFilters);
  const [applied, setApplied] = useState<Filters>(draft);
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { exporting, runExport } = useCsvExport();

  const { data: users = [] } = useQuery({ queryKey: ['users-select'], queryFn: fetchUsers, staleTime: 5 * 60_000 });

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['login-history', applied],
    queryFn: () => fetchHistory(applied),
  });

  // UserID → display name map. Only for the FILTER dropdown now: the table reads the name off the
  // row, because the API resolves it (see LoginHistoryService). Joining it here meant a sign-in by
  // anyone deactivated, deleted or past the 500-row cap on Users/Select read as "User 42", and a
  // retailer — who cannot call Users/Select at all — saw that for every row including their own.
  const userMap = useMemo(() => {
    const m = new Map<number, { name: string; sub: string }>();
    for (const u of users) {
      const id = Number(getField(u, 'UserMasterID') ?? 0);
      if (!id) continue;
      const name = String(getField(u, 'PersonName') || getField(u, 'FirmName') || `User ${id}`);
      const sub = [getField(u, 'FirmName'), getField(u, 'MobileNumber')].filter(Boolean).join(' · ');
      m.set(id, { name, sub });
    }
    return m;
  }, [users]);

  /** The person on a history row, from the row itself; the dropdown map is only a fallback. */
  const rowUser = (r: Row) => {
    const uid = Number(getField(r, 'UserID') ?? 0);
    const fromRow = String(getField(r, 'UserName') ?? '').trim();
    const firm = String(getField(r, 'FirmName') ?? '').trim();
    const mobile = String(getField(r, 'MobileNumber') ?? '').trim();
    const code = String(getField(r, 'UserCode') ?? '').trim();
    const role = String(getField(r, 'UserType') ?? '').trim();
    if (fromRow || firm) {
      return {
        name: fromRow || firm,
        sub: [code, firm && firm !== fromRow ? firm : '', mobile, role].filter(Boolean).join(' · '),
      };
    }
    const fallback = userMap.get(uid);
    return { name: fallback?.name ?? `User ${uid}`, sub: fallback?.sub ?? '' };
  };

  /** "19.0760, 72.8777" with a map link, or a dash. */
  const coords = (r: Row) => {
    const lat = String(getField(r, 'Latitude') ?? '').trim();
    const lng = String(getField(r, 'Longitude') ?? '').trim();
    return lat && lng ? { lat, lng } : null;
  };

  const userOptions = useMemo<AutocompleteOption[]>(
    () =>
      users
        .map((u): AutocompleteOption | null => {
          const id = Number(getField(u, 'UserMasterID') ?? 0);
          const info = userMap.get(id);
          return id && info ? { value: id, label: info.name, sublabel: info.sub } : null;
        })
        .filter((o): o is AutocompleteOption => o !== null),
    [users, userMap],
  );


  const runSearch = () => setApplied(draft);
  const resetFilters = () => { const d = defaultFilters(); setDraft(d); setApplied(d); };
  const today = apiDate(new Date());
  const setRange = (fromDate: string) => {
    const next = { ...draft, fromDate, toDate: today };
    setDraft(next);
    setApplied(next);
  };
  const isRange = (fromDate: string) => applied.fromDate === fromDate && applied.toDate === today;

  // CSV export — the same columns the table shows, in the same order. Applied per row so the
  // export can map the rows it fetches for itself, not just the ones the table is holding.
  const exportRow = (r: Row) => {
    const u = rowUser(r);
    return {
      User: u.name,
      UserCode: String(getField(r, 'UserCode') ?? ''),
      FirmName: String(getField(r, 'FirmName') ?? ''),
      MobileNumber: String(getField(r, 'MobileNumber') ?? ''),
      Role: String(getField(r, 'UserType') ?? ''),
      UserID: Number(getField(r, 'UserID') ?? 0),
      Through: String(getField(r, 'Through') ?? ''),
      IPAddress: String(getField(r, 'IPAddress') ?? ''),
      Latitude: String(getField(r, 'Latitude') ?? ''),
      Longitude: String(getField(r, 'Longitude') ?? ''),
      Location: String(getField(r, 'Location') ?? ''),
      Device: String(getField(r, 'SimCardDetails') ?? ''),
      Message: String(getField(r, 'LoginMessage') ?? ''),
      Date: dateTime(String(getField(r, 'Date') ?? '')),
    };
  };

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <ReportActions
            total={rows.length}
            loading={isFetching}
            onRefresh={() => refetch()}
            exportLoading={exporting}
            // Every sign-in the filters match, page after page — not the window on screen.
            onExport={() => runExport({
              filename: def.title,
              onScreen: rows,
              map: exportRow,
              fetchAll: async (skip, take) => ({ rows: await fetchHistory(applied, skip, take), count: 0 }),
            })}
            exportDisabled={rows.length === 0}
          />
        }
      />

      <ReportFilterBar
        onSearch={runSearch}
        onReset={resetFilters}
        loading={isFetching}
        extra={
          <div className="flex flex-wrap gap-1.5">
            {([
              ['Today', apiDate(new Date())],
              ['7 days', apiDate(subDays(new Date(), 7))],
              ['30 days', apiDate(subDays(new Date(), 30))],
              ['This month', apiDate(startOfMonth(new Date()))],
            ] as const).map(([label, fromDate]) => (
              <button
                key={label}
                type="button"
                onClick={() => setRange(fromDate)}
                className={cn(
                  'rounded-full px-3 py-1 text-xs font-medium ring-1 ring-inset transition-colors',
                  isRange(fromDate)
                    ? 'bg-brand-50 text-brand-700 ring-brand-500/25'
                    : 'text-slate-500 ring-slate-200 hover:bg-slate-50',
                )}
              >
                {label}
              </button>
            ))}
          </div>
        }
      >
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
            <Autocomplete
              label="User"
              placeholder="All users — type to search"
              options={userOptions}
              value={draft.userId}
              onChange={(v) => upd({ userId: v === '' ? '' : Number(v) })}
            />
            <Select label="Channel" value={draft.through} onChange={(e) => upd({ through: e.target.value })}>
              <option value="">All</option>
              <option value="Web">Web</option>
              <option value="Android">App / Android</option>
              <option value="API">API</option>
            </Select>
      </ReportFilterBar>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3 text-left">#</th>
                <th className="px-4 py-3 text-left">User</th>
                <th className="px-4 py-3 text-left">Channel</th>
                <th className="px-4 py-3 text-left">IP Address</th>
                <th className="px-4 py-3 text-left">Latitude</th>
                <th className="px-4 py-3 text-left">Longitude</th>
                <th className="px-4 py-3 text-left">Location</th>
                <th className="px-4 py-3 text-left">Message</th>
                <th className="px-4 py-3 text-left">Date &amp; Time</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 8 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 9 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[120px]" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={9}>
                    <EmptyState title="No login history" description="No sign-ins for this range. Widen the dates or clear the user filter." />
                  </td>
                </tr>
              ) : (
                rows.map((r, i) => {
                  const info = rowUser(r);
                  const through = String(getField(r, 'Through') ?? '');
                  const pos = coords(r);
                  const place = String(getField(r, 'Location') ?? '').trim();
                  return (
                    <tr key={String(getField(r, 'LoginHistoryID') ?? i)} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      <td className="px-3 py-3 tabular-nums text-slate-400">{i + 1}</td>
                      <td className="px-4 py-3">
                        <div className="font-medium text-slate-700">{info.name}</div>
                        {info.sub && <div className="text-xs text-slate-400">{info.sub}</div>}
                      </td>
                      <td className="px-4 py-3">
                        <span className="inline-flex items-center gap-1.5 rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600">
                          {throughIcon(through)}
                          {through || '—'}
                        </span>
                      </td>
                      <td className="px-4 py-3 font-medium tabular-nums text-slate-600">{String(getField(r, 'IPAddress') || '—')}</td>
                      <td className="px-4 py-3 tabular-nums text-slate-600">{pos?.lat ?? '—'}</td>
                      <td className="px-4 py-3 tabular-nums text-slate-600">{pos?.lng ?? '—'}</td>
                      <td className="px-4 py-3 text-slate-600">
                        {/* A pair of coordinates in a table is not a place until somebody can open
                            it. The link is the only thing that turns this column into an answer. */}
                        {pos ? (
                          <a
                            href={`https://www.google.com/maps/search/?api=1&query=${pos.lat},${pos.lng}`}
                            target="_blank"
                            rel="noreferrer noopener"
                            className="inline-flex items-center gap-1 font-medium text-brand-600 hover:underline"
                          >
                            <MapPin className="size-3.5" />
                            {place && !place.includes(',') ? place : 'View on map'}
                          </a>
                        ) : (
                          place || '—'
                        )}
                      </td>
                      <td className="px-4 py-3 text-slate-600">{String(getField(r, 'LoginMessage') || '—')}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{dateTime(String(getField(r, 'Date') ?? ''))}</td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
