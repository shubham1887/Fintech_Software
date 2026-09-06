import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { apiDate } from '@/lib/format';
import { isDistributorTier, isRetailer } from '@/lib/roles';
import type { DashboardData, TodaySummaryItem } from '@/types/dashboard';

/** Picks the role-appropriate dashboard endpoint and returns its (encrypted) data. */
export async function getDashboard(userTypeId: number, date: Date): Promise<DashboardData | null> {
  const endpoint = isRetailer(userTypeId)
    ? 'Dashboard/SelectForRetailer'
    : isDistributorTier(userTypeId)
      ? 'Dashboard/SelectForDistributor'
      : 'Dashboard/Select';

  const res = await apiPost<DashboardData>(endpoint, { Date: apiDate(date) });
  return res.data;
}

/** Today's account summary (Opening / Closing / Commission / Surcharge / Fund transferred …). */
export async function getTodaySummary(userTypeId: number, date: Date): Promise<TodaySummaryItem[]> {
  const endpoint = isRetailer(userTypeId) ? 'TodaySummary/SelectForRetailer' : 'TodaySummary/Select';
  const rows = await fetchList(endpoint, { Date: apiDate(date) });
  return rows as unknown as TodaySummaryItem[];
}
