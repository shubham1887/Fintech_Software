import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useLayoutEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { THEMES, type ThemePreset } from './themes';
import { getConfig } from '@/lib/runtimeConfig';
import {
  onThemeChange,
  reapplyTheme,
  readLocalTheme,
  resolveTheme,
  setConfigColors,
  setConfigPreset,
  writeLocalTheme,
} from './themeEngine';

interface BrandConfig {
  appName: string;
  tagline: string;
  logoUrl: string;
  /** The square app artwork, shown instead of the wide logo inside the phone app. */
  appIconUrl: string;
  loginHeadline: string;
  loginSubtext: string;
}

const DEFAULT_BRAND: BrandConfig = {
  appName: 'Fino',
  tagline: 'Console',
  logoUrl: '',
  appIconUrl: '',
  loginHeadline: 'The control room for your fintech network.',
  loginSubtext:
    'Recharges, money transfer, AEPS, wallets and settlements — one secure console for every role in your distribution chain.',
};

interface BrandContextValue extends BrandConfig {
  themeKey: string;
  /** Custom seed colour in effect (Theme Studio), undefined when a preset is active. */
  customPrimary?: string;
  themes: ThemePreset[];
  setTheme: (key: string) => void;
}

const BrandContext = createContext<BrandContextValue | undefined>(undefined);

export function BrandProvider({ children }: { children: ReactNode }) {
  const [brand, setBrand] = useState<BrandConfig>(DEFAULT_BRAND);
  const [resolved, setResolved] = useState(() => resolveTheme());

  // Apply the persisted theme before paint, and track engine re-applications
  // (server settings arriving, Theme Studio saves, palette switches).
  useLayoutEffect(() => {
    reapplyTheme();
    return onThemeChange(() => setResolved(resolveTheme()));
  }, []);

  // Per-client branding from the runtime config (public/config.json), already loaded before
  // the app mounted (see main.tsx → loadRuntimeConfig). Ship-time default layer — server
  // settings and the user's own choices still win over it.
  useEffect(() => {
    const cfg = getConfig();
    setBrand({
      appName: cfg.appName || DEFAULT_BRAND.appName,
      tagline: cfg.tagline || DEFAULT_BRAND.tagline,
      logoUrl: cfg.logoUrl || '',
      appIconUrl: cfg.appIconUrl || '',
      loginHeadline: cfg.loginHeadline || DEFAULT_BRAND.loginHeadline,
      loginSubtext: cfg.loginSubtext || DEFAULT_BRAND.loginSubtext,
    });
    if (cfg.theme) setConfigPreset(cfg.theme);
    setConfigColors({
      primary: cfg.brandColor,
      navbar: cfg.navbarColor,
      appbar: cfg.appbarColor,
    });
  }, []);

  useEffect(() => {
    document.title = `${brand.appName} · ${brand.tagline}`;
  }, [brand]);

  const setTheme = useCallback((key: string) => {
    // Picking a preset clears any custom seed so the preset ramp shows as-is.
    writeLocalTheme({ ...readLocalTheme(), preset: key, primary: undefined });
  }, []);

  const value = useMemo<BrandContextValue>(
    () => ({
      ...brand,
      themeKey: resolved.preset,
      customPrimary: resolved.primary,
      themes: THEMES,
      setTheme,
    }),
    [brand, resolved, setTheme],
  );

  return <BrandContext.Provider value={value}>{children}</BrandContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useBrand(): BrandContextValue {
  const ctx = useContext(BrandContext);
  if (!ctx) throw new Error('useBrand must be used within a BrandProvider');
  return ctx;
}
