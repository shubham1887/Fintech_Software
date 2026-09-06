import { apiPost } from '@/lib/apiClient';

/** POST Auth/ForgotPasswordGenerateOTP — send a reset OTP to a registered mobile. */
export function forgotPasswordGenerateOtp(mobileNumber: string) {
  return apiPost<string>('Auth/ForgotPasswordGenerateOTP', { MobileNumber: mobileNumber }, { auth: false });
}

/** POST Auth/ForgotPasswordVerifyOTP — verify OTP; a temporary password is SMSed on success. */
export function forgotPasswordVerifyOtp(mobileNumber: string, otp: string) {
  return apiPost<string>('Auth/ForgotPasswordVerifyOTP', { MobileNumber: mobileNumber, OTP: otp }, { auth: false });
}
