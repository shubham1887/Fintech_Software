import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, CheckCircle2, ChevronDown, ChevronRight, Download, FileJson,
  FileSpreadsheet, ScrollText, Scale, Save, ShieldAlert,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly, dateTime } from '@/lib/format';
import { cn } from '@/lib/cn';
import { canRaiseDocument, periodKeyOf, periodLabel, recentPeriods, useCompanyGstin } from '@/lib/gstDocs';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { Card, CardBody } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';

interface RateSlab { Rate: number; TaxableValue: number; IGST: number; CGST: number; SGST: number; Cess: number }

interface G1Document {
  TaxInvoiceHeaderID: number; DocumentNo: string; DocumentDate: string; DocumentType: string;
  InvoiceValue: number; PlaceOfSupplyStateCode: string; SupplyNature: string; ReverseCharge: boolean;
  OriginalDocumentNo: string; OriginalDocumentDate: string | null; NoteReason: string; Slabs: RateSlab[];
}
interface G1Party {
  GSTIN: string; PartyName: string; DocumentCount: number;
  TaxableValue: number; TotalTax: number; Documents: G1Document[];
}
interface G1B2cs {
  PlaceOfSupplyStateCode: string; PlaceOfSupplyName: string; SupplyType: string; Rate: number;
  TaxableValue: number; IGST: number; CGST: number; SGST: number; Cess: number; DocumentCount: number;
}
interface G1Hsn {
  SACCode: string; Description: string; Rate: number; Quantity: number; TotalValue: number;
  TaxableValue: number; IGST: number; CGST: number; SGST: number; Cess: number;
}
interface G1Docs {
  SeriesCode: string; DocumentType: string; FromNumber: string; ToNumber: string;
  TotalIssued: number; Cancelled: number; NetIssued: number;
}
interface G1Exempt { Category: string; NilRated: number; Exempted: number; NonGST: number }

interface Gstr1Result {
  CompanyGSTIN: string; PeriodKey: string; PeriodLabel: string; B2CLThreshold: number;
  B2B: G1Party[]; B2CL: G1Party[]; B2CS: G1B2cs[]; CDNR: G1Party[]; CDNUR: G1Party[];
  Exempt: G1Exempt[]; HSN: G1Hsn[]; Documents: G1Docs[];
  TotalTaxableValue: number; TotalIGST: number; TotalCGST: number; TotalSGST: number;
  TotalCess: number; TotalDocuments: number; Warnings: string[];
}

interface G3bOutward { Code: string; Nature: string; TaxableValue: number; IGST: number; CGST: number; SGST: number; Cess: number }
interface G3bInter { PlaceOfSupplyStateCode: string; PlaceOfSupplyName: string; Recipient: string; TaxableValue: number; IGST: number }
interface G3bItc { Code: string; Detail: string; IGST: number; CGST: number; SGST: number; Cess: number }
interface Gstr3bResult {
  CompanyGSTIN: string; PeriodKey: string; PeriodLabel: string;
  Outward: G3bOutward[]; InterStateSupplies: G3bInter[]; Itc: G3bItc[];
  OutputIGST: number; OutputCGST: number; OutputSGST: number; OutputCess: number;
  NetItcIGST: number; NetItcCGST: number; NetItcSGST: number; NetItcCess: number;
  PayableIGST: number; PayableCGST: number; PayableSGST: number; PayableCess: number; TotalPayable: number;
  OpeningCreditIGST: number; OpeningCreditCGST: number; OpeningCreditSGST: number; OpeningCreditCess: number;
  ClosingCreditIGST: number; ClosingCreditCGST: number; ClosingCreditSGST: number; ClosingCreditCess: number;
  Gstr1TaxableValue: number; Gstr1TotalTax: number;
  VarianceTaxableValue: number; VarianceTax: number; AgreesWithGstr1: boolean; Warnings: string[];
}

interface G9Month {
  PeriodKey: string; PeriodLabel: string; BooksTaxableValue: number; BooksTax: number;
  FiledTaxableValue: number; FiledTax: number; WasFiled: boolean;
  VarianceTaxableValue: number; VarianceTax: number;
}
interface Gstr9Result {
  CompanyGSTIN: string; FinancialYearStart: number; FinancialYearLabel: string;
  Months: G9Month[]; OutwardByRate: RateSlab[];
  ItcAvailedIGST: number; ItcAvailedCGST: number; ItcAvailedSGST: number; ItcAvailedCess: number; ItcReversed: number;
  TotalTurnover: number; TotalTaxPayable: number;
  TotalFiledTaxableValue: number; TotalVarianceTaxableValue: number;
  MonthsFiled: number; MonthsMissing: number; Warnings: string[];
}

interface FilingRow {
  GstReturnFilingID: number; ReturnType: string; PeriodKey: string; PeriodLabel: string;
  TotalTaxableValue: number; TotalIGST: number; TotalCGST: number; TotalSGST: number; TotalCess: number;
  DocumentCount: number; Status: string; FiledOn: string | null;
  AcknowledgementNo: string; Note: string; GeneratedOn: string;
}

type Tab = 'GSTR1' | 'GSTR3B' | 'GSTR9';

