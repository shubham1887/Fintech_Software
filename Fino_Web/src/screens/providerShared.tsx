import { forwardRef, useMemo, type TextareaHTMLAttributes } from 'react';
import { useQuery } from '@tanstack/react-query';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';

/**
 * Shared building blocks for the provider gateway suite (hub / endpoints / rules / operator
 * mapping). Everything config-driven: the placeholder chips come from the backend engine, so the
 * UI can never drift out of sync with what the gateway actually supports.
 */

export const OPT_PAYLOAD = { IsActive: true, PageNumber: 1, PageSize: 500 };

/** Canonical endpoint / rule kinds understood by the gateway engine. */
export const KIND_OPTIONS = [
  { value: 'TRANSACTION', label: 'Transaction (dispatch)' },
  { value: 'BALANCE', label: 'Balance enquiry' },
  { value: 'STATUS', label: 'Status check' },
  { value: 'COMPLAINT', label: 'Complaint / dispute' },
] as const;

export const KIND_BADGE: Record<string, string> = {
  TRANSACTION: 'bg-brand-50 text-brand-700',
  BALANCE: 'bg-emerald-50 text-emerald-700',
  STATUS: 'bg-amber-50 text-amber-700',
  COMPLAINT: 'bg-rose-50 text-rose-700',
};

/** Normalises stored kind values (incl. legacy spellings) to the canonical set. */
export function normalizeKind(kind: unknown): string {
  const k = String(kind ?? '').trim().toUpperCase();
  if (['PAY', 'RECHARGE', 'TXN', 'TRANSACTION', 'INITIAL'].includes(k)) return 'TRANSACTION';
  if (['BAL', 'BALANCE', 'BALANCECHECK'].includes(k)) return 'BALANCE';
  if (['STATUS', 'STATUSCHECK', 'ENQUIRY', 'CHECKSTATUS'].includes(k)) return 'STATUS';
  if (['COMPLAINT', 'COMPLAIN', 'DISPUTE', 'GRIEVANCE'].includes(k)) return 'COMPLAINT';
  return k;
}

export const RULE_STATUS_OPTIONS = [
  { value: 'SUCCESS', label: 'Success' },
  { value: 'FAILED', label: 'Failed' },
  { value: 'PENDING', label: 'Pending' },
] as const;

export const STATUS_BADGE: Record<string, string> = {
  SUCCESS: 'bg-emerald-50 text-emerald-700',
  FAILED: 'bg-danger-50 text-danger-600',
  PENDING: 'bg-amber-50 text-amber-700',
};

export function normalizeRuleStatus(status: unknown): string {
  const s = String(status ?? '').trim().toUpperCase();
  if (s.startsWith('SUCCESS') || s === 'OK') return 'SUCCESS';
  if (s.startsWith('FAIL')) return 'FAILED';
  return 'PENDING';
}

export type Row = Record<string, unknown>;

/** Active providers, for the provider pickers across the suite. */
export function useProviders() {
  return useQuery({
    queryKey: ['fk', 'ProviderMaster/GetAll'],
    queryFn: () => fetchList('ProviderMaster/GetAll', OPT_PAYLOAD),
    staleTime: 60_000,
  });
}

export function useServices() {
  return useQuery({
    queryKey: ['fk', 'Service/GetAll'],
    queryFn: () => fetchList('Service/GetAll', OPT_PAYLOAD),
    staleTime: 60_000,
  });
}

export interface Placeholder {
  Token: string;
  Purpose: string;
  Direction: 'request' | 'capture';
}

