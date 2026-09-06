import type { TxnReportConfig } from './txnReportTypes';

// UserTypeID role codes (mirror StaticModels/UserTypeCode): Admin=1, SuperDist=2, Dist=3, Retailer=4, ApiUser=5, Support=6
const ADMIN = [1, 6];
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
  actions: ['Print', 'Complaint', 'Wrong 2 Right', 'Success / Fail', 'Log', 'Hit Callback'],
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'UserMobile', label: 'Entity Mobile' },
    { key: 'OperatorName', label: 'Operator' },
    { key: 'Logo', label: 'Logo', type: 'image' },
    { key: 'ConsumerNumber', label: 'Mobile Number', type: 'bold' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Operator ID', type: 'bold' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'money' },
    { key: 'TDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'money' },
    { key: 'CommissionInfo', label: 'MDS/DS Commission', roles: ADMIN_DIST },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'ComplaintStatus', label: 'Complaint', type: 'status' },
    { key: 'Circle', label: 'Circle' },
    { key: 'APIUserRequestID', label: 'Request ID', roles: ADMIN_API },
    { key: 'ProcTime', label: 'ProcTime (s)' },
    { key: 'GST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'DistributerGST', label: 'Dist GST', type: 'money', roles: ADMIN },
    { key: 'DistributerTDS', label: 'Dist TDS', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerGST', label: 'Super GST', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerTDS', label: 'Super TDS', type: 'money', roles: ADMIN },
    { key: 'ProviderRefNo', label: 'Transaction Ref. Id' },
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
  actions: ['Log'],
  columns: [
    // Serial (1,2,3…) instead of the raw TransactionMasterID; the id stays on the row for the
    // Log / Manual-settle actions but is not shown as a column.
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'User Name', type: 'bold' },
    { key: 'ConsumerNumber', label: 'Number' },
    { key: 'OperatorName', label: 'Operator Name' },
    { key: 'WalletName', label: 'Wallet Name' },
    { key: 'ProviderName', label: 'Provider Name', roles: ADMIN },
    { key: 'DistributerName', label: 'Distributor Name', roles: ADMIN_DIST },
    { key: 'SuperDistributerName', label: 'Super-Distributor Name', roles: ADMIN_DIST },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'RetailerCommission', label: 'Comm', type: 'money' },
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
    { key: 'TotalCommission', label: 'Commission', type: 'money' },
  ],
};

