import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { AlertTriangle, Download, Info, Receipt } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr, apiDate } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';

interface TreatmentRow {
  Treatment: string; Kind: string; Source: string;
  BaseValue: number; GSTAmount: number; TransactionCount: number;
}
interface BasisRow {
  BillingBasis: string; SupplyNature: string;
  GrossValue: number; TaxableValue: number;
  CGSTAmount: number; SGSTAmount: number; IGSTAmount: number; CessAmount: number;
  TotalTax: number; TransactionCount: number;
}
interface ServiceRow {
  ServiceID: number; ServiceName: string; BillingBasis: string; SACCode: string;
  TransactionCount: number; Turnover: number;
  OutputTaxStamped: number; GSTOnChargesCollected: number; GSTOnAmountsPaid: number;
  Treatments: string[];
}
interface ReconNote { Level: string; Title: string; Amount: number; Detail: string }

interface Report {
  FromDate: string; ToDate: string; PeriodLabel: string;
  OutputTaxStamped: number; GSTCollectedOnCharges: number; GSTOnProviderCommission: number;
  InputCreditOnProviderCharges: number; InputCreditOnDownlineCommission: number;
  TotalOutput: number; TotalInputCredit: number; NetPayable: number;
  ByTreatment: TreatmentRow[]; ByBasis: BasisRow[]; ByService: ServiceRow[];
  SuccessCount: number; ChargeStampedCount: number; ChargeUnstampedCount: number;
  TaxStampedCount: number; TaxUnstampedCount: number;
  Notes: ReconNote[]; Summary: string;
}

/** The first of the current month. */
function monthStart(): string {
  const d = new Date();
  return apiDate(new Date(d.getFullYear(), d.getMonth(), 1));
}

const TREATMENT_TONE: Record<string, 'success' | 'info' | 'neutral' | 'warning'> = {
  INCLUSIVE: 'success',
  EXCLUSIVE: 'info',
  NONE: 'neutral',
  'NOT STAMPED': 'warning',
  '(unresolved)': 'warning',
};

/** Download whatever is on screen, because a desk reconciling a filing works in a spreadsheet. */
function toCsv(report: Report): string {
  const esc = (v: unknown) => {
    const s = String(v ?? '');
    return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
  };
  const lines: string[] = [];
  lines.push(`GST collection,${esc(report.PeriodLabel)}`);
  lines.push('');
  lines.push('Headline,Amount');
  lines.push(`Output tax stamped on sales,${report.OutputTaxStamped}`);
  lines.push(`GST on provider commission,${report.GSTOnProviderCommission}`);
  lines.push(`Total output,${report.TotalOutput}`);
  lines.push(`Input credit on provider charges,${report.InputCreditOnProviderCharges}`);
  lines.push(`Input credit on downline commission,${report.InputCreditOnDownlineCommission}`);
  lines.push(`Total input credit,${report.TotalInputCredit}`);
  lines.push(`Net payable,${report.NetPayable}`);
  lines.push('');
  lines.push('Treatment,Kind,Source,Base,GST,Transactions');
  for (const r of report.ByTreatment)
    lines.push([r.Treatment, r.Kind, r.Source, r.BaseValue, r.GSTAmount, r.TransactionCount].map(esc).join(','));
  lines.push('');
  lines.push('Billing basis,Supply nature,Gross,Taxable,CGST,SGST,IGST,Cess,Total tax,Transactions');
  for (const r of report.ByBasis)
    lines.push([r.BillingBasis, r.SupplyNature, r.GrossValue, r.TaxableValue, r.CGSTAmount, r.SGSTAmount,
      r.IGSTAmount, r.CessAmount, r.TotalTax, r.TransactionCount].map(esc).join(','));
  lines.push('');
  lines.push('Service,Basis,SAC,Turnover,Output tax,GST collected on charges,GST paid out,Transactions,Treatments');
  for (const r of report.ByService)
    lines.push([r.ServiceName, r.BillingBasis, r.SACCode, r.Turnover, r.OutputTaxStamped,
      r.GSTOnChargesCollected, r.GSTOnAmountsPaid, r.TransactionCount, r.Treatments.join(' / ')].map(esc).join(','));
  if (report.Notes.length > 0) {
    lines.push('');
    lines.push('Level,Difference,Amount,Detail');
    for (const n of report.Notes) lines.push([n.Level, n.Title, n.Amount, n.Detail].map(esc).join(','));
  }
  return lines.join('\n');
}

