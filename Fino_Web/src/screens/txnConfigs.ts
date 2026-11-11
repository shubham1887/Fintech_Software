import type { TxnReportConfig } from './txnReportTypes';

// UserTypeID role codes (mirror StaticModels/UserTypeCode): Admin=1, SuperDist=2, Dist=3, Retailer=4, ApiUser=5, Support=6
const ADMIN = [1, 6];
/**
 * Admin, Support AND the Accountant (9).
 *
 * The accounting columns — provider commission, GST on it, TDS, net profit — were gated on ADMIN,
 * which is Admin + Support only, so the one role whose whole job is to read them could not see
 * them. The accountant is already granted this data by ApiAccessPolicy and it is in the response
 * either way (a hidden column is presentation, never security), so hiding it only made the daybook
 * look broken for them.
 */
const ACCOUNTING = [1, 6, 9];
const ADMIN_DIST = [1, 6, 2, 3];
const ADMIN_API = [1, 6, 5];

/**
 * Recharge report — columns + filters mirror the legacy UI's RechargeReport.js exactly
 * (so the data/parameters match), but rendered in our own design via TxnReportScreen.
 * Endpoint + payload verified against Components/Api/TransactionReportAPI.js.
 */
export const RECHARGE_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectForRechargeFilter',
  filters: { service: true, operator: true, status: 'recharge', user: true, complaint: true, criteria: true },
  defaultFromToday: true,
  actions: ['Print', 'Complaint', 'Wrong 2 Right', 'Success / Fail', 'Log', 'Hit Callback'],
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'UserMobile', label: 'Entity Mobile' },
    { key: 'OperatorName', label: 'Operator' },
    // Operator logo dropped. It is a 24px image repeated down a column whose text already says
    // which operator the row is, and it pushed the figures — the reason anyone opens this
    // report — off the right-hand edge.
    { key: 'ConsumerNumber', label: 'Mobile Number', type: 'bold' },
    { key: 'Status', label: 'Status', type: 'status' },
    // The number the OPERATOR gave the transaction, which is the one a customer quotes and the
    // one a dispute is settled with. This column was labelled "Operator ID" and showed our own
    // ReferenceNo, so the number read off it meant nothing to the operator it was quoted to; our
    // reference is still here, under its own name, further along the row.
    { key: 'ProviderRefNo', label: 'Operator Ref', type: 'bold' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'signedMoney' },
    { key: 'RetailerTDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'signedMoney' },
    { key: 'CommissionInfo', label: 'MDS/DS Commission', roles: ADMIN_DIST },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'ComplaintStatus', label: 'Complaint', type: 'status' },
    { key: 'Circle', label: 'Circle' },
    { key: 'APIUserRequestID', label: 'Request ID', roles: ADMIN_API },
    { key: 'ProcTime', label: 'ProcTime (s)' },
    { key: 'RetailerGST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'DistributerGST', label: 'Dist GST', type: 'money', roles: ADMIN },
    { key: 'DistributerTDS', label: 'Dist TDS', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerGST', label: 'Super GST', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerTDS', label: 'Super TDS', type: 'money', roles: ADMIN },
    { key: 'ReferenceNo', label: 'Txn Reference ID' },
    { key: 'Remark', label: 'Remark' },
  ],
};

/**
 * Quick search — TransactionReport/Search matches a term against consumer number, our
 * reference, the provider reference/request id, or the transaction id itself. Names (wallet,
 * provider, distributor, super distributor) are filled by the report enrichment, so this
 * screen shows people and wallets rather than raw ids.
 */
export const SEARCH_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/Search',
  filters: {
    search: { label: 'Search', placeholder: 'Transaction ID / number / reference' },
    user: true,
  },
  manualSettle: true,
  // Log for the provider exchange, Complaint to raise one against the row, Print for the receipt.
  // The last two were missing here although every service report has them, so the one screen a
  // support desk searches from could not do the two things a search usually ends in.
  actions: ['Log', 'Complaint', 'Print'],
  columns: [
    // Serial (1,2,3…) instead of the raw TransactionMasterID; the id stays on the row for the
    // Log / Manual-settle actions but is not shown as a column.
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'User Name', type: 'bold' },
    { key: 'UserMobile', label: 'User Number' },
    { key: 'ConsumerNumber', label: 'Number' },
    { key: 'OperatorName', label: 'Operator Name' },
    { key: 'WalletName', label: 'Wallet Name' },
    { key: 'ProviderName', label: 'Provider Name', roles: ADMIN },
    // The chain above the user: name AND number for each tier. When the tier is the operator
    // itself (a retailer parked directly under the admin) the API writes "Admin".
    { key: 'DistributerName', label: 'Distributor', roles: ADMIN_DIST },
    { key: 'DistributerMobile', label: 'Distributor No.', roles: ADMIN_DIST },
    { key: 'SuperDistributerName', label: 'Super-Distributor', roles: ADMIN_DIST },
    { key: 'SuperDistributerMobile', label: 'Super-Distributor No.', roles: ADMIN_DIST },
    { key: 'Amount', label: 'Amount', type: 'money' },
    // Deliberately NO balance, commission or surcharge columns: this is the lookup screen, and a
    // search result read over a shoulder at a counter is not the place for the user's balance.
    // Those figures are on the service reports and on the detail modal.
    { key: 'Status', label: 'Status', type: 'status' },
  ],
};

/**
 * Same search for non-admin roles: no manual-settle action (the API restricts settlement to
 * admin/support anyway) and no user picker — the endpoint scopes non-admins to themselves.
 */
export const RETAILER_SEARCH_REPORT: TxnReportConfig = {
  ...SEARCH_REPORT,
  filters: { search: SEARCH_REPORT.filters!.search },
  manualSettle: false,
};

/** Top performance — legacy NPerformance.js (TransactionReport/SelectTopPerformer). */
export const TOP_PERFORMANCE: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectTopPerformer',
  filters: { service: true, user: true },
  columns: [
    { key: 'Rank', label: '#', type: 'number' },
    { key: 'FirmName', label: 'Firm Name', type: 'bold' },
    { key: 'Role', label: 'Role' },
    { key: 'UserMobile', label: 'Mobile' },
    { key: 'EmailID', label: 'Email' },
    { key: 'SuccessCount', label: 'Txns', type: 'number' },
    { key: 'TotalSale', label: 'Total Sale', type: 'money' },
    { key: 'TotalCommission', label: 'Commission', type: 'signedMoney' },
  ],
};

/** Downline — legacy Users/DownlineReport.js (Users/SelectDownlineReport). */
export const DOWNLINE_REPORT: TxnReportConfig = {
  endpoint: 'Users/SelectDownlineReport',
  // No dates: this is a SNAPSHOT of what the network holds right now, not a period. Offering a
  // range only invites the question of why changing it does nothing.
  filters: { user: true, hideDates: true },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'FirmName', label: 'Firm Name', type: 'bold' },
    { key: 'Role', label: 'Role' },
    { key: 'UserMobile', label: 'Mobile' },
    { key: 'ParentName', label: 'Upline' },
    { key: 'RechargeBalance', label: 'RCH Bal', type: 'money' },
    { key: 'DMTBalance', label: 'DMT Bal', type: 'money', roles: ADMIN_DIST },
    { key: 'AEPSBalance', label: 'AEPS Bal', type: 'money' },
    { key: 'TotalBalance', label: 'Total Bal', type: 'money' },
    { key: 'Outstanding', label: 'On Hold', type: 'money', roles: ADMIN },
    { key: 'Status', label: 'Status', type: 'status' },
  ],
};