/** Downline — legacy Users/DownlineReport.js (Users/SelectDownlineReport). */
export const DOWNLINE_REPORT: TxnReportConfig = {
  endpoint: 'Users/SelectDownlineReport',
  filters: { user: true },
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
  filters: { user: true },
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
  filters: { user: true },
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
    { key: 'AdminCommission', label: 'Admin Comm', type: 'money', roles: ADMIN },
    { key: 'ProviderCommission', label: 'API Comm', type: 'money', roles: ADMIN },
    { key: 'RetailerCommission', label: 'R. Comm', type: 'money' },
    { key: 'DistributerCommission', label: 'D. Comm', type: 'money', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'S. Comm', type: 'money', roles: ADMIN },
    { key: 'Percentage', label: 'Per (%)' },
    { key: 'Profit', label: 'Profit', type: 'money', roles: ADMIN },
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
    { key: 'RCommission', label: 'R Commission', type: 'money' },
    { key: 'DCommission', label: 'D Commission', type: 'money', roles: ADMIN_DIST },
    { key: 'SCommission', label: 'S Commission', type: 'money', roles: ADMIN },
    { key: 'AdminCommission', label: 'Admin Commission', type: 'money', roles: ADMIN },
    { key: 'NetProfit', label: 'Net Profit', type: 'money', roles: ADMIN },
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

/** Transfer-to-eBalance / move funds — legacy MoveFundsReport.js (Fund/SelectWalletTransferReport). */
export const MOVE_FUNDS: TxnReportConfig = {
  endpoint: 'Fund/SelectWalletTransferReport',
  filters: { user: true },
  columns: [
    { key: 'FundTransactionID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'Sender', label: 'Entity Name' },
    { key: 'Wallet', label: 'Wallet' },
    { key: 'OriginalAmount', label: 'Original Amt', type: 'money' },
    { key: 'Amount', label: 'Transfer Amt', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'SenderRemark', label: 'S. Remark' },
    { key: 'Remark', label: 'R. Remark' },
    { key: 'SenderOpeningBalance', label: 'S. Op Bal', type: 'money', roles: ADMIN },
    { key: 'SenderClosingBalance', label: 'S. Cl Bal', type: 'money', roles: ADMIN },
    { key: 'ReceiverOpeningBalance', label: 'R. Op Bal', type: 'money' },
    { key: 'ReceiverClosingBalance', label: 'R. Cl Bal', type: 'money' },
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
    { key: 'FailedCount', label: 'Failed', type: 'number' },
    { key: 'FailedAmount', label: 'Fail Amt', type: 'money' },
    { key: 'PendingCount', label: 'Pending', type: 'number' },
    { key: 'PendingAmount', label: 'Pending Amt', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'money' },
    { key: 'DistributerCommission', label: 'D. Comm', type: 'money', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'SD. Comm', type: 'money', roles: ADMIN },
    { key: 'Surcharge', label: 'Surcharge', type: 'money' },
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
    { key: 'RetailerCommission', label: 'R. Comm', type: 'money' },
    { key: 'DistributerCommission', label: 'D. Comm', type: 'money', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'S. Comm', type: 'money', roles: ADMIN },
    { key: 'OpeningBalance', label: 'Op. Bal', type: 'money' },
    { key: 'AdminCommission', label: 'Admin Comm', type: 'money', roles: ADMIN },
    { key: 'ClosingBalance', label: 'Cl. Bal', type: 'money' },
    { key: 'Difference', label: 'Diff', type: 'money' },
    { key: 'Profit', label: 'Profit', type: 'money', roles: ADMIN },
  ],
};

/** Service-wise sale — legacy ServicewiseSale.js (aggregate). */
export const SALE_SERVICEWISE: TxnReportConfig = {
  endpoint: 'TransactionReport/ServicewiseSale',
  filters: { service: true, user: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'ServiceName', label: 'Service', type: 'bold' },
    { key: 'SuccessAmt', label: 'Success Amt', type: 'money' },
    { key: 'SuccessCount', label: 'Success Count', type: 'number' },
    { key: 'FailAmt', label: 'Fail Amt', type: 'money' },
    { key: 'FailCount', label: 'Fail Count', type: 'number' },
    { key: 'PendingAmt', label: 'Pending Amt', type: 'money' },
    { key: 'PendingCount', label: 'Pending Count', type: 'number' },
    { key: 'SuccessRate', label: 'Success %' },
    { key: 'TotalAmt', label: 'Total Amt', type: 'money' },
    { key: 'RCommission', label: 'R Commission', type: 'money' },
    { key: 'DCommission', label: 'D Commission', type: 'money', roles: ADMIN_DIST },
    { key: 'SCommission', label: 'S Commission', type: 'money', roles: ADMIN },
    { key: 'AdminCommission', label: 'Admin Comm', type: 'money', roles: ADMIN },
    { key: 'NetProfit', label: 'Net Profit', type: 'money', roles: ADMIN },
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
  filters: { user: true },
  columns: [
    { key: 'ID', label: '#', type: 'number' },
    { key: 'FirmName', label: 'Name', type: 'bold' },
    { key: 'UserMobile', label: 'Mobile' },
    { key: 'Role', label: 'Role' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'OpeningBal', label: 'Opening Bal', type: 'money' },
    { key: 'ClosingBalance', label: 'Closing Bal', type: 'money' },
    { key: 'Sale', label: 'Sale', type: 'money' },
    { key: 'RetailerCommission', label: 'R. Commi', type: 'money' },
    { key: 'DistributerCommission', label: 'D. Commi', type: 'money', roles: ADMIN_DIST },
    { key: 'SuperDistributerCommission', label: 'S. Commi', type: 'money', roles: ADMIN },
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
    { key: 'RetailerCommission', label: 'Commission', type: 'money' },
    { key: 'RetailerTDS', label: 'Retailer TDS', type: 'money' },
    { key: 'DistributerTDS', label: 'Dist TDS', type: 'money', roles: ADMIN_DIST },
    { key: 'SuperDistributerTDS', label: 'Super TDS', type: 'money', roles: ADMIN },
    { key: 'TotalTDS', label: 'Total TDS', type: 'money' },
  ],
};

/** Account statement / settlement — legacy SettlementReport.js (TransactionDetails/SelectForFilter). */
const STATEMENT_COLUMNS = [
  { key: 'TransactionDetailID', label: '#', type: 'number' as const },
  { key: 'DOC', label: 'Date & Time', type: 'date' as const },
  { key: 'AliasName', label: 'Entity Identifier' },
  { key: 'PersonName', label: 'Entity Name' },
  { key: 'DOCType', label: 'Product Name' },
  { key: 'OperatorName', label: 'Service Name' },
  { key: 'AmountCreditDebit', label: 'Type' },
  { key: 'ReferenceNo', label: 'Txn ID', type: 'bold' as const },
  { key: 'ReferenceID', label: 'Reference ID' },
  { key: 'Amount', label: 'Transaction Amount', type: 'money' as const },
  { key: 'Charge', label: 'Charge', type: 'money' as const },
  { key: 'GST', label: 'GST', type: 'money' as const },
  { key: 'DiscountAmount', label: 'Commission', type: 'money' as const },
  { key: 'TDS', label: 'TDS', type: 'money' as const },
  { key: 'NetCommission', label: 'Net Commission', type: 'money' as const },
  { key: 'OpeningBalance', label: 'Opening Balance', type: 'money' as const },
  { key: 'ClosingBalance', label: 'Closing Balance', type: 'money' as const },
  { key: 'TransactionDescription', label: 'Status', type: 'status' as const },
];
export const STATEMENT_REPORT: TxnReportConfig = {
  endpoint: 'TransactionDetails/SelectForFilter',
  filters: { user: true, criteria: true, amountType: true },
  columns: STATEMENT_COLUMNS,
};
// Admin Ledger shows the company's own account statement — the detailed line-by-line company
// ledger. No per-user drill-down (inherently admin-scoped), so no user/entity dropdown.
// (Formerly the "Admin Accounting" screen; Admin Accounting is now the reconciliation summary.)
export const ADMIN_STATEMENT: TxnReportConfig = {
  endpoint: 'TransactionDetails/SelectAdminAccounting',
  filters: { criteria: true, amountType: true },
  columns: STATEMENT_COLUMNS,
};

/** Ledger — legacy Ledger.js (Ledger/Select). Detail-row view. */
const LEDGER_COLUMNS = [
  { key: 'TransactionDetailID', label: '#', type: 'number' as const },
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
export const ADMIN_LEDGER: TxnReportConfig = {
  endpoint: 'Ledger/AdminLedger',
  filters: { user: true },
  columns: LEDGER_COLUMNS,
};

/** Past success/fail — legacy PastSuccessFailReport.js (SelectPastSuccessFail). */
export const PAST_SUCCESS_FAIL: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectPastSuccessFail',
  filters: { status: 'simple', user: true, criteria: true },
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
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
    { key: 'TransactionMasterID', label: '#', type: 'number' },
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

/** Pending DMT — from legacy PendingDmt.js. */
export const PENDING_DMT: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectDMTPendingTransactionWithFilter',
  filters: { user: true },
  actions: ['Success / Refund', 'Check Status', 'Log'],
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
    { key: 'DOC', label: 'TXN Date', type: 'date' },
    { key: 'SenderName', label: 'Sender Name' },
    { key: 'SenderNo', label: 'Mobile Number' },
    { key: 'ConsumerNumber', label: 'Account Number', type: 'bold' },
    { key: 'Bank', label: 'Bank Name' },
    { key: 'BeneficiaryName', label: 'Beneficiary Name' },
    { key: 'Type', label: 'Payment Type' },
    { key: 'Amount', label: 'Total Amount', type: 'money' },
    { key: 'ProviderRequestID', label: 'TXN Ref. ID' },
  ],
};

/** DMT report — from legacy DMTReport.js (endpoint SelectForDMTFilter). */
export const DMT_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectForDMTFilter',
  filters: { status: 'dmt', user: true, criteria: true },
  actions: ['Print', 'Refund', 'Status Check', 'Log'],
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Txn Date', type: 'date' },
    { key: 'DMT_SenderName', label: 'Sender Name' },
    { key: 'DMT_SenderNumber', label: 'Sender Mobile' },
    { key: 'ConsumerNumber', label: 'Account Number', type: 'bold' },
    { key: 'DMT_Bank', label: 'Bank Name' },
    { key: 'DMT_IFSC', label: 'IFSC' },
    { key: 'DMT_BeneficiaryName', label: 'Beneficiary Name' },
    { key: 'DMT_BeneficaryNameByBank', label: 'Bene Name by Bank' },
    { key: 'DMT_PaymentType', label: 'Payment Mode' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'Charge', label: 'Charge', type: 'money' },
    { key: 'GST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'RetailerCommission', label: 'Commission', type: 'money' },
    { key: 'TDS', label: 'TDS', type: 'money', roles: ADMIN },
    { key: 'NetCommission', label: 'Net Commission', type: 'money' },
    { key: 'CommissionInfo', label: 'MDS/DS Commission', roles: ADMIN_DIST },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Txn Reference ID' },
    { key: 'ProviderRequestID', label: 'Request ID', roles: ADMIN_API },
  ],
};

