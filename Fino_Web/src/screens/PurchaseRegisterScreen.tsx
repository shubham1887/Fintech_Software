import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, Download, Eye, FileJson, FilePlus2, ListChecks, Pencil, Plus, RefreshCw, Trash2, Upload,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly, apiDate } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { canOperate, currentFyRange } from '@/lib/gstDocs';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
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
import type { CustomScreenProps } from './registry';

// ── What the API sends ─────────────────────────────────────────────────────────────────────

interface PurchaseRow {
  TaxInvoiceHeaderID: number;
  DocumentNo: string;
  DocumentDate: string;
  WalletCreditDate: string | null;
  PartyName: string;
  PartyGSTIN: string;
  SupplierStateCode: string;
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
  ProviderID: number | null;
  WalletID: number | null;
  Remark: string;
  ProviderName: string;
  Items: string;
  EligibleITC: number;
  ItcStatus: string;
  ItcBucket: string;
  Locked: boolean;
}

interface StoredLine {
  TaxInvoiceLineID: number;
  LineNo: number;
  Description: string;
  SACCode: string;
  TaxableValue: number;
  CGSTRate: number; CGSTAmount: number;
  SGSTRate: number; SGSTAmount: number;
  IGSTRate: number; IGSTAmount: number;
  CessAmount: number;
  LineTotal: number;
  ITCEligible: boolean;
  UnitQuantity: number;
  UnitRate: number;
  RateType: string;
}

interface PurchaseDetail {
  Header: PurchaseRow;
  Lines: StoredLine[];
  ProviderName: string;
  WalletName: string;
  ItcStatus: string;
  ItcBucket: string;
  EligibleITC: number;
  LockReason: string;
}

interface ProviderOpt {
  ProviderID: number; ProviderName: string; IsActive: boolean;
  SupplierName: string; SupplierGSTIN: string; SupplierStateCode: string; WalletID: number; Guessed: boolean;
}
interface WalletOpt { WalletID: number; Name: string; ShortName: string; IsActive: boolean }
interface Item {
  PurchaseItemMasterID: number; Name: string; SACCode: string; GSTRatePercent: number;
  DefaultRate: number; RateType: string; ITCEligible: boolean; IsActive: boolean;
}
interface SupplierOpt { SupplierName: string; SupplierGSTIN: string; SupplierStateCode: string; Bills: number }
interface Lookups {
  Providers: ProviderOpt[]; Wallets: WalletOpt[]; Items: Item[]; Suppliers: SupplierOpt[];
  CompanyGSTIN: string; CompanyStateCode: string;
}

interface CaptureResult {
  TaxInvoiceHeaderID: number; DocumentNo: string; SupplierName: string;
  GrandTotal: number; EligibleITC: number; AlreadyExisted: boolean; Warning: string;
}

// ── The form ───────────────────────────────────────────────────────────────────────────────

type RateType = 'PERCENT' | 'UNIT';

/** One line as it is typed. Everything is a string until it is sent. */
interface LineDraft {
  key: number;
  itemId: string;
  Description: string;
  SACCode: string;
  qty: string;
  rate: string;
  rateType: RateType;
  /** The line amount. Follows quantity × rate until somebody types over it. */
  amount: string;
  amountTyped: boolean;
  gst: string;
  ITCEligible: boolean;
  /** Used only when "enter tax by hand" is on — the figures exactly as the supplier printed them. */
  taxable: string; cgst: string; sgst: string; igst: string;
}

interface Head {
  id: number;
  ProviderID: string;
  SupplierName: string;
  SupplierGSTIN: string;
  SupplierStateCode: string;
  DocumentNo: string;
  DocumentDate: string;
  WalletID: string;
  WalletCreditDate: string;
  Remark: string;
  /** Whether the amounts on the bill already contain the GST. */
  inclusive: boolean;
  /** Type every tax figure by hand instead of working it out. */
  manualTax: boolean;
  /** The total printed on the supplier's bill — the round-off is whatever is left over. */
  billTotal: string;
}

const n = (v: string | number | null | undefined) => {
  const x = Number(String(v ?? '').replace(/,/g, '').trim());
  return Number.isFinite(x) ? x : 0;
};
const r2 = (v: number) => Math.round((v + Number.EPSILON) * 100) / 100;
const str = (v: number) => (v ? String(r2(v)) : '');

let keySeed = 1;
const blankLine = (): LineDraft => ({
  key: keySeed++, itemId: '', Description: '', SACCode: '', qty: '', rate: '', rateType: 'PERCENT',
  amount: '', amountTyped: false, gst: '18', ITCEligible: true, taxable: '', cgst: '', sgst: '', igst: '',
});

const blankHead = (): Head => ({
  id: 0, ProviderID: '', SupplierName: '', SupplierGSTIN: '', SupplierStateCode: '', DocumentNo: '',
  DocumentDate: apiDate(new Date()), WalletID: '', WalletCreditDate: '', Remark: '',
  inclusive: false, manualTax: false, billTotal: '',
});

/** Quantity × rate — as a percentage of the quantity, or per unit of it. */
function rated(l: LineDraft): number | null {
  const q = n(l.qty), rt = n(l.rate);
  if (!q || !rt) return null;
  return r2(l.rateType === 'PERCENT' ? (q * rt) / 100 : q * rt);
}

/** The figures a line comes to. One function, so the table, the totals and the request agree. */
function calc(l: LineDraft, inclusive: boolean, intra: boolean, manual: boolean) {
  const byRate = rated(l);
  const amount = !l.amountTyped && byRate !== null ? byRate : n(l.amount);
  const g = n(l.gst);

  if (manual) {
    const taxable = n(l.taxable), cgst = n(l.cgst), sgst = n(l.sgst), igst = n(l.igst);
    return { amount, taxable, cgst, sgst, igst, total: r2(taxable + cgst + sgst + igst), g };
  }

  const taxable = inclusive ? r2(amount / (1 + g / 100)) : r2(amount);
  const tax = inclusive ? r2(amount - taxable) : r2((amount * g) / 100);
  const cgst = intra ? r2(tax / 2) : 0;
  const sgst = intra ? r2(tax - cgst) : 0;
  const igst = intra ? 0 : tax;
  return { amount, taxable, cgst, sgst, igst, total: r2(taxable + cgst + sgst + igst), g };
}

