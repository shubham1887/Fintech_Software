import { useEffect, useMemo, useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Lock, Unlock } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { getField as get } from '@/lib/row';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { Modal } from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';

interface WalletBalance {
  WalletID: number;
  WalletName: string;
  Balance: number;
  HoldBalance: number;
}

/**
 * Hold or release part of a user's wallet.
 *
 * A hold freezes balance the user can still see but cannot spend, and balance lives PER WALLET —
 * a user can legitimately need their AEPS balance frozen while their recharge wallet keeps
 * trading. The dialog therefore asks which wallet, listing each one with what it holds and what is
 * already frozen, and the list comes from the user's own wallets rather than a fixed set, so a
 * wallet the admin adds later appears here on its own.
 *
 * Two other things it insists on: a reason (the hold report is only worth having if every row can
 * be explained), and a release path — before this there was no way to undo a hold from the panel
 * at all.
 *
 * Shared by All Users and the user profile so the two can never drift apart.
 */
export function HoldBalanceDialog({
  target, onClose, onDone,
}: {
  /** The user row being held, or null when the dialog is closed. */
  target: Record<string, unknown> | null;
  onClose: () => void;
  onDone: () => void;
}) {
  const toast = useToast();
  const [walletId, setWalletId] = useState('');
  const [amount, setAmount] = useState('');
  const [remark, setRemark] = useState('');
  const [isRelease, setIsRelease] = useState(false);

  const userName = String(get(target ?? {}, 'FirmName') || get(target ?? {}, 'PersonName') || '—');
  const userId = get(target ?? {}, 'UserMasterID', 'UserID');

  // The user's own wallets. Falls back to the flat Balance/HoldBalance pair for a row that
  // predates per-wallet balances, so the dialog still works rather than showing an empty picker.
  const wallets = useMemo<WalletBalance[]>(() => {
    const raw = get(target ?? {}, 'WalletBalances');
    if (Array.isArray(raw) && raw.length > 0) return raw as WalletBalance[];
    return [{
      WalletID: 0,
      WalletName: 'Primary wallet',
      Balance: Number(get(target ?? {}, 'Balance')) || 0,
      HoldBalance: Number(get(target ?? {}, 'HoldBalance')) || 0,
    }];
  }, [target]);

  // Default to the first wallet each time a different user is opened.
  useEffect(() => {
    if (target) setWalletId(String(wallets[0]?.WalletID ?? ''));
  }, [target, wallets]);

  const selected = wallets.find((w) => String(w.WalletID) === walletId) ?? wallets[0];
  const heldHere = Number(selected?.HoldBalance ?? 0);
  const spendable = Math.max(Number(selected?.Balance ?? 0) - heldHere, 0);

  const close = () => { setAmount(''); setRemark(''); setIsRelease(false); onClose(); };

  const mut = useMutation({
    mutationFn: () => apiPost('Users/AddHoldBalance', {
      TargetUserID: userId,
      WalletID: Number(walletId) || 0,
      Amount: Number(amount),
      IsRelease: isRelease,
      Remark: remark.trim(),
    }),
    onSuccess: (res) => {
      if (!res.ok) return toast.error(res.message);
      toast.success(
        isRelease
          ? `${inr(Number(amount))} released on ${selected?.WalletName}.`
          : `${inr(Number(amount))} held on ${selected?.WalletName}.`,
      );
      close();
      onDone();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  function submit() {
    const value = Number(amount);
    if (!value || value <= 0) { toast.error('Enter an amount greater than zero.'); return; }
    if (isRelease && value > heldHere) { toast.error(`Only ${inr(heldHere)} is on hold in this wallet.`); return; }
    if (!isRelease && value > spendable) { toast.error(`Only ${inr(spendable)} is spendable in this wallet.`); return; }
    if (!remark.trim()) { toast.error('Please say why — it is recorded on the hold report.'); return; }
    mut.mutate();
  }

  return (
    <Modal open={target !== null} onClose={close} title={isRelease ? 'Release hold' : 'Hold balance'}>
      {target && (
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            {isRelease ? 'Unfreeze part of' : 'Freeze part of'}{' '}
            <span className="font-medium text-slate-700">{userName}</span>’s wallet.
            {!isRelease && ' Held funds stay in the balance but cannot be spent.'}
          </p>

          {/* Hold / Release — the same dialog, because they are the same decision in two directions. */}
          <div className="flex gap-1 rounded-lg bg-slate-100 p-1">
            {[
              { on: false, label: 'Hold', icon: Lock },
              { on: true, label: 'Release', icon: Unlock },
            ].map(({ on, label, icon: Icon }) => (
              <button
                key={label}
                type="button"
                onClick={() => { setIsRelease(on); setAmount(''); }}
                className={cn(
                  'flex flex-1 items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
                  isRelease === on ? 'bg-white text-slate-800 shadow-sm' : 'text-slate-500 hover:text-slate-700',
                )}
              >
                <Icon className="size-3.5" />
                {label}
              </button>
            ))}
          </div>

          <Select label="Wallet" value={walletId} onChange={(e) => setWalletId(e.target.value)}>
            {wallets.map((w) => (
              <option key={w.WalletID} value={String(w.WalletID)}>
                {w.WalletName} — {inr(w.Balance)}{w.HoldBalance > 0 ? ` (${inr(w.HoldBalance)} held)` : ''}
              </option>
            ))}
          </Select>

          <div className="flex gap-4 rounded-lg bg-slate-50 p-3 text-sm">
            <div>
              <p className="text-xs text-slate-400">Balance</p>
              <p className="font-medium tabular-nums text-slate-800">{inr(Number(selected?.Balance ?? 0))}</p>
            </div>
            <div>
              <p className="text-xs text-slate-400">On hold</p>
              <p className="font-medium tabular-nums text-slate-800">{inr(heldHere)}</p>
            </div>
            <div>
              <p className="text-xs text-slate-400">Spendable</p>
              <p className="font-medium tabular-nums text-success-600">{inr(spendable)}</p>
            </div>
          </div>

          <Input
            label={isRelease ? 'Release amount (₹)' : 'Hold amount (₹)'}
            type="number"
            min="0"
            placeholder="0.00"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
          />
          <Input
            label="Reason (required)"
            placeholder={isRelease ? 'Why is the hold being lifted?' : 'Why is this being held?'}
            value={remark}
            onChange={(e) => setRemark(e.target.value)}
          />

          <div className="flex justify-end gap-2">
            <Button variant="outline" size="sm" onClick={close}>Cancel</Button>
            <Button
              size="sm"
              variant={isRelease ? 'primary' : 'danger'}
              loading={mut.isPending}
              onClick={submit}
            >
              {isRelease ? <Unlock className="size-4" /> : <Lock className="size-4" />}
              {isRelease ? 'Release' : 'Hold balance'}
            </Button>
          </div>
        </div>
      )}
    </Modal>
  );
}
