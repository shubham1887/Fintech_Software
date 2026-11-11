import { useQuery } from '@tanstack/react-query';
import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';
import { useMyStanding } from '@/screens/MyOutstandingCard';

/**
 * The two outstanding figures a dashboard carries, and they are not the same number:
 *
 *  • PAYABLE — what the signed-in user owes their own upline for funds taken on credit.
 *  • RECEIVABLE — what their downline owes them.
 *
 * A retailer only ever has the first; admin only ever has the second; a distributor has both at
 * once, and conflating them would net a debt they owe against money owed to them, which is the
 * one arithmetic nobody wants on a dashboard.
 *
 * This is a hook and not a card on purpose. Outstanding belongs beside the wallet balances, in
 * the same strip, because it is read in the same glance as them — "what do I hold, and what of it
 * is owed". A panel of its own pushed the page around and said more than the figure is worth.
 */
export interface OutstandingPosition {
  /** What the signed-in user owes upwards. */
  payable: number;
  /** Who they owe it to, when that is known. */
  payableTo: string;
  /** Their credit limit, 0 when they have none. */
  limit: number;
  /** What their downline owes them. 0 unless `showReceivable`. */
  receivable: number;
  /** How many downline accounts actually carry a balance. */
  accounts: number;
  loading: boolean;
}

export function useOutstandingPosition(showReceivable: boolean): OutstandingPosition {
  const mine = useMyStanding();

  const receivable = useQuery({
    queryKey: ['outstanding-receivable'],
    enabled: showReceivable,
    staleTime: 60_000,
    queryFn: async () => {
      const res = await apiPost<Record<string, unknown>>('CashCollection/OutstandingReport', {
        UserID: 0, SearchText: '', TakeCount: 0, SkipCount: 0, IsExcelDownload: false,
      });
      if (!res.ok || !res.data) return { total: 0, accounts: 0 };
      const d = res.data as Record<string, unknown>;
      const rows = getField(d, 'data', 'Data');
      const list = Array.isArray(rows) ? (rows as Record<string, unknown>[]) : [];
      return {
        total: Number(getField(d, 'TotalOutstanding') ?? 0),
        // Only accounts actually carrying a balance — the report also lists accounts that merely
        // have a limit, and counting those as debtors would overstate it.
        accounts: list.filter((r) => Number(getField(r, 'Outstanding') ?? 0) > 0).length,
      };
    },
  });

  return {
    payable: mine.data?.outstanding ?? 0,
    payableTo: mine.data?.parentName ?? '',
    limit: mine.data?.limit ?? 0,
    receivable: showReceivable ? (receivable.data?.total ?? 0) : 0,
    accounts: showReceivable ? (receivable.data?.accounts ?? 0) : 0,
    loading: mine.isLoading || (showReceivable && receivable.isLoading),
  };
}
