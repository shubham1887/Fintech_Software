import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FileCheck2, Pencil, Plus, RefreshCw, Trash2 } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField as get } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { Modal } from '@/components/ui/Modal';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

interface Draft {
  VerificationDocumentConfigID: number;
  DocumentName: string;
  DocumentCode: string;
  DocumentType: string;
  IsRequired: boolean;
  IsActive: boolean;
  DisplayOrder: number;
}

const DOC_TYPES = ['Identity', 'Business', 'Address', 'Bank', 'Other'];

const emptyDraft = (order: number): Draft => ({
  VerificationDocumentConfigID: 0,
  DocumentName: '',
  DocumentCode: '',
  DocumentType: 'Identity',
  IsRequired: false,
  IsActive: true,
  DisplayOrder: order,
});

const toDraft = (r: Row): Draft => ({
  VerificationDocumentConfigID: Number(get(r, 'VerificationDocumentConfigID') ?? 0),
  DocumentName: String(get(r, 'DocumentName') ?? ''),
  DocumentCode: String(get(r, 'DocumentCode') ?? ''),
  DocumentType: String(get(r, 'DocumentType') ?? '') || 'Identity',
  IsRequired: Boolean(get(r, 'IsRequired')),
  IsActive: Boolean(get(r, 'IsActive')),
  DisplayOrder: Number(get(r, 'DisplayOrder') ?? 0),
});

/** Compact On/Off pill switch. */
function Switch({ on, onChange, disabled }: { on: boolean; onChange: (v: boolean) => void; disabled?: boolean }) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={on}
      disabled={disabled}
      onClick={() => onChange(!on)}
      className={cn(
        'relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition-colors disabled:opacity-50',
        on ? 'bg-brand-600' : 'bg-slate-300',
      )}
    >
      <span className={cn('inline-block size-4 transform rounded-full bg-white shadow transition-transform', on ? 'translate-x-4' : 'translate-x-0.5')} />
    </button>
  );
}

/**
 * Verification Documents — admin master for Manual Verification. Fully database-driven: add any
 * document here and it appears on the user's Manual Verification screen, no code change. Manage
 * name / code / type / required / active / display order. (VerificationDocument/* endpoints.)
 */
