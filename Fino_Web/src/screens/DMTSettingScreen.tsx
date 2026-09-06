import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Landmark, Plus, Pencil, Trash2, Save, X, Power, Zap, Send, Globe,
  KeyRound, IndianRupee,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Modal } from '@/components/ui/Modal';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');
const dec = (v: unknown) => Number(v ?? 0);

/** One DMT provider gateway configuration — mirrors the DMTSetting entity / Create-Update DTO. */
interface Draft {
  DMTSettingID: number;
  Name: string;
  DisplayName: string;
  IsActiveForWebsite: boolean;
  IsActive: boolean;
  Token: string;
  WalletID: string;
  CallBackURL: string;
  UserName: string;
  SplitAmount: number;
  MinAmount: number;
  SenderLimit: number;
  IsCCF: boolean;
  IsNeft: boolean;
  Sequence: number;
  Mode: string;
  IsPayoutDMT: boolean;
  ProviderCode: string;
  IsExpressDMT: boolean;
}

const BLANK: Draft = {
  DMTSettingID: 0, Name: '', DisplayName: '', IsActiveForWebsite: true, IsActive: true,
  Token: '', WalletID: '', CallBackURL: '', UserName: '', SplitAmount: 0, MinAmount: 0,
  SenderLimit: 500000, IsCCF: false, IsNeft: false, Sequence: 0, Mode: '', IsPayoutDMT: false,
  ProviderCode: '', IsExpressDMT: false,
};

/** Transfer-mode values seen on the legacy DMT settings screen. Free text is still allowed. */
const MODE_OPTIONS = ['IMPS', 'NEFT', 'IMPS/NEFT'];

function toDraft(r: Row): Draft {
  return {
    DMTSettingID: Number(getField(r, 'DMTSettingID') ?? 0),
    Name: str(getField(r, 'Name')),
    DisplayName: str(getField(r, 'DisplayName')),
    IsActiveForWebsite: Boolean(getField(r, 'IsActiveForWebsite')),
    IsActive: Boolean(getField(r, 'IsActive')),
    Token: str(getField(r, 'Token')),
    WalletID: str(getField(r, 'WalletID')),
    CallBackURL: str(getField(r, 'CallBackURL')),
    UserName: str(getField(r, 'UserName')),
    SplitAmount: dec(getField(r, 'SplitAmount')),
    MinAmount: dec(getField(r, 'MinAmount')),
    SenderLimit: dec(getField(r, 'SenderLimit')),
    IsCCF: Boolean(getField(r, 'IsCCF')),
    IsNeft: Boolean(getField(r, 'IsNeft')),
    Sequence: Number(getField(r, 'Sequence') ?? 0),
    Mode: str(getField(r, 'Mode')),
    IsPayoutDMT: Boolean(getField(r, 'IsPayoutDMT')),
    ProviderCode: str(getField(r, 'ProviderCode')),
    IsExpressDMT: Boolean(getField(r, 'IsExpressDMT')),
  };
}

/** apiPost returns ok:false on a business failure without throwing — surface it to onError. */
async function mustPost(endpoint: string, payload: unknown) {
  const res = await apiPost(endpoint, payload);
  if (!res.ok) throw new Error(res.message || 'Request failed.');
  return res;
}

/**
 * DMT Settings — manage the Domestic Money Transfer provider gateways. Each entry carries the
 * API credentials (username / token / wallet / callback), the money limits (minimum, split,
 * per-sender cap) and the transfer flags (NEFT, CCF, Payout, Express) used by the DMT pipeline.
 * Full CRUD against DMTSetting/GetAll · Create · Update · Delete.
 */
