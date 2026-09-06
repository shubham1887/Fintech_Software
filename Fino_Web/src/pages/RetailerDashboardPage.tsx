import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  ArrowLeftRight, Banknote, CreditCard, Droplet, Fingerprint, Landmark, PlusCircle,
  ReceiptText, RefreshCw, Smartphone, Tv, Wallet, Wifi, Zap, Car,
  TrendingUp, TrendingDown, BadgeIndianRupee, ArrowDownToLine, ChevronRight, AlertTriangle,
  Search, BookOpenText, ScrollText, History, KeyRound, FileText, Lock, type LucideIcon,
} from 'lucide-react';
import { getMyWallets } from '@/api/wallet.api';
import { getDashboard, getTodaySummary } from '@/api/dashboard.api';
import { fetchCatalog, imageUrl, type CatalogService } from '@/api/catalog.api';
import { useAuth } from '@/features/auth/AuthContext';
import { apiDate, inr } from '@/lib/format';
import { UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';

// Keyword → icon + colour + destination for catalog services. First match wins; the icon is
// only the fallback when the admin hasn't uploaded a service image yet.
const SERVICE_LOOKS: Array<{ match: RegExp; icon: LucideIcon; tone: string; to?: string }> = [
  { match: /aeps/i, icon: Fingerprint, tone: 'bg-rose-50 text-rose-600', to: '/s/AEPSIndex' },
  { match: /micro\s*atm|matm/i, icon: Banknote, tone: 'bg-lime-50 text-lime-600', to: '/s/AEPSIndex' },
  { match: /money transfer|dmt/i, icon: ArrowLeftRight, tone: 'bg-emerald-50 text-emerald-600', to: '/s/DMTTransaction' },
  { match: /payout|settlement/i, icon: Landmark, tone: 'bg-indigo-50 text-indigo-600', to: '/s/PayoutV1' },
  { match: /prepaid/i, icon: Smartphone, tone: 'bg-blue-50 text-blue-600' },
  { match: /postpaid/i, icon: ReceiptText, tone: 'bg-cyan-50 text-cyan-600' },
  { match: /dth|cable/i, icon: Tv, tone: 'bg-violet-50 text-violet-600' },
  { match: /electricity/i, icon: Zap, tone: 'bg-amber-50 text-amber-600' },
  { match: /water/i, icon: Droplet, tone: 'bg-sky-50 text-sky-600' },
  { match: /broadband|landline|fiber/i, icon: Wifi, tone: 'bg-fuchsia-50 text-fuchsia-600' },
  { match: /fastag/i, icon: Car, tone: 'bg-orange-50 text-orange-600' },
  { match: /credit card/i, icon: CreditCard, tone: 'bg-purple-50 text-purple-600' },
  { match: /gas|lpg/i, icon: Zap, tone: 'bg-red-50 text-red-600' },
  { match: /insurance|loan|emi/i, icon: Landmark, tone: 'bg-teal-50 text-teal-600' },
];
const DEFAULT_LOOK = { icon: Wallet, tone: 'bg-slate-100 text-slate-600' };

function serviceLook(name: string) {
  return SERVICE_LOOKS.find((l) => l.match.test(name)) ?? DEFAULT_LOOK;
}

/** Where a catalog service tile navigates: special flows (AEPS/DMT/Payout) go to their own
 *  screens, everything else lands on the recharge/bill-pay flow pre-filtered to the service. */
function serviceHref(svc: CatalogService): string {
  const look = SERVICE_LOOKS.find((l) => l.match.test(svc.Name));
  return (look && 'to' in look && look.to) || `/recharge?service=${encodeURIComponent(svc.Name)}`;
}

export default function RetailerDashboardPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const userId = user?.UserID ?? 0;
  const roleId = user?.UserTypeID ?? 0;
  // Only true retail POS roles get the service grid; API users / others get a summary + links.
  const isRetail = roleId === UserTypeCode.Retailer || roleId === UserTypeCode.Client;
  const [dateStr] = useState(() => apiDate(new Date()));
  const date = useMemo(() => new Date(`${dateStr}T00:00:00`), [dateStr]);
  const firstName = user?.PersonName?.split(' ')[0] || user?.FirmName || 'there';

  // Auto-refresh so the dashboard stays in sync with the DB without a manual reload:
  // balances change on every transaction, so poll them and the day's summary periodically.
  const wallets = useQuery({ queryKey: ['my-wallets', userId], queryFn: () => getMyWallets(userId), enabled: userId > 0, refetchInterval: 30_000, retry: 1 });
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
  const allServices = useMemo(() => (catalogQ.data ?? []).flatMap((c) => c.Services), [catalogQ.data]);
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
                        svc.LockReason === 'KYC pending'
                          ? 'Your KYC is not approved yet. Complete your KYC from your profile to use this service.'
                          : 'This service is locked for your account. Please contact your service provider.'
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
                    to={serviceHref(svc)}
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
  { title: 'Quick Search', to: '/s/Search', icon: Search },
  { title: 'Userwise Sale', to: '/s/UserwiseSale', icon: TrendingUp },
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
          <span className="text-xs text-slate-400">Your account movement for today</span>
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
