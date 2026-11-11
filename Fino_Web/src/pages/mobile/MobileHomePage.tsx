import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  BookOpenText, ChevronRight, History, ScrollText, Search, type LucideIcon,
} from 'lucide-react';
import { useLiveWallets } from '@/features/live/useLiveSummary';
import { getDashboard } from '@/api/dashboard.api';
import { fetchCatalog } from '@/api/catalog.api';
import { selectReport } from '@/api/transactionReport.api';
import { useAuth } from '@/features/auth/AuthContext';
import { useUISettings } from '@/theme/UISettingsProvider';
import { blockGapClass } from '@/theme/mobileLayouts';
import { apiDate } from '@/lib/format';
import { isRetailer, UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';
import type { ServiceNames } from '@/lib/txn';
import { QuickServices } from '@/components/dashboard/QuickServices';
import { NewsBanner } from '@/components/layout/NewsBanner';
import { WorkQueueStrip } from '@/components/layout/WorkQueueStrip';
import { BalanceBlock } from './home/BalanceBlock';
import { ServicesBlock } from './home/ServicesBlock';
import { StatsBlock } from './home/StatsBlock';
import { ActivityBlock } from './home/ActivityBlock';

/**
 * The phone home screen.
 *
 * This file owns the DATA and the ORDER of the screen; what each block looks like belongs to
 * the block itself (`home/*`), picked by the layout the admin published. That split is what
 * makes the Android app's design a server setting: the queries below never change, so a new
 * preset is a rendering decision and never a new set of requests.
 *
 *   balance  →  pinned shortcuts  →  services / today  →  recent activity
 *
 * Screens OTHER than this one get a condensed balance strip in the app bar (MobileShell), so
 * the figure is never more than a glance away without being drawn twice on the same screen.
 */

export default function MobileHomePage() {
  const { user } = useAuth();
  // The admin's published phone layout decides the shape of this screen — which blocks
  // appear, in what order, and how each one is drawn. See theme/mobileLayouts.ts.
  const { mobileLayout: layout } = useUISettings();
  const roleId = user?.UserTypeID ?? 0;
  const isRetail = roleId === UserTypeCode.Retailer || roleId === UserTypeCode.Client;

  const [dateStr] = useState(() => apiDate(new Date()));
  const date = useMemo(() => new Date(`${dateStr}T00:00:00`), [dateStr]);

  // Balances come from the shared live feed — same numbers the shell's balance bar shows, and
  // no second poll of its own.
  const wallets = useLiveWallets();
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
    queryKey: ['recent-txns', dateStr, layout.recentCount],
    queryFn: () =>
      selectReport({ fromDate: dateStr, toDate: dateStr, pageNumber: 1, pageSize: layout.recentCount + 1 }),
    refetchInterval: 60_000,
  });

  const balance = layout.showBalanceCard ? (
    <BalanceBlock
      style={layout.balanceStyle}
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
  ) : null;

  const services = isRetail ? (
    <ServicesBlock categories={catalogQ.data ?? []} loading={catalogQ.isLoading} layout={layout} />
  ) : (
    <QuickLinks />
  );

  const today = layout.showTodayStats ? (
    <StatsBlock style={layout.statsStyle} data={dash.data} loading={dash.isLoading} />
  ) : null;

  const activity = (
    <ActivityBlock
      style={layout.activityStyle}
      rows={(recent.data ?? []).slice(0, layout.recentCount)}
      loading={recent.isLoading}
      names={serviceNames}
    />
  );

  // The hero balance is full-bleed and bleeds up into the app bar, so MobileShell holds back
  // the work-queue strip and the news banner on that layout and this screen places them
  // directly under the hero instead. Same condition on both sides — never both, never neither.
  const ownsChrome = layout.showBalanceCard && layout.balanceStyle === 'hero';

  return (
    <div className={blockGapClass(layout.density)}>
      {balance}
      {ownsChrome && (
        <>
          <WorkQueueStrip />
          <NewsBanner />
        </>
      )}
      {/* The retailer's own shortcuts sit directly under the balance, above the full catalogue:
          on a phone the whole point is not having to hunt for the four things you do all day.
          Retailer only — no other role works a service grid from this screen, and the API refuses
          QuickService/Select for them anyway, so the panel would render empty. */}
      {isRetailer(roleId) && <QuickServices compact />}
      {/* "Numbers first" is a preset decision, not a role one: an operator watching figures all
          day should not scroll past a service list they rarely tap. */}
      {layout.statsFirst ? (
        <>
          {today}
          {services}
        </>
      ) : (
        <>
          {services}
          {today}
        </>
      )}
      {activity}
    </div>
  );
}

/* ------------------------------------------------------------- quick links */

/**
 * Non-retail roles (admin, distributor, API user) have no service catalogue — without this the
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
      <h2 className="mb-2 px-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Shortcuts</h2>
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {QUICK_LINKS.map((q, i) => (
          <Link
            key={q.to}
            to={q.to}
            onClick={() => haptic()}
            className={cn('flex items-center gap-3 px-4 py-3.5 active:bg-slate-50', i > 0 && 'border-t border-slate-100')}
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