/** Newly-added users — legacy Users/DatewiseUserReport.js (Users/SelectDatewise). */
export const NEWLY_ADDED_USERS: TxnReportConfig = {
  endpoint: 'Users/SelectDatewise',
  filters: { user: true },
  columns: [
    { key: 'UserID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date', type: 'date' },
    { key: 'FirmName', label: 'Firm Name', type: 'bold' },
    { key: 'IsKYCApprovedStr', label: 'KYC', type: 'status' },
    { key: 'Balances', label: 'Balances' },
    { key: 'Role', label: 'Role' },
    { key: 'JoiningBy', label: 'Joining By' },
    { key: 'RegistrationType', label: 'Type' },
  ],
};

/** User onboarding list — legacy UserOnbordedReport.js (Users/SelectUserOnbordingList). */
export const USER_ONBOARDED: TxnReportConfig = {
  endpoint: 'Users/SelectUserOnbordingList',
  filters: { user: true },
  columns: [
    { key: 'id', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date', type: 'date' },
    { key: 'FirmName', label: 'Firm Name', type: 'bold' },
    { key: 'MerchantCode', label: 'Merchant Code' },
  ],
};

/** Operator-wise user-wise daily sale — legacy OperatorWiseUserWiseReport.js. */
export const OPERATOR_USER_REPORT: TxnReportConfig = {
  endpoint: 'TransactionWiseReport/SelectOperatorWisesUserWiseReport',
  filters: { user: true },
  columns: [
    { key: 'DOC', label: 'Date', type: 'date' },
    { key: 'MobileSale', label: 'Mobile', type: 'money' },
    { key: 'DTHSale', label: 'DTH', type: 'money' },
    { key: 'PostPaid', label: 'Post Paid', type: 'money' },
    { key: 'Electricity', label: 'Electricity', type: 'money' },
    { key: 'MoneyTransfer', label: 'DMT', type: 'money' },
    { key: 'ExpressMoneyTransfer', label: 'Express DMT', type: 'money' },
    { key: 'AepsSale', label: 'AEPS', type: 'money' },
    { key: 'PaymentGatewaySale', label: 'PG', type: 'money' },
  ],
};

/** User-wise service sell (yesterday/today) — legacy UserWiseServiceSellReport.js. */
export const USER_SERVICE_SELL: TxnReportConfig = {
  endpoint: 'TransactionWiseReport/SelectUserWiseOperatorWiseSellReport',
  // The report IS "yesterday versus today", so the two days are its subject rather than a filter.
  filters: { user: true, hideDates: true },
  columns: [
    { key: 'UserID', label: '#', type: 'number' },
    { key: 'FirmName', label: 'Firm Name', type: 'bold' },
    { key: 'Role', label: 'Role' },
    { key: 'YesterdayMobileSale', label: 'Y-Mobile', type: 'money' },
    { key: 'TodayMobileSale', label: 'T-Mobile', type: 'money' },
    { key: 'YesterdayDTHSale', label: 'Y-DTH', type: 'money' },
    { key: 'TodayDTHSale', label: 'T-DTH', type: 'money' },
    { key: 'YesterdayPostPaid', label: 'Y-Post Paid', type: 'money' },
    { key: 'TodayPostPaid', label: 'T-Post Paid', type: 'money' },
    { key: 'YesterdayElectricity', label: 'Y-Electricity', type: 'money' },
    { key: 'TodayElectricity', label: 'T-Electricity', type: 'money' },
    { key: 'YesterdayMoneyTransfer', label: 'Y-DMT', type: 'money' },
    { key: 'TodayMoneyTransfer', label: 'T-DMT', type: 'money' },
  ],
};

/** User-wise DMT range — legacy UserWiseDMTRangeReport.js (amount buckets). */
export const USER_DMT_RANGE: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectUserWiseDMTRangeReport',
  filters: { user: true, wallet: true, service: true, operator: true },
  columns: [
    { key: 'UserName', label: 'User', type: 'bold' },
    { key: 'Range1_2', label: '1–2', type: 'money' },
    { key: 'Range3_1000', label: '3–1k', type: 'money' },
    { key: 'Range1001_2000', label: '1k–2k', type: 'money' },
    { key: 'Range2001_3000', label: '2k–3k', type: 'money' },
    { key: 'Range3001_4000', label: '3k–4k', type: 'money' },
    { key: 'Range4001_5000', label: '4k–5k', type: 'money' },
    { key: 'Range5001_10000', label: '5k–10k', type: 'money' },
    { key: 'Range10001_15000', label: '10k–15k', type: 'money' },
    { key: 'Range15001_20000', label: '15k–20k', type: 'money' },
    { key: 'Range20001_25000', label: '20k–25k', type: 'money' },
    { key: 'Range25001_50000', label: '25k–50k', type: 'money' },
    { key: 'Range50001_1L', label: '50k–1L', type: 'money' },
    { key: 'TotalTransactions', label: 'Total Count', type: 'number' },
  ],
};

/** Provider daybook — legacy DayBookReport/ProviderDayBook.js (DayBookReport/ProviderDaybook). */
export const PROVIDER_DAYBOOK: TxnReportConfig = {
  endpoint: 'DayBookReport/ProviderDaybook',
  filters: { user: true },
  columns: [
    { key: 'ProviderName', label: 'API', type: 'bold' },
    { key: 'Operator', label: 'Operator' },
    { key: 'TotalHits', label: 'Total', type: 'number' },
    { key: 'TotalAmount', label: 'T Amt', type: 'money' },
    { key: 'SuccessHits', label: 'Success', type: 'number' },
    { key: 'SuccessAmount', label: 'S Amt', type: 'money' },
    { key: 'FailedHits', label: 'Failed', type: 'number' },
    { key: 'FailedAmount', label: 'F Amt', type: 'money' },
    { key: 'PendingHits', label: 'Pending', type: 'number' },
    { key: 'PendingAmount', label: 'P Amt', type: 'money' },
    { key: 'AdminCommission', label: 'Admin Comm', type: 'signedMoney', roles: ADMIN },
    { key: 'ProviderCommission', label: 'API Comm', type: 'signedMoney', roles: ADMIN },
    { key: 'RetailerCommission', label: 'R. Comm', type: 'signedMoney' },
    { key: 'DistributerCommission', label: 'D. Comm', type: 'signedMoney', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'S. Comm', type: 'signedMoney', roles: ADMIN },
    { key: 'Percentage', label: 'Per (%)' },
    { key: 'Profit', label: 'Profit', type: 'signedMoney', roles: ADMIN },
  ],
};

