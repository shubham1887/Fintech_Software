import { useQuery } from '@tanstack/react-query';
import { apiDate } from '@/lib/format';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { UserTypeCode } from '@/lib/roles';

/**
 * Shared pieces of the GST document screens (invoice list, exchange register, IRN queue,
 * credit-note alerts). The screens each own their own layout, but they agree on three things:
 * the default period, the document vocabulary, and — the one that matters for safety — which
 * role may press which button.
 */

/** India's financial year runs 1 April to 31 March. */
export function currentFyRange(today = new Date()): { from: string; to: string; label: string } {
  const y = today.getFullYear();
  const startYear = today.getMonth() + 1 >= 4 ? y : y - 1;
  const two = (n: number) => String(n % 100).padStart(2, '0');
  return {
    from: `${startYear}-04-01`,
    to: apiDate(today),
    label: `${startYear}-${two(startYear + 1)}`,
  };
}

/**
 * Who may press a write button on these screens — and the reason it is spelt out here rather
 * than read off a single "is admin" flag. `ApiAccessPolicy` on the server is deliberately in
 * two tiers for GST:
 *
 *  - OPERATIONAL acts (queue a document, run the IRP queue, detect a swap leg) allocate no
 *    number and decide nothing, so they are Admin **and** Support — someone has to be able to
 *    clear the queue at 11pm on the 30th day.
 *  - DOCUMENT acts (generate invoices, issue a self-invoice, issue a credit note, raise our side
 *    of a barter, supersede a rule) put a number into GSTR-1 that cannot be withdrawn, so they
 *    are Admin **alone** — narrower than everywhere else in the product.
 *
 * The accountant (role 9) reads every one of these screens and presses none of these buttons.
 * The server refuses the call regardless; showing a button nobody is allowed to press is the
 * bug this guards against. A hidden button is honest; a 403 toast is not.
 */
export const canOperate = (roleId: number | undefined) =>
  roleId === UserTypeCode.Admin || roleId === UserTypeCode.SupportUser;

export const canRaiseDocument = (roleId: number | undefined) => roleId === UserTypeCode.Admin;

/**
 * The company's own GSTIN, read from the Invoice Settings profile.
 *
 * Every inward screen needs it — GSTR-2B is filed per registration, so an import, a snapshot read
 * and a reconciliation all key on it — and none of them should ask the user to type it. Asking
 * would also be the one way to reconcile against the wrong registration.
 *
 * `missing` is the state worth handling on screen rather than silently sending an empty string:
 * until the profile is filled in, nothing on the inward side can run at all.
 */
export function useCompanyGstin(): { gstin: string; missing: boolean; loading: boolean } {
  const { data, isPending } = useQuery({
    queryKey: ['crud', 'InvoiceSetting/GetAll'],
    queryFn: () => fetchList('InvoiceSetting/GetAll', { PageNumber: 1, PageSize: 50 }),
  });
  const rows = data ?? [];
  // The seed script reads TOP 1 ORDER BY InvoiceSettingID, so the LOWEST id is the company profile.
  const profile = rows.length
    ? [...rows].sort(
        (a, b) => Number(getField(a, 'InvoiceSettingID') ?? 0) - Number(getField(b, 'InvoiceSettingID') ?? 0),
      )[0]
    : null;
  const gstin = String(getField(profile ?? {}, 'GSTIN') ?? '').trim();
  return { gstin, missing: !isPending && gstin.length !== 15, loading: isPending };
}

/** Period helpers. The GST period key is yyyyMM, which is what every inward endpoint takes. */
export function periodKeyOf(d = new Date()): string {
  return `${d.getFullYear()}${String(d.getMonth() + 1).padStart(2, '0')}`;
}

/** "202610" → "Oct 2026", for a heading that reads like a month rather than a number. */
export function periodLabel(periodKey: string): string {
  if (!/^\d{6}$/.test(periodKey)) return periodKey;
  const y = Number(periodKey.slice(0, 4));
  const m = Number(periodKey.slice(4, 6));
  if (m < 1 || m > 12) return periodKey;
  return new Date(y, m - 1, 1).toLocaleDateString('en-IN', { month: 'short', year: 'numeric' });
}

/** The last `count` period keys, newest first — the period picker on the inward screens. */
export function recentPeriods(count = 18, from = new Date()): string[] {
  const out: string[] = [];
  const d = new Date(from.getFullYear(), from.getMonth(), 1);
  for (let i = 0; i < count; i++) {
    out.push(periodKeyOf(d));
    d.setMonth(d.getMonth() - 1);
  }
  return out;
}

/**
 * The four reconciliation buckets, in the order an accountant works them: what agrees, then the
 * two one-sided gaps, then the ones that agree on existing but not on money.
 */
export const RECON_BUCKETS = [
  { key: 'Matched', label: 'Matched', tone: 'success' as const, help: 'In both, and the tax agrees within tolerance.' },
  { key: 'MissingIn2B', label: 'In books, not in 2B', tone: 'warning' as const, help: 'We captured it; the supplier has not filed it. The credit is not available yet.' },
  { key: 'MissingInBooks', label: 'In 2B, not in books', tone: 'info' as const, help: 'The supplier filed it; we never captured the bill. Capture it or the credit goes unclaimed.' },
  { key: 'Mismatch', label: 'Tax mismatch', tone: 'danger' as const, help: 'Both sides have it, but the tax differs by more than the tolerance.' },
] as const;

