import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Bell, Check, LifeBuoy, MessageSquareWarning, RotateCcw, Send, Timer, Wallet,
  type LucideIcon,
} from 'lucide-react';
import { useLiveSummary } from '@/features/live/useLiveSummary';
import type { LiveQueue, QueueTone } from '@/api/live.api';
import { findScreen } from '@/screens/registry';
import { haptic } from '@/lib/native';
import { cn } from '@/lib/cn';

/**
 * The work-queue strip: what is waiting for you, on every screen.
 *
 * Drawn as ONE bar with hairline-separated segments rather than a scatter of floating pills.
 * The pills read as seven unrelated objects — mostly showing zero, each with its own outline and
 * its own shadow — which is a lot of visual weight for "nothing is happening". Segments inside a
 * single container read as one control: the eye follows one line instead of counting shapes.
 *
 * When every queue is empty the bar collapses to a single quiet "All clear" instead of a row of
 * greyed zeros. A row of zeros is the state the strip is in most of the time, so it is the state
 * worth designing: it should take up almost no attention, and it should not look broken.
 *
 * Order is the operator's. Segments are dragged to rearrange and the order is remembered per user
 * per browser, because which queue matters most is a matter of the job, not of the product.
 *
 * Numbers come from the shared live feed (see useLiveSummary), so this costs no request of its
 * own however many pieces of chrome are on screen.
 */

/** One icon per server-side queue key. An unknown key falls back rather than disappearing. */
const ICONS: Record<string, LucideIcon> = {
  'pending-recharge': Timer,
  'pending-dmt': Send,
  'fund-request': Wallet,
  'my-fund-request': Wallet,
  'fund-reverse-request': RotateCcw,
  complaint: MessageSquareWarning,
  ticket: LifeBuoy,
  'unread-alert': Bell,
};

/** Tint per urgency. Money stuck in a pending transaction should not look like an unread notice. */
const TONES: Record<QueueTone, string> = {
  danger: 'text-danger-600',
  warning: 'text-warning-600',
  info: 'text-brand-600',
  success: 'text-success-600',
};

/** The dot that marks a queue with something in it, so urgency reads without parsing numbers. */
const DOTS: Record<QueueTone, string> = {
  danger: 'bg-danger-500',
  warning: 'bg-warning-500',
  info: 'bg-brand-500',
  success: 'bg-success-500',
};

const ORDER_KEY = 'fino.workqueue.order';

/** Backend menu URL → in-app route, the same mapping the sidebar uses. */
function hrefFor(url: string): string {
  if (!url) return '/dashboard';
  if (/^\/Transaction$/i.test(url)) return '/recharge';
  if (/^\/DMTTransaction$/i.test(url)) return '/dmt';
  if (findScreen(url)) return `/s${url}`;
  return '/coming-soon';
}

function readOrder(): string[] {
  try {
    const raw = localStorage.getItem(ORDER_KEY);
    const parsed = raw ? JSON.parse(raw) : null;
    return Array.isArray(parsed) ? parsed.filter((k): k is string => typeof k === 'string') : [];
  } catch {
    // Private mode / blocked storage — the server's own order is a perfectly good default.
    return [];
  }
}

function writeOrder(keys: string[]): void {
  try {
    localStorage.setItem(ORDER_KEY, JSON.stringify(keys));
  } catch {
    /* ignore — the order is a convenience, never state the app depends on */
  }
}

/** Apply the remembered order, keeping any queue the server added since as a tail. */
function applyOrder(items: LiveQueue[], order: string[]): LiveQueue[] {
  if (order.length === 0) return items;
  const rank = new Map(order.map((k, i) => [k, i]));
  return [...items].sort((a, b) => (rank.get(a.key) ?? 999) - (rank.get(b.key) ?? 999));
}