/** Income report — legacy IncomeReport.js (TransactionReport/Income). */
export const INCOME_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/Income',
  filters: { service: true, user: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'ServiceName', label: 'Service', type: 'bold' },
    { key: 'SuccessAmt', label: 'Success Amt', type: 'money' },
    { key: 'SuccessCount', label: 'Success Count', type: 'number' },
    { key: 'RCommission', label: 'R Commission', type: 'signedMoney' },
    { key: 'DCommission', label: 'D Commission', type: 'signedMoney', roles: ADMIN_DIST },
    { key: 'SCommission', label: 'S Commission', type: 'signedMoney', roles: ADMIN },
    { key: 'AdminCommission', label: 'Admin Commission', type: 'signedMoney', roles: ADMIN },
    { key: 'NetProfit', label: 'Net Profit', type: 'signedMoney', roles: ACCOUNTING },
  ],
};

/** Userwise accounting — legacy UserwiseAccounting.js (UserAccounting/UserwiseAccounting). */
export const USERWISE_ACCOUNTING: TxnReportConfig = {
  endpoint: 'UserAccounting/UserwiseAccounting',
  // Reconciles ONE wallet as at ONE day: the "To date" is that day and the wallet picker chooses
  // the book. With no wallet selected the API falls back to the first wallet in display order.
  filters: { user: true, wallet: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'FirmName', label: 'Name', type: 'bold' },
    { key: 'UserMobile', label: 'Mobile' },
    { key: 'Role', label: 'Role' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'OpeningBal', label: 'Opening Bal (A)', type: 'money' },
    { key: 'Purchase', label: 'Fund Transfer (B)', type: 'money' },
    { key: 'TotalOldAmountAdjustment', label: 'Old Amt Adj (C)', type: 'money' },
    { key: 'TotalNewAmountAdjustment', label: 'New Amt Adj (D)', type: 'money' },
    { key: 'TotalCrAmt', label: 'Total Cr Amt', type: 'money' },
    { key: 'Sale', label: 'Sale (E)', type: 'money' },
    { key: 'CommsionAmount', label: 'Txn Comm (F)', type: 'money' },
    { key: 'Surcharge', label: 'Txn Surcharge (G)', type: 'money' },
    { key: 'TotalDrAmt', label: 'Total Dr Amt', type: 'money' },
    { key: 'ManualUpdateCount', label: 'Manual Update', type: 'number' },
    { key: 'ClosingBalance', label: 'Closing Bal', type: 'money' },
  ],
};

/**
 * Settlement report — AEPS earnings moved out of the AEPS wallet.
 *
 * This is what "settlement" means on this platform: cash a retailer has dispensed over the
 * counter accrues in their AEPS wallet, and settling it is moving that balance into a wallet they
 * can spend from (Recharge, DMT) or pay out of. The screen used to render the ACCOUNT STATEMENT
 * config — every ledger posting the user had, which is a different report entirely and the reason
 * the figures on it never matched anything anyone was reconciling.
 *
 * The API pairs the two ledger legs of each move (the debit out of AEPS and the credit in), so
 * one settlement is one row and the amount is not counted twice; the from/to balances on the row
 * are the two wallets' own.
 */
export const MOVE_FUNDS: TxnReportConfig = {
  endpoint: 'Fund/SelectWalletTransferReport',
  filters: { user: true, wallet: true },
  defaultFromToday: true,
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'TransactionDOC', label: 'Date & Time', type: 'date' },
    { key: 'SenderName', label: 'Entity Name', type: 'bold' },
    // One row per move, reading the way it was made: "AEPS → Recharge". The API pairs the two
    // ledger legs (the debit out and the credit in) so the amount is not counted twice.
    { key: 'WalletName', label: 'Wallet Move', type: 'bold' },
    { key: 'Amount', label: 'Transfer Amt', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'SenderOpeningBalance', label: 'From Op Bal', type: 'money' },
    { key: 'SenderClosingBalance', label: 'From Cl Bal', type: 'money' },
    { key: 'ReceiverOpeningBalance', label: 'To Op Bal', type: 'money' },
    { key: 'ReceiverClosingBalance', label: 'To Cl Bal', type: 'money' },
    { key: 'Remark', label: 'Remark' },
  ],
};

/** Daybook — legacy DayBookReport/Select.js (operator-wise aggregate). */
export const DAYBOOK_REPORT: TxnReportConfig = {
  endpoint: 'DayBookReport/Select',
  filters: { service: true, operator: true, user: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'Operator', label: 'Operator', type: 'bold' },
    { key: 'ServiceName', label: 'Service' },
    { key: 'TotalCount', label: 'Total', type: 'number' },
    { key: 'TotalAmount', label: 'Total Amt', type: 'money' },
    { key: 'SuccessCount', label: 'Success', type: 'number' },
    { key: 'SuccessAmount', label: 'Success Amt', type: 'money' },
    // Failed is deliberately not a column. A daybook is what the day EARNED, and a failed
    // transaction earns nothing — it was refunded in full, commissions and all. Showing it beside
    // the money columns invited the figures to be read as though some of it counted. The counts
    // are still in the payload for anything that needs them; the Pending columns stay because
    // pending money is genuinely still out there.
    { key: 'PendingCount', label: 'Pending', type: 'number' },
    { key: 'PendingAmount', label: 'Pending Amt', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'signedMoney' },
    { key: 'DistributerCommission', label: 'D. Comm', type: 'signedMoney', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'SD. Comm', type: 'signedMoney', roles: ACCOUNTING },
    { key: 'Surcharge', label: 'Surcharge', type: 'money' },
    // What the provider paid us, and what the house actually keeps. "Admin commission" is gone as
    // a label: it was the same arithmetic but read as a payout rather than as profit, and it was
    // computed from the commission AS QUOTED — so a commission quoted inclusive of GST overstated
    // it by the tax inside. Net profit is from the figure that is actually income.
    // signedMoney, not money: on DMT the provider CHARGES us, so this figure is negative for the
    // highest-value service on the install. Rendering it unsigned read as income we never earned.
    { key: 'ProviderCommission', label: 'Provider Comm', type: 'signedMoney', roles: ACCOUNTING },
    // Just "GST": in GROSS mode this is the tax on the whole throughput, not on the commission,
    // so a label naming the commission would be wrong for half the configurations. Negative here
    // is input credit — the GST inside a charge the provider levied on us.
    { key: 'GSTOnCommission', label: 'GST (Prov)', type: 'signedMoney', roles: ACCOUNTING },
    // The surcharge split the same way. The gross figure is the 'Surcharge' column above; this is
    // the part that is income, and the GST beside it is output tax we owe.
    { key: 'SurchargeNet', label: 'Surcharge Net', type: 'money', roles: ACCOUNTING },
    { key: 'GSTOnSurcharge', label: 'GST (Out)', type: 'money', roles: ACCOUNTING },
    { key: 'TDSDeducted', label: 'TDS', type: 'money', roles: ACCOUNTING },
    { key: 'NetProfit', label: 'Net Profit', type: 'signedMoney', roles: ACCOUNTING },
  ],
};

