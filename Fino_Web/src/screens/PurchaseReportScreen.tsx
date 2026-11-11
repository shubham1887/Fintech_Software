import { useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import { Plus } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { inr, apiDate } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { ReportActions, ReportFilterBar, ReportTable, StatRow } from '@/components/report';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

const LIST_PAGE = { IsActive: true, PageNumber: 1, PageSize: 500 };
const num = (r: Row, k: string) => Number(getField(r, k) ?? 0) || 0;
const str = (r: Row, k: string) => String(getField(r, k) ?? '');

interface Filters {
  fromDate: string;
  toDate: string;
  providerId: string;
  view: 'Month' | 'Date';
}

function defaultFilters(): Filters {
  return { fromDate: apiDate(subDays(new Date(), 30)), toDate: apiDate(new Date()), providerId: '0', view: 'Month' };
}

/** Provider-payment rows (TransactionReport/PurchaseReport) — provider stock we paid for. */
async function fetchPurchases(f: Filters): Promise<Row[]> {
  const res = await apiPost<unknown>('TransactionReport/PurchaseReport', {
    FromDate: f.fromDate,
    ToDate: f.toDate,
    ProviderID: Number(f.providerId || 0),
    GroupBy: f.view,
    IsExcelDownload: false,
  });
  if (!res.ok) return [];
  const d = res.data as Row | unknown[];
  const listRaw = Array.isArray(d) ? d : getField(d as Row, 'list', 'List', 'Data', 'Items');
  return Array.isArray(listRaw) ? (listRaw as Row[]) : [];
}


/**
 * Purchase Report — how much we paid each provider (the manual "Provider Purchases" entries),
 * aggregated per provider for each period. The View toggle switches the period between
 * month-wise ("2026-08") and date-wise ("2026-08-25"). Company books, so Admin/Support only.
 */
export function PurchaseReportScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const qc = useQueryClient();
  const [draft, setDraft] = useState<Filters>(defaultFilters);
  const [applied, setApplied] = useState<Filters>(draft);
  const [addOpen, setAddOpen] = useState(false);
  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: providers = [] } = useQuery({
    queryKey: ['purchase-providers'], staleTime: 300_000, queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAGE),
  });
  const { data: wallets = [] } = useQuery({
    queryKey: ['purchase-wallets'], staleTime: 300_000, queryFn: () => fetchList('Wallet/Select', {}),
  });
  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['purchase-report', applied], queryFn: () => fetchPurchases(applied),
  });

  const summary = useMemo(() => {
    const totalPaid = rows.reduce((a, r) => a + num(r, 'Amount'), 0);
    const payments = rows.reduce((a, r) => a + num(r, 'Payments'), 0);
    const providerCount = new Set(rows.map((r) => str(r, 'ProviderName'))).size;
    return { totalPaid, payments, providerCount, avg: payments > 0 ? totalPaid / payments : 0 };
  }, [rows]);

  const periodLabel = applied.view === 'Date' ? 'Date' : 'Month';
  const runSearch = () => setApplied(draft);
  const resetFilters = () => { const d = defaultFilters(); setDraft(d); setApplied(d); };

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <ReportActions
            total={rows.length}
            loading={isFetching}
            onRefresh={() => refetch()}
            onExport={() => downloadCsv(rows, def.title)}
            exportDisabled={rows.length === 0}
          >
            <Button size="sm" onClick={() => setAddOpen(true)}>
              <Plus className="size-4" />
              Add Purchase
            </Button>
          </ReportActions>
        }
      />

      <ReportFilterBar onSearch={runSearch} onReset={resetFilters} loading={isFetching}>
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
            <Select label="Provider" value={draft.providerId} onChange={(e) => upd({ providerId: e.target.value })}>
              <option value="0">All providers</option>
              {providers.map((p, i) => {
                const id = String(getField(p, 'ProviderID') ?? i);
                return <option key={id} value={id}>{String(getField(p, 'ProviderName', 'Name') ?? id)}</option>;
              })}
            </Select>
            <Select label="View" value={draft.view} onChange={(e) => upd({ view: e.target.value as Filters['view'] })}>
              <option value="Month">Month-wise</option>
              <option value="Date">Date-wise</option>
            </Select>
      </ReportFilterBar>

      <StatRow
        columns={4}
        items={[
          { label: 'Total Paid', value: inr(summary.totalPaid), tone: 'text-slate-900' },
          { label: 'Payments', value: String(summary.payments) },
          { label: 'Providers', value: String(summary.providerCount) },
          { label: 'Avg / Payment', value: inr(summary.avg) },
        ]}
      />

      <ReportTable
        columns={[
          { key: '_serial', label: '#', type: 'serial' },
          { key: 'Period', label: periodLabel, type: 'bold' },
          { key: 'ProviderName', label: 'Provider' },
          { key: 'WalletName', label: 'Wallet' },
          { key: 'Payments', label: 'Payments', type: 'number', total: true },
          { key: 'Amount', label: 'Amount Paid', type: 'money', total: true },
        ]}
        rows={rows}
        loading={isFetching}
        emptyTitle="No provider payments"
        emptyDescription="Nothing was paid to a provider in this range. Record payments on the Provider Purchases screen, or widen the dates."
      />

      <AddPurchaseModal
        open={addOpen}
        providers={providers}
        wallets={wallets}
        onClose={() => setAddOpen(false)}
        onSaved={() => {
          setAddOpen(false);
          void qc.invalidateQueries({ queryKey: ['purchase-report'] });
        }}
      />
    </>
  );
}

