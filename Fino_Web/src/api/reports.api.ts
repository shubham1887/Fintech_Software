import { apiPost } from '@/lib/apiClient';
import { apiDate } from '@/lib/format';
import { subDays } from 'date-fns';

export interface ReportFilters {
  fromDate: string;
  toDate: string;
  status: string;
  serviceType: string;
  search: string;
}

export type ReportRow = Record<string, unknown>;

/** One page of a report: the rows to show, and the total the filters matched. */
export interface ReportPage {
  rows: ReportRow[];
  /**
   * Rows these filters matched. Reports that answer with a total of their own report it exactly;
   * for the ones that don't, this is what has been seen so far (see `hasMore`).
   */
  count: number;
  /** A full page and no total with it — there may be another page behind this one. */
  hasMore: boolean;
}

export function defaultReportFilters(): ReportFilters {
  return {
    fromDate: apiDate(subDays(new Date(), 7)),
    toDate: apiDate(new Date()),
    status: '',
    serviceType: '',
    search: '',
  };
}

/**
 * Generic report fetch — every TransactionReport endpoint takes the same TxnReportRequest shape.
 *
 * `skip` / `take` are sent twice over, as TakeCount/SkipCount and as PageNumber/PageSize: the
 * reports are not consistent about which pair they read, and sending both means the page the
 * screen asks for is the page it gets either way.
 *
 * `forExport` asks for the whole filtered result set rather than a page. Reports that understand
 * `IsExcelDownload` answer with everything at once; the ones that page regardless are collected
 * batch by batch by the caller (see `fetchAllRows`), which is why the window is still sent.
 */
export async function fetchReportPage(
  endpoint: string,
  f: ReportFilters,
  skip: number,
  take: number,
  forExport = false,
): Promise<ReportPage> {
  const res = await apiPost<unknown>(endpoint, {
    UserID: 0,
    ServiceType: f.serviceType,
    Status: f.status,
    FromDate: f.fromDate,
    ToDate: f.toDate,
    TakeCount: take,
    SkipCount: skip,
    PageNumber: Math.floor(skip / Math.max(1, take)) + 1,
    PageSize: take,
    IsExcelDownload: forExport,
    Search: f.search,
  });
  // Business-level FAIL ("No record found", or a not-yet-implemented endpoint) → empty,
  // so the screen shows a clean empty state. Transport/auth errors throw inside apiPost.
  if (!res.ok) return { rows: [], count: 0, hasMore: false };

  const all = normalizeRows(res.data);
  const reported = reportedCount(res.data);

  // Not every report pages server-side; several answer with the whole set and a truthful total.
  // Fewer rows than the total means this IS a page of a larger set and is passed through; rows
  // equal to the total means everything arrived, so the window is cut here instead — except for
  // an export, which asked for all of it on purpose.
  const serverPaged = reported !== undefined && all.length < reported;
  const rows = forExport || serverPaged ? all : all.slice(skip, skip + take);

  // A reply with no total of its own says nothing about what lies past it, so a page that came
  // back exactly full is treated as "there is probably more": the pager keeps Next alive and the
  // export keeps asking for the next batch instead of stopping at a round number.
  // More rows than the window asked for means the endpoint ignored paging and sent the whole set,
  // so its length IS the total and there is nothing behind it.
  const sentEverything = all.length > take;
  const hasMore = reported === undefined && !sentEverything && all.length >= take;
  const count = reported ?? (sentEverything ? all.length : skip + all.length);
  // The export loop needs the honest total — 0 for "unknown" — so that a full batch with no total
  // behind it does not read as the end of the set.
  return { rows, count: forExport ? reported ?? (sentEverything ? all.length : 0) : count, hasMore };
}

/** The Data payload may be a bare array or an object wrapping the list — normalise to rows. */
function normalizeRows(data: unknown): ReportRow[] {
  if (Array.isArray(data)) return data as ReportRow[];
  if (data && typeof data === 'object') {
    const obj = data as Record<string, unknown>;
    for (const key of ['List', 'list', 'Data', 'data', 'Items', 'Records', 'Result', 'rows']) {
      if (Array.isArray(obj[key])) return obj[key] as ReportRow[];
    }
    const firstArray = Object.values(obj).find((v) => Array.isArray(v));
    if (firstArray) return firstArray as ReportRow[];
  }
  return [];
}

/** The server's own total for the filters, when the reply carries one. */
function reportedCount(data: unknown): number | undefined {
  if (!data || typeof data !== 'object' || Array.isArray(data)) return undefined;
  const obj = data as Record<string, unknown>;
  for (const key of ['Count', 'count', 'TotalCount', 'totalCount', 'TotalRecords']) {
    const v = obj[key];
    if (typeof v === 'number') return v;
    if (typeof v === 'string' && v.trim() !== '' && !Number.isNaN(Number(v))) return Number(v);
  }
  return undefined;
}
