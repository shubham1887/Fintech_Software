import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  PlusCircle, RefreshCw, ArrowLeftRight,
  TrendingUp, TrendingDown, BadgeIndianRupee, ArrowDownToLine, ChevronRight, AlertTriangle,
  Search, BookOpenText, ScrollText, History, KeyRound, FileText, Lock, type LucideIcon,
} from 'lucide-react';
import { useLiveWallets } from '@/features/live/useLiveSummary';
import { useOutstandingPosition } from '@/features/live/useOutstandingPosition';
import { getDashboard, getTodaySummary } from '@/api/dashboard.api';
import { fetchCatalog, imageUrl, orderedServices } from '@/api/catalog.api';
import { useAuth } from '@/features/auth/AuthContext';
import { apiDate, inr } from '@/lib/format';
import { isRetailer, UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { lockMessage, serviceHref, serviceLook } from '@/lib/serviceTiles';
import { QuickServices } from '@/components/dashboard/QuickServices';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';


export default function RetailerDashboardPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const roleId = user?.UserTypeID ?? 0;
  // Only true retail POS roles get the service grid; API users / others get a summary + links.
  const isRetail = roleId === UserTypeCode.Retailer || roleId === UserTypeCode.Client;
  const [dateStr] = useState(() => apiDate(new Date()));
  const date = useMemo(() => new Date(`${dateStr}T00:00:00`), [dateStr]);
  const firstName = user?.PersonName?.split(' ')[0] || user?.FirmName || 'there';

  // Auto-refresh so the dashboard stays in sync with the DB without a manual reload:
  // balances change on every transaction, so poll them and the day's summary periodically.
  // Balances come from the shared live feed — same numbers the header shows, no second poll.
  const wallets = useLiveWallets();
  // A retailer has no downline, so only the payable half of the credit position applies.
  const outstanding = useOutstandingPosition(false);
  const dash = useQuery({ queryKey: ['dashboard', user?.UserTypeID, dateStr], queryFn: () => getDashboard(user?.UserTypeID ?? 0, date), refetchInterval: 30_000 });
  const catalogQ = useQuery({ queryKey: ['service-catalog'], queryFn: fetchCatalog, staleTime: 300_000, enabled: isRetail });
  // Today's account movement — shown to non-retail roles (API user etc.) in place of the grid.
  const summaryQ = useQuery({
    queryKey: ['today-summary', roleId, dateStr],
    queryFn: () => getTodaySummary(roleId, date),
    enabled: !isRetail,
    refetchInterval: 30_000,
  });

  // The live catalog drives the grid — only services the admin keeps active + displayed
  // arrive here (the backend filters for non-admin roles). One flat, admin-ordered grid.
  const [showAll, setShowAll] = useState(false);
  // One flat grid in the ADMIN's order. The catalogue arrives grouped by category and sorted
  // within each group, so flattening it alone produced category-by-category order — a service
  // the admin had moved to position 1 still sat below every service of an earlier category,
  // which is not what "display order" means on a grid that shows no category headings.
  const allServices = useMemo(() => orderedServices(catalogQ.data ?? []), [catalogQ.data]);
  const VISIBLE = 11; // 11 tiles + a "More" tile = two clean rows of 6
  const visibleServices = showAll || allServices.length <= VISIBLE + 1 ? allServices : allServices.slice(0, VISIBLE);

  const d = dash.data;
  const totalBalance = (wallets.data ?? []).reduce((s, w) => s + w.balance, 0);

  return (
    <div className="space-y-5">
      {/* Hero: greeting + total balance + wallet chips */}
      <div className="relative overflow-hidden rounded-card bg-gradient-to-br from-ink-900 via-ink-800 to-brand-800 p-6 text-white shadow-card sm:p-7">
        <div className="absolute -right-16 -top-20 size-72 rounded-full bg-brand-500/25 blur-3xl" />
        <div className="absolute -bottom-24 left-1/4 size-72 rounded-full bg-brand-600/20 blur-3xl" />
        <div className="relative flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-sm text-ink-300">Welcome back</p>
            <h1 className="font-display text-2xl font-extrabold sm:text-3xl">Hello, {firstName} 👋</h1>
            <p className="mt-1 text-sm text-ink-300">Total wallet balance</p>
            {/* A failed read must never render as ₹0.00 — see getMyWallets. */}
            {wallets.isError ? (
              <div className="mt-1">
                <p className="flex items-center gap-2 font-display text-xl font-bold">
                  <AlertTriangle className="size-5 shrink-0 text-warning-500" />
                  Balance unavailable
                </p>
                <p className="mt-1 text-sm text-ink-300">{(wallets.error as Error)?.message}</p>
              </div>
            ) : wallets.isLoading ? (
              <Skeleton className="mt-1 h-9 w-40 bg-white/20" />
            ) : (
              <p className="font-display text-3xl font-extrabold tabular-nums sm:text-4xl">{inr(totalBalance)}</p>
            )}
            <div className="mt-4 flex flex-wrap gap-2">
              <Button onClick={() => navigate('/s/Fund/Request')} className="!bg-white !text-brand-700 hover:!bg-white/90">
                <PlusCircle className="size-4" />
                Request Funds
              </Button>
              {/* Wallet to wallet. The sidebar is built from the client's own menu table, so a
                  fresh install may not carry a row for this screen; the balance panel is where
                  somebody looks when they want to move money between wallets anyway. */}
              <button
                onClick={() => navigate('/s/WalletTransfer')}
                className="inline-flex items-center gap-2 rounded-lg border border-white/20 bg-white/10 px-3 text-sm text-white transition-colors hover:bg-white/20"
              >
                <ArrowLeftRight className="size-4" />
                Wallet to Wallet
              </button>
              <button
                onClick={() => { wallets.refetch(); dash.refetch(); }}
                className="inline-flex items-center gap-2 rounded-lg border border-white/20 bg-white/10 px-3 text-sm text-white transition-colors hover:bg-white/20"
              >
                <RefreshCw className={cn('size-4', (wallets.isFetching || dash.isFetching) && 'animate-spin')} />
                Refresh
              </button>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3 lg:w-[46%]">
            {!wallets.isError && (wallets.data ?? []).slice(0, 3).map((w) => (
              <div key={w.walletId} className="rounded-xl border border-white/10 bg-white/10 p-4 backdrop-blur">
                <p className="text-xs font-medium text-ink-200">{w.name}</p>
                <p className="mt-1 font-display text-lg font-bold tabular-nums">{inr(w.balance)}</p>
              </div>
            ))}
            {!wallets.isLoading && !wallets.isError && (wallets.data ?? []).length === 0 && (
              <div className="rounded-xl border border-white/10 bg-white/10 p-4 text-sm text-ink-200 sm:col-span-3">
                No wallets provisioned yet.
              </div>
            )}
            {/* What this user owes their upline for funds taken on credit — one more tile in the
                wallet row, because it is the same question asked the other way round: this is how
                much of what they are holding is not theirs yet. Self-hiding, so a retailer who
                always pays cash never sees it. */}
            {outstanding.payable > 0 && (
              <Link
                to="/s/Outstanding"
                className="rounded-xl border border-warning-300/40 bg-warning-400/15 p-4 backdrop-blur transition-colors hover:bg-warning-400/25"
              >
                <p className="text-xs font-medium text-ink-200">Outstanding</p>
                <p className="mt-1 font-display text-lg font-bold tabular-nums text-warning-200">{inr(outstanding.payable)}</p>
                <p className="mt-0.5 truncate text-[11px] text-ink-200">
                  {outstanding.payableTo ? `Payable to ${outstanding.payableTo}` : 'Payable to your upline'}
                </p>
              </Link>
            )}
          </div>
        </div>
      </div>

      {/* Today KPIs */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Kpi label="Today's sale" value={inr(d?.TotalSuccessAmount)} icon={TrendingUp} tone="bg-success-50 text-success-600" loading={dash.isLoading} />
        <Kpi label="Failed" value={inr(d?.TotalFailedAmount)} icon={TrendingDown} tone="bg-danger-50 text-danger-600" loading={dash.isLoading} />
        <Kpi label="Commission earned" value={inr(d?.TotalCommission)} icon={BadgeIndianRupee} tone="bg-brand-50 text-brand-600" loading={dash.isLoading} />
        <Kpi label="Funds received" value={inr(d?.PurchaseAmount)} icon={ArrowDownToLine} tone="bg-info-50 text-info-600" loading={dash.isLoading} />
      </div>

      {/* The retailer's own pinned shortcuts, above everything they have to scroll for — and the
          retailer's alone. It is the counter operator's row: the one role that opens the same few
          services all day. Nobody else works a service grid here — the distributor tier lives in
          network screens and reports, and an API user is a server-to-server credential with no
          dashboard at all. QuickService/Select and /Save refuse every other role server-side too,
          so this panel could only ever render empty for them. */}
      {isRetailer(roleId) && <QuickServices />}

      {/* Retail POS roles get the service grid; everyone else (API user, etc.) gets a
          Today's Summary + quick links to their reports. */}
      {!isRetail ? (
        <NonRetailHome summary={summaryQ.data ?? []} loading={summaryQ.isLoading} />
      ) : (
      /* Services — live from the admin catalog (icons, on/off and visibility respected) */
      <Card>
        <CardHeader>
          <CardTitle>Services</CardTitle>
          <Link to="/transactions" className="flex items-center gap-1 text-sm text-brand-600 hover:text-brand-700">
            Transaction history <ChevronRight className="size-4" />
          </Link>
        </CardHeader>
        <CardBody>
          {catalogQ.isLoading ? (
            <div className="grid grid-cols-3 gap-3 sm:grid-cols-4 md:grid-cols-6">
              {Array.from({ length: 12 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-xl" />)}
            </div>
          ) : allServices.length === 0 ? (
            <p className="py-6 text-center text-sm text-slate-400">No services are enabled for your account yet.</p>
          ) : (
            <div className="grid grid-cols-3 gap-3 sm:grid-cols-4 md:grid-cols-6">
              {visibleServices.map((svc) => {
                const look = serviceLook(svc.Name);
                const img = imageUrl(svc.ServiceImagePath);
                const tile = (
                  <>
                    <span className={cn('relative grid size-13 place-items-center overflow-hidden rounded-2xl shadow-sm ring-1 ring-black/[0.04] transition-transform', svc.IsLocked ? 'bg-slate-100' : 'group-hover:scale-105', img && !svc.IsLocked ? 'bg-white' : !img && !svc.IsLocked ? look.tone : '')}>
                      {img
                        ? <img src={img} alt="" className={cn('size-full object-contain p-2', svc.IsLocked && 'opacity-40 grayscale')} />
                        : <look.icon className={cn('size-6', svc.IsLocked && 'text-slate-400')} />}
                      {svc.IsLocked && (
                        <span className="absolute inset-0 grid place-items-center bg-slate-900/5">
                          <Lock className="size-4 text-slate-500" />
                        </span>
                      )}
                    </span>
                    <span className={cn('line-clamp-2 text-xs font-medium leading-tight', svc.IsLocked ? 'text-slate-400' : 'text-slate-600')}>{svc.Name}</span>
                  </>
                );
                // A locked service stays on the grid so the retailer can see it exists, but it
                // doesn't open - the API refuses it anyway. The reason comes from the server, so a
                // service held back only by a pending KYC says so (the user can clear that
                // themselves) rather than reading as an admin lock they can do nothing about.
                if (svc.IsLocked) {
                  return (
                    <div
                      key={svc.ServiceID}
                      title={
                        lockMessage(svc.LockReason)
                      }
                      className="flex cursor-not-allowed flex-col items-center gap-2.5 rounded-2xl p-3.5 text-center"
                    >
                      {tile}
                    </div>
                  );
                }
                return (
                  <Link
                    key={svc.ServiceID}
                    to={serviceHref(svc.Name)}
                    className="group flex flex-col items-center gap-2.5 rounded-2xl p-3.5 text-center transition-all hover:-translate-y-0.5 hover:bg-brand-50/50"
                  >
                    {tile}
                  </Link>
                );
              })}
              {allServices.length > VISIBLE + 1 && (
                <button
                  type="button"
                  onClick={() => setShowAll((v) => !v)}
                  className="group flex flex-col items-center gap-2.5 rounded-2xl p-3.5 text-center transition-all hover:-translate-y-0.5 hover:bg-brand-50/50"
                >
                  <span className="grid size-13 place-items-center rounded-2xl bg-slate-100 text-slate-500 shadow-sm ring-1 ring-black/[0.04] transition-transform group-hover:scale-105">
                    {showAll ? <ChevronRight className="size-6 -rotate-90" /> : <span className="font-display text-sm font-bold">+{allServices.length - VISIBLE}</span>}
                  </span>
                  <span className="text-xs font-medium leading-tight text-slate-600">{showAll ? 'Show less' : 'View all'}</span>
                </button>
              )}
            </div>
          )}
        </CardBody>
      </Card>
      )}
    </div>
  );
}

/** Quick links for non-retail roles (API user, field officers) — reports they can reach. */
const QUICK_LINKS: { title: string; to: string; icon: LucideIcon }[] = [
  { title: 'Wallet to Wallet', to: '/s/WalletTransfer', icon: ArrowLeftRight },
  { title: 'Quick Search', to: '/s/Search', icon: Search },
  { title: 'Sales by User', to: '/s/UserwiseSale', icon: TrendingUp },
  { title: 'Daybook', to: '/s/Daybook/Report', icon: ScrollText },
  { title: 'Ledger', to: '/s/UserLedger', icon: BookOpenText },
  { title: 'Transaction History', to: '/transactions', icon: History },
  { title: 'API Access', to: '/s/ApiAccess', icon: KeyRound },
  { title: 'API Documentation', to: '/s/ApiDocs', icon: FileText },
];

/** Today's account movement + quick links — the non-retail home body. */
function NonRetailHome({ summary, loading }: { summary: { Type: string; TotalAmount: number }[]; loading: boolean }) {
  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
      <Card className="lg:col-span-2">
        <CardHeader>
          <CardTitle>Today's Summary</CardTitle>
          <span className="text-xs text-slate-400">Your account movement for today, across every wallet</span>
        </CardHeader>
        <CardBody className="p-0">
          {loading ? (
            <div className="space-y-2 p-5">{Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-6 rounded" />)}</div>
          ) : summary.length === 0 ? (
            <p className="py-10 text-center text-sm text-slate-400">No account movement today.</p>
          ) : (
            <div className="grid grid-cols-1 gap-x-10 px-5 py-2 sm:grid-cols-2">
              {summary.map((s, i) => {
                const t = s.Type.toLowerCase();
                const isTotal = t.includes('opening') || t.includes('closing');
                const isIn = (t.includes('success') || t === 'commission') && s.TotalAmount > 0;
                const isOut = (t.includes('refund') || t.includes('deducted') || t.includes('pending')) && s.TotalAmount > 0;
                return (
                  <div key={`${s.Type}-${i}`} className="flex items-center justify-between border-b border-slate-100 py-2.5 text-sm last:border-0 sm:[&:nth-last-child(2)]:border-0">
                    <span className={cn('text-slate-500', isTotal && 'font-semibold text-slate-800')}>{s.Type}</span>
                    <span className={cn('font-medium tabular-nums text-slate-700', isTotal && 'font-display font-bold text-slate-900', isIn && 'text-success-600', isOut && 'text-danger-600')}>
                      {inr(s.TotalAmount)}
                    </span>
                  </div>
                );
              })}
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
  );
}

function Kpi({ label, value, icon: Icon, tone, loading }: { label: string; value: string; icon: LucideIcon; tone: string; loading: boolean }) {
  return (
    <div className="rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
      <div className="flex items-start justify-between">
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-slate-500">{label}</p>
          {loading ? (
            <Skeleton className="mt-2 h-7 w-24" />
          ) : (
            <p className="mt-1 font-display text-2xl font-bold tracking-tight text-slate-900 tabular-nums">{value}</p>
          )}
        </div>
        <span className={cn('grid size-10 shrink-0 place-items-center rounded-xl', tone)}>
          <Icon className="size-5" />
        </span>
      </div>
    </div>
  );
}
