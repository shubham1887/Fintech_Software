import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  AlertTriangle, BookOpenText, CheckCircle2, Clock, FileText, Inbox, Landmark,
  Scale, ScrollText, ShieldCheck,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { periodKeyOf, periodLabel, useCompanyGstin } from '@/lib/gstDocs';
import { useAuth } from '@/features/auth/AuthContext';
import { Card, CardBody } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Spinner } from '@/components/ui/Spinner';

/**
 * The accountant's home.
 *
 * Role 9 used to land on the admin dashboard, which leads with Provider Stock and the network
 * wallet balances — every one of which reads zero for an accountant, because the role holds no
 * wallet. A dashboard whose headline figures are all zero teaches people to ignore dashboards.
 *
 * This one answers the questions the role actually has on any given morning: what has not been
 * filed, what credit is about to run out, what has not been reconciled, and whether the books
 * still tie out. Every tile is a link to the screen that settles it.
 */

interface IntegrityCheck { Check: string; Difference: number; Ok: boolean }
interface IntegrityReport { RanAt: string; AllOk: boolean; Checks: IntegrityCheck[] }
interface OutboxRow { AcctPostingOutboxID: number; Status: string }
interface FilingRow { ReturnType: string; PeriodKey: string; Status: string }
interface DeadlineAlert {
  ItcRegisterEntryID: number; DocumentNo: string; SupplierName: string;
  EligibleITC: number; DeadlineKind: string; DeadlineDate: string; DaysLeft: number; Passed: boolean;
}
interface Gstr3bLite {
  PeriodLabel: string; TotalPayable: number;
  VarianceTaxableValue: number; AgreesWithGstr1: boolean;
}
interface BankReconLite { Difference: number; Explained: boolean; Rows: { Bucket: string }[] }
interface BankAccountRow { BankAccountID: number }

/** The three months an accountant is realistically still working on. */
function recentThree(): string[] {
  const out: string[] = [];
  const d = new Date();
  d.setDate(1);
  for (let i = 0; i < 3; i++) {
    out.push(periodKeyOf(d));
    d.setMonth(d.getMonth() - 1);
  }
  return out;
}

