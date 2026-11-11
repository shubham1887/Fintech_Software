import { UserTypeCode } from '@/lib/roles';
import type { CrudConfig, FieldDef, FormConfig } from './crudTypes';
import type { TxnReportConfig } from './txnReportTypes';
import {
  RECHARGE_REPORT, DMT_REPORT, BBPS_REPORT, PAYOUT_REPORT, MICROATM_REPORT, AEPS_REPORT,
  STATEMENT_REPORT, USER_LEDGER,
  PAST_SUCCESS_FAIL, RANGEWISE_RECHARGE, SALE_SERVICEWISE, SALE_OPERATOR, SALE_USERWISE, TDS_REPORT,
  INCOME_REPORT, USERWISE_ACCOUNTING, MOVE_FUNDS, DAYBOOK_REPORT, PROVIDER_SALE,
  OPERATOR_USER_REPORT, USER_SERVICE_SELL, USER_DMT_RANGE,
  TOP_PERFORMANCE, DOWNLINE_REPORT, NEWLY_ADDED_USERS, USER_ONBOARDED,
  SENDER_BENEFICIARY, SERVICE_CHARGE_DEDUCTION, COMMISSION_STRUCTURE, FUND_REPORT, PAYOUT_BENEFICIARY,
  SEARCH_REPORT, RETAILER_SEARCH_REPORT, MANUAL_UPDATE, PENDING_RECHARGE, HOLD_REPORT,
  FUND_REVERSE_REPORT, FUND_REVERSE_REQUESTS, USER_CHANGE_HISTORY, PROFIT_LOSS,
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

// The `report` helper that used to sit here had one caller left — Reverse Requests — and that
// screen moved to kind 'txn' for the filters and the status badge it needs. The 'report' kind
// and ReportScreen stay available; nothing in the registry declares one today.

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
  // The tier first: it is the thing that decides whether this is the right user at all, and two
  // accounts under one person's name differ by nothing else in this list.
  optionSublabel: ['UserType', 'FirmName', 'MobileNumber'],
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

// User Transfer. "New parent" cascades off the user being moved rather than listing everybody:
// Users/SelectEligibleParents derives the legal tiers from that user's own role (a retailer may
// sit under Admin / Super Distributor / Distributor, a distributor only under Admin / Super
// Distributor) and drops their own downline, their current parent and deactivated accounts. Two
// flat user pickers offered moves the server then refused, which is why the screen read as broken.
const USER_TRANSFER_FIELDS: FieldDef[] = [
  USER_SELECT('UserID', 'User to move'),
  {
    name: 'NewParentID',
    label: 'New parent',
    type: 'select',
    required: true,
    searchable: true,
    optionsEndpoint: 'Users/SelectEligibleParents',
    dependsOn: 'UserID',
    optionsPayloadKey: 'TargetUserID',
    optionValue: 'UserMasterID',
    optionLabel: 'FirmName',
    optionSublabel: ['UserType', 'MobileNumber'],
    hint: 'Only the tiers this user is allowed to sit under',
  },
  // The backend refuses a blank remark — a silent re-parent cannot be explained afterwards, and
  // this is the text that lands in the user's change history.
  { name: 'Remark', label: 'Reason', type: 'text', colSpan: 2, required: true,
    hint: 'Recorded in the user change history with your name and IP' },
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
  searchable: true,
  // Which tier the name belongs to, and the number to confirm it by. A downline can hold the
  // same firm name at two tiers, and picking the wrong one here moves real money.
  optionSublabel: ['UserType', 'MobileNumber'],
});


const TRANSFER_MODE_OPTIONS = [
  { label: 'IMPS', value: 'IMPS' },
  { label: 'NEFT', value: 'NEFT' },
];


