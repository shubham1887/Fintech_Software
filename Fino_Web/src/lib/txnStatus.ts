/**
 * The one status vocabulary the whole panel speaks.
 *
 * Transaction state reaches the UI in whatever spelling the era of code that wrote it used —
 * FAILED and FAILURE, PENDING and PROCESS, REFUND and REFUNDED — plus the complaint and hold
 * states added later. Every screen was left to interpret those strings itself, which produced two
 * problems that matter in a payments product:
 *
 *   • A REFUND was coloured as a failure. A refund is a *settled* outcome — the customer has their
 *     money back and the case is closed. Painting it the same red as "this broke" makes an
 *     operations queue unreadable, because the rows that still need work stop standing out.
 *   • The raw machine token was printed at the user ("FAILURE", "PROCESS"). A retailer reads their
 *     own statement; SHOUTING enum values is not a status, it is a leak of the database.
 *
 * One mapping here, used by the badge and by anything that needs to reason about state.
 */

export type TxnStateKind =
  | 'success'   // money reached its destination
  | 'pending'   // still in flight, no final answer
  | 'failed'    // it did not go through
  | 'refunded'  // it did not go through, and the money is back — closed
  | 'held'      // deliberately parked by an operator
  | 'cancelled' // withdrawn before it was decided
  | 'unknown';

export interface TxnState {
  kind: TxnStateKind;
  /** What a human should read. */
  label: string;
  /** Badge tone. */
  tone: 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'brand';
  /** True while the row still needs somebody to do something. */
  actionable: boolean;
}

const STATES: Record<TxnStateKind, Omit<TxnState, 'kind'>> = {
  success:   { label: 'Success',   tone: 'success', actionable: false },
  pending:   { label: 'Pending',   tone: 'warning', actionable: true },
  failed:    { label: 'Failed',    tone: 'danger',  actionable: true },
  // Settled, not broken — deliberately not red.
  refunded:  { label: 'Refunded',  tone: 'info',    actionable: false },
  held:      { label: 'On hold',   tone: 'warning', actionable: true },
  cancelled: { label: 'Cancelled', tone: 'neutral', actionable: false },
  unknown:   { label: '—',         tone: 'neutral', actionable: false },
};

export function txnState(raw?: string | null): TxnState {
  const s = String(raw ?? '').trim().toUpperCase();
  if (!s) return { kind: 'unknown', ...STATES.unknown };

  let kind: TxnStateKind = 'unknown';
  if (['SUCCESS', 'SUCCESSFUL', 'COMPLETE', 'COMPLETED', 'ACCEPT', 'ACCEPTED', 'RELEASE', 'RELEASED'].includes(s)) kind = 'success';
  else if (['PENDING', 'PROCESS', 'PROCESSING', 'INITIATED', 'IN PROCESS'].includes(s)) kind = 'pending';
  else if (['FAILED', 'FAILURE', 'FAIL', 'REJECT', 'REJECTED', 'DECLINED'].includes(s)) kind = 'failed';
  else if (['REFUND', 'REFUNDED', 'REVERSED', 'REVERSAL'].includes(s)) kind = 'refunded';
  else if (['HOLD', 'HELD', 'ON HOLD'].includes(s)) kind = 'held';
  else if (['CANCEL', 'CANCELLED', 'CANCELED', 'WITHDRAWN'].includes(s)) kind = 'cancelled';

  // An unrecognised value is shown as it arrived rather than hidden — a status this file has not
  // been taught is a gap worth seeing, not one worth papering over with "Unknown".
  if (kind === 'unknown') return { kind, ...STATES.unknown, label: titleCase(s) };
  return { kind, ...STATES[kind] };
}

/** "IN PROCESS" → "In process" */
function titleCase(s: string): string {
  return s.charAt(0) + s.slice(1).toLowerCase();
}