/** Provider sale — legacy ProviderSale.js (TransactionReport/ProviderSale). */
export const PROVIDER_SALE: TxnReportConfig = {
  endpoint: 'TransactionReport/ProviderSale',
  filters: { user: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'ProvderName', label: 'API', type: 'bold' },
    { key: 'Purchase', label: 'Purchase', type: 'money' },
    { key: 'Sale', label: 'Sale', type: 'money' },
    { key: 'Fail', label: 'Failed', type: 'money' },
    { key: 'RetailerCommission', label: 'R. Comm', type: 'signedMoney' },
    { key: 'DistributerCommission', label: 'D. Comm', type: 'signedMoney', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'S. Comm', type: 'signedMoney', roles: ADMIN },
    { key: 'OpeningBalance', label: 'Op. Bal', type: 'money' },
    { key: 'AdminCommission', label: 'Admin Comm', type: 'signedMoney', roles: ADMIN },
    { key: 'ClosingBalance', label: 'Cl. Bal', type: 'money' },
    { key: 'Difference', label: 'Diff', type: 'signedMoney' },
    { key: 'Profit', label: 'Profit', type: 'signedMoney', roles: ADMIN },
  ],
};

/** Service-wise sale — legacy ServicewiseSale.js (aggregate). */
export const SALE_SERVICEWISE: TxnReportConfig = {
  endpoint: 'TransactionReport/ServicewiseSale',
  filters: { service: true, user: true },
  defaultFromToday: true,
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'ServiceName', label: 'Service', type: 'bold' },
    { key: 'SuccessAmt', label: 'Success Amt', type: 'money' },
    { key: 'SuccessCount', label: 'Success Count', type: 'number' },
    { key: 'FailAmt', label: 'Fail Amt', type: 'money' },
    { key: 'FailCount', label: 'Fail Count', type: 'number' },
    { key: 'PendingAmt', label: 'Pending Amt', type: 'money' },
    { key: 'PendingCount', label: 'Pending Count', type: 'number' },
    { key: 'SuccessRate', label: 'Success %' },
    { key: 'TotalAmt', label: 'Total Amt', type: 'money' },
    // Signed with a plain + / − rather than Cr / Dr. Each of these columns is two things at once —
    // commission the tier earned, or a surcharge it was charged — and the question asked of a
    // sales report is whether the service made money or cost money. "−₹2,592.74" says that; a
    // "Dr" suffix says it in a language the reader of this particular page is not thinking in.
    { key: 'RCommission', label: 'R Commission', type: 'plusMinusMoney' },
    { key: 'DCommission', label: 'D Commission', type: 'plusMinusMoney', roles: ADMIN_DIST },
    { key: 'SCommission', label: 'S Commission', type: 'plusMinusMoney', roles: ADMIN },
    { key: 'AdminCommission', label: 'Admin Comm', type: 'plusMinusMoney', roles: ADMIN },
    { key: 'NetProfit', label: 'Net Profit', type: 'plusMinusMoney', roles: ACCOUNTING },
  ],
};

/** Operator-wise sale — legacy OperatorSale.js (aggregate). */
export const SALE_OPERATOR: TxnReportConfig = {
  endpoint: 'TransactionReport/OperatorSale',
  filters: { service: true, user: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'UserName', label: 'User' },
    { key: 'ServiceName', label: 'Service' },
    { key: 'OperatorName', label: 'Operator', type: 'bold' },
    { key: 'SuccessCount', label: 'Success Count', type: 'number' },
    { key: 'SuccessAmount', label: 'Success Amount', type: 'money' },
    { key: 'ReatilerComm', label: 'Retailer Comm', type: 'money' },
    { key: 'RetailerCommPercentage', label: 'Retailer %' },
    { key: 'DistributerComm', label: 'Dist. Comm', type: 'money', roles: ADMIN_DIST },
    { key: 'DistributerCommPercentage', label: 'Dist. %', roles: ADMIN_DIST },
    { key: 'SuperDistributerComm', label: 'Super Comm', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerCommPercentage', label: 'Super %', roles: ADMIN },
  ],
};

/** User-wise sale — legacy UserWiseSale.js (aggregate). */
export const SALE_USERWISE: TxnReportConfig = {
  endpoint: 'TransactionReport/UserwiseSale',
  // Opening / Closing / Purchase are per WALLET, so the report has to say which one it is
  // reconciling — without the picker the balance columns were unattributable.
  filters: { user: true, wallet: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'FirmName', label: 'Name', type: 'bold' },
    { key: 'UserMobile', label: 'Mobile' },
    { key: 'Role', label: 'Role' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'OpeningBal', label: 'Opening Bal', type: 'money' },
    { key: 'ClosingBalance', label: 'Closing Bal', type: 'money' },
    { key: 'Sale', label: 'Sale', type: 'money' },
    { key: 'RetailerCommission', label: 'R. Commi', type: 'signedMoney' },
    { key: 'DistributerCommission', label: 'D. Commi', type: 'signedMoney', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'S. Commi', type: 'signedMoney', roles: ADMIN },
    { key: 'Surcharge', label: 'Surcharge', type: 'money' },
    { key: 'Purchase', label: 'Purchase', type: 'money' },
    { key: 'TransferAmount', label: 'Transfer', type: 'money', roles: ADMIN },
    { key: 'ReverseAmountDr', label: 'Reverse Dr', type: 'money' },
    { key: 'ReverseAmountCr', label: 'Reverse Cr', type: 'money', roles: ADMIN },
    { key: 'SuccessCount', label: 'S. Count', type: 'number' },
    { key: 'FailCount', label: 'F. Count', type: 'number' },
  ],
};

/** TDS report — legacy TDSReport.js (Tax/SelectTDS). */
export const TDS_REPORT: TxnReportConfig = {
  endpoint: 'Tax/SelectTDS',
  filters: { user: true, service: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date', type: 'date' },
    { key: 'FirmName', label: 'Firm Name', type: 'bold' },
    { key: 'ServiceName', label: 'Service' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'signedMoney' },
    { key: 'RetailerTDS', label: 'Retailer TDS', type: 'money' },
    { key: 'DistributerTDS', label: 'Dist TDS', type: 'money', roles: ADMIN_DIST },
    { key: 'SuperDistributerTDS', label: 'Super TDS', type: 'money', roles: ADMIN },
    { key: 'TotalTDS', label: 'Total TDS', type: 'money' },
  ],
};

