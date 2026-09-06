import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import { ChevronDown, Search, SlidersHorizontal } from 'lucide-react';
import { selectReport, type ReportFilter, type TransactionRow } from '@/api/transactionReport.api';
import { fetchCatalog } from '@/api/catalog.api';
import { apiDate, dateTime, inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';
import { txnNote, txnRef, txnService, txnSubtitle, txnTitle, txnWhen, type ServiceNames } from '@/lib/txn';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * Transaction history for a phone.
 *
 * The desktop screen is a seven-column table behind a six-field filter form — on a 375px
 * screen that becomes a horizontal scroll nobody uses. Here the same query is driven by one
 * search box plus status chips (the filter a retailer actually reaches for: "did it fail?"),
 * and each transaction is a row that expands for the IDs you only need when raising a
 * complaint. Dates stay available but folded away.
 */

const TODAY = apiDate(new Date());
const WEEK_AGO = apiDate(subDays(new Date(), 7));

const STATUS_CHIPS = [
  { label: 'All', value: '' },
  { label: 'Success', value: 'SUCCESS' },
  { label: 'Pending', value: 'PENDING' },
  { label: 'Failed', value: 'FAILED' },
  { label: 'Refund', value: 'REFUND' },
];

function statusTone(status?: string): string {
  const s = (status ?? '').toLowerCase();
  if (s.includes('success')) return 'bg-success-50 text-success-600';
  if (s.includes('fail') || s.includes('reject')) return 'bg-danger-50 text-danger-600';
  if (s.includes('refund')) return 'bg-info-50 text-info-600';
  return 'bg-warning-50 text-warning-600';
}

export default function MobileTransactionsPage() {
  const [filter, setFilter] = useState<ReportFilter>({
    fromDate: WEEK_AGO,
    toDate: TODAY,
    serviceType: '',
    status: '',
    search: '',
  });
  const [searchDraft, setSearchDraft] = useState('');
  const [datesOpen, setDatesOpen] = useState(false);
  const [openRow, setOpenRow] = useState<string | number | null>(null);

  const { data: rows = [], isFetching } = useQuery({
    queryKey: ['txn-report', filter],
    queryFn: () => selectReport({ ...filter, pageSize: 50 }),
  });

  // The report only carries ServiceID; the catalog turns that into "Money Transfer".
  // Same query key as the dashboard, so this is a cache hit there.
  const catalogQ = useQuery({ queryKey: ['service-catalog'], queryFn: fetchCatalog, staleTime: 300_000 });
  const serviceNames: ServiceNames = useMemo(() => {
    const map = new Map<number, string>();
    for (const cat of catalogQ.data ?? []) for (const s of cat.Services) map.set(s.ServiceID, s.Name);
    return map;
  }, [catalogQ.data]);

  const setStatus = (status: string) => {
    haptic();
    setFilter((f) => ({ ...f, status }));
  };

  return (
    <div className="space-y-3">
      <form
        onSubmit={(e) => {
          e.preventDefault();
          setFilter((f) => ({ ...f, search: searchDraft }));
        }}
        className="flex items-center gap-2"
      >
        <span className="relative flex-1">
          <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input
            value={searchDraft}
            onChange={(e) => setSearchDraft(e.target.value)}
            inputMode="search"
            placeholder="Consumer number / ref no"
            className="h-11 w-full rounded-xl border border-slate-200 bg-white pl-9 pr-3 text-sm outline-none placeholder:text-slate-400 focus:border-brand-400"
          />
        </span>
        <button
          type="button"
          onClick={() => {
            haptic();
            setDatesOpen((v) => !v);
          }}
          aria-label="Date range"
          className={cn(
            'grid size-11 shrink-0 place-items-center rounded-xl border transition-colors',
            datesOpen ? 'border-brand-400 bg-brand-50 text-brand-600' : 'border-slate-200 bg-white text-slate-500',
          )}
        >
          <SlidersHorizontal className="size-[18px]" />
        </button>
      </form>

      {datesOpen && (
        <div className="grid grid-cols-2 gap-2 rounded-card border border-slate-200/70 bg-white p-3 shadow-card animate-rise">
          <label className="text-xs font-medium text-slate-500">
            From
            <input
              type="date"
              value={filter.fromDate}
              max={filter.toDate}
              onChange={(e) => setFilter((f) => ({ ...f, fromDate: e.target.value }))}
              className="mt-1 h-10 w-full rounded-lg border border-slate-200 px-2 text-sm text-slate-800 outline-none focus:border-brand-400"
            />
          </label>
          <label className="text-xs font-medium text-slate-500">
            To
            <input
              type="date"
              value={filter.toDate}
              max={TODAY}
              onChange={(e) => setFilter((f) => ({ ...f, toDate: e.target.value }))}
              className="mt-1 h-10 w-full rounded-lg border border-slate-200 px-2 text-sm text-slate-800 outline-none focus:border-brand-400"
            />
          </label>
        </div>
      )}

      {/* Status is the filter people actually use — one tap, always visible. */}
      <div className="-mx-4 flex gap-2 overflow-x-auto px-4 pb-1 no-scrollbar">
        {STATUS_CHIPS.map((c) => (
          <button
            key={c.label}
            type="button"
            onClick={() => setStatus(c.value)}
            className={cn(
              'shrink-0 rounded-full px-3.5 py-1.5 text-xs font-semibold transition-colors',
              filter.status === c.value
                ? 'bg-brand-600 text-white'
                : 'border border-slate-200 bg-white text-slate-500',
            )}
          >
            {c.label}
          </button>
        ))}
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {isFetching ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-10 rounded-lg" />)}
          </div>
        ) : rows.length === 0 ? (
          <div className="px-6 py-12 text-center">
            <p className="text-sm font-medium text-slate-500">No transactions found</p>
            <p className="mt-1 text-xs text-slate-400">Try a wider date range or clear the filters.</p>
          </div>
        ) : (
          rows.map((r, i) => (
            <TxnRow
              key={r.TransactionMasterID ?? r.TxnID ?? i}
              row={r}
              first={i === 0}
              names={serviceNames}
              open={openRow === (r.TransactionMasterID ?? r.TxnID ?? i)}
              onToggle={() => {
                haptic();
                const id = r.TransactionMasterID ?? r.TxnID ?? i;
                setOpenRow((cur) => (cur === id ? null : id));
              }}
            />
          ))
        )}
      </div>
    </div>
  );
}