export function DMTSettingScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const [editing, setEditing] = useState<Draft | null>(null);
  const [togglingId, setTogglingId] = useState<number | null>(null);

  const { data: rows = [], isLoading } = useQuery({
    queryKey: ['dmt-settings'],
    queryFn: async () => (await fetchList('DMTSetting/GetAll', { PageNumber: 1, PageSize: 500 })).map(toDraft),
  });

  const refresh = () => qc.invalidateQueries({ queryKey: ['dmt-settings'] });

  const saveMut = useMutation({
    mutationFn: (d: Draft) =>
      mustPost(d.DMTSettingID > 0 ? 'DMTSetting/Update' : 'DMTSetting/Create', d),
    onSuccess: (_r, d) => {
      toast.success(`"${d.DisplayName || d.Name}" ${d.DMTSettingID > 0 ? 'updated' : 'added'}.`);
      setEditing(null);
      refresh();
    },
    onError: (e: Error) => toast.error(e.message || 'Could not save this DMT setting.'),
  });

  const toggleMut = useMutation({
    mutationFn: (d: Draft) => mustPost('DMTSetting/Update', { ...d, IsActive: !d.IsActive }),
    onMutate: (d) => setTogglingId(d.DMTSettingID),
    onSuccess: (_r, d) => { toast.success(`"${d.DisplayName || d.Name}" ${d.IsActive ? 'disabled' : 'enabled'}.`); refresh(); },
    onError: () => toast.error('Could not change the status.'),
    onSettled: () => setTogglingId(null),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => mustPost('DMTSetting/Delete', { DMTSettingID: id }),
    onSuccess: () => { toast.success('DMT setting removed.'); refresh(); },
    onError: () => toast.error('Could not remove this DMT setting.'),
  });

  return (
    <div className="mx-auto max-w-[1100px] space-y-4">
      <PageHeader
        title="DMT Settings"
        subtitle="Domestic Money Transfer gateways — limits, split, transfer modes & API credentials"
        icon={Landmark}
        actions={<Button onClick={() => setEditing({ ...BLANK })}><Plus className="size-4" /> Add DMT provider</Button>}
      />

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80">
                {['Provider', 'Mode', 'Options', 'Min amount', 'Split above', 'Sender limit', 'Status'].map((h) => (
                  <th key={h} className="whitespace-nowrap px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">{h}</th>
                ))}
                <th className="whitespace-nowrap px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                Array.from({ length: 3 }).map((_, i) => (
                  <tr key={i} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 8 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><div className="h-4 w-24 animate-pulse rounded bg-slate-100" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-4 py-14 text-center">
                    <Landmark className="mx-auto size-8 text-slate-300" />
                    <p className="mt-3 text-sm text-slate-500">No DMT providers configured yet.</p>
                    <Button className="mt-4" onClick={() => setEditing({ ...BLANK })}><Plus className="size-4" /> Add DMT provider</Button>
                  </td>
                </tr>
              ) : (
                rows.map((row) => (
                  <tr key={row.DMTSettingID} className={cn('border-b border-slate-100 transition-colors last:border-0 even:bg-slate-50/40 hover:bg-brand-50/40', !row.IsActive && 'opacity-60')}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-slate-800">{row.DisplayName || row.Name || 'Unnamed'}</span>
                        {row.ProviderCode && <span className="rounded-md border border-slate-200 bg-slate-50 px-1.5 py-0.5 font-mono text-[11px] text-slate-500">{row.ProviderCode}</span>}
                      </div>
                      <p className="mt-0.5 text-xs text-slate-400">{row.Name}{row.UserName ? ` · ${row.UserName}` : ''} · seq {row.Sequence}</p>
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">{row.IsNeft ? 'IMPS + NEFT' : 'IMPS only'}</td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-1">
                        {row.IsPayoutDMT && <Flag on label="Payout" icon={Send} />}
                        {row.IsExpressDMT && <Flag on label="Express" icon={Zap} />}
                        {row.IsCCF && <Flag on label="CCF" icon={IndianRupee} />}
                        {row.IsActiveForWebsite && <Flag on label="Website" icon={Globe} />}
                        {!row.IsPayoutDMT && !row.IsExpressDMT && !row.IsCCF && !row.IsActiveForWebsite && <span className="text-slate-300">—</span>}
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-700">{inr(row.MinAmount)}</td>
                    <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-700">{row.SplitAmount > 0 ? inr(row.SplitAmount) : <span className="text-slate-300">—</span>}</td>
                    <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-700">{inr(row.SenderLimit)}</td>
                    <td className="px-4 py-3"><StatusPill active={row.IsActive} /></td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1.5">
                        <button
                          title={row.IsActive ? 'Disable' : 'Enable'}
                          onClick={() => toggleMut.mutate(row)}
                          disabled={togglingId === row.DMTSettingID}
                          className="grid size-8 place-items-center rounded-lg border border-slate-200 text-slate-500 hover:border-brand-300 hover:text-brand-600 disabled:opacity-50"
                        ><Power className="size-4" /></button>
                        <button
                          title="Edit"
                          onClick={() => setEditing(row)}
                          className="grid size-8 place-items-center rounded-lg border border-slate-200 text-slate-500 hover:border-brand-300 hover:text-brand-600"
                        ><Pencil className="size-4" /></button>
                        <button
                          title="Delete"
                          onClick={() => { if (confirm(`Remove "${row.DisplayName || row.Name}"?`)) deleteMut.mutate(row.DMTSettingID); }}
                          className="grid size-8 place-items-center rounded-lg border border-slate-200 text-slate-400 hover:border-danger-300 hover:text-danger-600"
                        ><Trash2 className="size-4" /></button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        {!isLoading && rows.length > 0 && (
          <div className="flex items-center justify-between border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
            <span>Showing <span className="font-semibold text-slate-700 tabular-nums">{rows.length}</span> {rows.length === 1 ? 'provider' : 'providers'}</span>
          </div>
        )}
      </div>

      {editing && (
        <EditModal
          draft={editing}
          saving={saveMut.isPending}
          onCancel={() => setEditing(null)}
          onSave={(d) => saveMut.mutate(d)}
        />
      )}
    </div>
  );
}

function Flag({ on, label, icon: Icon }: { on: boolean; label: string; icon: typeof Zap }) {
  return (
    <span className={cn('inline-flex items-center gap-1 rounded-md border px-1.5 py-0.5 text-[11px] font-medium',
      on ? 'border-brand-200 bg-brand-50 text-brand-700' : 'border-slate-200 bg-white text-slate-400')}>
      <Icon className="size-3" /> {label}
    </span>
  );
}

function StatusPill({ active }: { active: boolean }) {
  return (
    <span className={cn('inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-semibold',
      active ? 'bg-success-50 text-success-700' : 'bg-slate-100 text-slate-500')}>
      <span className={cn('size-1.5 rounded-full', active ? 'bg-success-500' : 'bg-slate-400')} />
      {active ? 'Active' : 'Inactive'}
    </span>
  );
}

/** Section wrapper for the grouped edit form. */
function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div>
      <p className="mb-2 text-xs font-semibold uppercase tracking-wider text-slate-400">{title}</p>
      <div className="grid gap-3 sm:grid-cols-2">{children}</div>
    </div>
  );
}

