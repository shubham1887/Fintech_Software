import { Link } from 'react-router-dom';
import {
  AlertTriangle, Eye, EyeOff, Plus, ReceiptText, RefreshCw, Wallet,
} from 'lucide-react';
import type { WalletBalance } from '@/api/wallet.api';
import type { BalanceStyle } from '@/theme/mobileLayouts';
import { heroSurfaceStyle } from '@/theme/mobileLayouts';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';
import { MASK, useBalanceMasked } from '@/lib/balanceMask';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * The balance block on the phone home screen, in the five shapes a preset can ask for.
 *
 * All five read the SAME inputs and obey the same two rules, because those are correctness
 * rules rather than design ones:
 *
 *  1. A failed read is never drawn as ₹0.00. A wrong balance is worse than no balance, so an
 *     error replaces the figure with a retry affordance in every variant.
 *  2. The mask (`useBalanceMasked`) is shared state, not local — the app-bar strip shows the
 *     same numbers, and the two must never disagree.
 *
 * What differs is the presentation and the amount of screen it is allowed to take: `card` is
 * the anchor (≈190px), `hero` is the consumer banner, `strip` is one rail (≈56px), `plain`
 * is type on bare canvas, `split` is a figures panel for someone reading numbers all day.
 */

export interface BalanceProps {
  style: BalanceStyle;
  wallets: WalletBalance[];
  loading: boolean;
  /** Non-empty when the balance could not be READ. Never render a number in that case. */
  error: string;
  refreshing: boolean;
  onRefresh: () => void;
}

export function BalanceBlock(props: BalanceProps) {
  switch (props.style) {
    case 'hero':
      return <BalanceHero {...props} />;
    case 'strip':
      return <BalanceStrip {...props} />;
    case 'plain':
      return <BalancePlain {...props} />;
    case 'split':
      return <BalanceSplit {...props} />;
    default:
      return <BalanceCard {...props} />;
  }
}

function total(wallets: WalletBalance[]): number {
  return wallets.reduce((s, w) => s + w.balance, 0);
}

/* ------------------------------------------------------------- shared bits */

/** Eye + refresh, drawn for a dark surface or a light one. */
function BalanceTools({
  masked, toggleMask, refreshing, onRefresh, tone,
}: {
  masked: boolean;
  toggleMask: () => void;
  refreshing: boolean;
  onRefresh: () => void;
  tone: 'dark' | 'light';
}) {
  const btn = cn(
    'grid place-items-center rounded-lg transition-colors',
    tone === 'dark' ? 'size-9 text-white/60 active:bg-white/10' : 'size-9 text-slate-400 active:bg-slate-100',
  );

  return (
    <div className="flex shrink-0">
      <button type="button" onClick={toggleMask} aria-label={masked ? 'Show balance' : 'Hide balance'} className={btn}>
        {masked ? <EyeOff className="size-[18px]" /> : <Eye className="size-[18px]" />}
      </button>
      <button type="button" onClick={onRefresh} aria-label="Refresh balance" className={btn}>
        <RefreshCw className={cn('size-[18px]', refreshing && 'animate-spin')} />
      </button>
    </div>
  );
}

/** "Add money" / "Statement" — the only two actions the balance block ever offers. */
function BalanceActions({ tone }: { tone: 'dark' | 'light' }) {
  return (
    <div className="grid grid-cols-2 gap-2.5">
      <Link
        to="/s/Fund/Request"
        onClick={() => haptic('medium')}
        className={cn(
          'flex items-center justify-center gap-1.5 rounded-xl py-3 text-[13px] font-semibold transition-transform active:scale-[0.98]',
          tone === 'dark' ? 'bg-white text-ink-900' : 'bg-brand-600 text-white',
        )}
      >
        <Plus className="size-4" />
        Add money
      </Link>
      <Link
        to="/transactions"
        onClick={() => haptic()}
        className={cn(
          'flex items-center justify-center gap-1.5 rounded-xl border py-3 text-[13px] font-semibold',
          tone === 'dark'
            ? 'border-white/20 text-white active:bg-white/10'
            : 'border-slate-200 text-slate-700 active:bg-slate-50',
        )}
      >
        <ReceiptText className="size-4" />
        Statement
      </Link>
    </div>
  );
}

