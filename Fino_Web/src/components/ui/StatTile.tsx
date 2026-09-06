import type { LucideIcon } from 'lucide-react';
import { cn } from '@/lib/cn';
import { Skeleton } from './Skeleton';

/** Compact KPI tile used at the top of bespoke list pages. */
export function StatTile({
  label,
  value,
  icon: Icon,
  tone,
  loading,
}: {
  label: string;
  value: number | string;
  icon: LucideIcon;
  /** Tailwind bg+text classes for the icon chip, e.g. "bg-brand-50 text-brand-600". */
  tone: string;
  loading?: boolean;
}) {
  return (
    <div className="rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm font-medium text-slate-500">{label}</p>
          {loading ? (
            <Skeleton className="mt-2 h-7 w-12" />
          ) : (
            <p className="mt-1 font-display text-2xl font-bold text-slate-900">{value}</p>
          )}
        </div>
        <span className={cn('grid size-10 place-items-center rounded-xl', tone)}>
          <Icon className="size-5" />
        </span>
      </div>
    </div>
  );
}
