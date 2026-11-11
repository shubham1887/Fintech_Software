import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { AlertTriangle, CheckCircle2, FilePlus2, Info, X } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { periodKeyOf, periodLabel, recentPeriods } from '@/lib/gstDocs';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { Modal } from '@/components/ui/Modal';
import { Card, CardBody } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Spinner } from '@/components/ui/Spinner';

/** One document the run would (or did) issue. Mirrors Fino_API `GeneratedDocument`. */
interface PreviewDoc {
  TaxInvoiceHeaderID: number;
  DocumentNo: string;
  DocumentType: string;
  DocumentDate: string;
  PartyUserID: number;
  PartyName: string;
  PartyGSTIN: string;
  PartyRegistrationStatus: string;
  SourceCount: number;
  GrossTotal: number;
  TaxableTotal: number;
  TaxTotal: number;
  GrandTotal: number;
  BillingBasis: string;
  Description: string;
  SACCode: string;
  ServiceName: string;
  GSTRatePercent: number;
  CGSTAmount: number;
  SGSTAmount: number;
  IGSTAmount: number;
  SupplyNature: string;
  PlaceOfSupplyStateCode: string;
}

/** Mirrors Fino_API `Responses/TaxInvoice/MonthlyInvoiceResult.cs`. */
interface MonthResult {
  PeriodKey: string;
  PeriodFrom: string;
  PeriodTo: string;
  CompanyGSTIN: string;
  DryRun: boolean;
  Issued: boolean;
  DocumentsGenerated: number;
  LinesGenerated: number;
  TaxableTotal: number;
  TaxTotal: number;
  SkippedAlreadyInvoiced: number;
  CancelledDocuments: number;
  Refused: boolean;
  RefusalReason: string;
  Consolidation: string;
  Blockers: string[];
  Warnings: string[];
  Documents: PreviewDoc[];
  PartialMonth: boolean;
}

/**
 * How many documents a party's top-ups become. Asked rather than assumed, because it is a
 * business decision with a tax consequence: one top-up a day for a month is thirty invoices to
 * one customer.
 */
const GROUPINGS: { value: string; label: string; hint: string }[] = [
  { value: 'MONTHLY', label: 'One invoice per customer for the month', hint: 'Each customer gets a single invoice covering every top-up in the month. The usual choice.' },
  { value: 'DAILY', label: 'One invoice per customer per day', hint: 'A customer gets one invoice for each day they topped up.' },
  { value: 'TRANSACTION', label: 'One invoice for every top-up', hint: 'A separate invoice for every single approved top-up.' },
  { value: 'AUTO', label: 'Decide per customer', hint: 'Per top-up for GST-registered customers, per day for everyone else.' },
];

const isTopUp = (d: PreviewDoc) => (d.ServiceName ?? '').toLowerCase().startsWith('wallet top');

/** "8 top-ups" / "282 transactions" — what the quantity on the invoice line counts. */
function quantityLabel(d: PreviewDoc): string {
  const n = d.SourceCount || 0;
  const unit = isTopUp(d) ? 'top-up' : 'transaction';
  return `${n.toLocaleString('en-IN')} ${unit}${n === 1 ? '' : 's'}`;
}

/** "18% — IGST" or "18% — CGST 9% + SGST 9%", from the heads the engine actually charged. */
function rateLabel(d: PreviewDoc): string {
  const r = d.GSTRatePercent || 0;
  if (!r) return 'No GST';
  if (d.IGSTAmount > 0) return `${r}% IGST`;
  return `${r}% (CGST ${r / 2}% + SGST ${r / 2}%)`;
}

/**
 * Generate sale invoices — as a page section, not a pop-up.
 *
 * Choosing a month shows, straight away and without pressing anything, every invoice that
 * would be raised: who it is for, what is billed, how many, at what GST rate, and what it comes
 * to. That preview is the same server code that issues, run without writing, so the table is
 * exactly what Issue will produce.
 *
 * Issue stays a separate, deliberate press: an invoice number, once allocated, goes into GSTR-1
 * and can only be withdrawn by credit note.
 *
 * The month-end schedule runs the same method on the 2nd of each month, so what this refuses,
 * the schedule refuses too — and for the same stated reason.
 */
