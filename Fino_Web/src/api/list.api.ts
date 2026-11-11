import { apiPost } from '@/lib/apiClient';

export type ListRow = Record<string, unknown>;

/**
 * Fetches a master-data list from a `<Master>/GetAll` endpoint. The backend returns
 * a FAIL ("No record found") for empty sets — we treat that as an empty list, not an
 * error. Real transport/auth failures still throw (handled by React Query).
 */
export async function fetchList(endpoint: string, payload: Record<string, unknown>): Promise<ListRow[]> {
  const res = await apiPost<unknown>(endpoint, payload);
  if (!res.ok) return [];
  const data = res.data;
  if (Array.isArray(data)) return data as ListRow[];
  if (data && typeof data === 'object') {
    const arr = Object.values(data as Record<string, unknown>).find((v) => Array.isArray(v));
    if (arr) return arr as ListRow[];
  }
  return [];
}

/** One window of a list endpoint: the rows, and the total when the reply carries one. */
export interface ListPage {
  rows: ListRow[];
  /** 0 when the endpoint sends no total — the caller then reads the batch sizes instead. */
  count: number;
}

/**
 * Fetches a window of a list endpoint, for an export that has to collect every row.
 *
 * The list screens read one generous page and then search, filter and export in the browser, so
 * without this the export could only ever hold what that page happened to contain. Both paging
 * conventions are sent (TakeCount/SkipCount and PageNumber/PageSize) because the endpoints are
 * not consistent about which they read, and the ones that read neither simply answer with
 * everything — which the caller recognises from the size of the batch.
 */
export async function fetchListPage(
  endpoint: string,
  payload: Record<string, unknown>,
  skip: number,
  take: number,
): Promise<ListPage> {
  const res = await apiPost<unknown>(endpoint, {
    ...payload,
    TakeCount: take,
    SkipCount: skip,
    PageNumber: Math.floor(skip / Math.max(1, take)) + 1,
    PageSize: take,
  });
  if (!res.ok) return { rows: [], count: 0 };
  const data = res.data;
  if (Array.isArray(data)) return { rows: data as ListRow[], count: 0 };
  if (data && typeof data === 'object') {
    const obj = data as Record<string, unknown>;
    const arr = Object.values(obj).find((v) => Array.isArray(v));
    const total = [obj.Count, obj.count, obj.TotalCount].find((v) => typeof v === 'number');
    return { rows: (arr ?? []) as ListRow[], count: typeof total === 'number' ? total : 0 };
  }
  return { rows: [], count: 0 };
}
