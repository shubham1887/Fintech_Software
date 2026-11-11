import { useEffect, useRef, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { KeyRound, LogOut } from 'lucide-react';
import { getMpinStatus, verifyMpin } from '@/api/security.api';
import { useAuth } from '@/features/auth/AuthContext';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { BrandLockup, BrandLoading } from '@/components/ui/BrandMark';

/**
 * MPIN gate at app open.
 *
 * When the account requires an MPIN (the admin's platform-wide Key Setting, or the user's own
 * opt-in), this stands in front of the whole console once per browser session:
 *   - MPIN required but not set yet → send the user to set it (Account security).
 *   - MPIN required and set        → ask them to enter it before anything else renders.
 * Once verified it records a per-tab flag so navigating around does not re-prompt; closing the
 * tab (or signing out) clears it, so the next open asks again — which is the point of an
 * app-open PIN. When no MPIN is required, this is a transparent pass-through.
 *
 * This is the app-open half of the feature. Per-transaction prompts reuse the same
 * `verifyMpin` call at the point each transaction is confirmed.
 */
const VERIFIED_KEY = 'fino.mpin.verified';

export function MpinGate({ children }: { children: ReactNode }) {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();

  // 'checking' until we know the status; 'pass' when nothing is required or it is satisfied;
  // 'enter' when we must collect the PIN; 'setup' when one is required but not set.
  const [phase, setPhase] = useState<'checking' | 'pass' | 'enter' | 'setup'>(() =>
    sessionStorage.getItem(VERIFIED_KEY) === '1' ? 'pass' : 'checking',
  );
  const [pin, setPin] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const asked = useRef(false);

  // This gate renders in front of the ENTIRE app, so a check that neither resolves nor retries
  // is a permanently blank screen. It used to combine a once-only `asked` ref with an `alive`
  // flag cleared on teardown, and those two cancel each other out whenever the effect is torn
  // down and re-run (React StrictMode does exactly that on mount, and a remount can at any
  // time): the first run started the request and then disowned its result, while the re-run saw
  // `asked` already set and started nothing — leaving `phase` on 'checking' for good.
  //
  // The ref alone is enough. It still guarantees one request per mount, and a late response
  // after unmount is a no-op state update in React 18, not a leak.
  useEffect(() => {
    if (phase !== 'checking' || asked.current || !user) return;
    asked.current = true;
    (async () => {
      try {
        const s = await getMpinStatus();
        if (!s.Required) { setPhase('pass'); return; }
        setPhase(s.IsSet ? 'enter' : 'setup');
      } catch {
        // If the status check itself fails, don't lock the user out of their own app.
        setPhase('pass');
      }
    })();
  }, [phase, user]);

  async function submit() {
    if (!/^\d{4,6}$/.test(pin)) { setError('Enter your 4–6 digit MPIN.'); return; }
    setBusy(true); setError('');
    try {
      const res = await verifyMpin(pin);
      if (!res.ok) { setError(res.message || 'Incorrect MPIN.'); return; }
      try { sessionStorage.setItem(VERIFIED_KEY, '1'); } catch { /* private mode */ }
      setPhase('pass');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not verify the MPIN.');
    } finally { setBusy(false); }
  }

  if (phase === 'pass') return <>{children}</>;

  if (phase === 'checking') {
    return (
      <div className="grid min-h-screen place-items-center bg-app-canvas">
        <BrandLoading className="min-h-0" />
      </div>
    );
  }

  // A full-screen wall — nothing behind it renders until the PIN is satisfied.
  return (
    <div className="grid min-h-screen place-items-center bg-app-canvas px-4">
      <div className="w-full max-w-sm rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="mb-5 flex flex-col items-center text-center">
          <BrandLockup tone="light" size="md" className="mb-3" />
          <div className="grid size-11 -mt-1 place-items-center rounded-full bg-brand-50 text-brand-600">
            <KeyRound className="size-5" />
          </div>
          <h1 className="mt-3 text-lg font-semibold text-slate-900">
            {phase === 'setup' ? 'Set your MPIN' : 'Enter your MPIN'}
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            {phase === 'setup'
              ? 'Your account requires an MPIN. Set one to continue.'
              : 'For your security, confirm your MPIN to continue.'}
          </p>
        </div>

        {phase === 'setup' ? (
          <Button className="w-full" onClick={() => { sessionStorage.setItem(VERIFIED_KEY, '1'); setPhase('pass'); navigate('/s/AccountSecurity'); }}>Set MPIN now</Button>
        ) : (
          <div className="space-y-3">
            <Input
              label="MPIN"
              type="password"
              inputMode="numeric"
              autoFocus
              maxLength={6}
              placeholder="••••"
              value={pin}
              error={error || undefined}
              onChange={(e) => setPin(e.target.value.replace(/\D/g, '').slice(0, 6))}
              onKeyDown={(e) => { if (e.key === 'Enter') submit(); }}
            />
            <Button className="w-full" onClick={submit} loading={busy}>Unlock</Button>
          </div>
        )}

        <button
          type="button"
          onClick={() => void signOut()}
          className="mt-4 flex w-full items-center justify-center gap-1.5 text-sm text-slate-400 transition-colors hover:text-slate-600"
        >
          <LogOut className="size-4" /> Sign out instead
        </button>
      </div>
    </div>
  );
}
