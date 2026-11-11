import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { subDays, startOfMonth } from 'date-fns';
import {
  ArrowDownLeft, ArrowUpRight, Download, Printer, RefreshCw, Scale,
  Search as SearchIcon, SlidersHorizontal, TrendingUp, Wallet,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateTime, apiDate } from '@/lib/format';
import { useCsvExport } from '@/lib/exportAll';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useBrand } from '@/theme/BrandProvider';
import { StatRow } from '@/components/report/StatRow';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { Modal } from '@/components/ui/Modal';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

// The company statement can be long over a wide range; we pull one generous window so the
// KPI strip and running-balance totals are accurate, then paginate on the client.
const FETCH_CAP = 3000;
const PAGE_SIZES = [15, 25, 50, 100];

interface Filters {
  fromDate: string;
  toDate: string;
  side: '' | 'Credit' | 'Debit';
  criteria: '' | 'TransactionID' | 'ConsumerNumber';
  criteriaText: string;
}

function defaultFilters(): Filters {
  return {
    fromDate: apiDate(subDays(new Date(), 7)),
    toDate: apiDate(new Date()),
    side: '',
    criteria: '',
    criteriaText: '',
  };
}

const num = (r: Row, k: string) => Number(getField(r, k) ?? 0) || 0;
/** 'C' for credit, 'D' for debit — tolerant of "Credit"/"CR" / "Debit"/"DR". */
const sideOf = (r: Row): 'C' | 'D' | '' => {
  const s = String(getField(r, 'AmountCreditDebit') ?? '').trim().toUpperCase();
  return s.startsWith('C') ? 'C' : s.startsWith('D') ? 'D' : '';
};

interface LedgerData {
  rows: Row[];
  count: number;
}

/**
 * One window of the statement. The screen reads a window it can page through in the browser; an
 * export asks for every row the filters matched (`IsExcelDownload`, which the service honours by
 * skipping its own paging) rather than for the window on screen.
 */
async function fetchLedger(f: Filters, skip = 0, take = FETCH_CAP, forExport = false): Promise<LedgerData> {
  const res = await apiPost<unknown>('TransactionDetails/SelectAdminAccounting', {
    TakeCount: take,
    SkipCount: skip,
    IsExcelDownload: forExport,
    FromDate: f.fromDate,
    ToDate: f.toDate,
    AmountCreditDebit: f.side,
    Criteria: f.criteria,
    CriteriaText: f.criteriaText,
    Status: 'ALL',
    ReportBy: 'Inline',
    UserID: 0,
  });
  if (!res.ok) return { rows: [], count: 0 };
  const d = res.data as Row | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Row, 'list', 'List', 'Data', 'Items');
  const rows = Array.isArray(listRaw) ? (listRaw as Row[]) : [];
  const count = Array.isArray(d) ? rows.length : Number(getField(d as Row, 'Count', 'count') ?? rows.length);
  return { rows, count };
}

interface Totals {
  debit: number;
  credit: number;
  commission: number;
  gst: number;
  tds: number;
  opening: number;
  closing: number;
  count: number;
}

/** One KPI in the summary strip — label, value, an accent icon and a subtle tone. */

/**
 * Admin Ledger — the company's own account statement rendered as a true double-entry ledger:
 * separate Debit / Credit columns with a running Balance, a period summary strip (opening →
 * closing, credit / debit movement, commission, GST / TDS) and a printable statement. Backed by
 * TransactionDetails/SelectAdminAccounting (admin / support scoped, company wallet).
 */
