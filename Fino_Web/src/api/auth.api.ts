import { apiPost } from '@/lib/apiClient';
import type { LoginResult } from '@/types/auth';

export interface LoginCredentials {
  mobileNumber: string;
  password: string;
}

/**
 * Where this sign-in is happening, if the browser will say so without being asked.
 *
 * The Login History report shows a latitude, a longitude and a place for every sign-in, and on the
 * web that figure can only come from the browser. Two rules make this safe to call on the login
 * screen: permission is never REQUESTED (`maximumAge` lets a previously-granted, already-cached
 * position answer instantly, and a browser that would have to prompt simply times out), and the
 * whole thing is bounded — after three seconds the sign-in goes ahead with no location rather than
 * waiting on a dialog nobody is looking at. A denial, an insecure origin, or a browser with no
 * geolocation at all all land in the same place: blank fields, and a login that works.
 */
async function currentPosition(): Promise<{ Latitude: string; Longitude: string }> {
  const blank = { Latitude: '', Longitude: '' };
  if (typeof navigator === 'undefined' || !navigator.geolocation) return blank;
  try {
    return await new Promise((resolve) => {
      // Long enough for the browser's own permission prompt to be answered: three seconds was
      // shorter than the time it takes to read the prompt, so a first sign-in on any machine
      // logged no location even when the person pressed Allow.
      const done = setTimeout(() => resolve(blank), 12_000);
      navigator.geolocation.getCurrentPosition(
        (p) => {
          clearTimeout(done);
          resolve({
            Latitude: p.coords.latitude.toFixed(6),
            Longitude: p.coords.longitude.toFixed(6),
          });
        },
        () => { clearTimeout(done); resolve(blank); },
        { enableHighAccuracy: false, timeout: 11_000, maximumAge: 10 * 60_000 },
      );
    });
  } catch {
    return blank;
  }
}

/** POST api/Auth/login — anonymous; returns encrypted { user, tokens }. */
export async function login(creds: LoginCredentials) {
  return apiPost<LoginResult>(
    'Auth/login',
    {
      MobileNumber: creds.mobileNumber,
      Password: creds.password,
      DeviceType: 'Web',
      IPAddress: '',
      ...(await currentPosition()),
    },
    { auth: false },
  );
}

/**
 * POST api/Auth/login-2fa — anonymous; the second step of a two-step sign-in.
 *
 * The password goes up again alongside the code because the server keeps no half-built session
 * between the two calls: there is nothing pending to steal and nothing to expire.
 */
export async function loginTwoFactor(input: LoginCredentials & { code: string }) {
  return apiPost<LoginResult>(
    'Auth/login-2fa',
    {
      MobileNumber: input.mobileNumber,
      Password: input.password,
      Code: input.code,
      DeviceType: 'Web',
      IPAddress: '',
      ...(await currentPosition()),
    },
    { auth: false },
  );
}

/**
 * POST api/Auth/switch-profile — continue as the other role this number holds.
 *
 * Authenticated: the person is already signed in as their primary account. The server checks
 * the target really is one of their own linked logins and issues a fresh token pair for it.
 */
export function switchProfile(userId: number) {
  return apiPost<LoginResult>('Auth/SwitchProfile', { UserID: userId });
}

/** POST api/Auth/logout — revokes refresh tokens server-side. */
export function logout() {
  return apiPost('Auth/logout', {});
}
