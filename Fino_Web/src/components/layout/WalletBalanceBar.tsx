import { useQuery } from '@tanstack/react-query';
import { AlertTriangle } from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { getMyWallets } from '@/api/wallet.api';
import { inr } from '@/lib/format';

/**
 * Signed-in user's live wallet balances, shown as compact text in the app bar
 * (e.g. "RCH : ₹6,13,06,616.00   AEPS : ₹4,16,277.44"). Refreshes every 60s.
 * Renders nothing until at least one wallet balance is available.
 */
export function WalletBalanceBar() {
  const { user } = useAuth();
  const userId = user?.UserID ?? 0;

  const { data, isError, error, refetch, isFetching } = useQuery({
    queryKey: ['my-wallets', userId],
    queryFn: () => getMyWallets(userId),
    enabled: userId > 0,
    staleTime: 30_000,
    refetchInterval: 60_000,
    retry: 1,
  });

  // Never fall back to a number the server did not give us — a stale/zero balance in the
  // app bar is worse than saying it is unavailable.
  if (isError) {
    return (
      <button
        type="button"
        onClick={() => refetch()}
        title={(error as Error)?.message || 'Balance unavailable'}
        className="hidden items-center gap-1.5 whitespace-nowrap pr-1 text-xs font-medium text-warning-600 sm:flex"
      >
        <AlertTriangle className="size-3.5" />
        {isFetching ? 'Loading balance…' : 'Balance unavailable — retry'}
      </button>
    );
  }

  const wallets = data ?? [];
  if (wallets.length === 0) return null;

  return (
    <div className="hidden items-center gap-x-4 gap-y-0.5 pr-1 sm:flex sm:flex-wrap sm:justify-end">
      {wallets.map((w) => (
        <span key={w.walletId} className="whitespace-nowrap text-xs leading-tight">
          <span className="font-medium uppercase text-slate-400">{w.shortName} : </span>
          <span className="font-semibold tabular-nums text-brand-600">{inr(w.balance)}</span>
        </span>
      ))}
    </div>
  );
}
