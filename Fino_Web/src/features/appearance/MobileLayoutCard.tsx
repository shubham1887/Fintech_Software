import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { RotateCcw, Save, Smartphone } from 'lucide-react';
import { createUISetting, getUISettings, updateUISettingValue } from '@/api/uisettings.api';
import { useUISettings } from '@/theme/UISettingsProvider';
import {
  MOBILE_LAYOUT_LIST,
  type ActivityStyle, type BalanceStyle, type HeaderStyle, type MobileLayout,
  type MobileLayoutKey, type ServiceStyle, type TabStyle, type TileShape,
} from '@/theme/mobileLayouts';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { useToast } from '@/components/ui/Toast';
import { cn } from '@/lib/cn';
import { MobileLayoutPreview } from './MobileLayoutPreview';

/**
 * "Mobile app" — the Android app's design, chosen here.
 *
 * The Android build is this console in a WebView, so the app's look is a server setting, not
 * a build artefact. The admin picks a preset (and can override individual blocks of it), it is
 * published as UISetting rows, and every installed phone shows the new design at its next
 * launch. No APK, no store review, and nothing per-client to hand-edit.
 *
 * Two things this screen has to get right, both learned the hard way:
 *
 *  - The presets must be visibly different HERE. They are different compositions in the app
 *    (balance / services / activity blocks are separate components), so the picker renders a
 *    real miniature of each rather than a generic phone outline with the corner radius changed.
 *  - Nothing is live until Publish. This is not a device-local draft like Theme Studio — it
 *    changes the app for every retailer on the platform.
 */

const SETTINGS: Array<{ key: string; desc: string }> = [
  { key: 'MobileLayout', desc: 'Phone/app home layout preset: classic | compact | vibrant | minimal | business | focus' },
  { key: 'MobileBalanceStyle', desc: 'Balance block on the phone home screen: card | hero | strip | plain | split' },
  { key: 'MobileServiceStyle', desc: 'Service catalogue on the phone home screen: grid | dense | cards | list | sections | focus' },
  { key: 'MobileActivityStyle', desc: 'Recent transactions on the phone home screen: list | compact | timeline | table' },
  { key: 'MobileHeaderStyle', desc: 'App bar treatment on the phone: solid | gradient | glass' },
  { key: 'MobileTileShape', desc: 'Service tile shape on the phone home grid: rounded | circle | square | card' },
  { key: 'MobileGridColumns', desc: 'Service tiles per row on the phone home grid (3, 4 or 5)' },
  { key: 'MobileShowBalanceCard', desc: 'Show the balance block on the phone home screen (true | false)' },
  { key: 'MobileShowTodayStats', desc: 'Show the today figures block on the phone home screen (true | false)' },
  { key: 'MobileTabStyle', desc: 'Bottom tab bar on the phone: labelled | icons | floating' },
];

const BALANCES: Array<{ value: BalanceStyle; label: string }> = [
  { value: 'card', label: 'Card' },
  { value: 'hero', label: 'Banner' },
  { value: 'strip', label: 'Rail' },
  { value: 'plain', label: 'Plain' },
  { value: 'split', label: 'Split' },
];

const SERVICES: Array<{ value: ServiceStyle; label: string }> = [
  { value: 'grid', label: 'Icon grid' },
  { value: 'dense', label: 'Dense pad' },
  { value: 'cards', label: 'Cards' },
  { value: 'list', label: 'List' },
  { value: 'sections', label: 'Categories' },
  { value: 'focus', label: 'Large rows' },
];

const ACTIVITIES: Array<{ value: ActivityStyle; label: string }> = [
  { value: 'list', label: 'List' },
  { value: 'compact', label: 'Compact' },
  { value: 'timeline', label: 'Timeline' },
  { value: 'table', label: 'Table' },
];

const HEADERS: Array<{ value: HeaderStyle; label: string }> = [
  { value: 'solid', label: 'Solid' },
  { value: 'gradient', label: 'Gradient' },
  { value: 'glass', label: 'Glass' },
];

const SHAPES: Array<{ value: TileShape; label: string }> = [
  { value: 'rounded', label: 'Rounded' },
  { value: 'circle', label: 'Circle' },
  { value: 'square', label: 'Square' },
  { value: 'card', label: 'Card' },
];

const TABS: Array<{ value: TabStyle; label: string }> = [
  { value: 'labelled', label: 'Icon + label' },
  { value: 'icons', label: 'Icons only' },
  { value: 'floating', label: 'Floating bar' },
];

