import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { subDays, startOfMonth } from 'date-fns';
import { Download, RefreshCw, Search as SearchIcon, SlidersHorizontal, Globe, Smartphone, Cpu } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { dateTime, apiDate } from '@/lib/format';
import { exportToCsv } from '@/lib/csv';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
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

async function fetchHistory(f: Filters): Promise<Row[]> {
  const res = await apiPost<unknown>('LoginHistory/GetAll', {
    UserID: f.userId === '' ? 0 : f.userId,
    Through: f.through,
    FromDate: f.fromDate,
    ToDate: f.toDate,
    PageNumber: 1,
    PageSize: PAGE_SIZE,
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

  const { data: users = [] } = useQuery({ queryKey: ['users-select'], queryFn: fetchUsers, staleTime: 5 * 60_000 });

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['login-history', applied],
    queryFn: () => fetchHistory(applied),
  });

  // UserID → display name map, for both the dropdown and the table.
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

  const userName = (id: number) => userMap.get(id)?.name ?? `User ${id}`;

  const runSearch = () => setApplied(draft);
  const today = apiDate(new Date());
  const setRange = (fromDate: string) => {
    const next = { ...draft, fromDate, toDate: today };
    setDraft(next);
    setApplied(next);
  };
  const isRange = (fromDate: string) => applied.fromDate === fromDate && applied.toDate === today;

  // CSV export with the joined user name resolved.
  const exportRows = rows.map((r) => {
    const uid = Number(getField(r, 'UserID') ?? 0);
    return {
      User: userName(uid),
      UserID: uid,
      Through: String(getField(r, 'Through') ?? ''),
      IPAddress: String(getField(r, 'IPAddress') ?? ''),
      Message: String(getField(r, 'LoginMessage') ?? ''),
      Date: dateTime(String(getField(r, 'Date') ?? '')),
    };
  });

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <>
            <span className="hidden text-sm text-slate-400 sm:inline">{rows.length} rows</span>
            <Button variant="outline" size="sm" onClick={() => exportToCsv(exportRows, def.title)} disabled={rows.length === 0}>
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
          <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2 text-sm font-medium text-slate-600">
              <SlidersHorizontal className="size-4 text-brand-600" />
              Filters
            </div>
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
          </div>

          <form onSubmit={(e) => { e.preventDefault(); runSearch(); }} className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
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
            <div className="flex items-end lg:col-start-4">
              <Button type="submit" loading={isFetching} className="w-full">
                <SearchIcon className="size-4" />
                Search
              </Button>
            </div>
          </form>
        </CardBody>
      </Card>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3 text-left">#</th>
                <th className="px-4 py-3 text-left">User</th>
                <th className="px-4 py-3 text-left">Channel</th>
                <th className="px-4 py-3 text-left">IP Address</th>
                <th className="px-4 py-3 text-left">Message</th>
                <th className="px-4 py-3 text-left">Date &amp; Time</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 8 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 6 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[120px]" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={6}>
                    <EmptyState title="No login history" description="No sign-ins for this range. Widen the dates or clear the user filter." />
                  </td>
                </tr>
              ) : (
                rows.map((r, i) => {
                  const uid = Number(getField(r, 'UserID') ?? 0);
                  const info = userMap.get(uid);
                  const through = String(getField(r, 'Through') ?? '');
                  return (
                    <tr key={String(getField(r, 'LoginHistoryID') ?? i)} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      <td className="px-3 py-3 tabular-nums text-slate-400">{i + 1}</td>
                      <td className="px-4 py-3">
                        <div className="font-medium text-slate-700">{info?.name ?? `User ${uid}`}</div>
                        {info?.sub && <div className="text-xs text-slate-400">{info.sub}</div>}
                      </td>
                      <td className="px-4 py-3">
                        <span className="inline-flex items-center gap-1.5 rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600">
                          {throughIcon(through)}
                          {through || '—'}
                        </span>
                      </td>
                      <td className="px-4 py-3 font-medium tabular-nums text-slate-600">{String(getField(r, 'IPAddress') || '—')}</td>
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
