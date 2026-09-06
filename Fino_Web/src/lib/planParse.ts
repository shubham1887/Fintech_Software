import { getField, type Row } from './row';

/**
 * Parsers for the planapi.in provider payloads. The provider returns three quite different
 * shapes and the recharge screen needs one common structure out of all of them:
 *
 *  - Mobile plans   RDATA = { "Data": [{rs, validity, desc}], "Truly Unlimited": [...], ... }
 *  - R-Offer        RDATA = [{price, ofrtext, logdesc, ...}]
 *  - DTH plans      RDATA = { "Combo": [{Language, Details: [{PlanName, Channels, PricingList: [{Amount, Month}]}]}] }
 *
 * Everything collapses to PlanGroup[] (a named tab of PlanRow), so one modal renders all three.
 */

/** One selectable plan / offer line. */
export interface PlanRow {
  /** Numeric amount for click-to-fill; null when the provider gave no usable price. */
  amount: number | null;
  /** Amount exactly as the provider wrote it (may carry ₹ or be blank). */
  amountText: string;
  validity: string;
  detail: string;
  /** Secondary line — R-Offer log text, DTH channel counts. */
  note: string;
}

/** A provider category — becomes one tab in the plan modal. */
export interface PlanGroup {
  name: string;
  rows: PlanRow[];
}

export interface PlanResult {
  groups: PlanGroup[];
  operator: string;
  circle: string;
  /** Provider MESSAGE — surfaced when it returned no rows. */
  message: string;
  /** Provider ERROR flag: "0" means success. */
  error: string;
}

/** "₹3,599" / "361" / 3599 → 3599. Returns null when there is no usable number. */
function toAmount(v: unknown): number | null {
  const n = parseFloat(String(v ?? '').replace(/[^\d.]/g, ''));
  return Number.isFinite(n) && n > 0 ? n : null;
}

function text(rec: Row, ...names: string[]): string {
  const v = getField(rec, ...names);
  return v === undefined || v === null ? '' : String(v).trim();
}

/**
 * Turns one provider record into plan rows. A DTH record carries a PricingList (one price per
 * tenure, all sharing the pack's name and channel counts), so it expands to several rows;
 * everything else is a single row.
 */
function rowsFrom(rec: Row): PlanRow[] {
  const pricing = getField(rec, 'PricingList');
  if (Array.isArray(pricing)) {
    const detail = text(rec, 'PlanName', 'Plan', 'Name');
    const note = [text(rec, 'Channels'), text(rec, 'PaidChannels'), text(rec, 'HdChannels')]
      .filter(Boolean)
      .join(' · ');
    return pricing
      .filter((p): p is Row => Boolean(p) && typeof p === 'object')
      .map((p) => ({
        amount: toAmount(getField(p, 'Amount', 'amount', 'rs', 'price')),
        amountText: text(p, 'Amount', 'amount', 'rs', 'price'),
        validity: text(p, 'Month', 'Months', 'validity', 'Validity'),
        detail,
        note,
      }))
      .filter((r) => r.amount !== null || r.detail);
  }

  const amountText = text(rec, 'rs', 'price', 'amount', 'Amount', 'PlanAmount');
  const detail = text(rec, 'desc', 'ofrtext', 'detail', 'description', 'Description', 'PlanName', 'plan_name', 'offer');
  const note = text(rec, 'logdesc', 'log_desc', 'Channels');
  const validity = text(rec, 'validity', 'Validity', 'Month', 'validity_desc');
  if (!amountText && !detail) return [];
  return [{ amount: toAmount(amountText), amountText, validity, detail, note }];
}

