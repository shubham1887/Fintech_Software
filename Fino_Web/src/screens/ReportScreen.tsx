import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Download, Search } from 'lucide-react';
import { fetchReport, defaultReportFilters, type ReportFilters } from '@/api/reports.api';
import { exportToCsv } from '@/lib/csv';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { DynamicTable } from '@/components/ui/DynamicTable';
import { pickIcon } from '@/components/layout/iconMap';

export function ReportScreen({
  title,
  endpoint,
  subtitle,
}: {
  title: string;
  endpoint: string;
  subtitle?: string;
}) {
  const [draft, setDraft] = useState<ReportFilters>(defaultReportFilters);
  const [applied, setApplied] = useState<ReportFilters>(draft);

  const { data: rows = [], isFetching, isError, error } = useQuery({
    queryKey: ['report', endpoint, applied],
    queryFn: () => fetchReport(endpoint, applied),
  });

  const upd = (patch: Partial<ReportFilters>) => setDraft((d) => ({ ...d, ...patch }));

  return (
    <>
      <PageHeader
        title={title}
        subtitle={subtitle}
        icon={pickIcon(title)}
        actions={
          <>
            <span className="text-sm text-slate-400">{rows.length} rows</span>
            <Button variant="outline" size="sm" onClick={() => exportToCsv(rows, title)} disabled={rows.length === 0}>
              <Download className="size-4" />
              Export
            </Button>
          </>
        }
      />

      <Card className="mb-4">
        <CardBody>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setApplied(draft);
            }}
            className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-5"
          >
            <Input label="From" type="date" value={draft.fromDate} max={draft.toDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
            <Select label="Status" value={draft.status} onChange={(e) => upd({ status: e.target.value })}>
              <option value="">All</option>
              <option value="SUCCESS">Success</option>
              <option value="PENDING">Pending</option>
              <option value="FAILED">Failed</option>
              <option value="REFUND">Refund</option>
            </Select>
            <Input label="Search" placeholder="Consumer / Ref / User" value={draft.search} onChange={(e) => upd({ search: e.target.value })} />
            <div className="flex items-end">
              <Button type="submit" loading={isFetching} className="w-full">
                <Search className="size-4" />
                Search
              </Button>
            </div>
          </form>
        </CardBody>
      </Card>

      {isError ? (
        <Card>
          <CardBody>
            <p className="text-sm text-danger-600">{(error as Error)?.message || 'Failed to load this report.'}</p>
          </CardBody>
        </Card>
      ) : (
        <DynamicTable rows={rows} loading={isFetching} emptyDescription="Try widening the date range or clearing filters." />
      )}
    </>
  );
}
