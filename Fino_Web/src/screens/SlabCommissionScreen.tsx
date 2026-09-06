import { useMemo, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Save, Trash2 } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Autocomplete } from '@/components/ui/Autocomplete';

const OPT_PAYLOAD = { IsActive: true, PageNumber: 1, PageSize: 500 };

type CatKey = 'COM' | 'SUR';
type TypeKey = 'PER' | 'FIX';
type RoleKey = 'Retailer' | 'Distributor' | 'SuperDistributor' | 'Client';

const ROLES: { key: RoleKey; label: string }[] = [
  { key: 'Retailer', label: 'Retailer' },
  { key: 'Distributor', label: 'Distributor' },
  { key: 'SuperDistributor', label: 'Super Distributor' },
  { key: 'Client', label: 'Client' },
];

interface RoleComm {
  value: string;
  category: CatKey;
  type: TypeKey;
}
const emptyRole = (): RoleComm => ({ value: '0', category: 'COM', type: 'PER' });
const emptyRoles = (): Record<RoleKey, RoleComm> =>
  ({ Retailer: emptyRole(), Distributor: emptyRole(), SuperDistributor: emptyRole(), Client: emptyRole() });

// The commission engine reads CommType / CommissionCategory as free text, and the vocabulary it
// understands is Flexi|Fix and Commission|Surcharge (the same words the provider commission editor
// writes). This screen used to save "Percentage"/"Flat" and "COM"/"SUR", which matched nothing:
// every PER row was applied as a flat amount and every SUR row was paid out instead of charged.
const toApiType = (t: TypeKey) => (t === 'PER' ? 'Flexi' : 'Fix');
const toApiCat = (c: CatKey) => (c === 'SUR' ? 'Surcharge' : 'Commission');
// Read side stays tolerant so rows written by any older spelling still display correctly.
const fromApiType = (t: string): TypeKey => (/flexi|percent|per|%/i.test(t) ? 'PER' : 'FIX');
const asCat = (c: string): CatKey => (/sur|charge/i.test(c) ? 'SUR' : 'COM');

/** Two-option segmented toggle (COM/SUR, PER/FIX). */
function Segmented<T extends string>({
  value,
  options,
  onChange,
}: {
  value: T;
  options: { value: T; label: string }[];
  onChange: (v: T) => void;
}) {
  return (
    <div className="inline-flex overflow-hidden rounded-lg border border-slate-200">
      {options.map((o) => (
        <button
          key={o.value}
          type="button"
          onClick={() => onChange(o.value)}
          className={cn(
            'px-2.5 py-1 text-xs font-semibold transition-colors',
            value === o.value ? 'bg-brand-600 text-white' : 'bg-white text-slate-500 hover:bg-slate-50',
          )}
        >
          {o.label}
        </button>
      ))}
    </div>
  );
}

const CAT_OPTS: { value: CatKey; label: string }[] = [
  { value: 'COM', label: 'COM' },
  { value: 'SUR', label: 'SUR' },
];
const TYPE_OPTS: { value: TypeKey; label: string }[] = [
  { value: 'PER', label: 'PER' },
  { value: 'FIX', label: 'FIX' },
];

/** One role's editable commission block: General value + COM/SUR + PER/FIX. */
function RoleCommBlock({
  label,
  state,
  onChange,
}: {
  label: string;
  state: RoleComm;
  onChange: (next: RoleComm) => void;
}) {
  return (
    <div className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
      <p className="mb-2 text-sm font-semibold text-slate-700">{label}</p>
      <div className="mb-2 flex items-center gap-2">
        <span className="text-xs text-slate-500">General</span>
        <input
          type="number"
          step="0.01"
          value={state.value}
          onChange={(e) => onChange({ ...state, value: e.target.value })}
          className="h-9 w-full rounded-lg border border-slate-200 px-2 text-sm tabular-nums focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20"
        />
      </div>
      <div className="flex flex-wrap items-center gap-2">
        <Segmented value={state.category} options={CAT_OPTS} onChange={(category) => onChange({ ...state, category })} />
        <Segmented value={state.type} options={TYPE_OPTS} onChange={(type) => onChange({ ...state, type })} />
      </div>
    </div>
  );
}

