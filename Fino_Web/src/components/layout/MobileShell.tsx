import { useEffect, useMemo, useState } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  AlertTriangle, ChevronRight, History, Home, LogOut, MoreHorizontal, UserRound, Wallet, X,
} from 'lucide-react';
import { getMenu, type MenuNode } from '@/api/menu.api';
import { getMyWallets } from '@/api/wallet.api';
import { useAuth } from '@/features/auth/AuthContext';
import { useBrand } from '@/theme/BrandProvider';
import { inr } from '@/lib/format';
import { roleLabel } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { haptic, setNativeStatusBar } from '@/lib/native';
import { useBalanceMasked } from '@/lib/balanceMask';
import { Skeleton } from '@/components/ui/Skeleton';
import { NotificationsBell } from './NotificationsBell';
import { pickIcon } from './iconMap';
import { resolveHref } from './resolveRoute';
import { BrandGlyph } from '@/components/ui/BrandMark';

/**
 * The console's phone layout — used inside the Android shell and on phone-sized browsers.
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

  return (
    <div className="flex h-[100dvh] flex-col overflow-hidden bg-canvas">
      <MobileHeader showBalance={!onHome} />

      <main id="app-scroll" className="min-h-0 flex-1 overflow-y-auto overscroll-y-contain">
        <div className="px-4 pb-28 pt-4">
          <Outlet />
        </div>
      </main>

      <TabBar onMore={() => setMoreOpen(true)} moreActive={moreOpen} />

      {moreOpen && <MoreSheet onClose={() => setMoreOpen(false)} />}
    </div>
  );
}

/* ------------------------------------------------------------------ header */

function MobileHeader({ showBalance }: { showBalance: boolean }) {
  const { user } = useAuth();
  const { appName } = useBrand();

  const name = user?.PersonName?.trim() || user?.FirmName?.trim() || 'User';

  return (
    <header className="shrink-0 text-[var(--ui-fore)]" style={{ backgroundColor: 'var(--ui-appbar)' }}>
      <div className="flex h-14 items-center gap-3 px-4">
        <Link to="/profile" className="flex min-w-0 flex-1 items-center gap-2.5" onClick={() => haptic()}>
          <BrandGlyph className="size-9 shrink-0 rounded-xl" />
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
  const { user } = useAuth();
  const userId = user?.UserID ?? 0;
  const [masked] = useBalanceMasked();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['my-wallets', userId],
    queryFn: () => getMyWallets(userId),
    enabled: userId > 0,
    refetchInterval: 30_000,
    retry: 1,
  });

  if (isError) {
    // A failed read is never rendered as ₹0.00 — see getMyWallets.
    return (
      <button
        type="button"
        onClick={() => refetch()}
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

function TabBar({ onMore, moreActive }: { onMore: () => void; moreActive: boolean }) {
  return (
    <nav
      className="shrink-0 border-t border-slate-200 bg-white"
      style={{ paddingBottom: 'env(safe-area-inset-bottom, 0px)' }}
    >
      <div className="flex items-stretch">
        {TABS.map((t) => (
          <NavLink
            key={t.to}
            to={t.to}
            onClick={() => haptic()}
            className={({ isActive }) =>
              cn(
                'flex flex-1 flex-col items-center gap-1 py-2.5 text-[11px] font-medium transition-colors',
                isActive && !moreActive ? 'text-brand-600' : 'text-slate-400',
              )
            }
          >
            {({ isActive }) => (
              <>
                <t.icon className={cn('size-[22px]', isActive && !moreActive && 'stroke-[2.4]')} />
                {t.label}
              </>
            )}
          </NavLink>
        ))}

        <button
          type="button"
          onClick={() => {
            haptic();
            onMore();
          }}
          className={cn(
            'flex flex-1 flex-col items-center gap-1 py-2.5 text-[11px] font-medium transition-colors',
            moreActive ? 'text-brand-600' : 'text-slate-400',
          )}
        >
          <MoreHorizontal className={cn('size-[22px]', moreActive && 'stroke-[2.4]')} />
          More
        </button>
      </div>
    </nav>
  );
}

/* -------------------------------------------------------------- more sheet */

/** Everything else the role may open — same menu tree the desktop sidebar renders. */
function MoreSheet({ onClose }: { onClose: () => void }) {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();
  const { data, isLoading } = useQuery({ queryKey: ['menu'], queryFn: getMenu, staleTime: 300_000 });

  const sections = useMemo(() => (data ?? []).filter((n) => !/dashboard/i.test(n.name)), [data]);
  const name = user?.PersonName?.trim() || user?.FirmName?.trim() || 'User';

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-canvas animate-rise">
      <header
        className="flex h-14 shrink-0 items-center justify-between px-4 text-[var(--ui-fore)]"
        style={{ backgroundColor: 'var(--ui-appbar)' }}
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