/** BBPS report — from legacy BBPSReport.js (endpoint SelectForBBPS). */
export const BBPS_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/SelectForBBPS',
  filters: { service: true, operator: true, status: 'recharge', user: true, complaint: true, criteria: true },
  actions: ['Print', 'Complaint', 'Log'],
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'UserMobile', label: 'Mobile Number' },
    { key: 'OperatorName', label: 'Biller Name' },
    { key: 'BillerID', label: 'Biller Code' },
    { key: 'Logo', label: 'Logo', type: 'image' },
    { key: 'ConsumerNumber', label: 'Number', type: 'bold' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Operator Ref. ID', type: 'bold' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerCommission', label: 'Commission', type: 'money' },
    { key: 'TDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'money' },
    { key: 'CommissionInfo', label: 'MDS/DS Commission', roles: ADMIN_DIST },
    { key: 'DebitAmount', label: 'Debit', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'ProviderClosingBalance', label: 'API Cl Bal', type: 'money', roles: ADMIN },
    { key: 'AdminCommission', label: 'API Set Comm', type: 'money', roles: ADMIN },
    { key: 'Circle', label: 'Circle' },
    { key: 'APIUserRequestID', label: 'API Req', roles: ADMIN_API },
    { key: 'ProcTime', label: 'ProcTime (s)' },
    { key: 'IPAddress', label: 'IP' },
    { key: 'ProviderRequestID', label: 'Transaction Ref. Id' },
  ],
};