export function AdminLedgerScreen({ def }: CustomScreenProps) {
  const brand = useBrand();
  const [draft, setDraft] = useState<Filters>(defaultFilters);
  const [applied, setApplied] = useState<Filters>(draft);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  const [detail, setDetail] = useState<Row | null>(null);
  const { exporting, runExport } = useCsvExport();
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data, isFetching, refetch } = useQuery({
    queryKey: ['admin-ledger', applied],
    queryFn: () => fetchLedger(applied),
  });

  // Server returns newest-first; keep that for display (a statement reads latest at the top).
  const rows = data?.rows ?? [];
  const count = data?.count ?? 0;
  const truncated = count > rows.length;

  const totals = useMemo<Totals>(() => {
    const t = rows.reduce<Totals>(
      (a, r) => {
        const s = sideOf(r);
        const amt = num(r, 'Amount');
        return {
          debit: a.debit + (s === 'D' ? amt : 0),
          credit: a.credit + (s === 'C' ? amt : 0),
          commission: a.commission + num(r, 'NetCommission'),
          gst: a.gst + num(r, 'GST'),
          tds: a.tds + num(r, 'TDS'),
          opening: a.opening, closing: a.closing, count: a.count + 1,
        };
      },
      { debit: 0, credit: 0, commission: 0, gst: 0, tds: 0, opening: 0, closing: 0, count: 0 },
    );
    // Rows are newest-first: latest closing is the first row, earliest opening is the last row.
    t.closing = rows.length ? num(rows[0], 'ClosingBalance') : 0;
    t.opening = rows.length ? num(rows[rows.length - 1], 'OpeningBalance') : 0;
    return t;
  }, [rows]);

  const net = totals.credit - totals.debit;

  // Client-side pagination over the fetched window.
  const pageRows = useMemo(() => rows.slice(page * pageSize, page * pageSize + pageSize), [rows, page, pageSize]);
  const from = rows.length === 0 ? 0 : page * pageSize + 1;
  const to = Math.min((page + 1) * pageSize, rows.length);

  const runSearch = () => { setPage(0); setApplied(draft); };
  const setRange = (fromDate: string, toDate: string) => {
    const next = { ...draft, fromDate, toDate };
    setDraft(next);
    setPage(0);
    setApplied(next);
  };

  const today = apiDate(new Date());
  const isRange = (fromDate: string) => applied.fromDate === fromDate && applied.toDate === today;
  const printStatement = () =>
    printLedger(rows, totals, net, applied, brand.appName);

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <>
            <span className="hidden text-sm text-slate-400 sm:inline">{count} entries</span>
            <Button variant="outline" size="sm" onClick={printStatement} disabled={rows.length === 0}>
              <Printer className="size-4" />
              Print
            </Button>
            <Button
              variant="outline"
              size="sm"
              loading={exporting}
              disabled={rows.length === 0}
              // The whole statement for these filters, not the window the screen is paging through.
              onClick={() => runExport({
                filename: def.title,
                onScreen: rows,
                fetchAll: (skip, take) => fetchLedger(applied, skip, take, true),
              })}
            >
              <Download className="size-4" />
              Export
            </Button>
            <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
              <RefreshCw className="size-4" />
            </Button>
          </>
        }
      />

      <Card className="mb-4">
        <CardBody>
          <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2 text-sm font-medium text-slate-600">
              <SlidersHorizontal className="size-4 text-brand-600" />
              Filters
            </div>
            {/* Quick date presets. */}
            <div className="flex flex-wrap gap-1.5">
              {([
                ['Today', apiDate(new Date())],
                ['7 days', apiDate(subDays(new Date(), 7))],
                ['30 days', apiDate(subDays(new Date(), 30))],
                ['This month', apiDate(startOfMonth(new Date()))],
              ] as const).map(([label, fromDate]) => (
                <button
                  key={label}
                  type="button"
                  onClick={() => setRange(fromDate, today)}
                  className={cn(
                    'rounded-full px-3 py-1 text-xs font-medium ring-1 ring-inset transition-colors',
                    isRange(fromDate)
                      ? 'bg-brand-50 text-brand-700 ring-brand-500/25'
                      : 'text-slate-500 ring-slate-200 hover:bg-slate-50',
                  )}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>

          <form onSubmit={(e) => { e.preventDefault(); runSearch(); }} className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />

            {/* Credit / Debit segmented control. */}
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-500">Entry type</label>
              <div className="flex overflow-hidden rounded-lg border border-slate-200">
                {([['', 'All'], ['Credit', 'Credit'], ['Debit', 'Debit']] as const).map(([value, label]) => (
                  <button
                    key={label}
                    type="button"
                    onClick={() => upd({ side: value })}
                    className={cn(
                      'flex-1 px-2 py-2 text-sm font-medium transition-colors',
                      draft.side === value
                        ? value === 'Credit' ? 'bg-success-50 text-success-600'
                          : value === 'Debit' ? 'bg-danger-50 text-danger-600'
                            : 'bg-brand-50 text-brand-700'
                        : 'bg-white text-slate-500 hover:bg-slate-50',
                    )}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>

            <Select label="Search by" value={draft.criteria} onChange={(e) => upd({ criteria: e.target.value as Filters['criteria'], criteriaText: '' })}>
              <option value="">— none —</option>
              <option value="TransactionID">Transaction ID</option>
              <option value="ConsumerNumber">Consumer / Account No.</option>
            </Select>
            {draft.criteria && (
              <Input
                label={draft.criteria === 'TransactionID' ? 'Transaction ID' : 'Consumer / Account No.'}
                placeholder="Enter value"
                value={draft.criteriaText}
                onChange={(e) => upd({ criteriaText: e.target.value })}
              />
            )}

            <div className="flex items-end">
              <Button type="submit" loading={isFetching} className="w-full">
                <SearchIcon className="size-4" />
                Search
              </Button>
            </div>
          </form>
        </CardBody>
      </Card>

      {/* Period summary — the four bounding figures, then the derived ones. Both compact bars:
          this is context for the ledger below, and it used to cost two card strips of height. */}
      <StatRow
        columns={4}
        items={[
          { label: 'Opening Balance', value: inr(totals.opening), icon: <Wallet />, sub: 'Start of period' },
          { label: 'Total Credit', value: inr(totals.credit), tone: 'text-success-600', icon: <ArrowDownLeft />, sub: `${rows.filter((r) => sideOf(r) === 'C').length} entries` },
          { label: 'Total Debit', value: inr(totals.debit), tone: 'text-danger-600', icon: <ArrowUpRight />, sub: `${rows.filter((r) => sideOf(r) === 'D').length} entries` },
          { label: 'Closing Balance', value: inr(totals.closing), icon: <Scale />, sub: 'End of period' },
        ]}
      />
      <StatRow
        columns={4}
        items={[
          { label: 'Net Movement', value: inr(net), tone: net < 0 ? 'text-danger-600' : 'text-success-600', icon: <TrendingUp />, sub: 'Credit − Debit' },
          { label: 'Commission', value: inr(totals.commission), tone: 'text-success-600', icon: <TrendingUp />, sub: 'Net of TDS' },
          { label: 'GST', value: inr(totals.gst), icon: <TrendingUp /> },
          { label: 'TDS', value: inr(totals.tds), icon: <TrendingUp /> },
        ]}
      />

      {truncated && (
        <div className="mb-4 rounded-card border border-warning-500/20 bg-warning-50 px-4 py-3 text-sm text-warning-600">
          Showing the latest {rows.length.toLocaleString('en-IN')} of {count.toLocaleString('en-IN')} entries. Narrow the
          date range for a complete period statement.
        </div>
      )}

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3 text-left">#</th>
                <th className="px-4 py-3 text-left">Date &amp; Time</th>
                <th className="px-4 py-3 text-left">Particulars</th>
                <th className="px-4 py-3 text-left">Txn / Ref</th>
                <th className="px-4 py-3 text-right">Debit</th>
                <th className="px-4 py-3 text-right">Credit</th>
                <th className="px-4 py-3 text-right">Commission</th>
                <th className="px-4 py-3 text-right">Balance</th>
                <th className="px-4 py-3 text-left">Status</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 8 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 9 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[110px]" /></td>
                    ))}
                  </tr>
                ))
              ) : pageRows.length === 0 ? (
                <tr>
                  <td colSpan={9}>
                    <EmptyState title="No ledger entries" description="Change the date range / filters and try again." />
                  </td>
                </tr>
              ) : (
                pageRows.map((r, i) => {
                  const s = sideOf(r);
                  const amt = num(r, 'Amount');
                  const particular = String(getField(r, 'DOCType') || getField(r, 'OperatorName') || '—');
                  const service = String(getField(r, 'OperatorName') || '');
                  return (
                    <tr
                      key={i}
                      onClick={() => setDetail(r)}
                      className="cursor-pointer border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60"
                    >
                      <td className="px-3 py-3 tabular-nums text-slate-400">{page * pageSize + i + 1}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{dateTime(String(getField(r, 'DOC') ?? ''))}</td>
                      <td className="px-4 py-3">
                        <div className="font-medium text-slate-700">{particular}</div>
                        {service && service !== particular && <div className="text-xs text-slate-400">{service}</div>}
                      </td>
                      <td className="px-4 py-3 font-medium tabular-nums text-slate-600">{String(getField(r, 'ReferenceNo') || '—')}</td>
                      <td className="px-4 py-3 text-right tabular-nums">
                        {s === 'D' ? <span className="text-danger-600">{inr(amt)}</span> : <span className="text-slate-300">—</span>}
                      </td>
                      <td className="px-4 py-3 text-right tabular-nums">
                        {s === 'C' ? <span className="text-success-600">{inr(amt)}</span> : <span className="text-slate-300">—</span>}
                      </td>
                      <td className="px-4 py-3 text-right tabular-nums text-slate-500">
                        {num(r, 'NetCommission') ? inr(num(r, 'NetCommission')) : <span className="text-slate-300">—</span>}
                      </td>
                      <td className="px-4 py-3 text-right font-semibold tabular-nums text-slate-800">{inr(num(r, 'ClosingBalance'))}</td>
                      <td className="px-4 py-3"><StatusBadge status={String(getField(r, 'TransactionDescription') ?? '')} /></td>
                    </tr>
                  );
                })
              )}
            </tbody>
            {pageRows.length > 0 && !isFetching && (
              <tfoot>
                <tr className="border-t-2 border-slate-200 bg-slate-50/80 font-semibold text-slate-800">
                  <td className="px-4 py-3" colSpan={4}>Period total</td>
                  <td className="px-4 py-3 text-right tabular-nums text-danger-600">{inr(totals.debit)}</td>
                  <td className="px-4 py-3 text-right tabular-nums text-success-600">{inr(totals.credit)}</td>
                  <td className="px-4 py-3 text-right tabular-nums text-slate-600">{inr(totals.commission)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{inr(totals.closing)}</td>
                  <td className="px-4 py-3" />
                </tr>
              </tfoot>
            )}
          </table>
        </div>

        <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 px-4 py-3 text-sm text-slate-500">
          <div className="flex items-center gap-2">
            <span>Rows per page</span>
            <select
              className="rounded-lg border border-slate-200 bg-white px-2 py-1 text-sm text-slate-700 outline-none focus:border-brand-400 focus:ring-2 focus:ring-brand-500/15"
              value={pageSize}
              disabled={isFetching}
              onChange={(e) => { setPageSize(Number(e.target.value)); setPage(0); }}
            >
              {PAGE_SIZES.map((n) => <option key={n} value={n}>{n}</option>)}
            </select>
          </div>
          <span>{from}–{to} of {rows.length}</span>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" disabled={page === 0 || isFetching} onClick={() => setPage(page - 1)}>Prev</Button>
            <Button variant="outline" size="sm" disabled={to >= rows.length || isFetching} onClick={() => setPage(page + 1)}>Next</Button>
          </div>
        </div>
      </div>

      <Modal open={Boolean(detail)} onClose={() => setDetail(null)} title="Ledger entry" size="lg">
        {detail && (
          <div className="grid grid-cols-1 gap-x-6 gap-y-0 sm:grid-cols-2">
            {LEDGER_DETAIL_FIELDS.map(({ key, label, money, date }) => {
              const v = getField(detail, key);
              return (
                <div key={key} className="flex justify-between gap-3 border-b border-slate-100 py-1.5 text-sm">
                  <span className="shrink-0 text-slate-500">{label}</span>
                  <span className="truncate text-right font-medium text-slate-800">
                    {v === null || v === undefined || v === '' ? '—'
                      : money ? inr(Number(v) || 0)
                        : date ? dateTime(String(v))
                          : String(v)}
                  </span>
                </div>
              );
            })}
          </div>
        )}
      </Modal>
    </>
  );
}

