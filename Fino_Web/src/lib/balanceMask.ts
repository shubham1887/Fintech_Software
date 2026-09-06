import { useCallback, useEffect, useState } from 'react';

/**
 * "Hide my balance" toggle, shared across every component that shows money.
 *
 * A shop counter is a public place, so the retailer can blank the figures. The home card and
 * the app-bar strip both render balances, so the flag cannot live in either one's local state —
 * they would drift apart. It is one localStorage key plus an event so every mounted reader
 * updates together.
 */

const KEY = 'fino.balanceHidden';
const EVENT = 'fino:balance-mask';

function read(): boolean {
  try {
    return localStorage.getItem(KEY) === '1';
  } catch {
    return false; // private mode / storage blocked — just show it
  }
}

export function useBalanceMasked(): [boolean, () => void] {
  const [masked, setMasked] = useState(read);

  useEffect(() => {
    const sync = () => setMasked(read());
    window.addEventListener(EVENT, sync);
    // Another tab (or the same app in a second window) changed it.
    window.addEventListener('storage', sync);
    return () => {
      window.removeEventListener(EVENT, sync);
      window.removeEventListener('storage', sync);
    };
  }, []);

  const toggle = useCallback(() => {
    const next = !read();
    try {
      localStorage.setItem(KEY, next ? '1' : '0');
    } catch {
      /* session-only is fine */
    }
    window.dispatchEvent(new Event(EVENT));
  }, []);

  return [masked, toggle];
}

/** What to render in place of an amount while masking is on. */
export const MASK = '••••••';
