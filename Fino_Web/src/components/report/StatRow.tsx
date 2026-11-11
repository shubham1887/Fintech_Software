import type { ReactNode } from 'react';
import { cn } from '@/lib/cn';
import { Skeleton } from '@/components/ui/Skeleton';

export interface StatRowItem {
  label: string;
  value: string;
  /** Keeps the page's existing colour coding — credit green, debit red, and so on. */
  tone?: string;
  /**
   * Optional leading glyph. Kept small and muted: on a figures bar the icon is there to help you
   * find a metric again, not to decorate it, and the old card tiles gave it a 40px chip that cost
   * more height than the number it sat beside.
   */
  icon?: ReactNode;
  /**
   * A word of context under the figure — "Start of period", "12 entries". Only a couple of pages
   * need it, and only those pay the extra line of height.
   */
  sub?: string;
}

/**
 * The compact figures bar that sits above a report's table.
 *
 * It replaces a grid of padded KPI cards. Seven of those cost roughly 75px of height on a desktop
 * and four stacked rows on a phone — before a single row of the data they summarise was visible.
 * A summary that pushes the table below the fold is working against the page: these figures are
 * context for the rows, not the point of the screen.
 *
 * So: one bordered strip, hairline dividers, no per-metric card and no shadow; label at 10px over
 * the figure at 13px, tight padding. About 42px tall instead of 75, with the same seven values and
 * the same colours. It stays a grid rather than a flex row so the dividers stay correct when it
 * wraps — three across on a phone, four on a tablet, all seven in one line on a desktop.
 */
export function StatRow({
  items,
  loading,
  columns = 7,
}: {
  items: StatRowItem[];
  loading?: boolean;
  /** How many fit on one line at desktop width. */
  columns?: 4 | 5 | 6 | 7;
}) {
  if (!loading && items.length === 0) return null;

  // Tailwind needs whole class names, so the desktop track count is mapped rather than built.
  const LG: Record<number, string> = {
    4: 'lg:grid-cols-4',
    5: 'lg:grid-cols-5',
    6: 'lg:grid-cols-6',
    7: 'lg:grid-cols-7',
  };

  return (
    <div
      className={cn(
        'mb-4 grid grid-cols-3 divide-x divide-y divide-slate-200/70 overflow-hidden',
        'rounded-lg border border-slate-200/70 bg-white sm:grid-cols-4 lg:divide-y-0',
        LG[columns] ?? LG[7],
      )}
    >
      {(loading ? Array.from({ length: columns }, (): StatRowItem => ({ label: '', value: '' })) : items).map(
        (item, i) => (
          <div key={item.label || i} className="flex min-w-0 items-center gap-2 px-3 py-1.5">
            {loading ? (
              <div className="min-w-0 flex-1">
                <Skeleton className="h-2.5 w-14" />
                <Skeleton className="mt-1.5 h-3.5 w-20" />
              </div>
            ) : (
              <>
                {item.icon && <span className="shrink-0 text-slate-300 [&>svg]:size-4">{item.icon}</span>}
                <div className="min-w-0 flex-1">
                  <div
                    className="truncate text-[10px] font-medium uppercase tracking-wide text-slate-400"
                    title={item.label}
                  >
                    {item.label}
                  </div>
                  <div
                    className={cn('truncate text-[13px] font-semibold leading-tight tabular-nums', item.tone ?? 'text-slate-800')}
                    title={item.value}
                  >
                    {item.value}
                  </div>
                  {item.sub && (
                    <div className="truncate text-[9px] leading-tight text-slate-400" title={item.sub}>{item.sub}</div>
                  )}
                </div>
              </>
            )}
          </div>
        ),
      )}
    </div>
  );
}
