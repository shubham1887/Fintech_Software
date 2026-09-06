import type { ReactNode } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { ChevronRight, House } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import { WalletBalanceBar } from '@/components/layout/WalletBalanceBar';
import { useIsMobileApp } from '@/lib/native';

/** "/s/Users/UserTransfer" → ["Users", "User Transfer"]; "/recharge" → ["Recharge"]. */
function crumbsFromPath(pathname: string): string[] {
  return pathname
    .replace(/^\/s/, '')
    .split('/')
    .filter(Boolean)
    .map((seg) =>
      decodeURIComponent(seg)
        .replace(/[-_]/g, ' ')
        .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
        .replace(/\b\w/g, (c) => c.toUpperCase()),
    );
}

export function PageHeader({
  title,
  subtitle,
  actions,
  icon: Icon,
}: {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
  icon?: LucideIcon;
}) {
  const { pathname } = useLocation();
  const crumbs = crumbsFromPath(pathname);
  const isMobile = useIsMobileApp();

  // On a phone this block used to eat a quarter of the screen before any content: a
  // breadcrumb, a wallet strip the app bar already shows, a gradient icon tile and a
  // two-line title. A screen title is worth one line there, nothing more.
  if (isMobile) {
    return (
      <div className="mb-4">
        <h1 className="font-display text-lg font-bold leading-tight text-slate-900">{title}</h1>
        {subtitle && <p className="mt-0.5 text-xs leading-snug text-slate-500">{subtitle}</p>}
        {/* Actions get their own row: sharing one with the title squeezed both. */}
        {actions && (
          <div className="mt-3 flex flex-wrap items-center gap-2 [&>button]:flex-1">{actions}</div>
        )}
      </div>
    );
  }

  return (
    <div className="mb-6">
      {/* Breadcrumb trail + live wallet balances (right-aligned) */}
      <div className="mb-3 flex items-center justify-between gap-3">
        <nav className="flex items-center gap-1 text-xs text-slate-400">
          <Link to="/dashboard" className="flex items-center gap-1 transition-colors hover:text-brand-600">
            <House className="size-3.5" />
            Home
          </Link>
          {crumbs.map((c, i) => (
            <span key={i} className="flex items-center gap-1">
              <ChevronRight className="size-3.5 text-slate-300" />
              <span className={i === crumbs.length - 1 ? 'font-medium text-slate-600' : ''}>{c}</span>
            </span>
          ))}
        </nav>
        <WalletBalanceBar />
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          {Icon && (
            <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-brand-500 to-brand-600 text-white shadow-sm ring-1 ring-brand-600/20">
              <Icon className="size-5" />
            </span>
          )}
          <div>
            <h1 className="font-display text-xl font-bold text-slate-900 sm:text-2xl">{title}</h1>
            {subtitle && <p className="mt-0.5 text-sm text-slate-500">{subtitle}</p>}
          </div>
        </div>
        {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
      </div>
    </div>
  );
}