export function AccountantDashboardPage() {
  const { user } = useAuth();
  const { gstin, missing: gstinMissing } = useCompanyGstin();
  const months = useMemo(recentThree, []);
  const thisPeriod = months[0];
  const enabled = gstin.length === 15;

  const integrity = useQuery({
    queryKey: ['acct-integrity'],
    queryFn: async (): Promise<IntegrityReport> => {
      const res = await apiPost<IntegrityReport>('AcctIntegrity/RunAll', {});
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not run the checks.');
      return res.data;
    },
  });

  const outbox = useQuery({
    queryKey: ['acct-outbox', ''],
    queryFn: async (): Promise<OutboxRow[]> => {
      const res = await apiPost<OutboxRow[]>('AcctLedger/Outbox', { Status: '' });
      if (!res.ok) throw new Error(res.message || 'Could not read the outbox.');
      return res.data ?? [];
    },
  });

  const filings = useQuery({
    queryKey: ['gst-filings', gstin],
    enabled,
    queryFn: async (): Promise<FilingRow[]> => {
      const res = await apiPost<FilingRow[]>('GstReturn/Filings', { CompanyGSTIN: gstin });
      if (!res.ok) throw new Error(res.message || 'Could not read the saved returns.');
      return res.data ?? [];
    },
  });

  const summary = useQuery({
    queryKey: ['gstr3b', gstin, thisPeriod],
    enabled,
    queryFn: async (): Promise<Gstr3bLite> => {
      const res = await apiPost<Gstr3bLite>('GstReturn/Gstr3b', {
        CompanyGSTIN: gstin, PeriodKey: thisPeriod, B2CLThreshold: 0,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not build the summary.');
      return res.data;
    },
  });

  const rule37 = useQuery({
    queryKey: ['itc-rule37', '30'],
    queryFn: async (): Promise<DeadlineAlert[]> => {
      const res = await apiPost<DeadlineAlert[]>('Itc/Rule37Alerts', { DaysAhead: 30 });
      if (!res.ok) throw new Error(res.message || 'Could not read the alerts.');
      return res.data ?? [];
    },
  });

  const s164 = useQuery({
    queryKey: ['itc-164', '60'],
    queryFn: async (): Promise<DeadlineAlert[]> => {
      const res = await apiPost<DeadlineAlert[]>('Itc/Section164Alerts', { DaysAhead: 60 });
      if (!res.ok) throw new Error(res.message || 'Could not read the alerts.');
      return res.data ?? [];
    },
  });

  const accounts = useQuery({
    queryKey: ['bank-accounts'],
    queryFn: async (): Promise<BankAccountRow[]> => {
      const res = await apiPost<BankAccountRow[]>('BankAccount/Select', {});
      if (!res.ok) throw new Error(res.message || 'Could not read the bank accounts.');
      return res.data ?? [];
    },
  });

  const firstAccount = accounts.data?.[0]?.BankAccountID ?? 0;
  const bank = useQuery({
    queryKey: ['bank-recon-dash', firstAccount],
    enabled: firstAccount > 0,
    queryFn: async (): Promise<BankReconLite> => {
      const now = new Date();
      const from = new Date(now.getFullYear(), now.getMonth(), 1);
      const iso = (d: Date) => d.toISOString().slice(0, 10);
      const res = await apiPost<BankReconLite>('BankRecon/Preview', {
        BankAccountID: firstAccount, PeriodFrom: iso(from), PeriodTo: iso(now),
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not reconcile.');
      return res.data;
    },
  });

  const alerts = [...(rule37.data ?? []), ...(s164.data ?? [])].sort((a, b) => a.DaysLeft - b.DaysLeft);
  const failedOutbox = (outbox.data ?? []).filter((o) => o.Status?.toUpperCase() === 'FAILED').length;
  const pendingOutbox = (outbox.data ?? []).filter((o) => o.Status?.toUpperCase() === 'PENDING').length;
  const unreconciled = (bank.data?.Rows ?? []).filter((r) => r.Bucket !== 'Matched').length;

  const filedSet = new Set(
    (filings.data ?? []).filter((f) => f.Status === 'Filed').map((f) => `${f.ReturnType}:${f.PeriodKey}`),
  );
  const unfiled = months.flatMap((p) =>
    (['GSTR1', 'GSTR3B'] as const)
      .filter((t) => !filedSet.has(`${t}:${p}`))
      .map((t) => ({ period: p, type: t })),
  );

  return (
    <div className="space-y-4 p-4 lg:p-6">
      <div>
        <h1 className="text-xl font-semibold text-slate-900">
          Good day{user?.PersonName ? `, ${user.PersonName}` : ''}
        </h1>
        <p className="text-sm text-slate-500">
          The accounting desk — what is outstanding, what is running out of time, and whether the books
          still tie out.
        </p>
      </div>

      {gstinMissing && (
        <div className="flex gap-2.5 rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-sm text-danger-600">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>The company GSTIN is not set</strong>, so no return can be built and no document can be
            issued. An admin fills it in on Invoice Settings.
          </p>
        </div>
      )}

      {/* ── The four numbers worth knowing before anything else ───────────────────────── */}
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <Tile
          to="/s/GstReturns"
          icon={FileText}
          label="Returns not filed"
          value={enabled ? String(unfiled.length) : '—'}
          tone={unfiled.length === 0 ? 'success' : 'warning'}
          hint={enabled ? `across ${months.length} recent months` : 'needs the company GSTIN'}
          loading={filings.isPending}
        />
        <Tile
          to="/s/GstReturns"
          icon={Clock}
          label="Credits running out"
          value={String(alerts.length)}
          tone={alerts.some((a) => a.Passed) ? 'danger' : alerts.length > 0 ? 'warning' : 'success'}
          hint="Rule 37 and Section 16(4)"
          loading={rule37.isPending || s164.isPending}
        />
        <Tile
          to="/s/BankRecon"
          icon={Landmark}
          label="Unreconciled items"
          value={firstAccount > 0 ? String(unreconciled) : '—'}
          tone={unreconciled === 0 ? 'success' : 'warning'}
          hint="this month's bank statement"
          loading={bank.isPending && firstAccount > 0}
        />
        <Tile
          to="/s/LedgerVouchers"
          icon={Inbox}
          label="Postings failed"
          value={String(failedOutbox)}
          tone={failedOutbox > 0 ? 'danger' : pendingOutbox > 0 ? 'warning' : 'success'}
          hint={pendingOutbox > 0 ? `${pendingOutbox} still waiting` : 'outbox is clear'}
          loading={outbox.isPending}
        />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        {/* ── This month's position ──────────────────────────────────────────────────── */}
        <Card>
          <CardBody className="pb-2">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
              <Scale className="size-4 text-slate-400" />
              {periodLabel(thisPeriod)} — where the return stands
            </div>
          </CardBody>
          <div className="px-5 pb-4">
            {!enabled ? (
              <p className="text-sm text-slate-400">Set the company GSTIN to see this.</p>
            ) : summary.isPending ? (
              <div className="flex min-h-[80px] items-center justify-center"><Spinner className="size-5 text-brand-500" /></div>
            ) : summary.isError ? (
              <p className="text-sm text-slate-400">{(summary.error as Error).message}</p>
            ) : (
              <div className="space-y-3">
                <div className="flex items-baseline justify-between">
                  <span className="text-sm text-slate-600">Tax payable in cash</span>
                  <span className="text-lg font-semibold tabular-nums text-slate-900">{inr(summary.data!.TotalPayable)}</span>
                </div>
                <div
                  className={cn(
                    'flex gap-2 rounded-lg border p-2.5 text-xs',
                    summary.data!.AgreesWithGstr1
                      ? 'border-success-500/30 bg-success-50 text-success-700'
                      : 'border-danger-500/30 bg-danger-50 text-danger-700',
                  )}
                >
                  {summary.data!.AgreesWithGstr1
                    ? <CheckCircle2 className="mt-0.5 size-3.5 shrink-0" />
                    : <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />}
                  <p>
                    {summary.data!.AgreesWithGstr1
                      ? 'GSTR-3B agrees with GSTR-1 for this period.'
                      : `3B and GSTR-1 differ by ${inr(summary.data!.VarianceTaxableValue)} of taxable value — settle it before filing.`}
                  </p>
                </div>
                <Link to="/s/GstReturns" className="inline-flex items-center gap-1 text-sm text-brand-600 hover:underline">
                  <ScrollText className="size-3.5" /> Open the returns
                </Link>
              </div>
            )}
          </div>
        </Card>

        {/* ── Integrity ──────────────────────────────────────────────────────────────── */}
        <Card>
          <CardBody className="pb-2">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
              <ShieldCheck className="size-4 text-slate-400" />
              Do the books still tie out?
            </div>
          </CardBody>
          <div className="px-5 pb-4">
            {integrity.isPending ? (
              <div className="flex min-h-[80px] items-center justify-center"><Spinner className="size-5 text-brand-500" /></div>
            ) : integrity.isError ? (
              <p className="text-sm text-slate-400">{(integrity.error as Error).message}</p>
            ) : (
              <div className="space-y-2">
                {integrity.data!.Checks.map((c) => (
                  <div key={c.Check} className="flex items-center gap-2 text-sm">
                    {c.Ok
                      ? <CheckCircle2 className="size-4 shrink-0 text-success-600" />
                      : <AlertTriangle className="size-4 shrink-0 text-danger-600" />}
                    <span className="flex-1 truncate text-slate-600">{c.Check}</span>
                    <span className={cn('tabular-nums text-xs', c.Ok ? 'text-slate-400' : 'font-semibold text-danger-600')}>
                      {inr(c.Difference)}
                    </span>
                  </div>
                ))}
                <Link to="/s/TrialBalance" className="inline-flex items-center gap-1 pt-1 text-sm text-brand-600 hover:underline">
                  <BookOpenText className="size-3.5" /> Trial balance &amp; integrity
                </Link>
              </div>
            )}
          </div>
        </Card>
      </div>

      {/* ── Deadlines ────────────────────────────────────────────────────────────────── */}
      {alerts.length > 0 && (
        <Card>
          <CardBody className="pb-2">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
              <Clock className="size-4 text-warning-500" />
              Credits running out of time
            </div>
            <p className="mt-1 text-xs text-slate-400">
              Rule 37 means pay the supplier within 180 days or give the credit back. Section 16(4) means
              claim it before the window closes or lose it.
            </p>
          </CardBody>
          <div className="overflow-x-auto px-3 pb-3">
            <table className="w-full min-w-[640px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Document</th>
                  <th className="px-3 py-2 font-medium">Supplier</th>
                  <th className="px-3 py-2 font-medium">Rule</th>
                  <th className="px-3 py-2 text-right font-medium">Credit</th>
                  <th className="px-3 py-2 font-medium">Deadline</th>
                  <th className="px-3 py-2 text-right font-medium">Time left</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {alerts.slice(0, 8).map((a) => (
                  <tr key={`${a.DeadlineKind}-${a.ItcRegisterEntryID}`} className="hover:bg-slate-50">
                    <td className="px-3 py-2 font-mono text-xs text-slate-700">{a.DocumentNo}</td>
                    <td className="px-3 py-2"><span className="block max-w-[180px] truncate text-slate-600">{a.SupplierName}</span></td>
                    <td className="px-3 py-2">
                      <Badge tone={a.DeadlineKind === 'Rule37' ? 'warning' : 'info'}>
                        {a.DeadlineKind === 'Rule37' ? 'Rule 37' : 'Sec 16(4)'}
                      </Badge>
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(a.EligibleITC)}</td>
                    <td className="px-3 py-2 text-xs text-slate-500">{dateOnly(a.DeadlineDate)}</td>
                    <td className="px-3 py-2 text-right">
                      <Badge tone={a.Passed ? 'danger' : a.DaysLeft <= 7 ? 'warning' : 'neutral'}>
                        {a.Passed ? `${Math.abs(a.DaysLeft)}d over` : `${a.DaysLeft}d left`}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {alerts.length > 8 && (
              <Link to="/s/GstReturns" className="inline-block px-3 pt-2 text-sm text-brand-600 hover:underline">
                {alerts.length - 8} more in the ITC register
              </Link>
            )}
          </div>
        </Card>
      )}

      {/* ── What is not filed ────────────────────────────────────────────────────────── */}
      {enabled && unfiled.length > 0 && (
        <Card>
          <CardBody className="pb-2">
            <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
              <FileText className="size-4 text-slate-400" />
              Returns with no filed snapshot
            </div>
            <p className="mt-1 text-xs text-slate-400">
              A month only counts as filed once its snapshot is saved and marked so — that is what the
              annual return reconciles against.
            </p>
          </CardBody>
          <div className="flex flex-wrap gap-2 px-5 pb-4">
            {unfiled.map((u) => (
              <Link
                key={`${u.type}-${u.period}`}
                to="/s/GstReturns"
                className="rounded-lg border border-warning-500/30 bg-warning-50 px-3 py-1.5 text-sm text-warning-700 hover:border-warning-500/60"
              >
                {u.type === 'GSTR1' ? 'GSTR-1' : 'GSTR-3B'} · {periodLabel(u.period)}
              </Link>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
}

function Tile({
  to, icon: Icon, label, value, tone, hint, loading,
}: {
  to: string;
  icon: typeof Clock;
  label: string;
  value: string;
  tone: 'success' | 'warning' | 'danger';
  hint: string;
  loading?: boolean;
}) {
  const ring = tone === 'danger' ? 'border-danger-500/30' : tone === 'warning' ? 'border-warning-500/30' : 'border-slate-200';
  const colour = tone === 'danger' ? 'text-danger-600' : tone === 'warning' ? 'text-warning-600' : 'text-success-600';
  return (
    <Link to={to} className={cn('rounded-card border bg-white p-4 transition hover:border-slate-300', ring)}>
      <div className="flex items-center gap-2">
        <Icon className={cn('size-4', colour)} />
        <span className="text-xs font-medium uppercase tracking-wide text-slate-400">{label}</span>
      </div>
      {loading ? (
        <div className="mt-2 h-7"><Spinner className="size-5 text-brand-500" /></div>
      ) : (
        <p className={cn('mt-1.5 text-2xl font-semibold tabular-nums', colour)}>{value}</p>
      )}
      <p className="mt-0.5 text-xs text-slate-400">{hint}</p>
    </Link>
  );
}
