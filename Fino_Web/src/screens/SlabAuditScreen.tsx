import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import { History, RefreshCw, Search } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';
import { apiDate, inr } from '@/lib/format';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { Input } from '@/components/ui/Input';
import { PageHeader } from '@/components/ui/PageHeader';
import { Spinner } from '@/components/ui/Spinner';
import type { CustomScreenProps } from './registry';

/**
 * Commission Audit Trail — every commission figure that moved, on any slab, by anybody.
 *
 * Append-only on the server, scoped on the server: a distributor sees its own branch, admin sees
 * every branch. Each row names the tier that moved, what it moved from and to, and the ceiling it
 * was measured against AT THE TIME, which is the whole answer to "was that rate allowed when it
 * was set" once the parent card has been re-cut since.
 */

type Row = Record<string, unknown>;
const num = (r: Row, k: string) => Number(getField(r, k) ?? 0);
const str = (r: Row, k: string) => String(getField(r, k) ?? '');

/** The slab-level acts carry no tier figures — they read as a sentence, not a rate change. */
const ACTIONS: Record<string, { label: string; tone: 'brand' | 'info' | 'danger' | 'success' | 'neutral' }> = {
  CREATE: { label: 'Slab created', tone: 'brand' },
  ASSIGN: { label: 'Assigned', tone: 'info' },
  UNASSIGN: { label: 'Unassigned', tone: 'neutral' },
  INVALIDATE: { label: 'Marked over limit', tone: 'danger' },
  REVALIDATE: { label: 'Back in limit', tone: 'success' },
};

const TIER_LABEL: Record<string, string> = {
  Retailer: 'Retailer',
  Distributor: 'Distributor',
  SuperDistributor: 'Super Distributor',
  Client: 'Client',
};

export function SlabAuditScreen({ def }: CustomScreenProps) {
  // A month back by default: a commission dispute is usually about the last cycle, and an
  // unbounded first load on an append-only table is the one query that gets slower every week.
  const [fromDate, setFromDate] = useState(() => apiDate(subDays(new Date(), 30)));
  const [toDate, setToDate] = useState(() => apiDate(new Date()));
  const [filters, setFilters] = useState({ FromDate: '', ToDate: '' });
  const [page, setPage] = useState(1);
  const pageSize = 50;

  const effective = useMemo(
    () => ({ FromDate: filters.FromDate || fromDate, ToDate: filters.ToDate || toDate }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [filters],
  );

  const q = useQuery({
    queryKey: ['slab-audit', effective, page],
    queryFn: () =>
      apiPost<Row>('SlabDelegation/Audit', {
        FromDate: effective.FromDate,
        ToDate: effective.ToDate,
        PageNumber: page,
        PageSize: pageSize,
      }),
  });

  const data = (q.data?.ok ? q.data.data : null) as Row | null;
  const rows = ((data ? getField(data, 'Rows') : null) as Row[] | null) ?? [];
  const total = data ? num(data, 'TotalCount') : 0;
  const pages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={History}
        actions={
          <Button variant="outline" loading={q.isFetching} onClick={() => q.refetch()}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
        }
      />

      <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
          <Input
            label="From date"
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className="sm:w-48"
          />
          <Input
            label="To date"
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            className="sm:w-48"
          />
          <Button
            onClick={() => {
              setPage(1);
              setFilters({ FromDate: fromDate, ToDate: toDate });
            }}
          >
            <Search className="size-4" />
            Search
          </Button>
        </div>
      </div>

      <div className="rounded-card border border-slate-200/70 bg-white shadow-card">
        {q.isLoading ? (
          <div className="flex justify-center py-16"><Spinner /></div>
        ) : rows.length === 0 ? (
          <EmptyState
            icon={History}
            title="No commission changes in this period"
            description="Rate changes, slab creations and assignments appear here as they happen."
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[60rem] border-collapse text-left">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50/70 text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-5 py-2">When</th>
                    <th className="px-3 py-2">Slab</th>
                    <th className="px-3 py-2">Band</th>
                    <th className="px-3 py-2">What changed</th>
                    <th className="px-3 py-2">From → to</th>
                    <th className="px-3 py-2">Parent limit</th>
                    <th className="px-3 py-2">By</th>
                    <th className="px-3 py-2">IP</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r) => {
                    const tier = str(r, 'Tier');
                    const action = ACTIONS[tier.toUpperCase()];
                    return (
                      <tr key={num(r, 'SlabCommissionAuditID')} className="border-b border-slate-100 last:border-0">
                        <td className="whitespace-nowrap px-5 py-3 text-sm text-slate-600">
                          {new Date(str(r, 'DateTime')).toLocaleString()}
                        </td>
                        <td className="px-3 py-3 text-sm">
                          <p className="font-medium text-slate-800">{str(r, 'SlabName')}</p>
                          <p className="text-xs text-slate-500">{str(r, 'OwnerName')}</p>
                        </td>
                        <td className="px-3 py-3 text-sm text-slate-600">
                          {action ? (
                            <span className="text-slate-400">—</span>
                          ) : (
                            <>
                              <p>{str(r, 'OperatorName') || str(r, 'ServiceName') || '—'}</p>
                              <p className="text-xs tabular-nums text-slate-400">
                                {inr(num(r, 'FromAmount'))} – {inr(num(r, 'ToAmount'))}
                              </p>
                            </>
                          )}
                        </td>
                        <td className="px-3 py-3 text-sm">
                          {action ? (
                            <Badge tone={action.tone}>{action.label}</Badge>
                          ) : (
                            <span className="font-medium text-slate-700">{TIER_LABEL[tier] ?? tier}</span>
                          )}
                          {str(r, 'Remark') && (
                            <p className="mt-1 max-w-[22rem] text-xs text-slate-500">{str(r, 'Remark')}</p>
                          )}
                        </td>
                        <td className="px-3 py-3 text-sm tabular-nums text-slate-700">
                          {action ? (
                            <span className="text-slate-400">—</span>
                          ) : (
                            <>
                              {inr(num(r, 'OldCommission'))} <span className="text-slate-400">→</span>{' '}
                              <span className="font-semibold">{inr(num(r, 'NewCommission'))}</span>
                            </>
                          )}
                        </td>
                        <td className="px-3 py-3 text-sm tabular-nums text-slate-500">
                          {action ? '—' : inr(num(r, 'ParentCommission'))}
                        </td>
                        <td className="px-3 py-3 text-sm text-slate-600">{str(r, 'UpdatedByName')}</td>
                        <td className="px-3 py-3 text-xs text-slate-400">{str(r, 'IPAddress') || '—'}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {pages > 1 && (
              <div className="flex items-center justify-between border-t border-slate-100 px-5 py-3 text-sm text-slate-500">
                <span>
                  Page {page} of {pages} · {total} change(s)
                </span>
                <div className="flex gap-2">
                  <Button size="sm" variant="outline" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                    Previous
                  </Button>
                  <Button size="sm" variant="outline" disabled={page >= pages} onClick={() => setPage((p) => p + 1)}>
                    Next
                  </Button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </>
  );
}