/** Account statement / settlement — legacy SettlementReport.js (TransactionDetails/SelectForFilter). */
const STATEMENT_COLUMNS = [
  { key: '_serial', label: '#', type: 'serial' as const },
  { key: 'DOC', label: 'Date & Time', type: 'date' as const },
  // Whose line this is. The admin reads one user's statement at a time here, but the export
  // and the printed page leave the screen, and a statement with no name on it is unusable.
  { key: 'UserName', label: 'User Name', type: 'bold' as const },
  { key: 'MobileNumber', label: 'Mobile' },
  // Which wallet the money moved in. A statement without this is unreadable the moment a user
  // holds more than one wallet — a recharge debit and an AEPS credit look identical.
  { key: 'WalletName', label: 'Wallet' },
  { key: 'DOCType', label: 'Product Name' },
  { key: 'OperatorName', label: 'Service Name' },
  { key: 'ConsumerNumber', label: 'Number' },
  // Who the money went to or came from. The two columns this replaces (Entity Identifier and
  // Entity Name) both repeated the statement's OWN owner on every single row, which told the
  // reader nothing they did not already know from the page they were on.
  // Admin / support / accountant only. The counterparty on a downline row is the PROVIDER the
  // platform bought from — the upstream vendor's name and, read down a page, who the operator
  // buys each service from and in what mix. That is the platform's commercial position, not the
  // retailer's business, so it is off their statement and out of their export (the export runs
  // through the same role-filtered column list the table does).
  { key: 'Counterparty', label: 'Provider / Counterparty', roles: ACCOUNTING },
  { key: 'AmountCreditDebit', label: 'Type' },
  // Txn ID is now the OPERATOR's reference — the number a customer quotes — and Reference ID is
  // our own transaction id. They used to be the same value printed twice.
  { key: 'ReferenceNo', label: 'Operator Ref.', type: 'bold' as const },
  { key: 'ReferenceID', label: 'Txn ID', type: 'number' as const },
  { key: 'Amount', label: 'Transaction Amount', type: 'money' as const },
  { key: 'Charge', label: 'Charge', type: 'money' as const },
  { key: 'GST', label: 'GST', type: 'money' as const },
  { key: 'DiscountAmount', label: 'Commission', type: 'money' as const },
  { key: 'TDS', label: 'TDS', type: 'money' as const },
  { key: 'NetCommission', label: 'Net Commission', type: 'signedMoney' as const },
  { key: 'OpeningBalance', label: 'Opening Balance', type: 'money' as const },
  { key: 'ClosingBalance', label: 'Closing Balance', type: 'money' as const },
  { key: 'TransactionDescription', label: 'Status', type: 'status' as const },
];
export const STATEMENT_REPORT: TxnReportConfig = {
  endpoint: 'TransactionDetails/SelectForFilter',
  // A wallet picker, because a statement for someone holding Recharge, DMT and AEPS wallets at
  // once is three statements interleaved, and reconciling one of them means reading past the
  // other two. Blank = every wallet, which is what the screen opens on.
  filters: { user: true, wallet: true, criteria: true, amountType: true },
  defaultFromToday: true,
  columns: STATEMENT_COLUMNS,
};
// Admin Ledger shows the company's own account statement — the detailed line-by-line company
// ledger. No per-user drill-down (inherently admin-scoped), so no user/entity dropdown.
// (Formerly the "Admin Accounting" screen; Admin Accounting is now the reconciliation summary.)
export const ADMIN_STATEMENT: TxnReportConfig = {
  endpoint: 'TransactionDetails/SelectAdminAccounting',
  filters: { wallet: true, criteria: true, amountType: true },
  defaultFromToday: true,
  columns: STATEMENT_COLUMNS,
};

/** Ledger — legacy Ledger.js (Ledger/Select). Detail-row view. */
const LEDGER_COLUMNS = [
  { key: '_serial', label: '#', type: 'serial' as const },
  { key: 'DOC', label: 'Date', type: 'date' as const },
  { key: 'FirmName', label: 'User' },
  { key: 'WalletName', label: 'Wallet' },
  { key: 'Description', label: 'Description' },
  { key: 'OpeningBalance', label: 'Op. Bal', type: 'money' as const },
  { key: 'Amount', label: 'Amount', type: 'money' as const },
  { key: 'ConsumerNumber', label: 'Consumer Number' },
  { key: 'ClosingBalance', label: 'Cl. Bal', type: 'money' as const },
];
export const USER_LEDGER: TxnReportConfig = {
  endpoint: 'Ledger/Select',
  filters: { user: true },
  columns: LEDGER_COLUMNS,
};
/**
 * The PLATFORM's own fund ledger — the company account's credits and debits.
 *
 * No Entity picker, because there is no entity to pick: this report is the admin account itself,
 * and the endpoint reads the caller's own ledger by definition. The picker used to be here and
 * did nothing at all, which reads as a broken filter rather than as a filter that cannot apply.
 * One user's ledger is the Ledger screen above (`Ledger/Select`), which does take a user.
 */
export const ADMIN_LEDGER: TxnReportConfig = {
  endpoint: 'Ledger/AdminLedger',
  columns: LEDGER_COLUMNS,
};

/** Past success/fail — legacy PastSuccessFailReport.js (SelectPastSuccessFail). */
export const PAST_SUCCESS_FAIL: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectPastSuccessFail',
  filters: { status: 'simple', user: true, criteria: true },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'FirmName', label: 'Name' },
    { key: 'ProviderName', label: 'Provider' },
    { key: 'OperatorName', label: 'Operator' },
    { key: 'ConsumerNumber', label: 'Number', type: 'bold' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'DebitAmount', label: 'Debit Amount', type: 'money' },
    { key: 'DOC', label: 'Transaction Date', type: 'date' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'UpdatedDOC', label: 'Updated Date', type: 'date' },
    { key: 'ProcTime', label: 'ProcTime (s)' },
  ],
};

/** Manual Update — transactions that were manually settled (TransactionReport/ManualSuccessFail).
 *  Serial # instead of raw ids; the backend enriches user/operator/provider names. */
export const MANUAL_UPDATE: TxnReportConfig = {
  endpoint: 'TransactionReport/ManualSuccessFail',
  filters: { user: true },
  actions: ['Log'],
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'User Name', type: 'bold' },
    { key: 'OperatorName', label: 'Operator' },
    { key: 'ProviderName', label: 'Provider', roles: ADMIN },
    { key: 'ConsumerNumber', label: 'Number', type: 'bold' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'DebitAmount', label: 'Debit', type: 'money' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Operator Ref. ID' },
    { key: 'Remark', label: 'Remark' },
  ],
};

/** Range-wise recharge — legacy RangeWiseRecharge.js (aggregate buckets). */
export const RANGEWISE_RECHARGE: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectRangewiseRecharge',
  filters: { service: true, user: true },
  columns: [
    { key: 'Range', label: 'Range', type: 'bold' },
    { key: 'Count', label: 'Total Count', type: 'number' },
    { key: 'Sum', label: 'Total Sum', type: 'money' },
    { key: 'SuccessCount', label: 'Success Count', type: 'number' },
    { key: 'SuccessSum', label: 'Success Sum', type: 'money' },
    { key: 'FailCount', label: 'Fail Count', type: 'number' },
    { key: 'FailSum', label: 'Fail Sum', type: 'money' },
    { key: 'ProcessCount', label: 'Process Count', type: 'number' },
    { key: 'ProcessSum', label: 'Process Sum', type: 'money' },
  ],
};

/** Pending Recharge — from legacy PendingRecharge.js. */
export const PENDING_RECHARGE: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectPendingTransactionWithFilter',
  filters: { service: true, operator: true, user: true },
  actions: ['Success / Refund', 'Status Check', 'Without Status Check', 'Log'],
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date', type: 'date' },
    { key: 'FirmName', label: 'Name' },
    { key: 'ConsumerNumber', label: 'Number', type: 'bold' },
    { key: 'OperatorName', label: 'Operator' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'ProviderName', label: 'Provider', roles: ADMIN },
    { key: 'ProviderRequestID', label: 'Provider Request ID', roles: ADMIN },
    { key: 'AdditionalParam1', label: 'Additional Param', roles: ADMIN },
  ],
};

