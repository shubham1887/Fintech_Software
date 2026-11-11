import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, CheckCircle2, Download, Lock, LockOpen, RefreshCw, Scale, ShieldCheck,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateTime } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { cn } from '@/lib/cn';
import { canRaiseDocument, periodKeyOf, periodLabel, recentPeriods } from '@/lib/gstDocs';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface TrialBalanceRow {
  AccountCode: string;
  AccountName: string;
  AccountType: string;
  Debit: number;
  Credit: number;
  Balance: number;
}

interface TrialBalanceResult {
  PeriodKey: string;
  TotalDebit: number;
  TotalCredit: number;
  Balanced: boolean;
  Rows: TrialBalanceRow[];
}

interface IntegrityCheck {
  Check: string;
  LedgerFigure: number;
  OperationalFigure: number;
  Difference: number;
  Ok: boolean;
  Detail: string;
  // What the two figures are. The control checks compare the ledger against an operational total,
  // but not every check does — one that compares a commission credit against the funding debit
  // behind it would be flatly mislabelled as 'ledger vs operational'.
  LedgerLabel?: string;
  OperationalLabel?: string;
}

// Named by the API (LedgerIntegrityService.ChainCommissionsFunded). This check reads the operational
// ledger only, so its break means something different from a control-account difference and gets its
// own explanation below.
const CHAIN_COMMISSION_CHECK = 'Chain commissions funded';

interface IntegrityReport {
  RanAt: string;
  AllOk: boolean;
  Checks: IntegrityCheck[];
}

interface PeriodLock {
  AcctPeriodLockID: number;
  PeriodKey: string;
  LockedOn: string;
  LockedByUserID: number;
  Note: string;
}

