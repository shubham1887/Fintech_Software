import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { parseISO, isValid, format as formatDate } from 'date-fns';
import { Download, SlidersHorizontal, X } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { apiDate, dateTime } from '@/lib/format';
import { exportToCsv } from '@/lib/csv';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { DynamicForm } from '@/components/ui/DynamicForm';
import { DynamicTable } from '@/components/ui/DynamicTable';
import { pickIcon } from '@/components/layout/iconMap';
import type { FieldDef } from './crudTypes';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

const ADD_FIELDS: FieldDef[] = [
  { name: 'WalletID', label: 'Wallet', type: 'select', required: true, optionsEndpoint: 'Wallet/Select', optionValue: 'WalletID', optionLabel: 'Name', hint: 'Which of your wallets to credit' },
  { name: 'Amount', label: 'Amount (₹)', type: 'number', required: true },
];

const LIST_COLUMNS = [
  { key: 'WalletName', label: 'Wallet Name' },
  { key: 'DateTime', label: 'Date' },
  { key: 'OpeningBalance', label: 'Opening Balance' },
  { key: 'Amount', label: 'Amount' },
  { key: 'ClosingBalance', label: 'Closing Balance' },
];

/** The yyyy-MM-dd of a row's DateTime, or '' when it has no usable date. */
function rowDateKey(row: Row): string {
  const raw = getField(row, 'DateTime');
  if (!raw) return '';
  const d = raw instanceof Date ? raw : parseISO(String(raw));
  return isValid(d) && d.getFullYear() > 1 ? formatDate(d, 'yyyy-MM-dd') : '';
}

/**
 * Single Fund → Virtual Balance page: "Add balance" form on top (credits the signed-in
 * user's own wallet), and the list of virtual-balance entries below. The list defaults to
 * today's entries only; a collapsible filter (hidden until the Filter button is clicked)
 * narrows by date and wallet, and Export downloads the shown rows as a spreadsheet.
 */
export function VirtualBalanceScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();

  const today = apiDate(new Date());
  const [showFilter, setShowFilter] = useState(false);
  const [filterDate, setFilterDate] = useState(today);
  const [filterWallet, setFilterWallet] = useState(''); // '' = all wallets

  // WalletName and DateTime come from the API (VirtualBalanceService fills WalletName from
  // SelectAll, so disabled wallets still resolve on historic rows) — no client-side join needed.
  const listQ = useQuery({
    queryKey: ['crud', 'VirtualBalance/GetAll', {}],
    queryFn: () => fetchList('VirtualBalance/GetAll', {}),
  });

  // Wallet dropdown for the filter — same source as the Add-balance form.
  const walletsQ = useQuery({
    queryKey: ['crud', 'Wallet/Select', {}],
    staleTime: 300_000,
    queryFn: () => fetchList('Wallet/Select', {}),
  });

  const allRows = (listQ.data ?? []) as Row[];

  // Default view = today only. Any other date, or a wallet, is shown only once the user filters.
  const rows = useMemo(() => {
    return allRows.filter((r) => {
      if (filterDate && rowDateKey(r) !== filterDate) return false;
      if (filterWallet && String(getField(r, 'WalletID') ?? '') !== filterWallet) return false;
      return true;
    });
  }, [allRows, filterDate, filterWallet]);

  const resetFilter = () => {
    setFilterDate(today);
    setFilterWallet('');
  };

  const exportRows = () =>
    exportToCsv(
      rows.map((r) => ({
        'Wallet Name': getField(r, 'WalletName') ?? '',
        Date: dateTime(getField(r, 'DateTime') as string),
        'Opening Balance': getField(r, 'OpeningBalance') ?? '',
        Amount: getField(r, 'Amount') ?? '',
        'Closing Balance': getField(r, 'ClosingBalance') ?? '',
      })),
      `Virtual Balance ${filterDate || 'all'}`,
    );

  const addMut = useMutation({
    mutationFn: (values: Record<string, unknown>) => apiPost('VirtualBalance/Create', values),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success('Virtual balance credited to your wallet.');
        qc.invalidateQueries({ queryKey: ['crud', 'VirtualBalance/GetAll'] });
        qc.invalidateQueries({ queryKey: ['my-wallets'] });
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
        <h2 className="mb-1 font-display text-base font-bold text-slate-900">Add balance</h2>
        <p className="mb-4 text-sm text-slate-500">
          The amount is credited to your own wallet, from where you can fund-transfer it onward to users.
        </p>
        <DynamicForm
          fields={ADD_FIELDS}
          submitting={addMut.isPending}
          submitLabel="Add balance"
          onSubmit={(v) => addMut.mutate(v)}
        />
      </div>

      <div className="mb-3 flex items-center gap-3">
        <Button
          variant={showFilter ? 'primary' : 'outline'}
          size="sm"
          onClick={() => setShowFilter((s) => !s)}
        >
          <SlidersHorizontal className="size-4" />
          Filter
        </Button>
        <div className="flex-1" />
        <span className="hidden text-sm text-slate-400 sm:inline">{rows.length} rows</span>
        <Button variant="outline" size="sm" onClick={exportRows} disabled={rows.length === 0}>
          <Download className="size-4" />
          Export
        </Button>
      </div>

      {showFilter && (
        <div className="mb-4 rounded-card border border-slate-200/70 bg-white p-4 shadow-card">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <Input
              label="Date"
              type="date"
              value={filterDate}
              onChange={(e) => setFilterDate(e.target.value)}
            />
            <Select
              label="Wallet"
              value={filterWallet}
              onChange={(e) => setFilterWallet(e.target.value)}
            >
              <option value="">All wallets</option>
              {(walletsQ.data ?? []).map((w, i) => {
                const id = String(getField(w as Row, 'WalletID') ?? i);
                return (
                  <option key={id} value={id}>
                    {String(getField(w as Row, 'Name') ?? id)}
                  </option>
                );
              })}
            </Select>
            <div className="flex items-end">
              <Button variant="ghost" size="sm" onClick={resetFilter}>
                <X className="size-4" />
                Reset to today
              </Button>
            </div>
          </div>
        </div>
      )}

      <DynamicTable
        rows={rows}
        loading={listQ.isFetching}
        columns={LIST_COLUMNS}
        emptyDescription={
          filterDate === today && !filterWallet
            ? 'No virtual balance added today — use the form above, or open Filter to see other dates.'
            : 'No entries match the selected filter.'
        }
      />
    </>
  );
}
