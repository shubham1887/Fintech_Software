import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ChevronLeft, ChevronRight, Megaphone, X } from 'lucide-react';
import { getPublishedNews, type NewsItem } from '@/api/news.api';
import { imageUrl } from '@/api/catalog.api';
import { useAuth } from '@/features/auth/AuthContext';
import { relativeTime } from '@/lib/format';
import { haptic } from '@/lib/native';
import { cn } from '@/lib/cn';

/**
 * Published news and offers, where the network will actually see them.
 *
 * The bell carries news too (publishing raises an alert), but a bell is something you open —
 * an offer nobody opens is an offer nobody saw. This is the passive surface: one line on the
 * dashboard, no interaction required to read it.
 *
 * Dismissal is per item, not per session: closing an announcement you have read should not
 * bring it back tomorrow, and a NEW announcement must not inherit the old one's dismissal.
 */

const SEEN_KEY = 'fino.news.dismissed';
const ROTATE_MS = 7000;

function readDismissed(): number[] {
  try {
    const raw = localStorage.getItem(SEEN_KEY);
    const parsed = raw ? JSON.parse(raw) : null;
    return Array.isArray(parsed) ? parsed.filter((n): n is number => typeof n === 'number') : [];
  } catch {
    return [];
  }
}

function dismiss(id: number): number[] {
  const next = [...new Set([...readDismissed(), id])].slice(-50); // keep the list bounded
  try {
    localStorage.setItem(SEEN_KEY, JSON.stringify(next));
  } catch {
    /* ignore — worst case the banner comes back, which is not a failure */
  }
  return next;
}

export function NewsBanner({ className }: { className?: string }) {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [dismissed, setDismissed] = useState<number[]>(readDismissed);
  const [index, setIndex] = useState(0);

  const { data = [] } = useQuery({
    queryKey: ['published-news'],
    queryFn: getPublishedNews,
    enabled: isAuthenticated,
    staleTime: 300_000,
  });

  const items = useMemo(
    () => data.filter((n) => !dismissed.includes(n.id)).slice(0, 8),
    [data, dismissed],
  );

  // Keep the index in range when an item is dismissed or the list refreshes.
  useEffect(() => {
    if (index >= items.length) setIndex(0);
  }, [items.length, index]);

  // Rotate, so a second announcement is not hidden behind the first. One item never rotates.
  useEffect(() => {
    if (items.length < 2) return;
    const t = setInterval(() => setIndex((i) => (i + 1) % items.length), ROTATE_MS);
    return () => clearInterval(t);
  }, [items.length]);

  if (items.length === 0) return null;

  const item: NewsItem = items[Math.min(index, items.length - 1)];

  const open = () => {
    if (!item.hyperLink) return;
    haptic();
    const link = item.hyperLink.trim();
    if (/^https?:\/\//i.test(link)) window.open(link, '_blank', 'noopener,noreferrer');
    else navigate(link.startsWith('/') ? link : `/${link}`);
  };

  return (
    <div
      className={cn(
        'flex items-center gap-2.5 rounded-card border border-brand-500/20 bg-gradient-to-r from-brand-50 to-transparent px-3 py-2',
        className,
      )}
    >
      {/* The attached picture stands in for the megaphone when there is one — an offer is
          mostly its artwork, and this strip is where the network actually sees it. A broken or
          missing file falls back to the icon rather than leaving a torn-image placeholder. */}
      {item.imagePath ? (
        <img
          src={imageUrl(item.imagePath)}
          alt=""
          className="size-7 shrink-0 rounded-full object-cover ring-1 ring-brand-500/20"
          onError={(e) => { e.currentTarget.style.display = 'none'; }}
        />
      ) : (
        <span className="grid size-7 shrink-0 place-items-center rounded-full bg-brand-500/10 text-brand-600">
          <Megaphone className="size-4" />
        </span>
      )}

      <button
        type="button"
        onClick={open}
        disabled={!item.hyperLink}
        className="min-w-0 flex-1 text-left disabled:cursor-default"
      >
        <span className="flex items-baseline gap-2">
          <span className="truncate text-[13px] font-semibold text-slate-800">{item.title}</span>
          {item.at && (
            <span className="shrink-0 text-[11px] tabular-nums text-slate-400">{relativeTime(item.at)}</span>
          )}
        </span>
        {item.description && (
          <span className="mt-0.5 block truncate text-xs text-slate-600">{item.description}</span>
        )}
      </button>

      {items.length > 1 && (
        <span className="hidden shrink-0 items-center gap-0.5 sm:flex">
          <button
            type="button"
            aria-label="Previous announcement"
            onClick={() => setIndex((i) => (i - 1 + items.length) % items.length)}
            className="grid size-6 place-items-center rounded-md text-slate-400 hover:bg-white hover:text-slate-600"
          >
            <ChevronLeft className="size-4" />
          </button>
          <span className="px-0.5 text-[11px] tabular-nums text-slate-400">
            {index + 1}/{items.length}
          </span>
          <button
            type="button"
            aria-label="Next announcement"
            onClick={() => setIndex((i) => (i + 1) % items.length)}
            className="grid size-6 place-items-center rounded-md text-slate-400 hover:bg-white hover:text-slate-600"
          >
            <ChevronRight className="size-4" />
          </button>
        </span>
      )}

      <button
        type="button"
        aria-label="Dismiss this announcement"
        onClick={() => setDismissed(dismiss(item.id))}
        className="grid size-6 shrink-0 place-items-center rounded-md text-slate-400 transition-colors hover:bg-white hover:text-slate-600"
      >
        <X className="size-4" />
      </button>
    </div>
  );
}