/** The one error shape, tinted for whichever surface it lands on. */
function BalanceError({ message, onRetry, tone }: { message: string; onRetry: () => void; tone: 'dark' | 'light' }) {
  return (
    <button type="button" onClick={onRetry} className="flex w-full items-start gap-2 text-left">
      <AlertTriangle className={cn('mt-0.5 size-4 shrink-0', tone === 'dark' ? 'text-amber-300' : 'text-warning-600')} />
      <span className="min-w-0">
        <span className={cn('block font-display text-base font-bold', tone === 'dark' ? 'text-white' : 'text-slate-900')}>
          Balance unavailable
        </span>
        <span className={cn('mt-0.5 block text-xs leading-snug', tone === 'dark' ? 'text-white/60' : 'text-slate-500')}>
          {message} — tap to retry
        </span>
      </span>
    </button>
  );
}

/* ----------------------------------------------------------------- card */

/**
 * The classic anchor: the app's own dark surface (--ui-navbar, so it matches the sidebar and
 * the app bar) with a single low-opacity brand wash and a hairline top highlight.
 *
 * Deliberately NOT a multi-stop gradient with blurred colour blobs — that reads muddy at small
 * sizes and turns brown under a warm brand ramp. Flat, deep, holds up in every theme.
 */
function BalanceCard({ wallets, loading, error, refreshing, onRefresh }: BalanceProps) {
  const [masked, toggleMask] = useBalanceMasked();

  return (
    <section className="relative overflow-hidden rounded-card shadow-card" style={{ backgroundColor: 'var(--ui-navbar)' }}>
      <div className="pointer-events-none absolute inset-0 bg-gradient-to-br from-brand-500/20 via-transparent to-transparent" />
      <div className="pointer-events-none absolute inset-x-0 top-0 h-px bg-white/15" />

      <div className="relative p-5 text-white">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <p className="text-[10px] font-semibold uppercase tracking-[0.14em] text-white/50">Total balance</p>

            {error ? (
              <div className="mt-1.5">
                <BalanceError message={error} onRetry={onRefresh} tone="dark" />
              </div>
            ) : loading ? (
              <Skeleton className="mt-2 h-8 w-44 bg-white/15" />
            ) : (
              <p className="mt-1.5 font-display text-[34px] font-extrabold leading-none tabular-nums">
                {masked ? MASK : inr(total(wallets))}
              </p>
            )}
          </div>

          <div className="-mr-2 -mt-1.5">
            <BalanceTools masked={masked} toggleMask={toggleMask} refreshing={refreshing} onRefresh={onRefresh} tone="dark" />
          </div>
        </div>

        {/* Per-wallet breakdown — a fixed row, not floating pills, so the numbers line up. */}
        {!error && wallets.length > 0 && (
          <div className="mt-5 flex items-stretch rounded-xl bg-white/[0.07] ring-1 ring-white/10">
            {wallets.map((w, i) => (
              <div key={w.walletId} title={w.name} className={cn('min-w-0 flex-1 px-3 py-2.5', i > 0 && 'border-l border-white/10')}>
                <p className="truncate text-[10px] font-semibold uppercase tracking-wide text-white/45">{w.shortName}</p>
                <p className="mt-0.5 truncate font-display text-[14px] font-bold tabular-nums text-white/95">
                  {masked ? '••••' : inr(w.balance)}
                </p>
              </div>
            ))}
          </div>
        )}

        <div className="mt-4">
          <BalanceActions tone="dark" />
        </div>
      </div>
    </section>
  );
}

/* ----------------------------------------------------------------- hero */

/**
 * Full-bleed banner that continues the app bar downward and curves into the content.
 *
 * The figure is the biggest thing on the screen and the actions are pills rather than blocks —
 * this is the consumer-payments reading of the same data, for operators whose retailers came
 * from that kind of app.
 */
