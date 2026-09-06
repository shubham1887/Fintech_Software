import { useMemo, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ChevronDown, Eye, EyeOff, ImagePlus, Layers, ListChecks, Pencil, Plus, Trash2, Wallet as WalletIcon,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import {
  fetchCatalog, imageUrl, updateServiceField, uploadImage,
  type CatalogCategory, type CatalogService,
} from '@/api/catalog.api';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { StatTile } from '@/components/ui/StatTile';
import { ToggleDot } from './providerShared';
import type { CustomScreenProps } from './registry';

interface WalletOption { id: number; name: string }
interface MappingRow { id: number; serviceId: number; walletId: number }

interface ServiceForm {
  Name: string;
  ServiceCategory: string;
  UISequence: number;
  Charge: number;
  MinBalance: number;
  IsBBPS: boolean;
  IsActive: boolean;
  IsDisplay: boolean;
  ServiceImagePath: string;
  ReportKind: string;
}

// Report mapping options — mirrors backend ServiceKinds ('' = auto by keywords).
const REPORT_KIND_OPTIONS = [
  { value: '', label: 'Auto (by name/category)' },
  { value: 'RECHARGE', label: 'Recharge Report (Prepaid / DTH / Postpaid)' },
  { value: 'BBPS', label: 'BBPS Report (all bills)' },
  { value: 'DMT', label: 'DMT Report' },
  { value: 'PAYOUT', label: 'Payout Report' },
  { value: 'AEPS', label: 'AEPS Report' },
  { value: 'MATM', label: 'MicroATM Report' },
  { value: 'CASHDEPOSIT', label: 'Cash Deposit' },
  { value: 'NONE', label: 'No report' },
];

const emptyForm = (category = ''): ServiceForm => ({
  Name: '', ServiceCategory: category, UISequence: 0, Charge: 0, MinBalance: 0,
  IsBBPS: false, IsActive: true, IsDisplay: true, ServiceImagePath: '', ReportKind: '',
});

/**
 * Service Catalog — the admin console for the whole Category → Service → Operator tree.
 * Per-service: icon upload, Active + Display toggles, wallet mapping, edit, delete.
 * Per-category: bulk on/off, bulk show/hide, map every service to one wallet in a click.
 */
export function ServiceCatalogScreen(_props: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});
  const [editing, setEditing] = useState<CatalogService | null>(null);
  const [adding, setAdding] = useState<string | null>(null); // category prefill, '' = blank
  const [deleting, setDeleting] = useState<CatalogService | null>(null);

  const catalogQ = useQuery({ queryKey: ['service-catalog'], queryFn: fetchCatalog });
  const walletsQ = useQuery({
    queryKey: ['wallet-options'],
    queryFn: async (): Promise<WalletOption[]> => {
      const res = await apiPost<unknown[]>('Wallet/Select', {});
      return (Array.isArray(res.data) ? res.data : []).map((r) => ({
        id: Number(getField(r as Record<string, unknown>, 'WalletID') ?? 0),
        name: String(getField(r as Record<string, unknown>, 'Name', 'WalletName') ?? ''),
      })).filter((w) => w.id > 0);
    },
  });
  const mappingsQ = useQuery({
    queryKey: ['wallet-service-mappings'],
    queryFn: async (): Promise<MappingRow[]> => {
      const res = await apiPost<unknown[]>('WalletServiceMapping/GetAll', {});
      return (Array.isArray(res.data) ? res.data : []).map((r) => ({
        id: Number(getField(r as Record<string, unknown>, 'WalletServiceMappingID') ?? 0),
        serviceId: Number(getField(r as Record<string, unknown>, 'ServiceID') ?? 0),
        walletId: Number(getField(r as Record<string, unknown>, 'WalletID') ?? 0),
      }));
    },
  });

  const categories = catalogQ.data ?? [];
  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['service-catalog'] });
    qc.invalidateQueries({ queryKey: ['wallet-service-mappings'] });
  };

  const stats = useMemo(() => {
    const services = categories.flatMap((c) => c.Services);
    return {
      categories: categories.length,
      services: services.length,
      operators: services.reduce((n, s) => n + s.OperatorCount, 0),
      hidden: services.filter((s) => !s.IsDisplay || !s.IsActive).length,
    };
  }, [categories]);

  // ------------------------------------------------------------ mutations ----
  const fieldMut = useMutation({
    mutationFn: ({ svc, field, value }: { svc: CatalogService; field: string; value: unknown }) =>
      updateServiceField(svc.ServiceID, field, '-', value),
    onSuccess: (res) => { if (!res.ok) toast.error(res.message); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const walletMut = useMutation({
    mutationFn: async ({ serviceId, walletId }: { serviceId: number; walletId: number }) => {
      const existing = (mappingsQ.data ?? []).find((m) => m.serviceId === serviceId);
      return existing
        ? apiPost('WalletServiceMapping/Update', { WalletServiceMappingID: existing.id, ServiceID: serviceId, WalletID: walletId })
        : apiPost('WalletServiceMapping/Create', { ServiceID: serviceId, WalletID: walletId });
    },
    onSuccess: (res) => { if (!res.ok) toast.error(res.message); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const iconMut = useMutation({
    mutationFn: async ({ svc, file }: { svc: CatalogService; file: File }) => {
      const path = await uploadImage(file, 'services');
      const res = await updateServiceField(svc.ServiceID, 'serviceimagepath', svc.ServiceImagePath, path);
      if (!res.ok) throw new Error(res.message);
    },
    onSuccess: () => { toast.success('Icon updated.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const bulkMut = useMutation({
    mutationFn: async ({ cat, field, value }: { cat: CatalogCategory; field: 'isactive' | 'isdisplay'; value: boolean }) => {
      for (const svc of cat.Services) {
        const current = field === 'isactive' ? svc.IsActive : svc.IsDisplay;
        if (current !== value) await updateServiceField(svc.ServiceID, field, current, value);
      }
    },
    onSuccess: () => { toast.success('Category updated.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const bulkWalletMut = useMutation({
    mutationFn: async ({ cat, walletId }: { cat: CatalogCategory; walletId: number }) => {
      const mappings = mappingsQ.data ?? [];
      for (const svc of cat.Services) {
        const existing = mappings.find((m) => m.serviceId === svc.ServiceID);
        if (existing?.walletId === walletId) continue;
        const res = existing
          ? await apiPost('WalletServiceMapping/Update', { WalletServiceMappingID: existing.id, ServiceID: svc.ServiceID, WalletID: walletId })
          : await apiPost('WalletServiceMapping/Create', { ServiceID: svc.ServiceID, WalletID: walletId });
        if (!res.ok) throw new Error(res.message);
      }
    },
    onSuccess: () => { toast.success('Wallet mapped for the whole category.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (svc: CatalogService) => apiPost('Service/Delete', { ServiceID: svc.ServiceID }),
    onSuccess: (res) => {
      if (res.ok) toast.success('Service removed.'); else toast.error(res.message);
      setDeleting(null); refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const categoryNames = useMemo(() => categories.map((c) => c.Category), [categories]);

  return (
    <>
      <PageHeader
        title="Service Catalog"
        subtitle="Categories, services, operators, icons & wallet mapping — what retailers see"
        actions={<Button onClick={() => setAdding('')}><Plus className="size-4" />Add service</Button>}
      />

      <div className="mb-5 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatTile label="Categories" value={stats.categories} icon={Layers} tone="bg-brand-50 text-brand-600" loading={catalogQ.isLoading} />
        <StatTile label="Services" value={stats.services} icon={ListChecks} tone="bg-info-50 text-info-600" loading={catalogQ.isLoading} />
        <StatTile label="Operators" value={stats.operators} icon={WalletIcon} tone="bg-success-50 text-success-600" loading={catalogQ.isLoading} />
        <StatTile label="Hidden / off" value={stats.hidden} icon={EyeOff} tone="bg-warning-50 text-warning-600" loading={catalogQ.isLoading} />
      </div>

      {catalogQ.isLoading ? (
        <div className="rounded-card border border-slate-200/70 bg-white p-10 text-center text-sm text-slate-400 shadow-card">Loading catalog…</div>
      ) : categories.length === 0 ? (
        <EmptyState title="No services yet" description="Add your first service to build the catalog." />
      ) : (
        <div className="space-y-4">
          {categories.map((cat) => {
            const isCollapsed = collapsed[cat.Category] ?? false;
            const allOn = cat.Services.every((s) => s.IsActive);
            const allShown = cat.Services.every((s) => s.IsDisplay);
            const walletIds = [...new Set(cat.Services.map((s) => s.WalletID ?? 0))];
            const categoryWallet = walletIds.length === 1 ? walletIds[0] : 0;
            return (
              <section key={cat.Category} className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
                {/* Category header */}
                <div className="flex flex-wrap items-center gap-3 border-b border-slate-100 bg-slate-50/60 px-4 py-3">
                  <button
                    type="button"
                    onClick={() => setCollapsed((c) => ({ ...c, [cat.Category]: !isCollapsed }))}
                    className="flex items-center gap-2 text-left"
                  >
                    <ChevronDown className={cn('size-4 text-slate-400 transition-transform', isCollapsed && '-rotate-90')} />
                    <span className="font-display text-sm font-bold text-slate-800">{cat.Category}</span>
                    <span className="rounded-full bg-brand-50 px-2 py-0.5 text-[11px] font-semibold text-brand-700">
                      {cat.Services.length} service{cat.Services.length === 1 ? '' : 's'}
                    </span>
                  </button>
                  <div className="ml-auto flex flex-wrap items-center gap-4">
                    {/* Category-level wallet mapping */}
                    <label className="flex items-center gap-2 text-xs font-medium text-slate-500">
                      <WalletIcon className="size-3.5" />
                      Wallet
                      <select
                        value={categoryWallet}
                        disabled={bulkWalletMut.isPending || walletsQ.isLoading}
                        onChange={(e) => {
                          const walletId = Number(e.target.value);
                          if (walletId > 0) bulkWalletMut.mutate({ cat, walletId });
                        }}
                        className="rounded-lg border border-slate-200 bg-white px-2 py-1 text-xs font-semibold text-slate-700 focus:border-brand-400 focus:outline-none"
                      >
                        <option value={0}>{walletIds.length > 1 ? 'Mixed' : 'Select…'}</option>
                        {(walletsQ.data ?? []).map((w) => <option key={w.id} value={w.id}>{w.name}</option>)}
                      </select>
                    </label>
                    <label className="flex items-center gap-1.5 text-xs font-medium text-slate-500">
                      All on
                      <ToggleDot on={allOn} busy={bulkMut.isPending} onClick={() => bulkMut.mutate({ cat, field: 'isactive', value: !allOn })} />
                    </label>
                    <label className="flex items-center gap-1.5 text-xs font-medium text-slate-500">
                      All visible
                      <ToggleDot on={allShown} busy={bulkMut.isPending} onClick={() => bulkMut.mutate({ cat, field: 'isdisplay', value: !allShown })} />
                    </label>
                    <Button size="sm" variant="outline" onClick={() => setAdding(cat.Category)}>
                      <Plus className="size-3.5" />Add
                    </Button>
                  </div>
                </div>

                {/* Services */}
                {!isCollapsed && (
                  <ul className="divide-y divide-slate-100">
                    {cat.Services.map((svc) => (
                      <ServiceRow
                        key={svc.ServiceID}
                        svc={svc}
                        wallets={walletsQ.data ?? []}
                        busy={fieldMut.isPending || walletMut.isPending || iconMut.isPending}
                        onToggle={(field, value) => fieldMut.mutate({ svc, field, value })}
                        onWallet={(walletId) => walletMut.mutate({ serviceId: svc.ServiceID, walletId })}
                        onIcon={(file) => iconMut.mutate({ svc, file })}
                        onEdit={() => setEditing(svc)}
                        onDelete={() => setDeleting(svc)}
                      />
                    ))}
                  </ul>
                )}
              </section>
            );
          })}
        </div>
      )}

      {/* Add / edit modals */}
      {adding !== null && (
        <ServiceFormModal
          title="Add service"
          categories={categoryNames}
          initial={emptyForm(adding)}
          onClose={() => setAdding(null)}
          onSaved={() => { setAdding(null); refresh(); }}
        />
      )}
      {editing && (
        <ServiceFormModal
          title={`Edit — ${editing.Name}`}
          categories={categoryNames}
          initial={{
            Name: editing.Name, ServiceCategory: '', UISequence: editing.UISequence,
            Charge: editing.Charge, MinBalance: editing.MinBalance, IsBBPS: !!editing.IsBBPS,
            IsActive: editing.IsActive, IsDisplay: editing.IsDisplay, ServiceImagePath: editing.ServiceImagePath,
            ReportKind: editing.ReportKind ?? '',
          }}
          editingService={editing}
          editingCategory={categories.find((c) => c.Services.some((s) => s.ServiceID === editing.ServiceID))?.Category ?? ''}
          onClose={() => setEditing(null)}
          onSaved={() => { setEditing(null); refresh(); }}
        />
      )}

      {/* Delete confirm */}
      {deleting && (
        <Modal open onClose={() => setDeleting(null)} title="Remove service?">
          <p className="text-sm text-slate-600">
            <span className="font-semibold text-slate-800">{deleting.Name}</span> will be removed from the catalog
            (its {deleting.OperatorCount} operator{deleting.OperatorCount === 1 ? '' : 's'} will no longer be offered).
          </p>
          <div className="mt-5 flex justify-end gap-2">
            <Button variant="outline" onClick={() => setDeleting(null)}>Cancel</Button>
            <Button variant="danger" loading={deleteMut.isPending} onClick={() => deleteMut.mutate(deleting)}>
              <Trash2 className="size-4" />Remove
            </Button>
          </div>
        </Modal>
      )}
    </>
  );
}

// ------------------------------------------------------------- service row ----
function ServiceRow({
  svc, wallets, busy, onToggle, onWallet, onIcon, onEdit, onDelete,
}: {
  svc: CatalogService;
  wallets: WalletOption[];
  busy: boolean;
  onToggle: (field: 'isactive' | 'isdisplay', value: boolean) => void;
  onWallet: (walletId: number) => void;
  onIcon: (file: File) => void;
  onEdit: () => void;
  onDelete: () => void;
}) {
  const fileRef = useRef<HTMLInputElement>(null);
  const icon = imageUrl(svc.ServiceImagePath);
  return (
    <li className={cn('flex flex-wrap items-center gap-3 px-4 py-3', (!svc.IsActive || !svc.IsDisplay) && 'bg-slate-50/50')}>
      {/* Icon + upload */}
      <button
        type="button"
        title="Upload icon"
        onClick={() => fileRef.current?.click()}
        className="group relative grid size-11 shrink-0 place-items-center overflow-hidden rounded-xl border border-slate-200 bg-white"
      >
        {icon ? (
          <img src={icon} alt="" className="size-full object-contain p-1" />
        ) : (
          <span className="font-display text-sm font-bold text-brand-600">{svc.Name.slice(0, 2).toUpperCase()}</span>
        )}
        <span className="absolute inset-0 grid place-items-center bg-slate-900/60 opacity-0 transition-opacity group-hover:opacity-100">
          <ImagePlus className="size-4 text-white" />
        </span>
      </button>
      <input
        ref={fileRef}
        type="file"
        accept="image/png,image/jpeg,image/webp,image/gif"
        className="hidden"
        onChange={(e) => { const f = e.target.files?.[0]; if (f) onIcon(f); e.target.value = ''; }}
      />

      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold text-slate-800">{svc.Name}</p>
        <p className="text-xs text-slate-400">
          {svc.OperatorCount} operator{svc.OperatorCount === 1 ? '' : 's'}
          {svc.IsBBPS ? ' · BBPS' : ''}
          {svc.Charge > 0 ? ` · charge ₹${svc.Charge}` : ''}
        </p>
      </div>

      {/* Wallet mapping */}
      <select
        value={svc.WalletID ?? 0}
        disabled={busy}
        onChange={(e) => { const id = Number(e.target.value); if (id > 0) onWallet(id); }}
        className="rounded-lg border border-slate-200 bg-white px-2 py-1.5 text-xs font-semibold text-slate-700 focus:border-brand-400 focus:outline-none"
        title="Which wallet this service debits from"
      >
        <option value={0}>No wallet</option>
        {wallets.map((w) => <option key={w.id} value={w.id}>{w.name}</option>)}
      </select>

      {/* Toggles */}
      <label className="flex items-center gap-1.5 text-xs font-medium text-slate-500">
        Active
        <ToggleDot on={svc.IsActive} busy={busy} onClick={() => onToggle('isactive', !svc.IsActive)} />
      </label>
      <label
        className="flex items-center gap-1.5 text-xs font-medium text-slate-500"
        title="Off = hidden from the retailer portal (service keeps working for API users)"
      >
        {svc.IsDisplay ? <Eye className="size-3.5 text-success-600" /> : <EyeOff className="size-3.5 text-slate-400" />}
        Display
        <ToggleDot on={svc.IsDisplay} busy={busy} onClick={() => onToggle('isdisplay', !svc.IsDisplay)} />
      </label>

      <div className="flex items-center gap-1">
        <button type="button" onClick={onEdit} title="Edit" className="rounded-lg p-2 text-slate-400 transition-colors hover:bg-slate-100 hover:text-brand-600">
          <Pencil className="size-4" />
        </button>
        <button type="button" onClick={onDelete} title="Remove" className="rounded-lg p-2 text-slate-400 transition-colors hover:bg-danger-50 hover:text-danger-600">
          <Trash2 className="size-4" />
        </button>
      </div>
    </li>
  );
}

// -------------------------------------------------------------- form modal ----
function ServiceFormModal({
  title, categories, initial, editingService, editingCategory, onClose, onSaved,
}: {
  title: string;
  categories: string[];
  initial: ServiceForm;
  editingService?: CatalogService;
  editingCategory?: string;
  onClose: () => void;
  onSaved: () => void;
}) {
  const toast = useToast();
  const [form, setForm] = useState<ServiceForm>({ ...initial, ServiceCategory: editingCategory ?? initial.ServiceCategory });
  const [iconFile, setIconFile] = useState<File | null>(null);
  const set = <K extends keyof ServiceForm>(k: K, v: ServiceForm[K]) => setForm((f) => ({ ...f, [k]: v }));

  const saveMut = useMutation({
    mutationFn: async () => {
      if (!form.Name.trim() || !form.ServiceCategory.trim()) throw new Error('Name and category are required.');
      let imagePath = form.ServiceImagePath;
      if (iconFile) imagePath = await uploadImage(iconFile, 'services');

      if (!editingService) {
        const res = await apiPost('Service/Create', {
          Name: form.Name.trim(), ServiceCategory: form.ServiceCategory.trim(), ServiceImagePath: imagePath,
          UISequence: form.UISequence, HSNCode: '', HSNCodeForSelfInvoice: '',
          Charge: form.Charge, MinBalance: form.MinBalance, IsActive: form.IsActive, IsDisplay: form.IsDisplay,
          IsDefault: true, IsOutside: true, IsBBPS: form.IsBBPS, IsSubscriptionAllow: true, IsGSTInclusive: false,
          ReportKind: form.ReportKind,
        });
        if (!res.ok) throw new Error(res.message);
        return;
      }

      // Edit = field-level diff updates.
      const changes: Array<[string, unknown, unknown]> = [];
      if (form.Name.trim() !== editingService.Name) changes.push(['name', editingService.Name, form.Name.trim()]);
      if (form.ServiceCategory.trim() !== (editingCategory ?? '')) changes.push(['servicecategory', editingCategory, form.ServiceCategory.trim()]);
      if (form.UISequence !== editingService.UISequence) changes.push(['uisequence', editingService.UISequence, form.UISequence]);
      if (form.Charge !== editingService.Charge) changes.push(['charge', editingService.Charge, form.Charge]);
      if (form.MinBalance !== editingService.MinBalance) changes.push(['minbalance', editingService.MinBalance, form.MinBalance]);
      if (form.IsBBPS !== !!editingService.IsBBPS) changes.push(['isbbps', editingService.IsBBPS, form.IsBBPS]);
      if (form.IsActive !== editingService.IsActive) changes.push(['isactive', editingService.IsActive, form.IsActive]);
      if (form.IsDisplay !== editingService.IsDisplay) changes.push(['isdisplay', editingService.IsDisplay, form.IsDisplay]);
      if (form.ReportKind !== (editingService.ReportKind ?? '')) changes.push(['reportkind', editingService.ReportKind, form.ReportKind]);
      if (imagePath !== editingService.ServiceImagePath) changes.push(['serviceimagepath', editingService.ServiceImagePath, imagePath]);
      for (const [field, oldV, newV] of changes) {
        const res = await updateServiceField(editingService.ServiceID, field, oldV, newV);
        if (!res.ok) throw new Error(res.message);
      }
    },
    onSuccess: () => { toast.success('Saved.'); onSaved(); },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Modal open onClose={onClose} title={title} size="lg">
      <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); saveMut.mutate(); }}>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <Input label={<>Service name <span className="text-danger-500">*</span></>} value={form.Name} onChange={(e) => set('Name', e.target.value)} placeholder="e.g. Prepaid Recharge" />
          <div>
            <Input
              label={<>Category <span className="text-danger-500">*</span></>}
              value={form.ServiceCategory}
              onChange={(e) => set('ServiceCategory', e.target.value)}
              placeholder="e.g. Mobile Services"
              list="catalog-categories"
            />
            <datalist id="catalog-categories">
              {categories.map((c) => <option key={c} value={c} />)}
            </datalist>
          </div>
          <Input label="Display order" type="number" value={String(form.UISequence)} onChange={(e) => set('UISequence', Number(e.target.value) || 0)} hint="Lower shows first" />
          <div>
            <span className="mb-1.5 block text-sm font-medium text-slate-700">Show in report</span>
            <select
              value={form.ReportKind}
              onChange={(e) => set('ReportKind', e.target.value)}
              className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-brand-400 focus:outline-none"
              title="Which report page this service's transactions appear in"
            >
              {REPORT_KIND_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <Input label="Charge (₹)" type="number" value={String(form.Charge)} onChange={(e) => set('Charge', Number(e.target.value) || 0)} />
          <Input label="Min balance (₹)" type="number" value={String(form.MinBalance)} onChange={(e) => set('MinBalance', Number(e.target.value) || 0)} />
          <div>
            <span className="mb-1.5 block text-sm font-medium text-slate-700">Icon</span>
            <label className="flex cursor-pointer items-center gap-3 rounded-lg border border-dashed border-slate-300 px-3 py-2 text-sm text-slate-500 transition-colors hover:border-brand-400 hover:text-brand-600">
              <ImagePlus className="size-4" />
              {iconFile ? iconFile.name : form.ServiceImagePath ? 'Replace current icon' : 'Choose image (PNG/JPG/WEBP)'}
              <input type="file" accept="image/png,image/jpeg,image/webp,image/gif" className="hidden" onChange={(e) => setIconFile(e.target.files?.[0] ?? null)} />
            </label>
          </div>
        </div>
        <div className="flex flex-wrap gap-5 border-t border-slate-100 pt-4">
          <label className="flex items-center gap-2 text-sm text-slate-600">
            <input type="checkbox" checked={form.IsActive} onChange={(e) => set('IsActive', e.target.checked)} className="size-4 accent-brand-600" />
            Active
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-600">
            <input type="checkbox" checked={form.IsDisplay} onChange={(e) => set('IsDisplay', e.target.checked)} className="size-4 accent-brand-600" />
            Show on retailer portal
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-600">
            <input type="checkbox" checked={form.IsBBPS} onChange={(e) => set('IsBBPS', e.target.checked)} className="size-4 accent-brand-600" />
            BBPS service
          </label>
        </div>
        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
          <Button type="submit" loading={saveMut.isPending}>Save</Button>
        </div>
      </form>
    </Modal>
  );
}