/** Walks the provider node, emitting one group per named category it finds. */
function collect(node: unknown, name: string, out: PlanGroup[], depth: number): void {
  if (node === null || node === undefined || depth > 6) return;

  if (Array.isArray(node)) {
    const direct: PlanRow[] = [];
    for (const item of node) {
      if (!item || typeof item !== 'object') continue;
      const rec = item as Row;
      // DTH nests a second level: the array item is a language/section wrapper holding Details[].
      const details = getField(rec, 'Details');
      if (Array.isArray(details)) {
        const label = text(rec, 'Language', 'Name', 'Title', 'Category');
        collect(details, label ? `${name} · ${label}` : name, out, depth + 1);
        continue;
      }
      direct.push(...rowsFrom(rec));
    }
    if (direct.length) out.push({ name, rows: direct });
    return;
  }

  if (typeof node !== 'object') return;
  const rec = node as Row;
  // A record that is itself a plan (rather than a container of categories).
  const own = rowsFrom(rec);
  if (own.length) {
    out.push({ name, rows: own });
    return;
  }
  for (const [key, value] of Object.entries(rec)) {
    if (value && typeof value === 'object') collect(value, key, out, depth + 1);
  }
}

/** Parses any plan / offer / DTH-plan response into named groups. */
export function parsePlanResponse(data: unknown, fallbackGroupName = 'Plans'): PlanResult {
  const root: Row = data && typeof data === 'object' ? (data as Row) : {};
  const rdata = getField(root, 'RDATA', 'rdata', 'Records', 'records');
  const groups: PlanGroup[] = [];
  collect(rdata === undefined ? root : rdata, fallbackGroupName, groups, 0);
  return {
    groups: groups.filter((g) => g.rows.length > 0),
    operator: text(root, 'Operator', 'OperatorName'),
    circle: text(root, 'Circle'),
    message: text(root, 'MESSAGE', 'Message', 'message'),
    error: text(root, 'ERROR', 'error'),
  };
}

// --- DTH customer info -------------------------------------------------------------------

export interface DthInfoField {
  label: string;
  value: string;
}

export interface DthInfo {
  fields: DthInfoField[];
  /** Customer name, pulled out for the summary line. */
  name: string;
  /** Account balance as the provider reported it (blank on the basic-details call). */
  balance: string;
  message: string;
  error: string;
}

/** Provider key → the label shown to the user. Unlisted keys are shown with their own name. */
const DTH_LABELS: Record<string, string> = {
  vc: 'VC / Customer ID',
  name: 'Customer name',
  rmn: 'Registered mobile',
  balance: 'Balance',
  monthly: 'Monthly amount',
  'next recharge date': 'Next recharge date',
  'last recharge date': 'Last recharge date',
  plan: 'Plan',
  address: 'Address',
  city: 'City',
  district: 'District',
  state: 'State',
  'pin code': 'PIN code',
  status: 'Status',
};

/** Parses a DTH basic / full / last-recharge info response into display rows. */
export function parseDthInfo(data: unknown): DthInfo {
  const root: Row = data && typeof data === 'object' ? (data as Row) : {};
  const inner = getField(root, 'DATA', 'Data', 'data', 'RDATA');
  const rec: Row = inner && typeof inner === 'object' && !Array.isArray(inner) ? (inner as Row) : {};
  const fields: DthInfoField[] = [];
  for (const [key, value] of Object.entries(rec)) {
    if (value === null || value === undefined || typeof value === 'object') continue;
    const v = String(value).trim();
    if (!v) continue;
    fields.push({ label: DTH_LABELS[key.trim().toLowerCase()] ?? key.trim(), value: v });
  }
  return {
    fields,
    name: text(rec, 'Name', 'CustomerName'),
    balance: text(rec, 'Balance'),
    message: text(root, 'Message', 'MESSAGE', 'message'),
    error: text(root, 'error', 'ERROR'),
  };
}

// --- DTH operator detection --------------------------------------------------------------

export interface DetectedDthOperator {
  opCode: string;
  opName: string;
  message: string;
  error: string;
}

/** Parses the DthOperatorFetch response (DthName / DthOpCode). */
export function parseDthOperator(data: unknown): DetectedDthOperator {
  const root: Row = data && typeof data === 'object' ? (data as Row) : {};
  return {
    opCode: text(root, 'DthOpCode', 'OpCode', 'Opcode'),
    opName: text(root, 'DthName', 'Operator', 'OperatorName'),
    message: text(root, 'Message', 'MESSAGE', 'message'),
    error: text(root, 'ERROR', 'error'),
  };
}
