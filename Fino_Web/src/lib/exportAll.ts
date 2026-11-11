import { useCallback, useState } from 'react';

import { exportToCsv } from '@/lib/csv';
import { useToast } from '@/components/ui/Toast';

type Row = Record<string, unknown>;

/** One answer from a paged endpoint: the rows of this batch, and the total the filters matched. */
export interface RowBatch<T> {
  rows: T[];
  /** The server's own total for these filters. 0 / absent means "as many as came back". */
  count: number;
}

/** Rows per request while collecting an export. Big enough that 1000 entries is one round trip. */
const BATCH = 1000;
/** A stop so a mis-reported total can never turn an export into an endless request loop. */
const LIMIT = 200_000;

/** Enough of a row to recognise it again, without assuming any particular id column. */
function signature(row: unknown): string {
  try {
    return JSON.stringify(row).slice(0, 400);
  } catch {
    return '';
  }
}

/**
 * Collect EVERY row the current filters match, one batch at a time.
 *
 * An export is not a screenshot of the table: "download" has to mean the whole filtered result
 * set, however many pages the screen cuts it into. Reports page server-side, so the only honest
 * way to build the file is to keep asking for the next batch until the rows in hand reach the
 * total the server reported.
 *
 * Endpoints differ in what they do with paging — some page, some answer with the whole set and a
 * truthful count, a few honour `take` but ignore `skip` — so the loop reads the reply rather than
 * trusting any one behaviour, and reports `complete: false` instead of writing a short file
 * silently.
 */
export async function fetchAllRows<T>(
  fetchBatch: (skip: number, take: number) => Promise<RowBatch<T>>,
  opts: { batch?: number; limit?: number } = {},
): Promise<{ rows: T[]; total: number; complete: boolean }> {
  const batch = Math.max(1, opts.batch ?? BATCH);
  const limit = opts.limit ?? LIMIT;

  const first = await fetchBatch(0, batch);
  const rows = [...first.rows];
  // 0 means the reply carried no total of its own.
  let total = Number(first.count) || 0;

  /**
   * The set is in hand once a batch comes back a different size than it asked for — short means
   * the end of it, and MORE than asked for means the endpoint ignored the window and sent
   * everything it has.
   *
   * The reported total is deliberately not the stop condition, only the figure reported back.
   * Some reports count what they would have returned before a later filter thins the rows, so
   * stopping at that number while batches keep arriving full would end the export in the middle
   * of the data. Asking once more settles it, and costs one empty reply when the number of rows
   * happens to land exactly on a batch boundary.
   */
  const done = (lastBatchSize: number) => lastBatchSize !== batch;

  if (done(first.rows.length)) return { rows, total: Math.max(total, rows.length), complete: true };

  let previousFirst = signature(first.rows[0]);
  let guard = Math.ceil(limit / batch) + 2;
  let complete = false;

  while (rows.length < limit && guard-- > 0) {
    const next = await fetchBatch(rows.length, batch);
    if (next.rows.length === 0) {
      complete = true;
      break;
    }

    // An endpoint that honours `take` but ignores `skip` answers every request with page one.
    // Without this the file would hold that page repeated until the total was reached, which is
    // worse than a short export because it looks complete.
    const nextFirst = signature(next.rows[0]);
    if (nextFirst === previousFirst) break;
    previousFirst = nextFirst;

    rows.push(...next.rows);
    total = Math.max(total, Number(next.count) || 0);
    if (done(next.rows.length)) {
      complete = true;
      break;
    }
  }

  return { rows, total: Math.max(total, rows.length), complete };
}

/**
 * Writes a CSV from rows the screen already holds, and says how many went into the file.
 *
 * For the screens that read their whole result set and then filter in the browser: there is
 * nothing more to fetch, so the only thing missing was the confirmation — a download that comes
 * out short should never be something you discover later in a spreadsheet. Screens whose data is
 * paged server-side use `useCsvExport` instead, which collects the pages first.
 */
