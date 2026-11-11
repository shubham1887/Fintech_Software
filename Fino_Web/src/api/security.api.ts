import { apiPost } from '@/lib/apiClient';

/** Voluntary password change — reuses the backend's old→new change endpoint. */
export function changePassword(input: { OldPassword: string; NewPassword: string; ConfirmPassword: string }) {
  return apiPost('Credential/InitialChangePassword', input);
}

export interface MpinStatus {
  Required: boolean;
  IsSet: boolean;
  GlobalEnabled: boolean;
  UserEnabled: boolean;
}

/** Whether the signed-in user must use an MPIN, and whether they have set one yet. */
export async function getMpinStatus(): Promise<MpinStatus> {
  const res = await apiPost<MpinStatus>('Credential/MpinStatus', {});
  return res.data ?? { Required: false, IsSet: false, GlobalEnabled: false, UserEnabled: false };
}

/** Set or change the MPIN. `OldMpin` is required by the server only when one already exists. */
export function setMpin(input: { OldMpin?: string; NewMpin: string; ConfirmMpin: string; Enable?: boolean }) {
  return apiPost('Credential/SetMpin', { Enable: true, OldMpin: '', ...input });
}

/** Verify the MPIN (app open / a transaction). */
export function verifyMpin(mpin: string) {
  return apiPost('Credential/VerifyMpin', { Mpin: mpin });
}
