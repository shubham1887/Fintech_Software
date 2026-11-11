import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { FilePlus2, FileText, Layers, Printer, Search } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr, apiDate, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useCsvDownload } from '@/lib/exportAll';
import { useAuth } from '@/features/auth/AuthContext';
import { canRaiseDocument, currentFyRange, documentTypeLabel, statusTone } from '@/lib/gstDocs';
import { invoiceTemplateCss, useInvoiceTemplate, type InvoiceTemplate } from '@/lib/invoiceTemplate';
import { imageUrl } from '@/api/catalog.api';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { pickIcon } from '@/components/layout/iconMap';
import { GenerateInvoicesPanel } from './GenerateMonthDialog';
import type { CustomScreenProps } from './registry';

interface DocHeader {
  TaxInvoiceHeaderID: number;
  Direction: string;
  DocumentType: string;
  DocumentNo: string;
  DocumentDate: string;
  PartyUserID: number;
  PartyName: string;
  PartyGSTIN: string;
  PartyRegistrationStatus: string;
  CompanyGSTIN: string;
  SupplierStateCode: string;
  PlaceOfSupplyStateCode: string;
  SupplyNature: string;
  TaxableTotal: number;
  CGSTTotal: number;
  SGSTTotal: number;
  IGSTTotal: number;
  CessTotal: number;
  RoundOff: number;
  GrandTotal: number;
  Status: string;
  PeriodKey: string;
  Remark: string;
}

interface DocLine {
  TaxInvoiceLineID: number;
  LineNo: number;
  Description: string;
  SACCode: string;
  Quantity: number;
  TaxableValue: number;
  CGSTRate: number; CGSTAmount: number;
  SGSTRate: number; SGSTAmount: number;
  IGSTRate: number; IGSTAmount: number;
  LineTotal: number;
}

interface AnnexureLine {
  TaxInvoiceAnnexureLineID: number;
  TransactionDate: string;
  ReferenceNo: string;
  TransactionMasterID: number;
  FundTransactionID: number;
  GrossValue: number;
}

interface DocumentDetail {
  Header: DocHeader;
  Lines: DocLine[];
  Annexure: AnnexureLine[];
}

/** The company's own details as printed at the top of a bill — from Invoice Settings. */
interface Company {
  name: string; address: string; gstin: string; pan: string; state: string;
  email: string; website: string; bank: string; account: string; ifsc: string; holder: string; note: string;
  logo: string;
}

function useCompany(): Company {
  const { data } = useQuery({
    queryKey: ['crud', 'InvoiceSetting/GetAll'],
    queryFn: () => fetchList('InvoiceSetting/GetAll', { PageNumber: 1, PageSize: 50 }),
  });
  const rows = data ?? [];
  const p = rows.length
    ? [...rows].sort((a, b) => Number(getField(a, 'InvoiceSettingID') ?? 0) - Number(getField(b, 'InvoiceSettingID') ?? 0))[0]
    : {};
  const s = (k: string) => String(getField(p, k) ?? '').trim();
  return {
    name: s('CompanyName'), address: s('FullAddress') || s('Address'), gstin: s('GSTIN'), pan: s('PAN'),
    state: s('State'), email: s('Email'), website: s('WebsiteName'), bank: s('BankName'),
    account: s('AccountNo'), ifsc: s('IFSC'), holder: s('AccountHolderName'), note: s('Note'),
    logo: s('LogoURL'),
  };
}

