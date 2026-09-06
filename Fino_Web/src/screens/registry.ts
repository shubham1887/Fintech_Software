import type { CrudConfig, FieldDef, FormConfig } from './crudTypes';
import type { TxnReportConfig } from './txnReportTypes';
import {
  RECHARGE_REPORT, DMT_REPORT, BBPS_REPORT, PAYOUT_REPORT, MICROATM_REPORT, AEPS_REPORT,
  PENDING_DMT, STATEMENT_REPORT, USER_LEDGER,
  PAST_SUCCESS_FAIL, RANGEWISE_RECHARGE, SALE_SERVICEWISE, SALE_OPERATOR, SALE_USERWISE, TDS_REPORT,
  INCOME_REPORT, USERWISE_ACCOUNTING, MOVE_FUNDS, DAYBOOK_REPORT, PROVIDER_SALE,
  OPERATOR_USER_REPORT, USER_SERVICE_SELL, USER_DMT_RANGE,
  TOP_PERFORMANCE, DOWNLINE_REPORT, NEWLY_ADDED_USERS, USER_ONBOARDED,
  SENDER_BENEFICIARY, SERVICE_CHARGE_DEDUCTION, COMMISSION_STRUCTURE, FUND_REPORT, PAYOUT_BENEFICIARY,
  SEARCH_REPORT, RETAILER_SEARCH_REPORT, MANUAL_UPDATE, PENDING_RECHARGE,
} from './txnConfigs';

/**
 * Registry of dynamically-generated screens. Each entry maps a backend menu URL to a
 * config the ScreenHost renders â€” no per-screen component needed. Add a screen = one entry.
 *
 *  - kind 'report' â†’ date/status/search filters + results table (TransactionReport.*)
 *  - kind 'list'   â†’ read-only master-data table from a <Master>/GetAll endpoint
 *  - kind 'crud'   â†’ list + Add (form) + Edit (field-level) + Delete
 *  - kind 'custom' â†’ bespoke component (registry `component` key â†’ ScreenHost map),
 *                    for high-traffic pages that deserve a richer hand-built design
 *  - kind 'form'   â†’ a single config-driven action form (DynamicForm â†’ one endpoint)
 */
export type ScreenKind = 'report' | 'list' | 'crud' | 'custom' | 'form' | 'txn';

export interface ScreenDef {
  path: string;
  title: string;
  kind: ScreenKind;
  subtitle?: string;
  endpoint?: string;
  payload?: Record<string, unknown>;
  crud?: CrudConfig;
  form?: FormConfig;
  txn?: TxnReportConfig;
  /** For kind 'custom': key into ScreenHost's CUSTOM_SCREENS map. */
  component?: string;
  /**
   * Access (see `lib/access.ts`). Screens are gated on the role-filtered menu the backend
   * returns; these two cover the screens that are not menu entries of their own:
   *  - `parent`  — the menu URL this screen hangs off (detail / drill-down pages).
   *  - `roles`   — an explicit user-type list, when the screen is deliberately outside the menu.
   * A path whose own section is already in the menu (e.g. "/Users/Edit" under "/Users") needs
   * neither.
   */
  parent?: string;
  roles?: number[];
}

/** Props every bespoke ('custom') screen receives. Components use only what they need. */
export interface CustomScreenProps {
  def: ScreenDef;
}

const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 200 };
// Admin master lists must show OFF rows too (with the toggle), so no IsActive filter here.
const ADMIN_LIST_PAGE = { PageNumber: 1, PageSize: 500 };
// Superset payload â€” GetAll endpoints ignore unknown fields and default missing ones,
// so this satisfies both PageNumber/PageSize and TakeCount/SkipCount conventions.
const GENERIC_LIST = { PageNumber: 1, PageSize: 500, IsActive: true, TakeCount: 500, SkipCount: 0, ShowDeleted: false };

/** Helper for read-only master list screens. */
const list = (path: string, title: string, endpoint: string, subtitle?: string): ScreenDef => ({
  path,
  title,
  kind: 'list',
  endpoint,
  payload: GENERIC_LIST,
  subtitle,
});

/** Helper for transaction/report screens (date/status/search filters + table). */
const report = (path: string, title: string, endpoint: string, subtitle?: string): ScreenDef => ({
  path,
  title,
  kind: 'report',
  endpoint,
  subtitle,
});

/** Helper for single-endpoint action forms. */
const form = (
  path: string,
  title: string,
  endpoint: string,
  fields: FieldDef[],
  opts: { subtitle?: string; submitLabel?: string; successMessage?: string; description?: string } = {},
): ScreenDef => ({
  path,
  title,
  kind: 'form',
  subtitle: opts.subtitle,
  form: { endpoint, fields, submitLabel: opts.submitLabel, successMessage: opts.successMessage, description: opts.description },
});

/** Reusable foreign-key picker: a user dropdown (PersonName by UserMasterID). */
const USER_SELECT = (name: string, label: string): FieldDef => ({
  name,
  label,
  type: 'select',
  required: true,
  searchable: true, // long lists → type-to-search dropdown
  optionsEndpoint: 'Users/Select',
  optionsPayload: GENERIC_LIST,
  optionValue: 'UserMasterID',
  optionLabel: 'PersonName',
  optionSublabel: ['FirmName', 'MobileNumber'],
});

// Wallet picker — every fund action (transfer / request / reverse) carries a WalletID telling
// the account engine which wallet to debit/credit. Loads all active wallets (Wallet/Select →
// WalletMaster { WalletID, Name }). Kept as a helper so the four fund forms stay identical.
const WALLET_SELECT = (label = 'Wallet', hint?: string): FieldDef => ({
  name: 'WalletID',
  label,
  type: 'select',
  required: true,
  optionsEndpoint: 'Wallet/Select',
  optionValue: 'WalletID',
  optionLabel: 'Name',
  hint,
});

const ROLE_OPTIONS = [
  { label: 'Super Distributor', value: 2 },
  { label: 'Distributor', value: 3 },
  { label: 'Retailer', value: 4 },
  { label: 'API User', value: 5 },
];

// â”€â”€ Action-form field sets (used by 'form' screens) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Field sets below mirror the backend Create/request DTOs exactly (verified 2026-06-24).
// Sub-mode account details shown as read-only text under the Sub mode picker (no image):
// bank rows populate for a bank sub-mode, the UPI/QR row for a QR sub-mode. Only non-empty show.
const SUBMODE_DETAILS = [
  { key: 'BankName', label: 'Bank' },
  { key: 'AccountNumber', label: 'A/C number' },
  { key: 'IFSC', label: 'IFSC' },
  { key: 'BeneficiaryName', label: 'Beneficiary' },
  { key: 'QRCode', label: 'UPI / QR' },
];

