import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  AlertTriangle, ArrowDownToLine, ArrowLeftRight, BadgeIndianRupee, Banknote, BookOpenText, Car,
  ChevronRight, CreditCard, Droplet, Eye, EyeOff, Fingerprint, History, Landmark, Lock, Plus,
  ReceiptText, RefreshCw, ScrollText, Search, Smartphone, TrendingDown, TrendingUp, Tv, Wallet,
  Wifi, Zap, type LucideIcon,
} from 'lucide-react';
import { getMyWallets, type WalletBalance } from '@/api/wallet.api';
import { getDashboard } from '@/api/dashboard.api';
import { fetchCatalog, imageUrl, type CatalogService } from '@/api/catalog.api';
import { selectReport, type TransactionRow } from '@/api/transactionReport.api';
import { useAuth } from '@/features/auth/AuthContext';
import { apiDate, inr } from '@/lib/format';
import { UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';
import { MASK, useBalanceMasked } from '@/lib/balanceMask';
import { txnSubtitle, txnTitle, txnWhen, type ServiceNames } from '@/lib/txn';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * The phone home screen.
 *
 *   balance card  →  services grid  →  today (one slim row)  →  recent activity
 *
 * The balance card is the anchor: it is the first thing a retailer looks for and the only
 * element on the screen allowed real visual weight. Everything below it is deliberately quiet
 * so that hierarchy survives — one dark surface, then flat white cards.
 *
 * Screens OTHER than this one get a condensed balance strip in the app bar (MobileShell), so
 * the figure is never more than a glance away without being drawn twice on the same screen.
 */

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

function serviceHref(svc: CatalogService): string {
  const look = SERVICE_LOOKS.find((l) => l.match.test(svc.Name));
  return (look && 'to' in look && look.to) || `/recharge?service=${encodeURIComponent(svc.Name)}`;
}

const VISIBLE_TILES = 7; // 7 + "All" = two rows of four

export default function MobileHomePage() {
  const { user } = useAuth();
  const userId = user?.UserID ?? 0;
  const roleId = user?.UserTypeID ?? 0;
  const isRetail = roleId === UserTypeCode.Retailer || roleId === UserTypeCode.Client;

  const [dateStr] = useState(() => apiDate(new Date()));
  const date = useMemo(() => new Date(`${dateStr}T00:00:00`), [dateStr]);

  const wallets = useQuery({
    queryKey: ['my-wallets', userId],
    queryFn: () => getMyWallets(userId),
    enabled: userId > 0,
    refetchInterval: 30_000,
    // A 403 / policy failure will not fix itself — don't hammer it three times.
    retry: 1,
  });
  const dash = useQuery({
    queryKey: ['dashboard', roleId, dateStr],
    queryFn: () => getDashboard(roleId, date),
    refetchInterval: 30_000,
  });
  // Fetched for every role, not just retail: the grid below is retail-only, but the recent
  // list needs the ServiceID -> name map (the report API sends only the id).
  const catalogQ = useQuery({
    queryKey: ['service-catalog'],
    queryFn: fetchCatalog,
    staleTime: 300_000,
  });
  const serviceNames: ServiceNames = useMemo(() => {
    const map = new Map<number, string>();
    for (const cat of catalogQ.data ?? []) for (const s of cat.Services) map.set(s.ServiceID, s.Name);
    return map;
  }, [catalogQ.data]);
  const recent = useQuery({
    queryKey: ['recent-txns', dateStr],
    queryFn: () => selectReport({ fromDate: dateStr, toDate: dateStr, pageNumber: 1, pageSize: 6 }),
    refetchInterval: 60_000,
  });

  return (
    <div className="space-y-4">
      <BalanceCard
        wallets={wallets.data ?? []}
        loading={wallets.isLoading}
        error={wallets.isError ? (wallets.error as Error)?.message : ''}
        refreshing={wallets.isFetching}
        onRefresh={() => {
          haptic('medium');
          wallets.refetch();
          dash.refetch();
        }}
      />

      {isRetail ? (
        <ServiceGrid services={catalogQ.data?.flatMap((c) => c.Services) ?? []} loading={catalogQ.isLoading} />
      ) : (
        <QuickLinks />
      )}

      <TodayStrip data={dash.data} loading={dash.isLoading} />

      <RecentActivity rows={recent.data ?? []} loading={recent.isLoading} names={serviceNames} />
    </div>
  );
}

/* ------------------------------------------------------------ balance card */

/**
 * The one element on this screen with real visual weight.
 *
 * Deliberately NOT a multi-stop gradient with blurred colour blobs — that reads muddy at small
 * sizes and turns brown under a warm brand ramp. This is the app's own dark surface
 * (--ui-navbar, so it matches the sidebar and the app bar) with a single low-opacity brand
 * wash and a hairline top highlight. Flat, deep, and it holds up in every theme.
 */
function BalanceCard({
  wallets, loading, error, refreshing, onRefresh,
}: {
  wallets: WalletBalance[];
  loading: boolean;
  /** Non-empty when the balance could not be READ. Never render a number in that case. */
  error: string;
  refreshing: boolean;
  onRefresh: () => void;
}) {
  const [masked, toggleMask] = useBalanceMasked();
  const total = wallets.reduce((s, w) => s + w.balance, 0);

  return (
    <section
      className="relative overflow-hidden rounded-card shadow-card"
      style={{ backgroundColor: 'var(--ui-navbar)' }}
    >
      <div className="pointer-events-none absolute inset-0 bg-gradient-to-br from-brand-500/20 via-transparent to-transparent" />
      <div className="pointer-events-none absolute inset-x-0 top-0 h-px bg-white/15" />

      <div className="relative p-5 text-white">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <p className="text-[10px] font-semibold uppercase tracking-[0.14em] text-white/50">
              Total balance
            </p>

            {error ? (
              <div className="mt-1.5">
                <p className="flex items-center gap-1.5 font-display text-base font-bold">
                  <AlertTriangle className="size-4 shrink-0 text-amber-300" />
                  Balance unavailable
                </p>
                <p className="mt-1 text-xs leading-snug text-white/60">{error}</p>
              </div>
            ) : loading ? (
              <Skeleton className="mt-2 h-8 w-44 bg-white/15" />
            ) : (
              <p className="mt-1.5 font-display text-[34px] font-extrabold leading-none tabular-nums">
                {masked ? MASK : inr(total)}
              </p>
            )}
          </div>

          <div className="-mr-2 -mt-1.5 flex shrink-0">
            <button
              type="button"
              onClick={toggleMask}
              aria-label={masked ? 'Show balance' : 'Hide balance'}
              className="grid size-9 place-items-center rounded-lg text-white/60 active:bg-white/10"
            >
              {masked ? <EyeOff className="size-[18px]" /> : <Eye className="size-[18px]" />}
            </button>
            <button
              type="button"
              onClick={onRefresh}
              aria-label="Refresh balance"
              className="grid size-9 place-items-center rounded-lg text-white/60 active:bg-white/10"
            >
              <RefreshCw className={cn('size-[18px]', refreshing && 'animate-spin')} />
            </button>
          </div>
        </div>

        {/* Per-wallet breakdown — a fixed row, not floating pills, so the numbers line up. */}
        {!error && wallets.length > 0 && (
          <div className="mt-5 flex items-stretch rounded-xl bg-white/[0.07] ring-1 ring-white/10">
            {wallets.map((w, i) => (
              <div
                key={w.walletId}
                title={w.name}
                className={cn('min-w-0 flex-1 px-3 py-2.5', i > 0 && 'border-l border-white/10')}
              >
                <p className="truncate text-[10px] font-semibold uppercase tracking-wide text-white/45">
                  {w.shortName}
                </p>
                <p className="mt-0.5 truncate font-display text-[14px] font-bold tabular-nums text-white/95">
                  {masked ? '••••' : inr(w.balance)}
                </p>
              </div>
            ))}
          </div>
        )}

        <div className="mt-4 grid grid-cols-2 gap-2.5">
          <Link
            to="/s/Fund/Request"
            onClick={() => haptic('medium')}
            className="flex items-center justify-center gap-1.5 rounded-xl bg-white py-3 text-[13px] font-semibold text-ink-900 transition-transform active:scale-[0.98]"
          >
            <Plus className="size-4" />
            Add money
          </Link>
          <Link
            to="/transactions"
            onClick={() => haptic()}
            className="flex items-center justify-center gap-1.5 rounded-xl border border-white/20 py-3 text-[13px] font-semibold text-white active:bg-white/10"
          >
            <ReceiptText className="size-4" />
            Statement
          </Link>
        </div>
      </div>
    </section>
  );
}

/* ------------------------------------------------------------- quick links */

/**
 * Non-retail roles (admin, distributor, API user) have no service grid — without this the
 * phone home page would be two small cards on an empty screen. These are the reports those
 * roles actually open; everything else stays under More.
 */
const QUICK_LINKS: { title: string; to: string; icon: LucideIcon }[] = [
  { title: 'Quick search', to: '/s/Search', icon: Search },
  { title: 'Transaction history', to: '/transactions', icon: History },
  { title: 'Daybook', to: '/s/Daybook/Report', icon: ScrollText },
  { title: 'Ledger', to: '/s/UserLedger', icon: BookOpenText },
];

function QuickLinks() {
  return (
    <section>
      <h2 className="mb-2 px-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
        Shortcuts
      </h2>
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {QUICK_LINKS.map((q, i) => (
          <Link
            key={q.to}
            to={q.to}
            onClick={() => haptic()}
            className={cn(
              'flex items-center gap-3 px-4 py-3.5 active:bg-slate-50',
              i > 0 && 'border-t border-slate-100',
            )}
          >
            <q.icon className="size-[18px] shrink-0 text-slate-400" />
            <span className="min-w-0 flex-1 truncate text-sm text-slate-700">{q.title}</span>
            <ChevronRight className="size-4 shrink-0 text-slate-300" />
          </Link>
        ))}
      </div>
    </section>
  );
}

/* ------------------------------------------------------------- today strip */

/**
 * Four numbers, one row. This used to be a 2x2 grid of full-size cards that ate half the
 * screen for figures nobody acts on directly — they are a glance, not a destination, so they
 * get a glance's worth of space. The detail lives one tap away in History.
 */
function TodayStrip({
  data, loading,
}: {
  data: { TotalSuccessAmount?: number; TotalFailedAmount?: number; TotalCommission?: number; PurchaseAmount?: number } | null | undefined;
  loading: boolean;
}) {
  const items = [
    { label: 'Sale', value: data?.TotalSuccessAmount, icon: TrendingUp, tone: 'text-success-600' },
    { label: 'Comm.', value: data?.TotalCommission, icon: BadgeIndianRupee, tone: 'text-brand-600' },
    { label: 'Failed', value: data?.TotalFailedAmount, icon: TrendingDown, tone: 'text-danger-600' },
    { label: 'Received', value: data?.PurchaseAmount, icon: ArrowDownToLine, tone: 'text-info-600' },
  ];

  return (
    <section>
      <h2 className="mb-2 px-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Today</h2>
      <div className="flex items-stretch overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {items.map((it, i) => (
          <div key={it.label} className={cn('min-w-0 flex-1 px-1.5 py-2.5 text-center', i > 0 && 'border-l border-slate-100')}>
            <p className="flex items-center justify-center gap-1 text-[10px] font-medium text-slate-400">
              <it.icon className={cn('size-3 shrink-0', it.tone)} />
              <span className="truncate">{it.label}</span>
            </p>
            {loading ? (
              <Skeleton className="mx-auto mt-1.5 h-3.5 w-12" />
            ) : (
              <p className="mt-0.5 truncate font-display text-[13px] font-bold tabular-nums text-slate-900">
                {inr(it.value)}
              </p>
            )}
          </div>
        ))}
      </div>
    </section>
  );
}

