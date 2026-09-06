import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { login as apiLogin, logout as apiLogout, type LoginCredentials } from '@/api/auth.api';
import { SESSION_EXPIRED_EVENT } from '@/lib/apiClient';
import { queryClient } from '@/lib/queryClient';
import { clearSession, loadSession, saveSession } from '@/lib/storage';
import type { LoginUser, Session } from '@/types/auth';

interface AuthState {
  user: LoginUser | null;
  isAuthenticated: boolean;
  signIn: (creds: LoginCredentials) => Promise<{ ok: boolean; message: string }>;
  signOut: () => Promise<void>;
  /** Merge fresh fields into the stored session user (profile edit, KYC approval) so the
   *  topbar and greeting stop showing what was true at login. */
  updateUser: (patch: Partial<LoginUser>) => void;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => loadSession());

  const signIn = useCallback(async (creds: LoginCredentials) => {
    const res = await apiLogin(creds);
    if (res.ok && res.data?.tokens?.AccessToken && res.data.user) {
      // Drop any previous user's cached data (menu, wallets, dashboard, reports) so the new
      // role starts clean — otherwise the prior session's menu/balances leak into this one.
      queryClient.clear();
      const next: Session = { user: res.data.user, tokens: res.data.tokens };
      saveSession(next);
      setSession(next);
      return { ok: true, message: res.message };
    }
    return { ok: false, message: res.message || 'Invalid mobile number or password.' };
  }, []);

  const signOut = useCallback(async () => {
    try {
      await apiLogout();
    } catch {
      /* best-effort; clear locally regardless */
    }
    clearSession();
    setSession(null);
    queryClient.clear();
  }, []);

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
      setSession(null);
      queryClient.clear();
    };
    window.addEventListener(SESSION_EXPIRED_EVENT, handler);
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, handler);
  }, []);

  const value = useMemo<AuthState>(
    () => ({
      user: session?.user ?? null,
      isAuthenticated: Boolean(session?.tokens?.AccessToken),
      signIn,
      signOut,
      updateUser,
    }),
    [session, signIn, signOut, updateUser],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
}