const FUND_TRANSFER_FIELDS: FieldDef[] = [
  USER_SELECT('ReceiverID', 'Transfer to user'),
  WALLET_SELECT('Wallet', 'Which wallet to debit / credit'),
  {
    name: 'ModeID', label: 'Mode', type: 'select', required: true,
    optionsEndpoint: 'Mode/GetActive', optionValue: 'ModeID', optionLabel: 'ModeName',
    hint: 'How the funds are being sent',
  },
  {
    name: 'SubModeID', label: 'Sub mode', type: 'select', required: true,
    optionsEndpoint: 'SubMode/GetByModeId', dependsOn: 'ModeID',
    optionValue: 'SubModeID', optionLabel: 'SubModeName',
    hint: 'Source / destination account',
    detailFields: SUBMODE_DETAILS,
  },
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
  { name: 'UTRNumber', label: 'UTR / Receipt No', type: 'text' },
  { name: 'TransactionDate', label: 'Transaction date', type: 'date' },
  { name: 'Remark', label: 'Remark', type: 'text', required: true, colSpan: 2 },
  { name: 'Document', label: 'Payment slip / receipt', type: 'image', colSpan: 2, hint: 'Optional — attach a screenshot or receipt' },
];

const FUND_REVERSE_FIELDS: FieldDef[] = [
  USER_SELECT('SenderID', 'Reverse from user'),
  WALLET_SELECT('Wallet', 'Which wallet to reverse from'),
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
  { name: 'Remark', label: 'Reason', type: 'text', colSpan: 2, required: true },
];

// Change Status settles a transaction through the account engine (TransactionProcessing/
// UpdateStatusManually): the wallet debit is reversed and every commission tier — retailer,
// distributor, super-distributor and admin — is credited/debited for whoever the transaction
// went through. The engine uses the transaction's own amount, so no amount is entered here.
const CHANGE_STATUS_FIELDS: FieldDef[] = [
  { name: 'TransactionID', label: 'Transaction ID', type: 'number', required: true },
  { name: 'Status', label: 'New status', type: 'select', required: true, options: [
    { label: 'Success', value: 'SUCCESS' },
    { label: 'Failed (refund)', value: 'FAILED' },
    { label: 'Refund', value: 'REFUND' },
  ] },
  { name: 'TxnID', label: 'Operator / reference no.', type: 'text', hint: 'Optional — recorded against the transaction' },
  { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2, required: true },
];

