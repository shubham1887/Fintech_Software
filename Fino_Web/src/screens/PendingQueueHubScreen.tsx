import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Clock, RefreshCw } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { pickIcon } from '@/components/layout/iconMap';
import { PendingTxnScreen } from './PendingTxnScreen';
import { DMT_FAMILY, RECHARGE_FAMILY, genericFamily } from './pendingFamilies';
import type { CustomScreenProps } from './registry';

interface QueueItem {
  Kind: string;
  Label: string;
  PendingCount: number;
  PendingAmount: number;
  OldestPendingOn: string | null;
  OldestPendingHours: number;
  ServiceIDs: number[];
}

interface QueueSummary {
  Queues: QueueItem[];
  TotalPending: number;
  TotalAmount: number;
}

/**
 * How long is too long. A provider that has not answered for a few minutes is a provider being
 * slow; one that has not answered since yesterday is money nobody has accounted for, and the
 * difference is the only thing on this page that decides what to work on first.
 */
function ageTone(hours: number): string {
  if (hours >= 24) return 'text-danger-700 bg-danger-50 border-danger-200';
  if (hours >= 2) return 'text-warning-700 bg-warning-50 border-warning-200';
  return 'text-slate-600 bg-slate-50 border-slate-200';
}

function ageLabel(hours: number): string {
  if (hours <= 0) return '—';
  if (hours < 1) return `${Math.round(hours * 60)} min`;
  if (hours < 48) return `${hours.toFixed(1)} h`;
  return `${Math.round(hours / 24)} d`;
}

/**
 * Pending Queues — one page listing every queue this install actually has, with what is waiting
 * in each, and opening into the same workbench for whichever one is chosen.
 *
 * It exists because there were exactly two queues, Recharge and DMT, written one screen each.
 * Every other service — bill payment, electricity, payout, AEPS, Micro ATM — could leave
 * transactions sitting in PENDING with nowhere to see them and no way to status-check or settle
 * them by hand. The queues are now discovered from the service catalogue, so a service the admin
 * creates tomorrow gets a working queue with it.
 *
 * Recharge and DMT still open their own descriptions (provider switching, the global transfer
 * hold); everything else opens the generic one.
 */
export function PendingQueueHubScreen({ def }: CustomScreenProps) {
  const [openKind, setOpenKind] = useState<QueueItem | null>(null);

  const { data, isFetching, refetch } = useQuery({
    queryKey: ['pending-queues'],
    refetchInterval: 60_000,
    queryFn: async () => {
      const res = await apiPost<QueueSummary>('TransactionReport/SelectPendingQueues', {});
      return res.ok ? (res.data as QueueSummary) : null;
    },
  });

  if (openKind) {
    const family =
      openKind.Kind === 'RECHARGE' ? RECHARGE_FAMILY
      : openKind.Kind === 'DMT' ? DMT_FAMILY
      : genericFamily(openKind.Kind, openKind.Label);

    return (
      <>
        <Button variant="outline" size="sm" className="mb-3" onClick={() => setOpenKind(null)}>
          <ArrowLeft className="size-4" /> All queues
        </Button>
        <PendingTxnScreen def={{ ...def, title: `Pending ${openKind.Label}` }} family={family} />
      </>
    );
  }

  const queues = data?.Queues ?? [];

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={
          data
            ? `${data.TotalPending} transaction${data.TotalPending === 1 ? '' : 's'} waiting across ${queues.length} queue${queues.length === 1 ? '' : 's'}`
            : 'Every service with transactions awaiting a final answer'
        }
        icon={pickIcon(def.title)}
        actions={
          <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" /> Refresh
          </Button>
        }
      />

      {isFetching && !data ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <Card key={i}><CardBody><Skeleton className="h-20 w-full" /></CardBody></Card>
          ))}
        </div>
      ) : queues.length === 0 ? (
        <Card>
          <CardBody>
            <EmptyState
              title="No queues to show"
              description="No service is configured yet, so there is nothing that could be pending."
            />
          </CardBody>
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {queues.map((q) => (
            <button
              key={q.Kind}
              type="button"
              onClick={() => setOpenKind(q)}
              className={cn(
                'rounded-card border bg-white p-4 text-left shadow-card transition-all',
                'hover:-translate-y-0.5 hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-brand-500/30',
                q.PendingCount > 0 ? 'border-slate-200' : 'border-slate-200/70',
              )}
            >
              <div className="flex items-start justify-between gap-3">
                <span className="min-w-0 text-sm font-semibold text-slate-800">{q.Label}</span>
                {q.PendingCount > 0 && (
                  <span className={cn('shrink-0 rounded-full border px-2 py-0.5 text-[11px] font-semibold', ageTone(q.OldestPendingHours))}>
                    <Clock className="mr-1 inline size-3" />
                    {ageLabel(q.OldestPendingHours)}
                  </span>
                )}
              </div>

              <p className="mt-3 text-2xl font-semibold tabular-nums text-slate-900">{q.PendingCount}</p>
              <p className="text-xs text-slate-500">
                {q.PendingCount === 0
                  ? 'Nothing waiting'
                  : `${inr(q.PendingAmount)} awaiting a final answer`}
              </p>
            </button>
          ))}
        </div>
      )}
    </>
  );
}
