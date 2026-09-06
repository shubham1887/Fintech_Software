import { useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  KeyRound, Lock, MoreVertical, Pencil, Power, RefreshCw, ShieldCheck, Trash2,
  UserCheck, UserPlus, UserX, Users,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { deleteUser } from '@/api/users.api';
import { useAuth } from '@/features/auth/AuthContext';
import { UserTypeCode } from '@/lib/roles';
import { exportToCsv } from '@/lib/csv';
import { useClientSearch } from '@/lib/useClientSearch';
import { roleLabel } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { getField as get } from '@/lib/row';
import { inr, dateOnly } from '@/lib/format';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { Card } from '@/components/ui/Card';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { TableToolbar } from '@/components/ui/TableToolbar';
import { Modal } from '@/components/ui/Modal';

type Row = Record<string, unknown>;

// Role tabs. `type` filters on UserTypeID; 'all' shows everyone in the network.
const TABS: { key: string; label: string; type: number | 'all' }[] = [
  { key: 'all', label: 'All Users', type: 'all' },
  { key: 'retailer', label: 'Retailer', type: 4 },
  { key: 'superdist', label: 'Super Distributor', type: 2 },
  { key: 'dist', label: 'Distributor', type: 3 },
  { key: 'api', label: 'API User', type: 5 },
];

// Status sub-filter (applied within the selected role tab).
type StatusKey = 'all' | 'active' | 'inactive' | 'kyc' | 'deleted';
const STATUS_FILTERS: { key: StatusKey; label: string }[] = [
  { key: 'all', label: 'All' },
  { key: 'active', label: 'Active' },
  { key: 'inactive', label: 'Inactive' },
  { key: 'kyc', label: 'KYC pending' },
  { key: 'deleted', label: 'Deleted' },
];
/** Read a user row's per-wallet balances into a WalletID → {balance, hold} map. */
const walletMapOf = (r: Row): Map<number, { balance: number; hold: number }> => {
  const m = new Map<number, { balance: number; hold: number }>();
  const arr = get(r, 'WalletBalances');
  if (Array.isArray(arr)) {
    for (const w of arr as Row[]) {
      const id = Number(get(w, 'WalletID'));
      if (id > 0) m.set(id, { balance: Number(get(w, 'Balance')) || 0, hold: Number(get(w, 'HoldBalance')) || 0 });
    }
  }
  return m;
};

const matchesStatus = (r: Row, s: StatusKey): boolean => {
  const deleted = Boolean(get(r, 'IsDeleted'));
  const active = Boolean(get(r, 'IsActive'));
  const kyc = Boolean(get(r, 'IsKYCApproved'));
  switch (s) {
    case 'active': return !deleted && active;
    case 'inactive': return !deleted && !active;
    case 'kyc': return !deleted && !kyc;
    case 'deleted': return deleted;
    default: return true;
  }
};

/** Badge tone for the derived KYC state. */
const kycTone = (status: string): 'success' | 'danger' | 'warning' | 'neutral' => {
  const s = status.toLowerCase();
  if (s.startsWith('approved')) return 'success';
  if (s.startsWith('rejected')) return 'danger';
  if (s.startsWith('pending')) return 'warning';
  return 'neutral';
};

/** One flat KPI in the summary strip — label over value, no card chrome. */
function Kpi({ label, value, tone = 'text-slate-800', icon }: { label: string; value: string; tone?: string; icon: React.ReactNode }) {
  return (
    <div className="flex items-start gap-3 px-4 py-3.5">
      <div className="mt-0.5 grid size-8 shrink-0 place-items-center rounded-lg bg-slate-50 text-slate-400 ring-1 ring-inset ring-slate-200/70">
        {icon}
      </div>
      <div className="min-w-0">
        <div className="truncate text-xs font-medium uppercase tracking-wide text-slate-500">{label}</div>
        <div className={cn('mt-0.5 font-display text-lg font-bold tabular-nums', tone)}>{value}</div>
      </div>
    </div>
  );
}

export function UsersScreen() {
  const qc = useQueryClient();
  const toast = useToast();
  const navigate = useNavigate();
  const { user: me } = useAuth();
  // Switching an account on or off, and deleting one, are the platform operator's calls — the API
  // refuses them for every other role, so the menu doesn't offer what would only fail.
  const isAdmin = me?.UserTypeID === UserTypeCode.Admin || me?.UserTypeID === UserTypeCode.SupportUser;
  const [deleteTarget, setDeleteTarget] = useState<Row | null>(null);
  const [holdTarget, setHoldTarget] = useState<Row | null>(null);
  const [holdAmount, setHoldAmount] = useState('');
  const [holdRemark, setHoldRemark] = useState('');
  const [tab, setTab] = useState('all');
  const [status, setStatus] = useState<StatusKey>('all');

  // ShowDeleted so IsDeleted can be surfaced; admin sees the whole chain (scoped server-side).
  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['users-screen'],
    queryFn: () => fetchList('Users/Select', { TakeCount: 1000, SkipCount: 0, ShowDeleted: true }),
  });

  // Wallets are dynamic (WalletMaster) — one balance column per active wallet, in display order.
  const { data: wallets = [] } = useQuery({
    queryKey: ['users-wallets'], staleTime: 300_000,
    queryFn: () => fetchList('Wallet/Select', {}),
  });
  const walletCols = useMemo(
    () => [...wallets]
      .sort((a, b) => (Number(get(a, 'UISequence')) || 0) - (Number(get(b, 'UISequence')) || 0))
      .map((w) => ({ id: Number(get(w, 'WalletID')), name: String(get(w, 'Name', 'WalletName') ?? '') }))
      .filter((w) => w.id > 0),
    [wallets],
  );
  const colCount = 10 + walletCols.length;

  const tabRows = useMemo(() => {
    const t = TABS.find((x) => x.key === tab);
    if (!t || t.type === 'all') return rows;
    return rows.filter((r) => Number(get(r, 'UserTypeID')) === t.type);
  }, [rows, tab]);
  const statusRows = useMemo(() => tabRows.filter((r) => matchesStatus(r, status)), [tabRows, status]);
  const { query, setQuery, filtered } = useClientSearch(statusRows);

  const activateMut = useMutation({
    mutationFn: (v: { id: unknown; next: boolean }) => apiPost('Users/Activate', { TargetUserID: v.id, IsActive: v.next }),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success('User status updated.');
        qc.invalidateQueries({ queryKey: ['users-screen'] });
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Soft delete — the row is kept so history still resolves; it moves to Deleted Users. The API
  // refuses it while the user holds a balance or still has a downline mapped under them.
  const deleteMut = useMutation({
    mutationFn: (id: number) => deleteUser(id),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message); return; }
      toast.success('User deleted. You can restore them from Deleted Users.');
      setDeleteTarget(null);
      qc.invalidateQueries({ queryKey: ['users-screen'] });
      qc.invalidateQueries({ queryKey: ['deleted-users'] });
      qc.invalidateQueries({ queryKey: ['inactive-users'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Forgot password — resets the user and texts the new password via the configured SMS gateway.
  const pwdMut = useMutation({
    mutationFn: (id: unknown) => apiPost('Users/SendPassword', { TargetUserID: id }),
    onSuccess: (res) => (res.ok ? toast.success('New password sent by SMS.') : toast.error(res.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  // Add hold balance — freezes an amount of the user's wallet so it can't be spent.
  const holdMut = useMutation({
    mutationFn: (v: { id: unknown; amount: number; remark: string }) =>
      apiPost('Users/AddHoldBalance', { TargetUserID: v.id, Amount: v.amount, Remark: v.remark }),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success('Hold balance updated.');
        closeHold();
        qc.invalidateQueries({ queryKey: ['users-screen'] });
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const closeHold = () => { setHoldTarget(null); setHoldAmount(''); setHoldRemark(''); };
  const submitHold = () => {
    if (!holdTarget) return;
    const amount = Number(holdAmount);
    if (!amount || amount <= 0) { toast.error('Enter a hold amount greater than zero.'); return; }
    holdMut.mutate({ id: get(holdTarget, 'UserMasterID', 'UserID'), amount, remark: holdRemark.trim() });
  };

  // KPI strip reflects the selected role tab (the network segment you're looking at).
  // Count-only overview — no summed wallet balances (per-user balances stay in the table).
  const stats = useMemo(() => {
    const live = tabRows.filter((r) => !get(r, 'IsDeleted'));
    const active = live.filter((r) => Boolean(get(r, 'IsActive'))).length;
    const kyc = live.filter((r) => Boolean(get(r, 'IsKYCApproved'))).length;
    return { total: live.length, active, inactive: live.length - active, kyc, deleted: tabRows.length - live.length };
  }, [tabRows]);

  const countFor = (type: number | 'all') =>
    type === 'all' ? rows.length : rows.filter((r) => Number(get(r, 'UserTypeID')) === type).length;
  const statusCount = (s: StatusKey) => (s === 'all' ? tabRows.length : tabRows.filter((r) => matchesStatus(r, s)).length);

  // Flattened export — one column per wallet (not the raw WalletBalances array).
  const exportRows = () => exportToCsv(filtered.map((r) => {
    const wmap = walletMapOf(r);
    const wcols: Record<string, unknown> = {};
    walletCols.forEach((w) => { wcols[w.name] = wmap.get(w.id)?.balance ?? 0; });
    return {
      Name: get(r, 'PersonName', 'Name'),
      Firm: get(r, 'FirmName'),
      Mobile: get(r, 'MobileNumber', 'Mobile'),
      Email: get(r, 'EmailID', 'Email'),
      Role: get(r, 'UserType'),
      Slab: get(r, 'SlabName'),
      MappedUnder: get(r, 'ParentName'),
      Registered: dateOnly(String(get(r, 'DOC') ?? '')),
      ...wcols,
      KYC: String(get(r, 'KYCStatus') ?? '') || (get(r, 'IsKYCApproved') ? 'Approved' : 'Pending'),
      Status: get(r, 'IsDeleted') ? 'Deleted' : get(r, 'IsActive') ? 'Active' : 'Inactive',
    };
  }), 'Users');

  return (
    <>
      <PageHeader
        title="All Users"
        subtitle="Everyone in your distribution network"
        icon={Users}
        actions={
          <div className="flex items-center gap-2">
            <Button variant="outline" onClick={() => refetch()} loading={isFetching}>
              <RefreshCw className="size-4" />
              Refresh
            </Button>
            <Button onClick={() => navigate('/s/Users/Add')}>
              <UserPlus className="size-4" />
              New user
            </Button>
          </div>
        }
      />

      {/* Flat KPI strip — count-only overview for the selected role segment. */}
      <div className="mb-5 grid grid-cols-2 divide-x divide-y divide-slate-200/70 overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card sm:grid-cols-3 lg:grid-cols-5 lg:divide-y-0">
        <Kpi label="Total users" value={String(stats.total)} icon={<Users className="size-4" />} />
        <Kpi label="Active" value={String(stats.active)} tone="text-success-600" icon={<UserCheck className="size-4" />} />
        <Kpi label="Inactive" value={String(stats.inactive)} tone="text-danger-600" icon={<UserX className="size-4" />} />
        <Kpi label="KYC approved" value={String(stats.kyc)} tone="text-info-600" icon={<ShieldCheck className="size-4" />} />
        <Kpi label="Deleted" value={String(stats.deleted)} tone="text-slate-500" icon={<UserX className="size-4" />} />
      </div>

      {/* Role tabs */}
      <div className="mb-3 flex flex-wrap gap-1 border-b border-slate-200">
        {TABS.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={cn(
              '-mb-px border-b-2 px-4 py-2 text-sm font-medium transition-colors',
              tab === t.key ? 'border-brand-600 text-brand-700' : 'border-transparent text-slate-500 hover:text-slate-700',
            )}
          >
            {t.label} <span className="tabular-nums text-slate-400">({countFor(t.type)})</span>
          </button>
        ))}
      </div>

      {/* Status sub-filter */}
      <div className="mb-4 flex flex-wrap gap-1.5">
        {STATUS_FILTERS.map((s) => (
          <button
            key={s.key}
            type="button"
            onClick={() => setStatus(s.key)}
            className={cn(
              'rounded-full px-3 py-1 text-xs font-medium ring-1 ring-inset transition-colors',
              status === s.key ? 'bg-brand-50 text-brand-700 ring-brand-500/25' : 'text-slate-500 ring-slate-200 hover:bg-slate-50',
            )}
          >
            {s.label} <span className="tabular-nums opacity-70">({statusCount(s.key)})</span>
          </button>
        ))}
      </div>

      <TableToolbar query={query} onQuery={setQuery} count={filtered.length} onExport={exportRows} />

      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-3">#</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Contact</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Mapped under</th>
                <th className="px-4 py-3">Slab</th>
                <th className="px-4 py-3">Registered</th>
                {walletCols.map((w) => (
                  <th key={w.id} className="whitespace-nowrap px-4 py-3 text-right">{w.name}</th>
                ))}
                <th className="px-4 py-3">KYC</th>
                <th className="px-4 py-3">Status</th>
                <th className="w-10 px-3 py-3" />
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 6 }).map((_, i) => (
                  <tr key={i} className="border-b border-slate-100 last:border-0">
                    <td className="px-4 py-3" colSpan={colCount}>
                      <Skeleton className="h-9 w-full" />
                    </td>
                  </tr>
                ))
              ) : filtered.length === 0 ? (
                <tr>
                  <td colSpan={colCount}>
                    <EmptyState icon={Users} title="No users found" description="Users will appear here as they are onboarded." />
                  </td>
                </tr>
              ) : (
                filtered.map((r, i) => {
                  const name = String(get(r, 'PersonName', 'Name') ?? '—');
                  const firm = String(get(r, 'FirmName') ?? '');
                  const mobile = String(get(r, 'MobileNumber', 'Mobile') ?? '—');
                  const email = String(get(r, 'EmailID', 'Email') ?? '');
                  const roleName = String(get(r, 'UserType') ?? '') || roleLabel(Number(get(r, 'UserTypeID') ?? 0)) || '—';
                  const parentName = String(get(r, 'ParentName') ?? '');
                  const parentMobile = String(get(r, 'ParentMobile') ?? '');
                  const active = Boolean(get(r, 'IsActive'));
                  const deleted = Boolean(get(r, 'IsDeleted'));
                  const kyc = Boolean(get(r, 'IsKYCApproved'));
                  const kycStatus = String(get(r, 'KYCStatus') ?? '') || (kyc ? 'Approved' : 'Pending');
                  const hold = Boolean(get(r, 'IsCommissionHold'));
                  const initials = (firm || name || 'U').trim().slice(0, 1).toUpperCase();
                  const id = get(r, 'UserMasterID', 'UserID');
                  const slab = String(get(r, 'SlabName') ?? '');
                  const wmap = walletMapOf(r);
                  return (
                    <tr key={i} className={cn('border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60', deleted && 'opacity-60')}>
                      <td className="px-3 py-3 tabular-nums text-slate-400">{i + 1}</td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <span className="grid size-9 shrink-0 place-items-center rounded-full bg-slate-100 text-sm font-semibold text-slate-600 ring-1 ring-inset ring-slate-200">
                            {initials}
                          </span>
                          <div className="min-w-0">
                            <p className="truncate font-medium text-slate-800">{name}</p>
                            {firm && <p className="truncate text-xs text-slate-400">{firm}</p>}
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <p className="text-slate-700">{mobile}</p>
                        {email && <p className="truncate text-xs text-slate-400">{email}</p>}
                      </td>
                      <td className="px-4 py-3">
                        <Badge tone="brand">{roleName}</Badge>
                      </td>
                      <td className="px-4 py-3">
                        {parentName ? (
                          <div className="min-w-0">
                            <p className="truncate text-slate-700">{parentName}</p>
                            {parentMobile && <p className="truncate text-xs text-slate-400">{parentMobile}</p>}
                          </div>
                        ) : (
                          <span className="text-slate-300">—</span>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        {slab ? <span className="text-slate-700">{slab}</span> : <span className="text-slate-300">—</span>}
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{dateOnly(String(get(r, 'DOC') ?? ''))}</td>
                      {walletCols.map((w) => {
                        const wb = wmap.get(w.id);
                        return (
                          <td key={w.id} className="px-4 py-3 text-right">
                            {wb ? (
                              <>
                                <div className="font-medium tabular-nums text-slate-800">{inr(wb.balance)}</div>
                                {wb.hold > 0 && <div className="text-[11px] tabular-nums text-slate-400">{inr(wb.hold)} hold</div>}
                              </>
                            ) : (
                              <span className="text-slate-300">—</span>
                            )}
                          </td>
                        );
                      })}
                      <td className="px-4 py-3">
                        {/* The derived state from the server (Approved / Pending / Rejected /
                            Not submitted) — the same verdict the KYC Approvals queue shows. */}
                        <div className="flex flex-wrap items-center gap-1">
                          <Badge tone={kycTone(kycStatus)}>{kycStatus}</Badge>
                          {Boolean(get(r, 'IseKYC')) && <Badge tone="info">E-KYC</Badge>}
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex flex-wrap gap-1">
                          {deleted ? (
                            <Badge tone="danger">Deleted</Badge>
                          ) : (
                            <Badge tone={active ? 'success' : 'danger'}>{active ? 'Active' : 'Inactive'}</Badge>
                          )}
                          {hold && <Badge tone="neutral">Comm hold</Badge>}
                        </div>
                      </td>
                      <td className="px-3 py-3">
                        <RowMenu
                          deleted={deleted}
                          active={active}
                          isAdmin={isAdmin}
                          onView={() => navigate(`/s/Users/Profile?id=${id}`)}
                          onEdit={() => navigate(`/s/Users/Edit?id=${id}`)}
                          onToggleActive={() => activateMut.mutate({ id, next: !active })}
                          onHold={() => { setHoldTarget(r); setHoldAmount(''); setHoldRemark(''); }}
                          onResetPwd={() => pwdMut.mutate(id)}
                          onDelete={() => setDeleteTarget(r)}
                        />
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Delete confirmation. Soft delete, so the wording says exactly what survives. */}
      <Modal open={Boolean(deleteTarget)} onClose={() => setDeleteTarget(null)} title="Delete user" size="sm">
        {deleteTarget && (
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              Delete{' '}
              <span className="font-medium text-slate-900">
                {String(get(deleteTarget, 'PersonName') || get(deleteTarget, 'FirmName') || '—')}
              </span>{' '}
              ({String(get(deleteTarget, 'MobileNumber') ?? '—')})?
            </p>
            <div className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-xs text-slate-600">
              The account can no longer sign in or transact, but nothing is erased — the record moves to{' '}
              <span className="font-medium">Deleted Users</span>, where an administrator can restore it, and
              its past transactions and ledger entries stay intact.
            </div>
            <div className="rounded-lg border border-warning-500/25 bg-warning-50 px-3 py-2.5 text-xs text-warning-800">
              This is refused while the user holds a wallet balance or still has users mapped beneath them —
              move those first.
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="ghost" onClick={() => setDeleteTarget(null)}>Cancel</Button>
              <Button
                variant="danger"
                loading={deleteMut.isPending}
                onClick={() => deleteMut.mutate(Number(get(deleteTarget, 'UserMasterID', 'UserID')))}
              >
                <Trash2 className="size-4" />
                Delete user
              </Button>
            </div>
          </div>
        )}
      </Modal>

      <Modal open={Boolean(holdTarget)} onClose={closeHold} title="Add hold balance">
        {holdTarget && (
          <div className="space-y-4">
            <p className="text-sm text-slate-500">
              Freeze part of{' '}
              <span className="font-medium text-slate-700">{String(get(holdTarget, 'FirmName') || get(holdTarget, 'PersonName') || '—')}</span>’s
              wallet. Held funds stay in the balance but can’t be spent.
            </p>
            <div className="flex gap-4 rounded-lg bg-slate-50 p-3 text-sm">
              <div>
                <p className="text-xs text-slate-400">Balance</p>
                <p className="font-medium tabular-nums text-slate-800">{inr(Number(get(holdTarget, 'Balance')) || 0)}</p>
              </div>
              <div>
                <p className="text-xs text-slate-400">Currently on hold</p>
                <p className="font-medium tabular-nums text-slate-800">{inr(Number(get(holdTarget, 'HoldBalance')) || 0)}</p>
              </div>
            </div>
            <Input label="Hold amount (₹)" type="number" min="0" placeholder="0.00" value={holdAmount} onChange={(e) => setHoldAmount(e.target.value)} />
            <Input label="Remark (optional)" placeholder="Why is this being held?" value={holdRemark} onChange={(e) => setHoldRemark(e.target.value)} />
            <div className="flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={closeHold}>Cancel</Button>
              <Button size="sm" loading={holdMut.isPending} onClick={submitHold}>Add hold</Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}

/**
 * Per-row action menu rendered in a portal (fixed position) so the Card's overflow-hidden /
 * horizontal scroll can't clip it. Flips up / clamps to stay on-screen.
 */
function RowMenu({
  deleted, active, isAdmin, onView, onEdit, onToggleActive, onHold, onResetPwd, onDelete,
}: {
  deleted: boolean;
  active: boolean;
  /** Activate / deactivate and delete are admin-only — the API refuses them for anyone else. */
  isAdmin: boolean;
  onView: () => void;
  onEdit: () => void;
  onToggleActive: () => void;
  onHold: () => void;
  onResetPwd: () => void;
  onDelete: () => void;
}) {
  const [open, setOpen] = useState(false);
  const btnRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const [pos, setPos] = useState<{ top: number; left: number } | null>(null);

  const items: { label: string; icon: React.ReactNode; onClick: () => void; primary?: boolean; danger?: boolean }[] = [
    { label: 'View details', icon: null, onClick: onView, primary: true },
    ...(deleted ? [] : [
      { label: 'Edit', icon: <Pencil className="size-4 text-slate-400" />, onClick: onEdit },
      ...(isAdmin ? [{
        label: active ? 'Deactivate' : 'Activate',
        icon: <Power className={cn('size-4', active ? 'text-danger-500' : 'text-success-600')} />,
        onClick: onToggleActive,
      }] : []),
      { label: 'Add hold balance', icon: <Lock className="size-4 text-slate-400" />, onClick: onHold },
      { label: 'Forgot password (SMS)', icon: <KeyRound className="size-4 text-slate-400" />, onClick: onResetPwd },
      ...(isAdmin ? [{
        label: 'Delete user',
        icon: <Trash2 className="size-4 text-danger-500" />,
        onClick: onDelete,
        danger: true,
      }] : []),
    ]),
  ];

  useLayoutEffect(() => {
    if (!open || !btnRef.current) return;
    const r = btnRef.current.getBoundingClientRect();
    const menuW = 192; // w-48
    const menuH = 8 + items.length * 34;
    let left = Math.max(8, r.right - menuW);
    if (left + menuW > window.innerWidth - 8) left = window.innerWidth - 8 - menuW;
    let top = r.bottom + 4;
    if (top + menuH > window.innerHeight - 8) top = Math.max(8, r.top - menuH - 4);
    setPos({ top, left });
  }, [open, items.length]);

  useEffect(() => {
    if (!open) return;
    const onDoc = (e: MouseEvent) => {
      if (menuRef.current?.contains(e.target as Node) || btnRef.current?.contains(e.target as Node)) return;
      setOpen(false);
    };
    const onMove = () => setOpen(false);
    document.addEventListener('mousedown', onDoc);
    window.addEventListener('scroll', onMove, true);
    window.addEventListener('resize', onMove);
    return () => {
      document.removeEventListener('mousedown', onDoc);
      window.removeEventListener('scroll', onMove, true);
      window.removeEventListener('resize', onMove);
    };
  }, [open]);

  return (
    <>
      <button
        ref={btnRef}
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
        className="grid size-7 cursor-pointer place-items-center rounded-lg text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600"
      >
        <MoreVertical className="size-4" />
      </button>
      {open && pos && createPortal(
        <div
          ref={menuRef}
          role="menu"
          style={{ position: 'fixed', top: pos.top, left: pos.left }}
          className="z-50 w-48 overflow-hidden rounded-lg border border-slate-200 bg-white py-1 shadow-lg"
        >
          {items.map((it) => (
            <button
              key={it.label}
              type="button"
              role="menuitem"
              className={cn(
                'flex w-full cursor-pointer items-center gap-2 px-3 py-1.5 text-left text-sm',
                it.primary ? 'font-medium text-brand-700 hover:bg-brand-50'
                  : it.danger ? 'text-danger-600 hover:bg-danger-50'
                    : 'text-slate-700 hover:bg-slate-50',
              )}
              onClick={() => { it.onClick(); setOpen(false); }}
            >
              {it.icon}
              {it.label}
            </button>
          ))}
        </div>,
        document.body,
      )}
    </>
  );
}
