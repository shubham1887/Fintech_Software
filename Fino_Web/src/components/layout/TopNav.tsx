import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { NavLink, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ChevronDown } from 'lucide-react';
import { getMenu, type MenuNode } from '@/api/menu.api';
import { cn } from '@/lib/cn';
import { pickIcon } from './iconMap';
import { resolveHref, isActiveHref } from './resolveRoute';

/**
 * Horizontal top navigation (LayoutOrientation = 'horizontal'). Renders the same role-menu as
 * the sidebar, as a single menu bar of dropdowns.
 *
 * Design constraint: every top-level group stays reachable on the one bar — no "More" overflow
 * menu that hides half the product behind a second click.
 *
 * **Every group always shows its name.** Labels are never dropped and never truncated: an admin
 * menu is twelve groups of one or two words, and a bar of bare icons asks the operator to learn
 * twelve pictograms to find "Report". When the names do not fit, the row scrolls sideways
 * instead — the name is the thing worth keeping, the scroll is the thing worth spending.
 *
 * This replaced a measured three-tier degradation (icon + label → icon only → scroll). The
 * middle tier is gone: on a 12-group menu it was the tier that almost always won, so the bar
 * was icon-only in practice and the labels it was protecting were never seen. What it was
 * right about is kept — a label is shown whole or not at all, never as "Repo…" — because items
 * are `shrink-0` and the overflow goes to the scroll instead of into the words.
 *
 * The bar carries names only — no icons. Twelve pictograms competing with twelve words made the
 * row wider without making it clearer, and the icon was what pushed the names off the bar in the
 * first place. Icons still appear on every item inside the dropdown panels, where the width is
 * not contested.
 *
 * Because the row can scroll, it fades at whichever edge has more beyond it: a menu that
 * silently cuts off at the window edge reads as a menu that is missing items.
 *
 * The vertical sidebar (Sidebar.tsx) is a separate component and already names everything —
 * nothing here affects it.
 *
 * The dropdown panels render in a portal at measured coordinates so nothing clips them (a panel
 * absolutely-positioned inside the header would be cut off at its edges).
 */
export function TopNav() {
  const { data: menu = [] } = useQuery({ queryKey: ['menu'], queryFn: getMenu, staleTime: 5 * 60_000 });

  const navRef = useRef<HTMLDivElement | null>(null);
  // Which edges still have groups beyond them, so the fades only appear when they mean something.
  const [overflow, setOverflow] = useState({ start: false, end: false });

  useLayoutEffect(() => {
    const nav = navRef.current;
    if (!nav || menu.length === 0) return;

    const measure = () => {
      const el = navRef.current;
      if (!el) return;
      // 1px of slack: scrollWidth and clientWidth differ by a sub-pixel on fractional zoom even
      // when nothing is actually clipped, which would otherwise leave a fade permanently on.
      const maxScroll = el.scrollWidth - el.clientWidth;
      const left = el.scrollLeft;
      setOverflow({ start: left > 1, end: left < maxScroll - 1 });
    };

    measure();

    // resize covers the window; the observer covers the bar changing width on its own (a longer
    // user name pushing the header controls wider); scroll covers the user moving the row.
    window.addEventListener('resize', measure);
    nav.addEventListener('scroll', measure, { passive: true });
    const observer = new ResizeObserver(measure);
    observer.observe(nav);
    // Fonts land after first paint and change the row's width under us.
    document.fonts?.ready.then(measure).catch(() => {});

    return () => {
      window.removeEventListener('resize', measure);
      nav.removeEventListener('scroll', measure);
      observer.disconnect();
    };
  }, [menu]);

  if (menu.length === 0) return <div className="min-w-0 flex-1" />;

  return (
    <div className="relative hidden min-w-0 flex-1 lg:block">
      <div
        ref={navRef}
        // The row keeps every group reachable by scrolling rather than by hiding anything. The
        // scrollbar itself is hidden because a bar across the middle of the header looks broken;
        // the edge fades below are what tell the user there is more.
        className="flex items-center gap-0.5 overflow-x-auto"
        style={{ scrollbarWidth: 'none' }}
      >
        <nav className="flex items-center gap-0.5">
          {menu.map((node, i) => (
            <TopNavItem key={`${node.name}-${i}`} node={node} />
          ))}
        </nav>
      </div>

      {/* Edge fades: shown only on the side that actually has groups beyond it. pointer-events
          none so they never swallow a click on the group underneath. The colour is the header's
          own background, so this works whatever brand colour the client's logo produced. */}
      {overflow.start && (
        <span
          aria-hidden
          className="pointer-events-none absolute inset-y-0 left-0 w-8 bg-gradient-to-r from-[var(--ui-appbar)] to-transparent"
        />
      )}
      {overflow.end && (
        <span
          aria-hidden
          className="pointer-events-none absolute inset-y-0 right-0 w-8 bg-gradient-to-l from-[var(--ui-appbar)] to-transparent"
        />
      )}
    </div>
  );
}

