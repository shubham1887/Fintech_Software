/** A single column in a transaction report (explicit, mirrors the old UI's column set). */
export interface ReportColumn {
  /** Data field on each row. */
  key: string;
  label: string;
  /**
   * Rendering: money=₹, date=formatted, status=badge, bold, image=logo, number=right-aligned,
   * serial=running row number (1,2,3…) instead of a raw id.
   *
   * `signedMoney` is for figures whose SIGN is information rather than noise — the commission
   * tiers are signed in this system (positive = commission earned, negative = a surcharge the
   * user paid), as are profit and reconciliation differences. It renders the magnitude with a
   * Cr / Dr marker and a colour, instead of a leading minus that reads as a negative balance.
   */
  type?: 'text' | 'money' | 'signedMoney' | 'plusMinusMoney' | 'date' | 'status' | 'bold' | 'image' | 'number' | 'serial';
  /** Visible only for these UserTypeIDs (omit = visible to all roles). */
  roles?: number[];
  /**
   * How this column's figure in the Total row is reached. Omitted means a sum, which is right for
   * everything that accumulates.
   *
   * A BALANCE does not accumulate: the period's opening is the first row's opening and its
   * closing is the last row's closing. Summed instead, a week of rows reports about seven times
   * the float as the network's balance.
   */
  totalMode?: 'sum' | 'first' | 'last';
}

/** Which standard filters a report shows. */
export interface TxnReportFilters {
  service?: boolean; // Service dropdown (cascades to Operator)
  operator?: boolean; // Operator dropdown (depends on selected Service)
  status?: 'recharge' | 'dmt' | 'simple' | 'hold' | 'fund'; // status option set
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
   * Drop the From / To date inputs. Almost every report is a date range, but a few describe
   * current configuration rather than history — a commission structure is what the slab pays
   * today, not what it paid last week — and offering dates there only invites the question of
   * why changing them does nothing.
   */
  hideDates?: boolean;
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
  /**
   * Request fields this screen always sends, on top of the filter values.
   *
   * For endpoints that serve two readings of the same data. `Fund/ReverseReport` is the case
   * this was added for: it returns reverse REQUESTS and completed REVERSALS together, and the
   * "Reverse Requests" queue must only ever show the requests — a queue that also lists
   * finished reversals is not a queue. The narrowing belongs to the screen, not the operator,
   * so it is fixed here rather than offered as a filter they could clear by accident.
   */
  fixedPayload?: Record<string, unknown>;
  /**
   * Open on TODAY rather than the last seven days.
   *
   * The seven-day default was wrong for the reports people live in. A counter asking "what has
   * gone through today" got a week, had to narrow it on every single visit, and read yesterday's
   * figures in the strip above the table until they did. A report that is genuinely about a
   * period — sales analysis, a month's purchases — keeps the week.
   */
  defaultFromToday?: boolean;
  /**
   * Open the Service dropdown on the FIRST service rather than on "All services".
   *
   * For a report that is really about one service at a time. A commission structure for every
   * service at once is three different rate cards interleaved, and picking a service was the
   * first thing anyone did on that screen every single time.
   */
  defaultFirstService?: boolean;
  /**
   * Status the Status dropdown starts on (default 'ALL').
   *
   * A queue screen opens on the state that needs work — pending — instead of making the
   * operator narrow it on every visit. The dropdown still works, so the decided rows are one
   * change away.
   */
  defaultStatus?: string;
  /** Roles for whom this whole report is hidden (e.g. Commission Structure is not shown to admin). */
  hideForRoles?: number[];
  /** Row-action labels shown in the per-row kebab menu (Print, Log, Complaint…). */
  actions?: string[];
  /**
   * Endpoint the "Status Check" row action asks the PROVIDER with, e.g.
   * `TransactionProcessing/GetDMTTransctionStatus`. It takes `{ ID, TransactionID }` and settles
   * the transaction from the provider's answer (wallet and commissions included), so the dialog
   * confirms before sending and shows the resulting status.
   *
   * Without it, "Status Check" and "Log" both opened the transaction log — the same panel under
   * two names, and nothing ever actually re-asked the provider.
   */
  statusCheckEndpoint?: string;
  /**
   * Adds a working "Manual Success / Failed" row action that settles the transaction through
   * TransactionProcessing/UpdateStatusManually (wallet + commission reversal on failure).
   */
  manualSettle?: boolean;
}
