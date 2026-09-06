import { useEffect, useMemo, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BadgeCheck, Check, CircleSlash, Clock, ExternalLink, Eye, FileText, Info, Lock,
  RefreshCw, Save, ShieldAlert, ShieldCheck, Sparkles, Users, X,
} from 'lucide-react';
import {
  fetchKycPolicy, fetchKycUser, fetchKycUsers, reviewKycDocument, reviewKycUser, saveKycPolicy,
  KYC_STATUS, type KycDocumentRow, type KycPolicyService, type KycUserRow,
} from '@/api/kyc.api';
import { imageUrl } from '@/api/catalog.api';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
import { roleLabel, UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { dateOnly, dateTime } from '@/lib/format';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { Card } from '@/components/ui/Card';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { Modal } from '@/components/ui/Modal';
import { Kpi, KpiStrip } from './userLifecycleShared';

/**
 * KYC Approvals — the admin desk the platform was missing. One screen answers all three
 * questions the operator has:
 *
 *   Queue  — who is waiting, what did they submit, approve or reject it (per document, or the
 *            whole user in one action). E-KYC users show here too, flagged and already approved.
 *   Policy — is KYC enforced at all, and which services keep working while a KYC is pending.
 *
 * Everything here reads and writes the same rows the user's own profile page does, so the two
 * sides never disagree about a document's status.
 */

type Tab = 'queue' | 'policy';

const STATUS_FILTERS = [
  { key: 'All', label: 'All' },
  { key: KYC_STATUS.pending, label: 'Pending' },
  { key: KYC_STATUS.rejected, label: 'Rejected' },
  { key: KYC_STATUS.approved, label: 'Approved' },
  { key: KYC_STATUS.notSubmitted, label: 'Not submitted' },
];

const ROLE_FILTERS: { label: string; type: number }[] = [
  { label: 'Retailer', type: UserTypeCode.Retailer },
  { label: 'Distributor', type: UserTypeCode.Distributor },
  { label: 'Super Distributor', type: UserTypeCode.SuperDistributor },
  { label: 'API User', type: UserTypeCode.ApiUser },
];

/** Document status → badge. Empty status means the user hasn't submitted it. */
function docTone(status: string): { tone: 'success' | 'danger' | 'warning' | 'neutral'; label: string } {
  const s = (status || '').toUpperCase();
  if (s === 'APPROVE') return { tone: 'success', label: 'Approved' };
  if (s === 'REJECT') return { tone: 'danger', label: 'Rejected' };
  if (s === 'PROCESS') return { tone: 'warning', label: 'In review' };
  return { tone: 'neutral', label: 'Not submitted' };
}

function statusTone(status: string): 'success' | 'danger' | 'warning' | 'neutral' {
  if (status === KYC_STATUS.approved) return 'success';
  if (status === KYC_STATUS.rejected) return 'danger';
  if (status === KYC_STATUS.pending) return 'warning';
  return 'neutral';
}

export function KycApprovalsScreen() {
  const [tab, setTab] = useState<Tab>('queue');
  const { user: me } = useAuth();
  // Support can read the queue; only the administrator decides.
  const canDecide = me?.UserTypeID === UserTypeCode.Admin;

  return (
    <>
      <PageHeader
        title="KYC Approvals"
        subtitle="Review what users submitted, approve or reject it, and set what a pending KYC is allowed to do"
        icon={ShieldCheck}
      />

      <div className="mb-5 flex flex-wrap gap-1 border-b border-slate-200">
        {([['queue', 'Review queue'], ['policy', 'KYC policy']] as [Tab, string][]).map(([key, label]) => (
          <button
            key={key}
            type="button"
            onClick={() => setTab(key)}
            className={cn(
              '-mb-px cursor-pointer border-b-2 px-4 py-2 text-sm font-medium transition-colors',
              tab === key ? 'border-brand-600 text-brand-700' : 'border-transparent text-slate-500 hover:text-slate-700',
            )}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === 'queue' ? <ReviewQueue canDecide={canDecide} /> : <PolicyPanel canDecide={canDecide} />}
    </>
  );
}

// ── Review queue ───────────────────────────────────────────────────────────────

function ReviewQueue({ canDecide }: { canDecide: boolean }) {
  const [status, setStatus] = useState<string>(KYC_STATUS.pending);
  const [role, setRole] = useState<number>(0);
  const [openUser, setOpenUser] = useState<number | null>(null);

  const { data, isPending, isFetching, refetch } = useQuery({
    queryKey: ['kyc-users'],
    queryFn: () => fetchKycUsers({}),
  });

  const summary = data?.Summary;
  const all = useMemo(() => data?.Users ?? [], [data]);

  const scoped = useMemo(
    () => all.filter((r) => (status === 'All' || r.Status === status) && (role === 0 || r.UserTypeID === role)),
    [all, status, role],
  );
  const { query, setQuery, filtered } = useClientSearch(scoped as unknown as Record<string, unknown>[]);
  const visible = filtered as unknown as KycUserRow[];

  const statusCount = (key: string) => (key === 'All' ? all.length : all.filter((r) => r.Status === key).length);

  return (
    <>
      <KpiStrip columns={5}>
        <Kpi label="Total users" value={String(summary?.Total ?? 0)} icon={<Users className="size-4" />} />
        <Kpi label="Pending review" value={String(summary?.Pending ?? 0)} tone="text-warning-600" icon={<Clock className="size-4" />} />
        <Kpi label="Rejected" value={String(summary?.Rejected ?? 0)} tone="text-danger-600" icon={<X className="size-4" />} />
        <Kpi label="Approved" value={String(summary?.Approved ?? 0)} tone="text-success-600" icon={<BadgeCheck className="size-4" />} />
        <Kpi label="E-KYC verified" value={String(summary?.EkycVerified ?? 0)} tone="text-info-600" icon={<Sparkles className="size-4" />} />
      </KpiStrip>

      {(summary?.EkycVerified ?? 0) > 0 && (
        <div className="mb-4 flex items-start gap-2.5 rounded-xl border border-info-500/25 bg-info-50 px-4 py-3 text-sm text-info-700">
          <Sparkles className="mt-0.5 size-4 shrink-0" />
          <p>
            <span className="font-medium">{summary?.EkycVerified}</span>{' '}
            {summary?.EkycVerified === 1 ? 'user' : 'users'} verified through E-KYC. Those are approved
            automatically — they are listed here for the record, and there is nothing for you to action.
          </p>
        </div>
      )}

      {/* Status filter */}
      <div className="mb-3 flex flex-wrap gap-1.5">
        {STATUS_FILTERS.map((s) => (
          <button
            key={s.key}
            type="button"
            onClick={() => setStatus(s.key)}
            className={cn(
              'cursor-pointer rounded-full px-3 py-1 text-xs font-medium ring-1 ring-inset transition-colors',
              status === s.key ? 'bg-brand-50 text-brand-700 ring-brand-500/25' : 'text-slate-500 ring-slate-200 hover:bg-slate-50',
            )}
          >
            {s.label} <span className="tabular-nums opacity-70">({statusCount(s.key)})</span>
          </button>
        ))}
      </div>

      {/* Role filter + search + export */}
      <div className="mb-4 flex flex-wrap items-center gap-2">
        <div className="flex flex-wrap gap-1.5">
          <button
            type="button"
            onClick={() => setRole(0)}
            className={cn(
              'cursor-pointer rounded-full px-3 py-1 text-xs font-medium ring-1 ring-inset transition-colors',
              role === 0 ? 'bg-slate-900 text-white ring-slate-900' : 'text-slate-500 ring-slate-200 hover:bg-slate-50',
            )}
          >
            All roles
          </button>
          {ROLE_FILTERS.map((r) => (
            <button
              key={r.type}
              type="button"
              onClick={() => setRole(r.type)}
              className={cn(
                'cursor-pointer rounded-full px-3 py-1 text-xs font-medium ring-1 ring-inset transition-colors',
                role === r.type ? 'bg-slate-900 text-white ring-slate-900' : 'text-slate-500 ring-slate-200 hover:bg-slate-50',
              )}
            >
              {r.label}
            </button>
          ))}
        </div>
        <div className="ml-auto flex items-center gap-2">
          <div className="w-full sm:w-64">
            <Input placeholder="Search name, firm, mobile…" value={query} onChange={(e) => setQuery(e.target.value)} />
          </div>
          <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
          <Button
            variant="outline"
            size="sm"
            disabled={visible.length === 0}
            onClick={() => exportToCsv(visible.map((r) => ({
              Name: r.PersonName,
              Firm: r.FirmName,
              Mobile: r.MobileNumber,
              Role: r.UserType || roleLabel(r.UserTypeID),
              'Mapped under': r.ParentName,
              Mode: r.Mode,
              Status: r.Status,
              Submitted: `${r.ApprovedCount}/${r.SubmittedCount}`,
              Required: r.RequiredCount,
              'Last submitted': dateOnly(r.LastSubmittedOn),
              'Approved on': dateOnly(r.ApprovedOn),
              'Approved by': r.ApprovedBy,
              Remark: r.Remark,
            })), 'KYC approvals')}
          >
            Export
          </Button>
        </div>
      </div>

      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[1020px] border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3">#</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Contact</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Mode</th>
                <th className="px-4 py-3">Documents</th>
                <th className="px-4 py-3">Submitted</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isPending ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={i} className="border-b border-slate-100 last:border-0">
                    <td className="px-4 py-3" colSpan={9}><Skeleton className="h-9 w-full" /></td>
                  </tr>
                ))
              ) : visible.length === 0 ? (
                <tr>
                  <td colSpan={9}>
                    <EmptyState
                      icon={ShieldCheck}
                      title="Nothing to review"
                      description={status === 'All' ? 'No users in this network yet.' : `No user is ${status.toLowerCase()}.`}
                    />
                  </td>
                </tr>
              ) : (
                visible.map((r, i) => (
                  <tr key={r.UserID} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                    <td className="px-3 py-3 tabular-nums text-slate-400">{i + 1}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <span className="grid size-9 shrink-0 place-items-center rounded-full bg-slate-100 text-sm font-semibold text-slate-600 ring-1 ring-inset ring-slate-200">
                          {(r.FirmName || r.PersonName || 'U').trim().slice(0, 1).toUpperCase()}
                        </span>
                        <div className="min-w-0">
                          <p className="truncate font-medium text-slate-800">{r.PersonName || '—'}</p>
                          {r.FirmName && <p className="truncate text-xs text-slate-400">{r.FirmName}</p>}
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <p className="text-slate-700">{r.MobileNumber || '—'}</p>
                      {r.EmailID && <p className="truncate text-xs text-slate-400">{r.EmailID}</p>}
                    </td>
                    <td className="px-4 py-3">
                      <Badge tone="brand">{r.UserType || roleLabel(r.UserTypeID) || '—'}</Badge>
                    </td>
                    <td className="px-4 py-3">
                      {r.IseKYC
                        ? <Badge tone="info"><Sparkles className="size-3" /> E-KYC</Badge>
                        : <span className="text-xs text-slate-500">Manual</span>}
                    </td>
                    <td className="px-4 py-3">
                      <DocProgress row={r} />
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">
                      {r.LastSubmittedOn ? dateOnly(r.LastSubmittedOn) : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="px-4 py-3">
                      <Badge tone={statusTone(r.Status)}>{r.Status}</Badge>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1.5">
                        <Button variant={r.Status === KYC_STATUS.pending ? 'primary' : 'ghost'} size="sm" onClick={() => setOpenUser(r.UserID)}>
                          <Eye className="size-4" />
                          {r.Status === KYC_STATUS.pending ? 'Review' : 'View'}
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>

      <ReviewModal userID={openUser} onClose={() => setOpenUser(null)} canDecide={canDecide} />
    </>
  );
}

/** How far through their documents this user is — the queue's at-a-glance column. */
function DocProgress({ row }: { row: KycUserRow }) {
  if (row.IseKYC) return <span className="text-xs text-info-600">Verified electronically</span>;
  const total = Math.max(row.RequiredCount, row.SubmittedCount);
  if (total === 0) return <span className="text-xs text-slate-400">No documents configured</span>;
  const pct = Math.round((row.ApprovedCount / total) * 100);
  return (
    <div className="min-w-[130px]">
      <div className="flex items-baseline justify-between text-xs">
        <span className="tabular-nums text-slate-600">{row.ApprovedCount}/{total} approved</span>
        {row.RejectedCount > 0 && <span className="text-danger-600">{row.RejectedCount} rejected</span>}
      </div>
      <div className="mt-1 h-1.5 overflow-hidden rounded-full bg-slate-100">
        <div
          className={cn('h-full rounded-full transition-all', row.RejectedCount > 0 ? 'bg-danger-500' : pct === 100 ? 'bg-success-500' : 'bg-warning-500')}
          style={{ width: `${Math.max(pct, row.SubmittedCount > 0 ? 6 : 0)}%` }}
        />
      </div>
    </div>
  );
}

// ── Review modal ───────────────────────────────────────────────────────────────

function ReviewModal({ userID, onClose, canDecide }: { userID: number | null; onClose: () => void; canDecide: boolean }) {
  const qc = useQueryClient();
  const toast = useToast();
  const [remark, setRemark] = useState('');
  const [rejectAll, setRejectAll] = useState(false);
  const [preview, setPreview] = useState<KycDocumentRow | null>(null);

  const { data: detail, isPending } = useQuery({
    queryKey: ['kyc-user', userID],
    queryFn: () => fetchKycUser(userID!),
    enabled: userID != null,
  });

  useEffect(() => { setRemark(''); setRejectAll(false); setPreview(null); }, [userID]);

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ['kyc-users'] });
    qc.invalidateQueries({ queryKey: ['kyc-user', userID] });
    // The user's KYC state shows on the user lists too.
    qc.invalidateQueries({ queryKey: ['users-screen'] });
    qc.invalidateQueries({ queryKey: ['inactive-users'] });
  };

  const docMut = useMutation({
    mutationFn: (v: { id: number; approve: boolean; remark: string }) => reviewKycDocument(v.id, v.approve, v.remark),
    onSuccess: (res, v) => {
      if (!res.ok) { toast.error(res.message); return; }
      toast.success(v.approve ? 'Document approved.' : 'Document rejected.');
      setRemark('');
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const userMut = useMutation({
    mutationFn: (v: { approve: boolean; remark: string }) => reviewKycUser(userID!, v.approve, v.remark),
    onSuccess: (res, v) => {
      if (!res.ok) { toast.error(res.message); return; }
      toast.success(v.approve ? 'KYC approved — the user has been notified.' : 'KYC rejected — the user has been notified.');
      setRejectAll(false);
      setRemark('');
      refresh();
      onClose();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const busy = docMut.isPending || userMut.isPending;
  const submitted = (detail?.Documents ?? []).filter((d) => d.Status);
  const outstanding = (detail?.Documents ?? []).filter((d) => d.IsRequired && !d.Status);

  return (
    <Modal open={userID != null} onClose={onClose} title="KYC review" size="xl">
      {isPending || !detail ? (
        <div className="space-y-3">
          <Skeleton className="h-20 rounded-xl" />
          <Skeleton className="h-40 rounded-xl" />
        </div>
      ) : (
        <div className="space-y-4">
          {/* Applicant */}
          <div className="flex flex-col gap-3 rounded-xl border border-slate-200/80 bg-white p-4 sm:flex-row sm:items-center">
            <span className="grid size-12 shrink-0 place-items-center rounded-2xl bg-brand-600 font-display text-xl font-extrabold text-white">
              {(detail.FirmName || detail.PersonName || 'U').trim().slice(0, 1).toUpperCase()}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate font-display text-lg font-bold text-slate-900">{detail.PersonName || '—'}</p>
              <p className="truncate text-sm text-slate-500">
                {detail.FirmName} · {detail.MobileNumber}
                {detail.ParentName && <> · under {detail.ParentName}</>}
              </p>
            </div>
            <div className="flex flex-wrap items-center gap-1.5">
              <Badge tone="brand">{detail.UserType || roleLabel(detail.UserTypeID) || '—'}</Badge>
              {detail.IseKYC && <Badge tone="info"><Sparkles className="size-3" /> E-KYC</Badge>}
              <Badge tone={statusTone(detail.Status)}>{detail.Status}</Badge>
            </div>
          </div>

          {detail.IseKYC && (
            <div className="flex items-start gap-2.5 rounded-xl border border-info-500/25 bg-info-50 px-4 py-3 text-sm text-info-700">
              <Sparkles className="mt-0.5 size-4 shrink-0" />
              <p>
                This user was verified through E-KYC, so their KYC was approved automatically. Reject it
                only if the electronic verification turns out to be wrong — that reopens their KYC and
                blocks their services again.
              </p>
            </div>
          )}

          <div className="grid gap-4 lg:grid-cols-[1.6fr_1fr]">
            {/* Documents */}
            <div className="space-y-3">
              <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Documents ({submitted.length} submitted of {detail.Documents.length} configured)
              </h4>
              {detail.Documents.length === 0 ? (
                <EmptyState
                  icon={FileText}
                  title="No documents configured"
                  description="Add what users must submit under Settings → Verification Settings."
                />
              ) : (
                detail.Documents.map((doc) => (
                  <DocumentCard
                    key={`${doc.VerificationDocumentConfigID}-${doc.UserDocumentID}`}
                    doc={doc}
                    canDecide={canDecide}
                    busy={busy}
                    onPreview={() => setPreview(doc)}
                    onDecide={(approve, why) => docMut.mutate({ id: doc.UserDocumentID, approve, remark: why })}
                  />
                ))
              )}
            </div>

            {/* Side rail — profile facts + the whole-user decision */}
            <div className="space-y-3">
              <section className="rounded-xl border border-slate-200/80 bg-slate-50/40 p-4">
                <h4 className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-500">On record</h4>
                <div className="grid grid-cols-2 gap-x-4 gap-y-3">
                  <Fact label="PAN" value={detail.PANNo} mono />
                  <Fact label="Aadhaar" value={detail.AadharNo} mono />
                  <Fact label="GST" value={detail.GSTNo} mono />
                  <Fact label="Ref. no." value={detail.UniqueReferenceNo} mono />
                  <Fact label="City" value={detail.CityName} />
                  <Fact label="State" value={detail.StateName} />
                  <Fact label="Registered" value={dateOnly(detail.RegisteredOn)} />
                  <Fact label="Last submitted" value={detail.LastSubmittedOn ? dateOnly(detail.LastSubmittedOn) : ''} />
                </div>
                {detail.Address && (
                  <div className="mt-3">
                    <Fact label="Address" value={detail.Address} />
                  </div>
                )}
              </section>

              {detail.ApprovedOn && (
                <section className="rounded-xl border border-slate-200/80 bg-slate-50/40 p-4 text-sm">
                  <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">Last decision</h4>
                  <p className="text-slate-700">
                    {detail.IsKYCApproved ? 'Approved' : 'Reviewed'} on {dateTime(detail.ApprovedOn)}
                  </p>
                  {detail.ApprovedBy && <p className="text-xs text-slate-500">by {detail.ApprovedBy}</p>}
                  {detail.Remark && <p className="mt-1.5 text-xs text-slate-500">“{detail.Remark}”</p>}
                </section>
              )}

              {detail.EkycHistory.length > 0 && (
                <section className="rounded-xl border border-info-500/20 bg-info-50/50 p-4">
                  <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-info-700">E-KYC history</h4>
                  <ul className="space-y-1.5 text-xs text-slate-600">
                    {detail.EkycHistory.map((e, i) => (
                      <li key={i} className="flex items-baseline justify-between gap-2">
                        <span>{e.DocumentType || 'Verification'} {e.DocumentNo && <span className="font-mono">· {e.DocumentNo}</span>}</span>
                        <span className="shrink-0 text-slate-400">{dateOnly(e.DateTime)}</span>
                      </li>
                    ))}
                  </ul>
                </section>
              )}

              {outstanding.length > 0 && (
                <div className="flex items-start gap-2.5 rounded-xl border border-warning-500/25 bg-warning-50 px-3 py-2.5 text-xs text-warning-800">
                  <Info className="mt-0.5 size-3.5 shrink-0" />
                  <p>
                    Still outstanding: {outstanding.map((d) => d.DocumentName).join(', ')}. Approving the KYC
                    now marks it verified without them.
                  </p>
                </div>
              )}

              {canDecide && (
                <section className="rounded-xl border border-slate-200 bg-white p-4">
                  <h4 className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-500">Decide the whole KYC</h4>
                  <Input
                    label={rejectAll ? 'Reason (required)' : 'Remark (optional)'}
                    placeholder={rejectAll ? 'Why is this being rejected?' : 'Note for the audit trail'}
                    value={remark}
                    onChange={(e) => setRemark(e.target.value)}
                  />
                  <div className="mt-3 flex flex-wrap gap-2">
                    <Button
                      className="flex-1"
                      loading={userMut.isPending && !rejectAll}
                      disabled={busy || detail.Status === KYC_STATUS.approved}
                      onClick={() => { setRejectAll(false); userMut.mutate({ approve: true, remark }); }}
                    >
                      <Check className="size-4" />
                      Approve KYC
                    </Button>
                    <Button
                      variant="danger"
                      className="flex-1"
                      loading={userMut.isPending && rejectAll}
                      disabled={busy}
                      onClick={() => {
                        if (!remark.trim()) { setRejectAll(true); toast.error('Enter a reason before rejecting.'); return; }
                        setRejectAll(true);
                        userMut.mutate({ approve: false, remark });
                      }}
                    >
                      <X className="size-4" />
                      Reject KYC
                    </Button>
                  </div>
                  <p className="mt-2 text-[11px] leading-snug text-slate-400">
                    Approving marks every submitted document approved and unlocks the user's services.
                    Rejecting sends the reason back so they can re-submit.
                  </p>
                </section>
              )}

              {!canDecide && (
                <div className="flex items-start gap-2.5 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-xs text-slate-500">
                  <Lock className="mt-0.5 size-3.5 shrink-0" />
                  <p>Only an administrator can approve or reject a KYC. This view is read-only for you.</p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Document image viewer */}
      <Modal open={Boolean(preview)} onClose={() => setPreview(null)} title={preview?.DocumentName ?? 'Document'} size="lg">
        {preview && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-4">
              <Fact label="Document number" value={preview.DocumentNo} mono />
              <Fact label="Submitted" value={preview.SubmittedOn ? dateTime(preview.SubmittedOn) : ''} />
            </div>
            {preview.ImageURL ? (
              <div className="overflow-hidden rounded-xl border border-slate-200 bg-slate-50">
                <img src={imageUrl(preview.ImageURL)} alt={preview.DocumentName} className="max-h-[65vh] w-full object-contain" />
              </div>
            ) : (
              <EmptyState icon={FileText} title="No file attached" />
            )}
            {preview.ImageURL && (
              <a
                href={imageUrl(preview.ImageURL)}
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-1.5 text-sm font-medium text-brand-600 hover:underline"
              >
                <ExternalLink className="size-4" />
                Open in a new tab
              </a>
            )}
          </div>
        )}
      </Modal>
    </Modal>
  );
}

function Fact({ label, value, mono }: { label: string; value?: string; mono?: boolean }) {
  return (
    <div className="min-w-0">
      <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{label}</div>
      <div className={cn('mt-0.5 break-words text-sm text-slate-800', mono && 'font-mono text-[13px]')}>
        {value?.trim() || <span className="text-slate-300">—</span>}
      </div>
    </div>
  );
}

/** One configured document, with whatever the user submitted for it and the per-document decision. */
function DocumentCard({
  doc, canDecide, busy, onPreview, onDecide,
}: {
  doc: KycDocumentRow;
  canDecide: boolean;
  busy: boolean;
  onPreview: () => void;
  onDecide: (approve: boolean, remark: string) => void;
}) {
  const [reason, setReason] = useState('');
  const [rejecting, setRejecting] = useState(false);
  const { tone, label } = docTone(doc.Status);
  const submitted = Boolean(doc.Status);
  const decidable = canDecide && submitted && doc.UserDocumentID > 0;

  return (
    <div className={cn('rounded-xl border p-4', submitted ? 'border-slate-200 bg-white' : 'border-dashed border-slate-200 bg-slate-50/60')}>
      <div className="flex flex-wrap items-start gap-3">
        {doc.ImageURL ? (
          <button
            type="button"
            onClick={onPreview}
            className="group relative size-16 shrink-0 cursor-pointer overflow-hidden rounded-lg border border-slate-200 bg-slate-50"
            title="View document"
          >
            <img src={imageUrl(doc.ImageURL)} alt="" className="size-full object-cover transition-transform group-hover:scale-105" />
            <span className="absolute inset-0 grid place-items-center bg-ink-950/0 text-white opacity-0 transition group-hover:bg-ink-950/40 group-hover:opacity-100">
              <Eye className="size-4" />
            </span>
          </button>
        ) : (
          <span className="grid size-16 shrink-0 place-items-center rounded-lg border border-dashed border-slate-200 bg-white text-slate-300">
            <FileText className="size-5" />
          </span>
        )}

        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <p className="font-medium text-slate-800">{doc.DocumentName}</p>
            {doc.IsRequired && <Badge tone="neutral">Required</Badge>}
            <Badge tone={tone}>{label}</Badge>
          </div>
          <p className="mt-0.5 text-xs text-slate-500">
            {doc.DocumentNo ? <span className="font-mono">{doc.DocumentNo}</span> : 'No number submitted'}
            {doc.SubmittedOn && <> · {dateOnly(doc.SubmittedOn)}</>}
            {doc.DocumentType && <> · {doc.DocumentType}</>}
          </p>
          {doc.Remark && (
            <p className={cn('mt-1.5 text-xs', tone === 'danger' ? 'text-danger-600' : 'text-slate-500')}>
              “{doc.Remark}”
            </p>
          )}
        </div>

        {decidable && (
          <div className="flex shrink-0 items-center gap-1.5">
            <Button size="sm" variant="outline" disabled={busy} onClick={() => onDecide(true, '')}>
              <Check className="size-4 text-success-600" />
              Approve
            </Button>
            <Button size="sm" variant="ghost" disabled={busy} onClick={() => setRejecting((v) => !v)}>
              <X className="size-4 text-danger-500" />
              Reject
            </Button>
          </div>
        )}
      </div>

      {rejecting && decidable && (
        <div className="mt-3 flex flex-wrap items-end gap-2 border-t border-slate-100 pt-3">
          <div className="min-w-[220px] flex-1">
            <Input
              label="Reason"
              placeholder="e.g. the image is unreadable"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>
          <Button
            size="sm"
            variant="danger"
            disabled={busy || !reason.trim()}
            onClick={() => { onDecide(false, reason.trim()); setRejecting(false); setReason(''); }}
          >
            Reject document
          </Button>
          <Button size="sm" variant="ghost" onClick={() => { setRejecting(false); setReason(''); }}>Cancel</Button>
        </div>
      )}
    </div>
  );
}

// ── Policy ─────────────────────────────────────────────────────────────────────

/**
 * "Which options work while KYC is pending" — the admin's call, made here. The master switch
 * decides whether the gate applies at all; the grid picks the services that stay open regardless.
 */
function PolicyPanel({ canDecide }: { canDecide: boolean }) {
  const qc = useQueryClient();
  const toast = useToast();

  const { data: policy, isPending } = useQuery({ queryKey: ['kyc-policy'], queryFn: fetchKycPolicy });

  const [enforced, setEnforced] = useState(false);
  const [allowed, setAllowed] = useState<Set<number>>(new Set());
  const [dirty, setDirty] = useState(false);

  /**
   * Seed the editor from the server — but only when the SERVER's answer has actually changed.
   *
   * React Query hands back a new object on every background refetch (window focus, a sibling
   * invalidation), and re-seeding on each of those silently threw away whatever the admin had
   * just toggled: the switch snapped back and the next save posted the old value. Comparing a
   * signature of the saved policy means an identical refetch leaves the editor alone, while a
   * real change — including the one that follows a successful save — re-seeds it.
   */
  const seededRef = useRef<string>('');
  useEffect(() => {
    if (!policy) return;
    const allowedIDs = policy.Services.filter((s) => s.IsAllowedWithoutKYC).map((s) => s.ServiceID).sort((a, b) => a - b);
    const signature = JSON.stringify([policy.IsKYCEnforced, allowedIDs]);
    if (signature === seededRef.current) return;
    seededRef.current = signature;
    setEnforced(policy.IsKYCEnforced);
    setAllowed(new Set(allowedIDs));
    setDirty(false);
  }, [policy]);

  const grouped = useMemo(() => {
    const map = new Map<string, KycPolicyService[]>();
    for (const s of policy?.Services ?? []) {
      const key = s.ServiceCategory || 'Other Services';
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(s);
    }
    return [...map.entries()];
  }, [policy]);

  const saveMut = useMutation({
    mutationFn: () => saveKycPolicy(enforced, [...allowed]),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message); return; }
      toast.success('KYC policy saved. It applies to every user immediately.');
      setDirty(false);
      qc.invalidateQueries({ queryKey: ['kyc-policy'] });
      // The gate changes what every user sees — drop the cached catalog and menu.
      qc.invalidateQueries({ queryKey: ['catalog'] });
      qc.invalidateQueries({ queryKey: ['menu'] });
      qc.invalidateQueries({ queryKey: ['crud', 'SettingMaster/GetAll'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggle = (id: number) => {
    if (!canDecide) return;
    setAllowed((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
    setDirty(true);
  };

  if (isPending) return <Skeleton className="h-72 rounded-card" />;

  return (
    <div className="space-y-4">
      {/* Master switch */}
      <Card className="p-5">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
          <span className={cn(
            'grid size-11 shrink-0 place-items-center rounded-xl ring-1 ring-inset',
            enforced ? 'bg-brand-50 text-brand-600 ring-brand-500/20' : 'bg-slate-50 text-slate-400 ring-slate-200',
          )}>
            {enforced ? <ShieldCheck className="size-5" /> : <ShieldAlert className="size-5" />}
          </span>
          <div className="min-w-0 flex-1">
            <h3 className="font-display text-base font-semibold text-slate-900">Require an approved KYC to transact</h3>
            <p className="mt-1 text-sm text-slate-500">
              On → a user whose KYC is not approved is refused every service except the ones ticked below,
              on the web, in the app and through the API. Off → KYC is recorded but nothing is blocked.
            </p>
          </div>
          <Switch
            on={enforced}
            disabled={!canDecide}
            onChange={(v) => { setEnforced(v); setDirty(true); }}
          />
        </div>

        {enforced && !policy?.HasRequiredDocuments && !policy?.EVerification && (
          <div className="mt-4 flex items-start gap-2.5 rounded-xl border border-warning-500/25 bg-warning-50 px-4 py-3 text-sm text-warning-800">
            <ShieldAlert className="mt-0.5 size-4 shrink-0" />
            <p>
              No document is configured as required, so there is no way for a user to become approved.
              The gate stays open until you add one under{' '}
              <span className="font-medium">Settings → Verification Settings</span> — otherwise enforcing
              KYC would lock everybody out with no route back in.
            </p>
          </div>
        )}

        {policy?.EVerification && (
          <div className="mt-4 flex items-start gap-2.5 rounded-xl border border-info-500/25 bg-info-50 px-4 py-3 text-sm text-info-700">
            <Sparkles className="mt-0.5 size-4 shrink-0" />
            <p>
              E-Verification is on, so users verify through E-KYC and are approved automatically when they
              clear it. Manual document review still applies to anyone already holding submitted documents.
            </p>
          </div>
        )}
      </Card>

      {/* Per-service exemptions */}
      <Card>
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 px-5 py-4">
          <div>
            <h3 className="font-display text-base font-semibold text-slate-900">Allowed while KYC is pending</h3>
            <p className="mt-0.5 text-sm text-slate-500">
              Tick the services a user may still use before their KYC is approved. Everything unticked is
              blocked for them while the gate is on.
            </p>
          </div>
          <div className="flex items-center gap-2">
            <Badge tone={allowed.size > 0 ? 'info' : 'neutral'}>{allowed.size} allowed</Badge>
            {canDecide && (
              <Button loading={saveMut.isPending} disabled={!dirty} onClick={() => saveMut.mutate()}>
                <Save className="size-4" />
                Save policy
              </Button>
            )}
          </div>
        </div>

        <div className="p-5">
          {grouped.length === 0 ? (
            <EmptyState icon={CircleSlash} title="No services" description="Add services under Settings → Service Catalog." />
          ) : (
            <div className="space-y-5">
              {grouped.map(([category, services]) => (
                <div key={category}>
                  <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">{category}</h4>
                  <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-3">
                    {services.map((s) => {
                      const on = allowed.has(s.ServiceID);
                      return (
                        <button
                          key={s.ServiceID}
                          type="button"
                          disabled={!canDecide}
                          onClick={() => toggle(s.ServiceID)}
                          className={cn(
                            'flex items-center gap-3 rounded-xl border px-3.5 py-3 text-left transition-colors',
                            canDecide ? 'cursor-pointer' : 'cursor-default',
                            on ? 'border-brand-500/30 bg-brand-50/60' : 'border-slate-200 bg-white hover:bg-slate-50',
                          )}
                        >
                          <span className={cn(
                            'grid size-5 shrink-0 place-items-center rounded-md border transition-colors',
                            on ? 'border-brand-600 bg-brand-600 text-white' : 'border-slate-300 bg-white',
                          )}>
                            {on && <Check className="size-3.5" />}
                          </span>
                          <span className="min-w-0 flex-1">
                            <span className="block truncate text-sm font-medium text-slate-800">{s.Name}</span>
                            <span className="block text-xs text-slate-400">
                              {on ? 'Works without KYC' : 'Needs an approved KYC'}
                              {!s.IsActive && ' · service is off'}
                            </span>
                          </span>
                        </button>
                      );
                    })}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </Card>

      {!canDecide && (
        <p className="text-xs text-slate-400">
          Only an administrator can change the KYC policy. This view is read-only for you.
        </p>
      )}
    </div>
  );
}

/** Accessible On/Off pill switch (matches the Key Setting screen). */
function Switch({ on, onChange, disabled }: { on: boolean; onChange: (v: boolean) => void; disabled?: boolean }) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={on}
      disabled={disabled}
      onClick={() => onChange(!on)}
      className={cn(
        'relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors',
        on ? 'bg-brand-600' : 'bg-slate-300',
        disabled ? 'cursor-not-allowed opacity-50' : 'cursor-pointer',
      )}
    >
      <span
        className={cn(
          'inline-block size-5 transform rounded-full bg-white shadow transition-transform',
          on ? 'translate-x-5' : 'translate-x-0.5',
        )}
      />
    </button>
  );
}
