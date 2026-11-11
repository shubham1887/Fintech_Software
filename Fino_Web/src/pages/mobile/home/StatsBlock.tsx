import { ArrowDownToLine, BadgeIndianRupee, TrendingDown, TrendingUp, type LucideIcon } from 'lucide-react';
import type { DashboardData } from '@/types/dashboard';
import type { StatsStyle } from '@/theme/mobileLayouts';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * Today's four figures, as a slim rail or as cards.
 *
 * `strip` is the glance: four numbers on one row, because for most roles these are something
 * you notice on the way past, not a destination — the detail is one tap away in History.
 * `cards` is for the operator who opens the app FOR these numbers: same four figures with the
 * transaction counts beside them, which the strip has no room to carry.
 */

export interface StatsProps {
  style: StatsStyle;
  data: DashboardData | null | undefined;
  loading: boolean;
}

interface Figure {
  label: string;
  value: number | undefined;
  count?: number;
  icon: LucideIcon;
  tone: string;
  chip: string;
}

function figures(data: StatsProps['data']): Figure[] {
  return [
    {
      label: 'Sale',
      value: data?.TotalSuccessAmount,
      count: data?.PurchaseCount,
      icon: TrendingUp,
      tone: 'text-success-600',
      chip: 'bg-success-50 text-success-600',
    },
    {
      label: 'Commission',
      value: data?.TotalCommission,
      icon: BadgeIndianRupee,
      tone: 'text-brand-600',
      chip: 'bg-brand-50 text-brand-600',
    },
    {
      label: 'Failed',
      value: data?.TotalFailedAmount,
      icon: TrendingDown,
      tone: 'text-danger-600',
      chip: 'bg-danger-50 text-danger-600',
    },
    {
      label: 'Received',
      value: data?.PurchaseAmount,
      icon: ArrowDownToLine,
      tone: 'text-info-600',
      chip: 'bg-info-50 text-info-600',
    },
  ];
}

export function StatsBlock({ style, data, loading }: StatsProps) {
  return style === 'cards' ? <TodayCards data={data} loading={loading} /> : <TodayStrip data={data} loading={loading} />;
}

/* ---------------------------------------------------------------- strip */

/**
 * Four numbers, one row. This used to be a 2x2 grid of full-size cards that ate half the
 * screen for figures nobody acts on directly — they are a glance, not a destination, so they
 * get a glance's worth of space.
 */
function TodayStrip({ data, loading }: { data: StatsProps['data']; loading: boolean }) {
  const items = figures(data);

  return (
    <section>
      <h2 className="mb-2 px-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Today</h2>
      <div className="flex items-stretch overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {items.map((it, i) => (
          <div key={it.label} className={cn('min-w-0 flex-1 px-1.5 py-2.5 text-center', i > 0 && 'border-l border-slate-100')}>
            <p className="flex items-center justify-center gap-1 text-[10px] font-medium text-slate-400">
              <it.icon className={cn('size-3 shrink-0', it.tone)} />
              {/* The strip has ~70px per column, so the long label is shortened here rather
                  than truncated mid-word by the browser. */}
              <span className="truncate">{it.label === 'Commission' ? 'Comm.' : it.label}</span>
            </p>
            {loading ? (
              <Skeleton className="mx-auto mt-1.5 h-3.5 w-12" />
            ) : (
              <p className="mt-0.5 truncate font-display text-[13px] font-bold tabular-nums text-slate-900">
                {inr(it.value)}
              </p>
            )}
          </div>
        ))}
      </div>
    </section>
  );
}

/* ---------------------------------------------------------------- cards */

/** 2×2, full figures, counts where the API sends them. The business preset leads with this. */
function TodayCards({ data, loading }: { data: StatsProps['data']; loading: boolean }) {
  const items = figures(data);

  return (
    <section>
      <h2 className="mb-2 px-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Today</h2>
      <div className="grid grid-cols-2 gap-2.5">
        {items.map((it) => (
          <div key={it.label} className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
            <div className="flex items-center justify-between gap-2">
              <span className="truncate text-[11px] font-medium text-slate-500">{it.label}</span>
              <span className={cn('grid size-6 shrink-0 place-items-center rounded-lg', it.chip)}>
                <it.icon className="size-3.5" />
              </span>
            </div>
            {loading ? (
              <Skeleton className="mt-2 h-5 w-20" />
            ) : (
              <p className="mt-1.5 truncate font-display text-[18px] font-extrabold leading-none tabular-nums text-slate-900">
                {inr(it.value)}
              </p>
            )}
            {!loading && it.count != null && (
              <p className="mt-1 text-[11px] tabular-nums text-slate-400">
                {it.count} {it.count === 1 ? 'transaction' : 'transactions'}
              </p>
            )}
          </div>
        ))}
      </div>
    </section>
  );
}
