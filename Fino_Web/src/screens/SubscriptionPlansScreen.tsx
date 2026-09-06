import { useMemo, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, Upload, X, Package, Star, Layers, Users, Clock, ShieldCheck } from 'lucide-react';
import {
  fetchPlans, fetchServices, savePlan, deletePlan, fetchUsersStatus, setExempt,
  type SubscriptionPlan, type ServiceOption,
} from '@/api/subscription.api';
import { uploadImage, imageUrl } from '@/api/catalog.api';
import { inr, dateOnly } from '@/lib/format';
import { roleLabel } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { Badge } from '@/components/ui/Badge';

type Editing = { open: boolean; plan?: SubscriptionPlan };

// ── Add / edit plan modal ─────────────────────────────────────────────────────
function PlanForm({ plan, services, onCancel, onSave, saving }: {
  plan?: SubscriptionPlan;
  services: ServiceOption[];
  onCancel: () => void;
  onSave: (v: { PlanName: string; Amount: number; ImageURL: string; IsDefault: boolean; ValidityDays: number; ServiceIDs: number[] }) => void;
  saving: boolean;
}) {
  const toast = useToast();
  const [name, setName] = useState(plan?.PlanName ?? '');
  const [amount, setAmount] = useState(String(plan?.Amount ?? '0'));
  const [validity, setValidity] = useState(String(plan?.ValidityDays ?? '30'));
  const [image, setImage] = useState(plan?.ImageURL ?? '');
  const [isDefault, setIsDefault] = useState(Boolean(plan?.IsDefault));
  const [picked, setPicked] = useState<Set<number>>(new Set(plan?.ServiceIDs ?? []));
  const [err, setErr] = useState('');
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  const toggle = (id: number) => setPicked((s) => {
    const next = new Set(s);
    next.has(id) ? next.delete(id) : next.add(id);
    return next;
  });

  const pickImage = async (file?: File) => {
    if (!file) return;
    setUploading(true);
    try {
      const path = await uploadImage(file, 'misc');
      setImage(path);
      toast.success('Image uploaded.');
    } catch (e) { toast.error((e as Error).message); }
    finally { setUploading(false); }
  };

  const submit = () => {
    if (!name.trim()) { setErr('Plan name is required'); return; }
    if (picked.size === 0) { toast.error('Select at least one service for the plan.'); return; }
    onSave({
      PlanName: name.trim(),
      Amount: Number(amount) || 0,
      ImageURL: image,
      IsDefault: isDefault,
      ValidityDays: Math.max(0, Math.floor(Number(validity) || 0)),
      ServiceIDs: [...picked],
    });
  };

  return (
    <div className="space-y-5">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Input label={<>Plan name <span className="text-danger-500">*</span></>} value={name} error={err}
          placeholder="e.g. Retailer Starter, Premium Pack" onChange={(e) => { setName(e.target.value); setErr(''); }} />
        <Input label="Price (₹)" type="number" value={amount} onChange={(e) => setAmount(e.target.value)}
          hint="Charge deducted from the Recharge wallet on subscribe" />
        <Input label="Validity (days)" type="number" value={validity} onChange={(e) => setValidity(e.target.value)}
          hint="Expires after these many days; 0 = lifetime (no renewal)" />
      </div>

      {/* Plan image */}
      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700">Plan image</label>
        <div className="flex items-center gap-3">
          {image ? (
            <div className="relative">
              <img src={imageUrl(image)} alt="Plan" className="size-16 rounded-lg border border-slate-200 object-cover" />
              <button type="button" onClick={() => setImage('')}
                className="absolute -right-1.5 -top-1.5 rounded-full bg-danger-500 p-0.5 text-white shadow"><X className="size-3" /></button>
            </div>
          ) : (
            <div className="flex size-16 items-center justify-center rounded-lg border border-dashed border-slate-300 text-slate-300"><Package className="size-6" /></div>
          )}
          <Button type="button" variant="outline" size="sm" loading={uploading} onClick={() => fileRef.current?.click()}>
            <Upload className="size-4" /> Upload
          </Button>
          <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={(e) => pickImage(e.target.files?.[0])} />
        </div>
      </div>

      {/* Service picker */}
      <div>
        <div className="mb-1.5 flex items-center justify-between">
          <label className="text-sm font-medium text-slate-700">
            Services in this plan <span className="text-danger-500">*</span>
          </label>
          <span className="text-xs text-slate-400">{picked.size} selected</span>
        </div>
        <div className="max-h-64 overflow-y-auto rounded-xl border border-slate-200 p-2">
          {services.length === 0 ? (
            <p className="px-2 py-6 text-center text-sm text-slate-400">No services available.</p>
          ) : (
            <div className="grid grid-cols-1 gap-1.5 sm:grid-cols-2">
              {services.map((s) => {
                const on = picked.has(s.ServiceID);
                return (
                  <button key={s.ServiceID} type="button" onClick={() => toggle(s.ServiceID)}
                    className={cn('flex items-center gap-2.5 rounded-lg border px-3 py-2 text-left text-sm transition-colors',
                      on ? 'border-brand-300 bg-brand-50 text-brand-800' : 'border-slate-200 hover:bg-slate-50 text-slate-700')}>
                    <span className={cn('grid size-4 shrink-0 place-items-center rounded border',
                      on ? 'border-brand-500 bg-brand-500 text-white' : 'border-slate-300')}>
                      {on && <svg viewBox="0 0 12 12" className="size-3" fill="none"><path d="M2.5 6.5l2.5 2.5 4.5-5" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" /></svg>}
                    </span>
                    <span className="truncate">{s.Name}</span>
                  </button>
                );
              })}
            </div>
          )}
        </div>
      </div>

      <label className="flex cursor-pointer items-center gap-2.5 rounded-lg border border-slate-200 px-3 py-2.5">
        <input type="checkbox" checked={isDefault} onChange={(e) => setIsDefault(e.target.checked)}
          className="size-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
        <span className="text-sm font-medium text-slate-700">Default plan</span>
        <span className="text-xs text-slate-400">— shown first; only one plan can be default</span>
      </label>

      <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
        <Button variant="outline" onClick={onCancel} disabled={saving}>Cancel</Button>
        <Button onClick={submit} loading={saving}>{plan ? 'Save plan' : 'Create plan'}</Button>
      </div>
    </div>
  );
}

// ── Subscribers: who is in / out, with the permanent-free toggle ─────────────────
function SubscribersPanel() {
  const toast = useToast();
  const qc = useQueryClient();
  const { data: users = [], isLoading } = useQuery({ queryKey: ['users-sub-status'], queryFn: fetchUsersStatus });

  const exempt = useMutation({
    mutationFn: ({ userId, on }: { userId: number; on: boolean }) => setExempt(userId, on),
    onSuccess: (r) => { if (r.ok) { toast.success(r.message || 'Updated.'); qc.invalidateQueries({ queryKey: ['users-sub-status'] }); } else toast.error(r.message); },
    onError: (e: Error) => toast.error(e.message),
  });

  const active = users.filter((u) => u.Active && !u.Exempt).length;
  const free = users.filter((u) => u.Exempt).length;
  const out = users.filter((u) => !u.Active && !u.Exempt).length;

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        {[
          { label: 'Total users', value: users.length, tone: 'text-slate-900', accent: 'bg-slate-100 text-slate-500', Icon: Users },
          { label: 'Active', value: active, tone: 'text-success-600', accent: 'bg-success-50 text-success-600', Icon: ShieldCheck },
          { label: 'Not subscribed', value: out, tone: 'text-danger-600', accent: 'bg-danger-50 text-danger-600', Icon: Clock },
          { label: 'Free (exempt)', value: free, tone: 'text-brand-600', accent: 'bg-brand-50 text-brand-600', Icon: Star },
        ].map((s) => (
          <div key={s.label} className="flex items-center gap-3 rounded-card border border-slate-200 bg-white px-4 py-3.5 shadow-card">
            <span className={cn('grid size-9 shrink-0 place-items-center rounded-xl', s.accent)}><s.Icon className="size-4" /></span>
            <div className="min-w-0">
              <p className="truncate text-xs font-medium text-slate-400">{s.label}</p>
              <p className={cn('font-display text-xl font-bold tabular-nums', s.tone)}>{s.value}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="overflow-x-auto rounded-card border border-slate-200 bg-white">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
              <th className="px-4 py-2.5 font-semibold">User</th>
              <th className="px-4 py-2.5 font-semibold">Role</th>
              <th className="px-4 py-2.5 font-semibold">Status</th>
              <th className="px-4 py-2.5 font-semibold">Plan</th>
              <th className="px-4 py-2.5 font-semibold">Expiry</th>
              <th className="px-4 py-2.5 text-right font-semibold">Free</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={6} className="px-4 py-10 text-center text-sm text-slate-400">Loading…</td></tr>
            ) : users.length === 0 ? (
              <tr><td colSpan={6} className="px-4 py-10 text-center text-sm text-slate-400">No users to show.</td></tr>
            ) : users.map((u) => (
              <tr key={u.UserID} className="border-b border-slate-50 last:border-0 hover:bg-slate-50/60">
                <td className="px-4 py-3">
                  <div className="font-medium text-slate-800">{u.PersonName || u.FirmName || `User #${u.UserID}`}</div>
                  <div className="text-xs text-slate-400">{u.MobileNumber}</div>
                </td>
                <td className="px-4 py-3 text-slate-600">{roleLabel(u.UserTypeID)}</td>
                <td className="px-4 py-3">
                  {u.Exempt ? <Badge tone="brand">Free</Badge>
                    : u.Active ? <Badge tone="success">Active</Badge>
                    : <Badge tone="danger">Not subscribed</Badge>}
                </td>
                <td className="px-4 py-3 text-slate-600">{u.PlanName || '—'}</td>
                <td className="px-4 py-3 text-slate-600">
                  {u.Exempt ? '—' : u.Active ? (
                    <span className="flex items-center gap-1.5"><Clock className="size-3.5 text-slate-400" />{u.ExpiryDate ? dateOnly(u.ExpiryDate) : '—'} <span className="text-xs text-slate-400">({u.DaysRemaining}d)</span></span>
                  ) : '—'}
                </td>
                <td className="px-4 py-3">
                  <div className="flex justify-end">
                    <button type="button" role="switch" aria-checked={u.Exempt} disabled={exempt.isPending}
                      onClick={() => exempt.mutate({ userId: u.UserID, on: !u.Exempt })}
                      title={u.Exempt ? 'Free — click to require subscription' : 'Mark free (cascades to downline)'}
                      className={cn('relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition-colors disabled:opacity-50',
                        u.Exempt ? 'bg-brand-600' : 'bg-slate-300')}>
                      <span className={cn('inline-block size-4 transform rounded-full bg-white shadow transition-transform', u.Exempt ? 'translate-x-4' : 'translate-x-0.5')} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="text-xs text-slate-400">Marking a Distributor or Super-Distributor “Free” also frees everyone in their downline.</p>
    </div>
  );
}

// ── Main screen ────────────────────────────────────────────────────────────────
export function SubscriptionPlansScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const [tab, setTab] = useState<'plans' | 'subscribers'>('plans');
  const [modal, setModal] = useState<Editing>({ open: false });

  const { data: plans = [], isLoading } = useQuery({ queryKey: ['sub-plans'], queryFn: fetchPlans });
  const { data: services = [] } = useQuery({ queryKey: ['sub-services'], queryFn: fetchServices });

  const serviceName = useMemo(() => {
    const m = new Map<number, string>();
    services.forEach((s) => m.set(s.ServiceID, s.Name));
    return m;
  }, [services]);

  const refresh = () => qc.invalidateQueries({ queryKey: ['sub-plans'] });

  const save = useMutation({
    mutationFn: (v: { PlanName: string; Amount: number; ImageURL: string; IsDefault: boolean; ValidityDays: number; ServiceIDs: number[] }) =>
      savePlan({ PlanID: modal.plan?.ServiceSubscriptionCreatePlanMasterID ?? 0, ...v }),
    onSuccess: (r) => { if (r.ok) { toast.success('Plan saved.'); setModal({ open: false }); refresh(); } else toast.error(r.message); },
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: (id: number) => deletePlan(id),
    onSuccess: (r) => { if (r.ok) { toast.success('Plan deleted.'); refresh(); } else toast.error(r.message); },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <div className="space-y-5">
      <PageHeader
        title="Subscription Plans"
        subtitle="Bundle services into a paid plan users can subscribe to"
        actions={tab === 'plans' ? <Button size="sm" onClick={() => setModal({ open: true })}><Plus className="size-4" /> Add plan</Button> : undefined}
      />

      {/* Tabs */}
      <div className="flex gap-1 rounded-lg border border-slate-200 bg-slate-50 p-1">
        <button onClick={() => setTab('plans')}
          className={cn('flex flex-1 items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
            tab === 'plans' ? 'bg-white text-brand-700 shadow-sm' : 'text-slate-500 hover:text-slate-700')}>
          <Package className="size-4" /> Plans
        </button>
        <button onClick={() => setTab('subscribers')}
          className={cn('flex flex-1 items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
            tab === 'subscribers' ? 'bg-white text-brand-700 shadow-sm' : 'text-slate-500 hover:text-slate-700')}>
          <Users className="size-4" /> Subscribers
        </button>
      </div>

      {tab === 'subscribers' ? <SubscribersPanel /> : isLoading ? (
        <div className="rounded-card border border-slate-200 bg-white px-4 py-16 text-center text-sm text-slate-400">Loading plans…</div>
      ) : plans.length === 0 ? (
        <div className="rounded-card border border-dashed border-slate-300 bg-white px-4 py-16 text-center">
          <Package className="mx-auto mb-3 size-8 text-slate-300" />
          <p className="text-sm font-medium text-slate-600">No subscription plans yet</p>
          <p className="mt-1 text-sm text-slate-400">Create a plan to bundle services and let users subscribe.</p>
          <Button className="mt-4" size="sm" onClick={() => setModal({ open: true })}><Plus className="size-4" /> Add plan</Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {plans.map((p) => {
            const id = p.ServiceSubscriptionCreatePlanMasterID;
            return (
              <div key={id} className={cn('group relative flex flex-col overflow-hidden rounded-card bg-white shadow-card transition-all hover:-translate-y-0.5 hover:shadow-pop',
                p.IsDefault ? 'border border-brand-300 ring-1 ring-brand-500/10' : 'border border-slate-200 hover:border-slate-300')}>
                <span className={cn('absolute inset-x-0 top-0 h-1', p.IsDefault ? 'bg-gradient-to-r from-brand-500 to-brand-600' : 'bg-transparent')} />

                <div className="flex flex-col p-5 pt-6">
                  <div className="flex items-start gap-3">
                    {p.ImageURL ? (
                      <img src={imageUrl(p.ImageURL)} alt={p.PlanName} className="size-12 rounded-xl border border-slate-200 object-cover" />
                    ) : (
                      <span className={cn('grid size-12 place-items-center rounded-xl', p.IsDefault ? 'bg-brand-50 text-brand-600' : 'bg-slate-100 text-slate-400')}><Package className="size-6" /></span>
                    )}
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <h3 className="truncate font-display text-sm font-bold text-slate-900">{p.PlanName}</h3>
                        {p.IsDefault && <Badge tone="brand"><Star className="mr-0.5 size-3" /> Default</Badge>}
                      </div>
                      <p className="mt-1 flex items-baseline gap-1">
                        <span className="font-display text-2xl font-extrabold tabular-nums tracking-tight text-slate-900">{inr(p.Amount)}</span>
                        <span className="text-xs font-medium text-slate-400">{p.ValidityDays > 0 ? `/ ${p.ValidityDays}d` : '/ lifetime'}</span>
                      </p>
                    </div>
                  </div>

                  <div className="mt-4 flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wider text-slate-400">
                    <Layers className="size-3.5" /> {p.ServiceCount} service{p.ServiceCount === 1 ? '' : 's'}
                  </div>
                  <div className="mt-2 flex flex-wrap gap-1.5">
                    {p.ServiceIDs.slice(0, 6).map((sid) => (
                      <span key={sid} className="rounded-full bg-slate-100 px-2.5 py-1 text-[11px] font-medium text-slate-600">
                        {serviceName.get(sid) ?? `#${sid}`}
                      </span>
                    ))}
                    {p.ServiceIDs.length > 6 && (
                      <span className="rounded-full bg-slate-100 px-2.5 py-1 text-[11px] font-medium text-slate-500">+{p.ServiceIDs.length - 6} more</span>
                    )}
                    {p.ServiceIDs.length === 0 && <span className="text-[11px] text-slate-400">No services yet</span>}
                  </div>
                </div>

                <div className="mt-auto flex justify-end gap-1 border-t border-slate-100 px-5 py-3">
                  <button title="Edit" onClick={() => setModal({ open: true, plan: p })}
                    className="rounded-lg p-2 text-slate-400 transition-colors hover:bg-brand-50 hover:text-brand-600"><Pencil className="size-4" /></button>
                  <button title="Delete" onClick={() => { if (confirm(`Delete plan “${p.PlanName}”?`)) remove.mutate(id); }}
                    className="rounded-lg p-2 text-slate-400 transition-colors hover:bg-danger-50 hover:text-danger-600"><Trash2 className="size-4" /></button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      <Modal open={modal.open} onClose={() => setModal({ open: false })} title={modal.plan ? 'Edit plan' : 'Add plan'} size="lg">
        <PlanForm plan={modal.plan} services={services} saving={save.isPending}
          onCancel={() => setModal({ open: false })} onSave={(v) => save.mutate(v)} />
      </Modal>
    </div>
  );
}
