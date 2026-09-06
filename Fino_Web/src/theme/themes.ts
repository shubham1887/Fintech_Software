/**
 * White-label theme presets. Each preset is a full brand colour ramp (50–950) that
 * gets injected as runtime CSS variables (--color-brand-*), so the same codebase can
 * be re-branded per client by changing `public/config.json` (theme key or custom colours).
 *
 * A preset may also repaint the app's dark surfaces (`navbar` / `appbar` / `fore`). That is
 * what makes the seasonal skins read as a different *look* rather than just a different
 * button colour — the header is the largest coloured area on a phone. Those values sit
 * BELOW the user's own Theme Studio overrides and ABOVE the server defaults, so picking a
 * theme visibly changes the header while a hand-tuned override still wins.
 */
export type Ramp = Record<string, string>;

export interface ThemePreset {
  key: string;
  label: string;
  /** Representative swatch colour (== brand 600) for the picker. */
  swatch: string;
  brand: Ramp;

  /** Optional dark-surface overrides — sidebar, app bar, and the text on them. */
  navbar?: string;
  appbar?: string;
  fore?: string;

  /** One-line description shown under the label in the picker. */
  note?: string;

  /**
   * Seasonal skins are grouped separately in the picker. `window` (MM-DD, inclusive, may
   * wrap the year end) drives the "In season" badge. Festivals that follow the lunar
   * calendar move every year, so they carry no window — they stay pickable year round.
   */
  seasonal?: boolean;
  window?: { from: string; to: string };
}

/* ------------------------------------------------------------ brand presets */

const azure: Ramp = {
  '50': '#eef3ff', '100': '#dbe4ff', '200': '#bccdff', '300': '#92acff',
  '400': '#6280ff', '500': '#3d59ff', '600': '#2a3df0', '700': '#2230cf',
  '800': '#1f2ca6', '900': '#1f2c83', '950': '#161b4c',
};

const emerald: Ramp = {
  '50': '#ecfdf5', '100': '#d1fae5', '200': '#a7f3d0', '300': '#6ee7b7',
  '400': '#34d399', '500': '#10b981', '600': '#059669', '700': '#047857',
  '800': '#065f46', '900': '#064e3b', '950': '#022c22',
};

const violet: Ramp = {
  '50': '#f5f3ff', '100': '#ede9fe', '200': '#ddd6fe', '300': '#c4b5fd',
  '400': '#a78bfa', '500': '#8b5cf6', '600': '#7c3aed', '700': '#6d28d9',
  '800': '#5b21b6', '900': '#4c1d95', '950': '#2e1065',
};

const sunset: Ramp = {
  '50': '#fff7ed', '100': '#ffedd5', '200': '#fed7aa', '300': '#fdba74',
  '400': '#fb923c', '500': '#f97316', '600': '#ea580c', '700': '#c2410c',
  '800': '#9a3412', '900': '#7c2d12', '950': '#431407',
};

const rose: Ramp = {
  '50': '#fff1f2', '100': '#ffe4e6', '200': '#fecdd3', '300': '#fda4af',
  '400': '#fb7185', '500': '#f43f5e', '600': '#e11d48', '700': '#be123c',
  '800': '#9f1239', '900': '#881337', '950': '#4c0519',
};

const ocean: Ramp = {
  '50': '#ecfeff', '100': '#cffafe', '200': '#a5f3fc', '300': '#67e8f9',
  '400': '#22d3ee', '500': '#06b6d4', '600': '#0891b2', '700': '#0e7490',
  '800': '#155e75', '900': '#164e63', '950': '#083344',
};

/* --------------------------------------------------------- seasonal skins */

const diwali: Ramp = {
  '50': '#fffbeb', '100': '#fef3c7', '200': '#fde68a', '300': '#fcd34d',
  '400': '#fbbf24', '500': '#f59e0b', '600': '#d97706', '700': '#b45309',
  '800': '#92400e', '900': '#78350f', '950': '#451a03',
};

const holi: Ramp = {
  '50': '#fdf4ff', '100': '#fae8ff', '200': '#f5d0fe', '300': '#f0abfc',
  '400': '#e879f9', '500': '#d946ef', '600': '#c026d3', '700': '#a21caf',
  '800': '#86198f', '900': '#701a75', '950': '#4a044e',
};