/** Record a provider payment — ProviderPurchase/Create. Mirrors the /AddPurchase entry. */
function AddPurchaseModal({
  open, providers, wallets, onClose, onSaved,
}: {
  open: boolean;
  providers: Row[];
  wallets: Row[];
  onClose: () => void;
  onSaved: () => void;
}) {
  const toast = useToast();
  const [providerId, setProviderId] = useState('');
  const [walletId, setWalletId] = useState('');
  const [amount, setAmount] = useState('');
  const [date, setDate] = useState(apiDate(new Date()));
  const [remark, setRemark] = useState('');
  const [busy, setBusy] = useState(false);

  const reset = () => { setProviderId(''); setWalletId(''); setAmount(''); setDate(apiDate(new Date())); setRemark(''); };

  async function submit() {
    if (!providerId) { toast.error('Choose a provider.'); return; }
    // WalletID is an FK to WalletMaster — it must be a real wallet (0 fails the constraint).
    if (!walletId || Number(walletId) <= 0) { toast.error('Choose the wallet the payment came from.'); return; }
    const amt = Number(amount);
    if (!(amt > 0)) { toast.error('Enter an amount greater than 0.'); return; }
    if (!date) { toast.error('Choose the purchase date.'); return; }
    setBusy(true);
    try {
      const res = await apiPost('ProviderPurchase/Create', {
        ProviderID: Number(providerId),
        WalletID: Number(walletId || 0),
        Amount: amt,
        Remark: remark.trim(),
        PurchaseDate: date,
        PurchaseDOCInt: Number(date.replace(/-/g, '')) || 0,
        IsDelete: false,
      });
      if (!res.ok) { toast.error(res.message || 'Could not record the purchase.'); return; }
      toast.success('Purchase recorded.');
      reset();
      onSaved();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not record the purchase.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal open={open} onClose={() => { reset(); onClose(); }} title="Add provider purchase">
      <div className="space-y-4">
        <Select label="Provider" value={providerId} onChange={(e) => setProviderId(e.target.value)}>
          <option value="">— choose provider —</option>
          {providers.map((p, i) => {
            const id = String(getField(p, 'ProviderID') ?? i);
            return <option key={id} value={id}>{String(getField(p, 'ProviderName', 'Name') ?? id)}</option>;
          })}
        </Select>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="Amount (₹)" type="number" min={0} placeholder="0.00" value={amount} onChange={(e) => setAmount(e.target.value)} />
          <Input label="Purchase date" type="date" max={apiDate(new Date())} value={date} onChange={(e) => setDate(e.target.value)} />
        </div>
        <Select label="Wallet" value={walletId} onChange={(e) => setWalletId(e.target.value)}>
          <option value="">— choose wallet —</option>
          {wallets.map((w, i) => {
            const id = String(getField(w, 'WalletID') ?? i);
            return <option key={id} value={id}>{String(getField(w, 'Name', 'WalletName') ?? id)}</option>;
          })}
        </Select>
        <Input label="Remark (optional)" placeholder="Reference / note" value={remark} onChange={(e) => setRemark(e.target.value)} />

        <div className="flex justify-end gap-2 pt-1">
          <Button variant="outline" size="sm" onClick={() => { reset(); onClose(); }}>Cancel</Button>
          <Button size="sm" loading={busy} onClick={submit}>Save purchase</Button>
        </div>
      </div>
    </Modal>
  );
}
