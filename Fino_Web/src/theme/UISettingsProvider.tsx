import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getUISettings, toSettingsMap } from '@/api/uisettings.api';
import { useAuth } from '@/features/auth/AuthContext';
import { setServerTheme } from './themeEngine';

export type Orientation = 'vertical' | 'horizontal';

const LAYOUT_KEY = 'fino.layout';

interface UISettingsValue {
  settings: Record<string, string>;
  orientation: Orientation;
  setOrientation: (o: Orientation) => void;
  reload: () => void;
}

const Ctx = createContext<UISettingsValue | undefined>(undefined);

export function UISettingsProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const { data, refetch } = useQuery({
    queryKey: ['ui-settings'],
    queryFn: getUISettings,
    enabled: isAuthenticated,
    staleTime: 5 * 60_000,
  });

  const settings = useMemo(() => toSettingsMap(data ?? []), [data]);

  // Local override wins over the API value so the in-app toggle is instant + persistent.
  const [override, setOverride] = useState<Orientation | null>(
    () => (localStorage.getItem(LAYOUT_KEY) as Orientation | null),
  );

  // Feed the server layer into the theme engine — it re-applies with the correct
  // precedence (device-local overrides from Theme Studio still win).
  useEffect(() => {
    if (data && data.length) setServerTheme(settings);
  }, [data, settings]);

  const apiOrientation = (settings['layoutorientation'] as Orientation) || 'vertical';
  const orientation: Orientation = override ?? (apiOrientation === 'horizontal' ? 'horizontal' : 'vertical');

  const setOrientation = useCallback((o: Orientation) => {
    localStorage.setItem(LAYOUT_KEY, o);
    setOverride(o);
  }, []);

  const value = useMemo<UISettingsValue>(
    () => ({ settings, orientation, setOrientation, reload: () => refetch() }),
    [settings, orientation, setOrientation, refetch],
  );

  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useUISettings(): UISettingsValue {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error('useUISettings must be used within a UISettingsProvider');
  return ctx;
}
