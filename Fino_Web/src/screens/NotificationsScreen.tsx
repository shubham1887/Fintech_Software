import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Bell, CheckCheck, ChevronRight, CircleAlert, Gift, Info, LifeBuoy, Receipt, Settings2, Wallet,
  type LucideIcon,
} from 'lucide-react';
import {
  getNotificationFeed, markNotificationRead, type AppNotification, type NotificationKind,
} from '@/api/notifications.api';
import { imageUrl } from '@/api/catalog.api';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { dateTime, relativeTime } from '@/lib/format';
import { haptic } from '@/lib/native';
import { cn } from '@/lib/cn';
import type { CustomScreenProps } from './registry';

/**
 * The full notification history — what the bell's "See all" opens, and the screen the
 * Support menu points every role at.
 *
 * Deliberately the same data and the same visual language as the bell panel: a user who
 * taps through from the bell should recognise the list, not meet a different product.
 */

const KIND_LOOKS: Record<NotificationKind, { icon: LucideIcon; tone: string; label: string }> = {
  Info: { icon: Info, tone: 'bg-sky-50 text-sky-600', label: 'Info' },
  Alert: { icon: CircleAlert, tone: 'bg-amber-50 text-amber-600', label: 'Alert' },
  Promo: { icon: Gift, tone: 'bg-fuchsia-50 text-fuchsia-600', label: 'Offer' },
  Ticket: { icon: LifeBuoy, tone: 'bg-indigo-50 text-indigo-600', label: 'Ticket' },
  Fund: { icon: Wallet, tone: 'bg-emerald-50 text-emerald-600', label: 'Fund' },
  Txn: { icon: Receipt, tone: 'bg-blue-50 text-blue-600', label: 'Transaction' },
  System: { icon: Settings2, tone: 'bg-slate-100 text-slate-600', label: 'System' },
};

const FILTERS: Array<{ key: string; label: string }> = [
  { key: '', label: 'All' },
  { key: 'unread', label: 'Unread' },
  { key: 'Ticket', label: 'Tickets' },
  { key: 'Fund', label: 'Fund' },
  { key: 'Txn', label: 'Transactions' },
  { key: 'Promo', label: 'Offers' },
];

export function NotificationsScreen({ def }: CustomScreenProps) {
  const [filter, setFilter] = useState('');
  const navigate = useNavigate();
  const qc = useQueryClient();

  const unreadOnly = filter === 'unread';
  const kind = unreadOnly ? '' : filter;

  const { data, isLoading } = useQuery({
    queryKey: ['notifications-page', filter],
    queryFn: () => getNotificationFeed({ pageSize: 100, unreadOnly, kind }),
  });

  const markRead = useMutation({
    mutationFn: (id?: number) => markNotificationRead(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['notifications-page'] });
      qc.invalidateQueries({ queryKey: ['notification-feed'] });
      // The counts strip carries the same unread number; refresh it in the same breath.
      qc.invalidateQueries({ queryKey: ['work-queue'] });
    },
  });

  const items = data?.items ?? [];
  const unread = data?.unreadCount ?? 0;

  function open(item: AppNotification) {
    haptic();
    if (!item.isRead) markRead.mutate(item.id);
    if (item.actionUrl) navigate(item.actionUrl);
  }

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle={unread > 0 ? `${unread} unread` : 'Alerts, approvals and ticket replies'}
        actions={
          <Button
            variant="outline"
            disabled={unread === 0 || markRead.isPending}
            onClick={() => markRead.mutate(undefined)}
          >
            <CheckCheck className="size-4" />
            Mark all read
          </Button>
        }
      />

      <div className="flex flex-wrap gap-1.5">
        {FILTERS.map((f) => (
          <button
            key={f.key}
            type="button"
            onClick={() => setFilter(f.key)}
            className={cn(
              'rounded-full px-3 py-1.5 text-xs font-semibold transition-colors',
              filter === f.key
                ? 'bg-brand-600 text-white'
                : 'bg-white text-slate-500 ring-1 ring-slate-200 hover:text-slate-700',
            )}
          >
            {f.label}
          </button>
        ))}
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {isLoading ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 5 }).map((_, i) => (
              <Skeleton key={i} className="h-14 w-full rounded-lg" />
            ))}
          </div>
        ) : items.length === 0 ? (
          <div className="flex flex-col items-center gap-1 px-6 py-16 text-center">
            <span className="grid size-12 place-items-center rounded-full bg-slate-50 text-slate-300">
              <Bell className="size-6" />
            </span>
            <p className="mt-2 text-sm font-medium text-slate-600">Nothing here</p>
            <p className="text-xs text-slate-400">
              {filter ? 'No notifications match this filter.' : 'New alerts and approvals will show up here.'}
            </p>
          </div>
        ) : (
          items.map((item, i) => {
            const meta = KIND_LOOKS[item.kind] ?? KIND_LOOKS.Info;
            const Icon = meta.icon;
            return (
              <button
                key={item.id}
                type="button"
                onClick={() => open(item)}
                className={cn(
                  'flex w-full items-start gap-3 px-4 py-4 text-left transition-colors hover:bg-slate-50',
                  i > 0 && 'border-t border-slate-100',
                  !item.isRead && 'bg-brand-50/40',
                )}
              >
                <span className={cn('mt-0.5 grid size-9 shrink-0 place-items-center rounded-full', meta.tone)}>
                  <Icon className="size-[18px]" />
                </span>

                <span className="min-w-0 flex-1">
                  <span className="flex flex-wrap items-baseline gap-x-2">
                    <span
                      className={cn(
                        'text-sm',
                        item.isRead ? 'font-medium text-slate-700' : 'font-semibold text-slate-900',
                      )}
                    >
                      {item.title}
                    </span>
                    <span className="text-[11px] font-medium uppercase tracking-wide text-slate-400">
                      {meta.label}
                    </span>
                  </span>
                  <span className="mt-1 block text-[13px] leading-relaxed text-slate-600">{item.body}</span>
                  {/* An offer's artwork, where the alert carries one. Capped in height so a tall
                      image cannot push the rest of the list off the screen, and hidden outright
                      if the file has gone — a broken-image box is worse than no image. */}
                  {item.imageUrl && (
                    <img
                      src={imageUrl(item.imageUrl)}
                      alt=""
                      className="mt-2 max-h-44 w-full rounded-lg border border-slate-200 object-cover"
                      onError={(e) => { e.currentTarget.style.display = 'none'; }}
                    />
                  )}
                  <span className="mt-1 block text-[11px] tabular-nums text-slate-400" title={dateTime(item.at)}>
                    {relativeTime(item.at)} ago · {dateTime(item.at)}
                  </span>
                </span>

                {item.actionUrl && <ChevronRight className="mt-1 size-4 shrink-0 text-slate-300" />}
                {!item.isRead && <span className="mt-2 size-2 shrink-0 rounded-full bg-brand-500" />}
              </button>
            );
          })
        )}
      </div>
    </div>
  );
}
