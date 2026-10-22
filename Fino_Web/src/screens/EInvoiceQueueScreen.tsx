import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, CheckCircle2, Clock, Play, RefreshCw, ShieldAlert } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useAuth } from '@/features/auth/AuthContext';
import { canOperate, deadlineTone } from '@/lib/gstDocs';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface GatewayStatus {
  GatewayConfigured: boolean;
  Message: string;
}

interface DeadlineAlert {
  EInvoiceLogID: number;
  TaxInvoiceHeaderID: number;
  DocumentNo: string;
  DocumentDate: string;
  PartyName: string;
  GrandTotal: number;
  ReportingDeadline: string;
  DaysLeft: number;
  DeadlinePassed: boolean;
  AttemptCount: number;
  LastError: string;
}

interface ProcessResult {
  Attempted: number;
  Registered: number;
  FailedRetryable: number;
  FailedPermanent: number;
  PastDeadline: number;
  Messages: string[];
}

const money = (v: number) => inr(v);

export function EInvoiceQueueScreen({ def }: CustomScreenProps) {
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayOperate = canOperate(user?.UserTypeID);

  const [daysAhead, setDaysAhead] = useState('30');
  const [applied, setApplied] = useState(30);

  const status = useQuery({
    queryKey: ['einvoice-status'],
    queryFn: async (): Promise<GatewayStatus> => {
      const res = await apiPost<GatewayStatus>('EInvoice/Status', {});
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not read the gateway status.');
      return res.data;
    },
  });

  const alerts = useQuery({
    queryKey: ['einvoice-alerts', applied],
    queryFn: async (): Promise<DeadlineAlert[]> => {
      const res = await apiPost<DeadlineAlert[]>('EInvoice/DeadlineAlerts', { DaysAhead: applied });
      if (!res.ok) throw new Error(res.message || 'Could not load the deadline alerts.');
      return res.data ?? [];
    },
  });
  const rows = alerts.data ?? [];
  const overdue = rows.filter((r) => r.DeadlinePassed).length;

  const process = useMutation({
    mutationFn: async (): Promise<ProcessResult> => {
      const res = await apiPost<ProcessResult>('EInvoice/ProcessQueue', { MaxDocuments: 100 });
      if (!res.ok || !res.data) throw new Error(res.message || 'The queue could not be processed.');
      return res.data;
    },
    onSuccess: (r) => {
      toast.success(`Processed ${r.Attempted}: ${r.Registered} registered, ${r.FailedRetryable + r.FailedPermanent} failed, ${r.PastDeadline} past deadline.`);
      qc.invalidateQueries({ queryKey: ['einvoice-alerts'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const configured = status.data?.GatewayConfigured ?? false;

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Documents waiting for an IRN, and how long is left to get one"
        icon={pickIcon(def.title)}
        actions={
          mayOperate && (
            <Button onClick={() => process.mutate()} loading={process.isPending} disabled={!configured}>
              <Play className="mr-1.5 size-4" />
              Process queue
            </Button>
          )
        }
      />

      {/* Gateway status — stated plainly, because documents keep queuing and counting down either way. */}
      {status.isFetching ? (
        <div className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white p-3 text-sm text-slate-400">
          <Spinner className="size-4" /> Checking the gateway…
        </div>
      ) : (
        <div
          className={cn(
            'flex gap-2.5 rounded-lg border p-3 text-sm',
            configured
              ? 'border-success-500/30 bg-success-50 text-success-600'
              : 'border-warning-500/30 bg-warning-50 text-warning-600',
          )}
        >
          {configured ? <CheckCircle2 className="mt-0.5 size-4 shrink-0" /> : <ShieldAlert className="mt-0.5 size-4 shrink-0" />}
          <p>{status.data?.Message}</p>
        </div>
      )}

      {overdue > 0 && (
        <div className="flex gap-2.5 rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-sm text-danger-600">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>{overdue} document{overdue === 1 ? '' : 's'} past the 30-day limit.</strong> No IRN can be
            obtained for {overdue === 1 ? 'it' : 'them'} any more — the only correction left is a credit note and
            a fresh invoice.
          </p>
        </div>
      )}

      <Card>
        <CardBody className="flex flex-wrap items-end gap-3">
          <div className="w-32">
            <Input
              label="Days ahead"
              type="number"
              value={daysAhead}
              min={1}
              onChange={(e) => setDaysAhead(e.target.value)}
              hint="How far to look"
            />
          </div>
          <Button variant="outline" onClick={() => setApplied(Math.max(1, Number(daysAhead || 30)))} loading={alerts.isFetching}>
            <RefreshCw className="mr-1.5 size-4" />
            Refresh
          </Button>
          <div className="flex-1" />
          <span className="text-sm text-slate-500">
            {rows.length} document{rows.length === 1 ? '' : 's'} within {applied} days of their deadline
          </span>
        </CardBody>
      </Card>

      <Card>
        {alerts.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : alerts.isError ? (
          <CardBody>
            <EmptyState title="Could not load" description={(alerts.error as Error).message} />
          </CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title="Nothing is running out of time"
              description="No document within this window is still waiting for an IRN. Widen the days-ahead to look further."
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[720px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Document</th>
                  <th className="px-3 py-2.5 font-medium">Party</th>
                  <th className="px-3 py-2.5 text-right font-medium">Value</th>
                  <th className="px-3 py-2.5 font-medium">Deadline</th>
                  <th className="px-3 py-2.5 text-right font-medium">Time left</th>
                  <th className="px-3 py-2.5 text-right font-medium">Attempts</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => {
                  const tone = deadlineTone(r.DaysLeft, r.DeadlinePassed);
                  return (
                    <tr key={r.EInvoiceLogID} className="hover:bg-slate-50">
                      <td className="px-5 py-2.5">
                        <div className="font-mono text-sm font-semibold text-slate-800">{r.DocumentNo || `#${r.TaxInvoiceHeaderID}`}</div>
                        <div className="text-xs text-slate-400">{dateOnly(r.DocumentDate)}</div>
                      </td>
                      <td className="px-3 py-2.5">
                        <div className="max-w-[200px] truncate text-slate-700" title={r.PartyName}>{r.PartyName || '—'}</div>
                      </td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{money(r.GrandTotal)}</td>
                      <td className="px-3 py-2.5 text-slate-600">{dateOnly(r.ReportingDeadline)}</td>
                      <td className="px-3 py-2.5 text-right">
                        <Badge tone={tone}>
                          <Clock className="size-3" />
                          {r.DeadlinePassed ? `${Math.abs(r.DaysLeft)}d over` : `${r.DaysLeft}d left`}
                        </Badge>
                      </td>
                      <td className="px-3 py-2.5 text-right">
                        <span className="tabular-nums text-slate-500">{r.AttemptCount}</span>
                        {r.LastError && (
                          <div className="max-w-[180px] truncate text-xs text-danger-500" title={r.LastError}>{r.LastError}</div>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {!mayOperate && (
        <p className="text-xs text-slate-400">
          Read-only. Processing the IRP queue is limited to Admin and Support. You can see exactly what
          is waiting and how long is left, which is the alert this screen exists for.
        </p>
      )}
    </div>
  );
}
