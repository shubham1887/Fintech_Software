import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  AlertTriangle,
  ArrowDownToLine,
  BookOpenText,
  ChevronRight,
  History,
  Send,
  Users,
  RefreshCw,
  type LucideIcon,
} from 'lucide-react';
import { getDashboard } from '@/api/dashboard.api';
import { useLiveWallets } from '@/features/live/useLiveSummary';
import { useOutstandingPosition } from '@/features/live/useOutstandingPosition';
import { useAuth } from '@/features/auth/AuthContext';
import { apiDate, inr, num } from '@/lib/format';
import { roleLabel, UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * Dashboard for Distributor + Super-Distributor roles. Scoped to the signed-in user's own
 * network — sales/commission are aggregated by DistributerUserID / SuperDistributerUserID on the
 * backend (Dashboard/SelectForDistributor), and the downline table lists direct child users with
 * their wallet float and the day's sale. Minimal, English-only, flat-strip design.
 */
export default function DistributorDashboardPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const roleId = user?.UserTypeID ?? 0;
  const isSuper = roleId === UserTypeCode.SuperDistributor;

  const [dateStr, setDateStr] = useState(() => apiDate(new Date()));
  const date = useMemo(() => new Date(`${dateStr}T00:00:00`), [dateStr]);

  const dash = useQuery({
    queryKey: ['dashboard', roleId, dateStr],
    queryFn: () => getDashboard(roleId, date),
    refetchInterval: 30_000, // auto-sync with the DB without a manual reload
  });
  // Balances come from the shared live feed — same numbers the header shows, no second poll.
  const wallets = useLiveWallets();

  // A distributor sits in the middle: they owe upwards and are owed from below.
  const outstanding = useOutstandingPosition(true);

  const d = dash.data;
  const downline = d?.Downline ?? [];
  const services = d?.ServiceSplit ?? [];
  const downlineBalance = downline.reduce((s, r) => s + r.WalletBalance, 0);
  const downlineSale = downline.reduce((s, r) => s + r.SuccessAmount, 0);
  const activeDownline = downline.filter((r) => r.IsActive).length;

  const prettyDate = useMemo(
    () => date.toLocaleDateString('en-IN', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }),
    [date],
  );

  const teamLabel = isSuper ? 'Distributors' : 'Retailers';

  return (
    <div className="mx-auto max-w-[1400px] space-y-4">
      {/* Header */}
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-2xl font-bold tracking-tight text-slate-900">Dashboard</h1>
          <p className="mt-0.5 text-sm text-slate-500">
            {roleLabel(roleId, user?.UserType ?? '')} · {prettyDate}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <input
            type="date"
            value={dateStr}
            max={apiDate(new Date())}
            onChange={(e) => setDateStr(e.target.value)}
            className="h-9 rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-700 outline-none focus:border-brand-400 focus:ring-2 focus:ring-brand-500/15"
          />
          <button
            onClick={() => { dash.refetch(); wallets.refetch(); }}
            className="grid h-9 w-9 place-items-center rounded-lg border border-slate-200 bg-white text-slate-500 transition-colors hover:border-brand-300 hover:text-brand-600"
            aria-label="Refresh"
          >
            <RefreshCw className={cn('size-4', (dash.isFetching || wallets.isFetching) && 'animate-spin')} />
          </button>
        </div>
      </div>

      {/* Wallets + funds — one flat strip */}
      <Card>
        <CardBody className="flex flex-wrap items-stretch gap-y-3 divide-x divide-slate-100 p-2">
          {/* A failed read must never render as ₹0.00 — see getMyWallets. */}
          {wallets.isError ? (
            <button
              type="button"
              onClick={() => wallets.refetch()}
              title={(wallets.error as Error)?.message}
              className="flex min-w-40 flex-col items-start gap-0.5 px-4 py-2 text-left"
            >
              <span className="flex items-center gap-1.5 text-xs font-medium text-warning-600">
                <AlertTriangle className="size-3.5" />
                Balance unavailable
              </span>
              <span className="text-xs text-slate-400">Tap to retry</span>
            </button>
          ) : (
            <>
              {(wallets.data ?? []).map((w) => (
                <Stat key={w.walletId} label={w.name} value={inr(w.balance)} loading={wallets.isLoading} emphasis />
              ))}
              {!wallets.isLoading && (wallets.data ?? []).length === 0 && (
                <Stat label="Wallet Balance" value={inr(d?.AdminWalletBalance)} loading={dash.isLoading} emphasis />
              )}
            </>
          )}
          <Stat
            label="Funds Received"
            value={inr(d?.PurchaseAmount)}
            sub={`${num(d?.PurchaseCount)} today`}
            loading={dash.isLoading}
          />
          <Stat
            label="Funds Transferred"
            value={inr(d?.TransferAmount)}
            sub={`${num(d?.TransferCount)} today`}
            loading={dash.isLoading}
          />
          {/* Both halves of the credit position, beside the wallets rather than in a panel of
              their own: what this distributor owes upwards, and what their own downline owes
              them. Never netted against each other — they are different debts to different
              people. Each shows only when there is something to show. */}
          {outstanding.payable > 0 && (
            <Stat
              label="My Outstanding"
              value={inr(outstanding.payable)}
              sub={outstanding.payableTo ? `Payable to ${outstanding.payableTo}` : 'Payable to your upline'}
              tone="text-warning-600"
              loading={outstanding.loading}
            />
          )}
          {outstanding.receivable > 0 && (
            <Stat
              label="Outstanding"
              value={inr(outstanding.receivable)}
              sub={`${num(outstanding.accounts)} account${outstanding.accounts === 1 ? ' owes' : 's owe'} you`}
              tone="text-warning-600"
              loading={outstanding.loading}
            />
          )}
          <Stat label={`${teamLabel}`} value={num(downline.length)} sub={`${num(activeDownline)} active`} loading={dash.isLoading} />
        </CardBody>
      </Card>

      {/* Day performance — one flat strip */}
      <Card>
        <CardBody className="flex flex-wrap items-stretch gap-y-3 divide-x divide-slate-100 p-2">
          <Stat label="Network Sale" value={inr(d?.TotalSuccessAmount)} tone="text-success-600" loading={dash.isLoading} />
          <Stat label="Failed / Refunds" value={inr(d?.TotalFailedAmount)} tone="text-danger-600" loading={dash.isLoading} />
          <Stat label="Commission Earned" value={inr(d?.TotalCommission)} loading={dash.isLoading} />
          <Stat label="Surcharge" value={inr(d?.Surcharge)} loading={dash.isLoading} />
        </CardBody>
      </Card>


      {/* Main grid */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        {/* Left: downline table */}
        <div className="space-y-4 lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>My Network</CardTitle>
              <Link to="/s/Users" className="flex items-center gap-1 text-sm text-brand-600 hover:text-brand-700">
                Manage users <ChevronRight className="size-4" />
              </Link>
            </CardHeader>
            <CardBody className="p-0">
              {dash.isLoading ? (
                <div className="space-y-2 p-5">{Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-8 rounded" />)}</div>
              ) : downline.length === 0 ? (
                <p className="py-12 text-center text-sm text-slate-400">No {teamLabel.toLowerCase()} under your account yet.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="text-xs text-slate-400">
                        <th className="px-5 py-2.5 text-left font-medium">{isSuper ? 'Distributor' : 'Retailer'}</th>
                        <th className="px-3 py-2.5 text-right font-medium">Balance</th>
                        <th className="px-3 py-2.5 text-right font-medium">Today's Sale</th>
                        <th className="px-5 py-2.5 text-right font-medium">Txns</th>
                      </tr>
                    </thead>
                    <tbody>
                      {downline.map((r) => (
                        <tr key={r.UserID} className="border-t border-slate-100">
                          <td className="px-5 py-2.5">
                            <div className="font-medium text-slate-700">
                              {r.FirmName || r.PersonName || `User ${r.UserID}`}
                              {!r.IsActive && <span className="ml-1.5 text-xs text-slate-400">(inactive)</span>}
                            </div>
                            <div className="text-xs text-slate-400">{r.MobileNumber}</div>
                          </td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{inr(r.WalletBalance)}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-success-600">{inr(r.SuccessAmount)}</td>
                          <td className="px-5 py-2.5 text-right tabular-nums text-slate-500">{num(r.SuccessCount)}</td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot>
                      <tr className="border-t border-slate-200">
                        <td className="px-5 py-2.5 text-xs font-semibold uppercase tracking-wider text-slate-400">Total</td>
                        <td className="px-3 py-2.5 text-right font-display font-bold tabular-nums text-slate-900">{inr(downlineBalance)}</td>
                        <td className="px-3 py-2.5 text-right font-semibold tabular-nums text-success-700">{inr(downlineSale)}</td>
                        <td className="px-5 py-2.5" />
                      </tr>
                    </tfoot>
                  </table>
                </div>
              )}
            </CardBody>
          </Card>
        </div>

        {/* Right: service summary + quick links */}
        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Service Summary</CardTitle>
              <span className="text-xs text-slate-400">Selected date</span>
            </CardHeader>
            <CardBody className="p-0">
              {dash.isLoading ? (
                <div className="space-y-2 p-5">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-8 rounded" />)}</div>
              ) : services.length === 0 ? (
                <p className="py-10 text-center text-sm text-slate-400">No transactions on this date.</p>
              ) : (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-xs text-slate-400">
                      <th className="px-5 py-2 text-left font-medium">Service</th>
                      <th className="px-3 py-2 text-right font-medium">Success</th>
                      <th className="px-5 py-2 text-right font-medium">Failed</th>
                    </tr>
                  </thead>
                  <tbody>
                    {services.map((s) => (
                      <tr key={s.ServiceID} className="border-t border-slate-100">
                        <td className="px-5 py-2.5 font-medium text-slate-700">{s.ServiceName}</td>
                        <td className="px-3 py-2.5 text-right tabular-nums text-success-600">{inr(s.SuccessAmount)}</td>
                        <td className="px-5 py-2.5 text-right tabular-nums text-slate-500">{inr(s.FailedAmount)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Quick Links</CardTitle>
            </CardHeader>
            <CardBody className="p-2">
              <nav>
                {QUICK_LINKS.map((q) => (
                  <button
                    key={q.to}
                    onClick={() => navigate(q.to)}
                    className="group flex w-full items-center justify-between rounded-lg px-3 py-2.5 text-left text-sm text-slate-600 transition-colors hover:bg-slate-50 hover:text-brand-700"
                  >
                    <span className="flex items-center gap-2.5">
                      <q.icon className="size-4 text-slate-400 transition-colors group-hover:text-brand-600" />
                      {q.title}
                    </span>
                    <ChevronRight className="size-4 text-slate-300 transition-transform group-hover:translate-x-0.5 group-hover:text-brand-400" />
                  </button>
                ))}
              </nav>
            </CardBody>
          </Card>
        </div>
      </div>
    </div>
  );
}

const QUICK_LINKS: { title: string; to: string; icon: LucideIcon }[] = [
  { title: 'Fund Transfer', to: '/s/Fund/Transfer', icon: Send },
  { title: 'Fund Request', to: '/s/Fund/Request', icon: ArrowDownToLine },
  { title: 'My Users', to: '/s/Users', icon: Users },
  { title: 'Transaction History', to: '/transactions', icon: History },
  // No Downline Report link: that report is the platform operator's whole-network view and is
  // Admin/Support-only now, so a distributor following this tile would have hit a blocked route.
  // Their own network, with balances, is the team table further up this same dashboard.
  { title: 'Account Ledger', to: '/s/UserLedger', icon: BookOpenText },
];

function Stat({
  label,
  value,
  sub,
  tone,
  emphasis,
  loading,
}: {
  label: string;
  value: string;
  sub?: string;
  tone?: string;
  emphasis?: boolean;
  loading: boolean;
}) {
  return (
    <div className="min-w-[150px] flex-1 px-5 py-2.5">
      <p className="truncate text-[11px] font-semibold uppercase tracking-wider text-slate-400">{label}</p>
      {loading ? (
        <Skeleton className="mt-1.5 h-6 w-24" />
      ) : (
        <p className={cn('mt-1 font-display text-xl font-bold tabular-nums', emphasis ? 'text-brand-700' : 'text-slate-900', tone)}>
          {value}
        </p>
      )}
      {sub && !loading && <p className="mt-0.5 text-xs text-slate-400">{sub}</p>}
    </div>
  );
}