const ASSIGN_PLAN_FIELDS: FieldDef[] = [
  USER_SELECT('UserID', 'User'),
  { name: 'ServiceID', label: 'Service', type: 'select', required: true, optionsEndpoint: 'Service/GetAll', optionsPayload: LIST_PAGE, optionValue: 'ServiceID', optionLabel: 'Name' },
  { name: 'Charge', label: 'Charge (₹)', type: 'number', defaultValue: 0 },
  { name: 'DaysCount', label: 'Validity (days)', type: 'number', defaultValue: 30 },
  { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
];

const USER_TRANSFER_FIELDS: FieldDef[] = [
  USER_SELECT('UserID', 'User to move'),
  USER_SELECT('NewParentID', 'New parent'),
  { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
];

// Mirrors legacy UserBankDetailAdd.js (endpoint UserBankDetails/Insert).
const ADD_BANK_FIELDS: FieldDef[] = [
  USER_SELECT('UserID', 'User'),
  { name: 'BankName', label: 'Bank Name', type: 'text', required: true },
  { name: 'HolderName', label: 'Account Holder Name', type: 'text', required: true },
  { name: 'AccountNumber', label: 'Account Number', type: 'text', required: true },
  { name: 'Branch', label: 'Branch Name', type: 'text' },
  { name: 'IFSC', label: 'IFSC', type: 'text' },
  { name: 'UPINumber', label: 'UPI Number', type: 'text' },
];

const PUSH_NOTIFICATION_FIELDS: FieldDef[] = [
  { name: 'Title', label: 'Title', type: 'text', required: true, colSpan: 2 },
  { name: 'Description', label: 'Message', type: 'text', required: true, colSpan: 2 },
  { name: 'UserTypeID', label: 'Audience', type: 'select', options: [{ label: 'All users', value: 0 }, ...ROLE_OPTIONS] },
];

const MANUAL_ONBOARDING_FIELDS: FieldDef[] = [
  { name: 'Name', label: 'Full name', type: 'text', required: true, colSpan: 2 },
  { name: 'MobileNumber', label: 'Mobile number', type: 'text', required: true },
  { name: 'EmailID', label: 'Email', type: 'text' },
  { name: 'PanNo', label: 'PAN', type: 'text' },
  { name: 'AadhaarNo', label: 'Aadhaar', type: 'text' },
  { name: 'DOB', label: 'Date of birth', type: 'date' },
  { name: 'State', label: 'State', type: 'text' },
  { name: 'PinCode', label: 'Pincode', type: 'text' },
  { name: 'BankName', label: 'Bank name', type: 'text' },
  { name: 'AccountNumber', label: 'Account number', type: 'text' },
  { name: 'IFSC', label: 'IFSC', type: 'text' },
];

const RECON_UPLOAD_FIELDS: FieldDef[] = [
  { name: 'ProviderID', label: 'Provider', type: 'select', optionsEndpoint: 'ProviderMaster/GetAll', optionsPayload: LIST_PAGE, optionValue: 'ProviderID', optionLabel: 'ProviderName' },
  { name: 'ReconDate', label: 'Reconciliation date', type: 'date', required: true },
  { name: 'FileReference', label: 'File reference', type: 'text', hint: 'Uploaded file id / path' },
];

// Admin may only rename a wallet and enable/disable it â€” never create or delete one.
const WALLET_EDIT: FieldDef[] = [
  { name: 'Name', label: 'Wallet Name', type: 'text', required: true, colSpan: 2 },
  { name: 'IsActive', label: 'Active', type: 'checkbox' },
];

const OPERATOR_CREATE: FieldDef[] = [
  { name: 'OperatorName', label: 'Operator Name', type: 'text', required: true, colSpan: 2 },
  { name: 'ServiceID', label: 'Service', type: 'select', required: true, optionsEndpoint: 'Service/GetAll', optionsPayload: LIST_PAGE, optionValue: 'ServiceID', optionLabel: 'Name', hint: 'Pick a service' },
  { name: 'Opcode', label: 'Opcode', type: 'text', hint: '4 characters' },
  { name: 'MinimumRechargeAmount', label: 'Min Recharge (₹)', type: 'number', defaultValue: 0 },
  { name: 'MaximumRechargeAmount', label: 'Max Recharge (₹)', type: 'number', defaultValue: 0 },
  { name: 'RepeatRechargeInMinute', label: 'Repeat Recharge (min)', type: 'number', defaultValue: 0 },
  { name: 'NumberLength', label: 'Number Length', type: 'number', defaultValue: 0 },
  { name: 'MinLength', label: 'Min Length', type: 'number', defaultValue: 0 },
  { name: 'MaxLength', label: 'Max Length', type: 'number', defaultValue: 0 },
  { name: 'UISequence', label: 'UI Sequence', type: 'number', defaultValue: 0 },
  { name: 'HSNCode', label: 'HSN Code', type: 'text' },
  { name: 'BusinessModel', label: 'Business Model', type: 'text' },
  { name: 'Prefix', label: 'Prefix', type: 'text' },
  { name: 'AccountStartWith', label: 'Account Starts With', type: 'text' },
  { name: 'AccountName', label: 'Account Name', type: 'text' },
  { name: 'SequenceLogic', label: 'Sequence Logic', type: 'text' },
  { name: 'TollFreeNumber', label: 'Toll-free Number', type: 'text' },
  { name: 'OperatorImagePath', label: 'Logo Path', type: 'text' },
  { name: 'BlockedAmount', label: 'Blocked Amount', type: 'text' },
  { name: 'AllowedFor', label: 'Allowed For', type: 'text' },
  { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
  { name: 'IsActive', label: 'Active', type: 'checkbox', defaultValue: true },
  { name: 'IsBBPS', label: 'BBPS', type: 'checkbox', defaultValue: false },
  { name: 'IsBillFetch', label: 'Bill Fetch', type: 'checkbox', defaultValue: false },
  { name: 'IsPartialBill', label: 'Partial Bill', type: 'checkbox', defaultValue: false },
  { name: 'IsViewRequired', label: 'View Required', type: 'checkbox', defaultValue: false },
];

const OPERATOR_EDIT: FieldDef[] = [
  { name: 'OperatorName', label: 'Operator Name', type: 'text', colSpan: 2 },
  { name: 'Opcode', label: 'Opcode', type: 'text' },
  { name: 'MinimumRechargeAmount', label: 'Min Recharge (₹)', type: 'number' },
  { name: 'MaximumRechargeAmount', label: 'Max Recharge (₹)', type: 'number' },
  { name: 'HSNCode', label: 'HSN Code', type: 'text' },
  { name: 'UISequence', label: 'UI Sequence', type: 'number' },
  { name: 'IsDenominationApplicable', label: 'Denomination Applicable', type: 'checkbox', hint: 'Enforce denomination ranges for this operator' },
];

// â”€â”€ Retailer transaction-screen field sets (read from legacy UI/src/Pages/InnerPages) â”€â”€
/** Downline-user picker (Users/SelectDownlinForDropDown â†’ {UserID, FirmName}). */
const DOWNLINE_SELECT = (name: string, label: string): FieldDef => ({
  name,
  label,
  type: 'select',
  required: true,
  optionsEndpoint: 'Users/SelectDownlinForDropDown',
  optionsPayload: GENERIC_LIST,
  optionValue: 'UserID',
  optionLabel: 'FirmName',
});


const TRANSFER_MODE_OPTIONS = [
  { label: 'IMPS', value: 'IMPS' },
  { label: 'NEFT', value: 'NEFT' },
];

// Move funds to e-balance â€” mirrors legacy WalletTransfer.js (WalletApi.AEPSToOtherWalletTransfer).
const WALLET_TRANSFER_FIELDS: FieldDef[] = [
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true, colSpan: 2 },
  { name: 'Remark', label: 'Remark', type: 'text', required: true, colSpan: 2 },
];

// Fund Request â€” mirrors legacy Fund/Request.js (FundApi.RequestInsert).
const FUND_REQUEST_FIELDS: FieldDef[] = [
  WALLET_SELECT('Wallet', 'Which wallet to load the funds into'),
  {
    name: 'RequestTo', label: 'Request to', type: 'select', required: true,
    optionsEndpoint: 'Users/SelectParentWithAdmin', optionsPayload: GENERIC_LIST,
    optionValue: 'ParentID', optionLabel: 'FirmName',
  },
  {
    name: 'ModeID', label: 'Mode', type: 'select', required: true,
    optionsEndpoint: 'Mode/GetActive', optionValue: 'ModeID', optionLabel: 'ModeName',
    hint: 'How the funds are being sent',
  },
  {
    name: 'SubModeID', label: 'Sub mode', type: 'select', required: true,
    optionsEndpoint: 'SubMode/GetByModeId', dependsOn: 'ModeID',
    optionValue: 'SubModeID', optionLabel: 'SubModeName',
    hint: 'Source / destination account',
    detailFields: SUBMODE_DETAILS,
  },
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
  { name: 'UTRNumber', label: 'UTR / Receipt No', type: 'text' },
  { name: 'TransactionDate', label: 'Transaction date', type: 'date' },
  { name: 'Remark', label: 'Remark', type: 'text', required: true, colSpan: 2 },
  { name: 'Document', label: 'Payment slip / receipt', type: 'image', colSpan: 2, hint: 'Optional — attach a screenshot or receipt' },
];

// Fund Reverse Request â€” mirrors legacy Fund/ReverseRequest.js (FundApi.ReverseRequestInsert).
const FUND_REVERSE_REQUEST_FIELDS: FieldDef[] = [
  DOWNLINE_SELECT('SenderID', 'Reverse from (downline)'),
  WALLET_SELECT('Wallet', 'Which wallet to reverse from'),
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
  { name: 'Remark', label: 'Remark', type: 'text', required: true, colSpan: 2 },
];

// Payout â€” mirrors legacy PayoutV1 (TransactionProcessing/Payout).
const PAYOUT_FIELDS: FieldDef[] = [
  { name: 'BeneficiaryName', label: 'Beneficiary name', type: 'text', required: true, colSpan: 2 },
  { name: 'AccountNumber', label: 'Account number', type: 'text', required: true },
  { name: 'IFSC', label: 'IFSC code', type: 'text', required: true },
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
  { name: 'TransferMode', label: 'Transfer mode', type: 'select', required: true, defaultValue: 'IMPS', options: TRANSFER_MODE_OPTIONS },
  { name: 'MobileNumber', label: 'Mobile number', type: 'text' },
  { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
];

// Cash deposit â€” mirrors legacy CashDeposit flow (TransactionProcessing cash-deposit).
const CASH_DEPOSIT_FIELDS: FieldDef[] = [
  { name: 'BankName', label: 'Bank name', type: 'text', required: true },
  { name: 'AccountNumber', label: 'Account number', type: 'text', required: true },
  { name: 'IFSC', label: 'IFSC code', type: 'text' },
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
  { name: 'MobileNumber', label: 'Depositor mobile', type: 'text' },
  { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
];

// Bulk action on SD/Distributor downline â€” mirrors legacy UserWiseBulkActionSDAndDist.js (Users/BulkUpdate).
const BULK_ACTION_FIELDS: FieldDef[] = [
  { name: 'UserTypeID', label: 'Apply to role', type: 'select', required: true, options: ROLE_OPTIONS },
  { name: 'Action', label: 'Action', type: 'select', required: true, options: [
    { label: 'Activate', value: 'Activate' },
    { label: 'Deactivate', value: 'Deactivate' },
    { label: 'Hold Commission', value: 'HoldCommission' },
    { label: 'Release Commission', value: 'ReleaseCommission' },
  ] },
  { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
];

export const SCREENS: ScreenDef[] = [
  // â”€â”€ Reports (TransactionReport.*) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  { path: '/Recharge', title: 'Recharge Report', kind: 'txn', txn: RECHARGE_REPORT, subtitle: 'Mobile / DTH recharge transactions' },
  { path: '/AEPSReport', title: 'AEPS Report', kind: 'txn', txn: AEPS_REPORT, subtitle: 'Aadhaar Enabled Payment System' },
  { path: '/MicroATMReport', title: 'MicroATM Report', kind: 'txn', txn: MICROATM_REPORT, subtitle: 'Micro ATM transactions' },
  { path: '/Income', title: 'Income Report', kind: 'txn', txn: INCOME_REPORT, subtitle: 'Commission & surcharge income' },
  { path: '/ManualSuccessFailReport', title: 'Manual Update', kind: 'txn', txn: MANUAL_UPDATE, subtitle: 'Manually settled transactions' },

  // â”€â”€ Masters â€” full CRUD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  {
    path: '/Services/Index',
    title: 'Service Catalog',
    kind: 'custom',
    component: 'serviceCatalog',
    subtitle: 'Categories, services, icons, on/off, display & wallet mapping',
  },
  {
    path: '/Operators/Index',
    title: 'Operators',
    kind: 'custom',
    component: 'operators',
    subtitle: 'Add, edit and remove operators',
    crud: {
      idField: 'OperatorID',
      list: { endpoint: 'Operator/GetAll', payload: ADMIN_LIST_PAGE },
      create: { endpoint: 'Operator/Create', fields: OPERATOR_CREATE, successMessage: 'Operator created.' },
      update: { endpoint: 'Operator/Update', fields: OPERATOR_EDIT, successMessage: 'Operator updated.' },
      remove: { endpoint: 'Operator/Delete', successMessage: 'Operator deleted.' },
    },
  },

  {
    path: '/Wallets/Index',
    title: 'Wallets',
    kind: 'crud',
    subtitle: 'Rename or enable/disable wallets (create/delete not allowed)',
    crud: {
      idField: 'WalletID',
      list: { endpoint: 'Wallet/SelectAll' },
      // No create / remove â€” admin may only rename (edit Name) and disable (Active toggle).
      update: { endpoint: 'Wallet/Update', mode: 'field', fields: WALLET_EDIT, successMessage: 'Wallet updated.' },
    },
  },

  // â”€â”€ Bespoke pages (hand-built rich layouts) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  { path: '/RegistrationApprovals', title: 'Registration Approvals', kind: 'custom', component: 'registrationApprovals', subtitle: 'Approve or reject self-service sign-ups' },
  { path: '/Users', title: 'All Users', kind: 'custom', component: 'users', subtitle: 'Everyone in your distribution network' },
  { path: '/Users/Add', title: 'Add User', kind: 'custom', component: 'userForm', subtitle: 'Create a new user in your network' },
  { path: '/Users/Edit', title: 'Edit User', kind: 'custom', component: 'userForm', subtitle: 'Update user details' },
  { path: '/Users/Profile', title: 'User Profile', kind: 'custom', component: 'userProfile', subtitle: 'Full user details, balances & access' },
  { path: '/ApiAccess', title: 'API Access', kind: 'custom', component: 'apiAccess', subtitle: 'API token, IP security & callback URLs' },
  { path: '/ApiDocs', title: 'API Documentation', kind: 'custom', component: 'apiDocs', subtitle: 'Integration guide for the developer API' },

  // â”€â”€ Masters / settings â€” read-only lists (GetAll) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  { path: '/Complaints', title: 'Complaints', kind: 'custom', component: 'complaints', subtitle: 'Resolve user complaints with an outcome and remark' },
  {
    path: '/Slabs',
    title: 'Commission Slabs',
    kind: 'crud',
    subtitle: 'Create and manage commission slabs',
    crud: {
      idField: 'SlabMasterID',
      list: { endpoint: 'Slabs/GetAll' },
      create: { endpoint: 'Slabs/Create', fields: [{ name: 'SlabName', label: 'Slab Name', type: 'text', required: true, colSpan: 2 }, { name: 'IsActive', label: 'Active', type: 'checkbox', defaultValue: true }], successMessage: 'Slab created.' },
      update: { endpoint: 'Slabs/Update', mode: 'object', fields: [{ name: 'SlabName', label: 'Slab Name', type: 'text', required: true }, { name: 'IsActive', label: 'Active', type: 'checkbox' }], successMessage: 'Slab updated.' },
      remove: { endpoint: 'Slabs/Delete', successMessage: 'Slab deleted.' },
      links: [
        { label: 'Commission', to: (r) => `/s/Slabs/Commission?id=${r.SlabMasterID}&name=${encodeURIComponent(String(r.SlabName ?? ''))}` },
        { label: 'Assign', to: () => `/s/Slabs/Assign` },
      ],
    },
  },
  { path: '/Slabs/Commission', title: 'Slab Commission', kind: 'custom', component: 'slabCommission', subtitle: 'Per-operator commission rates' },
  // Operators / API
  {
    path: '/BBPSConfiguration/Index',
    title: 'BBPS Configuration',
    kind: 'crud',
    subtitle: 'Per-operator BBPS input fields',
    crud: {
      idField: 'BBPSConfigurationID',
      list: { endpoint: 'BBPSConfiguration/GetAll' },
      create: {
        endpoint: 'BBPSConfiguration/Create',
        fields: [
          { name: 'OperatorID', label: 'Operator', type: 'select', required: true, optionsEndpoint: 'Operator/GetAll', optionsPayload: LIST_PAGE, optionValue: 'OperatorID', optionLabel: 'OperatorName' },
          { name: 'Type', label: 'Field Type', type: 'text', placeholder: 'text / number / dropdown' },
          { name: 'Label', label: 'Label', type: 'text', required: true },
          { name: 'PlaceHolder', label: 'Placeholder', type: 'text' },
          { name: 'IsRequired', label: 'Required', type: 'checkbox', defaultValue: true },
          { name: 'ErrorMeeage', label: 'Error Message', type: 'text', colSpan: 2 },
          { name: 'Sequence', label: 'Sequence', type: 'number', defaultValue: 0 },
        ],
        successMessage: 'BBPS field added.',
      },
      update: {
        endpoint: 'BBPSConfiguration/Update',
        fields: [
          { name: 'Label', label: 'Label', type: 'text' },
          { name: 'PlaceHolder', label: 'Placeholder', type: 'text' },
          { name: 'Sequence', label: 'Sequence', type: 'number' },
        ],
        successMessage: 'BBPS field updated.',
      },
      remove: { endpoint: 'BBPSConfiguration/Delete', successMessage: 'BBPS field deleted.' },
    },
  },
  list('/OperatorUserwise', 'Userwise Operators', 'OperatorUserwise/GetAll'),
  // Provider gateway suite â€” hub + one tabbed Setting page (Request / Configuration / Response).
  { path: '/ProvidersAdd', title: 'API Providers', kind: 'custom', component: 'providerHub', subtitle: 'Upstream provider gateways' },
  { path: '/ProviderSetting', title: 'Provider Setting', kind: 'custom', component: 'providerSetting', subtitle: 'Request / Configuration / Response', parent: '/ProvidersAdd' },
  {
    path: '/DenominationDetails',
    title: 'Denominations',
    kind: 'crud',
    subtitle: 'Recharge denomination ranges',
    crud: {
      idField: 'DenominationMasterID',
      list: { endpoint: 'DenominationMaster/GetAll' },
      create: {
        endpoint: 'DenominationMaster/Create',
        fields: [
          { name: 'DenominationRange', label: 'Denomination Range', type: 'text', required: true, colSpan: 2, placeholder: 'e.g. 10-5000' },
          { name: 'Description', label: 'Description', type: 'text', colSpan: 2 },
          { name: 'IsStop', label: 'Stopped', type: 'checkbox', defaultValue: false },
        ],
        successMessage: 'Denomination added.',
      },
      update: {
        endpoint: 'DenominationMaster/Update',
        mode: 'object',
        fields: [
          { name: 'DenominationRange', label: 'Denomination Range', type: 'text' },
          { name: 'Description', label: 'Description', type: 'text' },
          { name: 'IsStop', label: 'Stopped', type: 'checkbox' },
        ],
        successMessage: 'Denomination updated.',
      },
      remove: { endpoint: 'DenominationMaster/Delete', successMessage: 'Denomination deleted.' },
    },
  },
  {
    path: '/DenominationMapping',
    title: 'Denomination Mapping',
    kind: 'crud',
    subtitle: 'Which provider serves which denomination per operator (applies only to operators flagged Denomination Applicable)',
    crud: {
      idField: 'ProviderOperatorDenominationMappingID',
      list: { endpoint: 'ProviderOperatorDenominationMapping/GetAll' },
      // Show names, not raw ids (backend resolves ProviderName / OperatorName / DenominationRange).
      columns: [
        { key: 'ProviderName', label: 'Provider' },
        { key: 'OperatorName', label: 'Operator' },
        { key: 'DenominationRange', label: 'Denomination' },
      ],
      create: {
        endpoint: 'ProviderOperatorDenominationMapping/Create',
        fields: [
          { name: 'ProviderID', label: 'Provider', type: 'select', required: true, optionsEndpoint: 'ProviderMaster/GetAll', optionsPayload: LIST_PAGE, optionValue: 'ProviderID', optionLabel: 'ProviderName' },
          { name: 'OperatorID', label: 'Operator', type: 'select', required: true, optionsEndpoint: 'Operator/GetAll', optionsPayload: LIST_PAGE, optionValue: 'OperatorID', optionLabel: 'OperatorName' },
          { name: 'DenominationMasterID', label: 'Denomination', type: 'select', required: true, optionsEndpoint: 'DenominationMaster/GetAll', optionsPayload: LIST_PAGE, optionValue: 'DenominationMasterID', optionLabel: 'DenominationRange' },
        ],
        successMessage: 'Denomination mapping added.',
      },
      update: {
        endpoint: 'ProviderOperatorDenominationMapping/Update',
        mode: 'object',
        fields: [
          { name: 'ProviderID', label: 'Provider', type: 'select', optionsEndpoint: 'ProviderMaster/GetAll', optionsPayload: LIST_PAGE, optionValue: 'ProviderID', optionLabel: 'ProviderName' },
          { name: 'OperatorID', label: 'Operator', type: 'select', optionsEndpoint: 'Operator/GetAll', optionsPayload: LIST_PAGE, optionValue: 'OperatorID', optionLabel: 'OperatorName' },
          { name: 'DenominationMasterID', label: 'Denomination', type: 'select', optionsEndpoint: 'DenominationMaster/GetAll', optionsPayload: LIST_PAGE, optionValue: 'DenominationMasterID', optionLabel: 'DenominationRange' },
        ],
        successMessage: 'Denomination mapping updated.',
      },
      remove: { endpoint: 'ProviderOperatorDenominationMapping/Delete', successMessage: 'Denomination mapping deleted.' },
    },
  },
  // /ResponseAdd merged into the Provider Setting page's Response tab.
  { path: '/ResponseAdd', title: 'Provider Setting', kind: 'custom', component: 'providerSetting', subtitle: 'Request / Configuration / Response', parent: '/ProvidersAdd' },
  list('/CircleAPIAdd', 'Circle APIs', 'CircleAPIMaster/GetAll'),
  list('/ReconciliationSettings', 'Reconciliation Settings', 'ReconciliationSetting/GetAll'),
  {
    path: '/AddPurchase',
    title: 'Provider Purchases',
    kind: 'crud',
    subtitle: 'Record provider stock purchases',
    crud: {
      idField: 'ProviderPurchaseID',
      list: { endpoint: 'ProviderPurchase/GetAll' },
      create: {
        endpoint: 'ProviderPurchase/Create',
        fields: [
          { name: 'ProviderID', label: 'Provider', type: 'select', required: true, optionsEndpoint: 'ProviderMaster/GetAll', optionValue: 'ProviderID', optionLabel: 'ProviderName' },
          // WalletID is an FK to WalletMaster — required, or the INSERT violates the constraint.
          { name: 'WalletID', label: 'Wallet', type: 'select', required: true, optionsEndpoint: 'Wallet/Select', optionValue: 'WalletID', optionLabel: 'Name' },
          { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
          { name: 'PurchaseDate', label: 'Purchase Date', type: 'date', required: true },
          { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
        ],
        successMessage: 'Purchase recorded.',
      },
      remove: { endpoint: 'ProviderPurchase/Delete', successMessage: 'Purchase deleted.' },
    },
  },
  // Settings — Key Setting = enable/disable client functions (feature toggles only).
  // Charges & limits (verify fee, transfer commission) live in the Slab / DMT Settings, not here.
  { path: '/KeySetting', title: 'Key Settings', kind: 'custom', component: 'keySettings', subtitle: 'Enable or disable client functions' },
  { path: '/VerificationDocuments', title: 'Verification Settings', kind: 'custom', component: 'verificationDocuments', subtitle: 'Configure the documents users upload for KYC / Manual Verification' },
  { path: '/UISetting', title: 'UI Settings', kind: 'custom', component: 'uiSettings', subtitle: 'Theme colours, fonts & layout â€” drives the whole app' },
  { path: '/WebsitePages', title: 'Website Pages', kind: 'custom', component: 'websiteBuilder', subtitle: 'Build the public website â€” pages, hero sliders, sections & HTML' },
  list('/PlanSetting', 'Plan Settings', 'PlanSetting/GetAll'),
  {
    path: '/SMSAdd',
    title: 'Communication Setup',
    kind: 'custom',
    component: 'messagingSetup',
    subtitle: 'SMS, WhatsApp & Email delivery settings — one place for every channel',
  },
  { path: '/MessageSetting', title: 'Message Settings', kind: 'custom', component: 'messageSetting', subtitle: 'SMS, WhatsApp & email templates for every event — with on/off switches' },
  list('/News/Index', 'News & Notifications', 'News/GetAll'),
  list('/Slider', 'Sliders', 'SliderMaster/GetAll'),
  list('/ServiceWiseSlider', 'Service-wise Sliders', 'ServiceWiseSliderSetting/GetAll'),
  list('/DMTBankSetting', 'DMT Banks', 'DMTBank/GetAll'),
  list('/VirtualAccountSetting', 'Virtual Accounts', 'VirtualAccountMaster/GetAll'),
  list('/MenuAdd', 'Menus', 'MenuMaster/GetAll'),
  list('/BulkMessageIndex', 'Bulk Messages', 'BulkMessage/GetAll'),
  // Service settings
  { path: '/DMTSettings', title: 'DMT Settings', kind: 'custom', component: 'dmtSetting', subtitle: 'Domestic Money Transfer gateways — limits, split, transfer modes & API credentials' },
  list('/AEPSSetting', 'AEPS Settings', 'AEPSSetting/GetAll'),
  list('/MicroATMSetting', 'MicroATM Settings', 'MicroATMSetting/GetAll'),
  list('/POSSetting', 'POS Settings', 'POSSettingMaster/GetAll'),
  list('/PayoutSetting', 'Payout Settings', 'PayoutSetting/GetAll'),
  list('/CashDepositSetting', 'Cash Deposit Settings', 'CashDepositSetting/GetAll'),
  list('/EcollectionSetting', 'E-Collection Settings', 'EcollectionSetting/GetAll'),
  list('/eKYCSetting', 'eKYC Settings', 'eKYCSetting/GetAll'),
  list('/CMSSetting', 'CMS Settings', 'CMSSetting/GetAll'),
  list('/InvoiceSettings', 'Invoice Settings', 'InvoiceSetting/GetAll'),
  list('/VerificationSettings', 'Verification Settings', 'VerificationSetting/GetAll'),
  // Admin group â€” logs, reports, onboarding (read-only)
  { path: '/SmsReport', title: 'Sent SMS', kind: 'custom', component: 'smsLog', subtitle: 'Outbound SMS log — request & response' },
  list('/EcollectionReport', 'E-Collection', 'EcollectionDetails/GetAll'),
  { path: '/ProviderLog', title: 'Provider Log', kind: 'custom', component: 'providerLog', subtitle: 'Provider request/response log' },
  list('/PaymentGatewayLog', 'Payment Gateway Log', 'PaymentGatewayLog/GetAll'),
  list('/ApiUserLog', 'API User Log', 'APIUserInitialRequestResponseLog/GetAll'),
  list('/BalanceCapping', 'Balance Capping', 'UserBalanceCapping/GetAll'),
  list('/TicketReport', 'Tickets', 'TicketMaster/GetAll', 'Support tickets'),
  { path: '/LoginHistory', title: 'Login History', kind: 'custom', component: 'loginHistory', subtitle: 'Successful sign-ins by user, channel and date' },
  list('/ViewAgentOnboarding', 'User Onboarding', 'OnboardingUser/GetAll'),
  list('/Invoice', 'Invoices', 'InvoiceNumberHistory/GetAll'),
  { path: '/VirtualBalance', title: 'Virtual Balance', kind: 'custom', component: 'virtualBalance', subtitle: 'Load balance into your own wallet, then fund-transfer it onward' },

  // â•â• Remaining admin menu â€” full coverage, in menu order â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
  // Top-level
  { path: '/Search', title: 'Search', kind: 'txn', txn: SEARCH_REPORT, subtitle: 'Find a transaction by ID, number or reference' },

  // Pending
  { path: '/Pending', title: 'Pending Recharge', kind: 'custom', component: 'pendingRecharge', subtitle: 'Recharges awaiting confirmation — check, resend, success or refund' },
  { path: '/PendingDmt', title: 'Pending DMT', kind: 'txn', txn: PENDING_DMT, subtitle: 'Money transfers awaiting confirmation' },
  { path: '/PendingRecon', title: 'Pending Reconciliation', kind: 'txn', txn: PENDING_RECHARGE, subtitle: 'Transactions still awaiting a final status' },

  // Fund
  { path: '/Fund', title: 'Pending Requests', kind: 'custom', component: 'fundRequests', subtitle: 'Fund-load requests awaiting your approval' },
  // Fund Request Mode + SubMode masters — one bespoke screen; sub modes (with bank / QR
  // details) are managed nested under each mode.
  { path: '/ModeMaster', title: 'Modes & Sub Modes', kind: 'custom', component: 'modeMaster', subtitle: 'Fund-request transfer methods' },
  form('/Fund/Transfer', 'Fund Transfer', 'Fund/Transfer', FUND_TRANSFER_FIELDS, {
    subtitle: 'Credit or debit a user wallet', submitLabel: 'Transfer', successMessage: 'Fund transfer submitted.',
    description: 'Move balance to or from a user wallet. The amount is applied instantly on success.',
  }),
  form('/FundReverse', 'Fund Reverse', 'Fund/Reverse', FUND_REVERSE_FIELDS, {
    subtitle: 'Reverse a fund transfer', submitLabel: 'Reverse', successMessage: 'Reverse request submitted.',
  }),
  report('/FundReverseRequestReport', 'Reverse Requests', 'Fund/ReverseReport', 'Fund reverse requests'),

  // Admin
  form('/ManualAgentOnboarding', 'Add User Onboarding', 'OnboardingUser/Create', MANUAL_ONBOARDING_FIELDS, {
    subtitle: 'Onboard a new agent manually', submitLabel: 'Onboard', successMessage: 'Onboarding submitted.',
  }),
  { path: '/AdminAccounting', title: 'Admin Accounting', kind: 'custom', component: 'adminAccounting', subtitle: 'Wallet-wise company accounting — opening / closing, sale, purchase, credit / debit, commission' },
  { path: '/AdminLedger', title: 'Admin Ledger', kind: 'custom', component: 'adminLedger', subtitle: 'Company account statement — running-balance ledger with period summary' },
  { path: '/ProviderDayBook', title: 'Provider Daybook', kind: 'custom', component: 'providerDaybook', subtitle: 'One row per provider — expand for operator & service sale' },
  { path: '/ProviderSale', title: 'Provider Sale', kind: 'txn', txn: PROVIDER_SALE, subtitle: 'Provider purchase / sale / profit' },
  { path: '/PurchaseReport', title: 'Purchase Report', kind: 'custom', component: 'purchaseReport', subtitle: 'Provider payments — month / date wise' },
  form('/ChangeStatus', 'Change Status', 'TransactionProcessing/UpdateStatusManually', CHANGE_STATUS_FIELDS, {
    subtitle: 'Manually settle a transaction', submitLabel: 'Update status', successMessage: 'Status updated.',
    description: 'Force a transaction to a final state. This credits/debits the wallet and reverses every commission tier (retailer, distributor, super-distributor, admin) for whoever the transaction went through. Use with care.',
  }),
  { path: '/UserOnboardedReport', title: 'User Onboard Report', kind: 'txn', txn: USER_ONBOARDED, subtitle: 'Onboarded merchants' },
  { path: '/SenderBeneficiaryReport', title: 'Sender Beneficiary', kind: 'txn', txn: SENDER_BENEFICIARY, subtitle: 'DMT senders & their beneficiaries' },

  // Report
  { path: '/DMTReport', title: 'DMT Report', kind: 'txn', txn: DMT_REPORT, subtitle: 'Domestic money transfer transactions' },
  { path: '/PayoutReport', title: 'Payout Report', kind: 'txn', txn: PAYOUT_REPORT, subtitle: 'Payout / settlement transactions' },
  { path: '/BBPSReport', title: 'BBPS Report', kind: 'txn', txn: BBPS_REPORT, subtitle: 'Bill payments' },
  { path: '/SettlementReport', title: 'Settlement Report', kind: 'txn', txn: STATEMENT_REPORT, subtitle: 'Settlement / transaction-detail statement' },
  { path: '/Fund/Report', title: 'Fund Report', kind: 'txn', txn: FUND_REPORT, subtitle: 'Every fund transfer, request and reversal in your network' },
  { path: '/MoveFundsReport', title: 'Transfer to e-Balance', kind: 'txn', txn: MOVE_FUNDS, subtitle: 'Wallet transfer report' },
  { path: '/AccountStatement', title: 'Account Statement', kind: 'txn', txn: STATEMENT_REPORT, subtitle: 'Full account statement' },
  { path: '/CommissionStructure', title: 'Commission Structure', kind: 'txn', txn: COMMISSION_STRUCTURE, subtitle: 'Slab commission by user & service' },
  { path: '/SaleReport', title: 'Service Wise Sale', kind: 'txn', txn: SALE_SERVICEWISE, subtitle: 'Sale totals by service' },
  { path: '/OperatorSale', title: 'Operator Sale', kind: 'txn', txn: SALE_OPERATOR, subtitle: 'Sale totals by operator' },
  { path: '/UserwiseSale', title: 'Userwise Sale', kind: 'txn', txn: SALE_USERWISE, subtitle: 'Sale totals by user' },
  { path: '/UserwiseAccounting', title: 'Userwise Accounting', kind: 'txn', txn: USERWISE_ACCOUNTING, subtitle: 'Per-user accounting summary' },
  { path: '/UserLedger', title: 'Ledger', kind: 'txn', txn: USER_LEDGER, subtitle: 'User wallet ledger' },
  { path: '/Daybook/Report', title: 'Daybook', kind: 'txn', txn: DAYBOOK_REPORT, subtitle: 'Operator-wise daybook' },
  { path: '/TDS', title: 'TDS Report', kind: 'txn', txn: TDS_REPORT, subtitle: 'Tax deducted at source' },
  { path: '/PastSuccessFailReport', title: 'Past Success/Fail', kind: 'txn', txn: PAST_SUCCESS_FAIL, subtitle: 'Manually settled past transactions' },
  { path: '/UserWiseDMTRangeReport', title: 'Userwise Range Report', kind: 'txn', txn: USER_DMT_RANGE, subtitle: 'DMT totals by amount range, per user' },
  { path: '/RangeWiseRecharge', title: 'Rangewise Recharge', kind: 'txn', txn: RANGEWISE_RECHARGE, subtitle: 'Recharge totals by amount range' },
  { path: '/ServiceChargeDeductionReport', title: 'Service Charge', kind: 'txn', txn: SERVICE_CHARGE_DEDUCTION, subtitle: 'Service charge deductions by user' },
  { path: '/OperatorWiseUserWiseReport', title: 'Operator/User Report', kind: 'txn', txn: OPERATOR_USER_REPORT, subtitle: 'Daily sale by service' },
  { path: '/UserWiseServiceSellReport', title: 'Userwise Operator Sale', kind: 'txn', txn: USER_SERVICE_SELL, subtitle: 'Yesterday vs today sale, per user' },
  { path: '/PayoutBeneficiaryReport', title: 'Payout Beneficiary', kind: 'txn', txn: PAYOUT_BENEFICIARY, subtitle: 'Bank beneficiaries registered for payouts' },
  { path: '/ProviderWisePerformanceGraph', title: 'Provider Performance', kind: 'custom', component: 'providerPerformance', subtitle: 'Success-rate performance by provider' },

  // Users
  list('/AgentDetailReport', 'Agent Detail', 'AgentDetails/GetAll', 'Agent detail upload / download'),
  { path: '/CreatePlanIndex', title: 'Subscription Plans', kind: 'custom', component: 'subscriptionPlans', subtitle: 'Bundle services into paid plans users can subscribe to' },
  form('/AssignPlan', 'Assign Plan', 'ServiceAndUserwiseSubscription/Create', ASSIGN_PLAN_FIELDS, {
    subtitle: 'Assign a subscription plan to a user', submitLabel: 'Assign', successMessage: 'Plan assigned.',
  }),
  // Bespoke lifecycle screens. They filter Users/Select SERVER-side (IsActive:false / OnlyDeleted),
  // so neither can ever show a user that doesn't belong on it.
  { path: '/Users/Inactive', title: 'Inactive Users', kind: 'custom', component: 'inactiveUsers', subtitle: 'Deactivated accounts, with the full record and the activate action' },
  { path: '/Users/KycApprovals', title: 'KYC Approvals', kind: 'custom', component: 'kycApprovals', subtitle: 'Review, approve or reject user KYC and set what a pending KYC may do' },
  { path: '/DownlineReport', title: 'Downline', kind: 'txn', txn: DOWNLINE_REPORT, subtitle: 'Your downline network + balances' },
  { path: '/Performance', title: 'Top Performance', kind: 'txn', txn: TOP_PERFORMANCE, subtitle: 'Top-selling users' },
  { path: '/Users/UserAlias', title: 'User Alias', kind: 'custom', component: 'userAlias', subtitle: 'The code each user type stamps on its users, and the sequence it counts' },
  { path: '/Users/DatewiseReport', title: 'Newly Added Users', kind: 'txn', txn: NEWLY_ADDED_USERS, subtitle: 'Users added by date' },
  { path: '/Users/DeletedUsers', title: 'Deleted Users', kind: 'custom', component: 'deletedUsers', subtitle: 'Removed accounts, kept on record' },
  form('/Users/UserTransfer', 'User Transfer', 'Users/TransferUsers', USER_TRANSFER_FIELDS, {
    subtitle: 'Move a user under a new parent', submitLabel: 'Transfer user', successMessage: 'User transferred.',
  }),
  form('/Users/UserWiseBulkAction', 'Userwise Bulk Action', 'Users/BulkUpdate', [
    { name: 'UserTypeID', label: 'Role', type: 'select', options: ROLE_OPTIONS },
    { name: 'Action', label: 'Action', type: 'select', required: true, options: [
      { label: 'Activate', value: 'Activate' }, { label: 'Deactivate', value: 'Deactivate' }, { label: 'Reset password', value: 'ResetPassword' },
    ] },
    { name: 'Remark', label: 'Remark', type: 'text', colSpan: 2 },
  ], { subtitle: 'Apply an action to many users at once', submitLabel: 'Apply', successMessage: 'Bulk action queued.' }),
  list('/IDFYProfile', 'IDFY Profiles', 'IdfyOnboarding/GetAll'),
  list('/MISInterestedAgents', 'Onboarding Leads', 'LeadGenerationMaster/GetAll', 'Interested-agent leads'),

  // Slab
  { path: '/Slabs/Assign', title: 'Assign Slab', kind: 'custom', component: 'slabAssign', subtitle: 'Assign commission slabs to users' },

  // Operators
  form('/Operators/Add', 'Add Operator', 'Operator/Create', OPERATOR_CREATE, {
    subtitle: 'Create a new operator', submitLabel: 'Create operator', successMessage: 'Operator created.',
  }),

  // API
  { path: '/UserwiseSequenceIndex', title: 'User-wise Switching', kind: 'custom', component: 'userwiseSequence', subtitle: 'Per-user provider sequence overrides' },
  list('/UserWiseDMTSequenceAdd', 'Userwise DMT Sequence', 'UserwiseDMTSequence/Select'),
  { path: '/ProvidersSequence', title: 'Provider Sequence', kind: 'custom', component: 'providerSequence', subtitle: 'Operator-wise provider switching priority' },
  list('/RangeWiseSequence', 'DMT Range Sequence', 'RangeWiseSequence/GetAll'),
  form('/ReconciliationUpload', 'Reconciliation Upload', 'Reconciliation/Upload', RECON_UPLOAD_FIELDS, {
    subtitle: 'Upload a provider reconciliation file', submitLabel: 'Upload', successMessage: 'Reconciliation uploaded.',
  }),

  // Setting
  list('/SupportAccess', 'Support Access', 'SupportUser/GetAll', 'Support-user access'),
  form('/AddBank', 'Add Bank', 'UserBankDetails/Create', ADD_BANK_FIELDS, {
    subtitle: 'Add a bank account', submitLabel: 'Add bank', successMessage: 'Bank added.',
  }),
  form('/PushNotifications', 'App Alert', 'PushNotifications/SendPushNotification', PUSH_NOTIFICATION_FIELDS, {
    subtitle: 'Send an in-app / push alert', submitLabel: 'Send alert', successMessage: 'Alert sent.',
  }),

  // Service â€” serviceâ†”wallet mapping (which wallet a service debits from)
  {
    path: '/SeviceswiseWallet',
    title: 'Service-wise Wallet',
    kind: 'crud',
    subtitle: 'Map which wallet each service debits from',
    crud: {
      idField: 'WalletServiceMappingID',
      list: { endpoint: 'WalletServiceMapping/GetAll', payload: LIST_PAGE },
      columns: [
        { key: 'WalletServiceMappingID', label: 'ID' },
        { key: 'WalletName', label: 'Wallet Name' },
        { key: 'ServiceName', label: 'Service Name' },
        { key: 'DateTime', label: 'Date Time' },
        { key: 'UpdatedateTime', label: 'Update Date Time' },
      ],
      create: {
        endpoint: 'WalletServiceMapping/Create',
        fields: [
          { name: 'ServiceID', label: 'Service', type: 'select', required: true, optionsEndpoint: 'Service/GetAll', optionsPayload: LIST_PAGE, optionValue: 'ServiceID', optionLabel: 'Name', hint: 'Which service' },
          { name: 'WalletID', label: 'Wallet', type: 'select', required: true, optionsEndpoint: 'Wallet/Select', optionValue: 'WalletID', optionLabel: 'Name', hint: 'Which wallet it debits from' },
        ],
        successMessage: 'Wallet mapping created.',
      },
      remove: { endpoint: 'WalletServiceMapping/Delete', successMessage: 'Mapping removed.' },
    },
  },
  { path: '/ServiceSubscriptionCharge', title: 'Service Subscription', kind: 'custom', component: 'serviceSubscription', subtitle: 'Subscribe to a plan to unlock its services' },

  // â”€â”€ Retailer transaction screens (other-role menus) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  form('/WalletTransfer', 'Move Funds to E-Balance', 'WalletToWalletTransfer/AEPSToOther', WALLET_TRANSFER_FIELDS, {
    subtitle: 'Transfer your AEPS balance to your main wallet', submitLabel: 'Transfer',
    successMessage: 'Funds moved to your e-balance.',
    description: 'Move available AEPS wallet balance into your main e-balance wallet.',
  }),
  form('/Fund/Request', 'Fund Request', 'Fund/RequestInsert', FUND_REQUEST_FIELDS, {
    subtitle: 'Raise a fund request to your parent / admin', submitLabel: 'Send request',
    successMessage: 'Fund request submitted.',
    description: 'Submit a fund-load request. Your parent will accept or reject it from their pending queue.',
  }),
  form('/Fund/ReverseRequest', 'Fund Reverse Request', 'Fund/ReverseRequest', FUND_REVERSE_REQUEST_FIELDS, {
    subtitle: 'Request a reversal of funds from a downline', submitLabel: 'Send request',
    successMessage: 'Reverse request submitted.',
    description: 'Raise a request to reverse funds previously transferred to a downline user.',
  }),
  form('/PayoutV1', 'Payout', 'TransactionProcessing/Payout', PAYOUT_FIELDS, {
    subtitle: 'Send money to a bank account', submitLabel: 'Pay out',
    successMessage: 'Payout initiated.',
    description: 'Instant bank transfer to any beneficiary account via IMPS / NEFT.',
  }),
  // Dynamic DMT flow: sender mobile → (registered ⇒ beneficiary list + transfer) /
  // (not registered ⇒ registration form → OTP verify → beneficiary add). Renders the
  // hand-built DmtPage instead of a flat one-shot form.
  { path: '/DMTTransaction', title: 'Money Transfer (DMT)', kind: 'custom', component: 'dmt', subtitle: 'Domestic money transfer to a bank account' },
  form('/CashDepositIndex', 'Cash Deposit', 'CashDeposit/Initiate', CASH_DEPOSIT_FIELDS, {
    subtitle: 'Deposit cash to a bank account', submitLabel: 'Deposit',
    successMessage: 'Cash deposit initiated.',
    description: 'Accept cash and deposit it directly to a customer bank account.',
  }),
  form('/UserWiseBulkActionSDAndDist', 'Bulk Action (SD / Distributor)', 'Users/BulkUpdate', BULK_ACTION_FIELDS, {
    subtitle: 'Apply an action across a downline role', submitLabel: 'Apply',
    successMessage: 'Bulk action applied.',
    description: 'Activate, deactivate or hold commission for all users of a selected role at once.',
  }),
  { path: '/RetailerSearch', title: 'Quick Search', kind: 'txn', txn: RETAILER_SEARCH_REPORT, subtitle: 'Search a transaction by consumer number' },
  { path: '/AEPSIndex', title: 'AEPS â€” Cash Withdrawal', kind: 'custom', component: 'aeps', subtitle: 'Aadhaar Enabled Payment System' },
];

const SCREEN_MAP: Record<string, ScreenDef> = Object.fromEntries(
  SCREENS.map((s) => [s.path.toLowerCase(), s]),
);

export function findScreen(path: string): ScreenDef | undefined {
  return SCREEN_MAP[path.toLowerCase()];
}
