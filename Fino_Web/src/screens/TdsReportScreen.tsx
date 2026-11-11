import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, BadgeIndianRupee, Banknote, Info, Wallet } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, apiDate } from '@/lib/format';
import { cn } from '@/lib/cn';
import { UserTypeCode } from '@/lib/roles';
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

interface MonthlyRow {
  PartyUserID: number;
  PartyName: string; PartyCode: string; UserTypeName: string;
  PAN: string; PANMissing: boolean;
  DeductionCount: number; CommissionBase: number; TDSAmount: number; NotWithheld: number;
  Sections: string[];
  SettlementMode: string; SettlementReference: string;
  SettledOn: string | null; SettledAmount: number;
  Pending: boolean;
}

interface MonthlyReport {
  PeriodKey: string; PeriodLabel: string; MasterSwitchOn: boolean;
  Rows: MonthlyRow[];
  PartyCount: number;
  TotalTDS: number; TotalNotWithheld: number;
  TotalDeposited: number; TotalWalletSettled: number; TotalPending: number;
  Note: string;
}

interface DetailRow {
  TransactionMasterID: number; DateTime: string; ServiceName: string; Tier: string;
  CommissionBase: number; ChargeType: string; RatePercent: number; TDSAmount: number;
  SectionCode: string; NoPANRateApplied: boolean; Withheld: boolean; DecidedBy: string;
}

interface PartyDetail {
  PeriodKey: string; PartyUserID: number; PartyName: string; PAN: string;
  TotalTDS: number; Rows: DetailRow[];
}

/** The last 18 months, newest first — long enough to cover a late filing, short enough to scan. */
function periodOptions(): { value: string; label: string }[] {
  const out: { value: string; label: string }[] = [];
  const now = new Date();
  for (let i = 0; i < 18; i++) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
    const key = `${d.getFullYear()}${String(d.getMonth() + 1).padStart(2, '0')}`;
    out.push({ value: key, label: d.toLocaleDateString('en-IN', { month: 'long', year: 'numeric' }) });
  }
  return out;
}

/**
 * TDS Report — one month, one row per party, and the two things that can be done with it.
 *
 * The whole point of this screen is the question the brief asks: "kiska kitna TDS bharna hai".
 * So the row is the PARTY, not the transaction, and the figure is the month's total — with the
 * deductions behind it one click away, because that detail is what a TDS certificate is made of.
 *
 * Two columns exist here that look like duplicates and are not:
 *  - TDS withheld is what came out of the party's wallet.
 *  - Not withheld is what was recorded but could not be taken (the wallet could not fund it).
 * Depositing the second with the government would be paying money the platform never collected,
 * so they are never added together.
 *
 * Recording a deposit is a bookkeeping fact and the accounting desk does it. Settling back into a
 * wallet moves real money out, so that button is Admin's alone — hidden here and refused by
 * ApiAccessPolicy, which is the check that actually counts.
 */