/** DMT report — from legacy DMTReport.js (endpoint SelectForDMTFilter). */
export const DMT_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectForDMTFilter',
  filters: { status: 'dmt', user: true, complaint: true, criteria: true },
  defaultFromToday: true,
  // "Refund" dropped. It was a label with nothing behind it: the only wired way to reverse a
  // transfer is Status Check (which settles from the provider's own answer) or an admin's manual
  // settle, and a Refund button that silently does nothing on a money-transfer report is worse
  // than no button — a retailer presses it and believes the customer has been refunded.
  actions: ['Print', 'Complaint', 'Status Check', 'Log'],
  statusCheckEndpoint: 'TransactionProcessing/GetDMTTransctionStatus',
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Txn Date', type: 'date' },
    { key: 'DMT_SenderName', label: 'Sender Name' },
    { key: 'DMT_SenderNumber', label: 'Sender Mobile' },
    { key: 'ConsumerNumber', label: 'Account Number', type: 'bold' },
    { key: 'DMT_Bank', label: 'Bank Name' },
    { key: 'DMT_IFSC', label: 'IFSC' },
    { key: 'DMT_BeneficiaryName', label: 'Beneficiary Name' },
    // "Bene Name by Bank" dropped: the gateways this install uses never return a name-at-bank,
    // so the column was blank on every row and only pushed the figures off the screen.
    { key: 'DMT_PaymentType', label: 'Payment Mode' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'Charge', label: 'Charge', type: 'money' },
    { key: 'RetailerGST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'RetailerCommission', label: 'Commission', type: 'signedMoney' },
    { key: 'RetailerTDS', label: 'TDS', type: 'money', roles: ADMIN },
    { key: 'NetCommission', label: 'Net Commission', type: 'signedMoney' },
    { key: 'CommissionInfo', label: 'MDS/DS Commission', roles: ADMIN_DIST },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    // The bank/operator's own reference first — it is what a customer and a bank dispute are
    // settled with — then ours.
    { key: 'ProviderRefNo', label: 'Operator Ref', type: 'bold' },
    { key: 'ReferenceNo', label: 'Txn Reference ID' },
    { key: 'ComplaintStatus', label: 'Complaint', type: 'status' },
    { key: 'ProviderRequestID', label: 'Request ID', roles: ADMIN_API },
  ],
};

/** BBPS report — from legacy BBPSReport.js (endpoint SelectForBBPS). */
export const BBPS_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectForBBPS',
  filters: { service: true, operator: true, status: 'recharge', user: true, complaint: true, criteria: true },
  defaultFromToday: true,
  actions: ['Print', 'Complaint', 'Log'],
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'UserMobile', label: 'Mobile Number' },
    { key: 'OperatorName', label: 'Biller Name' },
    // Biller logo dropped — see the recharge report.
    { key: 'BBPS_CustomerName', label: 'Customer Name' },
    { key: 'ConsumerNumber', label: 'Number', type: 'bold' },
    { key: 'Status', label: 'Status', type: 'status' },
    // The biller's own reference, not ours. Ours is further along, as "Txn Reference ID".
    { key: 'ProviderRefNo', label: 'Operator Ref', type: 'bold' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'signedMoney' },
    { key: 'RetailerTDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'signedMoney' },
    { key: 'CommissionInfo', label: 'MDS/DS Commission', roles: ADMIN_DIST },
    { key: 'DebitAmount', label: 'Debit', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'ProviderClosingBalance', label: 'API Cl Bal', type: 'money', roles: ADMIN },
    { key: 'AdminCommission', label: 'API Set Comm', type: 'signedMoney', roles: ADMIN },
    { key: 'Circle', label: 'Circle' },
    { key: 'APIUserRequestID', label: 'API Req', roles: ADMIN_API },
    { key: 'ProcTime', label: 'ProcTime (s)' },
    { key: 'IPAddress', label: 'IP' },
    { key: 'ReferenceNo', label: 'Txn Reference ID' },
    { key: 'ProviderRequestID', label: 'Request ID', roles: ADMIN_API },
    { key: 'ComplaintStatus', label: 'Complaint', type: 'status' },
  ],
};

/** Payout report — from legacy PayoutReport.js (endpoint Payout). */
export const PAYOUT_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/Payout',
  filters: { status: 'recharge', user: true, criteria: true },
  defaultFromToday: true,
  actions: ['Print', 'Status Check', 'Log'],
  statusCheckEndpoint: 'TransactionProcessing/GetPayoutTransactionStatus',
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'UserID', label: 'Entity ID', type: 'number' },
    { key: 'ConsumerNumber', label: 'A/C Number', type: 'bold' },
    // A payout is written by the same pipeline as a transfer (BankTransferService), so the
    // beneficiary's details live in the DMT_* columns it fills.
    { key: 'DMT_IFSC', label: 'IFSC' },
    { key: 'DMT_BeneficiaryName', label: 'A/C Holder Name' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'Charge', label: 'Charge', type: 'money', roles: ADMIN },
    { key: 'RetailerGST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'DebitAmount', label: 'Total Amount', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Transaction ID' },
    { key: 'Description', label: 'Remark' },
    { key: 'DMT_PaymentType', label: 'Payment Mode' },
    { key: 'ProviderRequestID', label: 'Request ID', roles: ADMIN_API },
  ],
};

/** MicroATM report — from legacy MicroATM/Report.js (endpoint MicroATM). */
export const MICROATM_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/MicroATM',
  filters: { status: 'recharge', user: true, criteria: true },
  defaultFromToday: true,
  actions: ['Print', 'Status Check', 'Log'],
  statusCheckEndpoint: 'TransactionProcessing/GetMicroATMTransactionStatus',
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date', type: 'date' },
    { key: 'FirmName', label: 'Name' },
    { key: 'OperatorName', label: 'Operator' },
    // Operator logo dropped — see the recharge report.
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'signedMoney' },
    { key: 'RetailerTDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'signedMoney' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ConsumerNumber', label: 'Card Number' },
    { key: 'ReferenceNo', label: 'Op ID' },
    { key: 'ProviderName', label: 'Provider' },
    { key: 'Remark', label: 'Remark' },
    { key: 'Through', label: 'Through' },
    { key: 'IPAddress', label: 'IP' },
    { key: 'RetailerGST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'DistributerGST', label: 'Dist GST', type: 'money', roles: ADMIN },
    { key: 'DistributerTDS', label: 'Dist TDS', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerGST', label: 'Super GST', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerTDS', label: 'Super TDS', type: 'money', roles: ADMIN },
    { key: 'ProviderRequestID', label: 'Request ID' },
  ],
};

