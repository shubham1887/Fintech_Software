import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';

export interface WalletBalance {
  walletId: number;
  name: string;
  shortName: string;
  balance: number;
}

/**
 * The balance could not be READ. Distinct from "the balance is zero" — see the note in
 * {@link getMyWallets}. Callers must show this, never a number.
 */
export class WalletLoadError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'WalletLoadError';
  }
}

/** Known wallet fallbacks (WalletMaster seed) when the name lookup is empty. */
const WALLET_NAMES: Record<number, { name: string; short: string }> = {
  1: { name: 'Recharge Wallet', short: 'RCH' },
  2: { name: 'DMT Wallet', short: 'DMT' },
  3: { name: 'AEPS Wallet', short: 'AEPS' },
};

/** The API's "this user simply has no wallet rows" answer — a real zero, not a failure. */
const NO_RECORDS = /no\s*record/i;

/**
 * The signed-in user's wallet balances. Lists EVERY active wallet (Wallet/Select) and joins the
 * user's balance from UserWalletMaster/GetByUserID — wallets the user has no row for show ₹0.00.
 *
 * THROWS {@link WalletLoadError} when the balance call fails.
 *
 * That is deliberate and is the whole point of this function's contract. `apiPost` does not throw
 * on a business-level failure (403 from the access/scope policy, a 404, a backend exception) — it
 * resolves with `{ ok: false, data: null }`. The previous version treated that the same as "no
 * rows", so every such failure silently rendered the user's balance as **₹0.00**. In a payments
 * app that is the worst possible failure mode: the retailer reads it as "my money is gone", or
 * trusts it and starts a transaction that the server then rejects. A failure must look like a
 * failure.
 *
 * The one case that is NOT a failure is the API answering "No records found", which is how it
 * says the user has no wallet rows yet — that IS a genuine zero.
 */
export async function getMyWallets(userId: number): Promise<WalletBalance[]> {
  if (!userId || userId <= 0) {
    throw new WalletLoadError('Your session is missing a user id. Please sign in again.');
  }

  // In parallel: on a phone two serial round trips is a visible delay on every dashboard load.
  const [balancesRes, defsRes] = await Promise.all([
    apiPost<unknown>('UserWalletMaster/GetByUserID', { UserID: userId }),
    apiPost<unknown>('Wallet/Select', {}).catch(() => null),
  ]);

  if (!balancesRes.ok && !NO_RECORDS.test(balancesRes.message ?? '')) {
    throw new WalletLoadError(
      balancesRes.message?.trim() || 'Wallet balance could not be loaded.',
    );
  }

  // Balance per walletId from the user's own wallet rows (missing → 0).
  const balanceById = new Map<number, number>();
  for (const r of asArray(balancesRes.data)) {
    const id = Number(getField(r, 'WalletID') ?? 0);
    if (Number.isFinite(id) && id > 0) balanceById.set(id, Number(getField(r, 'Balance') ?? 0));
  }

  const walletDefs = asArray(defsRes?.data);

  // Preferred path: iterate all active wallets so every system wallet shows (0 if no balance row).
  if (walletDefs.length > 0) {
    return walletDefs.map((r) => {
      const walletId = Number(getField(r, 'WalletID', 'ID') ?? 0);
      const fallback = WALLET_NAMES[walletId] ?? { name: `Wallet ${walletId}`, short: 'W' };
      return {
        walletId,
        name: String(getField(r, 'Name', 'WalletName') ?? '') || fallback.name,
        shortName: String(getField(r, 'ShortName', 'Name') ?? '') || fallback.short,
        balance: balanceById.get(walletId) ?? 0,
      };
    });
  }

  // Fallback (Wallet/Select unavailable): show whatever balance rows we have.
  return asArray(balancesRes.data).map((r) => {
    const walletId = Number(getField(r, 'WalletID') ?? 0);
    const fallback = WALLET_NAMES[walletId] ?? { name: `Wallet ${walletId}`, short: 'W' };
    return {
      walletId,
      name: fallback.name,
      shortName: fallback.short,
      balance: Number(getField(r, 'Balance') ?? 0),
    };
  });
}

function asArray(data: unknown): Record<string, unknown>[] {
  if (Array.isArray(data)) return data as Record<string, unknown>[];
  if (data && typeof data === 'object') {
    const arr = Object.values(data as Record<string, unknown>).find(Array.isArray);
    if (arr) return arr as Record<string, unknown>[];
  }
  return [];
}
