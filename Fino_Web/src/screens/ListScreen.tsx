import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleSlash, ListChecks, Database } from 'lucide-react';
import { fetchList, fetchListPage } from '@/api/list.api';
import { useCsvExport } from '@/lib/exportAll';
import { useClientSearch, filterRows } from '@/lib/useClientSearch';
import { getField } from '@/lib/row';
import { PageHeader } from '@/components/ui/PageHeader';
import { DynamicTable } from '@/components/ui/DynamicTable';
import { cn } from '@/lib/cn';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { ReportActions } from '@/components/report';
import { TableToolbar } from '@/components/ui/TableToolbar';
import { pickIcon } from '@/components/layout/iconMap';

export function ListScreen({
  title,
  endpoint,
  payload,
  subtitle,
}: {
  title: string;
  endpoint: string;
  payload?: Record<string, unknown>;
  subtitle?: string;
}) {
  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['list', endpoint, payload],
    queryFn: () => fetchList(endpoint, payload ?? {}),
  });
  const { query, setQuery, filtered } = useClientSearch(rows);
  const { exporting, runExport } = useCsvExport();

  /**
   * The file holds every record the endpoint has that matches the search box — not the page the
   * screen read. The list is fetched one generous page at a time for the table, so the export
   * collects the rest itself and then applies the same search over the whole set.
   */
  const exportAll = () => runExport({
    filename: title,
    onScreen: filtered,
    fetchAll: (skip, take) => fetchListPage(endpoint, payload ?? {}, skip, take),
    filter: (all) => filterRows(all, query),
  });

  // Derive a light summary strip. "Active" only shown when the data actually has an IsActive flag.
  const stats = useMemo(() => {
    const total = rows.length;
    const hasActiveFlag = rows.some((r) => Object.keys(r).some((k) => k.toLowerCase() === 'isactive'));
    const active = hasActiveFlag ? rows.filter((r) => Boolean(getField(r, 'IsActive'))).length : null;
    return { total, active };
  }, [rows]);

  return (
    <>
      <PageHeader
        title={title}
        subtitle={subtitle}
        icon={pickIcon(title)}
        actions={
          <ReportActions
            total={filtered.length}
            loading={isFetching}
            onRefresh={() => refetch()}
            exportLoading={exporting}
            onExport={exportAll}
            exportDisabled={filtered.length === 0}
          />
        }
      />
      {stats.total > 0 && (
        <div className={cn(STAT_STRIP, "grid-cols-2 lg:grid-cols-3")}>
          <StatTile label="Total records" value={stats.total} icon={Database} tone="bg-brand-50 text-brand-600" />
          {stats.active !== null && (
            <>
              <StatTile label="Active" value={stats.active} icon={ListChecks} tone="bg-success-50 text-success-600" />
              <StatTile label="Inactive" value={stats.total - stats.active} icon={CircleSlash} tone="bg-slate-100 text-slate-500" />
            </>
          )}
        </div>
      )}
      <TableToolbar query={query} onQuery={setQuery} count={filtered.length} onExport={exportAll} />
      <DynamicTable rows={filtered} loading={isFetching} emptyDescription="No records yet — add one to get started." />
    </>
  );
}
