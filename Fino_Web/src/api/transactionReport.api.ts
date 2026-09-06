import { apiPost } from '@/lib/apiClient';

export interface ReportFilter {
  serviceType?: string;
  status?: string;
  fromDate: string; // yyyy-MM-dd
  toDate: string; // yyyy-MM-dd
  pageNumber?: number;
  pageSize?: number;
  search?: string;
}

/** A single transaction row. The backend view model is wide, so fields are optional. */
export interface TransactionRow {
  TransactionMasterID?: number;
  TxnID?: string;
  UserID?: number;
  FirmName?: string;
  ServiceID?: number;
  ServiceType?: string;
  OperatorName?: string;
  ConsumerNumber?: string;
  Amount?: number;
  DebitAmount?: number;
  Status?: string;
  OperatorRefNo?: string;
  DateTime?: string;
  [key: string]: unknown;
}

/**
 * POST api/TransactionReport/SelectReport. The Data payload may be a bare array
 * or an object wrapping the list — normalise to an array of rows.
 */
export async function selectReport(f: ReportFilter): Promise<TransactionRow[]> {
  const res = await apiPost<unknown>('TransactionReport/SelectReport', {
    UserID: 0,
    ServiceType: f.serviceType ?? '',
    Status: f.status ?? '',
    FromDate: f.fromDate,
    ToDate: f.toDate,
    PageNumber: f.pageNumber ?? 1,
    PageSize: f.pageSize ?? 50,
    IsExcelDownload: false,
    Search: f.search ?? '',
  });
  return extractRows(res.data);
}

function extractRows(data: unknown): TransactionRow[] {
  if (Array.isArray(data)) return data as TransactionRow[];
  if (data && typeof data === 'object') {
    const obj = data as Record<string, unknown>;
    for (const key of ['List', 'list', 'Data', 'data', 'Transactions', 'Records']) {
      if (Array.isArray(obj[key])) return obj[key] as TransactionRow[];
    }
  }
  return [];
}
