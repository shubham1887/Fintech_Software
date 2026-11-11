import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Clock, RefreshCw, Undo2 } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useAuth } from '@/features/auth/AuthContext';
import { canRaiseDocument, deadlineTone } from '@/lib/gstDocs';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface WindowAlert {
  TaxInvoiceHeaderID: number;
  DocumentNo: string;
  DocumentDate: string;
  PartyUserID: number;
  PartyName: string;
  GrossTotal: number;
  AlreadyCredited: number;
  RemainingCreditable: number;
  FailedSourceGross: number;
  FailedSourceCount: number;
  WindowClosesOn: string;
  DaysLeft: number;
  WindowClosed: boolean;
}

const money = (v: number) => inr(v);

export function CreditNoteAlertsScreen({ def }: CustomScreenProps) {
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const canIssue = canRaiseDocument(user?.UserTypeID);

  const [daysAhead, setDaysAhead] = useState('30');
  const [applied, setApplied] = useState(30);
  const [issuing, setIssuing] = useState<WindowAlert | null>(null);

  const q = useQuery({
    queryKey: ['credit-note-alerts', applied],
    queryFn: async (): Promise<WindowAlert[]> => {
      const res = await apiPost<WindowAlert[]>('CreditNote/WindowAlerts', { DaysAhead: applied });
      if (!res.ok) throw new Error(res.message || 'Could not load the window alerts.');
      return res.data ?? [];
    },
  });
  const rows = q.data ?? [];
  const closed = rows.filter((r) => r.WindowClosed).length;

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Issued documents whose credit-note window is closing while they still cover a failed transaction"
        icon={pickIcon(def.title)}
      />

      <div className="flex gap-2.5 rounded-lg border border-info-500/30 bg-info-50 p-3 text-xs text-info-600">
        <Clock className="mt-0.5 size-4 shrink-0" />
        <p>
          A credit note can carry the tax only until <strong>30 November</strong> of the year after the
          supply&apos;s financial year. Past that the note still issues, but <strong>without tax</strong>, so a
          reversal decided late costs the whole GST. These are the documents where that window is closing
          while a source on them failed or was refunded.
        </p>
      </div>

      {closed > 0 && (
        <div className="flex gap-2.5 rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-sm text-danger-600">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>{closed} window{closed === 1 ? '' : 's'} already closed.</strong> A note raised against{' '}
            {closed === 1 ? 'it' : 'them'} now recovers no tax.
          </p>
        </div>
      )}

      <Card>
        <CardBody className="flex flex-wrap items-end gap-3">
          <div className="w-32">
            <Input label="Days ahead" type="number" min={1} value={daysAhead} onChange={(e) => setDaysAhead(e.target.value)} hint="Window closing within" />
          </div>
          <Button variant="outline" onClick={() => setApplied(Math.max(1, Number(daysAhead || 30)))} loading={q.isFetching}>
            <RefreshCw className="mr-1.5 size-4" />
            Refresh
          </Button>
          <div className="flex-1" />
          <span className="text-sm text-slate-500">{rows.length} document{rows.length === 1 ? '' : 's'}</span>
        </CardBody>
      </Card>

      <Card>
        {q.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : q.isError ? (
          <CardBody>
            <EmptyState title="Could not load" description={(q.error as Error).message} />
          </CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title="Nothing needs a decision"
              description="No issued document within this window still covers a failed or refunded transaction."
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[820px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Document</th>
                  <th className="px-3 py-2.5 font-medium">Party</th>
                  <th className="px-3 py-2.5 text-right font-medium">Reversible</th>
                  <th className="px-3 py-2.5 text-right font-medium">Failed sources</th>
                  <th className="px-3 py-2.5 font-medium">Window closes</th>
                  <th className="px-3 py-2.5 text-right font-medium">Time left</th>
                  {canIssue && <th className="px-3 py-2.5 text-right font-medium">Action</th>}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => (
                  <tr key={r.TaxInvoiceHeaderID} className={cn('hover:bg-slate-50', r.WindowClosed && 'bg-danger-50/40')}>
                    <td className="px-5 py-2.5">
                      <div className="font-mono text-sm font-semibold text-slate-800">{r.DocumentNo || `#${r.TaxInvoiceHeaderID}`}</div>
                      <div className="text-xs text-slate-400">{dateOnly(r.DocumentDate)}</div>
                    </td>
                    <td className="px-3 py-2.5">
                      <div className="max-w-[200px] truncate text-slate-700" title={r.PartyName}>{r.PartyName || `#${r.PartyUserID}`}</div>
                    </td>
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">
                      {money(r.RemainingCreditable)}
                      {r.AlreadyCredited > 0 && (
                        <div className="text-xs text-slate-400">of {money(r.GrossTotal)} · {money(r.AlreadyCredited)} done</div>
                      )}
                    </td>
                    <td className="px-3 py-2.5 text-right tabular-nums">
                      <span className="text-slate-700">{money(r.FailedSourceGross)}</span>
                      <div className="text-xs text-slate-400">{r.FailedSourceCount} source{r.FailedSourceCount === 1 ? '' : 's'}</div>
                    </td>
                    <td className="px-3 py-2.5 text-slate-600">{dateOnly(r.WindowClosesOn)}</td>
                    <td className="px-3 py-2.5 text-right">
                      {r.WindowClosed ? (
                        <Badge tone="danger">closed</Badge>
                      ) : (
                        <Badge tone={deadlineTone(r.DaysLeft, false)}>
                          <Clock className="size-3" />
                          {r.DaysLeft}d left
                        </Badge>
                      )}
                    </td>
                    {canIssue && (
                      <td className="px-3 py-2.5 text-right">
                        <Button variant="ghost" size="sm" onClick={() => setIssuing(r)}>
                          <Undo2 className="size-4" />
                          Credit note
                        </Button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {!canIssue && (
        <p className="text-xs text-slate-400">
          Read-only. Issuing a credit note reduces declared output tax, so it is limited to the
          administrator. Seeing what is about to expire is the accountant&apos;s job; raising the note is not.
        </p>
      )}

      {canIssue && issuing && (
        <IssueCreditNoteModal
          alert={issuing}
          onClose={() => setIssuing(null)}
          onIssued={() => {
            setIssuing(null);
            qc.invalidateQueries({ queryKey: ['credit-note-alerts'] });
          }}
          toastSuccess={(m) => toast.success(m)}
        />
      )}
    </div>
  );
}

/**
 * Issue a credit note against one alerted document. The GSTIN and state code come from the
 * original document on the server — the service re-reads them from the original it is reversing —
 * so this form only needs the amount and the reason. Amount 0 means "whatever is left".
 *
 * A warning (not a refusal) comes back when the window has closed: the note is still raised, but
 * without tax, and the message says so. That is surfaced rather than swallowed, because it is the
 * difference between a free reversal and one that costs the whole tax.
 */
function IssueCreditNoteModal({
  alert,
  onClose,
  onIssued,
  toastSuccess,
}: {
  alert: WindowAlert;
  onClose: () => void;
  onIssued: () => void;
  toastSuccess: (m: string) => void;
}) {
  const toast = useToast();
  const [amount, setAmount] = useState('');
  const [reason, setReason] = useState('');
  const [error, setError] = useState('');

  const issue = useMutation({
    mutationFn: async (): Promise<{ warning: string }> => {
      const res = await apiPost<{ Document: unknown; Warning: string }>('CreditNote/Issue', {
        OriginalInvoiceHeaderID: alert.TaxInvoiceHeaderID,
        GrossAmount: Number(amount || 0),
        Reason: reason.trim(),
        // The server takes these from the original when they are blank; sent anyway where derivable.
        CompanyGSTIN: '',
        SupplierStateCode: '',
        DryRun: false,
      });
      if (!res.ok) throw new Error(res.message || 'The credit note could not be issued.');
      return { warning: res.data?.Warning ?? '' };
    },
    onSuccess: ({ warning }) => {
      if (warning) toast.error(warning);
      else toastSuccess('Credit note issued.');
      onIssued();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const submit = () => {
    if (!reason.trim()) {
      setError('A reason is required — it is the first thing an auditor reads.');
      return;
    }
    setError('');
    issue.mutate();
  };

  const amt = Number(amount || 0);
  const overCap = amt > alert.RemainingCreditable + 0.005;

  return (
    <Modal open onClose={onClose} title={`Credit note against ${alert.DocumentNo || `#${alert.TaxInvoiceHeaderID}`}`} size="md">
      <div className="space-y-4">
        <div className="grid grid-cols-2 gap-3 rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm">
          <div>
            <p className="text-xs text-slate-400">Party</p>
            <p className="text-slate-700">{alert.PartyName || `#${alert.PartyUserID}`}</p>
          </div>
          <div className="text-right">
            <p className="text-xs text-slate-400">Reversible, at gross</p>
            <p className="font-semibold tabular-nums text-slate-800">{money(alert.RemainingCreditable)}</p>
          </div>
          {alert.FailedSourceCount > 0 && (
            <div className="col-span-2 text-xs text-slate-500">
              {alert.FailedSourceCount} source{alert.FailedSourceCount === 1 ? '' : 's'} on this document failed or were
              refunded, {money(alert.FailedSourceGross)} in all — probably the amount to reverse.
            </div>
          )}
        </div>

        {alert.WindowClosed && (
          <div className="flex gap-2.5 rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-xs text-danger-600">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" />
            <p>This window has closed. The note will issue <strong>without tax</strong> — the GST is no longer recoverable.</p>
          </div>
        )}

        <Input
          label="Amount to reverse (gross)"
          type="number"
          value={amount}
          max={alert.RemainingCreditable}
          placeholder={`0 = all remaining (${money(alert.RemainingCreditable)})`}
          hint={overCap ? undefined : 'Leave blank to reverse everything still open on this document.'}
          error={overCap ? `More than the ${money(alert.RemainingCreditable)} still reversible.` : undefined}
          onChange={(e) => setAmount(e.target.value)}
        />
        <Input
          label="Reason"
          value={reason}
          placeholder="e.g. Transaction FIN123456 failed and was refunded"
          error={error}
          onChange={(e) => setReason(e.target.value)}
        />

        <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
          <Button variant="outline" onClick={onClose} disabled={issue.isPending}>
            Cancel
          </Button>
          <Button onClick={submit} loading={issue.isPending} disabled={overCap}>
            <Undo2 className="mr-1.5 size-4" />
            Issue credit note
          </Button>
        </div>
      </div>
    </Modal>
  );
}
