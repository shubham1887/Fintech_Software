import { useMemo, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, Building2, CheckCircle2, CopyX, FileSpreadsheet, Info, Plus, RefreshCw, Tags, Trash2, Upload,
} from 'lucide-react';
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

interface BankRow {
  BankAccountID: number; AccountName: string; BankName: string; AccountNumber: string; IFSC: string;
  LedgerAccountCode: string; IsActive: boolean;
  SubModeID: number; ModeID: number; ModeName: string; SubModeName: string; SubModeMissing: boolean;
  FormatCount: number; ImportCount: number; LastImportOn: string | null; Gap: string;
}
interface UnlinkedSubMode {
  SubModeID: number; ModeID: number; ModeName: string; SubModeName: string;
  BankName: string; AccountNumber: string; IFSC: string; Gap: string;
}
interface BankView { Banks: BankRow[]; UnlinkedSubModes: UnlinkedSubMode[]; Note: string }

interface FormatRow {
  BankStatementFormatID: number; BankAccountID: number; BankName: string; Name: string; Source: string;
  HeaderRow: number; SkipRows: number; DateFormat: string;
  TxnDateColumn: string; ValueDateColumn: string; DescriptionColumn: string; ReferenceColumn: string;
  DebitColumn: string; CreditColumn: string; AmountColumn: string; BalanceColumn: string;
  PdfLinePattern: string; PdfSkipPattern: string; PdfJoinWrappedLines: boolean;
  IsActive: boolean; Remark: string; LastUsedOn: string | null; Summary: string;
}

interface RuleRow {
  BankLineCategoryRuleID: number; BankAccountID: number; BankName: string;
  Direction: string; MatchType: string; Pattern: string;
  MinAmount: number; MaxAmount: number; Category: string; LedgerAccountCode: string;
  Priority: number; IsActive: boolean; Remark: string; MatchedLines: number; Summary: string;
}

interface PreviewRow {
  RowNo: number; TxnDate: string; Description: string; Reference: string;
  Debit: number; Credit: number; Balance: string; Raw: string; Read: boolean; Problem: string;
}
interface Preview {
  Source: string; TotalRowsSeen: number; RowsRead: number; RowsSkipped: number;
  DetectedColumns: string; Rows: PreviewRow[]; Warnings: string[]; Verdict: string;
}

interface DupClaim {
  FundTransactionID: number; DateTime: string; UserID: number; UserName: string; UserCode: string;
  Amount: number; Status: string; PayMode: string; BankName: string; Remark: string; Approved: boolean;
}
interface DupGroup {
  Reference: string; ClaimCount: number; ApprovedCount: number; ApprovedAmount: number;
  SingleAmount: number; Exposure: number; SeenOnStatement: boolean; BankCredited: number;
  Claims: DupClaim[]; Verdict: string;
}
interface DupReport {
  FromDate: string; ToDate: string; GroupCount: number; TotalExposure: number;
  Groups: DupGroup[]; Summary: string;
}

const CATEGORIES = [
  { value: 'PURCHASE', label: 'Purchase — stock/float bought', side: 'DEBIT' },
  { value: 'EXPENSE', label: 'Expense — rent, salary, bills', side: 'DEBIT' },
  { value: 'BANK_CHARGES', label: 'Bank charges', side: 'DEBIT' },
  { value: 'USER_FUND', label: "User fund — a retailer's deposit", side: 'CREDIT' },
  { value: 'SETTLEMENT', label: 'Settlement from a gateway', side: 'CREDIT' },
  { value: 'INTEREST', label: 'Interest credited', side: 'CREDIT' },
  { value: 'REFUND', label: 'Refund / reversal in', side: 'CREDIT' },
  { value: 'TRANSFER', label: 'Transfer between our own accounts', side: 'ANY' },
] as const;

const CATEGORY_LABEL: Record<string, string> = Object.fromEntries(
  CATEGORIES.map((c) => [c.value, c.label.split(' — ')[0]]),
);

/** The file as base64 for XLSX/PDF, as text for CSV — the shape the API expects. */
function readFile(file: File): Promise<{ content: string; source: string }> {
  const ext = file.name.split('.').pop()?.toUpperCase() ?? '';
  const source = ext === 'PDF' ? 'PDF' : ext === 'XLSX' || ext === 'XLS' ? 'XLSX' : 'CSV';
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(new Error('That file could not be read.'));
    reader.onload = () => {
      const result = String(reader.result ?? '');
      resolve({ content: source === 'CSV' ? result : result.split(',')[1] ?? '', source });
    };
    if (source === 'CSV') reader.readAsText(file);
    else reader.readAsDataURL(file);
  });
}

const emptyFormat = (bankId: number): FormatRow => ({
  BankStatementFormatID: 0, BankAccountID: bankId, BankName: '', Name: '', Source: 'XLSX',
  HeaderRow: -1, SkipRows: 0, DateFormat: '',
  TxnDateColumn: '', ValueDateColumn: '', DescriptionColumn: '', ReferenceColumn: '',
  DebitColumn: '', CreditColumn: '', AmountColumn: '', BalanceColumn: '',
  PdfLinePattern: '', PdfSkipPattern: '', PdfJoinWrappedLines: true,
  IsActive: true, Remark: '', LastUsedOn: null, Summary: '',
});