/* ------------------------------------------------------------ service grid */

function ServiceGrid({ services, loading }: { services: CatalogService[]; loading: boolean }) {
  const [showAll, setShowAll] = useState(false);
  const visible = showAll || services.length <= VISIBLE_TILES + 1 ? services : services.slice(0, VISIBLE_TILES);

  return (
    <section>
      <h2 className="mb-2 px-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Services</h2>
      <div className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
        {loading ? (
          <div className="grid grid-cols-4 gap-y-4">
            {Array.from({ length: 8 }).map((_, i) => (
              <div key={i} className="flex flex-col items-center gap-2">
                <Skeleton className="size-14 rounded-2xl" />
                <Skeleton className="h-2.5 w-12" />
              </div>
            ))}
          </div>
        ) : services.length === 0 ? (
          <p className="py-8 text-center text-sm text-slate-400">No services enabled yet.</p>
        ) : (
          <div className="grid grid-cols-4 gap-y-3">
            {visible.map((svc) => (
              <ServiceTile key={svc.ServiceID} svc={svc} />
            ))}

            {services.length > VISIBLE_TILES + 1 && (
              <button
                type="button"
                onClick={() => {
                  haptic();
                  setShowAll((v) => !v);
                }}
                className="flex flex-col items-center gap-2 rounded-xl py-1.5 active:scale-95"
              >
                <span className="grid size-14 place-items-center rounded-2xl bg-slate-100 text-slate-500">
                  {showAll ? (
                    <ChevronRight className="size-6 -rotate-90" />
                  ) : (
                    <span className="font-display text-sm font-bold">+{services.length - VISIBLE_TILES}</span>
                  )}
                </span>
                <span className="text-[11px] font-medium leading-tight text-slate-600">
                  {showAll ? 'Less' : 'All'}
                </span>
              </button>
            )}
          </div>
        )}
      </div>
    </section>
  );
}

