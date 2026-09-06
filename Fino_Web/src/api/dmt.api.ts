import { apiPost } from '@/lib/apiClient';
import { fetchList, type ListRow } from './list.api';

/**
 * Domestic Money Transfer (DMT) API layer — mirrors the legacy YesBankDMT flow, retargeted at
 * the Fino_API local-DB endpoints:
 *
 *   1. DMTSender/Select      → is this mobile a registered + OTP-verified sender?
 *   2. DMTSender/Add         → register a new sender (may return "VERIFY_OTP")
 *   3. DMTSender/VerifyOTP   → confirm the sender's OTP
 *   4. DMTBeneficiary/Select → the sender's saved beneficiaries (name + bank details)
 *   5. DMTBeneficiary/Add    → add a beneficiary (may require OTP)
 *   6. DMTBeneficiary/VerifyOTP / CheckBranch / Delete
 *   7. YesBankDMT/MoneyTransfer → the actual money-out (shared bank-transfer pipeline, ServiceID 10)
 */

/** DMT service id used by the money-out pipeline (BankTransferService.ServiceID_DMT). */
export const DMT_SERVICE_ID = 10;

// ---- Sender -----------------------------------------------------------------

/** Registered + verified sender, as returned by DMTSender/Select. */
export interface SenderDetails {
  SenderGUID: string;
  Name: string;
  Mobile: string;
  Address: string;
  Consumed: number;
  RemainingLimit: number;
  /** Admin per-transfer cap from Key Setting 'ShowLimitInDMT'. 0 ⇒ not set. */
  DisplayLimit: number;
}

/** Look a sender up by mobile. ok+data ⇒ registered & verified; !ok ⇒ message says why
 *  ("Sender not registered." / "Sender OTP not verified." / "KYC not approved."). */
export function selectSender(mobile: string) {
  return apiPost<SenderDetails>('DMTSender/Select', { Mobile: mobile, ProviderID: 0 });
}

/** Register a new sender. On success `data` is a plain marker string: "VERIFY_OTP" when an OTP
 *  was dispatched, otherwise a success message. */
export function addSender(p: { name: string; mobile: string; address: string; pinCode: string }) {
  return apiPost<string>('DMTSender/Add', {
    Name: p.name,
    Mobile: p.mobile,
    Address: p.address,
    PinCode: p.pinCode,
    ProviderID: 0,
  });
}

export function verifySenderOtp(mobile: string, otp: string) {
  return apiPost<string>('DMTSender/VerifyOTP', { Mobile: mobile, OTP: otp });
}

export function resendSenderOtp(mobile: string) {
  return apiPost<string>('DMTSender/ResendOTP', { Mobile: mobile });
}

// ---- Beneficiary ------------------------------------------------------------

/** A saved beneficiary with its bank details (DMTBeneficiary/Select rows). */
export interface BeneficiaryDetail {
  Name: string;
  BankName: string;
  AccountNo: string;
  IFSC: string;
  SenderGUID: string;
  BeneficiaryGUID: string;
  SenderMobile: string;
  IsVerified: boolean;
}

export function selectBeneficiaries(senderGuid: string) {
  return apiPost<BeneficiaryDetail[]>('DMTBeneficiary/Select', { SenderGUID: senderGuid, ProviderID: 0 });
}

/** Add a beneficiary. `data.Message` tells the caller whether an OTP step is pending. */
export interface AddBeneficiaryResult {
  BeneficiaryGUID: string;
  Message: string;
}
export function addBeneficiary(p: {
  senderGuid: string;
  senderMobile: string;
  name: string;
  accountNo: string;
  ifsc: string;
  bankName: string;
  isNameVerified?: boolean;
}) {
  return apiPost<AddBeneficiaryResult>('DMTBeneficiary/Add', {
    SenderGUID: p.senderGuid,
    SenderMobile: p.senderMobile,
    Name: p.name,
    AccountNo: p.accountNo,
    IFSC: p.ifsc,
    BankName: p.bankName,
    IsNameVerified: p.isNameVerified ?? false,
    ProviderID: 0,
  });
}

/** Result of a beneficiary account verification (penny-drop name enquiry). */
export interface VerifyAccountResult {
  Status: string;
  VerifiedName: string;
  BankName: string;
  Charge: number;
  ClosingBalance: number;
  Message: string;
}