export function VerificationDocumentsScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const [edit, setEdit] = useState<Draft | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<Row | null>(null);

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['verification-docs'],
    queryFn: () => fetchList('VerificationDocument/GetAll', {}),
  });
  const sorted = useMemo(
    () => [...rows].sort((a, b) => (Number(get(a, 'DisplayOrder')) || 0) - (Number(get(b, 'DisplayOrder')) || 0)),
    [rows],
  );
  const nextOrder = useMemo(
    () => (sorted.length ? Math.max(...sorted.map((r) => Number(get(r, 'DisplayOrder')) || 0)) + 1 : 1),
    [sorted],
  );

  const invalidate = () => qc.invalidateQueries({ queryKey: ['verification-docs'] });

  const saveMut = useMutation({
    mutationFn: (d: Draft) => {
      const body = {
        DocumentName: d.DocumentName.trim(),
        DocumentCode: d.DocumentCode.trim().toUpperCase(),
        DocumentType: d.DocumentType,
        IsRequired: d.IsRequired,
        IsActive: d.IsActive,
        DisplayOrder: Number(d.DisplayOrder) || 0,
      };
      return d.VerificationDocumentConfigID > 0
        ? apiPost('VerificationDocument/Update', { VerificationDocumentConfigID: d.VerificationDocumentConfigID, ...body })
        : apiPost('VerificationDocument/Create', body);
    },
    onSuccess: (res) => {
      if (res.ok) { toast.success('Document saved.'); setEdit(null); invalidate(); }
      else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Inline flag toggle (Active / Required) → full-object Update.
  const toggleMut = useMutation({
    mutationFn: (v: { row: Row; patch: Partial<Draft> }) => {
      const d = { ...toDraft(v.row), ...v.patch };
      return apiPost('VerificationDocument/Update', {
        VerificationDocumentConfigID: d.VerificationDocumentConfigID,
        DocumentName: d.DocumentName, DocumentCode: d.DocumentCode, DocumentType: d.DocumentType,
        IsRequired: d.IsRequired, IsActive: d.IsActive, DisplayOrder: d.DisplayOrder,
      });
    },
    onSuccess: (res) => { if (res.ok) invalidate(); else toast.error(res.message); },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => apiPost('VerificationDocument/Delete', { VerificationDocumentConfigID: id }),
    onSuccess: (res) => {
      if (res.ok) { toast.success('Document deleted.'); setConfirmDelete(null); invalidate(); }
      else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const canSave = edit && edit.DocumentName.trim().length > 0;

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={FileCheck2}
        actions={
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
              <RefreshCw className="size-4" />
            </Button>
            <Button size="sm" onClick={() => setEdit(emptyDraft(nextOrder))}>
              <Plus className="size-4" />
              Add document
            </Button>
          </div>
        }
      />

      <div className="mb-4 rounded-card border border-info-500/20 bg-info-50 px-4 py-3 text-sm text-info-600">
        These documents drive <span className="font-medium">Manual Verification</span>. When Key Setting →
        <span className="font-medium"> E-Verification</span> is Off, users are asked to upload every document
        marked Active here (Required ones are mandatory). Add a new regulatory document any time — it appears
        for users automatically, no code change needed.
      </div>

      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">Order</th>
                <th className="px-4 py-3">Document</th>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Type</th>
                <th className="px-4 py-3">Required</th>
                <th className="px-4 py-3">Active</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isFetching && rows.length === 0 ? (
                Array.from({ length: 4 }).map((_, i) => (
                  <tr key={i} className="border-b border-slate-100 last:border-0">
                    <td className="px-4 py-3" colSpan={7}><Skeleton className="h-8 w-full" /></td>
                  </tr>
                ))
              ) : sorted.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <EmptyState icon={FileCheck2} title="No documents configured" description="Add the documents users must upload for manual verification." />
                  </td>
                </tr>
              ) : (
                sorted.map((r) => {
                  const id = Number(get(r, 'VerificationDocumentConfigID') ?? 0);
                  return (
                    <tr key={id} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      <td className="px-4 py-3 tabular-nums text-slate-400">{String(get(r, 'DisplayOrder') ?? '')}</td>
                      <td className="px-4 py-3 font-medium text-slate-800">{String(get(r, 'DocumentName') ?? '—')}</td>
                      <td className="px-4 py-3"><span className="rounded bg-slate-100 px-1.5 py-0.5 font-mono text-xs text-slate-600">{String(get(r, 'DocumentCode') ?? '—')}</span></td>
                      <td className="px-4 py-3 text-slate-600">{String(get(r, 'DocumentType') ?? '—') || '—'}</td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <Switch on={Boolean(get(r, 'IsRequired'))} disabled={toggleMut.isPending} onChange={(v) => toggleMut.mutate({ row: r, patch: { IsRequired: v } })} />
                          <Badge tone={Boolean(get(r, 'IsRequired')) ? 'brand' : 'neutral'}>{Boolean(get(r, 'IsRequired')) ? 'Required' : 'Optional'}</Badge>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <Switch on={Boolean(get(r, 'IsActive'))} disabled={toggleMut.isPending} onChange={(v) => toggleMut.mutate({ row: r, patch: { IsActive: v } })} />
                          <Badge tone={Boolean(get(r, 'IsActive')) ? 'success' : 'neutral'}>{Boolean(get(r, 'IsActive')) ? 'Active' : 'Disabled'}</Badge>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1">
                          <button type="button" onClick={() => setEdit(toDraft(r))} className="grid size-7 place-items-center rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600" title="Edit">
                            <Pencil className="size-4" />
                          </button>
                          <button type="button" onClick={() => setConfirmDelete(r)} className="grid size-7 place-items-center rounded-lg text-slate-400 hover:bg-danger-50 hover:text-danger-600" title="Delete">
                            <Trash2 className="size-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Add / edit modal */}
      <Modal open={Boolean(edit)} onClose={() => setEdit(null)} title={edit?.VerificationDocumentConfigID ? 'Edit document' : 'Add document'}>
        {edit && (
          <div className="space-y-4">
            <Input label="Document name *" placeholder="e.g. PAN Card" value={edit.DocumentName} onChange={(e) => setEdit({ ...edit, DocumentName: e.target.value })} />
            <div className="grid grid-cols-2 gap-4">
              <Input label="Document code" placeholder="PAN" value={edit.DocumentCode} onChange={(e) => setEdit({ ...edit, DocumentCode: e.target.value.toUpperCase() })} hint="Auto-derived from name if left blank" />
              <Select label="Document type" value={edit.DocumentType} onChange={(e) => setEdit({ ...edit, DocumentType: e.target.value })}>
                {DOC_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </Select>
            </div>
            <Input label="Display order" type="number" min="0" value={String(edit.DisplayOrder)} onChange={(e) => setEdit({ ...edit, DisplayOrder: Number(e.target.value) || 0 })} />
            <div className="flex gap-6">
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <Switch on={edit.IsRequired} onChange={(v) => setEdit({ ...edit, IsRequired: v })} /> Required
              </label>
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <Switch on={edit.IsActive} onChange={(v) => setEdit({ ...edit, IsActive: v })} /> Active
              </label>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="outline" size="sm" onClick={() => setEdit(null)}>Cancel</Button>
              <Button size="sm" loading={saveMut.isPending} disabled={!canSave} onClick={() => edit && saveMut.mutate(edit)}>Save</Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete confirm */}
      <Modal open={Boolean(confirmDelete)} onClose={() => setConfirmDelete(null)} title="Delete document">
        {confirmDelete && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              Delete <span className="font-medium text-slate-800">{String(get(confirmDelete, 'DocumentName') ?? '')}</span>?
              Users will no longer be asked for it. This cannot be undone.
            </p>
            <div className="flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={() => setConfirmDelete(null)}>Cancel</Button>
              <Button variant="danger" size="sm" loading={deleteMut.isPending} onClick={() => deleteMut.mutate(Number(get(confirmDelete, 'VerificationDocumentConfigID') ?? 0))}>Delete</Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
