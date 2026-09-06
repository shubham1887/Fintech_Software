import { Fragment, useState } from 'react';
import { Link } from 'react-router-dom';
import { Check, ChevronDown, LogOut, Menu, Palette, RefreshCw, Settings, UserRound } from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { useSubscriptionStatus } from '@/features/subscription/useSubscriptionStatus';
import { useBrand } from '@/theme/BrandProvider';
import { useUISettings } from '@/theme/UISettingsProvider';
import { roleLabel } from '@/lib/roles';
import { GlobalSearch } from './GlobalSearch';
import { NotificationsBell } from './NotificationsBell';
import { TopNav } from './TopNav';
import { BrandGlyph } from '@/components/ui/BrandMark';

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

export function Topbar({ onMenuClick }: { onMenuClick: () => void }) {
  const { user, signOut } = useAuth();
  const { appName } = useBrand();
  const { themes, themeKey, setTheme } = useBrand();
  const { orientation } = useUISettings();
  const [userOpen, setUserOpen] = useState(false);
  const [themeOpen, setThemeOpen] = useState(false);

  const displayName = user?.PersonName?.trim() || user?.FirmName?.trim() || 'User';
  const initials = displayName.slice(0, 1).toUpperCase();
  const role = roleLabel(user?.UserTypeID ?? 0, user?.UserType ?? '');
  const horizontal = orientation === 'horizontal';

  // When the appbar is coloured (horizontal, or a dark AppBarColor), text/icons use --ui-fore.
  const foreCls = horizontal ? 'text-[var(--ui-fore)]' : 'text-slate-500';

  return (
    <header
      className={horizontal
        ? 'sticky top-0 z-30 flex h-16 items-center gap-2 px-4 shadow-sm sm:px-6'
        : 'sticky top-0 z-30 flex h-16 items-center gap-2 border-b border-slate-200 bg-white/85 px-4 backdrop-blur sm:px-6'}
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
        <div className="flex items-center gap-2.5 pr-2">
          <BrandGlyph className="size-8 shrink-0 rounded-lg" />
          <span className="hidden font-display text-base font-bold text-[var(--ui-fore)] sm:block">{appName}</span>
        </div>
      )}

      {horizontal ? <TopNav /> : <><GlobalSearch /><div className="flex-1" /></>}

      <SubscriptionCounter />

      <NotificationsBell />

      {/* Live theme switcher (white-label) */}
      <div className="relative">
        <button
          type="button"
          onClick={() => {
            setThemeOpen((v) => !v);
            setUserOpen(false);
          }}
          className="grid size-9 place-items-center rounded-lg text-slate-500 transition-colors hover:bg-slate-100"
          aria-label="Change theme"
        >
          <Palette className="size-5" />
        </button>
        {themeOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setThemeOpen(false)} />
            <div className="absolute right-0 z-20 mt-2 w-52 rounded-xl border border-slate-200 bg-white p-2 shadow-pop">
              <p className="px-2 py-1 text-xs font-medium uppercase tracking-wide text-slate-400">Theme</p>
              {themes.map((t, i) => (
                <Fragment key={t.key}>
                  {/* Seasonal skins repaint the nav/app bar too — label them so that is not a surprise. */}
                  {t.seasonal && !themes[i - 1]?.seasonal && (
                    <p className="mt-1 border-t border-slate-100 px-2 pb-1 pt-2 text-xs font-medium uppercase tracking-wide text-slate-400">
                      Festival
                    </p>
                  )}
                <button
                  type="button"
                  onClick={() => {
                    setTheme(t.key);
                    setThemeOpen(false);
                  }}
                  className="flex w-full items-center gap-2.5 rounded-lg px-2 py-1.5 text-sm transition-colors hover:bg-slate-100"
                >
                  <span className="size-5 shrink-0 rounded-md" style={{ backgroundColor: t.swatch }} />
                  <span className="flex-1 text-left text-slate-700">{t.label}</span>
                  {themeKey === t.key && <Check className="size-4 text-brand-600" />}
                </button>
                </Fragment>
              ))}
              <Link
                to="/appearance"
                onClick={() => setThemeOpen(false)}
                className="mt-1 flex items-center gap-2.5 rounded-lg border-t border-slate-100 px-2 py-2 text-sm font-medium text-brand-600 transition-colors hover:bg-brand-50"
              >
                <Palette className="size-4" />
                Open Theme Studio
              </Link>
            </div>
          </>
        )}
      </div>

      {/* User menu */}
      <div className="relative">
        <button
          type="button"
          onClick={() => {
            setUserOpen((v) => !v);
            setThemeOpen(false);
          }}
          className="flex items-center gap-2.5 rounded-lg p-1 pr-2 transition-colors hover:bg-slate-100"
        >
          <span className="grid size-9 place-items-center rounded-full bg-brand-600 text-sm font-semibold text-white">
            {initials}
          </span>
          <span className="hidden text-left leading-tight sm:block">
            <span className="block max-w-[12rem] truncate text-sm font-medium text-slate-800">
              {displayName}
            </span>
            <span className="block text-xs text-slate-400">{role}</span>
          </span>
          <ChevronDown className="size-4 text-slate-400" />
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
                to="/appearance"
                onClick={() => setUserOpen(false)}
                className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-100"
              >
                <Settings className="size-4" />
                Appearance
              </Link>
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
