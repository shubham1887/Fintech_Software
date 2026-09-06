import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Save } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { OPT_PAYLOAD, Pill, ToggleDot, useServices, type Row } from './providerShared';

/** Editable state for one operator's route on this provider. */
interface RouteDraft {
  ConfigurationID: number; // 0 = not mapped yet
  OperatorID: number;
  OperatorName: string;
  OpCode: string;
  Type: string;
  Lapu: string;
  PendingCount: string;
  PendingInMinute: string;
  FailCount: string;
  FailInMinute: string;
  IsActive: boolean;
  // Commission fields live on the same route row (edited on the Commission tab) —
  // carried through here so a Configuration save never wipes them.
  AdminComm: number;
  AdminCommType: string;
  CommissionCategory: string;
}

const cellInput =
  'h-9 w-full rounded-lg border border-slate-200 px-2 text-sm focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20';

/**
 * "Configuration" tab of the provider Setting page — which operator routes through this
 * provider, with the provider-side operator code, SIM/lapu, channel type, and health
 * thresholds. One editable grid row per operator of the selected service.
 */
export function OperatorMappingPanel({ providerId }: { providerId: string }) {
  const toast = useToast();
  const qc = useQueryClient();

  const [serviceId, setServiceId] = useState('');
  const services = useServices();

  const operatorsAll = useQuery({
    queryKey: ['fk', 'Operator/GetAll'],
    queryFn: () => fetchList('Operator/GetAll', OPT_PAYLOAD),
    staleTime: 60_000,
  });

  const routesKey = ['provider-routes', providerId];
  const routes = useQuery({
    queryKey: routesKey,
    queryFn: () => fetchList('ProviderConfiguration/GetByProviderID', { ProviderID: Number(providerId) }),
    enabled: Number(providerId) > 0,
  });
  const refresh = () => qc.invalidateQueries({ queryKey: routesKey });

  // Merge: every operator of the service, overlaid with its existing route (if any).
  const [drafts, setDrafts] = useState<RouteDraft[]>([]);
  useEffect(() => {
    if (!serviceId) { setDrafts([]); return; }
    const ops = (operatorsAll.data ?? []).filter((o) => String(getField(o, 'ServiceID')) === serviceId);
    const existing = (routes.data ?? []).filter((r) => String(getField(r, 'ServiceID')) === serviceId);
    const byOperator = new Map<number, Row>(
      existing.map((r) => [Number(getField(r, 'OperatorID')), r]),
    );
    setDrafts(
      ops.map((o) => {
        const opId = Number(getField(o, 'OperatorID'));
        const r = byOperator.get(opId);
        return {
          ConfigurationID: r ? Number(getField(r, 'ConfigurationID')) : 0,
          OperatorID: opId,
          OperatorName: String(getField(o, 'OperatorName') ?? ''),
          OpCode: r ? String(getField(r, 'OpCode') ?? '') : '',
          Type: r ? String(getField(r, 'Type') ?? '') : '',
          Lapu: r ? String(getField(r, 'Lapu') ?? '') : '',
          PendingCount: r ? String(getField(r, 'PendingCount') ?? '0') : '0',
          PendingInMinute: r ? String(getField(r, 'PendingInMinute') ?? '0') : '0',
          FailCount: r ? String(getField(r, 'FailCount') ?? '0') : '0',
          FailInMinute: r ? String(getField(r, 'FailInMinute') ?? '0') : '0',
          IsActive: r ? Boolean(getField(r, 'IsActive')) : true,
          AdminComm: r ? Number(getField(r, 'AdminComm') ?? 0) : 0,
          AdminCommType: r ? String(getField(r, 'AdminCommType') ?? 'Fix') : 'Fix',
          CommissionCategory: r ? String(getField(r, 'CommissionCategory') ?? '') : '',
        };
      }),
    );
  }, [serviceId, operatorsAll.data, routes.data]);

  const patch = (operatorId: number, changes: Partial<RouteDraft>) =>
    setDrafts((prev) => prev.map((d) => (d.OperatorID === operatorId ? { ...d, ...changes } : d)));

  const buildPayload = (d: RouteDraft) => ({
    OperatorID: d.OperatorID,
    ProviderID: Number(providerId),
    ServiceID: Number(serviceId),
    OpCode: d.OpCode.trim(),
    Type: d.Type.trim(),
    Lapu: d.Lapu.trim(),
    Quantity: '',
    AdminComm: d.AdminComm,
    AdminCommType: d.AdminCommType || 'Fix',
    CommissionCategory: d.CommissionCategory,
    PendingCount: Number(d.PendingCount) || 0,
    PendingInMinute: Number(d.PendingInMinute) || 0,
    FailCount: Number(d.FailCount) || 0,
    FailInMinute: Number(d.FailInMinute) || 0,
    IsActive: d.IsActive,
  });

  const saveOne = async (d: RouteDraft) => {
    if (!d.OpCode.trim()) throw new Error(`${d.OperatorName}: the provider-side operator code is required.`);
    const res = d.ConfigurationID
      ? await apiPost('ProviderConfiguration/Update', { ...buildPayload(d), ConfigurationID: d.ConfigurationID })
      : await apiPost('ProviderConfiguration/Create', buildPayload(d));
    if (!res.ok) throw new Error(`${d.OperatorName}: ${res.message}`);
  };

  const [savingId, setSavingId] = useState<number | null>(null);
  const rowMut = useMutation({
    mutationFn: (d: RouteDraft) => saveOne(d),
    onSuccess: () => { toast.success('Mapping saved.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
    onSettled: () => setSavingId(null),
  });

  const saveAllMut = useMutation({
    mutationFn: async () => {
      const toSave = drafts.filter((d) => d.OpCode.trim());
      if (toSave.length === 0) throw new Error('Nothing to save — enter operator codes first.');
      for (const d of toSave) await saveOne(d);
      return toSave.length;
    },
    onSuccess: (n) => { toast.success(`${n} mapping(s) saved.`); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div className="w-64">
          <Autocomplete
            label="Service"
            placeholder="Search service…"
            value={serviceId}
            options={(services.data ?? []).map((s) => ({ value: String(getField(s, 'ServiceID')), label: String(getField(s, 'Name') ?? '') }))}
            onChange={(v) => setServiceId(String(v))}
          />
        </div>
        <Button
          size="sm"
          loading={saveAllMut.isPending}
          disabled={!providerId || !serviceId || drafts.length === 0}
          onClick={() => saveAllMut.mutate()}
        >
          <Save className="size-4" /> Save all
        </Button>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">Operator</th>
                <th className="px-3 py-3">Provider code *</th>
                <th className="px-3 py-3">Channel type</th>
                <th className="px-3 py-3">SIM / Lapu</th>
                <th className="px-3 py-3" title="Auto-alert when this many transactions stay pending within the window">Pending (count / min)</th>
                <th className="px-3 py-3" title="Auto-alert when this many transactions fail within the window">Failure (count / min)</th>
                <th className="px-3 py-3">Active</th>
                <th className="px-3 py-3 text-right">Save</th>
              </tr>
            </thead>
            <tbody>
              {!serviceId ? (
                <tr><td colSpan={8} className="px-4 py-10 text-center text-sm text-slate-400">Pick a service to map its operators through this provider.</td></tr>
              ) : drafts.length === 0 ? (
                <tr><td colSpan={8} className="px-4 py-10 text-center text-sm text-slate-400">No operators found for this service.</td></tr>
              ) : (
                drafts.map((d) => (
                  <tr key={d.OperatorID} className="border-b border-slate-100 last:border-0 hover:bg-slate-50/50">
                    <td className="px-4 py-2.5">
                      <p className="font-medium text-slate-800">{d.OperatorName}</p>
                      {d.ConfigurationID > 0 ? (
                        <Pill text="Mapped" tone="bg-emerald-50 text-emerald-700" />
                      ) : (
                        <Pill text="Not mapped" tone="bg-slate-100 text-slate-500" />
                      )}
                    </td>
                    <td className="px-3 py-2.5">
                      <input className={cellInput + ' w-24 font-mono'} value={d.OpCode} placeholder="code"
                        onChange={(e) => patch(d.OperatorID, { OpCode: e.target.value })} />
                    </td>
                    <td className="px-3 py-2.5">
                      <input className={cellInput + ' w-24'} value={d.Type} placeholder="optional"
                        onChange={(e) => patch(d.OperatorID, { Type: e.target.value })} />
                    </td>
                    <td className="px-3 py-2.5">
                      <input className={cellInput + ' w-32'} value={d.Lapu} placeholder="optional"
                        onChange={(e) => patch(d.OperatorID, { Lapu: e.target.value })} />
                    </td>
                    <td className="px-3 py-2.5">
                      <div className="flex items-center gap-1.5">
                        <input type="number" className={cellInput + ' w-16 tabular-nums'} value={d.PendingCount}
                          onChange={(e) => patch(d.OperatorID, { PendingCount: e.target.value })} />
                        <span className="text-xs text-slate-400">/</span>
                        <input type="number" className={cellInput + ' w-16 tabular-nums'} value={d.PendingInMinute}
                          onChange={(e) => patch(d.OperatorID, { PendingInMinute: e.target.value })} />
                      </div>
                    </td>
                    <td className="px-3 py-2.5">
                      <div className="flex items-center gap-1.5">
                        <input type="number" className={cellInput + ' w-16 tabular-nums'} value={d.FailCount}
                          onChange={(e) => patch(d.OperatorID, { FailCount: e.target.value })} />
                        <span className="text-xs text-slate-400">/</span>
                        <input type="number" className={cellInput + ' w-16 tabular-nums'} value={d.FailInMinute}
                          onChange={(e) => patch(d.OperatorID, { FailInMinute: e.target.value })} />
                      </div>
                    </td>
                    <td className="px-3 py-2.5">
                      <ToggleDot on={d.IsActive} onClick={() => patch(d.OperatorID, { IsActive: !d.IsActive })} />
                    </td>
                    <td className="px-3 py-2.5">
                      <div className="flex justify-end">
                        <Button size="sm" variant="ghost" title="Save this operator"
                          loading={rowMut.isPending && savingId === d.OperatorID}
                          onClick={() => { setSavingId(d.OperatorID); rowMut.mutate(d); }}>
                          <Save className="size-4 text-brand-600" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
