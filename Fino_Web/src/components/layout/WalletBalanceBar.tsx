import { AlertTriangle, RefreshCw } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useLiveSummary } from '@/features/live/useLiveSummary';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';

/**
 * The signed-in user's live wallet balances in the page header
 * (e.g. "RCH ₹6,13,06,616.00   AEPS ₹4,16,277.44").
 *
 * Reads the shared live feed rather than fetching for itself, so putting this on every page costs
 * no extra request at all — the strip, this bar and anything else on the chrome share one poll.
 * That is what makes it affordable to keep the balance genuinely current for a thousand users at
 * once instead of refreshing it once a minute and hoping.
 *
 * It never invents a number. When the read fails the bar says so and offers a retry; a balance
 * that quietly renders as ₹0.00 after a failed call is the worst thing a payments console can do
 * — the retailer either believes the money is gone, or trusts the zero into a transaction the
 * server then rejects.
 */
export function WalletBalanceBar() {
  const { summary, isLoading, error, refresh } = useLiveSummary();

  if (error || (!isLoading && !summary.walletsAvailable && summary.wallets.length === 0)) {
    return (
      <button
        type="button"
        onClick={refresh}
        title={error?.message || 'Balance unavailable'}
        className="hidden items-center gap-1.5 whitespace-nowrap pr-1 text-xs font-medium text-warning-600 sm:flex"
      >
        <AlertTriangle className="size-3.5" />
        Balance unavailable — retry
      </button>
    );
  }

  if (isLoading) {
    return (
      <span className="hidden items-center gap-1.5 whitespace-nowrap pr-1 text-xs text-slate-400 sm:flex">
        <RefreshCw className="size-3.5 animate-spin" />
        Loading balance…
      </span>
    );
  }

  const wallets = summary.wallets;
  // Outstanding is money OWED, not money held, so it is shown only when there is some. Someone
  // who has never taken funds on credit — or who has cleared what they took — sees nothing here,
  // which is what makes the figure mean something on the day it does appear.
  const owes = summary.outstanding > 0;
  if (wallets.length === 0 && !owes) return null;

  return (
    <div className="hidden items-center gap-x-4 gap-y-0.5 pr-1 sm:flex sm:flex-wrap sm:justify-end">
      {wallets.map((w) => (
        <span key={w.walletId} className="whitespace-nowrap text-xs leading-tight" title={w.name}>
          <span className="font-medium uppercase text-slate-400">{w.shortName} </span>
          <span className={cn('font-semibold tabular-nums text-brand-600')}>{inr(w.balance)}</span>
        </span>
      ))}
      {owes && (
        <Link
          to="/s/Outstanding"
          className="whitespace-nowrap text-xs leading-tight"
          title={
            summary.maxOutstanding > 0
              ? `Outstanding — you owe this to your upline. Credit limit ${inr(summary.maxOutstanding)}.`
              : 'Outstanding — you owe this to your upline.'
          }
        >
          {/* A different colour from the wallets on purpose: every other figure on this bar is
              money the user HAS, and one more brand-coloured number would read as one more
              wallet. This one is the opposite sign. */}
          <span className="font-medium uppercase text-slate-400">OUTST </span>
          <span className="font-semibold tabular-nums text-warning-600">{inr(summary.outstanding)}</span>
        </Link>
      )}
    </div>
  );
}
