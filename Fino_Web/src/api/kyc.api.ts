import { apiPost } from '@/lib/apiClient';

/**
 * The admin KYC desk (Fino_API `KycApprovalController`). Every endpoint here is Admin/Support
 * only — the API denies the rest by default, and the decision endpoints assert the administrator
 * a second time.
 */

export const KYC_STATUS = {
  approved: 'Approved',
  pending: 'Pending',
  rejected: 'Rejected',
  notSubmitted: 'Not submitted',
} as const;

export interface KycUserRow {
  UserID: number;
  PersonName: string;
  FirmName: string;
  MobileNumber: string;
  WhatsAppNumber: string;
  EmailID: string;
  UserTypeID: number;
  UserType: string;
  ParentName: string;
  CityName: string;
  StateName: string;
  /** Approved / Pending / Rejected / Not submitted. */
  Status: string;
  /** "E-KYC" or "Manual". */
  Mode: string;
  IseKYC: boolean;
  IsKYCApproved: boolean;
  IsActive: boolean;
  RequiredCount: number;
  SubmittedCount: number;
  ApprovedCount: number;
  RejectedCount: number;
  InReviewCount: number;
  RegisteredOn: string;
  LastSubmittedOn: string | null;
  ApprovedOn: string | null;
  ApprovedBy: string;
  Remark: string;
}

export interface KycDocumentRow {
  /** 0 until the user submits — the row still shows, as an outstanding requirement. */
  UserDocumentID: number;
  VerificationDocumentConfigID: number;
  DocumentName: string;
  DocumentCode: string;
  DocumentType: string;
  IsRequired: boolean;
  DisplayOrder: number;
  DocumentNo: string;
  ImageURL: string;
  /** APPROVE / REJECT / PROCESS, or empty when nothing was submitted. */
  Status: string;
  Remark: string;
  SubmittedOn: string | null;
}

export interface KycEkycEntry {
  DocumentType: string;
  DocumentNo: string;
  DateTime: string;
}

export interface KycUserDetail extends KycUserRow {
  PANNo: string;
  AadharNo: string;
  GSTNo: string;
  Address: string;
  ShopAddress: string;
  Pincode: string;
  UniqueReferenceNo: string;
  Documents: KycDocumentRow[];
  EkycHistory: KycEkycEntry[];
}

export interface KycSummary {
  Total: number;
  Approved: number;
  Pending: number;
  Rejected: number;
  NotSubmitted: number;
  EkycVerified: number;
}

export interface KycUserListResponse {
  Summary: KycSummary;
  Users: KycUserRow[];
}

export interface KycPolicyService {
  ServiceID: number;
  Name: string;
  ServiceCategory: string;
  IsActive: boolean;
  IsAllowedWithoutKYC: boolean;
}

export interface KycPolicy {
  IsKYCEnforced: boolean;
  /** Key Setting "eVerification" — E-KYC mode rather than manual documents. */
  EVerification: boolean;
  /** False when nothing is configured to verify, so the gate cannot bite yet. */
  HasRequiredDocuments: boolean;
  Services: KycPolicyService[];
}

const EMPTY: KycUserListResponse = {
  Summary: { Total: 0, Approved: 0, Pending: 0, Rejected: 0, NotSubmitted: 0, EkycVerified: 0 },
  Users: [],
};

/** POST KycApproval/SelectUsers — the review queue plus the headline counts. */
export async function fetchKycUsers(filter: { Status?: string; Search?: string; UserTypeID?: number } = {}) {
  const res = await apiPost<KycUserListResponse>('KycApproval/SelectUsers', filter);
  return res.ok && res.data ? res.data : EMPTY;
}

/** POST KycApproval/SelectUser — one user's full KYC file. */
export async function fetchKycUser(userID: number) {
  const res = await apiPost<KycUserDetail>('KycApproval/SelectUser', { UserID: userID });
  return res.ok ? res.data : null;
}

/** POST KycApproval/ReviewDocument — approve or reject one document. */
export function reviewKycDocument(userDocumentID: number, isApproved: boolean, remark: string) {
  return apiPost<string>('KycApproval/ReviewDocument', {
    UserDocumentID: userDocumentID,
    IsApproved: isApproved,
    Remark: remark,
  });
}

/** POST KycApproval/ReviewUser — approve or reject the whole KYC in one action. */
export function reviewKycUser(userID: number, isApproved: boolean, remark: string) {
  return apiPost<string>('KycApproval/ReviewUser', {
    UserID: userID,
    IsApproved: isApproved,
    Remark: remark,
  });
}

/** POST KycApproval/SelectPolicy — the gate switch + per-service exemptions. */
export async function fetchKycPolicy() {
  const res = await apiPost<KycPolicy>('KycApproval/SelectPolicy', {});
  return res.ok ? res.data : null;
}

/** POST KycApproval/SavePolicy. */
export function saveKycPolicy(isEnforced: boolean, allowedServiceIDs: number[]) {
  return apiPost<string>('KycApproval/SavePolicy', {
    IsKYCEnforced: isEnforced,
    ServicesAllowedWithoutKYC: allowedServiceIDs,
  });
}
