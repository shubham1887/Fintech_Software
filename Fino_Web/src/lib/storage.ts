import type { Session, Tokens } from '@/types/auth';

const SESSION_KEY = 'fino.session';

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
