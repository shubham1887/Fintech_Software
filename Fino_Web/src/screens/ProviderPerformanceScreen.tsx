import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { Activity, CheckCircle2, Percent, XCircle } from 'lucide-react';
import { fetchList } from '@/api/list.api';
import { apiDate, num } from '@/lib/format';
import { getField } from '@/lib/row';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { cn } from '@/lib/cn';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { DynamicTable } from '@/components/ui/DynamicTable';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface PerfRow {
  ProviderID: number;
  TotalCount: number;
  SuccessCount: number;
  FailureCount: number;
  PendingCount: number;
  RefundCount: number;
  SuccessRate: number;
  SuccessAmount: number;
  TotalAmount: number;
}

/**
 * Provider-wise performance — chart + table. Mirrors legacy ProviderWisePerformanceGraph.js
 * but renders the success/failure mix as a designed bar chart with KPI tiles. Data from
 * TransactionProcessing/AllProviderWisePerformance ({ProviderID, *Count, SuccessRate, *Amount});
 * provider names resolved via ProviderMaster/GetAll.
 */
export function ProviderPerformanceScreen({ def }: CustomScreenProps) {
  const [fromDate, setFromDate] = useState(() => apiDate(subDays(new Date(), 7)));
  const [toDate, setToDate] = useState(() => apiDate(new Date()));
  const [applied, setApplied] = useState({ fromDate, toDate });

  const { data: rows = [], isFetching } = useQuery({
    queryKey: ['provider-perf', applied],
    queryFn: () =>
      fetchList('TransactionProcessing/AllProviderWisePerformance', {
        UserID: 0,
        FromDate: applied.fromDate,
        ToDate: applied.toDate,
        PageNumber: 1,
        PageSize: 200,
      }) as Promise<unknown> as Promise<PerfRow[]>,
  });

  const { data: providers = [] } = useQuery({
    queryKey: ['providers-min'],
    queryFn: () => fetchList('ProviderMaster/GetAll', { PageNumber: 1, PageSize: 500, IsActive: true }),
    staleTime: 5 * 60_000,
  });

  const nameById = useMemo(() => {
    const m = new Map<number, string>();
    for (const p of providers) {
      const id = Number(getField(p, 'ProviderID', 'ID'));
      const name = String(getField(p, 'ProviderName', 'Name') ?? `Provider ${id}`);
      if (!Number.isNaN(id)) m.set(id, name);
    }
    return m;
  }, [providers]);

  const chartData = useMemo(
    () =>
      rows.map((r) => ({
        name: nameById.get(r.ProviderID) ?? `#${r.ProviderID}`,
        Success: r.SuccessCount ?? 0,
        Failed: r.FailureCount ?? 0,
      })),
    [rows, nameById],
  );

  const totals = useMemo(() => {
    const total = rows.reduce((s, r) => s + (r.TotalCount ?? 0), 0);
    const success = rows.reduce((s, r) => s + (r.SuccessCount ?? 0), 0);
    const failed = rows.reduce((s, r) => s + (r.FailureCount ?? 0), 0);
    const rate = total > 0 ? Math.round((success / total) * 1000) / 10 : 0;
    return { total, success, failed, rate };
  }, [rows]);

  // Table rows with provider names + a percent string.
  const tableRows = useMemo(
    () =>
      rows.map((r) => ({
        Provider: nameById.get(r.ProviderID) ?? `#${r.ProviderID}`,
        Total: r.TotalCount,
        Success: r.SuccessCount,
        Failed: r.FailureCount,
        Pending: r.PendingCount,
        Refund: r.RefundCount,
        'Success %': `${r.SuccessRate ?? 0}%`,
        SuccessAmount: r.SuccessAmount,
        TotalAmount: r.TotalAmount,
      })),
    [rows, nameById],
  );

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon('Performance')}
        actions={
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setApplied({ fromDate, toDate });
            }}
            className="flex items-end gap-2"
          >
            <Input label="From" type="date" value={fromDate} max={toDate} onChange={(e) => setFromDate(e.target.value)} />
            <Input label="To" type="date" value={toDate} max={apiDate(new Date())} onChange={(e) => setToDate(e.target.value)} />
            <Button type="submit" loading={isFetching}>
              Apply
            </Button>
          </form>
        }
      />

      <div className={cn(STAT_STRIP, "grid-cols-2 lg:grid-cols-4")}>
        <StatTile label="Total transactions" value={num(totals.total)} icon={Activity} tone="bg-brand-50 text-brand-600" loading={isFetching} />
        <StatTile label="Successful" value={num(totals.success)} icon={CheckCircle2} tone="bg-success-50 text-success-600" loading={isFetching} />
        <StatTile label="Failed" value={num(totals.failed)} icon={XCircle} tone="bg-danger-50 text-danger-600" loading={isFetching} />
        <StatTile label="Success rate" value={`${totals.rate}%`} icon={Percent} tone="bg-info-50 text-info-600" loading={isFetching} />
      </div>

      <Card className="mb-5">
        <CardHeader>
          <CardTitle>Success vs failed by provider</CardTitle>
          <span className="text-xs text-slate-400">Transaction counts</span>
        </CardHeader>
        <CardBody>
          {chartData.length === 0 ? (
            <div className="grid h-72 place-items-center text-sm text-slate-400">
              No provider activity for the selected range.
            </div>
          ) : (
            <div className="h-72 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData} margin={{ top: 8, right: 8, left: -8, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#eef2f7" vertical={false} />
                  <XAxis dataKey="name" tickLine={false} axisLine={false} tick={{ fontSize: 12, fill: '#64748b' }} />
                  <YAxis tickLine={false} axisLine={false} tick={{ fontSize: 12, fill: '#94a3b8' }} width={48} />
                  <Tooltip contentStyle={{ borderRadius: 12, border: '1px solid #e2e8f0', fontSize: 13 }} />
                  <Legend iconType="circle" wrapperStyle={{ fontSize: 12 }} />
                  <Bar dataKey="Success" fill="#10b981" radius={[6, 6, 0, 0]} maxBarSize={28} />
                  <Bar dataKey="Failed" fill="#f43f5e" radius={[6, 6, 0, 0]} maxBarSize={28} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}
        </CardBody>
      </Card>

      <DynamicTable rows={tableRows} loading={isFetching} emptyTitle="No performance data" emptyDescription="Adjust the date range to see provider activity." />
    </>
  );
}
