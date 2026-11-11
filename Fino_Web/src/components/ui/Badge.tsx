import type { ReactNode } from 'react';
import { cn } from '@/lib/cn';
import { txnState } from '@/lib/txnStatus';

type Tone = 'neutral' | 'success' | 'danger' | 'warning' | 'info' | 'brand';

const TONES: Record<Tone, string> = {
  neutral: 'bg-slate-100 text-slate-600 ring-slate-200',
  success: 'bg-success-50 text-success-600 ring-success-500/20',
  danger: 'bg-danger-50 text-danger-600 ring-danger-500/20',
  warning: 'bg-warning-50 text-warning-600 ring-warning-500/20',
  info: 'bg-info-50 text-info-600 ring-info-500/20',
  brand: 'bg-brand-50 text-brand-700 ring-brand-500/20',
};

export function Badge({
  tone = 'neutral',
  children,
  className,
}: {
  tone?: Tone;
  children: ReactNode;
  className?: string;
}) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset',
        TONES[tone],
        className,
      )}
    >
      {children}
    </span>
  );
}

const DOT_TONES: Record<Tone, string> = {
  neutral: 'bg-slate-400',
  success: 'bg-success-500',
  danger: 'bg-danger-500',
  warning: 'bg-warning-500',
  info: 'bg-info-500',
  brand: 'bg-brand-500',
};

/**
 * A transaction / settlement status.
 *
 * The state itself is resolved by `txnState` — one vocabulary for the whole panel, so REFUNDED is
 * never painted as a failure and no screen prints a raw enum at the user. The leading dot carries
 * the state as well as the colour does: an operations console is scanned, and at a glance a row of
 * pills reads as shape and position before it reads as text — which also means the status survives
 * for anyone who cannot separate the greens from the reds.
 */
export function StatusBadge({ status }: { status?: string }) {
  const state = txnState(status);
  return (
    <Badge tone={state.tone} className="pl-2">
      <span className={cn('size-1.5 shrink-0 rounded-full', DOT_TONES[state.tone])} aria-hidden />
      {state.label}
    </Badge>
  );
}