const emptyRule = (bankId: number): RuleRow => ({
  BankLineCategoryRuleID: 0, BankAccountID: bankId, BankName: '',
  Direction: 'ANY', MatchType: 'CONTAINS', Pattern: '',
  MinAmount: 0, MaxAmount: 0, Category: 'PURCHASE', LedgerAccountCode: '',
  Priority: 0, IsActive: true, Remark: '', MatchedLines: 0, Summary: '',
});

/**
 * Bank Setup — the configuration bank reconciliation needs before it can mean anything.
 *
 * Three tabs, and each one exists because the answer was previously in the wrong place:
 *
 *  Banks       the company's accounts are already maintained under Payment Modes, as the sub-modes
 *              a retailer deposits into. Sync brings them here instead of asking anyone to type an
 *              account number a second time, where the two copies can disagree.
 *  Formats     how each bank writes its statement, saved once. A CSV or Excel export usually needs
 *              nothing; a PDF always does, because a PDF has no columns — only text on a page.
 *              Preview runs the real importer against a real file so the pattern can be got right.
 *  Rules       what a line IS. Matching proves the money moved; only the business knows that this
 *              debit was stock and that credit was a retailer's deposit.
 *
 * Plus the one that costs money: fund requests claiming the same deposit twice.
 */
export function BankSetupScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const { user } = useAuth();
  const canConfigure = user?.UserTypeID === UserTypeCode.Admin || user?.UserTypeID === UserTypeCode.SupportUser;

  const [tab, setTab] = useState<'banks' | 'formats' | 'rules' | 'duplicates'>('banks');

  // ── Banks ─────────────────────────────────────────────────────────────────
  const banksQ = useQuery<BankView>({
    queryKey: ['bank-setup', 'banks'],
    queryFn: async () => {
      const res = await apiPost<BankView>('BankSetup/Banks', {});
      if (!res.ok) throw new Error(res.message || 'Could not load the banks.');
      return res.data as BankView;
    },
  });

  const syncMut = useMutation({
    mutationFn: async () => {
      const res = await apiPost<{ Created: number; Updated: number; Summary: string }>('BankSetup/SyncBanks', {});
      if (!res.ok) throw new Error(res.message || 'Could not bring the banks in.');
      return res.data;
    },
    onSuccess: (r) => {
      toast.success(r?.Summary ?? 'Banks brought in.');
      qc.invalidateQueries({ queryKey: ['bank-setup'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const banks = banksQ.data?.Banks ?? [];

  // ── Formats ───────────────────────────────────────────────────────────────
  const formatsQ = useQuery<FormatRow[]>({
    queryKey: ['bank-setup', 'formats'],
    queryFn: async () => {
      const res = await apiPost<FormatRow[]>('BankSetup/Formats', { BankStatementFormatID: 0 });
      if (!res.ok) throw new Error(res.message || 'Could not load the formats.');
      return (res.data as FormatRow[]) ?? [];
    },
  });

  const [formatDraft, setFormatDraft] = useState<FormatRow | null>(null);
  const patchFormat = (p: Partial<FormatRow>) => setFormatDraft((d) => (d ? { ...d, ...p } : d));

  const saveFormatMut = useMutation({
    mutationFn: async (d: FormatRow) => {
      const res = await apiPost('BankSetup/SaveFormat', d);
      if (!res.ok) throw new Error(res.message || 'Could not save the format.');
    },
    onSuccess: () => {
      toast.success('Format saved.');
      setFormatDraft(null);
      qc.invalidateQueries({ queryKey: ['bank-setup'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteFormatMut = useMutation({
    mutationFn: async (id: number) => {
      const res = await apiPost('BankSetup/DeleteFormat', { BankStatementFormatID: id });
      if (!res.ok) throw new Error(res.message || 'Could not remove the format.');
    },
    onSuccess: () => { toast.success('Format removed.'); qc.invalidateQueries({ queryKey: ['bank-setup'] }); },
    onError: (e: Error) => toast.error(e.message),
  });

  // Preview — the real importer, in dry run, against a real file.
  const fileRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<Preview | null>(null);

  const previewMut = useMutation({
    mutationFn: async (file: File) => {
      const { content, source } = await readFile(file);
      const res = await apiPost<Preview>('BankSetup/PreviewFormat', {
        BankStatementFormatID: formatDraft?.BankStatementFormatID ?? 0,
        Format: formatDraft ? { ...formatDraft, Source: formatDraft.Source || source } : null,
        Content: content,
        FileName: file.name,
        MaxRows: 15,
      });
      if (!res.ok) throw new Error(res.message || 'The file could not be read.');
      return res.data as Preview;
    },
    onSuccess: (p) => setPreview(p),
    onError: (e: Error) => { setPreview(null); toast.error(e.message); },
  });

  // ── Rules ─────────────────────────────────────────────────────────────────
  const rulesQ = useQuery<RuleRow[]>({
    queryKey: ['bank-setup', 'rules'],
    queryFn: async () => {
      const res = await apiPost<RuleRow[]>('BankSetup/Rules', { BankLineCategoryRuleID: 0 });
      if (!res.ok) throw new Error(res.message || 'Could not load the rules.');
      return (res.data as RuleRow[]) ?? [];
    },
  });

  const [ruleDraft, setRuleDraft] = useState<RuleRow | null>(null);
  const patchRule = (p: Partial<RuleRow>) => setRuleDraft((d) => (d ? { ...d, ...p } : d));

  const saveRuleMut = useMutation({
    mutationFn: async (d: RuleRow) => {
      const res = await apiPost('BankSetup/SaveRule', d);
      if (!res.ok) throw new Error(res.message || 'Could not save the rule.');
    },
    onSuccess: () => {
      toast.success('Rule saved.');
      setRuleDraft(null);
      qc.invalidateQueries({ queryKey: ['bank-setup'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteRuleMut = useMutation({
    mutationFn: async (id: number) => {
      const res = await apiPost('BankSetup/DeleteRule', { BankLineCategoryRuleID: id });
      if (!res.ok) throw new Error(res.message || 'Could not remove the rule.');
    },
    onSuccess: () => { toast.success('Rule removed.'); qc.invalidateQueries({ queryKey: ['bank-setup'] }); },
    onError: (e: Error) => toast.error(e.message),
  });

  const reclassifyMut = useMutation({
    mutationFn: async () => {
      const res = await apiPost<{ Summary: string }>('BankSetup/Reclassify', { BankAccountID: 0, BankStatementImportID: 0 });
      if (!res.ok) throw new Error(res.message || 'Could not re-classify.');
      return res.data;
    },
    onSuccess: (r) => toast.success(r?.Summary ?? 'Re-classified.'),
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Duplicate fund requests ───────────────────────────────────────────────
  const [dupFrom, setDupFrom] = useState(() => {
    const d = new Date();
    return apiDate(new Date(d.getFullYear(), d.getMonth() - 2, 1));
  });
  const [dupTo, setDupTo] = useState(() => apiDate(new Date()));

  const dupQ = useQuery<DupReport>({
    queryKey: ['bank-setup', 'duplicates', dupFrom, dupTo],
    queryFn: async () => {
      const res = await apiPost<DupReport>('BankSetup/DuplicateFunds', { FromDate: dupFrom, ToDate: dupTo });
      if (!res.ok) throw new Error(res.message || 'Could not look for duplicates.');
      return res.data as DupReport;
    },
    enabled: tab === 'duplicates',
  });

  const bankOptions = useMemo(
    () => banks.map((b) => ({ value: String(b.BankAccountID), label: `${b.BankName} ${b.AccountNumber}`.trim() })),
    [banks],
  );

  const TABS = [
    { key: 'banks' as const, label: 'Banks', icon: Building2, count: banks.length },
    { key: 'formats' as const, label: 'Statement formats', icon: FileSpreadsheet, count: formatsQ.data?.length ?? 0 },
    { key: 'rules' as const, label: 'What a line is', icon: Tags, count: rulesQ.data?.length ?? 0 },
    { key: 'duplicates' as const, label: 'Duplicate deposits', icon: CopyX, count: dupQ.data?.GroupCount ?? 0 },
  ];

  return (
    <>
      <PageHeader
        title="Bank Setup"
        subtitle="The banks we reconcile, how each one writes its statement, and what a line on it means"
      />

      <div className="mb-4 flex flex-wrap gap-1 border-b border-slate-200">
        {TABS.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={cn(
              '-mb-px flex items-center gap-2 border-b-2 px-3 py-2 text-sm font-medium transition-colors',
              tab === t.key
                ? 'border-brand-600 text-brand-700'
                : 'border-transparent text-slate-500 hover:text-slate-700',
            )}
          >
            <t.icon className="size-4" />
            {t.label}
            {t.count > 0 && <span className="rounded-full bg-slate-100 px-1.5 text-xs text-slate-600">{t.count}</span>}
          </button>
        ))}
      </div>

      {/* ── Banks ────────────────────────────────────────────────────────── */}
      {tab === 'banks' && (
        <>
          <Card className="mb-4">
            <CardBody className="flex flex-wrap items-center gap-3">
              <p className="min-w-0 flex-1 text-sm text-slate-600">{banksQ.data?.Note ?? 'Loading…'}</p>
              {canConfigure && (
                <Button loading={syncMut.isPending} onClick={() => syncMut.mutate()}>
                  <RefreshCw className="size-4" /> Sync from Payment Modes
                </Button>
              )}
            </CardBody>
          </Card>

          {banksQ.isFetching && banks.length === 0 ? (
            <div className="flex justify-center py-12"><Spinner /></div>
          ) : banks.length === 0 ? (
            <EmptyState
              icon={Building2}
              title="No bank set up yet"
              description="Your bank accounts are already under Settings → Payment Modes, as the sub-modes a retailer deposits into. Press Sync to bring them here — nothing is typed twice, and nothing is removed."
            />
          ) : (
            <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
              <div className="overflow-x-auto">
                <table className="w-full min-w-[52rem] border-collapse text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                      <th className="px-3 py-3">Bank</th>
                      <th className="px-3 py-3">Account</th>
                      <th className="px-3 py-3">From payment mode</th>
                      <th className="px-3 py-3 text-right">Formats</th>
                      <th className="px-3 py-3 text-right">Statements</th>
                      <th className="px-3 py-3">Ready?</th>
                    </tr>
                  </thead>
                  <tbody>
                    {banks.map((b) => (
                      <tr key={b.BankAccountID} className="border-b border-slate-100 last:border-0">
                        <td className="px-3 py-3">
                          <p className="font-medium text-slate-800">{b.BankName || '—'}</p>
                          <p className="text-xs text-slate-500">{b.AccountName}</p>
                        </td>
                        <td className="px-3 py-3">
                          {b.AccountNumber
                            ? <span className="font-mono text-xs text-slate-700">{b.AccountNumber}</span>
                            : <Badge tone="warning">not set</Badge>}
                          {b.IFSC && <p className="mt-0.5 font-mono text-xs text-slate-400">{b.IFSC}</p>}
                        </td>
                        <td className="px-3 py-3 text-xs">
                          {b.SubModeID > 0 ? (
                            b.SubModeMissing
                              ? <Badge tone="warning">sub-mode removed</Badge>
                              : <span className="text-slate-600">{b.ModeName} → {b.SubModeName}</span>
                          ) : (
                            <span className="text-slate-400">standalone</span>
                          )}
                        </td>
                        <td className="px-3 py-3 text-right tabular-nums">{b.FormatCount}</td>
                        <td className="px-3 py-3 text-right tabular-nums">
                          {b.ImportCount}
                          {b.LastImportOn && (
                            <p className="text-xs text-slate-400">{new Date(b.LastImportOn).toLocaleDateString('en-IN')}</p>
                          )}
                        </td>
                        <td className="px-3 py-3">
                          {b.Gap
                            ? <p className="flex items-start gap-1 text-xs text-warning-700"><AlertTriangle className="mt-0.5 size-3.5 shrink-0" />{b.Gap}</p>
                            : <span className="flex items-center gap-1 text-xs text-success-700"><CheckCircle2 className="size-3.5" />ready</span>}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {(banksQ.data?.UnlinkedSubModes.length ?? 0) > 0 && (
            <div className="mt-4 rounded-card border border-slate-200/70 bg-slate-50/60 p-4">
              <p className="mb-2 text-sm font-semibold text-slate-700">
                Payment sub-modes not brought in yet ({banksQ.data?.UnlinkedSubModes.length})
              </p>
              <ul className="space-y-1">
                {(banksQ.data?.UnlinkedSubModes ?? []).map((s) => (
                  <li key={s.SubModeID} className="text-xs text-slate-600">
                    <span className="font-medium text-slate-700">{s.ModeName} → {s.SubModeName}</span>
                    {s.AccountNumber ? ` · ${s.AccountNumber}` : ''}
                    {s.Gap && <span className="text-warning-700"> · {s.Gap}</span>}
                  </li>
                ))}
              </ul>
              <p className="mt-2 text-xs text-slate-400">
                Press Sync to add these. An account number already set up by hand is linked rather than duplicated.
              </p>
            </div>
          )}
        </>
      )}

      {/* ── Formats ──────────────────────────────────────────────────────── */}
      {tab === 'formats' && (
        <>
          <div className="mb-3 flex flex-wrap items-center gap-3">
            <p className="min-w-0 flex-1 text-sm text-slate-600">
              A CSV or Excel export usually needs no format — the importer knows the usual column names.
              A <span className="font-medium text-slate-700">PDF always needs one</span>: a PDF has no columns,
              only text on a page.
            </p>
            {canConfigure && banks.length > 0 && (
              <Button size="sm" onClick={() => { setFormatDraft(emptyFormat(banks[0].BankAccountID)); setPreview(null); }}>
                <Plus className="size-4" /> Add a format
              </Button>
            )}
          </div>

          {(formatsQ.data?.length ?? 0) === 0 ? (
            <EmptyState
              icon={FileSpreadsheet}
              title="No statement format yet"
              description="Add one for any bank whose statement does not import cleanly — and for every bank you upload a PDF for."
            />
          ) : (
            <div className="space-y-2">
              {(formatsQ.data ?? []).map((f) => (
                <div key={f.BankStatementFormatID} className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
                  <div className="flex flex-wrap items-start gap-3">
                    <div className="min-w-0 flex-1">
                      <p className="flex items-center gap-2 font-medium text-slate-800">
                        {f.Name}
                        <Badge tone={f.Source === 'PDF' ? 'info' : 'neutral'}>{f.Source}</Badge>
                        {!f.IsActive && <Badge tone="warning">inactive</Badge>}
                      </p>
                      <p className="text-xs text-slate-500">{f.BankName}</p>
                      <p className="mt-1 text-xs text-slate-600">{f.Summary}</p>
                      {f.Remark && <p className="mt-0.5 text-xs italic text-slate-400">{f.Remark}</p>}
                    </div>
                    {canConfigure && (
                      <div className="flex items-center gap-1">
                        <Button size="sm" variant="ghost" onClick={() => { setFormatDraft(f); setPreview(null); }}>Edit</Button>
                        <Button
                          size="sm" variant="ghost" className="text-danger-600 hover:bg-danger-50"
                          onClick={() => { if (window.confirm(`Remove the format "${f.Name}"?`)) deleteFormatMut.mutate(f.BankStatementFormatID); }}
                        >
                          <Trash2 className="size-4" />
                        </Button>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}

      {/* ── Rules ────────────────────────────────────────────────────────── */}
      {tab === 'rules' && (
        <>
          <div className="mb-3 flex flex-wrap items-center gap-3">
            <p className="min-w-0 flex-1 text-sm text-slate-600">
              Matching a line to a voucher proves the money moved. These say what it <em>was</em> — stock bought, a
              retailer&apos;s deposit, an expense — by reading the bank&apos;s own narration.
            </p>
            {canConfigure && (
              <>
                <Button size="sm" variant="outline" loading={reclassifyMut.isPending} onClick={() => reclassifyMut.mutate()}>
                  <RefreshCw className="size-4" /> Re-classify imported lines
                </Button>
                <Button size="sm" onClick={() => setRuleDraft(emptyRule(0))}>
                  <Plus className="size-4" /> Add a rule
                </Button>
              </>
            )}
          </div>

          {(rulesQ.data?.length ?? 0) === 0 ? (
            <EmptyState icon={Tags} title="No rule yet" description="Add one per phrase you recognise in your statements." />
          ) : (
            <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
              <div className="overflow-x-auto">
                <table className="w-full min-w-[48rem] border-collapse text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                      <th className="px-3 py-3">When the narration…</th>
                      <th className="px-3 py-3">Side</th>
                      <th className="px-3 py-3">…the line is</th>
                      <th className="px-3 py-3">Bank</th>
                      <th className="px-3 py-3 text-right">Used on</th>
                      <th className="px-3 py-3 text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(rulesQ.data ?? []).map((r) => (
                      <tr key={r.BankLineCategoryRuleID} className={cn('border-b border-slate-100 last:border-0', !r.IsActive && 'bg-slate-50/60')}>
                        <td className="px-3 py-3">
                          <p className="text-slate-800">
                            <span className="text-xs text-slate-500">
                              {r.MatchType === 'STARTS_WITH' ? 'starts with' : r.MatchType === 'REGEX' ? 'matches' : 'contains'}{' '}
                            </span>
                            <span className="font-mono text-xs">{r.Pattern}</span>
                          </p>
                          {(r.MinAmount > 0 || r.MaxAmount > 0) && (
                            <p className="text-xs text-slate-400">
                              {r.MinAmount > 0 && `above ${inr(r.MinAmount)}`}
                              {r.MinAmount > 0 && r.MaxAmount > 0 && ' · '}
                              {r.MaxAmount > 0 && `up to ${inr(r.MaxAmount)}`}
                            </p>
                          )}
                          {r.Remark && <p className="text-xs italic text-slate-400">{r.Remark}</p>}
                        </td>
                        <td className="px-3 py-3">
                          <Badge tone={r.Direction === 'DEBIT' ? 'danger' : r.Direction === 'CREDIT' ? 'success' : 'neutral'}>
                            {r.Direction === 'ANY' ? 'either' : r.Direction === 'DEBIT' ? 'money out' : 'money in'}
                          </Badge>
                        </td>
                        <td className="px-3 py-3 font-medium text-slate-800">
                          {CATEGORY_LABEL[r.Category] ?? r.Category}
                          {r.LedgerAccountCode && <p className="text-xs text-slate-400">account {r.LedgerAccountCode}</p>}
                        </td>
                        <td className="px-3 py-3 text-xs text-slate-600">{r.BankName}</td>
                        <td className="px-3 py-3 text-right tabular-nums text-slate-500">{r.MatchedLines}</td>
                        <td className="px-3 py-3">
                          <div className="flex items-center justify-end gap-1">
                            {canConfigure && (
                              <>
                                <Button size="sm" variant="ghost" onClick={() => setRuleDraft(r)}>Edit</Button>
                                <Button
                                  size="sm" variant="ghost" className="text-danger-600 hover:bg-danger-50"
                                  onClick={() => { if (window.confirm('Remove this rule?')) deleteRuleMut.mutate(r.BankLineCategoryRuleID); }}
                                >
                                  <Trash2 className="size-4" />
                                </Button>
                              </>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}

      {/* ── Duplicate deposits ───────────────────────────────────────────── */}
      {tab === 'duplicates' && (
        <>
          <Card className="mb-4">
            <CardBody className="flex flex-wrap items-end gap-3">
              <Input label="From" type="date" value={dupFrom} onChange={(e) => setDupFrom(e.target.value)} />
              <Input label="To" type="date" value={dupTo} onChange={(e) => setDupTo(e.target.value)} />
              <div className="flex-1" />
              {dupQ.data && (
                <div className="text-right">
                  <p className="text-xs text-slate-500">Funded beyond a single deposit</p>
                  <p className="font-display text-xl font-bold tabular-nums text-danger-600">
                    {inr(dupQ.data.TotalExposure)}
                  </p>
                </div>
              )}
            </CardBody>
          </Card>

          {dupQ.isFetching ? (
            <div className="flex justify-center py-12"><Spinner /></div>
          ) : (dupQ.data?.GroupCount ?? 0) === 0 ? (
            <EmptyState
              icon={CheckCircle2}
              title="No deposit claimed twice"
              description={dupQ.data?.Summary ?? 'Every approved fund request in this window has its own reference.'}
            />
          ) : (
            <div className="space-y-3">
              {(dupQ.data?.Groups ?? []).map((g) => (
                <div key={g.Reference} className="rounded-card border border-warning-200 bg-warning-50/40 p-4">
                  <div className="mb-2 flex flex-wrap items-center gap-3">
                    <span className="font-mono text-sm font-semibold text-slate-800">{g.Reference}</span>
                    <Badge tone="warning">{g.ApprovedCount} approved claims</Badge>
                    <span className="text-sm text-slate-600">
                      approved {inr(g.ApprovedAmount)} against a single deposit of {inr(g.SingleAmount)}
                    </span>
                    <span className="ml-auto font-semibold tabular-nums text-danger-600">{inr(g.Exposure)}</span>
                  </div>
                  <p className="mb-2 text-xs text-slate-600">{g.Verdict}</p>
                  <div className="overflow-x-auto rounded-lg border border-warning-200 bg-white">
                    <table className="w-full border-collapse text-sm">
                      <thead>
                        <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                          <th className="px-3 py-2">Request</th>
                          <th className="px-3 py-2">User</th>
                          <th className="px-3 py-2 text-right">Amount</th>
                          <th className="px-3 py-2">Status</th>
                          <th className="px-3 py-2">Mode</th>
                        </tr>
                      </thead>
                      <tbody>
                        {g.Claims.map((c) => (
                          <tr key={c.FundTransactionID} className="border-b border-slate-100 last:border-0">
                            <td className="whitespace-nowrap px-3 py-2">
                              <p className="font-medium text-slate-700">#{c.FundTransactionID}</p>
                              <p className="text-xs text-slate-400">{new Date(c.DateTime).toLocaleDateString('en-IN')}</p>
                            </td>
                            <td className="px-3 py-2">
                              <p className="text-slate-700">{c.UserName}</p>
                              {c.UserCode && <p className="text-xs text-slate-400">{c.UserCode}</p>}
                            </td>
                            <td className="whitespace-nowrap px-3 py-2 text-right tabular-nums">{inr(c.Amount)}</td>
                            <td className="px-3 py-2">
                              <Badge tone={c.Approved ? 'danger' : 'neutral'}>{c.Status || '—'}</Badge>
                            </td>
                            <td className="px-3 py-2 text-xs text-slate-500">
                              {c.PayMode}{c.BankName ? ` · ${c.BankName}` : ''}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              ))}
            </div>
          )}

          <p className="mt-3 flex items-start gap-1.5 text-xs text-slate-500">
            <Info className="mt-0.5 size-3.5 shrink-0 text-slate-400" />
            Only references with <span className="font-medium text-slate-600">more than one approved</span> request are
            shown — a retailer re-raising a rejected request is ordinary. Where a statement has been imported for these
            dates, what the bank actually credited is compared against what was approved.
          </p>
        </>
      )}

      {/* ── Format editor ────────────────────────────────────────────────── */}
      <Modal
        open={formatDraft !== null}
        onClose={() => { setFormatDraft(null); setPreview(null); }}
        title={formatDraft?.BankStatementFormatID ? 'Edit statement format' : 'Add statement format'}
        size="xl"
      >
        {formatDraft && (
          <div className="space-y-4">
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Select label="Bank" value={String(formatDraft.BankAccountID)} onChange={(e) => patchFormat({ BankAccountID: Number(e.target.value) })}>
                {bankOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </Select>
              <Select label="File type" value={formatDraft.Source} onChange={(e) => patchFormat({ Source: e.target.value })}>
                <option value="XLSX">Excel (.xlsx)</option>
                <option value="CSV">CSV</option>
                <option value="PDF">PDF</option>
              </Select>
              <Input label="Name" placeholder="e.g. HDFC monthly export" value={formatDraft.Name} onChange={(e) => patchFormat({ Name: e.target.value })} />
            </div>

            {formatDraft.Source === 'PDF' ? (
              <div className="space-y-3 rounded-card border border-info-200 bg-info-50/40 p-3">
                <p className="text-xs text-slate-600">
                  A PDF has no columns, so each line is split by a pattern. Leave the pattern blank to read the common
                  shape — a date, then narration, then the amounts at the end — and use Preview below to see whether
                  that worked before saving.
                </p>
                <Input
                  label="Line pattern (optional)"
                  placeholder="(?<date>\d{2}/\d{2}/\d{4})\s+(?<desc>.+?)\s+(?<debit>[\d,.]+)?\s+(?<credit>[\d,.]+)?\s+(?<balance>[\d,.]+)$"
                  value={formatDraft.PdfLinePattern}
                  onChange={(e) => patchFormat({ PdfLinePattern: e.target.value })}
                  hint="Named groups: date, valuedate, desc, ref, debit, credit, amount, balance"
                />
                <Input
                  label="Lines to ignore (optional)"
                  placeholder="Page \d+|Statement of account|Customer ID"
                  value={formatDraft.PdfSkipPattern}
                  onChange={(e) => patchFormat({ PdfSkipPattern: e.target.value })}
                  hint="Page headers and footers, so they are never read as transactions"
                />
                <label className="flex items-center gap-2 text-sm text-slate-700">
                  <input
                    type="checkbox"
                    checked={formatDraft.PdfJoinWrappedLines}
                    onChange={(e) => patchFormat({ PdfJoinWrappedLines: e.target.checked })}
                    className="size-4 rounded border-slate-300"
                  />
                  Join a narration that wrapped onto the next line
                </label>
              </div>
            ) : (
              <div className="space-y-3">
                <p className="text-xs text-slate-500">
                  Leave a column blank and the importer detects it, which is usually right. Fill one in only where your
                  bank names it something unusual — a header name, or a 0-based column number.
                </p>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                  <Input label="Date column" value={formatDraft.TxnDateColumn} onChange={(e) => patchFormat({ TxnDateColumn: e.target.value })} />
                  <Input label="Narration column" value={formatDraft.DescriptionColumn} onChange={(e) => patchFormat({ DescriptionColumn: e.target.value })} />
                  <Input label="Reference column" value={formatDraft.ReferenceColumn} onChange={(e) => patchFormat({ ReferenceColumn: e.target.value })} />
                  <Input label="Balance column" value={formatDraft.BalanceColumn} onChange={(e) => patchFormat({ BalanceColumn: e.target.value })} />
                  <Input label="Debit column" value={formatDraft.DebitColumn} onChange={(e) => patchFormat({ DebitColumn: e.target.value })} />
                  <Input label="Credit column" value={formatDraft.CreditColumn} onChange={(e) => patchFormat({ CreditColumn: e.target.value })} />
                  <Input label="Single amount column" value={formatDraft.AmountColumn} onChange={(e) => patchFormat({ AmountColumn: e.target.value })} hint="If the sign carries the direction" />
                  <Input label="Rows of preamble" type="number" value={String(formatDraft.SkipRows)} onChange={(e) => patchFormat({ SkipRows: Number(e.target.value) || 0 })} />
                </div>
              </div>
            )}

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <Input
                label="Date format (optional)" placeholder="dd/MM/yyyy"
                value={formatDraft.DateFormat} onChange={(e) => patchFormat({ DateFormat: e.target.value })}
                hint="Only where dates are ambiguous — 03/04 is April in India, March in a US export"
              />
              <Input label="Note (optional)" value={formatDraft.Remark} onChange={(e) => patchFormat({ Remark: e.target.value })} />
            </div>

            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="checkbox" checked={formatDraft.IsActive}
                onChange={(e) => patchFormat({ IsActive: e.target.checked })}
                className="size-4 rounded border-slate-300"
              />
              Active — used automatically when a file of this type is uploaded for this bank
            </label>

            {/* Try it against a real file. */}
            <div className="rounded-card border border-slate-200/70 bg-slate-50/60 p-3">
              <div className="flex flex-wrap items-center gap-3">
                <Button
                  size="sm" variant="outline"
                  loading={previewMut.isPending}
                  onClick={() => fileRef.current?.click()}
                >
                  <Upload className="size-4" /> Preview against a file
                </Button>
                <span className="text-xs text-slate-500">Nothing is imported — it only shows what this format reads.</span>
                <input
                  ref={fileRef} type="file" accept=".csv,.xlsx,.xls,.pdf" className="hidden"
                  onChange={(e) => { const f = e.target.files?.[0]; if (f) previewMut.mutate(f); e.target.value = ''; }}
                />
              </div>

              {preview && (
                <div className="mt-3">
                  <p className="text-sm font-medium text-slate-700">{preview.Verdict}</p>
                  {preview.DetectedColumns && (
                    <p className="mt-0.5 text-xs text-slate-500">Columns read as: {preview.DetectedColumns}</p>
                  )}
                  {preview.Warnings.map((w, i) => (
                    <p key={i} className="mt-1 flex items-start gap-1.5 text-xs text-warning-700">
                      <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />{w}
                    </p>
                  ))}
                  <div className="mt-2 max-h-72 overflow-auto rounded-lg border border-slate-200 bg-white">
                    <table className="w-full border-collapse text-xs">
                      <thead className="sticky top-0 bg-slate-50/95">
                        <tr className="border-b border-slate-200 text-left font-semibold uppercase tracking-wide text-slate-500">
                          <th className="px-2 py-1.5">Row</th>
                          <th className="px-2 py-1.5">Date</th>
                          <th className="px-2 py-1.5">Narration</th>
                          <th className="px-2 py-1.5">Ref</th>
                          <th className="px-2 py-1.5 text-right">Debit</th>
                          <th className="px-2 py-1.5 text-right">Credit</th>
                        </tr>
                      </thead>
                      <tbody>
                        {preview.Rows.map((r) => (
                          <tr key={r.RowNo} className={cn('border-b border-slate-100 last:border-0', !r.Read && 'bg-slate-50/60')}>
                            <td className="px-2 py-1.5 text-slate-400">{r.RowNo}</td>
                            {r.Read ? (
                              <>
                                <td className="whitespace-nowrap px-2 py-1.5">{r.TxnDate}</td>
                                <td className="px-2 py-1.5 text-slate-700">{r.Description}</td>
                                <td className="px-2 py-1.5 font-mono text-slate-500">{r.Reference}</td>
                                <td className="px-2 py-1.5 text-right tabular-nums">{r.Debit ? inr(r.Debit) : ''}</td>
                                <td className="px-2 py-1.5 text-right tabular-nums">{r.Credit ? inr(r.Credit) : ''}</td>
                              </>
                            ) : (
                              <td colSpan={5} className="px-2 py-1.5">
                                <p className="text-warning-700">{r.Problem}</p>
                                <p className="mt-0.5 font-mono text-slate-400">{r.Raw}</p>
                              </td>
                            )}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </div>

            <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
              <Button variant="secondary" onClick={() => { setFormatDraft(null); setPreview(null); }}>Cancel</Button>
              <Button
                loading={saveFormatMut.isPending}
                disabled={formatDraft.BankAccountID <= 0}
                onClick={() => saveFormatMut.mutate(formatDraft)}
              >
                Save format
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* ── Rule editor ──────────────────────────────────────────────────── */}
      <Modal
        open={ruleDraft !== null}
        onClose={() => setRuleDraft(null)}
        title={ruleDraft?.BankLineCategoryRuleID ? 'Edit rule' : 'Add rule'}
        size="lg"
      >
        {ruleDraft && (
          <div className="space-y-4">
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Select label="Match" value={ruleDraft.MatchType} onChange={(e) => patchRule({ MatchType: e.target.value })}>
                <option value="CONTAINS">Narration contains</option>
                <option value="STARTS_WITH">Narration starts with</option>
                <option value="REGEX">Narration matches (expression)</option>
              </Select>
              <div className="sm:col-span-2">
                <Input
                  label="Text to look for"
                  placeholder="e.g. JAIJUI, or NEFT DR-"
                  value={ruleDraft.Pattern}
                  onChange={(e) => patchRule({ Pattern: e.target.value })}
                  hint="Copy a phrase straight out of a statement you have"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Select label="The line is" value={ruleDraft.Category} onChange={(e) => patchRule({ Category: e.target.value })}>
                {CATEGORIES.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
              </Select>
              <Select label="Side" value={ruleDraft.Direction} onChange={(e) => patchRule({ Direction: e.target.value })}>
                <option value="ANY">Either</option>
                <option value="DEBIT">Money out</option>
                <option value="CREDIT">Money in</option>
              </Select>
              <Select label="Bank" value={String(ruleDraft.BankAccountID)} onChange={(e) => patchRule({ BankAccountID: Number(e.target.value) })}>
                <option value="0">Every bank</option>
                {bankOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </Select>
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Input label="Only above (₹)" type="number" value={String(ruleDraft.MinAmount || '')} onChange={(e) => patchRule({ MinAmount: Number(e.target.value) || 0 })} />
              <Input label="Only up to (₹)" type="number" value={String(ruleDraft.MaxAmount || '')} onChange={(e) => patchRule({ MaxAmount: Number(e.target.value) || 0 })} />
              <Input label="Ledger account (optional)" value={ruleDraft.LedgerAccountCode} onChange={(e) => patchRule({ LedgerAccountCode: e.target.value })} />
            </div>

            <Input label="Note (optional)" value={ruleDraft.Remark} onChange={(e) => patchRule({ Remark: e.target.value })} />

            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input type="checkbox" checked={ruleDraft.IsActive} onChange={(e) => patchRule({ IsActive: e.target.checked })} className="size-4 rounded border-slate-300" />
              Active
            </label>

            <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
              <Button variant="secondary" onClick={() => setRuleDraft(null)}>Cancel</Button>
              <Button
                loading={saveRuleMut.isPending}
                disabled={!ruleDraft.Pattern.trim()}
                onClick={() => saveRuleMut.mutate(ruleDraft)}
              >
                Save rule
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