/** AEPS report — from legacy AEPS/Report.js (endpoint AEPS). */
export const AEPS_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/AEPS',
  filters: { status: 'recharge', user: true, criteria: true },
  defaultFromToday: true,
  actions: ['Print', 'Log'],
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'UserMobile', label: 'Mobile Number' },
    { key: 'OperatorName', label: 'Transaction Type' },
    { key: 'ConsumerNumber', label: 'Aadhaar Number' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'signedMoney' },
    { key: 'RetailerTDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'signedMoney' },
    { key: 'DebitAmount', label: 'Total Amount', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Operator Ref. ID' },
    // AEPS stores the customer's bank in the shared DMT_Bank column (see AEPSService).
    { key: 'DMT_Bank', label: 'Bank Name' },
    { key: 'ProviderRequestID', label: 'Transaction Ref. ID' },
    { key: 'RetailerGST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'DistributerGST', label: 'Dist GST', type: 'money', roles: ADMIN },
    { key: 'DistributerTDS', label: 'Dist TDS', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerGST', label: 'Super GST', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerTDS', label: 'Super TDS', type: 'money', roles: ADMIN },
  ],
};

/** Sender / Beneficiary — legacy SenderBeneficiary/SenderBeneficiaryReport.js (sender list).
 *  Endpoint SenderBeneficiary/SelectSender; columns mirror the legacy sender table. */
export const SENDER_BENEFICIARY: TxnReportConfig = {
  endpoint: 'SenderBeneficiary/SelectSender',
  filters: {
    user: true,
    search: { label: 'Search', placeholder: 'Sender name / mobile / pincode' },
  },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Registered', type: 'date' },
    { key: 'Name', label: 'Sender Name', type: 'bold' },
    { key: 'Mobile', label: 'Mobile' },
    { key: 'BeneficiaryCount', label: 'Beneficiaries', type: 'number' },
    // Whose retailer counter the sender walks into — the report is useless to an admin without it.
    { key: 'UserName', label: 'Registered By', roles: ADMIN_DIST },
    { key: 'UserMobile', label: 'Entity Mobile', roles: ADMIN_DIST },
    { key: 'Address', label: 'Address' },
    { key: 'Pincode', label: 'Pincode' },
    { key: 'IsKYC', label: 'KYC' },
    { key: 'IsActive', label: 'Active' },
  ],
};

/**
 * Hold Report — every balance freeze and release: whose wallet, how much, who did it and why.
 *
 * A hold takes money a user can see but cannot spend. Before this report there was no record of
 * one at all — the remark typed into the hold dialog was discarded — so a frozen balance could not
 * be explained or confidently released. Each row also carries the hold total left on that wallet
 * afterwards, which is the figure to check before releasing.
 */
export const HOLD_REPORT: TxnReportConfig = {
  endpoint: 'Users/HoldHistory',
  filters: { user: true, wallet: true, status: 'hold' },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'UserName', label: 'User', type: 'bold' },
    { key: 'UserMobile', label: 'Mobile' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'Action', label: 'Action', type: 'status' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'HoldBalanceAfter', label: 'Held After', type: 'money' },
    { key: 'WalletBalance', label: 'Wallet Bal', type: 'money' },
    { key: 'ActionByName', label: 'Done By', roles: ADMIN },
    // The reason. The column this whole report exists for.
    { key: 'Remark', label: 'Reason' },
  ],
};

/** Service charge deduction — legacy ServiceChargeDeduction/Report.js (ServiceChargeDeduction/Select). */
export const SERVICE_CHARGE_DEDUCTION: TxnReportConfig = {
  endpoint: 'ServiceChargeDeduction/Select',
  filters: { user: true },
  columns: [
    { key: 'FirmName', label: 'Firm Name', type: 'bold' },
    { key: 'Date', label: 'Date', type: 'date' },
    { key: 'Charge', label: 'Charge', type: 'money' },
    { key: 'Remark', label: 'Remark' },
  ],
};

/**
 * Commission structure — "what does the slab assigned to me actually pay?".
 * SlabDetail/CommissionStructure resolves the caller's assigned slab (admins and distributors
 * may look up a downline user instead), then returns one row per service / operator / amount
 * band. StrMyCommission is the caller's OWN tier rate — the margin they earn — while the
 * per-tier columns beside it are only shown to the roles allowed to see them.
 */
export const COMMISSION_STRUCTURE: TxnReportConfig = {
  endpoint: 'SlabDetail/CommissionStructure',
  // A slab structure is what the commission IS right now, not a range of history — the date
  // inputs did nothing here. What is left is the service (everyone) and, for an admin or an
  // upline, the entity whose slab they want to look up. The user picker is already role-gated
  // by the screen, so a retailer is left with exactly the service filter.
  filters: { user: true, service: true, hideDates: true },
  defaultFirstService: true,
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'ServiceName', label: 'Service', type: 'bold' },
    { key: 'OperatorName', label: 'Operator' },
    { key: 'Details', label: 'Amount band' },
    { key: 'SlabName', label: 'Slab' },
    { key: 'StrMyCommission', label: 'My margin', type: 'bold' },
    // One tier per column: you see the tiers BELOW you, never the one above, and never a second
    // copy of your own (that is "My margin"). So a retailer is left with My margin alone, a
    // distributor adds Retailer, a super distributor adds Distributor, and admin/support see the
    // whole split. "Super Dist" used to be ADMIN_DIST — which showed a distributor exactly what
    // their own super distributor earns on the same transaction — and "Retailer" had no gate at
    // all. The API masks the same fields per role (ReportAnalyticsService.CanSeeTier), so these
    // gates only keep a permanently empty column off the screen.
    { key: 'StrRetailerComm', label: 'Retailer', roles: ADMIN_DIST },
    { key: 'StrDistributorComm', label: 'Distributor', roles: [...ADMIN, 2] },
    { key: 'StrSuperDistributorComm', label: 'Super Dist', roles: ADMIN },
    { key: 'StrClientComm', label: 'API / Client', roles: ADMIN },
  ],
};

/**
 * Fund Report — every fund movement the caller is part of (transfers down, requests raised,
 * reversals), with both sides' opening/closing balances. Fund/TransferReport is the working
 * endpoint; the TransactionReport/Fund path this screen used to call never existed in this API.
 */
export const FUND_REPORT: TxnReportConfig = {
  endpoint: 'Fund/TransferReport',
  filters: {
    user: true,
    status: 'simple',
    search: { label: 'Search', placeholder: 'Sender / receiver / UTR / remark' },
  },
  defaultFromToday: true,
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'TransactionDOC', label: 'Date & Time', type: 'date' },
    { key: 'SenderName', label: 'From', type: 'bold' },
    { key: 'ReceiverName', label: 'To', type: 'bold' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'Type', label: 'Type' },
    { key: 'ModeName', label: 'Mode' },
    // The mode says HOW the money came (bank, cash, UPI); the sub-mode says WHICH one — the
    // actual bank account or UPI handle. Reconciling a deposit needs both.
    { key: 'SubModeName', label: 'Sub Mode' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'UTRNumber', label: 'UTR / Ref' },
    { key: 'SenderOpeningBalance', label: 'S. Op Bal', type: 'money', roles: ADMIN_DIST },
    { key: 'SenderClosingBalance', label: 'S. Cl Bal', type: 'money', roles: ADMIN_DIST },
    { key: 'ReceiverOpeningBalance', label: 'R. Op Bal', type: 'money' },
    { key: 'ReceiverClosingBalance', label: 'R. Cl Bal', type: 'money' },
    { key: 'Remark', label: 'Remark' },
    { key: 'AdminRemark', label: 'Admin Remark', roles: ADMIN },
  ],
};

/** Payout beneficiaries registered across the caller's network (PayoutBeneficiary/GetAll). */
export const PAYOUT_BENEFICIARY: TxnReportConfig = {
  endpoint: 'PayoutBeneficiary/GetAll',
  filters: { user: true },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'BeneficiaryName', label: 'Beneficiary', type: 'bold' },
    { key: 'AccountNumber', label: 'Account No.' },
    { key: 'IFSC', label: 'IFSC' },
    { key: 'BankName', label: 'Bank' },
    { key: 'MobileNumber', label: 'Mobile' },
    { key: 'IsActive', label: 'Active' },
  ],
};