function CheckRow({ label, hint, checked, onChange }: { label: string; hint?: string; checked: boolean; onChange: (v: boolean) => void }) {
  return (
    <label className="flex cursor-pointer items-start gap-2.5 rounded-lg border border-slate-200 px-3 py-2.5 hover:border-slate-300">
      <input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} className="mt-0.5 size-4 rounded border-slate-300 text-brand-600" />
      <span>
        <span className="block text-sm font-medium text-slate-700">{label}</span>
        {hint && <span className="block text-xs text-slate-400">{hint}</span>}
      </span>
    </label>
  );
}

function EditModal({ draft, saving, onCancel, onSave }: {
  draft: Draft; saving: boolean; onCancel: () => void; onSave: (d: Draft) => void;
}) {
  const [d, setD] = useState<Draft>(draft);
  const [err, setErr] = useState<{ Name?: string; DisplayName?: string }>({});
  const set = (p: Partial<Draft>) => setD((s) => ({ ...s, ...p }));

  // The gateways registered in "Add API" (ProviderMaster). A DMT setting links to one of them by
  // name (stored in ProviderCode); the DMT pipeline then reads that provider's saved credentials —
  // so JaiJui's mobile / token / auth code are entered once, in Add API, not here and not in config.
  const { data: providers = [] } = useQuery({
    queryKey: ['provider-list-for-dmt'],
    queryFn: () => fetchList('ProviderMaster/GetAll', { PageNumber: 1, PageSize: 500, IsActive: false, TakeCount: 500, SkipCount: 0 }),
  });
  const providerNames = providers.map((p) => str(getField(p, 'ProviderName'))).filter(Boolean);
  const linkedKnown = !d.ProviderCode || providerNames.includes(d.ProviderCode);
  const numSet = (k: keyof Draft) => (e: React.ChangeEvent<HTMLInputElement>) => set({ [k]: Number(e.target.value) } as Partial<Draft>);

  const submit = () => {
    const next: typeof err = {};
    if (!d.Name.trim()) next.Name = 'Name is required';
    if (!d.DisplayName.trim()) next.DisplayName = 'Display name is required';
    setErr(next);
    if (Object.keys(next).length) return;
    onSave(d);
  };

  return (
    <Modal open onClose={onCancel} title={d.DMTSettingID > 0 ? 'Edit DMT provider' : 'Add DMT provider'} size="lg">
      <div className="space-y-5">
        <Section title="Identity">
          <Input label={<>Name (internal key) <span className="text-danger-500">*</span></>} value={d.Name} error={err.Name}
            placeholder="e.g. YesBankDMT" onChange={(e) => { set({ Name: e.target.value }); setErr((x) => ({ ...x, Name: undefined })); }} />
          <Input label={<>Display name <span className="text-danger-500">*</span></>} value={d.DisplayName} error={err.DisplayName}
            placeholder="e.g. Instant Money Transfer" onChange={(e) => { set({ DisplayName: e.target.value }); setErr((x) => ({ ...x, DisplayName: undefined })); }} />
          <Select label="Linked provider (Add API)" value={linkedKnown ? d.ProviderCode : '__custom'} onChange={(e) => set({ ProviderCode: e.target.value === '__custom' ? d.ProviderCode : e.target.value })}>
            <option value="">Not linked</option>
            {providerNames.map((name) => <option key={name} value={name}>{name}</option>)}
            {!linkedKnown && <option value="__custom">{d.ProviderCode} (not found)</option>}
          </Select>
          <Input label="Sequence" type="number" value={String(d.Sequence)} onChange={numSet('Sequence')} hint="Lower runs first in provider switching" />
        </Section>

        <Section title="API credentials">
          <p className="-mt-1 text-xs text-slate-500 sm:col-span-2">
            When a provider is linked above, the DMT pipeline uses that provider's saved credentials
            (e.g. JaiJui's mobile / API token / auth code from Add API). The fields below are only a
            fallback for gateways that store their credentials on the DMT row itself.
          </p>
          <Input label="Username" value={d.UserName} leftIcon={<KeyRound className="size-4" />} placeholder="Provider API username" onChange={(e) => set({ UserName: e.target.value })} />
          <Input label="Wallet ID" value={d.WalletID} placeholder="Provider wallet / merchant id" onChange={(e) => set({ WalletID: e.target.value })} />
          <div className="sm:col-span-2">
            <Input label="Token / API key" value={d.Token} placeholder="Secret token issued by the provider" onChange={(e) => set({ Token: e.target.value })} />
          </div>
          <div className="sm:col-span-2">
            <Input label="Callback URL" value={d.CallBackURL} leftIcon={<Globe className="size-4" />} placeholder="https://…/dmt/callback" onChange={(e) => set({ CallBackURL: e.target.value })} />
          </div>
        </Section>

        <Section title="Limits & transfer mode">
          <Input label="Minimum amount (₹)" type="number" value={String(d.MinAmount)} onChange={numSet('MinAmount')} hint="Smallest transfer allowed" />
          <Input label="Split amount (₹)" type="number" value={String(d.SplitAmount)} onChange={numSet('SplitAmount')} hint="Split a transfer above this into multiple legs (0 = off)" />
          <Input label="Sender limit (₹)" type="number" value={String(d.SenderLimit)} onChange={numSet('SenderLimit')} hint="Monthly cap per sender (RBI default ₹5,00,000)" />
          <Select label="Mode" value={d.Mode} onChange={(e) => set({ Mode: e.target.value })}>
            <option value="">Not set</option>
            {MODE_OPTIONS.map((m) => <option key={m} value={m}>{m}</option>)}
            {d.Mode && !MODE_OPTIONS.includes(d.Mode) && <option value={d.Mode}>{d.Mode}</option>}
          </Select>
        </Section>

        <Section title="Options">
          <CheckRow label="Allow NEFT" hint="Offer NEFT alongside IMPS" checked={d.IsNeft} onChange={(v) => set({ IsNeft: v })} />
          <CheckRow label="Payout DMT" hint="Route through the payout rail" checked={d.IsPayoutDMT} onChange={(v) => set({ IsPayoutDMT: v })} />
          <CheckRow label="Express DMT" hint="Faster settlement channel" checked={d.IsExpressDMT} onChange={(v) => set({ IsExpressDMT: v })} />
          <CheckRow label="Charge CCF" hint="Apply customer convenience fee" checked={d.IsCCF} onChange={(v) => set({ IsCCF: v })} />
          <CheckRow label="Active" hint="Available to the DMT pipeline" checked={d.IsActive} onChange={(v) => set({ IsActive: v })} />
          <CheckRow label="Show on website" hint="Visible to retailers on the site" checked={d.IsActiveForWebsite} onChange={(v) => set({ IsActiveForWebsite: v })} />
        </Section>

        <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
          <Button variant="outline" onClick={onCancel} disabled={saving}><X className="size-4" /> Cancel</Button>
          <Button onClick={submit} loading={saving}><Save className="size-4" /> {d.DMTSettingID > 0 ? 'Save changes' : 'Add provider'}</Button>
        </div>
      </div>
    </Modal>
  );
}
