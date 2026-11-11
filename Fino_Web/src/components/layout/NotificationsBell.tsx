import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Bell, CheckCheck, CircleAlert, Gift, Info, LifeBuoy, Receipt, Settings2, Wallet,
  type LucideIcon,
} from 'lucide-react';
import {
  getNotificationFeed, markNotificationRead, type AppNotification, type NotificationKind,
} from '@/api/notifications.api';
import { useAuth } from '@/features/auth/AuthContext';
import { useUISettings } from '@/theme/UISettingsProvider';
import { relativeTime } from '@/lib/format';
import { haptic } from '@/lib/native';
import { cn } from '@/lib/cn';

/**
 * The notification bell.
 *
 * It used to be decoration: a hard-coded "You're all caught up" panel with a permanent red
 * dot, wired to nothing. It now reads the real feed — the same rows an Android push mirrors —
 * so an alert is visible whether or not the phone was on when it was sent.
 *
 * Polling, not sockets: one small POST a minute is cheap, survives the WebView being
 * backgrounded, and needs no infrastructure on a customer's own server. A push arriving
 * refreshes it immediately through `native:push` (see lib/push.ts), so the poll is the floor,
 * not the mechanism.
 */

const POLL_MS = 60_000;

/** Icon + tint per kind, so money and marketing never look the same. */
const LOOKS: Record<NotificationKind, { icon: LucideIcon; tone: string }> = {
  Info: { icon: Info, tone: 'bg-sky-50 text-sky-600' },
  Alert: { icon: CircleAlert, tone: 'bg-amber-50 text-amber-600' },
  Promo: { icon: Gift, tone: 'bg-fuchsia-50 text-fuchsia-600' },
  Ticket: { icon: LifeBuoy, tone: 'bg-indigo-50 text-indigo-600' },
  Fund: { icon: Wallet, tone: 'bg-emerald-50 text-emerald-600' },
  Txn: { icon: Receipt, tone: 'bg-blue-50 text-blue-600' },
  System: { icon: Settings2, tone: 'bg-slate-100 text-slate-600' },
};

function look(kind: NotificationKind) {
  return LOOKS[kind] ?? LOOKS.Info;
}

