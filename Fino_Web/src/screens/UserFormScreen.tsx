import { useEffect, useMemo, useRef, useState, type ComponentType, type FormEvent, type ReactNode } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  ArrowLeft, BadgeCheck, Building2, Check, CheckCircle2, ChevronLeft, ChevronRight,
  FileText, Layers, Lock, MapPin, Network, Phone, Save, Search, ShieldCheck, Upload, UserPlus, X,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { uploadDocument } from '@/api/catalog.api';
import { roleLabel, UserTypeCode } from '@/lib/roles';
import { getField as get } from '@/lib/row';
import { loadSession } from '@/lib/storage';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { Button } from '@/components/ui/Button';
import { Spinner } from '@/components/ui/Spinner';
import type { CustomScreenProps } from './registry';

/** Roles that can be created, when the API's UserType list is unavailable. */
const FALLBACK_ROLES = [
  UserTypeCode.SuperDistributor,
  UserTypeCode.Distributor,
  UserTypeCode.Retailer,
  UserTypeCode.ApiUser,
];

/**
 * Which tiers may be a new user's parent, per role being created:
 *  - Super Distributor → Admin
 *  - Distributor       → Admin, Super Distributor
 *  - Retailer          → Admin, Super Distributor, Distributor
 *  - API User          → Admin
 */
const ALLOWED_PARENTS: Record<number, number[]> = {
  [UserTypeCode.SuperDistributor]: [UserTypeCode.Admin],
  [UserTypeCode.Distributor]: [UserTypeCode.Admin, UserTypeCode.SuperDistributor],
  [UserTypeCode.Retailer]: [UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor],
  [UserTypeCode.ApiUser]: [UserTypeCode.Admin],
};
const parentTierLabel = (tier: number) => (tier === UserTypeCode.Admin ? 'Admin' : roleLabel(tier));

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/** Format hint for a document's number field, by document code. */
const docNoPlaceholder = (code: string): string => {
  switch (code.toUpperCase()) {
    case 'PAN': return 'e.g. ABCDE1234F';
    case 'AADHAAR': return 'e.g. 1234 5678 9012';
    case 'GST': return 'e.g. 22ABCDE1234F1Z5';
    default: return 'Document number';
  }
};

/** Per-service grant state. on = enabled, lock = assigned but disabled, off = not granted. */
type SvcState = 'on' | 'lock' | 'off';

/** One KYC document the admin is attaching for the new user. */
interface DocEntry { docNo: string; file: File | null }

/** Truthy-string parser matching the backend Key Setting (true / 1 / yes / on). */
function parseOn(value: string, isOn: boolean): boolean {
  const v = value.trim().toLowerCase();
  if (v === '') return isOn;
  if (v === 'true' || v === 'false') return v === 'true';
  return v === '1' || v === 'yes' || v === 'on' || v === 'y';
}

interface FormState {
  UserTypeID: number;
  ParentType: number;
  ParentID: string;
  SlabMasterID: string;
  FirmName: string;
  PersonName: string;
  AliasName: string;
  MobileNumber: string;
  WhatsAppNumber: string;
  EmailID: string;
  Password: string;
  StateID: string;
  Address: string;
  CityName: string;
  Pincode: string;
  ShopAddress: string;
  GSTNo: string;
  PANNo: string;
  AadharNo: string;
}

const EMPTY: FormState = {
  UserTypeID: 0,
  ParentType: UserTypeCode.Admin,
  ParentID: '',
  SlabMasterID: '',
  FirmName: '',
  PersonName: '',
  AliasName: '',
  MobileNumber: '',
  WhatsAppNumber: '',
  EmailID: '',
  Password: '',
  StateID: '',
  Address: '',
  CityName: '',
  Pincode: '',
  ShopAddress: '',
  GSTNo: '',
  PANNo: '',
  AadharNo: '',
};

/** Standard responsive field grid used inside data-entry steps. */
function FieldGrid({ children }: { children: ReactNode }) {
  return <div className="grid grid-cols-1 gap-x-5 gap-y-4 sm:grid-cols-2">{children}</div>;
}

/** Professional click / drag-and-drop upload control (image or PDF). */
function FileDrop({ file, accept, onFile }: { file: File | null; accept: string; onFile: (f: File | null) => void }) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [drag, setDrag] = useState(false);
  return (
    <div>
      <label className="mb-1.5 block text-sm font-medium text-slate-700">Document file</label>
      <div
        onClick={() => inputRef.current?.click()}
        onDragOver={(e) => { e.preventDefault(); setDrag(true); }}
        onDragLeave={() => setDrag(false)}
        onDrop={(e) => { e.preventDefault(); setDrag(false); const f = e.dataTransfer.files?.[0]; if (f) onFile(f); }}
        className={cn(
          'flex cursor-pointer items-center gap-3 rounded-lg border border-dashed px-3 py-2.5 transition-colors',
          drag ? 'border-brand-400 bg-brand-50/60' : file ? 'border-slate-300 bg-slate-50/60' : 'border-slate-300 hover:border-brand-300 hover:bg-slate-50',
        )}
      >
        {file ? (
          <>
            <span className="grid size-8 shrink-0 place-items-center rounded bg-brand-50 text-brand-600"><FileText className="size-4" /></span>
            <span className="min-w-0 flex-1 truncate text-sm font-medium text-slate-700">{file.name}</span>
            <button
              type="button"
              onClick={(e) => { e.stopPropagation(); onFile(null); if (inputRef.current) inputRef.current.value = ''; }}
              className="grid size-6 shrink-0 place-items-center rounded text-slate-400 hover:bg-slate-200 hover:text-danger-600"
              aria-label="Remove file"
            >
              <X className="size-4" />
            </button>
          </>
        ) : (
          <>
            <span className="grid size-8 shrink-0 place-items-center rounded bg-slate-100 text-slate-400"><Upload className="size-4" /></span>
            <span className="text-sm text-slate-500">Click or drag a file <span className="text-slate-400">— image / PDF, ≤ 5 MB</span></span>
          </>
        )}
      </div>
      <input ref={inputRef} type="file" accept={accept} className="hidden" onChange={(e) => onFile(e.target.files?.[0] ?? null)} />
    </div>
  );
}

/** Three-way On / Lock / Off segmented control for one service. */
function SvcToggle({ value, onChange }: { value: SvcState; onChange: (v: SvcState) => void }) {
  const opts: { key: SvcState; label: string; on: string }[] = [
    { key: 'on', label: 'On', on: 'bg-success-50 text-success-600' },
    { key: 'lock', label: 'Lock', on: 'bg-warning-50 text-warning-600' },
    { key: 'off', label: 'Off', on: 'bg-slate-100 text-slate-500' },
  ];
  return (
    <div className="flex shrink-0 overflow-hidden rounded-lg border border-slate-200">
      {opts.map((o) => (
        <button
          key={o.key}
          type="button"
          onClick={() => onChange(o.key)}
          className={cn(
            'px-3 py-1 text-xs font-medium transition-colors',
            value === o.key ? o.on : 'bg-white text-slate-400 hover:bg-slate-50',
          )}
        >
          {o.label}
        </button>
      ))}
    </div>
  );
}

