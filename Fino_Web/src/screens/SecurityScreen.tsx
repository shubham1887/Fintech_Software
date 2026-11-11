import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { KeyRound, Lock, ShieldCheck, Loader2 } from 'lucide-react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { useToast } from '@/components/ui/Toast';
import { changePassword, getMpinStatus, setMpin } from '@/api/security.api';
import { TwoFactorCard } from '@/features/security/TwoFactorCard';
import type { CustomScreenProps } from './registry';

/** A digits-only field for a 4–6 digit MPIN. */
function PinInput(props: { label: string; value: string; onChange: (v: string) => void; hint?: string }) {
  return (
    <Input
      label={props.label}
      type="password"
      inputMode="numeric"
      autoComplete="off"
      maxLength={6}
      placeholder="••••"
      value={props.value}
      hint={props.hint}
      onChange={(e) => props.onChange(e.target.value.replace(/\D/g, '').slice(0, 6))}
    />
  );
}

/**
 * Account security — Change Password and Change / Set MPIN, on one page.
 *
 * The MPIN is the short transaction / app-open PIN. Whether it is *required* is decided by the
 * admin's Key Setting (platform-wide) or the user opting in here; either way the user sets and
 * changes it from this screen. The digits are never sent back from the server — this page only
 * ever knows whether an MPIN *is set*, not what it is.
 */
export function SecurityScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();

  // Change password
  const [oldPw, setOldPw] = useState('');
  const [newPw, setNewPw] = useState('');
  const [confPw, setConfPw] = useState('');
  const [pwBusy, setPwBusy] = useState(false);

  // MPIN
  const { data: mpin, isLoading: mpinLoading } = useQuery({ queryKey: ['mpin-status'], queryFn: getMpinStatus });
  const [oldPin, setOldPin] = useState('');
  const [newPin, setNewPin] = useState('');
  const [confPin, setConfPin] = useState('');
  const [pinBusy, setPinBusy] = useState(false);
  const hasMpin = Boolean(mpin?.IsSet);

  async function submitPassword() {
    if (!oldPw || !newPw || !confPw) { toast.error('Fill in every password field.'); return; }
    if (newPw !== confPw) { toast.error('New password and confirm password do not match.'); return; }
    if (newPw === oldPw) { toast.error('New password must be different from the current one.'); return; }
    setPwBusy(true);
    try {
      const res = await changePassword({ OldPassword: oldPw, NewPassword: newPw, ConfirmPassword: confPw });
      if (!res.ok) { toast.error(res.message || 'Could not change the password.'); return; }
      toast.success('Password changed.');
      setOldPw(''); setNewPw(''); setConfPw('');
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not change the password.');
    } finally { setPwBusy(false); }
  }

  async function submitMpin() {
    if (hasMpin && !oldPin) { toast.error('Enter your current MPIN.'); return; }
    if (!/^\d{4,6}$/.test(newPin)) { toast.error('MPIN must be 4 to 6 digits.'); return; }
    if (newPin !== confPin) { toast.error('MPIN and confirm MPIN do not match.'); return; }
    setPinBusy(true);
    try {
      const res = await setMpin({ OldMpin: oldPin, NewMpin: newPin, ConfirmMpin: confPin, Enable: true });
      if (!res.ok) { toast.error(res.message || 'Could not save the MPIN.'); return; }
      toast.success(hasMpin ? 'MPIN changed.' : 'MPIN set.');
      setOldPin(''); setNewPin(''); setConfPin('');
      qc.invalidateQueries({ queryKey: ['mpin-status'] });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not save the MPIN.');
    } finally { setPinBusy(false); }
  }

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={ShieldCheck} />

      <div className="grid gap-4 lg:grid-cols-2">
        {/* Change password */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><Lock className="size-4 text-brand-600" /> Change password</CardTitle>
          </CardHeader>
          <CardBody className="space-y-4">
            <Input label="Current password" type="password" autoComplete="current-password" value={oldPw} onChange={(e) => setOldPw(e.target.value)} />
            <Input label="New password" type="password" autoComplete="new-password" value={newPw} onChange={(e) => setNewPw(e.target.value)} hint="At least 8 characters, with a mix of letters and numbers." />
            <Input label="Confirm new password" type="password" autoComplete="new-password" value={confPw} onChange={(e) => setConfPw(e.target.value)} />
            <div className="flex justify-end">
              <Button onClick={submitPassword} loading={pwBusy}>Update password</Button>
            </div>
          </CardBody>
        </Card>

        {/* Change / set MPIN */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <KeyRound className="size-4 text-brand-600" /> {hasMpin ? 'Change MPIN' : 'Set MPIN'}
            </CardTitle>
            {!mpinLoading && (
              <Badge tone={mpin?.Required ? (hasMpin ? 'success' : 'warning') : 'neutral'}>
                {mpin?.Required ? (hasMpin ? 'Active' : 'Required — not set') : hasMpin ? 'Active' : 'Optional'}
              </Badge>
            )}
          </CardHeader>
          <CardBody className="space-y-4">
            {mpinLoading ? (
              <div className="flex items-center gap-2 py-6 text-sm text-slate-400"><Loader2 className="size-4 animate-spin" /> Loading…</div>
            ) : (
              <>
                <p className="text-sm text-slate-500">
                  A 4–6 digit PIN asked for on sensitive actions{mpin?.Required ? ' — required for your account' : ''}. It is separate from your password.
                </p>
                {hasMpin && <PinInput label="Current MPIN" value={oldPin} onChange={setOldPin} />}
                <PinInput label={hasMpin ? 'New MPIN' : 'MPIN'} value={newPin} onChange={setNewPin} hint="4 to 6 digits." />
                <PinInput label="Confirm MPIN" value={confPin} onChange={setConfPin} />
                <div className="flex justify-end">
                  <Button onClick={submitMpin} loading={pinBusy}>{hasMpin ? 'Change MPIN' : 'Set MPIN'}</Button>
                </div>
              </>
            )}
          </CardBody>
        </Card>

        {/* Authenticator app (TOTP) */}
        <div className="lg:col-span-2">
          <TwoFactorCard />
        </div>
      </div>
    </>
  );
}
