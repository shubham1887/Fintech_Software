import { Button } from '@/components/ui/Button';

export const REPORT_PAGE_SIZES = [10, 20, 50, 100];

/**
 * The pager every report uses: rows-per-page on the left, the range in the middle, Prev / Next on
 * the right. Screens differed on all three before — some had no size control, some no range, some
 * put the buttons on the left — which made paging feel like a different control on every report.
 *
 * Omit `onPageSize` for a report whose page size is fixed (a client-side list, say); the selector
 * is then left out rather than shown disabled.
 */
export function ReportPager({
  page, pageSize, total, loading, onPage, onPageSize, hasMore,
}: {
  /** Zero-based. */
  page: number;
  pageSize: number;
  total: number;
  loading?: boolean;
  onPage: (page: number) => void;
  onPageSize?: (size: number) => void;
  /**
   * For a report whose endpoint sends rows but no total: the page came back full, so there is
   * probably another one. The range then reads "1–20 of 20+" and Next stays reachable, rather
   * than the pager claiming the first page is the whole report.
   */
  hasMore?: boolean;
}) {
  const from = total === 0 ? 0 : page * pageSize + 1;
  const to = Math.min((page + 1) * pageSize, total);

  return (
    <div className="mt-3 flex flex-wrap items-center justify-between gap-3 rounded-card border border-slate-200/70 bg-white px-4 py-2.5 text-sm text-slate-500 shadow-card">
      {onPageSize ? (
        <div className="flex items-center gap-2">
          <span>Rows per page</span>
          <select
            className="rounded-lg border border-slate-200 bg-white px-2 py-1 text-sm text-slate-700 outline-none focus:border-brand-400 focus:ring-2 focus:ring-brand-500/15"
            value={pageSize}
            disabled={loading}
            onChange={(e) => onPageSize(Number(e.target.value))}
          >
            {REPORT_PAGE_SIZES.map((n) => <option key={n} value={n}>{n}</option>)}
          </select>
        </div>
      ) : (
        <span />
      )}

      <span className="tabular-nums">{from}–{to} of {total}{hasMore ? '+' : ''}</span>

      <div className="flex gap-2">
        <Button variant="outline" size="sm" disabled={page === 0 || loading} onClick={() => onPage(page - 1)}>Prev</Button>
        <Button variant="outline" size="sm" disabled={(to >= total && !hasMore) || loading} onClick={() => onPage(page + 1)}>Next</Button>
      </div>
    </div>
  );
}
