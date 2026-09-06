import { format as formatDate, parseISO, isValid } from 'date-fns';

const inrFmt = new Intl.NumberFormat('en-IN', {
  style: 'currency',
  currency: 'INR',
  maximumFractionDigits: 2,
});

const numFmt = new Intl.NumberFormat('en-IN');

function toNumber(value: number | string | null | undefined): number {
  if (value == null || value === '') return 0;
  const n = typeof value === 'string' ? Number(value) : value;
  return Number.isFinite(n) ? n : 0;
}

/** Format a value as Indian Rupees, e.g. ₹1,23,456.00 */
export function inr(value: number | string | null | undefined): string {
  return inrFmt.format(toNumber(value));
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

/** ISO date, no time — e.g. 2026-07-07 */
export function dateYmd(value: string | Date | null | undefined): string {
  return dateTime(value, 'yyyy-MM-dd');
}

/** Request-friendly yyyy-MM-dd. */
export function apiDate(value: Date): string {
  return formatDate(value, 'yyyy-MM-dd');
}