/**
 * GST Collection — what a period actually collected, and what of it is still to be paid.
 *
 * It is built around the question the desk asks out loud: "inclusive or exclusive, how much GST
 * did we collect?" So the first table is the treatment split, and the side of each row is decided
 * by which way the money went: a charge we took from a user carries OUR output tax, while a
 * commission we paid out carries GST that is ours to claim. The two are never added together.
 *
 * Beside it sits the output tax exactly as the returns see it, from the tax stamp the engine
 * wrote. Those two figures should agree, and where they do not this screen names the difference
 * with the reason and the fix rather than letting a total absorb it — a GST report that nets away
 * what it cannot explain is how a return gets filed short.
 */
export function GstCollectionScreen() {
  const toast = useToast();
  const [fromDate, setFromDate] = useState(monthStart);
  const [toDate, setToDate] = useState(() => apiDate(new Date()));
  const [serviceId, setServiceId] = useState('');

  const services = useQuery({
    queryKey: ['fk', 'Service/GetAll'],
    queryFn: () => fetchList('Service/GetAll', { IsActive: true, PageNumber: 1, PageSize: 500 }),
    staleTime: 60_000,
  });

  const { data, isFetching, refetch } = useQuery<Report>({
    queryKey: ['gst-collection', fromDate, toDate, serviceId],
    queryFn: async () => {
      const res = await apiPost<Report>('GstCollection/Report', {
        FromDate: fromDate || null,
        ToDate: toDate || null,
        ServiceID: Number(serviceId) || 0,
      });
      if (!res.ok) throw new Error(res.message || 'Could not build the GST report.');
      return res.data as Report;
    },
  });

  const download = () => {
    if (!data) return;
    const blob = new Blob([toCsv(data)], { type: 'text/csv;charset=utf-8;' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `gst-collection-${fromDate}-to-${toDate}.csv`;
    a.click();
    URL.revokeObjectURL(a.href);
    toast.success('Downloaded.');
  };

  const outputRows = useMemo(() => (data?.ByTreatment ?? []).filter((r) => r.Kind === 'OUTPUT'), [data]);
  const inputRows = useMemo(() => (data?.ByTreatment ?? []).filter((r) => r.Kind === 'INPUT'), [data]);

  const coverage = data
    ? `${data.ChargeStampedCount} of ${data.SuccessCount} successful transactions carry a charge breakdown`
    : '';

  return (
    <>
      <PageHeader
        title="GST Collection"
        subtitle="Inclusive or exclusive — what was collected, what may be claimed, and what is left to pay"
      />

      <Card className="mb-4">
        <CardBody className="flex flex-wrap items-end gap-3">
          <Input label="From" type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
          <Input label="To" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
          <div className="w-56">
            <Select label="Service" value={serviceId} onChange={(e) => setServiceId(e.target.value)}>
              <option value="">Every service</option>
              {(services.data ?? []).map((s) => (
                <option key={String(getField(s, 'ServiceID'))} value={String(getField(s, 'ServiceID'))}>
                  {String(getField(s, 'Name') ?? '')}
                </option>
              ))}
            </Select>
          </div>
          <Button variant="outline" loading={isFetching} onClick={() => refetch()}>Refresh</Button>
          <div className="flex-1" />
          <Button variant="secondary" disabled={!data} onClick={download}>
            <Download className="size-4" /> CSV
          </Button>
        </CardBody>
      </Card>

      {isFetching && !data ? (
        <div className="flex justify-center py-16"><Spinner /></div>
      ) : !data || data.SuccessCount === 0 ? (
        <EmptyState
          icon={Receipt}
          title="Nothing in this period"
          description={data?.Summary ?? 'No successful transaction in the dates selected.'}
        />
      ) : (
        <>
          {/* ── The headline ──────────────────────────────────────────────── */}
          <div className="mb-4 grid grid-cols-2 gap-3 lg:grid-cols-4">
            {[
              { label: 'Output GST', value: data.TotalOutput, tone: 'text-slate-900', hint: 'What we owe on our own sales' },
              { label: 'Input credit', value: data.TotalInputCredit, tone: 'text-info-600', hint: 'GST inside what we paid out' },
              { label: 'Net payable', value: data.NetPayable, tone: 'text-danger-600', hint: 'Output less input credit' },
              { label: 'Collected on charges', value: data.GSTCollectedOnCharges, tone: 'text-success-700', hint: 'GST inside the charges users paid' },
            ].map((t) => (
              <div key={t.label} className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
                <p className="text-xs text-slate-500">{t.label}</p>
                <p className={cn('mt-1 font-display text-xl font-bold tabular-nums', t.tone)}>{inr(t.value)}</p>
                <p className="mt-0.5 text-xs text-slate-400">{t.hint}</p>
              </div>
            ))}
          </div>

          {/* ── Differences first: a figure not being declared outranks every total ── */}
          {data.Notes.length > 0 && (
            <div className="mb-4 space-y-2">
              {data.Notes.map((n, i) => (
                <div
                  key={i}
                  className={cn(
                    'rounded-card border p-3',
                    n.Level === 'WARNING' ? 'border-warning-200 bg-warning-50/60' : 'border-slate-200/70 bg-slate-50/60',
                  )}
                >
                  <p className={cn(
                    'flex items-start gap-1.5 text-sm font-semibold',
                    n.Level === 'WARNING' ? 'text-warning-800' : 'text-slate-700',
                  )}>
                    {n.Level === 'WARNING'
                      ? <AlertTriangle className="mt-0.5 size-4 shrink-0" />
                      : <Info className="mt-0.5 size-4 shrink-0" />}
                    {n.Title}
                  </p>
                  <p className="mt-1 pl-5 text-xs leading-relaxed text-slate-600">{n.Detail}</p>
                </div>
              ))}
            </div>
          )}

          {/* ── Inclusive / exclusive ─────────────────────────────────────── */}
          <h2 className="mb-2 font-display text-base font-bold text-slate-900">Inclusive or exclusive</h2>
          <div className="mb-6 overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
            <div className="overflow-x-auto">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-3 py-3">Treatment</th>
                    <th className="px-3 py-3">Source</th>
                    <th className="px-3 py-3 text-right">Charge without tax</th>
                    <th className="px-3 py-3 text-right">GST</th>
                    <th className="px-3 py-3 text-right">Txns</th>
                  </tr>
                </thead>
                <tbody>
                  {outputRows.length > 0 && (
                    <tr className="bg-slate-50/40">
                      <td colSpan={5} className="px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-slate-500">
                        Ours to pay — output GST
                      </td>
                    </tr>
                  )}
                  {outputRows.map((r, i) => (
                    <tr key={`o-${i}`} className="border-b border-slate-100">
                      <td className="px-3 py-2.5">
                        <Badge tone={TREATMENT_TONE[r.Treatment] ?? 'neutral'}>{r.Treatment}</Badge>
                      </td>
                      <td className="px-3 py-2.5 text-slate-700">{r.Source}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.BaseValue)}</td>
                      <td className="px-3 py-2.5 text-right font-semibold tabular-nums">{inr(r.GSTAmount)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-500">{r.TransactionCount}</td>
                    </tr>
                  ))}
                  {inputRows.length > 0 && (
                    <tr className="bg-slate-50/40">
                      <td colSpan={5} className="px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-slate-500">
                        Ours to claim — input credit
                      </td>
                    </tr>
                  )}
                  {inputRows.map((r, i) => (
                    <tr key={`i-${i}`} className="border-b border-slate-100 last:border-0">
                      <td className="px-3 py-2.5">
                        <Badge tone={TREATMENT_TONE[r.Treatment] ?? 'neutral'}>{r.Treatment}</Badge>
                      </td>
                      <td className="px-3 py-2.5 text-slate-700">{r.Source}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.BaseValue)}</td>
                      <td className="px-3 py-2.5 text-right font-semibold tabular-nums text-info-600">{inr(r.GSTAmount)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-500">{r.TransactionCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* ── Output tax as the returns see it ──────────────────────────── */}
          <h2 className="mb-2 font-display text-base font-bold text-slate-900">
            Output tax as the returns see it
          </h2>
          <div className="mb-6 overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[48rem] border-collapse text-sm">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-3 py-3">Billing basis</th>
                    <th className="px-3 py-3">Supply</th>
                    <th className="px-3 py-3 text-right">Taxable</th>
                    <th className="px-3 py-3 text-right">CGST</th>
                    <th className="px-3 py-3 text-right">SGST</th>
                    <th className="px-3 py-3 text-right">IGST</th>
                    <th className="px-3 py-3 text-right">Total tax</th>
                    <th className="px-3 py-3 text-right">Txns</th>
                  </tr>
                </thead>
                <tbody>
                  {data.ByBasis.map((r, i) => (
                    <tr key={i} className="border-b border-slate-100 last:border-0">
                      <td className="px-3 py-2.5 font-medium text-slate-800">{r.BillingBasis}</td>
                      <td className="px-3 py-2.5">
                        <Badge tone={r.SupplyNature === 'INTERSTATE' ? 'info' : 'neutral'}>{r.SupplyNature}</Badge>
                      </td>
                      <td className="px-3 py-2.5 text-right tabular-nums">{inr(r.TaxableValue)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.CGSTAmount)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.SGSTAmount)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.IGSTAmount)}</td>
                      <td className="px-3 py-2.5 text-right font-semibold tabular-nums">{inr(r.TotalTax)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-500">{r.TransactionCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* ── Per service ───────────────────────────────────────────────── */}
          <h2 className="mb-2 font-display text-base font-bold text-slate-900">Per service</h2>
          <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[52rem] border-collapse text-sm">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-3 py-3">Service</th>
                    <th className="px-3 py-3">Basis</th>
                    <th className="px-3 py-3">SAC</th>
                    <th className="px-3 py-3 text-right">Turnover</th>
                    <th className="px-3 py-3 text-right">Output tax</th>
                    <th className="px-3 py-3 text-right">Collected on charges</th>
                    <th className="px-3 py-3 text-right">Paid out</th>
                    <th className="px-3 py-3 text-right">Txns</th>
                  </tr>
                </thead>
                <tbody>
                  {data.ByService.map((r) => (
                    <tr key={r.ServiceID} className="border-b border-slate-100 last:border-0">
                      <td className="px-3 py-2.5">
                        <p className="font-medium text-slate-800">{r.ServiceName}</p>
                        {r.Treatments.length > 0 && (
                          <p className="mt-0.5 text-xs text-slate-400">{r.Treatments.join(' / ')}</p>
                        )}
                      </td>
                      <td className="px-3 py-2.5 text-xs text-slate-600">{r.BillingBasis}</td>
                      <td className="px-3 py-2.5">
                        {r.SACCode === '(not set)'
                          ? <Badge tone="warning">not set</Badge>
                          : <span className="font-mono text-xs text-slate-600">{r.SACCode}</span>}
                      </td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">{inr(r.Turnover)}</td>
                      <td className="px-3 py-2.5 text-right font-semibold tabular-nums">{inr(r.OutputTaxStamped)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-success-700">{inr(r.GSTOnChargesCollected)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-info-600">{inr(r.GSTOnAmountsPaid)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-500">{r.TransactionCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          <p className="mt-3 flex items-start gap-1.5 text-xs text-slate-500">
            <Info className="mt-0.5 size-3.5 shrink-0 text-slate-400" />
            <span>
              {coverage}. GST on a charge the user paid us is <span className="font-medium text-slate-600">output</span> tax;
              GST inside a commission or charge we paid out is <span className="font-medium text-slate-600">input credit</span>,
              and the two are never added together. Only successful transactions appear — a failed one was refunded in
              full, charges and all, so it owes nothing.
            </span>
          </p>
        </>
      )}
    </>
  );
}
