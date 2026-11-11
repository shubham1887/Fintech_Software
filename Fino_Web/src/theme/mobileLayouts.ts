/**
 * Phone / Android app layout presets.
 *
 * The Android app is this console inside a WebView, so "the app's design" IS this file.
 * That is what makes it changeable: the admin picks a preset in Settings → Appearance, it
 * is published as a UISetting row, and every phone picks it up on the next launch. No APK
 * rebuild, no Play Store review, no per-client code — which matters because this platform
 * is installed fresh for each client by someone non-technical.
 *
 * A preset is a COMPOSITION, not a stylesheet and not a set of knobs. The first version of
 * this file only varied corner radius, column count and a few toggles, so all five presets
 * rendered the same screen — dark balance card, icon grid, activity list — and an operator
 * comparing them could not tell them apart. The screen is now assembled from interchangeable
 * blocks instead:
 *
 *   balanceStyle   card | hero | strip | plain | split      how money is presented
 *   serviceStyle   grid | dense | cards | list | sections | focus   how the catalogue is browsed
 *   statsStyle     strip | cards                            how today's figures read
 *   activityStyle  list | compact | timeline | table        how recent transactions read
 *
 * Each value is a genuinely different component (see `pages/mobile/home/*`), so two presets
 * differ in structure and information density, not in decoration. Adding a seventh preset is
 * one entry here; adding a new *kind* of block is one case in the matching block file.
 *
 * Every field can also be overridden on its own, so an operator who likes "vibrant" but
 * wants three columns is not forced to fork a preset.
 */

export type MobileLayoutKey = 'classic' | 'compact' | 'vibrant' | 'minimal' | 'business' | 'focus';
export type HeaderStyle = 'solid' | 'gradient' | 'glass';
export type TileShape = 'rounded' | 'circle' | 'square' | 'card';
export type TabStyle = 'labelled' | 'icons' | 'floating';

/**
 * How the balance block on Home is drawn.
 *  - `card`  the dark anchor card: total, per-wallet row, two actions. The default.
 *  - `hero`  full-bleed gradient banner that continues the app bar — consumer-app look.
 *  - `strip` one slim rail: total left, wallets right. Buys back ~120px of screen.
 *  - `plain` no surface at all; large numerals straight on the canvas with hairline rules.
 *  - `split` a white panel: total on the left, each wallet as its own row on the right.
 */
export type BalanceStyle = 'card' | 'hero' | 'strip' | 'plain' | 'split';

/**
 * How the service catalogue is browsed on Home.
 *  - `grid`     icon tiles in a card, the classic recharge-app grid.
 *  - `dense`    edge-to-edge small tiles, no card chrome, maximum services above the fold.
 *  - `cards`    the first services as wide colour-filled cards, the rest as a small grid.
 *  - `list`     one row per service with name + chevron. Reads as a tool, not a shop.
 *  - `sections` grouped under the admin's own category headings.
 *  - `focus`    one column of large full-width targets, for field use and first-time users.
 */
export type ServiceStyle = 'grid' | 'dense' | 'cards' | 'list' | 'sections' | 'focus';

/** Today's figures: one slim four-up row, or 2×2 cards with counts. */
export type StatsStyle = 'strip' | 'cards';

/**
 * Recent transactions.
 *  - `list`     dot + title + amount, the readable default.
 *  - `compact`  two-up rows, tighter leading, more rows per screen.
 *  - `timeline` a hairline rail down the left with time markers — quiet, chronological.
 *  - `table`    aligned columns (time / service / amount / status) for back-office reading.
 */
export type ActivityStyle = 'list' | 'compact' | 'timeline' | 'table';

export interface MobileLayout {
  key: MobileLayoutKey;
  label: string;
  /** One line the admin reads in the picker. */
  note: string;

  headerStyle: HeaderStyle;
  tileShape: TileShape;
  /** Service tiles per row on the home grid. */
  gridColumns: 3 | 4 | 5;
  /** The balance block on Home. Off ⇒ the app-bar strip carries the balance everywhere. */
  showBalanceCard: boolean;
  /** Today's figures block. */
  showTodayStats: boolean;
  /** Recent-activity rows shown on Home. */
  recentCount: number;
  tabStyle: TabStyle;
  /** Tiles before the grid folds into "All services". */
  visibleTiles: number;
  /** Tint the service tiles per service (vibrant) or keep one neutral tone (minimal). */
  colourfulTiles: boolean;

  /* ----- composition: which block renders in each slot, and in what order ----- */
  balanceStyle: BalanceStyle;
  serviceStyle: ServiceStyle;
  statsStyle: StatsStyle;
  activityStyle: ActivityStyle;
  /** Figures above the services — for an operator who watches numbers, not tiles. */
  statsFirst: boolean;
  /** Vertical rhythm between blocks. Dense layouts breathe less, on purpose. */
  density: 'comfortable' | 'compact';
}

