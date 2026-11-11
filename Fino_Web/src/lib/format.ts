import { format as formatDate, parseISO, isValid } from 'date-fns';

/**
 * Money is always shown to exactly two decimals.
 *
 * `maximumFractionDigits` alone let ₹500 render as "₹500" and ₹500.50 as "₹500.5", so in a column
 * of amounts no two decimal points lined up and the eye could not compare magnitudes down the
 * page — the one thing a ledger column exists to allow. Paise are also not cosmetic in a payments
 * system: a commission of ₹0.35 and one of ₹0.3 are different numbers, and rounding the display
 * hides reconciliation differences. Both bounds are pinned.
 */
const inrFmt = new Intl.NumberFormat('en-IN', {
  style: 'currency',
  currency: 'INR',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const numFmt = new Intl.NumberFormat('en-IN');

function toNumber(value: number | string | null | undefined): number {
  if (value == null || value === '') return 0;
  const n = typeof value === 'string' ? Number(value) : value;
  return Number.isFinite(n) ? n : 0;
}

/** True when the API gave us nothing at all — as opposed to a genuine zero. */
function isAbsent(value: number | string | null | undefined): boolean {
  return value == null || value === '' || (typeof value === 'string' && !Number.isFinite(Number(value)));
}

/** Format a value as Indian Rupees, e.g. ₹1,23,456.00 */
export function inr(value: number | string | null | undefined): string {
  return inrFmt.format(toNumber(value));
}

/**
 * Money, or a dash when the figure is genuinely missing.
 *
 * `inr()` turns null into "₹0.00", which in a financial report asserts something the data never
 * said: "this is zero" and "we do not have this" are different facts, and an operator reconciling
 * a statement has to be able to tell them apart. Use this anywhere a blank is possible.
 */
export function inrOrDash(value: number | string | null | undefined): string {
  return isAbsent(value) ? '—' : inrFmt.format(toNumber(value));
}

/**
 * The direction money moved, as banking shows it: magnitude plus Cr / Dr.
 *
 * A bare "-₹500.00" reads as a negative balance rather than as a debit, and a minus sign is easy
 * to lose at the start of a right-aligned column. Returns the parts so the caller can colour and
 * align them.
 */
export function money(value: number | string | null | undefined): {
  text: string;
  amount: number;
  direction: 'credit' | 'debit' | 'zero';
  suffix: 'Cr' | 'Dr' | '';
} {
  const n = toNumber(value);
  const direction = n > 0 ? 'credit' : n < 0 ? 'debit' : 'zero';
  return {
    text: inrFmt.format(Math.abs(n)),
    amount: n,
    direction,
    suffix: direction === 'credit' ? 'Cr' : direction === 'debit' ? 'Dr' : '',
  };
}

/** Group a number Indian-style without a currency symbol. */
export function num(value: number | string | null | undefined): string {
  return numFmt.format(toNumber(value));
}

function asDate(value: string | Date | null | undefined): Date | null {
  if (!value) return null;
  let d = value instanceof Date ? value : parseISO(value);
  // Fallback for non-standard ISO the strict parser rejects (e.g. 7-digit ".fffffff" fractions).
  if (!isValid(d) && typeof value === 'string') d = new Date(value);
  if (!isValid(d)) return null;
  // A .NET DateTime the API never set serialises as 0001-01-01, which parses fine but means
  // "no date" — render it blank rather than as a year-1 timestamp.
  return d.getFullYear() <= 1 ? null : d;
}

/** e.g. 23 Jun 2026, 04:35 PM */
export function dateTime(
  value: string | Date | null | undefined,
  pattern = 'dd MMM yyyy, hh:mm a',
): string {
  const d = asDate(value);
  return d ? formatDate(d, pattern) : '—';
}

/** e.g. 23 Jun 2026 */
export function dateOnly(value: string | Date | null | undefined): string {
  return dateTime(value, 'dd MMM yyyy');
}

/**
 * Date, and the time with it when the value actually carries one - e.g. "14 Oct 2026, 10:00 AM"
 * for a transaction, "14 Oct 2026" for a plain calendar date.
 *
 * Report tables used to print every timestamp through `dateYmd`, which drops the time. A column
 * headed "Date & Time" that shows only a date is not cosmetic in a payments console: the time is
 * how an operator matches a transfer against a customer complaint, a bank UTR or a provider log
 * line, and two transfers to the same account on the same day are indistinguishable without it.
 * A genuinely date-only value lands on midnight and still prints as a plain date, so nothing
 * grows a meaningless "12:00 AM".
 */
export function dateSmart(value: string | Date | null | undefined): string {
  const d = asDate(value);
  if (!d) return '—';
  const hasTime = d.getHours() !== 0 || d.getMinutes() !== 0 || d.getSeconds() !== 0;
  return formatDate(d, hasTime ? 'dd MMM yyyy, hh:mm a' : 'dd MMM yyyy');
}

/** ISO date, no time - e.g. 2026-07-07 */
export function dateYmd(value: string | Date | null | undefined): string {
  return dateTime(value, 'yyyy-MM-dd');
}

/** Request-friendly yyyy-MM-dd. */
export function apiDate(value: Date): string {
  return formatDate(value, 'yyyy-MM-dd');
}

/**
 * "2m", "4h", "3d" — the compact age used in the notification feed, where a full
 * timestamp on every row would drown the text it sits next to. Falls back to the
 * date once something is older than a week, because "37d" stops meaning anything.
 */
export function relativeTime(value: string | Date | null | undefined): string {
  const d = asDate(value);
  if (!d) return '';

  const seconds = Math.floor((Date.now() - d.getTime()) / 1000);
  if (seconds < 60) return 'now';
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m`;
  if (seconds < 86_400) return `${Math.floor(seconds / 3600)}h`;
  if (seconds < 604_800) return `${Math.floor(seconds / 86_400)}d`;
  return dateOnly(d);
}
