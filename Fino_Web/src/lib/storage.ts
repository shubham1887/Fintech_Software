import type { Session, Tokens } from '@/types/auth';

const SESSION_KEY = 'fino.session';

/**
 * Which user THIS TAB was built for, kept in sessionStorage so it is per-tab rather than shared.
 *
 * The session itself lives in localStorage, which every tab on the origin shares. That is what
 * lets a second tab stay signed in — and what let it keep working as somebody else: signing in as
 * another user in one tab replaced the shared token, and the tab still showing the previous user's
 * name, menu and screens went on calling the API with the new user's token. The reply was the new
 * user's data, so the balance changed while the identity on screen did not.
 *
 * Pinning the identity per tab makes that detectable: the tab can compare who it is rendering for
 * against who the stored session now belongs to, and stop.
 */
const TAB_USER_KEY = 'fino.tab.user';

export function loadSession(): Session | null {
  try {
    const raw = localStorage.getItem(SESSION_KEY);
    return raw ? (JSON.parse(raw) as Session) : null;
  } catch {
    return null;
  }
}

export function saveSession(session: Session): void {
  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export function patchTokens(tokens: Tokens): Session | null {
  const current = loadSession();
  if (!current) return null;
  const next: Session = { ...current, tokens };
  saveSession(next);
  return next;
}

export function clearSession(): void {
  localStorage.removeItem(SESSION_KEY);
}

/** The localStorage key the session lives under, for the cross-tab `storage` listener. */
export const SESSION_STORAGE_KEY = SESSION_KEY;

/** Record which user this tab is rendering for. Called whenever a session is established. */
export function pinTabUser(userId: number): void {
  try {
    sessionStorage.setItem(TAB_USER_KEY, String(userId));
  } catch {
    /* private mode / blocked storage — the guard below then simply never fires */
  }
}

/** Who this tab was built for, or null when it has not been pinned (a fresh tab). */
export function pinnedTabUser(): number | null {
  try {
    const raw = sessionStorage.getItem(TAB_USER_KEY);
    if (!raw) return null;
    const id = Number(raw);
    return Number.isFinite(id) && id > 0 ? id : null;
  } catch {
    return null;
  }
}

export function clearPinnedTabUser(): void {
  try {
    sessionStorage.removeItem(TAB_USER_KEY);
  } catch {
    /* as above */
  }
}

/**
 * True when the stored session belongs to a DIFFERENT user than the one this tab is rendering.
 *
 * Only ever true when both are known, so a fresh tab (nothing pinned yet) and a signed-out tab
 * (no session) are both left alone.
 */
export function tabIdentityDrifted(): boolean {
  const pinned = pinnedTabUser();
  if (pinned == null) return false;
  const stored = loadSession()?.user?.UserID;
  if (stored == null) return false;
  return Number(stored) !== pinned;
}
