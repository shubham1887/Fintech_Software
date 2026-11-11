import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeftRight, Scale, Search, TrendingUp } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, apiDate } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useCsvDownload } from '@/lib/exportAll';
import { currentFyRange } from '@/lib/gstDocs';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface RegisterRow {
  PartnerUserID: number;
  PartnerName: string;
  PartnerGSTIN: string;
  PeriodKey: string;
  OutwardAmount: number;
  OutwardLegs: number;
  InwardAmount: number;
  InwardLegs: number;
  NetPosition: number;
  TotalTurnover: number;
  OutwardAwaitingDocument: number;
  InwardAwaitingDocument: number;
  InwardAwaitingAmount: number;
}

interface RegisterResult {
  PeriodFrom: string;
  PeriodTo: string;
  TotalOutward: number;
  TotalInward: number;
  TotalTurnover: number;
  TotalAwaitingDocument: number;
  TotalAwaitingAmount: number;
  Rows: RegisterRow[];
}

const money = (v: number) => inr(v);

export function ExchangeRegisterScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const fy = currentFyRange();
  const [from, setFrom] = useState(fy.from);
  const [to, setTo] = useState(fy.to);
  const [applied, setApplied] = useState({ from, to });

  const q = useQuery({
    queryKey: ['exchange-register', applied],
    queryFn: async (): Promise<RegisterResult> => {
      const res = await apiPost<RegisterResult>('ExchangeRegister/Register', {
        PeriodFrom: applied.from,
        PeriodTo: applied.to,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'The register could not be loaded.');
      return res.data;
    },
  });
  const data = q.data;
  const rows = data?.Rows ?? [];

  const exportRows = () =>
    downloadCsv(
      rows.map((r) => ({
        Partner: r.PartnerName,
        GSTIN: r.PartnerGSTIN,
        Period: r.PeriodKey,
        Outward: r.OutwardAmount,
        Inward: r.InwardAmount,
        Net: r.NetPosition,
        Turnover: r.TotalTurnover,
        AwaitingDocuments: r.InwardAwaitingDocument,
        AwaitingAmount: r.InwardAwaitingAmount,
      })),
      'Exchange register',
    );

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Barter with exchange partners — net nets to nothing, but each leg is taxable turnover"
        icon={pickIcon(def.title)}
      />

      {/* Why the two numbers differ — this is the whole reason the screen exists. */}
      <div className="flex gap-2.5 rounded-lg border border-info-500/30 bg-info-50 p-3 text-xs text-info-600">
        <Scale className="mt-0.5 size-4 shrink-0" />
        <p>
          A reciprocal swap nets to zero against a partner, which is why its turnover goes missing.
          Under GST Section 7(1)(a) barter is supply and <strong>each leg</strong> is taxable, so the
          turnover is the <strong>sum</strong> of the legs, not the difference. Both numbers sit side
          by side below so the gap cannot be overlooked.
        </p>
      </div>

      <Card>
        <CardBody className="grid grid-cols-1 gap-3 sm:grid-cols-4">
          <Input label="From" type="date" value={from} max={to} onChange={(e) => setFrom(e.target.value)} />
          <Input label="To" type="date" value={to} max={apiDate(new Date())} onChange={(e) => setTo(e.target.value)} />
          <div className="flex items-end sm:col-span-2">
            <Button onClick={() => setApplied({ from, to })} loading={q.isFetching}>
              <Search className="mr-1.5 size-4" />
              Show register
            </Button>
          </div>
        </CardBody>
      </Card>

      <div className={cn(STAT_STRIP, 'grid-cols-2 lg:grid-cols-4')}>
        <StatTile label="Turnover (both legs)" value={money(data?.TotalTurnover ?? 0)} icon={TrendingUp} tone="text-brand-600" loading={q.isFetching} />
        <StatTile label="Outward (our supply)" value={money(data?.TotalOutward ?? 0)} icon={ArrowLeftRight} tone="text-slate-600" loading={q.isFetching} />
        <StatTile label="Inward (their supply)" value={money(data?.TotalInward ?? 0)} icon={ArrowLeftRight} tone="text-slate-600" loading={q.isFetching} />
        <StatTile
          label="Awaiting their invoice"
          value={data?.TotalAwaitingDocument ?? 0}
          icon={Scale}
          tone={data && data.TotalAwaitingDocument > 0 ? 'text-warning-600' : 'text-slate-400'}
          loading={q.isFetching}
        />
      </div>

      <Card>
        <div className="flex items-center justify-between gap-3 border-b border-slate-100 px-5 py-3">
          <span className="text-sm text-slate-500">{rows.length} partner{rows.length === 1 ? '' : 's'}</span>
          <Button variant="outline" size="sm" onClick={exportRows} disabled={rows.length === 0}>
            Export
          </Button>
        </div>

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
              title="No exchange partners in this period"
              description="Either no partner is flagged (PartyTaxProfiles.IsExchangePartner), or the detection run has not been done for this period."
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[760px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Partner</th>
                  <th className="px-3 py-2.5 text-right font-medium">Outward</th>
                  <th className="px-3 py-2.5 text-right font-medium">Inward</th>
                  <th className="px-3 py-2.5 text-right font-medium">Net</th>
                  <th className="px-3 py-2.5 text-right font-medium">Turnover</th>
                  <th className="px-3 py-2.5 text-right font-medium">Awaiting</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => (
                  <tr key={`${r.PartnerUserID}-${r.PeriodKey}`} className="hover:bg-slate-50">
                    <td className="px-5 py-2.5">
                      <div className="max-w-[220px] truncate text-slate-700" title={r.PartnerName}>{r.PartnerName || `#${r.PartnerUserID}`}</div>
                      <div className="flex items-center gap-1.5 text-xs text-slate-400">
                        {r.PartnerGSTIN ? <span className="font-mono">{r.PartnerGSTIN}</span> : <span>no GSTIN on file</span>}
                        {r.PeriodKey && <span>· {r.PeriodKey}</span>}
                      </div>
                    </td>
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">
                      {money(r.OutwardAmount)}
                      <div className="text-xs text-slate-400">{r.OutwardLegs} leg{r.OutwardLegs === 1 ? '' : 's'}</div>
                    </td>
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-600">
                      {money(r.InwardAmount)}
                      <div className="text-xs text-slate-400">{r.InwardLegs} leg{r.InwardLegs === 1 ? '' : 's'}</div>
                    </td>
                    {/* Net is greyed on purpose — it is the misleading number, kept visible but never emphasised. */}
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-400">{money(r.NetPosition)}</td>
                    <td className="px-3 py-2.5 text-right font-semibold tabular-nums text-brand-700">{money(r.TotalTurnover)}</td>
                    <td className="px-3 py-2.5 text-right">
                      {r.InwardAwaitingDocument > 0 ? (
                        <div className="flex flex-col items-end gap-0.5">
                          <Badge tone="warning">{r.InwardAwaitingDocument} to chase</Badge>
                          <span className="text-xs tabular-nums text-slate-400">{money(r.InwardAwaitingAmount)}</span>
                        </div>
                      ) : r.OutwardAwaitingDocument > 0 ? (
                        <Badge tone="danger">{r.OutwardAwaitingDocument} un-invoiced</Badge>
                      ) : (
                        <span className="text-xs text-slate-400">—</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr className="border-t border-slate-200 bg-slate-50 text-sm font-semibold text-slate-700">
                  <td className="px-5 py-2.5">Total</td>
                  <td className="px-3 py-2.5 text-right tabular-nums">{money(data?.TotalOutward ?? 0)}</td>
                  <td className="px-3 py-2.5 text-right tabular-nums">{money(data?.TotalInward ?? 0)}</td>
                  <td className="px-3 py-2.5 text-right tabular-nums text-slate-400">
                    {money((data?.TotalOutward ?? 0) - (data?.TotalInward ?? 0))}
                  </td>
                  <td className="px-3 py-2.5 text-right tabular-nums text-brand-700">{money(data?.TotalTurnover ?? 0)}</td>
                  <td className="px-3 py-2.5 text-right tabular-nums">{money(data?.TotalAwaitingAmount ?? 0)}</td>
                </tr>
              </tfoot>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
}
