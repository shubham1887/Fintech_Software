import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { Columns3, Check } from 'lucide-react';
import { cn } from '@/lib/cn';
import { getField } from '@/lib/row';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { useIsMobileApp } from '@/lib/native';
import { MobileRecordList } from '@/components/ui/MobileRecordList';
import { formatReportCell, isNumericType, type ReportCellType } from './reportFormat';

export interface ReportTableColumn<T = Record<string, unknown>> {
  /** Field on the row. Also the column's identity, so it must be unique within a report. */
  key: string;
  label: string;
  type?: ReportCellType;
  /** Overrides the alignment implied by `type`. */
  align?: 'left' | 'right';
  /** Bespoke rendering. When given, `type` is only used for alignment. */
  render?: (row: T, serial: number) => ReactNode;
  /** Sum this column across the rows on screen and show it in the footer. */
  total?: boolean;
  /**
   * How the footer figure for this column is reached. 'sum' is the default and is right for
   * anything that accumulates — sale, commission, tax.
   *
   * BALANCES do not accumulate. Adding up each day's opening balance produces a number that is
   * not a balance of anything: on a week of rows it reads roughly seven times the float, which is
   * alarming and wrong. For those, the period's opening is the FIRST row's and the period's
   * closing is the LAST row's, which is what 'first' and 'last' take.
   */
  totalMode?: 'sum' | 'first' | 'last';
  /** Hidden by default in the column chooser (still exportable). */
  optional?: boolean;
  /** Tooltip on the header — for abbreviated labels like "SD. Comm". */
  title?: string;
}

interface ReportTableProps<T> {
  columns: ReportTableColumn<T>[];
  rows: T[];
  loading?: boolean;
  /** 1-based number of the first row on this page, so serials continue across pages. */
  serialStart?: number;
  rowKey?: (row: T, index: number) => string;
  /** Leading cell on every row — the kebab menu, a checkbox, whatever the screen needs. */
  rowLeading?: (row: T, index: number) => ReactNode;
  onRowClick?: (row: T) => void;
  /** Shown in place of the generic "nothing found" text — usually the API's own reason. */
  emptyReason?: string;
  emptyTitle?: string;
  emptyDescription?: string;
  /** Offer the "Columns" chooser. On by default for reports with more than eight columns. */
  columnChooser?: boolean;
  /** Show the totals footer for columns marked `total`. Defaults to on when any column is marked. */
  showTotals?: boolean;
  footerLabel?: string;
}

/**
 * The report table.
 *
 * Every report in the product renders through this: same header treatment, same zebra striping,
 * same right-aligned numerics, same skeleton while loading, same empty state, same totals row, and
 * the same "Columns" chooser on wide reports. Screens supply columns and rows; anything specific
 * to one report goes in a column's `render`.
 *
 * On a phone it falls back to the card list — these tables are ten to twenty columns wide, and
 * horizontal scrolling on a handset hides exactly the columns someone opened the report to see.
 */
