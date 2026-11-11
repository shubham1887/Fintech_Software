import { useMemo, useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Check, Network, Users as UsersIcon } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField as get } from '@/lib/row';
import { loadSession } from '@/lib/storage';
import { cn } from '@/lib/cn';
import { UserTypeCode, roleLabel } from '@/lib/roles';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
// No IsActive here, deliberately. Users/Select treats IsActive as a TRI-STATE: present-and-false
// means "deactivated users only", which is what the Inactive Users screen wants and the exact
// opposite of what this screen wants. Sending false made the pick list show only deactivated
// accounts — so on a healthy network it was empty and the whole screen read as broken. Omitting
// the key leaves it null, which means "any status": the point of this screen is to activate and
// deactivate, so both have to be on the list.
const USER_PAGE = { PageNumber: 1, PageSize: 1000, TakeCount: 1000, SkipCount: 0 };

const ROLE_OPTIONS = [
  { label: 'Any role', value: 0 },
  { label: 'Super Distributor', value: UserTypeCode.SuperDistributor },
  { label: 'Distributor', value: UserTypeCode.Distributor },
  { label: 'Retailer', value: UserTypeCode.Retailer },
  { label: 'API User', value: UserTypeCode.ApiUser },
];

const ACTIONS = [
  { value: 'Shift', label: 'Shift under a new parent', destructive: false },
  { value: 'Activate', label: 'Activate', destructive: false },
  { value: 'Deactivate', label: 'Deactivate', destructive: true },
  { value: 'HoldCommission', label: 'Hold commission', destructive: true },
  { value: 'ReleaseCommission', label: 'Release commission', destructive: false },
];

const name = (r: Row) => String(get(r, 'FirmName') || get(r, 'PersonName') || `User ${get(r, 'UserMasterID')}`);

/**
 * Userwise Bulk Action.
 *
 * Two things were wrong with the form this replaces. It posted a role and an action to
 * Users/BulkUpdate, which reads neither — the endpoint wants the user ids, so the form could never
 * do anything at all. And the action people actually need here is SHIFTING: moving a user, or a
 * whole branch of the network, under a different parent. That is why targets can be chosen two
 * ways — tick individual users, or name an upline and take everyone below it (optionally narrowed
 * to one role), which is the difference between re-parenting forty retailers in one action and
 * doing it forty times.
 *
 * The tree rules are enforced server-side (no user under itself or its own descendant, no parent of
 * an equal or lower role, nothing outside your own network); a target that breaks one is reported
 * by name and the rest still go through.
 */