export function NotificationsBell() {
  const [open, setOpen] = useState(false);
  const navigate = useNavigate();
  const qc = useQueryClient();
  const { isAuthenticated } = useAuth();
  const panelRef = useRef<HTMLDivElement>(null);

  // On the coloured horizontal appbar a slate-500 bell is invisible — follow --ui-fore there.
  const horizontal = useUISettings().orientation === 'horizontal';

  const { data, isLoading } = useQuery({
    queryKey: ['notification-feed'],
    queryFn: () => getNotificationFeed({ pageSize: 15 }),
    enabled: isAuthenticated,
    refetchInterval: POLL_MS,
    // The tray is the fallback while the tab is hidden; polling a background tab is waste.
    refetchIntervalInBackground: false,
    staleTime: 15_000,
  });

  const markRead = useMutation({
    mutationFn: (id?: number) => markNotificationRead(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['notification-feed'] });
      // The strip carries the same unread number — the two must never disagree on screen.
      qc.invalidateQueries({ queryKey: ['work-queue'] });
    },
  });

  const unread = data?.unreadCount ?? 0;
  const items = data?.items ?? [];

  // Escape closes the panel — it is a menu, and a menu you cannot dismiss from the keyboard
  // is a trap for anyone not using a mouse.
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false);
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [open]);

  function openItem(item: AppNotification) {
    haptic();
    if (!item.isRead) markRead.mutate(item.id);
    setOpen(false);
    if (item.actionUrl) navigate(item.actionUrl);
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => {
          haptic();
          setOpen((v) => !v);
        }}
        className={cn(
          'relative grid size-9 place-items-center rounded-lg transition-colors',
          horizontal
            ? 'text-[var(--ui-fore)]/85 hover:bg-white/10 hover:text-[var(--ui-fore)]'
            : 'text-slate-500 hover:bg-slate-100',
        )}
        aria-label={unread > 0 ? `Notifications, ${unread} unread` : 'Notifications'}
        aria-expanded={open}
      >
        <Bell className="size-5" />
        {/* The badge is the count, not a decorative dot: a dot that is always on says nothing. */}
        {unread > 0 && (
          <span className="absolute -right-0.5 -top-0.5 grid min-w-[17px] place-items-center rounded-full bg-danger-500 px-1 text-[10px] font-bold leading-[17px] text-white ring-2 ring-white">
            {unread > 99 ? '99+' : unread}
          </span>
        )}
      </button>

      {open && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setOpen(false)} />
          <div
            ref={panelRef}
            className="absolute right-0 z-20 mt-2 w-[min(22rem,calc(100vw-2rem))] overflow-hidden rounded-xl border border-slate-200 bg-white shadow-pop"
          >
            <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
              <p className="text-sm font-semibold text-slate-800">
                Notifications
                {unread > 0 && <span className="ml-1.5 font-normal text-slate-400">{unread} new</span>}
              </p>
              <button
                type="button"
                disabled={unread === 0 || markRead.isPending}
                onClick={() => markRead.mutate(undefined)}
                className="flex items-center gap-1 rounded-md px-1.5 py-1 text-xs font-medium text-slate-500 transition-colors enabled:hover:bg-slate-50 enabled:hover:text-slate-700 disabled:text-slate-300"
                title="Mark all as read"
              >
                <CheckCheck className="size-4" />
                Mark all
              </button>
            </div>

            <div className="max-h-[22rem] overflow-y-auto overscroll-contain">
              {isLoading ? (
                <p className="px-6 py-10 text-center text-sm text-slate-400">Loading…</p>
              ) : items.length === 0 ? (
                <div className="flex flex-col items-center gap-1 px-6 py-10 text-center">
                  <span className="grid size-11 place-items-center rounded-full bg-slate-50 text-slate-300">
                    <Bell className="size-5" />
                  </span>
                  <p className="mt-2 text-sm font-medium text-slate-600">You&apos;re all caught up</p>
                  <p className="text-xs text-slate-400">New alerts and approvals will show up here.</p>
                </div>
              ) : (
                items.map((item, i) => {
                  const { icon: Icon, tone } = look(item.kind);
                  return (
                    <button
                      key={item.id}
                      type="button"
                      onClick={() => openItem(item)}
                      className={cn(
                        'flex w-full items-start gap-3 px-4 py-3 text-left transition-colors hover:bg-slate-50',
                        i > 0 && 'border-t border-slate-100',
                        !item.isRead && 'bg-brand-50/40',
                      )}
                    >
                      <span className={cn('mt-0.5 grid size-8 shrink-0 place-items-center rounded-full', tone)}>
                        <Icon className="size-4" />
                      </span>
                      <span className="min-w-0 flex-1">
                        <span className="flex items-baseline gap-2">
                          <span
                            className={cn(
                              'min-w-0 flex-1 truncate text-[13px]',
                              item.isRead ? 'font-medium text-slate-700' : 'font-semibold text-slate-900',
                            )}
                          >
                            {item.title}
                          </span>
                          <span className="shrink-0 text-[11px] tabular-nums text-slate-400">
                            {relativeTime(item.at)}
                          </span>
                        </span>
                        <span className="mt-0.5 line-clamp-2 block text-xs leading-snug text-slate-500">
                          {item.body}
                        </span>
                      </span>
                      {!item.isRead && <span className="mt-2 size-1.5 shrink-0 rounded-full bg-brand-500" />}
                    </button>
                  );
                })
              )}
            </div>

            <button
              type="button"
              onClick={() => {
                setOpen(false);
                navigate('/s/Notifications');
              }}
              className="w-full border-t border-slate-100 py-2.5 text-center text-xs font-semibold text-brand-600 hover:bg-slate-50"
            >
              See all notifications
            </button>
          </div>
        </>
      )}
    </div>
  );
}
