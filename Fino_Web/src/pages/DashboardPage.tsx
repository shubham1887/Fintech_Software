import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import {
  BookOpenText,
  ChevronRight,
  Clock,
  History,
  Landmark,
  RefreshCw,
  ScrollText,
  Search,
  Send,
  ShoppingCart,
  type LucideIcon,
} from 'lucide-react';
import { getDashboard, getTodaySummary } from '@/api/dashboard.api';
import { useAuth } from '@/features/auth/AuthContext';
import { apiDate, inr, num } from '@/lib/format';
import { isAdminTier, isDistributorTier, roleLabel } from '@/lib/roles';
import RetailerDashboardPage from '@/pages/RetailerDashboardPage';
import MobileHomePage from '@/pages/mobile/MobileHomePage';
import DistributorDashboardPage from '@/pages/DistributorDashboardPage';
import { cn } from '@/lib/cn';
import { useIsMobileApp } from '@/lib/native';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';

export default function DashboardPage() {
  const { user } = useAuth();
  const isMobile = useIsMobileApp();

  const roleId = user?.UserTypeID ?? 0;

  // On a phone every role gets the same app home — balance, today, services, recent activity.
  // The role-specific desktop dashboards (network tables, provider stock, charts) stay on
  // desktop, where there is room for them; on mobile they live under the More tab as reports.
  if (isMobile) return <MobileHomePage />;

  // Admin-tier only (Admin / Support / Accountant) sees the company-wide dashboard —
  // Provider Stock, Network Wallets and the global account summary. Nobody else does.
  if (isAdminTier(roleId)) {
    return <AdminDashboard />;
  }

  // Distributor + Super-Distributor get the network-scoped dashboard (their own downline).
  if (isDistributorTier(roleId)) {
    return <DistributorDashboardPage />;
  }

  // Everyone else — Retailer, Client, API User, field officers — gets the transaction-first
  // home scoped to their own account (own balance, own today's sale, own commission).
  return <RetailerDashboardPage />;
}

const QUICK_LINKS: { title: string; to: string; icon: LucideIcon }[] = [
  { title: 'Transaction History', to: '/transactions', icon: History },
  { title: 'Account Ledger', to: '/s/AdminLedger', icon: BookOpenText },
  { title: 'Purchase History', to: '/s/PurchaseReport', icon: ShoppingCart },
  { title: 'Day Book', to: '/s/Daybook/Report', icon: ScrollText },
  { title: 'Fund Requests', to: '/s/Fund', icon: Landmark },
  { title: 'Pending Recharge', to: '/s/Pending', icon: Clock },
  { title: 'DMT Report', to: '/s/DMTReport', icon: Send },
  { title: 'Quick Search', to: '/s/Search', icon: Search },
];

