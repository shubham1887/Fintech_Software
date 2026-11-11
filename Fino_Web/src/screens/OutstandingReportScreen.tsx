import { useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, Clock, Download, HandCoins, RefreshCw, Search as SearchIcon, Users, Wallet,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useCsvDownload } from '@/lib/exportAll';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { pickIcon } from '@/components/layout/iconMap';
import { OutstandingStatement } from './OutstandingStatement';
import type { CustomScreenProps } from './registry';

/** One account on the report, as CashCollection/OutstandingReport returns it. */
interface Row {
  UserID: number;
  UserName: string;
  FirmName: string;
  PersonName: string;
  MobileNumber: string;
  UserType: string;
  ParentID: number;
  ParentName: string;
  Outstanding: number;
  MaxOutstanding: number;
  AvailableCredit: number;
  LastMovementDate: string | null;
  Age0To30: number;
  Age31To60: number;
  Age61To90: number;
  Age90Plus: number;
  OldestUnpaidDate: string | null;
  OldestUnpaidDays: number;
  LastCollectionDate: string | null;
  UtilisationPercent: number;
  IsOverLimit: boolean;
}

interface Report {
  data: Row[];
  TotalOutstanding: number;
  TotalLimit: number;
  TotalAvailable: number;
  AccountsWithBalance: number;
  OverLimitCount: number;
  TotalAge0To30: number;
  TotalAge31To60: number;
  TotalAge61To90: number;
  TotalAge90Plus: number;
  MyOutstanding: number;
  MyLimit: number;
  MyAvailable: number;
  MyPayableTo: string;
  AsOn: string;
}

type Bucket = 'all' | '0' | '31' | '61' | '91';

const BUCKETS: { value: Bucket; label: string }[] = [
  { value: 'all', label: 'Any age' },
  { value: '0', label: '0–30 days' },
  { value: '31', label: '31–60 days' },
  { value: '61', label: '61–90 days' },
  { value: '91', label: 'Over 90 days' },
];

function day(v: string | null): string {
  if (!v) return '—';
  const d = new Date(v);
  return Number.isNaN(d.getTime()) ? '—' : d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}

/**
 * Outstanding Report — the credit position of the whole network on one page.
 *
 * This is the reading half of the Outstanding screen, and deliberately a different page. The
 * other one is a desk: set a limit, record a collection, chase someone. This one answers the
 * questions a report has to answer and nothing else — how much is out, how old it is, who is past
 * their limit, and who has not paid anything in months — and it exports.
 *
 * Two numbers that look alike and are not, so they are never added together:
 *   • What I OWE my upline — one figure, at the top, because for a retailer it is the only one
 *     that exists and for everyone else it is still their own position.
 *   • What my downline owes ME — the table.
 *
 * The ageing is computed on the server by matching collections against credits oldest-first, and
 * the four buckets always total the balance; see `CashCollectionMasterService.ApplyAgeing` for
 * why that matters. The client never re-derives them — a report whose footer disagrees with its
 * own rows is worse than no report.
 *
 * Scoping is the server's: admin and support see the whole network, everyone else their own
 * downline. The client asks for everything it is entitled to and no more.
 */