const LEDGER_DETAIL_FIELDS: { key: string; label: string; money?: boolean; date?: boolean }[] = [
  { key: 'TransactionDetailID', label: 'Entry ID' },
  { key: 'DOC', label: 'Date & Time', date: true },
  { key: 'PersonName', label: 'Entity Name' },
  { key: 'AliasName', label: 'Entity Identifier' },
  { key: 'DOCType', label: 'Product' },
  { key: 'OperatorName', label: 'Service' },
  { key: 'AmountCreditDebit', label: 'Entry Type' },
  { key: 'ReferenceNo', label: 'Transaction ID' },
  { key: 'ReferenceID', label: 'Reference ID' },
  { key: 'Amount', label: 'Amount', money: true },
  { key: 'Charge', label: 'Charge', money: true },
  { key: 'GST', label: 'GST', money: true },
  { key: 'DiscountAmount', label: 'Commission', money: true },
  { key: 'TDS', label: 'TDS', money: true },
  { key: 'NetCommission', label: 'Net Commission', money: true },
  { key: 'OpeningBalance', label: 'Opening Balance', money: true },
  { key: 'ClosingBalance', label: 'Closing Balance', money: true },
  { key: 'TransactionDescription', label: 'Status' },
];

/** Open a print-friendly company statement in a new window (self-contained HTML). */
function printLedger(rows: Row[], totals: Totals, net: number, f: Filters, appName: string): void {
  const esc = (s: unknown) =>
    String(s ?? '').replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c] as string));
  const money = (n: number) => `₹${(n || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  const body = rows.map((r, i) => {
    const s = sideOf(r);
    const amt = num(r, 'Amount');
    return `<tr>
      <td>${i + 1}</td>
      <td>${esc(dateTime(String(getField(r, 'DOC') ?? '')))}</td>
      <td>${esc(getField(r, 'DOCType') || getField(r, 'OperatorName') || '—')}</td>
      <td>${esc(getField(r, 'ReferenceNo') || '—')}</td>
      <td class="r">${s === 'D' ? money(amt) : ''}</td>
      <td class="r">${s === 'C' ? money(amt) : ''}</td>
      <td class="r">${esc(money(num(r, 'ClosingBalance')))}</td>
    </tr>`;
  }).join('');

  const html = `<!doctype html><html><head><meta charset="utf-8"><title>${esc(appName)} — Account Statement</title>
    <style>
      *{box-sizing:border-box}body{font-family:'Segoe UI',Arial,sans-serif;color:#0f172a;margin:32px;font-size:12px}
      h1{font-size:18px;margin:0}.muted{color:#64748b}
      .head{display:flex;justify-content:space-between;align-items:flex-start;border-bottom:2px solid #0f172a;padding-bottom:12px;margin-bottom:16px}
      .kpis{display:grid;grid-template-columns:repeat(4,1fr);gap:8px;margin-bottom:16px}
      .kpi{border:1px solid #e2e8f0;border-radius:8px;padding:8px 12px}
      .kpi .l{font-size:10px;text-transform:uppercase;letter-spacing:.04em;color:#64748b}
      .kpi .v{font-size:15px;font-weight:700;margin-top:2px}
      table{width:100%;border-collapse:collapse}
      th,td{padding:6px 8px;border-bottom:1px solid #e2e8f0;text-align:left}
      th{background:#f8fafc;font-size:10px;text-transform:uppercase;letter-spacing:.04em;color:#475569}
      td.r,th.r{text-align:right;font-variant-numeric:tabular-nums}
      tfoot td{font-weight:700;border-top:2px solid #0f172a}
      @media print{body{margin:12mm}}
    </style></head><body>
    <div class="head">
      <div><h1>${esc(appName)}</h1><div class="muted">Company Account Statement</div></div>
      <div class="muted" style="text-align:right">
        <div>Period: ${esc(f.fromDate)} to ${esc(f.toDate)}</div>
        <div>Generated: ${esc(dateTime(new Date()))}</div>
        <div>Entries: ${rows.length}</div>
      </div>
    </div>
    <div class="kpis">
      <div class="kpi"><div class="l">Opening Balance</div><div class="v">${money(totals.opening)}</div></div>
      <div class="kpi"><div class="l">Total Credit</div><div class="v">${money(totals.credit)}</div></div>
      <div class="kpi"><div class="l">Total Debit</div><div class="v">${money(totals.debit)}</div></div>
      <div class="kpi"><div class="l">Closing Balance</div><div class="v">${money(totals.closing)}</div></div>
    </div>
    <table>
      <thead><tr><th>#</th><th>Date &amp; Time</th><th>Particulars</th><th>Txn / Ref</th><th class="r">Debit</th><th class="r">Credit</th><th class="r">Balance</th></tr></thead>
      <tbody>${body}</tbody>
      <tfoot><tr><td colspan="4">Period total &nbsp;·&nbsp; Net ${money(net)}</td><td class="r">${money(totals.debit)}</td><td class="r">${money(totals.credit)}</td><td class="r">${money(totals.closing)}</td></tr></tfoot>
    </table>
    <p class="muted" style="margin-top:24px">System-generated statement — no signature required.</p>
    <script>window.onload=function(){window.print()}</script>
    </body></html>`;

  const w = window.open('', '_blank', 'width=980,height=720');
  if (!w) return;
  w.document.write(html);
  w.document.close();
}