export const MOBILE_LAYOUTS: Record<MobileLayoutKey, MobileLayout> = {
  classic: {
    key: 'classic',
    label: 'Classic',
    note: 'Dark balance card, icon grid, activity list. The familiar recharge-app home — safest default.',
    headerStyle: 'solid',
    tileShape: 'rounded',
    gridColumns: 4,
    showBalanceCard: true,
    showTodayStats: true,
    recentCount: 5,
    tabStyle: 'labelled',
    visibleTiles: 7,
    colourfulTiles: true,
    balanceStyle: 'card',
    serviceStyle: 'grid',
    statsStyle: 'strip',
    activityStyle: 'list',
    statsFirst: false,
    density: 'comfortable',
  },
  compact: {
    key: 'compact',
    label: 'Compact',
    note: 'Balance shrinks to one rail and the services run edge-to-edge — roughly twice as much above the fold.',
    headerStyle: 'solid',
    tileShape: 'circle',
    gridColumns: 5,
    showBalanceCard: true,
    showTodayStats: false,
    recentCount: 7,
    tabStyle: 'icons',
    visibleTiles: 14,
    colourfulTiles: true,
    balanceStyle: 'strip',
    serviceStyle: 'dense',
    statsStyle: 'strip',
    activityStyle: 'compact',
    statsFirst: false,
    density: 'compact',
  },
  vibrant: {
    key: 'vibrant',
    label: 'Vibrant',
    note: 'Full-bleed gradient banner and wide colour-filled service cards. Reads like a consumer payments app.',
    headerStyle: 'gradient',
    tileShape: 'card',
    gridColumns: 4,
    showBalanceCard: true,
    showTodayStats: true,
    recentCount: 5,
    tabStyle: 'floating',
    visibleTiles: 8,
    colourfulTiles: true,
    balanceStyle: 'hero',
    serviceStyle: 'cards',
    statsStyle: 'strip',
    activityStyle: 'list',
    statsFirst: false,
    density: 'comfortable',
  },
  minimal: {
    key: 'minimal',
    label: 'Minimal',
    note: 'No cards, no colour: typographic balance on bare canvas and a plain list of services.',
    headerStyle: 'solid',
    tileShape: 'square',
    gridColumns: 4,
    showBalanceCard: true,
    showTodayStats: false,
    recentCount: 7,
    tabStyle: 'labelled',
    visibleTiles: 8,
    colourfulTiles: false,
    balanceStyle: 'plain',
    serviceStyle: 'list',
    statsStyle: 'strip',
    activityStyle: 'timeline',
    statsFirst: false,
    density: 'comfortable',
  },
  business: {
    key: 'business',
    label: 'Business',
    note: 'Numbers first: 2×2 figures with counts, services under category headings, activity as a column table.',
    headerStyle: 'glass',
    tileShape: 'rounded',
    gridColumns: 4,
    showBalanceCard: true,
    showTodayStats: true,
    recentCount: 8,
    tabStyle: 'labelled',
    visibleTiles: 8,
    colourfulTiles: false,
    balanceStyle: 'split',
    serviceStyle: 'sections',
    statsStyle: 'cards',
    activityStyle: 'table',
    statsFirst: true,
    density: 'comfortable',
  },
  focus: {
    key: 'focus',
    label: 'Focus',
    note: 'One service per row, large tap targets, short activity list. For field agents and first-time users.',
    headerStyle: 'solid',
    tileShape: 'rounded',
    gridColumns: 3,
    showBalanceCard: true,
    showTodayStats: false,
    recentCount: 4,
    tabStyle: 'labelled',
    visibleTiles: 6,
    colourfulTiles: true,
    balanceStyle: 'card',
    serviceStyle: 'focus',
    statsStyle: 'strip',
    activityStyle: 'compact',
    statsFirst: false,
    density: 'comfortable',
  },
};

export const DEFAULT_MOBILE_LAYOUT: MobileLayoutKey = 'classic';

export const MOBILE_LAYOUT_LIST = Object.values(MOBILE_LAYOUTS);

/** UISetting keys (lowercased, the shape UISettingsProvider hands out). */
const KEYS = {
  layout: 'mobilelayout',
  header: 'mobileheaderstyle',
  tile: 'mobiletileshape',
  columns: 'mobilegridcolumns',
  balanceCard: 'mobileshowbalancecard',
  todayStats: 'mobileshowtodaystats',
  tabs: 'mobiletabstyle',
  balanceStyle: 'mobilebalancestyle',
  serviceStyle: 'mobileservicestyle',
  activityStyle: 'mobileactivitystyle',
} as const;

function oneOf<T extends string>(value: string | undefined, allowed: readonly T[], fallback: T): T {
  const v = (value ?? '').trim().toLowerCase();
  return allowed.find((a) => a.toLowerCase() === v) ?? fallback;
}

