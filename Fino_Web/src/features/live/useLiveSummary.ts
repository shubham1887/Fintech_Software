import { useCallback, useEffect, useRef, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@/features/auth/AuthContext';
import { getLiveSummary, EMPTY_SUMMARY, type LiveSummary } from '@/api/live.api';

/**
 * The app's single live feed: work-queue counts and wallet balances, polled once and read by
 * every piece of chrome that needs them.
 *
 * Everything here exists because of one number: a thousand people signed in at once. The header
 * renders on every screen, so whatever it costs is paid continuously by every user, forever.
 * Four rules keep that affordable without the balance ever looking stale:
 *
 *  1. ONE query for the whole app. The strip and the balance bar read the same cache entry, so
 *     mounting more chrome costs nothing. (This replaced three separate polls.)
 *
 *  2. It only runs while someone is LOOKING. A hidden tab polls nothing — a phone in a pocket and
 *     a laptop with ten background tabs contribute zero load. React Query pauses on hidden, and
 *     the interval below goes further: an open-but-untouched tab slows right down.
 *
 *  3. Idle backs off. Actively working → every 15s. Nothing touched for two minutes → every 60s.
 *     The person watching a balance after sending money gets the fast rate; the person who left
 *     a tab open on another monitor does not.
 *
 *  4. Tabs share. The first tab to fetch broadcasts the answer; the others take it and skip their
 *     own poll. Five open tabs stay one request.
 *
 * And the poll itself is nearly free: the version token means an unchanged answer comes back with
 * no payload at all (see live.api.ts).
 *
 * Immediacy comes from `refresh()` rather than from a short interval — anything that moves money
 * calls it, and the new balance lands straight away.
 */

const QUERY_KEY = ['live-summary'] as const;

/** Someone is working: the rate at which a balance should visibly keep up. */
const ACTIVE_MS = 15_000;
/** Tab open, nobody touching it. Still current, a quarter of the cost. */
const IDLE_MS = 60_000;
/** No keyboard or pointer for this long counts as idle. */
const IDLE_AFTER_MS = 120_000;

/**
 * Up to this much random delay is added to every interval.
 *
 * Without it, a thousand clients that signed in after the same morning outage would poll in
 * lockstep forever, turning a steady trickle into a spike every fifteen seconds.
 */
const JITTER_MS = 4_000;

const CHANNEL = 'fino.live.summary';

/** Shared across tabs where supported; null in browsers (or modes) without BroadcastChannel. */
function openChannel(): BroadcastChannel | null {
  try {
    return typeof BroadcastChannel === 'function' ? new BroadcastChannel(CHANNEL) : null;
  } catch {
    return null;
  }
}

interface Shared {
  summary: LiveSummary;
  at: number;
}

export interface LiveState {
  summary: LiveSummary;
  /** True only before the very first answer — never while refreshing in the background. */
  isLoading: boolean;
  /** Set when the poll failed. The chrome says "unavailable"; it never renders zeros. */
  error: Error | null;
  /** Force an immediate refresh — call after anything that moves money. */
  refresh: () => void;
}

export function useLiveSummary(): LiveState {
  const { isAuthenticated } = useAuth();
  const qc = useQueryClient();

  // Last interaction, used only to decide the polling rate.
  const lastActivity = useRef(Date.now());
  const [isIdle, setIsIdle] = useState(false);

  useEffect(() => {
    const mark = () => {
      lastActivity.current = Date.now();
      setIsIdle((was) => (was ? false : was));
    };
    // Passive: these fire constantly and must never delay a scroll.
    const opts = { passive: true } as const;
    window.addEventListener('pointerdown', mark, opts);
    window.addEventListener('keydown', mark, opts);
    window.addEventListener('scroll', mark, opts);
    window.addEventListener('focus', mark);

    const timer = window.setInterval(() => {
      setIsIdle(Date.now() - lastActivity.current > IDLE_AFTER_MS);
    }, 30_000);

    return () => {
      window.removeEventListener('pointerdown', mark);
      window.removeEventListener('keydown', mark);
      window.removeEventListener('scroll', mark);
      window.removeEventListener('focus', mark);
      window.clearInterval(timer);
    };
  }, []);

  // ---- cross-tab sharing -------------------------------------------------
  // A fresh answer from another tab is written straight into this tab's cache, so the second tab
  // renders the same numbers without asking the server for them again.
  const channelRef = useRef<BroadcastChannel | null>(null);
  const lastSharedAt = useRef(0);

  useEffect(() => {
    if (!isAuthenticated) return;
    const ch = openChannel();
    channelRef.current = ch;
    if (!ch) return;

    const onMessage = (e: MessageEvent<Shared>) => {
      const payload = e.data;
      if (!payload?.summary || typeof payload.at !== 'number') return;
      lastSharedAt.current = payload.at;
      qc.setQueryData(QUERY_KEY, payload.summary);
    };

    ch.addEventListener('message', onMessage);
    return () => {
      ch.removeEventListener('message', onMessage);
      ch.close();
      channelRef.current = null;
    };
  }, [isAuthenticated, qc]);

  const query = useQuery({
    queryKey: QUERY_KEY,
    queryFn: async () => {
      const previous = qc.getQueryData<LiveSummary>(QUERY_KEY) ?? null;

      // Another tab answered this moment ago — reuse it rather than making the same call.
      if (previous && Date.now() - lastSharedAt.current < ACTIVE_MS) return previous;

      const next = await getLiveSummary(previous?.version ?? '', previous);

      // Only publish a genuinely new answer. A "not modified" result returns the same object,
      // so this comparison also stops tabs echoing unchanged state at each other.
      if (next !== previous) {
        try {
          channelRef.current?.postMessage({ summary: next, at: Date.now() } satisfies Shared);
        } catch {
          /* a closed or unavailable channel is never worth failing the poll over */
        }
      }
      return next;
    },
    enabled: isAuthenticated,
    // Jittered so a thousand clients never line up on the same tick.
    refetchInterval: () => (isIdle ? IDLE_MS : ACTIVE_MS) + Math.random() * JITTER_MS,
    // The rule that makes background tabs free.
    refetchIntervalInBackground: false,
    // Coming back to the tab should show current numbers immediately.
    refetchOnWindowFocus: true,
    staleTime: ACTIVE_MS,
    retry: 1,
    // Keep the last good answer on screen while a refetch is in flight, so the balance never
    // blinks out and back.
    placeholderData: (prev) => prev,
  });

  const refresh = useCallback(() => {
    void qc.invalidateQueries({ queryKey: QUERY_KEY });
  }, [qc]);

  return {
    summary: query.data ?? EMPTY_SUMMARY,
    isLoading: query.isLoading && !query.data,
    error: (query.error as Error) ?? null,
    refresh,
  };
}

/**
 * Refresh the live numbers from anywhere — after a transfer, a recharge, an accepted request.
 *
 * Exported separately so code that only needs to *push* an update does not have to subscribe to
 * the feed (and so re-render on every tick).
 */
export function refreshLiveSummary(qc: ReturnType<typeof useQueryClient>): void {
  void qc.invalidateQueries({ queryKey: QUERY_KEY });
}

/**
 * The wallet half of the feed, shaped like the `useQuery` result the dashboards used to hold.
 *
 * Exists so the pages that show balances could move onto the shared feed without being rewritten
 * around a different shape — the call site stays `wallets.data` / `wallets.isError` and simply
 * stops costing a request.
 */
export function useLiveWallets() {
  const { summary, isLoading, error, refresh } = useLiveSummary();

  // "Could not read" and "has no wallets" are different answers and must stay that way: the
  // dashboards render the first as a warning and the second as nothing, and neither as ₹0.00.
  const isError = Boolean(error) || (!isLoading && !summary.walletsAvailable && summary.wallets.length === 0);

  return {
    data: summary.wallets,
    isLoading,
    isError,
    error,
    /** Nothing to spin: a background refresh never blanks the numbers already on screen. */
    isFetching: false,
    refetch: refresh,
  };
}
