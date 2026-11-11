import { useEffect, useMemo, useState } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  AlertTriangle, ArrowLeftRight, ChevronRight, History, Home, LogOut, MoreHorizontal, UserRound,
  Wallet, X,
} from 'lucide-react';
import { getMenu, type MenuNode } from '@/api/menu.api';
import { useAuth } from '@/features/auth/AuthContext';
import { useLiveSummary } from '@/features/live/useLiveSummary';
import { useBrand } from '@/theme/BrandProvider';
import { useUISettings } from '@/theme/UISettingsProvider';
import { headerStyleFor, type HeaderStyle, type TabStyle } from '@/theme/mobileLayouts';
import { inr } from '@/lib/format';
import { roleLabel } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { haptic, setNativeStatusBar } from '@/lib/native';
import { useBalanceMasked } from '@/lib/balanceMask';
import { Skeleton } from '@/components/ui/Skeleton';
import { NewsBanner } from './NewsBanner';
import { NotificationsBell } from './NotificationsBell';
import { SystemDownBanner } from './SystemDownBanner';
import { WorkQueueStrip } from './WorkQueueStrip';
import { pickIcon } from './iconMap';
import { resolveHref } from './resolveRoute';
import { BrandBadge } from '@/components/ui/BrandMark';

/**
 * The console's phone layout — used inside the Android shell and on phone-sized browsers.
 *
 * The app bar treatment and the tab bar are driven by the layout preset the admin publishes
 * (Settings → Appearance → Mobile app), resolved in `theme/mobileLayouts.ts`. That is what
 * makes the Android UI changeable without shipping a new APK.
 *
 * Three decisions worth knowing:
 *
 * 1. **Balance is always reachable, but never shown twice.** Home carries the full balance card;
 *    every OTHER screen gets a condensed strip in the app bar that taps back to Home. Showing
 *    both at once was the duplication that made the app bar feel cramped.
 *
 * 2. **The service grid exists in exactly one place — Home.** There is no "Services" tab: it
 *    used to open the recharge page, which shows its own service chip row, so the same catalog
 *    appeared twice in two shapes. Tabs are Home, History, Fund and More.
 *
 * 3. **No theme switcher here.** Look-and-feel is an operator decision made once from the
 *    desktop console (Theme Studio), not something a retailer flips on their phone. The
 *    presets still apply — they just aren't changeable from the app.
 */

export function MobileShell() {
  const [moreOpen, setMoreOpen] = useState(false);
  const location = useLocation();
  const { mobileLayout } = useUISettings();

  // NOTE: AppShell already locks the document scroll while the console is mounted, so the
  // <main> below is the only scroll region and the tab bar stays pinned. Don't lock it here
  // too — two effects restoring the same style would leave it stuck on unmount.

  // Paint the Android status bar with the same colour as the header so the two read as one
  // surface. Re-runs when the theme changes (the operator can repaint the app bar).
  const { themeKey } = useBrand();
  useEffect(() => {
    const appbar = getComputedStyle(document.documentElement)
      .getPropertyValue('--ui-appbar')
      .trim();
    if (appbar) setNativeStatusBar(appbar);
  }, [themeKey]);

  useEffect(() => setMoreOpen(false), [location.pathname]);

  const onHome = location.pathname === '/dashboard';

  // The `hero` balance block is full-bleed and bleeds UP into the app bar, so nothing may sit
  // between it and the header. On that one layout the home page renders the work-queue strip
  // and the news banner itself, directly under the hero — see MobileHomePage, which derives
  // this from the same resolved layout so the two can never both draw them.
  const heroHome = onHome && mobileLayout.showBalanceCard && mobileLayout.balanceStyle === 'hero';

  return (
    <div className="flex h-[100dvh] flex-col overflow-hidden bg-canvas">
      {/* Home carries its own balance block, so the app bar strip would be the second copy —
          unless the layout has the block switched off, in which case the strip IS the balance. */}
      <MobileHeader
        showBalance={!onHome || !mobileLayout.showBalanceCard}
        headerStyle={mobileLayout.headerStyle}
      />

      <main id="app-scroll" className="min-h-0 flex-1 overflow-y-auto overscroll-y-contain">
        <div className="px-4 pb-28 pt-4">
          {/* Same strip as the desktop console — a retailer's phone is where "is my fund request
              approved yet?" actually gets asked. Horizontally scrollable, so a narrow handset
              never has to wrap it onto a second line. */}
          {/* Unconditional, even on the hero home: the counter operator is the person who will
              otherwise try a recharge and be refused without being told why. */}
          <SystemDownBanner className="mb-3" />
          {!heroHome && <WorkQueueStrip className="mb-3" />}
          {onHome && !heroHome && <NewsBanner className="mb-3" />}
          <Outlet />
        </div>
      </main>

      <TabBar onMore={() => setMoreOpen(true)} moreActive={moreOpen} tabStyle={mobileLayout.tabStyle} />

      {moreOpen && <MoreSheet onClose={() => setMoreOpen(false)} headerStyle={mobileLayout.headerStyle} />}
    </div>
  );
}

