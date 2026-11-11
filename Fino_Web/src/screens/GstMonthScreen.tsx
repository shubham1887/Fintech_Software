import { useMemo, useRef, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, CheckCircle2, ChevronDown, ChevronRight, Circle, ExternalLink, FileJson,
  FilePlus2, FileSpreadsheet, IndianRupee, Receipt, ShoppingCart, Upload, Wallet,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly } from '@/lib/format';
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
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { pickIcon } from '@/components/layout/iconMap';
import { GenerateMonthDialog } from './GenerateMonthDialog';
import { GstReturnTables } from './GstReturnsScreen';
import type { CustomScreenProps } from './registry';

/** Mirrors Fino_API `GstMonthSummary`. */
interface PurchaseRow {
  TaxInvoiceHeaderID: number; DocumentNo: string; DocumentDate: string; SupplierName: string;
  SupplierGSTIN: string; TaxableValue: number; Tax: number; Total: number; Credit: number; CreditStatus: string;
}
interface MissingBill { SupplierName: string; SupplierGSTIN: string; DocumentNo: string; DocumentDate: string; Tax: number }
interface Summary {
  CompanyGSTIN: string; CompanyName: string; PeriodKey: string; PeriodLabel: string; MonthStillOpen: boolean;
  PortalUsername: string; Gstr1DueDate: string; Gstr3bDueDate: string;
  SaleInvoiceCount: number; SaleInvoiceValue: number; SaleTaxableValue: number;
  OutputIGST: number; OutputCGST: number; OutputSGST: number; OutputTax: number;
  PendingInvoiceCount: number; PendingInvoiceTaxable: number; PendingInvoiceTax: number; PendingInvoiceBlocker: string;
  PurchaseCount: number; PurchaseTaxableValue: number; PurchaseTax: number;
  CreditConfirmed: number; CreditWaiting: number; Purchases: PurchaseRow[];
  TwoBImported: boolean; TwoBImportedOn: string | null; TwoBLineCount: number; TwoBTax: number;
  TwoBMatched: number; TwoBMissingIn2B: number; TwoBMissingInBooks: number; TwoBMismatch: number;
  TwoBMissingBills: MissingBill[];
  OpeningCredit: number; CreditUsedThisMonth: number;
  PayableIGST: number; PayableCGST: number; PayableSGST: number; TotalPayable: number; ClosingCredit: number;
  PayableIfCreditConfirmed: number; ClosingCreditIfConfirmed: number;
  Gstr1Filed: boolean; Gstr1FiledOn: string | null; Gstr1Ack: string;
  Gstr3bFiled: boolean; Gstr3bFiledOn: string | null; Gstr3bAck: string;
  AgreesWithGstr1: boolean; Warnings: string[];
}
interface TwoBResult {
  PeriodKey: string; LinesInFile: number; TaxInFile: number; Matched: number; MissingIn2B: number;
  MissingInBooks: number; Mismatch: number; CreditsClaimed: number; CreditClaimedValue: number; Warnings: string[];
}

const PORTAL_URL = 'https://services.gst.gov.in/services/login';