/** Payout report — from legacy PayoutReport.js (endpoint Payout). */
export const PAYOUT_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/Payout',
  filters: { status: 'recharge', user: true, criteria: true },
  actions: ['Print', 'Log'],
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'EntityID', label: 'Entity ID' },
    { key: 'ConsumerNumber', label: 'A/C Number', type: 'bold' },
    { key: 'IFSC', label: 'IFSC' },
    { key: 'BeneName', label: 'A/C Holder Name' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'Charge', label: 'Charge', type: 'money', roles: ADMIN },
    { key: 'GST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'DebitAmount', label: 'Total Amount', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Transaction ID' },
    { key: 'Description', label: 'Remark' },
    { key: 'PaymentType', label: 'Payment Mode' },
    { key: 'ProviderRequestID', label: 'Request ID', roles: ADMIN_API },
  ],
};

/** MicroATM report — from legacy MicroATM/Report.js (endpoint MicroATM). */
export const MICROATM_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/MicroATM',
  filters: { status: 'recharge', user: true, criteria: true },
  actions: ['Print', 'Log'],
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date', type: 'date' },
    { key: 'FirmName', label: 'Name' },
    { key: 'OperatorName', label: 'Operator' },
    { key: 'Logo', label: 'Logo', type: 'image' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerComm', label: 'Commission', type: 'money' },
    { key: 'TDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ConsumerNumber', label: 'Card Number' },
    { key: 'ReferenceNo', label: 'Op ID' },
    { key: 'ProviderName', label: 'Provider' },
    { key: 'Remark', label: 'Remark' },
    { key: 'Through', label: 'Through' },
    { key: 'IPAddress', label: 'IP' },
    { key: 'GST', label: 'GST', type: 'money', roles: ADMIN },
    { key: 'DistributerGST', label: 'Dist GST', type: 'money', roles: ADMIN },
    { key: 'DistributerTDS', label: 'Dist TDS', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerGST', label: 'Super GST', type: 'money', roles: ADMIN },
    { key: 'SuperDistributerTDS', label: 'Super TDS', type: 'money', roles: ADMIN },
    { key: 'RequestID', label: 'Request ID' },
  ],
};

