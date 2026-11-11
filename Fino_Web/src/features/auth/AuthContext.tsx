import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { useNavigate } from 'react-router-dom';
import {
  login as apiLogin,
  loginTwoFactor as apiLoginTwoFactor,
  logout as apiLogout,
  switchProfile as apiSwitchProfile,
  type LoginCredentials,
} from '@/api/auth.api';
import { SESSION_EXPIRED_EVENT, SESSION_SWITCHED_EVENT } from '@/lib/apiClient';
import { queryClient } from '@/lib/queryClient';
import { forgetPushToken, unregisterThisDevice } from '@/lib/push';
import {
  SESSION_STORAGE_KEY, clearPinnedTabUser, clearSession, loadSession, pinTabUser, pinnedTabUser,
  saveSession,
} from '@/lib/storage';
import type { LoginProfileOption, LoginUser, Session, Tokens } from '@/types/auth';

/**
 * Outcome of a sign-in attempt. `twoFactorRequired` is a THIRD state, distinct from both success
 * and failure: the password was accepted but no session exists yet, so the caller must collect an
 * authenticator code and call `completeTwoFactor`.
 */
export interface SignInResult {
  ok: boolean;
  message: string;
  twoFactorRequired?: boolean;
  /**
   * The person holds more than one login on this number, so the caller must ask which one to
   * continue as before going anywhere. This is NOT a fourth failure state — the session is
   * already established as the primary account, which is a perfectly valid place to land; it
   * only means the choice has not been offered yet.
   */
  chooseProfile?: LoginProfileOption[];
}

/**
 * How long signing out waits for the server before clearing the session locally anyway.
 *
 * Long enough that the push-token removal and the server-side logout normally both complete
 * (they are two quick POSTs), short enough that a cold API or a weak connection never leaves
 * someone staring at a Logout button that looks broken. See the note in `signOut`.
 */
const SIGN_OUT_SERVER_DEADLINE_MS = 1_500;

