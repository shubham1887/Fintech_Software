import { useNavigate } from 'react-router-dom';
import { Pencil, Plus, Power, SlidersHorizontal, Trash2 } from 'lucide-react';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
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
  const { query, setQuery, filtered } = useClientSearch(rows);

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

      <TableToolbar query={query} onQuery={setQuery} count={filtered.length} onExport={() => exportToCsv(filtered, title)} />

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
