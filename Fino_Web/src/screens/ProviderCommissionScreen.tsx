import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Save } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { OPT_PAYLOAD, Pill, useServices, type Row } from './providerShared';

/** Editable commission state for one mapped operator route on this provider. */
interface CommDraft {
  ConfigurationID: number;
  OperatorID: number;
  OperatorName: string;
  AdminComm: string;
  AdminCommType: string;      // Fix | Flexi
  CommissionCategory: string; // Commission | Surcharge
  route: Row;                 // full row — non-commission fields pass through on save
}

const cellInput =
  'h-9 w-full rounded-lg border border-slate-200 px-2 text-sm focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20';

/**
 * "Commission" tab of the provider Setting page — what this provider pays us per operator
 * (captured live from the provider reply when it sends one; this configured value is the
 * fallback). Fix = flat per transaction, Flexi = percent of amount; the Surcharge category
 * means we pay the provider instead.
 */
export function ProviderCommissionPanel({ providerId }: { providerId: string }) {
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

  // Only operators already mapped on the Configuration tab can carry a commission.
  const [drafts, setDrafts] = useState<CommDraft[]>([]);
  useEffect(() => {
    if (!serviceId) { setDrafts([]); return; }
    const opName = new Map(
      (operatorsAll.data ?? []).map((o) => [Number(getField(o, 'OperatorID')), String(getField(o, 'OperatorName') ?? '')]),
    );
    setDrafts(
      (routes.data ?? [])
        .filter((r) => String(getField(r, 'ServiceID')) === serviceId)
        .map((r) => ({
          ConfigurationID: Number(getField(r, 'ConfigurationID')),
          OperatorID: Number(getField(r, 'OperatorID')),
          OperatorName: opName.get(Number(getField(r, 'OperatorID'))) ?? `#${getField(r, 'OperatorID')}`,
          AdminComm: String(getField(r, 'AdminComm') ?? '0'),
          AdminCommType: String(getField(r, 'AdminCommType') ?? 'Fix') || 'Fix',
          CommissionCategory: String(getField(r, 'CommissionCategory') ?? '') || 'Commission',
          route: r,
        }))
        .sort((a, b) => a.OperatorName.localeCompare(b.OperatorName)),
    );
  }, [serviceId, operatorsAll.data, routes.data]);

  const patch = (configurationId: number, changes: Partial<CommDraft>) =>
    setDrafts((prev) => prev.map((d) => (d.ConfigurationID === configurationId ? { ...d, ...changes } : d)));

  const saveOne = async (d: CommDraft) => {
    const res = await apiPost('ProviderConfiguration/Update', {
      ConfigurationID: d.ConfigurationID,
      OperatorID: d.OperatorID,
      ProviderID: Number(providerId),
      ServiceID: Number(serviceId),
      OpCode: String(getField(d.route, 'OpCode') ?? ''),
      Type: String(getField(d.route, 'Type') ?? ''),
      Lapu: String(getField(d.route, 'Lapu') ?? ''),
      Quantity: String(getField(d.route, 'Quantity') ?? ''),
      AdminComm: Number(d.AdminComm) || 0,
      AdminCommType: d.AdminCommType || 'Fix',
      CommissionCategory: d.CommissionCategory,
      PendingCount: Number(getField(d.route, 'PendingCount') ?? 0),
      PendingInMinute: Number(getField(d.route, 'PendingInMinute') ?? 0),
      FailCount: Number(getField(d.route, 'FailCount') ?? 0),
      FailInMinute: Number(getField(d.route, 'FailInMinute') ?? 0),
      IsActive: Boolean(getField(d.route, 'IsActive')),
    });
    if (!res.ok) throw new Error(`${d.OperatorName}: ${res.message}`);
  };

  const [savingId, setSavingId] = useState<number | null>(null);
  const rowMut = useMutation({
    mutationFn: (d: CommDraft) => saveOne(d),
    onSuccess: () => { toast.success('Commission saved.'); qc.invalidateQueries({ queryKey: routesKey }); },
    onError: (e: Error) => toast.error(e.message),
    onSettled: () => setSavingId(null),
  });

  const saveAllMut = useMutation({
    mutationFn: async () => {
      for (const d of drafts) await saveOne(d);
      return drafts.length;
    },
    onSuccess: (n) => { toast.success(`${n} commission(s) saved.`); qc.invalidateQueries({ queryKey: routesKey }); },
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
        <Button size="sm" loading={saveAllMut.isPending} disabled={!providerId || !serviceId || drafts.length === 0} onClick={() => saveAllMut.mutate()}>
          <Save className="size-4" /> Save all
        </Button>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">Operator</th>
                <th className="px-3 py-3" title="What the provider pays us for this operator's transactions">Provider commission</th>
                <th className="px-3 py-3" title="Fix = flat amount per transaction; Flexi = percent of the amount">Type</th>
                <th className="px-3 py-3" title="Commission = the provider pays us; Surcharge = we pay the provider">Category</th>
                <th className="px-3 py-3 text-right">Save</th>
              </tr>
            </thead>
            <tbody>
              {!serviceId ? (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-sm text-slate-400">Pick a service to set this provider's commission per operator.</td></tr>
              ) : drafts.length === 0 ? (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-sm text-slate-400">No mapped operators for this service — map them on the Configuration tab first.</td></tr>
              ) : (
                drafts.map((d) => (
                  <tr key={d.ConfigurationID} className="border-b border-slate-100 last:border-0 hover:bg-slate-50/50">
                    <td className="px-4 py-2.5">
                      <p className="font-medium text-slate-800">{d.OperatorName}</p>
                      <Pill text="Mapped" tone="bg-emerald-50 text-emerald-700" />
                    </td>
                    <td className="px-3 py-2.5">
                      <input
                        type="number" step="0.00001"
                        className={cellInput + ' w-28 tabular-nums'}
                        value={d.AdminComm}
                        onChange={(e) => patch(d.ConfigurationID, { AdminComm: e.target.value })}
                      />
                    </td>
                    <td className="px-3 py-2.5">
                      <select
                        className={cellInput + ' w-28'}
                        value={d.AdminCommType}
                        onChange={(e) => patch(d.ConfigurationID, { AdminCommType: e.target.value })}
                      >
                        <option value="Fix">Fix (flat)</option>
                        <option value="Flexi">Flexi (%)</option>
                      </select>
                    </td>
                    <td className="px-3 py-2.5">
                      <select
                        className={cellInput + ' w-32'}
                        value={d.CommissionCategory}
                        onChange={(e) => patch(d.ConfigurationID, { CommissionCategory: e.target.value })}
                      >
                        <option value="Commission">Commission</option>
                        <option value="Surcharge">Surcharge</option>
                      </select>
                    </td>
                    <td className="px-3 py-2.5">
                      <div className="flex justify-end">
                        <Button size="sm" variant="ghost" title="Save this operator's commission"
                          loading={rowMut.isPending && savingId === d.ConfigurationID}
                          onClick={() => { setSavingId(d.ConfigurationID); rowMut.mutate(d); }}>
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

      <p className="mt-3 text-xs text-slate-400">
        When a provider's reply carries a commission value, the live value is stored on the transaction; this configured value is used otherwise.
      </p>
    </>
  );
}
