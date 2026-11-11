import { Link } from 'react-router-dom';
import { ChevronRight } from 'lucide-react';
import type { TransactionRow } from '@/api/transactionReport.api';
import type { ActivityStyle } from '@/theme/mobileLayouts';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';
import { txnSubtitle, txnTitle, txnWhen, type ServiceNames } from '@/lib/txn';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * Recent transactions, in the four shapes a preset can ask for.
 *
 * The status colour is the one constant: green / amber / red carry the same meaning in all
 * four, because that is what a retailer reads first and it must not change meaning when the
 * operator changes the layout.
 */

export interface ActivityProps {
  style: ActivityStyle;
  rows: TransactionRow[];
  loading: boolean;
  names: ServiceNames;
}

export function ActivityBlock(props: ActivityProps) {
  const body = () => {
    switch (props.style) {
      case 'compact':
        return <CompactRows {...props} />;
      case 'timeline':
        return <TimelineRows {...props} />;
      case 'table':
        return <TableRows {...props} />;
      default:
        return <ListRows {...props} />;
    }
  };

  return (
    <section>
      <div className="mb-2 flex items-center justify-between px-1">
        <h2 className="text-[11px] font-semibold uppercase tracking-wider text-slate-400">Recent</h2>
        <Link to="/transactions" onClick={() => haptic()} className="flex items-center gap-0.5 text-xs font-medium text-brand-600">
          See all <ChevronRight className="size-3.5" />
        </Link>
      </div>
      {body()}
    </section>
  );
}

function statusTone(status?: string): { dot: string; text: string; bg: string } {
  const s = (status ?? '').toLowerCase();
  if (s.includes('success')) return { dot: 'bg-success-500', text: 'text-success-600', bg: 'bg-success-50' };
  if (s.includes('fail') || s.includes('reject')) return { dot: 'bg-danger-500', text: 'text-danger-600', bg: 'bg-danger-50' };
  return { dot: 'bg-warning-500', text: 'text-warning-600', bg: 'bg-warning-50' };
}

function rowKey(r: TransactionRow, i: number): string | number {
  return r.TransactionMasterID ?? r.TxnID ?? i;
}

/** One card shell so all four variants share the same border, radius and empty state. */
function Panel({
  loading, empty, rows, children, skeletonHeight = 'h-9',
}: {
  loading: boolean;
  empty: boolean;
  rows: number;
  children: React.ReactNode;
  skeletonHeight?: string;
}) {
  return (
    <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
      {loading ? (
        <div className="space-y-3 p-4">
          {Array.from({ length: rows }).map((_, i) => <Skeleton key={i} className={cn(skeletonHeight, 'rounded-lg')} />)}
        </div>
      ) : empty ? (
        <p className="py-10 text-center text-sm text-slate-400">No transactions today yet.</p>
      ) : (
        children
      )}
    </div>
  );
}

/* ----------------------------------------------------------------- list */

/** Dot, title, secondary line, amount + status. The readable default. */
function ListRows({ rows, loading, names }: ActivityProps) {
  return (
    <Panel loading={loading} empty={rows.length === 0} rows={4}>
      {rows.map((r, i) => {
        const tone = statusTone(r.Status);
        const subtitle = txnSubtitle(r, names);
        return (
          <div key={rowKey(r, i)} className={cn('flex items-center gap-3 px-4 py-3', i > 0 && 'border-t border-slate-100')}>
            <span className={cn('size-2 shrink-0 rounded-full', tone.dot)} />
            <span className="min-w-0 flex-1">
              <span className="block truncate text-sm font-medium text-slate-800">{txnTitle(r, names)}</span>
              <span className="block truncate text-xs text-slate-400">
                {[subtitle, txnWhen(r.DateTime)].filter(Boolean).join(' · ')}
              </span>
            </span>
            <span className="shrink-0 text-right">
              <span className="block font-display text-sm font-bold tabular-nums text-slate-900">{inr(r.Amount)}</span>
              <span className={cn('block text-[11px] font-medium', tone.text)}>{r.Status}</span>
            </span>
          </div>
        );
      })}
    </Panel>
  );
}

/* -------------------------------------------------------------- compact */

/**
 * One line per transaction: everything on a single row with the status as a coloured bar on
 * the left edge. Fits roughly half again as many rows in the same height as `list`.
 */