function TxnRow({
  row, first, open, onToggle, names,
}: {
  row: TransactionRow;
  first: boolean;
  open: boolean;
  onToggle: () => void;
  names: ServiceNames;
}) {
  const title = txnTitle(row, names);
  const subtitle = txnSubtitle(row, names);
  const service = txnService(row, names);
  const note = txnNote(row);

  return (
    <div className={cn(!first && 'border-t border-slate-100')}>
      <button type="button" onClick={onToggle} className="flex w-full items-center gap-3 px-4 py-3 text-left active:bg-slate-50">
        <span className="min-w-0 flex-1">
          <span className="block truncate text-sm font-semibold text-slate-800">{title}</span>
          <span className="block truncate text-xs text-slate-400">
            {[subtitle, txnWhen(row.DateTime)].filter(Boolean).join(' · ')}
          </span>
        </span>

        <span className="shrink-0 text-right">
          <span className="block font-display text-sm font-bold tabular-nums text-slate-900">
            {inr(row.Amount ?? row.DebitAmount ?? 0)}
          </span>
          <span className={cn('mt-0.5 inline-block rounded-full px-2 py-0.5 text-[10px] font-semibold', statusTone(row.Status))}>
            {row.Status ?? '—'}
          </span>
        </span>

        <ChevronDown className={cn('size-4 shrink-0 text-slate-300 transition-transform', open && 'rotate-180')} />
      </button>

      {/* The IDs you only need when raising a complaint — hidden until asked for. */}
      {open && (
        <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1.5 border-t border-slate-100 bg-slate-50/60 px-4 py-3 text-xs">
          <dt className="text-slate-400">Date</dt>
          <dd className="text-slate-700">{dateTime(row.DateTime)}</dd>
          <dt className="text-slate-400">Service</dt>
          <dd className="truncate text-slate-700">{service || '—'}</dd>
          {row.DMT_Bank ? (
            <>
              <dt className="text-slate-400">Bank</dt>
              <dd className="truncate text-slate-700">{String(row.DMT_Bank)}</dd>
            </>
          ) : null}
          <dt className="text-slate-400">Txn ID</dt>
          <dd className="truncate font-mono text-slate-700">{row.TransactionMasterID ?? row.TxnID ?? '—'}</dd>
          <dt className="text-slate-400">Ref no</dt>
          <dd className="truncate font-mono text-slate-700">{txnRef(row) || '—'}</dd>
          {note ? (
            <>
              <dt className="text-slate-400">Remark</dt>
              <dd className="text-slate-700">{note}</dd>
            </>
          ) : null}
          {row.FirmName ? (
            <>
              <dt className="text-slate-400">Firm</dt>
              <dd className="truncate text-slate-700">{row.FirmName}</dd>
            </>
          ) : null}
        </dl>
      )}
    </div>
  );
}
