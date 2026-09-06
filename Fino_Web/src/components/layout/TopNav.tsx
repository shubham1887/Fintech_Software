import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ChevronDown } from 'lucide-react';
import { getMenu, type MenuNode } from '@/api/menu.api';
import { cn } from '@/lib/cn';
import { pickIcon } from './iconMap';
import { resolveHref } from './resolveRoute';

/**
 * Horizontal top navigation (used when LayoutOrientation = 'horizontal'). Renders the same
 * role-menu as the sidebar: top-level items open a hover/click dropdown of their leaf links.
 */
export function TopNav() {
  const { data: menu = [] } = useQuery({ queryKey: ['menu'], queryFn: getMenu, staleTime: 5 * 60_000 });
  return (
    <nav className="hidden min-w-0 flex-1 items-center gap-0.5 overflow-x-auto lg:flex">
      {menu.map((node, i) => (
        <TopNavItem key={`${node.name}-${i}`} node={node} />
      ))}
    </nav>
  );
}

function TopNavItem({ node }: { node: MenuNode }) {
  const [open, setOpen] = useState(false);
  const hasChildren = Array.isArray(node.children) && node.children.length > 0;
  const Icon = pickIcon(node.name, node.icon);

  if (!hasChildren) {
    return (
      <NavLink
        to={resolveHref(node)}
        className="flex items-center gap-1.5 whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium text-[var(--ui-fore)]/80 transition-colors hover:bg-white/10 hover:text-[var(--ui-fore)]"
      >
        <Icon className="size-4" />
        {node.name}
      </NavLink>
    );
  }

  return (
    <div className="relative" onMouseEnter={() => setOpen(true)} onMouseLeave={() => setOpen(false)}>
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="flex items-center gap-1.5 whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium text-[var(--ui-fore)]/80 transition-colors hover:bg-white/10 hover:text-[var(--ui-fore)]"
      >
        <Icon className="size-4" />
        {node.name}
        <ChevronDown className={cn('size-3.5 transition-transform', open && 'rotate-180')} />
      </button>
      {open && (
        <div className="absolute left-0 top-full z-40 max-h-[70vh] w-60 overflow-y-auto rounded-xl border border-slate-200 bg-white p-1.5 shadow-pop">
          {node.children!.map((child, i) => (
            <ChildLink key={`${child.name}-${i}`} node={child} depth={0} onNavigate={() => setOpen(false)} />
          ))}
        </div>
      )}
    </div>
  );
}

function ChildLink({ node, depth, onNavigate }: { node: MenuNode; depth: number; onNavigate: () => void }) {
  const [open, setOpen] = useState(true);
  const hasChildren = Array.isArray(node.children) && node.children.length > 0;
  const Icon = pickIcon(node.name, node.icon);

  if (hasChildren) {
    return (
      <div>
        <button
          type="button"
          onClick={() => setOpen((v) => !v)}
          className="flex w-full items-center gap-2 rounded-lg px-2.5 py-1.5 text-left text-xs font-semibold uppercase tracking-wide text-slate-400"
        >
          {node.name}
        </button>
        {open && node.children!.map((c, i) => <ChildLink key={`${c.name}-${i}`} node={c} depth={depth + 1} onNavigate={onNavigate} />)}
      </div>
    );
  }

  return (
    <NavLink
      to={resolveHref(node)}
      onClick={onNavigate}
      className={({ isActive }) =>
        cn(
          'flex items-center gap-2 rounded-lg px-2.5 py-2 text-sm transition-colors',
          depth > 0 && 'pl-4',
          isActive ? 'bg-brand-50 font-medium text-brand-700' : 'text-slate-600 hover:bg-slate-50',
        )
      }
    >
      <Icon className="size-4 shrink-0 text-slate-400" />
      <span className="truncate">{node.name}</span>
    </NavLink>
  );
}