/* ------------------------------------------------------------------ header */

function MobileHeader({ showBalance, headerStyle }: { showBalance: boolean; headerStyle: HeaderStyle }) {
  const { user } = useAuth();
  const { appName } = useBrand();

  const name = user?.PersonName?.trim() || user?.FirmName?.trim() || 'User';

  return (
    <header className="shrink-0 text-[var(--ui-fore)]" style={headerStyleFor(headerStyle)}>
      <div className="flex h-14 items-center gap-3 px-4">
        <Link to="/profile" className="flex min-w-0 flex-1 items-center gap-2.5" onClick={() => haptic()}>
          <BrandBadge className="h-9 shrink-0 rounded-xl" />
          <span className="min-w-0">
            <span className="block truncate text-[14px] font-semibold leading-tight">{name}</span>
            <span className="block truncate text-[11px] leading-tight text-[var(--ui-fore)]/60">
              {roleLabel(user?.UserTypeID ?? 0, user?.UserType ?? '') || appName}
            </span>
          </span>
        </Link>

        <NotificationsBell />
      </div>

      {showBalance && <BalanceBar />}
    </header>
  );
}

/**
 * Condensed balances for screens other than Home. Tapping it goes back to Home, where the
 * full card lives — so this is a reminder, not a second control surface.
 */
function BalanceBar() {
  const [masked] = useBalanceMasked();

  // The shared live feed, not a poll of its own. This bar sits on EVERY mobile screen, so a
  // private 30-second refresh here was, at a thousand users, the single most expensive thing the
  // app did — and it duplicated what the header was already asking for.
  const { summary, isLoading, error, refresh } = useLiveSummary();
  const data = summary.wallets;
  const isError = Boolean(error) || (!isLoading && !summary.walletsAvailable && data.length === 0);

  if (isError) {
    // A failed read is never rendered as ₹0.00 — a wrong balance is worse than no balance.
    return (
      <button
        type="button"
        onClick={() => refresh()}
        className="flex w-full items-center gap-1.5 border-t border-white/10 px-4 py-2 text-left"
      >
        <AlertTriangle className="size-3.5 shrink-0 text-amber-300" />
        <span className="truncate text-[11px] font-medium">Balance unavailable — tap to retry</span>
      </button>
    );
  }

  return (
    <Link
      to="/dashboard"
      onClick={() => haptic()}
      className="flex items-stretch gap-0 border-t border-white/10 px-4 py-2"
    >
      {isLoading
        ? Array.from({ length: 3 }).map((_, i) => (
            <span key={i} className="flex-1 px-2">
              <Skeleton className="h-3.5 w-full rounded bg-white/15" />
            </span>
          ))
        : (data ?? []).map((w, i) => (
            <span
              key={w.walletId}
              title={w.name}
              className={cn('min-w-0 flex-1 px-2 first:pl-0 last:pr-0', i > 0 && 'border-l border-white/12')}
            >
              <span className="block truncate text-[9px] font-semibold uppercase tracking-[0.1em] text-[var(--ui-fore)]/50">
                {w.shortName}
              </span>
              <span className="block truncate font-display text-[13px] font-bold tabular-nums">
                {masked ? '••••' : inr(w.balance)}
              </span>
            </span>
          ))}
    </Link>
  );
}

