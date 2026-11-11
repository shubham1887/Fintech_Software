import { useMemo, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  RefreshCw, Search as SearchIcon, RotateCcw, Eye, Plus, MessageSquare, ShieldCheck,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getMpinStatus } from '@/api/security.api';
import { inr } from '@/lib/format';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useAuth } from '@/features/auth/AuthContext';
import { isAdminTier, isRetailer } from '@/lib/roles';
import { MyOutstandingCard } from './MyOutstandingCard';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Modal } from '@/components/ui/Modal';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { pickIcon } from '@/components/layout/iconMap';
import { OutstandingStatement } from './OutstandingStatement';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

/**
 * Outstanding — who owes how much, and the statement behind each figure.
 *
 * What the number means: a fund request or transfer whose Mode is "Credit" funds the receiver's
 * wallet AND records the same amount as a debt to the upline who approved it. Which wallet the
 * money went into does not matter — the debt is to a person. Reversing the funds takes the debt
 * with them; recording a collection here reduces it.
 *
 * Scoping is done by the server from the signed token: admin and support see the whole network,
 * everyone else sees their own downline. A figure here is a commercial fact about another
 * business, so the client never asks for a wider set than it is entitled to.
 *
 * Endpoints: CashCollection/OutstandingReport, /Statement, /Add, /SetLimit, /SendCollectionMessage.
 */
