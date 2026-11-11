import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';

/**
 * "What is waiting for me?" — one call behind the counts strip that sits on every screen.
 *
 * The server decides which queues this role owns and scopes every count to the signed-in user
 * (see `Fino_API/Services/WorkQueueService.cs`). The client renders what it is given and does
 * no filtering of its own — a role that must not see the platform's pending recharges is not
 * sent that number at all.
 */

export type QueueTone = 'danger' | 'warning' | 'info' | 'success';

export interface WorkQueueItem {
  /** Stable id used to pick the icon and to remember the user's ordering. */
  key: string;
  /** Tooltip / accessible name — the strip itself shows icon + number only. */
  label: string;
  count: number;
  /** Backend menu URL, e.g. "/Pending". Routed through `resolveHref`-style mapping. */
  url: string;
  tone: QueueTone;
}

export interface WorkQueue {
  items: WorkQueueItem[];
  total: number;
}

const EMPTY: WorkQueue = { items: [], total: 0 };

const TONES: QueueTone[] = ['danger', 'warning', 'info', 'success'];

export async function getWorkQueue(): Promise<WorkQueue> {
  const res = await apiPost<unknown>('WorkQueue/Counts', {});
  // "Nothing waiting" is the good case, not a failure — never let it surface as an error on a
  // strip that renders on every screen.
  if (!res.ok || !res.data || typeof res.data !== 'object') return EMPTY;

  const data = res.data as Record<string, unknown>;
  const rows = getField(data, 'Queues');
  if (!Array.isArray(rows)) return EMPTY;

  const items = (rows as Record<string, unknown>[])
    .map((r) => {
      const tone = String(getField(r, 'Tone') ?? 'info') as QueueTone;
      return {
        key: String(getField(r, 'Key') ?? ''),
        label: String(getField(r, 'Label') ?? ''),
        count: Number(getField(r, 'Count') ?? 0),
        url: String(getField(r, 'Url') ?? ''),
        tone: TONES.includes(tone) ? tone : 'info',
      };
    })
    .filter((i) => i.key !== '');

  return { items, total: Number(getField(data, 'Total') ?? items.reduce((n, i) => n + i.count, 0)) };
}
