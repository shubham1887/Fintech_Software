import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { CornerDownLeft, Search } from 'lucide-react';
import { getMenu, type MenuNode } from '@/api/menu.api';
import { cn } from '@/lib/cn';
import { pickIcon } from './iconMap';
import { resolveHref } from './resolveRoute';

interface Hit {
  name: string;
  group?: string;
  href: string;
  icon?: string;
}

/** Flatten the role menu into navigable leaf items (deepest nodes), tagged by their group. */
function flatten(nodes: MenuNode[], group: string | undefined, out: Hit[]) {
  for (const n of nodes) {
    const hasChildren = Array.isArray(n.children) && n.children.length > 0;
    if (hasChildren) {
      flatten(n.children!, n.name, out);
    } else {
      out.push({ name: n.name, group, href: resolveHref(n), icon: n.icon });
    }
  }
}

/**
 * Command-palette style global search in the Topbar. Searches the role-filtered menu and
 * jumps to any screen — the kind of fast navigation a real operations console needs.
 */
export function GlobalSearch() {
  const navigate = useNavigate();
  const [q, setQ] = useState('');
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);

  const { data: menu = [] } = useQuery({ queryKey: ['menu'], queryFn: getMenu, staleTime: 5 * 60_000 });

  const items = useMemo(() => {
    const out: Hit[] = [];
    flatten(menu, undefined, out);
    // de-dup by name+href
    const seen = new Set<string>();
    return out.filter((h) => {
      const k = `${h.name}|${h.href}`;
      if (seen.has(k)) return false;
      seen.add(k);
      return true;
    });
  }, [menu]);

  const results = useMemo(() => {
    const term = q.trim().toLowerCase();
    if (!term) return [];
    return items
      .filter((h) => h.name.toLowerCase().includes(term) || h.group?.toLowerCase().includes(term))
      .slice(0, 8);
  }, [q, items]);

  useEffect(() => setActive(0), [q]);

  // ⌘K / Ctrl-K to focus
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        inputRef.current?.focus();
        setOpen(true);
      }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, []);

  const go = (h: Hit) => {
    setQ('');
    setOpen(false);
    inputRef.current?.blur();
    navigate(h.href);
  };

  return (
    <div className="relative hidden w-full max-w-md md:block">
      <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
      <input
        ref={inputRef}
        value={q}
        onChange={(e) => {
          setQ(e.target.value);
          setOpen(true);
        }}
        onFocus={() => setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 120)}
        onKeyDown={(e) => {
          if (e.key === 'ArrowDown') {
            e.preventDefault();
            setActive((i) => Math.min(i + 1, results.length - 1));
          } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setActive((i) => Math.max(i - 1, 0));
          } else if (e.key === 'Enter' && results[active]) {
            go(results[active]);
          } else if (e.key === 'Escape') {
            setOpen(false);
            inputRef.current?.blur();
          }
        }}
        placeholder="Search screens, reports, settings…"
        className="h-9 w-full rounded-lg border border-slate-200 bg-slate-50/80 pl-9 pr-12 text-sm text-slate-700 outline-none transition-colors placeholder:text-slate-400 focus:border-brand-400 focus:bg-white focus:ring-2 focus:ring-brand-500/20"
      />
      <kbd className="pointer-events-none absolute right-2.5 top-1/2 hidden -translate-y-1/2 rounded border border-slate-200 bg-white px-1.5 py-0.5 text-[10px] font-medium text-slate-400 lg:block">
        ⌘K
      </kbd>

      {open && q.trim() && (
        <div className="absolute left-0 right-0 top-11 z-30 overflow-hidden rounded-xl border border-slate-200 bg-white p-1.5 shadow-pop">
          {results.length === 0 ? (
            <p className="px-3 py-6 text-center text-sm text-slate-400">No screens match “{q}”.</p>
          ) : (
            results.map((h, i) => {
              const Icon = pickIcon(h.name, h.icon);
              return (
                <button
                  key={`${h.name}-${i}`}
                  type="button"
                  onMouseDown={(e) => {
                    e.preventDefault();
                    go(h);
                  }}
                  onMouseEnter={() => setActive(i)}
                  className={cn(
                    'flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-left text-sm transition-colors',
                    i === active ? 'bg-brand-50 text-brand-700' : 'text-slate-700 hover:bg-slate-50',
                  )}
                >
                  <Icon className={cn('size-4 shrink-0', i === active ? 'text-brand-500' : 'text-slate-400')} />
                  <span className="flex-1 truncate">{h.name}</span>
                  {h.group && <span className="shrink-0 text-xs text-slate-400">{h.group}</span>}
                  {i === active && <CornerDownLeft className="size-3.5 shrink-0 text-brand-400" />}
                </button>
              );
            })
          )}
        </div>
      )}
    </div>
  );
}
