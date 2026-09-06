import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Check, Eye, RefreshCw, X } from 'lucide-react';
import {
  approveRegistration,
  fetchPendingRegistrations,
  rejectRegistration,
  type PendingRegistration,
} from '@/api/registration.api';
import { useClientSearch } from '@/lib/useClientSearch';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';

type Tab = 'Pending' | 'Approved' | 'Rejected' | 'All';
const TABS: Tab[] = ['Pending', 'Approved', 'Rejected', 'All'];

export function RegistrationApprovalsScreen() {
  const qc = useQueryClient();
  const toast = useToast();

  const [tab, setTab] = useState<Tab>('Pending');
  const [view, setView] = useState<PendingRegistration | null>(null);
  const [rejectTarget, setRejectTarget] = useState<PendingRegistration | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const { data: res, isFetching, refetch } = useQuery({
    queryKey: ['pending-registrations'],
    queryFn: fetchPendingRegistrations,
  });

  const rows = useMemo(() => {
    const all = res?.data ?? [];
    return tab === 'All' ? all : all.filter((r) => r.Status === tab);
  }, [res, tab]);

  const { query, setQuery, filtered } = useClientSearch(rows as unknown as Record<string, unknown>[]);
  const visible = filtered as unknown as PendingRegistration[];

  const approveMut = useMutation({
    mutationFn: (id: number) => approveRegistration(id),
    onSuccess: (r) => {
      if (r.ok) {
        toast.success('Registration approved. Credentials sent to the user.');
        setView(null);
        qc.invalidateQueries({ queryKey: ['pending-registrations'] });
      } else toast.error(r.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const rejectMut = useMutation({
    mutationFn: (v: { id: number; reason: string }) => rejectRegistration(v.id, v.reason),
    onSuccess: (r) => {
      if (r.ok) {
        toast.success('Registration rejected.');
        setRejectTarget(null);
        setRejectReason('');
        setView(null);
        qc.invalidateQueries({ queryKey: ['pending-registrations'] });
      } else toast.error(r.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const busy = approveMut.isPending || rejectMut.isPending;

  return (
    <div>
      <PageHeader
        title="Registration Approvals"
        subtitle="Self-service sign-ups awaiting review — approve to create the account, or reject with a reason."
        actions={
          <Button variant="secondary" onClick={() => refetch()} disabled={isFetching}>
            <RefreshCw className={isFetching ? 'size-4 animate-spin' : 'size-4'} /> Refresh
          </Button>
        }
      />

      <div className="mt-4 flex flex-wrap items-center gap-2">
        {TABS.map((t) => {
          const count = (res?.data ?? []).filter((r) => (t === 'All' ? true : r.Status === t)).length;
          return (
            <button
              key={t}
              type="button"
              onClick={() => setTab(t)}
              className={
                'rounded-full px-3.5 py-1.5 text-sm font-medium transition ' +
                (tab === t ? 'bg-brand-600 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200')
              }
            >
              {t} <span className="opacity-70">({count})</span>
            </button>
          );
        })}
        <div className="ml-auto w-full sm:w-64">
          <Input placeholder="Search name, mobile, PAN…" value={query} onChange={(e) => setQuery(e.target.value)} />
        </div>
      </div>

      <div className="mt-4 overflow-x-auto rounded-2xl border border-slate-200 bg-white">
        {isFetching && !res ? (
          <div className="flex justify-center py-16">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : visible.length === 0 ? (
          <EmptyState title="Nothing here" description={`No ${tab.toLowerCase()} registrations.`} />
        ) : (
          <table className="w-full min-w-[820px] text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-left text-xs uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3 font-semibold">Applicant</th>
                <th className="px-4 py-3 font-semibold">Mobile</th>
                <th className="px-4 py-3 font-semibold">Role</th>
                <th className="px-4 py-3 font-semibold">PAN</th>
                <th className="px-4 py-3 font-semibold">State</th>
                <th className="px-4 py-3 font-semibold">Submitted</th>
                <th className="px-4 py-3 font-semibold">Status</th>
                <th className="px-4 py-3 text-right font-semibold">Actions</th>
              </tr>
            </thead>
            <tbody>
              {visible.map((r) => (
                <tr key={r.PendingRegistrationID} className="border-b border-slate-100 last:border-0 hover:bg-slate-50">
                  <td className="px-4 py-3">
                    <div className="font-medium text-slate-900">{r.PersonName || '—'}</div>
                    <div className="text-xs text-slate-500">{r.FirmName}</div>
                  </td>
                  <td className="px-4 py-3 text-slate-700">{r.MobileNumber}</td>
                  <td className="px-4 py-3 text-slate-700">{r.UserTypeName || '—'}</td>
                  <td className="px-4 py-3 text-slate-700">{r.PANNo || '—'}</td>
                  <td className="px-4 py-3 text-slate-700">{r.StateName || '—'}</td>
                  <td className="px-4 py-3 text-slate-500">{r.DateTime}</td>
                  <td className="px-4 py-3">
                    <StatusBadge status={r.Status} />
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center justify-end gap-1.5">
                      <Button variant="ghost" size="sm" onClick={() => setView(r)} title="View details">
                        <Eye className="size-4" />
                      </Button>
                      {r.Status === 'Pending' && (
                        <>
                          <Button
                            size="sm"
                            loading={busy}
                            onClick={() => approveMut.mutate(r.PendingRegistrationID)}
                          >
                            <Check className="size-4" /> Approve
                          </Button>
                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => {
                              setRejectTarget(r);
                              setRejectReason('');
                            }}
                          >
                            <X className="size-4" /> Reject
                          </Button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Details modal */}
      <Modal open={Boolean(view)} onClose={() => setView(null)} title="Registration details" size="lg">
        {view && (
          <div className="space-y-4">
            <div className="grid gap-3 sm:grid-cols-2">
              <Detail label="Full name" value={view.PersonName} />
              <Detail label="Firm / shop" value={view.FirmName} />
              <Detail label="Mobile" value={view.MobileNumber} />
              <Detail label="WhatsApp" value={view.WhatsAppNumber} />
              <Detail label="Email" value={view.EmailID} />
              <Detail label="Role" value={view.UserTypeName} />
              <Detail label="PAN" value={view.PANNo} />
              <Detail label="Aadhaar" value={view.AadharNo} />
              <Detail label="GST" value={view.GSTNo} />
              <Detail label="Referral code" value={view.ReferralCode} />
              <Detail label="Address" value={view.Address} />
              <Detail label="City" value={view.CityName} />
              <Detail label="District" value={view.District} />
              <Detail label="Pincode" value={view.Pincode} />
              <Detail label="State" value={view.StateName} />
              <Detail label="Mobile verified" value={view.IsMobileVerified ? 'Yes' : 'No'} />
              <Detail label="Submitted" value={view.DateTime} />
              <Detail label="Status" value={view.Status} />
            </div>
            {view.Status === 'Rejected' && view.RejectReason && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                <span className="font-medium">Reject reason:</span> {view.RejectReason}
              </div>
            )}
            {view.Status === 'Pending' && (
              <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
                <Button
                  variant="secondary"
                  onClick={() => {
                    setRejectTarget(view);
                    setRejectReason('');
                  }}
                >
                  <X className="size-4" /> Reject
                </Button>
                <Button loading={busy} onClick={() => approveMut.mutate(view.PendingRegistrationID)}>
                  <Check className="size-4" /> Approve &amp; create account
                </Button>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* Reject reason modal */}
      <Modal open={Boolean(rejectTarget)} onClose={() => setRejectTarget(null)} title="Reject registration" size="sm">
        {rejectTarget && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              Rejecting <span className="font-medium text-slate-900">{rejectTarget.PersonName}</span> ({rejectTarget.MobileNumber}).
            </p>
            <Input
              label="Reason (optional)"
              placeholder="Why is this being rejected?"
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
            />
            <div className="flex justify-end gap-2">
              <Button variant="ghost" onClick={() => setRejectTarget(null)}>
                Cancel
              </Button>
              <Button
                variant="danger"
                loading={rejectMut.isPending}
                onClick={() => rejectMut.mutate({ id: rejectTarget.PendingRegistrationID, reason: rejectReason })}
              >
                Reject
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

function Detail({ label, value }: { label: string; value?: string }) {
  return (
    <div>
      <div className="text-xs uppercase tracking-wide text-slate-400">{label}</div>
      <div className="mt-0.5 text-sm text-slate-800">{value || '—'}</div>
    </div>
  );
}