function bool(value: string | undefined, fallback: boolean): boolean {
  const v = (value ?? '').trim().toLowerCase();
  if (v === 'true' || v === '1' || v === 'yes') return true;
  if (v === 'false' || v === '0' || v === 'no') return false;
  return fallback;
}

export const BALANCE_STYLES: readonly BalanceStyle[] = ['card', 'hero', 'strip', 'plain', 'split'];
export const SERVICE_STYLES: readonly ServiceStyle[] = ['grid', 'dense', 'cards', 'list', 'sections', 'focus'];
export const ACTIVITY_STYLES: readonly ActivityStyle[] = ['list', 'compact', 'timeline', 'table'];

/**
 * Resolve the effective layout from the admin's published UISetting rows.
 *
 * Precedence: the preset supplies every field, then any individually-set key overrides it.
 * An unknown or missing value never breaks the app — it falls back to the preset, and the
 * preset falls back to `classic`. A phone must not render blank because a settings row was
 * typo'd.
 */
export function resolveMobileLayout(settings: Record<string, string>): MobileLayout {
  const key = oneOf<MobileLayoutKey>(
    settings[KEYS.layout],
    Object.keys(MOBILE_LAYOUTS) as MobileLayoutKey[],
    DEFAULT_MOBILE_LAYOUT,
  );
  const base = MOBILE_LAYOUTS[key];

  const columnsRaw = Number.parseInt((settings[KEYS.columns] ?? '').trim(), 10);
  const columns = columnsRaw === 3 || columnsRaw === 4 || columnsRaw === 5 ? columnsRaw : base.gridColumns;

  return {
    ...base,
    headerStyle: oneOf<HeaderStyle>(settings[KEYS.header], ['solid', 'gradient', 'glass'], base.headerStyle),
    tileShape: oneOf<TileShape>(settings[KEYS.tile], ['rounded', 'circle', 'square', 'card'], base.tileShape),
    gridColumns: columns,
    showBalanceCard: bool(settings[KEYS.balanceCard], base.showBalanceCard),
    showTodayStats: bool(settings[KEYS.todayStats], base.showTodayStats),
    tabStyle: oneOf<TabStyle>(settings[KEYS.tabs], ['labelled', 'icons', 'floating'], base.tabStyle),
    balanceStyle: oneOf<BalanceStyle>(settings[KEYS.balanceStyle], BALANCE_STYLES, base.balanceStyle),
    serviceStyle: oneOf<ServiceStyle>(settings[KEYS.serviceStyle], SERVICE_STYLES, base.serviceStyle),
    activityStyle: oneOf<ActivityStyle>(settings[KEYS.activityStyle], ACTIVITY_STYLES, base.activityStyle),
  };
}

/* --------------------------------------------------- class-name helpers */

/** Tailwind grid-columns class for the service grid. Written out so JIT keeps them. */
export function gridColsClass(columns: 3 | 4 | 5): string {
  return columns === 3 ? 'grid-cols-3' : columns === 5 ? 'grid-cols-5' : 'grid-cols-4';
}

/** Tile corner treatment. `card` also gets a border + shadow from the caller. */
export function tileShapeClass(shape: TileShape): string {
  switch (shape) {
    case 'circle':
      return 'rounded-full';
    case 'square':
      return 'rounded-md';
    case 'card':
      return 'rounded-2xl';
    default:
      return 'rounded-xl';
  }
}

/** Gap between the home blocks. Compact layouts give up air to give back rows. */
export function blockGapClass(density: MobileLayout['density']): string {
  return density === 'compact' ? 'space-y-3' : 'space-y-4';
}

/**
 * Inline style for the app bar. Gradient and glass are built from the SAME
 * `--ui-appbar` the operator already sets, so a preset never fights the brand colour.
 */
export function headerStyleFor(style: HeaderStyle): React.CSSProperties {
  switch (style) {
    case 'gradient':
      return {
        backgroundImage:
          'linear-gradient(135deg, var(--ui-appbar) 0%, color-mix(in oklab, var(--ui-appbar) 55%, var(--color-brand-500)) 100%)',
      };
    case 'glass':
      return {
        backgroundColor: 'color-mix(in oklab, var(--ui-appbar) 88%, transparent)',
        backdropFilter: 'saturate(160%) blur(10px)',
      };
    default:
      return { backgroundColor: 'var(--ui-appbar)' };
  }
}

/**
 * The hero banner's surface — continues the app bar downward so the two read as one piece of
 * chrome, then leans into the brand colour at the bottom edge. Built from the operator's own
 * `--ui-appbar`, never a hard-coded palette.
 */
export function heroSurfaceStyle(): React.CSSProperties {
  return {
    backgroundImage:
      'linear-gradient(165deg, var(--ui-appbar) 0%, color-mix(in oklab, var(--ui-appbar) 62%, var(--color-brand-600)) 55%, color-mix(in oklab, var(--ui-appbar) 30%, var(--color-brand-500)) 100%)',
  };
}
