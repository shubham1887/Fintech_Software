import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Pencil, Plus, Power, SlidersHorizontal, Trash2 } from 'lucide-react';
import { Select } from '@/components/ui/Select';
import { fetchListPage } from '@/api/list.api';
import { useCsvExport } from '@/lib/exportAll';
import { useClientSearch, filterRows } from '@/lib/useClientSearch';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { DynamicTable } from '@/components/ui/DynamicTable';
import { TableToolbar } from '@/components/ui/TableToolbar';
import { Modal } from '@/components/ui/Modal';
import { DynamicForm } from '@/components/ui/DynamicForm';
import { pickIcon } from '@/components/layout/iconMap';
import { useCrud } from './useCrud';
import type { CrudConfig } from './crudTypes';

type Row = Record<string, unknown>;

export function CrudScreen({ title, subtitle, crud }: { title: string; subtitle?: string; crud: CrudConfig }) {
  const navigate = useNavigate();
  const { rows, isFetching, adding, setAdding, editing, setEditing, createMut, updateMut, deleteMut, toggleMut, onDelete } =
    useCrud(crud);
  const { query, setQuery, filtered: searched } = useClientSearch(rows);
  const { exporting, runExport } = useCsvExport();

  // Column filters (crud.filters): each one narrows to rows whose value equals the pick.
  const [picks, setPicks] = useState<Record<string, string>>({});
  const filterOptions = useMemo(() => {
    const out: Record<string, string[]> = {};
    for (const f of crud.filters ?? []) {
      const values = new Set<string>();
      for (const r of rows) {
        const v = getField(r, f.key);
        if (v !== null && v !== undefined && String(v).trim() !== '') values.add(String(v));
      }
      out[f.key] = [...values].sort((a, b) => a.localeCompare(b));
    }
    return out;
  }, [rows, crud.filters]);
  const applyPicks = (list: Row[]) =>
    list.filter((r) => Object.entries(picks).every(([k, v]) => !v || String(getField(r, k) ?? '') === v));
  const filtered = useMemo(() => {
    const narrowed = applyPicks(searched);
    // A "_serial" column is the row's position in the list, not a stored field.
    return crud.columns?.some((c) => c.key === '_serial')
      ? narrowed.map((r, i) => ({ ...r, _serial: i + 1 }))
      : narrowed;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searched, picks, crud.columns]);

  // Every record the master holds that matches the search — the table reads one page of it.
  const exportAll = () => runExport({
    filename: title,
    onScreen: filtered,
    fetchAll: (skip, take) => fetchListPage(crud.list.endpoint, crud.list.payload ?? {}, skip, take),
    filter: (all) => applyPicks(filterRows(all, query)),
  });

  const rowActions =
    crud.update || crud.remove || crud.links
      ? (row: Row) => (
          <div className="flex justify-end gap-1">
            {crud.links?.map((lnk) => (
              <Button key={lnk.label} variant="ghost" size="sm" onClick={() => navigate(lnk.to(row))} title={lnk.label}>
                <SlidersHorizontal className="size-4" />
                {lnk.label}
              </Button>
            ))}
            {crud.update && getField(row, 'IsActive') !== undefined && (
              <Button
                variant="ghost"
                size="sm"
                loading={toggleMut.isPending}
                onClick={() => toggleMut.mutate(row)}
                title="Toggle active status"
              >
                <Power className={cn('size-4', getField(row, 'IsActive') ? 'text-success-600' : 'text-slate-400')} />
                {getField(row, 'IsActive') ? 'Active' : 'Inactive'}
              </Button>
            )}
            {crud.update && (
              <Button variant="ghost" size="sm" onClick={() => setEditing(row)}>
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
                onClick={() => onDelete(row)}
              >
                <Trash2 className="size-4" />
                Delete
              </Button>
            )}
          </div>
        )
      : undefined;

  return (
    <>
      <PageHeader
        title={title}
        subtitle={subtitle}
        icon={pickIcon(title)}
        actions={
          crud.create && (
            <Button onClick={() => setAdding(true)}>
              <Plus className="size-4" />
              Add new
            </Button>
          )
        }
      />

      <TableToolbar query={query} onQuery={setQuery} count={filtered.length} onExport={exportAll} exportLoading={exporting} />

      {(crud.filters?.length ?? 0) > 0 && (
        <div className="mb-3 grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {crud.filters!.map((f) => (
            <Select
              key={f.key}
              label={f.label}
              value={picks[f.key] ?? ''}
              onChange={(e) => setPicks((p) => ({ ...p, [f.key]: e.target.value }))}
            >
              <option value="">{`All (${filterOptions[f.key]?.length ?? 0})`}</option>
              {(filterOptions[f.key] ?? []).map((v) => (
                <option key={v} value={v}>{v}</option>
              ))}
            </Select>
          ))}
        </div>
      )}

      <DynamicTable
        rows={filtered}
        loading={isFetching}
        emptyDescription="No records yet — add one to get started."
        actions={rowActions}
        columns={crud.columns}
      />

      {crud.create && (
        <Modal open={adding} onClose={() => setAdding(false)} title={`Add ${title}`} size="lg">
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
        <Modal open={Boolean(editing)} onClose={() => setEditing(null)} title={`Edit ${title}`} size="lg">
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