function BalanceHero({ wallets, loading, error, refreshing, onRefresh }: BalanceProps) {
  const [masked, toggleMask] = useBalanceMasked();

  return (
    <section
      className="-mx-4 -mt-4 overflow-hidden rounded-b-[28px] px-5 pb-6 pt-5 text-white shadow-card"
      style={heroSurfaceStyle()}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-[0.14em] text-white/60">
            <Wallet className="size-3.5" />
            Total balance
          </p>

          {error ? (
            <div className="mt-2">
              <BalanceError message={error} onRetry={onRefresh} tone="dark" />
            </div>
          ) : loading ? (
            <Skeleton className="mt-2.5 h-10 w-52 bg-white/15" />
          ) : (
            <p className="mt-2 font-display text-[40px] font-extrabold leading-none tabular-nums drop-shadow-sm">
              {masked ? MASK : inr(total(wallets))}
            </p>
          )}
        </div>

        <div className="-mr-2 -mt-1">
          <BalanceTools masked={masked} toggleMask={toggleMask} refreshing={refreshing} onRefresh={onRefresh} tone="dark" />
        </div>
      </div>

      {/* Wallets as scrollable chips — a hero is a banner, not a table. */}
      {!error && wallets.length > 0 && (
        <div className="-mx-1 mt-4 flex gap-2 overflow-x-auto px-1 pb-0.5 [scrollbar-width:none]">
          {wallets.map((w) => (
            <span
              key={w.walletId}
              title={w.name}
              className="shrink-0 rounded-full bg-white/15 px-3 py-1.5 ring-1 ring-white/15 backdrop-blur"
            >
              <span className="text-[10px] font-semibold uppercase tracking-wide text-white/60">{w.shortName}</span>
              <span className="ml-1.5 font-display text-[13px] font-bold tabular-nums">
                {masked ? '••••' : inr(w.balance)}
              </span>
            </span>
          ))}
        </div>
      )}

      <div className="mt-5 flex gap-2.5">
        <Link
          to="/s/Fund/Request"
          onClick={() => haptic('medium')}
          className="flex flex-1 items-center justify-center gap-1.5 rounded-full bg-white py-3 text-[13px] font-bold text-ink-900 transition-transform active:scale-[0.98]"
        >
          <Plus className="size-4" />
          Add money
        </Link>
        <Link
          to="/transactions"
          onClick={() => haptic()}
          className="flex flex-1 items-center justify-center gap-1.5 rounded-full bg-white/15 py-3 text-[13px] font-bold text-white ring-1 ring-white/20 active:bg-white/25"
        >
          <ReceiptText className="size-4" />
          Statement
        </Link>
      </div>
    </section>
  );
}

/* ---------------------------------------------------------------- strip */

/**
 * One rail. Total on the left, wallets scrolling on the right, actions folded into two icon
 * buttons. Costs ~56px against the card's ~190px — which is the entire point of the compact
 * preset: the screen below it starts that much higher.
 */
function BalanceStrip({ wallets, loading, error, refreshing, onRefresh }: BalanceProps) {
  const [masked, toggleMask] = useBalanceMasked();

  if (error) {
    return (
      <button
        type="button"
        onClick={onRefresh}
        className="flex w-full items-center gap-2 rounded-card border border-warning-500/25 bg-warning-50 px-3 py-2.5 text-left"
      >
        <AlertTriangle className="size-4 shrink-0 text-warning-600" />
        <span className="truncate text-xs font-medium text-slate-700">Balance unavailable — tap to retry</span>
      </button>
    );
  }

  return (
    <section className="flex items-center gap-3 rounded-card border border-slate-200/70 bg-white px-3 py-2.5 shadow-card">
      <Link to="/transactions" onClick={() => haptic()} className="min-w-0 shrink-0">
        <span className="block text-[9px] font-semibold uppercase tracking-[0.12em] text-slate-400">Balance</span>
        {loading ? (
          <Skeleton className="mt-1 h-4 w-20" />
        ) : (
          <span className="block font-display text-[17px] font-extrabold leading-tight tabular-nums text-slate-900">
            {masked ? MASK : inr(total(wallets))}
          </span>
        )}
      </Link>

      <span className="h-8 w-px shrink-0 bg-slate-200" />

      <div className="flex min-w-0 flex-1 gap-3 overflow-x-auto [scrollbar-width:none]">
        {wallets.map((w) => (
          <span key={w.walletId} title={w.name} className="shrink-0">
            <span className="block text-[9px] font-semibold uppercase tracking-wide text-slate-400">{w.shortName}</span>
            <span className="block font-display text-[13px] font-bold tabular-nums text-slate-700">
              {masked ? '••••' : inr(w.balance)}
            </span>
          </span>
        ))}
      </div>

      <div className="-mr-1.5 flex shrink-0 items-center">
        <Link
          to="/s/Fund/Request"
          onClick={() => haptic('medium')}
          aria-label="Add money"
          className="grid size-9 place-items-center rounded-lg bg-brand-600 text-white active:scale-95"
        >
          <Plus className="size-[18px]" />
        </Link>
        <BalanceTools masked={masked} toggleMask={toggleMask} refreshing={refreshing} onRefresh={onRefresh} tone="light" />
      </div>
    </section>
  );
}

/* ---------------------------------------------------------------- plain */

/**
 * No surface at all: the figure sits straight on the canvas, separated by hairlines. With the
 * minimal preset's neutral tiles, the whole screen ends up with exactly one colour — the brand
 * one, on the two text actions — which is what makes it scan fast.
 */