function AdminDashboard() {
  const { user } = useAuth();
  const [dateStr, setDateStr] = useState(() => apiDate(new Date()));
  const date = useMemo(() => new Date(`${dateStr}T00:00:00`), [dateStr]);

  const { data, isLoading, isFetching, refetch } = useQuery({
    queryKey: ['dashboard', user?.UserTypeID, dateStr],
    queryFn: () => getDashboard(user?.UserTypeID ?? 0, date),
    refetchInterval: 30_000, // keep balances/stocks in sync with the DB without a manual reload
  });

  const { data: summary = [], isLoading: summaryLoading } = useQuery({
    queryKey: ['today-summary', user?.UserTypeID, dateStr],
    queryFn: () => getTodaySummary(user?.UserTypeID ?? 0, date),
    refetchInterval: 30_000,
  });

  const adminWallets = data?.AdminWallets ?? [];
  const providerStocks = data?.ProviderStocks ?? [];
  const providerRchTotal = providerStocks.reduce((s, p) => s + p.RechargeBalance, 0);
  const providerDmtTotal = providerStocks.reduce((s, p) => s + p.DMTBalance, 0);
  const userTypeBalances = data?.UserTypeBalances ?? [];
  const userTypeTotal = userTypeBalances.reduce((s, t) => s + t.Balance, 0);
  const serviceSplit = data?.ServiceSplit ?? [];

  const monthly = (data?.MonthlyPerformance ?? []).map((d) => ({
    day: d.Day,
    'This month': d.CurrentMonth,
    'Last month': d.LastMonth,
  }));

  const prettyDate = useMemo(
    () => date.toLocaleDateString('en-IN', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }),
    [date],
  );

  return (
    <div className="mx-auto max-w-[1400px] space-y-4">
      {/* Page header */}
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-2xl font-bold tracking-tight text-slate-900">Dashboard</h1>
          <p className="mt-0.5 text-sm text-slate-500">
            {roleLabel(user?.UserTypeID ?? 0, user?.UserType ?? '')} · {prettyDate}
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
            onClick={() => refetch()}
            className="grid h-9 w-9 place-items-center rounded-lg border border-slate-200 bg-white text-slate-500 transition-colors hover:border-brand-300 hover:text-brand-600"
            aria-label="Refresh"
          >
            <RefreshCw className={cn('size-4', isFetching && 'animate-spin')} />
          </button>
        </div>
      </div>

      {/* Balances — one flat strip */}
      <Card>
        <CardBody className="flex flex-wrap items-stretch gap-y-3 divide-x divide-slate-100 p-2">
          {adminWallets.map((w) => (
            <Stat key={w.WalletID} label={w.WalletName} value={inr(w.Balance)} loading={isLoading} emphasis />
          ))}
          {adminWallets.length === 0 && <Stat label="Wallet Balance" value={inr(data?.AdminWalletBalance)} loading={isLoading} emphasis />}
          <Stat label="User Wallet Float" value={inr(data?.TotalUserWalletBalance)} loading={isLoading} />
          <Stat label="Provider Stock" value={inr(data?.ProviderStockTotal)} loading={isLoading} />
          <Stat
            label="Pending Fund Requests"
            value={`${inr(data?.FundRequestPendingAmount)}`}
            sub={`${num(data?.FundRequestPendingCount)} requests`}
            loading={isLoading}
          />
        </CardBody>
      </Card>

      {/* Day performance — one flat strip */}
      <Card>
        <CardBody className="flex flex-wrap items-stretch gap-y-3 divide-x divide-slate-100 p-2">
          <Stat label="Success Volume" value={inr(data?.TotalSuccessAmount)} tone="text-success-600" loading={isLoading} />
          <Stat label="Failed / Refunds" value={inr(data?.TotalFailedAmount)} tone="text-danger-600" loading={isLoading} />
          <Stat label="Commission" value={inr(data?.TotalAdminCommission)} loading={isLoading} />
          <Stat label="Surcharge" value={inr(data?.TotalAdminSurcharge)} loading={isLoading} />
          <Stat label="Admin Profit" value={inr((data?.AdminCommissionProfit ?? 0) + (data?.AdminSurchargeProfit ?? 0))} loading={isLoading} />
          <Stat label="Fund Transfers" value={inr(data?.TransferAmount)} sub={`${num(data?.TransferCount)} transfers`} loading={isLoading} />
        </CardBody>
      </Card>

      {/* Main grid */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        {/* Left: chart + today's summary */}
        <div className="space-y-4 lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Monthly Performance</CardTitle>
              <span className="text-xs text-slate-400">Successful volume per day — this month vs last month</span>
            </CardHeader>
            <CardBody>
              <div className="h-64 w-full">
                {isLoading ? (
                  <Skeleton className="h-full rounded-xl" />
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <AreaChart data={monthly} margin={{ top: 6, right: 6, left: -6, bottom: 0 }}>
                      <defs>
                        <linearGradient id="gCur" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="0%" stopColor="var(--color-brand-600)" stopOpacity={0.25} />
                          <stop offset="100%" stopColor="var(--color-brand-600)" stopOpacity={0.02} />
                        </linearGradient>
                      </defs>
                      <CartesianGrid strokeDasharray="3 3" stroke="#eef2f7" vertical={false} />
                      <XAxis dataKey="day" tickLine={false} axisLine={false} tick={{ fontSize: 11, fill: '#94a3b8' }} interval={2} />
                      <YAxis tickLine={false} axisLine={false} tick={{ fontSize: 11, fill: '#94a3b8' }} width={60} />
                      <Tooltip
                        formatter={(v) => inr(Number(v))}
                        labelFormatter={(d) => `Day ${d}`}
                        contentStyle={{ borderRadius: 10, border: '1px solid #e2e8f0', fontSize: 13 }}
                      />
                      <Area type="monotone" dataKey="Last month" stroke="#cbd5e1" strokeWidth={1.5} fill="none" />
                      <Area type="monotone" dataKey="This month" stroke="var(--color-brand-600)" strokeWidth={2} fill="url(#gCur)" />
                    </AreaChart>
                  </ResponsiveContainer>
                )}
              </div>
              <div className="mt-2 flex justify-center gap-6 text-xs text-slate-500">
                <span className="flex items-center gap-1.5"><span className="h-0.5 w-4 rounded bg-brand-600" />This month</span>
                <span className="flex items-center gap-1.5"><span className="h-0.5 w-4 rounded bg-slate-300" />Last month</span>
              </div>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Today's Summary</CardTitle>
              <span className="text-xs text-slate-400">Account movement for the selected date</span>
            </CardHeader>
            <CardBody className="p-0">
              {summaryLoading ? (
                <div className="space-y-2 p-5">{Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-6 rounded" />)}</div>
              ) : summary.length === 0 ? (
                <p className="py-10 text-center text-sm text-slate-400">No data for this date.</p>
              ) : (
                <div className="grid grid-cols-1 gap-x-10 px-5 py-2 sm:grid-cols-2">
                  {summary.map((s, i) => {
                    const t = s.Type.toLowerCase();
                    const isTotal = t.includes('opening') || t.includes('closing');
                    const isIn = (t.includes('success') || t === 'commission') && s.TotalAmount > 0;
                    const isOut = (t.includes('refund') || t.includes('deducted') || t.includes('pending')) && s.TotalAmount > 0;
                    return (
                      <div
                        key={`${s.Type}-${i}`}
                        className="flex items-center justify-between border-b border-slate-100 py-2.5 text-sm last:border-0 sm:[&:nth-last-child(2)]:border-0"
                      >
                        <span className={cn('text-slate-500', isTotal && 'font-semibold text-slate-800')}>{s.Type}</span>
                        <span className={cn(
                          'font-medium tabular-nums text-slate-700',
                          isTotal && 'font-display font-bold text-slate-900',
                          isIn && 'text-success-600',
                          isOut && 'text-danger-600',
                        )}>
                          {inr(s.TotalAmount)}
                        </span>
                      </div>
                    );
                  })}
                </div>
              )}

              {serviceSplit.length > 0 && (
                <>
                  <div className="border-t border-slate-100 px-5 pb-1 pt-4">
                    <p className="text-xs font-semibold uppercase tracking-wider text-slate-400">Services · selected date</p>
                  </div>
                  <div className="overflow-x-auto px-2 pb-2">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="text-xs text-slate-400">
                          <th className="px-3 py-2 text-left font-medium">Service</th>
                          <th className="px-3 py-2 text-right font-medium">Transactions</th>
                          <th className="px-3 py-2 text-right font-medium">Success</th>
                          <th className="px-3 py-2 text-right font-medium">Failed</th>
                        </tr>
                      </thead>
                      <tbody>
                        {serviceSplit.slice(0, 6).map((s) => (
                          <tr key={s.ServiceID} className="border-t border-slate-100">
                            <td className="px-3 py-2 font-medium text-slate-700">{s.ServiceName}</td>
                            <td className="px-3 py-2 text-right tabular-nums text-slate-500">{num(s.SuccessCount + s.FailedCount)}</td>
                            <td className="px-3 py-2 text-right tabular-nums text-success-600">{inr(s.SuccessAmount)}</td>
                            <td className="px-3 py-2 text-right tabular-nums text-slate-500">{inr(s.FailedAmount)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </>
              )}
            </CardBody>
          </Card>
        </div>

        {/* Right: provider stock, network wallets, quick links */}
        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Provider Stock</CardTitle>
            </CardHeader>
            <CardBody className="p-0">
              {isLoading ? (
                <div className="space-y-2 p-5">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-8 rounded" />)}</div>
              ) : providerStocks.length === 0 ? (
                <p className="py-8 text-center text-sm text-slate-400">No providers configured.</p>
              ) : (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-xs text-slate-400">
                      <th className="px-5 py-2 text-left font-medium">Provider</th>
                      <th className="px-3 py-2 text-right font-medium">Recharge</th>
                      <th className="px-5 py-2 text-right font-medium">DMT</th>
                    </tr>
                  </thead>
                  <tbody>
                    {providerStocks.map((p) => (
                      <tr key={p.ProviderID} className="border-t border-slate-100">
                        <td className="px-5 py-2.5 font-medium text-slate-700">
                          {p.ProviderName}
                          {!p.IsActive && <span className="ml-1.5 text-xs text-slate-400">(inactive)</span>}
                        </td>
                        <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{inr(p.RechargeBalance)}</td>
                        <td className="px-5 py-2.5 text-right tabular-nums text-slate-500">{inr(p.DMTBalance)}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="border-t border-slate-200">
                      <td className="px-5 py-2.5 text-xs font-semibold uppercase tracking-wider text-slate-400">Total</td>
                      <td className="px-3 py-2.5 text-right font-display font-bold tabular-nums text-slate-900">{inr(providerRchTotal)}</td>
                      <td className="px-5 py-2.5 text-right font-semibold tabular-nums text-slate-600">{inr(providerDmtTotal)}</td>
                    </tr>
                  </tfoot>
                </table>
              )}
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Network Wallets</CardTitle>
              <span className="text-xs text-slate-400">Balance by user type</span>
            </CardHeader>
            <CardBody className="p-0">
              {isLoading ? (
                <div className="space-y-2 p-5">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-8 rounded" />)}</div>
              ) : userTypeBalances.length === 0 ? (
                <p className="py-8 text-center text-sm text-slate-400">No wallet data.</p>
              ) : (
                <div className="px-5 py-2">
                  {userTypeBalances.map((t) => (
                    <div key={t.UserTypeID} className="flex items-center justify-between border-b border-slate-100 py-2.5 text-sm last:border-0">
                      <span className="text-slate-600">
                        {t.UserType}
                        <span className="ml-1.5 text-xs text-slate-400">({num(t.UserCount)})</span>
                      </span>
                      <span className="font-medium tabular-nums text-slate-800">{inr(t.Balance)}</span>
                    </div>
                  ))}
                  <div className="flex items-center justify-between border-t border-slate-200 py-2.5 text-sm">
                    <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Total</span>
                    <span className="font-display font-bold tabular-nums text-slate-900">{inr(userTypeTotal)}</span>
                  </div>
                </div>
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
                  <Link
                    key={q.to}
                    to={q.to}
                    className="group flex items-center justify-between rounded-lg px-3 py-2.5 text-sm text-slate-600 transition-colors hover:bg-slate-50 hover:text-brand-700"
                  >
                    <span className="flex items-center gap-2.5">
                      <q.icon className="size-4 text-slate-400 transition-colors group-hover:text-brand-600" />
                      {q.title}
                    </span>
                    <ChevronRight className="size-4 text-slate-300 transition-transform group-hover:translate-x-0.5 group-hover:text-brand-400" />
                  </Link>
                ))}
              </nav>
            </CardBody>
          </Card>
        </div>
      </div>
    </div>
  );
}

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

