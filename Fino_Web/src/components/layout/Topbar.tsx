import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { ArrowLeftRight, ChevronDown, LogOut, Menu, RefreshCw, Settings, ShieldCheck, UserRound } from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { useSubscriptionStatus } from '@/features/subscription/useSubscriptionStatus';
import { cn } from '@/lib/cn';
import { useLiveSummary } from '@/features/live/useLiveSummary';
import { useUISettings } from '@/theme/UISettingsProvider';
import { roleLabel, UserTypeCode } from '@/lib/roles';
import { GlobalSearch } from './GlobalSearch';
import { NotificationsBell } from './NotificationsBell';
import { TopNav } from './TopNav';
import { BrandLockup } from '@/components/ui/BrandMark';

/**
 * Header renewal counter. For users actually required to subscribe (feature on, not exempt,
 * transacting role) it shows a circular progress ring of days left — green, amber ≤ 7 days,
 * and a red "Renew" pill once lapsed. The ring depletes as expiry nears (30-day reference).
 * Links to the subscribe page.
 */
function SubscriptionCounter() {
  const status = useSubscriptionStatus();
  if (!status?.Required) return null;

  const lapsed = !status.Active;
  if (lapsed) {
    return (
      <Link
        to="/s/ServiceSubscriptionCharge"
        title="Subscription expired — tap to renew"
        className="flex h-9 items-center gap-1.5 rounded-full bg-danger-50 px-3 text-danger-600 ring-1 ring-danger-500/25 transition-transform hover:scale-105"
      >
        <RefreshCw className="size-4" />
        <span className="text-xs font-semibold">Renew</span>
      </Link>
    );
  }

  const days = status.DaysRemaining;
  const low = days <= 7;
  const color = low ? '#d97706' /* warning-600 */ : '#059669' /* success-600 */;
  const R = 15;
  const C = 2 * Math.PI * R;
  const frac = Math.max(0.06, Math.min(1, days / 30));

  return (
    <Link
      to="/s/ServiceSubscriptionCharge"
      title={`${days} day${days === 1 ? '' : 's'} left · ${status.PlanName}`}
      className="relative grid size-9 place-items-center rounded-full transition-transform hover:scale-105"
    >
      <svg viewBox="0 0 36 36" className="size-9 -rotate-90">
        <circle cx="18" cy="18" r={R} fill="none" stroke="currentColor" strokeWidth="3" className="text-slate-200" />
        <circle
          cx="18" cy="18" r={R} fill="none" stroke={color} strokeWidth="3" strokeLinecap="round"
          strokeDasharray={C} strokeDashoffset={C * (1 - frac)}
        />
      </svg>
      <span className="absolute inset-0 grid place-items-center text-[11px] font-bold leading-none tabular-nums" style={{ color }}>
        {days}
      </span>
    </Link>
  );
}

/**
 * Refresh everything on screen, from the header.
 *
 * On a phone the app refreshes by pulling the page down; on a desktop there is no such gesture, so
 * an operator who had just taken a payment at the counter had no way to make the panel show it
 * other than reloading the browser — which drops them back to the dashboard and re-downloads the
 * whole app. This does what the reload did and nothing more: it re-reads the live balances and
 * re-runs the queries the OPEN screen is actually using (its report, its list, its dashboard), so
 * the page they are standing on updates in place.
 */
