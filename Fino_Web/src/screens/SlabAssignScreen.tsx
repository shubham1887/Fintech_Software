import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Pencil, Plus, RotateCcw, Save, Trash2, X } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

const USER_LIST = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0, ShowDeleted: false };
const SLAB_LIST = { IsActive: true, PageNumber: 1, PageSize: 500 };

type Row = Record<string, unknown>;

/** "SD19 Firm Name [9999999999]" style label. */
function userDisplay(u: Row): string {
  const id = Number(getField(u, 'UserMasterID', 'UserID') ?? 0);
  const name = String(getField(u, 'FirmName') ?? '').trim() || String(getField(u, 'PersonName') ?? '').trim() || `User ${id}`;
  const mobile = String(getField(u, 'MobileNumber') ?? '').trim();
  return mobile ? `${name} [${mobile}]` : name;
}

/**
 * Slab Assign — assign a commission slab to a user. Inline form (Select User + Select Slab +
 * Add / Reset) on top, and the "Slab and User list" below with User / Role / Slab columns and
 * inline Edit (change slab) + Delete. User name & role are joined client-side from Users/Select.
 */
export function SlabAssignScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const [userId, setUserId] = useState('');
  const [slabId, setSlabId] = useState('');
  const [query, setQuery] = useState('');
  const [editing, setEditing] = useState<{ id: number; userId: number; slabId: string } | null>(null);

  const usersQ = useQuery({ queryKey: ['crud', 'Users/Select', USER_LIST], queryFn: () => fetchList('Users/Select', USER_LIST), staleTime: 60_000 });
  const slabsQ = useQuery({ queryKey: ['crud', 'Slabs/GetAll', SLAB_LIST], queryFn: () => fetchList('Slabs/GetAll', SLAB_LIST), staleTime: 60_000 });
  const listQ = useQuery({ queryKey: ['crud', 'UserSlabMapping/GetAll', {}], queryFn: () => fetchList('UserSlabMapping/GetAll', {}) });

  const userMeta = useMemo(() => {
    const m = new Map<number, { display: string; role: string }>();
    for (const u of usersQ.data ?? []) {
      const id = Number(getField(u, 'UserMasterID', 'UserID') ?? 0);
      if (!Number.isNaN(id)) m.set(id, { display: userDisplay(u), role: String(getField(u, 'UserType') ?? '') });
    }
    return m;
  }, [usersQ.data]);

  const slabName = useMemo(() => {
    const m = new Map<number, string>();
    for (const s of slabsQ.data ?? []) m.set(Number(getField(s, 'SlabMasterID')), String(getField(s, 'SlabName') ?? ''));
    return m;
  }, [slabsQ.data]);

  const rows = useMemo(() => {
    const enriched = (listQ.data ?? []).map((r) => {
      const uid = Number(getField(r, 'UserID') ?? 0);
      const sid = Number(getField(r, 'SlabMasterID') ?? 0);
      const meta = userMeta.get(uid);
      return {
        id: Number(getField(r, 'UserSlabMappingID') ?? 0),
        userId: uid,
        user: meta?.display ?? `User ${uid}`,
        role: meta?.role ?? '',
        slabId: sid,
        slab: String(getField(r, 'SlabName') ?? '') || slabName.get(sid) || '',
      };
    });
    const q = query.trim().toLowerCase();
    return q ? enriched.filter((e) => `${e.user} ${e.role} ${e.slab}`.toLowerCase().includes(q)) : enriched;
  }, [listQ.data, userMeta, slabName, query]);

  const refresh = () => qc.invalidateQueries({ queryKey: ['crud', 'UserSlabMapping/GetAll'] });

  const addMut = useMutation({
    mutationFn: () => apiPost('UserSlabMapping/Create', { UserID: Number(userId), SlabMasterID: Number(slabId) }),
    onSuccess: (res) => {
      if (res.ok) { toast.success('Slab assigned.'); setUserId(''); setSlabId(''); refresh(); } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const updateMut = useMutation({
    mutationFn: (v: { id: number; userId: number; slabId: number }) =>
      apiPost('UserSlabMapping/Update', { UserSlabMappingID: v.id, UserID: v.userId, SlabMasterID: v.slabId }),
    onSuccess: (res) => {
      if (res.ok) { toast.success('Assignment updated.'); setEditing(null); refresh(); } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => apiPost('UserSlabMapping/Delete', { UserSlabMappingID: id }),
    onSuccess: (res) => {
      if (res.ok) { toast.success('Assignment removed.'); refresh(); } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const slabOptions = (slabsQ.data ?? []).map((s, i) => {
    const id = String(getField(s, 'SlabMasterID') ?? '');
    return <option key={`${id}-${i}`} value={id}>{String(getField(s, 'SlabName') ?? id)}</option>;
  });

  const canAdd = userId !== '' && slabId !== '' && !addMut.isPending;

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      {/* Assign form */}
      <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
        <h2 className="mb-4 font-display text-base font-bold text-slate-900">Slab Assign</h2>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
          <div className="flex-1">
            <Select label="Select User" value={userId} onChange={(e) => setUserId(e.target.value)}>
              <option value="">{usersQ.isLoading ? 'Loading…' : 'Select User'}</option>
              {(usersQ.data ?? []).map((u, i) => {
                const id = String(getField(u, 'UserMasterID', 'UserID') ?? '');
                return <option key={`${id}-${i}`} value={id}>{userMeta.get(Number(id))?.display ?? id}</option>;
              })}
            </Select>
          </div>
          <div className="flex-1">
            <Select label="Select Slab" value={slabId} onChange={(e) => setSlabId(e.target.value)}>
              <option value="">{slabsQ.isLoading ? 'Loading…' : 'Select Slab'}</option>
              {slabOptions}
            </Select>
          </div>
          <div className="flex gap-2">
            <Button onClick={() => addMut.mutate()} loading={addMut.isPending} disabled={!canAdd}>
              <Plus className="size-4" />Add
            </Button>
            <Button variant="outline" onClick={() => { setUserId(''); setSlabId(''); }}>
              <RotateCcw className="size-4" />Reset
            </Button>
          </div>
        </div>
      </div>

      {/* List */}
      <div className="mb-3 flex items-center justify-between gap-3">
        <h2 className="font-display text-base font-bold text-slate-900">Slab and User list</h2>
        <div className="w-72 max-w-full">
          <Input placeholder="Search user / role / slab…" value={query} onChange={(e) => setQuery(e.target.value)} />
        </div>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">#</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Slab</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {listQ.isFetching && rows.length === 0 ? (
                <tr><td colSpan={5} className="px-4 py-8 text-center text-sm text-slate-400">Loading…</td></tr>
              ) : rows.length === 0 ? (
                <tr><td colSpan={5}><EmptyState title="No assignments yet" description="Assign a slab to a user using the form above." /></td></tr>
              ) : (
                rows.map((r, i) => (
                  <tr key={r.id} className="border-b border-slate-100 last:border-0 even:bg-slate-50/40 hover:bg-brand-50/40">
                    <td className="px-4 py-3 text-slate-500">{i + 1}</td>
                    <td className="px-4 py-3 font-medium text-slate-800">{r.user}</td>
                    <td className="px-4 py-3 text-slate-600">{r.role || '—'}</td>
                    <td className="px-4 py-3">
                      {editing?.id === r.id ? (
                        <div className="w-52">
                          <Select value={editing.slabId} onChange={(e) => setEditing({ ...editing, slabId: e.target.value })} className="h-9">
                            {slabOptions}
                          </Select>
                        </div>
                      ) : (
                        <span className="text-slate-700">{r.slab || '—'}</span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-1">
                        {editing?.id === r.id ? (
                          <>
                            <Button size="sm" loading={updateMut.isPending} onClick={() => updateMut.mutate({ id: r.id, userId: r.userId, slabId: Number(editing.slabId) })}>
                              <Save className="size-4" />Save
                            </Button>
                            <Button size="sm" variant="ghost" onClick={() => setEditing(null)}><X className="size-4" /></Button>
                          </>
                        ) : (
                          <>
                            <Button size="sm" variant="ghost" onClick={() => setEditing({ id: r.id, userId: r.userId, slabId: String(r.slabId) })}>
                              <Pencil className="size-4" />Edit
                            </Button>
                            <Button size="sm" variant="ghost" className="text-danger-600 hover:bg-danger-50" loading={deleteMut.isPending}
                              onClick={() => { if (window.confirm('Remove this slab assignment?')) deleteMut.mutate(r.id); }}>
                              <Trash2 className="size-4" />Delete
                            </Button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        {!listQ.isFetching && rows.length > 0 && (
          <div className="border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
            Showing <span className="font-semibold text-slate-700 tabular-nums">{rows.length}</span> {rows.length === 1 ? 'assignment' : 'assignments'}
          </div>
        )}
      </div>
    </>
  );
}
