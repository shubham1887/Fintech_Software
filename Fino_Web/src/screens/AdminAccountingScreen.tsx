import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Download, RefreshCw, Search as SearchIcon, SlidersHorizontal } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { inr, apiDate } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { getField } from '@/lib/row';
import { StatRow } from '@/components/report/StatRow';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

interface Filters {
  fromDate: string;
  toDate: string;
  walletId: string;
}

interface Totals {
  opening: number;
  closing: number;
  sale: number;
  purchase: number;
  credit: number;
  debit: number;
  comm: number;
}

function defaultFilters(): Filters {
  // Opens on today: the summary is read as "where do the books stand right now", and a week of
  // opening/closing rows was the wrong answer to that on every visit.
  return { fromDate: apiDate(new Date()), toDate: apiDate(new Date()), walletId: '0' };
}

const num = (r: Row, k: string) => Number(getField(r, k) ?? 0) || 0;

async function fetchReconciliation(f: Filters): Promise<{ rows: Row[]; totals: Row | null }> {
  const res = await apiPost<unknown>('Ledger/AdminReconciliation', {
    FromDate: f.fromDate,
    ToDate: f.toDate,
    WalletID: Number(f.walletId || 0),
    Type: '',
    IsExcelDownload: false,
  });
  if (!res.ok) return { rows: [], totals: null };
  const d = res.data as Record<string, unknown> | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Row, 'data', 'list', 'Data', 'List');
  const totals = Array.isArray(d) ? null : (getField(d as Row, 'Totals', 'totals') as Row | null) ?? null;
  return { rows: Array.isArray(listRaw) ? (listRaw as Row[]) : [], totals };
}

/** A signed money cell — green for gains, red for negatives / dues, muted for zero. */
function Money({ value, positive = 'text-slate-700', negative = 'text-danger-600' }: { value: number; positive?: string; negative?: string }) {
  if (value === 0) return <span className="text-slate-300">—</span>;
  return <span className={`tabular-nums ${value < 0 ? negative : positive}`}>{inr(value)}</span>;
}


/**
 * Admin Accounting — company wallet-wise accounting. One row per wallet over a date range with
 * Opening / Closing balance, Sale / Purchase volume, Credit / Debit movement and Commission
 * earned, plus a flat summary strip and totals. (Ledger/AdminReconciliation.)
 */