export function TdsReportScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const { user } = useAuth();
  const isAdmin = user?.UserTypeID === UserTypeCode.Admin;

  const periods = useMemo(periodOptions, []);
  const [period, setPeriod] = useState(periods[0]?.value ?? '');
  const [pendingOnly, setPendingOnly] = useState(false);

  const key = ['tds-report', period, pendingOnly];
  const { data, isFetching } = useQuery<MonthlyReport>({
    queryKey: key,
    queryFn: async () => {
      const res = await apiPost<MonthlyReport>('TdsReport/Monthly', { PeriodKey: period, PendingOnly: pendingOnly });
      if (!res.ok) throw new Error(res.message || 'Could not load the TDS report.');
      return res.data as MonthlyReport;
    },
    enabled: Boolean(period),
  });
  const refresh = () => qc.invalidateQueries({ queryKey: ['tds-report'] });

  // ── The deductions behind one party's figure ──────────────────────────────
  const [detailFor, setDetailFor] = useState<MonthlyRow | null>(null);
  const detail = useQuery<PartyDetail>({
    queryKey: ['tds-party-detail', period, detailFor?.PartyUserID],
    queryFn: async () => {
      const res = await apiPost<PartyDetail>('TdsReport/PartyDetail', {
        PeriodKey: period, PartyUserID: detailFor?.PartyUserID ?? 0,
      });
      if (!res.ok) throw new Error(res.message || 'Could not load the deductions.');
      return res.data as PartyDetail;
    },
    enabled: detailFor !== null,
  });

  // ── Deposit ───────────────────────────────────────────────────────────────
  const [depositFor, setDepositFor] = useState<MonthlyRow | null>(null);
  const [reference, setReference] = useState('');
  const [referenceDate, setReferenceDate] = useState(apiDate(new Date()));
  const [depositRemark, setDepositRemark] = useState('');

  const depositMut = useMutation({
    mutationFn: async () => {
      const res = await apiPost('TdsReport/MarkDeposited', {
        PeriodKey: period,
        PartyUserID: depositFor?.PartyUserID ?? 0,
        Reference: reference,
        ReferenceDate: referenceDate || null,
        Remark: depositRemark,
      });
      if (!res.ok) throw new Error(res.message || 'Could not record the deposit.');
    },
    onSuccess: () => {
      toast.success('Recorded as deposited.');
      setDepositFor(null); setReference(''); setDepositRemark('');
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Settle back into the wallet ───────────────────────────────────────────
  const [settleFor, setSettleFor] = useState<MonthlyRow | null>(null);
  const [settleRemark, setSettleRemark] = useState('');

  const settleMut = useMutation({
    mutationFn: async () => {
      const res = await apiPost<{ Credited: number }>('TdsReport/SettleToWallet', {
        PeriodKey: period,
        PartyUserID: settleFor?.PartyUserID ?? 0,
        Remark: settleRemark,
      });
      if (!res.ok) throw new Error(res.message || 'Could not settle this into the wallet.');
      return res.data?.Credited ?? 0;
    },
    onSuccess: (credited) => {
      toast.success(`${inr(credited)} credited back to the wallet.`);
      setSettleFor(null); setSettleRemark('');
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const rows = data?.Rows ?? [];

  return (
    <>
      <PageHeader
        title="TDS Report"
        subtitle="Whose TDS was withheld this month, and whether it was deposited or given back"
      />

      <Card className="mb-4">
        <CardBody className="flex flex-wrap items-end gap-3">
          <div className="w-56">
            <Select label="Month" value={period} onChange={(e) => setPeriod(e.target.value)}>
              {periods.map((p) => <option key={p.value} value={p.value}>{p.label}</option>)}
            </Select>
          </div>
          <label className="flex h-9 items-center gap-2 text-sm text-slate-700">
            <input
              type="checkbox"
              checked={pendingOnly}
              onChange={(e) => setPendingOnly(e.target.checked)}
              className="size-4 rounded border-slate-300"
            />
            Only what is still to be dealt with
          </label>
          <div className="flex-1" />
          {data && !data.MasterSwitchOn && (
            <Badge tone="warning">TDS is off for the platform</Badge>
          )}
        </CardBody>
      </Card>

      {data && (
        <div className="mb-4 grid grid-cols-2 gap-3 lg:grid-cols-5">
          {[
            { label: 'Withheld', value: data.TotalTDS, tone: 'text-slate-900' },
            { label: 'Still to deal with', value: data.TotalPending, tone: 'text-warning-700' },
            { label: 'Deposited', value: data.TotalDeposited, tone: 'text-success-700' },
            { label: 'Returned to wallets', value: data.TotalWalletSettled, tone: 'text-info-600' },
            { label: 'Recorded, not collected', value: data.TotalNotWithheld, tone: 'text-danger-600' },
          ].map((t) => (
            <div key={t.label} className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
              <p className="text-xs text-slate-500">{t.label}</p>
              <p className={cn('mt-1 font-display text-lg font-bold tabular-nums', t.tone)}>{inr(t.value)}</p>
            </div>
          ))}
        </div>
      )}

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {isFetching && rows.length === 0 ? (
          <div className="flex justify-center py-12"><Spinner /></div>
        ) : rows.length === 0 ? (
          <EmptyState
            icon={BadgeIndianRupee}
            title={`Nothing for ${data?.PeriodLabel ?? 'this month'}`}
            description={data?.Note ?? 'No TDS was deducted in this period.'}
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[60rem] border-collapse text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <th className="px-3 py-3">Party</th>
                  <th className="px-3 py-3">PAN</th>
                  <th className="px-3 py-3 text-right">Commission</th>
                  <th className="px-3 py-3 text-right">TDS withheld</th>
                  <th className="px-3 py-3 text-right">Not collected</th>
                  <th className="px-3 py-3">Section</th>
                  <th className="px-3 py-3">State</th>
                  <th className="px-3 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r) => (
                  <tr key={r.PartyUserID} className="border-b border-slate-100 last:border-0">
                    <td className="px-3 py-3 align-top">
                      <button
                        type="button"
                        className="text-left font-medium text-brand-700 hover:underline"
                        onClick={() => setDetailFor(r)}
                      >
                        {r.PartyName}
                      </button>
                      <p className="text-xs text-slate-500">
                        {r.PartyCode && <span>{r.PartyCode} · </span>}
                        {r.UserTypeName} · {r.DeductionCount} deduction{r.DeductionCount === 1 ? '' : 's'}
                      </p>
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 align-top">
                      {r.PANMissing
                        ? <Badge tone="warning">no PAN</Badge>
                        : <span className="font-mono text-xs text-slate-600">{r.PAN}</span>}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right align-top tabular-nums text-slate-600">
                      {inr(r.CommissionBase)}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right align-top font-semibold tabular-nums">
                      {inr(r.TDSAmount)}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-right align-top tabular-nums">
                      {r.NotWithheld > 0
                        ? <span className="font-medium text-danger-600">{inr(r.NotWithheld)}</span>
                        : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 align-top">
                      {r.Sections.map((s) => <Badge key={s} tone="neutral" className="mr-1">{s}</Badge>)}
                    </td>
                    <td className="px-3 py-3 align-top">
                      {r.SettlementMode === 'DEPOSITED' ? (
                        <>
                          <Badge tone="success">Deposited</Badge>
                          <p className="mt-1 text-xs text-slate-500">{r.SettlementReference}</p>
                        </>
                      ) : r.SettlementMode === 'WALLET_SETTLED' ? (
                        <>
                          <Badge tone="info">Returned to wallet</Badge>
                          <p className="mt-1 text-xs text-slate-500">{inr(r.SettledAmount)}</p>
                        </>
                      ) : (
                        <Badge tone="warning">Pending</Badge>
                      )}
                    </td>
                    <td className="px-3 py-3 align-top">
                      <div className="flex items-center justify-end gap-1">
                        {r.Pending ? (
                          <>
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => { setDepositFor(r); setReference(''); setDepositRemark(''); }}
                            >
                              <Banknote className="size-4" /> Deposited
                            </Button>
                            {isAdmin && (
                              <Button
                                size="sm"
                                variant="ghost"
                                title="Give the withheld TDS back to the party as wallet balance"
                                onClick={() => { setSettleFor(r); setSettleRemark(''); }}
                              >
                                <Wallet className="size-4" /> To wallet
                              </Button>
                            )}
                          </>
                        ) : (
                          <span className="text-xs text-slate-400">settled</span>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <p className="mt-3 flex items-start gap-1.5 text-xs text-slate-500">
        <Info className="mt-0.5 size-3.5 shrink-0 text-slate-400" />
        <span>
          <span className="font-medium text-slate-600">TDS withheld</span> is what actually came out of the
          party&apos;s wallet. <span className="font-medium text-slate-600">Not collected</span> is what was recorded
          but could not be taken, and it is never deposited — that would be paying money the platform does not hold.
          A month can be settled once per party: either deposited with the government against its challan, or
          returned to the wallet it came out of.
        </span>
      </p>

      {/* ── The deductions behind a figure ───────────────────────────────── */}
      <Modal
        open={detailFor !== null}
        onClose={() => setDetailFor(null)}
        title={detailFor ? `${detailFor.PartyName} — ${data?.PeriodLabel ?? ''}` : ''}
        size="xl"
      >
        {detail.isFetching ? (
          <div className="flex justify-center py-10"><Spinner /></div>
        ) : (
          <div>
            <div className="mb-3 flex flex-wrap items-center gap-3 text-sm">
              <span>Total <span className="font-semibold tabular-nums">{inr(detail.data?.TotalTDS ?? 0)}</span></span>
              {detail.data?.PAN
                ? <span className="font-mono text-xs text-slate-600">PAN {detail.data.PAN}</span>
                : <Badge tone="warning">no PAN — deducted at the higher rate, and no certificate can be issued</Badge>}
            </div>
            <div className="max-h-[60vh] overflow-auto rounded-card border border-slate-200/70">
              <table className="w-full border-collapse text-sm">
                <thead className="sticky top-0 bg-slate-50/95">
                  <tr className="border-b border-slate-200 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-3 py-2">Txn</th>
                    <th className="px-3 py-2">Service</th>
                    <th className="px-3 py-2">Tier</th>
                    <th className="px-3 py-2 text-right">Base</th>
                    <th className="px-3 py-2 text-right">Rate</th>
                    <th className="px-3 py-2 text-right">TDS</th>
                    <th className="px-3 py-2">Collected</th>
                  </tr>
                </thead>
                <tbody>
                  {(detail.data?.Rows ?? []).map((d, i) => (
                    <tr key={`${d.TransactionMasterID}-${d.Tier}-${i}`} className="border-b border-slate-100 last:border-0">
                      <td className="whitespace-nowrap px-3 py-2">
                        <p className="font-medium text-slate-700">#{d.TransactionMasterID}</p>
                        <p className="text-xs text-slate-400">{new Date(d.DateTime).toLocaleDateString('en-IN')}</p>
                      </td>
                      <td className="px-3 py-2 text-xs text-slate-600">{d.ServiceName}</td>
                      <td className="px-3 py-2 text-xs text-slate-600">{d.Tier}</td>
                      <td className="whitespace-nowrap px-3 py-2 text-right tabular-nums">{inr(d.CommissionBase)}</td>
                      <td className="whitespace-nowrap px-3 py-2 text-right tabular-nums text-xs">
                        {d.ChargeType === 'FIX' ? 'flat' : `${d.RatePercent}%`}
                        {d.NoPANRateApplied && <span className="ml-1 text-warning-600">no-PAN</span>}
                      </td>
                      <td className="whitespace-nowrap px-3 py-2 text-right font-semibold tabular-nums">{inr(d.TDSAmount)}</td>
                      <td className="px-3 py-2">
                        {d.Withheld
                          ? <Badge tone="success">yes</Badge>
                          : <Badge tone="danger">no</Badge>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {(detail.data?.Rows ?? []).some((d) => d.DecidedBy) && (
              <p className="mt-2 text-xs text-slate-400">
                Rule used: {(detail.data?.Rows ?? []).find((d) => d.DecidedBy)?.DecidedBy}
              </p>
            )}
          </div>
        )}
      </Modal>

      {/* ── Record a deposit ─────────────────────────────────────────────── */}
      <Modal
        open={depositFor !== null}
        onClose={() => setDepositFor(null)}
        title={depositFor ? `Deposited — ${depositFor.PartyName}` : ''}
      >
        {depositFor && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              Recording that <span className="font-semibold tabular-nums">{inr(depositFor.TDSAmount)}</span> of{' '}
              {depositFor.PartyName}&apos;s TDS for {data?.PeriodLabel} was paid to the government.
              This records a fact; it moves no money here.
            </p>
            {depositFor.NotWithheld > 0 && (
              <p className="flex items-start gap-1.5 rounded-lg bg-warning-50 p-2.5 text-xs font-medium text-warning-700">
                <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />
                {inr(depositFor.NotWithheld)} of this was never actually collected from the party&apos;s wallet.
                Check that before depositing it.
              </p>
            )}
            <Input
              label="Challan / BSR reference"
              placeholder="e.g. 0510308-27102026-00042"
              value={reference}
              onChange={(e) => setReference(e.target.value)}
              hint="A TDS certificate is issued against it, so a deposit without one cannot be proved"
            />
            <Input label="Deposit date" type="date" value={referenceDate} onChange={(e) => setReferenceDate(e.target.value)} />
            <Input
              label="Note (optional)"
              value={depositRemark}
              onChange={(e) => setDepositRemark(e.target.value)}
            />
            <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
              <Button variant="secondary" onClick={() => setDepositFor(null)}>Cancel</Button>
              <Button
                loading={depositMut.isPending}
                disabled={!reference.trim()}
                onClick={() => depositMut.mutate()}
              >
                Record deposit
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* ── Settle back into the wallet ──────────────────────────────────── */}
      <Modal
        open={settleFor !== null}
        onClose={() => setSettleFor(null)}
        title={settleFor ? `Return to wallet — ${settleFor.PartyName}` : ''}
      >
        {settleFor && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              This credits the TDS that was actually collected from {settleFor.PartyName} in {data?.PeriodLabel} back
              into the wallet it came out of, instead of depositing it with the government.
            </p>
            <p className="flex items-start gap-1.5 rounded-lg bg-danger-50 p-2.5 text-xs font-medium text-danger-700">
              <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />
              This moves real money. It can be done once for this month, and only the{' '}
              {inr(settleFor.TDSAmount - settleFor.NotWithheld)} that was genuinely withheld can be returned.
            </p>
            <Input
              label="Why it is being returned"
              placeholder="e.g. PAN produced on 27 Oct; deduction was not due"
              value={settleRemark}
              onChange={(e) => setSettleRemark(e.target.value)}
              hint="Required — in six months somebody will ask on what grounds"
            />
            <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
              <Button variant="secondary" onClick={() => setSettleFor(null)}>Cancel</Button>
              <Button
                loading={settleMut.isPending}
                disabled={!settleRemark.trim()}
                onClick={() => settleMut.mutate()}
              >
                Credit the wallet
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
