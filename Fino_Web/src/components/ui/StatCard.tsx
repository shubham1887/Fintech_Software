import type { LucideIcon } from 'lucide-react';
import { cn } from '@/lib/cn';
import { Skeleton } from './Skeleton';

type Accent = 'brand' | 'success' | 'danger' | 'warning' | 'info' | 'neutral';

const ACCENTS: Record<Accent, string> = {
  brand: 'bg-brand-50 text-brand-600',
  success: 'bg-success-50 text-success-600',
  danger: 'bg-danger-50 text-danger-600',
  warning: 'bg-warning-50 text-warning-600',
  info: 'bg-info-50 text-info-600',
  neutral: 'bg-slate-100 text-slate-500',
};

export function StatCard({
  label,
  value,
  sub,
  icon: Icon,
  accent = 'brand',
  loading = false,
}: {
  label: string;
  value: string;
  sub?: string;
  icon?: LucideIcon;
  accent?: Accent;
  loading?: boolean;
}) {
  return (
    <div className="rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
      <div className="flex items-start justify-between">
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-slate-500">{label}</p>
          {loading ? (
            <Skeleton className="mt-2 h-7 w-28" />
          ) : (
            <p className="mt-1 font-display text-2xl font-bold tracking-tight text-slate-900">
              {value}
            </p>
          )}
          {sub && !loading && <p className="mt-1 text-xs text-slate-400">{sub}</p>}
        </div>
        {Icon && (
          <span className={cn('grid size-10 shrink-0 place-items-center rounded-xl', ACCENTS[accent])}>
            <Icon className="size-5" />
          </span>
        )}
      </div>
    </div>
  );
}
