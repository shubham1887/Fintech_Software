import { apiPost } from '@/lib/apiClient';
import type { LoginResult } from '@/types/auth';

export interface LoginCredentials {
  mobileNumber: string;
  password: string;
}

/** POST api/Auth/login — anonymous; returns encrypted { user, tokens }. */
export function login(creds: LoginCredentials) {
  return apiPost<LoginResult>(
    'Auth/login',
    {
      MobileNumber: creds.mobileNumber,
      Password: creds.password,
      DeviceType: 'Web',
      IPAddress: '',
    },
    { auth: false },
  );
}

/** POST api/Auth/logout — revokes refresh tokens server-side. */
export function logout() {
  return apiPost('Auth/logout', {});
}
