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
    // 0 = every matching row (the service skips its paging for it). The user screens filter
    // and search in the browser over what they hold, so a cap here was not "the first 1000"
    // but "everything past the first 1000 is invisible to the filters and missing from the export".
    TakeCount: 0,
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
/**
 * POST Users/Delete — soft delete. `reason` is recorded against the user's change history with
 * the operator's name, the time and the server-observed IP, so a removed account can be
 * accounted for afterwards; the API keeps the field optional for older callers.
 */
export function deleteUser(targetUserID: number, reason = '') {
  return apiPost<string>('Users/Delete', { TargetUserID: targetUserID, Reason: reason });
}

/** POST Users/Restore — undo a soft delete. The user comes back deactivated. */
export function restoreUser(targetUserID: number) {
  return apiPost<string>('Users/Restore', { TargetUserID: targetUserID });
}

// ── One mobile number, two roles ────────────────────────────────────────────────
//
// A distributor who also runs a counter, a super-distributor who sells directly, an API
// client that needs a retailer login to test against — each needed a second mobile number
// before this, because sign-in is keyed on the number. The second login is a real account
// (own wallet, slab, ledger and user code) that shares the number and the password.
// See `Fino_API/StaticModels/LinkedProfilePolicy.cs` for who may set one up, and as what.

export interface LinkedProfile {
  UserID: number;
  UserTypeID: number;
  RoleName: string;
  FirmName: string;
  PersonName: string;
  UserCode: string;
  MobileNumber: string;
  /** The account sign-in authenticates against — the one that owns the pair. */
  IsPrimary: boolean;
  /** The one this session is currently working as. */
  IsCurrent: boolean;
  IsActive: boolean;
}

export interface LinkedProfiles {
  PrimaryUserID: number;
  CurrentUserID: number;
  Profiles: LinkedProfile[];
  /** The caller may add a second login right now. */
  CanSetUp: boolean;
  AllowedRoles: Array<{ UserTypeID: number; RoleName: string }>;
  /** Why they may not — printed as-is, so the card never reads as broken. */
  Reason: string;
}

const NO_PROFILES: LinkedProfiles = {
  PrimaryUserID: 0, CurrentUserID: 0, Profiles: [], CanSetUp: false, AllowedRoles: [], Reason: '',
};

/** POST Users/LinkedProfiles — the logins this person holds, and whether they may add one. */
export async function fetchLinkedProfiles(): Promise<LinkedProfiles> {
  const res = await apiPost<LinkedProfiles>('Users/LinkedProfiles', {});
  if (!res.ok || !res.data) return NO_PROFILES;
  return { ...NO_PROFILES, ...res.data };
}

/** POST Users/LinkedProfileAdd — set up the caller's own second login. */
export function addLinkedProfile(input: { userTypeId: number; firmName?: string }) {
  return apiPost<LinkedProfiles>('Users/LinkedProfileAdd', {
    UserTypeID: input.userTypeId,
    FirmName: input.firmName ?? '',
  });
}