function saveBase64(base64: string, fileName: string, contentType: string) {
  const bin = atob(base64);
  const bytes = new Uint8Array(bin.length);
  for (let i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
  const url = URL.createObjectURL(new Blob([bytes], { type: contentType }));
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}

function creditTone(status: string): 'success' | 'warning' | 'danger' | 'neutral' {
  if (status === 'Confirmed') return 'success';
  if (status === 'No credit') return 'neutral';
  if (status === 'Reversed' || status === 'Tax differs from 2B') return 'danger';
  return 'warning';
}

/**
 * GST Returns — one month, top to bottom.
 *
 * The page is the order a person actually deals with a month in: what was sold, what was
 * bought, what the government says the suppliers filed (the 2B file), what that leaves to pay,
 * and the files to upload on the portal. Each step says in a sentence whether it is done and
 * what to do if it is not. The portal's own tables are still here, in a section at the bottom
 * that stays closed until somebody wants to read them.
 */
export function GstMonthScreen({ def }: CustomScreenProps) {
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const isAdmin = canRaiseDocument(user?.UserTypeID);

  const periods = useMemo(() => recentPeriods(18), []);
  // The month that has just ended is the one being filed, so the page opens on it.
  const [period, setPeriod] = useState(periods[1] ?? periodKeyOf());
  const [generateOpen, setGenerateOpen] = useState(false);
  const [tablesOpen, setTablesOpen] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  const month = useQuery({
    queryKey: ['gst-month', period],
    queryFn: async (): Promise<Summary> => {
      const res = await apiPost<Summary>('GstReturn/MonthSummary', { PeriodKey: period });
      if (!res.ok || !res.data) throw new Error(res.message || 'The month could not be worked out.');
      return res.data;
    },
  });
  const s = month.data;
  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['gst-month'] });
    qc.invalidateQueries({ queryKey: ['gstr1'] });
    qc.invalidateQueries({ queryKey: ['gstr3b'] });
    qc.invalidateQueries({ queryKey: ['gst-filings'] });
    qc.invalidateQueries({ queryKey: ['purchase-register'] });
  };

  // ── Step 3: the 2B file ──────────────────────────────────────────────────────────────────
  const [twoB, setTwoB] = useState<TwoBResult | null>(null);
  const upload2b = useMutation({
    mutationFn: async (file: File): Promise<TwoBResult> => {
      const json = await file.text();
      const res = await apiPost<TwoBResult>('GstReturn/Import2b', { Json: json, PeriodKey: period });
      if (!res.ok || !res.data) throw new Error(res.message || 'The GSTR-2B file could not be used.');
      return res.data;
    },
    onSuccess: (r) => {
      setTwoB(r);
      toast.success(r.CreditsClaimed > 0
        ? `GSTR-2B read. ${r.CreditsClaimed} bill${r.CreditsClaimed === 1 ? '' : 's'} matched — credit of ${inr(r.CreditClaimedValue)} confirmed.`
        : 'GSTR-2B read. No new credit could be confirmed — see the notes under step 3.');
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Step 5: files and filing ─────────────────────────────────────────────────────────────
  const exportFile = useMutation({
    mutationFn: async (v: { type: 'GSTR1' | 'GSTR3B'; format: 'JSON' | 'EXCEL' }) => {
      const res = await apiPost<{ FileName: string; ContentType: string; Base64: string }>('GstReturn/Export', {
        ReturnType: v.type, CompanyGSTIN: s?.CompanyGSTIN ?? '', PeriodKey: period,
        FinancialYearStart: 0, B2CLThreshold: 0, Format: v.format,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The file could not be produced.');
      return res.data;
    },
    onSuccess: (f) => { saveBase64(f.Base64, f.FileName, f.ContentType); toast.success(`${f.FileName} downloaded.`); },
    onError: (e: Error) => toast.error(e.message),
  });

  const [fileDialog, setFileDialog] = useState<'GSTR1' | 'GSTR3B' | null>(null);
  const [ack, setAck] = useState('');
  const markFiled = useMutation({
    mutationFn: async (type: 'GSTR1' | 'GSTR3B') => {
      const res = await apiPost('GstReturn/Save', {
        ReturnType: type, CompanyGSTIN: s?.CompanyGSTIN ?? '', PeriodKey: period, FinancialYearStart: 0,
        B2CLThreshold: 0, MarkFiled: true, AcknowledgementNo: ack.trim(), Note: '',
      });
      if (!res.ok) throw new Error(res.message || 'Could not record the filing.');
    },
    onSuccess: () => { toast.success('Recorded as filed.'); setFileDialog(null); setAck(''); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const step1Done = !!s && s.PendingInvoiceCount === 0 && !s.PendingInvoiceBlocker;
  const step2Done = !!s && s.PurchaseCount > 0;
  const step3Done = !!s && (s.TwoBImported || s.PurchaseCount === 0) && s.CreditWaiting === 0;
  const step5Done = !!s && s.Gstr1Filed && s.Gstr3bFiled;

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="One month at a time — what you sold, what you bought, and the GST left to pay"
        icon={pickIcon(def.title)}
        actions={
          <a href={PORTAL_URL} target="_blank" rel="noopener noreferrer">
            <Button variant="outline">
              <ExternalLink className="mr-1.5 size-4" />
              Open GST portal
            </Button>
          </a>
        }
      />

      <Card>
        <CardBody className="flex flex-wrap items-end gap-4">
          <div className="w-56">
            <Select label="Month" value={period} onChange={(e) => { setPeriod(e.target.value); setTwoB(null); }}>
              {periods.map((p) => (
                <option key={p} value={p}>{`${periodLabel(p)}${p === periodKeyOf() ? ' (this month, not finished)' : ''}`}</option>
              ))}
            </Select>
          </div>
          {s && (
            <div className="text-sm text-slate-500">
              <div><span className="font-medium text-slate-700">{s.CompanyName || 'Company'}</span> · GSTIN <span className="font-mono">{s.CompanyGSTIN}</span></div>
              <div>
                GSTR-1 due {dateOnly(s.Gstr1DueDate)} · GSTR-3B due {dateOnly(s.Gstr3bDueDate)}
                {s.PortalUsername && <> · portal user name <span className="font-mono text-slate-700">{s.PortalUsername}</span></>}
              </div>
            </div>
          )}
        </CardBody>
      </Card>

      {month.isPending ? (
        <Card><div className="flex min-h-[40vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div></Card>
      ) : month.isError ? (
        <Card><CardBody><EmptyState title="This page needs one thing first" description={(month.error as Error).message} /></CardBody></Card>
      ) : s ? (
        <>
          {/* ── The answer, before the working ─────────────────────────────────────────── */}
          <div className={cn(STAT_STRIP, 'grid-cols-2 lg:grid-cols-4')}>
            <StatTile label="GST on your sales" value={inr(s.OutputTax)} icon={Receipt} tone="text-brand-600" loading={month.isFetching} />
            <StatTile label="GST credit from purchases" value={inr(s.CreditConfirmed + s.CreditWaiting)} icon={ShoppingCart} tone="text-info-600" loading={month.isFetching} />
            <StatTile label="Credit from earlier months" value={inr(s.OpeningCredit)} icon={Wallet} tone="text-slate-600" loading={month.isFetching} />
            <StatTile label="GST to pay" value={inr(s.TotalPayable)} icon={IndianRupee} tone={s.TotalPayable > 0 ? 'text-danger-600' : 'text-success-600'} loading={month.isFetching} />
          </div>

          {s.MonthStillOpen && (
            <Note tone="info">
              {s.PeriodLabel} is not over yet, so these figures will keep moving. Returns are filed after the month ends.
            </Note>
          )}

          {/* ── 1. Sales ───────────────────────────────────────────────────────────────── */}
          <Step no={1} title="Sales invoices" done={step1Done}
            summary={`${s.SaleInvoiceCount} invoice${s.SaleInvoiceCount === 1 ? '' : 's'} · taxable ${inr(s.SaleTaxableValue)} · GST ${inr(s.OutputTax)}`}>
            <Figures items={[
              ['Invoices issued', String(s.SaleInvoiceCount)],
              ['Invoice value', inr(s.SaleInvoiceValue)],
              ['Taxable value', inr(s.SaleTaxableValue)],
              ['GST collected', inr(s.OutputTax)],
            ]} />
            <p className="mt-2 text-xs text-slate-500">
              GST collected is IGST {inr(s.OutputIGST)} + CGST {inr(s.OutputCGST)} + SGST {inr(s.OutputSGST)}.
            </p>
            {s.PendingInvoiceBlocker ? (
              <Note tone="warning" className="mt-3">
                {s.PendingInvoiceBlocker}{' '}
                <Link to="/s/PartyPlaceOfSupply" className="font-medium underline">Open Place of Supply</Link>
              </Note>
            ) : s.PendingInvoiceCount > 0 ? (
              <Note tone="warning" className="mt-3">
                <strong>{s.PendingInvoiceCount} invoice{s.PendingInvoiceCount === 1 ? ' is' : 's are'} not generated yet</strong> for this month
                (taxable {inr(s.PendingInvoiceTaxable)}, GST {inr(s.PendingInvoiceTax)}). Until they are, this GST is missing from the return.
                {isAdmin
                  ? <Button size="sm" className="ml-3" onClick={() => setGenerateOpen(true)}><FilePlus2 className="size-4" />Generate invoices</Button>
                  : ' Ask the administrator to generate them.'}
              </Note>
            ) : (
              <p className="mt-3 text-sm text-success-700">Every sale of this month has its invoice.</p>
            )}
            <div className="mt-3">
              <Link to="/s/Invoice" className="text-sm font-medium text-brand-600 hover:underline">View the invoices →</Link>
            </div>
          </Step>

          {/* ── 2. Purchases ───────────────────────────────────────────────────────────── */}
          <Step no={2} title="Purchase invoices" done={step2Done}
            summary={s.PurchaseCount === 0 ? 'No supplier bill entered for this month' : `${s.PurchaseCount} bill${s.PurchaseCount === 1 ? '' : 's'} · GST on them ${inr(s.PurchaseTax)}`}>
            {s.PurchaseCount === 0 ? (
              <Note tone="warning">
                No purchase invoice has been entered for {s.PeriodLabel}. The GST on your suppliers' bills is what reduces the GST
                you pay — without them the full amount is payable.
              </Note>
            ) : (
              <div className="overflow-x-auto rounded-lg border border-slate-200">
                <table className="w-full min-w-[720px] text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                      <th className="px-3 py-2 font-medium">Bill</th>
                      <th className="px-3 py-2 font-medium">Supplier</th>
                      <th className="px-3 py-2 text-right font-medium">Taxable</th>
                      <th className="px-3 py-2 text-right font-medium">GST</th>
                      <th className="px-3 py-2 text-right font-medium">Total</th>
                      <th className="px-3 py-2 font-medium">GST credit</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {s.Purchases.map((p) => (
                      <tr key={p.TaxInvoiceHeaderID}>
                        <td className="px-3 py-2">
                          <div className="font-mono text-xs font-semibold text-slate-700">{p.DocumentNo}</div>
                          <div className="text-[11px] text-slate-400">{dateOnly(p.DocumentDate)}</div>
                        </td>
                        <td className="px-3 py-2">
                          <div className="max-w-[230px] truncate text-slate-700" title={p.SupplierName}>{p.SupplierName || '—'}</div>
                          <div className="font-mono text-[11px] text-slate-400">{p.SupplierGSTIN}</div>
                        </td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(p.TaxableValue)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(p.Tax)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(p.Total)}</td>
                        <td className="px-3 py-2"><Badge tone={creditTone(p.CreditStatus)}>{p.CreditStatus}</Badge></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
            <div className="mt-3">
              <Link to="/s/PurchaseRegister" className="text-sm font-medium text-brand-600 hover:underline">Add or correct purchase invoices →</Link>
            </div>
          </Step>

          {/* ── 3. GSTR-2B ─────────────────────────────────────────────────────────────── */}
          <Step no={3} title="Confirm the purchase credit (GSTR-2B)" done={step3Done}
            summary={s.TwoBImported
              ? `File uploaded ${dateOnly(s.TwoBImportedOn)} · credit confirmed ${inr(s.CreditConfirmed)}${s.CreditWaiting > 0 ? ` · still waiting ${inr(s.CreditWaiting)}` : ''}`
              : s.CreditWaiting > 0 ? `${inr(s.CreditWaiting)} of credit is waiting for the 2B file` : 'Nothing to confirm'}>
            <p className="text-sm text-slate-600">
              GSTR-2B is the government's list of the bills your suppliers have filed against your GSTIN. Credit can be used
              only for bills that appear in it. Upload the file and the matching and claiming are done for you.
            </p>

            <div className="mt-3 grid gap-3 sm:grid-cols-3">
              <Mini label="Credit confirmed" value={inr(s.CreditConfirmed)} tone="text-success-700" />
              <Mini label="Credit still waiting" value={inr(s.CreditWaiting)} tone={s.CreditWaiting > 0 ? 'text-warning-700' : 'text-slate-700'} />
              <Mini label="Bills in the 2B file" value={s.TwoBImported ? String(s.TwoBLineCount) : 'not uploaded'} tone="text-slate-700" />
            </div>

            {!s.TwoBImported && (
              <ol className="mt-3 list-inside list-decimal space-y-1 rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
                <li>Open the GST portal and sign in (button at the top of this page).</li>
                <li>Go to <strong>Returns → Returns Dashboard</strong>, choose {s.PeriodLabel}, and open <strong>GSTR-2B</strong>.</li>
                <li>Press <strong>Download → Generate JSON file</strong> and save it. It is available from the 14th of the following month.</li>
                <li>Upload that file here.</li>
              </ol>
            )}

            {s.TwoBImported && (
              <div className="mt-3 flex flex-wrap gap-2 text-sm">
                <Badge tone="success">{s.TwoBMatched} matched</Badge>
                {s.TwoBMissingIn2B > 0 && <Badge tone="warning">{s.TwoBMissingIn2B} not filed by supplier yet</Badge>}
                {s.TwoBMismatch > 0 && <Badge tone="danger">{s.TwoBMismatch} with different tax</Badge>}
                {s.TwoBMissingInBooks > 0 && <Badge tone="info">{s.TwoBMissingInBooks} in 2B but not entered here</Badge>}
              </div>
            )}

            {s.TwoBMissingBills.length > 0 && (
              <Note tone="info" className="mt-3">
                <strong>These bills are in your GSTR-2B but have not been entered as purchase invoices</strong> — enter them and upload the file again to take their credit:
                <ul className="mt-1 list-inside list-disc">
                  {s.TwoBMissingBills.map((b) => (
                    <li key={`${b.SupplierGSTIN}-${b.DocumentNo}`}>{b.DocumentNo} · {b.SupplierName || b.SupplierGSTIN} · {dateOnly(b.DocumentDate)} · GST {inr(b.Tax)}</li>
                  ))}
                </ul>
              </Note>
            )}

            {twoB && twoB.Warnings.length > 0 && (
              <Note tone="warning" className="mt-3">
                <ul className="list-inside list-disc">{twoB.Warnings.map((w) => <li key={w}>{w}</li>)}</ul>
              </Note>
            )}

            {isAdmin ? (
              <div className="mt-3">
                <input
                  ref={fileRef}
                  type="file"
                  accept=".json,application/json"
                  className="hidden"
                  onChange={(e) => {
                    const f = e.target.files?.[0];
                    if (f) upload2b.mutate(f);
                    e.target.value = '';
                  }}
                />
                <Button onClick={() => fileRef.current?.click()} loading={upload2b.isPending} variant={s.TwoBImported ? 'outline' : 'primary'}>
                  <Upload className="mr-1.5 size-4" />
                  {s.TwoBImported ? 'Upload a newer GSTR-2B file' : 'Upload the GSTR-2B file'}
                </Button>
              </div>
            ) : (
              <p className="mt-3 text-xs text-slate-400">Uploading the 2B file claims credit, so it is done by the administrator.</p>
            )}
          </Step>

          {/* ── 4. What is left to pay ─────────────────────────────────────────────────── */}
          <Step no={4} title="GST to pay" done={s.CreditWaiting === 0}
            summary={s.TotalPayable > 0 ? `${inr(s.TotalPayable)} to pay` : `Nothing to pay · credit of ${inr(s.ClosingCredit)} carries forward`}>
            <div className="overflow-hidden rounded-lg border border-slate-200">
              <table className="w-full text-sm">
                <tbody className="divide-y divide-slate-100">
                  <Row label="GST collected on your sales" value={inr(s.OutputTax)} />
                  <Row label="Less: credit brought forward from earlier months" value={`− ${inr(s.OpeningCredit)}`} />
                  <Row label="Less: purchase credit confirmed this month" value={`− ${inr(s.CreditUsedThisMonth)}`} />
                  <tr className="bg-slate-50">
                    <td className="px-4 py-3 text-base font-semibold text-slate-900">GST to pay in cash</td>
                    <td className={cn('px-4 py-3 text-right text-xl font-semibold tabular-nums', s.TotalPayable > 0 ? 'text-danger-600' : 'text-success-700')}>{inr(s.TotalPayable)}</td>
                  </tr>
                  <Row label="Credit carried forward to next month" value={inr(s.ClosingCredit)} tone="text-success-700" />
                </tbody>
              </table>
            </div>
            {s.TotalPayable > 0 && (
              <p className="mt-2 text-xs text-slate-500">
                To pay: IGST {inr(s.PayableIGST)} · CGST {inr(s.PayableCGST)} · SGST {inr(s.PayableSGST)}.
              </p>
            )}
            {s.CreditWaiting > 0 && (
              <Note tone="warning" className="mt-3">
                <strong>{inr(s.CreditWaiting)} of purchase credit is not counted above</strong>, because it has not been confirmed by the
                GSTR-2B file yet (step 3). Once it is, the GST to pay becomes{' '}
                <strong>{inr(s.PayableIfCreditConfirmed)}</strong>
                {s.ClosingCreditIfConfirmed > 0 && <> and <strong>{inr(s.ClosingCreditIfConfirmed)}</strong> of credit carries forward</>}.
              </Note>
            )}
            <p className="mt-2 text-xs text-slate-400">
              Credit is used in the order the law sets — IGST credit first against IGST, then CGST and SGST — and the amounts
              brought forward are worked out from the invoices and credits recorded in this system.
            </p>
          </Step>

          {/* ── 5. File ────────────────────────────────────────────────────────────────── */}
          <Step no={5} title="File on the GST portal" done={step5Done}
            summary={step5Done ? 'Both returns recorded as filed' : `${s.Gstr1Filed ? 'GSTR-1 filed' : 'GSTR-1 not filed'} · ${s.Gstr3bFiled ? 'GSTR-3B filed' : 'GSTR-3B not filed'}`}>
            <div className="grid gap-3 lg:grid-cols-2">
              <FilingCard
                name="GSTR-1" what="Your sales, invoice by invoice" due={s.Gstr1DueDate}
                filed={s.Gstr1Filed} filedOn={s.Gstr1FiledOn} ack={s.Gstr1Ack}
                how="On the portal: Returns → GSTR-1 → Prepare offline → Upload, and choose the JSON file."
                onJson={() => exportFile.mutate({ type: 'GSTR1', format: 'JSON' })}
                onExcel={() => exportFile.mutate({ type: 'GSTR1', format: 'EXCEL' })}
                onFiled={isAdmin ? () => setFileDialog('GSTR1') : undefined}
                busy={exportFile.isPending}
              />
              <FilingCard
                name="GSTR-3B" what="The summary, and the GST you pay" due={s.Gstr3bDueDate}
                filed={s.Gstr3bFiled} filedOn={s.Gstr3bFiledOn} ack={s.Gstr3bAck}
                how="On the portal: Returns → GSTR-3B. Type in the figures from the Excel sheet, then pay and file."
                onJson={() => exportFile.mutate({ type: 'GSTR3B', format: 'JSON' })}
                onExcel={() => exportFile.mutate({ type: 'GSTR3B', format: 'EXCEL' })}
                onFiled={isAdmin ? () => setFileDialog('GSTR3B') : undefined}
                busy={exportFile.isPending}
              />
            </div>
            {!s.AgreesWithGstr1 && (
              <Note tone="warning" className="mt-3">
                GSTR-1 and GSTR-3B do not show the same sales for this month. Open the detailed tables below before filing.
              </Note>
            )}
            <p className="mt-3 text-xs text-slate-400">
              The portal asks for a captcha and an OTP at sign-in, so the upload itself is done by you there — it cannot be
              done by a program. Everything up to that point is prepared here.
            </p>
          </Step>

          {s.Warnings.length > 0 && (
            <details className="rounded-lg border border-slate-200 bg-white p-3 text-sm">
              <summary className="cursor-pointer font-medium text-slate-700">{s.Warnings.length} note{s.Warnings.length === 1 ? '' : 's'} for your CA</summary>
              <ul className="mt-2 list-inside list-disc space-y-1 text-xs text-slate-600">{s.Warnings.map((w) => <li key={w}>{w}</li>)}</ul>
            </details>
          )}

          {/* ── The portal's own tables ────────────────────────────────────────────────── */}
          <Card>
            <button type="button" onClick={() => setTablesOpen((o) => !o)} className="flex w-full items-center gap-3 px-5 py-3 text-left hover:bg-slate-50">
              {tablesOpen ? <ChevronDown className="size-4 text-slate-400" /> : <ChevronRight className="size-4 text-slate-400" />}
              <div>
                <div className="text-sm font-semibold text-slate-800">Detailed tables — GSTR-1, GSTR-3B and GSTR-9</div>
                <div className="text-xs text-slate-400">The same figures laid out the way the portal lays them out. For your CA.</div>
              </div>
            </button>
            {tablesOpen && <div className="border-t border-slate-100 p-4"><GstReturnTables period={period} /></div>}
          </Card>
        </>
      ) : null}

      <GenerateMonthDialog
        open={generateOpen}
        onClose={() => setGenerateOpen(false)}
        onIssued={refresh}
        initialPeriod={period}
      />

      <Modal open={fileDialog !== null} onClose={() => setFileDialog(null)} title={`Record ${fileDialog === 'GSTR1' ? 'GSTR-1' : 'GSTR-3B'} as filed`} size="sm">
        <div className="space-y-3">
          <p className="text-sm text-slate-600">
            Do this after the return has been filed on the portal. It freezes this month's figures as what was declared,
            and the invoices of the month can no longer be re-issued.
          </p>
          <Input label="Acknowledgement number (ARN)" value={ack} onChange={(e) => setAck(e.target.value)} hint="Optional — shown on the portal after filing" />
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setFileDialog(null)}>Cancel</Button>
            <Button onClick={() => fileDialog && markFiled.mutate(fileDialog)} loading={markFiled.isPending}>Record as filed</Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

// ── Pieces ─────────────────────────────────────────────────────────────────────────────────

function Step({ no, title, summary, done, children }: { no: number; title: string; summary: string; done: boolean; children: ReactNode }) {
  return (
    <Card>
      <CardBody>
        <div className="flex items-start gap-3">
          <div className={cn('mt-0.5 flex size-8 shrink-0 items-center justify-center rounded-full text-sm font-semibold',
            done ? 'bg-success-100 text-success-700' : 'bg-warning-100 text-warning-700')}>
            {done ? <CheckCircle2 className="size-5" /> : no}
          </div>
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-baseline justify-between gap-x-4">
              <h3 className="text-base font-semibold text-slate-900">{no}. {title}</h3>
              <span className={cn('text-xs font-medium', done ? 'text-success-700' : 'text-warning-700')}>
                {done ? 'Done' : <span className="inline-flex items-center gap-1"><Circle className="size-3" /> Needs attention</span>}
              </span>
            </div>
            <p className="text-sm text-slate-500">{summary}</p>
            <div className="mt-3">{children}</div>
          </div>
        </div>
      </CardBody>
    </Card>
  );
}

function Note({ tone, children, className }: { tone: 'info' | 'warning'; children: ReactNode; className?: string }) {
  return (
    <div className={cn('flex gap-2.5 rounded-lg border p-3 text-sm',
      tone === 'warning' ? 'border-warning-500/30 bg-warning-50 text-warning-800' : 'border-info-200 bg-info-50/50 text-slate-700', className)}>
      <AlertTriangle className={cn('mt-0.5 size-4 shrink-0', tone === 'warning' ? 'text-warning-600' : 'text-info-600')} />
      <div className="min-w-0">{children}</div>
    </div>
  );
}

function Figures({ items }: { items: [string, string][] }) {
  return (
    <dl className="grid grid-cols-2 gap-3 sm:grid-cols-4">
      {items.map(([label, value]) => <Mini key={label} label={label} value={value} tone="text-slate-800" />)}
    </dl>
  );
}

function Mini({ label, value, tone }: { label: string; value: string; tone: string }) {
  return (
    <div className="rounded-lg border border-slate-200 p-2.5">
      <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-400">{label}</p>
      <p className={cn('mt-0.5 text-base font-semibold tabular-nums', tone)}>{value}</p>
    </div>
  );
}

function Row({ label, value, tone }: { label: string; value: string; tone?: string }) {
  return (
    <tr>
      <td className="px-4 py-2.5 text-slate-600">{label}</td>
      <td className={cn('px-4 py-2.5 text-right tabular-nums text-slate-800', tone)}>{value}</td>
    </tr>
  );
}

function FilingCard({
  name, what, due, filed, filedOn, ack, how, onJson, onExcel, onFiled, busy,
}: {
  name: string; what: string; due: string; filed: boolean; filedOn: string | null; ack: string; how: string;
  onJson: () => void; onExcel: () => void; onFiled?: () => void; busy: boolean;
}) {
  return (
    <div className="rounded-lg border border-slate-200 p-3">
      <div className="flex items-start justify-between gap-2">
        <div>
          <div className="text-sm font-semibold text-slate-900">{name}</div>
          <div className="text-xs text-slate-500">{what} · due {dateOnly(due)}</div>
        </div>
        <Badge tone={filed ? 'success' : 'warning'}>{filed ? `Filed${filedOn ? ` ${dateOnly(filedOn)}` : ''}` : 'Not filed'}</Badge>
      </div>
      {filed && ack && <div className="mt-1 font-mono text-xs text-slate-500">ARN {ack}</div>}
      <p className="mt-2 text-xs text-slate-500">{how}</p>
      <div className="mt-3 flex flex-wrap gap-2">
        <Button size="sm" variant="outline" onClick={onJson} disabled={busy}><FileJson className="mr-1 size-4" />JSON file</Button>
        <Button size="sm" variant="outline" onClick={onExcel} disabled={busy}><FileSpreadsheet className="mr-1 size-4" />Excel sheet</Button>
        {onFiled && !filed && <Button size="sm" onClick={onFiled}>Mark as filed</Button>}
      </div>
    </div>
  );
}
