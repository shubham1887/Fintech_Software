import { useState, type ReactNode } from 'react';
import { ChevronRight, Copy, Check } from 'lucide-react';
import { cn } from '@/lib/cn';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * A log table: identifying columns on the row, long payloads (request / response / message body)
 * in the table itself rather than behind a View button.
 *
 * The screens that use this — Provider Log, Sent SMS — exist so somebody can read what we sent and
 * what came back. Putting those payloads in a dialog meant opening a row, reading, closing, opening
 * the next: you could never compare two calls, and the one column that did show a payload was
 * single-line truncated to an ellipsis, which is exactly the part you needed. Here every payload is
 * on the row, wrapped and clamped to a few lines, and one click opens it in full. Nothing is hidden
 * behind a modal and nothing ends in "…".
 */
export interface LogTableColumn<T> {
  label: string;
  /** Extra classes for both the header cell and the body cell (alignment, width). */
  className?: string;
  render: (row: T) => ReactNode;
}

export interface LogTablePayload<T> {
  label: string;
  value: (row: T) => string;
}

interface LogTableProps<T> {
  columns: LogTableColumn<T>[];
  /** The long text blocks shown under each row, in order. */
  payloads: LogTablePayload<T>[];
  rows: T[];
  loading?: boolean;
  rowKey: (row: T, index: number) => string;
  /** Serial number of the first row on this page (1-based). */
  startIndex?: number;
  emptyTitle: string;
  emptyDescription: string;
}

export function LogTable<T>({
  columns, payloads, rows, loading, rowKey, startIndex = 1, emptyTitle, emptyDescription,
}: LogTableProps<T>) {
  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [expandAll, setExpandAll] = useState(false);

  const toggle = (key: string) =>
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(key)) next.delete(key); else next.add(key);
      return next;
    });

  // +2 = the serial column and the expander column.
  const colCount = columns.length + 2;

  return (
    <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
      <div className="flex items-center justify-between border-b border-slate-100 px-4 py-2 text-xs text-slate-500">
        <span>Click a row to open its full {payloads.map((p) => p.label.toLowerCase()).join(' / ')}.</span>
        <Button variant="ghost" size="sm" onClick={() => { setExpandAll((v) => !v); setExpanded(new Set()); }}>
          {expandAll ? 'Collapse all' : 'Expand all'}
        </Button>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
              <th className="w-12 px-4 py-3 text-right">#</th>
              {columns.map((c) => (
                <th key={c.label} className={cn('whitespace-nowrap px-4 py-3', c.className)}>{c.label}</th>
              ))}
              <th className="w-10 px-2 py-3" aria-label="Expand" />
            </tr>
          </thead>
          <tbody>
            {loading ? (
              Array.from({ length: 8 }).map((_, r) => (
                <tr key={r} className="border-b border-slate-100 last:border-0">
                  {Array.from({ length: colCount }).map((__, c) => (
                    <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[120px]" /></td>
                  ))}
                </tr>
              ))
            ) : rows.length === 0 ? (
              <tr>
                <td colSpan={colCount}>
                  <EmptyState title={emptyTitle} description={emptyDescription} />
                </td>
              </tr>
            ) : (
              rows.map((row, i) => {
                const key = rowKey(row, i);
                const open = expandAll || expanded.has(key);
                return (
                  <LogRows
                    key={key}
                    row={row}
                    serial={startIndex + i}
                    columns={columns}
                    payloads={payloads}
                    open={open}
                    colCount={colCount}
                    onToggle={() => toggle(key)}
                  />
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function LogRows<T>({ row, serial, columns, payloads, open, colCount, onToggle }: {
  row: T;
  serial: number;
  columns: LogTableColumn<T>[];
  payloads: LogTablePayload<T>[];
  open: boolean;
  colCount: number;
  onToggle: () => void;
}) {
  return (
    <>
      <tr
        onClick={onToggle}
        className={cn(
          'cursor-pointer border-b border-slate-100 transition-colors hover:bg-brand-50/40',
          open ? 'bg-brand-50/30' : 'even:bg-slate-50/40',
        )}
      >
        <td className="px-4 py-3 text-right tabular-nums text-slate-400">{serial}</td>
        {columns.map((c) => (
          <td key={c.label} className={cn('whitespace-nowrap px-4 py-3 text-slate-600', c.className)}>{c.render(row)}</td>
        ))}
        <td className="px-2 py-3 text-right">
          <ChevronRight className={cn('size-4 text-slate-400 transition-transform', open && 'rotate-90')} />
        </td>
      </tr>
      <tr className={cn('border-b border-slate-100', open ? 'bg-slate-50/60' : 'bg-transparent')}>
        <td colSpan={colCount} className="px-4 pb-3 pt-0">
          <div className={cn('grid gap-3', payloads.length > 1 && 'lg:grid-cols-2')}>
            {payloads.map((p) => (
              <PayloadBlock key={p.label} label={p.label} value={p.value(row)} expanded={open} />
            ))}
          </div>
        </td>
      </tr>
    </>
  );
}

/**
 * One payload. Collapsed it wraps onto three lines (so you read the beginning of the text, not a
 * truncated fragment ending in an ellipsis); expanded it shows everything, scrolling if it is very
 * long. Copy puts the raw text on the clipboard, which is what anyone comparing it against a
 * provider's own logs actually wants.
 */
export function PayloadBlock({ label, value, expanded }: { label: string; value: string; expanded: boolean }) {
  const [copied, setCopied] = useState(false);
  const text = value?.trim() ? value : '—';

  async function copy(e: React.MouseEvent) {
    e.stopPropagation();
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1500);
    } catch {
      // Clipboard is blocked in some embedded browsers — the text is on screen either way.
    }
  }

  return (
    <div className="min-w-0 rounded-lg border border-slate-200 bg-white">
      <div className="flex items-center justify-between border-b border-slate-100 px-3 py-1.5">
        <span className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">{label}</span>
        <button
          type="button"
          onClick={copy}
          title={`Copy ${label.toLowerCase()}`}
          className="grid size-6 place-items-center rounded text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600"
        >
          {copied ? <Check className="size-3.5 text-success-600" /> : <Copy className="size-3.5" />}
        </button>
      </div>
      <pre
        className={cn(
          'whitespace-pre-wrap break-all px-3 py-2 text-xs leading-relaxed text-slate-700',
          expanded ? 'max-h-72 overflow-auto' : 'line-clamp-3',
        )}
      >
        {text}
      </pre>
    </div>
  );
}
