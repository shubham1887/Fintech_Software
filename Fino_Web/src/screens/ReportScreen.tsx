import { useState } from 'react';
import { useQuery, keepPreviousData } from '@tanstack/react-query';

import { fetchReportPage, defaultReportFilters, type ReportFilters } from '@/api/reports.api';
import { useCsvExport } from '@/lib/exportAll';
import { ReportActions, ReportFilterBar, ReportPager } from '@/components/report';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
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
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const { exporting, runExport } = useCsvExport();

  const { data, isFetching, isError, error } = useQuery({
    queryKey: ['report', endpoint, applied, page, pageSize],
    queryFn: () => fetchReportPage(endpoint, applied, page * pageSize, pageSize),
    placeholderData: keepPreviousData,
  });

  const rows = data?.rows ?? [];
  const total = data?.count ?? 0;
  const hasMore = data?.hasMore ?? false;

  const upd = (patch: Partial<ReportFilters>) => setDraft((d) => ({ ...d, ...patch }));
  const runSearch = () => { setPage(0); setApplied(draft); };

  return (
    <>
      <PageHeader
        title={title}
        subtitle={subtitle}
        icon={pickIcon(title)}
        actions={
          <ReportActions
            total={total}
            loading={isFetching}
            exportLoading={exporting}
            // The file holds every row the filters matched, not the page on screen.
            onExport={() => runExport({
              filename: title,
              onScreen: rows,
              fetchAll: (skip, take) => fetchReportPage(endpoint, applied, skip, take, true),
            })}
            exportDisabled={total === 0}
          />
        }
      />

      <ReportFilterBar
        onSearch={runSearch}
        onReset={() => { const d = defaultReportFilters(); setDraft(d); setApplied(d); setPage(0); }}
        loading={isFetching}
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
      </ReportFilterBar>

      {isError ? (
        <Card>
          <CardBody>
            <p className="text-sm text-danger-600">{(error as Error)?.message || 'Failed to load this report.'}</p>
          </CardBody>
        </Card>
      ) : (
        <>
          <DynamicTable rows={rows} loading={isFetching} emptyDescription="Try widening the date range or clearing filters." />
          {total > 0 && (
            <ReportPager
              page={page}
              pageSize={pageSize}
              total={total}
              hasMore={hasMore}
              loading={isFetching}
              onPage={setPage}
              onPageSize={(n) => { setPageSize(n); setPage(0); }}
            />
          )}
        </>
      )}
    </>
  );
}
