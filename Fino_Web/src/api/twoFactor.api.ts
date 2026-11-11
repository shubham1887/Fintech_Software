import { apiPost } from '@/lib/apiClient';

/** Mirrors Fino_API `Responses/TwoFactor/TwoFactorStatusResponse.cs`. */
export interface TwoFactorStatus {
  IsEnabled: boolean;
  /** Setup was started but never confirmed with a code — offer to resume. */
  IsPending: boolean;
  EnabledOn: string | null;
  RecoveryCodesRemaining: number;
}

/** Mirrors Fino_API `Responses/TwoFactor/TwoFactorSetupResponse.cs`. */
export interface TwoFactorSetup {
  /** The otpauth:// URI the QR encodes. Contains the shared secret — never log or forward it. */
  ProvisioningUri: string;
  /** The same secret grouped in fours, for typing in by hand. */
  ManualEntryKey: string;
  Account: string;
  Issuer: string;
}

/** Whether the signed-in user has an authenticator set up. */
export async function getTwoFactorStatus(): Promise<TwoFactorStatus> {
  const res = await apiPost<TwoFactorStatus>('TwoFactor/Status', {});
  return res.data ?? { IsEnabled: false, IsPending: false, EnabledOn: null, RecoveryCodesRemaining: 0 };
}

/**
 * Mint a secret and get the QR payload. Two-step verification is NOT on after this call —
 * `confirmTwoFactor` is what switches it on, which is why abandoning this screen is harmless.
 */
export function beginTwoFactorSetup() {
  return apiPost<TwoFactorSetup>('TwoFactor/BeginSetup', {});
}

/** Finish enrolment. The returned recovery codes are shown once and never again. */
export function confirmTwoFactor(code: string) {
  return apiPost<{ RecoveryCodes: string[] }>('TwoFactor/Confirm', { Code: code });
}

/** Switch two-step verification off. Needs the account password AND a live code. */
export function disableTwoFactor(input: { Password: string; Code: string }) {
  return apiPost('TwoFactor/Disable', input);
}

/** Admin only — clear another user's enrolment when they have lost the phone. */
export function clearTwoFactorForUser(userId: number) {
  return apiPost('TwoFactor/ClearForUser', { UserID: userId });
}