function TopNavItem({ node }: { node: MenuNode }) {
  const [open, setOpen] = useState(false);
  const [rect, setRect] = useState<{ left: number; top: number } | null>(null);
  const triggerRef = useRef<HTMLDivElement | null>(null);
  const location = useLocation();

  const hasChildren = Array.isArray(node.children) && node.children.length > 0;

  // Changing route closes whatever was open — otherwise the panel hangs over the new page.
  useEffect(() => setOpen(false), [location.pathname]);

  const place = useCallback(() => {
    const el = triggerRef.current;
    if (!el) return;
    const r = el.getBoundingClientRect();
    const PANEL = 240; // w-60
    const alignRight = r.left + PANEL > window.innerWidth - 8;
    setRect({ left: alignRight ? Math.max(8, r.right - PANEL) : r.left, top: r.bottom });
  }, []);

  // The panel is positioned in viewport coordinates, so it has to follow scroll and resize.
  useEffect(() => {
    if (!open) return;
    place();
    const onMove = () => place();
    window.addEventListener('resize', onMove);
    window.addEventListener('scroll', onMove, true);
    const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') setOpen(false); };
    document.addEventListener('keydown', onKey);
    return () => {
      window.removeEventListener('resize', onMove);
      window.removeEventListener('scroll', onMove, true);
      document.removeEventListener('keydown', onKey);
    };
  }, [open, place]);

  // No icon on the bar: the group names are what people navigate by, and a pictogram beside
  // every one of them only competed for the width the names need. They are still on each item
  // inside the dropdown, where there is room for a second cue.
  //
  // shrink-0 + whitespace-nowrap: an item is shown whole or not at all. Without them the names
  // would compress into "Repo…" the moment the row got tight, which is the one thing a label is
  // there to prevent — the row scrolls instead.
  const triggerCls = cn(
    'flex shrink-0 items-center gap-1.5 whitespace-nowrap rounded-lg px-2.5 py-2 text-[13px] font-medium transition-colors',
    'text-[var(--ui-fore)]/80 hover:bg-white/10 hover:text-[var(--ui-fore)]',
  );

  if (!hasChildren) {
    return (
      <NavLink
        to={resolveHref(node)}
        title={node.name}
        className={({ isActive }) => cn(triggerCls, isActive && 'bg-white/15 text-[var(--ui-fore)]')}
      >
        <span>{node.name}</span>
      </NavLink>
    );
  }

  return (
    <div
      ref={triggerRef}
      className="shrink-0"
      onMouseEnter={() => setOpen(true)}
      onMouseLeave={() => setOpen(false)}
    >
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        aria-expanded={open}
        title={node.name}
        className={cn(triggerCls, 'w-full', open && 'bg-white/15 text-[var(--ui-fore)]')}
      >
        <span>{node.name}</span>
        <ChevronDown className={cn('size-3.5 shrink-0 transition-transform', open && 'rotate-180')} />
      </button>

      {open && rect && createPortal(
        <div
          style={{ left: rect.left, top: rect.top, maxHeight: `calc(100vh - ${rect.top + 16}px)` }}
          className="fixed z-50 w-60 overflow-y-auto overscroll-contain rounded-xl border border-slate-200 bg-white p-1.5 shadow-pop"
          onMouseEnter={() => setOpen(true)}
          onMouseLeave={() => setOpen(false)}
        >
          {node.children!.map((child, i) => (
            <ChildLink key={`${child.name}-${i}`} node={child} depth={0} onNavigate={() => setOpen(false)} />
          ))}
        </div>,
        document.body,
      )}
    </div>
  );
}

function ChildLink({ node, depth, onNavigate }: { node: MenuNode; depth: number; onNavigate: () => void }) {
  const [open, setOpen] = useState(true);
  const { pathname, search } = useLocation();
  const hasChildren = Array.isArray(node.children) && node.children.length > 0;
  const Icon = pickIcon(node.name, node.icon);

  if (hasChildren) {
    return (
      <div>
        <button
          type="button"
          onClick={() => setOpen((v) => !v)}
          className="flex w-full items-center justify-between gap-2 rounded-lg px-2.5 py-1.5 text-left text-xs font-semibold uppercase tracking-wide text-slate-400"
        >
          {node.name}
          <ChevronDown className={cn('size-3 transition-transform', !open && '-rotate-90')} />
        </button>
        {open && node.children!.map((c, i) => <ChildLink key={`${c.name}-${i}`} node={c} depth={depth + 1} onNavigate={onNavigate} />)}
      </div>
    );
  }

  // Matched on the full href (query included) rather than NavLink's pathname-only
  // `isActive`: every not-yet-built item resolves to the same /coming-soon pathname, so that
  // flag highlighted all of them in this dropdown at once. See isActiveHref.
  const href = resolveHref(node);
  const active = isActiveHref(href, pathname, search);

  return (
    <NavLink
      to={href}
      onClick={onNavigate}
      className={cn(
        'flex items-center gap-2 rounded-lg px-2.5 py-2 text-sm transition-colors',
        depth > 0 && 'pl-4',
        active ? 'bg-brand-50 font-medium text-brand-700' : 'text-slate-600 hover:bg-slate-50',
      )}
    >
      <Icon className="size-4 shrink-0 text-slate-400" />
      <span className="truncate">{node.name}</span>
    </NavLink>
  );
}
