import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Pencil, Plus, Trash2, X } from 'lucide-react';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { pickIcon } from '@/components/layout/iconMap';
import { useServices } from './providerShared';
import {
  deleteAllUserwiseSequences,
  deleteUserwiseSequence,
  fetchAllUserwiseSequences,
  fetchBulkSequenceProviders,
  fetchUserwiseSequence,
  insertUserwiseSequence,
} from '@/api/sequence.api';
import type { CustomScreenProps } from './registry';

const USER_LIST = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0, ShowDeleted: false };

type Row = Record<string, unknown>;

interface ChipProvider {
  ProviderID: number;
  ProviderName: string;
}

function providerChips(row: Row): ChipProvider[] {
  return (((getField(row, 'ProviderList') as unknown[]) ?? []) as Row[]).map((p) => ({
    ProviderID: Number(getField(p, 'ProviderID') ?? 0),
    ProviderName: String(getField(p, 'ProviderName') ?? ''),
  }));
}

function userDisplay(u: Row): string {
  const id = Number(getField(u, 'UserMasterID', 'UserID') ?? 0);
  const name = String(getField(u, 'FirmName') ?? '').trim() || `User ${id}`;
  const mobile = String(getField(u, 'MobileNumber') ?? '').trim();
  return mobile ? `${name} [${mobile}]` : name;
}

/** Ordered green chips — the sequence position is part of the data, so show it. */
function ChipRail({ chips }: { chips: ChipProvider[] }) {
  if (chips.length === 0) return <span className="text-xs text-slate-400">Default sequence</span>;
  return (
    <div className="flex flex-wrap gap-1.5">
      {chips.map((c, i) => (
        <span
          key={`${c.ProviderID}-${i}`}
          className="inline-flex items-center gap-1 rounded-md bg-emerald-50 px-2 py-1 text-[11px] font-semibold text-emerald-700 ring-1 ring-emerald-200"
        >
          <span className="tabular-nums">{i + 1}</span>
          {c.ProviderName}
        </span>
      ))}
    </div>
  );
}

/**
 * User-wise switching (legacy UserwiseSequenceIndex + SequenceUserWiseAdd): per-user,
 * per-operator provider priority that overrides the default sequence. List view shows every
 * override; Add view drills into a user + service and edits each operator's ordered providers.
 */
