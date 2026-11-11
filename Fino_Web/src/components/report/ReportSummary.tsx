import { StatRow } from './StatRow';
import { formatSummaryValue, humanize } from './reportFormat';

export interface ReportSummaryItem {
  label: string;
  value: string;
  /** Colour the figure — used for the success / failure pair most reports carry. */
  tone?: 'default' | 'success' | 'danger' | 'brand';
}

const TONES: Record<string, string> = {
  default: 'text-slate-900',
  success: 'text-success-600',
  danger: 'text-danger-600',
  brand: 'text-brand-700',
};

/**
 * The figures strip that sits above a report's table — totals, counts, the bottom line.
 *
 * Every report that has a summary shows it the same way: one flat row of tiles, label above
 * figure, tabular digits. Reports whose API returns an untyped `HeaderSummary` bag can hand it
 * straight to `summaryFromRecord`, so the strip needs no per-report code at all.
 */
export function ReportSummary({ items, loading }: { items: ReportSummaryItem[]; loading?: boolean }) {
  if (!loading && items.length === 0) return null;

  // One compact bar, not a grid of shadowed cards. Six of those cost about 75px of height and, on
  // a phone, three stacked rows — spent before a single row of the data they summarise is visible.
  // These figures are context for the table; they should not push it off the screen.
  return (
    <StatRow
      loading={loading}
      columns={6}
      items={items.map((item) => ({
        label: item.label,
        value: item.value,
        tone: TONES[item.tone ?? 'default'],
      }))}
    />
  );
}

/**
 * Turn an API summary object into tiles. Only scalar entries are kept — a nested object in a
 * HeaderSummary is structure, not a headline figure, and rendering it would print "[object Object]".
 */
export function summaryFromRecord(summary: Record<string, unknown> | undefined, limit = 12): ReportSummaryItem[] {
  if (!summary) return [];
  return Object.entries(summary)
    .filter(([, v]) => typeof v === 'number' || typeof v === 'string')
    .slice(0, limit)
    .map(([k, v]) => {
      const label = humanize(k);
      const lower = k.toLowerCase();
      const tone: ReportSummaryItem['tone'] =
        lower.includes('success') ? 'success'
        : lower.includes('fail') || lower.includes('refund') ? 'danger'
        : lower.includes('profit') ? 'brand'
        : 'default';
      return { label, value: formatSummaryValue(k, v), tone };
    });
}