function BalancePlain({ wallets, loading, error, refreshing, onRefresh }: BalanceProps) {
  const [masked, toggleMask] = useBalanceMasked();

  return (
    <section className="border-b border-slate-200 pb-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-[11px] font-medium uppercase tracking-[0.16em] text-slate-400">Total balance</p>

          {error ? (
            <div className="mt-2">
              <BalanceError message={error} onRetry={onRefresh} tone="light" />
            </div>
          ) : loading ? (
            <Skeleton className="mt-2.5 h-9 w-48" />
          ) : (
            <p className="mt-2 font-display text-[38px] font-extrabold leading-none tabular-nums text-slate-900">
              {masked ? MASK : inr(total(wallets))}
            </p>
          )}
        </div>

        <div className="-mr-2 -mt-1">
          <BalanceTools masked={masked} toggleMask={toggleMask} refreshing={refreshing} onRefresh={onRefresh} tone="light" />
        </div>
      </div>

      {!error && wallets.length > 0 && (
        <div className="mt-4 flex flex-wrap gap-x-5 gap-y-2">
          {wallets.map((w) => (
            <span key={w.walletId} title={w.name} className="text-[12px] text-slate-500">
              {w.shortName}
              <span className="ml-1.5 font-display font-bold tabular-nums text-slate-800">
                {masked ? '••••' : inr(w.balance)}
              </span>
            </span>
          ))}
        </div>
      )}

      <div className="mt-4 flex gap-5">
        <Link
          to="/s/Fund/Request"
          onClick={() => haptic('medium')}
          className="text-[13px] font-semibold text-brand-600 underline-offset-4 active:underline"
        >
          Add money
        </Link>
        <Link
          to="/transactions"
          onClick={() => haptic()}
          className="text-[13px] font-semibold text-slate-500 underline-offset-4 active:underline"
        >
          Statement
        </Link>
      </div>
    </section>
  );
}

/* ---------------------------------------------------------------- split */

/**
 * Total on the left, every wallet listed on the right with its share of the float.
 *
 * For an operator with three or four wallets, the card variant's equal-width row says nothing
 * about proportion; this one puts the numbers in a column where they are comparable, which is
 * the question someone actually asks at the start of a day.
 */
function BalanceSplit({ wallets, loading, error, refreshing, onRefresh }: BalanceProps) {
  const [masked, toggleMask] = useBalanceMasked();
  const sum = total(wallets);

  return (
    <section className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
      <div className="flex items-stretch">
        <div className="flex min-w-0 flex-[1.1] flex-col justify-between p-4" style={{ backgroundColor: 'var(--ui-navbar)' }}>
          <div>
            <p className="text-[10px] font-semibold uppercase tracking-[0.14em] text-white/50">Total balance</p>
            {error ? (
              <div className="mt-2">
                <BalanceError message={error} onRetry={onRefresh} tone="dark" />
              </div>
            ) : loading ? (
              <Skeleton className="mt-2 h-7 w-28 bg-white/15" />
            ) : (
              <p className="mt-1.5 font-display text-[26px] font-extrabold leading-none tabular-nums text-white">
                {masked ? MASK : inr(sum)}
              </p>
            )}
          </div>

          <div className="-ml-2 mt-3">
            <BalanceTools masked={masked} toggleMask={toggleMask} refreshing={refreshing} onRefresh={onRefresh} tone="dark" />
          </div>
        </div>

        <div className="min-w-0 flex-1">
          {loading ? (
            <div className="space-y-2 p-3">
              {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-5" />)}
            </div>
          ) : wallets.length === 0 ? (
            <p className="p-4 text-xs text-slate-400">No wallets</p>
          ) : (
            wallets.map((w, i) => {
              const share = sum > 0 ? Math.round((w.balance / sum) * 100) : 0;
              return (
                <div key={w.walletId} title={w.name} className={cn('px-3.5 py-2.5', i > 0 && 'border-t border-slate-100')}>
                  <div className="flex items-baseline justify-between gap-2">
                    <span className="truncate text-[10px] font-semibold uppercase tracking-wide text-slate-400">
                      {w.shortName}
                    </span>
                    <span className="shrink-0 font-display text-[13px] font-bold tabular-nums text-slate-900">
                      {masked ? '••••' : inr(w.balance)}
                    </span>
                  </div>
                  {/* Share of the float — the comparison this variant exists for. Hidden while
                      masked, since the bar would leak the proportion the mask is hiding. */}
                  <div className="mt-1.5 h-1 overflow-hidden rounded-full bg-slate-100">
                    <div
                      className="h-full rounded-full bg-brand-500/70"
                      style={{ width: masked ? '0%' : `${Math.min(100, share)}%` }}
                    />
                  </div>
                </div>
              );
            })
          )}
        </div>
      </div>

      <div className="border-t border-slate-100 p-3">
        <BalanceActions tone="light" />
      </div>
    </section>
  );
}