export function UserwiseSequenceScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();

  const [mode, setMode] = useState<'list' | 'add'>('list');
  const [userId, setUserId] = useState('');
  const [serviceId, setServiceId] = useState('');
  const [query, setQuery] = useState('');
  const [editing, setEditing] = useState<{ operatorId: number; operatorName: string; picked: ChipProvider[] } | null>(null);

  const servicesQ = useServices();
  const usersQ = useQuery({
    queryKey: ['crud', 'Users/Select', USER_LIST],
    queryFn: () => fetchList('Users/Select', USER_LIST),
    staleTime: 60_000,
  });
  const allQ = useQuery({
    queryKey: ['seq', 'userwise-all'],
    queryFn: fetchAllUserwiseSequences,
    enabled: mode === 'list',
  });
  const gridQ = useQuery({
    queryKey: ['seq', 'userwise-grid', userId, serviceId],
    queryFn: () => fetchUserwiseSequence(Number(userId), Number(serviceId)),
    enabled: mode === 'add' && userId !== '' && serviceId !== '',
  });
  const providersQ = useQuery({
    queryKey: ['seq', 'bulk-providers'],
    queryFn: fetchBulkSequenceProviders,
    staleTime: 60_000,
    enabled: mode === 'add',
  });

  const refreshAll = () => {
    qc.invalidateQueries({ queryKey: ['seq', 'userwise-all'] });
    qc.invalidateQueries({ queryKey: ['seq', 'userwise-grid'] });
  };

  const insertMut = useMutation({
    mutationFn: (v: { operatorId: number; picked: ChipProvider[] }) =>
      insertUserwiseSequence({
        UserID: Number(userId),
        ServiceID: Number(serviceId),
        OperatorID: v.operatorId,
        ProviderList: v.picked,
      }),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(res.message || 'User-wise sequence saved.');
        setEditing(null);
        refreshAll();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (v: { userId: number; operatorId: number }) => deleteUserwiseSequence(v.userId, v.operatorId),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(res.message || 'Set back to default sequence.');
        refreshAll();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const resetAllMut = useMutation({
    mutationFn: deleteAllUserwiseSequences,
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(res.message || 'All user-wise sequences reset.');
        refreshAll();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const listRows = useMemo(() => {
    const rows = (allQ.data ?? []).map((r) => ({
      userId: Number(getField(r, 'UserID') ?? 0),
      user: String(getField(r, 'UserName') ?? ''),
      service: String(getField(r, 'ServiceName') ?? ''),
      operatorId: Number(getField(r, 'OperatorID') ?? 0),
      operator: String(getField(r, 'OperatorName') ?? ''),
      chips: providerChips(r),
    }));
    const q = query.trim().toLowerCase();
    return q ? rows.filter((r) => `${r.user} ${r.service} ${r.operator}`.toLowerCase().includes(q)) : rows;
  }, [allQ.data, query]);

  // ── Add / edit view ─────────────────────────────────────────────────────────

  if (mode === 'add') {
    return (
      <>
        <PageHeader title="New User-wise Sequence" subtitle="Override the default provider switching for one user" icon={pickIcon(def.title)} />

        <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
          <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
            <Autocomplete
              label="User"
              placeholder="Search user…"
              loading={usersQ.isLoading}
              value={userId}
              options={(usersQ.data ?? []).map((u) => {
                const id = String(getField(u, 'UserMasterID', 'UserID') ?? '');
                return { value: id, label: userDisplay(u) };
              })}
              onChange={(v) => setUserId(String(v))}
            />
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
            <div className="flex items-end gap-2 lg:col-span-2">
              <Button variant="outline" onClick={() => setMode('list')}>
                <ArrowLeft className="size-4" />
                Back to list
              </Button>
              <Button
                variant="danger"
                onClick={() => {
                  if (window.confirm('Reset ALL user-wise sequences for every user?')) resetAllMut.mutate();
                }}
                loading={resetAllMut.isPending}
              >
                Reset all user-wise
              </Button>
            </div>
          </div>
        </div>

        <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <th className="w-10 px-4 py-3">#</th>
                  <th className="w-56 px-4 py-3">Operator</th>
                  <th className="px-4 py-3">Provider sequence (this user)</th>
                  <th className="w-44 px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {userId === '' || serviceId === '' ? (
                  <tr>
                    <td colSpan={4}>
                      <EmptyState title="Pick a user and service" description="Choose both above to load the operator list." />
                    </td>
                  </tr>
                ) : gridQ.isFetching ? (
                  <tr>
                    <td colSpan={4} className="px-4 py-8 text-center text-sm text-slate-400">
                      Loading…
                    </td>
                  </tr>
                ) : (gridQ.data ?? []).length === 0 ? (
                  <tr>
                    <td colSpan={4}>
                      <EmptyState title="No operators" description="No active operators found for this service." />
                    </td>
                  </tr>
                ) : (
                  (gridQ.data ?? []).map((r, i) => {
                    const operatorId = Number(getField(r, 'OperatorID') ?? 0);
                    const operatorName = String(getField(r, 'OperatorName') ?? '');
                    const chips = providerChips(r);
                    return (
                      <tr key={operatorId} className="border-b border-slate-100 last:border-0 even:bg-slate-50/40">
                        <td className="px-4 py-3 text-slate-500">{i + 1}</td>
                        <td className="px-4 py-3 font-medium text-slate-800">{operatorName}</td>
                        <td className="px-4 py-3">
                          <ChipRail chips={chips} />
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex justify-end gap-1">
                            <Button size="sm" variant="ghost" onClick={() => setEditing({ operatorId, operatorName, picked: chips })}>
                              <Pencil className="size-4" />
                              Edit
                            </Button>
                            {chips.length > 0 && (
                              <Button
                                size="sm"
                                variant="ghost"
                                className="text-danger-600 hover:bg-danger-50"
                                onClick={() => {
                                  if (window.confirm(`Set ${operatorName} back to the default sequence for this user?`))
                                    deleteMut.mutate({ userId: Number(userId), operatorId });
                                }}
                              >
                                Set default
                              </Button>
                            )}
                          </div>
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>

        {editing && (
          <Modal open onClose={() => setEditing(null)} title={`Sequence — ${editing.operatorName}`} size="lg">
            <div className="space-y-4">
              <div>
                <p className="mb-2 text-sm font-medium text-slate-700">
                  Selected providers <span className="text-xs font-normal text-slate-400">(click to remove — order = priority)</span>
                </p>
                {editing.picked.length === 0 ? (
                  <p className="text-xs text-slate-400">Nothing selected — pick providers below in the order they should be tried.</p>
                ) : (
                  <div className="flex flex-wrap gap-1.5">
                    {editing.picked.map((p, i) => (
                      <button
                        key={`${p.ProviderID}-${i}`}
                        type="button"
                        onClick={() =>
                          setEditing({ ...editing, picked: editing.picked.filter((x) => x.ProviderID !== p.ProviderID) })
                        }
                        className="inline-flex items-center gap-1 rounded-md bg-emerald-50 px-2.5 py-1.5 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200 hover:bg-emerald-100"
                      >
                        <span className="tabular-nums">{i + 1}</span>
                        {p.ProviderName}
                        <X className="size-3" />
                      </button>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <p className="mb-2 text-sm font-medium text-slate-700">Available providers</p>
                <div className="flex max-h-48 flex-wrap gap-1.5 overflow-y-auto rounded-lg border border-slate-200 p-2">
                  {(providersQ.data ?? [])
                    .filter((p) => !editing.picked.some((x) => x.ProviderID === Number(getField(p, 'ProviderID'))))
                    .map((p, i) => {
                      const id = Number(getField(p, 'ProviderID') ?? 0);
                      const name = String(getField(p, 'ProviderName') ?? id);
                      return (
                        <button
                          key={`${id}-${i}`}
                          type="button"
                          onClick={() => setEditing({ ...editing, picked: [...editing.picked, { ProviderID: id, ProviderName: name }] })}
                          className="rounded-full border border-slate-200 bg-slate-50 px-2.5 py-1 text-[11px] font-semibold text-slate-600 hover:bg-brand-50 hover:text-brand-700"
                        >
                          + {name}
                        </button>
                      );
                    })}
                  {providersQ.isLoading && <span className="text-xs text-slate-400">Loading…</span>}
                </div>
              </div>

              <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
                <Button variant="outline" onClick={() => setEditing(null)}>
                  Cancel
                </Button>
                <Button
                  loading={insertMut.isPending}
                  onClick={() => insertMut.mutate({ operatorId: editing.operatorId, picked: editing.picked })}
                >
                  Update sequence
                </Button>
              </div>
            </div>
          </Modal>
        )}
      </>
    );
  }

  // ── List view ───────────────────────────────────────────────────────────────

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      <div className="mb-3 flex flex-wrap items-center justify-between gap-3">
        <div className="w-72 max-w-full">
          <Input placeholder="Search user / service / operator…" value={query} onChange={(e) => setQuery(e.target.value)} />
        </div>
        <Button onClick={() => setMode('add')}>
          <Plus className="size-4" />
          New User-wise Add
        </Button>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="w-10 px-4 py-3">#</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Service</th>
                <th className="px-4 py-3">Operator</th>
                <th className="px-4 py-3">Provider</th>
                <th className="w-24 px-4 py-3 text-right">Delete</th>
              </tr>
            </thead>
            <tbody>
              {allQ.isFetching && listRows.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-8 text-center text-sm text-slate-400">
                    Loading…
                  </td>
                </tr>
              ) : listRows.length === 0 ? (
                <tr>
                  <td colSpan={6}>
                    <EmptyState
                      title="No user-wise switching yet"
                      description='Use "New User-wise Add" to give a user their own provider sequence.'
                    />
                  </td>
                </tr>
              ) : (
                listRows.map((r, i) => (
                  <tr key={`${r.userId}-${r.operatorId}`} className="border-b border-slate-100 last:border-0 even:bg-slate-50/40 hover:bg-brand-50/40">
                    <td className="px-4 py-3 text-slate-500">{i + 1}</td>
                    <td className="px-4 py-3 font-medium text-slate-800">{r.user}</td>
                    <td className="px-4 py-3 text-slate-600">{r.service}</td>
                    <td className="px-4 py-3 text-slate-600">{r.operator}</td>
                    <td className="px-4 py-3">
                      <ChipRail chips={r.chips} />
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end">
                        <Button
                          size="sm"
                          variant="ghost"
                          className="text-danger-600 hover:bg-danger-50"
                          loading={deleteMut.isPending && deleteMut.variables?.userId === r.userId && deleteMut.variables?.operatorId === r.operatorId}
                          onClick={() => {
                            if (window.confirm('Are you sure to delete this user-wise switching?'))
                              deleteMut.mutate({ userId: r.userId, operatorId: r.operatorId });
                          }}
                        >
                          <Trash2 className="size-4" />
                          Delete
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        {listRows.length > 0 && (
          <div className="border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
            Showing <span className="font-semibold text-slate-700 tabular-nums">{listRows.length}</span> row(s)
          </div>
        )}
      </div>
    </>
  );
}