/** AEPS report — from legacy AEPS/Report.js (endpoint AEPS). */
export const AEPS_REPORT: TxnReportConfig = {
  endpoint: 'TransactionReport/AEPS',
  filters: { status: 'recharge', user: true, criteria: true },
  actions: ['Print', 'Log'],
  columns: [
    { key: 'TransactionMasterID', label: '#', type: 'number' },
    { key: 'DOC', label: 'Date & Time', type: 'date' },
    { key: 'FirmName', label: 'Entity Name' },
    { key: 'SenderNumber', label: 'Mobile Number' },
    { key: 'OperatorName', label: 'Transaction Type' },
    { key: 'ConsumerNumber', label: 'Aadhaar Number' },
    { key: 'OpeningBalance', label: 'Op Bal', type: 'money' },
    { key: 'Amount', label: 'Amount', type: 'money' },
    { key: 'RetailerComm', label: 'Commission', type: 'money' },
    { key: 'TDS', label: 'TDS', type: 'money' },
    { key: 'NetCommission', label: 'Net Commission', type: 'money' },
    { key: 'DebitAmount', label: 'Total Amount', type: 'money' },
    { key: 'ClosingBalance', label: 'Cl Bal', type: 'money' },
    { key: 'Status', label: 'Status', type: 'status' },
    { key: 'ReferenceNo', label: 'Operator Ref. ID' },
    { key: 'BankName', label: 'Bank Name' },
    { key: 'RequestID', label: 'Transaction Ref. ID' },
    { key: 'DeviceType', label: 'Device Type' },
    { key: 'GST', label: 'GST', type: 'money', roles: ADMIN },
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
  filters: { user: true },
  columns: [
    { key: 'SenderID', label: '#', type: 'number' },
    { key: 'Name', label: 'Sender Name', type: 'bold' },
    { key: 'Mobile', label: 'Mobile' },
    { key: 'BeneficiaryCount', label: 'Beneficiaries', type: 'number' },
    { key: 'Address', label: 'Address' },
    { key: 'Pincode', label: 'Pincode' },
    { key: 'IsActive', label: 'Active' },
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
  filters: { user: true, service: true },
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'ServiceName', label: 'Service', type: 'bold' },
    { key: 'OperatorName', label: 'Operator' },
    { key: 'Details', label: 'Amount band' },
    { key: 'SlabName', label: 'Slab' },
    { key: 'StrMyCommission', label: 'My margin', type: 'bold' },
    { key: 'StrRetailerComm', label: 'Retailer' },
    { key: 'StrDistributorComm', label: 'Distributor', roles: ADMIN_DIST },
    { key: 'StrSuperDistributorComm', label: 'Super Dist', roles: ADMIN_DIST },
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
  columns: [
    { key: '_serial', label: '#', type: 'serial' },
    { key: 'TransactionDOC', label: 'Date & Time', type: 'date' },
    { key: 'SenderName', label: 'From', type: 'bold' },
    { key: 'ReceiverName', label: 'To', type: 'bold' },
    { key: 'WalletName', label: 'Wallet' },
    { key: 'Type', label: 'Type' },
    { key: 'ModeName', label: 'Mode' },
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