export function OutstandingReportScreen({ def }: CustomScreenProps) {
  const qc = useQueryClient();
  const downloadCsv = useCsvDownload();
  // Every role that can reach this screen is one that is OWED money (the distributor tier) or
  // reads the books (the accountant). The retailer and API-user tiers are kept off it on both
  // sides — the menu does not list it and the endpoint refuses them — because the report is a
  // downline view and they have no downline. So there is no empty-table case to draw here.

  const [search, setSearch] = useState('');
  const [applied, setApplied] = useState('');
  const [bucket, setBucket] = useState<Bucket>('all');
  const [overLimitOnly, setOverLimitOnly] = useState(false);
  const [viewing, setViewing] = useState<Row | null>(null);

  const { data, isLoading, isFetching, isError, error } = useQuery<Report>({
    queryKey: ['outstanding-report', applied],
    queryFn: async () => {
      const res = await apiPost<Report>('CashCollection/OutstandingReport', {
        UserID: 0, SearchText: applied, TakeCount: 0, SkipCount: 0, IsExcelDownload: false,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not load the outstanding report.');
      return res.data;
    },
    staleTime: 30_000,
  });

  const rows = useMemo(() => {
    let list = data?.data ?? [];
    if (overLimitOnly) list = list.filter((r) => r.IsOverLimit);
    if (bucket !== 'all') {
      const pick = (r: Row) =>
        bucket === '0' ? r.Age0To30 : bucket === '31' ? r.Age31To60 : bucket === '61' ? r.Age61To90 : r.Age90Plus;
      list = list.filter((r) => pick(r) > 0);
    }
    return list;
  }, [data, bucket, overLimitOnly]);

  // The visible set's own totals. Shown in the table footer, which is honest about being the
  // filtered view; the strip above keeps the server's totals for the whole scoped set.
  const shown = useMemo(() => ({
    outstanding: rows.reduce((s, r) => s + r.Outstanding, 0),
    a: rows.reduce((s, r) => s + r.Age0To30, 0),
    b: rows.reduce((s, r) => s + r.Age31To60, 0),
    c: rows.reduce((s, r) => s + r.Age61To90, 0),
    d: rows.reduce((s, r) => s + r.Age90Plus, 0),
  }), [rows]);

  const exportRows = () =>
    downloadCsv(
      rows.map((r) => ({
        Firm: r.FirmName,
        Contact: r.PersonName,
        Mobile: r.MobileNumber,
        Role: r.UserType,
        'Owed to': r.ParentName,
        Outstanding: r.Outstanding,
        'Credit limit': r.MaxOutstanding,
        Available: r.AvailableCredit,
        'Utilisation %': r.UtilisationPercent,
        'Over limit': r.IsOverLimit ? 'Yes' : 'No',
        '0-30 days': r.Age0To30,
        '31-60 days': r.Age31To60,
        '61-90 days': r.Age61To90,
        'Over 90 days': r.Age90Plus,
        'Oldest unpaid': day(r.OldestUnpaidDate),
        'Days outstanding': r.OldestUnpaidDays,
        'Last collection': day(r.LastCollectionDate),
        'Last movement': day(r.LastMovementDate),
      })),
      `outstanding-report-${new Date().toISOString().slice(0, 10)}`,
    );

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <div className="flex items-center gap-2">
            <Button variant="ghost" onClick={() => qc.invalidateQueries({ queryKey: ['outstanding-report'] })}>
              <RefreshCw className={cn('size-4', isFetching && 'animate-spin')} />
              Refresh
            </Button>
            <Button variant="secondary" onClick={exportRows} disabled={rows.length === 0}>
              <Download className="size-4" />
              Export
            </Button>
          </div>
        }
      />

      {/* What the reader themself owes. First, because it is the one figure on this page that is
          about them, and for a retailer it is the whole report. */}
      {(data?.MyOutstanding ?? 0) > 0 && (
        <Card className="mb-4 border-warning-200 bg-warning-50/50">
          <CardBody className="flex flex-wrap items-center justify-between gap-4 p-4">
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">My outstanding</p>
              <p className="mt-1 font-display text-2xl font-extrabold tabular-nums text-warning-700">
                {inr(data?.MyOutstanding ?? 0)}
              </p>
              <p className="mt-0.5 text-sm text-slate-600">
                {data?.MyPayableTo ? <>Payable to <span className="font-medium">{data.MyPayableTo}</span></> : 'Payable to your upline'}
              </p>
            </div>
            <div className="flex items-center gap-6 text-right">
              <div>
                <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">Credit limit</p>
                <p className="mt-1 font-semibold tabular-nums text-slate-800">{inr(data?.MyLimit ?? 0)}</p>
              </div>
              <div>
                <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">Available</p>
                <p className="mt-1 font-semibold tabular-nums text-slate-800">{inr(data?.MyAvailable ?? 0)}</p>
              </div>
            </div>
          </CardBody>
        </Card>
      )}

      <>
          <div className={cn(STAT_STRIP, 'grid-cols-2 lg:grid-cols-7')}>
            <StatTile label="Credit out" value={inr(data?.TotalOutstanding ?? 0)} icon={HandCoins} tone="text-warning-600" loading={isLoading} />
            <StatTile label="Accounts owing" value={String(data?.AccountsWithBalance ?? 0)} icon={Users} tone="text-brand-600" loading={isLoading} />
            <StatTile label="Over limit" value={String(data?.OverLimitCount ?? 0)} icon={AlertTriangle} tone="text-danger-600" loading={isLoading} />
            <StatTile label="0–30 days" value={inr(data?.TotalAge0To30 ?? 0)} icon={Clock} tone="text-slate-500" loading={isLoading} />
            <StatTile label="31–60 days" value={inr(data?.TotalAge31To60 ?? 0)} icon={Clock} tone="text-slate-500" loading={isLoading} />
            <StatTile label="61–90 days" value={inr(data?.TotalAge61To90 ?? 0)} icon={Clock} tone="text-warning-600" loading={isLoading} />
            <StatTile label="Over 90 days" value={inr(data?.TotalAge90Plus ?? 0)} icon={Clock} tone="text-danger-600" loading={isLoading} />
          </div>

          <Card>
            <CardBody className="flex flex-wrap items-end gap-3 border-b border-slate-100 p-4">
              <div className="min-w-[220px] flex-1">
                <label className="mb-1 block text-xs font-medium text-slate-500">Search</label>
                <div className="flex gap-2">
                  <Input
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    onKeyDown={(e) => { if (e.key === 'Enter') setApplied(search.trim()); }}
                    placeholder="Firm, contact, mobile or upline"
                  />
                  <Button variant="secondary" onClick={() => setApplied(search.trim())}>
                    <SearchIcon className="size-4" />
                  </Button>
                </div>
              </div>
              <div className="w-44">
                <label className="mb-1 block text-xs font-medium text-slate-500">Age</label>
                <Select value={bucket} onChange={(e) => setBucket(e.target.value as Bucket)}>
                  {BUCKETS.map((b) => <option key={b.value} value={b.value}>{b.label}</option>)}
                </Select>
              </div>
              <label className="flex h-9 cursor-pointer items-center gap-2 rounded-lg border border-slate-200 px-3 text-sm text-slate-700">
                <input type="checkbox" checked={overLimitOnly} onChange={(e) => setOverLimitOnly(e.target.checked)} />
                Over limit only
              </label>
              {data?.AsOn && (
                <span className="ml-auto text-xs text-slate-400">Aged as on {day(data.AsOn)}</span>
              )}
            </CardBody>

            <CardBody className="p-0">
              {isError ? (
                <div className="flex items-center gap-2 p-6 text-sm text-warning-700">
                  <AlertTriangle className="size-4 shrink-0" />
                  {(error as Error)?.message}
                </div>
              ) : isLoading ? (
                <div className="space-y-2 p-4">
                  {Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-10 rounded-lg" />)}
                </div>
              ) : rows.length === 0 ? (
                <EmptyState
                  icon={Wallet}
                  title="Nothing outstanding"
                  description={
                    applied || overLimitOnly || bucket !== 'all'
                      ? 'No account matches these filters.'
                      : 'No account in your network is carrying a balance or a credit limit.'
                  }
                />
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead className="bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-500">
                      <tr>
                        <th className="px-4 py-2.5 font-semibold">Account</th>
                        <th className="px-3 py-2.5 font-semibold">Owed to</th>
                        <th className="px-3 py-2.5 text-right font-semibold">Outstanding</th>
                        <th className="px-3 py-2.5 text-right font-semibold">Limit</th>
                        <th className="px-3 py-2.5 text-right font-semibold">Available</th>
                        <th className="px-3 py-2.5 text-right font-semibold">0–30</th>
                        <th className="px-3 py-2.5 text-right font-semibold">31–60</th>
                        <th className="px-3 py-2.5 text-right font-semibold">61–90</th>
                        <th className="px-3 py-2.5 text-right font-semibold">90+</th>
                        <th className="px-3 py-2.5 font-semibold">Oldest</th>
                        <th className="px-3 py-2.5 font-semibold">Last paid</th>
                        <th className="px-3 py-2.5" />
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                      {rows.map((r) => (
                        <tr key={r.UserID} className="hover:bg-slate-50/70">
                          <td className="px-4 py-2.5">
                            <div className="font-medium text-slate-800">{r.FirmName || r.UserName}</div>
                            <div className="text-xs text-slate-400">
                              {r.PersonName}{r.PersonName && r.MobileNumber ? ' · ' : ''}{r.MobileNumber}
                              {r.UserType ? ` · ${r.UserType}` : ''}
                            </div>
                          </td>
                          <td className="px-3 py-2.5 text-slate-600">{r.ParentName || '—'}</td>
                          <td className="px-3 py-2.5 text-right font-semibold tabular-nums text-slate-900">
                            {inr(r.Outstanding)}
                            {r.IsOverLimit && <Badge tone="danger" className="ml-2">Over limit</Badge>}
                          </td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.MaxOutstanding)}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.AvailableCredit)}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{r.Age0To30 ? inr(r.Age0To30) : '—'}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{r.Age31To60 ? inr(r.Age31To60) : '—'}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-warning-700">{r.Age61To90 ? inr(r.Age61To90) : '—'}</td>
                          <td className="px-3 py-2.5 text-right font-medium tabular-nums text-danger-600">{r.Age90Plus ? inr(r.Age90Plus) : '—'}</td>
                          <td className="px-3 py-2.5 text-xs text-slate-500">
                            {r.OldestUnpaidDate ? <>{day(r.OldestUnpaidDate)}<span className="block text-slate-400">{r.OldestUnpaidDays} day{r.OldestUnpaidDays === 1 ? '' : 's'}</span></> : '—'}
                          </td>
                          <td className="px-3 py-2.5 text-xs text-slate-500">{day(r.LastCollectionDate)}</td>
                          <td className="px-3 py-2.5 text-right">
                            <Button variant="ghost" className="h-7 px-2 text-xs" onClick={() => setViewing(r)}>
                              Statement
                            </Button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot className="border-t-2 border-slate-200 bg-slate-50 text-sm font-semibold text-slate-800">
                      <tr>
                        <td className="px-4 py-2.5" colSpan={2}>
                          {rows.length} account{rows.length === 1 ? '' : 's'}
                          {rows.length !== (data?.data.length ?? 0) && (
                            <span className="ml-1 font-normal text-slate-400">of {data?.data.length ?? 0}</span>
                          )}
                        </td>
                        <td className="px-3 py-2.5 text-right tabular-nums">{inr(shown.outstanding)}</td>
                        <td className="px-3 py-2.5" />
                        <td className="px-3 py-2.5" />
                        <td className="px-3 py-2.5 text-right tabular-nums">{inr(shown.a)}</td>
                        <td className="px-3 py-2.5 text-right tabular-nums">{inr(shown.b)}</td>
                        <td className="px-3 py-2.5 text-right tabular-nums">{inr(shown.c)}</td>
                        <td className="px-3 py-2.5 text-right tabular-nums text-danger-600">{inr(shown.d)}</td>
                        <td className="px-3 py-2.5" colSpan={3} />
                      </tr>
                    </tfoot>
                  </table>
                </div>
              )}
            </CardBody>
          </Card>
      </>

      <Modal
        open={viewing !== null}
        onClose={() => setViewing(null)}
        title={viewing ? `Statement — ${viewing.FirmName || viewing.UserName}` : ''}
        size="xl"
      >
        {viewing && <OutstandingStatement userId={viewing.UserID} parentId={viewing.ParentID} />}
      </Modal>
    </>
  );
}
