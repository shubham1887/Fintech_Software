import { useEffect, useState } from 'react';

/**
 * Bridge to the Android WebView shell (Fino_Android).
 *
 * The shell injects `window.NativeApp` on every page load and puts a `native-app` class on
 * <html>. Everything here degrades to a no-op in a normal browser, so the same code runs in
 * both places — never branch on "is this Android" to decide whether a feature works, only to
 * decide whether to *offer* it.
 */

export interface NativeInfo {
  platform: string;
  isNativeApp?: boolean;
  client?: string;
  brand?: string;
  packageName?: string;
  versionName?: string;
  versionCode?: number;
  androidVersion?: string;
  device?: string;
}

type HapticKind = 'light' | 'medium' | 'heavy' | 'success';

interface NativeApi {
  platform: string;
  info: NativeInfo;
  config: Record<string, string>;
  toast(message: string): void;
  haptic(kind: HapticKind): void;
  share(title: string, text: string): void;
  copy(text: string, label?: string): void;
  open(url: string): void;
  whatsapp(number: string, message: string): void;
  call(number: string): void;
  print(name: string): void;
  saveFile(name: string, mime: string, base64: string): void;
  secureScreen(on: boolean): void;
  statusBarColor(hex: string): void;
  notifications(): Promise<{ granted: boolean }>;
  biometricAvailable(): boolean;
  biometric(title: string, subtitle: string): Promise<{ ok: boolean; reason?: string }>;
  secure: {
    set(key: string, value: string): boolean;
    get(key: string): string | null;
    remove(key: string): void;
  };
  ready(): void;
  reload(): void;
  logout(): void;
  exit(): void;
  on(event: string, cb: (e: Event) => void): () => void;
}

declare global {
  interface Window {
    NativeApp?: NativeApi;
  }
}

/** The bridge, or undefined in a plain browser. */
export function native(): NativeApi | undefined {
  return typeof window === 'undefined' ? undefined : window.NativeApp;
}

/** Running inside the Android shell? */
export function isNativeApp(): boolean {
  if (typeof window === 'undefined') return false;
  return !!window.NativeApp || document.documentElement.classList.contains('native-app');
}

/**
 * `isNativeApp()` as a hook.
 *
 * The shell injects its bridge after the document loads, which can be after React's first
 * render, so a component that only read `isNativeApp()` once would decide "browser" and never
 * correct itself. This re-reads when the shell announces itself.
 */
export function useIsNativeApp(): boolean {
  const [inApp, setInApp] = useState(isNativeApp);

  useEffect(() => {
    const recompute = () => setInApp(isNativeApp());
    window.addEventListener('native:ready', recompute);
    recompute();
    return () => window.removeEventListener('native:ready', recompute);
  }, []);

  return inApp;
}

/** Short vibration on a primary action. No-op on the web. */
export function haptic(kind: HapticKind = 'light'): void {
  try {
    native()?.haptic(kind);
  } catch {
    /* bridge missing — ignore */
  }
}

/** Tell the shell its branded loading overlay can go away. */
export function nativeReady(): void {
  try {
    native()?.ready();
  } catch {
    /* ignore */
  }
}

/** Paint the Android status bar to match the app header. */
export function setNativeStatusBar(hex: string): void {
  try {
    native()?.statusBarColor(hex);
  } catch {
    /* ignore */
  }
}

/** Screenshot / screen-record block for a sensitive screen. Returns a restore function. */
export function secureScreen(on: boolean): void {
  try {
    native()?.secureScreen(on);
  } catch {
    /* ignore */
  }
}

const MOBILE_QUERY = '(max-width: 767px)';

function computeMobile(): boolean {
  if (typeof window === 'undefined') return false;
  if (isNativeApp()) return true;

  // A hidden / not-yet-laid-out embedding reports width 0, which would match the media
  // query and flash the phone layout at a desktop user. Width 0 means "don't know yet".
  if (window.innerWidth === 0) return false;

  return window.matchMedia(MOBILE_QUERY).matches;
}

/**
 * True on a phone-sized viewport OR inside the Android shell — the console then renders its
 * app layout (bottom tabs, balance card, service grid) instead of the desktop sidebar shell.
 *
 * The shell injects its bridge after the document loads, which can be *after* React's first
 * render, so we also listen for the `native:ready` event it fires.
 */
export function useIsMobileApp(): boolean {
  const [mobile, setMobile] = useState(computeMobile);

  useEffect(() => {
    const recompute = () => setMobile(computeMobile());
    const mq = window.matchMedia(MOBILE_QUERY);
    mq.addEventListener('change', recompute);
    window.addEventListener('native:ready', recompute);
    // The bridge may have landed between the initial state and this effect.
    recompute();
    return () => {
      mq.removeEventListener('change', recompute);
      window.removeEventListener('native:ready', recompute);
    };
  }, []);

  return mobile;
}
