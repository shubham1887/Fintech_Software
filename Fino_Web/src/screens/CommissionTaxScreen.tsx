import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, Calculator, Info, Plus, Scale, Trash2,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr, apiDate, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { canRaiseDocument } from '@/lib/gstDocs';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface ProfileRow {
  CommissionTaxProfileID: number;
  Scope: string; ScopeID: number; ScopeName: string;
  ServiceID: number; ServiceName: string;
  OperatorID: number; OperatorName: string;
  TaxMode: string; TaxBase: string;
  GSTRatePercent: number;
  PerTransactionCharge: number; PerTransactionChargeTaxMode: string;
  TDSEnabled: boolean; TDSSectionCode: string; TDSRatePercent: number;
  EffectiveFrom: string; EffectiveTo: string | null;
  Priority: number; Remark: string; IsActive: boolean;
}

interface Treatment {
  Scope: string; ScopeLabel: string; TaxMode: string; TaxBase: string;
  GSTRatePercent: number; RateFromTaxEngine: boolean;
  TDSEnabled: boolean; TDSSectionCode: string; TDSRatePercent: number;
  Warning: string;
}
interface Split {
  CommissionAsQuoted: number; NetCommission: number; GST: number;
  Payable: number; TaxableBase: number; TaxMode: string; TaxBase: string; RatePercent: number;
}
interface Preview {
  Treatment: Treatment; Split: Split;
  TDSAmount: number; TDSSectionCode: string; NetToParty: number; Steps: string[];
}

const SCOPES = [
  { value: 'GLOBAL', label: 'House default', hint: 'Applies when nothing more specific does' },
  { value: 'SERVICE', label: 'Service', hint: 'Everything on one service' },
  { value: 'PROVIDER', label: 'Provider', hint: 'Money we receive FROM this provider' },
  { value: 'SLAB', label: 'Slab', hint: 'Money we pay OUT on this slab' },
] as const;

const MODES = [
  { value: 'INCLUSIVE', label: 'Inclusive of GST', hint: 'The figure already contains the tax; it is carved out' },
  { value: 'EXCLUSIVE', label: 'Exclusive of GST', hint: 'The tax is added on top of the figure' },
  { value: 'NONE', label: 'No GST', hint: 'No GST on this commission at all' },
] as const;

const BASES = [
  { value: 'COMMISSION', label: 'On the commission', hint: 'Agency work — DMT, AEPS, payout' },
  { value: 'GROSS', label: 'On the whole amount', hint: 'Trading — talktime bought and resold' },
] as const;

const scopeTone = (s: string): 'success' | 'info' | 'warning' | 'neutral' => {
  switch (s) {
    case 'SLAB': return 'success';
    case 'PROVIDER': return 'info';
    case 'SERVICE': return 'warning';
    default: return 'neutral';
  }
};

const blankForm = () => ({
  CommissionTaxProfileID: 0,
  Scope: 'PROVIDER',
  ScopeID: '',
  ServiceID: '',
  OperatorID: '',
  TaxMode: 'INCLUSIVE',
  TaxBase: 'COMMISSION',
  GSTRatePercent: '',
  PerTransactionCharge: '',
  PerTransactionChargeTaxMode: 'INCLUSIVE',
  TDSEnabled: false,
  TDSSectionCode: '',
  TDSRatePercent: '',
  EffectiveFrom: apiDate(new Date()),
  EffectiveTo: '',
  Priority: '',
  Remark: '',
  IsActive: true,
});

