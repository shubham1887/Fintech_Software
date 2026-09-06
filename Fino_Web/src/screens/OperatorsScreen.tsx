import { useMemo } from 'react';
import { Pencil, Plus, Power, Radio, Signal, Trash2, Wifi } from 'lucide-react';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
import { getField as get } from '@/lib/row';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Card } from '@/components/ui/Card';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { StatTile } from '@/components/ui/StatTile';
import { TableToolbar } from '@/components/ui/TableToolbar';
import { Modal } from '@/components/ui/Modal';
import { DynamicForm } from '@/components/ui/DynamicForm';
import { useCrud } from './useCrud';
import type { CustomScreenProps } from './registry';

const TILE_TONES = [
  'bg-brand-500',
  'bg-emerald-500',
  'bg-amber-500',
  'bg-rose-500',
  'bg-violet-500',
  'bg-sky-500',
  'bg-cyan-500',
  'bg-indigo-500',
];
function tileTone(seed: string) {
  let h = 0;
  for (let i = 0; i < seed.length; i++) h = (h * 31 + seed.charCodeAt(i)) >>> 0;
  return TILE_TONES[h % TILE_TONES.length];
}
function initials(name: string) {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase() || 'OP';
}
function inr(v: unknown) {
  const n = Number(v);
  return Number.isFinite(n) && n > 0 ? `₹${n.toLocaleString('en-IN')}` : '—';
}

export function OperatorsScreen({ def }: CustomScreenProps) {
  const crud = def.crud;
  const { rows, isFetching, adding, setAdding, editing, setEditing, createMut, updateMut, deleteMut, toggleMut, onDelete } = useCrud(
    crud ?? { idField: 'OperatorID', list: { endpoint: 'Operator/GetAll', payload: {} } },
  );
  const { query, setQuery, filtered } = useClientSearch(rows);

  const stats = useMemo(() => {
    const total = rows.length;
    const active = rows.filter((r) => Boolean(get(r, 'IsActive'))).length;
    const bbps = rows.filter((r) => Boolean(get(r, 'IsBBPS'))).length;
    return { total, active, bbps };
  }, [rows]);

  if (!crud) return null;

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={Radio}
        actions={
          crud.create && (
            <Button onClick={() => setAdding(true)}>
              <Plus className="size-4" />
              Add operator
            </Button>
          )
        }
      />

      <div className="mb-5 grid grid-cols-2 gap-4 lg:grid-cols-3">
        <StatTile label="Total operators" value={stats.total} icon={Radio} tone="bg-brand-50 text-brand-600" loading={isFetching} />
        <StatTile label="Active" value={stats.active} icon={Signal} tone="bg-success-50 text-success-600" loading={isFetching} />
        <StatTile label="BBPS enabled" value={stats.bbps} icon={Wifi} tone="bg-info-50 text-info-600" loading={isFetching} />
      </div>

      <TableToolbar query={query} onQuery={setQuery} count={filtered.length} onExport={() => exportToCsv(filtered, def.title)} />

      {isFetching ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Card key={i} className="p-4">
              <Skeleton className="h-24 w-full" />
            </Card>
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <Card>
          <EmptyState
            icon={Radio}
            title="No operators yet"
            description="Add an operator to start routing recharges and bill payments."
            action={
              crud.create && (
                <Button onClick={() => setAdding(true)}>
                  <Plus className="size-4" />
                  Add operator
                </Button>
              )
            }
          />
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {filtered.map((r, i) => {
            const name = String(get(r, 'OperatorName', 'Name') ?? '—');
            const opcode = String(get(r, 'Opcode') ?? '');
            const active = Boolean(get(r, 'IsActive'));
            const bbps = Boolean(get(r, 'IsBBPS'));
            const min = get(r, 'MinimumRechargeAmount');
            const max = get(r, 'MaximumRechargeAmount');
            return (
              <Card key={i} className="group flex flex-col gap-3 p-4 transition-shadow hover:shadow-lg">
                <div className="flex items-start gap-3">
                  <span
                    className={cn(
                      'grid size-12 shrink-0 place-items-center rounded-xl text-base font-bold text-white shadow-sm',
                      tileTone(name),
                    )}
                  >
                    {initials(name)}
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-semibold text-slate-800">{name}</p>
                    {opcode && <p className="text-xs uppercase tracking-wide text-slate-400">{opcode}</p>}
                  </div>
                  <Badge tone={active ? 'success' : 'danger'}>{active ? 'Active' : 'Off'}</Badge>
                </div>

                <div className="flex flex-wrap gap-1.5">
                  {bbps && <Badge tone="info">BBPS</Badge>}
                  {Boolean(get(r, 'IsBillFetch')) && <Badge tone="neutral">Bill fetch</Badge>}
                  {Boolean(get(r, 'IsDenominationApplicable')) && <Badge tone="warning">Denomination</Badge>}
                </div>

                <div className="grid grid-cols-2 gap-2 rounded-lg bg-slate-50 p-2.5 text-sm">
                  <div>
                    <p className="text-xs text-slate-400">Min</p>
                    <p className="font-medium text-slate-700">{inr(min)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-slate-400">Max</p>
                    <p className="font-medium text-slate-700">{inr(max)}</p>
                  </div>
                </div>

                <div className="mt-auto flex items-center justify-end gap-1 border-t border-slate-100 pt-2">
                  {crud.update && (
                    <Button variant="ghost" size="sm" loading={toggleMut.isPending} onClick={() => toggleMut.mutate(r)} title="Toggle active">
                      <Power className={cn('size-4', active ? 'text-success-600' : 'text-slate-400')} />
                      {active ? 'On' : 'Off'}
                    </Button>
                  )}
                  {crud.update && (
                    <Button variant="ghost" size="sm" onClick={() => setEditing(r)}>
                      <Pencil className="size-4" />
                      Edit
                    </Button>
                  )}
                  {crud.remove && (
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-danger-600 hover:bg-danger-50"
                      loading={deleteMut.isPending}
                      onClick={() => onDelete(r)}
                    >
                      <Trash2 className="size-4" />
                      Delete
                    </Button>
                  )}
                </div>
              </Card>
            );
          })}
        </div>
      )}

      {crud.create && (
        <Modal open={adding} onClose={() => setAdding(false)} title={`Add ${def.title}`} size="lg">
          <DynamicForm
            fields={crud.create.fields}
            submitting={createMut.isPending}
            submitLabel="Create"
            onSubmit={(v) => createMut.mutate(v)}
            onCancel={() => setAdding(false)}
          />
        </Modal>
      )}

      {crud.update && (
        <Modal open={Boolean(editing)} onClose={() => setEditing(null)} title={`Edit ${def.title}`} size="lg">
          {editing && (
            <DynamicForm
              fields={crud.update.fields}
              initial={editing}
              submitting={updateMut.isPending}
              submitLabel="Save changes"
              onSubmit={(v) => updateMut.mutate(v)}
              onCancel={() => setEditing(null)}
            />
          )}
        </Modal>
      )}
    </>
  );
}
