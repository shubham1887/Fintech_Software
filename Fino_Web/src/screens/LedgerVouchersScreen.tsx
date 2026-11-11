import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, ChevronDown, ChevronRight, Download, Inbox, PenLine, Play, Plus, RotateCcw, Trash2,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly, dateTime, apiDate } from '@/lib/format';
import { useCsvExport } from '@/lib/exportAll';
import { cn } from '@/lib/cn';
import {
  canOperate, canRaiseDocument, outboxTone, periodKeyOf, periodLabel, recentPeriods,
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
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface VoucherLine {
  AccountCode: string;
  AccountName: string;
  Debit: number;
  Credit: number;
  Narration: string;
}

interface Voucher {
  AcctVoucherID: number;
  VoucherNo: string;
  VoucherDate: string;
  PeriodKey: string;
  VoucherType: string;
  Narration: string;
  TotalDebit: number;
  TotalCredit: number;
  Status: string;
  Lines: VoucherLine[];
}

interface OutboxRow {
  AcctPostingOutboxID: number;
  EventType: string;
  EventKey: string;
  EventDate: string;
  Narration: string;
  Status: string;
  AttemptCount: number;
  LastAttemptOn: string | null;
  LastError: string;
  PostedVoucherID: number;
}

interface OutboxResult {
  Attempted: number;
  Posted: number;
  Failed: number;
  Skipped: number;
  Messages: string[];
}

interface JournalLine {
  key: number;
  AccountCode: string;
  Debit: string;
  Credit: string;
  Narration: string;
}

const blankJournalLine = (key: number): JournalLine => ({ key, AccountCode: '', Debit: '', Credit: '', Narration: '' });

const n = (v: string) => {
  const x = Number(String(v).replace(/,/g, '').trim());
  return Number.isFinite(x) ? x : 0;
};

/** Vouchers the screen reads at once. The export walks these windows until one comes back short. */
const READ_WINDOW = 1000;

async function fetchVouchers(period: string, skip: number, take: number): Promise<Voucher[]> {
  const res = await apiPost<Voucher[]>('AcctLedger/Vouchers', {
    PeriodKey: period,
    SkipCount: skip,
    TakeCount: take,
  });
  if (!res.ok) throw new Error(res.message || 'Could not load the vouchers.');
  return res.data ?? [];
}

export function LedgerVouchersScreen({ def }: CustomScreenProps) {
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayOperate = canOperate(user?.UserTypeID);
  const mayWriteBooks = canRaiseDocument(user?.UserTypeID);

  const periods = useMemo(() => recentPeriods(18), []);
  const [period, setPeriod] = useState(periodKeyOf());
  const [outboxFilter, setOutboxFilter] = useState('');
  const [open, setOpen] = useState<number[]>([]);
  const { exporting, runExport } = useCsvExport();

  const vouchers = useQuery({
    queryKey: ['acct-vouchers', period],
    queryFn: () => fetchVouchers(period, 0, READ_WINDOW),
  });

  const outbox = useQuery({
    queryKey: ['acct-outbox', outboxFilter],
    queryFn: async (): Promise<OutboxRow[]> => {
      const res = await apiPost<OutboxRow[]>('AcctLedger/Outbox', { Status: outboxFilter });
      if (!res.ok) throw new Error(res.message || 'Could not load the outbox.');
      return res.data ?? [];
    },
  });

  const rows = vouchers.data ?? [];
  const queue = outbox.data ?? [];

  const counts = useMemo(() => {
    const by = (s: string) => queue.filter((q) => q.Status?.toUpperCase() === s).length;
    return { pending: by('PENDING'), failed: by('FAILED'), skipped: by('SKIPPED'), posted: by('POSTED') };
  }, [queue]);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ['acct-vouchers'] });
    qc.invalidateQueries({ queryKey: ['acct-outbox'] });
    qc.invalidateQueries({ queryKey: ['acct-trial-balance'] });
  };

  const process = useMutation({
    mutationFn: async (): Promise<OutboxResult> => {
      const res = await apiPost<OutboxResult>('AcctLedger/ProcessOutbox', { MaxItems: 500 });
      if (!res.ok || !res.data) throw new Error(res.message || 'The outbox could not be processed.');
      return res.data;
    },
    onSuccess: (r) => {
      toast.success(`Attempted ${r.Attempted}: ${r.Posted} posted, ${r.Failed} failed, ${r.Skipped} skipped.`);
      r.Messages.slice(0, 5).forEach((m) => toast.info(m));
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Manual journal ──────────────────────────────────────────────────────────────────
  const [journalOpen, setJournalOpen] = useState(false);
  const [jNarration, setJNarration] = useState('');
  const [jDate, setJDate] = useState(apiDate(new Date()));
  const [jLines, setJLines] = useState<JournalLine[]>([blankJournalLine(1), blankJournalLine(2)]);

  const jTotals = useMemo(
    () =>
      jLines.reduce((a, l) => ({ dr: a.dr + n(l.Debit), cr: a.cr + n(l.Credit) }), { dr: 0, cr: 0 }),
    [jLines],
  );
  const jBalanced = Math.abs(jTotals.dr - jTotals.cr) < 0.005 && jTotals.dr > 0;

  const journalErrors: string[] = [];
  if (!jNarration.trim()) journalErrors.push('A narration is required — an entry nobody can explain is one nobody can defend.');
  if (jLines.filter((l) => l.AccountCode.trim()).length < 2) journalErrors.push('A journal needs at least two lines with an account code.');
  if (!jBalanced) journalErrors.push('Debits and credits must be equal, and more than zero.');

  const postJournal = useMutation({
    mutationFn: async () => {
      const res = await apiPost('AcctLedger/PostJournal', {
        VoucherDate: jDate,
        Narration: jNarration.trim(),
        Lines: jLines
          .filter((l) => l.AccountCode.trim())
          .map((l) => ({
            AccountCode: l.AccountCode.trim(),
            Debit: n(l.Debit),
            Credit: n(l.Credit),
            Narration: l.Narration.trim(),
          })),
      });
      if (!res.ok) throw new Error(res.message || 'The journal could not be posted.');
    },
    onSuccess: () => {
      toast.success('Journal posted.');
      setJournalOpen(false);
      setJNarration('');
      setJLines([blankJournalLine(1), blankJournalLine(2)]);
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Reverse a voucher ───────────────────────────────────────────────────────────────
  const [reverseFor, setReverseFor] = useState<Voucher | null>(null);
  const [rNarration, setRNarration] = useState('');

  const reverse = useMutation({
    mutationFn: async () => {
      const res = await apiPost('AcctLedger/Reverse', {
        AcctVoucherID: reverseFor?.AcctVoucherID ?? 0,
        Narration: rNarration.trim() || `Reversal of ${reverseFor?.VoucherNo}`,
        ReversalDate: null,
      });
      if (!res.ok) throw new Error(res.message || 'The voucher could not be reversed.');
    },
    onSuccess: () => {
      toast.success('Voucher reversed — a mirror entry was posted rather than the original being changed.');
      setReverseFor(null);
      setRNarration('');
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggle = (id: number) => setOpen((o) => (o.includes(id) ? o.filter((x) => x !== id) : [...o, id]));

  /** One CSV line per posting line, which is what a book-keeper reconciles against. */
  const voucherLines = (vs: Voucher[]) =>
    vs.flatMap((v) =>
      v.Lines.map((l) => ({
        Voucher: v.VoucherNo,
        Date: dateOnly(v.VoucherDate),
        Type: v.VoucherType,
        Narration: v.Narration,
        AccountCode: l.AccountCode,
        AccountName: l.AccountName,
        Debit: l.Debit,
        Credit: l.Credit,
        Status: v.Status,
      })),
    );

  // The screen reads a window of the period; the file holds every voucher in it.
  const download = () => runExport({
    filename: `ledger-vouchers-${period}`,
    onScreen: rows,
    fetchAll: async (skip, take) => ({ rows: await fetchVouchers(period, skip, take), count: 0 }),
    shape: voucherLines,
  });

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="The posted books, and the outbox that feeds them — including what could not post, and why"
        icon={pickIcon(def.title)}
        actions={
          <div className="flex gap-2">
            {mayWriteBooks && (
              <Button variant="outline" onClick={() => setJournalOpen(true)}>
                <PenLine className="mr-1.5 size-4" />
                Post journal
              </Button>
            )}
            {mayOperate && (
              <Button onClick={() => process.mutate()} loading={process.isPending} disabled={counts.pending + counts.failed === 0}>
                <Play className="mr-1.5 size-4" />
                Process outbox
              </Button>
            )}
          </div>
        }
      />

      <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
        <StatTile label="Waiting" value={counts.pending} icon={Inbox} tone="text-warning-600" loading={outbox.isFetching} />
        <StatTile label="Failed" value={counts.failed} icon={AlertTriangle} tone="text-danger-600" loading={outbox.isFetching} />
        <StatTile label="Skipped" value={counts.skipped} icon={ChevronRight} tone="text-slate-600" loading={outbox.isFetching} />
        <StatTile label={`Vouchers in ${periodLabel(period)}`} value={rows.length} icon={Download} tone="text-success-600" loading={vouchers.isFetching} />
      </div>

      {counts.failed > 0 && (
        <div className="flex gap-2.5 rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-sm text-danger-600">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>{counts.failed} event{counts.failed === 1 ? '' : 's'} could not post.</strong> Each keeps its
            error below and is retried on the next pass — a posting that cannot be made is a visible row here,
            never a number quietly left wrong.
          </p>
        </div>
      )}

      {/* ── The outbox ────────────────────────────────────────────────────────────────── */}
      <Card>
        <CardBody className="flex flex-wrap items-end gap-3 pb-2">
          <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
            <Inbox className="size-4 text-slate-400" />
            Posting outbox
          </div>
          <div className="flex-1" />
          <div className="w-44">
            <Select label="" value={outboxFilter} onChange={(e) => setOutboxFilter(e.target.value)}>
              <option value="">Every status</option>
              <option value="Pending">Pending</option>
              <option value="Failed">Failed</option>
              <option value="Skipped">Skipped</option>
              <option value="Posted">Posted</option>
            </Select>
          </div>
        </CardBody>
        {outbox.isFetching ? (
          <div className="flex min-h-[18vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : queue.length === 0 ? (
          <CardBody>
            <EmptyState
              title="The outbox is empty"
              description="Business events land here as they happen — a wallet top-up, a settled sale, a provider purchase — and the posting engine turns each into a balanced voucher."
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto px-3 pb-3">
            <table className="w-full min-w-[860px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Event</th>
                  <th className="px-3 py-2 font-medium">Key</th>
                  <th className="px-3 py-2 font-medium">Date</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 text-right font-medium">Tries</th>
                  <th className="px-3 py-2 font-medium">Last error / voucher</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {queue.map((q) => (
                  <tr key={q.AcctPostingOutboxID} className="hover:bg-slate-50">
                    <td className="px-3 py-2">
                      <div className="font-mono text-xs font-semibold text-slate-800">{q.EventType}</div>
                      <div className="max-w-[220px] truncate text-xs text-slate-400" title={q.Narration}>{q.Narration}</div>
                    </td>
                    <td className="px-3 py-2">
                      <span className="block max-w-[200px] truncate font-mono text-xs text-slate-500" title={q.EventKey}>{q.EventKey}</span>
                    </td>
                    <td className="px-3 py-2 text-xs text-slate-600">{dateOnly(q.EventDate)}</td>
                    <td className="px-3 py-2"><Badge tone={outboxTone(q.Status)}>{q.Status}</Badge></td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-500">{q.AttemptCount}</td>
                    <td className="px-3 py-2">
                      {q.LastError ? (
                        <span className="block max-w-[320px] truncate text-xs text-danger-600" title={q.LastError}>{q.LastError}</span>
                      ) : q.PostedVoucherID > 0 ? (
                        <span className="font-mono text-xs text-success-600">voucher #{q.PostedVoucherID}</span>
                      ) : (
                        <span className="text-xs text-slate-300">—</span>
                      )}
                      {q.LastAttemptOn && <div className="text-[11px] text-slate-400">{dateTime(q.LastAttemptOn)}</div>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {/* ── The vouchers ─────────────────────────────────────────────────────────────── */}
      <Card>
        <CardBody className="flex flex-wrap items-end gap-3 pb-2">
          <div className="w-44">
            <Select label="Period" value={period} onChange={(e) => setPeriod(e.target.value)}>
              {periods.map((p) => (
                <option key={p} value={p}>{periodLabel(p)}</option>
              ))}
            </Select>
          </div>
          <div className="flex-1" />
          <Button variant="ghost" onClick={download} loading={exporting} disabled={rows.length === 0}>
            <Download className="mr-1.5 size-4" />
            CSV
          </Button>
        </CardBody>

        {vouchers.isFetching ? (
          <div className="flex min-h-[25vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : vouchers.isError ? (
          <CardBody>
            <EmptyState title="Could not load" description={(vouchers.error as Error).message} />
          </CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title={`No vouchers in ${periodLabel(period)}`}
              description="Nothing has posted into this period. If the outbox above has waiting rows, process it."
            />
          </CardBody>
        ) : (
          <div className="divide-y divide-slate-100">
            {rows.map((v) => {
              const isOpen = open.includes(v.AcctVoucherID);
              const balanced = Math.abs(v.TotalDebit - v.TotalCredit) < 0.005;
              return (
                <div key={v.AcctVoucherID}>
                  <button
                    type="button"
                    onClick={() => toggle(v.AcctVoucherID)}
                    className="flex w-full items-center gap-3 px-5 py-3 text-left hover:bg-slate-50"
                  >
                    {isOpen ? <ChevronDown className="size-4 shrink-0 text-slate-400" /> : <ChevronRight className="size-4 shrink-0 text-slate-400" />}
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <span className="font-mono text-sm font-semibold text-slate-800">{v.VoucherNo}</span>
                        <Badge tone="neutral">{v.VoucherType}</Badge>
                        {!balanced && <Badge tone="danger">unbalanced</Badge>}
                      </div>
                      <div className="truncate text-xs text-slate-400" title={v.Narration}>
                        {dateOnly(v.VoucherDate)} · {v.Narration || 'no narration'}
                      </div>
                    </div>
                    <span className="shrink-0 tabular-nums text-sm font-semibold text-slate-800">{inr(v.TotalDebit)}</span>
                  </button>

                  {isOpen && (
                    <div className="bg-slate-50/60 px-5 pb-4">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="text-left text-xs uppercase tracking-wide text-slate-400">
                            <th className="py-1.5 font-medium">Account</th>
                            <th className="py-1.5 text-right font-medium">Debit</th>
                            <th className="py-1.5 text-right font-medium">Credit</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-200/70">
                          {v.Lines.map((l, i) => (
                            <tr key={`${v.AcctVoucherID}-${i}`}>
                              <td className="py-1.5">
                                <span className="font-mono text-xs text-slate-500">{l.AccountCode}</span>{' '}
                                <span className="text-slate-700">{l.AccountName}</span>
                                {l.Narration && <div className="text-xs text-slate-400">{l.Narration}</div>}
                              </td>
                              <td className="py-1.5 text-right tabular-nums text-slate-700">{l.Debit > 0 ? inr(l.Debit) : ''}</td>
                              <td className="py-1.5 text-right tabular-nums text-slate-700">{l.Credit > 0 ? inr(l.Credit) : ''}</td>
                            </tr>
                          ))}
                        </tbody>
                        <tfoot>
                          <tr className="border-t border-slate-300 font-semibold">
                            <td className="py-1.5 text-slate-600">Total</td>
                            <td className="py-1.5 text-right tabular-nums text-slate-800">{inr(v.TotalDebit)}</td>
                            <td className="py-1.5 text-right tabular-nums text-slate-800">{inr(v.TotalCredit)}</td>
                          </tr>
                        </tfoot>
                      </table>
                      {mayWriteBooks && (
                        <div className="mt-3 flex justify-end">
                          <Button
                            variant="ghost"
                            onClick={() => { setReverseFor(v); setRNarration(`Reversal of ${v.VoucherNo}`); }}
                          >
                            <RotateCcw className="mr-1.5 size-4" />
                            Reverse this voucher
                          </Button>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </Card>

      {!mayWriteBooks && (
        <p className="text-xs text-slate-400">
          {mayOperate
            ? 'Posting a journal and reversing a voucher are limited to Admin — writing the books by hand carries the same weight as issuing a document. Working the outbox is available to you.'
            : 'Read-only. Working the outbox, posting a journal and reversing a voucher are all limited to Admin / Support. Everything that posted, and everything that could not, is visible here.'}
        </p>
      )}

      {/* ── Manual journal ───────────────────────────────────────────────────────────── */}
      <Modal open={journalOpen} onClose={() => setJournalOpen(false)} title="Post a manual journal" size="lg">
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            For an adjustment or an opening balance. It is written through the same balanced-write as every
            other voucher, so it cannot post unless debits equal credits.
          </p>
          <div className="grid gap-3 sm:grid-cols-3">
            <Input label="Voucher date" type="date" value={jDate} onChange={(e) => setJDate(e.target.value)} />
            <div className="sm:col-span-2">
              <Input label="Narration" value={jNarration} onChange={(e) => setJNarration(e.target.value)} hint="Why this entry is being made" />
            </div>
          </div>

          <div className="overflow-x-auto rounded-lg border border-slate-200">
            <table className="w-full min-w-[620px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Account code</th>
                  <th className="px-2 py-2 text-right font-medium">Debit</th>
                  <th className="px-2 py-2 text-right font-medium">Credit</th>
                  <th className="px-3 py-2 font-medium">Line narration</th>
                  <th className="px-2 py-2" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {jLines.map((l, i) => {
                  const set = (k: keyof JournalLine, v: string) =>
                    setJLines((ls) => ls.map((x, j) => (j === i ? { ...x, [k]: v } : x)));
                  return (
                    <tr key={l.key}>
                      <td className="w-32 px-3 py-1.5"><Input value={l.AccountCode} onChange={(e) => set('AccountCode', e.target.value)} placeholder="1000" /></td>
                      <td className="w-28 px-2 py-1.5"><Input value={l.Debit} onChange={(e) => set('Debit', e.target.value)} className="text-right" /></td>
                      <td className="w-28 px-2 py-1.5"><Input value={l.Credit} onChange={(e) => set('Credit', e.target.value)} className="text-right" /></td>
                      <td className="px-3 py-1.5"><Input value={l.Narration} onChange={(e) => set('Narration', e.target.value)} /></td>
                      <td className="px-2 py-1.5">
                        {jLines.length > 2 && (
                          <button type="button" onClick={() => setJLines((ls) => ls.filter((_, j) => j !== i))} className="text-slate-400 hover:text-danger-500" aria-label="Remove line">
                            <Trash2 className="size-4" />
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
              <tfoot>
                <tr className="border-t border-slate-200 bg-slate-50">
                  <td className="px-3 py-2">
                    <button type="button" onClick={() => setJLines((ls) => [...ls, blankJournalLine(Date.now())])} className="inline-flex items-center gap-1 text-sm text-brand-600 hover:underline">
                      <Plus className="size-3.5" /> Add line
                    </button>
                  </td>
                  <td className={cn('px-2 py-2 text-right font-semibold tabular-nums', jBalanced ? 'text-slate-800' : 'text-danger-600')}>{inr(jTotals.dr)}</td>
                  <td className={cn('px-2 py-2 text-right font-semibold tabular-nums', jBalanced ? 'text-slate-800' : 'text-danger-600')}>{inr(jTotals.cr)}</td>
                  <td className="px-3 py-2 text-xs text-slate-500" colSpan={2}>
                    {jBalanced ? 'Balanced' : `Out by ${inr(Math.abs(jTotals.dr - jTotals.cr))}`}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>

          {journalErrors.length > 0 && (
            <ul className="space-y-1 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
              {journalErrors.map((e) => <li key={e}>• {e}</li>)}
            </ul>
          )}

          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setJournalOpen(false)}>Cancel</Button>
            <Button onClick={() => postJournal.mutate()} loading={postJournal.isPending} disabled={journalErrors.length > 0}>
              Post journal
            </Button>
          </div>
        </div>
      </Modal>

      {/* ── Reverse ──────────────────────────────────────────────────────────────────── */}
      <Modal open={reverseFor !== null} onClose={() => setReverseFor(null)} title="Reverse a voucher" size="sm">
        <div className="space-y-3">
          <p className="text-sm text-slate-600">
            {reverseFor?.VoucherNo} — {inr(reverseFor?.TotalDebit ?? 0)}. A mirror entry is posted; the
            original voucher is never altered, which is what keeps the audit trail readable.
          </p>
          <Input label="Narration" value={rNarration} onChange={(e) => setRNarration(e.target.value)} />
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setReverseFor(null)}>Cancel</Button>
            <Button variant="danger" onClick={() => reverse.mutate()} loading={reverse.isPending}>
              <RotateCcw className="mr-1.5 size-4" />
              Reverse
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
