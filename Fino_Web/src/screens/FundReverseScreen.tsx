import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Lock, Undo2, Wallet } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Skeleton } from '@/components/ui/Skeleton';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

const USER_PAGE = { PageNumber: 1, PageSize: 2000, TakeCount: 2000, SkipCount: 0, ShowDeleted: false };

interface UserWallet {
  walletId: number;
  name: string;
  balance: number;
  hold: number;
  available: number;
}

/**
 * Fund Reverse — pull balance back from a user's wallet.
 *
 * The generic form this replaces asked for a user, a wallet and an amount and answered
 * "insufficient balance" whenever part of that wallet was on hold (a balance cap), with no way to
 * see how much was held or to do anything about it. This screen shows every wallet the chosen user
 * holds — balance, what is on hold, what is free — and, when the amount would need the held part,
 * offers to release that much of the hold in the same action. The server applies the same rule.
 */
export function FundReverseScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const [userId, setUserId] = useState<string | number>('');
  const [walletId, setWalletId] = useState('');
  const [amount, setAmount] = useState('');
  const [remark, setRemark] = useState('');
  const [releaseHold, setReleaseHold] = useState(false);

  const { data: users = [], isLoading: usersLoading } = useQuery({
    queryKey: ['user-opts-all'],
    staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', USER_PAGE),
  });
  const userOptions: AutocompleteOption[] = useMemo(
    () => users.map((u) => ({
      value: String(getField(u, 'UserMasterID', 'UserID') ?? ''),
      label: String(getField(u, 'FirmName', 'PersonName', 'Name') ?? '—'),
      sublabel: [getField(u, 'UserType'), getField(u, 'MobileNumber', 'Mobile')].filter(Boolean).map(String).join(' · ') || undefined,
    })),
    [users],
  );

  const { data: walletNames = [] } = useQuery({
    queryKey: ['wallet-select'],
    staleTime: 300_000,
    queryFn: () => fetchList('Wallet/Select', {}),
  });

  const uid = Number(userId || 0);
  const { data: wallets = [], isFetching: walletsLoading } = useQuery({
    queryKey: ['user-wallets', uid],
    enabled: uid > 0,
    queryFn: async (): Promise<UserWallet[]> => {
      const rows = await fetchList('UserWalletMaster/GetByUserID', { UserID: uid });
      const nameOf = (id: number) =>
        String(getField(walletNames.find((w) => Number(getField(w, 'WalletID')) === id) ?? {}, 'Name') ?? `Wallet ${id}`);
      return rows
        .map((r) => {
          const balance = Number(getField(r, 'Balance') ?? 0) || 0;
          const hold = Number(getField(r, 'HoldBalance') ?? 0) || 0;
          const id = Number(getField(r, 'WalletID') ?? 0);
          return { walletId: id, name: nameOf(id), balance, hold, available: Math.max(0, balance - hold) };
        })
        .sort((a, b) => a.walletId - b.walletId);
    },
  });

  // Pick the first wallet with money in it once they load, so one click less on every reversal.
  useEffect(() => {
    if (!walletId && wallets.length > 0) {
      const first = wallets.find((w) => w.balance > 0) ?? wallets[0];
      setWalletId(String(first.walletId));
    }
  }, [wallets, walletId]);

  const wallet = wallets.find((w) => String(w.walletId) === walletId);
  const amountNum = Number(amount) || 0;
  const exceedsBalance = Boolean(wallet) && amountNum > (wallet?.balance ?? 0);
  const needsHold = Boolean(wallet) && !exceedsBalance && amountNum > (wallet?.available ?? 0);
  const holdToRelease = needsHold && wallet ? amountNum - wallet.available : 0;

  const reverse = useMutation({
    mutationFn: () => apiPost('Fund/Reverse', {
      SenderID: uid,
      WalletID: Number(walletId),
      Amount: amountNum,
      Remark: remark.trim(),
      ReleaseHold: needsHold && releaseHold,
    }),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message || 'The reversal could not be made.'); return; }
      toast.success(`${inr(amountNum)} reversed from ${wallet?.name ?? 'the wallet'}.`);
      setAmount(''); setRemark(''); setReleaseHold(false);
      qc.invalidateQueries({ queryKey: ['user-wallets', uid] });
      qc.invalidateQueries({ queryKey: ['live-summary'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const canSubmit =
    uid > 0 && Boolean(wallet) && amountNum > 0 && !exceedsBalance && (!needsHold || releaseHold)
    && remark.trim().length > 0 && !reverse.isPending;

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Reverse</CardTitle>
            <span className="text-xs text-slate-400">The amount goes back to the user's parent wallet</span>
          </CardHeader>
          <CardBody>
            <form
              onSubmit={(e) => { e.preventDefault(); if (canSubmit) reverse.mutate(); }}
              className="grid grid-cols-1 gap-4 sm:grid-cols-2"
            >
              <div className="sm:col-span-2">
                <Autocomplete
                  label="Reverse from user"
                  placeholder="Type a firm name or mobile…"
                  options={userOptions}
                  value={userId}
                  loading={usersLoading}
                  onChange={(v) => { setUserId(v); setWalletId(''); setReleaseHold(false); }}
                />
              </div>

              <Select label="Wallet" value={walletId} onChange={(e) => { setWalletId(e.target.value); setReleaseHold(false); }} disabled={wallets.length === 0}>
                {wallets.length === 0 && <option value="">{uid > 0 ? (walletsLoading ? 'Loading…' : 'No wallet') : 'Pick a user first'}</option>}
                {wallets.map((w) => (
                  <option key={w.walletId} value={String(w.walletId)}>{`${w.name} — free ${inr(w.available)}`}</option>
                ))}
              </Select>

              <Input
                label="Amount (₹)"
                type="number"
                min={0}
                value={amount}
                onChange={(e) => { setAmount(e.target.value); setReleaseHold(false); }}
                error={exceedsBalance ? `The wallet holds ${inr(wallet?.balance ?? 0)} in total.` : undefined}
                hint={wallet ? `Free to reverse now: ${inr(wallet.available)}` : undefined}
              />

              <div className="sm:col-span-2">
                <Input label="Reason" value={remark} onChange={(e) => setRemark(e.target.value)} hint="Recorded on the reversal and sent to the user." />
              </div>

              {needsHold && wallet && (
                <div className="sm:col-span-2 rounded-xl border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
                  <div className="flex items-start gap-2">
                    <Lock className="mt-0.5 size-4 shrink-0" />
                    <div>
                      <p>
                        <strong>{inr(wallet.hold)}</strong> of this wallet is on hold (balance cap), so only{' '}
                        <strong>{inr(wallet.available)}</strong> is free. Reversing {inr(amountNum)} needs{' '}
                        <strong>{inr(holdToRelease)}</strong> of the held amount.
                      </p>
                      <label className="mt-2 flex items-center gap-2 text-slate-800">
                        <input type="checkbox" checked={releaseHold} onChange={(e) => setReleaseHold(e.target.checked)} className="size-4 rounded border-slate-300" />
                        Release {inr(holdToRelease)} of the hold and reverse the full {inr(amountNum)}
                      </label>
                      <p className="mt-1 text-xs text-warning-700/80">
                        Or lower the amount to {inr(wallet.available)} to leave the hold as it is.
                      </p>
                    </div>
                  </div>
                </div>
              )}

              <div className="flex justify-end gap-2 sm:col-span-2">
                <Button type="submit" disabled={!canSubmit} loading={reverse.isPending}>
                  <Undo2 className="size-4" />
                  Reverse {amountNum > 0 ? inr(amountNum) : ''}
                </Button>
              </div>
            </form>
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>User's wallets</CardTitle>
            <span className="text-xs text-slate-400">Balance, on hold and free</span>
          </CardHeader>
          <CardBody className="p-0">
            {uid === 0 ? (
              <p className="px-5 py-8 text-center text-sm text-slate-400">Pick a user to see their wallets.</p>
            ) : walletsLoading && wallets.length === 0 ? (
              <div className="space-y-2 p-5">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-10 rounded" />)}</div>
            ) : wallets.length === 0 ? (
              <p className="px-5 py-8 text-center text-sm text-slate-400">This user has no wallet.</p>
            ) : (
              <div className="divide-y divide-slate-100">
                {wallets.map((w) => (
                  <button
                    key={w.walletId}
                    type="button"
                    onClick={() => { setWalletId(String(w.walletId)); setReleaseHold(false); }}
                    className={cn(
                      'flex w-full items-center justify-between gap-3 px-5 py-3 text-left transition-colors hover:bg-slate-50',
                      String(w.walletId) === walletId && 'bg-brand-50/60',
                    )}
                  >
                    <span className="flex items-center gap-2 text-sm font-medium text-slate-800">
                      <Wallet className="size-4 text-slate-400" />
                      {w.name}
                    </span>
                    <span className="text-right">
                      <span className="block font-display text-base font-bold tabular-nums text-slate-900">{inr(w.balance)}</span>
                      {w.hold > 0 ? (
                        <span className="block text-xs text-warning-600">on hold {inr(w.hold)} · free {inr(w.available)}</span>
                      ) : (
                        <span className="block text-xs text-slate-400">nothing on hold</span>
                      )}
                    </span>
                  </button>
                ))}
                <div className="flex items-center justify-between px-5 py-3 text-sm">
                  <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Total</span>
                  <span className="font-display font-bold tabular-nums text-slate-900">{inr(wallets.reduce((s, w) => s + w.balance, 0))}</span>
                </div>
              </div>
            )}
          </CardBody>
        </Card>
      </div>

      <div className="mt-4 flex items-start gap-2 rounded-xl bg-slate-50 px-4 py-3 text-xs text-slate-500">
        <AlertTriangle className="mt-0.5 size-4 shrink-0 text-warning-500" />
        <span>A reversal moves money out of the user's wallet immediately and notifies them with the reason. It cannot be undone from here; to give it back, make a fund transfer.</span>
      </div>
    </>
  );
}