function ServiceTile({ svc }: { svc: CatalogService }) {
  const look = serviceLook(svc.Name);
  const img = imageUrl(svc.ServiceImagePath);

  const face = (
    <>
      <span
        className={cn(
          'relative grid size-14 place-items-center overflow-hidden rounded-2xl',
          svc.IsLocked ? 'bg-slate-100' : img ? 'bg-white ring-1 ring-black/[0.04]' : look.tone,
        )}
      >
        {img ? (
          <img src={img} alt="" className={cn('size-full object-contain p-2.5', svc.IsLocked && 'opacity-40 grayscale')} />
        ) : (
          <look.icon className={cn('size-6', svc.IsLocked && 'text-slate-400')} />
        )}
        {svc.IsLocked && (
          <span className="absolute inset-0 grid place-items-center bg-slate-900/5">
            <Lock className="size-4 text-slate-500" />
          </span>
        )}
      </span>
      <span
        className={cn(
          'line-clamp-2 px-0.5 text-center text-[11px] font-medium leading-tight',
          svc.IsLocked ? 'text-slate-400' : 'text-slate-600',
        )}
      >
        {svc.Name}
      </span>
    </>
  );

  // Locked tiles stay on the grid but don't open. The server says why, so "KYC pending" reads as
  // something the user can fix rather than an admin lock they can't.
  if (svc.IsLocked) {
    return (
      <div
        className="flex cursor-not-allowed flex-col items-center gap-2 py-1.5"
        title={
          svc.LockReason === 'KYC pending'
            ? 'Your KYC is not approved yet. Complete your KYC from your profile to use this service.'
            : 'This service is locked for your account. Please contact your service provider.'
        }
      >
        {face}
      </div>
    );
  }

  return (
    <Link
      to={serviceHref(svc)}
      onClick={() => haptic()}
      className="flex flex-col items-center gap-2 rounded-xl py-1.5 transition-transform active:scale-95"
    >
      {face}
    </Link>
  );
}

