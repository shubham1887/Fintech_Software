import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, BadgeCheck, Clock, Download, Hammer, HandCoins, RotateCcw, Undo2, Wallet,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { cn } from '@/lib/cn';
import {
  canOperate, canRaiseDocument, deadlineTone, itcStatusTone, periodKeyOf, periodLabel, recentPeriods,
} from '@/lib/gstDocs';
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
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface ItcRow {
  ItcRegisterEntryID: number;
  TaxInvoiceHeaderID: number;
  DocumentNo: string;
  DocumentDate: string;
  SupplierGSTIN: string;
  SupplierName: string;
  EligibleITC: number;
  Status: string;
  ReconBucket: string;
  ClaimedInPeriod: string;
  SupplierPaid: boolean;
  Rule37DueDate: string;
  Section16_4Deadline: string;
}

interface DeadlineAlert {
  ItcRegisterEntryID: number;
  TaxInvoiceHeaderID: number;
  DocumentNo: string;
  DocumentDate: string;
  SupplierName: string;
  EligibleITC: number;
  DeadlineKind: string;
  DeadlineDate: string;
  DaysLeft: number;
  Passed: boolean;
  Action: string;
}

interface BuildResult {
  PeriodKey: string;
  EntriesCreated: number;
  EntriesUpdated: number;
  TotalEligibleITC: number;
}

const daysTo = (iso: string): number => {
  const t = new Date(iso).getTime();
  if (!Number.isFinite(t)) return 0;
  return Math.ceil((t - Date.now()) / 86_400_000);
};