export function bucketMeta(bucket: string) {
  return RECON_BUCKETS.find((b) => b.key.toLowerCase() === (bucket ?? '').toLowerCase());
}

/**
 * The bank reconciliation's four buckets. Deliberately the same shape as the 2B ones above — an
 * accountant who has learnt one has learnt the other — but the help text differs, because the
 * action each calls for is a different job.
 */
export const BANK_BUCKETS = [
  { key: 'Matched', label: 'Matched', tone: 'success' as const, help: 'On the statement and in the ledger, agreeing within tolerance. Nothing to do.' },
  { key: 'MissingInBooks', label: 'In bank, not in books', tone: 'danger' as const, help: 'Real money moved that the books do not know about — bank charges, interest, a direct credit. This is the bucket that finds missing entries.' },
  { key: 'MissingInBank', label: 'In books, not in bank', tone: 'warning' as const, help: 'A payment we recorded that the bank has not shown. In transit if it is recent; a mistake if it is old.' },
  { key: 'Mismatch', label: 'Amount mismatch', tone: 'info' as const, help: 'Paired on a reference, but the amounts disagree. One of the two was keyed wrong.' },
] as const;

export function bankBucketMeta(bucket: string) {
  return BANK_BUCKETS.find((b) => b.key.toLowerCase() === (bucket ?? '').toLowerCase());
}

/** How a bank line was paired. A reference match is strong; amount-and-date is circumstantial. */
export function bankMethodTone(method: string): 'success' | 'warning' | 'info' | 'neutral' {
  switch ((method ?? '').toLowerCase()) {
    case 'reference':
    case 'fundrequest':
      return 'success';
    case 'amountanddate':
    case 'fundrequestamount':
    case 'providerpurchase':
      return 'warning';
    case 'manual':
      return 'info';
    default:
      return 'neutral';
  }
}

export function bankMethodLabel(method: string): string {
  switch ((method ?? '').toLowerCase()) {
    case 'reference':
      return 'reference';
    case 'amountanddate':
      return 'amount + date';
    case 'manual':
      return 'by hand';
    case 'fundrequest':
      return 'fund request (UTR)';
    case 'fundrequestamount':
      return 'fund request (amount)';
    case 'providerpurchase':
      return 'provider purchase';
    default:
      return '—';
  }
}

/** Tone for an ITC register status. Reversed is the one that costs money. */
export function itcStatusTone(status: string): 'success' | 'warning' | 'danger' | 'neutral' | 'info' {
  switch ((status ?? '').toUpperCase()) {
    case 'CLAIMED':
      return 'success';
    case 'ELIGIBLE':
      return 'info';
    case 'PENDING':
      return 'warning';
    case 'REVERSED':
    case 'BLOCKED':
      return 'danger';
    default:
      return 'neutral';
  }
}

/** Tone for an outbox row. Failed must look different from Skipped — see the engine's own note. */
export function outboxTone(status: string): 'success' | 'warning' | 'danger' | 'neutral' {
  switch ((status ?? '').toUpperCase()) {
    case 'POSTED':
      return 'success';
    case 'PENDING':
      return 'warning';
    case 'FAILED':
      return 'danger';
    case 'SKIPPED':
      return 'neutral';
    default:
      return 'neutral';
  }
}

/** The document types, as the API names them (DocumentType column / filter). */
export const DOCUMENT_TYPES = [
  { value: '', label: 'All documents' },
  { value: 'TaxInvoice', label: 'Tax invoice' },
  { value: 'BillOfSupply', label: 'Bill of supply' },
  { value: 'CreditNote', label: 'Credit note' },
  { value: 'DebitNote', label: 'Debit note' },
  { value: 'SelfInvoice', label: 'Self-invoice (RCM)' },
] as const;

export const DIRECTIONS = [
  { value: '', label: 'Sale & purchase' },
  { value: 'SALE', label: 'Sale (outward)' },
  { value: 'PURCHASE', label: 'Purchase (inward)' },
] as const;

/** A short human label for a DocumentType value. */
export function documentTypeLabel(type: string): string {
  return DOCUMENT_TYPES.find((d) => d.value === type)?.label ?? type;
}

/** Tone for a document status badge — only an Issued document is turnover. */
export function statusTone(status: string): 'success' | 'warning' | 'danger' | 'neutral' {
  const s = status.toUpperCase();
  if (s === 'ISSUED') return 'success';
  if (s === 'DRAFT') return 'warning';
  if (s === 'CANCELLED') return 'danger';
  return 'neutral';
}

/** Tone for an IRN (e-invoice) status. */
export function irnTone(status: string): 'success' | 'warning' | 'danger' | 'neutral' | 'info' {
  switch (status.toUpperCase()) {
    case 'REGISTERED':
      return 'success';
    case 'PENDING':
      return 'warning';
    case 'FAILED':
      return 'danger';
    case 'NOTAPPLICABLE':
      return 'neutral';
    default:
      return 'info';
  }
}

/** Days-left tone shared by both deadline screens: red once it has run out, amber when close. */
export function deadlineTone(daysLeft: number, passed: boolean): 'danger' | 'warning' | 'neutral' {
  if (passed || daysLeft < 0) return 'danger';
  if (daysLeft <= 7) return 'warning';
  return 'neutral';
}