/* ---------------------------------------------------------- recent activity */

function statusTone(status?: string): { dot: string; text: string } {
  const s = (status ?? '').toLowerCase();
  if (s.includes('success')) return { dot: 'bg-success-500', text: 'text-success-600' };
  if (s.includes('fail') || s.includes('reject')) return { dot: 'bg-danger-500', text: 'text-danger-600' };
  return { dot: 'bg-warning-500', text: 'text-warning-600' };
}

function RecentActivity({
  rows, loading, names,
}: {
  rows: TransactionRow[];
  loading: boolean;
  names: ServiceNames;
}) {
  return (
    <section>
      <div className="mb-2 flex items-center justify-between px-1">
        <h2 className="text-[11px] font-semibold uppercase tracking-wider text-slate-400">Recent</h2>
        <Link to="/transactions" onClick={() => haptic()} className="flex items-center gap-0.5 text-xs font-medium text-brand-600">
          See all <ChevronRight className="size-3.5" />
        </Link>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {loading ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-9 rounded-lg" />)}
          </div>
        ) : rows.length === 0 ? (
          <p className="py-10 text-center text-sm text-slate-400">No transactions today yet.</p>
        ) : (
          rows.map((r, i) => {
            const tone = statusTone(r.Status);
            const title = txnTitle(r, names);
            const subtitle = txnSubtitle(r, names);
            return (
              <div
                key={r.TransactionMasterID ?? r.TxnID ?? i}
                className={cn('flex items-center gap-3 px-4 py-3', i > 0 && 'border-t border-slate-100')}
              >
                <span className={cn('size-2 shrink-0 rounded-full', tone.dot)} />
                <span className="min-w-0 flex-1">
                  <span className="block truncate text-sm font-medium text-slate-800">{title}</span>
                  <span className="block truncate text-xs text-slate-400">
                    {[subtitle, txnWhen(r.DateTime)].filter(Boolean).join(' · ')}
                  </span>
                </span>
                <span className="shrink-0 text-right">
                  <span className="block font-display text-sm font-bold tabular-nums text-slate-900">
                    {inr(r.Amount)}
                  </span>
                  <span className={cn('block text-[11px] font-medium', tone.text)}>{r.Status}</span>
                </span>
              </div>
            );
          })
        )}
      </div>
    </section>
  );
}