/* ---------------------------------------------------------------- tab bar */

const TABS = [
  { to: '/dashboard', label: 'Home', icon: Home },
  { to: '/transactions', label: 'History', icon: History },
  { to: '/s/Fund/Request', label: 'Fund', icon: Wallet },
] as const;

/**
 * The bottom tabs, in the three shapes a preset can ask for:
 *  - `labelled` icon + text (the default; clearest for a first-time retailer)
 *  - `icons`    icon only, shorter bar — buys back a row of content on a small phone
 *  - `floating` a detached pill, the look of a consumer payments app
 */
function TabBar({
  onMore,
  moreActive,
  tabStyle,
}: {
  onMore: () => void;
  moreActive: boolean;
  tabStyle: TabStyle;
}) {
  const showLabels = tabStyle !== 'icons';
  const floating = tabStyle === 'floating';

  const itemClass = (active: boolean) =>
    cn(
      'flex flex-1 flex-col items-center gap-1 transition-colors',
      showLabels ? 'py-2.5 text-[11px] font-medium' : 'py-3',
      active ? 'text-brand-600' : 'text-slate-400',
    );

  return (
    <nav
      className={cn(
        'shrink-0',
        floating
          ? 'mx-3 rounded-2xl border border-slate-200/80 bg-white/95 shadow-pop backdrop-blur'
          : 'border-t border-slate-200 bg-white',
      )}
      // A detached bar has to clear the gesture area itself — the safe-area inset is margin
      // for it and padding for the attached bar, or the last row of tabs sits under the
      // Android navigation pill.
      style={
        floating
          ? { marginBottom: 'calc(env(safe-area-inset-bottom, 0px) + 0.75rem)' }
          : { paddingBottom: 'env(safe-area-inset-bottom, 0px)' }
      }
    >
      <div className="flex items-stretch">
        {TABS.map((t) => (
          <NavLink
            key={t.to}
            to={t.to}
            onClick={() => haptic()}
            aria-label={t.label}
            className={({ isActive }) => itemClass(isActive && !moreActive)}
          >
            {({ isActive }) => (
              <>
                <t.icon className={cn('size-[22px]', isActive && !moreActive && 'stroke-[2.4]')} />
                {showLabels && t.label}
              </>
            )}
          </NavLink>
        ))}

        <button type="button" onClick={() => { haptic(); onMore(); }} aria-label="More" className={itemClass(moreActive)}>
          <MoreHorizontal className={cn('size-[22px]', moreActive && 'stroke-[2.4]')} />
          {showLabels && 'More'}
        </button>
      </div>
    </nav>
  );
}

/* -------------------------------------------------------------- more sheet */

