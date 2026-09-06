import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import type { CrudConfig } from './crudTypes';

type Row = Record<string, unknown>;

/**
 * Encapsulates the list + Add + field-level Edit + Delete behaviour for a CrudConfig.
 * Shared by the generic CrudScreen and bespoke screens (e.g. OperatorsScreen) so the
 * mutation/diff logic lives in exactly one place.
 */
export function useCrud(crud: CrudConfig) {
  const toast = useToast();
  const qc = useQueryClient();
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Row | null>(null);

  const { data: rows = [], isFetching } = useQuery({
    queryKey: ['crud', crud.list.endpoint, crud.list.payload],
    queryFn: () => fetchList(crud.list.endpoint, crud.list.payload ?? {}),
  });
  const refresh = () => qc.invalidateQueries({ queryKey: ['crud', crud.list.endpoint] });

  const createMut = useMutation({
    mutationFn: (values: Row) => apiPost(crud.create!.endpoint, values),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(crud.create?.successMessage ?? 'Created successfully.');
        setAdding(false);
        refresh();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Field-level update: one Update call per changed field.
  const updateMut = useMutation({
    mutationFn: async (values: Row): Promise<{ ok: boolean; message: string }> => {
      const row = editing!;
      // Object mode: post the whole record in one call.
      if (crud.update!.mode === 'object') {
        const res = await apiPost(crud.update!.endpoint, { [crud.idField]: row[crud.idField], ...values });
        return res.ok ? { ok: true, message: 'Updated.' } : { ok: false, message: res.message };
      }
      const changed = crud.update!.fields.filter((f) => String(values[f.name] ?? '') !== String(row[f.name] ?? ''));
      if (changed.length === 0) return { ok: true, message: 'No changes.' };
      const results = await Promise.all(
        changed.map((f) =>
          apiPost(crud.update!.endpoint, {
            [crud.idField]: row[crud.idField],
            FieldName: f.name,
            OldValue: String(row[f.name] ?? ''),
            NewValue: String(values[f.name] ?? ''),
          }),
        ),
      );
      const failed = results.find((r) => !r.ok);
      return failed ? { ok: false, message: failed.message } : { ok: true, message: 'Updated.' };
    },
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(crud.update?.successMessage ?? 'Updated successfully.');
        setEditing(null);
        refresh();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (id: unknown) => apiPost(crud.remove!.endpoint, { [crud.idField]: id }),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(crud.remove?.successMessage ?? 'Deleted successfully.');
        refresh();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const onDelete = (row: Row) => {
    if (window.confirm('Delete this record? This cannot be undone.')) deleteMut.mutate(row[crud.idField]);
  };

  // Active/Inactive toggle — reuses the master's Update endpoint (mode-aware), no extra config.
  const toggleMut = useMutation({
    mutationFn: (row: Row) => {
      if (!crud.update) return Promise.resolve({ ok: false, message: 'Toggle not supported.' });
      const cur = Boolean(getField(row, 'IsActive'));
      let payload: Row;
      if (crud.update.mode === 'object') {
        payload = { [crud.idField]: row[crud.idField], IsActive: !cur };
        for (const f of crud.update.fields) if (f.name !== 'IsActive') payload[f.name] = getField(row, f.name) ?? '';
      } else {
        payload = { [crud.idField]: row[crud.idField], FieldName: 'IsActive', OldValue: String(cur), NewValue: String(!cur) };
      }
      return apiPost(crud.update.endpoint, payload);
    },
    onSuccess: (res) => {
      if (res.ok) {
        toast.success('Status updated.');
        refresh();
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return { rows, isFetching, adding, setAdding, editing, setEditing, createMut, updateMut, deleteMut, toggleMut, onDelete };
}
