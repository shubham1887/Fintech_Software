import { Download, Pencil, Plus, Power, Search, Trash2 } from 'lucide-react';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { DynamicTable } from '@/components/ui/DynamicTable';
import { Modal } from '@/components/ui/Modal';
import { DynamicForm } from '@/components/ui/DynamicForm';
import { useCrud } from './useCrud';
import type { CrudConfig } from './crudTypes';

type Row = Record<string, unknown>;

/**
 * The list + Add + Edit + Delete body of a CRUD master, WITHOUT a PageHeader — so it can be
 * dropped inside a tab or section (e.g. the Communication Setup screen). `label` names the
 * record in the Add/Edit dialog titles and the CSV export. Shares all mutation logic with the
 * generic CrudScreen via useCrud.
 */
export function CrudPanel({ label, crud, addLabel }: { label: string; crud: CrudConfig; addLabel?: string }) {
  const { rows, isFetching, adding, setAdding, editing, setEditing, createMut, updateMut, deleteMut, toggleMut, onDelete } =
    useCrud(crud);
  const { query, setQuery, filtered } = useClientSearch(rows);

  const rowActions =
    crud.update || crud.remove
      ? (row: Row) => (
          <div className="flex justify-end gap-1">
            {crud.update && getField(row, 'IsActive') !== undefined && (
              <Button variant="ghost" size="sm" loading={toggleMut.isPending} onClick={() => toggleMut.mutate(row)} title="Toggle active status">
                <Power className={cn('size-4', getField(row, 'IsActive') ? 'text-success-600' : 'text-slate-400')} />
                {getField(row, 'IsActive') ? 'Active' : 'Inactive'}
              </Button>
            )}
            {crud.update && (
              <Button variant="ghost" size="sm" onClick={() => setEditing(row)}>
                <Pencil className="size-4" /> Edit
              </Button>
            )}
            {crud.remove && (
              <Button variant="ghost" size="sm" className="text-danger-600 hover:bg-danger-50" loading={deleteMut.isPending} onClick={() => onDelete(row)}>
                <Trash2 className="size-4" /> Delete
              </Button>
            )}
          </div>
        )
      : undefined;

  return (
    <div>
      <div className="mb-3 flex items-center gap-3">
        <div className="w-full max-w-xs">
          <Input placeholder="Search…" value={query} onChange={(e) => setQuery(e.target.value)} leftIcon={<Search className="size-4" />} />
        </div>
        <div className="flex-1" />
        <span className="hidden text-sm text-slate-400 sm:inline">{filtered.length} rows</span>
        <Button variant="outline" size="sm" onClick={() => exportToCsv(filtered, label)} disabled={filtered.length === 0}>
          <Download className="size-4" /> Export
        </Button>
        {crud.create && (
          <Button size="sm" onClick={() => setAdding(true)}>
            <Plus className="size-4" /> {addLabel ?? `Add ${label}`}
          </Button>
        )}
      </div>

      <DynamicTable
        rows={filtered}
        loading={isFetching}
        emptyDescription="No records yet — add one to get started."
        actions={rowActions}
        columns={crud.columns}
      />

      {crud.create && (
        <Modal open={adding} onClose={() => setAdding(false)} title={`Add ${label}`} size="lg">
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
        <Modal open={Boolean(editing)} onClose={() => setEditing(null)} title={`Edit ${label}`} size="lg">
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
    </div>
  );
}
