/**
 * Central theme engine — the single writer for all runtime theme CSS variables.
 *
 * Layers (weakest → strongest):
 *   1. built-in defaults (index.css tokens)
 *   2. public/config.json  — per-client default preset (white-label ship-time default)
 *   3. server UISetting keys — admin-published portal theme (UISetting/GetAll)
 *   4. device-local overrides — the signed-in user's own Theme Studio choices
 *
 * Server keys understood (case-insensitive): ThemePreset, PrimaryColor, PrimaryHoverColor,
 * NavBarColor, AppBarColor, ForeColor, SecondaryColor, ReportFontSize, ConsumerNumberFontSize.
 */
import { DEFAULT_THEME, getTheme, THEMES } from './themes';
import { isHexColor, rampFromSeed } from './colorRamp';

export interface LocalTheme {
  /** Preset key from themes.ts. */
  preset?: string;
  /** Custom brand seed colour (hex); when set it wins over `preset`. */
  primary?: string;
  navbar?: string;
  appbar?: string;
  fore?: string;
  secondary?: string;
  reportFs?: number;
  consumerFs?: number;
}

export interface ResolvedTheme {
  preset: string;
  /** Custom seed colour actually in effect (undefined → preset ramp is used). */
  primary?: string;
  /** Server-only override of the brand 600 shade (legacy PrimaryHoverColor key). */
  hover?: string;
  navbar: string;
  appbar: string;
  fore: string;
  secondary: string;
  reportFs: number;
  consumerFs: number;
}

/** Mirrors the fallback values declared in index.css @theme. */
export const THEME_DEFAULTS = {
  navbar: '#0a0e1c',
  appbar: '#111729',
  fore: '#ffffff',
  secondary: '#0ea5e9',
  reportFs: 14,
  consumerFs: 18,
} as const;

const LOCAL_KEY = 'fino.theme.local';
/** Legacy key holding just the preset name — kept in sync for older sessions. */
const LEGACY_PRESET_KEY = 'fino.theme';

let configPreset: string | undefined;
/** Per-client colours from config.json (installer-written, derived from the client's logo). */
let configColors: { primary?: string; navbar?: string; appbar?: string } = {};
/** Lowercased key → value map from UISetting/GetAll (set by UISettingsProvider). */
let serverMap: Record<string, string> = {};

type Listener = () => void;
const listeners = new Set<Listener>();

/** Subscribe to theme re-applications (returns unsubscribe). */
export function onThemeChange(fn: Listener): () => void {
  listeners.add(fn);
  return () => listeners.delete(fn);
}

// ---------------------------------------------------------------- storage

export function readLocalTheme(): LocalTheme {
  try {
    const raw = localStorage.getItem(LOCAL_KEY);
    if (raw) return JSON.parse(raw) as LocalTheme;
  } catch {
    /* corrupted JSON — fall through to legacy/empty */
  }
  const legacy = localStorage.getItem(LEGACY_PRESET_KEY);
  return legacy ? { preset: legacy } : {};
}

export function writeLocalTheme(t: LocalTheme): void {
  const clean = Object.fromEntries(
    Object.entries(t).filter(([, v]) => v !== undefined && v !== ''),
  ) as LocalTheme;
  if (Object.keys(clean).length === 0) {
    clearLocalTheme();
    return;
  }
  localStorage.setItem(LOCAL_KEY, JSON.stringify(clean));
  if (clean.preset) localStorage.setItem(LEGACY_PRESET_KEY, clean.preset);
  else localStorage.removeItem(LEGACY_PRESET_KEY);
  reapplyTheme();
}

export function clearLocalTheme(): void {
  localStorage.removeItem(LOCAL_KEY);
  localStorage.removeItem(LEGACY_PRESET_KEY);
  reapplyTheme();
}

export function hasLocalOverrides(): boolean {
  return Object.keys(readLocalTheme()).length > 0;
}

// ---------------------------------------------------------------- layer inputs

export function setConfigPreset(key: string | undefined): void {
  configPreset = key;
  reapplyTheme();
}

/** Ship-time client colours (config.json). Only valid hex values are kept. */
export function setConfigColors(c: { primary?: string; navbar?: string; appbar?: string }): void {
  configColors = {
    primary: hexFrom(c.primary),
    navbar: hexFrom(c.navbar),
    appbar: hexFrom(c.appbar),
  };
  reapplyTheme();
}

export function setServerTheme(map: Record<string, string>): void {
  serverMap = map;
  reapplyTheme();
}

// ---------------------------------------------------------------- resolution

const srv = (k: string) => serverMap[k]?.trim() || undefined;

