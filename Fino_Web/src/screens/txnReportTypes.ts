/** A single column in a transaction report (explicit, mirrors the old UI's column set). */
export interface ReportColumn {
  /** Data field on each row. */
  key: string;
  label: string;
  /** Rendering: money=₹, date=formatted, status=badge, bold, image=logo, number=right-aligned,
   *  serial=running row number (1,2,3…) instead of a raw id. */
  type?: 'text' | 'money' | 'date' | 'status' | 'bold' | 'image' | 'number' | 'serial';
  /** Visible only for these UserTypeIDs (omit = visible to all roles). */
  roles?: number[];
}

/** Which standard filters a report shows. */
export interface TxnReportFilters {
  service?: boolean; // Service dropdown (cascades to Operator)
  operator?: boolean; // Operator dropdown (depends on selected Service)
  status?: 'recharge' | 'dmt' | 'simple'; // status option set
  user?: boolean; // Entity (user) autocomplete — admin/distributor only
  complaint?: boolean; // Complaint dropdown
  criteria?: boolean; // Selection-criteria dropdown + value field
  provider?: boolean; // Provider dropdown
  /** Wallet dropdown (sent as `WalletID`) — for reports that reconcile one wallet at a time. */
  wallet?: boolean;
  /** Credit / Debit dropdown (sent as `AmountCreditDebit`: ''|'Credit'|'Debit'). */
  amountType?: boolean;
  /** Free-text box sent as the request's `Search` term (endpoints that match on it directly). */
  search?: { label: string; placeholder?: string };
  /**
   * Grouping toggle sent as the request's `GroupBy`. Lets one report switch aggregation
   * granularity (e.g. Purchase Report: month-wise vs date-wise) without a second screen.
   */
  groupBy?: { label: string; options: { label: string; value: string }[]; default?: string };
}

export interface TxnReportConfig {
  endpoint: string;
  columns: ReportColumn[];
  filters?: TxnReportFilters;
  /** Roles for whom this whole report is hidden (e.g. Commission Structure is not shown to admin). */
  hideForRoles?: number[];
  /** Row-action labels shown in the per-row kebab menu (Print, Log, Complaint…). */
  actions?: string[];
  /**
   * Adds a working "Manual Success / Failed" row action that settles the transaction through
   * TransactionProcessing/UpdateStatusManually (wallet + commission reversal on failure).
   */
  manualSettle?: boolean;
}