const TABS: { key: Tab; label: string; hint: string }[] = [
  { key: 'GSTR1', label: 'GSTR-1', hint: 'Outward supplies, document by document' },
  { key: 'GSTR3B', label: 'GSTR-3B', hint: 'The monthly summary and the tax payable' },
  { key: 'GSTR9', label: 'GSTR-9', hint: 'The year, books beside what was filed' },
];

/**
 * "27 — Maharashtra", or just the code when the server could not name it. The server answers an
 * unknown code with the code itself, so printing both unconditionally gives "00 — 00".
 */
function posLabel(code: string, name: string): string {
  const c = (code ?? '').trim() || '(not set)';
  return !name || name === c ? c : `${c} — ${name}`;
}

/** The current financial year's start: India's FY begins 1 April. */
function fyStartOf(d = new Date()): number {
  return d.getMonth() + 1 >= 4 ? d.getFullYear() : d.getFullYear() - 1;
}

/** Hand the browser a base64 payload as a file download. */
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

/**
 * The returns in the portal's own tables — GSTR-1, 3B and 9, line by line.
 *
 * This is the detail behind the GST Returns page, shown there in a section that stays closed
 * until somebody (usually the CA) wants to read a table. `period` follows the month chosen on
 * that page.
 */
export function GstReturnTables({ period: periodProp }: { period?: string }) {
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const maySave = canRaiseDocument(user?.UserTypeID);
  const { gstin, missing: gstinMissing, loading: gstinLoading } = useCompanyGstin();

  const periods = useMemo(() => recentPeriods(18), []);
  const [tab, setTab] = useState<Tab>('GSTR1');
  const [period, setPeriod] = useState(periodProp || periodKeyOf());
  useEffect(() => { if (periodProp) setPeriod(periodProp); }, [periodProp]);
  const [fyStart, setFyStart] = useState(fyStartOf());
  const [open, setOpen] = useState<string[]>(['B2B']);

  const enabled = gstin.length === 15;
  const monthly = { CompanyGSTIN: gstin, PeriodKey: period, B2CLThreshold: 0 };

  const g1 = useQuery({
    queryKey: ['gstr1', gstin, period],
    enabled: enabled && tab !== 'GSTR9',
    queryFn: async (): Promise<Gstr1Result> => {
      const res = await apiPost<Gstr1Result>('GstReturn/Gstr1', monthly);
      if (!res.ok || !res.data) throw new Error(res.message || 'GSTR-1 could not be built.');
      return res.data;
    },
  });

  const g3b = useQuery({
    queryKey: ['gstr3b', gstin, period],
    enabled: enabled && tab === 'GSTR3B',
    queryFn: async (): Promise<Gstr3bResult> => {
      const res = await apiPost<Gstr3bResult>('GstReturn/Gstr3b', monthly);
      if (!res.ok || !res.data) throw new Error(res.message || 'GSTR-3B could not be built.');
      return res.data;
    },
  });

  const g9 = useQuery({
    queryKey: ['gstr9', gstin, fyStart],
    enabled: enabled && tab === 'GSTR9',
    queryFn: async (): Promise<Gstr9Result> => {
      const res = await apiPost<Gstr9Result>('GstReturn/Gstr9', { CompanyGSTIN: gstin, FinancialYearStart: fyStart });
      if (!res.ok || !res.data) throw new Error(res.message || 'GSTR-9 could not be built.');
      return res.data;
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

  const busy = (tab === 'GSTR1' && g1.isFetching) || (tab === 'GSTR3B' && (g1.isFetching || g3b.isFetching))
            || (tab === 'GSTR9' && g9.isFetching);
  const error = (tab === 'GSTR1' && g1.error) || (tab === 'GSTR3B' && g3b.error) || (tab === 'GSTR9' && g9.error);

  const warnings = tab === 'GSTR1' ? g1.data?.Warnings
                 : tab === 'GSTR3B' ? g3b.data?.Warnings
                 : g9.data?.Warnings;

  const exportFile = useMutation({
    mutationFn: async (format: 'JSON' | 'EXCEL') => {
      const res = await apiPost<{ FileName: string; ContentType: string; Base64: string }>('GstReturn/Export', {
        ReturnType: tab, CompanyGSTIN: gstin,
        PeriodKey: tab === 'GSTR9' ? '' : period,
        FinancialYearStart: tab === 'GSTR9' ? fyStart : 0,
        B2CLThreshold: 0, Format: format,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The file could not be produced.');
      return res.data;
    },
    onSuccess: (f) => {
      saveBase64(f.Base64, f.FileName, f.ContentType);
      toast.success(`${f.FileName} downloaded.`);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Save / file ─────────────────────────────────────────────────────────────────────
  const [saveOpen, setSaveOpen] = useState(false);
  const [markFiled, setMarkFiled] = useState(false);
  const [ack, setAck] = useState('');
  const [note, setNote] = useState('');

  const save = useMutation({
    mutationFn: async () => {
      const res = await apiPost('GstReturn/Save', {
        ReturnType: tab, CompanyGSTIN: gstin,
        PeriodKey: tab === 'GSTR9' ? '' : period,
        FinancialYearStart: tab === 'GSTR9' ? fyStart : 0,
        B2CLThreshold: 0, MarkFiled: markFiled,
        AcknowledgementNo: ack.trim(), Note: note.trim(),
      });
      if (!res.ok) throw new Error(res.message || 'The return could not be saved.');
    },
    onSuccess: () => {
      toast.success(markFiled
        ? 'Saved and marked filed. GSTR-9 will reconcile the year against this snapshot.'
        : 'Saved as a draft snapshot.');
      setSaveOpen(false); setAck(''); setNote(''); setMarkFiled(false);
      qc.invalidateQueries({ queryKey: ['gst-filings'] });
      qc.invalidateQueries({ queryKey: ['gstr9'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggle = (k: string) => setOpen((o) => (o.includes(k) ? o.filter((x) => x !== k) : [...o, k]));

  const fyOptions = useMemo(() => {
    const now = fyStartOf();
    return Array.from({ length: 6 }, (_, i) => now - i);
  }, []);

  if (gstinLoading) {
    return <div className="flex min-h-[40vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>;
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-slate-500">Every figure is derived from documents already issued — nothing is entered twice.</p>
        {
          !gstinMissing && (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => exportFile.mutate('JSON')} loading={exportFile.isPending}>
                <FileJson className="mr-1.5 size-4" />
                JSON
              </Button>
              <Button variant="outline" onClick={() => exportFile.mutate('EXCEL')} loading={exportFile.isPending}>
                <FileSpreadsheet className="mr-1.5 size-4" />
                Excel
              </Button>
              {maySave && (
                <Button onClick={() => setSaveOpen(true)}>
                  <Save className="mr-1.5 size-4" />
                  Save snapshot
                </Button>
              )}
            </div>
          )
        }
      </div>

      {gstinMissing && (
        <div className="flex gap-2.5 rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-sm text-danger-600">
          <ShieldAlert className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>The company GSTIN is not set.</strong> A return is filed per registration, so nothing
            here can be built until the GSTIN is filled in on <strong>Invoice Settings</strong>.
          </p>
        </div>
      )}

      {!gstinMissing && (
        <>
          <Card>
            <CardBody className="flex flex-wrap items-end gap-3">
              <div className="flex rounded-lg border border-slate-200 p-0.5">
                {TABS.map((t) => (
                  <button
                    key={t.key}
                    type="button"
                    onClick={() => setTab(t.key)}
                    title={t.hint}
                    className={cn(
                      'rounded-md px-3 py-1.5 text-sm font-medium transition',
                      tab === t.key ? 'bg-brand-600 text-white' : 'text-slate-600 hover:bg-slate-100',
                    )}
                  >
                    {t.label}
                  </button>
                ))}
              </div>
              <div className="w-44">
                {tab === 'GSTR9' ? (
                  <Select label="Financial year" value={String(fyStart)} onChange={(e) => setFyStart(Number(e.target.value))}>
                    {fyOptions.map((y) => (
                      <option key={y} value={y}>{`${y}-${String((y + 1) % 100).padStart(2, '0')}`}</option>
                    ))}
                  </Select>
                ) : (
                  <Select label="Period" value={period} onChange={(e) => setPeriod(e.target.value)}>
                    {periods.map((p) => (
                      <option key={p} value={p}>{periodLabel(p)}</option>
                    ))}
                  </Select>
                )}
              </div>
              <div className="flex-1" />
              <span className="text-sm text-slate-500">{TABS.find((t) => t.key === tab)?.hint}</span>
            </CardBody>
          </Card>

          {(warnings?.length ?? 0) > 0 && (
            <div className="space-y-1.5 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
              {warnings!.map((w, i) => (
                <div key={i} className="flex gap-2.5">
                  <AlertTriangle className="mt-0.5 size-4 shrink-0" />
                  <p>{w}</p>
                </div>
              ))}
            </div>
          )}

          {busy ? (
            <Card><div className="flex min-h-[35vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div></Card>
          ) : error ? (
            <Card><CardBody><EmptyState title="Could not build the return" description={(error as Error).message} /></CardBody></Card>
          ) : tab === 'GSTR1' ? (
            <Gstr1View data={g1.data} open={open} toggle={toggle} />
          ) : tab === 'GSTR3B' ? (
            <Gstr3bView data={g3b.data} />
          ) : (
            <Gstr9View data={g9.data} />
          )}

          {(filings.data?.length ?? 0) > 0 && (
            <Card>
              <CardBody className="pb-2">
                <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <ScrollText className="size-4 text-slate-400" />
                  Saved returns
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  A regeneration supersedes rather than overwrites — the old snapshot is the evidence of
                  what was declared, and GSTR-9 reconciles the year against the ones marked filed.
                </p>
              </CardBody>
              <div className="overflow-x-auto px-3 pb-3">
                <table className="w-full min-w-[820px] text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                      <th className="px-3 py-2 font-medium">Return</th>
                      <th className="px-3 py-2 font-medium">Period</th>
                      <th className="px-3 py-2 text-right font-medium">Taxable</th>
                      <th className="px-3 py-2 text-right font-medium">Tax</th>
                      <th className="px-3 py-2 text-right font-medium">Docs</th>
                      <th className="px-3 py-2 font-medium">Status</th>
                      <th className="px-3 py-2 font-medium">Acknowledgement</th>
                      <th className="px-3 py-2 font-medium">Generated</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {filings.data!.map((f) => (
                      <tr key={f.GstReturnFilingID} className="hover:bg-slate-50">
                        <td className="px-3 py-2 font-mono text-xs font-semibold text-slate-800">{f.ReturnType}</td>
                        <td className="px-3 py-2 text-slate-700">{f.PeriodLabel}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(f.TotalTaxableValue)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">
                          {inr(f.TotalIGST + f.TotalCGST + f.TotalSGST + f.TotalCess)}
                        </td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-500">{f.DocumentCount}</td>
                        <td className="px-3 py-2">
                          <Badge tone={f.Status === 'Filed' ? 'success' : f.Status === 'Draft' ? 'warning' : 'neutral'}>
                            {f.Status}
                          </Badge>
                          {f.FiledOn && <div className="text-[11px] text-slate-400">{dateOnly(f.FiledOn)}</div>}
                        </td>
                        <td className="px-3 py-2 font-mono text-xs text-slate-500">{f.AcknowledgementNo || '—'}</td>
                        <td className="px-3 py-2 text-xs text-slate-400">{dateTime(f.GeneratedOn)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          )}
        </>
      )}

      {!maySave && !gstinMissing && (
        <p className="text-xs text-slate-400">
          Building and downloading a return are yours — they change nothing. Saving a snapshot is limited
          to Admin, because a snapshot marked filed is what GSTR-9 reconciles the year against.
        </p>
      )}

      <Modal open={saveOpen} onClose={() => setSaveOpen(false)} title={`Save ${tab}`} size="sm">
        <div className="space-y-3">
          <p className="text-sm text-slate-500">
            The return as it stands is kept. Marking it filed freezes it as what was declared for this
            period — GSTR-9 reads these snapshots, not a recomputation.
          </p>
          <label className="flex items-center gap-2 text-sm text-slate-700">
            <input type="checkbox" className="size-4" checked={markFiled} onChange={(e) => setMarkFiled(e.target.checked)} />
            Mark as filed on the portal
          </label>
          {markFiled && (
            <Input label="Acknowledgement number" value={ack} onChange={(e) => setAck(e.target.value)} hint="Optional — the ARN, if you have it" />
          )}
          <Input label="Note" value={note} onChange={(e) => setNote(e.target.value)} />
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setSaveOpen(false)}>Cancel</Button>
            <Button onClick={() => save.mutate()} loading={save.isPending}>Save</Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

// ── GSTR-1 ──────────────────────────────────────────────────────────────────────────────

function Section({
  id, title, hint, count, open, toggle, children,
}: {
  id: string; title: string; hint: string; count: number;
  open: string[]; toggle: (k: string) => void; children: React.ReactNode;
}) {
  const isOpen = open.includes(id);
  return (
    <Card>
      <button type="button" onClick={() => toggle(id)} className="flex w-full items-center gap-3 px-5 py-3 text-left hover:bg-slate-50">
        {isOpen ? <ChevronDown className="size-4 shrink-0 text-slate-400" /> : <ChevronRight className="size-4 shrink-0 text-slate-400" />}
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-slate-800">{title}</span>
            <Badge tone={count > 0 ? 'info' : 'neutral'}>{count}</Badge>
          </div>
          <div className="truncate text-xs text-slate-400">{hint}</div>
        </div>
      </button>
      {isOpen && <div className="border-t border-slate-100">{children}</div>}
    </Card>
  );
}

function PartyTables({ parties, noteStyle }: { parties: G1Party[]; noteStyle: boolean }) {
  if (parties.length === 0) {
    return <div className="px-5 py-6 text-sm text-slate-400">Nothing in this table for the period.</div>;
  }
  return (
    <div className="divide-y divide-slate-100">
      {parties.map((p) => (
        <div key={p.GSTIN} className="px-5 py-3">
          <div className="flex flex-wrap items-baseline gap-2">
            <span className="font-mono text-sm font-semibold text-slate-800">{p.GSTIN || '(no GSTIN)'}</span>
            <span className="text-sm text-slate-600">{p.PartyName}</span>
            <div className="flex-1" />
            <span className="text-xs tabular-nums text-slate-500">
              {p.DocumentCount} doc · taxable {inr(p.TaxableValue)} · tax {inr(p.TotalTax)}
            </span>
          </div>
          <div className="mt-2 overflow-x-auto">
            <table className="w-full min-w-[720px] text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="py-1.5 font-medium">{noteStyle ? 'Note' : 'Invoice'}</th>
                  <th className="py-1.5 font-medium">Date</th>
                  {noteStyle && <th className="py-1.5 font-medium">Against</th>}
                  <th className="py-1.5 font-medium">POS</th>
                  <th className="py-1.5 text-right font-medium">Rate</th>
                  <th className="py-1.5 text-right font-medium">Taxable</th>
                  <th className="py-1.5 text-right font-medium">IGST</th>
                  <th className="py-1.5 text-right font-medium">CGST</th>
                  <th className="py-1.5 text-right font-medium">SGST</th>
                  <th className="py-1.5 text-right font-medium">Value</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {p.Documents.flatMap((d) =>
                  d.Slabs.map((s, i) => (
                    <tr key={`${d.TaxInvoiceHeaderID}-${i}`}>
                      <td className="py-1.5 font-mono text-xs text-slate-700">{i === 0 ? d.DocumentNo : ''}</td>
                      <td className="py-1.5 text-xs text-slate-500">{i === 0 ? dateOnly(d.DocumentDate) : ''}</td>
                      {noteStyle && (
                        <td className="py-1.5 font-mono text-xs text-slate-500">{i === 0 ? d.OriginalDocumentNo || '—' : ''}</td>
                      )}
                      <td className="py-1.5 text-xs text-slate-500">{i === 0 ? d.PlaceOfSupplyStateCode : ''}</td>
                      <td className="py-1.5 text-right tabular-nums text-slate-600">{s.Rate}%</td>
                      <td className="py-1.5 text-right tabular-nums text-slate-700">{inr(s.TaxableValue)}</td>
                      <td className="py-1.5 text-right tabular-nums text-slate-700">{s.IGST ? inr(s.IGST) : ''}</td>
                      <td className="py-1.5 text-right tabular-nums text-slate-700">{s.CGST ? inr(s.CGST) : ''}</td>
                      <td className="py-1.5 text-right tabular-nums text-slate-700">{s.SGST ? inr(s.SGST) : ''}</td>
                      <td className="py-1.5 text-right tabular-nums text-slate-700">{i === 0 ? inr(d.InvoiceValue) : ''}</td>
                    </tr>
                  )),
                )}
              </tbody>
            </table>
          </div>
        </div>
      ))}
    </div>
  );
}

function Gstr1View({ data, open, toggle }: { data?: Gstr1Result; open: string[]; toggle: (k: string) => void }) {
  if (!data) return null;
  const tax = data.TotalIGST + data.TotalCGST + data.TotalSGST + data.TotalCess;

  return (
    <div className="space-y-4">
      <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
        <StatTile label="Documents" value={data.TotalDocuments} icon={ScrollText} tone="text-brand-600" />
        <StatTile label="Taxable value" value={inr(data.TotalTaxableValue)} icon={Scale} tone="text-slate-600" />
        <StatTile label="Total tax" value={inr(tax)} icon={Download} tone="text-success-600" />
        <StatTile label="B2CL limit used" value={inr(data.B2CLThreshold)} icon={AlertTriangle} tone="text-warning-600" />
      </div>

      <Section id="B2B" title="4A/4B/4C — B2B" hint="Supplies to a registered person, invoice by invoice" count={data.B2B.length} open={open} toggle={toggle}>
        <PartyTables parties={data.B2B} noteStyle={false} />
      </Section>

      <Section id="B2CL" title="5A — B2CL" hint={`Inter-state supplies to unregistered persons above ${inr(data.B2CLThreshold)}`} count={data.B2CL.length} open={open} toggle={toggle}>
        <PartyTables parties={data.B2CL} noteStyle={false} />
      </Section>

      <Section id="B2CS" title="7 — B2CS" hint="Every other supply to an unregistered person, consolidated by place of supply and rate" count={data.B2CS.length} open={open} toggle={toggle}>
        {data.B2CS.length === 0 ? (
          <div className="px-5 py-6 text-sm text-slate-400">Nothing in this table for the period.</div>
        ) : (
          <div className="overflow-x-auto px-5 py-3">
            <table className="w-full min-w-[720px] text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="py-1.5 font-medium">Place of supply</th>
                  <th className="py-1.5 font-medium">Type</th>
                  <th className="py-1.5 text-right font-medium">Rate</th>
                  <th className="py-1.5 text-right font-medium">Taxable</th>
                  <th className="py-1.5 text-right font-medium">IGST</th>
                  <th className="py-1.5 text-right font-medium">CGST</th>
                  <th className="py-1.5 text-right font-medium">SGST</th>
                  <th className="py-1.5 text-right font-medium">Docs</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.B2CS.map((b, i) => (
                  <tr key={i}>
                    <td className="py-1.5 text-slate-700">{posLabel(b.PlaceOfSupplyStateCode, b.PlaceOfSupplyName)}</td>
                    <td className="py-1.5"><Badge tone={b.SupplyType === 'INTER' ? 'info' : 'neutral'}>{b.SupplyType}</Badge></td>
                    <td className="py-1.5 text-right tabular-nums text-slate-600">{b.Rate}%</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{inr(b.TaxableValue)}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{b.IGST ? inr(b.IGST) : ''}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{b.CGST ? inr(b.CGST) : ''}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{b.SGST ? inr(b.SGST) : ''}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-500">{b.DocumentCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Section>

      <Section id="CDNR" title="9B — Credit / debit notes (registered)" hint="Notes against a registered party's document" count={data.CDNR.length} open={open} toggle={toggle}>
        <PartyTables parties={data.CDNR} noteStyle />
      </Section>

      <Section id="CDNUR" title="9B — Credit / debit notes (unregistered)" hint="Notes against a B2CL document" count={data.CDNUR.length} open={open} toggle={toggle}>
        <PartyTables parties={data.CDNUR} noteStyle />
      </Section>

      <Section id="EXEMP" title="8 — Nil rated, exempt, non-GST" hint="Turnover with no tax, still reported" count={data.Exempt.length} open={open} toggle={toggle}>
        {data.Exempt.length === 0 ? (
          <div className="px-5 py-6 text-sm text-slate-400">Nothing in this table for the period.</div>
        ) : (
          <div className="px-5 py-3">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="py-1.5 font-medium">Category</th>
                  <th className="py-1.5 text-right font-medium">Nil rated</th>
                  <th className="py-1.5 text-right font-medium">Exempted</th>
                  <th className="py-1.5 text-right font-medium">Non-GST</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.Exempt.map((e, i) => (
                  <tr key={i}>
                    <td className="py-1.5 text-slate-700">{e.Category}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{inr(e.NilRated)}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{inr(e.Exempted)}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{inr(e.NonGST)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Section>

      <Section id="HSN" title="12 — HSN / SAC summary" hint="Everything supplied, by SAC and rate" count={data.HSN.length} open={open} toggle={toggle}>
        {data.HSN.length === 0 ? (
          <div className="px-5 py-6 text-sm text-slate-400">Nothing in this table for the period.</div>
        ) : (
          <div className="overflow-x-auto px-5 py-3">
            <table className="w-full min-w-[760px] text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="py-1.5 font-medium">SAC</th>
                  <th className="py-1.5 font-medium">Description</th>
                  <th className="py-1.5 text-right font-medium">Rate</th>
                  <th className="py-1.5 text-right font-medium">Qty</th>
                  <th className="py-1.5 text-right font-medium">Taxable</th>
                  <th className="py-1.5 text-right font-medium">IGST</th>
                  <th className="py-1.5 text-right font-medium">CGST</th>
                  <th className="py-1.5 text-right font-medium">SGST</th>
                  <th className="py-1.5 text-right font-medium">Total</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.HSN.map((h, i) => (
                  <tr key={i}>
                    <td className="py-1.5 font-mono text-xs text-slate-700">{h.SACCode}</td>
                    <td className="py-1.5"><span className="block max-w-[220px] truncate text-slate-600" title={h.Description}>{h.Description}</span></td>
                    <td className="py-1.5 text-right tabular-nums text-slate-600">{h.Rate}%</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-500">{h.Quantity}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{inr(h.TaxableValue)}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{h.IGST ? inr(h.IGST) : ''}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{h.CGST ? inr(h.CGST) : ''}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{h.SGST ? inr(h.SGST) : ''}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{inr(h.TotalValue)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Section>

      <Section id="DOCS" title="13 — Documents issued" hint="The numbers each series used, and how many were cancelled" count={data.Documents.length} open={open} toggle={toggle}>
        {data.Documents.length === 0 ? (
          <div className="px-5 py-6 text-sm text-slate-400">No document was issued in this period.</div>
        ) : (
          <div className="overflow-x-auto px-5 py-3">
            <table className="w-full min-w-[640px] text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="py-1.5 font-medium">Series</th>
                  <th className="py-1.5 font-medium">Type</th>
                  <th className="py-1.5 font-medium">From</th>
                  <th className="py-1.5 font-medium">To</th>
                  <th className="py-1.5 text-right font-medium">Issued</th>
                  <th className="py-1.5 text-right font-medium">Cancelled</th>
                  <th className="py-1.5 text-right font-medium">Net</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.Documents.map((d, i) => (
                  <tr key={i}>
                    <td className="py-1.5 font-mono text-xs text-slate-700">{d.SeriesCode}</td>
                    <td className="py-1.5 text-slate-600">{d.DocumentType}</td>
                    <td className="py-1.5 font-mono text-xs text-slate-500">{d.FromNumber}</td>
                    <td className="py-1.5 font-mono text-xs text-slate-500">{d.ToNumber}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{d.TotalIssued}</td>
                    <td className="py-1.5 text-right tabular-nums text-slate-700">{d.Cancelled}</td>
                    <td className="py-1.5 text-right tabular-nums font-semibold text-slate-800">{d.NetIssued}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Section>
    </div>
  );
}

// ── GSTR-3B ─────────────────────────────────────────────────────────────────────────────

function Gstr3bView({ data }: { data?: Gstr3bResult }) {
  if (!data) return null;
  return (
    <div className="space-y-4">
      <div
        className={cn(
          'flex gap-2.5 rounded-lg border p-3 text-sm',
          data.AgreesWithGstr1
            ? 'border-success-500/30 bg-success-50 text-success-700'
            : 'border-danger-500/30 bg-danger-50 text-danger-700',
        )}
      >
        {data.AgreesWithGstr1 ? <CheckCircle2 className="mt-0.5 size-4 shrink-0" /> : <AlertTriangle className="mt-0.5 size-4 shrink-0" />}
        <p>
          {data.AgreesWithGstr1 ? (
            <><strong>3B agrees with GSTR-1.</strong> Taxable value {inr(data.Gstr1TaxableValue)} and tax {inr(data.Gstr1TotalTax)} on both.</>
          ) : (
            <>
              <strong>3B does not agree with GSTR-1.</strong> Taxable value differs by {inr(data.VarianceTaxableValue)} and
              tax by {inr(data.VarianceTax)}. A 1-vs-3B mismatch is the most common notice a taxpayer receives,
              so this is worth settling before filing.
            </>
          )}
        </p>
      </div>

      <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
        <StatTile label="Output tax" value={inr(data.OutputIGST + data.OutputCGST + data.OutputSGST + data.OutputCess)} icon={Scale} tone="text-brand-600" />
        <StatTile label="Net ITC" value={inr(data.NetItcIGST + data.NetItcCGST + data.NetItcSGST + data.NetItcCess)} icon={Download} tone="text-success-600" />
        <StatTile label="Tax payable" value={inr(data.TotalPayable)} icon={AlertTriangle} tone="text-danger-600" />
        <StatTile label="GSTR-1 taxable" value={inr(data.Gstr1TaxableValue)} icon={ScrollText} tone="text-slate-600" />
      </div>

      <Card>
        <CardBody className="pb-2"><div className="text-sm font-semibold text-slate-700">3.1 — Outward supplies and inward reverse charge</div></CardBody>
        <div className="overflow-x-auto px-3 pb-3">
          <table className="w-full min-w-[760px] text-sm">
            <thead>
              <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-3 py-2 font-medium">Nature of supply</th>
                <th className="px-3 py-2 text-right font-medium">Taxable</th>
                <th className="px-3 py-2 text-right font-medium">IGST</th>
                <th className="px-3 py-2 text-right font-medium">CGST</th>
                <th className="px-3 py-2 text-right font-medium">SGST</th>
                <th className="px-3 py-2 text-right font-medium">Cess</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {data.Outward.map((o) => (
                <tr key={o.Code} className="hover:bg-slate-50">
                  <td className="px-3 py-2 text-slate-700"><span className="font-mono text-xs text-slate-400">({o.Code})</span> {o.Nature}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(o.TaxableValue)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{o.IGST ? inr(o.IGST) : ''}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{o.CGST ? inr(o.CGST) : ''}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{o.SGST ? inr(o.SGST) : ''}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{o.Cess ? inr(o.Cess) : ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {data.InterStateSupplies.length > 0 && (
        <Card>
          <CardBody className="pb-2"><div className="text-sm font-semibold text-slate-700">3.2 — Of 3.1(a), to unregistered persons in another state</div></CardBody>
          <div className="overflow-x-auto px-3 pb-3">
            <table className="w-full min-w-[520px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Place of supply</th>
                  <th className="px-3 py-2 font-medium">Recipient</th>
                  <th className="px-3 py-2 text-right font-medium">Taxable</th>
                  <th className="px-3 py-2 text-right font-medium">IGST</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.InterStateSupplies.map((r) => (
                  <tr key={r.PlaceOfSupplyStateCode} className="hover:bg-slate-50">
                    <td className="px-3 py-2 text-slate-700">{posLabel(r.PlaceOfSupplyStateCode, r.PlaceOfSupplyName)}</td>
                    <td className="px-3 py-2 text-slate-500">{r.Recipient}</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(r.TaxableValue)}</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(r.IGST)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      <Card>
        <CardBody className="pb-2"><div className="text-sm font-semibold text-slate-700">4 — Input tax credit</div></CardBody>
        <div className="overflow-x-auto px-3 pb-3">
          <table className="w-full min-w-[700px] text-sm">
            <thead>
              <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-3 py-2 font-medium">Details</th>
                <th className="px-3 py-2 text-right font-medium">IGST</th>
                <th className="px-3 py-2 text-right font-medium">CGST</th>
                <th className="px-3 py-2 text-right font-medium">SGST</th>
                <th className="px-3 py-2 text-right font-medium">Cess</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {data.Itc.map((i) => (
                <tr key={i.Code} className={cn('hover:bg-slate-50', i.Code === 'C' && 'font-semibold')}>
                  <td className="px-3 py-2 text-slate-700"><span className="font-mono text-xs text-slate-400">{i.Code}</span> {i.Detail}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(i.IGST)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(i.CGST)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(i.SGST)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(i.Cess)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      <Card>
        <CardBody className="pb-2"><div className="text-sm font-semibold text-slate-700">6.1 — Tax payable</div></CardBody>
        <div className="overflow-x-auto px-3 pb-3">
          <table className="w-full min-w-[600px] text-sm">
            <thead>
              <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-3 py-2 font-medium" />
                <th className="px-3 py-2 text-right font-medium">IGST</th>
                <th className="px-3 py-2 text-right font-medium">CGST</th>
                <th className="px-3 py-2 text-right font-medium">SGST</th>
                <th className="px-3 py-2 text-right font-medium">Cess</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              <tr><td className="px-3 py-2 text-slate-600">Output tax</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OutputIGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OutputCGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OutputSGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OutputCess)}</td></tr>
              <tr><td className="px-3 py-2 text-slate-600">Credit brought forward from earlier months</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OpeningCreditIGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OpeningCreditCGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OpeningCreditSGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.OpeningCreditCess)}</td></tr>
              <tr><td className="px-3 py-2 text-slate-600">Credit claimed this month (net ITC)</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.NetItcIGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.NetItcCGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.NetItcSGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{inr(data.NetItcCess)}</td></tr>
              <tr className="border-t-2 border-slate-300 bg-slate-50 font-semibold">
                <td className="px-3 py-2 text-slate-800">Payable in cash</td>
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.PayableIGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.PayableCGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.PayableSGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.PayableCess)}</td></tr>
              <tr><td className="px-3 py-2 text-slate-600">Credit carried forward to next month</td>
                <td className="px-3 py-2 text-right tabular-nums text-success-700">{inr(data.ClosingCreditIGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-success-700">{inr(data.ClosingCreditCGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-success-700">{inr(data.ClosingCreditSGST)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-success-700">{inr(data.ClosingCreditCess)}</td></tr>
            </tbody>
          </table>
          <p className="px-3 pt-2 text-xs text-slate-400">
            Credit is used in the order the law sets: IGST credit pays IGST first, then CGST and SGST; CGST
            and SGST credit pay their own head, then IGST. Whatever is not used carries forward — it is
            not a refund on this return.
          </p>
        </div>
      </Card>
    </div>
  );
}

// ── GSTR-9 ──────────────────────────────────────────────────────────────────────────────

function Gstr9View({ data }: { data?: Gstr9Result }) {
  if (!data) return null;
  return (
    <div className="space-y-4">
      <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
        <StatTile label="Turnover (books)" value={inr(data.TotalTurnover)} icon={Scale} tone="text-brand-600" />
        <StatTile label="Declared in returns" value={inr(data.TotalFiledTaxableValue)} icon={ScrollText} tone="text-slate-600" />
        <StatTile label="Variance" value={inr(data.TotalVarianceTaxableValue)} icon={AlertTriangle}
          tone={Math.abs(data.TotalVarianceTaxableValue) < 1 ? 'text-success-600' : 'text-danger-600'} />
        <StatTile label="Months filed" value={`${data.MonthsFiled} of 12`} icon={CheckCircle2}
          tone={data.MonthsMissing === 0 ? 'text-success-600' : 'text-warning-600'} />
      </div>

      <Card>
        <CardBody className="pb-2">
          <div className="text-sm font-semibold text-slate-700">The year, month by month</div>
          <p className="mt-1 text-xs text-slate-400">
            "As filed" comes from the saved GSTR-1 snapshots, never from recomputing — recomputing would
            answer "as per books" twice and hide the difference this return exists to surface.
          </p>
        </CardBody>
        <div className="overflow-x-auto px-3 pb-3">
          <table className="w-full min-w-[700px] text-sm">
            <thead>
              <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-3 py-2 font-medium">Month</th>
                <th className="px-3 py-2 text-right font-medium">Books taxable</th>
                <th className="px-3 py-2 text-right font-medium">Books tax</th>
                <th className="px-3 py-2 text-right font-medium">Filed taxable</th>
                <th className="px-3 py-2 text-right font-medium">Filed tax</th>
                <th className="px-3 py-2 text-right font-medium">Variance</th>
                <th className="px-3 py-2 font-medium">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {data.Months.map((m) => (
                <tr key={m.PeriodKey} className="hover:bg-slate-50">
                  <td className="px-3 py-2 text-slate-700">{m.PeriodLabel}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(m.BooksTaxableValue)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(m.BooksTax)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{m.WasFiled ? inr(m.FiledTaxableValue) : '—'}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{m.WasFiled ? inr(m.FiledTax) : '—'}</td>
                  <td className="px-3 py-2 text-right">
                    <span className={cn('tabular-nums', Math.abs(m.VarianceTaxableValue) > 1 ? 'font-semibold text-danger-600' : 'text-slate-400')}>
                      {inr(m.VarianceTaxableValue)}
                    </span>
                  </td>
                  <td className="px-3 py-2">
                    <Badge tone={m.WasFiled ? 'success' : 'warning'}>{m.WasFiled ? 'filed' : 'not filed'}</Badge>
                  </td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr className="border-t-2 border-slate-300 bg-slate-50 font-semibold">
                <td className="px-3 py-2 text-slate-800">Total</td>
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.TotalTurnover)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.TotalTaxPayable)}</td>
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.TotalFiledTaxableValue)}</td>
                <td className="px-3 py-2" />
                <td className="px-3 py-2 text-right tabular-nums text-slate-800">{inr(data.TotalVarianceTaxableValue)}</td>
                <td className="px-3 py-2" />
              </tr>
            </tfoot>
          </table>
        </div>
      </Card>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardBody className="pb-2"><div className="text-sm font-semibold text-slate-700">Outward supplies by rate</div></CardBody>
          <div className="px-3 pb-3">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Rate</th>
                  <th className="px-3 py-2 text-right font-medium">Taxable</th>
                  <th className="px-3 py-2 text-right font-medium">Tax</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.OutwardByRate.map((s) => (
                  <tr key={s.Rate}>
                    <td className="px-3 py-2 tabular-nums text-slate-700">{s.Rate}%</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(s.TaxableValue)}</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(s.IGST + s.CGST + s.SGST + s.Cess)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>

        <Card>
          <CardBody className="pb-2"><div className="text-sm font-semibold text-slate-700">Input tax credit for the year</div></CardBody>
          <div className="px-3 pb-3">
            <table className="w-full text-sm">
              <tbody className="divide-y divide-slate-100">
                <tr><td className="px-3 py-2 text-slate-600">IGST availed</td><td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(data.ItcAvailedIGST)}</td></tr>
                <tr><td className="px-3 py-2 text-slate-600">CGST availed</td><td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(data.ItcAvailedCGST)}</td></tr>
                <tr><td className="px-3 py-2 text-slate-600">SGST availed</td><td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(data.ItcAvailedSGST)}</td></tr>
                <tr><td className="px-3 py-2 text-slate-600">Cess availed</td><td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(data.ItcAvailedCess)}</td></tr>
                <tr className="border-t-2 border-slate-300"><td className="px-3 py-2 font-semibold text-slate-800">Reversed</td><td className="px-3 py-2 text-right tabular-nums font-semibold text-danger-600">{inr(data.ItcReversed)}</td></tr>
              </tbody>
            </table>
          </div>
        </Card>
      </div>
    </div>
  );
}