/** Header shown at the top of every wizard step body. */
function StepHead({ icon: Icon, title, subtitle }: { icon: ComponentType<{ className?: string }>; title: string; subtitle?: string }) {
  return (
    <div className="mb-5 flex items-center gap-3">
      <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-brand-50 text-brand-600 ring-1 ring-inset ring-brand-500/15">
        <Icon className="size-5" />
      </span>
      <div>
        <h2 className="font-display text-base font-semibold text-slate-900">{title}</h2>
        {subtitle && <p className="mt-0.5 text-xs text-slate-500">{subtitle}</p>}
      </div>
    </div>
  );
}

/** One label/value line in the Review step. */
function ReviewRow({ label, value }: { label: string; value: ReactNode }) {
  const empty = value == null || value === '';
  return (
    <div className="flex items-baseline justify-between gap-4 border-b border-slate-100 py-2 last:border-0">
      <dt className="shrink-0 text-xs text-slate-500">{label}</dt>
      <dd className={cn('min-w-0 truncate text-right text-sm', empty ? 'text-slate-300' : 'font-medium text-slate-800')}>
        {empty ? 'Not provided' : value}
      </dd>
    </div>
  );
}

/** A grouped card in the Review step, with an "Edit" jump back to the relevant step. */
function ReviewCard({ icon: Icon, title, onEdit, children }: { icon: ComponentType<{ className?: string }>; title: string; onEdit?: () => void; children: ReactNode }) {
  return (
    <div className="rounded-xl border border-slate-200/70 bg-white p-4">
      <div className="mb-1 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <Icon className="size-4 text-slate-400" />
          <h3 className="text-sm font-semibold text-slate-800">{title}</h3>
        </div>
        {onEdit && (
          <button type="button" onClick={onEdit} className="text-xs font-medium text-brand-600 hover:text-brand-700">Edit</button>
        )}
      </div>
      <dl>{children}</dl>
    </div>
  );
}

interface WStep {
  id: string;
  title: string;
  short: string;
  icon: ComponentType<{ className?: string }>;
  required: boolean;
  done: boolean;
}

