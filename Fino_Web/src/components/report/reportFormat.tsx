import type { ReactNode } from 'react';
import { inr, money, dateSmart, dateTime } from '@/lib/format';
import { cn } from '@/lib/cn';
import { StatusBadge } from '@/components/ui/Badge';

/**
 * How a report cell is rendered. One list, used by every report in the product, so a money column
 * looks and aligns the same whether it is on the Recharge report, the Provider Daybook or a
 * hand-built screen.
 */
export type ReportCellType =
  | 'text'
  | 'money'
  /** Money whose sign is meaningful — rendered with direction (Cr / Dr) and colour. */
  | 'signedMoney'
  /**
   * Money whose sign is meaningful, shown as a plain + or − rather than Cr / Dr.
   *
   * For the commission columns on the sale summaries. Those columns mix two different things in
   * one figure — commission the tier EARNED and a surcharge it was CHARGED — and the question
   * being asked of them is "did this service make me money or cost me money". A "Dr" suffix
   * answers that in accounting terms; "−₹2,592.74" answers it in the terms the person reading a
   * sales report is thinking in.
   */
  | 'plusMinusMoney'
  | 'number'
  | 'date'
  | 'datetime'
  | 'status'
  | 'bold'
  | 'image'
  | 'serial';

/** Columns whose values are right-aligned, because the eye compares digits by their last one. */
export const isNumericType = (type?: ReportCellType) =>
  type === 'money' || type === 'signedMoney' || type === 'plusMinusMoney' || type === 'number' || type === 'serial';

/** "ClosingBalance" → "Closing Balance"; "TotalTDS" → "Total TDS". */
export function humanize(key: string): string {
  return key
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/_/g, ' ')
    .replace(/\bI D\b/g, 'ID')
    .trim();
}

/**
 * Keys that hold money, for reports whose summary block arrives as an untyped bag of numbers
 * (HeaderSummary). Matching on the name is a heuristic, but it is the only signal the API gives —
 * and showing ₹ on a count is a smaller error than showing a raw float for a balance.
 */
export const MONEYISH =
  // "margin" and "income" are money too. Missing from this list, the P&L strip printed its margin
  // as a bare number beside seven figures carrying a rupee sign, which reads as a count.
  /(amount|amt|balance|commission|charge|tds|gst|surcharge|opening|closing|credit|debit|sale|purchase|profit|margin|income|hold|value)/i;

const ISO_DATETIME = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/;

/**
 * Render one value for a report cell. Empty values become a dash rather than a blank gap, so a
 * missing figure is visibly missing instead of looking like the column ran out.
 */
export function formatReportCell(type: ReportCellType | undefined, value: unknown, serial?: number): ReactNode {
  if (type === 'serial') return <span className="tabular-nums text-slate-500">{serial}</span>;
  if (value === null || value === undefined || value === '') return <span className="text-slate-300">—</span>;

  switch (type) {
    case 'status':
      return <StatusBadge status={String(value)} />;
    case 'money':
      return <span className="tabular-nums">{inr(Number(value) || 0)}</span>;
    case 'signedMoney': {
      // Direction shown as Cr / Dr rather than a leading minus: a "-" at the head of a
      // right-aligned column is the easiest character on the page to miss, and it reads as a
      // negative balance rather than as money going out.
      const m = money(value as number);
      return (
        <span
          className={cn(
            'tabular-nums',
            m.direction === 'credit' && 'text-success-700',
            m.direction === 'debit' && 'text-danger-700',
            m.direction === 'zero' && 'text-slate-400',
          )}
        >
          {m.text}
          {m.suffix && <span className="ml-1 text-[10px] font-semibold uppercase opacity-70">{m.suffix}</span>}
        </span>
      );
    }
    case 'plusMinusMoney': {
      // A commission column on a sale summary is two things at once: a positive figure is margin
      // earned, a negative one is a surcharge the tier paid. Printed with the sign in front, in
      // the colour that says which, so a loss-making service is visible at a glance.
      const n = Number(value) || 0;
      const sign = n > 0 ? '+' : n < 0 ? '-' : '';
      return (
        <span
          className={cn(
            'tabular-nums',
            n > 0 && 'text-success-700',
            n < 0 && 'text-danger-700',
            n === 0 && 'text-slate-400',
          )}
        >
          {sign}{inr(Math.abs(n))}
        </span>
      );
    }
    case 'number':
      return <span className="tabular-nums">{String(value)}</span>;
    case 'date':
      // Date AND time when the row carries one (see dateSmart). One line, so a column of
      // timestamps stays scannable instead of wrapping mid-value.
      return <span className="whitespace-nowrap">{dateSmart(String(value))}</span>;
    case 'datetime':
      return <span className="whitespace-nowrap">{dateTime(String(value))}</span>;
    case 'bold':
      return <span className="font-medium text-slate-800">{String(value)}</span>;
    case 'image':
      return (
        <img
          src={String(value)}
          alt=""
          className="h-7 w-7 rounded object-contain"
          onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = 'none'; }}
        />
      );
    default:
      // An untyped column holding an ISO timestamp — format it rather than leak the raw string.
      if (typeof value === 'string' && ISO_DATETIME.test(value)) return dateSmart(value);
      return String(value);
  }
}

/** Format a summary-strip value: money keys get ₹, everything else is shown as-is. */
export function formatSummaryValue(key: string, value: unknown): string {
  if (typeof value === 'number' && MONEYISH.test(key)) return inr(value);
  return String(value);
}