export function OutstandingScreen({ def }: CustomScreenProps) {
  const qc = useQueryClient();
  const toast = useToast();
  const { user } = useAuth();
  const roleId = Number(user?.UserTypeID ?? 0);
  const isAdmin = isAdminTier(roleId);
  const currentUserId = Number(user?.UserID ?? 0);
  // A retailer has nobody under them, so "who owes me" can only ever be empty for them.
  const hasDownline = !isRetailer(roleId);

  const [search, setSearch] = useState('');
  const [applied, setApplied] = useState('');

  /** The user whose statement is open (the View button). */
  const [viewing, setViewing] = useState<Row | null>(null);
  /** The user a collection is being recorded against (the Add button). */
  const [collecting, setCollecting] = useState<Row | null>(null);
  /** The user whose credit limit is being set (admin only). */
  const [limiting, setLimiting] = useState<Row | null>(null);

  // The network default, so the limit dialog can pre-fill it and say where it came from instead
  // of making an admin go and look it up in Key Settings.
  const { data: defaultLimit = 0 } = useQuery({
    queryKey: ['default-credit-limit'],
    enabled: isAdmin,
    staleTime: 300_000,
    queryFn: async () => {
      const rows = await fetchList('SettingMaster/GetAll', { PageNumber: 1, PageSize: 500 });
      const row = rows.find((r) => String(getField(r, 'Key') ?? '') === 'DefaultCreditLimit');
      if (!row) return 0;
      // Honoured only while the switch is on — the same rule the server applies, so the dialog
      // cannot advertise a default that would not actually be granted.
      return getField(row, 'IsOn') === true ? Number(getField(row, 'Value') ?? 0) || 0 : 0;
    },
  });

  const { data: wrapper, isFetching, refetch } = useQuery({
    queryKey: ['outstanding-report', applied],
    queryFn: async () => {
      const res = await apiPost<Record<string, unknown>>('CashCollection/OutstandingReport', {
        UserID: 0, SearchText: applied, TakeCount: 0, SkipCount: 0, IsExcelDownload: false,
      });
      if (!res.ok) throw new Error(res.message || 'Could not load outstanding balances.');
      return (res.data ?? {}) as Record<string, unknown>;
    },
  });

  const totalOutstanding = Number(getField(wrapper ?? {}, 'TotalOutstanding') ?? 0);
  const allRows = useMemo(() => {
    const d = wrapper ? getField(wrapper, 'data', 'Data') : null;
    return Array.isArray(d) ? (d as Row[]) : [];
  }, [wrapper]);
  const withBalance = allRows.filter((r) => Number(getField(r, 'Outstanding') ?? 0) > 0);
  // Accounts at ₹0.00 are hidden by default: the page is "who owes me", and a list padded with
  // every account that owes nothing buries the ones that do. A search still finds them (so a
  // limit can be set on a clean account), and the toggle shows them all.
  const [showZero, setShowZero] = useState(false);
  const rows = showZero || applied ? allRows : withBalance;

  const remind = useMutation({
    mutationFn: (userId: number) => apiPost('CashCollection/SendCollectionMessage', { UserID: userId }),
    onSuccess: (res) => {
      if (res.ok) toast.success('Reminder sent.');
      else toast.error(res.message || 'The reminder could not be sent.');
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const runSearch = () => setApplied(search.trim());
  const reset = () => { setSearch(''); setApplied(''); };
  const refreshAll = () => {
    qc.invalidateQueries({ queryKey: ['outstanding-report'] });
    qc.invalidateQueries({ queryKey: ['outstanding-statement'] });
    // The header tile reads the shared live feed, so a collection should move it immediately.
    qc.invalidateQueries({ queryKey: ['live-summary'] });
  };

  const columns = ['Action', '#', 'User', 'Owed to', 'Outstanding', 'Credit limit', 'Available'];

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={`${withBalance.length} account${withBalance.length === 1 ? '' : 's'} carrying a balance`}
        icon={pickIcon(def.title)}
        actions={
          <div className="flex items-center gap-2">
            {/* Without this, setting the first limit is impossible from the UI: every account
                starts at a zero limit and zero balance, the list hides those rows, so an admin
                opens an empty page with nothing to click - and every credit request keeps being
                refused. The picker does not depend on the user being listed. */}
            {isAdmin && (
              <Button variant="outline" size="sm" onClick={() => setLimiting({})}>
                <ShieldCheck className="size-4" />
                Set credit limit
              </Button>
            )}
            <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
              <RefreshCw className="size-4" />
              Refresh
            </Button>
          </div>
        }
      />

      {/* What the signed-in user themselves owes. First, because for a retailer it is the ONLY
          part of this screen that applies — the table below is a downline view and theirs is
          empty. Without this the feature looked like it did not apply to the people carrying the
          debt. */}
      <MyOutstandingCard userId={currentUserId} />

      {/* Who owes ME. Hidden for a role with no downline rather than shown as a permanently empty
          table with a "nothing outstanding" message that reads like a fault. */}
      {(hasDownline || rows.length > 0) && (
      <Card className="overflow-hidden">
        <div className="flex flex-wrap items-center justify-between gap-x-6 gap-y-3 border-b border-slate-100 px-5 py-3.5">
          <div className="flex items-center gap-5">
            <Metric label="Accounts" value={String(withBalance.length)} />
            <span className="h-8 w-px bg-slate-200/80" />
            <Metric label="Total outstanding" value={inr(totalOutstanding)} accent />
          </div>
          <form
            onSubmit={(e) => { e.preventDefault(); runSearch(); }}
            className="flex items-end gap-2"
          >
            <label className="mr-2 flex items-center gap-1.5 whitespace-nowrap text-xs text-slate-500">
              <input type="checkbox" checked={showZero} onChange={(e) => setShowZero(e.target.checked)} className="size-3.5 rounded border-slate-300" />
              Show ₹0.00 accounts
            </label>
            <Input
              placeholder="Firm / person / mobile…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-56"
            />
            <Button type="submit" size="sm" loading={isFetching}>
              <SearchIcon className="size-4" />
              Search
            </Button>
            <Button type="button" size="sm" variant="outline" onClick={reset}>
              <RotateCcw className="size-4" />
            </Button>
          </form>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-xs font-semibold uppercase tracking-wide text-slate-500">
                {columns.map((h) => (
                  <th
                    key={h}
                    className={cn(
                      'whitespace-nowrap px-4 py-3',
                      ['Outstanding', 'Credit limit', 'Available'].includes(h) ? 'text-right'
                        : h === 'Action' ? 'text-right' : 'text-left',
                    )}
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 5 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {columns.map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[110px]" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={columns.length}>
                    <EmptyState
                      title="Nothing outstanding"
                      description={
                        isAdmin
                          ? 'Nobody is carrying a credit balance and no credit limits are set. Until an account has a limit it cannot take funds on credit — use "Set credit limit" above to give one. Searching a firm name also finds accounts that are still at zero.'
                          : 'Nobody in your network is carrying a credit balance. Credit limits are set by the admin.'
                      }
                    />
                  </td>
                </tr>
              ) : (
                rows.map((row, i) => {
                  const userId = Number(getField(row, 'UserID') ?? 0);
                  const owed = Number(getField(row, 'Outstanding') ?? 0);
                  const limit = Number(getField(row, 'MaxOutstanding') ?? 0);
                  const available = Number(getField(row, 'AvailableCredit') ?? 0);
                  // Over the limit is not an error state — an admin lowering a limit below an
                  // existing balance is a normal way to stop further credit — but it is the thing
                  // the person reading this page most needs to spot.
                  const overLimit = limit > 0 && owed > limit;

                  return (
                    <tr key={userId || i} className="border-b border-slate-100 transition-colors last:border-0 hover:bg-slate-50/60">
                      <td className="whitespace-nowrap px-4 py-3 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          <IconBtn title="View statement" onClick={() => setViewing(row)}>
                            <Eye className="size-3.5" />
                          </IconBtn>
                          <IconBtn title="Record a collection" onClick={() => setCollecting(row)} disabled={owed <= 0}>
                            <Plus className="size-3.5" />
                          </IconBtn>
                          <IconBtn
                            title="Send a reminder now"
                            onClick={() => remind.mutate(userId)}
                            disabled={owed <= 0 || remind.isPending}
                          >
                            <MessageSquare className="size-3.5" />
                          </IconBtn>
                          {isAdmin && (
                            <IconBtn title="Set credit limit" onClick={() => setLimiting(row)}>
                              <ShieldCheck className="size-3.5" />
                            </IconBtn>
                          )}
                        </div>
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-400">{i + 1}</td>
                      <td className="px-4 py-3">
                        <div className="font-medium text-slate-800">{String(getField(row, 'FirmName') || getField(row, 'UserName') || '—')}</div>
                        <div className="text-xs text-slate-500">
                          {String(getField(row, 'MobileNumber') ?? '')}
                          {getField(row, 'UserType') ? ` · ${String(getField(row, 'UserType'))}` : ''}
                        </div>
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{String(getField(row, 'ParentName') || '—')}</td>
                      <td className={cn(
                        'whitespace-nowrap px-4 py-3 text-right font-semibold tabular-nums',
                        owed > 0 ? 'text-warning-600' : 'text-slate-400',
                      )}>
                        {inr(owed)}
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-right tabular-nums text-slate-600">
                        {limit > 0 ? inr(limit) : <span className="text-slate-400">No credit</span>}
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-right tabular-nums">
                        {overLimit
                          ? <span className="rounded-full bg-danger-50 px-2 py-0.5 text-[11px] font-medium text-danger-700">Over limit</span>
                          : <span className="text-slate-600">{inr(available)}</span>}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </Card>
      )}

      {/* The statement — "kab liya, kaise liya, kab diya". */}
      <Modal
        open={Boolean(viewing)}
        onClose={() => setViewing(null)}
        title={`Outstanding statement — ${String(getField(viewing ?? {}, 'FirmName') || getField(viewing ?? {}, 'UserName') || '')}`}
        size="xl"
      >
        {viewing && <OutstandingStatement userId={Number(getField(viewing, 'UserID') ?? 0)} />}
      </Modal>

      {collecting && (
        <CollectionModal
          row={collecting}
          onClose={() => setCollecting(null)}
          onDone={() => { setCollecting(null); refreshAll(); }}
        />
      )}

      {limiting && (
        <CreditLimitModal
          row={limiting}
          defaultLimit={defaultLimit}
          onClose={() => setLimiting(null)}
          onDone={() => { setLimiting(null); refreshAll(); }}
        />
      )}
    </>
  );
}

/**
 * Recording a payment, laid out like the fund-request form because it is the same act from the
 * other side: money changed hands and the books have to say so.
 *
 * The MPIN is required by the server, not decoration — it is what makes this the collector's own
 * act rather than a session someone walked up to at a busy counter.
 */
function CollectionModal({ row, onClose, onDone }: { row: Row; onClose: () => void; onDone: () => void }) {
  const toast = useToast();
  const userId = Number(getField(row, 'UserID') ?? 0);
  const owed = Number(getField(row, 'Outstanding') ?? 0);

  const [amount, setAmount] = useState('');
  const [payMode, setPayMode] = useState('Cash');
  const [utr, setUtr] = useState('');
  const [remark, setRemark] = useState('');
  const [pin, setPin] = useState('');

  // The MPIN is asked for only when the install (Key Settings) or this user requires one; the
  // server applies the same rule, so an install with MPIN off never sees a PIN box it cannot fill.
  const { data: mpin } = useQuery({ queryKey: ['mpin-status'], queryFn: getMpinStatus, staleTime: 60_000 });
  const pinRequired = Boolean(mpin?.Required);

  const amountNum = Number(amount) || 0;
  // Collecting more than is owed would make the balance an advance payment — something nothing
  // downstream represents — so the server refuses it. Saying so here saves a round trip and
  // explains the rule while the number is still being typed.
  const tooMuch = amountNum > owed;

  const save = useMutation({
    mutationFn: () => apiPost('CashCollection/Add', {
      ReceiverID: userId,
      Amount: amountNum,
      PayMode: payMode,
      UTRNumber: utr,
      Remark: remark,
      Pin: pin,
    }),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message || 'The collection could not be recorded.'); return; }
      toast.success('Collection recorded.');
      onDone();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const canSave = amountNum > 0 && !tooMuch && (!pinRequired || pin.length > 0) && !save.isPending;

  return (
    <Modal open onClose={onClose} title={`Collect from ${String(getField(row, 'FirmName') || getField(row, 'UserName') || '')}`} size="md">
      <div className="space-y-4">
        <div className="rounded-xl bg-slate-50 px-4 py-3 text-sm">
          <div className="flex items-center justify-between">
            <span className="text-slate-500">Currently outstanding</span>
            <span className="font-semibold tabular-nums text-warning-600">{inr(owed)}</span>
          </div>
          {amountNum > 0 && !tooMuch && (
            <div className="mt-1.5 flex items-center justify-between border-t border-slate-200 pt-1.5">
              <span className="text-slate-500">After this collection</span>
              <span className="font-semibold tabular-nums text-slate-800">{inr(owed - amountNum)}</span>
            </div>
          )}
        </div>

        <Input
          label="Amount (₹)"
          type="number"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          error={tooMuch ? `More than the outstanding of ${inr(owed)}.` : undefined}
          autoFocus
        />

        <Select label="Received by" value={payMode} onChange={(e) => setPayMode(e.target.value)}>
          <option value="Cash">Cash</option>
          <option value="Bank">Bank transfer</option>
          <option value="UPI">UPI</option>
          <option value="Cheque">Cheque</option>
          <option value="Other">Other</option>
        </Select>

        <Input
          label="UTR / reference"
          value={utr}
          onChange={(e) => setUtr(e.target.value)}
          hint="Optional for cash; worth recording for anything that went through a bank."
        />

        <Input label="Remark" value={remark} onChange={(e) => setRemark(e.target.value)} />

        {pinRequired && (
          <Input
            label="MPIN"
            type="password"
            value={pin}
            onChange={(e) => setPin(e.target.value)}
            hint="Confirms this collection is yours."
          />
        )}

        <div className="flex justify-end gap-2 pt-1">
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button onClick={() => save.mutate()} disabled={!canSave} loading={save.isPending}>
            Record collection
          </Button>
        </div>
      </div>
    </Modal>
  );
}

/**
 * The credit limit — admin only, and the one real control on this feature.
 *
 * It decides two things: whether the Credit payment mode is offered to this user at all (a limit
 * of zero hides it), and the ceiling the posting refuses to go past when a credit request is
 * approved.
 */
function CreditLimitModal({
  row, defaultLimit, onClose, onDone,
}: { row: Row; defaultLimit: number; onClose: () => void; onDone: () => void }) {
  const toast = useToast();
  // Opened from a row, the user is fixed. Opened from the header, it has to be chosen — that is
  // the only way to give a limit to an account that has never had one.
  const rowUserId = Number(getField(row, 'UserID') ?? 0);
  const [pickedUserId, setPickedUserId] = useState('');
  const userId = rowUserId > 0 ? rowUserId : Number(pickedUserId) || 0;
  const owed = Number(getField(row, 'Outstanding') ?? 0);
  // Pre-filled with whatever is in force for this account — its own limit, or the network default
  // when it has never been given one. Typing over a figure is a smaller ask than finding out what
  // the current one is first.
  const existing = Number(getField(row, 'MaxOutstanding') ?? 0);
  const [limit, setLimit] = useState(String(existing > 0 ? existing : defaultLimit || 0));
  const usesDefault = getField(row, 'IsDefaultLimit') === true;

  const { data: users = [] } = useQuery({
    queryKey: ['outstanding-limit-users'],
    enabled: rowUserId <= 0,
    staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0 }),
  });

  const userOpts: AutocompleteOption[] = useMemo(
    () => users.map((u, i) => ({
      value: String(getField(u, 'UserMasterID') ?? i),
      label: String(getField(u, 'FirmName') || getField(u, 'PersonName') || ''),
      sublabel: [getField(u, 'UserType'), getField(u, 'MobileNumber')].filter(Boolean).map(String).join(' · '),
    })),
    [users],
  );

  const limitNum = Number(limit) || 0;

  const save = useMutation({
    mutationFn: () => apiPost('CashCollection/SetLimit', { UserID: userId, MaxOutstanding: limitNum }),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message || 'The credit limit could not be saved.'); return; }
      toast.success('Credit limit saved.');
      onDone();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Hands the account back to the network default. Without it, setting a limit once is
  // irreversible — the only way back would be to guess today's default and type it, which stops
  // being right the moment the default changes.
  const useDefault = useMutation({
    mutationFn: () => apiPost('CashCollection/ClearLimit', { UserID: userId, MaxOutstanding: 0 }),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message || 'Could not switch this account to the default.'); return; }
      toast.success('This account now uses the network default.');
      onDone();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Modal
      open
      onClose={onClose}
      title={rowUserId > 0
        ? `Credit limit — ${String(getField(row, 'FirmName') || getField(row, 'UserName') || '')}`
        : 'Set a credit limit'}
      size="sm"
    >
      <div className="space-y-4">
        {rowUserId <= 0 && (
          <Autocomplete
            label="User"
            options={userOpts}
            value={pickedUserId}
            onChange={(v) => setPickedUserId(String(v || ''))}
          />
        )}
        <Input
          label="Credit limit (₹)"
          type="number"
          value={limit}
          onChange={(e) => setLimit(e.target.value)}
          hint={
            defaultLimit > 0
              ? `Saving a figure here overrides the network default of ${inr(defaultLimit)} for this account. Zero means no credit at all for them, even while the default is on.`
              : 'Zero means this account cannot take funds on credit. There is no network default set, so every account needs its own limit.'
          }
          autoFocus
        />

        {usesDefault && defaultLimit > 0 && (
          <p className="rounded-lg bg-info-50 px-3 py-2 text-xs text-info-700">
            This account is currently on the network default of {inr(defaultLimit)}. Nobody has set a
            limit for it specifically.
          </p>
        )}

        {limitNum > 0 && limitNum < owed && (
          <p className="rounded-lg bg-warning-50 px-3 py-2 text-xs text-warning-700">
            This is below the {inr(owed)} already outstanding. That is allowed and changes nothing
            they already owe — it simply stops them taking any more on credit.
          </p>
        )}

        <div className="flex items-center justify-between gap-2 pt-1">
          {!usesDefault && userId > 0 && defaultLimit > 0 ? (
            <Button
              variant="outline"
              onClick={() => useDefault.mutate()}
              loading={useDefault.isPending}
            >
              Use network default
            </Button>
          ) : <span />}
          <div className="flex gap-2">
            <Button variant="outline" onClick={onClose}>Cancel</Button>
            <Button onClick={() => save.mutate()} loading={save.isPending} disabled={userId <= 0}>
              Save limit
            </Button>
          </div>
        </div>
      </div>
    </Modal>
  );
}

function Metric({ label, value, accent }: { label: string; value: string; accent?: boolean }) {
  return (
    <div>
      <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{label}</div>
      <div className={cn('text-sm font-semibold tabular-nums', accent ? 'text-warning-600' : 'text-slate-800')}>{value}</div>
    </div>
  );
}

function IconBtn({
  title, onClick, disabled, children,
}: { title: string; onClick: () => void; disabled?: boolean; children: React.ReactNode }) {
  return (
    <button
      type="button"
      title={title}
      aria-label={title}
      onClick={onClick}
      disabled={disabled}
      className={cn(
        'inline-flex size-7 items-center justify-center rounded-lg border transition-colors',
        disabled
          ? 'cursor-not-allowed border-slate-100 text-slate-300'
          : 'border-slate-200 text-slate-600 hover:bg-slate-50 hover:text-brand-600',
      )}
    >
      {children}
    </button>
  );
}
