import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Eye, Wallet, AlertTriangle } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr } from '@/lib/format';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Skeleton } from '@/components/ui/Skeleton';
import { OutstandingStatement } from './OutstandingStatement';

export interface MyStanding {
  outstanding: number;
  limit: number;
  available: number;
  isDefaultLimit: boolean;
  parentName: string;
  parentMobile: string;
  featureEnabled: boolean;
}

/**
 * The caller's OWN outstanding — what they owe their upline, their limit, and their statement.
 *
 * This exists because the outstanding list is a downline view: it answers "who owes me". A
 * retailer has no downline, so that list is empty for them and the whole feature looked like it
 * did not apply to the people who actually carry the debt. Every role needs the other half —
 * "what do I owe, to whom, and against what entries" — and it has to be the same statement their
 * upline is looking at, or the two of them are arguing from different numbers.
 *
 * Endpoint: CashCollection/SelectByUserID (no id = the signed-in user; the server will not serve
 * anyone else's).
 */
export function useMyStanding() {
  return useQuery({
    queryKey: ['my-outstanding'],
    queryFn: async (): Promise<MyStanding> => {
      const res = await apiPost<Record<string, unknown>>('CashCollection/SelectByUserID', { UserID: 0 });
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not read your outstanding.');
      const d = res.data as Record<string, unknown>;
      return {
        outstanding: Number(getField(d, 'Outstanding') ?? 0),
        limit: Number(getField(d, 'MaxOutStanding', 'MaxOutstanding') ?? 0),
        available: Number(getField(d, 'AvailableCredit') ?? 0),
        isDefaultLimit: getField(d, 'IsDefaultLimit') === true,
        parentName: String(getField(d, 'ParentName') ?? ''),
        parentMobile: String(getField(d, 'ParentMobile') ?? ''),
        featureEnabled: getField(d, 'FeatureEnabled') !== false,
      };
    },
    staleTime: 30_000,
  });
}

export function MyOutstandingCard({ userId }: { userId: number }) {
  const { data, isLoading, isError, error } = useMyStanding();
  const [open, setOpen] = useState(false);

  if (isLoading) {
    return (
      <Card className="p-5">
        <Skeleton className="h-5 w-40" />
        <Skeleton className="mt-3 h-8 w-32" />
      </Card>
    );
  }

  // A failed read is never drawn as ₹0.00 — in a credit screen that reads as "I owe nothing",
  // which is the one wrong conclusion that costs somebody money.
  if (isError) {
    return (
      <Card className="flex items-center gap-2 p-5 text-sm text-warning-700">
        <AlertTriangle className="size-4 shrink-0" />
        Your outstanding could not be loaded. {(error as Error)?.message}
      </Card>
    );
  }

  if (!data) return null;

  // Nothing owed and no credit facility at all: there is nothing here worth a panel, and an empty
  // "₹0.00 outstanding" box on every visit is just furniture.
  if (data.outstanding <= 0 && data.limit <= 0) return null;

  const overLimit = data.limit > 0 && data.outstanding > data.limit;
  const usedPct = data.limit > 0 ? Math.min(100, (data.outstanding / data.limit) * 100) : 0;

  return (
    <>
      <Card className="overflow-hidden">
        <div className="flex flex-wrap items-start justify-between gap-4 p-5">
          <div>
            <div className="flex items-center gap-2 text-[11px] font-medium uppercase tracking-wide text-slate-400">
              <Wallet className="size-3.5" />
              My outstanding
            </div>
            <div className={cn(
              'mt-1 font-display text-2xl font-extrabold tabular-nums',
              data.outstanding > 0 ? 'text-warning-600' : 'text-slate-800',
            )}>
              {inr(data.outstanding)}
            </div>
            {data.parentName && (
              <p className="mt-1 text-sm text-slate-500">
                Payable to <span className="font-medium text-slate-700">{data.parentName}</span>
                {data.parentMobile ? ` · ${data.parentMobile}` : ''}
              </p>
            )}
            {!data.featureEnabled && (
              <p className="mt-1 text-xs text-slate-400">
                Cash collection is currently switched off — this balance is still payable.
              </p>
            )}
          </div>

          <div className="flex items-center gap-5">
            <div className="text-right">
              <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">Credit limit</div>
              <div className="text-sm font-semibold tabular-nums text-slate-800">
                {data.limit > 0 ? inr(data.limit) : 'No credit'}
              </div>
              {data.limit > 0 && data.isDefaultLimit && (
                <div className="text-[11px] text-slate-400">network default</div>
              )}
            </div>
            <span className="h-10 w-px bg-slate-200/80" />
            <div className="text-right">
              <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">Available</div>
              <div className={cn(
                'text-sm font-semibold tabular-nums',
                overLimit ? 'text-danger-600' : 'text-success-600',
              )}>
                {overLimit ? 'Over limit' : inr(data.available)}
              </div>
            </div>
            <Button variant="outline" size="sm" onClick={() => setOpen(true)}>
              <Eye className="size-4" />
              Statement
            </Button>
          </div>
        </div>

        {/* How much of the facility is used. A bar rather than a percentage, because the question
            being asked is "have I got room for one more load", not "what fraction is it". */}
        {data.limit > 0 && (
          <div className="h-1.5 w-full bg-slate-100">
            <div
              className={cn('h-full transition-all', overLimit ? 'bg-danger-500' : usedPct > 80 ? 'bg-warning-500' : 'bg-brand-500')}
              style={{ width: `${overLimit ? 100 : usedPct}%` }}
            />
          </div>
        )}
      </Card>

      <Modal open={open} onClose={() => setOpen(false)} title="My outstanding statement" size="xl">
        <OutstandingStatement userId={userId} />
      </Modal>
    </>
  );
}