// AEPS to Other Wallet. The source is always the AEPS wallet, so it is not asked for — the only
// decision is where the AEPS earnings should land. The picker lists the user's other wallets with
// their live balances and leaves AEPS itself out (WalletToWalletTransfer/AEPSDestinations), so
// there is no option here that the transfer would refuse.
const AEPS_TRANSFER_FIELDS: FieldDef[] = [
  {
    name: 'WalletID',
    label: 'Transfer to',
    type: 'select',
    required: true,
    optionsEndpoint: 'WalletToWalletTransfer/AEPSDestinations',
    optionValue: 'WalletID',
    optionLabel: 'Label',
    hint: 'The wallet your AEPS balance moves into',
    colSpan: 2,
  },
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

export const SCREENS: ScreenDef[] = [
  // â”€â”€ Reports (TransactionReport.*) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  { path: '/Recharge', title: 'Recharge Report', kind: 'txn', txn: RECHARGE_REPORT, subtitle: 'Mobile / DTH recharge transactions' },
  { path: '/AEPSReport', title: 'AEPS Report', kind: 'txn', txn: AEPS_REPORT, subtitle: 'Aadhaar Enabled Payment System' },
  { path: '/MicroATMReport', title: 'MicroATM Report', kind: 'txn', txn: MICROATM_REPORT, subtitle: 'Micro ATM transactions' },
  { path: '/Income', title: 'Income Report', kind: 'txn', txn: INCOME_REPORT, subtitle: 'Commission & surcharge income' },
  { path: '/ManualSuccessFailReport', title: 'Manual Settlement', kind: 'txn', txn: MANUAL_UPDATE, subtitle: 'Manually settled transactions' },

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
    title: 'Operator List',
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
  // Account security — every signed-in user can change their own password and MPIN, so it is
  // gated by an explicit all-roles list rather than a menu entry (it lives in the user menu).
  { path: '/AccountSecurity', title: 'Account Security', kind: 'custom', component: 'security', subtitle: 'Password, MPIN and two-step verification', roles: [1, 2, 3, 4, 5, 6, 7, 9, 10, 11] },
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
      // Explicit columns so the raw OperatorID stays out of the table — the operator's NAME is
      // what identifies the row to a human, and the id is a database detail nobody configuring a
      // BBPS field needs to read. (The endpoint already returns OperatorName.)
      columns: [
        { key: 'OperatorName', label: 'Operator' },
        { key: 'Label', label: 'Label' },
        { key: 'Type', label: 'Field Type' },
        { key: 'PlaceHolder', label: 'Placeholder' },
        { key: 'IsRequired', label: 'Required' },
        { key: 'Length', label: 'Length' },
        { key: 'ErrorMeeage', label: 'Error Message' },
        { key: 'Sequence', label: 'Sequence' },
        { key: 'IsDisplay', label: 'Shown' },
      ],
      // One operator has several input fields and the install has hundreds of operators, so
      // the list is read one operator at a time.
      filters: [{ key: 'OperatorName', label: 'Operator' }, { key: 'Type', label: 'Field type' }],
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
  {
    path: '/CircleAPIAdd',
    title: 'Circle-wise Routing',
    kind: 'crud',
    subtitle: 'Endpoints used to look up a mobile number’s telecom circle',
    crud: {
      idField: 'CircleAPIMasterID',
      list: { endpoint: 'CircleAPIMaster/GetAll' },
      columns: [
        { key: 'CircleAPIMasterID', label: '#' },
        { key: 'CircleAPIURL', label: 'Lookup URL' },
        { key: 'IsActive', label: 'Active' },
        { key: 'DateTime', label: 'Added' },
      ],
      create: {
        endpoint: 'CircleAPIMaster/Create',
        fields: [
          { name: 'CircleAPIURL', label: 'Lookup URL', type: 'text', required: true, colSpan: 2, placeholder: 'https://…/circle?number={mobile}', hint: 'The endpoint called to resolve a number’s circle before a recharge is routed' },
          { name: 'IsActive', label: 'Active', type: 'checkbox', defaultValue: true },
        ],
        successMessage: 'Circle API added.',
      },
      update: {
        endpoint: 'CircleAPIMaster/Update',
        mode: 'object',
        fields: [
          { name: 'CircleAPIURL', label: 'Lookup URL', type: 'text', required: true, colSpan: 2 },
          { name: 'IsActive', label: 'Active', type: 'checkbox' },
        ],
        successMessage: 'Circle API updated.',
      },
      remove: { endpoint: 'CircleAPIMaster/Delete', successMessage: 'Circle API deleted.' },
    },
  },
  {
    path: '/AddPurchase',
    title: 'Provider Purchases',
    kind: 'crud',
    subtitle: 'Record provider stock purchases',
    crud: {
      idField: 'ProviderPurchaseID',
      list: { endpoint: 'ProviderPurchase/GetAll' },
      // Names, not ids: the endpoint resolves ProviderName / WalletName alongside the row, and
      // the purchase id is a serial nobody reads.
      columns: [
        { key: '_serial', label: '#' },
        { key: 'PurchaseDate', label: 'Purchase Date' },
        { key: 'ProviderName', label: 'Provider' },
        { key: 'WalletName', label: 'Wallet' },
        { key: 'Amount', label: 'Amount (₹)' },
        { key: 'Remark', label: 'Remark' },
        { key: 'DateTime', label: 'Recorded on' },
      ],
      filters: [{ key: 'ProviderName', label: 'Provider' }, { key: 'WalletName', label: 'Wallet' }],
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
  {
    path: '/SMSAdd',
    title: 'Communication Setup',
    kind: 'custom',
    component: 'messagingSetup',
    subtitle: 'SMS, WhatsApp & Email delivery settings — one place for every channel',
  },
  { path: '/MessageSetting', title: 'Message Templates', kind: 'custom', component: 'messageSetting', subtitle: 'SMS, WhatsApp & email templates for every event — with on/off switches' },
  {
    path: '/News/Index',
    title: 'News & Notifications',
    kind: 'crud',
    subtitle: 'Announcements shown to the whole network — publish, edit or take one down',
    crud: {
      idField: 'NewsID',
      list: { endpoint: 'News/GetAll' },
      // The author's UserID is stamped from the signed-in session on the server, so it is
      // neither a form field nor a column anyone needs to read as a number.
      columns: [
        { key: 'Title', label: 'Title' },
        { key: 'Description', label: 'Message' },
        { key: 'HyperLink', label: 'Link' },
        { key: 'IsView', label: 'Published' },
        { key: 'ExpiresOn', label: 'Hides on' },
        { key: 'PublishedBy', label: 'Published by' },
        { key: 'DateTime', label: 'Date' },
      ],
      create: {
        endpoint: 'News/Create',
        fields: [
          { name: 'Title', label: 'Title', type: 'text', required: true, colSpan: 2 },
          { name: 'Description', label: 'Message', type: 'text', required: true, colSpan: 2, hint: 'Shown to every user on the dashboard' },
          { name: 'HyperLink', label: 'Link (optional)', type: 'text', colSpan: 2, placeholder: 'https://…' },
          // An upload, not a URL box. Asking an operator to paste a hosted image URL for an
          // offer meant the field was left empty and offers went out as plain text.
          { name: 'ImagePath', label: 'Image (optional)', type: 'image', colSpan: 2, hint: 'Shown in the notification and on the dashboard banner' },
          // An offer or a maintenance notice stops being true on a known day, and nothing
          // used to take one down — the banner kept showing last month's offer until
          // somebody remembered to unpublish it.
          { name: 'ExpiresOn', label: 'Hide after (optional)', type: 'date', hint: 'Stops showing at the end of this day. Leave empty to keep it up until you take it down.' },
          { name: 'IsView', label: 'Publish now', type: 'checkbox', defaultValue: true, hint: 'Leave off to save it as a draft only you can see' },
        ],
        successMessage: 'Notification published.',
      },
      update: {
        endpoint: 'News/Update',
        mode: 'object',
        fields: [
          { name: 'Title', label: 'Title', type: 'text', required: true, colSpan: 2 },
          { name: 'Description', label: 'Message', type: 'text', required: true, colSpan: 2 },
          { name: 'HyperLink', label: 'Link', type: 'text', colSpan: 2 },
          { name: 'ImagePath', label: 'Image', type: 'image', colSpan: 2, hint: 'Shown in the notification and on the dashboard banner' },
          { name: 'ExpiresOn', label: 'Hide after', type: 'date', hint: 'Stops showing at the end of this day. Clear it to keep the announcement up.' },
          { name: 'IsView', label: 'Published', type: 'checkbox' },
        ],
        successMessage: 'Notification updated.',
      },
      remove: { endpoint: 'News/Delete', successMessage: 'Notification removed.' },
    },
  },
  // Sliders, Service-wise Sliders and Menus are gone: banners are edited on Website Pages and
  // the menu from Menu Visibility; the three read-only dumps only invited edits in the wrong place.
  list('/DMTBankSetting', 'DMT Banks', 'DMTBank/GetAll'),
  list('/VirtualAccountSetting', 'Virtual Accounts', 'VirtualAccountMaster/GetAll'),
  // Which of the product's screens this install puts on the sidebar. Menu-only: it never changes
  // who may call an endpoint, which stays the server access policy's job.
  { path: '/MenuVisibility', title: 'Menu Visibility', kind: 'custom', component: 'menuVisibility',
    subtitle: 'Show or hide any menu entry — for the whole install or one role',
    roles: [UserTypeCode.Admin, UserTypeCode.SupportUser] },
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
  // The company's own profile, not a master list. It is a form rather than a `list` entry
  // because the company GSTIN had no way in otherwise, and without a GSTIN the document series
  // cannot be seeded and nothing can be invoiced.
  { path: '/InvoiceSettings', title: 'Invoice Settings', kind: 'custom', component: 'invoiceSettings', subtitle: 'Company identity on an invoice — GSTIN, PAN, address, bank & signature' },
  list('/VerificationSettings', 'Verification Settings', 'VerificationSetting/GetAll'),
  // Admin group â€” logs, reports, onboarding (read-only)
  { path: '/SmsReport', title: 'SMS Log', kind: 'custom', component: 'smsLog', subtitle: 'Outbound SMS log — request & response' },
  list('/EcollectionReport', 'E-Collection', 'EcollectionDetails/GetAll'),
  { path: '/ProviderLog', title: 'Provider Logs', kind: 'custom', component: 'providerLog', subtitle: 'Provider request/response log' },
  list('/PaymentGatewayLog', 'Payment Gateway Log', 'PaymentGatewayLog/GetAll'),
  // Filtered by date / service / operator / partner, with names instead of ids — the raw table
  // dump it replaced listed a log id and a user id and nothing an operator could search by.
  { path: '/ApiUserLog', title: 'API Partner Logs', kind: 'custom', component: 'apiUserLog', subtitle: 'What each API partner sent and what we answered — by date, service and operator' },
  list('/BalanceCapping', 'Balance Capping', 'UserBalanceCapping/GetAll'),
  { path: '/TicketReport', title: 'Support Tickets', kind: 'custom', component: 'tickets', subtitle: 'Raise, track and answer support tickets' },
  // The bell's "See all" target, and the Support menu entry every role gets.
  { path: '/Notifications', title: 'Notifications', kind: 'custom', component: 'notifications', subtitle: 'Alerts, approvals and ticket replies' },
  {
    path: '/TicketCategory',
    title: 'Ticket Categories',
    kind: 'crud',
    subtitle: 'The categories users pick from when raising a ticket',
    parent: '/TicketReport',
    crud: {
      idField: 'TicketCategoryID',
      list: { endpoint: 'TicketCategory/GetAll', payload: GENERIC_LIST },
      columns: [
        { key: 'TicketCategoryID', label: '#' },
        { key: 'Name', label: 'Category' },
      ],
      create: {
        endpoint: 'TicketCategory/Create',
        fields: [{ name: 'Name', label: 'Category name', type: 'text', required: true, colSpan: 2 }],
        successMessage: 'Category added.',
      },
      update: {
        endpoint: 'TicketCategory/Update',
        mode: 'field',
        fields: [{ name: 'Name', label: 'Category name', type: 'text', required: true, colSpan: 2 }],
        successMessage: 'Category updated.',
      },
      remove: { endpoint: 'TicketCategory/Delete', successMessage: 'Category deleted.' },
    },
  },
  { path: '/LoginHistory', title: 'Login History', kind: 'custom', component: 'loginHistory', subtitle: 'Successful sign-ins by user, channel and date' },
  { path: '/HoldReport', title: 'Balance Hold Report', kind: 'txn', txn: HOLD_REPORT, subtitle: 'Every balance hold and release — wallet, amount, who did it and why' },
  list('/ViewAgentOnboarding', 'User Onboarding', 'OnboardingUser/GetAll'),
  // The sale invoices themselves, as bills that can be opened and printed. This row used to
  // list InvoiceNumberHistory — a legacy numbering table nothing writes to — so it was always
  // empty while the real invoices sat on another screen.
  { path: '/Invoice', title: 'Invoices', kind: 'custom', component: 'invoices', subtitle: 'Sale invoice bills — open, print or download each one' },
  { path: '/VirtualBalance', title: 'Virtual Balance', kind: 'custom', component: 'virtualBalance', subtitle: 'Load balance into your own wallet, then fund-transfer it onward' },

  // â•â• Remaining admin menu â€” full coverage, in menu order â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
  // Top-level
  { path: '/Search', title: 'Search', kind: 'txn', txn: SEARCH_REPORT, subtitle: 'Find a transaction by ID, number or reference' },

  // Pending
  { path: '/Pending', title: 'Pending Recharge', kind: 'custom', component: 'pendingRecharge', subtitle: 'Recharges awaiting confirmation — check, resend, success or refund' },
  { path: '/PendingDmt', title: 'Pending DMT', kind: 'custom', component: 'pendingDmt', subtitle: 'Money transfers awaiting confirmation — check, resend, hold, success or refund' },
  { path: '/PendingRecon', title: 'Pending Reconciliation', kind: 'txn', txn: PENDING_RECHARGE, subtitle: 'Transactions still awaiting a final status' },
  // The hub. One page per SERVICE KIND, built from what the install sells, so bill payments and
  // electricity stop being invisible when they stick.
  { path: '/PendingQueues', title: 'Pending Queues', kind: 'custom', component: 'pendingQueues', subtitle: 'Every service with transactions awaiting a final answer' },

  // Fund
  { path: '/Fund', title: 'Fund Requests', kind: 'custom', component: 'fundRequests', subtitle: 'Fund-load requests awaiting your approval' },
  // Outstanding (cash collection). Sits beside Fund Requests because it is the other half of the
  // same act: a fund request approved on Credit funds a wallet AND records a debt, and this is
  // where that debt is read, chased and cleared.
  //
  // An explicit role list rather than relying on a menu row: everyone who can owe money or be
  // owed it needs to reach it, including a retailer opening their own balance from the header
  // tile, and that tile is a link rather than a menu entry. The server scopes what each of them
  // actually sees, so this list widens reachability, never visibility.
  {
    path: '/Outstanding',
    title: 'Outstanding',
    kind: 'custom',
    component: 'outstanding',
    subtitle: 'Credit given, cash collected and what is still owed',
    roles: [
      UserTypeCode.Admin, UserTypeCode.SupportUser, UserTypeCode.SuperDistributor,
      UserTypeCode.Distributor, UserTypeCode.Retailer, UserTypeCode.SDFOS, UserTypeCode.DFOS,
    ],
  },
  // The reading half of the same ledger, and a separate screen on purpose: the one above is a
  // desk (set a limit, record a collection, chase someone), this one is a report — how much is
  // out, how old it is, who is past their limit, and an export. Same roles, same server scoping.
  {
    path: '/OutstandingReport',
    title: 'Outstanding Report',
    kind: 'custom',
    component: 'outstandingReport',
    subtitle: 'What is owed, how old it is, and who is over their limit',
    // The tiers that are OWED money, plus the accounting desk. Not the retailer or API-user
    // tier: the report is a downline view and the endpoint refuses them, so listing it for them
    // would only open an error. Their own balance lives on /Outstanding and the header bar.
    roles: [
      UserTypeCode.Admin, UserTypeCode.SupportUser, UserTypeCode.SuperDistributor,
      UserTypeCode.Distributor, UserTypeCode.Accountant,
      UserTypeCode.SDFOS, UserTypeCode.DFOS,
    ],
  },
  // Fund Request Mode + SubMode masters — one bespoke screen; sub modes (with bank / QR
  // details) are managed nested under each mode.
  { path: '/ModeMaster', title: 'Modes & Sub Modes', kind: 'custom', component: 'modeMaster', subtitle: 'Fund-request transfer methods' },
  form('/Fund/Transfer', 'Fund Transfer', 'Fund/Transfer', FUND_TRANSFER_FIELDS, {
    subtitle: 'Credit or debit a user wallet', submitLabel: 'Transfer', successMessage: 'Fund transfer submitted.',
    description: 'Move balance to or from a user wallet. The amount is applied instantly on success.',
  }),
  // Bespoke: shows every wallet the chosen user holds, with what is on hold, and offers to
  // release the hold when the amount would otherwise bounce as "insufficient balance".
  { path: '/FundReverse', title: 'Fund Reverse', kind: 'custom', component: 'fundReverse', subtitle: 'Pull balance back from a user wallet' },
  // The queue of reverse requests awaiting a decision. A txn screen rather than the generic
  // report kind because it needs two things the generic one cannot do: pin Type to
  // REVERSE_REQUEST (the endpoint also returns completed reversals) and open on Pending.
  { path: '/FundReverseRequestReport', title: 'Reversal Requests', kind: 'txn', txn: FUND_REVERSE_REQUESTS,
    subtitle: 'Reverse requests awaiting a decision — pending first' },
  // The reversal ledger. Same endpoint as Reverse Requests above, but a full report screen with
  // the wallet / status / entity filters and the before-and-after balances, because "what did we
  // pull back, from whom, and why" is a reconciliation question rather than a queue to work.
  { path: '/FundReverseReport', title: 'Fund Reversal Report', kind: 'txn', txn: FUND_REVERSE_REPORT,
    subtitle: 'Every fund reversal and reverse request in your network' },

  // Admin
  form('/ManualAgentOnboarding', 'Add User Onboarding', 'OnboardingUser/Create', MANUAL_ONBOARDING_FIELDS, {
    subtitle: 'Onboard a new agent manually', submitLabel: 'Onboard', successMessage: 'Onboarding submitted.',
  }),
  { path: '/AdminAccounting', title: 'Accounting Summary', kind: 'custom', component: 'adminAccounting', subtitle: 'Wallet-wise company accounting — opening / closing, sale, purchase, credit / debit, commission' },
  { path: '/AdminLedger', title: 'Platform Ledger', kind: 'custom', component: 'adminLedger', subtitle: 'Company account statement — running-balance ledger with period summary' },
  { path: '/TaxSimulator', title: 'Tax Rule Simulator', kind: 'custom', component: 'taxSimulator', subtitle: 'Try a transaction against the live rules — see the tax, the TDS, and which rule won' },
  // GST documents. The endpoints behind these are registered in ApiAccessPolicy for the
  // accounting desk (read) with the write/queue actions pinned to Admin / Admin+Support; each
  // screen hides the buttons the signed-in role cannot use rather than letting the server 403.
  { path: '/TaxInvoices', title: 'GST Documents', kind: 'custom', component: 'taxInvoices', subtitle: 'Invoices, credit notes and bills of supply — with the transactions behind each figure' },
  { path: '/ExchangeRegister', title: 'Exchange Register', kind: 'custom', component: 'exchangeRegister', subtitle: 'Barter with exchange partners — net against turnover, and what still needs their invoice' },
  { path: '/EInvoiceQueue', title: 'E-Invoice Queue', kind: 'custom', component: 'einvoiceQueue', subtitle: 'Documents waiting for an IRN, and the 30-day deadline counting down on each' },
  { path: '/CreditNoteAlerts', title: 'Credit-Note Window', kind: 'custom', component: 'creditNoteAlerts', subtitle: 'Documents whose credit-note window is closing while they still cover a failed transaction' },
  // Phase 3 — the inward side. Reads are open to the accounting desk; Capture / Import / Commit /
  // Build / Claim / Reverse are pinned to Admin (or Admin+Support) in ApiAccessPolicy and each
  // screen hides the button rather than letting the server answer 403.
  { path: '/PurchaseRegister', title: 'Purchase Invoice', kind: 'custom', component: 'purchaseRegister', subtitle: 'Bills from your suppliers — add, view and correct them; the GST on them is your credit' },
  { path: '/Gstr2bRecon', title: 'GSTR-2B Reconciliation', kind: 'custom', component: 'gstr2bRecon', subtitle: 'Four buckets against the portal snapshot, with the provider wallet as the third check' },
  { path: '/ItcRegister', title: 'ITC Register', kind: 'custom', component: 'itcRegister', subtitle: 'Every credit we may claim, with the Rule 37 and Section 16(4) clocks on each' },
  // Phase 4 — the double-entry ledger.
  { path: '/ChartOfAccounts', title: 'Chart of Accounts', kind: 'custom', component: 'chartOfAccounts', subtitle: 'The accounts the posting rules and the integrity checks refer to' },
  { path: '/LedgerVouchers', title: 'Ledger Vouchers', kind: 'custom', component: 'ledgerVouchers', subtitle: 'Posted vouchers, and the outbox behind them — including what failed and why' },
  { path: '/TrialBalance', title: 'Trial Balance & Integrity', kind: 'custom', component: 'trialBalance', subtitle: 'The books proved against themselves, the wallets and provider stock — plus the period lock' },
  // Phase 5 — the bank statement against the ledger's Bank account. Preview is open to the
  // accounting desk (it changes nothing); Upload / Commit / Match / Unmatch are Admin+Support.
  { path: '/BankRecon', title: 'Bank Reconciliation', kind: 'custom', component: 'bankRecon', subtitle: "The bank's own statement beside the ledger's Bank account — and the difference, explained" },
  // Phase 6 — the returns. Building and exporting change nothing, so the accounting desk does
  // both; only saving a snapshot is Admin, because a filed snapshot is what GSTR-9 reconciles to.
  { path: '/GstReturns', title: 'GST Returns', kind: 'custom', component: 'gstReturns', subtitle: 'The month in five steps — sales, purchases, the 2B file, the GST to pay, and the files to upload' },
  // The state every document is stamped with. It decides CGST+SGST against IGST and the portal
  // rejects an invalid one, so it has a screen of its own rather than hiding on a party form.
  { path: '/PartyPlaceOfSupply', title: 'Place of Supply', kind: 'custom', component: 'partyState', subtitle: 'The state each party is billed in — derived from what onboarding already captured' },
  // Whether a commission is inclusive or exclusive of GST, what the tax is charged on, and
  // whether TDS is deducted. These were literals in the code until now.
  { path: '/CommissionTax', title: 'Commission Tax', kind: 'custom', component: 'commissionTax', subtitle: 'Inclusive or exclusive, on the commission or the whole amount, and whether TDS is deducted' },
  // How much TDS comes off a commission and whose — per user, per role, per service, or for the
  // whole platform, as a percentage or a flat figure. Admin and the accountant both set these.
  { path: '/TdsSetting', title: 'TDS Setting', kind: 'custom', component: 'tdsSetting', subtitle: 'Who loses how much TDS on a commission — and try a figure before a month is deducted on it' },
  // What a period actually collected, split by how each charge was quoted (inclusive / exclusive /
  // no GST), beside the output tax the returns are built from — with every difference named.
  { path: '/GstCollection', title: 'GST Collection', kind: 'custom', component: 'gstCollection', subtitle: 'Inclusive or exclusive — GST collected, input credit, and what is left to pay' },
  // The configuration bank reconciliation needs: the banks pulled from Payment Modes, how each one
  // writes its statement (PDF included), what a line on it means, and deposits claimed twice.
  { path: '/BankSetup', title: 'Bank Setup', kind: 'custom', component: 'bankSetup', subtitle: 'Banks from payment modes, statement formats for Excel and PDF, and what each line means' },
  // The month: who owes what, and whether it was deposited or handed back.
  { path: '/TdsMonthly', title: 'TDS Report', kind: 'custom', component: 'tdsMonthlyReport', subtitle: 'Whose TDS was withheld this month, and whether it was deposited or returned to their wallet' },
  { path: '/ProviderDayBook', title: 'Provider Daybook', kind: 'custom', component: 'providerDaybook', subtitle: 'One row per provider — expand for operator & service sale' },
  { path: '/ProviderSale', title: 'Provider Sales', kind: 'txn', txn: PROVIDER_SALE, subtitle: 'Provider purchase / sale / profit' },
  { path: '/PurchaseReport', title: 'Purchase Report', kind: 'custom', component: 'purchaseReport', subtitle: 'Provider payments — month / date wise' },
  form('/ChangeStatus', 'Change Status', 'TransactionProcessing/UpdateStatusManually', CHANGE_STATUS_FIELDS, {
    subtitle: 'Manually settle a transaction', submitLabel: 'Update status', successMessage: 'Status updated.',
    description: 'Force a transaction to a final state. This credits/debits the wallet and reverses every commission tier (retailer, distributor, super-distributor, admin) for whoever the transaction went through. Any open complaint on the transaction is closed by the same decision — Failed refunds the customer and accepts the complaint, Success rejects it. Use with care.',
  }),
  { path: '/UserOnboardedReport', title: 'User Onboarding Report', kind: 'txn', txn: USER_ONBOARDED, subtitle: 'Onboarded merchants' },
  { path: '/SenderBeneficiaryReport', title: 'Sender & Beneficiary Report', kind: 'txn', txn: SENDER_BENEFICIARY, subtitle: 'DMT senders & their beneficiaries' },

  // Report
  { path: '/DMTReport', title: 'DMT Report', kind: 'txn', txn: DMT_REPORT, subtitle: 'Domestic money transfer transactions' },
  { path: '/PayoutReport', title: 'Payout Report', kind: 'txn', txn: PAYOUT_REPORT, subtitle: 'Payout / settlement transactions' },
  { path: '/BBPSReport', title: 'BBPS Report', kind: 'txn', txn: BBPS_REPORT, subtitle: 'Bill payments' },
  // Settlement = AEPS cash moved out of the AEPS wallet into a spendable one. This screen used to
  // render the ACCOUNT STATEMENT config — every ledger posting the user had — which is a different
  // report entirely and is already on the menu under its own name; nothing on it was the
  // settlement anybody came looking for.
  { path: '/SettlementReport', title: 'Settlement Report', kind: 'txn', txn: MOVE_FUNDS, subtitle: 'AEPS settled into the AEPS wallet and out to payout' },
  { path: '/Fund/Report', title: 'Fund Report', kind: 'txn', txn: FUND_REPORT, subtitle: 'Every fund transfer, request and reversal in your network' },
  // The accounting desk's P&L. Grouped by date, service, operator or user from one screen.
  { path: '/ProfitLoss', title: 'Profit & Loss', kind: 'txn', txn: PROFIT_LOSS, subtitle: 'Opening, purchase, sale, closing, commission, GST and net profit' },
  // No longer a menu row (it is the same report as Settlement Report above), so it carries its
  // own role list to stay reachable by direct link.
  { path: '/MoveFundsReport', title: 'Wallet Transfer Report', kind: 'txn', txn: MOVE_FUNDS, subtitle: 'Wallet transfer report', roles: [1, 2, 3, 4, 5, 6, 7, 9, 10, 11] },
  { path: '/AccountStatement', title: 'Account Statement', kind: 'txn', txn: STATEMENT_REPORT, subtitle: 'Full account statement' },
  { path: '/CommissionStructure', title: 'Commission Structure', kind: 'txn', txn: COMMISSION_STRUCTURE, subtitle: 'Slab commission by user & service' },
  { path: '/SaleReport', title: 'Sales by Service', kind: 'txn', txn: SALE_SERVICEWISE, subtitle: 'Sale totals by service' },
  { path: '/OperatorSale', title: 'Sales by Operator', kind: 'txn', txn: SALE_OPERATOR, subtitle: 'Sale totals by operator' },
  { path: '/UserwiseSale', title: 'Sales by User', kind: 'txn', txn: SALE_USERWISE, subtitle: 'Sale totals by user' },
  { path: '/UserwiseAccounting', title: 'User Accounting', kind: 'txn', txn: USERWISE_ACCOUNTING, subtitle: 'Per-user accounting summary' },
  // Off the menu as a duplicate of Account Statement; kept reachable by direct link.
  { path: '/UserLedger', title: 'Ledger', kind: 'txn', txn: USER_LEDGER, subtitle: 'User wallet ledger', roles: [1, 2, 3, 4, 5, 6, 7] },
  { path: '/Daybook/Report', title: 'Daybook', kind: 'txn', txn: DAYBOOK_REPORT, subtitle: 'Operator-wise daybook' },
  { path: '/TDS', title: 'TDS Report', kind: 'txn', txn: TDS_REPORT, subtitle: 'Tax deducted at source' },
  { path: '/PastSuccessFailReport', title: 'Past Settlements', kind: 'txn', txn: PAST_SUCCESS_FAIL, subtitle: 'Manually settled past transactions' },
  { path: '/UserWiseDMTRangeReport', title: 'DMT Range by User', kind: 'txn', txn: USER_DMT_RANGE, subtitle: 'DMT totals by amount range, per user' },
  { path: '/RangeWiseRecharge', title: 'Recharge by Range', kind: 'txn', txn: RANGEWISE_RECHARGE, subtitle: 'Recharge totals by amount range' },
  { path: '/ServiceChargeDeductionReport', title: 'Service Charges', kind: 'txn', txn: SERVICE_CHARGE_DEDUCTION, subtitle: 'Service charge deductions by user' },
  { path: '/OperatorWiseUserWiseReport', title: 'Operator Sale by User', kind: 'txn', txn: OPERATOR_USER_REPORT, subtitle: 'Daily sale by service' },
  { path: '/UserWiseServiceSellReport', title: 'Daily Sale Comparison', kind: 'txn', txn: USER_SERVICE_SELL, subtitle: 'Yesterday vs today sale, per user' },
  { path: '/PayoutBeneficiaryReport', title: 'Payout Beneficiaries', kind: 'txn', txn: PAYOUT_BENEFICIARY, subtitle: 'Bank beneficiaries registered for payouts' },
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
  { path: '/DownlineReport', title: 'Downline Network', kind: 'txn', txn: DOWNLINE_REPORT, subtitle: 'Your downline network + balances' },
  { path: '/Performance', title: 'Top Performers', kind: 'txn', txn: TOP_PERFORMANCE, subtitle: 'Top-selling users' },
  { path: '/Users/UserAlias', title: 'User Aliases', kind: 'custom', component: 'userAlias', subtitle: 'The code each user type stamps on its users, and the sequence it counts' },
  { path: '/Users/DatewiseReport', title: 'New User Report', kind: 'txn', txn: NEWLY_ADDED_USERS, subtitle: 'Users added by date' },
  { path: '/Users/DeletedUsers', title: 'Deleted Users', kind: 'custom', component: 'deletedUsers', subtitle: 'Removed accounts, kept on record' },
  form('/Users/UserTransfer', 'User Transfer', 'Users/TransferUsers', USER_TRANSFER_FIELDS, {
    subtitle: 'Move a user under a new parent', submitLabel: 'Transfer user', successMessage: 'User transferred.',
  }),
  { path: '/Users/UserWiseBulkAction', title: 'Bulk User Actions', kind: 'custom', component: 'bulkAction',
    subtitle: 'Shift users under a new parent, or activate / deactivate / hold commission for many at once' },
  // Every change made to a user record — one row per field, with who, why and from which IP.
  { path: '/Users/ChangeHistory', title: 'User Audit Trail', kind: 'txn', txn: USER_CHANGE_HISTORY,
    subtitle: 'Who changed what on a user, when, why and from where' },
  list('/IDFYProfile', 'IDFY Profiles', 'IdfyOnboarding/GetAll'),
  list('/MISInterestedAgents', 'Onboarding Leads', 'LeadGenerationMaster/GetAll', 'Interested-agent leads'),

  // Slab
  { path: '/Slabs/Assign', title: 'Assign Slab', kind: 'custom', component: 'slabAssign', subtitle: 'Assign commission slabs to users' },
  // Hierarchy delegation: a super-distributor or distributor cutting its own rate cards out of
  // the one it was assigned. Both are menu entries of their own (MenuService, roles 1/2/3/6), so
  // neither needs a `parent` or a `roles` list here.
  {
    path: '/Slabs/Delegation',
    title: 'My Commission Slabs',
    kind: 'custom',
    component: 'slabDelegation',
    subtitle: 'Create and assign commission slabs for your own network',
  },
  {
    path: '/Slabs/Audit',
    title: 'Commission Audit Trail',
    kind: 'custom',
    component: 'slabAudit',
    subtitle: 'Every commission change, who made it and the limit it was measured against',
  },

  // Operators
  form('/Operators/Add', 'Add Operator', 'Operator/Create', OPERATOR_CREATE, {
    subtitle: 'Create a new operator', submitLabel: 'Create operator', successMessage: 'Operator created.',
  }),

  // API
  { path: '/UserwiseSequenceIndex', title: 'User-wise Routing', kind: 'custom', component: 'userwiseSequence', subtitle: 'Per-user provider sequence overrides' },
  {
    path: '/UserWiseDMTSequenceAdd',
    title: 'User-wise DMT Routing',
    kind: 'crud',
    subtitle: 'Route one user’s money transfers through a specific provider',
    crud: {
      idField: 'UserwiseDMTMappingID',
      list: { endpoint: 'UserwiseDMTSequence/Select' },
      // Names, not ids — the endpoint now returns them alongside.
      columns: [
        { key: 'UserName', label: 'User' },
        { key: 'FirmName', label: 'Firm' },
        { key: 'MobileNumber', label: 'Mobile' },
        { key: 'ServiceName', label: 'Service' },
        { key: 'OperatorName', label: 'Operator' },
        { key: 'ProviderName', label: 'Routed through' },
        { key: 'UpdatedateTime', label: 'Updated' },
      ],
      // Insert is an upsert: the backend clears any existing rule for the same user + operator
      // before writing, so re-adding a pair replaces it rather than creating a duplicate route.
      create: {
        endpoint: 'UserwiseDMTSequence/Insert',
        fields: [
          USER_SELECT('UserID', 'User'),
          {
            name: 'ServiceID', label: 'Service', type: 'select', required: true,
            optionsEndpoint: 'Service/GetAll', optionsPayload: LIST_PAGE,
            optionValue: 'ServiceID', optionLabel: 'Name',
          },
          {
            name: 'OperatorID', label: 'Operator', type: 'select', required: true,
            optionsEndpoint: 'Operator/GetAll', optionsPayload: ADMIN_LIST_PAGE,
            dependsOn: 'ServiceID', optionsPayloadKey: 'ServiceID',
            optionValue: 'OperatorID', optionLabel: 'OperatorName',
          },
          {
            name: 'ProviderID', label: 'Route through provider', type: 'select', required: true,
            optionsEndpoint: 'ProviderMaster/GetAll', optionsPayload: GENERIC_LIST,
            optionValue: 'ProviderID', optionLabel: 'ProviderName',
            hint: 'This user’s transfers for the chosen operator go out through this provider',
          },
        ],
        successMessage: 'Sequence saved.',
      },
      remove: { endpoint: 'UserwiseDMTSequence/DeleteByUserID', successMessage: 'Sequence removed.' },
    },
  },
  { path: '/ProvidersSequence', title: 'Provider Routing', kind: 'custom', component: 'providerSequence', subtitle: 'Operator-wise provider switching priority' },
  {
    path: '/RangeWiseSequence',
    title: 'DMT Range Routing',
    kind: 'crud',
    subtitle: 'Send transfers through a different provider depending on the amount',
    crud: {
      idField: 'RangeWiseSequenceID',
      list: { endpoint: 'RangeWiseSequence/GetAll' },
      columns: [
        { key: 'ServiceName', label: 'Service' },
        { key: 'OperatorName', label: 'Operator' },
        { key: 'FromAmount', label: 'From (₹)' },
        { key: 'ToAmount', label: 'To (₹)' },
        { key: 'ProviderName', label: 'Routed through' },
        { key: 'UpdatedateTime', label: 'Updated' },
      ],
      create: {
        endpoint: 'RangeWiseSequence/Create',
        fields: [
          {
            name: 'ServiceID', label: 'Service', type: 'select', required: true,
            optionsEndpoint: 'Service/GetAll', optionsPayload: LIST_PAGE,
            optionValue: 'ServiceID', optionLabel: 'Name',
            // DMT only: this is the DMT range routing, so the recharge, BBPS and AEPS services
            // (and their operators, which cascade from here) are not offered.
            optionsFilter: { keys: ['Name', 'ServiceCategory', 'ReportKind'], pattern: 'dmt|money ?transfer' },
            hint: 'Money transfer only',
          },
          {
            name: 'OperatorID', label: 'Operator', type: 'select', required: true,
            optionsEndpoint: 'Operator/GetAll', optionsPayload: ADMIN_LIST_PAGE,
            dependsOn: 'ServiceID', optionsPayloadKey: 'ServiceID',
            optionValue: 'OperatorID', optionLabel: 'OperatorName',
          },
          { name: 'FromAmount', label: 'From amount (₹)', type: 'number', required: true },
          { name: 'ToAmount', label: 'To amount (₹)', type: 'number', required: true },
          {
            name: 'ProviderID', label: 'Route through provider', type: 'select', required: true,
            optionsEndpoint: 'ProviderMaster/GetAll', optionsPayload: GENERIC_LIST,
            optionValue: 'ProviderID', optionLabel: 'ProviderName', colSpan: 2,
            hint: 'Ranges for one operator may not overlap — the server rejects a clash',
          },
        ],
        successMessage: 'Range added.',
      },
      update: {
        endpoint: 'RangeWiseSequence/Update',
        mode: 'object',
        fields: [
          {
            name: 'ServiceID', label: 'Service', type: 'select', required: true,
            optionsEndpoint: 'Service/GetAll', optionsPayload: LIST_PAGE,
            optionValue: 'ServiceID', optionLabel: 'Name',
            optionsFilter: { keys: ['Name', 'ServiceCategory', 'ReportKind'], pattern: 'dmt|money ?transfer' },
          },
          {
            name: 'OperatorID', label: 'Operator', type: 'select', required: true,
            optionsEndpoint: 'Operator/GetAll', optionsPayload: ADMIN_LIST_PAGE,
            optionValue: 'OperatorID', optionLabel: 'OperatorName',
          },
          { name: 'FromAmount', label: 'From amount (₹)', type: 'number', required: true },
          { name: 'ToAmount', label: 'To amount (₹)', type: 'number', required: true },
          {
            name: 'ProviderID', label: 'Route through provider', type: 'select', required: true,
            optionsEndpoint: 'ProviderMaster/GetAll', optionsPayload: GENERIC_LIST,
            optionValue: 'ProviderID', optionLabel: 'ProviderName', colSpan: 2,
          },
        ],
        successMessage: 'Range updated.',
      },
      remove: { endpoint: 'RangeWiseSequence/Delete', successMessage: 'Range deleted.' },
    },
  },
  // Setting
  list('/SupportAccess', 'Support Access', 'SupportUser/GetAll', 'Support-user access'),
  // App Alert — a bespoke composer, not a generic form. The generic version's single
  // "Audience" dropdown had no default, and a blank audience meant "everybody on the
  // platform"; see AlertComposerScreen.
  {
    path: '/PushNotifications',
    title: 'Broadcast Alert',
    kind: 'custom',
    component: 'alertComposer',
    subtitle: 'Send an in-app alert and push notification',
  },

  // Service â€” serviceâ†”wallet mapping (which wallet a service debits from)
  {
    path: '/SeviceswiseWallet',
    title: 'Service Wallet Mapping',
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
  { path: '/ServiceSubscriptionCharge', title: 'Subscription Charges', kind: 'custom', component: 'serviceSubscription', subtitle: 'Subscribe to a plan to unlock its services' },

  // What a user is told when a provider answers in its own words. Admin/support only - these
  // rules decide what every user of the platform reads. Both paths resolve here so a menu row
  // added with either URL lands on the screen.
  { path: '/MessageRules', title: 'Response Messages', kind: 'custom', component: 'messageRules', subtitle: 'Rewrite provider messages before a user reads them', roles: [1, 6] },
  { path: '/ResponseMessages', title: 'Response Messages', kind: 'custom', component: 'messageRules', subtitle: 'Rewrite provider messages before a user reads them', roles: [1, 6] },

  // â”€â”€ Retailer transaction screens (other-role menus) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  // AEPS to Other Wallet — the ONE wallet movement this product offers. Free wallet-to-wallet
  // (pick any source, pick any destination) was deliberately dropped: what an operator needs is
  // for AEPS earnings to become spendable balance, not for a retailer to shuffle money between
  // Recharge and DMT at will. The server enforces the same rule, so the screen and the endpoint
  // agree. Both paths are registered so a menu row pointing at either one lands here.
  form('/WalletTransfer', 'AEPS to Other Wallet', 'WalletToWalletTransfer/AEPSToOther', AEPS_TRANSFER_FIELDS, {
    subtitle: 'Move your AEPS balance into another wallet', submitLabel: 'Transfer',
    successMessage: 'AEPS balance moved.',
    description: 'Move what you have earned in the AEPS wallet into the wallet you spend from — Recharge or DMT. The money stays in your account; only the wallet it sits in changes.',
  }),
  form('/WalletToWallet', 'AEPS to Other Wallet', 'WalletToWalletTransfer/AEPSToOther', AEPS_TRANSFER_FIELDS, {
    subtitle: 'Move your AEPS balance into another wallet', submitLabel: 'Transfer',
    successMessage: 'AEPS balance moved.',
    description: 'Move what you have earned in the AEPS wallet into the wallet you spend from — Recharge or DMT. The money stays in your account; only the wallet it sits in changes.',
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
  // There is deliberately NO distributor-tier copy of Userwise Bulk Action. Re-parenting,
  // deactivating and holding commission in bulk re-shapes the commission tree, so Users/BulkUpdate
  // is Admin/Support-only server-side — the SD/Distributor screen that used to live here could
  // only ever 403, which is a worse answer than not offering it.
  { path: '/RetailerSearch', title: 'Quick Search', kind: 'txn', txn: RETAILER_SEARCH_REPORT, subtitle: 'Search a transaction by consumer number' },
  { path: '/AEPSIndex', title: 'AEPS â€” Cash Withdrawal', kind: 'custom', component: 'aeps', subtitle: 'Aadhaar Enabled Payment System' },
];

const SCREEN_MAP: Record<string, ScreenDef> = Object.fromEntries(
  SCREENS.map((s) => [s.path.toLowerCase(), s]),
);

export function findScreen(path: string): ScreenDef | undefined {
  return SCREEN_MAP[path.toLowerCase()];
}
