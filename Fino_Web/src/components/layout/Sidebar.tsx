import { useEffect, useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ChevronRight } from 'lucide-react';
import { getMenu, type MenuNode } from '@/api/menu.api';
import { useSubscriptionStatus } from '@/features/subscription/useSubscriptionStatus';
import { cn } from '@/lib/cn';
import { pickIcon } from './iconMap';
import { resolveHref, isActiveHref } from './resolveRoute';
import { BrandLockup } from '@/components/ui/BrandMark';

/** True when this node (or any descendant) maps to the currently-open route. */
function containsActivePath(node: MenuNode, pathname: string): boolean {
  const children = node.children;
  if (Array.isArray(children) && children.length > 0) {
    return children.some((c) => containsActivePath(c, pathname));
  }
  const href = resolveHref(node);
  // Ignore query strings (coming-soon links) — match on the path only.
  return href.split('?')[0] === pathname;
}

const FALLBACK_MENU: MenuNode[] = [
  { name: 'Dashboard', icon: 'dashboard' },
  { name: 'Recharge', icon: 'recharge' },
  { name: 'Transactions', icon: 'transaction' },
];

// Subscription menu leaves — hidden everywhere while the feature (Key Setting) is off.
const SUBSCRIPTION_URLS = new Set(['/createplanindex', '/servicesubscriptioncharge']);

/** Drop subscription leaves (and any group they leave empty) when the feature is disabled. */
function pruneSubscription(nodes: MenuNode[], enabled: boolean): MenuNode[] {
  if (enabled) return nodes;
  return nodes
    .map((n) => (n.children ? { ...n, children: pruneSubscription(n.children, enabled) } : n))
    .filter((n) => {
      const url = n.url?.toLowerCase();
      if (url && SUBSCRIPTION_URLS.has(url)) return false;
      if (Array.isArray(n.children) && n.children.length === 0 && !n.url) return false;
      return true;
    });
}

export function Sidebar({ onNavigate }: { onNavigate?: () => void }) {
  const { pathname } = useLocation();
  const { data, isLoading, isError } = useQuery({
    queryKey: ['menu'],
    queryFn: getMenu,
    staleTime: 5 * 60_000,
  });

  const subEnabled = useSubscriptionStatus()?.Enabled === true;
  const baseMenu = data && data.length > 0 ? data : isLoading ? [] : FALLBACK_MENU;
  const menu = pruneSubscription(baseMenu, subEnabled);

  // Only one top-level group is open at a time (accordion). `openKey` = that group's name.
  const [openKey, setOpenKey] = useState<string | null>(null);

  // Navigating opens the group that owns the active route and closes the others.
  useEffect(() => {
    const active = menu.find(
      (n) => Array.isArray(n.children) && n.children.length > 0 && containsActivePath(n, pathname),
    );
    if (active) setOpenKey(active.name);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pathname, data]);

  return (
    <aside className="flex h-full w-72 flex-col text-ink-200" style={{ backgroundColor: 'var(--ui-navbar)' }}>
      {/* 76px, matching the app bar on the right. On a 3.5:1 wordmark the twelve pixels bought
          over a 64px bar are forty-odd pixels of width, which is what fills the plate. */}
      <div className="flex h-[4.75rem] shrink-0 items-center border-b border-white/5 px-3">
        <BrandLockup tone="dark" size="md" fill />
      </div>

      <nav className="no-scrollbar flex-1 space-y-1 overflow-y-auto px-3 py-4">
        {isLoading ? (
          <div className="space-y-2 px-2">
            {Array.from({ length: 8 }).map((_, i) => (
              <div key={i} className="h-9 animate-pulse rounded-lg bg-white/5" />
            ))}
          </div>
        ) : (
          menu.map((node, i) => (
            <MenuEntry
              key={`${node.name}-${i}`}
              node={node}
              depth={0}
              onNavigate={onNavigate}
              openKey={openKey}
              setOpenKey={setOpenKey}
            />
          ))
        )}
        {isError && (
          <p className="px-3 pt-2 text-xs text-ink-400">
            Showing offline menu — sign-in session may have expired.
          </p>
        )}
      </nav>
    </aside>
  );
}

function MenuEntry({
  node,
  depth,
  onNavigate,
  openKey,
  setOpenKey,
}: {
  node: MenuNode;
  depth: number;
  onNavigate?: () => void;
  /** Single-open accordion control (top-level groups only). */
  openKey?: string | null;
  setOpenKey?: (key: string | null) => void;
}) {
  const { pathname, search } = useLocation();
  const hasChildren = Array.isArray(node.children) && node.children.length > 0;
  const isActiveGroup = hasChildren && containsActivePath(node, pathname);

  // Top-level groups are controlled by the shared `openKey` (one open at a time). Nested
  // sub-groups keep their own local open/closed state.
  const controlled = depth === 0 && !!setOpenKey;
  const [localOpen, setLocalOpen] = useState(isActiveGroup);
  const open = controlled ? openKey === node.name : localOpen;
  const toggle = () => {
    if (controlled) setOpenKey!(openKey === node.name ? null : node.name);
    else setLocalOpen((v) => !v);
  };
  const Icon = pickIcon(node.name, node.icon);

  // Nested groups auto-expand when the route moves into them (top-level handled in Sidebar).
  useEffect(() => {
    if (!controlled && isActiveGroup) setLocalOpen(true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pathname]);

  if (hasChildren) {
    return (
      <div className={cn(depth === 0 && 'pt-1')}>
        <button
          type="button"
          onClick={toggle}
          className={cn(
            'flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-ink-200 transition-colors hover:bg-white/5 hover:text-white',
            depth > 0 && 'text-[13px]',
          )}
        >
          {depth === 0 && <Icon className="size-4 shrink-0 text-ink-400" />}
          <span className="flex-1 text-left">{node.name}</span>
          <ChevronRight className={cn('size-4 text-ink-400 transition-transform', open && 'rotate-90')} />
        </button>
        {open && (
          <div className={cn('mt-0.5 space-y-0.5', depth === 0 ? 'ml-3 border-l border-white/5 pl-2' : 'ml-4')}>
            {node.children!.map((child, i) => (
              <MenuEntry key={`${child.name}-${i}`} node={child} depth={depth + 1} onNavigate={onNavigate} />
            ))}
          </div>
        )}
      </div>
    );
  }

  // Active state is computed from the full href (query string included) instead of taken
  // from NavLink's own `isActive`, which ignores the query — see isActiveHref.
  const href = resolveHref(node);
  const active = isActiveHref(href, pathname, search);

  return (
    <NavLink
      to={href}
      onClick={onNavigate}
      className={cn(
        'flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm transition-colors',
        depth > 0 && 'text-[13px]',
        active
          ? 'bg-brand-600 font-medium text-white shadow-sm'
          : 'text-ink-200 hover:bg-white/5 hover:text-white',
      )}
    >
      <Icon className={cn('shrink-0', depth === 0 ? 'size-4' : 'size-3.5', 'opacity-90')} />
      <span className="truncate">{node.name}</span>
    </NavLink>
  );
}