/** Verify a beneficiary account through the active DMT gateway — returns the bank's account-holder
 *  name. Charges the retailer's DMT wallet a configurable fee (refunded automatically if it fails). */
export function verifyBeneficiaryAccount(p: {
  senderGuid: string;
  senderMobile: string;
  accountNo: string;
  ifsc: string;
  bankName: string;
  paymentType?: string;
}) {
  return apiPost<VerifyAccountResult>('DMTBeneficiary/VerifyAccount', {
    SenderGUID: p.senderGuid,
    SenderMobile: p.senderMobile,
    AccountNo: p.accountNo,
    IFSC: p.ifsc.toUpperCase(),
    BankName: p.bankName,
    PaymentType: p.paymentType ?? 'IMPS',
  });
}

export function verifyBeneficiaryOtp(beneficiaryGuid: string, otp: string) {
  return apiPost<string>('DMTBeneficiary/VerifyOTP', { BeneficiaryGUID: beneficiaryGuid, OTP: otp });
}

/** Dispatch a fresh OTP to the sender for an existing (unverified) beneficiary. */
export function resendBeneficiaryOtp(beneficiaryGuid: string) {
  return apiPost<string>('DMTBeneficiary/ResendOTP', { BeneficiaryGUID: beneficiaryGuid });
}

export function deleteBeneficiary(beneficiaryGuid: string) {
  return apiPost<string>('DMTBeneficiary/Delete', { BeneficiaryGUID: beneficiaryGuid });
}

// ---- Bank master (dropdown) -------------------------------------------------

/** A DMT bank for the add-beneficiary dropdown (name + its default IFSC). */
export interface DmtBankOption {
  DMTBankID: number;
  Name: string;
  DefaultIFSC: string;
}

/** The active DMT banks, used to populate the bank dropdown on the add-beneficiary form. */
export function fetchDmtBanks() {
  return apiPost<DmtBankOption[]>('DMTBank/GetDropdown');
}

/** Bank / branch details for an IFSC (Razorpay lookup, matched to a known DMT bank). */
export interface BranchDetails {
  Status: string;
  Branch: string;
  Address: string;
  BankName: string;
  IFSC: string;
}
export function checkBranch(ifsc: string) {
  return apiPost<BranchDetails>('DMTBeneficiary/CheckBranch', { IFSC: ifsc });
}

// ---- Transfer ---------------------------------------------------------------

export type PaymentType = 'IMPS' | 'NEFT';

/** Settlement result from the shared money-out pipeline. */
export interface TransferResult {
  Status: string; // SUCCESS / FAILED / PENDING
  Message: string;
  TransactionMasterID: number;
  AccountNo: string;
  IFSC: string;
  Amount: number;
  OpeningBalance: number;
  ClosingBalance: number;
  ProviderRefNo: string;
  DateTime: string;
}

export function moneyTransfer(p: {
  sender: SenderDetails;
  beneficiary: BeneficiaryDetail;
  amount: number;
  paymentType: PaymentType;
  remark?: string;
}) {
  return apiPost<TransferResult>('YesBankDMT/MoneyTransfer', {
    ServiceID: DMT_SERVICE_ID,
    OperatorID: 0,
    Amount: p.amount,
    AccountNo: p.beneficiary.AccountNo,
    IFSC: p.beneficiary.IFSC,
    BankName: p.beneficiary.BankName,
    BeneficiaryName: p.beneficiary.Name,
    Mobile: p.sender.Mobile,
    SenderID: p.sender.SenderGUID,
    BeneficiaryID: p.beneficiary.BeneficiaryGUID,
    PaymentType: p.paymentType,
    Remark: p.remark ?? '',
    Through: 'WEB',
  });
}

// ---- Recent DMT transactions ------------------------------------------------

/** Caller's recent transactions; the page filters these to the DMT service. */
export function fetchRecentTransactions(pageSize = 15): Promise<ListRow[]> {
  return fetchList('TransactionProcessing/Select', {
    Status: '',
    ServiceType: '',
    FromDate: '',
    ToDate: '',
    PageNumber: 1,
    PageSize: pageSize,
  });
}