export function WorkQueueStrip({ className }: { className?: string }) {
  const navigate = useNavigate();
  const { summary } = useLiveSummary();

  const [order, setOrder] = useState<string[]>(readOrder);
  const [dragKey, setDragKey] = useState<string | null>(null);
  const [overKey, setOverKey] = useState<string | null>(null);
  // Touch drag needs its own bookkeeping: there is no HTML5 drag-and-drop on a phone, and the
  // Android shell is exactly where this strip is most useful.
  const touchOrigin = useRef<string | null>(null);

  const items = useMemo(() => applyOrder(summary.queues, order), [summary.queues, order]);
  const pending = useMemo(() => items.reduce((n, i) => n + i.count, 0), [items]);

  const move = useCallback(
    (from: string, to: string) => {
      if (from === to) return;
      const keys = items.map((i) => i.key);
      const fromIdx = keys.indexOf(from);
      const toIdx = keys.indexOf(to);
      if (fromIdx < 0 || toIdx < 0) return;
      // Remove first, then insert at the target's index: the dragged segment ends up exactly where
      // the one it was dropped on used to be, in either direction.
      const [moved] = keys.splice(fromIdx, 1);
      keys.splice(toIdx, 0, moved);
      setOrder(keys);
      writeOrder(keys);
      haptic();
    },
    [items],
  );

  // A drag that ends outside any segment must still clear the highlight, or the bar is left
  // looking like it is mid-drag forever.
  useEffect(() => {
    const clear = () => {
      setDragKey(null);
      setOverKey(null);
      touchOrigin.current = null;
    };
    window.addEventListener('dragend', clear);
    window.addEventListener('drop', clear);
    return () => {
      window.removeEventListener('dragend', clear);
      window.removeEventListener('drop', clear);
    };
  }, []);

  // Nothing to show before the first answer arrives. Rendering a skeleton row on every screen
  // would be more visual noise than the strip itself is worth.
  if (items.length === 0) return null;

  // Everything empty: one quiet line. This is the usual state, so it gets the lightest treatment
  // the bar has — present enough to confirm it is working, faint enough to ignore.
  if (pending === 0) {
    return (
      <div className={cn('flex items-center', className)}>
        <span className="inline-flex h-8 items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-2.5 text-xs font-medium text-slate-400 shadow-sm">
          <Check className="size-3.5 text-success-500" />
          All clear
        </span>
      </div>
    );
  }

  return (
    <div className={cn('flex items-center', className)}>
      <div
        role="navigation"
        aria-label="Work queues"
        className="inline-flex max-w-full items-center divide-x divide-slate-100 overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm"
        style={{ scrollbarWidth: 'none' }}
      >
        {items.map((item) => {
          const Icon = ICONS[item.key] ?? Bell;
          const has = item.count > 0;
          const dragging = dragKey === item.key;
          const isOver = overKey === item.key && dragKey !== null && dragKey !== item.key;

          return (
            <button
              key={item.key}
              type="button"
              draggable
              onDragStart={(e) => {
                setDragKey(item.key);
                e.dataTransfer.effectAllowed = 'move';
                // Firefox refuses to start a drag without payload.
                e.dataTransfer.setData('text/plain', item.key);
              }}
              onDragOver={(e) => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                setOverKey(item.key);
              }}
              onDragLeave={() => setOverKey((k) => (k === item.key ? null : k))}
              onDrop={(e) => {
                e.preventDefault();
                const from = e.dataTransfer.getData('text/plain') || dragKey;
                if (from) move(from, item.key);
                setDragKey(null);
                setOverKey(null);
              }}
              // ---- touch: reorder for the phone / Android shell ----
              onTouchStart={() => {
                touchOrigin.current = item.key;
              }}
              onTouchMove={(e) => {
                const t = e.touches[0];
                if (!t || !touchOrigin.current) return;
                const el = document.elementFromPoint(t.clientX, t.clientY);
                const seg = el?.closest<HTMLElement>('[data-queue-key]');
                const key = seg?.dataset.queueKey ?? null;
                // Only treat it as a drag once the finger has actually left the origin segment —
                // otherwise every tap would be read as a one-element reorder and swallow the click.
                if (key && key !== touchOrigin.current) {
                  setDragKey(touchOrigin.current);
                  setOverKey(key);
                }
              }}
              onTouchEnd={() => {
                if (dragKey && overKey) move(dragKey, overKey);
                setDragKey(null);
                setOverKey(null);
                touchOrigin.current = null;
              }}
              data-queue-key={item.key}
              onClick={() => {
                // A finished drag is not a click. Without this the segment you dropped onto would
                // also navigate away from the page you are reordering on.
                if (dragKey) return;
                haptic();
                navigate(hrefFor(item.url));
              }}
              title={`${item.label}: ${item.count}`}
              aria-label={`${item.label}, ${item.count}. Opens the ${item.label.toLowerCase()} page.`}
              className={cn(
                'relative flex h-9 shrink-0 cursor-grab items-center gap-2 px-3 text-xs font-semibold tabular-nums transition-colors',
                'first:pl-3.5 last:pr-3.5 hover:bg-slate-50 active:cursor-grabbing',
                has ? TONES[item.tone] ?? TONES.info : 'text-slate-400',
                dragging && 'opacity-40',
                // An inset ring marks the drop target without shifting the bar by a pixel —
                // an outer ring on a divided row makes every neighbour appear to jump.
                isOver && 'ring-2 ring-inset ring-brand-500',
              )}
            >
              <span className="relative grid place-items-center">
                <Icon className="size-4 shrink-0" />
                {has && (
                  <span
                    className={cn(
                      'absolute -right-1 -top-1 size-1.5 rounded-full ring-2 ring-white',
                      DOTS[item.tone] ?? DOTS.info,
                    )}
                  />
                )}
              </span>
              <span className={cn(!has && 'text-slate-300')}>{item.count}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
