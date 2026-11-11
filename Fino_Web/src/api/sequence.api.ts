import { apiPost } from '@/lib/apiClient';
import { fetchList, type ListRow } from './list.api';

/**
 * Provider-sequence (operator switching) endpoints — the AES contract mirrors the
 * legacy UI: Select returns one row per operator with the full provider grid.
 */

export interface SequenceProviderItem {
  OperatorID: number;
  ProviderID: number;
  ProviderName: string;
  Count: number;
  IsActive: boolean;
}

export interface SequenceGridRow {
  ServiceID: number;
  ServiceName: string;
  UserID: number;
  UserName: string;
  OperatorID: number;
  OperatorName: string;
  Detail: string;
  ProviderList: SequenceProviderItem[];
  RealProviderID: number;
  ViewProviderID: number;
  OperatorSequenceUserWiseID: number;
}

export interface BulkUpdateRow {
  Criteria: string;
  ServiceID: number;
  OperatorID: number;
  Count: number;
  ProviderList: Array<{ ProviderID: number; ProviderName: string }>;
  ViewProviderID: number;
  RealProviderID: number;
}

// ── Default (operator-level) sequence ──────────────────────────────────────────

export function fetchProviderSequence(serviceId: number): Promise<ListRow[]> {
  return fetchList('OperatorSequence/Select', { ServiceID: serviceId });
}

export function insertProviderSequence(payload: {
  ServiceID: number;
  OperatorID: number;
  ProviderList: Array<{ ProviderID: number; ProviderName: string; Count: number }>;
  ViewProviderID: number;
  RealProviderID: number;
}) {
  return apiPost('OperatorSequence/Insert', payload);
}

export function bulkUpdateSequence(rows: BulkUpdateRow[]) {
  return apiPost('OperatorSequence/BulkUpdate', rows);
}

/**
 * Providers that can dispatch for this service's rail — the same set the grid rows show, so a
 * bulk "Add sequence" can never offer a provider the rows below it do not have.
 */
export function fetchBulkSequenceProviders(serviceId: number): Promise<ListRow[]> {
  return fetchList('OperatorSequence/SelectProviderForBulkSequence', { ServiceID: serviceId });
}

/**
 * The services Provider Routing may actually route: the operator-based rails (recharge and
 * BBPS) that have operators. DMT, AEPS, payment gateway, payout and verification each route
 * from their own screen, so offering them here only produced a grid that never took effect.
 */
export function fetchRoutableServices(): Promise<ListRow[]> {
  return fetchList('OperatorSequence/SelectRoutableServices', {});
}

export function fetchViewApiProviders(): Promise<ListRow[]> {
  return fetchList('OperatorSequence/SelectForViewAPIDropDown', {});
}

// ── User-wise sequence ──────────────────────────────────────────────────────────

export function fetchAllUserwiseSequences(): Promise<ListRow[]> {
  return fetchList('OperatorSequenceUserWise/SelectAllUserwiseSequenceList', {});
}

export function fetchUserwiseSequence(userId: number, serviceId: number): Promise<ListRow[]> {
  return fetchList('OperatorSequenceUserWise/Select', { UserID: userId, ServiceID: serviceId });
}

export function insertUserwiseSequence(payload: {
  UserID: number;
  ServiceID: number;
  OperatorID: number;
  ProviderList: Array<{ ProviderID: number; ProviderName: string }>;
}) {
  return apiPost('OperatorSequenceUserWise/Insert', payload);
}

export function deleteUserwiseSequence(userId: number, operatorId: number) {
  return apiPost('OperatorSequenceUserWise/DeleteByUserID', { UserID: userId, OperatorID: operatorId });
}

export function deleteAllUserwiseSequences() {
  return apiPost('OperatorSequenceUserWise/DeleteAll', {});
}