export function BulkActionScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const myTypeId = Number(loadSession()?.user?.UserTypeID ?? 0);
  const isAdmin = myTypeId === UserTypeCode.Admin || myTypeId === UserTypeCode.SupportUser;

  const [mode, setMode] = useState<'pick' | 'downline'>('pick');
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [sourceUserId, setSourceUserId] = useState('');
  const [includeSource, setIncludeSource] = useState(false);
  const [roleFilter, setRoleFilter] = useState(0);
  const [action, setAction] = useState('Shift');
  const [newParentId, setNewParentId] = useState('');
  const [remark, setRemark] = useState('');
  const [search, setSearch] = useState('');

  const { data: users = [], isFetching } = useQuery({
    queryKey: ['bulk-action-users'], staleTime: 300_000,
    queryFn: () => fetchList('Users/Select', USER_PAGE),
  });

  const userOptions = useMemo<AutocompleteOption[]>(
    () => users.map((u, i) => ({
      value: String(get(u, 'UserMasterID') ?? i),
      label: name(u),
      sublabel: [String(get(u, 'UserType') ?? ''), String(get(u, 'MobileNumber') ?? '')].filter(Boolean).join(' · '),
    })),
    [users],
  );

  // The pick list: role filter + text search, so a large network stays navigable.
  const pickable = useMemo(() => {
    const q = search.trim().toLowerCase();
    return users.filter((u) => {
      if (roleFilter > 0 && Number(get(u, 'UserTypeID')) !== roleFilter) return false;
      if (!q) return true;
      return [name(u), String(get(u, 'MobileNumber') ?? ''), String(get(u, 'AliasName') ?? '')]
        .some((v) => v.toLowerCase().includes(q));
    });
  }, [users, roleFilter, search]);

  const chosenAction = ACTIONS.find((a) => a.value === action);
  const isShift = action === 'Shift';
  const targetCount = mode === 'pick' ? selected.size : (sourceUserId ? 1 : 0);

  const mut = useMutation({
    mutationFn: () => apiPost('Users/BulkUpdate', {
      UserIDs: mode === 'pick' ? [...selected] : [],
      SourceUserID: mode === 'downline' ? Number(sourceUserId) || 0 : 0,
      IncludeSourceUser: mode === 'downline' && includeSource,
      UserTypeID: mode === 'downline' ? roleFilter : 0,
      Action: action,
      NewParentID: isShift ? Number(newParentId) || 0 : 0,
      Remark: remark.trim(),
    }),
    onSuccess: (res) => {
      if (!res.ok) return toast.error(res.message);
      // The API replies with what it changed and what it skipped — show that, not a generic "done".
      toast.success(res.message || 'Bulk action applied.');
      setSelected(new Set());
    },
    onError: (e: Error) => toast.error(e.message),
  });

  function submit() {
    if (mode === 'pick' && selected.size === 0) return toast.error('Tick at least one user.');
    if (mode === 'downline' && !sourceUserId) return toast.error('Choose whose downline to act on.');
    if (isShift && !newParentId) return toast.error('Choose the user to move them under.');
    if (!remark.trim()) return toast.error('Add a remark — bulk changes should be explainable.');
    mut.mutate();
  }

  const toggle = (id: number) => setSelected((prev) => {
    const next = new Set(prev);
    if (next.has(id)) next.delete(id); else next.add(id);
    return next;
  });

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      <Card className="mb-4">
        <CardBody className="space-y-4">
          {/* Who to act on */}
          <div>
            <p className="mb-2 text-sm font-medium text-slate-700">1. Choose who</p>
            <div className="flex gap-1 rounded-lg bg-slate-100 p-1">
              {[
                { key: 'pick' as const, label: 'Pick users', icon: UsersIcon },
                { key: 'downline' as const, label: "Everyone under a user", icon: Network },
              ].map(({ key, label, icon: Icon }) => (
                <button
                  key={key}
                  type="button"
                  onClick={() => setMode(key)}
                  className={cn(
                    'flex flex-1 items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
                    mode === key ? 'bg-white text-slate-800 shadow-sm' : 'text-slate-500 hover:text-slate-700',
                  )}
                >
                  <Icon className="size-3.5" />
                  {label}
                </button>
              ))}
            </div>
          </div>

          {mode === 'downline' ? (
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Autocomplete
                label="Whose downline"
                placeholder="Search by name…"
                options={userOptions}
                value={sourceUserId}
                onChange={(v) => setSourceUserId(String(v))}
              />
              <Select label="Limit to role" value={String(roleFilter)} onChange={(e) => setRoleFilter(Number(e.target.value))}>
                {ROLE_OPTIONS.map((r) => <option key={r.value} value={r.value}>{r.label}</option>)}
              </Select>
              <label className="flex items-end gap-2 pb-2 text-sm text-slate-600">
                <input
                  type="checkbox"
                  checked={includeSource}
                  onChange={(e) => setIncludeSource(e.target.checked)}
                  className="size-4 accent-brand-600"
                />
                Include that user too
              </label>
              <p className="text-xs text-slate-400 sm:col-span-3">
                Takes everyone below that user, however deep — not just their direct children.
              </p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <Select label="Role" value={String(roleFilter)} onChange={(e) => setRoleFilter(Number(e.target.value))}>
                  {ROLE_OPTIONS.map((r) => <option key={r.value} value={r.value}>{r.label}</option>)}
                </Select>
                <Input label="Search" placeholder="Name / mobile / code…" value={search} onChange={(e) => setSearch(e.target.value)} />
              </div>

              <div className="max-h-72 overflow-auto rounded-lg border border-slate-200">
                {isFetching ? (
                  <div className="space-y-2 p-3">{Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-8 rounded" />)}</div>
                ) : pickable.length === 0 ? (
                  <EmptyState title="No users" description="Nothing matches this role or search." />
                ) : (
                  pickable.map((u, i) => {
                    const id = Number(get(u, 'UserMasterID') ?? 0);
                    const on = selected.has(id);
                    return (
                      <button
                        key={id || i}
                        type="button"
                        onClick={() => toggle(id)}
                        className={cn(
                          'flex w-full items-center gap-3 border-b border-slate-100 px-3 py-2 text-left text-sm transition-colors last:border-0',
                          on ? 'bg-brand-50/60' : 'hover:bg-slate-50',
                        )}
                      >
                        <span className={cn(
                          'grid size-4 shrink-0 place-items-center rounded border',
                          on ? 'border-brand-500 bg-brand-500 text-white' : 'border-slate-300',
                        )}>
                          {on && <Check className="size-3" />}
                        </span>
                        <span className="min-w-0 flex-1 truncate font-medium text-slate-700">{name(u)}</span>
                        <span className="shrink-0 text-xs text-slate-400">
                          {roleLabel(Number(get(u, 'UserTypeID') ?? 0), String(get(u, 'UserType') ?? ''))}
                        </span>
                        <span className="shrink-0 tabular-nums text-xs text-slate-400">{String(get(u, 'MobileNumber') ?? '')}</span>
                      </button>
                    );
                  })
                )}
              </div>
              <p className="text-xs text-slate-500">
                <span className="font-semibold tabular-nums text-slate-700">{selected.size}</span> selected
                {selected.size > 0 && (
                  <button type="button" onClick={() => setSelected(new Set())} className="ml-2 text-brand-600 hover:underline">
                    clear
                  </button>
                )}
              </p>
            </>
          )}
        </CardBody>
      </Card>

      <Card>
        <CardBody className="space-y-4">
          <p className="text-sm font-medium text-slate-700">2. Choose what to do</p>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Select label="Action" value={action} onChange={(e) => setAction(e.target.value)}>
              {ACTIONS.map((a) => <option key={a.value} value={a.value}>{a.label}</option>)}
            </Select>
            {isShift && (
              <Autocomplete
                label="Move under"
                placeholder="Search by name…"
                options={userOptions}
                value={newParentId}
                onChange={(v) => setNewParentId(String(v))}
              />
            )}
            <Input
              label="Remark (required)"
              placeholder="Why is this being done?"
              value={remark}
              onChange={(e) => setRemark(e.target.value)}
              className={isShift ? undefined : 'sm:col-span-1'}
            />
          </div>

          {isShift && (
            <p className="rounded-lg bg-slate-50 p-3 text-sm text-slate-600">
              Moving a user re-parents their whole branch with them. A user cannot be placed under
              itself or under anyone already inside its own downline, and a parent must be a higher
              role than the user{isAdmin ? '' : ' — and inside your own network'}. Anything that
              breaks those rules is skipped and named in the result.
            </p>
          )}

          <div className="flex items-center justify-end gap-3">
            <span className="text-xs text-slate-400">
              {mode === 'pick'
                ? `${targetCount} user(s) selected`
                : sourceUserId ? 'Applies to the whole downline' : 'No user chosen'}
            </span>
            <Button
              variant={chosenAction?.destructive ? 'danger' : 'primary'}
              loading={mut.isPending}
              onClick={submit}
            >
              Apply
            </Button>
          </div>
        </CardBody>
      </Card>
    </>
  );
}