export function TrialBalanceScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const maySeal = canRaiseDocument(user?.UserTypeID);

  const periods = useMemo(() => recentPeriods(18), []);
  const [period, setPeriod] = useState(periodKeyOf());

  const tb = useQuery({
    queryKey: ['acct-trial-balance', period],
    queryFn: async (): Promise<TrialBalanceResult> => {
      const res = await apiPost<TrialBalanceResult>('AcctIntegrity/TrialBalance', { PeriodKey: period });
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not load the trial balance.');
      return res.data;
    },
  });

  const integrity = useQuery({
    queryKey: ['acct-integrity'],
    queryFn: async (): Promise<IntegrityReport> => {
      const res = await apiPost<IntegrityReport>('AcctIntegrity/RunAll', {});
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not run the integrity checks.');
      return res.data;
    },
  });

  const locks = useQuery({
    queryKey: ['acct-periods'],
    queryFn: async (): Promise<PeriodLock[]> => {
      const res = await apiPost<PeriodLock[]>('AcctPeriod/Select', {});
      if (!res.ok) throw new Error(res.message || 'Could not read the period locks.');
      return res.data ?? [];
    },
  });

  const rows = tb.data?.Rows ?? [];
  const lockList = locks.data ?? [];
  const lockedNow = lockList.some((l) => l.PeriodKey === period);

  // ── Lock / unlock ───────────────────────────────────────────────────────────────────
  const [lockOpen, setLockOpen] = useState(false);
  const [unlockOpen, setUnlockOpen] = useState(false);
  const [note, setNote] = useState('');

  const lock = useMutation({
    mutationFn: async () => {
      const res = await apiPost('AcctPeriod/Lock', { PeriodKey: period, Note: note.trim() });
      if (!res.ok) throw new Error(res.message || 'The period could not be locked.');
    },
    onSuccess: () => {
      toast.success(`${periodLabel(period)} is sealed — nothing can post into it now.`);
      setLockOpen(false);
      setNote('');
      qc.invalidateQueries({ queryKey: ['acct-periods'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const unlock = useMutation({
    mutationFn: async () => {
      const res = await apiPost('AcctPeriod/Unlock', { PeriodKey: period, Note: note.trim() });
      if (!res.ok) throw new Error(res.message || 'The period could not be reopened.');
    },
    onSuccess: () => {
      toast.success(`${periodLabel(period)} is reopened.`);
      setUnlockOpen(false);
      setNote('');
      qc.invalidateQueries({ queryKey: ['acct-periods'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const download = () =>
    downloadCsv(
      rows.map((r) => ({
        Code: r.AccountCode,
        Account: r.AccountName,
        Type: r.AccountType,
        Debit: r.Debit,
        Credit: r.Credit,
        Balance: r.Balance,
      })),
      `trial-balance-${period}.csv`,
    );

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="The books proved against themselves, and against the wallets and provider stock they describe"
        icon={pickIcon(def.title)}
        actions={
          maySeal && (
            <div className="flex gap-2">
              {lockedNow ? (
                <Button variant="outline" onClick={() => setUnlockOpen(true)}>
                  <LockOpen className="mr-1.5 size-4" />
                  Reopen {periodLabel(period)}
                </Button>
              ) : (
                <Button onClick={() => setLockOpen(true)}>
                  <Lock className="mr-1.5 size-4" />
                  Seal {periodLabel(period)}
                </Button>
              )}
            </div>
          )
        }
      />

      {/* ── The daily checks ─────────────────────────────────────────────────────────── */}
      <Card>
        <CardBody className="flex flex-wrap items-center gap-3 pb-2">
          <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
            <ShieldCheck className="size-4 text-slate-400" />
            Integrity checks
          </div>
          {integrity.data && (
            <Badge tone={integrity.data.AllOk ? 'success' : 'danger'}>
              {integrity.data.AllOk ? 'All agree' : 'Differences found'}
            </Badge>
          )}
          <div className="flex-1" />
          {integrity.data && <span className="text-xs text-slate-400">ran {dateTime(integrity.data.RanAt)}</span>}
          <Button variant="ghost" onClick={() => integrity.refetch()} loading={integrity.isFetching}>
            <RefreshCw className="mr-1.5 size-4" />
            Re-run
          </Button>
        </CardBody>

        {integrity.isFetching ? (
          <div className="flex min-h-[18vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : integrity.isError ? (
          <CardBody>
            <EmptyState title="Could not run the checks" description={(integrity.error as Error).message} />
          </CardBody>
        ) : (
          <div className="space-y-2 px-3 pb-3">
            {(integrity.data?.Checks ?? []).map((c) => (
              <div
                key={c.Check}
                className={cn(
                  'rounded-lg border p-3',
                  c.Ok ? 'border-success-500/30 bg-success-50' : 'border-danger-500/30 bg-danger-50',
                )}
              >
                <div className="flex flex-wrap items-center gap-2">
                  {c.Ok ? (
                    <CheckCircle2 className="size-4 shrink-0 text-success-600" />
                  ) : (
                    <AlertTriangle className="size-4 shrink-0 text-danger-600" />
                  )}
                  <span className={cn('text-sm font-semibold', c.Ok ? 'text-success-700' : 'text-danger-700')}>{c.Check}</span>
                  <div className="flex-1" />
                  <span className="text-xs tabular-nums text-slate-500">
                    {c.LedgerLabel ?? 'ledger'} {inr(c.LedgerFigure)} · {c.OperationalLabel ?? 'operational'}{' '}
                    {inr(c.OperationalFigure)}
                  </span>
                  <Badge tone={c.Ok ? 'success' : 'danger'}>{inr(c.Difference)}</Badge>
                </div>
                <p className="mt-1 text-xs text-slate-500">{c.Detail}</p>
              </div>
            ))}

            {(integrity.data?.Checks ?? []).some((c) => !c.Ok && c.Check !== CHAIN_COMMISSION_CHECK) && (
              <div className="rounded-lg border border-slate-200 bg-white p-3 text-xs text-slate-500">
                A control-account difference is usually one of two things: an <strong>opening balance that was
                never posted</strong> (the ledger starts from the day events began flowing, while the wallets
                carry years of history), or a business event that has not reached the outbox. Post the opening
                balance as a manual journal on <strong>Ledger Vouchers</strong>, then re-run this.
              </div>
            )}

            {(integrity.data?.Checks ?? []).some((c) => !c.Ok && c.Check === CHAIN_COMMISSION_CHECK) && (
              <div className="rounded-lg border border-danger-500/30 bg-white p-3 text-xs text-slate-500">
                This one is <strong>not</strong> an opening-balance gap, and no journal will clear it. A
                commission leg is missing its pair: either a downline was paid a commission the admin was never
                debited for, or a refund credited the admin back for a funding debit it never took. The figure
                is money the books cannot account for, so it needs a decision rather than a posting — the
                transaction numbers are listed above.
              </div>
            )}
          </div>
        )}
      </Card>

      {/* ── Trial balance ───────────────────────────────────────────────────────────── */}
      <Card>
        <CardBody className="flex flex-wrap items-end gap-3 pb-2">
          <div className="w-44">
            <Select label="Period" value={period} onChange={(e) => setPeriod(e.target.value)}>
              {periods.map((p) => (
                <option key={p} value={p}>{periodLabel(p)}</option>
              ))}
            </Select>
          </div>
          {lockedNow && (
            <Badge tone="info">
              <Lock className="size-3" /> sealed
            </Badge>
          )}
          <div className="flex-1" />
          <Button variant="ghost" onClick={download} disabled={rows.length === 0}>
            <Download className="mr-1.5 size-4" />
            CSV
          </Button>
        </CardBody>

        {tb.isFetching ? (
          <div className="flex min-h-[25vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : tb.isError ? (
          <CardBody>
            <EmptyState title="Could not load" description={(tb.error as Error).message} />
          </CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title={`Nothing posted in ${periodLabel(period)}`}
              description="The trial balance is drawn from posted vouchers. If events are waiting, process the outbox on Ledger Vouchers."
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[720px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Code</th>
                  <th className="px-3 py-2.5 font-medium">Account</th>
                  <th className="px-3 py-2.5 font-medium">Type</th>
                  <th className="px-3 py-2.5 text-right font-medium">Debit</th>
                  <th className="px-3 py-2.5 text-right font-medium">Credit</th>
                  <th className="px-3 py-2.5 text-right font-medium">Balance</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => (
                  <tr key={r.AccountCode} className="hover:bg-slate-50">
                    <td className="px-5 py-2.5 font-mono text-sm font-semibold text-slate-800">{r.AccountCode}</td>
                    <td className="px-3 py-2.5 text-slate-700">{r.AccountName}</td>
                    <td className="px-3 py-2.5 text-xs text-slate-500">{r.AccountType}</td>
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{r.Debit > 0 ? inr(r.Debit) : ''}</td>
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{r.Credit > 0 ? inr(r.Credit) : ''}</td>
                    <td className="px-3 py-2.5 text-right tabular-nums font-semibold text-slate-800">{inr(r.Balance)}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr className="border-t-2 border-slate-300 bg-slate-50 font-semibold">
                  <td className="px-5 py-2.5 text-slate-600" colSpan={3}>
                    <span className="inline-flex items-center gap-2">
                      <Scale className="size-4 text-slate-400" />
                      Total
                      <Badge tone={tb.data?.Balanced ? 'success' : 'danger'}>
                        {tb.data?.Balanced ? 'balanced' : 'OUT OF BALANCE'}
                      </Badge>
                    </span>
                  </td>
                  <td className="px-3 py-2.5 text-right tabular-nums text-slate-800">{inr(tb.data?.TotalDebit ?? 0)}</td>
                  <td className="px-3 py-2.5 text-right tabular-nums text-slate-800">{inr(tb.data?.TotalCredit ?? 0)}</td>
                  <td className="px-3 py-2.5" />
                </tr>
              </tfoot>
            </table>
          </div>
        )}
      </Card>

      {/* ── Sealed periods ──────────────────────────────────────────────────────────── */}
      {lockList.length > 0 && (
        <Card>
          <CardBody className="pb-2">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
              <Lock className="size-4 text-slate-400" />
              Sealed periods
            </div>
            <p className="mt-1 text-xs text-slate-400">
              A sealed period rejects every posting into it. That is the control which makes a filed month
              unchangeable, so reopening one is deliberately Admin-only.
            </p>
          </CardBody>
          <div className="px-3 pb-3">
            <table className="w-full text-sm">
              <tbody className="divide-y divide-slate-100">
                {[...lockList].sort((a, b) => b.PeriodKey.localeCompare(a.PeriodKey)).map((l) => (
                  <tr key={l.AcctPeriodLockID} className="hover:bg-slate-50">
                    <td className="px-3 py-2 font-mono text-slate-800">{periodLabel(l.PeriodKey)}</td>
                    <td className="px-3 py-2 text-xs text-slate-500">sealed {dateTime(l.LockedOn)}</td>
                    <td className="px-3 py-2 text-xs text-slate-500">{l.Note || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {!maySeal && (
        <p className="text-xs text-slate-400">
          Read-only. Sealing and reopening a period are limited to Admin — a period lock is what makes a filed
          month unchangeable. Every figure and every check on this screen is visible to you.
        </p>
      )}

      <Modal open={lockOpen} onClose={() => setLockOpen(false)} title={`Seal ${periodLabel(period)}`} size="sm">
        <div className="space-y-3">
          <p className="text-sm text-slate-600">
            Once sealed, no event can post into {periodLabel(period)} — the outbox will record the refusal
            rather than changing a filed month. Check the trial balance and the integrity checks first.
          </p>
          <Input label="Note" value={note} onChange={(e) => setNote(e.target.value)} hint="e.g. GSTR-3B filed 20-Nov" />
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setLockOpen(false)}>Cancel</Button>
            <Button onClick={() => lock.mutate()} loading={lock.isPending}>
              <Lock className="mr-1.5 size-4" />
              Seal
            </Button>
          </div>
        </div>
      </Modal>

      <Modal open={unlockOpen} onClose={() => setUnlockOpen(false)} title={`Reopen ${periodLabel(period)}`} size="sm">
        <div className="space-y-3">
          <div className="flex gap-2.5 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" />
            <p>
              This period was sealed because it was filed. Reopening it allows postings that will not match
              the return already submitted. Record why.
            </p>
          </div>
          <Input label="Reason" value={note} onChange={(e) => setNote(e.target.value)} />
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setUnlockOpen(false)}>Cancel</Button>
            <Button variant="danger" onClick={() => unlock.mutate()} loading={unlock.isPending} disabled={note.trim().length === 0}>
              <LockOpen className="mr-1.5 size-4" />
              Reopen
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
