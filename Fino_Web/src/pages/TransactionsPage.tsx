import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import { Search } from 'lucide-react';
import { selectReport, type ReportFilter, type TransactionRow } from '@/api/transactionReport.api';
import { apiDate, dateTime, inr } from '@/lib/format';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/Badge';
import { DataTable, type Column } from '@/components/ui/DataTable';
import { useIsMobileApp } from '@/lib/native';
import MobileTransactionsPage from '@/pages/mobile/MobileTransactionsPage';

const TODAY = apiDate(new Date());
const WEEK_AGO = apiDate(subDays(new Date(), 7));

const COLUMNS: Column<TransactionRow>[] = [
  { key: 'id', header: 'Txn ID', render: (r) => r.TransactionMasterID ?? r.TxnID ?? '—' },
  { key: 'DateTime', header: 'Date', render: (r) => dateTime(r.DateTime) },
  {
    key: 'service',
    header: 'Service',
    render: (r) => r.ServiceType ?? r.OperatorName ?? (r.ServiceID != null ? `#${r.ServiceID}` : '—'),
  },
  { key: 'ConsumerNumber', header: 'Consumer', render: (r) => r.ConsumerNumber ?? '—' },
  { key: 'Amount', header: 'Amount', align: 'right', render: (r) => inr(r.Amount ?? r.DebitAmount ?? 0) },
  { key: 'Status', header: 'Status', render: (r) => <StatusBadge status={r.Status} /> },
  { key: 'OperatorRefNo', header: 'Ref No', render: (r) => r.OperatorRefNo ?? '—' },
];

export default function TransactionsPage() {
  const isMobile = useIsMobileApp();
  const [draft, setDraft] = useState<ReportFilter>({
    fromDate: WEEK_AGO,
    toDate: TODAY,
    serviceType: '',
    status: '',
    search: '',
  });
  const [applied, setApplied] = useState<ReportFilter>(draft);

  const { data: rows = [], isFetching } = useQuery({
    queryKey: ['txn-report', applied],
    queryFn: () => selectReport(applied),
    // Hooks must stay above the mobile branch below, but the desktop query itself
    // shouldn't fire a second, thrown-away request on a phone.
    enabled: !isMobile,
  });

  const update = (patch: Partial<ReportFilter>) => setDraft((d) => ({ ...d, ...patch }));

  // A seven-column table does not survive a 375px screen — phones get the card list.
  if (isMobile) return <MobileTransactionsPage />;

  return (
    <>
      <PageHeader title="Transactions" subtitle="Search and review transaction history" />

      <Card className="mb-4">
        <CardBody>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setApplied(draft);
            }}
            className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-6"
          >
            <Input
              label="From"
              type="date"
              value={draft.fromDate}
              max={draft.toDate}
              onChange={(e) => update({ fromDate: e.target.value })}
            />
            <Input
              label="To"
              type="date"
              value={draft.toDate}
              max={TODAY}
              onChange={(e) => update({ toDate: e.target.value })}
            />
            <Select label="Service" value={draft.serviceType} onChange={(e) => update({ serviceType: e.target.value })}>
              <option value="">All</option>
              <option value="RECHARGE">Recharge</option>
              <option value="DMT">DMT</option>
              <option value="AEPS">AEPS</option>
              <option value="PAYOUT">Payout</option>
            </Select>
            <Select label="Status" value={draft.status} onChange={(e) => update({ status: e.target.value })}>
              <option value="">All</option>
              <option value="SUCCESS">Success</option>
              <option value="PENDING">Pending</option>
              <option value="FAILED">Failed</option>
              <option value="REFUND">Refund</option>
            </Select>
            <Input
              label="Search"
              placeholder="Consumer / Ref"
              value={draft.search}
              onChange={(e) => update({ search: e.target.value })}
            />
            <div className="flex items-end">
              <Button type="submit" loading={isFetching} className="w-full">
                <Search className="size-4" />
                Search
              </Button>
            </div>
          </form>
        </CardBody>
      </Card>

      <DataTable
        columns={COLUMNS}
        rows={rows}
        loading={isFetching}
        emptyTitle="No transactions found"
        emptyDescription="Try widening the date range or clearing filters."
        rowKey={(r, i) => r.TransactionMasterID ?? r.TxnID ?? i}
      />
    </>
  );
}
