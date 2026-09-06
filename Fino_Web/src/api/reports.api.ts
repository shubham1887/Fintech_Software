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

export function defaultReportFilters(): ReportFilters {
  return {
    fromDate: apiDate(subDays(new Date(), 7)),
    toDate: apiDate(new Date()),
    status: '',
    serviceType: '',
    search: '',
  };
}

/** Generic report fetch — every TransactionReport endpoint takes the same TxnReportRequest shape. */
export async function fetchReport(endpoint: string, f: ReportFilters): Promise<ReportRow[]> {
  const res = await apiPost<unknown>(endpoint, {
    UserID: 0,
    ServiceType: f.serviceType,
    Status: f.status,
    FromDate: f.fromDate,
    ToDate: f.toDate,
    PageNumber: 1,
    PageSize: 200,
    IsExcelDownload: false,
    Search: f.search,
  });
  // Business-level FAIL ("No record found", or a not-yet-implemented endpoint) → empty,
  // so the screen shows a clean empty state. Transport/auth errors throw inside apiPost.
  if (!res.ok) return [];
  return normalizeRows(res.data);
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
