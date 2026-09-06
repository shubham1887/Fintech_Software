import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CircleSlash, RefreshCw, ListChecks, Database } from 'lucide-react';
import { fetchList } from '@/api/list.api';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
import { getField } from '@/lib/row';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { DynamicTable } from '@/components/ui/DynamicTable';
import { StatTile } from '@/components/ui/StatTile';
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
          <Button variant="outline" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
        }
      />
      {stats.total > 0 && (
        <div className="mb-5 grid grid-cols-2 gap-4 lg:grid-cols-3">
          <StatTile label="Total records" value={stats.total} icon={Database} tone="bg-brand-50 text-brand-600" />
          {stats.active !== null && (
            <>
              <StatTile label="Active" value={stats.active} icon={ListChecks} tone="bg-success-50 text-success-600" />
              <StatTile label="Inactive" value={stats.total - stats.active} icon={CircleSlash} tone="bg-slate-100 text-slate-500" />
            </>
          )}
        </div>
      )}
      <TableToolbar query={query} onQuery={setQuery} count={filtered.length} onExport={() => exportToCsv(filtered, title)} />
      <DynamicTable rows={filtered} loading={isFetching} emptyDescription="No records yet — add one to get started." />
    </>
  );
}