const saffron: Ramp = {
  '50': '#fff8ed', '100': '#ffefd4', '200': '#ffdca8', '300': '#ffc271',
  '400': '#ff9f38', '500': '#f97e12', '600': '#e35f08', '700': '#bc450a',
  '800': '#963710', '900': '#7a2f10', '950': '#421506',
};

const evergreen: Ramp = {
  '50': '#fef2f2', '100': '#fee2e2', '200': '#fecaca', '300': '#fca5a5',
  '400': '#f87171', '500': '#ef4444', '600': '#dc2626', '700': '#b91c1c',
  '800': '#991b1b', '900': '#7f1d1d', '950': '#450a0a',
};

const crescent: Ramp = {
  '50': '#f0fdfa', '100': '#ccfbf1', '200': '#99f6e4', '300': '#5eead4',
  '400': '#2dd4bf', '500': '#14b8a6', '600': '#0d9488', '700': '#0f766e',
  '800': '#115e59', '900': '#134e4a', '950': '#042f2e',
};

export const THEMES: ThemePreset[] = [
  { key: 'azure', label: 'Azure', swatch: azure['600'], brand: azure, note: 'Default — royal blue' },
  { key: 'emerald', label: 'Emerald', swatch: emerald['600'], brand: emerald, note: 'Calm green' },
  { key: 'violet', label: 'Violet', swatch: violet['600'], brand: violet, note: 'Premium purple' },
  { key: 'sunset', label: 'Sunset', swatch: sunset['600'], brand: sunset, note: 'Warm orange' },
  { key: 'rose', label: 'Rose', swatch: rose['600'], brand: rose, note: 'Bold crimson' },
  { key: 'ocean', label: 'Ocean', swatch: ocean['600'], brand: ocean, note: 'Cool cyan' },

  {
    key: 'diwali', label: 'Diwali', swatch: diwali['600'], brand: diwali,
    navbar: '#2a0e0b', appbar: '#3a1410', fore: '#ffffff',
    note: 'Gold on deep maroon', seasonal: true,
    window: { from: '10-10', to: '11-20' },
  },
  {
    key: 'holi', label: 'Holi', swatch: holi['600'], brand: holi,
    navbar: '#2e1065', appbar: '#3b1178', fore: '#ffffff',
    note: 'Magenta on violet', seasonal: true,
    window: { from: '02-25', to: '03-25' },
  },
  {
    key: 'independence', label: 'Independence', swatch: saffron['600'], brand: saffron,
    navbar: '#0b3d2b', appbar: '#0f4a35', fore: '#ffffff',
    note: 'Saffron on forest green', seasonal: true,
    window: { from: '08-05', to: '08-20' },
  },
  {
    key: 'festive', label: 'Festive', swatch: evergreen['600'], brand: evergreen,
    navbar: '#08351f', appbar: '#0b442a', fore: '#ffffff',
    note: 'Christmas & New Year', seasonal: true,
    window: { from: '12-10', to: '01-05' },
  },
  {
    key: 'crescent', label: 'Crescent', swatch: crescent['600'], brand: crescent,
    navbar: '#042f2e', appbar: '#06403e', fore: '#ffffff',
    // Eid follows the lunar calendar, so its date moves every year — no fixed window.
    note: 'Eid — teal on deep green', seasonal: true,
  },
];

export const DEFAULT_THEME = 'azure';

export function getTheme(key: string | undefined): ThemePreset {
  return THEMES.find((t) => t.key === key) ?? THEMES[0];
}

/** MM-DD of a date, for window comparison. */
function monthDay(d: Date): string {
  return `${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

/** Is this seasonal skin inside its window today? Windows may wrap the year end. */
export function isInSeason(preset: ThemePreset, now: Date = new Date()): boolean {
  const w = preset.window;
  if (!w) return false;
  const today = monthDay(now);
  return w.from <= w.to
    ? today >= w.from && today <= w.to
    : today >= w.from || today <= w.to; // e.g. 12-10 → 01-05
}

/** The seasonal skin whose window covers today, if any. */
export function seasonalToday(now: Date = new Date()): ThemePreset | undefined {
  return THEMES.find((t) => t.seasonal && isInSeason(t, now));
}
