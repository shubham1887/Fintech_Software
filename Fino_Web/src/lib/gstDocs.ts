import { apiDate } from '@/lib/format';
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