function RefreshButton({ tone }: { tone: string }) {
  const qc = useQueryClient();
  const toast = useToast();
  const { refresh } = useLiveSummary();
  const [busy, setBusy] = useState(false);

  const onRefresh = async () => {
    if (busy) return;
    setBusy(true);
    try {
      refresh();                                   // balances + work-queue counts
      await qc.refetchQueries({ type: 'active' }); // whatever this screen is showing
      toast.success('Updated.');
    } catch {
      toast.error('Could not refresh. Check your connection and try again.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <button
      type="button"
      onClick={onRefresh}
      disabled={busy}
      title="Refresh this page"
      aria-label="Refresh this page"
      aria-busy={busy}
      className={`hidden size-9 place-items-center rounded-lg transition-colors hover:bg-black/10 disabled:opacity-60 sm:grid ${tone}`}
    >
      <RefreshCw className={cn('size-[1.15rem]', busy && 'animate-spin')} />
    </button>
  );
}

export function Topbar({ onMenuClick }: { onMenuClick: () => void }) {
  const { user, signOut, profiles, switchProfile } = useAuth();
  const toast = useToast();

  const { orientation } = useUISettings();
  const [userOpen, setUserOpen] = useState(false);
  const [switching, setSwitching] = useState(false);

  // The other logins this mobile number holds. Empty for everyone who has not set up a second
  // role, and the menu then looks exactly as it always did — no dead switcher on every screen.
  const otherProfiles = profiles.filter((p) => p.UserID !== user?.UserID);

  // Appearance is the whole network's branding, not a personal setting — see routes/AdminRoute.
  const isAdmin = (user?.UserTypeID ?? 0) === UserTypeCode.Admin;

  const onSwitch = async (userId: number, label: string) => {
    setSwitching(true);
    try {
      const res = await switchProfile(userId);
      if (res.ok) toast.success(`Now working as ${label}.`);
      else toast.error(res.message);
    } finally {
      setSwitching(false);
      setUserOpen(false);
    }
  };

  const displayName = user?.PersonName?.trim() || user?.FirmName?.trim() || 'User';
  const initials = displayName.slice(0, 1).toUpperCase();
  const role = roleLabel(user?.UserTypeID ?? 0, user?.UserType ?? '');
  const horizontal = orientation === 'horizontal';

  // In horizontal mode the header is painted with the brand appbar colour, which is usually dark.
  // Every control on it has to switch with it — slate-500 icons and slate-800 text on a dark blue
  // bar are effectively invisible, which is most of why horizontal mode looked broken. These three
  // classes carry that switch to the icons, the secondary text and the hover surfaces.
  const foreCls = horizontal ? 'text-[var(--ui-fore)]' : 'text-slate-500';
  const mutedCls = horizontal ? 'text-[var(--ui-fore)]/70' : 'text-slate-400';

  return (
    <header
      className={horizontal
        ? 'sticky top-0 z-30 flex h-[4.75rem] items-center gap-2 px-4 shadow-sm sm:px-6'
        : 'sticky top-0 z-30 flex h-[4.75rem] items-center gap-2 border-b border-slate-200 bg-white/85 px-4 backdrop-blur sm:px-6'}
      style={horizontal ? { backgroundColor: 'var(--ui-appbar)', color: 'var(--ui-fore)' } : undefined}
    >
      <button
        type="button"
        onClick={onMenuClick}
        className={`grid size-9 place-items-center rounded-lg hover:bg-black/10 lg:hidden ${foreCls}`}
        aria-label="Open menu"
      >
        <Menu className="size-5" />
      </button>

      {horizontal && (
        <BrandLockup tone="dark" size="sm" className="pr-2" />
      )}

      {horizontal ? <TopNav /> : <><GlobalSearch /><div className="flex-1" /></>}

      <RefreshButton tone={foreCls} />

      <SubscriptionCounter />

      <NotificationsBell />

      {/* The quick theme switcher was removed from the bar — theming lives in Appearance / Theme
          Studio (reachable from the user menu below), so it no longer clutters every screen. */}

      {/* User menu */}
      <div className="relative">
        <button
          type="button"
          onClick={() => setUserOpen((v) => !v)}
          className={`flex items-center gap-2.5 rounded-lg p-1 pr-2 transition-colors ${horizontal ? "hover:bg-white/10" : "hover:bg-slate-100"}`}
        >
          <span className="grid size-9 place-items-center rounded-full bg-brand-600 text-sm font-semibold text-white">
            {initials}
          </span>
          <span className="hidden text-left leading-tight sm:block">
            <span className={`block max-w-[12rem] truncate text-sm font-medium ${horizontal ? "text-[var(--ui-fore)]" : "text-slate-800"}`}>
              {displayName}
            </span>
            <span className={`block text-xs ${mutedCls}`}>{role}</span>
          </span>
          <ChevronDown className={`size-4 ${mutedCls}`} />
        </button>

        {userOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setUserOpen(false)} />
            <div className="absolute right-0 z-20 mt-2 w-60 overflow-hidden rounded-xl border border-slate-200 bg-white p-1.5 shadow-pop">
              <div className="border-b border-slate-100 px-3 py-2.5">
                <p className="truncate text-sm font-medium text-slate-800">{displayName}</p>
                <p className="truncate text-xs text-slate-400">{user?.MobileNumber}</p>
              </div>
              <Link
                to="/profile"
                onClick={() => setUserOpen(false)}
                className="mt-1 flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-100"
              >
                <UserRound className="size-4" />
                My profile
              </Link>
              <Link
                to="/s/AccountSecurity"
                onClick={() => setUserOpen(false)}
                className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-100"
              >
                <ShieldCheck className="size-4" />
                Password &amp; MPIN
              </Link>

              {/* Account switcher — only for a number that carries two roles. */}
              {otherProfiles.length > 0 && (
                <div className="my-1 border-y border-slate-100 py-1">
                  <p className="px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
                    Switch to
                  </p>
                  {otherProfiles.map((p) => {
                    const label = roleLabel(p.UserTypeID, p.UserType);
                    return (
                      <button
                        key={p.UserID}
                        type="button"
                        disabled={switching}
                        onClick={() => void onSwitch(p.UserID, label)}
                        className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-sm text-slate-700 transition-colors hover:bg-slate-100 disabled:opacity-60"
                      >
                        <ArrowLeftRight className="size-4 shrink-0" />
                        <span className="min-w-0 flex-1">
                          <span className="block truncate font-medium">{label}</span>
                          <span className="block truncate text-xs text-slate-400">{p.FirmName}</span>
                        </span>
                      </button>
                    );
                  })}
                </div>
              )}
              {isAdmin && (
                <Link
                  to="/appearance"
                  onClick={() => setUserOpen(false)}
                  className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-100"
                >
                  <Settings className="size-4" />
                  Appearance
                </Link>
              )}
              <button
                type="button"
                onClick={() => {
                  setUserOpen(false);
                  void signOut();
                }}
                className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-danger-600 transition-colors hover:bg-danger-50"
              >
                <LogOut className="size-4" />
                Sign out
              </button>
            </div>
          </>
        )}
      </div>
    </header>
  );
}
