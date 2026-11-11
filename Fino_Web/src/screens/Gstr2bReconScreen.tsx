import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, CheckCircle2, Download, FileJson, Scale, ShieldAlert, Upload, Wallet,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { cn } from '@/lib/cn';
import {
  RECON_BUCKETS, bucketMeta, canOperate, periodKeyOf, periodLabel, recentPeriods, useCompanyGstin,
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
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface ReconRow {
  Bucket: string;
  SupplierGSTIN: string;
  SupplierName: string;
  DocumentNo: string;
  DocumentDate: string;
  TaxInvoiceHeaderID: number | null;
  BooksTaxable: number;
  BooksTax: number;
  Gstr2bLineID: number | null;
  Portal2BTaxable: number;
  Portal2BTax: number;
  TaxVariance: number;
  ItcAvailableIn2B: boolean;
}

interface BucketSummary {
  Bucket: string;
  Count: number;
  BooksTax: number;
  Portal2BTax: number;
}

interface ThreeWayRow {
  ProviderID: number;
  ProviderName: string;
  WalletTopUps: number;
  CapturedPurchaseGross: number;
  CapturedPurchaseTax: number;
  Matched2BTax: number;
  WalletVsBooks: number;
}

interface ReconResult {
  CompanyGSTIN: string;
  PeriodKey: string;
  Tolerance: number;
  Committed: boolean;
  No2BImported: boolean;
  Summary: BucketSummary[];
  Rows: ReconRow[];
  ThreeWay: ThreeWayRow[];
}

interface ImportResult {
  Gstr2bImportID: number;
  PeriodKey: string;
  LinesImported: number;
  TotalTaxable: number;
  TotalTax: number;
  SupersededPrevious: boolean;
  Warnings: string[];
}

interface SnapshotRow {
  Gstr2bImportID: number;
  PeriodKey: string;
  GeneratedDate: string | null;
  Source: string;
  LineCount: number;
  TotalTaxable: number;
  TotalTax: number;
  Status: string;
}

export function Gstr2bReconScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayWrite = canOperate(user?.UserTypeID);
  const { gstin, missing: gstinMissing, loading: gstinLoading } = useCompanyGstin();

  const periods = useMemo(() => recentPeriods(18), []);
  const [period, setPeriod] = useState(periodKeyOf());
  const [tolerance, setTolerance] = useState('0.05');
  const [bucket, setBucket] = useState<string>('');

  // The snapshots we hold, so "no 2B imported" is a statement about data rather than a guess.
  const snapshots = useQuery({
    queryKey: ['gstr2b-imports', gstin],
    enabled: gstin.length === 15,
    queryFn: async (): Promise<SnapshotRow[]> => {
      const res = await apiPost<SnapshotRow[]>('Gstr2b/Select', { CompanyGSTIN: gstin });
      if (!res.ok) throw new Error(res.message || 'Could not read the 2B snapshots.');
      return res.data ?? [];
    },
  });

  const recon = useQuery({
    queryKey: ['purchase-recon', gstin, period, tolerance],
    enabled: gstin.length === 15,
    queryFn: async (): Promise<ReconResult> => {
      const res = await apiPost<ReconResult>('PurchaseRecon/Preview', {
        CompanyGSTIN: gstin,
        PeriodKey: period,
        Tolerance: Number(tolerance) || 0,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The reconciliation could not be run.');
      return res.data;
    },
  });

  const result = recon.data;
  const rows = result?.Rows ?? [];
  const shown = bucket ? rows.filter((r) => r.Bucket?.toLowerCase() === bucket.toLowerCase()) : rows;

  const summaryFor = (key: string) =>
    result?.Summary.find((s) => s.Bucket?.toLowerCase() === key.toLowerCase());

  const commit = useMutation({
    mutationFn: async (): Promise<ReconResult> => {
      const res = await apiPost<ReconResult>('PurchaseRecon/Commit', {
        CompanyGSTIN: gstin,
        PeriodKey: period,
        Tolerance: Number(tolerance) || 0,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The match could not be committed.');
      return res.data;
    },
    onSuccess: (r) => {
      const matched = r.Summary.find((s) => s.Bucket?.toLowerCase() === 'matched')?.Count ?? 0;
      toast.success(`Committed ${periodLabel(r.PeriodKey)} — ${matched} matched document(s) written back to the 2B lines and the ITC register.`);
      qc.invalidateQueries({ queryKey: ['purchase-recon'] });
      qc.invalidateQueries({ queryKey: ['itc-register'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── 2B import ───────────────────────────────────────────────────────────────────────
  const [importOpen, setImportOpen] = useState(false);
  const [json, setJson] = useState('');

  const importJson = useMutation({
    mutationFn: async (): Promise<ImportResult> => {
      const res = await apiPost<ImportResult>('Gstr2b/Import', {
        Json: json,
        CompanyGSTIN: gstin,
        PeriodKey: period,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The 2B JSON could not be imported.');
      return res.data;
    },
    onSuccess: (r) => {
      toast.success(
        `Imported ${r.LinesImported} line(s) for ${periodLabel(r.PeriodKey)} — taxable ${inr(r.TotalTaxable)}, tax ${inr(r.TotalTax)}.`,
      );
      if (r.SupersededPrevious)
        toast.info('A previous snapshot for this period was superseded — a later 2B can drop a line a supplier amended, so it replaces rather than merges.');
      r.Warnings.forEach((w) => toast.info(w));
      setImportOpen(false);
      setJson('');
      qc.invalidateQueries({ queryKey: ['gstr2b-imports'] });
      qc.invalidateQueries({ queryKey: ['purchase-recon'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const onPickFile = async (file: File | undefined) => {
    if (!file) return;
    try {
      setJson(await file.text());
    } catch {
      toast.error('That file could not be read.');
    }
  };

  const download = () =>
    downloadCsv(
      shown.map((r) => ({
        Bucket: r.Bucket,
        Document: r.DocumentNo,
        Date: dateOnly(r.DocumentDate),
        Supplier: r.SupplierName,
        SupplierGSTIN: r.SupplierGSTIN,
        BooksTaxable: r.BooksTaxable,
        BooksTax: r.BooksTax,
        Portal2BTaxable: r.Portal2BTaxable,
        Portal2BTax: r.Portal2BTax,
        TaxVariance: r.TaxVariance,
        ItcAvailableIn2B: r.ItcAvailableIn2B ? 'Yes' : 'No',
      })),
      `gstr2b-recon-${period}${bucket ? `-${bucket}` : ''}.csv`,
    );

  if (gstinLoading) {
    return (
      <div className="flex min-h-[40vh] items-center justify-center">
        <Spinner className="size-6 text-brand-500" />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="What we were billed against what our suppliers filed — in four buckets, with the provider wallet as the third check"
        icon={pickIcon(def.title)}
        actions={
          mayWrite && !gstinMissing && (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setImportOpen(true)}>
                <FileJson className="mr-1.5 size-4" />
                Import GSTR-2B
              </Button>
              <Button onClick={() => commit.mutate()} loading={commit.isPending} disabled={!result || result.No2BImported}>
                <Scale className="mr-1.5 size-4" />
                Commit match
              </Button>
            </div>
          )
        }
      />

      {gstinMissing && (
        <div className="flex gap-2.5 rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-sm text-danger-600">
          <ShieldAlert className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>The company GSTIN is not set.</strong> GSTR-2B is filed per registration, so nothing on
            this screen can run until the GSTIN is filled in on <strong>Invoice Settings</strong>.
          </p>
        </div>
      )}

      {!gstinMissing && (
        <>
          <Card>
            <CardBody className="flex flex-wrap items-end gap-3">
              <div className="w-44">
                <Select label="Period" value={period} onChange={(e) => setPeriod(e.target.value)}>
                  {periods.map((p) => (
                    <option key={p} value={p}>{periodLabel(p)}</option>
                  ))}
                </Select>
              </div>
              <div className="w-36">
                <Input
                  label="Tax tolerance"
                  value={tolerance}
                  onChange={(e) => setTolerance(e.target.value)}
                  hint="Rupees per head"
                />
              </div>
              <div className="flex-1" />
              <Button variant="ghost" onClick={download} disabled={shown.length === 0}>
                <Download className="mr-1.5 size-4" />
                CSV
              </Button>
            </CardBody>
          </Card>

          {result?.No2BImported && (
            <div className="flex gap-2.5 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <p>
                <strong>No 2B snapshot for {periodLabel(period)}.</strong> Every captured bill therefore falls
                into "in books, not in 2B" — that is an <em>import the 2B first</em> signal, not a real gap.
              </p>
            </div>
          )}

          {/* The four buckets, as clickable filters. */}
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {RECON_BUCKETS.map((b) => {
              const s = summaryFor(b.key);
              const active = bucket.toLowerCase() === b.key.toLowerCase();
              return (
                <button
                  key={b.key}
                  type="button"
                  onClick={() => setBucket(active ? '' : b.key)}
                  className={cn(
                    'rounded-card border bg-white p-3 text-left transition',
                    active ? 'border-brand-500 ring-1 ring-brand-500/30' : 'border-slate-200 hover:border-slate-300',
                  )}
                  title={b.help}
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="text-xs font-medium uppercase tracking-wide text-slate-400">{b.label}</span>
                    <Badge tone={b.tone}>{s?.Count ?? 0}</Badge>
                  </div>
                  <p className="mt-1.5 text-sm tabular-nums text-slate-700">
                    books {inr(s?.BooksTax ?? 0)}
                  </p>
                  <p className="text-xs tabular-nums text-slate-400">2B {inr(s?.Portal2BTax ?? 0)}</p>
                </button>
              );
            })}
          </div>

          {/* Three-way: wallet paid vs bill captured vs what the supplier filed. */}
          {(result?.ThreeWay.length ?? 0) > 0 && (
            <Card>
              <CardBody className="pb-0">
                <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <Wallet className="size-4 text-slate-400" />
                  Three-way against the provider wallet
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  What we paid the provider, against the bill we captured, against what they filed. A gap means a
                  payment with no bill, or a bill with no payment.
                </p>
              </CardBody>
              <div className="overflow-x-auto p-3">
                <table className="w-full min-w-[680px] text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                      <th className="px-3 py-2 font-medium">Provider</th>
                      <th className="px-3 py-2 text-right font-medium">Wallet top-ups</th>
                      <th className="px-3 py-2 text-right font-medium">Bills captured</th>
                      <th className="px-3 py-2 text-right font-medium">Tax on bills</th>
                      <th className="px-3 py-2 text-right font-medium">Matched in 2B</th>
                      <th className="px-3 py-2 text-right font-medium">Wallet − books</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {result!.ThreeWay.map((t) => (
                      <tr key={t.ProviderID} className="hover:bg-slate-50">
                        <td className="px-3 py-2 text-slate-700">{t.ProviderName || `#${t.ProviderID}`}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(t.WalletTopUps)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(t.CapturedPurchaseGross)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(t.CapturedPurchaseTax)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(t.Matched2BTax)}</td>
                        <td className="px-3 py-2 text-right">
                          <Badge tone={Math.abs(t.WalletVsBooks) <= 1 ? 'success' : 'danger'}>
                            {inr(t.WalletVsBooks)}
                          </Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          )}

          <Card>
            {recon.isFetching ? (
              <div className="flex min-h-[30vh] items-center justify-center">
                <Spinner className="size-6 text-brand-500" />
              </div>
            ) : recon.isError ? (
              <CardBody>
                <EmptyState title="Could not reconcile" description={(recon.error as Error).message} />
              </CardBody>
            ) : shown.length === 0 ? (
              <CardBody>
                <EmptyState
                  title={bucket ? `Nothing in ${bucketMeta(bucket)?.label ?? bucket}` : 'Nothing to reconcile'}
                  description={
                    bucket
                      ? 'Clear the bucket filter to see the rest of the period.'
                      : 'Neither our books nor the 2B snapshot have a document in this period.'
                  }
                />
              </CardBody>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[980px] text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                      <th className="px-5 py-2.5 font-medium">Document</th>
                      <th className="px-3 py-2.5 font-medium">Supplier</th>
                      <th className="px-3 py-2.5 font-medium">Bucket</th>
                      <th className="px-3 py-2.5 text-right font-medium">Books taxable</th>
                      <th className="px-3 py-2.5 text-right font-medium">Books tax</th>
                      <th className="px-3 py-2.5 text-right font-medium">2B taxable</th>
                      <th className="px-3 py-2.5 text-right font-medium">2B tax</th>
                      <th className="px-3 py-2.5 text-right font-medium">Variance</th>
                      <th className="px-3 py-2.5 text-center font-medium">ITC in 2B</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {shown.map((r, i) => {
                      const meta = bucketMeta(r.Bucket);
                      return (
                        <tr key={`${r.DocumentNo}-${r.SupplierGSTIN}-${i}`} className="hover:bg-slate-50">
                          <td className="px-5 py-2.5">
                            <div className="font-mono text-sm font-semibold text-slate-800">{r.DocumentNo || '—'}</div>
                            <div className="text-xs text-slate-400">{dateOnly(r.DocumentDate)}</div>
                          </td>
                          <td className="px-3 py-2.5">
                            <div className="max-w-[180px] truncate text-slate-700" title={r.SupplierName}>{r.SupplierName || '—'}</div>
                            <div className="font-mono text-xs text-slate-400">{r.SupplierGSTIN || 'no GSTIN'}</div>
                          </td>
                          <td className="px-3 py-2.5">
                            <Badge tone={meta?.tone ?? 'neutral'}>{meta?.label ?? r.Bucket}</Badge>
                          </td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{r.TaxInvoiceHeaderID ? inr(r.BooksTaxable) : '—'}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{r.TaxInvoiceHeaderID ? inr(r.BooksTax) : '—'}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{r.Gstr2bLineID ? inr(r.Portal2BTaxable) : '—'}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{r.Gstr2bLineID ? inr(r.Portal2BTax) : '—'}</td>
                          <td className="px-3 py-2.5 text-right">
                            <span className={cn('tabular-nums', Math.abs(r.TaxVariance) > 0.05 ? 'font-semibold text-danger-600' : 'text-slate-400')}>
                              {inr(r.TaxVariance)}
                            </span>
                          </td>
                          <td className="px-3 py-2.5 text-center">
                            {r.Gstr2bLineID ? (
                              r.ItcAvailableIn2B ? <CheckCircle2 className="inline size-4 text-success-500" /> : <AlertTriangle className="inline size-4 text-warning-500" />
                            ) : (
                              <span className="text-slate-300">—</span>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </Card>

          {/* The snapshots we hold, newest first — the provenance of the right-hand column above. */}
          {(snapshots.data?.length ?? 0) > 0 && (
            <Card>
              <CardBody className="pb-2">
                <div className="text-sm font-semibold text-slate-700">2B snapshots held</div>
              </CardBody>
              <div className="overflow-x-auto px-3 pb-3">
                <table className="w-full min-w-[620px] text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                      <th className="px-3 py-2 font-medium">Period</th>
                      <th className="px-3 py-2 font-medium">Generated</th>
                      <th className="px-3 py-2 font-medium">Source</th>
                      <th className="px-3 py-2 text-right font-medium">Lines</th>
                      <th className="px-3 py-2 text-right font-medium">Taxable</th>
                      <th className="px-3 py-2 text-right font-medium">Tax</th>
                      <th className="px-3 py-2 font-medium">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {snapshots.data!.map((s) => (
                      <tr key={s.Gstr2bImportID} className="hover:bg-slate-50">
                        <td className="px-3 py-2 font-mono text-slate-700">{periodLabel(s.PeriodKey)}</td>
                        <td className="px-3 py-2 text-slate-600">{s.GeneratedDate ? dateOnly(s.GeneratedDate) : '—'}</td>
                        <td className="px-3 py-2 text-slate-500">{s.Source || '—'}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{s.LineCount}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(s.TotalTaxable)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(s.TotalTax)}</td>
                        <td className="px-3 py-2">
                          <Badge tone={s.Status?.toUpperCase() === 'ACTIVE' ? 'success' : 'neutral'}>{s.Status || '—'}</Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          )}
        </>
      )}

      {!mayWrite && !gstinMissing && (
        <p className="text-xs text-slate-400">
          Read-only. Importing a 2B snapshot and committing a match are limited to Admin and Support —
          a commit writes the match onto the 2B lines and the ITC register.
        </p>
      )}

      <Modal open={importOpen} onClose={() => setImportOpen(false)} title={`Import GSTR-2B for ${periodLabel(period)}`} size="lg">
        <div className="space-y-3">
          <p className="text-sm text-slate-500">
            Paste the GSTR-2B JSON downloaded from the portal, or pick the file. If the file carries its own
            return period, that one is used; otherwise the period selected on the screen is.
          </p>
          <input
            type="file"
            accept=".json,application/json"
            onChange={(e) => onPickFile(e.target.files?.[0])}
            className="block w-full rounded-lg border border-slate-200 p-2 text-sm text-slate-600 file:mr-3 file:rounded-md file:border-0 file:bg-brand-50 file:px-3 file:py-1.5 file:text-sm file:text-brand-700"
          />
          <textarea
            value={json}
            onChange={(e) => setJson(e.target.value)}
            rows={12}
            spellCheck={false}
            placeholder='{ "data": { "rtnprd": "102026", "docdata": { "b2b": [ ... ] } } }'
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
