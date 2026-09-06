import { useMemo, type ReactNode } from 'react';
import { cn } from '@/lib/cn';
import { dateYmd, inr } from '@/lib/format';
import { StatusBadge } from './Badge';
import { useIsMobileApp } from '@/lib/native';
import { EmptyState } from './EmptyState';
import { MobileRecordList } from './MobileRecordList';
import { Skeleton } from './Skeleton';

type Row = Record<string, unknown>;

const MONEY = /(amount|amt|balance|commission|charge|tds|gst|surcharge|opening|closing|credit|debit|price|\bfee\b|payable|discount|profit)/i;
const DATEKEY = /(date|time)/i;
const HIDE = /(password|hash|token|isdeleted|^id$|guid|secret)/i;

/** "TransactionMasterID" → "Transaction Master ID" */
function humanize(key: string): string {
  return key
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/_/g, ' ')
    .replace(/\bI D\b/g, 'ID')
    .replace(/\s+/g, ' ')
    .trim()
    .replace(/^\w/, (c) => c.toUpperCase());
}

function formatCell(key: string, value: unknown): ReactNode {
  if (value === null || value === undefined || value === '') return <span className="text-slate-300">—</span>;
  if (/status/i.test(key) && typeof value === 'string') return <StatusBadge status={value} />;
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  // Identifier columns (e.g. VirtualBalanceID, ServiceID) are plain numbers — never money,
  // even though "…Balance ID" would otherwise match the money keyword list.
  if (/id$/i.test(key)) return String(value);
  if (MONEY.test(key) && value !== '' && !Number.isNaN(Number(value))) return inr(value as number);
  if (DATEKEY.test(key) && (typeof value === 'string' || value instanceof Date)) return dateYmd(value as string);
  // Any ISO date-time value (e.g. a "DOC" column) — format it even when the column name
  // doesn't contain "date"/"time", so raw "2026-07-07T20:03:40.7171791" never leaks through.
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(value)) return dateYmd(value);
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

/** Renders any array of objects as a table, deriving columns from the data keys. */
export function DynamicTable({
  rows,
  loading = false,
  maxColumns = 14,
  emptyTitle = 'No records found',
  emptyDescription,
  actions,
  columns,
}: {
  rows: Row[];
  loading?: boolean;
  maxColumns?: number;
  emptyTitle?: string;
  emptyDescription?: string;
  actions?: (row: Row) => ReactNode;
  /** Explicit columns (key + header label). When set, auto-derivation is skipped.
   *  `render` overrides the default cell for that column (e.g. an image thumbnail). */
  columns?: { key: string; label: string; render?: (row: Row) => ReactNode }[];
}) {
  const keys = useMemo(() => {
    if (columns && columns.length) return columns.map((c) => c.key);
    const seen: string[] = [];
    for (const row of rows.slice(0, 8)) {
      for (const k of Object.keys(row)) {
        if (!seen.includes(k) && !HIDE.test(k)) seen.push(k);
      }
    }
    return seen.slice(0, maxColumns);
  }, [rows, maxColumns, columns]);

  const labelFor = (k: string) => columns?.find((c) => c.key === k)?.label ?? humanize(k);
  const renderFor = (k: string) => columns?.find((c) => c.key === k)?.render;
  const isMoney = (k: string) => MONEY.test(k);
  const colCount = Math.max(keys.length, 1) + (actions ? 1 : 0);
  const isMobile = useIsMobileApp();

  // Same rows as cards on a phone — a 14-column report is unreadable as a table there.
  if (isMobile) {
    return (
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {loading ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-12 rounded-lg" />)}
          </div>
        ) : rows.length === 0 ? (
          <EmptyState title={emptyTitle} description={emptyDescription} />
        ) : (
          <MobileRecordList
              rows={rows as unknown[]}
              actions={actions as ((row: unknown) => ReactNode) | undefined}
              fields={keys.map((k) => ({
                key: k,
                label: labelFor(k),
                render: (row: unknown) => {
                  const r = row as Row;
                  const custom = renderFor(k);
                  return custom ? custom(r) : formatCell(k, r[k]);
                },
              }))}
          />
        )}
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-slate-200 bg-slate-50/80">
              {(keys.length ? keys : ['—']).map((k) => (
                <th
                  key={k}
                  className={cn(
                    'whitespace-nowrap px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-500',
                    isMoney(k) ? 'text-right' : 'text-left',
                  )}
                >
                  {keys.length ? labelFor(k) : ''}
                </th>
              ))}
              {actions && (
                <th className="whitespace-nowrap px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-500">
                  Actions
                </th>
              )}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              Array.from({ length: 8 }).map((_, r) => (
                <tr key={r} className="border-b border-slate-100 last:border-0">
                  {Array.from({ length: colCount }).map((__, c) => (
                    <td key={c} className="px-4 py-3">
                      <Skeleton className="h-4 w-full max-w-[120px]" />
                    </td>
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
              rows.map((row, i) => (
                <tr key={i} className="border-b border-slate-100 transition-colors last:border-0 even:bg-slate-50/40 hover:bg-brand-50/40">
                  {keys.map((k) => {
                    const custom = renderFor(k);
                    return (
                      <td
                        key={k}
                        className={cn('whitespace-nowrap px-4 py-3 text-slate-700', isMoney(k) && !custom && 'text-right tabular-nums')}
                      >
                        {custom ? custom(row) : formatCell(k, row[k])}
                      </td>
                    );
                  })}
                  {actions && <td className="whitespace-nowrap px-4 py-3 text-right">{actions(row)}</td>}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
      {!loading && rows.length > 0 && (
        <div className="flex items-center justify-between border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
          <span>
            Showing <span className="font-semibold text-slate-700 tabular-nums">{rows.length}</span>{' '}
            {rows.length === 1 ? 'record' : 'records'}
          </span>
          <span className="hidden sm:inline">{keys.length} columns</span>
        </div>
      )}
    </div>
  );
}
