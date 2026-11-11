import type { LucideIcon } from 'lucide-react';
import { cn } from '@/lib/cn';
import { Skeleton } from './Skeleton';

/**
 * One metric in a page's figures bar.
 *
 * This used to be a card: its own border, its own shadow, 20px of padding and a 40px icon chip
 * beside a 24px number — about 90px of height per metric, four of them across the top of a list
 * page, before a single row of the list was visible. On a phone they stacked two-wide and cost
 * four rows.
 *
 * It is now a strip item, matching `StatRow` elsewhere in the product: the chrome belongs to the
 * container (see `STAT_STRIP`), the icon is a small muted glyph rather than a coloured chip, and
 * the whole thing is about 45px tall. `tone` is still accepted so call sites need no edit, but it
 * now colours the glyph alone — a tinted square per metric was decoration competing with the data.
 */
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
  /** Tailwind bg+text classes, e.g. "bg-brand-50 text-brand-600". Only the text colour is used. */
  tone: string;
  loading?: boolean;
}) {
  // Keep just the text-* class out of the old "bg-x text-y" pair.
  const iconColor = tone.split(/\s+/).find((c) => c.startsWith('text-')) ?? 'text-slate-400';

  return (
    <div className="flex min-w-0 items-center gap-2 px-3 py-1.5">
      <Icon className={cn('size-4 shrink-0 opacity-70', iconColor)} />
      <div className="min-w-0 flex-1">
        <p className="truncate text-[10px] font-medium uppercase tracking-wide text-slate-400" title={label}>
          {label}
        </p>
        {loading ? (
          <Skeleton className="mt-1 h-3.5 w-16" />
        ) : (
          <p className="truncate text-[13px] font-semibold leading-tight tabular-nums text-slate-800">{value}</p>
        )}
      </div>
    </div>
  );
}

/**
 * The container a row of `StatTile`s goes in — one bordered strip with hairline dividers, no
 * shadow, no gaps. Screens pass their own column count on top of this.
 */
export const STAT_STRIP =
  'mb-4 grid divide-x divide-y divide-slate-200/70 overflow-hidden rounded-lg border border-slate-200/70 bg-white lg:divide-y-0';