export function AdminAccountingScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const [draft, setDraft] = useState<Filters>(defaultFilters);
  const [applied, setApplied] = useState<Filters>(draft);
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: wallets = [] } = useQuery({
    queryKey: ['admin-accounting-wallets'], staleTime: 300_000, queryFn: () => fetchList('Wallet/Select', {}),
  });
  const { data, isFetching, refetch } = useQuery({
    queryKey: ['admin-accounting', applied], queryFn: () => fetchReconciliation(applied),
  });

  const rows = data?.rows ?? [];

  // Prefer the server totals row; otherwise fold the visible rows.
  const totals = useMemo<Totals>(() => {
    if (data?.totals) {
      const t = data.totals;
      return {
        opening: num(t, 'OpeningBalance'), closing: num(t, 'ClosingBalance'),
        sale: num(t, 'Sale'), purchase: num(t, 'Purchase'),
        credit: num(t, 'Credit'), debit: num(t, 'Debit'), comm: num(t, 'Commission'),
      };
    }
    return rows.reduce<Totals>(
      (a, r) => ({
        opening: a.opening + num(r, 'OpeningBalance'), closing: a.closing + num(r, 'ClosingBalance'),
        sale: a.sale + num(r, 'Sale'), purchase: a.purchase + num(r, 'Purchase'),
        credit: a.credit + num(r, 'Credit'), debit: a.debit + num(r, 'Debit'), comm: a.comm + num(r, 'Commission'),
      }),
      { opening: 0, closing: 0, sale: 0, purchase: 0, credit: 0, debit: 0, comm: 0 },
    );
  }, [data, rows]);

  const runSearch = () => setApplied(draft);

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <>
            <Button variant="outline" size="sm" onClick={() => downloadCsv(rows, def.title)} disabled={rows.length === 0}>
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
          <div className="mb-3 flex items-center gap-2 text-sm font-medium text-slate-600">
            <SlidersHorizontal className="size-4 text-brand-600" />
            Filters
          </div>
          <form onSubmit={(e) => { e.preventDefault(); runSearch(); }} className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
            <Select label="Wallet" value={draft.walletId} onChange={(e) => upd({ walletId: e.target.value })}>
              <option value="0">All wallets</option>
              {wallets.map((w, i) => {
                const id = String(getField(w, 'WalletID') ?? i);
                return <option key={id} value={id}>{String(getField(w, 'Name', 'WalletName') ?? id)}</option>;
              })}
            </Select>
            <div className="flex items-end">
              <Button type="submit" loading={isFetching} className="w-full">
                <SearchIcon className="size-4" />
                Search
              </Button>
            </div>
          </form>
        </CardBody>
      </Card>

      {/* Compact wallet-wise summary bar — same seven figures and the same colour coding, in
          roughly half the height, so the wallet table starts above the fold. */}
      <StatRow
        items={[
          { label: 'Opening Balance', value: inr(totals.opening) },
          { label: 'Closing Balance', value: inr(totals.closing) },
          { label: 'Sale', value: inr(totals.sale) },
          { label: 'Purchase', value: inr(totals.purchase) },
          { label: 'Credit', value: inr(totals.credit), tone: 'text-success-600' },
          { label: 'Debit', value: inr(totals.debit), tone: 'text-danger-600' },
          { label: 'Commission', value: inr(totals.comm), tone: 'text-success-600' },
        ]}
      />

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3 text-left">#</th>
                <th className="px-4 py-3 text-left">Wallet</th>
                <th className="px-4 py-3 text-right">Opening Balance</th>
                <th className="px-4 py-3 text-right">Closing Balance</th>
                <th className="px-4 py-3 text-right">Sale</th>
                <th className="px-4 py-3 text-right">Purchase</th>
                <th className="px-4 py-3 text-right">Credit</th>
                <th className="px-4 py-3 text-right">Debit</th>
                <th className="px-4 py-3 text-right">Commission</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 6 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 9 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[100px]" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={9}>
                    <EmptyState title="No accounting data" description="Change the date range / wallet and try again." />
                  </td>
                </tr>
              ) : (
                rows.map((r, i) => (
                  <tr key={i} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                    <td className="px-3 py-3 tabular-nums text-slate-500">{i + 1}</td>
                    <td className="px-4 py-3 font-medium text-slate-700">{String(getField(r, 'WalletName') || '—')}</td>
                    <td className="px-4 py-3 text-right"><Money value={num(r, 'OpeningBalance')} /></td>
                    <td className="px-4 py-3 text-right"><Money value={num(r, 'ClosingBalance')} /></td>
                    <td className="px-4 py-3 text-right"><Money value={num(r, 'Sale')} /></td>
                    <td className="px-4 py-3 text-right"><Money value={num(r, 'Purchase')} /></td>
                    <td className="px-4 py-3 text-right"><Money value={num(r, 'Credit')} positive="text-success-600" /></td>
                    <td className="px-4 py-3 text-right"><Money value={num(r, 'Debit')} positive="text-danger-600" /></td>
                    <td className="px-4 py-3 text-right"><Money value={num(r, 'Commission')} positive="text-success-600" /></td>
                  </tr>
                ))
              )}
            </tbody>
            {rows.length > 0 && !isFetching && (
              <tfoot>
                <tr className="border-t-2 border-slate-200 bg-slate-50/80 font-semibold text-slate-800">
                  <td className="px-3 py-3" colSpan={2}>Total</td>
                  <td className="px-4 py-3 text-right tabular-nums">{inr(totals.opening)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{inr(totals.closing)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{inr(totals.sale)}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{inr(totals.purchase)}</td>
                  <td className="px-4 py-3 text-right tabular-nums text-success-600">{inr(totals.credit)}</td>
                  <td className="px-4 py-3 text-right tabular-nums text-danger-600">{inr(totals.debit)}</td>
                  <td className="px-4 py-3 text-right tabular-nums text-success-600">{inr(totals.comm)}</td>
                </tr>
              </tfoot>
            )}
          </table>
        </div>
      </div>
    </>
  );
}
