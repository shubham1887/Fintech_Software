import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { IdCard, Pencil, RefreshCw, RotateCcw } from 'lucide-react';
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
  UserTypeID: number;
  UserType: string;
  UserCodeFormatID: number;
  Code: string;
  Pattern: string;
  SequenceLength: number;
  StartNumber: number;
  ResetPolicy: string;
  IsActive: boolean;
  /** Next running number, for the live example only. */
  NextSequence: number;
}

/** Ready-made shapes. Anything else can be typed in as a custom format. */
const PRESETS: { pattern: string; label: string }[] = [
  { pattern: '{CODE}{SEQ}', label: 'Code + number' },
  { pattern: '{SEQ}{CODE}', label: 'Number + code' },
  { pattern: '{CODE}{YYYY}{SEQ}', label: 'Code + year + number' },
  { pattern: '{CODE}{YY}{MM}{SEQ}', label: 'Code + year + month + number' },
  { pattern: 'USR-{CODE}-{YYYY}-{SEQ}', label: 'USR-CODE-YEAR-NUMBER' },
];

const RESET_OPTIONS = [
  { value: 'None', label: 'Never' },
  { value: 'Yearly', label: 'Every year' },
  { value: 'Monthly', label: 'Every month' },
];

/** Client-side mirror of the server renderer, so the format can be judged while it is typed. */
function renderCode(pattern: string, code: string, sequence: number, padLength: number): string {
  const now = new Date();
  const pad = Math.max(padLength || 1, 1);
  return (pattern || '{CODE}{SEQ}')
    .replace(/\{CODE\}/gi, code)
    .replace(/\{YYYY\}/gi, String(now.getFullYear()))
    .replace(/\{YY\}/gi, String(now.getFullYear()).slice(-2))
    .replace(/\{MM\}/gi, String(now.getMonth() + 1).padStart(2, '0'))
    .replace(/\{SEQ\}/gi, String(sequence).padStart(pad, '0'));
}

