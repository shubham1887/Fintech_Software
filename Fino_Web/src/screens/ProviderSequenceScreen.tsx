import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { RotateCcw, Save, Search } from 'lucide-react';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Select } from '@/components/ui/Select';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { pickIcon } from '@/components/layout/iconMap';
import { useServices } from './providerShared';
import {
  bulkUpdateSequence,
  fetchBulkSequenceProviders,
  fetchProviderSequence,
  fetchViewApiProviders,
  insertProviderSequence,
  type BulkUpdateRow,
} from '@/api/sequence.api';
import type { CustomScreenProps } from './registry';

/** Editable copy of one operator row of the sequence grid. */
interface RowState {
  operatorId: number;
  operatorName: string;
  serviceId: number;
  serviceName: string;
  viewProviderId: number;
  providers: Array<{ providerId: number; providerName: string; count: number; active: boolean }>;
  dirty: boolean;
}

const CRITERIA_OPTIONS = [
  { value: 'AddSequence', label: 'Add Sequence' },
  { value: 'RemoveSequence', label: 'Remove Sequence' },
  { value: 'ViewAPI', label: 'View API' },
] as const;

/**
 * Provider Sequence — per-operator provider switching priorities (legacy ProvidersSequence).
 * Pick a service → every operator lists all PAY-capable providers with a checkbox + priority
 * count; save per row, or select rows and bulk Add/Remove sequence / set View API.
 */