// ── Amount in words (Indian numbering) ─────────────────────────────────────────────────────
const ONES = ['', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten', 'Eleven', 'Twelve',
  'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];
const TENS = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

function below100(n: number): string {
  if (n < 20) return ONES[n];
  return `${TENS[Math.floor(n / 10)]}${n % 10 ? ` ${ONES[n % 10]}` : ''}`;
}
function below1000(n: number): string {
  const h = Math.floor(n / 100), r = n % 100;
  return `${h ? `${ONES[h]} Hundred${r ? ' ' : ''}` : ''}${r ? below100(r) : ''}`;
}
export function rupeesInWords(value: number): string {
  const v = Math.abs(Math.round((value || 0) * 100));
  let rupees = Math.floor(v / 100);
  const paise = v % 100;
  if (rupees === 0 && paise === 0) return 'Zero Rupees Only';
  const parts: string[] = [];
  const crore = Math.floor(rupees / 10000000); rupees %= 10000000;
  const lakh = Math.floor(rupees / 100000); rupees %= 100000;
  const thousand = Math.floor(rupees / 1000); rupees %= 1000;
  if (crore) parts.push(`${crore > 999 ? crore.toLocaleString('en-IN') : below1000(crore)} Crore`);
  if (lakh) parts.push(`${below100(lakh)} Lakh`);
  if (thousand) parts.push(`${below100(thousand)} Thousand`);
  if (rupees) parts.push(below1000(rupees));
  const r = parts.length ? `${parts.join(' ')} Rupees` : '';
  const p = paise ? `${below100(paise)} Paise` : '';
  return `${[r, p].filter(Boolean).join(' and ')} Only`;
}

const esc = (v: unknown) =>
  String(v ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const amt = (v: number) => (v || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/**
 * The bill as a self-contained HTML page. One builder feeds both the on-screen view (in a
 * frame) and the print window, so what is printed is exactly what was looked at.
 */
function billHtml(detail: DocumentDetail, c: Company, template: InvoiceTemplate = 'Classic'): string {
  const h = detail.Header;
  const brand = (typeof window !== 'undefined'
    ? getComputedStyle(document.documentElement).getPropertyValue('--color-brand-600').trim()
    : '') || '#1d4ed8';
  const logo = c.logo ? `<img class="logo" src="${esc(imageUrl(c.logo))}" alt="" />` : '';
  const intra = (h.SupplyNature || '').toUpperCase() === 'INTRASTATE';
  const cancelled = (h.Status || '').toUpperCase() === 'CANCELLED';
  const title = h.DocumentType === 'BillOfSupply' ? 'BILL OF SUPPLY'
    : h.DocumentType === 'CreditNote' ? 'CREDIT NOTE'
    : h.DocumentType === 'DebitNote' ? 'DEBIT NOTE' : 'TAX INVOICE';

  const lineRows = detail.Lines.map((l, i) => {
    const rate = (l.CGSTRate || 0) + (l.SGSTRate || 0) + (l.IGSTRate || 0);
    return `<tr>
      <td class="c">${i + 1}</td>
      <td>${esc(l.Description)}</td>
      <td class="c">${esc(l.SACCode)}</td>
      <td class="r">${l.Quantity || 1}</td>
      <td class="r">${amt(l.TaxableValue)}</td>
      <td class="c">${rate ? `${rate}%` : '—'}</td>
      ${intra
        ? `<td class="r">${amt(l.CGSTAmount)}</td><td class="r">${amt(l.SGSTAmount)}</td>`
        : `<td class="r">${amt(l.IGSTAmount)}</td>`}
      <td class="r b">${amt(l.LineTotal)}</td>
    </tr>`;
  }).join('');

  const taxHead = intra ? '<th class="r">CGST</th><th class="r">SGST</th>' : '<th class="r">IGST</th>';
  const taxFoot = intra
    ? `<td class="r b">${amt(h.CGSTTotal)}</td><td class="r b">${amt(h.SGSTTotal)}</td>`
    : `<td class="r b">${amt(h.IGSTTotal)}</td>`;

  const annexure = detail.Annexure.length > 1
    ? `<h3>Annexure — the ${detail.Annexure.length} transactions this invoice covers</h3>
       <table><thead><tr><th class="c">#</th><th>Date</th><th>Reference</th><th class="r">Amount</th></tr></thead><tbody>
       ${detail.Annexure.map((a, i) => `<tr><td class="c">${i + 1}</td><td>${esc(dateOnly(a.TransactionDate))}</td>
         <td>${esc(a.ReferenceNo || (a.FundTransactionID > 0 ? `Wallet top-up #${a.FundTransactionID}` : `Transaction #${a.TransactionMasterID}`))}</td>
         <td class="r">${amt(a.GrossValue)}</td></tr>`).join('')}
       </tbody></table>`
    : '';

  return `<!doctype html><html><head><meta charset="utf-8"><title>${esc(h.DocumentNo)}</title>
<style>
  *{box-sizing:border-box} body{font-family:Arial,Helvetica,sans-serif;color:#1e293b;font-size:12px;margin:0;padding:24px;background:#fff}
  .wrap{max-width:820px;margin:0 auto;position:relative}
  .top{display:flex;justify-content:space-between;gap:24px;border-bottom:2px solid #1e293b;padding-bottom:12px}
  .co{font-size:18px;font-weight:700} .muted{color:#64748b} .title{font-size:16px;font-weight:700;letter-spacing:.06em;text-align:right}
  .meta{display:flex;justify-content:space-between;gap:24px;margin:14px 0}
  .box{border:1px solid #cbd5e1;border-radius:6px;padding:10px;flex:1} .lbl{font-size:10px;text-transform:uppercase;letter-spacing:.05em;color:#64748b;margin-bottom:4px}
  table{width:100%;border-collapse:collapse;margin-top:6px} th,td{border:1px solid #cbd5e1;padding:6px 8px;vertical-align:top}
  th{background:#f1f5f9;font-size:10px;text-transform:uppercase;letter-spacing:.04em;text-align:left}
  .r{text-align:right;white-space:nowrap} .c{text-align:center} .b{font-weight:700}
  .tot{display:flex;justify-content:flex-end;margin-top:10px} .tot table{width:320px} .tot td{border:none;border-bottom:1px solid #e2e8f0}
  .grand td{font-size:14px;font-weight:700;border-bottom:2px solid #1e293b}
  h3{font-size:12px;margin:18px 0 4px} .foot{display:flex;justify-content:space-between;gap:24px;margin-top:22px}
  .sign{text-align:right;margin-top:40px} .stamp{position:absolute;top:120px;left:50%;transform:translateX(-50%) rotate(-18deg);font-size:64px;font-weight:700;color:rgba(220,38,38,.18);border:6px solid rgba(220,38,38,.18);padding:4px 24px;border-radius:12px}
  @media print{body{padding:0} .wrap{max-width:none}}
  ${invoiceTemplateCss(template, brand)}
</style></head><body><div class="wrap">
  ${cancelled ? '<div class="stamp">CANCELLED</div>' : ''}
  <div class="top">
    <div>
      ${logo}
      <div class="co">${esc(c.name || 'Company')}</div>
      <div class="muted" style="max-width:420px">${esc(c.address)}</div>
      <div style="margin-top:4px"><b>GSTIN:</b> ${esc(h.CompanyGSTIN || c.gstin)}${c.pan ? ` &nbsp; <b>PAN:</b> ${esc(c.pan)}` : ''}</div>
      ${c.email || c.website ? `<div class="muted">${esc([c.email, c.website].filter(Boolean).join(' · '))}</div>` : ''}
    </div>
    <div>
      <div class="title">${title}</div>
      <table style="margin-top:8px;width:auto;margin-left:auto">
        <tr><td class="muted" style="border:none;padding:2px 8px">Invoice no.</td><td class="b" style="border:none;padding:2px 0">${esc(h.DocumentNo)}</td></tr>
        <tr><td class="muted" style="border:none;padding:2px 8px">Date</td><td class="b" style="border:none;padding:2px 0">${esc(dateOnly(h.DocumentDate))}</td></tr>
      </table>
    </div>
  </div>

  <div class="meta">
    <div class="box">
      <div class="lbl">Billed to</div>
      <div class="b" style="font-size:13px">${esc(h.PartyName || `User ${h.PartyUserID}`)}</div>
      <div>${h.PartyGSTIN ? `<b>GSTIN:</b> ${esc(h.PartyGSTIN)}` : 'Unregistered (no GSTIN)'}</div>
    </div>
    <div class="box">
      <div class="lbl">Supply</div>
      <div><b>Place of supply:</b> state code ${esc(h.PlaceOfSupplyStateCode || '—')}</div>
      <div>${intra ? 'Within the state — CGST + SGST' : 'To another state — IGST'}</div>
    </div>
  </div>

  <table>
    <thead><tr><th class="c">#</th><th>Description</th><th class="c">SAC</th><th class="r">Qty</th><th class="r">Taxable value</th><th class="c">GST</th>${taxHead}<th class="r">Total</th></tr></thead>
    <tbody>${lineRows}</tbody>
    <tfoot><tr><td colspan="4" class="r b">Total</td><td class="r b">${amt(h.TaxableTotal)}</td><td></td>${taxFoot}<td class="r b">${amt(h.GrandTotal - (h.RoundOff || 0))}</td></tr></tfoot>
  </table>

  <div class="tot"><table>
    <tr><td>Taxable value</td><td class="r">${amt(h.TaxableTotal)}</td></tr>
    ${intra
      ? `<tr><td>CGST</td><td class="r">${amt(h.CGSTTotal)}</td></tr><tr><td>SGST</td><td class="r">${amt(h.SGSTTotal)}</td></tr>`
      : `<tr><td>IGST</td><td class="r">${amt(h.IGSTTotal)}</td></tr>`}
    ${h.RoundOff ? `<tr><td>Round off</td><td class="r">${amt(h.RoundOff)}</td></tr>` : ''}
    <tr class="grand"><td>Invoice total</td><td class="r">₹ ${amt(h.GrandTotal)}</td></tr>
  </table></div>
  <div style="margin-top:8px"><b>In words:</b> ${esc(rupeesInWords(h.GrandTotal))}</div>

  ${annexure}

  <div class="foot">
    <div style="max-width:440px">
      ${c.bank || c.account ? `<div class="lbl">Bank details</div><div>${esc([c.holder, c.bank, c.account && `A/c ${c.account}`, c.ifsc && `IFSC ${c.ifsc}`].filter(Boolean).join(' · '))}</div>` : ''}
      ${c.note ? `<div class="muted" style="margin-top:8px">${esc(c.note)}</div>` : ''}
    </div>
    <div class="sign"><div>For <b>${esc(c.name || 'Company')}</b></div><div style="margin-top:36px" class="muted">Authorised signatory</div></div>
  </div>
  <div class="muted" style="margin-top:16px;text-align:center;font-size:10px">This is a computer-generated invoice.</div>
</div></body></html>`;
}

function printHtml(html: string) {
  const w = window.open('', '_blank', 'width=900,height=1000');
  if (!w) return false;
  w.document.open();
  w.document.write(html);
  w.document.close();
  w.focus();
  // Give the new window a moment to lay the page out before the print dialog measures it.
  setTimeout(() => w.print(), 300);
  return true;
}

interface Filters { from: string; to: string; status: string; search: string }

/**
 * Invoices — the sale bills.
 *
 * Every invoice raised from an approved wallet top-up or from the month's service charges,
 * as a bill that can be opened and printed. Invoices are raised by "Generate invoices" (and
 * automatically on the 2nd of each month for the month just ended), so a top-up approved this
 * month appears here once its invoice has been generated.
 */
export function InvoicesScreen({ def }: CustomScreenProps) {
  const { user } = useAuth();
  const downloadCsv = useCsvDownload();
  const company = useCompany();
  const mayIssue = canRaiseDocument(user?.UserTypeID);

  const fy = currentFyRange();
  const [filters, setFilters] = useState<Filters>({ from: fy.from, to: fy.to, status: 'ISSUED', search: '' });
  const [applied, setApplied] = useState({ from: fy.from, to: fy.to });
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [generateOpen, setGenerateOpen] = useState(false);
  const set = (k: keyof Filters, v: string) => setFilters((s) => ({ ...s, [k]: v }));

  const list = useQuery({
    queryKey: ['sale-invoices', applied],
    queryFn: async (): Promise<DocHeader[]> => {
      const res = await apiPost<DocHeader[]>('TaxInvoice/Select', {
        PeriodFrom: applied.from, PeriodTo: applied.to, Direction: 'SALE', DocumentType: '', PartyUserID: 0,
      });
      if (!res.ok) throw new Error(res.message || 'The invoices could not be loaded.');
      return res.data ?? [];
    },
  });

  const rows = useMemo(() => {
    const q = filters.search.trim().toLowerCase();
    return (list.data ?? []).filter((r) => {
      if (filters.status && (r.Status || '').toUpperCase() !== filters.status) return false;
      if (!q) return true;
      return `${r.DocumentNo} ${r.PartyName} ${r.PartyGSTIN}`.toLowerCase().includes(q);
    });
  }, [list.data, filters.status, filters.search]);

  const totals = useMemo(() => {
    const issued = rows.filter((r) => (r.Status || '').toUpperCase() === 'ISSUED');
    return {
      count: issued.length,
      taxable: issued.reduce((a, r) => a + (r.TaxableTotal || 0), 0),
      tax: issued.reduce((a, r) => a + (r.CGSTTotal || 0) + (r.SGSTTotal || 0) + (r.IGSTTotal || 0) + (r.CessTotal || 0), 0),
      total: issued.reduce((a, r) => a + (r.GrandTotal || 0), 0),
    };
  }, [rows]);

  const detail = useQuery({
    queryKey: ['sale-invoice-detail', selectedId],
    enabled: selectedId !== null,
    queryFn: async (): Promise<DocumentDetail> => {
      const res = await apiPost<DocumentDetail>('TaxInvoice/GetById', { TaxInvoiceHeaderID: selectedId });
      if (!res.ok || !res.data) throw new Error(res.message || 'The invoice could not be loaded.');
      return res.data;
    },
  });
  const { template } = useInvoiceTemplate();
  const html = useMemo(() => (detail.data ? billHtml(detail.data, company, template) : ''), [detail.data, company, template]);

  const exportRows = () =>
    downloadCsv(
      rows.map((r) => ({
        Invoice: r.DocumentNo, Date: dateOnly(r.DocumentDate), Customer: r.PartyName, GSTIN: r.PartyGSTIN,
        Taxable: r.TaxableTotal, CGST: r.CGSTTotal, SGST: r.SGSTTotal, IGST: r.IGSTTotal, Total: r.GrandTotal, Status: r.Status,
      })),
      `invoices-${applied.from}-to-${applied.to}.csv`,
    );

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Every sale invoice — open one to see the bill, print it or save it as PDF"
        icon={pickIcon(def.title)}
        actions={
          mayIssue ? (
            <Button onClick={() => setGenerateOpen((o) => !o)} variant={generateOpen ? 'secondary' : 'primary'}>
              <FilePlus2 className="size-4" />
              {generateOpen ? 'Close generator' : 'Generate invoices'}
            </Button>
          ) : undefined
        }
      />

      {generateOpen && (
        <GenerateInvoicesPanel onClose={() => setGenerateOpen(false)} onIssued={() => list.refetch()} />
      )}

      <div className={cn(STAT_STRIP, 'grid-cols-2 lg:grid-cols-4')}>
        <StatTile label="Invoices" value={totals.count} icon={FileText} tone="text-brand-600" loading={list.isFetching} />
        <StatTile label="Taxable value" value={inr(totals.taxable)} icon={Layers} tone="text-slate-600" loading={list.isFetching} />
        <StatTile label="GST" value={inr(totals.tax)} icon={Layers} tone="text-info-600" loading={list.isFetching} />
        <StatTile label="Invoice total" value={inr(totals.total)} icon={Layers} tone="text-success-600" loading={list.isFetching} />
      </div>

      <Card>
        <CardBody className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <Input label="From" type="date" value={filters.from} max={filters.to} onChange={(e) => set('from', e.target.value)} />
          <Input label="To" type="date" value={filters.to} max={apiDate(new Date())} onChange={(e) => set('to', e.target.value)} />
          <Select label="Status" value={filters.status} onChange={(e) => set('status', e.target.value)}>
            <option value="ISSUED">Issued</option>
            <option value="CANCELLED">Cancelled</option>
            <option value="">All</option>
          </Select>
          <Input label="Search" placeholder="Invoice no. or customer" value={filters.search} onChange={(e) => set('search', e.target.value)} />
          <div className="flex items-end gap-2">
            <Button onClick={() => setApplied({ from: filters.from, to: filters.to })} loading={list.isFetching} className="flex-1">
              <Search className="mr-1.5 size-4" />
              Show
            </Button>
            <Button variant="outline" onClick={exportRows} disabled={rows.length === 0}>CSV</Button>
          </div>
        </CardBody>
      </Card>

      <Card>
        {list.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>
        ) : list.isError ? (
          <CardBody><EmptyState title="Could not load" description={(list.error as Error).message} /></CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title="No invoices for these dates"
              description={mayIssue
                ? 'Invoices are raised for approved wallet top-ups and for the month\'s service charges. Press Generate invoices to raise them — it also happens automatically on the 2nd of every month.'
                : 'No invoice has been raised for these dates yet.'}
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[860px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Invoice</th>
                  <th className="px-3 py-2.5 font-medium">Date</th>
                  <th className="px-3 py-2.5 font-medium">Customer</th>
                  <th className="px-3 py-2.5 text-right font-medium">Taxable</th>
                  <th className="px-3 py-2.5 text-right font-medium">GST</th>
                  <th className="px-3 py-2.5 text-right font-medium">Total</th>
                  <th className="px-3 py-2.5 font-medium">Status</th>
                  <th className="px-3 py-2.5" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => {
                  const tax = (r.CGSTTotal || 0) + (r.SGSTTotal || 0) + (r.IGSTTotal || 0) + (r.CessTotal || 0);
                  return (
                    <tr key={r.TaxInvoiceHeaderID} className="cursor-pointer transition-colors hover:bg-slate-50" onClick={() => setSelectedId(r.TaxInvoiceHeaderID)}>
                      <td className="px-5 py-2.5">
                        <div className="font-mono text-sm font-semibold text-slate-800">{r.DocumentNo || '—'}</div>
                        <div className="text-xs text-slate-400">{documentTypeLabel(r.DocumentType)}</div>
                      </td>
                      <td className="px-3 py-2.5 text-slate-600">{dateOnly(r.DocumentDate)}</td>
                      <td className="px-3 py-2.5">
                        <div className="max-w-[220px] truncate text-slate-700" title={r.PartyName}>{r.PartyName || '—'}</div>
                        <div className="text-xs text-slate-400">{r.PartyGSTIN ? <span className="font-mono">{r.PartyGSTIN}</span> : 'Unregistered'}</div>
                      </td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{inr(r.TaxableTotal)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{inr(tax)}</td>
                      <td className="px-3 py-2.5 text-right font-semibold tabular-nums text-slate-900">{inr(r.GrandTotal)}</td>
                      <td className="px-3 py-2.5"><Badge tone={statusTone(r.Status || '')}>{r.Status || '—'}</Badge></td>
                      <td className="px-3 py-2.5 text-right">
                        <span className="inline-flex items-center gap-1 text-sm font-medium text-brand-600">
                          <FileText className="size-4" /> View bill
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal open={selectedId !== null} onClose={() => setSelectedId(null)} title="Invoice" size="xl">
        {detail.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>
        ) : detail.isError ? (
          <EmptyState title="Could not load" description={(detail.error as Error).message} />
        ) : detail.data ? (
          <div className="space-y-3">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="text-sm text-slate-500">
                To save as PDF, press Print and choose <strong>Save as PDF</strong> as the printer.
              </p>
              <Button onClick={() => printHtml(html)}>
                <Printer className="size-4" />
                Print / Save PDF
              </Button>
            </div>
            <iframe
              title={detail.data.Header.DocumentNo}
              srcDoc={html}
              sandbox=""
              className="h-[70vh] w-full rounded-lg border border-slate-200 bg-white"
            />
          </div>
        ) : null}
      </Modal>
    </div>
  );
}
