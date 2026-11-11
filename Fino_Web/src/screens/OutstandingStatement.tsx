import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { format } from 'date-fns';
import { Download, Search as SearchIcon } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr } from '@/lib/format';
import { getField } from '@/lib/row';
import { exportToCsv } from '@/lib/csv';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';

type Row = Record<string, unknown>;

/** One page of statement rows. Also the batch size an export walks the window in. */
const PAGE_SIZE = 100;

/**
 * One user's outstanding statement: date, who put it there, opening, credit, debit, closing,
 * remark — the page the View button opens.
 *
 * Why opening and closing are shown per row and not just a running total: each row's closing IS
 * the next row's opening, so the chain is checkable by eye. A posting that went missing, or one
 * that was written twice, shows up as a break in that chain rather than only as a total that
 * nobody can tie out six months later.
 *
 * Endpoint: CashCollection/Statement.
 */
export function OutstandingStatement({ userId, parentId = 0 }: { userId: number; parentId?: number }) {
  const toast = useToast();
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [applied, setApplied] = useState({ fromDate: '', toDate: '' });
  const [page, setPage] = useState(0);
  const [exporting, setExporting] = useState(false);

  const query = (skip: number, take: number) => ({
    UserID: userId,
    ParentID: parentId,
    FromDate: applied.fromDate,
    ToDate: applied.toDate,
    SkipCount: skip,
    TakeCount: take,
    IsExcelDownload: false,
  });

  const { data, isFetching } = useQuery({
    queryKey: ['outstanding-statement', userId, parentId, applied.fromDate, applied.toDate, page],
    queryFn: async () => {
      const res = await apiPost<Record<string, unknown>>('CashCollection/Statement', query(page * PAGE_SIZE, PAGE_SIZE));
      if (!res.ok) throw new Error(res.message || 'Could not load the statement.');
      return (res.data ?? {}) as Record<string, unknown>;
    },
  });

  const rowsRaw = data ? getField(data, 'Rows') : null;
  const rows: Row[] = Array.isArray(rowsRaw) ? (rowsRaw as Row[]) : [];
  const totalCount = Number(getField(data ?? {}, 'TotalCount') ?? 0);
  const currentOutstanding = Number(getField(data ?? {}, 'CurrentOutstanding') ?? 0);
  const maxOutstanding = Number(getField(data ?? {}, 'MaxOutstanding') ?? 0);
  const pageCount = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  const runSearch = () => { setApplied({ fromDate, toDate }); setPage(0); };

  /**
   * Export the WHOLE filtered window, not the page on screen.
   *
   * It loops until it has TotalCount rows (or a batch comes back short), because that is the only
   * thing that generalises here: nothing guarantees a single request can carry four hundred rows,
   * and a CSV that silently stopped at the first hundred is the worst kind of wrong — it looks
   * complete.
   */
  const exportAll = async () => {
    setExporting(true);
    try {
      const all: Row[] = [];
      for (let skip = 0; skip < Math.max(totalCount, 1); skip += PAGE_SIZE) {
        const res = await apiPost<Record<string, unknown>>('CashCollection/Statement', query(skip, PAGE_SIZE));
        if (!res.ok) throw new Error(res.message || 'The export could not be completed.');
        const batchRaw = getField((res.data ?? {}) as Record<string, unknown>, 'Rows');
        const batch: Row[] = Array.isArray(batchRaw) ? (batchRaw as Row[]) : [];
        all.push(...batch);
        if (batch.length < PAGE_SIZE) break;
      }

      if (all.length === 0) { toast.error('There is nothing to export.'); return; }

      // Shaped explicitly rather than handing over the raw rows: the export is what someone
      // reconciles against, so the columns are the ones on screen, named the same way, in the
      // same order - not whatever fields the API happens to carry.
      exportToCsv(
        all.map((r) => ({
          Date: when(getField(r, 'Date')),
          User: String(getField(r, 'UserName') ?? ''),
          Counterparty: String(getField(r, 'ParentName') ?? ''),
          Opening: Number(getField(r, 'Opening') ?? 0).toFixed(2),
          Credit: Number(getField(r, 'Credit') ?? 0).toFixed(2),
          Debit: Number(getField(r, 'Debit') ?? 0).toFixed(2),
          Closing: Number(getField(r, 'Closing') ?? 0).toFixed(2),
          Wallet: String(getField(r, 'WalletName') ?? ''),
          'Wallet opening': money(getField(r, 'WalletOpening')),
          'Wallet closing': money(getField(r, 'WalletClosing')),
          Type: String(getField(r, 'TypeLabel') ?? ''),
          'Pay mode': String(getField(r, 'PayMode') ?? ''),
          UTR: String(getField(r, 'UTRNumber') ?? ''),
          Remark: String(getField(r, 'Remark') ?? ''),
        })),
        `outstanding-statement-${userId}`,
        ['Date', 'User', 'Counterparty', 'Opening', 'Credit', 'Debit', 'Closing',
         'Wallet', 'Wallet opening', 'Wallet closing', 'Type', 'Pay mode', 'UTR', 'Remark'],
      );
    } catch (e) {
      toast.error((e as Error).message);
    } finally {
      setExporting(false);
    }
  };

  // Two opening/closing pairs, deliberately: the first says what the DEBT did, the second what the
  // WALLET did. They answer different questions and somebody checking a credit line needs both —
  // "1,50,000 credit, into which wallet, and what did that wallet go from and to".
  const columns = [
    'Date', 'Counterparty', 'Opening', 'Credit', 'Debit', 'Closing',
    'Wallet', 'Wallet opening', 'Wallet closing', 'Type', 'Remark',
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div className="flex items-center gap-5">
          <div>
            <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">Outstanding now</div>
            <div className="text-sm font-semibold tabular-nums text-warning-600">{inr(currentOutstanding)}</div>
          </div>
          <span className="h-8 w-px bg-slate-200/80" />
          <div>
            <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">Credit limit</div>
            <div className="text-sm font-semibold tabular-nums text-slate-800">
              {maxOutstanding > 0 ? inr(maxOutstanding) : 'No credit'}
            </div>
          </div>
          <span className="h-8 w-px bg-slate-200/80" />
          <div>
            <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">Entries</div>
            <div className="text-sm font-semibold tabular-nums text-slate-800">{totalCount}</div>
          </div>
        </div>

        <div className="flex items-end gap-2">
          <Input label="From" type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
          <Input label="To" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
          <Button size="sm" onClick={runSearch} loading={isFetching}>
            <SearchIcon className="size-4" />
          </Button>
          <Button size="sm" variant="outline" onClick={exportAll} loading={exporting} disabled={totalCount === 0}>
            <Download className="size-4" />
            CSV
          </Button>
        </div>
      </div>

      <div className="overflow-x-auto rounded-xl border border-slate-200">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
              {columns.map((h) => (
                <th
                  key={h}
                  className={cn(
                    'whitespace-nowrap px-3 py-2.5',
                    ['Opening', 'Credit', 'Debit', 'Closing', 'Wallet opening', 'Wallet closing'].includes(h)
                      ? 'text-right' : 'text-left',
                  )}
                >
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {isFetching ? (
              Array.from({ length: 6 }).map((_, r) => (
                <tr key={r} className="border-b border-slate-100 last:border-0">
                  {columns.map((__, c) => (
                    <td key={c} className="px-3 py-2.5"><Skeleton className="h-4 w-full max-w-[90px]" /></td>
                  ))}
                </tr>
              ))
            ) : rows.length === 0 ? (
              <tr>
                <td colSpan={columns.length}>
                  <EmptyState
                    title="No entries"
                    description="Nothing has been credited or collected on this account for the selected dates."
                  />
                </td>
              </tr>
            ) : (
              rows.map((r, i) => {
                const credit = Number(getField(r, 'Credit') ?? 0);
                const debit = Number(getField(r, 'Debit') ?? 0);
                return (
                  <tr key={String(getField(r, 'CashCollectionDetailID') ?? i)} className="border-b border-slate-100 last:border-0 hover:bg-slate-50/60">
                    <td className="whitespace-nowrap px-3 py-2.5 tabular-nums text-slate-600">{when(getField(r, 'Date'))}</td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-slate-700">{String(getField(r, 'ParentName') || '—')}</td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-right tabular-nums text-slate-500">{inr(Number(getField(r, 'Opening') ?? 0))}</td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-right tabular-nums">
                      {credit > 0 ? <span className="font-medium text-warning-600">{inr(credit)}</span> : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-right tabular-nums">
                      {debit > 0 ? <span className="font-medium text-success-600">{inr(debit)}</span> : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-right font-semibold tabular-nums text-slate-800">{inr(Number(getField(r, 'Closing') ?? 0))}</td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-slate-600">{String(getField(r, 'WalletName') || '—')}</td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-right tabular-nums text-slate-500">{money(getField(r, 'WalletOpening'))}</td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-right tabular-nums text-slate-500">{money(getField(r, 'WalletClosing'))}</td>
                    <td className="whitespace-nowrap px-3 py-2.5 text-slate-600">{String(getField(r, 'TypeLabel') ?? '')}</td>
                    <td className="px-3 py-2.5 text-slate-600">{String(getField(r, 'Remark') ?? '')}</td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {pageCount > 1 && (
        <div className="flex items-center justify-between text-xs text-slate-500">
          <span>Page {page + 1} of {pageCount}</span>
          <div className="flex gap-2">
            <Button size="sm" variant="outline" disabled={page === 0} onClick={() => setPage((p) => p - 1)}>Previous</Button>
            <Button size="sm" variant="outline" disabled={page + 1 >= pageCount} onClick={() => setPage((p) => p + 1)}>Next</Button>
          </div>
        </div>
      )}
    </div>
  );
}

/**
 * A wallet balance that may legitimately not exist.
 *
 * A cash collection moves no wallet, so these are null on those rows — and a dash is the honest
 * rendering. Showing 0.00 would read as "the wallet was empty", which is a different and alarming
 * claim about somebody's money.
 */
function money(v: unknown): string {
  if (v === null || v === undefined || v === '') return '—';
  const n = Number(v);
  return Number.isFinite(n) ? inr(n) : '—';
}

/** Date + time: two postings on one day are common, and the order they happened in is the point. */
function when(v: unknown): string {
  const d = new Date(String(v ?? ''));
  return Number.isFinite(d.getTime()) && d.getFullYear() > 1 ? format(d, 'dd MMM yy HH:mm') : '—';
}