export function GenerateInvoicesPanel({
  onIssued, onClose, initialPeriod,
}: {
  onIssued: () => void;
  onClose?: () => void;
  initialPeriod?: string;
}) {
  // The current month is offered too, invoiced up to yesterday — for the business that wants a
  // bill as soon as a top-up is approved rather than at month end.
  const periods = useMemo(() => recentPeriods(18), []);
  const thisMonth = periodKeyOf();
  const [period, setPeriod] = useState(initialPeriod && periods.includes(initialPeriod) ? initialPeriod : (periods[1] ?? periods[0] ?? ''));
  const [grouping, setGrouping] = useState('MONTHLY');
  const [done, setDone] = useState<MonthResult | null>(null);
  const [confirmRegroup, setConfirmRegroup] = useState(false);

  useEffect(() => { setDone(null); setConfirmRegroup(false); }, [period, grouping]);

  const body = (dryRun: boolean) => ({
    PeriodKey: period,
    DryRun: dryRun,
    Consolidation: grouping,
    // Remembered only on a real issue, so that next month's automatic run invoices the way
    // this person just chose to. A preview changes nothing.
    SaveAsDefault: !dryRun,
  });

  const preview = useQuery({
    queryKey: ['invoice-preview', period, grouping],
    enabled: period.length === 6,
    queryFn: async (): Promise<MonthResult> => {
      const res = await apiPost<MonthResult>('TaxInvoice/GenerateMonth', body(true));
      if (!res.ok || !res.data) throw new Error(res.message || 'The preview could not be built.');
      return res.data;
    },
  });

  const run = (endpoint: string) => async (): Promise<MonthResult> => {
    const res = await apiPost<MonthResult>(endpoint, body(false));
    if (!res.ok || !res.data) throw new Error(res.message || 'The invoices could not be issued.');
    return res.data;
  };
  const finish = (r: MonthResult) => { setDone(r); if (r.Issued) onIssued(); preview.refetch(); };
  const issueRun = useMutation({ mutationFn: run('TaxInvoice/GenerateMonth'), onSuccess: finish });
  // Re-issuing a month that is already invoiced. Separate from Issue on purpose: it withdraws
  // documents that exist, which is not something to reach by pressing the same button twice.
  const regroupRun = useMutation({ mutationFn: run('TaxInvoice/RegroupMonth'), onSuccess: finish });

  const shown = done ?? preview.data ?? null;
  const docs = shown?.Documents ?? [];
  const busy = issueRun.isPending || regroupRun.isPending;
  const error = (preview.error ?? issueRun.error ?? regroupRun.error) as Error | null;
  const canIssue = !done && !!preview.data && !preview.data.Refused && preview.data.DocumentsGenerated > 0 && !preview.isFetching;

  const totals = useMemo(() => docs.reduce(
    (a, d) => {
      a.taxable += d.TaxableTotal || 0;
      a.tax += d.TaxTotal || 0;
      a.total += d.GrandTotal || 0;
      return a;
    }, { taxable: 0, tax: 0, total: 0 }), [docs]);

  return (
    <Card className="border-brand-200">
      <CardBody className="space-y-4">
        <div className="flex items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-slate-900">Generate sale invoices</h3>
            <p className="mt-0.5 text-sm text-slate-500">
              Pick the month. The table shows every invoice that will be raised — customer, what is billed,
              how many, the GST rate and the amount — before anything is issued.
            </p>
          </div>
          {onClose && (
            <button type="button" onClick={onClose} className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600" aria-label="Close">
              <X className="size-5" />
            </button>
          )}
        </div>

        <div className="flex flex-wrap items-end gap-3">
          <div className="w-56">
            <Select label="Month" value={period} onChange={(e) => setPeriod(e.target.value)}>
              {periods.map((p) => (
                <option key={p} value={p}>{`${periodLabel(p)}${p === thisMonth ? ' (this month, so far)' : ''}`}</option>
              ))}
            </Select>
          </div>
          <div className="w-72">
            <Select label="How to group wallet top-ups" value={grouping} onChange={(e) => setGrouping(e.target.value)}>
              {GROUPINGS.map((g) => <option key={g.value} value={g.value}>{g.label}</option>)}
            </Select>
          </div>
          <Button onClick={() => issueRun.mutate()} loading={issueRun.isPending} disabled={!canIssue || busy}>
            <FilePlus2 className="size-4" />
            {canIssue ? `Issue ${preview.data!.DocumentsGenerated} invoice${preview.data!.DocumentsGenerated === 1 ? '' : 's'}` : 'Issue invoices'}
          </Button>
        </div>

        <p className="text-xs text-slate-500">
          {GROUPINGS.find((g) => g.value === grouping)?.hint}{' '}
          Service charges and commission are always one invoice per customer per service.
          {period === thisMonth && ' This month is invoiced up to yesterday; whatever comes later goes on another invoice at month end — nothing is billed twice.'}
        </p>

        {error && (
          <div className="flex items-start gap-2 rounded-lg border border-danger-200 bg-danger-50 p-3 text-sm text-danger-700">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" />
            {error.message}
          </div>
        )}

        {preview.isFetching && !shown ? (
          <div className="flex min-h-[18vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>
        ) : shown ? (
          <div className="space-y-3">
            {shown.Refused ? (
              <div className="rounded-lg border border-warning-200 bg-warning-50 p-3 text-sm text-warning-800">
                <div className="flex items-start gap-2 font-medium">
                  <AlertTriangle className="mt-0.5 size-4 shrink-0" />
                  Nothing can be issued for {periodLabel(shown.PeriodKey || period)} yet
                </div>
                <p className="mt-1">{shown.RefusalReason}</p>
                {shown.Blockers.length > 0 && (
                  <ul className="mt-2 list-inside list-disc space-y-0.5 text-xs">
                    {shown.Blockers.map((b) => <li key={b}>{b}</li>)}
                  </ul>
                )}
              </div>
            ) : (
              <>
                <div className={cn(
                  'flex items-start gap-2 rounded-lg border p-3 text-sm font-medium',
                  shown.Issued ? 'border-success-200 bg-success-50 text-success-800' : 'border-slate-200 bg-slate-50 text-slate-700',
                )}>
                  {shown.Issued ? <CheckCircle2 className="mt-0.5 size-4 shrink-0" /> : <Info className="mt-0.5 size-4 shrink-0" />}
                  <span>
                    {shown.Issued
                      ? `Issued ${shown.DocumentsGenerated} invoice${shown.DocumentsGenerated === 1 ? '' : 's'} for ${periodLabel(shown.PeriodKey)}.`
                      : shown.DocumentsGenerated === 0
                        ? shown.SkippedAlreadyInvoiced > 0
                          ? `Everything in ${periodLabel(shown.PeriodKey)} is already invoiced — nothing left to issue.`
                          : `There is nothing to invoice in ${periodLabel(shown.PeriodKey)}.`
                        : `${shown.DocumentsGenerated} invoice${shown.DocumentsGenerated === 1 ? '' : 's'} will be issued for ${periodLabel(shown.PeriodKey)}.`}
                    {shown.CancelledDocuments > 0 && ` ${shown.CancelledDocuments} earlier invoice${shown.CancelledDocuments === 1 ? ' was' : 's were'} cancelled and replaced.`}
                  </span>
                </div>

                {docs.length > 0 && (
                  <div className="overflow-x-auto rounded-lg border border-slate-200">
                    <table className="w-full min-w-[980px] text-sm">
                      <thead>
                        <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                          <th className="px-3 py-2 font-medium">{shown.Issued ? 'Invoice' : '#'}</th>
                          <th className="px-3 py-2 font-medium">Customer</th>
                          <th className="px-3 py-2 font-medium">Billed for</th>
                          <th className="px-3 py-2 text-right font-medium">Quantity</th>
                          <th className="px-3 py-2 text-right font-medium">Value</th>
                          <th className="px-3 py-2 font-medium">GST rate</th>
                          <th className="px-3 py-2 text-right font-medium">Taxable</th>
                          <th className="px-3 py-2 text-right font-medium">GST</th>
                          <th className="px-3 py-2 text-right font-medium">Invoice total</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-100">
                        {docs.map((d, i) => (
                          <tr key={`${d.PartyUserID}-${i}`} className="align-top">
                            <td className="px-3 py-2 font-mono text-xs text-slate-500">
                              {shown.Issued ? d.DocumentNo : i + 1}
                              <div className="font-sans text-[11px] text-slate-400">{dateOnly(d.DocumentDate)}</div>
                            </td>
                            <td className="px-3 py-2">
                              <div className="max-w-[190px] truncate font-medium text-slate-800" title={d.PartyName}>{d.PartyName || `User ${d.PartyUserID}`}</div>
                              <div className="text-xs text-slate-400">
                                {d.PartyGSTIN ? <span className="font-mono">{d.PartyGSTIN}</span> : 'Unregistered'}
                                {d.PlaceOfSupplyStateCode && ` · state ${d.PlaceOfSupplyStateCode}`}
                              </div>
                            </td>
                            <td className="px-3 py-2">
                              <div className="text-slate-700">{d.ServiceName || 'Service'}</div>
                              <div className="max-w-[240px] truncate text-xs text-slate-400" title={d.Description}>
                                {d.Description}{d.SACCode && ` · SAC ${d.SACCode}`}
                              </div>
                            </td>
                            <td className="whitespace-nowrap px-3 py-2 text-right tabular-nums text-slate-600">{quantityLabel(d)}</td>
                            <td className="px-3 py-2 text-right tabular-nums text-slate-600" title={isTopUp(d) ? 'Total of the top-ups' : 'Value of the transactions the charge was earned on'}>
                              {inr(d.GrossTotal)}
                            </td>
                            <td className="px-3 py-2">
                              <Badge tone={d.GSTRatePercent ? 'info' : 'neutral'} className="whitespace-nowrap">{rateLabel(d)}</Badge>
                            </td>
                            <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(d.TaxableTotal)}</td>
                            <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(d.TaxTotal)}</td>
                            <td className="px-3 py-2 text-right font-semibold tabular-nums text-slate-900">{inr(d.GrandTotal)}</td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot>
                        <tr className="border-t-2 border-slate-200 bg-slate-50 font-semibold text-slate-800">
                          <td className="px-3 py-2" colSpan={6}>{docs.length} invoice{docs.length === 1 ? '' : 's'}</td>
                          <td className="px-3 py-2 text-right tabular-nums">{inr(totals.taxable)}</td>
                          <td className="px-3 py-2 text-right tabular-nums">{inr(totals.tax)}</td>
                          <td className="px-3 py-2 text-right tabular-nums">{inr(totals.total)}</td>
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                )}

                {/* A month already invoiced under another grouping. Re-issuing is offered here
                    rather than hidden, but it is deliberately a second press with its own
                    warning, because it withdraws documents that already exist. */}
                {!done && shown.DocumentsGenerated === 0 && shown.SkippedAlreadyInvoiced > 0 && (
                  <div className="rounded-lg border border-slate-200 p-3">
                    <p className="text-xs text-slate-600">
                      To re-issue this month grouped as <strong>{GROUPINGS.find((g) => g.value === grouping)?.label.toLowerCase()}</strong>,
                      the existing invoices have to be cancelled and replaced.
                    </p>
                    {!confirmRegroup ? (
                      <Button variant="secondary" className="mt-2" onClick={() => setConfirmRegroup(true)}>Re-issue this month…</Button>
                    ) : (
                      <div className="mt-2 rounded-lg border border-warning-300 bg-warning-50 p-3">
                        <p className="flex items-start gap-2 text-xs text-warning-800">
                          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
                          This cancels the invoices already issued for {periodLabel(period)} and raises new ones. The old
                          numbers are kept and declared as cancelled — but if any of them was already sent to a customer,
                          send them the replacement too.
                        </p>
                        <div className="mt-2 flex gap-2">
                          <Button onClick={() => regroupRun.mutate()} loading={regroupRun.isPending} disabled={busy}>Cancel and re-issue</Button>
                          <Button variant="ghost" onClick={() => setConfirmRegroup(false)}>Keep as is</Button>
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </>
            )}

            {shown.Warnings.length > 0 && (
              <details className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm">
                <summary className="cursor-pointer font-medium text-slate-700">
                  {shown.Warnings.length} note{shown.Warnings.length === 1 ? '' : 's'} about this run
                </summary>
                <ul className="mt-2 list-inside list-disc space-y-1 text-xs text-slate-600">
                  {shown.Warnings.map((w) => <li key={w}>{w}</li>)}
                </ul>
              </details>
            )}
          </div>
        ) : null}
      </CardBody>
    </Card>
  );
}

/** The same generator in a pop-up, for screens that only need it occasionally. */
export function GenerateMonthDialog({
  open, onClose, onIssued, initialPeriod,
}: {
  open: boolean;
  onClose: () => void;
  onIssued: () => void;
  initialPeriod?: string;
}) {
  return (
    <Modal open={open} onClose={onClose} title="Sale invoices" size="xl">
      {open && <GenerateInvoicesPanel onIssued={onIssued} initialPeriod={initialPeriod} />}
    </Modal>
  );
}
