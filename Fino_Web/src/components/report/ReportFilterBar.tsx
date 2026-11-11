import { useState, type ReactNode } from 'react';
import { ChevronDown, RotateCcw, Search as SearchIcon, SlidersHorizontal } from 'lucide-react';
import { cn } from '@/lib/cn';
import { Card, CardBody } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';

/**
 * The filter block every report wears.
 *
 * Before this each report screen laid out its own filters: some in a card, some in a bare row,
 * some collapsible and some not, with Search and Reset in a different place each time. The fields
 * genuinely differ from report to report — that is what `children` is for — but the frame, the
 * grid, the submit-on-Enter behaviour and the two buttons are the same everywhere, and now come
 * from here.
 *
 * Fields go in as children and lay themselves out on a 1 / 2 / 4 column grid. A field that needs
 * more room can span with `className="sm:col-span-2"`.
 */
export function ReportFilterBar({
  children,
  onSearch,
  onReset,
  loading,
  /**
   * Omit `onSearch` when the report filters as you type (client-side lists). The Search button is
   * then left out rather than shown doing nothing, and the note below says the filters are live.
   */
  /** Start collapsed behind a "Filters" toggle — for screens whose filters are rarely touched. */
  collapsible = false,
  defaultOpen = true,
  /** Extra controls (export, grouping) pinned to the right of the action row. */
  extra,
  searchLabel = 'Search',
}: {
  children: ReactNode;
  onSearch?: () => void;
  onReset?: () => void;
  loading?: boolean;
  collapsible?: boolean;
  defaultOpen?: boolean;
  extra?: ReactNode;
  searchLabel?: string;
}) {
  const [open, setOpen] = useState(defaultOpen || !collapsible);
  const shown = collapsible ? open : true;

  return (
    <Card className="mb-4">
      <CardBody className={cn(collapsible && !shown && 'py-3')}>
        <div className="mb-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-sm font-medium text-slate-600">
            <SlidersHorizontal className="size-4 text-brand-600" />
            Filters
          </div>
          {collapsible && (
            <button
              type="button"
              onClick={() => setOpen((v) => !v)}
              aria-expanded={shown}
              className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 px-3 py-1.5 text-xs font-medium text-slate-600 transition-colors hover:bg-slate-50"
            >
              {shown ? 'Hide' : 'Show'}
              <ChevronDown className={cn('size-3.5 transition-transform', shown && 'rotate-180')} />
            </button>
          )}
        </div>

        {shown && (
          <form
            onSubmit={(e) => { e.preventDefault(); onSearch?.(); }}
            className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4"
          >
            {children}
            <div className="flex items-end gap-2">
              {onSearch ? (
                <Button type="submit" loading={loading} className="flex-1">
                  <SearchIcon className="size-4" />
                  {searchLabel}
                </Button>
              ) : (
                <p className="flex-1 pb-2.5 text-xs text-slate-400">Filters apply as you type.</p>
              )}
              {onReset && (
                <Button type="button" variant="outline" onClick={onReset} title="Reset every filter">
                  <RotateCcw className="size-4" />
                </Button>
              )}
            </div>
            {extra && <div className="flex items-end justify-end gap-2 sm:col-span-2 lg:col-span-4">{extra}</div>}
          </form>
        )}
      </CardBody>
    </Card>
  );
}