/** Everything else the role may open — same menu tree the desktop sidebar renders. */
function MoreSheet({ onClose, headerStyle }: { onClose: () => void; headerStyle: HeaderStyle }) {
  const { user, signOut, profiles, switchProfile } = useAuth();
  const navigate = useNavigate();
  const [switching, setSwitching] = useState(false);

  // The other role this mobile number holds. Empty for everyone else, so the sheet is
  // unchanged for them.
  const otherProfiles = profiles.filter((p) => p.UserID !== user?.UserID);
  const { data, isLoading } = useQuery({ queryKey: ['menu'], queryFn: getMenu, staleTime: 300_000 });

  const sections = useMemo(() => (data ?? []).filter((n) => !/dashboard/i.test(n.name)), [data]);
  const name = user?.PersonName?.trim() || user?.FirmName?.trim() || 'User';

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-canvas animate-rise">
      <header
        className="flex h-14 shrink-0 items-center justify-between px-4 text-[var(--ui-fore)]"
        style={headerStyleFor(headerStyle)}
      >
        <span className="font-display text-base font-bold">Menu</span>
        <button type="button" onClick={onClose} aria-label="Close" className="-mr-2 grid size-10 place-items-center">
          <X className="size-5" />
        </button>
      </header>

      <div
        className="min-h-0 flex-1 overflow-y-auto overscroll-y-contain px-4 py-4"
        style={{ paddingBottom: 'calc(env(safe-area-inset-bottom, 0px) + 1rem)' }}
      >
        <button
          type="button"
          onClick={() => {
            onClose();
            navigate('/profile');
          }}
          className="mb-4 flex w-full items-center gap-3 rounded-card border border-slate-200/70 bg-white p-4 text-left shadow-card active:scale-[0.99]"
        >
          <span className="grid size-11 place-items-center rounded-full bg-brand-50 text-brand-600">
            <UserRound className="size-5" />
          </span>
          <span className="min-w-0 flex-1">
            <span className="block truncate text-sm font-semibold text-slate-900">{name}</span>
            <span className="block truncate text-xs text-slate-500">
              {roleLabel(user?.UserTypeID ?? 0, user?.UserType ?? '')} · View profile
            </span>
          </span>
          <ChevronRight className="size-5 shrink-0 text-slate-300" />
        </button>

        {otherProfiles.length > 0 && (
          <div className="mb-4 overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
            <p className="px-4 pt-3 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
              Switch to
            </p>
            {otherProfiles.map((p) => (
              <button
                key={p.UserID}
                type="button"
                disabled={switching}
                onClick={async () => {
                  haptic('medium');
                  setSwitching(true);
                  try {
                    // switchProfile lands on the dashboard itself; this only closes the sheet.
                    const res = await switchProfile(p.UserID);
                    if (res.ok) onClose();
                  } finally {
                    setSwitching(false);
                  }
                }}
                className="flex w-full items-center gap-3 px-4 py-3 text-left active:bg-slate-50 disabled:opacity-60"
              >
                <span className="grid size-9 shrink-0 place-items-center rounded-full bg-slate-100 text-slate-500">
                  <ArrowLeftRight className="size-4" />
                </span>
                <span className="min-w-0 flex-1">
                  <span className="block truncate text-sm font-semibold text-slate-800">
                    {roleLabel(p.UserTypeID, p.UserType)}
                  </span>
                  <span className="block truncate text-xs text-slate-500">{p.FirmName}</span>
                </span>
                <ChevronRight className="size-5 shrink-0 text-slate-300" />
              </button>
            ))}
          </div>
        )}

        {isLoading ? (
          <p className="py-10 text-center text-sm text-slate-400">Loading menu…</p>
        ) : (
          sections.map((section) => <MenuSection key={section.name} node={section} onNavigate={onClose} />)
        )}

        <button
          type="button"
          onClick={() => {
            haptic('medium');
            signOut();
          }}
          className="mt-4 flex w-full items-center justify-center gap-2 rounded-card border border-danger-500/20 bg-danger-50 py-3.5 text-sm font-semibold text-danger-600 active:scale-[0.99]"
        >
          <LogOut className="size-4" />
          Sign out
        </button>
      </div>
    </div>
  );
}

/* -------------------------------------------------------------- menu list */

function MenuSection({ node, onNavigate }: { node: MenuNode; onNavigate: () => void }) {
  const children = node.children ?? [];

  // A top-level entry with no children is a link in its own right.
  const rows: MenuNode[] = children.length > 0 ? children : [node];

  return (
    <section className="mb-4">
      <h2 className="mb-1.5 px-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
        {node.name}
      </h2>
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {rows.map((row, i) => {
          const Icon = pickIcon(row.name, row.icon);
          return (
            <Link
              key={`${row.name}-${i}`}
              to={resolveHref(row)}
              onClick={() => {
                haptic();
                onNavigate();
              }}
              className={cn(
                'flex items-center gap-3 px-4 py-3.5 active:bg-slate-50',
                i > 0 && 'border-t border-slate-100',
              )}
            >
              <Icon className="size-[18px] shrink-0 text-slate-400" />
              <span className="min-w-0 flex-1 truncate text-sm text-slate-700">{row.name}</span>
              <ChevronRight className="size-4 shrink-0 text-slate-300" />
            </Link>
          );
        })}
      </div>
    </section>
  );
}