/** Where a bill's credit stands, in words a non-accountant can act on. */
function creditBadge(status: string, bucket: string, eligible: number): { label: string; tone: 'success' | 'warning' | 'danger' | 'neutral' | 'info' } {
  const s = (status || '').toLowerCase(), b = (bucket || '').toLowerCase();
  if (s === 'claimed') return { label: 'Credit confirmed', tone: 'success' };
  if (s === 'reversed') return { label: 'Credit reversed', tone: 'danger' };
  if (s === 'ineligible' || (!s && eligible <= 0)) return { label: 'No credit', tone: 'neutral' };
  if (b === 'mismatch') return { label: 'Tax differs from 2B', tone: 'danger' };
  if (b === 'missingin2b') return { label: 'Supplier has not filed', tone: 'warning' };
  if (b === 'matched') return { label: 'Matched in 2B', tone: 'info' };
  return { label: 'Waiting for 2B', tone: 'warning' };
}

export function PurchaseRegisterScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayWrite = canOperate(user?.UserTypeID);

  const fy = currentFyRange();
  const [from, setFrom] = useState(fy.from);
  const [to, setTo] = useState(fy.to);
  const [applied, setApplied] = useState({ from: fy.from, to: fy.to });

  const purchases = useQuery({
    queryKey: ['purchase-register', applied.from, applied.to],
    queryFn: async (): Promise<PurchaseRow[]> => {
      const res = await apiPost<PurchaseRow[]>('Purchase/Select', { PeriodFrom: applied.from, PeriodTo: applied.to });
      if (!res.ok) throw new Error(res.message || 'Could not load the purchase invoices.');
      return res.data ?? [];
    },
  });
  const rows = purchases.data ?? [];

  const lookups = useQuery({
    queryKey: ['purchase-lookups'],
    queryFn: async (): Promise<Lookups> => {
      const res = await apiPost<Lookups>('Purchase/Lookups', {});
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not load the providers and items.');
      return res.data;
    },
  });
  const lk = lookups.data;
  const items = lk?.Items ?? [];
  const activeItems = items.filter((i) => i.IsActive);

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['purchase-register'] });
    qc.invalidateQueries({ queryKey: ['purchase-lookups'] });
    qc.invalidateQueries({ queryKey: ['gst-month'] });
  };

  const totals = useMemo(() => rows.reduce(
    (a, r) => {
      a.taxable += r.TaxableTotal ?? 0;
      a.tax += (r.CGSTTotal ?? 0) + (r.SGSTTotal ?? 0) + (r.IGSTTotal ?? 0) + (r.CessTotal ?? 0);
      a.gross += r.GrandTotal ?? 0;
      if ((r.ItcStatus || '').toLowerCase() === 'claimed') a.confirmed += r.EligibleITC ?? 0;
      return a;
    }, { taxable: 0, tax: 0, gross: 0, confirmed: 0 }), [rows]);

  // ── Add / edit ───────────────────────────────────────────────────────────────────────────
  const [formOpen, setFormOpen] = useState(false);
  const [head, setHead] = useState<Head>(blankHead);
  const [lines, setLines] = useState<LineDraft[]>([blankLine()]);
  const [loadingEdit, setLoadingEdit] = useState(false);
  const setH = <K extends keyof Head>(k: K, v: Head[K]) => setHead((h) => ({ ...h, [k]: v }));
  const setLine = (i: number, patch: Partial<LineDraft>) =>
    setLines((ls) => ls.map((x, j) => (j === i ? { ...x, ...patch } : x)));

  // The supplier's state decides CGST + SGST against IGST. It is read from their GSTIN — the
  // first two digits ARE the state — and only asked for when the supplier has no GSTIN.
  const gstin = head.SupplierGSTIN.trim().toUpperCase();
  const supplierState = gstin.length >= 2 && /^\d{2}/.test(gstin) ? gstin.slice(0, 2) : head.SupplierStateCode.trim();
  const companyState = lk?.CompanyStateCode ?? '';
  const intra = !supplierState || !companyState || supplierState === companyState;

  const computed = lines.map((l) => calc(l, head.inclusive, intra, head.manualTax));
  const sum = computed.reduce(
    (a, c) => ({ taxable: a.taxable + c.taxable, cgst: a.cgst + c.cgst, sgst: a.sgst + c.sgst, igst: a.igst + c.igst, total: a.total + c.total }),
    { taxable: 0, cgst: 0, sgst: 0, igst: 0, total: 0 },
  );
  const linesTotal = r2(sum.total);
  const printed = n(head.billTotal);
  const roundOff = head.billTotal.trim() ? r2(printed - linesTotal) : 0;

  const openNew = () => { setHead(blankHead()); setLines([blankLine()]); setFormOpen(true); };

  const pickProvider = (id: string) => {
    const p = lk?.Providers.find((x) => String(x.ProviderID) === id);
    setHead((h) => ({
      ...h,
      ProviderID: id,
      // Fill the supplier from the provider's last bill — but never wipe something already typed
      // when the provider has no bill to learn from yet.
      SupplierName: p?.SupplierName || h.SupplierName,
      SupplierGSTIN: p?.SupplierGSTIN || h.SupplierGSTIN,
      SupplierStateCode: p?.SupplierStateCode || h.SupplierStateCode,
      WalletID: p?.WalletID ? String(p.WalletID) : h.WalletID,
    }));
  };

  const pickSupplierByName = (name: string) => {
    const s = lk?.Suppliers.find((x) => x.SupplierName.toLowerCase() === name.trim().toLowerCase());
    setHead((h) => ({
      ...h,
      SupplierName: name,
      SupplierGSTIN: s?.SupplierGSTIN || h.SupplierGSTIN,
      SupplierStateCode: s?.SupplierStateCode || h.SupplierStateCode,
    }));
  };

  const pickItem = (i: number, id: string) => {
    const it = items.find((x) => String(x.PurchaseItemMasterID) === id);
    if (!it) { setLine(i, { itemId: '' }); return; }
    setLine(i, {
      itemId: id,
      Description: it.Name,
      SACCode: it.SACCode,
      gst: String(it.GSTRatePercent),
      rate: it.DefaultRate ? String(it.DefaultRate) : lines[i].rate,
      rateType: it.RateType === 'UNIT' ? 'UNIT' : 'PERCENT',
      ITCEligible: it.ITCEligible,
      amountTyped: false,
    });
  };

  /** Turning "by hand" on starts from the worked-out figures, so only the paisa that differs is retyped. */
  const toggleManual = (on: boolean) => {
    if (on) {
      setLines((ls) => ls.map((l) => {
        const c = calc(l, head.inclusive, intra, false);
        return { ...l, taxable: str(c.taxable), cgst: str(c.cgst), sgst: str(c.sgst), igst: str(c.igst) };
      }));
    }
    setH('manualTax', on);
  };

  const openEdit = async (id: number) => {
    setLoadingEdit(true);
    try {
      const res = await apiPost<PurchaseDetail>('Purchase/GetById', { TaxInvoiceHeaderID: id });
      if (!res.ok || !res.data) throw new Error(res.message || 'The bill could not be opened.');
      const d = res.data;
      if (d.LockReason) { toast.error(d.LockReason); return; }
      const h = d.Header;
      const drafts: LineDraft[] = d.Lines.map((l) => {
        const hasRate = !!(l.UnitQuantity || l.UnitRate);
        const tax = (l.CGSTAmount || 0) + (l.SGSTAmount || 0) + (l.IGSTAmount || 0);
        const g = (l.CGSTRate || 0) + (l.SGSTRate || 0) + (l.IGSTRate || 0)
          || (l.TaxableValue ? Math.round((tax / l.TaxableValue) * 100) : 0);
        const item = items.find((x) => x.Name.toLowerCase() === (l.Description || '').toLowerCase());
        return {
          key: keySeed++,
          itemId: item ? String(item.PurchaseItemMasterID) : '',
          Description: l.Description, SACCode: l.SACCode,
          qty: hasRate ? String(l.UnitQuantity) : '', rate: hasRate ? String(l.UnitRate) : '',
          rateType: l.RateType === 'UNIT' ? 'UNIT' : 'PERCENT',
          amount: hasRate ? '' : String(l.TaxableValue), amountTyped: !hasRate,
          gst: String(g), ITCEligible: l.ITCEligible,
          taxable: String(l.TaxableValue), cgst: str(l.CGSTAmount), sgst: str(l.SGSTAmount), igst: str(l.IGSTAmount),
        };
      });
      setHead({
        id: h.TaxInvoiceHeaderID,
        ProviderID: h.ProviderID ? String(h.ProviderID) : '',
        SupplierName: h.PartyName, SupplierGSTIN: h.PartyGSTIN, SupplierStateCode: h.SupplierStateCode,
        DocumentNo: h.DocumentNo, DocumentDate: (h.DocumentDate || '').slice(0, 10),
        WalletID: h.WalletID ? String(h.WalletID) : '',
        WalletCreditDate: (h.WalletCreditDate || '').slice(0, 10), Remark: h.Remark || '',
        // A saved bill re-opens with its tax exactly as stored — the figures the supplier printed
        // must not move by a paisa just because the bill was opened. Untick to work them out again.
        inclusive: false, manualTax: true, billTotal: String(h.GrandTotal),
      });
      setLines(drafts.length ? drafts : [blankLine()]);
      setViewId(null);
      setFormOpen(true);
    } catch (e) {
      toast.error((e as Error).message);
    } finally {
      setLoadingEdit(false);
    }
  };

  const formErrors: string[] = [];
  if (!head.SupplierName.trim() && !gstin) formErrors.push('Choose a provider, or type the supplier\'s name.');
  if (gstin && gstin.length !== 15) formErrors.push('A GSTIN has 15 characters.');
  if (!head.DocumentNo.trim()) formErrors.push('Enter the invoice number printed on the supplier\'s bill.');
  if (!head.DocumentDate) formErrors.push('Enter the invoice date.');
  if (sum.taxable <= 0) formErrors.push('Add at least one line with an amount.');
  if (head.billTotal.trim() && Math.abs(roundOff) > 1)
    formErrors.push(`The lines add up to ${inr(linesTotal)} but the bill total is ${inr(printed)} — a difference of ${inr(Math.abs(roundOff))}. A round-off is under one rupee, so one of the lines is wrong.`);

  const save = useMutation({
    mutationFn: async (): Promise<CaptureResult> => {
      const payload = {
        TaxInvoiceHeaderID: head.id,
        SupplierUserID: 0,
        SupplierName: head.SupplierName.trim(),
        SupplierGSTIN: gstin,
        SupplierStateCode: supplierState,
        PlaceOfSupplyStateCode: '',
        CompanyGSTIN: '',
        DocumentNo: head.DocumentNo.trim(),
        DocumentDate: head.DocumentDate,
        WalletCreditDate: head.WalletCreditDate || null,
        ProviderID: Number(head.ProviderID || 0),
        WalletID: Number(head.WalletID || 0),
        RoundOff: roundOff,
        Remark: head.Remark.trim(),
        Lines: lines
          .map((l, i) => ({ l, c: computed[i] }))
          .filter(({ c }) => c.taxable > 0)
          .map(({ l, c }) => ({
            ServiceID: 0,
            Description: l.Description.trim(),
            SACCode: l.SACCode.trim(),
            Quantity: 1,
            TaxableValue: c.taxable,
            CGSTRate: c.cgst > 0 ? c.g / 2 : 0, CGSTAmount: c.cgst,
            SGSTRate: c.sgst > 0 ? c.g / 2 : 0, SGSTAmount: c.sgst,
            IGSTRate: c.igst > 0 ? c.g : 0, IGSTAmount: c.igst,
            CessAmount: 0,
            ITCEligible: l.ITCEligible,
            ReverseCharge: false,
            UnitQuantity: n(l.qty),
            UnitRate: n(l.rate),
            RateType: l.rateType,
          })),
      };
      const res = await apiPost<CaptureResult>(head.id ? 'Purchase/Update' : 'Purchase/Capture', payload);
      if (!res.ok || !res.data) throw new Error(res.message || 'The purchase invoice could not be saved.');
      return res.data;
    },
    onSuccess: (r) => {
      if (r.AlreadyExisted) toast.info(`${r.DocumentNo} was already entered — nothing was added twice.`);
      else toast.success(`${head.id ? 'Updated' : 'Saved'} ${r.DocumentNo} — ${inr(r.GrandTotal)}, GST credit ${inr(r.EligibleITC)}.`);
      if (r.Warning) toast.info(r.Warning);
      setFormOpen(false);
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── View / delete ────────────────────────────────────────────────────────────────────────
  const [viewId, setViewId] = useState<number | null>(null);
  const view = useQuery({
    queryKey: ['purchase-detail', viewId],
    enabled: viewId !== null,
    queryFn: async (): Promise<PurchaseDetail> => {
      const res = await apiPost<PurchaseDetail>('Purchase/GetById', { TaxInvoiceHeaderID: viewId });
      if (!res.ok || !res.data) throw new Error(res.message || 'The bill could not be opened.');
      return res.data;
    },
  });

  const [deleteRow, setDeleteRow] = useState<PurchaseRow | null>(null);
  const remove = useMutation({
    mutationFn: async (id: number) => {
      const res = await apiPost('Purchase/Delete', { TaxInvoiceHeaderID: id });
      if (!res.ok) throw new Error(res.message || 'The bill could not be removed.');
    },
    onSuccess: () => { toast.success('The purchase invoice was removed.'); setDeleteRow(null); setViewId(null); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── The item list ────────────────────────────────────────────────────────────────────────
  const [itemsOpen, setItemsOpen] = useState(false);
  const blankItem = { PurchaseItemMasterID: 0, Name: '', SACCode: '', GSTRatePercent: '18', DefaultRate: '', RateType: 'PERCENT', ITCEligible: true, IsActive: true };
  const [itemDraft, setItemDraft] = useState(blankItem);
  const saveItem = useMutation({
    mutationFn: async () => {
      const res = await apiPost('Purchase/SaveItem', {
        ...itemDraft, Name: itemDraft.Name.trim(), SACCode: itemDraft.SACCode.trim(),
        GSTRatePercent: n(itemDraft.GSTRatePercent), DefaultRate: n(itemDraft.DefaultRate),
      });
      if (!res.ok) throw new Error(res.message || 'The item could not be saved.');
    },
    onSuccess: () => { toast.success('Item saved.'); setItemDraft(blankItem); qc.invalidateQueries({ queryKey: ['purchase-lookups'] }); },
    onError: (e: Error) => toast.error(e.message),
  });
  const deleteItem = useMutation({
    mutationFn: async (id: number) => {
      const res = await apiPost('Purchase/DeleteItem', { PurchaseItemMasterID: id });
      if (!res.ok) throw new Error(res.message || 'The item could not be removed.');
    },
    onSuccess: () => { toast.success('Item removed.'); qc.invalidateQueries({ queryKey: ['purchase-lookups'] }); },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── E-invoice JSON import ────────────────────────────────────────────────────────────────
  const [importOpen, setImportOpen] = useState(false);
  const [json, setJson] = useState('');
  const importJson = useMutation({
    mutationFn: async (): Promise<CaptureResult> => {
      const res = await apiPost<CaptureResult>('Purchase/ImportEInvoice', {
        Json: json, CompanyGSTIN: '', ProviderID: 0, WalletID: 0, DefaultServiceID: 0,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The e-invoice file could not be read.');
      return res.data;
    },
    onSuccess: (r) => {
      if (r.AlreadyExisted) toast.info(`${r.DocumentNo} was already entered.`);
      else toast.success(`Added ${r.DocumentNo} from ${r.SupplierName || 'the supplier'} — ${inr(r.GrandTotal)}.`);
      setImportOpen(false); setJson(''); refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const download = () =>
    downloadCsv(
      rows.map((r) => ({
        Invoice: r.DocumentNo, Date: dateOnly(r.DocumentDate), Supplier: r.PartyName, GSTIN: r.PartyGSTIN,
        Provider: r.ProviderName, Items: r.Items, Taxable: r.TaxableTotal, CGST: r.CGSTTotal, SGST: r.SGSTTotal,
        IGST: r.IGSTTotal, RoundOff: r.RoundOff, Total: r.GrandTotal, Credit: r.EligibleITC,
        CreditStatus: creditBadge(r.ItcStatus, r.ItcBucket, r.EligibleITC).label,
      })),
      `purchase-invoices-${applied.from}-to-${applied.to}.csv`,
    );

  const vd = view.data;

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Bills from your suppliers. The GST on them is credit that reduces the GST you pay."
        icon={pickIcon(def.title)}
        actions={
          mayWrite && (
            <div className="flex flex-wrap gap-2">
              <Button variant="outline" onClick={() => setItemsOpen(true)}>
                <ListChecks className="mr-1.5 size-4" />
                Items &amp; rates
              </Button>
              <Button variant="outline" onClick={() => setImportOpen(true)}>
                <FileJson className="mr-1.5 size-4" />
                Import e-invoice
              </Button>
              <Button onClick={openNew}>
                <FilePlus2 className="mr-1.5 size-4" />
                Add purchase invoice
              </Button>
            </div>
          )
        }
      />

      <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
        <StatTile label="Bills" value={rows.length} icon={FilePlus2} tone="text-brand-600" loading={purchases.isFetching} />
        <StatTile label="Taxable value" value={inr(totals.taxable)} icon={Download} tone="text-slate-600" loading={purchases.isFetching} />
        <StatTile label="GST on bills" value={inr(totals.tax)} icon={Upload} tone="text-warning-600" loading={purchases.isFetching} />
        <StatTile label="Credit confirmed" value={inr(totals.confirmed)} icon={RefreshCw} tone="text-success-600" loading={purchases.isFetching} />
      </div>

      <Card>
        <CardBody className="flex flex-wrap items-end gap-3">
          <div className="w-40"><Input label="From" type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></div>
          <div className="w-40"><Input label="To" type="date" value={to} onChange={(e) => setTo(e.target.value)} /></div>
          <Button variant="outline" onClick={() => setApplied({ from, to })} loading={purchases.isFetching}>
            <RefreshCw className="mr-1.5 size-4" />
            Show
          </Button>
          <div className="flex-1" />
          <Button variant="ghost" onClick={download} disabled={rows.length === 0}>
            <Download className="mr-1.5 size-4" />
            CSV
          </Button>
        </CardBody>
      </Card>

      <Card>
        {purchases.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>
        ) : purchases.isError ? (
          <CardBody><EmptyState title="Could not load" description={(purchases.error as Error).message} /></CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title="No purchase invoices for these dates"
              description={mayWrite ? 'Press Add purchase invoice and enter the bill your supplier sent you.' : 'Nothing has been entered for these dates yet.'}
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[1040px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Invoice</th>
                  <th className="px-3 py-2.5 font-medium">Supplier</th>
                  <th className="px-3 py-2.5 font-medium">For</th>
                  <th className="px-3 py-2.5 text-right font-medium">Taxable</th>
                  <th className="px-3 py-2.5 text-right font-medium">GST</th>
                  <th className="px-3 py-2.5 text-right font-medium">Total</th>
                  <th className="px-3 py-2.5 font-medium">GST credit</th>
                  <th className="px-3 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => {
                  const tax = (r.CGSTTotal ?? 0) + (r.SGSTTotal ?? 0) + (r.IGSTTotal ?? 0) + (r.CessTotal ?? 0);
                  const badge = creditBadge(r.ItcStatus, r.ItcBucket, r.EligibleITC);
                  return (
                    <tr key={r.TaxInvoiceHeaderID} className="hover:bg-slate-50">
                      <td className="px-5 py-2.5">
                        <button type="button" onClick={() => setViewId(r.TaxInvoiceHeaderID)} className="font-mono text-sm font-semibold text-brand-700 hover:underline">
                          {r.DocumentNo || `#${r.TaxInvoiceHeaderID}`}
                        </button>
                        <div className="text-xs text-slate-400">{dateOnly(r.DocumentDate)}</div>
                      </td>
                      <td className="px-3 py-2.5">
                        <div className="max-w-[210px] truncate text-slate-700" title={r.PartyName}>{r.PartyName || '—'}</div>
                        <div className="font-mono text-xs text-slate-400">{r.PartyGSTIN || 'no GSTIN'}</div>
                      </td>
                      <td className="px-3 py-2.5">
                        <div className="max-w-[190px] truncate text-slate-600" title={r.Items}>{r.Items || '—'}</div>
                        {r.ProviderName && <div className="text-xs text-slate-400">{r.ProviderName}</div>}
                      </td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{inr(r.TaxableTotal)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">
                        {inr(tax)}
                        <div className="text-[11px] text-slate-400">{r.IGSTTotal > 0 ? 'IGST' : 'CGST + SGST'}</div>
                      </td>
                      <td className="px-3 py-2.5 text-right font-semibold tabular-nums text-slate-800">{inr(r.GrandTotal)}</td>
                      <td className="px-3 py-2.5">
                        <Badge tone={badge.tone}>{badge.label}</Badge>
                        {r.EligibleITC > 0 && <div className="mt-0.5 text-[11px] tabular-nums text-slate-400">{inr(r.EligibleITC)}</div>}
                      </td>
                      <td className="px-3 py-2.5">
                        <div className="flex items-center justify-end gap-1">
                          <IconButton label="View" onClick={() => setViewId(r.TaxInvoiceHeaderID)}><Eye className="size-4" /></IconButton>
                          {mayWrite && !r.Locked && (
                            <>
                              <IconButton label="Edit" onClick={() => openEdit(r.TaxInvoiceHeaderID)} disabled={loadingEdit}><Pencil className="size-4" /></IconButton>
                              <IconButton label="Delete" danger onClick={() => setDeleteRow(r)}><Trash2 className="size-4" /></IconButton>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {!mayWrite && (
        <p className="text-xs text-slate-400">
          Read-only. Adding and correcting purchase invoices is limited to Admin and Support.
        </p>
      )}

      {/* ── Add / edit ─────────────────────────────────────────────────────────────────── */}
      <Modal open={formOpen} onClose={() => setFormOpen(false)} title={head.id ? `Edit purchase invoice ${head.DocumentNo}` : 'Add a purchase invoice'} size="xl">
        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            <Select label="Provider" value={head.ProviderID} onChange={(e) => pickProvider(e.target.value)}>
              <option value="">— not a provider bill —</option>
              {(lk?.Providers ?? []).map((p) => (
                <option key={p.ProviderID} value={p.ProviderID}>{`${p.ProviderName}${p.IsActive ? '' : ' (inactive)'}`}</option>
              ))}
            </Select>
            <div>
              <Input
                label="Supplier name"
                list="purchase-suppliers"
                value={head.SupplierName}
                onChange={(e) => pickSupplierByName(e.target.value)}
                hint="Fills in from the provider's last bill"
              />
              <datalist id="purchase-suppliers">
                {(lk?.Suppliers ?? []).map((s) => <option key={s.SupplierGSTIN || s.SupplierName} value={s.SupplierName} />)}
              </datalist>
            </div>
            <Input
              label="Supplier GSTIN"
              value={head.SupplierGSTIN}
              onChange={(e) => setH('SupplierGSTIN', e.target.value.toUpperCase())}
              hint={supplierState
                ? intra ? `State ${supplierState} — same as yours: CGST + SGST` : `State ${supplierState} — another state: IGST`
                : '15 characters'}
            />
            {!gstin && (
              <Input label="Supplier state code" value={head.SupplierStateCode} onChange={(e) => setH('SupplierStateCode', e.target.value)} hint="Only for a supplier with no GSTIN — two digits, e.g. 27" />
            )}
            <Input label="Invoice number" value={head.DocumentNo} onChange={(e) => setH('DocumentNo', e.target.value)} hint="As printed on the supplier's bill" />
            <Input label="Invoice date" type="date" value={head.DocumentDate} onChange={(e) => setH('DocumentDate', e.target.value)} />
            <Select label="Wallet it was bought for" value={head.WalletID} onChange={(e) => setH('WalletID', e.target.value)}>
              <option value="">— none —</option>
              {(lk?.Wallets ?? []).map((w) => (
                <option key={w.WalletID} value={w.WalletID}>{`${w.Name}${w.IsActive ? '' : ' (inactive)'}`}</option>
              ))}
            </Select>
            <Input label="Wallet credited on" type="date" value={head.WalletCreditDate} onChange={(e) => setH('WalletCreditDate', e.target.value)} hint="Optional" />
            <Input label="Remark" value={head.Remark} onChange={(e) => setH('Remark', e.target.value)} />
          </div>

          <div className="flex flex-wrap items-center gap-x-6 gap-y-2 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm">
            <label className="flex items-center gap-2 text-slate-700">
              <input type="checkbox" className="size-4" checked={head.inclusive} disabled={head.manualTax} onChange={(e) => setH('inclusive', e.target.checked)} />
              The amounts already include GST
            </label>
            <label className="flex items-center gap-2 text-slate-700">
              <input type="checkbox" className="size-4" checked={head.manualTax} onChange={(e) => toggleManual(e.target.checked)} />
              Enter the tax by hand (to match the bill to the paisa)
            </label>
          </div>

          <div className="overflow-x-auto rounded-lg border border-slate-200">
            <table className="w-full min-w-[1080px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Description</th>
                  <th className="px-2 py-2 font-medium">SAC</th>
                  <th className="px-2 py-2 text-right font-medium">Quantity</th>
                  <th className="px-2 py-2 text-right font-medium">Rate</th>
                  <th className="px-2 py-2 text-right font-medium">Amount</th>
                  <th className="px-2 py-2 text-right font-medium">GST %</th>
                  <th className="px-2 py-2 text-right font-medium">Taxable</th>
                  <th className="px-2 py-2 text-right font-medium">{intra ? 'CGST' : 'IGST'}</th>
                  {intra && <th className="px-2 py-2 text-right font-medium">SGST</th>}
                  <th className="px-2 py-2 text-right font-medium">Total</th>
                  <th className="px-2 py-2 text-center font-medium" title="Whether the GST on this line can be taken as credit">Credit</th>
                  <th className="px-2 py-2" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {lines.map((l, i) => {
                  const c = computed[i];
                  const byRate = rated(l);
                  return (
                    <tr key={l.key} className="align-top">
                      <td className="min-w-[220px] px-3 py-1.5">
                        {/* A native select: the shared Select draws its menu absolutely, and inside
                            this horizontally scrolling table the menu is clipped out of sight. */}
                        <select
                          value={l.itemId}
                          onChange={(e) => pickItem(i, e.target.value)}
                          className="w-full rounded-lg border border-slate-200 bg-white px-2.5 py-2 text-sm text-slate-800 focus:border-brand-400 focus:outline-none"
                        >
                          <option value="">— choose an item —</option>
                          {activeItems.map((it) => <option key={it.PurchaseItemMasterID} value={it.PurchaseItemMasterID}>{it.Name}</option>)}
                        </select>
                        <Input className="mt-1" placeholder="Description on the bill" value={l.Description} onChange={(e) => setLine(i, { Description: e.target.value })} />
                      </td>
                      <td className="w-24 px-2 py-1.5"><Input value={l.SACCode} onChange={(e) => setLine(i, { SACCode: e.target.value })} /></td>
                      <td className="w-32 px-2 py-1.5"><Input value={l.qty} onChange={(e) => setLine(i, { qty: e.target.value, amountTyped: false })} className="text-right" placeholder="0" /></td>
                      <td className="w-36 px-2 py-1.5">
                        <Input value={l.rate} onChange={(e) => setLine(i, { rate: e.target.value, amountTyped: false })} className="text-right" placeholder="0" />
                        <select
                          value={l.rateType}
                          onChange={(e) => setLine(i, { rateType: e.target.value as RateType, amountTyped: false })}
                          className="mt-1 w-full rounded-md border border-slate-200 bg-white px-1.5 py-1 text-xs text-slate-600"
                        >
                          <option value="PERCENT">% of quantity</option>
                          <option value="UNIT">per unit</option>
                        </select>
                      </td>
                      <td className="w-32 px-2 py-1.5">
                        <Input
                          value={!l.amountTyped && byRate !== null ? String(byRate) : l.amount}
                          onChange={(e) => setLine(i, { amount: e.target.value, amountTyped: true })}
                          className="text-right"
                          placeholder="0.00"
                        />
                        {!l.amountTyped && byRate !== null && <div className="mt-0.5 text-right text-[11px] text-slate-400">qty × rate</div>}
                      </td>
                      <td className="w-20 px-2 py-1.5"><Input value={l.gst} onChange={(e) => setLine(i, { gst: e.target.value })} className="text-right" disabled={head.manualTax} /></td>
                      {head.manualTax ? (
                        <>
                          <td className="w-28 px-2 py-1.5"><Input value={l.taxable} onChange={(e) => setLine(i, { taxable: e.target.value })} className="text-right" /></td>
                          {intra ? (
                            <>
                              <td className="w-24 px-2 py-1.5"><Input value={l.cgst} onChange={(e) => setLine(i, { cgst: e.target.value })} className="text-right" /></td>
                              <td className="w-24 px-2 py-1.5"><Input value={l.sgst} onChange={(e) => setLine(i, { sgst: e.target.value })} className="text-right" /></td>
                            </>
                          ) : (
                            <td className="w-24 px-2 py-1.5"><Input value={l.igst} onChange={(e) => setLine(i, { igst: e.target.value })} className="text-right" /></td>
                          )}
                        </>
                      ) : (
                        <>
                          <td className="px-2 py-3 text-right tabular-nums text-slate-700">{inr(c.taxable)}</td>
                          <td className="px-2 py-3 text-right tabular-nums text-slate-600">{inr(intra ? c.cgst : c.igst)}</td>
                          {intra && <td className="px-2 py-3 text-right tabular-nums text-slate-600">{inr(c.sgst)}</td>}
                        </>
                      )}
                      <td className="px-2 py-3 text-right font-semibold tabular-nums text-slate-800">{inr(c.total)}</td>
                      <td className="px-2 py-3 text-center">
                        <input type="checkbox" checked={l.ITCEligible} onChange={(e) => setLine(i, { ITCEligible: e.target.checked })} className="size-4" />
                      </td>
                      <td className="px-2 py-3">
                        {lines.length > 1 && (
                          <button type="button" onClick={() => setLines((ls) => ls.filter((_, j) => j !== i))} className="text-slate-400 hover:text-danger-500" aria-label="Remove line">
                            <Trash2 className="size-4" />
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
              <tfoot>
                <tr className="border-t border-slate-200 bg-slate-50 text-sm">
                  <td className="px-3 py-2" colSpan={6}>
                    <button type="button" onClick={() => setLines((ls) => [...ls, blankLine()])} className="inline-flex items-center gap-1 text-brand-600 hover:underline">
                      <Plus className="size-3.5" /> Add another line
                    </button>
                  </td>
                  <td className="px-2 py-2 text-right font-semibold tabular-nums text-slate-800">{inr(sum.taxable)}</td>
                  <td className="px-2 py-2 text-right font-semibold tabular-nums text-slate-800">{inr(intra ? sum.cgst : sum.igst)}</td>
                  {intra && <td className="px-2 py-2 text-right font-semibold tabular-nums text-slate-800">{inr(sum.sgst)}</td>}
                  <td className="px-2 py-2 text-right font-semibold tabular-nums text-slate-800">{inr(linesTotal)}</td>
                  <td colSpan={2} />
                </tr>
              </tfoot>
            </table>
          </div>

          <p className="text-xs text-slate-500">
            <strong>Amount = quantity × rate.</strong> Choose “% of quantity” when the supplier bills a percentage
            (for example 0.83% on ₹10,00,000 of top-up = ₹8,300), or “per unit” when the rate is for each one
            (for example 2,000 verifications at ₹0.50 = ₹1,000). If the bill only shows an amount, type it straight into Amount.
          </p>

          <div className="flex flex-wrap items-end justify-between gap-4 rounded-lg border border-slate-200 p-3">
            <div className="w-56">
              <Input
                label="Total printed on the bill"
                value={head.billTotal}
                onChange={(e) => setH('billTotal', e.target.value)}
                className="text-right"
                hint="Optional — checks your entry against the bill"
              />
            </div>
            <dl className="grid grid-cols-2 gap-x-8 gap-y-1 text-sm">
              <dt className="text-slate-500">Lines total</dt><dd className="text-right tabular-nums text-slate-800">{inr(linesTotal)}</dd>
              <dt className="text-slate-500">Round off</dt><dd className="text-right tabular-nums text-slate-800">{inr(roundOff)}</dd>
              <dt className="font-semibold text-slate-700">Invoice total</dt><dd className="text-right text-base font-semibold tabular-nums text-slate-900">{inr(r2(linesTotal + roundOff))}</dd>
              <dt className="text-slate-500">GST credit on this bill</dt>
              <dd className="text-right tabular-nums text-success-700">
                {inr(lines.reduce((a, l, i) => a + (l.ITCEligible ? computed[i].cgst + computed[i].sgst + computed[i].igst : 0), 0))}
              </dd>
            </dl>
          </div>

          {formErrors.length > 0 && (
            <ul className="space-y-1 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
              {formErrors.map((e) => <li key={e} className="flex gap-2"><AlertTriangle className="mt-0.5 size-4 shrink-0" />{e}</li>)}
            </ul>
          )}

          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button onClick={() => save.mutate()} loading={save.isPending} disabled={formErrors.length > 0}>
              {head.id ? 'Save changes' : 'Save purchase invoice'}
            </Button>
          </div>
        </div>
      </Modal>

      {/* ── View ───────────────────────────────────────────────────────────────────────── */}
      <Modal open={viewId !== null} onClose={() => setViewId(null)} title="Purchase invoice" size="xl">
        {view.isFetching ? (
          <div className="flex min-h-[20vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>
        ) : view.isError ? (
          <EmptyState title="Could not load" description={(view.error as Error).message} />
        ) : vd ? (
          <div className="space-y-4">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <div className="font-mono text-lg font-semibold text-slate-900">{vd.Header.DocumentNo}</div>
                <p className="text-sm text-slate-500">{dateOnly(vd.Header.DocumentDate)} · {vd.Header.PartyName}</p>
                <p className="font-mono text-xs text-slate-400">{vd.Header.PartyGSTIN || 'no GSTIN'}</p>
              </div>
              <div className="text-right">
                <p className="text-xs uppercase tracking-wide text-slate-400">Invoice total</p>
                <p className="text-xl font-semibold tabular-nums text-slate-900">{inr(vd.Header.GrandTotal)}</p>
                <Badge tone={creditBadge(vd.ItcStatus, vd.ItcBucket, vd.EligibleITC).tone} className="mt-1">
                  {creditBadge(vd.ItcStatus, vd.ItcBucket, vd.EligibleITC).label} · {inr(vd.EligibleITC)}
                </Badge>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3 text-sm sm:grid-cols-4">
              <Fact label="Provider" value={vd.ProviderName || '—'} />
              <Fact label="Wallet" value={vd.WalletName || '—'} />
              <Fact label="Supply" value={vd.Header.IGSTTotal > 0 ? 'Another state — IGST' : 'Same state — CGST + SGST'} />
              <Fact label="Return month" value={vd.Header.PeriodKey || '—'} />
            </div>

            <div className="overflow-x-auto rounded-lg border border-slate-200">
              <table className="w-full min-w-[760px] text-sm">
                <thead>
                  <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                    <th className="px-3 py-2 font-medium">Description</th>
                    <th className="px-3 py-2 font-medium">SAC</th>
                    <th className="px-3 py-2 text-right font-medium">Quantity × rate</th>
                    <th className="px-3 py-2 text-right font-medium">Taxable</th>
                    <th className="px-3 py-2 text-right font-medium">CGST</th>
                    <th className="px-3 py-2 text-right font-medium">SGST</th>
                    <th className="px-3 py-2 text-right font-medium">IGST</th>
                    <th className="px-3 py-2 text-right font-medium">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {vd.Lines.map((l) => (
                    <tr key={l.TaxInvoiceLineID}>
                      <td className="px-3 py-2 text-slate-700">{l.Description || '—'}{!l.ITCEligible && <span className="ml-1 text-xs text-slate-400">(no credit)</span>}</td>
                      <td className="px-3 py-2 font-mono text-xs text-slate-500">{l.SACCode || '—'}</td>
                      <td className="px-3 py-2 text-right tabular-nums text-slate-500">
                        {l.UnitQuantity || l.UnitRate
                          ? `${l.UnitQuantity.toLocaleString('en-IN')} × ${l.UnitRate}${l.RateType === 'UNIT' ? '' : '%'}`
                          : '—'}
                      </td>
                      <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(l.TaxableValue)}</td>
                      <td className="px-3 py-2 text-right tabular-nums text-slate-600">{l.CGSTAmount ? inr(l.CGSTAmount) : '—'}</td>
                      <td className="px-3 py-2 text-right tabular-nums text-slate-600">{l.SGSTAmount ? inr(l.SGSTAmount) : '—'}</td>
                      <td className="px-3 py-2 text-right tabular-nums text-slate-600">{l.IGSTAmount ? inr(l.IGSTAmount) : '—'}</td>
                      <td className="px-3 py-2 text-right font-semibold tabular-nums text-slate-800">{inr(l.LineTotal)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="border-t border-slate-200 bg-slate-50 font-semibold text-slate-700">
                    <td className="px-3 py-2" colSpan={3}>
                      Total{vd.Header.RoundOff ? ` (round off ${inr(vd.Header.RoundOff)})` : ''}
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums">{inr(vd.Header.TaxableTotal)}</td>
                    <td className="px-3 py-2 text-right tabular-nums">{inr(vd.Header.CGSTTotal)}</td>
                    <td className="px-3 py-2 text-right tabular-nums">{inr(vd.Header.SGSTTotal)}</td>
                    <td className="px-3 py-2 text-right tabular-nums">{inr(vd.Header.IGSTTotal)}</td>
                    <td className="px-3 py-2 text-right tabular-nums">{inr(vd.Header.GrandTotal)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>

            {vd.Header.Remark && <p className="text-sm text-slate-500">Remark: {vd.Header.Remark}</p>}
            {vd.LockReason && (
              <p className="flex gap-2 rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
                <AlertTriangle className="mt-0.5 size-4 shrink-0 text-slate-400" />{vd.LockReason}
              </p>
            )}

            <div className="flex justify-end gap-2">
              <Button variant="ghost" onClick={() => setViewId(null)}>Close</Button>
              {mayWrite && !vd.LockReason && (
                <>
                  <Button variant="outline" onClick={() => setDeleteRow(vd.Header)}>
                    <Trash2 className="mr-1.5 size-4" />Delete
                  </Button>
                  <Button onClick={() => openEdit(vd.Header.TaxInvoiceHeaderID)} loading={loadingEdit}>
                    <Pencil className="mr-1.5 size-4" />Edit
                  </Button>
                </>
              )}
            </div>
          </div>
        ) : null}
      </Modal>

      {/* ── Delete ─────────────────────────────────────────────────────────────────────── */}
      <Modal open={deleteRow !== null} onClose={() => setDeleteRow(null)} title="Delete this purchase invoice?" size="sm">
        <div className="space-y-3">
          <p className="text-sm text-slate-600">
            <strong>{deleteRow?.DocumentNo}</strong> from {deleteRow?.PartyName || 'the supplier'} ({inr(deleteRow?.GrandTotal ?? 0)}) will be
            removed, along with the GST credit on it. You can enter it again later if this is a mistake.
          </p>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setDeleteRow(null)}>Keep it</Button>
            <Button variant="danger" onClick={() => deleteRow && remove.mutate(deleteRow.TaxInvoiceHeaderID)} loading={remove.isPending}>Delete</Button>
          </div>
        </div>
      </Modal>

      {/* ── Items & rates ──────────────────────────────────────────────────────────────── */}
      <Modal open={itemsOpen} onClose={() => setItemsOpen(false)} title="Items & rates" size="lg">
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            The things your suppliers bill you for. Each one carries its own SAC code, GST % and usual rate, so
            choosing it on a bill fills the line in. Change them here whenever a supplier changes a rate.
          </p>
          <div className="overflow-x-auto rounded-lg border border-slate-200">
            <table className="w-full min-w-[640px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Item</th>
                  <th className="px-3 py-2 font-medium">SAC</th>
                  <th className="px-3 py-2 text-right font-medium">GST %</th>
                  <th className="px-3 py-2 text-right font-medium">Usual rate</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {items.length === 0 && <tr><td colSpan={6} className="px-3 py-4 text-center text-slate-400">No items yet — add the first one below.</td></tr>}
                {items.map((it) => (
                  <tr key={it.PurchaseItemMasterID}>
                    <td className="px-3 py-2 text-slate-800">{it.Name}</td>
                    <td className="px-3 py-2 font-mono text-xs text-slate-500">{it.SACCode || '—'}</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-600">{it.GSTRatePercent}%</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-600">
                      {it.DefaultRate ? `${it.DefaultRate}${it.RateType === 'UNIT' ? ' per unit' : '%'}` : 'typed on each bill'}
                    </td>
                    <td className="px-3 py-2"><Badge tone={it.IsActive ? 'success' : 'neutral'}>{it.IsActive ? 'In use' : 'Hidden'}</Badge></td>
                    <td className="px-3 py-2">
                      <div className="flex justify-end gap-1">
                        <IconButton label="Edit" onClick={() => setItemDraft({
                          PurchaseItemMasterID: it.PurchaseItemMasterID, Name: it.Name, SACCode: it.SACCode,
                          GSTRatePercent: String(it.GSTRatePercent), DefaultRate: it.DefaultRate ? String(it.DefaultRate) : '',
                          RateType: it.RateType === 'UNIT' ? 'UNIT' : 'PERCENT', ITCEligible: it.ITCEligible, IsActive: it.IsActive,
                        })}><Pencil className="size-4" /></IconButton>
                        <IconButton label="Delete" danger onClick={() => deleteItem.mutate(it.PurchaseItemMasterID)}><Trash2 className="size-4" /></IconButton>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="rounded-lg border border-slate-200 p-3">
            <p className="mb-2 text-sm font-semibold text-slate-700">{itemDraft.PurchaseItemMasterID ? 'Change item' : 'Add an item'}</p>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
              <div className="lg:col-span-2"><Input label="Name" value={itemDraft.Name} onChange={(e) => setItemDraft((d) => ({ ...d, Name: e.target.value }))} placeholder="e.g. E-Topup" /></div>
              <Input label="SAC code" value={itemDraft.SACCode} onChange={(e) => setItemDraft((d) => ({ ...d, SACCode: e.target.value }))} />
              <Input label="GST %" value={itemDraft.GSTRatePercent} onChange={(e) => setItemDraft((d) => ({ ...d, GSTRatePercent: e.target.value }))} className="text-right" />
              <Input label="Usual rate" value={itemDraft.DefaultRate} onChange={(e) => setItemDraft((d) => ({ ...d, DefaultRate: e.target.value }))} className="text-right" placeholder="optional" />
              <Select label="Rate is" value={itemDraft.RateType} onChange={(e) => setItemDraft((d) => ({ ...d, RateType: e.target.value }))}>
                <option value="PERCENT">% of quantity</option>
                <option value="UNIT">per unit</option>
              </Select>
              <label className="flex items-center gap-2 self-end pb-2 text-sm text-slate-700">
                <input type="checkbox" className="size-4" checked={itemDraft.ITCEligible} onChange={(e) => setItemDraft((d) => ({ ...d, ITCEligible: e.target.checked }))} />
                GST can be taken as credit
              </label>
              <label className="flex items-center gap-2 self-end pb-2 text-sm text-slate-700">
                <input type="checkbox" className="size-4" checked={itemDraft.IsActive} onChange={(e) => setItemDraft((d) => ({ ...d, IsActive: e.target.checked }))} />
                Show in the list
              </label>
            </div>
            <div className="mt-3 flex justify-end gap-2">
              {itemDraft.PurchaseItemMasterID > 0 && <Button variant="ghost" onClick={() => setItemDraft(blankItem)}>Cancel change</Button>}
              <Button onClick={() => saveItem.mutate()} loading={saveItem.isPending} disabled={!itemDraft.Name.trim()}>
                {itemDraft.PurchaseItemMasterID ? 'Save change' : 'Add item'}
              </Button>
            </div>
          </div>
        </div>
      </Modal>

      {/* ── E-invoice import ──────────────────────────────────────────────────────────── */}
      <Modal open={importOpen} onClose={() => setImportOpen(false)} title="Import a supplier e-invoice" size="lg">
        <div className="space-y-3">
          <p className="text-sm text-slate-500">
            If the supplier sent the e-invoice JSON file, pick it here. The supplier, the number, the lines and
            the tax are all read from the file — nothing is typed.
          </p>
          <input
            type="file"
            accept=".json,application/json"
            onChange={async (e) => {
              const file = e.target.files?.[0];
              if (!file) return;
              try { setJson(await file.text()); } catch { toast.error('That file could not be read.'); }
            }}
            className="block w-full rounded-lg border border-slate-200 p-2 text-sm text-slate-600 file:mr-3 file:rounded-md file:border-0 file:bg-brand-50 file:px-3 file:py-1.5 file:text-sm file:text-brand-700"
          />
          <textarea
            value={json}
            onChange={(e) => setJson(e.target.value)}
            rows={10}
            spellCheck={false}
            placeholder='{ "Version": "1.1", "SellerDtls": { ... }, "ItemList": [ ... ] }'
            className="w-full rounded-lg border border-slate-200 p-3 font-mono text-xs text-slate-700 focus:border-brand-400 focus:outline-none"
          />
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setImportOpen(false)}>Cancel</Button>
            <Button onClick={() => importJson.mutate()} loading={importJson.isPending} disabled={json.trim().length === 0}>
              <Upload className="mr-1.5 size-4" />
              Import
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

function IconButton({
  label, onClick, children, danger, disabled,
}: { label: string; onClick: () => void; children: React.ReactNode; danger?: boolean; disabled?: boolean }) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className={`rounded-md p-1.5 text-slate-500 transition hover:bg-slate-100 disabled:opacity-40 ${danger ? 'hover:text-danger-600' : 'hover:text-brand-600'}`}
    >
      {children}
    </button>
  );
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-200 p-2.5">
      <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-400">{label}</p>
      <p className="mt-0.5 text-slate-800">{value}</p>
    </div>
  );
}
