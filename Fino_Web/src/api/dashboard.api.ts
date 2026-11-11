import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { apiDate } from '@/lib/format';
import { isAdminTier, isDistributorTier } from '@/lib/roles';
import type { DashboardData, TodaySummaryItem } from '@/types/dashboard';

/**
 * Picks the role-appropriate dashboard endpoint and returns its (encrypted) data.
 *
 * Only the operator roles get `Dashboard/Select` — that is the PLATFORM-WIDE report: every
 * user's transactions for the day, the whole network's float, provider stock. It used to be
 * the fallback for anything that was not a retailer or a distributor, so an API user, a client
 * or a field officer opened their dashboard and read the panel's turnover in the "Today's sale"
 * and "Failed" tiles — a brand-new account that had never transacted still showed figures.
 * Every remaining role gets the retailer variant, which is the plain own-account view.
 * The API decides this from the signed token as well, so an old cached build cannot leak
 * the panel figures either (same fix as TodaySummary below).
 */
export async function getDashboard(userTypeId: number, date: Date): Promise<DashboardData | null> {
  const endpoint = isAdminTier(userTypeId)
    ? 'Dashboard/Select'
    : isDistributorTier(userTypeId)
      ? 'Dashboard/SelectForDistributor'
      : 'Dashboard/SelectForRetailer';

  const res = await apiPost<DashboardData>(endpoint, { Date: apiDate(date) });
  return res.data;
}

/**
 * Today's account summary (Opening / Closing / Commission / Surcharge / Fund transferred …).
 *
 * Only the operator roles get the panel-wide report; every other role gets their OWN wallet.
 * This used to send everything except a retailer to `TodaySummary/Select`, which is the
 * platform-wide report — so a distributor or an API user saw the whole network's float and
 * fund movement on their dashboard, under the heading "your account movement for today".
 * The API now decides this from the signed token as well, so an old cached build cannot
 * leak the panel figures either.
 */
export async function getTodaySummary(userTypeId: number, date: Date): Promise<TodaySummaryItem[]> {
  const endpoint = isAdminTier(userTypeId)
    ? 'TodaySummary/Select'
    : isDistributorTier(userTypeId)
      ? 'TodaySummary/SelectForDistributor'
      : 'TodaySummary/SelectForRetailer';
  const rows = await fetchList(endpoint, { Date: apiDate(date) });
  return rows as unknown as TodaySummaryItem[];
}

/** Outcome of one provider's live balance enquiry. */
export interface ProviderBalanceResult {
  ProviderID: number;
  ProviderName: string;
  Status: string;
  Message: string;
  RechargeBalance: number | null;
  DMTBalance: number | null;
}

/**
 * Re-enquire every active provider's balance and persist what comes back.
 *
 * "Provider Stock" on the dashboard shows the balance stored on each provider row, which is only
 * written when something actually calls that provider — so between calls it drifts, and refreshing
 * the page just re-read the same stale numbers. This asks the providers themselves, which is what
 * the refresh button should have been doing. It is one HTTP round-trip per provider, so it is
 * driven by the button rather than by the background poll.
 */
export async function refreshProviderBalances(): Promise<ProviderBalanceResult[]> {
  const res = await apiPost<ProviderBalanceResult[]>('ProviderGateway/RefreshAllBalances', {});
  if (!res.ok || !Array.isArray(res.data)) return [];
  return res.data;
}
