import { apiPost } from '@/lib/apiClient';
import { fetchList, type ListRow } from './list.api';

export interface RechargePayload {
  operatorId: number;
  serviceId: number;
  consumerNumber: string;
  amount: number;
  mobileNo?: string;
  pin?: string;
  /** Extra biller parameters from the operator's BBPS field configuration. */
  param1?: string;
  param2?: string;
  param3?: string;
  customerName?: string;
}

/** Settlement result returned by the recharge pipeline (shape is provider-dependent). */
export interface RechargeResult {
  TransactionMasterID?: number;
  Status?: string;
  Message?: string;
  OperatorRefNo?: string;
  [key: string]: unknown;
}

/** POST api/TransactionProcessing/Recharge — full debit → dispatch → settle pipeline. */
export function doRecharge(p: RechargePayload) {
  return apiPost<RechargeResult>('TransactionProcessing/Recharge', {
    OperatorID: p.operatorId,
    ServiceID: p.serviceId,
    ConsumerNumber: p.consumerNumber,
    Amount: p.amount,
    MobileNo: p.mobileNo ?? '',
    Pin: p.pin ?? '',
    Through: 'WEB',
    APIUserRequestID: '',
    Param1: p.param1 ?? '',
    Param2: p.param2 ?? '',
    Param3: p.param3 ?? '',
    BBPS_CustomerName: p.customerName ?? '',
    Latitude: '',
    Longitude: '',
  });
}

/** One dynamic input field for a biller, from BBPSConfiguration (FK → OperatorID). */
export interface BillerField {
  id: number;
  label: string;
  type: string; // text | number | list | date
  placeholder: string;
  required: boolean;
  error: string;
  length: number;
  options: string[];
}

/** The operator's configured bill-input fields, ordered by Sequence. */
export async function fetchBillerFields(operatorId: number): Promise<BillerField[]> {
  const rows = await fetchList('BBPSConfiguration/GetAll', { OperatorID: operatorId, PageNumber: 1, PageSize: 50 });
  return rows
    .map((r) => ({
      id: Number(r.BBPSConfigurationID ?? 0),
      label: String(r.Label ?? ''),
      type: String(r.Type ?? 'text').toLowerCase(),
      placeholder: String(r.PlaceHolder ?? ''),
      required: Boolean(r.IsRequired),
      error: String(r.ErrorMeeage ?? ''),
      length: Number(r.Length ?? 0),
      options: String(r.ConfigList ?? '').split(',').map((s) => s.trim()).filter(Boolean),
      display: r.IsDisplay === undefined ? true : Boolean(r.IsDisplay),
      sequence: Number(r.Sequence ?? 0),
    }))
    .filter((f) => f.label && f.display)
    .sort((a, b) => a.sequence - b.sequence);
}

/** Live bill details through the provider gateway (Utility/BillFetch). */
export interface BillDetails {
  Status?: string;
  CustomerName?: string;
  BillAmount?: number | null;
  DueDate?: string;
  BillDate?: string;
  BillNumber?: string;
  Message?: string;
}

export function fetchBill(p: { operatorId: number; serviceId: number; consumerNumber: string; param1?: string; param2?: string; param3?: string }) {
  return apiPost<BillDetails>('Utility/BillFetch', {
    OperatorID: p.operatorId,
    ServiceID: p.serviceId,
    ConsumerNumber: p.consumerNumber,
    Param1: p.param1 ?? '',
    Param2: p.param2 ?? '',
    Param3: p.param3 ?? '',
  });
}

/** All operators (full master rows — ServiceID/Name, image, length & amount limits).
 *  No IsActive filter: the dropdown shows active ones, but old transactions still need
 *  names/logos of operators that were switched off later. */
export function fetchOperators(): Promise<ListRow[]> {
  return fetchList('Operator/GetAll', { PageNumber: 1, PageSize: 500 });
}

/** Caller's recent transactions (RECHARGE scope), newest first. */
export function fetchRecentRecharges(pageSize = 10): Promise<ListRow[]> {
  return fetchList('TransactionProcessing/Select', {
    Status: '',
    ServiceType: '',
    FromDate: '',
    ToDate: '',
    PageNumber: 1,
    PageSize: pageSize,
  });
}

/** Operator + circle/state auto-detect for a mobile number (planapi.in OperatorFetchNew).
 *  Response fields: Operator, OpCode, Circle, CircleCode. */
export interface DetectedOperator {
  Operator?: string;
  OpCode?: string;
  Circle?: string;
  CircleCode?: string;
  [key: string]: unknown;
}
export function fetchOperatorByMobile(mobileNo: string) {
  return apiPost<DetectedOperator>('ROffer/GetOperatorByMobile', { MobileNo: mobileNo });
}

/** Live plan list from planapi.in — DTH uses the DTH endpoint, everything else mobile plans.
 *
 *  Send the OperatorID, not the operator master's Opcode: that column holds the mnemonic used to
 *  ROUTE a recharge ("AIRT", "TPLY"), while the plan provider has its own numeric codes ("2", "28").
 *  The server resolves OperatorID → provider code. `opCode` is only for DTH, where the provider's
 *  own code came back from the DTH-number detection.
 *
 *  `circle` is the circle CODE (e.g. "70" = Rajasthan); the mobile plan API returns an empty
 *  RDATA without it. */
export function fetchPlans(operatorId: number, isDth: boolean, circle = '', opCode = '') {
  const body = { OperatorID: operatorId, OpCode: opCode, Circle: circle };
  return isDth ? apiPost<unknown>('ROffer/SelectDTHPlan', body) : apiPost<unknown>('ROffer/SelectPlan', body);
}

/** R-Offer (special offers for one mobile number). */
export function fetchOffers(operatorId: number, mobileNo: string, circle = '') {
  return apiPost<unknown>('ROffer/SelectOffer', { OperatorID: operatorId, MobileNo: mobileNo, Circle: circle });
}

/** DTH operator auto-detect from the customer/VC number (planapi.in DthOperatorFetch).
 *  Response fields: DthNumber, DthName, DthOpCode. */
export function fetchOperatorByDthNumber(dthNumber: string) {
  return apiPost<unknown>('ROffer/GetOperatorByDTHNumber', { DthNumber: dthNumber });
}

/** How much DTH customer detail to pull. The provider prices these differently — `full` is
 *  billed as 3 hits — so the screen asks for `basic` by default. */
export type DthInfoDepth = 'basic' | 'full' | 'lastRecharge';

const DTH_INFO_ENDPOINT: Record<DthInfoDepth, string> = {
  basic: 'ROffer/SelectDTHBasicInfo',
  full: 'ROffer/SelectDTHCustInfo',
  lastRecharge: 'ROffer/SelectDTHInfoWithLastRecharge',
};

/** DTH customer details for a customer/VC number — name, RMN, balance, plan, address.
 *  `opCode` is the provider's own code when DTH-number detection supplied one; otherwise the
 *  server resolves it from OperatorID. */
export function fetchDthCustomerInfo(operatorId: number, dthNumber: string, depth: DthInfoDepth = 'basic', opCode = '') {
  return apiPost<unknown>(DTH_INFO_ENDPOINT[depth], { OperatorID: operatorId, OpCode: opCode, DthNumber: dthNumber });
}
