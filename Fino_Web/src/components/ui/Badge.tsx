import type { ReactNode } from 'react';
import { cn } from '@/lib/cn';

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

/** Maps a transaction/settlement status to a coloured badge. */
export function StatusBadge({ status }: { status?: string }) {
  const s = (status ?? '').toUpperCase();
  const tone: Tone =
    s === 'SUCCESS' || s === 'ACCEPT' || s === 'ACCEPTED'
      ? 'success'
      : s === 'PENDING' || s === 'PROCESSING' || s === 'PROCESS' || s === 'INITIATED'
        ? 'warning'
        : s === 'FAILED' || s === 'FAILURE' || s === 'REFUND' || s === 'REFUNDED' || s === 'REVERSED' || s === 'REJECT'
          ? 'danger'
          : 'neutral';
  return <Badge tone={tone}>{status || '—'}</Badge>;
}