export function UserFormScreen({ def }: CustomScreenProps) {
  const navigate = useNavigate();
  const toast = useToast();
  const isEdit = def.path.toLowerCase().endsWith('/edit');
  const targetId = new URLSearchParams(useLocation().search).get('id') ?? '';
  const currentUserTypeID = Number(loadSession()?.user?.UserTypeID ?? 0);
  const currentUserID = Number(loadSession()?.user?.UserID ?? 0);
  const isAdmin = currentUserTypeID === UserTypeCode.Admin;

  const [form, setForm] = useState<FormState>(EMPTY);
  const [svc, setSvc] = useState<Record<number, SvcState>>({});
  const [svcFilter, setSvcFilter] = useState('');
  const [docEntries, setDocEntries] = useState<Record<number, DocEntry>>({});
  // Inline duplicate-check state for the Contact step (mobile / email already registered).
  const [dupMobile, setDupMobile] = useState(false);
  const [dupEmail, setDupEmail] = useState(false);
  const [checkingMobile, setCheckingMobile] = useState(false);
  const [checkingEmail, setCheckingEmail] = useState(false);
  const set = <K extends keyof FormState>(k: K, v: FormState[K]) => setForm((f) => ({ ...f, [k]: v }));
  const setDoc = (id: number, patch: Partial<DocEntry>) =>
    setDocEntries((s) => {
      const base: DocEntry = s[id] ?? { docNo: '', file: null };
      return { ...s, [id]: { ...base, ...patch } };
    });

  // Role dropdown — from the DB, falling back to the known role codes.
  const { data: roleRows = [] } = useQuery({
    queryKey: ['usertype-ddl'],
    queryFn: () => fetchList('UserType/GetDropdown', {}),
  });
  const roleOptions = useMemo(() => {
    const rows = roleRows.length
      ? roleRows.map((r) => ({ value: Number(get(r, 'UserTypeID')), label: String(get(r, 'UserType') ?? '') }))
      : FALLBACK_ROLES.map((id) => ({ value: id, label: roleLabel(id) }));
    return rows.filter((o) => ALLOWED_PARENTS[o.value]);
  }, [roleRows]);

  // The User Code (Alias Name) is generated from the role, never typed in. As soon as a role is
  // picked we show the code this user will receive; the number itself is only consumed on save,
  // so two admins filling the form at once still end up with different codes.
  const { data: codePreview, isFetching: codeLoading } = useQuery({
    queryKey: ['user-code-preview', form.UserTypeID],
    queryFn: () => apiPost<Record<string, unknown>>('UserCode/Preview', { UserTypeID: form.UserTypeID }),
    enabled: !isEdit && form.UserTypeID > 0,
    staleTime: 0,
  });
  const nextUserCode = codePreview?.ok ? String(get(codePreview.data ?? {}, 'UserCode') ?? '') : '';
  // On edit the code is whatever the user already carries; a record that predates auto-coding
  // gets one the moment it is saved.
  const userCode = isEdit ? form.AliasName : nextUserCode;

  const allowedParents = ALLOWED_PARENTS[form.UserTypeID] ?? [];
  const needsParentUser = form.ParentType !== UserTypeCode.Admin;

  const onRole = (id: number) => {
    const allowed = ALLOWED_PARENTS[id] ?? [UserTypeCode.Admin];
    setForm((f) => ({ ...f, UserTypeID: id, ParentType: allowed[0], ParentID: '' }));
  };

  // State dropdown.
  const { data: stateRows = [] } = useQuery({
    queryKey: ['state-ddl'],
    queryFn: () => fetchList('State/GetDropdown', {}),
  });

  // Upline (parent) users of the selected tier.
  const { data: uplineRows = [] } = useQuery({
    queryKey: ['upline-ddl', form.ParentType],
    queryFn: () => fetchList('Users/SelectForDDLWithUserType', { UserTypeID: form.ParentType }),
    enabled: needsParentUser && Boolean(form.ParentType),
  });

  // Commission slabs (admin-only).
  const { data: slabRows = [] } = useQuery({
    queryKey: ['slab-ddl'],
    queryFn: () => fetchList('Slabs/GetDropdown', {}),
    enabled: isAdmin,
  });
  const slabOptions = useMemo(
    () => slabRows.map((s) => ({ id: Number(get(s, 'SlabMasterID')), name: String(get(s, 'SlabName') ?? '') })),
    [slabRows],
  );

  // Parent's assigned slab — default the new user's slab to it.
  const { data: parentSlabRows = [] } = useQuery({
    queryKey: ['parent-slab', form.ParentID],
    queryFn: () => fetchList('UserSlabMapping/GetByUserID', { UserID: Number(form.ParentID) }),
    enabled: isAdmin && !isEdit && needsParentUser && Boolean(form.ParentID),
  });
  const parentSlab = useMemo(() => {
    const row = parentSlabRows[0];
    return row ? { id: Number(get(row, 'SlabMasterID')), name: String(get(row, 'SlabName') ?? '') } : null;
  }, [parentSlabRows]);
  useEffect(() => {
    if (parentSlab?.id) setForm((f) => ({ ...f, SlabMasterID: String(parentSlab.id) }));
  }, [parentSlab?.id]);

  // Services list — the grid is seeded once both this and (on edit) the user's grants have loaded.
  const { data: serviceRows = [] } = useQuery({
    queryKey: ['svc-list'], staleTime: 300_000,
    queryFn: () => fetchList('Service/GetAll', { IsActive: true, PageNumber: 1, PageSize: 500 }),
  });
  const services = useMemo(
    () => serviceRows
      .map((s) => ({ id: Number(get(s, 'ServiceID')), name: String(get(s, 'ServiceName', 'Name') ?? '') }))
      .filter((s) => s.id > 0),
    [serviceRows],
  );
  const setAllSvc = (v: SvcState) => setSvc(Object.fromEntries(services.map((s) => [s.id, v])));

  // KYC / verification documents — config-driven, admin + Manual mode only.
  const { data: settings = [] } = useQuery({
    queryKey: ['crud', 'SettingMaster/GetAll'], staleTime: 300_000,
    queryFn: () => fetchList('SettingMaster/GetAll', { PageNumber: 1, PageSize: 500 }),
    enabled: isAdmin,
  });
  const eVerification = useMemo(() => {
    const row = settings.find((r) => String(get(r, 'Key') ?? '').toLowerCase() === 'everification');
    return row ? parseOn(String(get(row, 'Value') ?? ''), Boolean(get(row, 'IsOn'))) : false;
  }, [settings]);
  const { data: docConfigs = [] } = useQuery({
    queryKey: ['verification-docs-config'], staleTime: 300_000,
    queryFn: () => fetchList('VerificationDocument/GetAll', {}),
    enabled: isAdmin,
  });
  const activeDocs = useMemo(
    () => docConfigs
      .filter((d) => Boolean(get(d, 'IsActive')))
      .map((d) => ({
        id: Number(get(d, 'VerificationDocumentConfigID') ?? 0),
        name: String(get(d, 'DocumentName') ?? ''),
        code: String(get(d, 'DocumentCode') ?? '').toUpperCase(),
        type: String(get(d, 'DocumentType') ?? ''),
        required: Boolean(get(d, 'IsRequired')),
        order: Number(get(d, 'DisplayOrder') ?? 0),
      }))
      .filter((d) => d.id > 0)
      .sort((a, b) => a.order - b.order),
    [docConfigs],
  );
  const showDocs = isAdmin && !eVerification;
  const showKyc = showDocs || (isAdmin && eVerification);

  // Edit: prefill from the existing record.
  const { data: existing, isLoading: loadingUser } = useQuery({
    queryKey: ['user-by-id', targetId],
    queryFn: () => apiPost<Record<string, unknown>>('Users/SelectByID', { TargetUserID: Number(targetId) }),
    enabled: isEdit && Boolean(targetId),
  });
  /** The user's current grid state per ServiceID. null until the record has loaded. */
  const grants = useMemo(() => {
    if (!isEdit) return null;
    if (!existing?.ok || !existing.data) return null;
    const rows = get(existing.data, 'Services');
    const map = new Map<number, SvcState>();
    if (Array.isArray(rows)) {
      for (const r of rows as Record<string, unknown>[]) {
        const id = Number(get(r, 'ServiceID') ?? 0);
        if (id <= 0) continue;
        // Rows written before IsGranted existed have no such field; those were all grants.
        const granted = get(r, 'IsGranted');
        const isGranted = granted === undefined || granted === null ? true : Boolean(granted);
        map.set(id, !isGranted ? 'off' : Boolean(get(r, 'IsActive')) ? 'on' : 'lock');
      }
    }
    return map;
  }, [isEdit, existing]);

  /** KYC documents already on file, keyed by document code. null until the record has loaded. */
  const savedDocs = useMemo(() => {
    if (!isEdit || !existing?.ok || !existing.data) return null;
    const rows = get(existing.data, 'Documents');
    const map = new Map<string, { docNo: string; imageUrl: string; status: string }>();
    if (Array.isArray(rows)) {
      for (const r of rows as Record<string, unknown>[]) {
        const code = String(get(r, 'DocumentCode') ?? '').toUpperCase();
        if (!code) continue;
        map.set(code, {
          docNo: String(get(r, 'DocumentNo') ?? ''),
          imageUrl: String(get(r, 'ImageURL') ?? ''),
          status: String(get(r, 'Status') ?? ''),
        });
      }
    }
    return map;
  }, [isEdit, existing]);

  useEffect(() => {
    if (services.length === 0) return;
    // Edit waits for the record — its grants decide the initial state of the grid.
    if (isEdit && grants === null) return;
    setSvc((prev) => {
      if (Object.keys(prev).length) return prev;
      const next: Record<number, SvcState> = {};
      // Granted + enabled = On, granted + disabled = Lock, not granted = Off. A user with no grants
      // at all (created before services were assignable) falls back to Add's all-on default, so
      // opening their record doesn't silently revoke everything.
      if (grants && grants.size > 0) {
        services.forEach((s) => { next[s.id] = grants.get(s.id) ?? 'off'; });
      } else {
        services.forEach((s) => { next[s.id] = 'on'; });
      }
      return next;
    });
  }, [services, isEdit, grants]);

  // Prefill the document numbers already on file so the admin edits them rather than re-keying.
  useEffect(() => {
    if (!isEdit || !savedDocs || activeDocs.length === 0) return;
    setDocEntries((prev) => {
      if (Object.keys(prev).length) return prev;
      const next: Record<number, DocEntry> = {};
      activeDocs.forEach((d) => {
        const row = savedDocs.get(d.code);
        if (row) next[d.id] = { docNo: row.docNo, file: null };
      });
      return next;
    });
  }, [isEdit, savedDocs, activeDocs]);

  useEffect(() => {
    if (!isEdit || !existing?.ok || !existing.data) return;
    const d = existing.data;
    // The stored upline tier decides which "maps under" option is preselected; a legacy record
    // pointing at a tier this role can't sit under falls back to the first legal tier.
    const role = Number(get(d, 'UserTypeID') ?? 0);
    const allowed = ALLOWED_PARENTS[role] ?? [UserTypeCode.Admin];
    const storedTier = Number(get(d, 'ParentUserTypeID') ?? 0);
    const parentType = allowed.includes(storedTier) ? storedTier : allowed[0];
    setForm({
      ...EMPTY,
      UserTypeID: role,
      ParentType: parentType,
      // The upline is only picked by id below the admin tier — "maps under Admin" needs no user.
      ParentID: parentType === UserTypeCode.Admin ? '' : String(get(d, 'ParentID') ?? ''),
      SlabMasterID: String(Number(get(d, 'SlabMasterID') ?? 0) || ''),
      FirmName: String(get(d, 'FirmName') ?? ''),
      PersonName: String(get(d, 'PersonName') ?? ''),
      AliasName: String(get(d, 'AliasName') ?? ''),
      MobileNumber: String(get(d, 'MobileNumber') ?? ''),
      WhatsAppNumber: String(get(d, 'WhatsAppNumber') ?? ''),
      EmailID: String(get(d, 'EmailID') ?? ''),
      StateID: String(get(d, 'StateID') ?? ''),
      Address: String(get(d, 'Address') ?? ''),
      CityName: String(get(d, 'CityName') ?? ''),
      Pincode: String(get(d, 'Pincode') ?? ''),
      ShopAddress: String(get(d, 'ShopAddress') ?? ''),
      GSTNo: String(get(d, 'GSTNo') ?? ''),
      PANNo: String(get(d, 'PANNo') ?? ''),
      AadharNo: String(get(d, 'AadharNo') ?? ''),
    });
  }, [isEdit, existing]);

  // Debounced "already registered?" check — fires as the admin types a valid mobile / email,
  // so a duplicate is flagged inline before submit (add mode only; mobile/email are immutable on edit).
  useEffect(() => {
    if (isEdit) return;
    const m = form.MobileNumber.trim();
    if (!/^\d{10}$/.test(m)) { setDupMobile(false); setCheckingMobile(false); return; }
    let cancelled = false;
    setCheckingMobile(true);
    const t = setTimeout(async () => {
      try {
        const res = await apiPost<{ MobileTaken?: boolean }>('Users/CheckAvailability', { MobileNumber: m });
        if (!cancelled) setDupMobile(Boolean(res.ok && res.data?.MobileTaken));
      } catch { if (!cancelled) setDupMobile(false); }
      finally { if (!cancelled) setCheckingMobile(false); }
    }, 400);
    return () => { cancelled = true; clearTimeout(t); };
  }, [form.MobileNumber, isEdit]);

  useEffect(() => {
    const e = form.EmailID.trim();
    if (!e || !EMAIL_RE.test(e)) { setDupEmail(false); setCheckingEmail(false); return; }
    let cancelled = false;
    setCheckingEmail(true);
    const t = setTimeout(async () => {
      try {
        // On edit the user's own address must not read as taken.
        const res = await apiPost<{ EmailTaken?: boolean }>('Users/CheckAvailability',
          { EmailID: e, ExcludeUserID: isEdit ? Number(targetId) : 0 });
        if (!cancelled) setDupEmail(Boolean(res.ok && res.data?.EmailTaken));
      } catch { if (!cancelled) setDupEmail(false); }
      finally { if (!cancelled) setCheckingEmail(false); }
    }, 400);
    return () => { cancelled = true; clearTimeout(t); };
  }, [form.EmailID, isEdit, targetId]);

  /**
   * Service grants in the shape both Insert and Update expect. Every service in the grid is sent,
   * Off ones included: an Off service has to be stored as a real "not granted" row, because a user
   * with no rows at all means "never configured" — which the API treats as unrestricted.
   */
  const buildServices = () => services.map((s) => {
    const state = svc[s.id] ?? 'on';
    return { ServiceID: s.id, IsGranted: state !== 'off', IsActive: state === 'on' };
  });

  /**
   * GST / PAN / Aadhaar live both as plain columns on the user and as KYC documents. When the
   * documents are in play the number typed on the card wins, so the two never drift apart.
   */
  const statutoryNos = () => {
    const docNoByCode = (code: string) => {
      const doc = activeDocs.find((x) => x.code === code);
      return doc ? (docEntries[doc.id]?.docNo ?? '').trim() : '';
    };
    return {
      GSTNo: (showDocs ? docNoByCode('GST') : '') || form.GSTNo.trim(),
      PANNo: (showDocs ? docNoByCode('PAN') : '') || form.PANNo.trim(),
      AadharNo: (showDocs ? docNoByCode('AADHAAR') : '') || form.AadharNo.trim(),
    };
  };

  /** Uploads any newly attached KYC files and returns the rows to persist. */
  const buildDocuments = async () => {
    const out: { VerificationDocumentConfigID: number; DocumentNo: string; ImageURL: string }[] = [];
    if (!showDocs) return out;
    for (const d of activeDocs) {
      const entry = docEntries[d.id];
      if (!entry) continue;
      const saved = savedDocs?.get(d.code);
      const numberChanged = (entry.docNo ?? '').trim() !== (saved?.docNo ?? '');
      if (!entry.file && !numberChanged) continue;
      const path = entry.file ? await uploadDocument(entry.file) : (saved?.imageUrl ?? '');
      out.push({ VerificationDocumentConfigID: d.id, DocumentNo: (entry.docNo ?? '').trim(), ImageURL: path });
    }
    return out;
  };

  const saveMut = useMutation({
    mutationFn: async () => {
      if (isEdit) {
        // "Maps under Admin" points at the admin themselves; below that the picked user is the upline.
        const editParentID = needsParentUser
          ? (form.ParentID ? Number(form.ParentID) : null)
          : (isAdmin && currentUserID > 0 ? currentUserID : null);
        return apiPost('Users/Update', {
          TargetUserID: Number(targetId),
          UserTypeID: form.UserTypeID > 0 ? form.UserTypeID : null,
          ParentID: editParentID,
          SlabMasterID: isAdmin ? (form.SlabMasterID ? Number(form.SlabMasterID) : 0) : null,
          // null = leave the grants alone. Sending [] when the service catalog failed to load
          // would revoke everything the user has, so the grid has to be real before it is saved.
          Services: services.length > 0 ? buildServices() : null,
          Documents: await buildDocuments(),
          FirmName: form.FirmName.trim(),
          PersonName: form.PersonName.trim(),
          EmailID: form.EmailID.trim(),
          WhatsAppNumber: form.WhatsAppNumber.trim(),
          Address: form.Address.trim(),
          CityName: form.CityName.trim(),
          Pincode: form.Pincode.trim(),
          StateID: form.StateID ? Number(form.StateID) : null,
          ShopAddress: form.ShopAddress.trim(),
          ...statutoryNos(),
        });
      }
      const parentId = needsParentUser && form.ParentID ? Number(form.ParentID) : null;
      const servicesPayload = buildServices();
      const documentsPayload = await buildDocuments();
      return apiPost('Users/Insert', {
        UserTypeID: form.UserTypeID,
        ParentID: parentId,
        SlabMasterID: isAdmin && form.SlabMasterID ? Number(form.SlabMasterID) : null,
        Services: servicesPayload,
        Documents: documentsPayload,
        FirmName: form.FirmName.trim(),
        PersonName: form.PersonName.trim(),
        MobileNumber: form.MobileNumber.trim(),
        WhatsAppNumber: form.WhatsAppNumber.trim(),
        EmailID: form.EmailID.trim(),
        Password: form.Password,
        StateID: form.StateID ? Number(form.StateID) : null,
        Address: form.Address.trim(),
        CityName: form.CityName.trim(),
        Pincode: form.Pincode.trim(),
        ShopAddress: form.ShopAddress.trim(),
        ...statutoryNos(),
      });
    },
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(isEdit ? 'User updated.' : 'User created.');
        navigate('/s/Users');
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ---- Derived completion state -------------------------------------------------
  const roleDone = form.UserTypeID > 0 && (!needsParentUser || Boolean(form.ParentID));
  const businessDone = Boolean(form.FirmName.trim() && form.PersonName.trim());
  const contactDone = !dupEmail && (isEdit || (/^\d{10}$/.test(form.MobileNumber.trim()) && !dupMobile));
  const onCount = services.filter((s) => (svc[s.id] ?? 'on') === 'on').length;
  const lockCount = services.filter((s) => svc[s.id] === 'lock').length;
  const offCount = services.length - onCount - lockCount;

  // ---- Wizard step model --------------------------------------------------------
  const steps = useMemo<WStep[]>(() => ([
    { id: 'account', title: 'Account & role', short: 'Account', icon: Network, required: true, done: roleDone },
    { id: 'business', title: 'Business details', short: 'Business', icon: Building2, required: true, done: businessDone },
    { id: 'contact', title: 'Contact & login', short: 'Contact', icon: Phone, required: !isEdit, done: contactDone },
    { id: 'location', title: 'Address', short: 'Location', icon: MapPin, required: false, done: true },
    { id: 'compliance', title: 'Compliance & documents', short: 'Compliance', icon: ShieldCheck, required: false, done: true },
    { id: 'services', title: 'Services & access', short: 'Services', icon: Lock, required: false, done: true },
    { id: 'review', title: isEdit ? 'Review & save' : 'Review & create', short: 'Review', icon: BadgeCheck, required: false, done: true },
  ].filter(Boolean) as WStep[]), [isEdit, roleDone, businessDone, contactDone]);

  const [rawStep, setRawStep] = useState(0);
  const step = Math.min(rawStep, steps.length - 1);
  const current = steps[step];
  const isReview = current?.id === 'review';
  const requiredSteps = steps.filter((s) => s.required);
  const doneCount = requiredSteps.filter((s) => s.done).length;

  const indexOf = (id: string) => steps.findIndex((s) => s.id === id);

  /** Validation for one step id; returns an error message or null. */
  function stepError(id: string): string | null {
    if (id === 'account') {
      if (form.UserTypeID <= 0) return 'Please select a role.';
      if (needsParentUser && !form.ParentID) return `Please select the ${parentTierLabel(form.ParentType)} this user maps under.`;
    }
    if (id === 'business') {
      if (!form.FirmName.trim()) return 'Firm name is required.';
      if (!form.PersonName.trim()) return 'Person name is required.';
    }
    if (id === 'contact') {
      if (!isEdit && !/^\d{10}$/.test(form.MobileNumber.trim())) return 'Enter a valid 10-digit mobile number.';
      if (!isEdit && dupMobile) return 'This mobile number is already registered.';
      if (form.EmailID.trim() && !EMAIL_RE.test(form.EmailID.trim())) return 'Enter a valid email address.';
      if (dupEmail) return 'This email address is already registered.';
    }
    return null;
  }

  const goNext = () => {
    const err = stepError(current.id);
    if (err) return toast.error(err);
    setRawStep((s) => Math.min(s + 1, steps.length - 1));
  };
  const goBack = () => setRawStep((s) => Math.max(s - 1, 0));
  // A step is reachable if it's the current/earlier one, or every required step before it is done.
  // Prevents jumping ahead via the stepper while required steps are still empty.
  const canGoTo = (i: number) => i <= step || steps.slice(0, i).every((s) => !s.required || s.done);
  const goTo = (i: number) => { if (canGoTo(i)) setRawStep(i); };

  function submit(e: FormEvent) {
    e.preventDefault();
    // Guard: only the Review step's primary button submits.
    if (!isReview) return;
    for (const s of steps) {
      const err = stepError(s.id);
      if (err) { setRawStep(indexOf(s.id)); return toast.error(err); }
    }
    saveMut.mutate();
  }

  // Names for the Review step.
  const roleName = form.UserTypeID > 0 ? roleLabel(form.UserTypeID) : '';
  const parentName = useMemo(() => {
    if (!needsParentUser) return 'Admin (you)';
    const row = uplineRows.find((r) => String(get(r, 'UserMasterID') ?? '') === form.ParentID);
    return row ? String(get(row, 'FirmName') ?? '') : '';
  }, [uplineRows, form.ParentID, needsParentUser]);
  const stateName = useMemo(() => {
    const row = stateRows.find((r) => String(get(r, 'StateID') ?? '') === form.StateID);
    return row ? String(get(row, 'StateName') ?? '') : '';
  }, [stateRows, form.StateID]);
  const slabName = useMemo(() => {
    const row = slabOptions.find((s) => String(s.id) === form.SlabMasterID);
    return row?.name ?? '';
  }, [slabOptions, form.SlabMasterID]);

  return (
    <>
      <PageHeader
        icon={isEdit ? BadgeCheck : UserPlus}
        title={isEdit ? 'Edit User' : 'Add User'}
        subtitle={isEdit ? 'Update user details' : 'Create a new user in your network'}
        actions={
          <Button variant="outline" onClick={() => navigate('/s/Users')}>
            <ArrowLeft className="size-4" />
            Back to users
          </Button>
        }
      />

      {isEdit && loadingUser ? (
        <div className="flex min-h-[40vh] items-center justify-center">
          <Spinner className="size-6 text-brand-500" />
        </div>
      ) : (
        <div>
          {/* ---- Stepper header ------------------------------------------------ */}
          <div className="overflow-hidden rounded-t-card border border-slate-200/70 bg-white shadow-card">
            {/* Mobile: compact "Step x of n" */}
            <div className="flex items-center justify-between gap-3 border-b border-slate-100 px-5 py-3 sm:hidden">
              <span className="text-sm font-semibold text-slate-800">{current?.title}</span>
              <span className="text-xs tabular-nums text-slate-400">Step {step + 1} / {steps.length}</span>
            </div>
            {/* sm+: full horizontal numbered stepper */}
            <ol className="hidden items-center gap-1 px-5 py-4 sm:flex">
              {steps.map((s, i) => {
                const active = i === step;
                const complete = i < step || (s.required ? s.done : i < step);
                const reachable = canGoTo(i);
                return (
                  <li key={s.id} className={cn('flex items-center', i < steps.length - 1 && 'flex-1')}>
                    <button
                      type="button"
                      onClick={() => goTo(i)}
                      disabled={!reachable}
                      className={cn('group flex items-center gap-2.5', !reachable && 'cursor-not-allowed')}
                      title={reachable ? s.title : 'Complete the earlier required steps first'}
                    >
                      <span
                        className={cn(
                          'grid size-8 shrink-0 place-items-center rounded-full text-xs font-semibold tabular-nums ring-1 ring-inset transition-colors',
                          active
                            ? 'bg-brand-500 text-white ring-brand-500'
                            : complete
                              ? 'bg-success-50 text-success-600 ring-success-500/25'
                              : 'bg-white text-slate-400 ring-slate-200 group-hover:ring-slate-300',
                        )}
                      >
                        {complete && !active ? <Check className="size-4" /> : i + 1}
                      </span>
                      <span className={cn('hidden whitespace-nowrap text-sm lg:block', active ? 'font-semibold text-slate-800' : 'text-slate-500 group-hover:text-slate-700')}>
                        {s.short}
                      </span>
                    </button>
                    {i < steps.length - 1 && <span className="mx-2 hidden h-px flex-1 bg-slate-200 sm:block" aria-hidden />}
                  </li>
                );
              })}
            </ol>
          </div>

          {/* ---- Step body + footer (one continuous card) --------------------- */}
          <form
            onSubmit={submit}
            onKeyDown={(e) => {
              // Prevent an accidental Enter from submitting before the Review step.
              if (e.key === 'Enter' && !isReview) {
                const t = e.target as HTMLElement;
                if (t.tagName !== 'TEXTAREA') e.preventDefault();
              }
            }}
            className="rounded-b-card border border-t-0 border-slate-200/70 bg-white shadow-card"
          >
            <div className="min-h-[13rem] p-5 sm:px-6 sm:py-6">
              {/* ===== Account & role ===== */}
              {current?.id === 'account' && (
                <div>
                  <StepHead icon={Network} title="Account & role" subtitle="Where this user sits in your network" />
                  <FieldGrid>
                    <Select label="Role *" value={form.UserTypeID} onChange={(e) => onRole(Number(e.target.value))}>
                      <option value={0}>Select role</option>
                      {roleOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                    </Select>
                    {form.UserTypeID > 0 && (
                      <Select label="Maps under *" value={form.ParentType} onChange={(e) => setForm((f) => ({ ...f, ParentType: Number(e.target.value), ParentID: '' }))}>
                        {allowedParents.map((tier) => <option key={tier} value={tier}>{parentTierLabel(tier)}</option>)}
                      </Select>
                    )}
                    {needsParentUser && (
                      <Autocomplete
                        label={`${parentTierLabel(form.ParentType)} *`}
                        placeholder={`Search ${parentTierLabel(form.ParentType).toLowerCase()}…`}
                        value={form.ParentID}
                        options={uplineRows.map((r) => ({
                          value: String(get(r, 'UserMasterID') ?? ''),
                          label: String(get(r, 'FirmName') ?? '—'),
                          sublabel: String(get(r, 'MobileNumber') ?? ''),
                        }))}
                        onChange={(v) => set('ParentID', String(v))}
                      />
                    )}
                  </FieldGrid>
                  {form.UserTypeID <= 0 && (
                    <p className="mt-4 text-xs text-slate-400">Pick the role first — the tiers this user can map under depend on it.</p>
                  )}
                  {isEdit && (
                    <p className="mt-4 text-xs text-slate-400">
                      The role can only be changed while no users are mapped under this user. Moving the upline re-parents
                      this user and their whole downline.
                    </p>
                  )}
                </div>
              )}

              {/* ===== Business details ===== */}
              {current?.id === 'business' && (
                <div>
                  <StepHead icon={Building2} title="Business details" subtitle="The firm and the person who owns it" />
                  <FieldGrid>
                    <Input label="Firm name *" value={form.FirmName} onChange={(e) => set('FirmName', e.target.value)} />
                    <Input label="Person name *" value={form.PersonName} onChange={(e) => set('PersonName', e.target.value)} />
                    <Input
                      label="Alias name (user code)"
                      value={userCode || (codeLoading ? 'Generating…' : 'Assigned on save')}
                      readOnly
                      className="bg-slate-50 font-mono text-slate-600"
                      hint={isEdit
                        ? 'Generated from the user type — not editable.'
                        : 'Generated automatically from the role. Configure the format under Users → User Alias.'}
                    />
                  </FieldGrid>
                  {isAdmin && (
                    <div className="mt-6 border-t border-slate-100 pt-5">
                      <div className="mb-4 flex items-center gap-2">
                        <Layers className="size-4 text-slate-400" />
                        <h3 className="text-sm font-semibold text-slate-800">Commission slab</h3>
                      </div>
                      <FieldGrid>
                        <Select label="Assign slab" value={form.SlabMasterID} onChange={(e) => set('SlabMasterID', e.target.value)}>
                          <option value="">— No slab —</option>
                          {slabOptions.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
                        </Select>
                      </FieldGrid>
                      {parentSlab?.id && (
                        <p className="mt-3 flex items-center gap-1.5 text-xs text-slate-500">
                          <ShieldCheck className="size-3.5 text-success-600" />
                          Parent’s slab <span className="font-medium text-slate-700">{parentSlab.name}</span> applied by default — change it above if needed.
                        </p>
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* ===== Contact & login ===== */}
              {current?.id === 'contact' && (
                <div>
                  <StepHead icon={Phone} title="Contact & login" subtitle="How the user is reached and signs in" />
                  <FieldGrid>
                    <Input
                      label="Mobile number *"
                      value={form.MobileNumber}
                      disabled={isEdit}
                      inputMode="numeric"
                      onChange={(e) => set('MobileNumber', e.target.value.replace(/\D/g, '').slice(0, 10))}
                      error={!isEdit && dupMobile ? 'This mobile number is already registered.' : undefined}
                      hint={isEdit ? 'Mobile number cannot be changed' : checkingMobile ? 'Checking availability…' : 'Used as the login ID'}
                    />
                    <Input
                      label="WhatsApp number"
                      value={form.WhatsAppNumber}
                      inputMode="numeric"
                      placeholder="Defaults to mobile"
                      onChange={(e) => set('WhatsAppNumber', e.target.value.replace(/\D/g, '').slice(0, 10))}
                    />
                    <Input
                      label="Email"
                      type="email"
                      value={form.EmailID}
                      onChange={(e) => set('EmailID', e.target.value)}
                      error={dupEmail ? 'This email address is already registered.' : undefined}
                      hint={checkingEmail ? 'Checking availability…' : undefined}
                    />
                  </FieldGrid>
                  {!isEdit && (
                    <p className="mt-4 flex items-center gap-2 rounded-lg bg-brand-50/60 px-3 py-2.5 text-xs text-brand-700 ring-1 ring-inset ring-brand-500/15">
                      <ShieldCheck className="size-4 shrink-0" />
                      A secure password is generated automatically and sent to the user by SMS — no need to set one here.
                    </p>
                  )}
                </div>
              )}

              {/* ===== Address ===== */}
              {current?.id === 'location' && (
                <div>
                  <StepHead icon={MapPin} title="Address" subtitle="Location & shop details (optional)" />
                  <FieldGrid>
                    <Autocomplete
                      label="State"
                      placeholder="Search a state…"
                      value={form.StateID}
                      options={stateRows.map((r) => ({ value: String(get(r, 'StateID') ?? ''), label: String(get(r, 'StateName') ?? '') }))}
                      onChange={(v) => set('StateID', String(v))}
                    />
                    <Input label="City" value={form.CityName} onChange={(e) => set('CityName', e.target.value)} />
                    <Input label="Pincode" value={form.Pincode} inputMode="numeric" onChange={(e) => set('Pincode', e.target.value.replace(/\D/g, '').slice(0, 6))} />
                    <Input label="Address" value={form.Address} onChange={(e) => set('Address', e.target.value)} />
                    <Input label="Shop address" value={form.ShopAddress} onChange={(e) => set('ShopAddress', e.target.value)} />
                  </FieldGrid>
                </div>
              )}

              {/* ===== Compliance & documents ===== */}
              {current?.id === 'compliance' && (
                <div>
                  <StepHead icon={ShieldCheck} title="Compliance & documents" subtitle="Statutory identifiers and KYC (optional — can be completed later)" />
                  {!showDocs && (
                    <FieldGrid>
                      <Input label="GST number" value={form.GSTNo} onChange={(e) => set('GSTNo', e.target.value.toUpperCase().slice(0, 15))} />
                      <Input label="PAN number" value={form.PANNo} onChange={(e) => set('PANNo', e.target.value.toUpperCase().slice(0, 10))} />
                      <Input label="Aadhaar number" value={form.AadharNo} inputMode="numeric" onChange={(e) => set('AadharNo', e.target.value.replace(/\D/g, '').slice(0, 12))} />
                    </FieldGrid>
                  )}
                  {showKyc && (
                    <div className={cn(!showDocs && 'mt-6 border-t border-slate-100 pt-5')}>
                      {!showDocs ? (
                        <div className="flex items-start gap-3 rounded-lg bg-success-50 p-3 text-sm text-success-600">
                          <BadgeCheck className="mt-0.5 size-4 shrink-0" />
                          <p>E-Verification (E-KYC) is enabled — manual document upload isn’t required here.</p>
                        </div>
                      ) : activeDocs.length === 0 ? (
                        <p className="text-sm text-slate-400">No verification documents are configured. Add them under Settings → Verification Settings.</p>
                      ) : (
                        <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
                          {activeDocs.map((d) => {
                            const entry = docEntries[d.id] ?? { docNo: '', file: null };
                            return (
                              <div key={d.id} className="rounded-lg border border-slate-200/70 bg-slate-50/30 p-4">
                                <div className="mb-3 flex flex-wrap items-center gap-2">
                                  <span className="text-sm font-semibold text-slate-800">{d.name}</span>
                                  <span className={cn('rounded-full px-2 py-0.5 text-[11px] font-medium ring-1 ring-inset',
                                    d.required ? 'bg-brand-50 text-brand-700 ring-brand-500/20' : 'bg-slate-100 text-slate-500 ring-slate-200')}>
                                    {d.required ? 'Required' : 'Optional'}
                                  </span>
                                  {d.type && <span className="text-[11px] text-slate-400">{d.type}</span>}
                                </div>
                                <div className="space-y-3">
                                  <Input label="Document number" placeholder={docNoPlaceholder(d.code)} value={entry.docNo} onChange={(e) => setDoc(d.id, { docNo: e.target.value })} />
                                  <FileDrop file={entry.file} accept="image/png,image/jpeg,image/webp,application/pdf" onFile={(f) => setDoc(d.id, { file: f })} />
                                  {(() => {
                                    const saved = savedDocs?.get(d.code);
                                    if (!saved?.imageUrl || entry.file) return null;
                                    return (
                                      <p className="flex items-center gap-1.5 text-xs text-slate-500">
                                        <FileText className="size-3.5 text-slate-400" />
                                        A file is already on file{saved.status ? ` (${saved.status})` : ''} — upload a new one to replace it.
                                      </p>
                                    );
                                  })()}
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* ===== Services & access ===== */}
              {current?.id === 'services' && (
                <div>
                  <StepHead icon={Lock} title="Services & access" subtitle="Grant, lock, or withhold each service for this user" />
                  {services.length === 0 ? (
                    <p className="text-sm text-slate-400">No services available to assign.</p>
                  ) : (
                    <>
                      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
                        <div className="flex flex-wrap items-center gap-x-4 gap-y-1.5 text-xs text-slate-500">
                          <span className="inline-flex items-center gap-1.5"><span className="size-2 rounded-full bg-success-500" />{onCount} on</span>
                          <span className="inline-flex items-center gap-1.5"><span className="size-2 rounded-full bg-warning-500" />{lockCount} locked</span>
                          <span className="inline-flex items-center gap-1.5"><span className="size-2 rounded-full bg-slate-300" />{offCount} off</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <div className="w-44">
                            <Input leftIcon={<Search className="size-4" />} placeholder="Filter services…" value={svcFilter} onChange={(e) => setSvcFilter(e.target.value)} className="h-9" />
                          </div>
                          <Button type="button" variant="outline" size="sm" onClick={() => setAllSvc('on')}>All on</Button>
                          <Button type="button" variant="outline" size="sm" onClick={() => setAllSvc('off')}>All off</Button>
                        </div>
                      </div>
                      {(() => {
                        const q = svcFilter.trim().toLowerCase();
                        const shown = q ? services.filter((s) => s.name.toLowerCase().includes(q)) : services;
                        if (shown.length === 0) return <p className="py-6 text-center text-sm text-slate-400">No services match “{svcFilter}”.</p>;
                        return (
                          <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
                            {shown.map((s) => {
                              const state = svc[s.id] ?? 'on';
                              return (
                                <div key={s.id} className={cn('flex items-center justify-between gap-3 rounded-lg border px-3 py-2 transition-colors',
                                  state === 'off' ? 'border-slate-200/70 bg-slate-50/40' : 'border-slate-200/70 hover:border-slate-300')}>
                                  <span className={cn('min-w-0 truncate text-sm', state === 'off' ? 'text-slate-400' : 'text-slate-700')}>{s.name}</span>
                                  <SvcToggle value={state} onChange={(v) => setSvc((p) => ({ ...p, [s.id]: v }))} />
                                </div>
                              );
                            })}
                          </div>
                        );
                      })()}
                    </>
                  )}
                </div>
              )}

              {/* ===== Review & create ===== */}
              {current?.id === 'review' && (
                <div>
                  <StepHead
                    icon={BadgeCheck}
                    title={isEdit ? 'Review & save' : 'Review & create'}
                    subtitle={isEdit ? 'Confirm the details below, then save the changes' : 'Confirm the details below, then create the user'}
                  />
                  <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                    <ReviewCard icon={Network} title="Account & role" onEdit={() => goTo(indexOf('account'))}>
                      <ReviewRow label="Role" value={roleName} />
                      <ReviewRow label="Maps under" value={needsParentUser ? `${parentTierLabel(form.ParentType)} · ${parentName}` : 'Admin (you)'} />
                    </ReviewCard>
                    <ReviewCard icon={Building2} title="Business" onEdit={() => goTo(indexOf('business'))}>
                      <ReviewRow label="Firm" value={form.FirmName.trim()} />
                      <ReviewRow label="Person" value={form.PersonName.trim()} />
                      <ReviewRow label="User code" value={userCode || 'Assigned on save'} />
                      {isAdmin && <ReviewRow label="Slab" value={slabName || (form.SlabMasterID ? '' : 'No slab')} />}
                    </ReviewCard>
                    <ReviewCard icon={Phone} title="Contact & login" onEdit={() => goTo(indexOf('contact'))}>
                      <ReviewRow label="Mobile" value={form.MobileNumber.trim()} />
                      <ReviewRow label="WhatsApp" value={form.WhatsAppNumber.trim() || (form.MobileNumber.trim() ? 'Same as mobile' : '')} />
                      <ReviewRow label="Email" value={form.EmailID.trim()} />
                      {!isEdit && <ReviewRow label="Password" value="Auto-generated · sent by SMS" />}
                    </ReviewCard>
                    <ReviewCard icon={MapPin} title="Address" onEdit={() => goTo(indexOf('location'))}>
                      <ReviewRow label="State" value={stateName} />
                      <ReviewRow label="City" value={form.CityName.trim()} />
                      <ReviewRow label="Pincode" value={form.Pincode.trim()} />
                      <ReviewRow label="Address" value={form.Address.trim()} />
                    </ReviewCard>
                    {!showDocs && (
                      <ReviewCard icon={FileText} title="Compliance" onEdit={() => goTo(indexOf('compliance'))}>
                        <ReviewRow label="GST" value={form.GSTNo.trim()} />
                        <ReviewRow label="PAN" value={form.PANNo.trim()} />
                        <ReviewRow label="Aadhaar" value={form.AadharNo.trim()} />
                      </ReviewCard>
                    )}
                    {showDocs && (
                      <ReviewCard icon={ShieldCheck} title="KYC documents" onEdit={() => goTo(indexOf('compliance'))}>
                        {activeDocs.length === 0 ? (
                          <ReviewRow label="Documents" value="None configured" />
                        ) : activeDocs.map((d) => {
                          const entry = docEntries[d.id];
                          const val = entry?.file ? (entry.docNo ? `${entry.docNo} · ${entry.file.name}` : entry.file.name) : (entry?.docNo || '');
                          return <ReviewRow key={d.id} label={d.name} value={val} />;
                        })}
                      </ReviewCard>
                    )}
                    <ReviewCard icon={Lock} title="Services & access" onEdit={() => goTo(indexOf('services'))}>
                      <ReviewRow label="Enabled" value={`${onCount} service${onCount === 1 ? '' : 's'}`} />
                      <ReviewRow label="Locked" value={lockCount ? `${lockCount} service${lockCount === 1 ? '' : 's'}` : 'None'} />
                      <ReviewRow label="Withheld" value={offCount ? `${offCount} service${offCount === 1 ? '' : 's'}` : 'None'} />
                    </ReviewCard>
                  </div>
                </div>
              )}
            </div>

            {/* ---- Footer nav ---- */}
            <div className="flex items-center justify-between gap-3 border-t border-slate-100 bg-slate-50/50 px-5 py-3.5">
              <div className="hidden items-center gap-2 text-xs text-slate-500 sm:flex">
                {requiredSteps.length && doneCount >= requiredSteps.length ? (
                  <><CheckCircle2 className="size-4 text-success-600" /> All required steps complete</>
                ) : (
                  <><span className="size-1.5 rounded-full bg-amber-400" /> {requiredSteps.length - doneCount} required step{requiredSteps.length - doneCount === 1 ? '' : 's'} left</>
                )}
              </div>
              <div className="flex flex-1 items-center justify-end gap-2 sm:flex-none">
                {step > 0 ? (
                  <Button type="button" variant="outline" onClick={goBack}>
                    <ChevronLeft className="size-4" />
                    Back
                  </Button>
                ) : (
                  <Button type="button" variant="outline" onClick={() => navigate('/s/Users')}>Cancel</Button>
                )}
                {isReview ? (
                  <Button type="submit" loading={saveMut.isPending}>
                    <Save className="size-4" />
                    {isEdit ? 'Save changes' : 'Create user'}
                  </Button>
                ) : (
                  <Button type="button" onClick={goNext}>
                    Continue
                    <ChevronRight className="size-4" />
                  </Button>
                )}
              </div>
            </div>
          </form>
        </div>
      )}
    </>
  );
}