const toDraft = (r: Row): Draft => ({
  UserTypeID: Number(get(r, 'UserTypeID') ?? 0),
  UserType: String(get(r, 'UserType') ?? ''),
  UserCodeFormatID: Number(get(r, 'UserCodeFormatID') ?? 0),
  Code: String(get(r, 'Code') ?? ''),
  Pattern: String(get(r, 'Pattern') ?? '') || '{CODE}{SEQ}',
  SequenceLength: Number(get(r, 'SequenceLength') ?? 6) || 6,
  StartNumber: Number(get(r, 'StartNumber') ?? 1) || 1,
  ResetPolicy: String(get(r, 'ResetPolicy') ?? '') || 'None',
  IsActive: get(r, 'IsActive') !== false,
  NextSequence: Math.max(Number(get(r, 'LastSequence') ?? 0) + 1, Number(get(r, 'StartNumber') ?? 1) || 1),
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
 * User Alias — the code every user type stamps on its users. Each type carries its own code
 * (AD / RD / DS …) and its own running sequence, so a new Retailer picks up where the last
 * Retailer left off (RD000004 → RD000005) with no bearing on how many Admins exist. The code is
 * written to the user's Alias Name automatically when the user is created; nobody types it in.
 * (UserCode/GetAll · Save · Delete.)
 */
export function UserAliasScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const [edit, setEdit] = useState<Draft | null>(null);
  const [customFormat, setCustomFormat] = useState(false);
  const [confirmReset, setConfirmReset] = useState<Row | null>(null);

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['user-code-formats'],
    queryFn: () => fetchList('UserCode/GetAll', {}),
  });

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ['user-code-formats'] });
    qc.invalidateQueries({ queryKey: ['user-code-preview'] });
  };

  const saveMut = useMutation({
    mutationFn: (d: Draft) => apiPost('UserCode/Save', {
      UserTypeID: d.UserTypeID,
      Code: d.Code.trim().toUpperCase(),
      Pattern: d.Pattern.trim(),
      SequenceLength: Number(d.SequenceLength) || 6,
      StartNumber: Number(d.StartNumber) || 1,
      ResetPolicy: d.ResetPolicy,
      IsActive: d.IsActive,
    }),
    onSuccess: (res) => {
      if (res.ok) { toast.success('Code format saved.'); setEdit(null); invalidate(); }
      else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const resetMut = useMutation({
    mutationFn: (id: number) => apiPost('UserCode/Delete', { UserCodeFormatID: id }),
    onSuccess: (res) => {
      if (res.ok) { toast.success('Reset to the default format.'); setConfirmReset(null); invalidate(); }
      else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const openEdit = (r: Row) => {
    const d = toDraft(r);
    setCustomFormat(!PRESETS.some((p) => p.pattern === d.Pattern));
    setEdit(d);
  };

  const example = useMemo(
    () => (edit ? renderCode(edit.Pattern, edit.Code.toUpperCase() || 'XX', edit.NextSequence, edit.SequenceLength) : ''),
    [edit],
  );
  const canSave = Boolean(edit && edit.Code.trim() && /\{SEQ\}/i.test(edit.Pattern));

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={IdCard}
        actions={
          <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
          </Button>
        }
      />

      <p className="mb-4 text-sm text-slate-500">
        Every user type stamps its own code on the users created under it, and counts its own
        sequence — a new Retailer continues the Retailer series regardless of how many Admins exist.
        The code is written to the user&apos;s Alias Name automatically on creation and cannot be
        edited afterwards, so no two users can ever share one.
      </p>

      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">User type</th>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Format</th>
                <th className="px-4 py-3">Last issued</th>
                <th className="px-4 py-3">Next code</th>
                <th className="px-4 py-3">Coded users</th>
                <th className="px-4 py-3">Reset</th>
                <th className="px-4 py-3">Auto code</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isFetching && rows.length === 0 ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={i} className="border-b border-slate-100 last:border-0">
                    <td className="px-4 py-3" colSpan={9}><Skeleton className="h-8 w-full" /></td>
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={9}>
                    <EmptyState icon={IdCard} title="No user types" description="Add a user type first — each one gets its own code and sequence here." />
                  </td>
                </tr>
              ) : (
                rows.map((r) => {
                  const active = get(r, 'IsActive') !== false;
                  const configured = Boolean(get(r, 'IsConfigured'));
                  const formatId = Number(get(r, 'UserCodeFormatID') ?? 0);
                  const lastCode = String(get(r, 'LastCode') ?? '');
                  return (
                    <tr key={Number(get(r, 'UserTypeID') ?? 0)} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      <td className="px-4 py-3 font-medium text-slate-800">
                        {String(get(r, 'UserType') ?? '—')}
                        {!configured && <span className="ml-2 text-xs font-normal text-slate-400">not configured</span>}
                      </td>
                      <td className="px-4 py-3">
                        <span className="rounded bg-slate-100 px-1.5 py-0.5 font-mono text-xs text-slate-600">{String(get(r, 'Code') ?? '—') || '—'}</span>
                      </td>
                      <td className="px-4 py-3 font-mono text-xs text-slate-500">{String(get(r, 'Pattern') ?? '—')}</td>
                      <td className="px-4 py-3 font-mono text-xs text-slate-500">{lastCode || '—'}</td>
                      <td className="px-4 py-3 font-mono text-xs font-medium text-slate-800">{String(get(r, 'NextCode') ?? '') || '—'}</td>
                      <td className="px-4 py-3 tabular-nums text-slate-600">{String(get(r, 'UsersWithCode') ?? 0)}</td>
                      <td className="px-4 py-3 text-slate-600">
                        {RESET_OPTIONS.find((o) => o.value === String(get(r, 'ResetPolicy') ?? 'None'))?.label ?? 'Never'}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <Switch
                            on={active}
                            disabled={saveMut.isPending}
                            onChange={(v) => saveMut.mutate({ ...toDraft(r), IsActive: v })}
                          />
                          <Badge tone={active ? 'success' : 'neutral'}>{active ? 'On' : 'Off'}</Badge>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1">
                          <button type="button" onClick={() => openEdit(r)} className="grid size-7 place-items-center rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600" title="Edit format">
                            <Pencil className="size-4" />
                          </button>
                          {configured && (
                            <button type="button" onClick={() => setConfirmReset(r)} className="grid size-7 place-items-center rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-600" title="Reset to default" disabled={formatId <= 0}>
                              <RotateCcw className="size-4" />
                            </button>
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
      </Card>

      {/* Format editor */}
      <Modal open={Boolean(edit)} onClose={() => setEdit(null)} title={edit ? `${edit.UserType} code` : 'Code format'}>
        {edit && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Code *"
                placeholder="RD"
                value={edit.Code}
                onChange={(e) => setEdit({ ...edit, Code: e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, '') })}
                hint="Letters and digits, e.g. AD, RD, DS"
              />
              <Input
                label="Number length"
                type="number"
                min="1"
                max="12"
                value={String(edit.SequenceLength)}
                onChange={(e) => setEdit({ ...edit, SequenceLength: Number(e.target.value) || 1 })}
                hint="6 → 000001"
              />
            </div>

            <Select
              label="Format"
              value={customFormat ? 'custom' : edit.Pattern}
              onChange={(e) => {
                if (e.target.value === 'custom') { setCustomFormat(true); return; }
                setCustomFormat(false);
                setEdit({ ...edit, Pattern: e.target.value });
              }}
            >
              {PRESETS.map((p) => (
                <option key={p.pattern} value={p.pattern}>{`${p.label} — ${renderCode(p.pattern, edit.Code.toUpperCase() || 'XX', edit.NextSequence, edit.SequenceLength)}`}</option>
              ))}
              <option value="custom">Custom…</option>
            </Select>

            {customFormat && (
              <Input
                label="Custom format *"
                value={edit.Pattern}
                onChange={(e) => setEdit({ ...edit, Pattern: e.target.value })}
                className="font-mono"
                hint="Tokens: {CODE} {SEQ} {YYYY} {YY} {MM}. Everything else is used as written. {SEQ} is required."
              />
            )}

            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Start from"
                type="number"
                min="1"
                value={String(edit.StartNumber)}
                onChange={(e) => setEdit({ ...edit, StartNumber: Number(e.target.value) || 1 })}
              />
              <Select label="Restart the count" value={edit.ResetPolicy} onChange={(e) => setEdit({ ...edit, ResetPolicy: e.target.value })}>
                {RESET_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </Select>
            </div>

            <div className="flex items-center justify-between rounded-card border border-slate-200 bg-slate-50 px-4 py-3">
              <span className="text-xs uppercase tracking-wide text-slate-500">Next user gets</span>
              <span className="font-mono text-sm font-medium text-slate-800">{example || '—'}</span>
            </div>

            <label className="flex items-center gap-2 text-sm text-slate-700">
              <Switch on={edit.IsActive} onChange={(v) => setEdit({ ...edit, IsActive: v })} /> Generate codes for this user type
            </label>

            <p className="text-xs text-slate-400">
              Changing the code or the format restarts the count from whatever that new shape has
              already reached, so an existing code is never handed out twice.
            </p>

            <div className="flex justify-end gap-2 pt-2">
              <Button variant="outline" size="sm" onClick={() => setEdit(null)}>Cancel</Button>
              <Button size="sm" loading={saveMut.isPending} disabled={!canSave} onClick={() => edit && saveMut.mutate(edit)}>Save</Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Reset confirm */}
      <Modal open={Boolean(confirmReset)} onClose={() => setConfirmReset(null)} title="Reset code format">
        {confirmReset && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              Drop the configured format for <span className="font-medium text-slate-800">{String(get(confirmReset, 'UserType') ?? '')}</span>?
              New users of this type fall back to the default shape, continuing from the highest code already issued.
              Codes already given to users are untouched.
            </p>
            <div className="flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={() => setConfirmReset(null)}>Cancel</Button>
              <Button variant="danger" size="sm" loading={resetMut.isPending} onClick={() => resetMut.mutate(Number(get(confirmReset, 'UserCodeFormatID') ?? 0))}>Reset</Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