export function ProviderSequenceScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();

  const [serviceId, setServiceId] = useState('');
  const [criteria, setCriteria] = useState('');
  const [bulkProviderIds, setBulkProviderIds] = useState<number[]>([]);
  const [bulkCount, setBulkCount] = useState('0');
  const [bulkViewProviderId, setBulkViewProviderId] = useState('');
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [query, setQuery] = useState('');
  const [rows, setRows] = useState<RowState[]>([]);

  const servicesQ = useServices();
  const gridQ = useQuery({
    queryKey: ['seq', 'OperatorSequence/Select', serviceId],
    queryFn: () => fetchProviderSequence(Number(serviceId)),
    enabled: serviceId !== '',
  });
  const bulkProvidersQ = useQuery({
    queryKey: ['seq', 'bulk-providers'],
    queryFn: fetchBulkSequenceProviders,
    staleTime: 60_000,
    enabled: criteria === 'AddSequence' || criteria === 'RemoveSequence',
  });
  const viewApiQ = useQuery({
    queryKey: ['seq', 'viewapi-providers'],
    queryFn: fetchViewApiProviders,
    staleTime: 60_000,
  });

  // Editable copy of the grid.
  useEffect(() => {
    const next: RowState[] = (gridQ.data ?? []).map((r) => ({
      operatorId: Number(getField(r, 'OperatorID') ?? 0),
      operatorName: String(getField(r, 'OperatorName') ?? ''),
      serviceId: Number(getField(r, 'ServiceID') ?? 0),
      serviceName: String(getField(r, 'ServiceName') ?? ''),
      viewProviderId: Number(getField(r, 'ViewProviderID') ?? 0),
      providers: ((getField(r, 'ProviderList') as unknown[]) ?? []).map((p) => ({
        providerId: Number(getField(p as Record<string, unknown>, 'ProviderID') ?? 0),
        providerName: String(getField(p as Record<string, unknown>, 'ProviderName') ?? ''),
        count: Number(getField(p as Record<string, unknown>, 'Count') ?? 0),
        active: Boolean(getField(p as Record<string, unknown>, 'IsActive')),
      })),
      dirty: false,
    }));
    setRows(next);
    setSelected(new Set());
  }, [gridQ.data]);

  const visibleRows = useMemo(() => {
    const q = query.trim().toLowerCase();
    return q ? rows.filter((r) => `${r.operatorName} ${r.serviceName}`.toLowerCase().includes(q)) : rows;
  }, [rows, query]);

  const patchRow = (operatorId: number, patch: (r: RowState) => RowState) =>
    setRows((prev) => prev.map((r) => (r.operatorId === operatorId ? { ...patch(r), dirty: true } : r)));

  const refresh = () => qc.invalidateQueries({ queryKey: ['seq', 'OperatorSequence/Select'] });

  const saveMut = useMutation({
    mutationFn: (row: RowState) =>
      insertProviderSequence({
        ServiceID: row.serviceId,
        OperatorID: row.operatorId,
        ProviderList: row.providers
          .filter((p) => p.active)
          .map((p) => ({ ProviderID: p.providerId, ProviderName: p.providerName, Count: p.count })),
        ViewProviderID: row.viewProviderId,
        RealProviderID: 0,
      }),
    onSuccess: (res, row) => {
      if (res.ok) {
        toast.success(`${row.operatorName}: sequence saved.`);
        refresh();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const bulkMut = useMutation({
    mutationFn: (payload: BulkUpdateRow[]) => bulkUpdateSequence(payload),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(res.message || 'Sequence updated.');
        setSelected(new Set());
        setCriteria('');
        setBulkProviderIds([]);
        setBulkCount('0');
        setBulkViewProviderId('');
        refresh();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const applyBulk = () => {
    if (!criteria) return toast.error('Select a criteria for bulk update.');
    if (selected.size === 0) return toast.error('Select one or more operators first.');
    if ((criteria === 'AddSequence' || criteria === 'RemoveSequence') && bulkProviderIds.length === 0)
      return toast.error('Select one or more providers.');
    if (criteria === 'ViewAPI' && bulkViewProviderId === '') return toast.error('Select a ViewAPI provider.');

    const providerName = (id: number) =>
      String(
        (bulkProvidersQ.data ?? []).find((p) => Number(getField(p, 'ProviderID')) === id)?.ProviderName ?? '',
      );

    const payload: BulkUpdateRow[] = [...selected].map((operatorId) => ({
      Criteria: criteria,
      ServiceID: Number(serviceId),
      OperatorID: operatorId,
      Count: Number(bulkCount) || 0,
      ProviderList: bulkProviderIds.map((id) => ({ ProviderID: id, ProviderName: providerName(id) })),
      ViewProviderID: Number(bulkViewProviderId) || 0,
      RealProviderID: 0,
    }));
    bulkMut.mutate(payload);
  };

  const resetBulk = () => {
    setCriteria('');
    setBulkProviderIds([]);
    setBulkCount('0');
    setBulkViewProviderId('');
    setSelected(new Set());
  };

  const toggleAll = () =>
    setSelected((prev) =>
      prev.size === visibleRows.length ? new Set() : new Set(visibleRows.map((r) => r.operatorId)),
    );

  const viewApiOptions = (viewApiQ.data ?? []).map((p, i) => {
    const id = String(getField(p, 'ProviderID') ?? '');
    return (
      <option key={`${id}-${i}`} value={id}>
        {String(getField(p, 'ProviderName') ?? id)}
      </option>
    );
  });

  const needsProviderPicker = criteria === 'AddSequence' || criteria === 'RemoveSequence';

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      {/* Controls */}
      <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
        <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
          <Autocomplete
            label="Service"
            placeholder="Search service…"
            loading={servicesQ.isLoading}
            value={serviceId}
            options={(servicesQ.data ?? []).map((s) => {
              const id = String(getField(s, 'ServiceID') ?? '');
              return { value: id, label: String(getField(s, 'Name', 'ServiceName') ?? id) };
            })}
            onChange={(v) => setServiceId(String(v))}
          />

          <Select
            label="Selection Criteria For Bulk Update"
            value={criteria}
            onChange={(e) => setCriteria(e.target.value)}
          >
            <option value="">Selection Criteria For Bulk Update</option>
            {CRITERIA_OPTIONS.map((c) => (
              <option key={c.value} value={c.value}>
                {c.label}
              </option>
            ))}
          </Select>

          {needsProviderPicker && (
            <div>
              <span className="mb-1.5 block text-sm font-medium text-slate-700">Provider(s)</span>
              <div className="flex max-h-24 flex-wrap gap-1.5 overflow-y-auto rounded-lg border border-slate-200 p-2">
                {(bulkProvidersQ.data ?? []).map((p, i) => {
                  const id = Number(getField(p, 'ProviderID') ?? 0);
                  const name = String(getField(p, 'ProviderName') ?? id);
                  const on = bulkProviderIds.includes(id);
                  return (
                    <button
                      key={`${id}-${i}`}
                      type="button"
                      onClick={() =>
                        setBulkProviderIds((prev) => (on ? prev.filter((x) => x !== id) : [...prev, id]))
                      }
                      className={cn(
                        'rounded-full px-2.5 py-1 text-[11px] font-semibold transition-colors',
                        on
                          ? 'bg-brand-600 text-white'
                          : 'border border-slate-200 bg-slate-50 text-slate-600 hover:bg-slate-100',
                      )}
                    >
                      {name}
                    </button>
                  );
                })}
                {bulkProvidersQ.isLoading && <span className="text-xs text-slate-400">Loading…</span>}
              </div>
            </div>
          )}

          {criteria === 'AddSequence' && (
            <Input
              label="Count"
              type="number"
              value={bulkCount}
              onChange={(e) => setBulkCount(e.target.value)}
            />
          )}

          {criteria === 'ViewAPI' && (
            <Select
              label="ViewAPI"
              value={bulkViewProviderId}
              onChange={(e) => setBulkViewProviderId(e.target.value)}
            >
              <option value="">Select ViewAPI</option>
              {viewApiOptions}
            </Select>
          )}

          <div className="flex items-end gap-2">
            <Button onClick={applyBulk} loading={bulkMut.isPending} disabled={serviceId === ''}>
              Update
            </Button>
            <Button variant="outline" onClick={resetBulk}>
              <RotateCcw className="size-4" />
              Reset
            </Button>
          </div>
        </div>
        {selected.size > 0 && (
          <p className="mt-3 text-xs text-slate-500">
            <span className="font-semibold text-brand-700 tabular-nums">{selected.size}</span> operator(s)
            selected for bulk update
          </p>
        )}
      </div>

      {/* Grid */}
      <div className="mb-3 flex items-center justify-between gap-3">
        <h2 className="font-display text-base font-bold text-slate-900">Operator-wise provider priority</h2>
        <div className="relative w-72 max-w-full">
          <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <Input
            className="pl-9"
            placeholder="Search operator…"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
        </div>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="w-10 px-4 py-3">
                  <input
                    type="checkbox"
                    className="size-4 accent-brand-600"
                    checked={visibleRows.length > 0 && selected.size === visibleRows.length}
                    onChange={toggleAll}
                  />
                </th>
                <th className="w-10 px-2 py-3">#</th>
                <th className="w-40 px-4 py-3">Detail</th>
                <th className="px-4 py-3">Providers (check = in sequence, number = priority weight)</th>
                <th className="w-48 px-4 py-3">View API</th>
                <th className="w-20 px-4 py-3 text-right">Save</th>
              </tr>
            </thead>
            <tbody>
              {serviceId === '' ? (
                <tr>
                  <td colSpan={6}>
                    <EmptyState title="Pick a service" description="Select a service above to load its operators and provider sequence." />
                  </td>
                </tr>
              ) : gridQ.isFetching && rows.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-sm text-slate-400">
                    Loading…
                  </td>
                </tr>
              ) : visibleRows.length === 0 ? (
                <tr>
                  <td colSpan={6}>
                    <EmptyState title="No operators" description="No active operators found for this service." />
                  </td>
                </tr>
              ) : (
                visibleRows.map((row, i) => (
                  <tr
                    key={row.operatorId}
                    className="border-b border-slate-100 align-top last:border-0 even:bg-slate-50/40"
                  >
                    <td className="px-4 py-4">
                      <input
                        type="checkbox"
                        className="size-4 accent-brand-600"
                        checked={selected.has(row.operatorId)}
                        onChange={() =>
                          setSelected((prev) => {
                            const next = new Set(prev);
                            if (next.has(row.operatorId)) next.delete(row.operatorId);
                            else next.add(row.operatorId);
                            return next;
                          })
                        }
                      />
                    </td>
                    <td className="px-2 py-4 text-slate-500">{i + 1}</td>
                    <td className="px-4 py-4">
                      <p className="text-xs text-slate-500">{row.serviceName}</p>
                      <p className="font-semibold text-slate-800">{row.operatorName}</p>
                    </td>
                    <td className="px-4 py-3">
                      <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                        {row.providers.map((p) => (
                          <div
                            key={p.providerId}
                            className={cn(
                              'rounded-lg border p-2 transition-colors',
                              p.active ? 'border-brand-300 bg-brand-50/50' : 'border-slate-200 bg-white',
                            )}
                          >
                            <label className="flex cursor-pointer items-center gap-2 text-xs font-medium text-slate-700">
                              <input
                                type="checkbox"
                                className="size-3.5 accent-brand-600"
                                checked={p.active}
                                onChange={() =>
                                  patchRow(row.operatorId, (r) => ({
                                    ...r,
                                    providers: r.providers.map((x) =>
                                      x.providerId === p.providerId ? { ...x, active: !x.active } : x,
                                    ),
                                  }))
                                }
                              />
                              <span className="truncate" title={p.providerName}>
                                {p.providerName}
                              </span>
                            </label>
                            <input
                              type="number"
                              value={p.count}
                              onChange={(e) =>
                                patchRow(row.operatorId, (r) => ({
                                  ...r,
                                  providers: r.providers.map((x) =>
                                    x.providerId === p.providerId
                                      ? { ...x, count: Number(e.target.value) || 0 }
                                      : x,
                                  ),
                                }))
                              }
                              className="mt-1.5 h-8 w-full rounded-md border border-slate-200 px-2 text-xs tabular-nums focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20"
                            />
                          </div>
                        ))}
                      </div>
                    </td>
                    <td className="px-4 py-4">
                      <Select
                        value={row.viewProviderId ? String(row.viewProviderId) : ''}
                        onChange={(e) =>
                          patchRow(row.operatorId, (r) => ({ ...r, viewProviderId: Number(e.target.value) || 0 }))
                        }
                        className="h-9"
                      >
                        <option value="">Select ViewAPI</option>
                        {viewApiOptions}
                      </Select>
                    </td>
                    <td className="px-4 py-4">
                      <div className="flex justify-end">
                        <Button
                          size="sm"
                          variant={row.dirty ? 'primary' : 'ghost'}
                          loading={saveMut.isPending && saveMut.variables?.operatorId === row.operatorId}
                          onClick={() => saveMut.mutate(row)}
                          title="Save this operator's sequence"
                        >
                          <Save className="size-4" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        {visibleRows.length > 0 && (
          <div className="border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
            Showing <span className="font-semibold text-slate-700 tabular-nums">{visibleRows.length}</span>{' '}
            operator(s)
          </div>
        )}
      </div>
    </>
  );
}
