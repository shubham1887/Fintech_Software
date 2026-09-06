import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, BadgeCheck, Building2, CalendarDays, CheckCircle2, ExternalLink, FileText,
  Hash, IdCard, Mail, MapPin, Network, Pencil, Phone, ShieldCheck, Smartphone, Upload, UserRound, X,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { uploadDocument, imageUrl } from '@/api/catalog.api';
import { useAuth } from '@/features/auth/AuthContext';
import { roleLabel } from '@/lib/roles';
import { getField as get } from '@/lib/row';
import { dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { Skeleton } from '@/components/ui/Skeleton';

type Row = Record<string, unknown>;

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/** Truthy-string parser matching the backend Key Setting (true / 1 / yes / on). */
function parseOn(value: string, isOn: boolean): boolean {
  const v = value.trim().toLowerCase();
  if (v === '') return isOn;
  if (v === 'true' || v === 'false') return v === 'true';
  return v === '1' || v === 'yes' || v === 'on' || v === 'y';
}

/** Format hint for a document's number field, by document code. */
function docNoPlaceholder(code: string): string {
  switch (code.toUpperCase()) {
    case 'PAN': return 'e.g. ABCDE1234F';
    case 'AADHAAR': return 'e.g. 1234 5678 9012';
    case 'GST': return 'e.g. 22ABCDE1234F1Z5';
    default: return 'Document number';
  }
}

/** Where one document sits in the review cycle. */
type DocStage = 'approved' | 'rejected' | 'review' | 'missing';

/** What the user should read on the badge. The stored codes (APPROVE / PROCESS / REJECT) are
 *  internal — showing them raw made the profile look like a database dump. */
const DOC_STAGE_LABEL: Record<DocStage, string> = {
  approved: 'Approved',
  rejected: 'Rejected',
  review: 'In review',
  missing: 'Not submitted',
};

const DOC_STAGE_TONE: Record<DocStage, 'success' | 'danger' | 'warning' | 'neutral'> = {
  approved: 'success',
  rejected: 'danger',
  review: 'warning',
  missing: 'neutral',
};

function docStage(status: string): DocStage {
  const s = status.trim().toUpperCase();
  if (s === 'APPROVE' || s === 'APPROVED') return 'approved';
  if (s === 'REJECT' || s === 'REJECTED') return 'rejected';
  if (s) return 'review';
  return 'missing';
}

/** A label/value line inside a details card. */
function Field({ icon: Icon, label, value }: { icon?: typeof UserRound; label: string; value: ReactNode }) {
  const empty = value === null || value === undefined || value === '' || value === '—';
  return (
    <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-2.5 last:border-0">
      <span className="flex shrink-0 items-center gap-2 text-sm text-slate-500">
        {Icon && <Icon className="size-4 text-slate-400" />}
        {label}
      </span>
      <span className={cn('min-w-0 text-right text-sm font-medium', empty ? 'text-slate-300' : 'text-slate-800')}>
        {empty ? '—' : value}
      </span>
    </div>
  );
}

function SectionCard({ title, subtitle, action, children }: {
  title: string; subtitle?: string; action?: ReactNode; children: ReactNode;
}) {
  return (
    <Card className="p-5">
      <div className="mb-3 flex items-start justify-between gap-3">
        <div>
          <h3 className="text-sm font-semibold text-slate-800">{title}</h3>
          {subtitle && <p className="mt-0.5 text-xs text-slate-500">{subtitle}</p>}
        </div>
        {action}
      </div>
      {children}
    </Card>
  );
}

interface EditState {
  PersonName: string;
  AliasName: string;
  EmailID: string;
  WhatsAppNumber: string;
  Address: string;
  CityName: string;
  Pincode: string;
  StateID: string;
  ShopAddress: string;
}

interface DocDraft { docNo: string; file: File | null }

export default function ProfilePage() {
  const { user, updateUser } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();

  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState<EditState | null>(null);
  const [drafts, setDrafts] = useState<Record<number, DocDraft>>({});
  const [busyDoc, setBusyDoc] = useState<number | null>(null);

  // ---- The caller's own record. SelectProfile takes no id, so every role may call it. ----
  const profileQ = useQuery({
    queryKey: ['my-profile'],
    queryFn: () => apiPost<Row>('Users/SelectProfile', {}),
  });
  const me = profileQ.data?.ok ? profileQ.data.data : undefined;

  // ---- KYC mode: E-Verification (E-KYC) or manual document upload. ----
  const { data: settings = [], isLoading: settingsLoading } = useQuery({
    queryKey: ['crud', 'SettingMaster/GetAll'], staleTime: 300_000,
    queryFn: () => fetchList('SettingMaster/GetAll', { PageNumber: 1, PageSize: 500 }),
  });
  const eVerification = useMemo(() => {
    const row = settings.find((r) => String(get(r, 'Key') ?? '').toLowerCase() === 'everification');
    return row ? parseOn(String(get(row, 'Value') ?? ''), Boolean(get(row, 'IsOn'))) : false;
  }, [settings]);

  /**
   * The admin-configured document list, already merged with what this user has submitted.
   *
   * Loaded whichever verification mode is on. It used to be skipped while E-Verification was
   * enabled, which hid a user's own documents from them the moment the provider switched to
   * E-KYC — including a REJECTED one they were being asked to re-submit. The mode decides which
   * card we lead with, never whether the user can see what they have already sent.
   */
  const { data: docs = [], isLoading: docsLoading } = useQuery({
    queryKey: ['verification-required'],
    queryFn: () => fetchList('Verification/GetRequiredDocuments', {}),
    enabled: !settingsLoading,
  });
  const documents = useMemo(
    () => [...docs]
      .map((r) => ({
        id: Number(get(r, 'VerificationDocumentConfigID') ?? 0),
        name: String(get(r, 'DocumentName') ?? '—'),
        code: String(get(r, 'DocumentCode') ?? '').toUpperCase(),
        type: String(get(r, 'DocumentType') ?? ''),
        required: Boolean(get(r, 'IsRequired')),
        order: Number(get(r, 'DisplayOrder') ?? 0),
        status: String(get(r, 'Status') ?? ''),
        docNo: String(get(r, 'DocumentNo') ?? ''),
        imageUrl: String(get(r, 'ImageURL') ?? ''),
        remark: String(get(r, 'Remark') ?? ''),
      }))
      .filter((d) => d.id > 0)
      .sort((a, b) => a.order - b.order),
    [docs],
  );

  const { data: stateRows = [] } = useQuery({
    queryKey: ['state-ddl'], staleTime: 300_000,
    queryFn: () => fetchList('State/GetDropdown', {}),
    enabled: editing,
  });

  const kycApproved = Boolean(get(me ?? {}, 'IsKYCApproved'));
  const isEkyc = Boolean(get(me ?? {}, 'IseKYC'));
  const submitted = documents.filter((d) => docStage(d.status) !== 'missing').length;
  const approvedCount = documents.filter((d) => docStage(d.status) === 'approved').length;
  const rejected = documents.filter((d) => docStage(d.status) === 'rejected');
  const pendingRequired = documents.filter((d) => d.required && docStage(d.status) === 'missing');
  const inReview = documents.filter((d) => docStage(d.status) === 'review');

  /**
   * One headline for the whole KYC state, used by the hero pill and the KYC card. It leads with
   * the server's own answer (Users/SelectProfile → KYCStatus, the same value the admin's queue
   * shows) so the two sides can never disagree, and only falls back to the documents when an
   * older API build doesn't send it.
   *
   * E-KYC is a *route*, not a state: having E-Verification switched on does not mean this user
   * has been through it. Only IseKYC says that.
   */
  const serverStatus = String(get(me ?? {}, 'KYCStatus') ?? '');
  // Anything already sent to the provider. If documents exist, the user is shown them even in
  // E-KYC mode — they may be the very thing holding the account up.
  const anySubmitted = documents.some((d) => docStage(d.status) !== 'missing');
  const kycState: KycState =
    kycApproved ? 'verified'
      : serverStatus === 'Rejected' || rejected.length > 0 ? 'rejected'
        // Nothing configured to upload — E-KYC is the only route, or there is nothing to do.
        : documents.length === 0 ? (eVerification && !isEkyc ? 'ekycPending' : 'none')
          // Documents ARE configured, so the upload list is always the answer. E-Verification
          // being on only adds a note offering the faster route; it must never replace the
          // upload option, or a user who hasn't done E-KYC is left with no way to get verified.
          : pendingRequired.length > 0 || !anySubmitted ? 'action'
            : inReview.length > 0 ? 'review'
              : 'action';

  // Keep the session in step with the record, so the topbar isn't stuck on login-time values.
  useEffect(() => {
    if (!me || !user) return;
    const fresh = {
      PersonName: String(get(me, 'PersonName') ?? ''),
      FirmName: String(get(me, 'FirmName') ?? ''),
      EmailID: String(get(me, 'EmailID') ?? ''),
      IsKYCApproved: Boolean(get(me, 'IsKYCApproved')),
    };
    if (
      fresh.PersonName !== user.PersonName || fresh.FirmName !== user.FirmName
      || fresh.EmailID !== user.EmailID || fresh.IsKYCApproved !== user.IsKYCApproved
    ) updateUser(fresh);
  }, [me, user, updateUser]);

  const startEdit = () => {
    if (!me) return;
    setForm({
      PersonName: String(get(me, 'PersonName') ?? ''),
      AliasName: String(get(me, 'AliasName') ?? ''),
      EmailID: String(get(me, 'EmailID') ?? ''),
      WhatsAppNumber: String(get(me, 'WhatsAppNumber') ?? ''),
      Address: String(get(me, 'Address') ?? ''),
      CityName: String(get(me, 'CityName') ?? ''),
      Pincode: String(get(me, 'Pincode') ?? ''),
      StateID: String(get(me, 'StateID') ?? ''),
      ShopAddress: String(get(me, 'ShopAddress') ?? ''),
    });
    setEditing(true);
  };
  const setF = <K extends keyof EditState>(k: K, v: EditState[K]) =>
    setForm((f) => (f ? { ...f, [k]: v } : f));

  const saveMut = useMutation({
    mutationFn: async () => {
      if (!form) throw new Error('Nothing to save.');
      if (!form.PersonName.trim()) throw new Error('Person name is required.');
      if (form.EmailID.trim() && !EMAIL_RE.test(form.EmailID.trim())) throw new Error('Enter a valid email address.');
      const res = await apiPost('Users/UpdateProfile', {
        PersonName: form.PersonName.trim(),
        EmailID: form.EmailID.trim(),
        WhatsAppNumber: form.WhatsAppNumber.trim(),
        Address: form.Address.trim(),
        CityName: form.CityName.trim(),
        Pincode: form.Pincode.trim(),
        StateID: form.StateID ? Number(form.StateID) : null,
        ShopAddress: form.ShopAddress.trim(),
      });
      if (!res.ok) throw new Error(res.message || 'Profile update failed.');
    },
    onSuccess: () => {
      toast.success('Profile updated.');
      setEditing(false);
      qc.invalidateQueries({ queryKey: ['my-profile'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const draft = (id: number): DocDraft => drafts[id] ?? { docNo: '', file: null };
  const setDraft = (id: number, patch: Partial<DocDraft>) =>
    setDrafts((s) => ({ ...s, [id]: { ...draft(id), ...patch } }));

  const submitDocMut = useMutation({
    mutationFn: async (doc: KycDoc) => {
      const d = draft(doc.id);
      const docNo = (d.docNo || doc.docNo).trim();
      if (!docNo) throw new Error(`Enter the ${doc.name} number.`);
      if (!d.file && !doc.imageUrl) throw new Error(`Attach a copy of your ${doc.name}.`);
      setBusyDoc(doc.id);
      const path = d.file ? await uploadDocument(d.file) : doc.imageUrl;
      const res = await apiPost('Verification/SubmitDocument', {
        VerificationDocumentConfigID: doc.id,
        DocumentNo: docNo,
        ImageURL: path,
      });
      if (!res.ok) throw new Error(res.message || 'Submission failed.');
    },
    onSuccess: () => {
      toast.success('Document submitted for verification.');
      setDrafts({});
      qc.invalidateQueries({ queryKey: ['verification-required'] });
      qc.invalidateQueries({ queryKey: ['my-profile'] });
    },
    onError: (e: Error) => toast.error(e.message),
    onSettled: () => setBusyDoc(null),
  });

  if (!user) return null;

  const name = String(get(me ?? {}, 'PersonName') || user.PersonName || user.FirmName || 'User');
  const firm = String(get(me ?? {}, 'FirmName') || user.FirmName || '');
  const initials = name.trim().slice(0, 1).toUpperCase() || 'U';
  const roleName = String(get(me ?? {}, 'UserType') || '') || roleLabel(user.UserTypeID, user.UserType);
  const stateName = String(get(me ?? {}, 'StateName') ?? '');
  const joined = get(me ?? {}, 'DOC');

  return (
    <>
      <PageHeader
        title="My profile"
        subtitle="Your account, contact details and verification status"
        icon={UserRound}
        actions={!editing && me ? (
          <Button variant="outline" onClick={startEdit}>
            <Pencil className="size-4" /> Edit details
          </Button>
        ) : undefined}
      />

      {profileQ.isLoading ? (
        <div className="space-y-4">
          <Skeleton className="h-28 rounded-card" />
          <div className="grid gap-4 lg:grid-cols-3">
            <Skeleton className="h-64 rounded-card lg:col-span-2" />
            <Skeleton className="h-64 rounded-card" />
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          {/* ---- Identity strip -------------------------------------------------- */}
          <Card className="p-5">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
              <div className="grid size-14 shrink-0 place-items-center rounded-2xl bg-brand-600 font-display text-2xl font-extrabold text-white">
                {initials}
              </div>
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-2">
                  <h2 className="font-display text-lg font-bold text-slate-900">{name}</h2>
                  <Badge tone="brand">{roleName}</Badge>
                </div>
                {firm && <p className="mt-0.5 truncate text-sm text-slate-500">{firm}</p>}
              </div>
              <KycPill state={kycState} ekyc={isEkyc} />
            </div>
          </Card>

          {/* ---- KYC — the whole verification flow now lives here ----------------- */}
          <KycSection
            state={kycState}
            ekyc={isEkyc}
            offerEkyc={eVerification && !isEkyc}
            loading={settingsLoading || docsLoading}
            documents={documents}
            submitted={submitted}
            approvedCount={approvedCount}
            draft={draft}
            setDraft={setDraft}
            busyDoc={busyDoc}
            pending={submitDocMut.isPending}
            onSubmit={(d) => submitDocMut.mutate(d)}
          />

          <div className="grid gap-4 lg:grid-cols-3">
            {/* ---- Contact & address (editable) --------------------------------- */}
            <div className="lg:col-span-2">
              {editing && form ? (
                <SectionCard
                  title="Edit your details"
                  subtitle="Firm name, mobile number and statutory IDs are held against your KYC — your service provider changes those."
                  action={
                    <Button variant="outline" size="sm" onClick={() => setEditing(false)}>
                      <X className="size-4" /> Cancel
                    </Button>
                  }
                >
                  <div className="grid grid-cols-1 gap-x-5 gap-y-4 sm:grid-cols-2">
                    <Input label="Person name *" value={form.PersonName} onChange={(e) => setF('PersonName', e.target.value)} />
                    <Input
                      label="Alias name (user code)"
                      value={form.AliasName || '—'}
                      readOnly
                      className="bg-slate-50 font-mono text-slate-600"
                      hint="System-generated from your user type — not editable."
                    />
                    <Input label="Email" type="email" value={form.EmailID} onChange={(e) => setF('EmailID', e.target.value)} />
                    <Input
                      label="WhatsApp number"
                      value={form.WhatsAppNumber}
                      inputMode="numeric"
                      placeholder="Defaults to your mobile"
                      onChange={(e) => setF('WhatsAppNumber', e.target.value.replace(/\D/g, '').slice(0, 10))}
                    />
                    <Autocomplete
                      label="State"
                      placeholder="Search a state…"
                      value={form.StateID}
                      options={stateRows.map((r) => ({ value: String(get(r, 'StateID') ?? ''), label: String(get(r, 'StateName') ?? '') }))}
                      onChange={(v) => setF('StateID', String(v))}
                    />
                    <Input label="City" value={form.CityName} onChange={(e) => setF('CityName', e.target.value)} />
                    <Input
                      label="Pincode"
                      value={form.Pincode}
                      inputMode="numeric"
                      onChange={(e) => setF('Pincode', e.target.value.replace(/\D/g, '').slice(0, 6))}
                    />
                    <Input label="Address" value={form.Address} onChange={(e) => setF('Address', e.target.value)} />
                    <Input label="Shop address" value={form.ShopAddress} onChange={(e) => setF('ShopAddress', e.target.value)} />
                  </div>
                  <div className="mt-5 flex justify-end gap-2 border-t border-slate-100 pt-4">
                    <Button variant="outline" onClick={() => setEditing(false)}>Cancel</Button>
                    <Button loading={saveMut.isPending} onClick={() => saveMut.mutate()}>Save changes</Button>
                  </div>
                </SectionCard>
              ) : (
                <SectionCard title="Contact & address">
                  <Field icon={UserRound} label="Person name" value={String(get(me ?? {}, 'PersonName') ?? '')} />
                  <Field icon={Building2} label="Firm" value={firm} />
                  <Field icon={IdCard} label="Alias" value={String(get(me ?? {}, 'AliasName') ?? '')} />
                  <Field icon={Phone} label="Mobile" value={String(get(me ?? {}, 'MobileNumber') || user.MobileNumber)} />
                  <Field icon={Smartphone} label="WhatsApp" value={String(get(me ?? {}, 'WhatsAppNumber') ?? '')} />
                  <Field icon={Mail} label="Email" value={String(get(me ?? {}, 'EmailID') || user.EmailID)} />
                  <Field
                    icon={MapPin}
                    label="Address"
                    value={[String(get(me ?? {}, 'Address') ?? ''), String(get(me ?? {}, 'CityName') ?? ''), stateName, String(get(me ?? {}, 'Pincode') ?? '')]
                      .filter(Boolean).join(', ')}
                  />
                  <Field icon={Building2} label="Shop address" value={String(get(me ?? {}, 'ShopAddress') ?? '')} />
                </SectionCard>
              )}
            </div>

            {/* ---- Account facts (read-only) ------------------------------------ */}
            <div className="space-y-4">
              <SectionCard title="Account">
                <Field icon={ShieldCheck} label="Role" value={roleName} />
                <Field icon={Network} label="Reports to" value={String(get(me ?? {}, 'ParentName') ?? '')} />
                <Field icon={Hash} label="Reference no" value={String(get(me ?? {}, 'UniqueReferenceNo') ?? '')} />
                <Field icon={CalendarDays} label="Member since" value={joined ? dateOnly(String(joined)) : ''} />
              </SectionCard>

              <SectionCard title="Statutory IDs" subtitle="Held against your KYC — contact your service provider to change these.">
                <Field label="GST" value={String(get(me ?? {}, 'GSTNo') ?? '')} />
                <Field label="PAN" value={String(get(me ?? {}, 'PANNo') ?? '')} />
                <Field label="Aadhaar" value={String(get(me ?? {}, 'AadharNo') ?? '')} />
              </SectionCard>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

/**
 * Where this account's KYC stands. `verified` / `rejected` / `review` / `action` mirror the
 * admin's Approved / Rejected / Pending, so the user sees the same verdict the operator does.
 */
type KycState = 'verified' | 'rejected' | 'review' | 'action' | 'ekycPending' | 'none';

/** The single headline status shown next to the user's name. */
function KycPill({ state, ekyc }: { state: KycState; ekyc?: boolean }) {
  const look = {
    verified: { icon: BadgeCheck, text: ekyc ? 'KYC done · E-KYC' : 'KYC approved', cls: 'bg-success-50 text-success-600 ring-success-500/20' },
    rejected: { icon: X, text: 'KYC rejected', cls: 'bg-danger-50 text-danger-600 ring-danger-500/20' },
    review: { icon: ShieldCheck, text: 'KYC under review', cls: 'bg-warning-50 text-warning-600 ring-warning-500/20' },
    action: { icon: AlertTriangle, text: 'KYC pending', cls: 'bg-danger-50 text-danger-600 ring-danger-500/20' },
    ekycPending: { icon: AlertTriangle, text: 'E-KYC pending', cls: 'bg-warning-50 text-warning-600 ring-warning-500/20' },
    none: { icon: ShieldCheck, text: 'KYC not required', cls: 'bg-slate-100 text-slate-500 ring-slate-200' },
  }[state];
  const Icon = look.icon;
  return (
    <span className={cn('inline-flex shrink-0 items-center gap-1.5 rounded-full px-3 py-1.5 text-sm font-medium ring-1 ring-inset', look.cls)}>
      <Icon className="size-4" /> {look.text}
    </span>
  );
}

interface KycDoc {
  id: number; name: string; code: string; type: string; required: boolean; order: number;
  status: string; docNo: string; imageUrl: string; remark: string;
}

/**
 * Verification, in place of the old standalone Manual Verification page. It only asks for
 * anything while something is actually outstanding — once the account is approved it collapses
 * to a plain "KYC done" confirmation.
 */
function KycSection({ state, ekyc, offerEkyc, loading, documents, submitted, approvedCount, draft, setDraft, busyDoc, pending, onSubmit }: {
  state: KycState;
  /** The user cleared the electronic flow — their approval was automatic. */
  ekyc?: boolean;
  /** E-Verification is on and this user hasn't been through it: offer that route alongside the
   *  upload list, never instead of it. */
  offerEkyc?: boolean;
  loading: boolean;
  documents: KycDoc[];
  submitted: number;
  approvedCount: number;
  draft: (id: number) => DocDraft;
  setDraft: (id: number, patch: Partial<DocDraft>) => void;
  busyDoc: number | null;
  pending: boolean;
  onSubmit: (doc: KycDoc) => void;
}) {
  if (loading) return <Skeleton className="h-32 rounded-card" />;

  if (state === 'verified') {
    return (
      <Card className="p-5">
        <div className="flex items-start gap-3">
          <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-success-50 text-success-600">
            <BadgeCheck className="size-5" />
          </span>
          <div className="min-w-0 flex-1">
            <h3 className="text-sm font-semibold text-slate-800">
              KYC approved{ekyc && <Badge tone="info" className="ml-2 align-middle">E-KYC</Badge>}
            </h3>
            <p className="mt-0.5 text-xs text-slate-500">
              {ekyc
                ? 'You cleared E-KYC, so your account was verified automatically. Nothing is pending from your side.'
                : 'Your service provider has approved your documents. Nothing is pending from your side.'}
            </p>
            {documents.length > 0 && (
              <div className="mt-3 flex flex-wrap gap-x-4 gap-y-1.5">
                {documents.filter((d) => docStage(d.status) === 'approved').map((d) => (
                  <span key={d.id} className="inline-flex items-center gap-1.5 text-xs text-slate-500">
                    <CheckCircle2 className="size-3.5 text-success-600" />
                    {d.name}{d.docNo ? ` · ${d.docNo}` : ''}
                  </span>
                ))}
              </div>
            )}
          </div>
        </div>
      </Card>
    );
  }

  // E-Verification is switched on but this user hasn't been through it yet. The old build showed
  // "verified" here purely because the feature was on, which told the user their KYC was done
  // when it wasn't — and their services stay blocked until it actually is.
  if (state === 'ekycPending') {
    return (
      <Card className="p-5">
        <div className="flex items-start gap-3">
          <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-warning-50 text-warning-600">
            <AlertTriangle className="size-5" />
          </span>
          <div>
            <h3 className="text-sm font-semibold text-slate-800">Complete your E-KYC</h3>
            <p className="mt-0.5 text-xs text-slate-500">
              Your service provider verifies accounts through E-KYC. Your KYC is not approved yet —
              finish the E-KYC flow and your account is verified automatically, with no document
              upload and no waiting for a review.
            </p>
          </div>
        </div>
      </Card>
    );
  }

  if (state === 'none') {
    return (
      <Card className="p-5">
        <div className="flex items-start gap-3">
          <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-slate-100 text-slate-400">
            <ShieldCheck className="size-5" />
          </span>
          <div>
            <h3 className="text-sm font-semibold text-slate-800">Nothing to upload yet</h3>
            <p className="mt-0.5 text-xs text-slate-500">
              Your KYC is still pending, but your service provider has not published the list of
              documents to submit. Contact them to have your account verified.
            </p>
          </div>
        </div>
      </Card>
    );
  }

  const total = documents.length;
  const rejectedDocs = documents.filter((d) => docStage(d.status) === 'rejected');
  // Three shades of "not done yet", each with a different thing for the user to do: something was
  // sent back (fix and re-submit), something is missing (upload it), or it's all with the reviewer.
  const banner = state === 'rejected'
    ? {
      icon: X,
      tone: 'danger' as const,
      title: rejectedDocs.length === 0 ? 'Your KYC was rejected'
        : rejectedDocs.length === 1 ? 'A document was rejected'
          : `${rejectedDocs.length} documents were rejected`,
      note: rejectedDocs.length === 0
        ? 'Your service provider rejected your verification. Contact them for the reason, or re-submit your documents below.'
        : 'Check the reason on each one below, then upload a corrected copy. Your account stays unverified until they are approved.',
    }
    : state === 'action'
      ? {
        icon: AlertTriangle,
        tone: 'danger' as const,
        title: 'Complete your KYC',
        note: 'Upload the documents below to get your account verified.',
      }
      : {
        icon: ShieldCheck,
        tone: 'warning' as const,
        title: 'KYC under review',
        note: 'Your documents are with your service provider. You will be notified once they are approved.',
      };
  const BannerIcon = banner.icon;
  const danger = banner.tone === 'danger';

  return (
    <Card className="overflow-hidden">
      {/* Banner */}
      <div className={cn('flex flex-wrap items-center justify-between gap-3 border-b px-5 py-4',
        danger ? 'border-danger-500/15 bg-danger-50/50' : 'border-warning-500/15 bg-warning-50/40')}>
        <div className="flex items-start gap-3">
          <span className={cn('grid size-10 shrink-0 place-items-center rounded-xl',
            danger ? 'bg-danger-50 text-danger-600' : 'bg-warning-50 text-warning-600')}>
            <BannerIcon className="size-5" />
          </span>
          <div>
            <h3 className="text-sm font-semibold text-slate-800">{banner.title}</h3>
            <p className="mt-0.5 text-xs text-slate-600">{banner.note}</p>
          </div>
        </div>
        <div className="flex items-center gap-2 text-xs text-slate-500">
          <span className="tabular-nums font-medium text-slate-700">{approvedCount}/{total}</span> approved
          <span className="h-1.5 w-24 overflow-hidden rounded-full bg-slate-200">
            <span
              className={cn('block h-full rounded-full', approvedCount === total ? 'bg-success-500' : 'bg-brand-500')}
              style={{ width: `${total ? Math.round((submitted / total) * 100) : 0}%` }}
            />
          </span>
        </div>
      </div>

      <CardBody className="space-y-3">
        {/* E-Verification is on but this user hasn't cleared it. Offer that route as the quicker
            option — the upload list below stays available either way. */}
        {offerEkyc && (
          <div className="flex items-start gap-2.5 rounded-xl border border-info-500/25 bg-info-50 px-4 py-3 text-xs text-info-700">
            <BadgeCheck className="mt-0.5 size-4 shrink-0" />
            <p>
              <span className="font-medium">E-KYC is available on your account.</span> Completing it
              verifies you instantly, with no upload and no waiting for a review. You can also submit
              the documents below and your service provider will approve them.
            </p>
          </div>
        )}

        {documents.map((d) => {
          const stage = docStage(d.status);
          const s = draft(d.id);
          const locked = stage === 'approved';
          return (
            <div
              key={d.id}
              className={cn('rounded-xl border p-4',
                stage === 'approved' ? 'border-success-500/20 bg-success-50/30'
                  : stage === 'rejected' ? 'border-danger-500/20 bg-danger-50/25'
                    : 'border-slate-200/70')}
            >
              <div className="flex flex-wrap items-center gap-2">
                <span className="text-sm font-semibold text-slate-800">{d.name}</span>
                <Badge tone={d.required ? 'brand' : 'neutral'}>{d.required ? 'Required' : 'Optional'}</Badge>
                <Badge tone={DOC_STAGE_TONE[stage]}>{DOC_STAGE_LABEL[stage]}</Badge>
                {d.type && <span className="text-[11px] text-slate-400">{d.type}</span>}
              </div>

              {d.remark && stage === 'rejected' && (
                <p className="mt-2 text-xs text-danger-600">Reason: {d.remark}</p>
              )}

              {d.imageUrl && (
                <a
                  href={imageUrl(d.imageUrl)}
                  target="_blank"
                  rel="noreferrer"
                  className="mt-2 inline-flex items-center gap-1 text-xs font-medium text-brand-600 hover:underline"
                >
                  <FileText className="size-3.5" /> View submitted file <ExternalLink className="size-3" />
                </a>
              )}

              {locked ? (
                <p className="mt-2 flex items-center gap-1.5 text-xs text-success-600">
                  <CheckCircle2 className="size-3.5" /> Verified{d.docNo ? ` · ${d.docNo}` : ''}
                </p>
              ) : (
                <div className="mt-3 flex flex-col gap-3 lg:flex-row lg:items-end">
                  <div className="lg:w-56">
                    <Input
                      label="Document number"
                      placeholder={docNoPlaceholder(d.code)}
                      value={s.docNo || d.docNo}
                      onChange={(e) => setDraft(d.id, { docNo: e.target.value })}
                    />
                  </div>
                  <div className="min-w-0 lg:w-64">
                    <label className="mb-1.5 block text-sm font-medium text-slate-700">Document file</label>
                    <input
                      type="file"
                      accept="image/png,image/jpeg,image/webp,application/pdf"
                      onChange={(e) => setDraft(d.id, { file: e.target.files?.[0] ?? null })}
                      className="block w-full text-sm text-slate-600 file:mr-3 file:rounded-lg file:border-0 file:bg-brand-50 file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-brand-700 hover:file:bg-brand-100"
                    />
                    {s.file && <p className="mt-1 truncate text-xs text-slate-400">{s.file.name}</p>}
                  </div>
                  <Button
                    size="sm"
                    loading={busyDoc === d.id && pending}
                    onClick={() => onSubmit(d)}
                  >
                    <Upload className="size-4" />
                    {d.status ? 'Re-submit' : 'Submit'}
                  </Button>
                </div>
              )}
            </div>
          );
        })}
        <p className="text-xs text-slate-400">
          Images or PDF, up to 5 MB. Submitted documents go to your service provider for approval.
        </p>
      </CardBody>
    </Card>
  );
}