/** Fallback chips if the backend catalogue is unreachable — mirrors the engine's defaults. */
const FALLBACK_PLACEHOLDERS: Placeholder[] = [
  { Token: '[Number]', Purpose: 'Consumer / mobile number', Direction: 'request' },
  { Token: '[Amount]', Purpose: 'Transaction amount', Direction: 'request' },
  { Token: '[OperatorCode]', Purpose: 'Provider-side operator code', Direction: 'request' },
  { Token: '[RequestID]', Purpose: 'Our transaction reference', Direction: 'request' },
  { Token: '[LoginID]', Purpose: 'Provider login / member id', Direction: 'request' },
  { Token: '[Token]', Purpose: 'Provider API token', Direction: 'request' },
  { Token: '[Date]', Purpose: 'Current date (endpoint format)', Direction: 'request' },
  { Token: '[RefID]', Purpose: "Provider's reference number", Direction: 'capture' },
  { Token: '[TxnID]', Purpose: "Provider's transaction id", Direction: 'capture' },
  { Token: '[Message]', Purpose: 'Provider message', Direction: 'capture' },
  { Token: '[Balance]', Purpose: 'Remaining balance', Direction: 'capture' },
];

export function usePlaceholders(direction: 'request' | 'capture') {
  const query = useQuery({
    queryKey: ['provider-placeholders'],
    queryFn: () => fetchList('ProviderGateway/Placeholders', {}),
    staleTime: 5 * 60_000,
  });
  const items = useMemo(() => {
    const source = (query.data?.length ? query.data : FALLBACK_PLACEHOLDERS) as unknown as Row[];
    return source
      .map((p) => ({
        Token: String(getField(p, 'Token') ?? ''),
        Purpose: String(getField(p, 'Purpose') ?? ''),
        Direction: String(getField(p, 'Direction') ?? '') as Placeholder['Direction'],
      }))
      .filter((p) => p.Token && p.Direction === direction);
  }, [query.data, direction]);
  return { items, loading: query.isLoading };
}

/**
 * Clickable token chips. `onInsert` receives the token so the caller can append it into the
 * focused template field.
 */
export function PlaceholderChips({
  direction,
  onInsert,
}: {
  direction: 'request' | 'capture';
  onInsert: (token: string) => void;
}) {
  const { items } = usePlaceholders(direction);
  if (items.length === 0) return null;
  return (
    <div className="flex flex-wrap gap-1.5">
      {items.map((p) => (
        <button
          key={p.Token}
          type="button"
          title={p.Purpose}
          onClick={() => onInsert(p.Token)}
          className="rounded-full border border-brand-200 bg-brand-50/60 px-2.5 py-1 font-mono text-[11px] font-semibold text-brand-700 transition-colors hover:bg-brand-100"
        >
          {p.Token}
        </button>
      ))}
    </div>
  );
}

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
}

/** Labeled textarea styled to match Input/Select (monospace — templates are code-like). */
export const TemplateArea = forwardRef<HTMLTextAreaElement, TextareaProps>(function TemplateArea(
  { label, className, ...rest },
  ref,
) {
  return (
    <label className="block">
      {label && <span className="mb-1.5 block text-sm font-medium text-slate-700">{label}</span>}
      <textarea
        ref={ref}
        rows={3}
        className={cn(
          'w-full rounded-lg border border-slate-200 px-3 py-2 font-mono text-xs leading-relaxed text-slate-800',
          'focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20',
          className,
        )}
        {...rest}
      />
    </label>
  );
});

/** Tiny status pill. */
export function Pill({ text, tone }: { text: string; tone: string }) {
  return (
    <span className={cn('inline-flex rounded-full px-2 py-0.5 text-[11px] font-semibold', tone)}>
      {text}
    </span>
  );
}

/** Standard on/off switch used across the suite tables. */
export function ToggleDot({
  on,
  onClick,
  busy,
}: {
  on: boolean;
  onClick: () => void;
  busy?: boolean;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={busy}
      className={cn(
        'relative h-5 w-9 rounded-full transition-colors disabled:opacity-50',
        on ? 'bg-brand-600' : 'bg-slate-300',
      )}
      title={on ? 'Active — click to deactivate' : 'Inactive — click to activate'}
    >
      <span
        className={cn(
          'absolute top-0.5 size-4 rounded-full bg-white shadow transition-all',
          on ? 'left-[18px]' : 'left-0.5',
        )}
      />
    </button>
  );
}