export function useCsvDownload() {
  const toast = useToast();
  return useCallback(
    (rows: Row[], filename: string, only?: string[]): void => {
      if (rows.length === 0) {
        toast.error('Nothing to export.');
        return;
      }
      exportToCsv(rows, filename, only);
      toast.success(`Exported ${rows.length} ${rows.length === 1 ? 'row' : 'rows'}.`);
    },
    [toast],
  );
}

export interface CsvExportOptions<T> {
  /** File name, without the extension. */
  filename: string;
  /**
   * Fixed column list. A screen that hides fields by role has to pass this, or the export hands
   * back exactly the fields the screen withheld.
   */
  columns?: string[];
  /** Rows the table is holding — the fallback if the full fetch fails. */
  onScreen?: T[];
  /** Fetches one batch of the full result set. Omit for a screen that already has every row. */
  fetchAll?: (skip: number, take: number) => Promise<RowBatch<T>>;
  /** Every row, for a screen that filters client-side and so already holds the complete set. */
  rows?: T[];
  /**
   * The screen's own client-side narrowing (a search box, a tab, a bucket), applied to the rows
   * the export fetched. Without it a screen that filters in the browser would export every row
   * the server holds and ignore what the user actually filtered to.
   */
  filter?: (rows: T[]) => T[];
  /** Shapes a row into the columns the file should have. */
  map?: (row: T) => Row;
  /**
   * Shapes the whole set at once, for a file whose rows are not the screen's rows — a voucher
   * that writes one line per posting, say. Takes precedence over `map`.
   */
  shape?: (rows: T[]) => Row[];
  /** Rows per request, when an endpoint is unhappy with the default batch. */
  batch?: number;
}

/**
 * The Export button of a report: fetches every matching row, writes the CSV, and says in a toast
 * how many rows went into it.
 *
 * Exports used to be built from whatever the table was holding, so a filtered report of 1000
 * entries downloaded as the 20 rows on screen — and nothing said so. The count in the toast is
 * part of the fix: a file that came out short announces it.
 */
export function useCsvExport() {
  const toast = useToast();
  const [exporting, setExporting] = useState(false);

  const runExport = useCallback(
    async <T,>(o: CsvExportOptions<T>): Promise<void> => {
      setExporting(true);
      try {
        let out: T[] = o.rows ?? o.onScreen ?? [];
        let complete = true;
        // The fetch failing has already been said in a toast of its own; don't say it twice.
        let failed = false;

        if (o.fetchAll) {
          try {
            const all = await fetchAllRows(o.fetchAll, { batch: o.batch });
            complete = all.complete;
            const narrowed = o.filter ? o.filter(all.rows) : all.rows;
            // Never hand back less than the screen already shows: if the export request came back
            // thinner than the table (a stricter scope, a flaky page), the table wins.
            out = narrowed.length >= out.length ? narrowed : out;
          } catch {
            complete = false;
            failed = true;
            toast.error('Could not fetch the full report — exported the rows on screen.');
          }
        }

        if (out.length === 0) {
          toast.error('Nothing to export.');
          return;
        }

        // Without a map or a shape the rows go out as they came in — a screen holding typed rows
        // (rather than loose records) passes one of the two.
        const csvRows = o.shape ? o.shape(out) : o.map ? out.map(o.map) : (out as unknown as Row[]);
        if (csvRows.length === 0) {
          toast.error('Nothing to export.');
          return;
        }
        exportToCsv(csvRows, o.filename, o.columns);
        if (complete) {
          toast.success(`Exported ${csvRows.length} ${csvRows.length === 1 ? 'row' : 'rows'}.`);
        } else if (!failed) {
          toast.error(`Exported ${csvRows.length} rows — the report has more than the export could fetch.`);
        }
      } finally {
        setExporting(false);
      }
    },
    [toast],
  );

  return { exporting, runExport };
}