export function MobileLayoutCard() {
  const { mobileLayout } = useUISettings();
  const toast = useToast();
  const qc = useQueryClient();

  const [draft, setDraft] = useState<MobileLayout>(mobileLayout);
  const [touched, setTouched] = useState(false);

  // Follow the server until the admin starts editing — then their edits win.
  useEffect(() => {
    if (!touched) setDraft(mobileLayout);
  }, [mobileLayout, touched]);

  const up = (patch: Partial<MobileLayout>) => {
    setTouched(true);
    setDraft((d) => ({ ...d, ...patch }));
  };

  /** Switching preset resets the individual overrides — that is what "preset" means. */
  const pickPreset = (key: MobileLayoutKey) => {
    const preset = MOBILE_LAYOUT_LIST.find((p) => p.key === key);
    if (preset) {
      setTouched(true);
      setDraft(preset);
    }
  };

  const values = useMemo<Record<string, string>>(
    () => ({
      MobileLayout: draft.key,
      MobileBalanceStyle: draft.balanceStyle,
      MobileServiceStyle: draft.serviceStyle,
      MobileActivityStyle: draft.activityStyle,
      MobileHeaderStyle: draft.headerStyle,
      MobileTileShape: draft.tileShape,
      MobileGridColumns: String(draft.gridColumns),
      MobileShowBalanceCard: String(draft.showBalanceCard),
      MobileShowTodayStats: String(draft.showTodayStats),
      MobileTabStyle: draft.tabStyle,
    }),
    [draft],
  );

  /** True once any block differs from the preset the admin started with. */
  const preset = MOBILE_LAYOUT_LIST.find((p) => p.key === draft.key);
  const customised = Boolean(
    preset &&
      (preset.balanceStyle !== draft.balanceStyle ||
        preset.serviceStyle !== draft.serviceStyle ||
        preset.activityStyle !== draft.activityStyle ||
        preset.headerStyle !== draft.headerStyle ||
        preset.tileShape !== draft.tileShape ||
        preset.gridColumns !== draft.gridColumns ||
        preset.tabStyle !== draft.tabStyle ||
        preset.showBalanceCard !== draft.showBalanceCard ||
        preset.showTodayStats !== draft.showTodayStats),
  );

  const publish = useMutation({
    mutationFn: async () => {
      const rows = await getUISettings();
      const byKey = new Map(rows.map((r) => [r.key.toLowerCase(), r]));
      let saved = 0;
      const failed: string[] = [];

      for (const s of SETTINGS) {
        const value = values[s.key];
        const row = byKey.get(s.key.toLowerCase());
        // Create the row on first publish — a fresh install that never ran the seed still works,
        // and the three composition keys below are newer than most live databases.
        if (!row) {
          const res = await createUISetting(s.key, value, s.desc);
          if (res.ok) saved++;
          else failed.push(s.key);
        } else if (row.value !== value) {
          const res = await updateUISettingValue(row.id, row.value, value);
          if (res.ok) saved++;
          else failed.push(s.key);
        }
      }
      return { saved, failed };
    },
    onSuccess: ({ saved, failed }) => {
      if (failed.length) {
        toast.error(`Could not save: ${failed.join(', ')}`);
        return;
      }
      setTouched(false);
      qc.invalidateQueries({ queryKey: ['ui-settings'] });
      qc.invalidateQueries({ queryKey: ['ui-settings-admin'] });
      toast.success(
        saved === 0
          ? 'The app layout is already up to date.'
          : 'App layout published — phones pick it up on their next launch.',
      );
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Mobile app layout</CardTitle>
        <span className="text-xs text-slate-400">How the Android app looks — published live, no new APK</span>
      </CardHeader>

      <CardBody className="space-y-6">
        {/* ------------------------------------------------------------ presets */}
        <div>
          <p className="mb-2.5 text-sm font-medium text-slate-700">
            Design
            <span className="ml-2 text-xs font-normal text-slate-400">
              Each one is a different home screen, not a different colour
            </span>
          </p>
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
            {MOBILE_LAYOUT_LIST.map((p) => {
              const active = draft.key === p.key;
              return (
                <button
                  key={p.key}
                  type="button"
                  onClick={() => pickPreset(p.key)}
                  aria-pressed={active}
                  className={cn(
                    'flex gap-3 rounded-xl border p-3 text-left transition-colors',
                    active
                      ? 'border-brand-500 bg-brand-50/40 ring-1 ring-brand-500'
                      : 'border-slate-200 hover:border-slate-300 hover:bg-slate-50',
                  )}
                >
                  <MobileLayoutPreview layout={p} />
                  <span className="flex min-w-0 flex-1 flex-col">
                    <span className="flex items-center gap-2">
                      <span className="text-sm font-semibold text-slate-800">{p.label}</span>
                      {active && customised && (
                        <span className="rounded bg-brand-100 px-1.5 py-px text-[10px] font-semibold uppercase tracking-wide text-brand-700">
                          Edited
                        </span>
                      )}
                    </span>
                    <span className="mt-1 block text-xs leading-snug text-slate-500">{p.note}</span>
                  </span>
                </button>
              );
            })}
          </div>
        </div>

        {/* ------------------------------------------------------- fine tuning */}
        <div className="grid gap-5 border-t border-slate-100 pt-5 lg:grid-cols-[1fr_auto]">
          <div className="grid gap-4 sm:grid-cols-2">
            <Chooser
              label="Balance"
              hint="How much of the screen the money gets"
              options={BALANCES}
              value={draft.balanceStyle}
              onChange={(v) => up({ balanceStyle: v })}
              disabled={!draft.showBalanceCard}
            />
            <Chooser
              label="Services"
              hint="How the catalogue is browsed"
              options={SERVICES}
              value={draft.serviceStyle}
              onChange={(v) => up({ serviceStyle: v })}
            />
            <Chooser
              label="Recent activity"
              options={ACTIVITIES}
              value={draft.activityStyle}
              onChange={(v) => up({ activityStyle: v })}
            />
            <Chooser label="App bar" options={HEADERS} value={draft.headerStyle} onChange={(v) => up({ headerStyle: v })} />
            <Chooser label="Tile shape" options={SHAPES} value={draft.tileShape} onChange={(v) => up({ tileShape: v })} />
            <Chooser
              label="Tiles per row"
              options={[
                { value: 3 as const, label: '3' },
                { value: 4 as const, label: '4' },
                { value: 5 as const, label: '5' },
              ]}
              value={draft.gridColumns}
              onChange={(v) => up({ gridColumns: v })}
            />
            <Chooser label="Bottom tabs" options={TABS} value={draft.tabStyle} onChange={(v) => up({ tabStyle: v })} />

            <div className="grid gap-3 sm:col-span-2 sm:grid-cols-2">
              <Toggle
                label="Balance on home"
                hint="Off: the strip in the app bar carries it on every screen"
                checked={draft.showBalanceCard}
                onChange={(v) => up({ showBalanceCard: v })}
              />
              <Toggle
                label="Today's figures"
                hint="Sale, commission, failed and received"
                checked={draft.showTodayStats}
                onChange={(v) => up({ showTodayStats: v })}
              />
            </div>
          </div>

          {/* The draft, drawn with every override applied — the answer to "what will the
              retailer actually see", without publishing to find out. */}
          <div className="flex flex-col items-center gap-2 lg:w-[132px]">
            <p className="text-xs font-medium text-slate-500">Preview</p>
            <MobileLayoutPreview layout={draft} className="scale-110" />
            {customised && (
              <button
                type="button"
                onClick={() => pickPreset(draft.key)}
                className="mt-1 flex items-center gap-1 text-[11px] font-medium text-slate-400 hover:text-slate-600"
              >
                <RotateCcw className="size-3" />
                Reset to {preset?.label}
              </button>
            )}
          </div>
        </div>

        <div className="flex items-center justify-between gap-3 border-t border-slate-100 pt-4">
          <p className="text-xs text-slate-400">
            Applies to every phone and to the Android app. Nothing is installed — the app reads this on launch.
          </p>
          <Button disabled={!touched} loading={publish.isPending} onClick={() => publish.mutate()}>
            <Save className="size-4" />
            Publish
          </Button>
        </div>
      </CardBody>
    </Card>
  );
}

/* --------------------------------------------------------------- controls */

function Chooser<T extends string | number>({
  label, hint, options, value, onChange, disabled,
}: {
  label: string;
  hint?: string;
  options: ReadonlyArray<{ value: T; label: string }>;
  value: T;
  onChange: (v: T) => void;
  disabled?: boolean;
}) {
  return (
    <div className={cn(disabled && 'opacity-40')}>
      <p className="text-sm font-medium text-slate-700">{label}</p>
      {hint && <p className="mb-1.5 text-xs text-slate-400">{hint}</p>}
      <div className={cn('flex flex-wrap gap-1.5', !hint && 'mt-1.5')}>
        {options.map((o) => (
          <button
            key={String(o.value)}
            type="button"
            disabled={disabled}
            onClick={() => onChange(o.value)}
            className={cn(
              'rounded-lg px-3 py-1.5 text-xs font-semibold transition-colors disabled:cursor-not-allowed',
              value === o.value
                ? 'bg-brand-600 text-white'
                : 'bg-white text-slate-500 ring-1 ring-slate-200 hover:text-slate-700',
            )}
          >
            {o.label}
          </button>
        ))}
      </div>
    </div>
  );
}

function Toggle({
  label, hint, checked, onChange,
}: {
  label: string;
  hint: string;
  checked: boolean;
  onChange: (v: boolean) => void;
}) {
  return (
    <label className="flex cursor-pointer items-start gap-2.5">
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="mt-0.5 size-4 accent-[var(--color-brand-600)]"
      />
      <span className="min-w-0">
        <span className="block text-sm font-medium text-slate-700">{label}</span>
        <span className="block text-xs text-slate-400">{hint}</span>
      </span>
    </label>
  );
}

/** Exported for the Appearance page header icon. */
export const MobileLayoutIcon = Smartphone;