interface AuthState {
  user: LoginUser | null;
  isAuthenticated: boolean;
  signIn: (creds: LoginCredentials) => Promise<SignInResult>;
  /** Second step: the same credentials plus the authenticator (or recovery) code. */
  completeTwoFactor: (input: LoginCredentials & { code: string }) => Promise<SignInResult>;
  signOut: () => Promise<void>;
  /** Merge fresh fields into the stored session user (profile edit, KYC approval) so the
   *  topbar and greeting stop showing what was true at login. */
  updateUser: (patch: Partial<LoginUser>) => void;
  /** The logins this number holds. Length 0 or 1 = nothing to switch between. */
  profiles: LoginProfileOption[];
  /** Continue as another of this person's own logins. Re-issues the session for it. */
  switchProfile: (userId: number) => Promise<SignInResult>;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => loadSession());
  const navigate = useNavigate();

  /**
   * Store a freshly issued session. Shared by both sign-in paths so a session is only ever
   * established in one place.
   */
  const establishSession = useCallback((user: LoginUser, tokens: Tokens) => {
    // Drop any previous user's cached data (menu, wallets, dashboard, reports) so the new
    // role starts clean — otherwise the prior session's menu/balances leak into this one.
    queryClient.clear();
    const next: Session = { user, tokens };
    saveSession(next);
    // Record who this TAB is now rendering for. The session is shared across tabs; this is not,
    // and it is what lets every later request check it is still acting as the right person.
    pinTabUser(Number(user.UserID));
    setSession(next);
  }, []);

  const signIn = useCallback(async (creds: LoginCredentials): Promise<SignInResult> => {
    const res = await apiLogin(creds);

    if (res.ok && res.data?.tokens?.AccessToken && res.data.user) {
      establishSession(res.data.user, res.data.tokens);
      const options = res.data.user.Profiles ?? [];
      return options.length > 1
        ? { ok: true, message: res.message, chooseProfile: options }
        : { ok: true, message: res.message };
    }

    // Succeeded, but no token: the account wants an authenticator code. Reporting this as a
    // failure would tell the user their password was wrong when it was not.
    if (res.ok && res.data?.user?.IsTwoFA && !res.data.tokens) {
      return { ok: false, twoFactorRequired: true, message: res.message };
    }

    return { ok: false, message: res.message || 'Invalid mobile number or password.' };
  }, [establishSession]);

  const completeTwoFactor = useCallback(
    async (input: LoginCredentials & { code: string }): Promise<SignInResult> => {
      const res = await apiLoginTwoFactor(input);
      if (res.ok && res.data?.tokens?.AccessToken && res.data.user) {
        establishSession(res.data.user, res.data.tokens);
        const options = res.data.user.Profiles ?? [];
        return options.length > 1
          ? { ok: true, message: res.message, chooseProfile: options }
          : { ok: true, message: res.message };
      }
      return { ok: false, twoFactorRequired: true, message: res.message || 'That code is not correct.' };
    },
    [establishSession],
  );

  const signOut = useCallback(async () => {
    // Signing out must FEEL instant, and must happen even when the server cannot be reached.
    //
    // Both server-side calls still run, in this order and with the session token still valid:
    // the handset's push token is dropped first, so the next person to sign in on a shared
    // retail phone does not keep receiving the previous user's alerts, and only then is the
    // session invalidated. But the local clear below no longer waits indefinitely on them.
    //
    // Why the deadline: these are two sequential round trips to an API that may be cold, and
    // nothing visible happened until both returned — on a weak counter connection that is
    // several seconds of a dead-looking Logout button, and if a call hangs it was the full
    // axios timeout. Waiting is also the wrong trade-off at the end of it: a local session
    // that refuses to clear is strictly more dangerous than a refresh token the server has
    // not been told about, because the person walks away believing they are signed out.
    const serverSide = (async () => {
      await unregisterThisDevice();
      try {
        await apiLogout();
      } catch {
        /* best-effort; clear locally regardless */
      }
    })();

    await Promise.race([
      serverSide,
      new Promise<void>((resolve) => setTimeout(resolve, SIGN_OUT_SERVER_DEADLINE_MS)),
    ]);

    clearSession();
    clearPinnedTabUser();
    // Drop the per-tab "MPIN already entered this session" flag, so the next user to sign in on
    // this tab is gated afresh rather than inheriting the previous user's unlock.
    try { sessionStorage.removeItem('fino.mpin.verified'); } catch { /* private mode */ }
    setSession(null);
    queryClient.clear();
  }, []);

  /**
   * Work as another of this person's own logins.
   *
   * A full session replacement, not a view filter: the server mints a token for the other
   * account and everything cached under the previous one is dropped. Anything less would leave
   * the distributor's menu, wallet and reports on screen while the token underneath said
   * retailer — which is the subtle, dangerous version of this feature.
   */
  const switchProfile = useCallback(async (userId: number): Promise<SignInResult> => {
    const res = await apiSwitchProfile(userId);
    if (res.ok && res.data?.tokens?.AccessToken && res.data.user) {
      // The handset's push token is registered against whoever was signed in. Forgetting the
      // "already sent" marker makes the bridge re-register it for the role being switched to,
      // so alerts follow the account actually in use rather than the one left behind.
      forgetPushToken();
      establishSession(res.data.user, res.data.tokens);
      // Always land on the dashboard. The page you were on belonged to the role you just left —
      // a distributor's fund-transfer screen is not a retailer's, and staying there would hand
      // the new role a screen it cannot use (or, on a report, someone else's figures still on
      // the glass until the next fetch). The dashboard is the one page every role has.
      navigate('/dashboard', { replace: true });
      return { ok: true, message: res.message };
    }
    return { ok: false, message: res.message || 'Could not switch to that login.' };
  }, [establishSession, navigate]);

  const updateUser = useCallback((patch: Partial<LoginUser>) => {
    setSession((current) => {
      if (!current) return current;
      const next: Session = { ...current, user: { ...current.user, ...patch } };
      saveSession(next);
      return next;
    });
  }, []);

  // The api client dispatches this when a refresh fails (hard session expiry).
  useEffect(() => {
    const handler = () => {
      clearSession();
      clearPinnedTabUser();
      setSession(null);
      queryClient.clear();
    };
    window.addEventListener(SESSION_EXPIRED_EVENT, handler);
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, handler);
  }, []);

  /**
   * Keep every tab honest about who is signed in.
   *
   * The session lives in localStorage, which all tabs on the origin share, and React state does
   * not. So two tabs signed in as one user used to drift apart the moment either of them changed
   * the session:
   *
   *  * Signing out in tab A removed the shared session, and tab B went on showing the user's name,
   *    menu and balances - looking signed in, with no token behind it.
   *  * Signing in as a DIFFERENT user in tab A replaced the shared token, and tab B kept showing
   *    the first user while every request it made was answered for the second. The balance
   *    changed and nothing else did, which is the dangerous version: one person's screen over
   *    another person's money.
   *
   * A `storage` event fires in every OTHER tab when localStorage changes, which is exactly the
   * signal needed. Signed out elsewhere, this tab signs out too. Signed in as someone else, the
   * tab is reloaded rather than patched - the menu, the role gates and every open screen were
   * built for the previous user, and re-deriving all of it from a state update is how a stale
   * screen survives the switch.
   */
  useEffect(() => {
    /**
     * Take on the identity the shared session now carries, and rebuild the tab for it.
     *
     * The re-pin has to happen BEFORE the reload. sessionStorage survives a reload, so a tab that
     * reloaded while still pinned to the previous user would come back drifted, refuse its own
     * first request, and reload again — a loop that renders nothing. Pinning first makes the
     * reloaded tab consistent with the session it is about to read.
     */
    const adoptNewUser = (userId: number) => {
      pinTabUser(Number(userId));
      // The MPIN unlock belonged to the user who has just been replaced.
      try { sessionStorage.removeItem('fino.mpin.verified'); } catch { /* private mode */ }
      window.location.reload();
    };

    const onStorage = (event: StorageEvent) => {
      // key === null is localStorage.clear(); anything else is another key, not ours.
      if (event.key !== null && event.key !== SESSION_STORAGE_KEY) return;

      const next = loadSession();
      const pinned = pinnedTabUser();

      if (!next?.tokens?.AccessToken) {
        // Signed out in another tab.
        clearPinnedTabUser();
        setSession(null);
        queryClient.clear();
        try { sessionStorage.removeItem('fino.mpin.verified'); } catch { /* private mode */ }
        return;
      }

      if (pinned != null && Number(next.user.UserID) !== pinned) adoptNewUser(next.user.UserID);
    };

    // Dispatched by the api client when it catches the same drift on its way out, which beats the
    // storage event on a request already in flight.
    const onSwitched = () => {
      const current = loadSession();
      if (current?.user?.UserID != null) adoptNewUser(current.user.UserID);
    };

    window.addEventListener('storage', onStorage);
    window.addEventListener(SESSION_SWITCHED_EVENT, onSwitched);
    return () => {
      window.removeEventListener('storage', onStorage);
      window.removeEventListener(SESSION_SWITCHED_EVENT, onSwitched);
    };
  }, []);

  /**
   * Pin the tab to whoever the session says, on the way in.
   *
   * A tab opened while already signed in — duplicated, restored with the window, middle-clicked
   * into a second tab — has a session but no pin of its own, and those are exactly the tabs the
   * guard exists for.
   *
   * It pins unconditionally rather than only when unpinned, which also settles the one case that
   * could otherwise loop: a tab that reloads while still carrying a pin for the previous user.
   * At mount the identity is not in question — this tab is being BUILT for whoever the stored
   * session names — so adopting it here is always the right answer, and drift can only mean
   * something that happened after.
   */
  useEffect(() => {
    const userId = Number(session?.user?.UserID ?? 0);
    if (userId > 0 && pinnedTabUser() !== userId) pinTabUser(userId);
  }, [session]);

  const value = useMemo<AuthState>(
    () => ({
      user: session?.user ?? null,
      isAuthenticated: Boolean(session?.tokens?.AccessToken),
      signIn,
      completeTwoFactor,
      signOut,
      updateUser,
      profiles: session?.user?.Profiles ?? [],
      switchProfile,
    }),
    [session, signIn, completeTwoFactor, signOut, updateUser, switchProfile],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
}
