/**
 * Runtime deployment config, loaded from the static `public/config.json` at startup —
 * BEFORE the app (and therefore apiClient / crypto) is imported. This is what makes the
 * build white-label / resale-friendly: the API URL and the AES transport key/IV are read
 * at run time from a file the reseller edits after deployment, so shipping to a new domain
 * is a one-file change with NO rebuild.
 *
 * Precedence for each value: config.json (if the field is non-empty) → build-time .env
 * (VITE_*) → hard-coded default. So dev keeps using .env, and a deployed copy is retargeted
 * by editing dist/config.json only.
 */
export interface RuntimeConfig {
  // Branding (also consumed by BrandProvider)
  appName?: string;
  tagline?: string;
  logoUrl?: string;
  /**
   * The client's square app artwork (the Android launcher tile). Shown INSTEAD of the wide logo
   * inside the phone app, where a 3:1 wordmark has nowhere to go and the icon is what people
   * already tapped to get there. Optional: without it the app shows the wide logo as before.
   */
  appIconUrl?: string;
  theme?: string;
  /**
   * Per-client colours, normally written by the installer after reading the client's logo, so a
   * fresh deployment already looks like the company it belongs to instead of like the demo.
   * `brandColor` seeds the whole 50-950 brand ramp; the other two paint the dark surfaces.
   * All three sit below the admin's published theme and the user's own Theme Studio choices.
   */
  brandColor?: string;  // hex, e.g. "#1d6ff2"
  navbarColor?: string; // hex — sidebar
  appbarColor?: string; // hex — app bar / mobile header
  loginHeadline?: string;
  loginSubtext?: string;
  // Deployment — the resale knobs
  apiUrl?: string; // e.g. "https://api.yourclient.com/api/"
  encKey?: string; // 32-char AES key, MUST match Fino_API appsettings Encryption:Key
  encIv?: string;  // 16-char AES IV,  MUST match Fino_API appsettings Encryption:IV
}

let cfg: RuntimeConfig = {};
let loaded = false;

/** Fetch config.json once. Never throws — a missing/broken file just leaves env+defaults in play. */
export async function loadRuntimeConfig(): Promise<RuntimeConfig> {
  if (loaded) return cfg;
  try {
    const base = import.meta.env.BASE_URL ?? '/';
    const res = await fetch(`${base}config.json`, { cache: 'no-cache' });
    if (res.ok) {
      const json = await res.json();
      if (json && typeof json === 'object') cfg = json as RuntimeConfig;
    }
  } catch {
    /* keep empty; VITE_* / defaults apply */
  }
  loaded = true;
  return cfg;
}

/** The config loaded at startup. Safe to call synchronously after loadRuntimeConfig() resolved. */
export function getConfig(): RuntimeConfig {
  return cfg;
}

/** Trimmed non-empty string, else undefined — so blank config fields fall through to the next source. */
export function cfgValue(v: string | undefined): string | undefined {
  const s = (v ?? '').trim();
  return s.length > 0 ? s : undefined;
}