function hexFrom(v: string | undefined): string | undefined {
  return v && isHexColor(v) ? v : undefined;
}

function sizeFrom(v: string | undefined): number | undefined {
  if (!v) return undefined;
  const n = Number(v.replace(/px$/i, '').trim());
  return Number.isFinite(n) && n >= 8 && n <= 40 ? n : undefined;
}

/** True if the key names one of the built-in presets. */
function isPresetKey(key: string | undefined): key is string {
  return !!key && THEMES.some((t) => t.key === key);
}

/**
 * Merge all layers into the effective theme. Pass a draft to resolve "what would
 * this look like"; defaults to the persisted device-local overrides.
 */
export function resolveTheme(local: LocalTheme = readLocalTheme()): ResolvedTheme {
  const localBrand = !!(local.primary || isPresetKey(local.preset));
  const serverPreset = isPresetKey(srv('themepreset')) ? srv('themepreset') : undefined;

  let preset = DEFAULT_THEME;
  let primary: string | undefined;
  if (local.primary && isHexColor(local.primary)) {
    primary = local.primary;
    preset = isPresetKey(local.preset) ? local.preset : serverPreset ?? configPreset ?? DEFAULT_THEME;
  } else if (isPresetKey(local.preset)) {
    preset = local.preset;
  } else if (serverPreset) {
    preset = serverPreset;
  } else if (hexFrom(srv('primarycolor'))) {
    primary = hexFrom(srv('primarycolor'));
    preset = configPreset && isPresetKey(configPreset) ? configPreset : DEFAULT_THEME;
  } else if (configColors.primary) {
    // The client's own colour, taken off their logo at install time.
    primary = configColors.primary;
    preset = isPresetKey(configPreset) ? configPreset : DEFAULT_THEME;
  } else if (isPresetKey(configPreset)) {
    preset = configPreset;
  }

  // A preset may repaint the dark surfaces (the seasonal skins do). Those sit below the
  // user's own Theme Studio overrides and above the server defaults, so picking a theme
  // visibly changes the header while a hand-tuned override still wins.
  //
  // Skipped when a custom seed colour is in effect: the user has defined their own brand,
  // so dragging a festival's header colour along with it would be a surprise.
  const skin = primary ? undefined : getTheme(preset);

  return {
    preset,
    primary,
    hover: localBrand ? undefined : hexFrom(srv('primaryhovercolor')),
    navbar: local.navbar ?? skin?.navbar ?? hexFrom(srv('navbarcolor')) ?? configColors.navbar ?? THEME_DEFAULTS.navbar,
    appbar: local.appbar ?? skin?.appbar ?? hexFrom(srv('appbarcolor')) ?? configColors.appbar ?? THEME_DEFAULTS.appbar,
    fore: local.fore ?? skin?.fore ?? hexFrom(srv('forecolor')) ?? THEME_DEFAULTS.fore,
    secondary: local.secondary ?? hexFrom(srv('secondarycolor')) ?? THEME_DEFAULTS.secondary,
    reportFs: local.reportFs ?? sizeFrom(srv('reportfontsize')) ?? THEME_DEFAULTS.reportFs,
    consumerFs: local.consumerFs ?? sizeFrom(srv('consumernumberfontsize')) ?? THEME_DEFAULTS.consumerFs,
  };
}

/** The brand colour ramp a resolved theme produces (custom seed or preset). */
export function rampOf(r: Pick<ResolvedTheme, 'preset' | 'primary'>): Record<string, string> {
  return r.primary ? rampFromSeed(r.primary) : getTheme(r.preset).brand;
}

// ---------------------------------------------------------------- application

function applyResolved(r: ResolvedTheme): void {
  const root = document.documentElement;
  for (const [shade, val] of Object.entries(rampOf(r))) {
    root.style.setProperty(`--color-brand-${shade}`, val);
  }
  if (r.hover) root.style.setProperty('--color-brand-600', r.hover);
  root.style.setProperty('--ui-navbar', r.navbar);
  root.style.setProperty('--ui-appbar', r.appbar);
  root.style.setProperty('--ui-fore', r.fore);
  root.style.setProperty('--ui-secondary', r.secondary);
  root.style.setProperty('--ui-report-fs', `${r.reportFs}px`);
  root.style.setProperty('--ui-consumer-fs', `${r.consumerFs}px`);
}

/** Re-apply the persisted theme (all layers) and notify subscribers. */
export function reapplyTheme(): void {
  applyResolved(resolveTheme());
  for (const fn of listeners) fn();
}

/** Apply a draft live WITHOUT persisting (Theme Studio preview). */
export function previewTheme(draft: LocalTheme): void {
  applyResolved(resolveTheme(draft));
}
