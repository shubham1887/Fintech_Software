import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Archive, Eye, Info, RefreshCw, RotateCcw, Trash2, Users,
} from 'lucide-react';
import { fetchUsers, restoreUser, type UserRow } from '@/api/users.api';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
import { roleLabel, UserTypeCode } from '@/lib/roles';
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
 * Deleted Users — the archive, and nothing else. Users/Select is called with OnlyDeleted, so the
 * server returns soft-deleted rows exclusively: a live user can never surface here, however the
 * client filters are set.
 *
 * A deleted user is a record, not an account: there are no operational actions on this screen
 * (no edit, no activate, no fund, no password reset). The one thing an administrator can do is
 * restore the record — and a restored user comes back DEACTIVATED, so bringing them back is
 * never the same as putting them live.
 */
export function DeletedUsersScreen() {
  const qc = useQueryClient();
  const toast = useToast();
  const { user: me } = useAuth();

  const isAdmin = me?.UserTypeID === UserTypeCode.Admin || me?.UserTypeID === UserTypeCode.SupportUser;

  const [view, setView] = useState<UserRow | null>(null);
  const [confirm, setConfirm] = useState<UserRow | null>(null);

  const { data: rows = [], isPending, isFetching, refetch } = useQuery({
    queryKey: ['deleted-users'],
    queryFn: () => fetchUsers({ OnlyDeleted: true }),
  });

  const { tab, setTab, filtered: roleRows, countFor } = useRoleFilter(rows);
  const { query, setQuery, filtered } = useClientSearch(roleRows as unknown as Record<string, unknown>[]);
  const visible = filtered as unknown as UserRow[];

  const stats = useMemo(() => {
    const thirtyDaysAgo = Date.now() - 30 * 24 * 60 * 60 * 1000;
    return {
      total: rows.length,
      recent: rows.filter((r) => {
        const t = Date.parse(r.LastUpdated);
        return Number.isFinite(t) && t >= thirtyDaysAgo;
      }).length,
      retailers: rows.filter((r) => r.UserTypeID === UserTypeCode.Retailer).length,
      kycApproved: rows.filter((r) => r.IsKYCApproved).length,
    };
  }, [rows]);

  const restoreMut = useMutation({
    mutationFn: (id: number) => restoreUser(id),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message); return; }
      toast.success('User restored. They are inactive — activate them to put the account back in service.');
      setConfirm(null);
      setView(null);
      qc.invalidateQueries({ queryKey: ['deleted-users'] });
      qc.invalidateQueries({ queryKey: ['inactive-users'] });
      qc.invalidateQueries({ queryKey: ['users-screen'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <PageHeader
        title="Deleted Users"
        subtitle="Removed accounts, kept on record — their history and ledger still resolve against them"
        icon={Archive}
        actions={
          <Button variant="outline" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
        }
      />

      <KpiStrip>
        <Kpi label="Deleted users" value={String(stats.total)} tone="text-slate-600" icon={<Trash2 className="size-4" />} />
        <Kpi label="Deleted in 30 days" value={String(stats.recent)} tone="text-danger-600" icon={<Trash2 className="size-4" />} />
        <Kpi label="Retailers" value={String(stats.retailers)} icon={<Users className="size-4" />} />
        <Kpi label="Were KYC approved" value={String(stats.kycApproved)} icon={<Users className="size-4" />} />
      </KpiStrip>

      <div className="mb-4 flex items-start gap-2.5 rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-600">
        <Info className="mt-0.5 size-4 shrink-0 text-slate-400" />
        <p>
          Deletion is reversible and nothing is erased — the record is kept so past transactions,
          commissions and ledger entries still resolve to a real user. These accounts cannot sign in
          or be transacted on.
        </p>
      </div>

      <RoleTabs value={tab} onChange={setTab} countFor={countFor} />

      <TableToolbar
        query={query}
        onQuery={setQuery}
        count={visible.length}
        onExport={() => exportToCsv(visible.map(userExportRow), 'Deleted users')}
      />

      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[980px] border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3">#</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Contact</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Was mapped under</th>
                <th className="px-4 py-3">KYC at deletion</th>
                <th className="px-4 py-3">Registered</th>
                <th className="px-4 py-3">Deleted</th>
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
                      icon={Archive}
                      title="Nothing deleted"
                      description="No user has been removed from this network. Deleted accounts are archived here."
                    />
                  </td>
                </tr>
              ) : (
                visible.map((r, i) => (
                  <tr key={r.UserMasterID} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                    <td className="px-3 py-3 tabular-nums text-slate-400">{i + 1}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <span className="grid size-9 shrink-0 place-items-center rounded-full bg-slate-100 text-sm font-semibold text-slate-400 ring-1 ring-inset ring-slate-200">
                          {(r.FirmName || r.PersonName || 'U').trim().slice(0, 1).toUpperCase()}
                        </span>
                        <div className="min-w-0">
                          <p className="truncate font-medium text-slate-700 line-through decoration-slate-300">
                            {r.PersonName || '—'}
                          </p>
                          {r.FirmName && <p className="truncate text-xs text-slate-400">{r.FirmName}</p>}
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <p className="text-slate-600">{r.MobileNumber || '—'}</p>
                      {r.EmailID && <p className="truncate text-xs text-slate-400">{r.EmailID}</p>}
                    </td>
                    <td className="px-4 py-3">
                      <Badge tone="neutral">{r.UserType || roleLabel(r.UserTypeID) || '—'}</Badge>
                    </td>
                    <td className="px-4 py-3">
                      {r.ParentName
                        ? <span className="text-slate-600">{r.ParentName}</span>
                        : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="px-4 py-3"><KycBadge status={r.KYCStatus} ekyc={r.IseKYC} /></td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-500">{dateOnly(r.DOC)}</td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-700">{dateOnly(r.LastUpdated)}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1.5">
                        <Button variant="ghost" size="sm" onClick={() => setView(r)} title="View full details">
                          <Eye className="size-4" />
                        </Button>
                        {isAdmin && (
                          <Button variant="outline" size="sm" onClick={() => setConfirm(r)}>
                            <RotateCcw className="size-4" />
                            Restore
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Full record — read-only. */}
      <Modal open={Boolean(view)} onClose={() => setView(null)} title="Deleted user" size="lg">
        {view && (
          <div className="space-y-4">
            <div className="flex items-start gap-2.5 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-xs text-slate-500">
              <Trash2 className="mt-0.5 size-3.5 shrink-0" />
              <p>
                Deleted on <span className="font-medium text-slate-700">{dateTime(view.LastUpdated)}</span>. This
                is an archived record — the account cannot sign in or transact.
              </p>
            </div>
            <UserDetailPanel user={view} />
            {isAdmin && (
              <div className="flex justify-end border-t border-slate-100 pt-4">
                <Button variant="outline" onClick={() => setConfirm(view)}>
                  <RotateCcw className="size-4" />
                  Restore this user
                </Button>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* Restore confirmation */}
      <Modal open={Boolean(confirm)} onClose={() => setConfirm(null)} title="Restore user" size="sm">
        {confirm && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              Restore <span className="font-medium text-slate-900">{confirm.PersonName || confirm.FirmName}</span>{' '}
              ({confirm.MobileNumber})?
            </p>
            <div className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-xs text-slate-600">
              The account comes back <span className="font-medium">inactive</span>. It only goes live once an
              administrator activates it from Inactive Users, so restoring never puts a transacting
              account back on the network by itself.
            </div>
            <dl className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <dt className="text-xs text-slate-400">Role</dt>
                <dd className="text-slate-700">{confirm.UserType || roleLabel(confirm.UserTypeID) || '—'}</dd>
              </div>
              <div>
                <dt className="text-xs text-slate-400">Last wallet balance</dt>
                <dd className="tabular-nums text-slate-700">
                  {inr((confirm.WalletBalances ?? []).reduce((s, w) => s + (w.Balance ?? 0), 0))}
                </dd>
              </div>
            </dl>
            <div className="flex justify-end gap-2">
              <Button variant="ghost" onClick={() => setConfirm(null)}>Cancel</Button>
              <Button loading={restoreMut.isPending} onClick={() => restoreMut.mutate(confirm.UserMasterID)}>
                <RotateCcw className="size-4" />
                Restore
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
