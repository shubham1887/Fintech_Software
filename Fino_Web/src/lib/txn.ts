import type { TransactionRow } from '@/api/transactionReport.api';

/**
 * Human labels for a transaction row.
 *
 * `TransactionReport/SelectReport` does NOT send `ServiceType` at all, and `OperatorName`
 * comes back empty for a lot of rows (DMT especially). So a naive
 * `ServiceType ?? OperatorName ?? '#' + ServiceID` renders "#10" — which means nothing to a
 * retailer. These helpers fall through what the row actually carries, in the order a person
 * would recognise it, and let the caller supply the catalog's ServiceID → name map so the
 * last resort is "Money Transfer" rather than "#10".
 */

function str(value: unknown): string {
  return typeof value === 'string' ? value.trim() : '';
}

/** ServiceID → service name, built from the admin catalog. */
export type ServiceNames = Map<number, string>;

export function txnTitle(row: TransactionRow, names?: ServiceNames): string {
  const fromCatalog = row.ServiceID != null ? names?.get(Number(row.ServiceID)) : undefined;

  return (
    str(row.OperatorName) ||
    str(row.DMT_BeneficiaryName) ||
    str(row.ServiceType) ||
    str(fromCatalog) ||
    str(row.WalletName) ||
    (row.ServiceID != null ? `Service #${row.ServiceID}` : '') ||
    'Transaction'
  );
}

/**
 * The second line — ONE secondary identifier, not a breadcrumb. At 375px a
 * "service · bank · consumer · date" line just truncates and takes the date with it, so
 * everything else moves into the expandable detail block.
 */
export function txnSubtitle(row: TransactionRow, names?: ServiceNames): string {
  const consumer = str(row.ConsumerNumber);
  if (consumer) return consumer;

  const service = txnService(row, names);
  return service === txnTitle(row, names) ? '' : service;
}

/** The catalog's name for this row's service, for the detail block. */
export function txnService(row: TransactionRow, names?: ServiceNames): string {
  const fromCatalog = row.ServiceID != null ? str(names?.get(Number(row.ServiceID))) : '';
  return fromCatalog || str(row.ServiceType) || (row.ServiceID != null ? `#${row.ServiceID}` : '');
}

/** Provider's own message — the useful bit when a transaction failed. */
export function txnNote(row: TransactionRow): string {
  return str(row.Description) || str(row.Remark);
}

export function txnRef(row: TransactionRow): string {
  return str(row.ProviderRefNo) || str(row.OperatorRefNo) || str(row.ReferenceNo) || str(row.ProviderRequestID);
}

/**
 * Compact timestamp for a list row. The full "03 Aug 2026, 11:24 PM" is too wide next to a
 * consumer number on a 375px screen and truncates away the time — which is the part you
 * scan for. Today collapses to just the time, this year drops the year.
 */
export function txnWhen(value?: string): string {
  if (!value) return '';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '';

  const now = new Date();
  const time = d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  if (d.toDateString() === now.toDateString()) return time;

  const day = d.toLocaleDateString([], { day: '2-digit', month: 'short' });
  return d.getFullYear() === now.getFullYear() ? `${day}, ${time}` : `${day} ${d.getFullYear()}`;
}
