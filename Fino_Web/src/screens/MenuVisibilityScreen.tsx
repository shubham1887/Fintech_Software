import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Eye, EyeOff, Info, RefreshCw, Save, Search as SearchIcon } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { cn } from '@/lib/cn';
import { useAuth } from '@/features/auth/AuthContext';
import { UserTypeCode, roleLabel } from '@/lib/roles';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface Item {
  MenuKey: string;
  MenuName: string;
  GroupName: string;
  Url: string;
  IsGroup: boolean;
  UserTypeID: number;
  IsVisible: boolean;
  HasOverride: boolean;
  DefinedForRoles: number[];
}

/** The scopes a decision can be made at. 0 is the install — the one most admins ever need. */
const SCOPES: { value: number; label: string }[] = [
  { value: 0, label: 'Everyone (whole install)' },
  { value: UserTypeCode.Admin, label: roleLabel(UserTypeCode.Admin) },
  { value: UserTypeCode.SuperDistributor, label: roleLabel(UserTypeCode.SuperDistributor) },
  { value: UserTypeCode.Distributor, label: roleLabel(UserTypeCode.Distributor) },
  { value: UserTypeCode.Retailer, label: roleLabel(UserTypeCode.Retailer) },
  { value: UserTypeCode.ApiUser, label: roleLabel(UserTypeCode.ApiUser) },
  { value: UserTypeCode.Accountant, label: roleLabel(UserTypeCode.Accountant) },
  { value: UserTypeCode.SupportUser, label: roleLabel(UserTypeCode.SupportUser) },
];

/**
 * Menu Visibility — which of the product's screens this install puts on the sidebar.
 *
 * The product ships with every screen it has. Any one network uses a fraction of them, and the
 * operator scrolling a thirty-row Reports group to reach the two they use is exactly the
 * complaint. Rather than deleting rows from a definition the next install needs, the decision is
 * stored: switch an entry off and it leaves the menu.
 *
 * Two things this screen is careful to say out loud, because getting either wrong is expensive:
 *
 *  1. It changes the MENU, not permissions. The endpoint behind a hidden screen is exactly as
 *     reachable as before, and who may call it is still decided by the server's access policy.
 *     A menu switch that quietly became a security control would be a control nobody audits.
 *  2. Nothing here is stored until Save. An entry with no decision is simply behaving as the
 *     product ships it, and the screen says "As shipped" rather than implying somebody chose it.
 */