/**
 * Fund Reverse Report — money actually pulled back OUT of a downline wallet.
 *
 * Distinct from the Fund Report (every movement) and from Reverse Requests (the queue of
 * requests awaiting a decision). Fund/ReverseReport answers with both the requests and the
 * completed reversals, so Status separates them: PROCESS is a request still waiting, SUCCESS is
 * money that moved. Opening/closing balances are shown because a reversal is the movement a user
 * is most likely to query, and "what was my balance before and after" is the first question.
 */
export const FUND_REVERSE_REPORT: TxnReportConfig = {
  endpoint: 'Fund/ReverseReport',
  filters: {
    user: true,
    wallet: true,
    status: 'fund',
    search: { label: 'Search', placeholder: 'Sender / receiver / UTR / reason' },
  },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'TransactionDOC', label: 'Date & Time', type: 'date' },
    // Both kinds of row are in this report, so each one says which it is. Without it a
    // request and a completed reversal are two identical-looking lines with the same status.
    { key: 'ReverseKind', label: 'Kind' },
    { key: 'SenderName', label: 'Reversed from', type: 'bold' },
    { key: 'ReceiverName', label: 'Reversed to', type: 'bold' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'SenderOpeningBalance', label: 'Op Bal (from)', type: 'money', roles: ADMIN_DIST },
    { key: 'SenderClosingBalance', label: 'Cl Bal (from)', type: 'money', roles: ADMIN_DIST },
    { key: 'ReceiverClosingBalance', label: 'Cl Bal (to)', type: 'money', roles: ADMIN_DIST },
    { key: 'Remark', label: 'Reason' },
    { key: 'AdminRemark', label: 'Admin Remark', roles: ADMIN },
  ],
};

/**
 * Reverse Requests — the operator's QUEUE, not a ledger.
 *
 * Same endpoint as the Fund Reverse Report, read differently on purpose:
 *
 *  • `Type: REVERSE_REQUEST` is pinned, because Fund/ReverseReport returns completed reversals
 *    too. The queue screen used to send no Type at all, so direct reverses that nobody has to
 *    decide on sat in the middle of the list of things waiting to be decided.
 *  • It opens on Pending for the same reason — a queue's default view is the work in it. The
 *    Status dropdown still reaches the approved and rejected ones.
 *
 * Balances are left out: they belong to money that has moved, and nothing has moved yet.
 */
export const FUND_REVERSE_REQUESTS: TxnReportConfig = {
  endpoint: 'Fund/ReverseReport',
  fixedPayload: { Type: 'REVERSE_REQUEST' },
  defaultStatus: 'PROCESS',
  filters: {
    user: true,
    wallet: true,
    status: 'fund',
    search: { label: 'Search', placeholder: 'Requested from / by / reason' },
  },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'TransactionDOC', label: 'Requested on', type: 'date' },
    { key: 'SenderName', label: 'Reverse from', type: 'bold' },
    { key: 'ReceiverName', label: 'Requested by', type: 'bold' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'Remark', label: 'Reason' },
    { key: 'AdminRemark', label: 'Admin Remark', roles: ADMIN },
  ],
};

/**
 * User Change History — who changed what on a user record, when, why, and from which IP.
 *
 * One row per changed FIELD, not per save, so "what actually moved" is readable without
 * diffing two snapshots. There is no Action dropdown: the free-text box already matches the
 * action, the field, both values, the reason and the IP, which covers "show me every
 * deactivation" and "who touched this mobile number" with one control instead of four.
 */
export const USER_CHANGE_HISTORY: TxnReportConfig = {
  endpoint: 'Users/ChangeHistory',
  filters: {
    user: true,
    search: { label: 'Search', placeholder: 'Action / field / value / reason / IP' },
  },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'When', type: 'date' },
    { key: 'UserName', label: 'User', type: 'bold' },
    { key: 'UserCode', label: 'Code' },
    { key: 'UserType', label: 'Role' },
    { key: 'Action', label: 'Action', type: 'status' },
    { key: 'FieldName', label: 'Field' },
    { key: 'OldValue', label: 'From' },
    { key: 'NewValue', label: 'To' },
    { key: 'Reason', label: 'Reason' },
    { key: 'ChangedByName', label: 'Changed by', type: 'bold' },
    { key: 'ChangedByType', label: 'By role' },
    { key: 'IPAddress', label: 'IP' },
    { key: 'Platform', label: 'Channel' },
  ],
};

/**
 * Profit & Loss — the accounting desk's one page.
 *
 * Opening, purchase, sale, closing, each commission tier, the GST on both sides, the margin
 * between them and the net profit, in the order a reader works across them. The figures were
 * spread over four reports that did not agree and had to be reconciled by hand; the endpoint
 * computes them together, from the same per-transaction GST stamps the daybook reads.
 *
 * `signedMoney` on every commission column is deliberate: in this system a positive figure is
 * commission PAID OUT and a negative one is a surcharge COLLECTED, so the sign is the information
 * and a bare minus would read as a negative balance. The GST columns are plain money, because a
 * tax amount has no direction — the column name carries it.
 *
 * Grouping is a filter rather than four screens: the same line items read down the calendar, per
 * service, per operator or per user answer four different questions about the same money.
 */
export const PROFIT_LOSS: TxnReportConfig = {
  endpoint: 'DayBookReport/ProfitLoss',
  filters: {
    user: true,
    service: true,
    operator: true,
    groupBy: {
      label: 'Group by',
      options: [
        { label: 'Date', value: '' },
        { label: 'Service', value: 'Service' },
        { label: 'Operator', value: 'Operator' },
        { label: 'User', value: 'User' },
      ],
      default: '',
    },
  },
  columns: [
    { key: 'ID', label: '#', type: 'serial' },
    { key: 'Group', label: 'Period / Group', type: 'bold' },
    // Balances bound the period rather than accumulating over it: the total opening is the first
    // row's and the total closing is the last row's. Summed, they report the float once per row.
    { key: 'Opening', label: 'Opening', type: 'money', totalMode: 'first' },
    { key: 'Purchase', label: 'Purchase', type: 'money' },
    { key: 'Sale', label: 'Sale', type: 'money' },
    { key: 'Closing', label: 'Closing', type: 'money', totalMode: 'last' },
    { key: 'RetailerCommission', label: 'R Comm / Charge', type: 'signedMoney' },
    { key: 'DistributorCommission', label: 'D Comm / Charge', type: 'signedMoney' },
    { key: 'SuperDistributorCommission', label: 'SD Comm / Charge', type: 'signedMoney' },
    { key: 'TotalCommission', label: 'Total Comm / Charge', type: 'signedMoney' },
    { key: 'TotalGST', label: 'Total GST', type: 'money' },
    { key: 'ProviderCommission', label: 'Provider Comm / Charge', type: 'signedMoney' },
    { key: 'ProviderGST', label: 'Provider GST', type: 'money' },
    { key: 'DiffCommission', label: 'Diff Comm / Charge', type: 'signedMoney' },
    { key: 'DiffGST', label: 'Diff GST', type: 'signedMoney' },
    { key: 'NetProfit', label: 'Net Profit', type: 'signedMoney' },
  ],
};
