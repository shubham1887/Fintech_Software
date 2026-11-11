import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, ArrowDown, ArrowUp, ArrowUpDown, Banknote, CheckCircle2, Download, Landmark, Link2, Link2Off,
  Scale, Tag, Upload, Wallet,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr, dateOnly, apiDate } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { cn } from '@/lib/cn';
import {
  BANK_BUCKETS, bankBucketMeta, bankMethodLabel, bankMethodTone, canOperate,
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
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface BankAccountRow {
  BankAccountID: number;
  AccountName: string;
  BankName: string;
  AccountNumber: string;
  IFSC: string;
  LedgerAccountCode: string;
  IsActive: boolean;
}

interface ImportRow {
  BankStatementImportID: number;
  BankAccountID: number;
  PeriodFrom: string;
  PeriodTo: string;
  FileName: string;
  Source: string;
  LineCount: number;
  TotalDebit: number;
  TotalCredit: number;
  ClosingBalance: number | null;
  Status: string;
  ImportedOn: string;
  UnmatchedLines: number;
}

interface ImportResult {
  BankStatementImportID: number;
  PeriodFrom: string;
  PeriodTo: string;
  LinesImported: number;
  TotalDebit: number;
  TotalCredit: number;
  ClosingBalance: number | null;
  SupersededPrevious: boolean;
  DetectedColumns: string;
  Warnings: string[];
}

interface ReconRow {
  Bucket: string;
  BankStatementLineID: number | null;
  BankDate: string | null;
  Description: string;
  ReferenceNo: string;
  BankDebit: number;
  BankCredit: number;
  Category: string;
  Counterparty: string;
  DuplicateReason: string;
  AcctVoucherLineID: number | null;
  AcctVoucherID: number | null;
  VoucherNo: string;
  VoucherDate: string | null;
  Narration: string;
  LedgerDebit: number;
  LedgerCredit: number;
  Variance: number;
  Method: string;
  DateGapDays: number;
  Resolution: string;
  ResolutionNote: string;
}

interface BucketSummary {
  Bucket: string;
  Count: number;
  BankAmount: number;
  LedgerAmount: number;
}

interface ReconResult {
  BankAccountID: number;
  BankAccountName: string;
  LedgerAccountCode: string;
  PeriodFrom: string;
  PeriodTo: string;
  Tolerance: number;
  DateWindowDays: number;
  Committed: boolean;
  NoStatementImported: boolean;
  BankClosingBalance: number;
  LedgerClosingBalance: number;
  Difference: number;
  Explained: boolean;
  Summary: BucketSummary[];
  Rows: ReconRow[];
}

/** What a statement line can be, by the side of the statement it sits on. */
const CATEGORIES = [
  { value: 'PURCHASE', label: 'Provider purchase — stock / float bought', side: 'DEBIT' },
  { value: 'EXPENSE', label: 'Expense — rent, salary, bills', side: 'DEBIT' },
  { value: 'BANK_CHARGES', label: 'Bank charges', side: 'DEBIT' },
  { value: 'USER_FUND', label: "User fund — a retailer's deposit", side: 'CREDIT' },
  { value: 'SETTLEMENT', label: 'Settlement from a gateway', side: 'CREDIT' },
  { value: 'INTEREST', label: 'Interest credited', side: 'CREDIT' },
  { value: 'REFUND', label: 'Refund / reversal in', side: 'CREDIT' },
  { value: 'TRANSFER', label: 'Transfer between our own accounts', side: 'ANY' },
] as const;
const categoryLabel = (c: string) => CATEGORIES.find((x) => x.value === c)?.label.split(' — ')[0] ?? c.replace(/_/g, ' ').toLowerCase();

/** First day of the previous whole month → today, which is the range a statement usually covers. */
function defaultRange(): { from: string; to: string } {
  const now = new Date();
  const first = new Date(now.getFullYear(), now.getMonth(), 1);
  return { from: apiDate(first), to: apiDate(now) };
}

type SortKey = 'date' | 'in' | 'out' | 'desc' | 'resolution';
type Resolution = '' | 'Pending' | 'Resolved';

const isResolved = (r: ReconRow) => r.Bucket === 'Matched' || r.Resolution === 'Resolved';

/**
 * Bank Reconciliation — the bank's own statement beside the ledger's Bank account, every line
 * explained by whatever record the operator has for it.
 *
 * The ledger side is thin on most installs (vouchers are only posted for what the accounting
 * rules cover), so the API also reads each unexplained bank line against the operational tables:
 * a credit against the approved fund requests, a debit against the Provider Purchase entries.
 * What that leaves is PENDING with a reason — "provider purchase entry not recorded yet", "needs a
 * category" — and this screen is where the operator clears it: pick the provider or the user,
 * categorise the line, or follow the link to record the purchase.
 */
export function BankReconScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayWrite = canOperate(user?.UserTypeID);

  const range = useMemo(defaultRange, []);
  const [accountId, setAccountId] = useState(0);
  const [from, setFrom] = useState(range.from);
  const [to, setTo] = useState(range.to);
  const [applied, setApplied] = useState({ from: range.from, to: range.to });
  const [bucket, setBucket] = useState('');
  const [search, setSearch] = useState('');
  const [resolution, setResolution] = useState<Resolution>('');
  const [sort, setSort] = useState<{ key: SortKey; dir: 'asc' | 'desc' }>({ key: 'date', dir: 'asc' });

  const accounts = useQuery({
    queryKey: ['bank-accounts'],
    queryFn: async (): Promise<BankAccountRow[]> => {
      const res = await apiPost<BankAccountRow[]>('BankAccount/Select', {});
      if (!res.ok) throw new Error(res.message || 'Could not load the bank accounts.');
      return res.data ?? [];
    },
  });

  // Pick the first account once they arrive, so the screen is useful without a click.
  useEffect(() => {
    if (accountId === 0 && (accounts.data?.length ?? 0) > 0) {
      setAccountId(accounts.data![0].BankAccountID);
    }
  }, [accounts.data, accountId]);

  const imports = useQuery({
    queryKey: ['bank-imports', accountId],
    enabled: accountId > 0,
    queryFn: async (): Promise<ImportRow[]> => {
      const res = await apiPost<ImportRow[]>('BankStatement/Select', { BankAccountID: accountId });
      if (!res.ok) throw new Error(res.message || 'Could not load the statement uploads.');
      return res.data ?? [];
    },
  });

  const recon = useQuery({
    queryKey: ['bank-recon', accountId, applied.from, applied.to],
    enabled: accountId > 0,
    queryFn: async (): Promise<ReconResult> => {
      const res = await apiPost<ReconResult>('BankRecon/Preview', {
        BankAccountID: accountId,
        PeriodFrom: applied.from,
        PeriodTo: applied.to,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The reconciliation could not be run.');
      return res.data;
    },
  });

  const result = recon.data;
  const rows = useMemo(() => result?.Rows ?? [], [result]);

  // Bucket card → search box → resolution → sort, in that order, so each narrows the last.
  const shown = useMemo(() => {
    const q = search.trim().toLowerCase();
    let list = bucket ? rows.filter((r) => r.Bucket?.toLowerCase() === bucket.toLowerCase()) : rows;
    if (resolution === 'Pending') list = list.filter((r) => !isResolved(r));
    if (resolution === 'Resolved') list = list.filter(isResolved);
    if (q) {
      list = list.filter((r) =>
        [r.Description, r.ReferenceNo, r.Counterparty, r.Category, r.VoucherNo, r.Narration, r.ResolutionNote,
          String(r.BankCredit || ''), String(r.BankDebit || ''), String(r.LedgerDebit || ''), String(r.LedgerCredit || '')]
          .some((v) => String(v ?? '').toLowerCase().includes(q)));
    }
    const dir = sort.dir === 'asc' ? 1 : -1;
    const dateOf = (r: ReconRow) => new Date(r.BankDate ?? r.VoucherDate ?? 0).getTime();
    return [...list].sort((a, b) => {
      switch (sort.key) {
        case 'in': return (a.BankCredit - b.BankCredit) * dir;
        case 'out': return (a.BankDebit - b.BankDebit) * dir;
        case 'desc': return (a.Description || a.Narration).localeCompare(b.Description || b.Narration) * dir;
        case 'resolution': return ((isResolved(a) ? 1 : 0) - (isResolved(b) ? 1 : 0)) * dir;
        default: return (dateOf(a) - dateOf(b)) * dir;
      }
    });
  }, [rows, bucket, search, resolution, sort]);

  const pendingCount = rows.filter((r) => !isResolved(r)).length;
  const summaryFor = (key: string) =>
    result?.Summary.find((s) => s.Bucket?.toLowerCase() === key.toLowerCase());

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ['bank-recon'] });
    qc.invalidateQueries({ queryKey: ['bank-imports'] });
  };

  const commit = useMutation({
    mutationFn: async (): Promise<ReconResult> => {
      const res = await apiPost<ReconResult>('BankRecon/Commit', {
        BankAccountID: accountId,
        PeriodFrom: applied.from,
        PeriodTo: applied.to,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The match could not be committed.');
      return res.data;
    },
    onSuccess: (r) => {
      const matched = r.Summary.find((s) => s.Bucket?.toLowerCase() === 'matched')?.Count ?? 0;
      toast.success(`Committed — ${matched} movement(s) are now reconciled and will not be offered again.`);
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const unmatch = useMutation({
    mutationFn: async (lineId: number) => {
      const res = await apiPost('BankRecon/Unmatch', { BankStatementLineID: lineId });
      if (!res.ok) throw new Error(res.message || 'The pairing could not be broken.');
    },
    onSuccess: () => {
      toast.success('Pairing broken — both sides are unmatched again.');
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Manual match ────────────────────────────────────────────────────────────────────
  const [matchFor, setMatchFor] = useState<ReconRow | null>(null);
  const [pickLedgerId, setPickLedgerId] = useState(0);

  const ledgerCandidates = useMemo(
    () => rows.filter((r) => r.Bucket === 'MissingInBank' && r.AcctVoucherLineID),
    [rows],
  );

  const manualMatch = useMutation({
    mutationFn: async () => {
      const res = await apiPost('BankRecon/Match', {
        BankStatementLineID: matchFor?.BankStatementLineID ?? 0,
        AcctVoucherLineID: pickLedgerId,
      });
      if (!res.ok) throw new Error(res.message || 'The two lines could not be paired.');
    },
    onSuccess: () => {
      toast.success('Paired.');
      setMatchFor(null);
      setPickLedgerId(0);
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Categorise (what the line IS, and who it is about) ─────────────────────────────
  const [catFor, setCatFor] = useState<ReconRow | null>(null);

  // ── Upload ──────────────────────────────────────────────────────────────────────────
  const [uploadOpen, setUploadOpen] = useState(false);
  const [csv, setCsv] = useState('');
  const [xlsxB64, setXlsxB64] = useState('');
  const [fileName, setFileName] = useState('');

  const resetUpload = () => { setCsv(''); setXlsxB64(''); setFileName(''); };

  const onPickFile = async (file: File | undefined) => {
    if (!file) return;
    setFileName(file.name);
    try {
      if (/\.xlsx?$/i.test(file.name)) {
        const buf = await file.arrayBuffer();
        // btoa on a large string blows the argument limit, so chunk it.
        const bytes = new Uint8Array(buf);
        let binary = '';
        for (let i = 0; i < bytes.length; i += 8192) {
          binary += String.fromCharCode(...bytes.subarray(i, i + 8192));
        }
        setXlsxB64(btoa(binary));
        setCsv('');
      } else {
        setCsv(await file.text());
        setXlsxB64('');
      }
    } catch {
      toast.error('That file could not be read.');
    }
  };

  const upload = useMutation({
    mutationFn: async (): Promise<ImportResult> => {
      const isXlsx = xlsxB64.length > 0;
      const res = await apiPost<ImportResult>('BankStatement/Import', {
        BankAccountID: accountId,
        Content: isXlsx ? xlsxB64 : csv,
        Source: isXlsx ? 'XLSX' : 'CSV',
        FileName: fileName,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The statement could not be imported.');
      return res.data;
    },
    onSuccess: (r) => {
      toast.success(
        `Imported ${r.LinesImported} line(s), ${dateOnly(r.PeriodFrom)} to ${dateOnly(r.PeriodTo)} — in ${inr(r.TotalCredit)}, out ${inr(r.TotalDebit)}.`,
      );
      if (r.DetectedColumns) toast.info(`Columns read as: ${r.DetectedColumns}`);
      if (r.SupersededPrevious)
        toast.info('A previous statement for the same range was superseded — a bank can restate a line, so it replaces rather than merges.');
      r.Warnings.slice(0, 4).forEach((w) => toast.info(w));
      setUploadOpen(false);
      resetUpload();
      // Look at what was just uploaded.
      setFrom(r.PeriodFrom.slice(0, 10));
      setTo(r.PeriodTo.slice(0, 10));
      setApplied({ from: r.PeriodFrom.slice(0, 10), to: r.PeriodTo.slice(0, 10) });
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const download = () =>
    downloadCsv(
      shown.map((r) => ({
        Date: r.BankDate ? dateOnly(r.BankDate) : (r.VoucherDate ? dateOnly(r.VoucherDate) : ''),
        Bucket: bankBucketMeta(r.Bucket)?.label ?? r.Bucket,
        Description: r.Description,
        Reference: r.ReferenceNo,
        BankIn: r.BankCredit,
        BankOut: r.BankDebit,
        Category: r.Category ? categoryLabel(r.Category) : '',
        ProviderOrUser: r.Counterparty,
        Resolution: r.Bucket === 'Matched' ? 'Matched' : r.Resolution,
        Note: r.ResolutionNote,
        ExplainedBy: r.VoucherNo,
        VoucherDate: r.VoucherDate ? dateOnly(r.VoucherDate) : '',
        Narration: r.Narration,
        LedgerDr: r.LedgerDebit,
        LedgerCr: r.LedgerCredit,
        Variance: r.Variance,
        DuplicateFlag: r.DuplicateReason,
        MatchedBy: bankMethodLabel(r.Method),
      })),
      `bank-recon-${applied.from}-to-${applied.to}${bucket ? `-${bucket}` : ''}.csv`,
    );

  const noAccounts = !accounts.isPending && (accounts.data?.length ?? 0) === 0;

  const toggleSort = (key: SortKey) =>
    setSort((s) => (s.key === key ? { key, dir: s.dir === 'asc' ? 'desc' : 'asc' } : { key, dir: 'asc' }));
  const SortIcon = ({ k }: { k: SortKey }) =>
    sort.key !== k ? <ArrowUpDown className="ml-1 inline size-3 text-slate-300" />
      : sort.dir === 'asc' ? <ArrowUp className="ml-1 inline size-3 text-brand-600" /> : <ArrowDown className="ml-1 inline size-3 text-brand-600" />;

  if (accounts.isPending) {
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
        subtitle="The bank's own statement beside the ledger's Bank account — every line explained, or named as pending"
        icon={pickIcon(def.title)}
        actions={
          mayWrite && !noAccounts && (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => setUploadOpen(true)}>
                <Upload className="mr-1.5 size-4" />
                Upload statement
              </Button>
              <Button
                onClick={() => commit.mutate()}
                loading={commit.isPending}
                disabled={!result || result.NoStatementImported}
              >
                <Scale className="mr-1.5 size-4" />
                Commit match
              </Button>
            </div>
          )
        }
      />

      {noAccounts && (
        <div className="flex gap-2.5 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>No bank account is set up.</strong> Add the bank on <Link to="/s/BankSetup" className="underline">Bank Setup</Link> (it is pulled from Payment Modes) and come back.
          </p>
        </div>
      )}

      {!noAccounts && (
        <>
          <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
            <StatTile label="Bank closing balance" value={inr(result?.BankClosingBalance ?? 0)} icon={Landmark} tone="text-brand-600" loading={recon.isFetching} />
            <StatTile label="Ledger Bank balance" value={inr(result?.LedgerClosingBalance ?? 0)} icon={Wallet} tone="text-slate-600" loading={recon.isFetching} />
            <StatTile
              label="Difference"
              value={inr(result?.Difference ?? 0)}
              icon={Scale}
              tone={Math.abs(result?.Difference ?? 0) < 0.05 ? 'text-success-600' : 'text-danger-600'}
              loading={recon.isFetching}
            />
            <StatTile
              label="Pending lines"
              value={pendingCount}
              icon={Banknote}
              tone={pendingCount === 0 ? 'text-success-600' : 'text-warning-600'}
              loading={recon.isFetching}
            />
          </div>

          <Card>
            <CardBody className="flex flex-wrap items-end gap-3">
              <div className="w-80">
                <Select
                  label="Bank account"
                  value={String(accountId)}
                  onChange={(e) => { setAccountId(Number(e.target.value)); setBucket(''); }}
                >
                  {/* One string child, not several: the Select reads an option's children to
                      build its own list, and an array of children comes back comma-joined.
                      The BANK's name leads — that is how the statement in hand is recognised. */}
                  {(accounts.data ?? []).map((a) => (
                    <option key={a.BankAccountID} value={a.BankAccountID}>
                      {`${a.BankName || 'Bank'} — ${a.AccountName}${a.AccountNumber ? ` (A/C …${a.AccountNumber.slice(-4)})` : ''}`}
                    </option>
                  ))}
                </Select>
              </div>
              <div className="w-40">
                <Input label="From" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
              </div>
              <div className="w-40">
                <Input label="To" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
              </div>
              <Button variant="outline" onClick={() => setApplied({ from, to })} loading={recon.isFetching}>
                Run
              </Button>
              <div className="flex-1" />
              <Button variant="ghost" onClick={download} disabled={shown.length === 0}>
                <Download className="mr-1.5 size-4" />
                Excel / CSV
              </Button>
            </CardBody>
          </Card>

          {result?.NoStatementImported && (
            <div className="flex gap-2.5 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <p>
                <strong>No statement covers this range.</strong> Every ledger movement therefore falls into
                "in books, not in bank" — that is an <em>upload the statement first</em> signal, not a real gap.
              </p>
            </div>
          )}

          {result && !result.NoStatementImported && (
            <div
              className={cn(
                'flex gap-2.5 rounded-lg border p-3 text-sm',
                pendingCount === 0
                  ? 'border-success-500/30 bg-success-50 text-success-700'
                  : 'border-warning-500/30 bg-warning-50 text-warning-700',
              )}
            >
              {pendingCount === 0 ? <CheckCircle2 className="mt-0.5 size-4 shrink-0" /> : <AlertTriangle className="mt-0.5 size-4 shrink-0" />}
              <p>
                {pendingCount === 0 ? (
                  <><strong>Every line is explained.</strong> Bank {inr(result.BankClosingBalance)} against ledger {inr(result.LedgerClosingBalance)}; each movement is matched, paired with a fund request or a provider purchase, or categorised.</>
                ) : (
                  <><strong>{pendingCount} line{pendingCount === 1 ? '' : 's'} still pending.</strong> Each one says what it needs — a provider purchase entry, a category, or the user it belongs to. Filter by <em>Pending</em> below to work through them.</>
                )}
              </p>
            </div>
          )}

          {/* The four buckets, as clickable filters. */}
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {BANK_BUCKETS.map((b) => {
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
                  <p className="mt-1.5 text-sm tabular-nums text-slate-700">bank {inr(s?.BankAmount ?? 0)}</p>
                  <p className="text-xs tabular-nums text-slate-400">ledger {inr(s?.LedgerAmount ?? 0)}</p>
                </button>
              );
            })}
          </div>

          <Card>
            {/* Table toolbar: search + resolution filter, like a spreadsheet's filter row. */}
            <div className="flex flex-wrap items-center gap-3 border-b border-slate-100 px-4 py-3">
              <div className="w-full max-w-sm">
                <Input placeholder="Search description, reference, provider, user, amount…" value={search} onChange={(e) => setSearch(e.target.value)} />
              </div>
              <div className="flex overflow-hidden rounded-lg border border-slate-200 text-sm">
                {([['', 'All'], ['Pending', 'Pending'], ['Resolved', 'Resolved']] as const).map(([v, label]) => (
                  <button
                    key={label}
                    type="button"
                    onClick={() => setResolution(v)}
                    className={cn('px-3 py-1.5 font-medium transition-colors', resolution === v ? 'bg-brand-50 text-brand-700' : 'bg-white text-slate-500 hover:bg-slate-50')}
                  >
                    {label}
                  </button>
                ))}
              </div>
              <span className="ml-auto text-xs text-slate-400">{shown.length} of {rows.length} lines · click a heading to sort</span>
            </div>

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
                  title={bucket || search || resolution ? 'Nothing matches these filters' : 'Nothing to reconcile'}
                  description={
                    bucket || search || resolution
                      ? 'Clear the bucket, search or resolution filter to see the rest of the period.'
                      : 'Neither the statement nor the ledger has a movement in this range.'
                  }
                />
              </CardBody>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[1180px] text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                      <th className="cursor-pointer whitespace-nowrap px-4 py-2.5 font-medium" onClick={() => toggleSort('date')}>Date<SortIcon k="date" /></th>
                      <th className="px-3 py-2.5 font-medium">Bucket</th>
                      <th className="cursor-pointer px-3 py-2.5 font-medium" onClick={() => toggleSort('desc')}>Bank statement<SortIcon k="desc" /></th>
                      <th className="cursor-pointer whitespace-nowrap px-3 py-2.5 text-right font-medium" onClick={() => toggleSort('in')}>In<SortIcon k="in" /></th>
                      <th className="cursor-pointer whitespace-nowrap px-3 py-2.5 text-right font-medium" onClick={() => toggleSort('out')}>Out<SortIcon k="out" /></th>
                      <th className="px-3 py-2.5 font-medium">Provider / user</th>
                      <th className="px-3 py-2.5 font-medium">Explained by</th>
                      <th className="cursor-pointer whitespace-nowrap px-3 py-2.5 font-medium" onClick={() => toggleSort('resolution')}>Status<SortIcon k="resolution" /></th>
                      {mayWrite && <th className="px-3 py-2.5" />}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {shown.map((r, i) => {
                      const meta = bankBucketMeta(r.Bucket);
                      const resolved = isResolved(r);
                      const needsPurchase = !resolved && /purchase entry/i.test(r.ResolutionNote);
                      return (
                        <tr key={`${r.BankStatementLineID ?? 'x'}-${r.AcctVoucherLineID ?? 'y'}-${i}`} className="hover:bg-slate-50">
                          <td className="whitespace-nowrap px-4 py-2.5 tabular-nums text-slate-700">
                            {r.BankDate ? dateOnly(r.BankDate) : r.VoucherDate ? <span className="text-slate-400">{dateOnly(r.VoucherDate)}</span> : '—'}
                          </td>
                          <td className="px-3 py-2.5">
                            <Badge tone={meta?.tone ?? 'neutral'}>{meta?.label ?? r.Bucket}</Badge>
                          </td>
                          <td className="px-3 py-2.5">
                            {r.BankStatementLineID ? (
                              <>
                                <div className="max-w-[260px] truncate text-slate-700" title={r.Description}>{r.Description || '—'}</div>
                                <div className="text-xs text-slate-400">
                                  {r.ReferenceNo && <span className="font-mono">{r.ReferenceNo}</span>}
                                  {r.Category && r.Category !== 'UNCATEGORISED' && (
                                    <span className="ml-1.5 rounded bg-slate-100 px-1.5 py-0.5 text-[11px] text-slate-600">{categoryLabel(r.Category)}</span>
                                  )}
                                  {r.DuplicateReason && (
                                    <span title={r.DuplicateReason} className="ml-1.5 cursor-help"><Badge tone="warning">possible duplicate</Badge></span>
                                  )}
                                </div>
                              </>
                            ) : (
                              <span className="text-xs text-slate-400">ledger only — {r.Narration || 'no narration'}</span>
                            )}
                          </td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-success-700">{r.BankCredit > 0 ? inr(r.BankCredit) : ''}</td>
                          <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{r.BankDebit > 0 ? inr(r.BankDebit) : ''}</td>
                          <td className="px-3 py-2.5">
                            {r.Counterparty ? (
                              <span className="max-w-[180px] truncate text-slate-700" title={r.Counterparty}>{r.Counterparty}</span>
                            ) : (
                              <span className="text-slate-300">—</span>
                            )}
                          </td>
                          <td className="px-3 py-2.5">
                            {r.VoucherNo ? (
                              <>
                                <div className="font-mono text-xs font-semibold text-slate-800">{r.VoucherNo}</div>
                                <div className="max-w-[220px] truncate text-xs text-slate-400" title={r.Narration}>
                                  {r.VoucherDate && `${dateOnly(r.VoucherDate)} · `}{r.Narration || 'no narration'}
                                </div>
                                {(r.LedgerDebit > 0 || r.LedgerCredit > 0) && r.AcctVoucherLineID && (
                                  <div className="text-[11px] tabular-nums text-slate-400">
                                    {r.LedgerDebit > 0 ? `Dr ${inr(r.LedgerDebit)}` : `Cr ${inr(r.LedgerCredit)}`}
                                  </div>
                                )}
                              </>
                            ) : (
                              <span className="text-slate-300">—</span>
                            )}
                          </td>
                          <td className="px-3 py-2.5">
                            {r.Bucket === 'Matched' ? (
                              <div className="flex flex-col gap-0.5">
                                <Badge tone={bankMethodTone(r.Method)}>{bankMethodLabel(r.Method)}</Badge>
                                {r.DateGapDays > 0 && <span className="text-[11px] text-slate-400">{r.DateGapDays}d apart</span>}
                              </div>
                            ) : r.Bucket === 'Mismatch' ? (
                              <span className="text-[11px] font-semibold text-danger-600">out by {inr(Math.abs(r.Variance))}</span>
                            ) : r.Resolution ? (
                              <div className="flex flex-col gap-0.5">
                                <Badge tone={resolved ? 'success' : 'warning'}>{resolved ? 'Resolved' : 'Pending'}</Badge>
                                <span className="max-w-[220px] text-[11px] leading-snug text-slate-500" title={r.ResolutionNote}>
                                  {r.ResolutionNote}
                                  {needsPurchase && <> · <Link to="/s/AddPurchase" className="text-brand-600 underline">record it</Link></>}
                                </span>
                              </div>
                            ) : (
                              <span className="text-xs text-slate-300">—</span>
                            )}
                          </td>
                          {mayWrite && (
                            <td className="whitespace-nowrap px-3 py-2.5 text-right">
                              {r.Method && r.BankStatementLineID && r.AcctVoucherLineID ? (
                                <button type="button" onClick={() => unmatch.mutate(r.BankStatementLineID!)} className="inline-flex items-center gap-1 text-xs text-slate-400 hover:text-danger-600">
                                  <Link2Off className="size-3.5" /> Unmatch
                                </button>
                              ) : r.Bucket === 'MissingInBooks' && r.BankStatementLineID ? (
                                <div className="flex items-center justify-end gap-3">
                                  <button type="button" onClick={() => setCatFor(r)} className="inline-flex items-center gap-1 text-xs text-brand-600 hover:underline">
                                    <Tag className="size-3.5" /> {r.Category && r.Category !== 'UNCATEGORISED' ? 'Change' : 'Categorise'}
                                  </button>
                                  {ledgerCandidates.length > 0 && (
                                    <button type="button" onClick={() => { setMatchFor(r); setPickLedgerId(0); }} className="inline-flex items-center gap-1 text-xs text-slate-500 hover:underline">
                                      <Link2 className="size-3.5" /> Match
                                    </button>
                                  )}
                                </div>
                              ) : null}
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

          {/* The uploads held, newest first — provenance for the bank column above. */}
          {(imports.data?.length ?? 0) > 0 && (
            <Card>
              <CardBody className="pb-2">
                <div className="text-sm font-semibold text-slate-700">Statements uploaded</div>
              </CardBody>
              <div className="overflow-x-auto px-3 pb-3">
                <table className="w-full min-w-[760px] text-sm">
                  <thead>
                    <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                      <th className="px-3 py-2 font-medium">Period</th>
                      <th className="px-3 py-2 font-medium">File</th>
                      <th className="px-3 py-2 text-right font-medium">Lines</th>
                      <th className="px-3 py-2 text-right font-medium">In</th>
                      <th className="px-3 py-2 text-right font-medium">Out</th>
                      <th className="px-3 py-2 text-right font-medium">Closing</th>
                      <th className="px-3 py-2 text-right font-medium">Unmatched</th>
                      <th className="px-3 py-2 font-medium">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {imports.data!.map((im) => (
                      <tr key={im.BankStatementImportID} className="hover:bg-slate-50">
                        <td className="px-3 py-2 text-slate-700">{dateOnly(im.PeriodFrom)} – {dateOnly(im.PeriodTo)}</td>
                        <td className="px-3 py-2"><div className="max-w-[200px] truncate text-xs text-slate-500" title={im.FileName}>{im.FileName || im.Source}</div></td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{im.LineCount}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(im.TotalCredit)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{inr(im.TotalDebit)}</td>
                        <td className="px-3 py-2 text-right tabular-nums text-slate-700">{im.ClosingBalance === null ? '—' : inr(im.ClosingBalance)}</td>
                        <td className="px-3 py-2 text-right"><Badge tone={im.UnmatchedLines === 0 ? 'success' : 'warning'}>{im.UnmatchedLines}</Badge></td>
                        <td className="px-3 py-2"><Badge tone={im.Status?.toLowerCase() === 'active' ? 'success' : 'neutral'}>{im.Status}</Badge></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          )}
        </>
      )}

      {!mayWrite && !noAccounts && (
        <p className="text-xs text-slate-400">
          Read-only. Running the comparison is yours — it changes nothing. Uploading a statement, categorising a
          line and committing, pairing or breaking a match are limited to Admin and Support.
        </p>
      )}

      {/* ── Upload ───────────────────────────────────────────────────────────────────── */}
      <Modal open={uploadOpen} onClose={() => setUploadOpen(false)} title="Upload a bank statement" size="lg">
        <div className="space-y-3">
          <p className="text-sm text-slate-500">
            Pick the CSV or Excel file the bank gave you. The columns are worked out from the header row —
            every bank names them differently — and what was detected is shown back to you after the
            import, because a statement read into the wrong columns reconciles to nonsense quietly.
          </p>
          <input
            type="file"
            accept=".csv,.txt,.xls,.xlsx,text/csv,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            onChange={(e) => onPickFile(e.target.files?.[0])}
            className="block w-full rounded-lg border border-slate-200 p-2 text-sm text-slate-600 file:mr-3 file:rounded-md file:border-0 file:bg-brand-50 file:px-3 file:py-1.5 file:text-sm file:text-brand-700"
          />
          {xlsxB64.length > 0 ? (
            <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
              <strong>{fileName}</strong> is ready to upload as Excel.
            </div>
          ) : (
            <textarea
              value={csv}
              onChange={(e) => setCsv(e.target.value)}
              rows={10}
              spellCheck={false}
              placeholder={'Date,Narration,Chq / Ref No,Withdrawal Amt,Deposit Amt,Closing Balance\n01/10/2026,NEFT SBIN0123456789,SBIN0123456789,,50000.00,150000.00'}
              className="w-full rounded-lg border border-slate-200 p-3 font-mono text-xs text-slate-700 focus:border-brand-400 focus:outline-none"
            />
          )}
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => { setUploadOpen(false); resetUpload(); }}>Cancel</Button>
            <Button onClick={() => upload.mutate()} loading={upload.isPending} disabled={csv.trim().length === 0 && xlsxB64.length === 0}>
              <Upload className="mr-1.5 size-4" />
              Import
            </Button>
          </div>
        </div>
      </Modal>

      {/* ── Manual match ─────────────────────────────────────────────────────────────── */}
      <Modal open={matchFor !== null} onClose={() => setMatchFor(null)} title="Pair this with a ledger entry" size="md">
        <div className="space-y-3">
          <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm">
            <div className="text-slate-700">{matchFor?.Description || '—'}</div>
            <div className="mt-1 text-xs text-slate-500">
              {matchFor?.BankDate && dateOnly(matchFor.BankDate)}
              {' · '}
              {(matchFor?.BankCredit ?? 0) > 0 ? `in ${inr(matchFor!.BankCredit)}` : `out ${inr(matchFor?.BankDebit ?? 0)}`}
            </div>
          </div>

          {ledgerCandidates.length === 0 ? (
            <EmptyState
              title="No unmatched ledger entry to pair with"
              description="Every Bank line in this range is already matched. If this movement is genuinely missing from the books, post it as a journal on Ledger Vouchers instead of pairing it."
            />
          ) : (
            <>
              <Select label="Unmatched ledger entry" value={String(pickLedgerId)} onChange={(e) => setPickLedgerId(Number(e.target.value))}>
                <option value="0">Choose one…</option>
                {ledgerCandidates.map((c) => (
                  <option key={c.AcctVoucherLineID!} value={c.AcctVoucherLineID!}>
                    {`${c.VoucherNo} · ${dateOnly(c.VoucherDate!)} · ${c.LedgerDebit > 0 ? `Dr ${inr(c.LedgerDebit)}` : `Cr ${inr(c.LedgerCredit)}`}`}
                  </option>
                ))}
              </Select>
              <p className="text-xs text-slate-400">
                A pairing made here is recorded as "by hand", so a reviewer can tell it from one the match found on a reference.
              </p>
            </>
          )}

          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setMatchFor(null)}>Cancel</Button>
            <Button onClick={() => manualMatch.mutate()} loading={manualMatch.isPending} disabled={pickLedgerId <= 0}>
              <Link2 className="mr-1.5 size-4" />
              Pair
            </Button>
          </div>
        </div>
      </Modal>

      {catFor && (
        <CategoriseModal row={catFor} onClose={() => setCatFor(null)} onDone={() => { setCatFor(null); invalidate(); }} />
      )}
    </div>
  );
}

/**
 * What one statement line IS, and who it is about. A debit offers the provider list (a stock
 * purchase), a credit the user list (a retailer's deposit); the server refuses a category that
 * does not fit the side. Setting it by hand is final — no rule overwrites it afterwards.
 */
function CategoriseModal({ row, onClose, onDone }: { row: ReconRow; onClose: () => void; onDone: () => void }) {
  const toast = useToast();
  const side = row.BankDebit > 0 ? 'DEBIT' : 'CREDIT';
  const options = CATEGORIES.filter((c) => c.side === side || c.side === 'ANY');
  const [category, setCategory] = useState(row.Category && row.Category !== 'UNCATEGORISED' ? row.Category : options[0].value);
  const [providerId, setProviderId] = useState('');
  const [userId, setUserId] = useState<string | number>('');
  const [clearDup, setClearDup] = useState(false);

  const { data: providers = [] } = useQuery({
    queryKey: ['prov-opts'], staleTime: 300_000, enabled: side === 'DEBIT',
    queryFn: () => fetchList('ProviderMaster/GetAll', { IsActive: true, PageNumber: 1, PageSize: 500 }),
  });
  const { data: users = [], isLoading: usersLoading } = useQuery({
    queryKey: ['user-opts-all'], staleTime: 300_000, enabled: side === 'CREDIT',
    queryFn: () => fetchList('Users/Select', { PageNumber: 1, PageSize: 2000, TakeCount: 2000, SkipCount: 0, ShowDeleted: false }),
  });
  const userOptions: AutocompleteOption[] = useMemo(
    () => users.map((u) => ({
      value: String(getField(u, 'UserMasterID', 'UserID') ?? ''),
      label: String(getField(u, 'FirmName', 'PersonName', 'Name') ?? '—'),
      sublabel: [getField(u, 'UserType'), getField(u, 'MobileNumber', 'Mobile')].filter(Boolean).map(String).join(' · ') || undefined,
    })),
    [users],
  );

  const save = useMutation({
    mutationFn: () => apiPost('BankSetup/SetLineCategory', {
      BankStatementLineID: row.BankStatementLineID,
      Category: category,
      ProviderID: category === 'PURCHASE' ? Number(providerId || 0) : 0,
      CounterpartyUserID: category === 'USER_FUND' ? Number(userId || 0) : 0,
      ClearDuplicateFlag: clearDup,
    }),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message || 'The line could not be categorised.'); return; }
      toast.success('Saved.');
      onDone();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Modal open onClose={onClose} title="What is this line?" size="md">
      <div className="space-y-3">
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm">
          <div className="text-slate-700">{row.Description || '—'}</div>
          <div className="mt-1 text-xs text-slate-500">
            {row.BankDate && dateOnly(row.BankDate)}{row.ReferenceNo && ` · ${row.ReferenceNo}`}{' · '}
            {side === 'CREDIT' ? `in ${inr(row.BankCredit)}` : `out ${inr(row.BankDebit)}`}
          </div>
        </div>

        <Select label="Category" value={category} onChange={(e) => setCategory(e.target.value)}>
          {options.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
        </Select>

        {category === 'PURCHASE' && (
          <>
            <Select label="Provider" value={providerId} onChange={(e) => setProviderId(e.target.value)}>
              <option value="">Choose the provider…</option>
              {providers.map((p, i) => {
                const id = String(getField(p, 'ProviderID') ?? i);
                return <option key={id} value={id}>{String(getField(p, 'ProviderName', 'Name') ?? id)}</option>;
              })}
            </Select>
            <p className="text-xs text-slate-500">
              The line stays <strong>pending</strong> until a matching entry exists on{' '}
              <Link to="/s/AddPurchase" className="text-brand-600 underline">Provider Purchases</Link> — that entry is what
              moves the provider's stock, so the category alone does not close it.
            </p>
          </>
        )}

        {category === 'USER_FUND' && (
          <Autocomplete
            label="User"
            placeholder="Whose deposit is this?"
            options={userOptions}
            value={userId}
            loading={usersLoading}
            onChange={(v) => setUserId(v)}
          />
        )}

        {row.DuplicateReason && (
          <label className="flex items-start gap-2 text-sm text-slate-700">
            <input type="checkbox" checked={clearDup} onChange={(e) => setClearDup(e.target.checked)} className="mt-0.5 size-4 rounded border-slate-300" />
            <span>Clear the duplicate flag — I have checked and this is not the same money twice.<br /><span className="text-xs text-slate-400">{row.DuplicateReason}</span></span>
          </label>
        )}

        <div className="flex justify-end gap-2 pt-1">
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button onClick={() => save.mutate()} loading={save.isPending} disabled={category === 'PURCHASE' && !providerId}>
            <Tag className="mr-1.5 size-4" />
            Save
          </Button>
        </div>
      </div>
    </Modal>
  );
}