export function ItcRegisterScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayOperate = canOperate(user?.UserTypeID);
  const mayMoveCredit = canRaiseDocument(user?.UserTypeID);

  const periods = useMemo(() => recentPeriods(18), []);
  const [period, setPeriod] = useState(periodKeyOf());
  const [daysAhead, setDaysAhead] = useState('45');
  const [selected, setSelected] = useState<number[]>([]);

  const register = useQuery({
    queryKey: ['itc-register', period],
    queryFn: async (): Promise<ItcRow[]> => {
      const res = await apiPost<ItcRow[]>('Itc/Register', { PeriodKey: period });
      if (!res.ok) throw new Error(res.message || 'Could not load the ITC register.');
      return res.data ?? [];
    },
  });

  const rule37 = useQuery({
    queryKey: ['itc-rule37', daysAhead],
    queryFn: async (): Promise<DeadlineAlert[]> => {
      const res = await apiPost<DeadlineAlert[]>('Itc/Rule37Alerts', { DaysAhead: Number(daysAhead) || 45 });
      if (!res.ok) throw new Error(res.message || 'Could not load the Rule 37 alerts.');
      return res.data ?? [];
    },
  });

  const s164 = useQuery({
    queryKey: ['itc-164', daysAhead],
    queryFn: async (): Promise<DeadlineAlert[]> => {
      const res = await apiPost<DeadlineAlert[]>('Itc/Section164Alerts', { DaysAhead: Number(daysAhead) || 45 });
      if (!res.ok) throw new Error(res.message || 'Could not load the Section 16(4) alerts.');
      return res.data ?? [];
    },
  });

  const rows = register.data ?? [];
  const alerts = [...(rule37.data ?? []), ...(s164.data ?? [])].sort((a, b) => a.DaysLeft - b.DaysLeft);

  const totals = useMemo(() => {
    const eligible = rows.filter((r) => r.Status?.toUpperCase() !== 'REVERSED');
    return {
      count: rows.length,
      eligible: eligible.reduce((a, r) => a + (r.EligibleITC ?? 0), 0),
      claimed: rows.filter((r) => r.Status?.toUpperCase() === 'CLAIMED').reduce((a, r) => a + (r.EligibleITC ?? 0), 0),
      unpaid: rows.filter((r) => !r.SupplierPaid).length,
    };
  }, [rows]);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ['itc-register'] });
    qc.invalidateQueries({ queryKey: ['itc-rule37'] });
    qc.invalidateQueries({ queryKey: ['itc-164'] });
  };

  const build = useMutation({
    mutationFn: async (): Promise<BuildResult> => {
      const res = await apiPost<BuildResult>('Itc/Build', { PeriodKey: period });
      if (!res.ok || !res.data) throw new Error(res.message || 'The register could not be built.');
      return res.data;
    },
    onSuccess: (r) => {
      toast.success(
        `${periodLabel(r.PeriodKey)}: ${r.EntriesCreated} created, ${r.EntriesUpdated} updated — claimable ${inr(r.TotalEligibleITC)}.`,
      );
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const markPaid = useMutation({
    mutationFn: async (entryId: number) => {
      const res = await apiPost('Itc/MarkPaid', { EntryID: entryId, PaidOn: null });
      if (!res.ok) throw new Error(res.message || 'The supplier could not be marked paid.');
    },
    onSuccess: () => {
      toast.success('Marked as paid — the Rule 37 clock stops on that one.');
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const claim = useMutation({
    mutationFn: async (): Promise<number> => {
      const res = await apiPost<{ Claimed: number } | number>('Itc/Claim', {
        EntryIDs: selected,
        ClaimPeriod: period,
      });
      if (!res.ok) throw new Error(res.message || 'The credit could not be claimed.');
      const d = res.data as { Claimed?: number } | number | null;
      return typeof d === 'number' ? d : (d?.Claimed ?? selected.length);
    },
    onSuccess: (count) => {
      toast.success(`Claimed ${count} credit(s) in ${periodLabel(period)}.`);
      setSelected([]);
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Reverse ─────────────────────────────────────────────────────────────────────────
  const [reverseFor, setReverseFor] = useState<ItcRow | null>(null);
  const [reason, setReason] = useState('Rule37');

  const reverse = useMutation({
    mutationFn: async () => {
      const res = await apiPost('Itc/Reverse', { EntryID: reverseFor?.ItcRegisterEntryID ?? 0, Reason: reason });
      if (!res.ok) throw new Error(res.message || 'The credit could not be reversed.');
    },
    onSuccess: () => {
      toast.success('Credit reversed.');
      setReverseFor(null);
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggle = (id: number) =>
    setSelected((s) => (s.includes(id) ? s.filter((x) => x !== id) : [...s, id]));

  const claimable = rows.filter((r) => ['ELIGIBLE', 'PENDING'].includes((r.Status ?? '').toUpperCase()));

  const download = () =>
    downloadCsv(
      rows.map((r) => ({
        Document: r.DocumentNo,
        Date: dateOnly(r.DocumentDate),
        Supplier: r.SupplierName,
        SupplierGSTIN: r.SupplierGSTIN,
        EligibleITC: r.EligibleITC,
        Status: r.Status,
        ReconBucket: r.ReconBucket,
        ClaimedInPeriod: r.ClaimedInPeriod,
        SupplierPaid: r.SupplierPaid ? 'Yes' : 'No',
        Rule37Due: dateOnly(r.Rule37DueDate),
        Section164Deadline: dateOnly(r.Section16_4Deadline),
      })),
      `itc-register-${period}.csv`,
    );

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Every credit we may claim, with the two clocks that can take it away"
        icon={pickIcon(def.title)}
        actions={
          <div className="flex gap-2">
            {mayMoveCredit && selected.length > 0 && (
              <Button onClick={() => claim.mutate()} loading={claim.isPending}>
                <HandCoins className="mr-1.5 size-4" />
                Claim {selected.length}
              </Button>
            )}
            {mayOperate && (
              <Button variant="outline" onClick={() => build.mutate()} loading={build.isPending}>
                <Hammer className="mr-1.5 size-4" />
                Rebuild register
              </Button>
            )}
          </div>
        }
      />

      <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
        <StatTile label="Entries" value={totals.count} icon={BadgeCheck} tone="text-brand-600" loading={register.isFetching} />
        <StatTile label="Claimable ITC" value={inr(totals.eligible)} icon={Wallet} tone="text-success-600" loading={register.isFetching} />
        <StatTile label="Already claimed" value={inr(totals.claimed)} icon={HandCoins} tone="text-slate-600" loading={register.isFetching} />
        <StatTile label="Supplier unpaid" value={totals.unpaid} icon={AlertTriangle} tone="text-warning-600" loading={register.isFetching} />
      </div>

      <Card>
        <CardBody className="flex flex-wrap items-end gap-3">
          <div className="w-44">
            <Select label="Period" value={period} onChange={(e) => { setPeriod(e.target.value); setSelected([]); }}>
              {periods.map((p) => (
                <option key={p} value={p}>{periodLabel(p)}</option>
              ))}
            </Select>
          </div>
          <div className="w-32">
            <Input label="Alert window" type="number" min={1} value={daysAhead} onChange={(e) => setDaysAhead(e.target.value)} hint="Days ahead" />
          </div>
          <div className="flex-1" />
          <Button variant="ghost" onClick={download} disabled={rows.length === 0}>
            <Download className="mr-1.5 size-4" />
            CSV
          </Button>
        </CardBody>
      </Card>

      {/* The deadlines, first — this is the screen's reason to exist. */}
      {alerts.length > 0 && (
        <Card>
          <CardBody className="pb-2">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
              <Clock className="size-4 text-warning-500" />
              Credits running out of time
            </div>
            <p className="mt-1 text-xs text-slate-400">
              Rule 37 means <em>pay the supplier</em> within 180 days or give the credit back. Section 16(4)
              means <em>claim it</em> before the cut-off or lose it. The action differs, so the rule is named.
            </p>
          </CardBody>
          <div className="overflow-x-auto px-3 pb-3">
            <table className="w-full min-w-[760px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Document</th>
                  <th className="px-3 py-2 font-medium">Supplier</th>
                  <th className="px-3 py-2 font-medium">Rule</th>
                  <th className="px-3 py-2 text-right font-medium">Credit</th>
                  <th className="px-3 py-2 font-medium">Deadline</th>
                  <th className="px-3 py-2 text-right font-medium">Time left</th>
                  <th className="px-3 py-2 font-medium">What to do</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {alerts.map((a) => (
                  <tr key={`${a.DeadlineKind}-${a.ItcRegisterEntryID}`} className="hover:bg-slate-50">
                    <td className="px-3 py-2">
                      <div className="font-mono text-sm font-semibold text-slate-800">{a.DocumentNo || `#${a.TaxInvoiceHeaderID}`}</div>
                      <div className="text-xs text-slate-400">{dateOnly(a.DocumentDate)}</div>
                    </td>
                    <td className="px-3 py-2">
                      <div className="max-w-[170px] truncate text-slate-700" title={a.SupplierName}>{a.SupplierName || '—'}</div>
                    </td>
                    <td className="px-3 py-2">
                      <Badge tone={a.DeadlineKind === 'Rule37' ? 'warning' : 'info'}>
                        {a.DeadlineKind === 'Rule37' ? 'Rule 37' : 'Sec 16(4)'}
                      </Badge>
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(a.EligibleITC)}</td>
                    <td className="px-3 py-2 text-slate-600">{dateOnly(a.DeadlineDate)}</td>
                    <td className="px-3 py-2 text-right">
                      <Badge tone={deadlineTone(a.DaysLeft, a.Passed)}>
                        {a.Passed ? `${Math.abs(a.DaysLeft)}d over` : `${a.DaysLeft}d left`}
                      </Badge>
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex items-center gap-2">
                        <span className="max-w-[180px] truncate text-xs text-slate-500" title={a.Action}>{a.Action}</span>
                        {mayOperate && a.DeadlineKind === 'Rule37' && (
                          <Button variant="ghost" onClick={() => markPaid.mutate(a.ItcRegisterEntryID)} loading={markPaid.isPending}>
                            Mark paid
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      <Card>
        {register.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : register.isError ? (
          <CardBody>
            <EmptyState title="Could not load" description={(register.error as Error).message} />
          </CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title={`Nothing in the register for ${periodLabel(period)}`}
              description={
                mayOperate
                  ? 'The register is built from the captured purchases. Capture the bills, then rebuild it here.'
                  : 'No credit has been registered for this period yet.'
              }
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[1000px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  {mayMoveCredit && (
                    <th className="px-4 py-2.5">
                      <input
                        type="checkbox"
                        className="size-4"
                        checked={claimable.length > 0 && selected.length === claimable.length}
                        onChange={(e) => setSelected(e.target.checked ? claimable.map((r) => r.ItcRegisterEntryID) : [])}
                        aria-label="Select all claimable"
                      />
                    </th>
                  )}
                  <th className="px-3 py-2.5 font-medium">Document</th>
                  <th className="px-3 py-2.5 font-medium">Supplier</th>
                  <th className="px-3 py-2.5 text-right font-medium">Credit</th>
                  <th className="px-3 py-2.5 font-medium">Status</th>
                  <th className="px-3 py-2.5 font-medium">Bucket</th>
                  <th className="px-3 py-2.5 font-medium">Rule 37</th>
                  <th className="px-3 py-2.5 font-medium">Sec 16(4)</th>
                  <th className="px-3 py-2.5 font-medium">Claimed in</th>
                  {mayMoveCredit && <th className="px-3 py-2.5" />}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => {
                  const isClaimable = ['ELIGIBLE', 'PENDING'].includes((r.Status ?? '').toUpperCase());
                  const r37 = daysTo(r.Rule37DueDate);
                  const d164 = daysTo(r.Section16_4Deadline);
                  return (
                    <tr key={r.ItcRegisterEntryID} className="hover:bg-slate-50">
                      {mayMoveCredit && (
                        <td className="px-4 py-2.5">
                          <input
                            type="checkbox"
                            className="size-4"
                            disabled={!isClaimable}
                            checked={selected.includes(r.ItcRegisterEntryID)}
                            onChange={() => toggle(r.ItcRegisterEntryID)}
                            aria-label={`Select ${r.DocumentNo}`}
                          />
                        </td>
                      )}
                      <td className="px-3 py-2.5">
                        <div className="font-mono text-sm font-semibold text-slate-800">{r.DocumentNo || `#${r.TaxInvoiceHeaderID}`}</div>
                        <div className="text-xs text-slate-400">{dateOnly(r.DocumentDate)}</div>
                      </td>
                      <td className="px-3 py-2.5">
                        <div className="max-w-[170px] truncate text-slate-700" title={r.SupplierName}>{r.SupplierName || '—'}</div>
                        <div className="font-mono text-xs text-slate-400">{r.SupplierGSTIN || 'no GSTIN'}</div>
                      </td>
                      <td className="px-3 py-2.5 text-right tabular-nums font-semibold text-slate-800">{inr(r.EligibleITC)}</td>
                      <td className="px-3 py-2.5"><Badge tone={itcStatusTone(r.Status)}>{r.Status || '—'}</Badge></td>
                      <td className="px-3 py-2.5 text-xs text-slate-500">{r.ReconBucket || '—'}</td>
                      <td className="px-3 py-2.5">
                        {r.SupplierPaid ? (
                          <Badge tone="success">Paid</Badge>
                        ) : (
                          <span className={cn('text-xs tabular-nums', r37 < 0 ? 'font-semibold text-danger-600' : r37 <= 30 ? 'text-warning-600' : 'text-slate-500')}>
                            {dateOnly(r.Rule37DueDate)}
                          </span>
                        )}
                      </td>
                      <td className="px-3 py-2.5">
                        <span className={cn('text-xs tabular-nums', d164 < 0 ? 'font-semibold text-danger-600' : d164 <= 30 ? 'text-warning-600' : 'text-slate-500')}>
                          {dateOnly(r.Section16_4Deadline)}
                        </span>
                      </td>
                      <td className="px-3 py-2.5 font-mono text-xs text-slate-500">
                        {r.ClaimedInPeriod ? periodLabel(r.ClaimedInPeriod) : '—'}
                      </td>
                      {mayMoveCredit && (
                        <td className="px-3 py-2.5 text-right">
                          {(r.Status ?? '').toUpperCase() !== 'REVERSED' && (
                            <button
                              type="button"
                              onClick={() => { setReverseFor(r); setReason('Rule37'); }}
                              className="inline-flex items-center gap-1 text-xs text-slate-400 hover:text-danger-600"
                            >
                              <Undo2 className="size-3.5" /> Reverse
                            </button>
                          )}
                        </td>
                      )}
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {!mayMoveCredit && (
        <p className="text-xs text-slate-400">
          {mayOperate
            ? 'Claiming and reversing a credit are limited to Admin — both move the return, and the list of who did it should be short. Rebuilding the register and marking a supplier paid are available to you.'
            : 'Read-only. Building the register, marking a supplier paid, claiming and reversing are all limited to Admin / Support — this screen is the alert, which is what it is for.'}
        </p>
      )}

      <Modal open={reverseFor !== null} onClose={() => setReverseFor(null)} title="Reverse this credit" size="sm">
        <div className="space-y-3">
          <p className="text-sm text-slate-600">
            {reverseFor?.DocumentNo} — {inr(reverseFor?.EligibleITC ?? 0)} from {reverseFor?.SupplierName || 'the supplier'}.
          </p>
          <Select label="Reason" value={reason} onChange={(e) => setReason(e.target.value)}>
            <option value="Rule37">Rule 37 — supplier not paid within 180 days</option>
            <option value="Section16_4">Section 16(4) — claim window closed</option>
            <option value="Manual">Manual adjustment</option>
          </Select>
          <p className="text-xs text-slate-400">
            A reversal changes declared input tax, so it is recorded against this entry with the reason above.
          </p>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setReverseFor(null)}>Cancel</Button>
            <Button variant="danger" onClick={() => reverse.mutate()} loading={reverse.isPending}>
              <RotateCcw className="mr-1.5 size-4" />
              Reverse
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
