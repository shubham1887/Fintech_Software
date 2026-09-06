import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, Clock, Eye, Power, RefreshCw, ShieldCheck, UserRoundX, Users, Wallet,
} from 'lucide-react';
import { fetchUsers, setUserActive, type UserRow } from '@/api/users.api';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
import { roleLabel, UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { inr, dateOnly, dateTime } from '@/lib/format';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Card } from '@/components/ui/Card';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { TableToolbar } from '@/components/ui/TableToolbar';
import { Modal } from '@/components/ui/Modal';
import {
  Kpi, KpiStrip, KycBadge, RoleTabs, UserDetailPanel, useRoleFilter, userExportRow,
} from './userLifecycleShared';

/**
 * Inactive Users — every deactivated account in the network, with the full record behind each
 * row, and the one action that matters here: switch it back on.
 *
 * The list is filtered SERVER-side (Users/Select with IsActive:false), so an active or deleted
 * user cannot appear on this screen even if the client filter were wrong. Activating is the
 * platform operator's call: the button is admin-only here and the API refuses it for everyone
 * else, so a distributor can't reactivate their own downline.
 */
export function InactiveUsersScreen() {
  const qc = useQueryClient();
  const toast = useToast();
  const navigate = useNavigate();
  const { user: me } = useAuth();

  const isAdmin = me?.UserTypeID === UserTypeCode.Admin || me?.UserTypeID === UserTypeCode.SupportUser;

  const [view, setView] = useState<UserRow | null>(null);
  const [confirm, setConfirm] = useState<UserRow | null>(null);

  const { data: rows = [], isPending, isFetching, refetch } = useQuery({
    queryKey: ['inactive-users'],
    queryFn: () => fetchUsers({ IsActive: false }),
  });

  const { tab, setTab, filtered: roleRows, countFor } = useRoleFilter(rows);
  const { query, setQuery, filtered } = useClientSearch(roleRows as unknown as Record<string, unknown>[]);
  const visible = filtered as unknown as UserRow[];

  const stats = useMemo(() => {
    const withBalance = rows.filter((r) => (r.WalletBalances ?? []).some((w) => w.Balance > 0));
    const funds = rows.reduce(
      (sum, r) => sum + (r.WalletBalances ?? []).reduce((s, w) => s + (w.Balance ?? 0), 0),
      0,
    );
    return {
      total: rows.length,
      kycPending: rows.filter((r) => !r.IsKYCApproved).length,
      withBalance: withBalance.length,
      funds,
    };
  }, [rows]);

  const activateMut = useMutation({
    mutationFn: (id: number) => setUserActive(id, true),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message); return; }
      toast.success('User activated. They can sign in and transact again.');
      setConfirm(null);
      setView(null);
      // The user leaves this list and rejoins All Users, so both have to reload.
      qc.invalidateQueries({ queryKey: ['inactive-users'] });
      qc.invalidateQueries({ queryKey: ['users-screen'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <PageHeader
        title="Inactive Users"
        subtitle="Accounts that are switched off — they cannot sign in or transact until an administrator activates them"
        icon={UserRoundX}
        actions={
          <Button variant="outline" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
        }
      />

      <KpiStrip>
        <Kpi label="Inactive users" value={String(stats.total)} tone="text-danger-600" icon={<UserRoundX className="size-4" />} />
        <Kpi label="KYC not approved" value={String(stats.kycPending)} tone="text-warning-600" icon={<ShieldCheck className="size-4" />} />
        <Kpi label="Holding a balance" value={String(stats.withBalance)} icon={<Wallet className="size-4" />} />
        <Kpi label="Funds parked" value={inr(stats.funds)} icon={<Wallet className="size-4" />} />
      </KpiStrip>

      {stats.withBalance > 0 && (
        <div className="mb-4 flex items-start gap-2.5 rounded-xl border border-warning-500/25 bg-warning-50 px-4 py-3 text-sm text-warning-800">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" />
          <p>
            <span className="font-medium">{stats.withBalance}</span> deactivated{' '}
            {stats.withBalance === 1 ? 'account is' : 'accounts are'} still holding{' '}
            <span className="font-medium tabular-nums">{inr(stats.funds)}</span>. Those funds stay frozen
            until the account is activated or the wallet is settled.
          </p>
        </div>
      )}

      <RoleTabs value={tab} onChange={setTab} countFor={countFor} />

      <TableToolbar
        query={query}
        onQuery={setQuery}
        count={visible.length}
        onExport={() => exportToCsv(visible.map(userExportRow), 'Inactive users')}
      />

      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[1000px] border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3">#</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Contact</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Mapped under</th>
                <th className="px-4 py-3">KYC</th>
                <th className="px-4 py-3 text-right">Wallet balance</th>
                <th className="px-4 py-3">Registered</th>
                <th className="px-4 py-3">Deactivated</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isPending ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={i} className="border-b border-slate-100 last:border-0">
                    <td className="px-4 py-3" colSpan={10}><Skeleton className="h-9 w-full" /></td>
                  </tr>
                ))
              ) : visible.length === 0 ? (
                <tr>
                  <td colSpan={10}>
                    <EmptyState
                      icon={Users}
                      title="No inactive users"
                      description="Every account in this segment is active. Deactivated users appear here."
                    />
                  </td>
                </tr>
              ) : (
                visible.map((r, i) => {
                  const balance = (r.WalletBalances ?? []).reduce((s, w) => s + (w.Balance ?? 0), 0);
                  return (
                    <tr key={r.UserMasterID} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      <td className="px-3 py-3 tabular-nums text-slate-400">{i + 1}</td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <span className="grid size-9 shrink-0 place-items-center rounded-full bg-slate-100 text-sm font-semibold text-slate-500 ring-1 ring-inset ring-slate-200">
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
                        {r.ParentName ? (
                          <div className="min-w-0">
                            <p className="truncate text-slate-700">{r.ParentName}</p>
                            {r.ParentMobile && <p className="truncate text-xs text-slate-400">{r.ParentMobile}</p>}
                          </div>
                        ) : (
                          <span className="text-slate-300">—</span>
                        )}
                      </td>
                      <td className="px-4 py-3"><KycBadge status={r.KYCStatus} ekyc={r.IseKYC} /></td>
                      <td className="px-4 py-3 text-right">
                        <span className={cn('font-medium tabular-nums', balance > 0 ? 'text-warning-700' : 'text-slate-400')}>
                          {inr(balance)}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{dateOnly(r.DOC)}</td>
                      <td className="whitespace-nowrap px-4 py-3">
                        <span className="inline-flex items-center gap-1.5 text-slate-600">
                          <Clock className="size-3.5 text-slate-300" />
                          {dateOnly(r.LastUpdated)}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1.5">
                          <Button variant="ghost" size="sm" onClick={() => setView(r)} title="View full details">
                            <Eye className="size-4" />
                          </Button>
                          {isAdmin && (
                            <Button size="sm" onClick={() => setConfirm(r)}>
                              <Power className="size-4" />
                              Activate
                            </Button>
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

      {!isAdmin && visible.length > 0 && (
        <p className="mt-3 text-xs text-slate-400">
          Only an administrator can activate an account. Contact your service provider to have one switched back on.
        </p>
      )}

      {/* Full record */}
      <Modal open={Boolean(view)} onClose={() => setView(null)} title="Inactive user" size="lg">
        {view && (
          <div className="space-y-4">
            <UserDetailPanel user={view} />
            <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 pt-4">
              <Button variant="ghost" size="sm" onClick={() => navigate(`/s/Users/Profile?id=${view.UserMasterID}`)}>
                Open full profile
              </Button>
              {isAdmin && (
                <Button onClick={() => setConfirm(view)}>
                  <Power className="size-4" />
                  Activate this user
                </Button>
              )}
            </div>
          </div>
        )}
      </Modal>

      {/* Activate confirmation */}
      <Modal open={Boolean(confirm)} onClose={() => setConfirm(null)} title="Activate user" size="sm">
        {confirm && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              Activate <span className="font-medium text-slate-900">{confirm.PersonName || confirm.FirmName}</span>{' '}
              ({confirm.MobileNumber})? They will be able to sign in and transact immediately, and they
              are notified by SMS.
            </p>
            {!confirm.IsKYCApproved && (
              <div className="flex items-start gap-2.5 rounded-lg border border-warning-500/25 bg-warning-50 px-3 py-2.5 text-xs text-warning-800">
                <ShieldCheck className="mt-0.5 size-3.5 shrink-0" />
                <p>
                  This user's KYC is <span className="font-medium">{confirm.KYCStatus.toLowerCase()}</span>. While
                  KYC enforcement is on they will still be blocked from every service that isn't allowed
                  without KYC.
                </p>
              </div>
            )}
            <dl className="grid grid-cols-2 gap-3 rounded-lg bg-slate-50 p-3 text-sm">
              <div>
                <dt className="text-xs text-slate-400">Role</dt>
                <dd className="text-slate-700">{confirm.UserType || roleLabel(confirm.UserTypeID) || '—'}</dd>
              </div>
              <div>
                <dt className="text-xs text-slate-400">Deactivated on</dt>
                <dd className="text-slate-700">{dateTime(confirm.LastUpdated)}</dd>
              </div>
            </dl>
            <div className="flex justify-end gap-2">
              <Button variant="ghost" onClick={() => setConfirm(null)}>Cancel</Button>
              <Button loading={activateMut.isPending} onClick={() => activateMut.mutate(confirm.UserMasterID)}>
                <Power className="size-4" />
                Activate
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
