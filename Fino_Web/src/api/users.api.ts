import { apiPost } from '@/lib/apiClient';

/**
 * The user-lifecycle calls the admin screens share. Users/Select is the one list endpoint
 * behind All Users, Inactive Users and Deleted Users — each screen just asks it a narrower
 * question, so the three can never disagree about who is who.
 */

/** One wallet's balance on a user row (wallets are dynamic — WalletMaster drives the columns). */
export interface UserWalletBalance {
  WalletID: number;
  WalletName: string;
  Balance: number;
  HoldBalance: number;
}

/** Mirrors Fino_API `Responses/User/UserDetailViewModel.cs`. */
export interface UserRow {
  UserMasterID: number;
  FirmName: string;
  PersonName: string;
  MobileNumber: string;
  WhatsAppNumber: string;
  EmailID: string;
  UserType: string;
  UserTypeID: number;
  ParentID: number | null;
  ParentName: string;
  ParentMobile: string;
  Address: string;
  CityName: string;
  Pincode: string;
  StateName: string;
  GSTNo: string;
  PANNo: string;
  AadharNo: string;
  ShopAddress: string;
  AliasName: string;
  UniqueReferenceNo: string;
  IsActive: boolean;
  IsDeleted: boolean;
  IsKYCApproved: boolean;
  IseKYC: boolean;
  /** Derived — Approved / Pending / Rejected / Not submitted. */
  KYCStatus: string;
  IsCommissionHold: boolean;
  IsTwoFA: boolean;
  IsLoginOTP: boolean;
  Balance: number;
  HoldBalance: number;
  WalletBalances: UserWalletBalance[];
  SlabName: string;
  /** Registered on. */
  DOC: string;
  /** Last written — for an inactive or deleted user, when that happened. */
  LastUpdated: string;
}

export interface UserListFilter {
  /** true = active only, false = inactive only, undefined = both. */
  IsActive?: boolean;
  /** Return ONLY soft-deleted rows. */
  OnlyDeleted?: boolean;
  ShowDeleted?: boolean;
  UserTypeID?: number;
  KYCStatus?: string;
  Search?: string;
  TakeCount?: number;
  SkipCount?: number;
}

interface UserListWrapper {
  Count: number;
  data: UserRow[];
}

/**
 * POST Users/Select. The API answers "no record found" as a business-level FAIL rather than an
 * empty success, so an empty result is normalised to [] here instead of surfacing as an error.
 */
export async function fetchUsers(filter: UserListFilter = {}): Promise<UserRow[]> {
  const res = await apiPost<UserListWrapper>('Users/Select', {
    TakeCount: 1000,
    SkipCount: 0,
    ...filter,
  });
  if (!res.ok || !res.data) return [];
  return Array.isArray(res.data.data) ? res.data.data : [];
}

/** POST Users/Activate — switch an account on or off. Admin only (enforced server-side). */
export function setUserActive(targetUserID: number, isActive: boolean) {
  return apiPost<string>('Users/Activate', { TargetUserID: targetUserID, IsActive: isActive });
}

/** POST Users/Delete — soft delete. Refused while the user holds a balance or has a downline. */
export function deleteUser(targetUserID: number) {
  return apiPost<string>('Users/Delete', { TargetUserID: targetUserID });
}

/** POST Users/Restore — undo a soft delete. The user comes back deactivated. */
export function restoreUser(targetUserID: number) {
  return apiPost<string>('Users/Restore', { TargetUserID: targetUserID });
}