export function ReportTable<T extends Record<string, unknown>>({
  columns,
  rows,
  loading,
  serialStart = 1,
  rowKey,
  rowLeading,
  onRowClick,
  emptyReason,
  emptyTitle,
  emptyDescription,
  columnChooser,
  showTotals,
  footerLabel = 'Total',
}: ReportTableProps<T>) {
  const isMobile = useIsMobileApp();
  const [hidden, setHidden] = useState<Set<string>>(() => new Set(columns.filter((c) => c.optional).map((c) => c.key)));
  const [chooserOpen, setChooserOpen] = useState(false);
  const chooserRef = useRef<HTMLDivElement>(null);

  // A report can swap its whole column set when the role or the grouping changes; drop any hidden
  // key that no longer exists so the chooser cannot accumulate ghosts.
  useEffect(() => {
    setHidden((prev) => {
      const live = new Set(columns.map((c) => c.key));
      const next = new Set([...prev].filter((k) => live.has(k)));
      return next.size === prev.size ? prev : next;
    });
  }, [columns]);

  useEffect(() => {
    if (!chooserOpen) return;
    const onDoc = (e: MouseEvent) => {
      if (!chooserRef.current?.contains(e.target as Node)) setChooserOpen(false);
    };
    document.addEventListener('mousedown', onDoc);
    return () => document.removeEventListener('mousedown', onDoc);
  }, [chooserOpen]);

  const visible = useMemo(() => columns.filter((c) => !hidden.has(c.key)), [columns, hidden]);
  const offerChooser = columnChooser ?? columns.length > 8;
  const totalColumns = useMemo(() => columns.filter((c) => c.total), [columns]);
  const wantTotals = (showTotals ?? totalColumns.length > 0) && rows.length > 0 && !loading;

  const totals = useMemo(() => {
    if (!wantTotals) return {};
    const acc: Record<string, number> = {};
    for (const c of totalColumns) {
      const value = (r: T) => Number(getField(r, c.key)) || 0;
      acc[c.key] =
        c.totalMode === 'first' ? value(rows[0])
        : c.totalMode === 'last' ? value(rows[rows.length - 1])
        : rows.reduce((s, r) => s + value(r), 0);
    }
    return acc;
  }, [wantTotals, totalColumns, rows]);

  const alignRight = (c: ReportTableColumn<T>) => (c.align ? c.align === 'right' : isNumericType(c.type));
  const cell = (c: ReportTableColumn<T>, row: T, serial: number) =>
    c.render ? c.render(row, serial) : formatReportCell(c.type, getField(row, c.key), serial);

  const empty = (
    <EmptyState
      title={emptyTitle ?? (emptyReason ? 'Nothing to show' : 'No records found')}
      description={emptyReason || emptyDescription || 'Change your date range / filters and try again.'}
    />
  );

  if (isMobile) {
    return (
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {loading ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-12 rounded-lg" />)}
          </div>
        ) : rows.length === 0 ? (
          empty
        ) : (
          <MobileRecordList
            rows={rows as unknown[]}
            actions={rowLeading ? (row) => rowLeading(row as T, rows.indexOf(row as T)) : undefined}
            fields={visible.map((c) => ({
              key: c.key,
              label: c.label,
              render: (row: unknown) => {
                const i = rows.indexOf(row as T);
                return cell(c, row as T, serialStart + i);
              },
            }))}
          />
        )}
      </div>
    );
  }

  const colCount = visible.length + (rowLeading ? 1 : 0);

  return (
    <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
      {offerChooser && (
        <div className="relative flex justify-end border-b border-slate-100 px-3 py-2" ref={chooserRef}>
          <button
            type="button"
            onClick={() => setChooserOpen((v) => !v)}
            className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 px-2.5 py-1 text-xs font-medium text-slate-600 transition-colors hover:bg-slate-50"
          >
            <Columns3 className="size-3.5" />
            Columns
            <span className="tabular-nums text-slate-400">{visible.length}/{columns.length}</span>
          </button>
          {chooserOpen && (
            <div className="absolute right-3 top-full z-20 mt-1 max-h-80 w-60 overflow-auto rounded-lg border border-slate-200 bg-white py-1 shadow-pop">
              {columns.map((c) => {
                const on = !hidden.has(c.key);
                return (
                  <button
                    key={c.key}
                    type="button"
                    onClick={() => setHidden((prev) => {
                      const next = new Set(prev);
                      // Never let the last column be hidden — an empty table is not a view.
                      if (on && prev.size >= columns.length - 1) return prev;
                      if (on) next.add(c.key); else next.delete(c.key);
                      return next;
                    })}
                    className="flex w-full items-center gap-2 px-3 py-1.5 text-left text-sm text-slate-600 transition-colors hover:bg-slate-50"
                  >
                    <span className={cn(
                      'grid size-4 shrink-0 place-items-center rounded border',
                      on ? 'border-brand-500 bg-brand-500 text-white' : 'border-slate-300',
                    )}>
                      {on && <Check className="size-3" />}
                    </span>
                    <span className="truncate">{c.label}</span>
                  </button>
                );
              })}
            </div>
          )}
        </div>
      )}

      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
              {rowLeading && <th className="w-10 px-3 py-3" />}
              {visible.map((c) => (
                <th
                  key={c.key}
                  title={c.title}
                  className={cn('whitespace-nowrap px-4 py-3', alignRight(c) ? 'text-right' : 'text-left')}
                >
                  {c.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              Array.from({ length: 8 }).map((_, r) => (
                <tr key={r} className="border-b border-slate-100 last:border-0">
                  {Array.from({ length: Math.max(colCount, 1) }).map((__, c) => (
                    <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[120px]" /></td>
                  ))}
                </tr>
              ))
            ) : rows.length === 0 ? (
              <tr><td colSpan={Math.max(colCount, 1)}>{empty}</td></tr>
            ) : (
              rows.map((row, i) => (
                <tr
                  key={rowKey ? rowKey(row, i) : i}
                  onClick={onRowClick ? () => onRowClick(row) : undefined}
                  className={cn(
                    'border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60',
                    onRowClick && 'cursor-pointer',
                  )}
                >
                  {rowLeading && <td className="px-3 py-3">{rowLeading(row, i)}</td>}
                  {visible.map((c) => (
                    <td
                      key={c.key}
                      className={cn('whitespace-nowrap px-4 py-3 text-slate-700', alignRight(c) && 'text-right')}
                    >
                      {cell(c, row, serialStart + i)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
          {wantTotals && (
            <tfoot>
              <tr className="border-t-2 border-slate-200 bg-slate-50/80 font-semibold text-slate-800">
                {rowLeading && <td className="px-3 py-3" />}
                {visible.map((c, i) => (
                  <td key={c.key} className={cn('px-4 py-3', alignRight(c) && 'text-right')}>
                    {c.total
                      // The footer renders through the same cell formatter as the rows, so a
                      // signed column's total carries its Cr / Dr marker too and the footer lines
                      // up with the column above it instead of being formatted a second way.
                      ? formatReportCell(c.type === 'number' ? 'number' : c.type, totals[c.key] ?? 0)
                      : i === 0 ? footerLabel : null}
                  </td>
                ))}
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  );
}