/** Builds the create/update payload shared by both flows. */
function buildPayload(
  base: { SlabMasterID: number; ServiceID: number; OperatorID: number; FromAmount: number; ToAmount: number },
  roles: Record<RoleKey, RoleComm>,
) {
  const forRole = (r: RoleKey) => {
    const s = roles[r];
    const v = Number(s.value) || 0;
    return {
      [`${r}Comm`]: v,
      [`${r}CommType`]: toApiType(s.type),
      [`${r}CommissionCategory`]: toApiCat(s.category),
      // Mirror to the "Real" fields the engine reads for the actually-applied rate.
      [`Real${r}Comm`]: v,
      [`Real${r}CommType`]: toApiType(s.type),
      [`Real${r}CommissionCategory`]: toApiCat(s.category),
    };
  };
  return {
    ...base,
    ...forRole('Retailer'),
    ...forRole('Distributor'),
    ...forRole('SuperDistributor'),
    ...forRole('Client'),
    IsActive: true,
  };
}

type Row = Record<string, unknown>;

/** One editable row in the Commission Details table. */
function DetailRow({
  row,
  slabId,
  onSaved,
}: {
  row: Row;
  slabId: number;
  onSaved: () => void;
}) {
  const toast = useToast();
  const [roles, setRoles] = useState<Record<RoleKey, RoleComm>>(() => {
    const init = emptyRoles();
    for (const { key } of ROLES) {
      init[key] = {
        value: String(getField(row, `${key}Comm`) ?? '0'),
        category: asCat(String(getField(row, `${key}CommissionCategory`) ?? 'COM')),
        type: fromApiType(String(getField(row, `${key}CommType`) ?? 'Percentage')),
      };
    }
    return init;
  });

  const saveMut = useMutation({
    mutationFn: () =>
      apiPost('SlabDetail/Update', {
        SlabDetailID: getField(row, 'SlabDetailID'),
        ...buildPayload(
          {
            SlabMasterID: slabId,
            ServiceID: Number(getField(row, 'ServiceID') ?? 0),
            OperatorID: Number(getField(row, 'OperatorID') ?? 0),
            FromAmount: Number(getField(row, 'FromAmount') ?? 0),
            ToAmount: Number(getField(row, 'ToAmount') ?? 0),
          },
          roles,
        ),
      }),
    onSuccess: (r) => (r.ok ? (toast.success('Commission updated.'), onSaved()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const delMut = useMutation({
    mutationFn: () => apiPost('SlabDetail/Delete', { SlabDetailID: getField(row, 'SlabDetailID') }),
    onSuccess: (r) => (r.ok ? (toast.success('Removed.'), onSaved()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <tr className="border-b border-slate-100 last:border-0">
      <td className="whitespace-nowrap px-3 py-3 align-top text-sm">
        <p className="font-medium text-slate-800">{String(getField(row, 'OperatorName') ?? '—')}</p>
        <p className="text-xs text-slate-500">{String(getField(row, 'ServiceName') ?? '')}</p>
        <p className="text-xs text-slate-400">
          ₹{String(getField(row, 'FromAmount') ?? 0)} – ₹{String(getField(row, 'ToAmount') ?? 0)}
        </p>
      </td>
      {ROLES.map(({ key }) => (
        <td key={key} className="px-3 py-3 align-top">
          <div className="flex flex-col gap-1.5">
            <input
              type="number"
              step="0.01"
              value={roles[key].value}
              onChange={(e) => setRoles((p) => ({ ...p, [key]: { ...p[key], value: e.target.value } }))}
              className="h-8 w-24 rounded-lg border border-slate-200 px-2 text-sm tabular-nums focus:border-brand-500 focus:outline-none"
            />
            <div className="flex gap-1">
              <Segmented value={roles[key].category} options={CAT_OPTS} onChange={(category) => setRoles((p) => ({ ...p, [key]: { ...p[key], category } }))} />
              <Segmented value={roles[key].type} options={TYPE_OPTS} onChange={(type) => setRoles((p) => ({ ...p, [key]: { ...p[key], type } }))} />
            </div>
          </div>
        </td>
      ))}
      <td className="px-3 py-3 align-top">
        <div className="flex items-center gap-1">
          <Button size="sm" variant="ghost" loading={saveMut.isPending} onClick={() => saveMut.mutate()} title="Save">
            <Save className="size-4 text-brand-600" />
          </Button>
          <Button size="sm" variant="ghost" className="text-danger-600 hover:bg-danger-50" loading={delMut.isPending} onClick={() => { if (window.confirm('Delete this commission row?')) delMut.mutate(); }} title="Delete">
            <Trash2 className="size-4" />
          </Button>
        </div>
      </td>
    </tr>
  );
}

/**
 * Slab → Commission setup ("Set Commission Range"). Reached from the Slabs list Commission
 * link (?id=&name=). Rebuilds the legacy SlabCommission editor on our design system:
 *  - top form: Service + Operator + From/To amount, and a per-role (Retailer/Distributor/
 *    SuperDistributor/Client) commission block with COM/SUR and PER/FIX toggles → creates a row.
 *  - Commission Details table: existing rows for the slab, each editable inline + delete.
 */
export function SlabCommissionScreen() {
  const params = new URLSearchParams(useLocation().search);
  const slabId = Number(params.get('id') || 0);
  const slabName = params.get('name') || 'Slab';
  const toast = useToast();
  const qc = useQueryClient();

  const [serviceId, setServiceId] = useState('');
  const [operatorId, setOperatorId] = useState('');
  const [fromAmount, setFromAmount] = useState('0');
  const [toAmount, setToAmount] = useState('');
  const [roles, setRoles] = useState<Record<RoleKey, RoleComm>>(emptyRoles);

  const services = useQuery({
    queryKey: ['fk', 'Service/GetAll'],
    queryFn: () => fetchList('Service/GetAll', OPT_PAYLOAD),
    staleTime: 60_000,
  });
  const operatorsAll = useQuery({
    queryKey: ['fk', 'Operator/GetAll'],
    queryFn: () => fetchList('Operator/GetAll', OPT_PAYLOAD),
    staleTime: 60_000,
  });
  const operators = useMemo(
    () => (operatorsAll.data ?? []).filter((o) => !serviceId || String(getField(o, 'ServiceID')) === serviceId),
    [operatorsAll.data, serviceId],
  );

  const detailsKey = ['slab-details', slabId];
  const details = useQuery({
    queryKey: detailsKey,
    queryFn: () => fetchList('SlabDetail/GetBySlabMasterID', { SlabMasterID: slabId }),
    enabled: slabId > 0,
  });
  const refresh = () => qc.invalidateQueries({ queryKey: detailsKey });

  // ── Amount-range rules ──────────────────────────────────────────────────
  // Only one row may ever match a transaction amount, so the bands set up for one
  // service + operator have to be disjoint: with 1 – 1000 already saved, the next band starts at
  // 1001. The API enforces the same rule; this is the immediate feedback.
  const rangeRowsFor = (svc: string, op: string) =>
    (details.data ?? []).filter(
      (r) => String(getField(r, 'ServiceID')) === svc && String(getField(r, 'OperatorID')) === op,
    );
  const nextFromFor = (svc: string, op: string) => {
    const rows = rangeRowsFor(svc, op);
    return rows.length ? Math.max(...rows.map((r) => Number(getField(r, 'ToAmount')) || 0)) + 1 : 1;
  };

  // An empty To Amount is simply "not filled in yet": it blocks Save without showing a red error.
  const rangeIncomplete = !serviceId || !operatorId || toAmount.trim() === '' || fromAmount.trim() === '';

  const rangeError = useMemo(() => {
    if (rangeIncomplete) return '';
    const from = Number(fromAmount);
    const to = Number(toAmount);
    if (!Number.isFinite(from) || !Number.isFinite(to) || to <= 0) return 'Enter a valid amount range.';
    if (from < 0) return 'From Amount cannot be negative.';
    if (to < from) return 'To Amount must be greater than or equal to From Amount.';

    const clash = rangeRowsFor(serviceId, operatorId).find(
      (r) => from <= (Number(getField(r, 'ToAmount')) || 0) && to >= (Number(getField(r, 'FromAmount')) || 0),
    );
    if (!clash) return '';
    const money = (v: unknown) => Number(v ?? 0).toLocaleString('en-IN');
    return `This range overlaps ₹${money(getField(clash, 'FromAmount'))} – ₹${money(getField(clash, 'ToAmount'))}, which is already set for this operator. Start this range at ₹${money(nextFromFor(serviceId, operatorId))}.`;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [details.data, serviceId, operatorId, fromAmount, toAmount, rangeIncomplete]);

  const createMut = useMutation({
    mutationFn: () =>
      apiPost('SlabDetail/Create', buildPayload(
        { SlabMasterID: slabId, ServiceID: Number(serviceId), OperatorID: Number(operatorId), FromAmount: Number(fromAmount) || 0, ToAmount: Number(toAmount) || 0 },
        roles,
      )),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success('Commission range saved.');
      setRoles(emptyRoles());
      setOperatorId('');
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const onSave = () => {
    if (!serviceId) return toast.error('Please select a service.');
    if (!operatorId) return toast.error('Please select a valid operator.');
    if (rangeIncomplete) return toast.error('Enter a valid amount range.');
    if (rangeError) return toast.error(rangeError);
    createMut.mutate();
  };

  const selectedServiceName = useMemo(
    () => String(getField((services.data ?? []).find((s) => String(getField(s, 'ServiceID')) === serviceId) ?? {}, 'Name') ?? ''),
    [services.data, serviceId],
  );

  // Commission Details is shown service-wise: when a service is picked above, only that
  // service's rows are listed (matching the legacy screen); otherwise all rows for the slab.
  const visibleRows = useMemo(() => {
    const all = details.data ?? [];
    return serviceId ? all.filter((r) => String(getField(r, 'ServiceID')) === serviceId) : all;
  }, [details.data, serviceId]);

  if (!slabId) return <PageHeader title="Commission setup" subtitle="No slab selected — open this from the Slabs list." />;

  const rows = visibleRows;

  return (
    <>
      <Link to="/s/Slabs">
        <Button variant="ghost" size="sm" className="mb-2">
          <ArrowLeft className="size-4" />
          Back to slabs
        </Button>
      </Link>
      <PageHeader title={`Set Commission Range — ${slabName}`} subtitle="Service / operator commission per role for this slab" />

      {/* ── Set Commission Range form ─────────────────────────────────────────── */}
      <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <Autocomplete
            label="Service"
            placeholder="Search service…"
            value={serviceId}
            options={(services.data ?? []).map((s) => ({ value: String(getField(s, 'ServiceID')), label: String(getField(s, 'Name') ?? '') }))}
            onChange={(v) => { setServiceId(String(v)); setOperatorId(''); }}
          />
          <Autocomplete
            label="Operator"
            placeholder="Search operator…"
            value={operatorId}
            options={operators.map((o) => ({ value: String(getField(o, 'OperatorID')), label: String(getField(o, 'OperatorName') ?? '') }))}
            onChange={(v) => {
              const op = String(v);
              setOperatorId(op);
              // Start the new band where the last one for this operator ended.
              if (op) { setFromAmount(String(nextFromFor(serviceId, op))); setToAmount(''); }
            }}
          />
          <div>
            <div className="grid grid-cols-2 gap-3">
              <Input label="From Amount" type="number" value={fromAmount} onChange={(e) => setFromAmount(e.target.value)} />
              <Input label="To Amount" type="number" value={toAmount} onChange={(e) => setToAmount(e.target.value)} />
            </div>
            {rangeError ? (
              <p className="mt-1.5 text-xs font-medium text-danger-600">{rangeError}</p>
            ) : serviceId && operatorId ? (
              <p className="mt-1.5 text-xs text-slate-400">
                Ranges cannot overlap — the next one starts at ₹{Number(nextFromFor(serviceId, operatorId)).toLocaleString('en-IN')}.
              </p>
            ) : null}
          </div>
        </div>

        <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {ROLES.map(({ key, label }) => (
            <RoleCommBlock key={key} label={label} state={roles[key]} onChange={(next) => setRoles((p) => ({ ...p, [key]: next }))} />
          ))}
        </div>

        <div className="mt-4 flex justify-end gap-2">
          <Button variant="secondary" onClick={() => { setRoles(emptyRoles()); setOperatorId(''); setServiceId(''); setFromAmount('0'); setToAmount(''); }}>Reset</Button>
          <Button loading={createMut.isPending} disabled={!!rangeError || rangeIncomplete} onClick={onSave}><Save className="size-4" />Save</Button>
        </div>
      </div>

      {/* ── Commission Details (service-wise) ─────────────────────────────────── */}
      <h2 className="mb-2 font-display text-base font-bold text-slate-900">
        Commission Details{selectedServiceName ? ` — ${selectedServiceName}` : ''}
      </h2>
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3">Details</th>
                {ROLES.map(({ key, label }) => (<th key={key} className="px-3 py-3">{label}</th>))}
                <th className="px-3 py-3">Action</th>
              </tr>
            </thead>
            <tbody>
              {details.isFetching && rows.length === 0 ? (
                <tr><td colSpan={ROLES.length + 2} className="px-3 py-8 text-center text-sm text-slate-400">Loading…</td></tr>
              ) : rows.length === 0 ? (
                <tr><td colSpan={ROLES.length + 2} className="px-3 py-8 text-center text-sm text-slate-400">{serviceId ? `No commission set for ${selectedServiceName} yet — add one using the form above.` : 'No commission set yet — add one using the form above.'}</td></tr>
              ) : (
                rows.map((row, i) => <DetailRow key={String(getField(row, 'SlabDetailID') ?? i)} row={row} slabId={slabId} onSaved={refresh} />)
              )}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