function CompactRows({ rows, loading, names }: ActivityProps) {
  return (
    <Panel loading={loading} empty={rows.length === 0} rows={6} skeletonHeight="h-5">
      {rows.map((r, i) => {
        const tone = statusTone(r.Status);
        return (
          <div
            key={rowKey(r, i)}
            className={cn('flex items-center gap-2.5 py-2 pl-0 pr-3', i > 0 && 'border-t border-slate-100')}
          >
            <span className={cn('h-7 w-[3px] shrink-0 rounded-r', tone.dot)} />
            <span className="min-w-0 flex-1 truncate text-[13px] font-medium text-slate-800">{txnTitle(r, names)}</span>
            <span className="shrink-0 text-[11px] tabular-nums text-slate-400">{txnWhen(r.DateTime)}</span>
            <span className="shrink-0 font-display text-[13px] font-bold tabular-nums text-slate-900">{inr(r.Amount)}</span>
          </div>
        );
      })}
    </Panel>
  );
}

/* ------------------------------------------------------------- timeline */

/**
 * A hairline rail down the left with a node per transaction, time first.
 *
 * Chronology is the organising idea here rather than the amount — it suits the minimal preset,
 * where the question is "what happened today", not "how much came in".
 */
function TimelineRows({ rows, loading, names }: ActivityProps) {
  if (loading) {
    return (
      <div className="space-y-3 py-1">
        {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-10 rounded-lg" />)}
      </div>
    );
  }

  if (rows.length === 0) {
    return <p className="py-10 text-center text-sm text-slate-400">No transactions today yet.</p>;
  }

  return (
    <div className="relative pl-5">
      {/* The rail. Stops short of the last node so it does not dangle past the list. */}
      <span className="absolute bottom-3 left-[5px] top-2 w-px bg-slate-200" aria-hidden />
      {rows.map((r, i) => {
        const tone = statusTone(r.Status);
        const subtitle = txnSubtitle(r, names);
        return (
          <div key={rowKey(r, i)} className={cn('relative', i > 0 && 'mt-3.5')}>
            <span className={cn('absolute -left-5 top-1.5 size-[11px] rounded-full ring-[3px] ring-canvas', tone.dot)} />
            <div className="flex items-baseline justify-between gap-3">
              <span className="min-w-0 flex-1">
                <span className="block truncate text-sm font-medium text-slate-800">{txnTitle(r, names)}</span>
                <span className="block truncate text-xs text-slate-400">
                  {[txnWhen(r.DateTime), subtitle].filter(Boolean).join(' · ')}
                </span>
              </span>
              <span className="shrink-0 text-right">
                <span className="block font-display text-sm font-bold tabular-nums text-slate-900">{inr(r.Amount)}</span>
                <span className={cn('block text-[11px]', tone.text)}>{r.Status}</span>
              </span>
            </div>
          </div>
        );
      })}
    </div>
  );
}

/* ---------------------------------------------------------------- table */

/**
 * Aligned columns with a header row — time, what, amount, status.
 *
 * Back-office reading: when every row lines up you can scan one column at a time, which is how
 * someone reconciling a day actually reads. The amount column is right-aligned and tabular so
 * the digits stack.
 */
function TableRows({ rows, loading, names }: ActivityProps) {
  return (
    <Panel loading={loading} empty={rows.length === 0} rows={6} skeletonHeight="h-6">
      <table className="w-full table-fixed border-collapse text-left">
        <thead>
          <tr className="border-b border-slate-100 bg-slate-50/70">
            <th className="w-[66px] px-2.5 py-2 text-[10px] font-semibold uppercase tracking-wide text-slate-400">Time</th>
            <th className="px-1 py-2 text-[10px] font-semibold uppercase tracking-wide text-slate-400">Transaction</th>
            <th className="w-[86px] px-3 py-2 text-right text-[10px] font-semibold uppercase tracking-wide text-slate-400">
              Amount
            </th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r, i) => {
            const tone = statusTone(r.Status);
            const subtitle = txnSubtitle(r, names);
            return (
              <tr key={rowKey(r, i)} className={cn(i > 0 && 'border-t border-slate-100')}>
                {/* The timestamp must not wrap — a column that breaks "10:49 AM" across two
                    lines costs a row of height on every row in the table. */}
                <td className="whitespace-nowrap px-2.5 py-2.5 align-top text-[11px] tabular-nums text-slate-400">
                  {txnWhen(r.DateTime)}
                </td>
                <td className="min-w-0 px-1 py-2.5 align-top">
                  <span className="block truncate text-[13px] font-medium text-slate-800">{txnTitle(r, names)}</span>
                  {subtitle && <span className="block truncate text-[11px] text-slate-400">{subtitle}</span>}
                </td>
                <td className="px-3 py-2.5 text-right align-top">
                  <span className="block font-display text-[13px] font-bold tabular-nums text-slate-900">{inr(r.Amount)}</span>
                  <span className={cn('mt-0.5 inline-block rounded px-1 py-px text-[10px] font-medium', tone.bg, tone.text)}>
                    {r.Status}
                  </span>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </Panel>
  );
}
