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
