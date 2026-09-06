import { apiPost } from '@/lib/apiClient';

// ---- Public sign-up ("Create account") --------------------------------------

export interface StateOption {
  StateID: number;
  StateName: string;
}

export interface RoleOption {
  UserTypeID: number;
  UserType: string;
}

export interface PincodeDetails {
  Pincode: string;
  City: string;
  District: string;
  State: string;
  StateID: number;
}

export interface RegisterPayload {
  PersonName: string;
  FirmName: string;
  MobileNumber: string;
  WhatsAppNumber: string;
  EmailID: string;
  PANNo: string;
  AadharNo: string;
  GSTNo: string;
  Address: string;
  CityName: string;
  District: string;
  Pincode: string;
  StateID: number | null;
  UserTypeID: number;
  ReferralCode: string;
}

/** POST OutsideRegistration/SelectState — anonymous list of states. */
export function selectStates() {
  return apiPost<StateOption[]>('OutsideRegistration/SelectState', {}, { auth: false });
}

/** POST OutsideRegistration/SelectRoleForSignUp — anonymous list of roles. */
export function selectSignUpRoles(referralCode = '') {
  return apiPost<RoleOption[]>('OutsideRegistration/SelectRoleForSignUp', { ReferralCode: referralCode }, { auth: false });
}

/** POST OutsideRegistration/FetchDetailsForPincodeForOutSide — city/district/state for a pincode. */
export function fetchPincodeDetails(pincode: string) {
  return apiPost<PincodeDetails>('OutsideRegistration/FetchDetailsForPincodeForOutSide', { Pincode: pincode }, { auth: false });
}

/** POST OutsideRegistration/SignUpGenerateOTP — send an OTP to the (unregistered) mobile. */
export function signUpGenerateOtp(mobileNumber: string) {
  return apiPost<string>('OutsideRegistration/SignUpGenerateOTP', { MobileNumber: mobileNumber }, { auth: false });
}

/** POST OutsideRegistration/SignUpVerifyOTP — verify the sign-up OTP. */
export function signUpVerifyOtp(mobileNumber: string, otp: string) {
  return apiPost<string>('OutsideRegistration/SignUpVerifyOTP', { MobileNumber: mobileNumber, OTP: otp }, { auth: false });
}

/** POST OutsideRegistration/Insert — submit the registration for admin approval. */
export function registerAccount(payload: RegisterPayload) {
  return apiPost<string>('OutsideRegistration/Insert', payload, { auth: false });
}

// ---- Admin approval queue ---------------------------------------------------

export interface PendingRegistration {
  PendingRegistrationID: number;
  PersonName: string;
  FirmName: string;
  MobileNumber: string;
  WhatsAppNumber: string;
  EmailID: string;
  PANNo: string;
  AadharNo: string;
  GSTNo: string;
  Address: string;
  CityName: string;
  District: string;
  Pincode: string;
  StateID: number | null;
  StateName: string;
  UserTypeID: number;
  UserTypeName: string;
  ReferralCode: string;
  Status: string;
  RejectReason: string;
  IsMobileVerified: boolean;
  CreatedUserID: number | null;
  DateTime: string;
}

/** POST PendingRegistration/GetAll — admin: full registration queue. */
export function fetchPendingRegistrations() {
  return apiPost<PendingRegistration[]>('PendingRegistration/GetAll', {});
}

/** POST PendingRegistration/Approve — admin: approve → creates the user + SMSes credentials. */
export function approveRegistration(id: number) {
  return apiPost<string>('PendingRegistration/Approve', { PendingRegistrationID: id });
}

/** POST PendingRegistration/Reject — admin: reject with a reason. */
export function rejectRegistration(id: number, reason: string) {
  return apiPost<string>('PendingRegistration/Reject', { PendingRegistrationID: id, RejectReason: reason });
}