export function CommissionTaxScreen({ def }: CustomScreenProps) {
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayWrite = canRaiseDocument(user?.UserTypeID);

  const [scopeFilter, setScopeFilter] = useState('');

  const profiles = useQuery({
    queryKey: ['commission-tax', scopeFilter],
    queryFn: async (): Promise<ProfileRow[]> => {
      const res = await apiPost<ProfileRow[]>('CommissionTax/Select', { Scope: scopeFilter });
      if (!res.ok) throw new Error(res.message || 'Could not load the profiles.');
      return res.data ?? [];
    },
  });

  // The pickers. Loaded once; a profile is useless without being able to name its provider.
  const services = useQuery({
    queryKey: ['crud', 'Service/GetAll'],
    queryFn: () => fetchList('Service/GetAll', { PageNumber: 1, PageSize: 500 }),
  });
  const providers = useQuery({
    queryKey: ['crud', 'ProviderMaster/GetAll'],
    queryFn: () => fetchList('ProviderMaster/GetAll', { PageNumber: 1, PageSize: 500 }),
  });
  const slabs = useQuery({
    queryKey: ['crud', 'SlabMaster/GetAll'],
    queryFn: () => fetchList('SlabMaster/GetAll', { PageNumber: 1, PageSize: 500 }),
  });

  const scopeOptions = useMemo(() => {
    const map = (rows: Record<string, unknown>[] | undefined, idKeys: string[], nameKeys: string[]) =>
      (rows ?? []).map((r) => ({
        id: Number(getField(r, ...idKeys) ?? 0),
        name: String(getField(r, ...nameKeys) ?? ''),
      })).filter((x) => x.id > 0);
    return {
      SERVICE: map(services.data, ['ServiceID'], ['Name', 'ServiceName']),
      PROVIDER: map(providers.data, ['ProviderID'], ['ProviderName', 'Name']),
      SLAB: map(slabs.data, ['SlabMasterID'], ['SlabName', 'Name']),
      GLOBAL: [],
    } as Record<string, { id: number; name: string }[]>;
  }, [services.data, providers.data, slabs.data]);

  // ── Edit ────────────────────────────────────────────────────────────────────────────
  const [form, setForm] = useState(blankForm());
  const [open, setOpen] = useState(false);
  const set = (k: keyof ReturnType<typeof blankForm>, v: unknown) => setForm((f) => ({ ...f, [k]: v }));

  const edit = (p: ProfileRow) => {
    setForm({
      CommissionTaxProfileID: p.CommissionTaxProfileID,
      Scope: p.Scope,
      ScopeID: p.ScopeID ? String(p.ScopeID) : '',
      ServiceID: p.ServiceID ? String(p.ServiceID) : '',
      OperatorID: p.OperatorID ? String(p.OperatorID) : '',
      TaxMode: p.TaxMode,
      TaxBase: p.TaxBase,
      GSTRatePercent: p.GSTRatePercent ? String(p.GSTRatePercent) : '',
      PerTransactionCharge: p.PerTransactionCharge ? String(p.PerTransactionCharge) : '',
      PerTransactionChargeTaxMode: p.PerTransactionChargeTaxMode || 'INCLUSIVE',
      TDSEnabled: p.TDSEnabled,
      TDSSectionCode: p.TDSSectionCode,
      TDSRatePercent: p.TDSRatePercent ? String(p.TDSRatePercent) : '',
      EffectiveFrom: p.EffectiveFrom?.slice(0, 10) ?? apiDate(new Date()),
      EffectiveTo: p.EffectiveTo?.slice(0, 10) ?? '',
      Priority: p.Priority ? String(p.Priority) : '',
      Remark: p.Remark,
      IsActive: p.IsActive,
    });
    setOpen(true);
  };

  const save = useMutation({
    mutationFn: async () => {
      const res = await apiPost('CommissionTax/Save', {
        CommissionTaxProfileID: form.CommissionTaxProfileID,
        Scope: form.Scope,
        ScopeID: Number(form.ScopeID || 0),
        ServiceID: Number(form.ServiceID || 0),
        OperatorID: Number(form.OperatorID || 0),
        TaxMode: form.TaxMode,
        TaxBase: form.TaxBase,
        GSTRatePercent: Number(form.GSTRatePercent || 0),
        PerTransactionCharge: Number(form.PerTransactionCharge || 0),
        PerTransactionChargeTaxMode: form.PerTransactionChargeTaxMode,
        TDSEnabled: form.TDSEnabled,
        TDSSectionCode: form.TDSSectionCode,
        TDSRatePercent: Number(form.TDSRatePercent || 0),
        EffectiveFrom: form.EffectiveFrom,
        EffectiveTo: form.EffectiveTo || null,
        Priority: Number(form.Priority || 0),
        Remark: form.Remark,
        IsActive: form.IsActive,
      });
      if (!res.ok) throw new Error(res.message || 'It could not be saved.');
    },
    onSuccess: () => {
      toast.success('Saved. It applies to every commission settled from now on.');
      setOpen(false);
      qc.invalidateQueries({ queryKey: ['commission-tax'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: async (id: number) => {
      const res = await apiPost('CommissionTax/Delete', { CommissionTaxProfileID: id });
      if (!res.ok) throw new Error(res.message || 'It could not be removed.');
    },
    onSuccess: () => {
      toast.success('Removed.');
      qc.invalidateQueries({ queryKey: ['commission-tax'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Try it ──────────────────────────────────────────────────────────────────────────
  const [tryOpen, setTryOpen] = useState(false);
  const [tryForm, setTryForm] = useState({
    ServiceID: '', OperatorID: '', ProviderID: '', SlabMasterID: '',
    Amount: '1000', Commission: '15', PartyUserID: '',
  });
  const [preview, setPreview] = useState<Preview | null>(null);

  const runPreview = useMutation({
    mutationFn: async (): Promise<Preview> => {
      const res = await apiPost<Preview>('CommissionTax/Preview', {
        ServiceID: Number(tryForm.ServiceID || 0),
        OperatorID: Number(tryForm.OperatorID || 0),
        ProviderID: Number(tryForm.ProviderID || 0),
        SlabMasterID: Number(tryForm.SlabMasterID || 0),
        Amount: Number(tryForm.Amount || 0),
        Commission: Number(tryForm.Commission || 0),
        PartyUserID: Number(tryForm.PartyUserID || 0),
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'It could not be worked out.');
      return res.data;
    },
    onSuccess: (p) => setPreview(p),
    onError: (e: Error) => toast.error(e.message),
  });

  const rows = profiles.data ?? [];
  const tdsOn = rows.some((r) => r.TDSEnabled && r.IsActive);

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Is a commission inclusive or exclusive of GST, is the tax on the commission or the whole amount, and is TDS deducted"
        icon={pickIcon(def.title)}
        actions={
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => { setPreview(null); setTryOpen(true); }}>
              <Calculator className="mr-1.5 size-4" />
              Try a figure
            </Button>
            {mayWrite && (
              <Button onClick={() => { setForm(blankForm()); setOpen(true); }}>
                <Plus className="mr-1.5 size-4" />
                Add profile
              </Button>
            )}
          </div>
        }
      />

      <div className="flex gap-2.5 rounded-lg border border-slate-200 bg-white p-3 text-sm text-slate-600">
        <Info className="mt-0.5 size-4 shrink-0 text-slate-400" />
        <p>
          Until now these answers were fixed in the code — every commission was treated as{' '}
          <strong>inclusive</strong> of GST and TDS was never deducted. That is why a provider paying
          commission <em>exclusive</em> of GST and one paying it <em>inclusive</em> produced the same net
          profit. The most specific row wins: <strong>slab</strong> beats <strong>provider</strong>, which
          beats <strong>service</strong>, which beats the <strong>house default</strong>.
        </p>
      </div>

      {tdsOn && (
        <div className="flex gap-2.5 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            <strong>TDS is switched on somewhere.</strong> From now on, tax is withheld from the downline's
            commission on every settled transaction that matches — real money, off real people. Check the
            section and rate before leaving it on.
          </p>
        </div>
      )}

      <Card>
        <CardBody className="flex flex-wrap items-end gap-3">
          <div className="w-52">
            <Select label="Scope" value={scopeFilter} onChange={(e) => setScopeFilter(e.target.value)}>
              <option value="">All scopes</option>
              {SCOPES.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
            </Select>
          </div>
          <div className="flex-1" />
          <span className="text-sm text-slate-500">{rows.length} profile{rows.length === 1 ? '' : 's'}</span>
        </CardBody>
      </Card>

      <Card>
        {profiles.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>
        ) : profiles.isError ? (
          <CardBody><EmptyState title="Could not load" description={(profiles.error as Error).message} /></CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title="No profile configured"
              description="The install script seeds a house default (inclusive, on the commission, TDS off). If this is empty, 68_patch_commission_tax_profiles.sql has not been run on this database."
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[1040px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Applies to</th>
                  <th className="px-3 py-2.5 font-medium">Narrowed by</th>
                  <th className="px-3 py-2.5 font-medium">GST</th>
                  <th className="px-3 py-2.5 text-right font-medium">Rate</th>
                  <th className="px-3 py-2.5 text-right font-medium">Per txn</th>
                  <th className="px-3 py-2.5 font-medium">TDS</th>
                  <th className="px-3 py-2.5 font-medium">Effective</th>
                  {mayWrite && <th className="px-3 py-2.5" />}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((p) => (
                  <tr key={p.CommissionTaxProfileID} className={cn('hover:bg-slate-50', !p.IsActive && 'opacity-50')}>
                    <td className="px-5 py-2.5">
                      <div className="flex items-center gap-2">
                        <Badge tone={scopeTone(p.Scope)}>{p.Scope}</Badge>
                        <span className="font-medium text-slate-800">{p.ScopeName}</span>
                      </div>
                      {p.Remark && <div className="max-w-[260px] truncate text-xs text-slate-400" title={p.Remark}>{p.Remark}</div>}
                    </td>
                    <td className="px-3 py-2.5 text-xs text-slate-500">
                      {p.ServiceName === 'Any' && p.OperatorName === 'Any'
                        ? '—'
                        : `${p.ServiceName}${p.OperatorName !== 'Any' ? ` · ${p.OperatorName}` : ''}`}
                    </td>
                    <td className="px-3 py-2.5">
                      <Badge tone={p.TaxMode === 'EXCLUSIVE' ? 'warning' : p.TaxMode === 'NONE' ? 'neutral' : 'success'}>
                        {p.TaxMode === 'INCLUSIVE' ? 'inclusive' : p.TaxMode === 'EXCLUSIVE' ? 'exclusive' : 'no GST'}
                      </Badge>
                      <div className="text-[11px] text-slate-400">
                        on {p.TaxBase === 'GROSS' ? 'the whole amount' : 'the commission'}
                      </div>
                    </td>
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">
                      {p.GSTRatePercent > 0 ? `${p.GSTRatePercent}%` : <span className="text-xs text-slate-400">from rules</span>}
                    </td>
                    <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">
                      {p.PerTransactionCharge > 0 ? inr(p.PerTransactionCharge) : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="px-3 py-2.5">
                      {p.TDSEnabled ? (
                        <>
                          <Badge tone="danger">on</Badge>
                          <div className="text-[11px] text-slate-500">
                            {p.TDSSectionCode || '194H'}{p.TDSRatePercent > 0 ? ` @ ${p.TDSRatePercent}%` : ''}
                          </div>
                        </>
                      ) : <Badge tone="neutral">off</Badge>}
                    </td>
                    <td className="px-3 py-2.5 text-xs text-slate-500">
                      {dateOnly(p.EffectiveFrom)}{p.EffectiveTo ? ` – ${dateOnly(p.EffectiveTo)}` : ''}
                    </td>
                    {mayWrite && (
                      <td className="px-3 py-2.5 text-right">
                        <div className="flex items-center justify-end gap-3">
                          <button type="button" onClick={() => edit(p)} className="text-xs text-brand-600 hover:underline">Edit</button>
                          {p.Scope !== 'GLOBAL' && (
                            <button
                              type="button"
                              onClick={() => remove.mutate(p.CommissionTaxProfileID)}
                              className="text-slate-400 hover:text-danger-600"
                              aria-label="Remove"
                            >
                              <Trash2 className="size-3.5" />
                            </button>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {!mayWrite && (
        <p className="text-xs text-slate-400">
          Read-only. Trying a figure is yours — it changes nothing. Saving a profile is limited to Admin,
          because it changes how every later commission splits and whether money is withheld from people.
        </p>
      )}

      {/* ── Add / edit ───────────────────────────────────────────────────────────────── */}
      <Modal open={open} onClose={() => setOpen(false)} title={form.CommissionTaxProfileID ? 'Edit profile' : 'Add profile'} size="lg">
        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2">
            <Select label="Applies to" value={form.Scope} onChange={(e) => { set('Scope', e.target.value); set('ScopeID', ''); }}>
              {SCOPES.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
            </Select>
            {form.Scope !== 'GLOBAL' ? (
              <Select label={SCOPES.find((s) => s.value === form.Scope)?.label ?? 'Which'} value={form.ScopeID} onChange={(e) => set('ScopeID', e.target.value)}>
                <option value="">Choose…</option>
                {(scopeOptions[form.Scope] ?? []).map((o) => (
                  <option key={o.id} value={o.id}>{o.name}</option>
                ))}
              </Select>
            ) : (
              <div className="self-end text-xs text-slate-400">
                The fallback for everything no other row covers. There can only be one.
              </div>
            )}
          </div>

          <div className="grid gap-3 sm:grid-cols-2">
            <Select label="Is the commission…" value={form.TaxMode} onChange={(e) => set('TaxMode', e.target.value)}>
              {MODES.map((m) => <option key={m.value} value={m.value}>{m.label}</option>)}
            </Select>
            <Select label="GST is charged" value={form.TaxBase} onChange={(e) => set('TaxBase', e.target.value)}>
              {BASES.map((b) => <option key={b.value} value={b.value}>{b.label}</option>)}
            </Select>
          </div>
          <p className="-mt-1 text-xs text-slate-400">
            {MODES.find((m) => m.value === form.TaxMode)?.hint} · {BASES.find((b) => b.value === form.TaxBase)?.hint}
          </p>

          <div className="grid gap-3 sm:grid-cols-3">
            <Input label="GST rate %" value={form.GSTRatePercent} onChange={(e) => set('GSTRatePercent', e.target.value)} hint="Blank uses the tax rules" />
            <Input label="Per-transaction charge" value={form.PerTransactionCharge} onChange={(e) => set('PerTransactionCharge', e.target.value)} hint="For DMT-style count billing" />
            <Select label="…and that charge is" value={form.PerTransactionChargeTaxMode} onChange={(e) => set('PerTransactionChargeTaxMode', e.target.value)}>
              {MODES.map((m) => <option key={m.value} value={m.value}>{m.label}</option>)}
            </Select>
          </div>

          <div className="rounded-lg border border-slate-200 p-3">
            <label className="flex items-center gap-2 text-sm font-medium text-slate-700">
              <input type="checkbox" className="size-4" checked={form.TDSEnabled} onChange={(e) => set('TDSEnabled', e.target.checked)} />
              Deduct TDS on the commission
            </label>
            {form.TDSEnabled && (
              <>
                <div className="mt-3 grid gap-3 sm:grid-cols-2">
                  <Input label="Section" value={form.TDSSectionCode} onChange={(e) => set('TDSSectionCode', e.target.value.toUpperCase())} hint="Blank uses 194H" />
                  <Input label="Rate %" value={form.TDSRatePercent} onChange={(e) => set('TDSRatePercent', e.target.value)} hint="Blank uses the section's own rate" />
                </div>
                <p className="mt-2 text-xs text-warning-700">
                  TDS is computed on the commission <strong>after</strong> GST is taken out of it — tax is on
                  income, not on the tax collected with it. A party with no PAN gets the section's higher
                  no-PAN rate automatically.
                </p>
              </>
            )}
          </div>

          <div className="grid gap-3 sm:grid-cols-3">
            <Input label="Effective from" type="date" value={form.EffectiveFrom} onChange={(e) => set('EffectiveFrom', e.target.value)} />
            <Input label="Effective to" type="date" value={form.EffectiveTo} onChange={(e) => set('EffectiveTo', e.target.value)} hint="Blank = open-ended" />
            <Input label="Priority" value={form.Priority} onChange={(e) => set('Priority', e.target.value)} hint="Blank = derived" />
          </div>

          <Input label="Remark" value={form.Remark} onChange={(e) => set('Remark', e.target.value)} hint="Why this row exists — the contract clause, say" />

          <label className="flex items-center gap-2 text-sm text-slate-700">
            <input type="checkbox" className="size-4" checked={form.IsActive} onChange={(e) => set('IsActive', e.target.checked)} />
            Active
          </label>

          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setOpen(false)}>Cancel</Button>
            <Button onClick={() => save.mutate()} loading={save.isPending}>Save</Button>
          </div>
        </div>
      </Modal>

      {/* ── Try a figure ─────────────────────────────────────────────────────────────── */}
      <Modal open={tryOpen} onClose={() => setTryOpen(false)} title="Try a commission" size="lg">
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            Work a figure through whichever profile wins, and see every step. Nothing is saved.
          </p>
          <div className="grid gap-3 sm:grid-cols-4">
            <Select label="Service" value={tryForm.ServiceID} onChange={(e) => setTryForm((f) => ({ ...f, ServiceID: e.target.value }))}>
              <option value="">Any</option>
              {(scopeOptions.SERVICE ?? []).map((o) => <option key={o.id} value={o.id}>{o.name}</option>)}
            </Select>
            <Select label="Provider" value={tryForm.ProviderID} onChange={(e) => setTryForm((f) => ({ ...f, ProviderID: e.target.value }))}>
              <option value="">Any</option>
              {(scopeOptions.PROVIDER ?? []).map((o) => <option key={o.id} value={o.id}>{o.name}</option>)}
            </Select>
            <Select label="Slab" value={tryForm.SlabMasterID} onChange={(e) => setTryForm((f) => ({ ...f, SlabMasterID: e.target.value }))}>
              <option value="">Any</option>
              {(scopeOptions.SLAB ?? []).map((o) => <option key={o.id} value={o.id}>{o.name}</option>)}
            </Select>
            <Input label="Party user ID" value={tryForm.PartyUserID} onChange={(e) => setTryForm((f) => ({ ...f, PartyUserID: e.target.value }))} hint="For TDS" />
          </div>
          <div className="grid gap-3 sm:grid-cols-2">
            <Input label="Transaction amount" value={tryForm.Amount} onChange={(e) => setTryForm((f) => ({ ...f, Amount: e.target.value }))} />
            <Input label="Commission as quoted" value={tryForm.Commission} onChange={(e) => setTryForm((f) => ({ ...f, Commission: e.target.value }))} />
          </div>

          <div className="flex justify-end">
            <Button onClick={() => runPreview.mutate()} loading={runPreview.isPending}>
              <Scale className="mr-1.5 size-4" />
              Work it out
            </Button>
          </div>

          {preview && (
            <div className="space-y-3 rounded-lg border border-slate-200 bg-slate-50 p-3">
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <Figure label="Income" value={inr(preview.Split.NetCommission)} tone="text-slate-800" />
                <Figure label="GST" value={inr(preview.Split.GST)} tone="text-brand-600" />
                <Figure label="TDS" value={inr(preview.TDSAmount)} tone={preview.TDSAmount > 0 ? 'text-danger-600' : 'text-slate-400'} />
                <Figure label="Party receives" value={inr(preview.NetToParty)} tone="text-success-700" />
              </div>
              <ol className="space-y-1 text-xs text-slate-600">
                {preview.Steps.map((s, i) => (
                  <li key={i} className="flex gap-2">
                    <span className="shrink-0 font-mono text-slate-400">{i + 1}.</span>
                    <span>{s}</span>
                  </li>
                ))}
              </ol>
              {preview.Treatment.Warning && (
                <p className="flex gap-2 rounded border border-warning-500/30 bg-warning-50 p-2 text-xs text-warning-700">
                  <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />
                  {preview.Treatment.Warning}
                </p>
              )}
            </div>
          )}

          <div className="flex justify-end">
            <Button variant="ghost" onClick={() => setTryOpen(false)}>Close</Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

function Figure({ label, value, tone }: { label: string; value: string; tone: string }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-2.5">
      <p className="text-[10px] font-medium uppercase tracking-wide text-slate-400">{label}</p>
      <p className={cn('mt-0.5 text-sm font-semibold tabular-nums', tone)}>{value}</p>
    </div>
  );
}
