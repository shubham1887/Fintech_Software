import { useEffect, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import QRCode from 'qrcode';
import { Check, Copy, Loader2, ShieldCheck, Smartphone } from 'lucide-react';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { useToast } from '@/components/ui/Toast';
import { useAuth } from '@/features/auth/AuthContext';
import {
  beginTwoFactorSetup,
  confirmTwoFactor,
  disableTwoFactor,
  getTwoFactorStatus,
  type TwoFactorSetup,
} from '@/api/twoFactor.api';

/**
 * Two-step verification with an authenticator app (Google Authenticator, Authy, Microsoft
 * Authenticator, 2FAS — any of them; the codes are plain RFC 6238 TOTP).
 *
 * The QR is rendered in this browser from the otpauth:// URI, never fetched from a QR-image
 * service: that URI contains the shared secret, and handing it to a third-party image host would
 * give away the one thing the whole mechanism depends on staying private.
 *
 * The manual-entry key sits beside the QR because the common case in retail is the panel being
 * open on the same phone that holds the authenticator — there is no second camera to point.
 */
export function TwoFactorCard() {
  const toast = useToast();
  const qc = useQueryClient();
  const { user, updateUser } = useAuth();

  const { data: status, isLoading } = useQuery({ queryKey: ['two-factor-status'], queryFn: getTwoFactorStatus });

  const [setup, setSetup] = useState<TwoFactorSetup | null>(null);
  const [qrDataUri, setQrDataUri] = useState('');
  const [code, setCode] = useState('');
  const [busy, setBusy] = useState(false);

  // Shown once, after confirming. The server only keeps hashes, so if these are lost an operator
  // has to clear the enrolment by hand.
  const [recoveryCodes, setRecoveryCodes] = useState<string[] | null>(null);
  const [copied, setCopied] = useState(false);

  // Disabling
  const [disablePw, setDisablePw] = useState('');
  const [disableCode, setDisableCode] = useState('');

  const isEnabled = Boolean(status?.IsEnabled);

  // Draw the QR whenever a new secret is minted.
  useEffect(() => {
    if (!setup?.ProvisioningUri) { setQrDataUri(''); return; }
    let cancelled = false;
    QRCode.toDataURL(setup.ProvisioningUri, { width: 240, margin: 1, errorCorrectionLevel: 'M' })
      .then((uri) => { if (!cancelled) setQrDataUri(uri); })
      .catch(() => {
        // Not fatal — the manual-entry key below is a complete alternative to scanning.
        if (!cancelled) setQrDataUri('');
      });
    return () => { cancelled = true; };
  }, [setup?.ProvisioningUri]);

  async function startSetup() {
    setBusy(true);
    try {
      const res = await beginTwoFactorSetup();
      if (!res.ok || !res.data) { toast.error(res.message || 'Could not start the setup.'); return; }
      setSetup(res.data);
      setCode('');
      setRecoveryCodes(null);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not start the setup.');
    } finally { setBusy(false); }
  }

  async function submitConfirm() {
    if (!/^\d{6}$/.test(code.replace(/\s/g, ''))) { toast.error('Enter the 6-digit code shown in the app.'); return; }
    setBusy(true);
    try {
      const res = await confirmTwoFactor(code.replace(/\s/g, ''));
      if (!res.ok || !res.data) { toast.error(res.message || 'That code is not correct.'); return; }
      toast.success('Two-step verification is on.');
      setRecoveryCodes(res.data.RecoveryCodes ?? []);
      setSetup(null);
      setCode('');
      qc.invalidateQueries({ queryKey: ['two-factor-status'] });
      // Keep the stored session in step, so the profile screen's flag stops contradicting this one.
      if (user) updateUser({ IsTwoFA: true });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not confirm the code.');
    } finally { setBusy(false); }
  }

  async function submitDisable() {
    if (!disablePw) { toast.error('Enter your account password.'); return; }
    if (!disableCode.trim()) { toast.error('Enter a code from your authenticator app.'); return; }
    setBusy(true);
    try {
      const res = await disableTwoFactor({ Password: disablePw, Code: disableCode.trim() });
      if (!res.ok) { toast.error(res.message || 'Could not switch it off.'); return; }
      toast.success('Two-step verification is off.');
      setDisablePw(''); setDisableCode('');
      qc.invalidateQueries({ queryKey: ['two-factor-status'] });
      if (user) updateUser({ IsTwoFA: false });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not switch it off.');
    } finally { setBusy(false); }
  }

  async function copyRecoveryCodes() {
    if (!recoveryCodes) return;
    try {
      await navigator.clipboard.writeText(recoveryCodes.join('\n'));
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      toast.error('Could not copy — select the codes and copy them by hand.');
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ShieldCheck className="size-4 text-brand-600" /> Authenticator app
        </CardTitle>
        {!isLoading && (
          <Badge tone={isEnabled ? 'success' : 'neutral'}>
            {isEnabled ? 'On' : status?.IsPending ? 'Setup not finished' : 'Off'}
          </Badge>
        )}
      </CardHeader>

      <CardBody className="space-y-4">
        {isLoading ? (
          <div className="flex items-center gap-2 py-6 text-sm text-slate-400">
            <Loader2 className="size-4 animate-spin" /> Loading…
          </div>
        ) : recoveryCodes ? (
          /* ─── Just enrolled: show the recovery codes once ─── */
          <div className="space-y-3">
            <p className="text-sm font-medium text-slate-900">Save your recovery codes</p>
            <p className="text-sm text-slate-500">
              Each code works once, and only if you lose access to the app. They are not shown
              again — store them somewhere other than the phone holding the authenticator.
            </p>
            <div className="grid grid-cols-2 gap-2 rounded-lg bg-slate-50 p-4 font-mono text-sm text-slate-700 sm:grid-cols-3">
              {recoveryCodes.map((c) => <span key={c}>{c}</span>)}
            </div>
            <div className="flex flex-wrap justify-end gap-2">
              <Button variant="outline" onClick={copyRecoveryCodes}>
                {copied ? <Check className="size-4" /> : <Copy className="size-4" />}
                {copied ? 'Copied' : 'Copy codes'}
              </Button>
              <Button onClick={() => setRecoveryCodes(null)}>I have saved them</Button>
            </div>
          </div>
        ) : isEnabled ? (
          /* ─── Enrolled: offer to switch it off ─── */
          <div className="space-y-4">
            <p className="text-sm text-slate-500">
              Signing in asks for a code from your authenticator app as well as your password.
              {typeof status?.RecoveryCodesRemaining === 'number' && (
                <> {status.RecoveryCodesRemaining} recovery code
                  {status.RecoveryCodesRemaining === 1 ? '' : 's'} unused.</>
              )}
            </p>
            <div className="rounded-lg border border-slate-200 p-4 space-y-4">
              <p className="text-sm font-medium text-slate-900">Switch off two-step verification</p>
              <p className="text-sm text-slate-500">
                Needs your password and a current code — switching it off is the first thing someone
                on a borrowed session would try.
              </p>
              <Input
                label="Account password"
                type="password"
                autoComplete="current-password"
                value={disablePw}
                onChange={(e) => setDisablePw(e.target.value)}
              />
              <Input
                label="Authenticator or recovery code"
                inputMode="numeric"
                autoComplete="one-time-code"
                maxLength={14}
                placeholder="000000"
                value={disableCode}
                onChange={(e) => setDisableCode(e.target.value)}
              />
              <div className="flex justify-end">
                <Button variant="danger" onClick={submitDisable} loading={busy}>Switch off</Button>
              </div>
            </div>
          </div>
        ) : setup ? (
          /* ─── Mid-enrolment: scan, then prove a code ─── */
          <div className="grid gap-6 sm:grid-cols-[auto,1fr]">
            <div className="space-y-2">
              {qrDataUri ? (
                <img
                  src={qrDataUri}
                  alt="QR code for your authenticator app"
                  className="rounded-lg border border-slate-200 bg-white p-2"
                  width={240}
                  height={240}
                />
              ) : (
                <div className="grid size-60 place-items-center rounded-lg border border-slate-200 text-sm text-slate-400">
                  Use the key below
                </div>
              )}
            </div>

            <div className="space-y-4">
              <ol className="space-y-2 text-sm text-slate-600">
                <li>1. Open Google Authenticator (or any authenticator app).</li>
                <li>2. Scan this QR, or tap “Enter a setup key” and type the key below.</li>
                <li>3. Enter the 6-digit code it shows, to confirm.</li>
              </ol>

              <div>
                <p className="text-xs font-medium uppercase tracking-wide text-slate-400">Setup key</p>
                <p className="mt-1 select-all font-mono text-sm tracking-wide text-slate-700">
                  {setup.ManualEntryKey}
                </p>
                <p className="mt-1 text-xs text-slate-400">
                  Account “{setup.Account}” under “{setup.Issuer}”.
                </p>
              </div>

              <Input
                label="Code from the app"
                inputMode="numeric"
                autoComplete="one-time-code"
                maxLength={7}
                placeholder="000000"
                value={code}
                onChange={(e) => setCode(e.target.value.replace(/[^\d\s]/g, ''))}
                hint="Codes change every 30 seconds."
              />

              <div className="flex flex-wrap justify-end gap-2">
                <Button variant="ghost" onClick={() => { setSetup(null); setCode(''); }}>Cancel</Button>
                <Button onClick={submitConfirm} loading={busy}>Confirm and turn on</Button>
              </div>
            </div>
          </div>
        ) : (
          /* ─── Not enrolled ─── */
          <div className="space-y-4">
            <p className="text-sm text-slate-500">
              Asks for a 6-digit code from your phone as well as your password when you sign in.
              The codes are generated on the device itself, so they work with no signal and cost
              nothing to send.
            </p>
            {status?.IsPending && (
              <p className="text-sm text-warning-600">
                A setup was started but never confirmed, so two-step verification is still off.
                Starting again issues a fresh QR — the old one stops working.
              </p>
            )}
            <div className="flex justify-end">
              <Button onClick={startSetup} loading={busy}>
                <Smartphone className="size-4" />
                {status?.IsPending ? 'Start again' : 'Set up'}
              </Button>
            </div>
          </div>
        )}
      </CardBody>
    </Card>
  );
}
