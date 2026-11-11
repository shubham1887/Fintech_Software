import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';

/**
 * The single poll behind the app chrome — work-queue counts and wallet balances together.
 *
 * Mirrors `Fino_API/Responses/Live/LiveSummaryResponse.cs`. Replaces three separate calls
 * (WorkQueue/Counts, UserWalletMaster/GetByUserID, Wallet/Select) that every signed-in user used
 * to run on every screen.
 */

export type QueueTone = 'danger' | 'warning' | 'info' | 'success';

export interface LiveQueue {
  key: string;
  label: string;
  count: number;
  url: string;
  tone: QueueTone;
}

export interface LiveWallet {
  walletId: number;
  name: string;
  shortName: string;
  balance: number;
}

export interface LiveSummary {
  /** Opaque change token, echoed back on the next poll so an unchanged answer costs nothing. */
  version: string;
  queues: LiveQueue[];
  total: number;
  wallets: LiveWallet[];
  /**
   * False when the server could not read the balances.
   *
   * Kept separate from "no wallets" on purpose: a failed read must never render as ₹0.00. In a
   * payments console that zero is read as "my money is gone", or trusted into a transaction the
   * server then rejects.
   */
  walletsAvailable: boolean;
  /**
   * What the signed-in user owes their upline for funds taken on credit.
   *
   * Zero means nothing is owed, and the chrome renders nothing at all for it — the tile exists
   * only for someone who actually has a balance, so a user who always pays cash never carries a
   * debt counter at ₹0 around the app.
   */
  outstanding: number;
  /** Their credit limit. Zero = this account may not take funds on credit. */
  maxOutstanding: number;
  serverTimeUtc: string;
}

/** What a "nothing yet" state looks like — never shown as a failure. */
export const EMPTY_SUMMARY: LiveSummary = {
  version: '',
  queues: [],
  total: 0,
  wallets: [],
  walletsAvailable: false,
  outstanding: 0,
  maxOutstanding: 0,
  serverTimeUtc: '',
};

const TONES: QueueTone[] = ['danger', 'warning', 'info', 'success'];

/**
 * Thrown when the poll itself fails, so the chrome can say "unavailable" rather than draw zeros.
 */
export class LiveSummaryError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'LiveSummaryError';
  }
}

/**
 * Fetch the summary.
 *
 * @param knownVersion the version from the previous answer. When the server's state still matches
 *   it replies "not modified" with no payload, and this returns `previous` unchanged — which is
 *   what makes a 15-second poll affordable for a thousand users.
 */
export async function getLiveSummary(
  knownVersion: string,
  previous: LiveSummary | null,
): Promise<LiveSummary> {
  const res = await apiPost<unknown>('Live/Summary', { Version: knownVersion ?? '' });

  if (!res.ok || !res.data || typeof res.data !== 'object') {
    throw new LiveSummaryError(res.message?.trim() || 'Live data could not be loaded.');
  }

  const data = res.data as Record<string, unknown>;

  if (getField(data, 'NotModified') === true && previous) {
    // Nothing changed. Keep the object identity stable so React re-renders nothing at all.
    return previous;
  }

  const queueRows = getField(data, 'Queues');
  const walletRows = getField(data, 'Wallets');

  const queues: LiveQueue[] = Array.isArray(queueRows)
    ? (queueRows as Record<string, unknown>[])
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
        .filter((q) => q.key !== '')
    : [];

  const wallets: LiveWallet[] = Array.isArray(walletRows)
    ? (walletRows as Record<string, unknown>[])
        .map((r) => ({
          walletId: Number(getField(r, 'WalletID') ?? 0),
          name: String(getField(r, 'Name') ?? ''),
          shortName: String(getField(r, 'ShortName') ?? '') || String(getField(r, 'Name') ?? ''),
          balance: Number(getField(r, 'Balance') ?? 0),
        }))
        .filter((w) => w.walletId > 0)
    : [];

  return {
    version: String(getField(data, 'Version') ?? ''),
    queues,
    total: Number(getField(data, 'Total') ?? queues.reduce((n, q) => n + q.count, 0)),
    wallets,
    walletsAvailable: getField(data, 'WalletsAvailable') === true,
    outstanding: Number(getField(data, 'Outstanding') ?? 0),
    maxOutstanding: Number(getField(data, 'MaxOutstanding') ?? 0),
    serverTimeUtc: String(getField(data, 'ServerTimeUtc') ?? ''),
  };
}