export function MenuVisibilityScreen({ def }: CustomScreenProps) {
  const qc = useQueryClient();
  const toast = useToast();
  const { user } = useAuth();
  // Admin and support both. Support is admin-equivalent everywhere a mistake can be undone,
  // and hiding a menu row is the definition of reversible — the server resolves this controller
  // the same way, so the button and the endpoint agree.
  const roleId = Number(user?.UserTypeID ?? 0);
  const mayEdit = roleId === UserTypeCode.Admin || roleId === UserTypeCode.SupportUser;

  const [scope, setScope] = useState(0);
  const [search, setSearch] = useState('');
  /** Unsaved edits, keyed by MenuKey. Empty means nothing has been touched. */
  const [draft, setDraft] = useState<Record<string, boolean>>({});

  const { data, isLoading, isFetching, isError, error } = useQuery<{ data: Item[] }>({
    queryKey: ['menu-visibility', scope],
    queryFn: async () => {
      const res = await apiPost<{ data: Item[] }>('MenuVisibility/Select', { UserTypeID: scope });
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not load the menu.');
      return res.data;
    },
  });

  // Switching scope abandons edits rather than carrying them to a different role, which would
  // apply a decision to a scope nobody chose it for.
  useEffect(() => { setDraft({}); }, [scope]);

  const items = data?.data ?? [];

  const visibleOf = (it: Item) => draft[it.MenuKey] ?? it.IsVisible;

  const groups = useMemo(() => {
    const q = search.trim().toLowerCase();
    const match = (it: Item) =>
      !q || it.MenuName.toLowerCase().includes(q) || it.Url.toLowerCase().includes(q);

    // Top-level entries without children come first under a heading of their own, then each
    // group with the leaves inside it. The order is the menu's own, which is how the admin
    // recognises the thing they are switching off.
    const out: { name: string; groupKey: string | null; rows: Item[] }[] = [];
    const standalone: Item[] = [];

    for (const it of items) {
      if (it.GroupName) continue;
      if (it.IsGroup) {
        const rows = items.filter((c) => c.GroupName === it.MenuName && match(c));
        if (rows.length > 0 || match(it)) out.push({ name: it.MenuName, groupKey: it.MenuKey, rows });
      } else if (match(it)) standalone.push(it);
    }

    if (standalone.length > 0) out.unshift({ name: 'Top level', groupKey: null, rows: standalone });
    return out;
  }, [items, search]);

  const dirty = Object.keys(draft).length;

  const save = useMutation({
    mutationFn: async () => {
      const payload = items
        .filter((it) => it.MenuKey in draft)
        .map((it) => ({
          MenuKey: it.MenuKey,
          MenuName: it.MenuName,
          UserTypeID: scope,
          IsVisible: draft[it.MenuKey],
        }));
      const res = await apiPost<{ Changed: number }>('MenuVisibility/Save', { Items: payload });
      if (!res.ok) throw new Error(res.message || 'Could not save.');
      return res.data;
    },
    onSuccess: () => {
      setDraft({});
      toast.success('Menu updated. Users see the change the next time their menu loads.');
      qc.invalidateQueries({ queryKey: ['menu-visibility'] });
      // The signed-in admin's own sidebar is the first place they will look.
      qc.invalidateQueries({ queryKey: ['menu'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <div className="flex items-center gap-2">
            <Button variant="ghost" onClick={() => qc.invalidateQueries({ queryKey: ['menu-visibility'] })}>
              <RefreshCw className={cn('size-4', isFetching && 'animate-spin')} />
              Refresh
            </Button>
            {mayEdit && (
              <Button onClick={() => save.mutate()} disabled={dirty === 0 || save.isPending}>
                <Save className="size-4" />
                {dirty === 0 ? 'Save' : `Save ${dirty} change${dirty === 1 ? '' : 's'}`}
              </Button>
            )}
          </div>
        }
      />

      <Card className="mb-4 border-info-200 bg-info-50/40">
        <CardBody className="flex gap-2 p-3 text-sm text-slate-600">
          <Info className="mt-0.5 size-4 shrink-0 text-info-600" />
          <p>
            Switching an entry off removes it from the sidebar — it does not change who is allowed
            to use it. Access is decided by the server's access policy and is unaffected by
            anything on this page. Entries that are off for this install can be turned back on at
            any time; nothing is deleted.
          </p>
        </CardBody>
      </Card>

      <Card>
        <CardBody className="flex flex-wrap items-end gap-3 border-b border-slate-100 p-4">
          <div className="w-64">
            <label className="mb-1 block text-xs font-medium text-slate-500">Applies to</label>
            <Select value={String(scope)} onChange={(e) => setScope(Number(e.target.value))}>
              {SCOPES.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
            </Select>
          </div>
          <div className="min-w-[220px] flex-1">
            <label className="mb-1 block text-xs font-medium text-slate-500">Find a screen</label>
            <div className="relative">
              <SearchIcon className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <Input value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" placeholder="Report name or route" />
            </div>
          </div>
          {scope > 0 && (
            <p className="text-xs text-slate-500">
              A decision for one role overrides the install-wide one — so an entry can be off for
              everyone and still on here.
            </p>
          )}
        </CardBody>

        <CardBody className="p-0">
          {isError ? (
            <div className="flex items-center gap-2 p-6 text-sm text-warning-700">
              <AlertTriangle className="size-4 shrink-0" />
              {(error as Error)?.message}
            </div>
          ) : isLoading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 8 }).map((_, i) => <Skeleton key={i} className="h-9 rounded-lg" />)}
            </div>
          ) : groups.length === 0 ? (
            <EmptyState icon={SearchIcon} title="Nothing matches" description="No menu entry matches that search." />
          ) : (
            <div className="divide-y divide-slate-100">
              {groups.map((g) => {
                const groupItem = g.groupKey ? items.find((i) => i.MenuKey === g.groupKey) : undefined;
                const groupOn = groupItem ? visibleOf(groupItem) : true;
                return (
                  <div key={g.name}>
                    <div className="flex items-center justify-between gap-3 bg-slate-50 px-4 py-2">
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-semibold uppercase tracking-wide text-slate-500">{g.name}</span>
                        {groupItem && !groupOn && <Badge tone="neutral">Group hidden</Badge>}
                      </div>
                      {groupItem && (
                        <Toggle
                          on={groupOn}
                          disabled={!mayEdit}
                          label="Whole group"
                          onChange={(v) => setDraft((d) => ({ ...d, [groupItem.MenuKey]: v }))}
                        />
                      )}
                    </div>
                    {g.rows.map((it) => {
                      const on = visibleOf(it);
                      // A leaf inside a hidden group cannot appear whatever its own switch says.
                      const suppressed = groupItem !== undefined && !groupOn;
                      return (
                        <div
                          key={it.MenuKey}
                          className={cn(
                            'flex items-center justify-between gap-3 px-4 py-2.5',
                            it.MenuKey in draft && 'bg-brand-50/50',
                          )}
                        >
                          <div className="min-w-0">
                            <p className={cn('truncate text-sm', on && !suppressed ? 'text-slate-800' : 'text-slate-400')}>
                              {it.MenuName}
                            </p>
                            <p className="truncate text-xs text-slate-400">
                              {it.Url || '—'}
                              {!it.HasOverride && <span className="ml-2 text-slate-300">As shipped</span>}
                              {suppressed && <span className="ml-2 text-slate-400">Hidden with its group</span>}
                            </p>
                          </div>
                          <Toggle
                            on={on}
                            disabled={!mayEdit}
                            onChange={(v) => setDraft((d) => ({ ...d, [it.MenuKey]: v }))}
                          />
                        </div>
                      );
                    })}
                  </div>
                );
              })}
            </div>
          )}
        </CardBody>
      </Card>

      {!mayEdit && (
        <p className="mt-3 text-xs text-slate-400">
          You can read this list. Changing it is the administrator's or support's — the server
          refuses the save for every other role.
        </p>
      )}
    </>
  );
}

function Toggle({
  on, onChange, disabled, label,
}: { on: boolean; onChange: (v: boolean) => void; disabled?: boolean; label?: string }) {
  return (
    <button
      type="button"
      disabled={disabled}
      onClick={() => onChange(!on)}
      title={label ? `${label} — ${on ? 'shown' : 'hidden'}` : on ? 'Shown' : 'Hidden'}
      className={cn(
        'inline-flex shrink-0 items-center gap-1.5 rounded-lg border px-2.5 py-1 text-xs font-medium transition-colors',
        on
          ? 'border-success-500/25 bg-success-50 text-success-700 hover:bg-success-100'
          : 'border-slate-200 bg-slate-50 text-slate-500 hover:bg-slate-100',
        disabled && 'cursor-not-allowed opacity-60',
      )}
    >
      {on ? <Eye className="size-3.5" /> : <EyeOff className="size-3.5" />}
      {on ? 'Shown' : 'Hidden'}
    </button>
  );
}
