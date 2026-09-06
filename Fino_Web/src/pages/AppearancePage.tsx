import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Check,
  ExternalLink,
  LayoutGrid,
  Minus,
  Paintbrush,
  Palette,
  PanelLeft,
  Plus,
  RotateCcw,
  Save,
  SlidersHorizontal,
  Type,
  Users,
} from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { isAdminTier } from '@/lib/roles';
import { useBrand } from '@/theme/BrandProvider';
import { useUISettings, type Orientation } from '@/theme/UISettingsProvider';
import { getTheme, isInSeason } from '@/theme/themes';
import { isHexColor } from '@/theme/colorRamp';
import {
  clearLocalTheme,
  hasLocalOverrides,
  previewTheme,
  rampOf,
  reapplyTheme,
  resolveTheme,
  writeLocalTheme,
  type LocalTheme,
} from '@/theme/themeEngine';
import { createUISetting, getUISettings, updateUISettingValue } from '@/api/uisettings.api';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { useToast } from '@/components/ui/Toast';
import { cn } from '@/lib/cn';

/** Full-valued working copy the studio edits (resolved → editable). */
interface Draft {
  preset: string;
  primary?: string;
  navbar: string;
  appbar: string;
  fore: string;
  secondary: string;
  reportFs: number;
  consumerFs: number;
}

function draftFromCurrent(): Draft {
  const r = resolveTheme();
  return {
    preset: r.preset,
    primary: r.primary,
    navbar: r.navbar,
    appbar: r.appbar,
    fore: r.fore,
    secondary: r.secondary,
    reportFs: r.reportFs,
    consumerFs: r.consumerFs,
  };
}

/** Expand #abc → #aabbcc so <input type="color"> accepts it. */
function toInputHex(v: string): string {
  const h = v.trim().replace(/^#/, '');
  if (h.length === 3) return '#' + h.split('').map((c) => c + c).join('');
  return '#' + h;
}

export default function AppearancePage() {
  const { user } = useAuth();
  const { themes } = useBrand();
  const { settings, orientation, setOrientation } = useUISettings();
  const toast = useToast();
  const qc = useQueryClient();
  const admin = isAdminTier(user?.UserTypeID ?? 0);

  const [draft, setDraft] = useState<Draft>(draftFromCurrent);
  const [touched, setTouched] = useState(false);

  // Refresh the draft when the server layer arrives — but never clobber live edits.
  useEffect(() => {
    if (!touched) setDraft(draftFromCurrent());
  }, [settings, touched]);

  // Live preview: every edit re-skins the whole console instantly (not persisted yet).
  useEffect(() => {
    if (touched) previewTheme(draft as LocalTheme);
  }, [draft, touched]);

  // Leaving without applying restores the persisted theme.
  useEffect(() => () => reapplyTheme(), []);

  const up = (patch: Partial<Draft>) => {
    setTouched(true);
    setDraft((d) => ({ ...d, ...patch }));
  };

  function applyOnDevice() {
    // Persist only the fields that differ from the config/server baseline, so
    // future admin-published changes still flow through untouched settings.
    const base = resolveTheme({});
    const local: LocalTheme = {};
    if (draft.primary) {
      if (draft.primary !== base.primary) local.primary = draft.primary;
    } else if (draft.preset !== base.preset || base.primary) {
      local.preset = draft.preset;
    }
    if (draft.navbar !== base.navbar) local.navbar = draft.navbar;
    if (draft.appbar !== base.appbar) local.appbar = draft.appbar;
    if (draft.fore !== base.fore) local.fore = draft.fore;
    if (draft.secondary !== base.secondary) local.secondary = draft.secondary;
    if (draft.reportFs !== base.reportFs) local.reportFs = draft.reportFs;
    if (draft.consumerFs !== base.consumerFs) local.consumerFs = draft.consumerFs;
    writeLocalTheme(local);
    setTouched(false);
    toast.success('Theme applied — saved on this device.');
  }

  function resetToPortalDefault() {
    clearLocalTheme();
    setTouched(false);
    setDraft(draftFromCurrent());
    toast.success('Back to the portal default theme.');
  }

  const publishMut = useMutation({
    mutationFn: async () => {
      const rows = await getUISettings();
      const byKey = new Map(rows.map((r) => [r.key.toLowerCase(), r]));
      const entries: Array<{ key: string; value: string; desc: string }> = [
        { key: 'ThemePreset', value: draft.primary ? 'custom' : draft.preset, desc: 'Portal theme preset key (Theme Studio)' },
        { key: 'PrimaryColor', value: draft.primary ?? getTheme(draft.preset).brand['500'], desc: 'Brand primary colour — hex seed for the full ramp' },
        { key: 'PrimaryHoverColor', value: rampOf(draft)['600'], desc: 'Brand hover/pressed colour (600 shade)' },
        { key: 'NavBarColor', value: draft.navbar, desc: 'Sidebar / vertical navigation background' },
        { key: 'AppBarColor', value: draft.appbar, desc: 'Topbar background (horizontal layout)' },
        { key: 'ForeColor', value: draft.fore, desc: 'Navigation text & icon colour' },
        { key: 'SecondaryColor', value: draft.secondary, desc: 'Secondary accent colour' },
        { key: 'LayoutOrientation', value: orientation, desc: 'vertical | horizontal navigation layout' },
        { key: 'ReportFontSize', value: String(draft.reportFs), desc: 'Report table font size (px)' },
        { key: 'ConsumerNumberFontSize', value: String(draft.consumerFs), desc: 'Consumer number font size (px)' },
      ];
      let saved = 0;
      const failed: string[] = [];
      for (const e of entries) {
        const row = byKey.get(e.key.toLowerCase());
        if (!row) {
          const res = await createUISetting(e.key, e.value, e.desc);
          if (res.ok) saved++;
          else failed.push(e.key);
        } else if (row.value !== e.value) {
          const res = await updateUISettingValue(row.id, row.value, e.value);
          if (res.ok) saved++;
          else failed.push(e.key);
        }
      }
      return { saved, failed };
    },
    onSuccess: ({ saved, failed }) => {
      if (failed.length) {
        toast.error(`Could not save: ${failed.join(', ')}`);
        return;
      }
      // Server is now the source of truth — drop this device's overrides.
      clearLocalTheme();
      setTouched(false);
      qc.invalidateQueries({ queryKey: ['ui-settings'] });
      qc.invalidateQueries({ queryKey: ['ui-settings-admin'] });
      toast.success(
        saved === 0 ? 'Portal theme is already up to date.' : `Theme published to the whole portal (${saved} setting${saved === 1 ? '' : 's'} saved).`,
      );
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const isCustom = !!draft.primary;
  const dirty = touched;
  const canReset = dirty || hasLocalOverrides();

  return (
    <>
      <PageHeader
        title="Theme Studio"
        subtitle="Customise the portal's look — every change previews live across the console"
        icon={Palette}
      />

      <div className="grid gap-4 lg:grid-cols-3">
        {/* ------------------------------------------------ left: controls */}
        <div className="space-y-4 lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Brand colour</CardTitle>
              <span className="text-xs text-slate-400">Preset or your own colour — drives buttons, links &amp; accents</span>
            </CardHeader>
            <CardBody className="space-y-5">
              {/* Brand presets and seasonal skins are separate decisions — a festival skin also
                  repaints the nav/app bar, so mixing them in one flat grid hid that. */}
              {([
                { title: 'Brand', list: themes.filter((t) => !t.seasonal) },
                { title: 'Festival', list: themes.filter((t) => t.seasonal) },
              ] as const).map((group) => (
                <div key={group.title}>
                  <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-400">
                    {group.title}
                  </p>
                  <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
                    {group.list.map((t) => {
                      const active = !isCustom && draft.preset === t.key;
                      return (
                        <button
                          key={t.key}
                          type="button"
                          onClick={() => up({ preset: t.key, primary: undefined })}
                          className={cn(
                            'flex items-center gap-3 rounded-xl border p-3 text-left transition-colors',
                            active ? 'border-brand-500 ring-2 ring-brand-500/30' : 'border-slate-200 hover:border-slate-300',
                          )}
                        >
                          <span className="size-9 shrink-0 rounded-lg" style={{ backgroundColor: t.swatch }} />
                          <span className="min-w-0 flex-1">
                            <span className="flex items-center gap-1.5">
                              <span className="truncate text-sm font-medium text-slate-800">{t.label}</span>
                              {isInSeason(t) && (
                                <span className="shrink-0 rounded-full bg-success-50 px-1.5 py-0.5 text-[10px] font-semibold text-success-600">
                                  In season
                                </span>
                              )}
                            </span>
                            <span className="block truncate text-xs text-slate-400">{t.note ?? t.key}</span>
                          </span>
                          {active && <Check className="size-4 shrink-0 text-brand-600" />}
                        </button>
                      );
                    })}
                  </div>
                </div>
              ))}

              <div
                className={cn(
                  'rounded-xl border p-4 transition-colors',
                  isCustom ? 'border-brand-500 ring-2 ring-brand-500/30' : 'border-dashed border-slate-300',
                )}
              >
                <div className="flex flex-wrap items-center gap-4">
                  <div className="flex items-center gap-2.5">
                    <span className="grid size-9 place-items-center rounded-lg bg-slate-100 text-slate-500">
                      <Paintbrush className="size-4.5" />
                    </span>
                    <div>
                      <p className="text-sm font-medium text-slate-800">Custom brand colour</p>
                      <p className="text-xs text-slate-400">Pick any colour — we build the full shade ramp for you</p>
                    </div>
                  </div>
                  <div className="ml-auto">
                    <ColorField
                      label=""
                      value={draft.primary ?? getTheme(draft.preset).brand['500']}
                      onChange={(v) => up({ primary: v })}
                    />
                  </div>
                </div>
                {isCustom && (
                  <div className="mt-3 flex overflow-hidden rounded-lg border border-slate-200">
                    {Object.entries(rampOf(draft)).map(([shade, val]) => (
                      <span key={shade} className="h-6 flex-1" style={{ backgroundColor: val }} title={`${shade} · ${val}`} />
                    ))}
                  </div>
                )}
              </div>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Interface colours</CardTitle>
              <span className="text-xs text-slate-400">Navigation surfaces &amp; accents</span>
            </CardHeader>
            <CardBody>
              <div className="grid gap-4 sm:grid-cols-2">
                <ColorField label="Sidebar background" value={draft.navbar} onChange={(v) => up({ navbar: v })} />
                <ColorField label="Topbar background" value={draft.appbar} onChange={(v) => up({ appbar: v })} />
                <ColorField label="Navigation text" value={draft.fore} onChange={(v) => up({ fore: v })} />
                <ColorField label="Secondary accent" value={draft.secondary} onChange={(v) => up({ secondary: v })} />
              </div>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Layout &amp; typography</CardTitle>
              <span className="text-xs text-slate-400">Navigation style &amp; table text sizes</span>
            </CardHeader>
            <CardBody className="space-y-5">
              <div className="flex flex-wrap gap-3">
                <OrientationCard
                  active={orientation === 'vertical'}
                  icon={PanelLeft}
                  title="Vertical"
                  desc="Left sidebar navigation"
                  onClick={() => switchOrientation('vertical')}
                />
                <OrientationCard
                  active={orientation === 'horizontal'}
                  icon={LayoutGrid}
                  title="Horizontal"
                  desc="Top menu bar navigation"
                  onClick={() => switchOrientation('horizontal')}
                />
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <SizeStepper
                  label="Report font size"
                  hint="Tables & reports"
                  value={draft.reportFs}
                  onChange={(v) => up({ reportFs: v })}
                />
                <SizeStepper
                  label="Consumer number size"
                  hint="Highlighted account numbers"
                  value={draft.consumerFs}
                  onChange={(v) => up({ consumerFs: v })}
                />
              </div>
            </CardBody>
          </Card>
        </div>

        {/* ------------------------------------------------ right: preview + actions */}
        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Live preview</CardTitle>
              <span className="text-xs text-slate-400">Your console with this theme</span>
            </CardHeader>
            <CardBody>
              <ConsolePreview draft={draft} orientation={orientation} />
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>This device</CardTitle>
              <SlidersHorizontal className="size-4 text-slate-400" />
            </CardHeader>
            <CardBody className="space-y-3">
              <p className="text-sm text-slate-500">
                Save your look on this device — it stays on top of whatever the portal default is.
              </p>
              <div className="flex gap-2">
                <Button className="flex-1" disabled={!dirty} onClick={applyOnDevice}>
                  <Save className="size-4" /> Apply
                </Button>
                <Button variant="outline" disabled={!canReset} onClick={resetToPortalDefault}>
                  <RotateCcw className="size-4" /> Reset
                </Button>
              </div>
            </CardBody>
          </Card>

          {admin && (
            <Card>
              <CardHeader>
                <CardTitle>Publish for all users</CardTitle>
                <Users className="size-4 text-slate-400" />
              </CardHeader>
              <CardBody className="space-y-3">
                <p className="text-sm text-slate-500">
                  Save this exact look to the server (<span className="font-medium text-slate-700">UI Setting master</span>)
                  as the portal default — every user gets it on their next load. Users can still personalise on
                  their own device.
                </p>
                <Button className="w-full" loading={publishMut.isPending} onClick={() => publishMut.mutate()}>
                  <Users className="size-4" /> Publish theme to portal
                </Button>
                <Link
                  to="/s/UISetting"
                  className="flex items-center justify-center gap-1.5 text-sm font-medium text-brand-600 hover:text-brand-700"
                >
                  Advanced key/value editor <ExternalLink className="size-3.5" />
                </Link>
              </CardBody>
            </Card>
          )}

          {!admin && (
            <Card>
              <CardBody className="flex gap-3">
                <Palette className="size-5 shrink-0 text-brand-500" />
                <p className="text-sm text-slate-500">
                  Your choices are saved on this device only. The portal-wide default theme is managed by your
                  administrator.
                </p>
              </CardBody>
            </Card>
          )}
        </div>
      </div>
    </>
  );

  function switchOrientation(o: Orientation) {
    setOrientation(o); // instant + persisted on this device; admin publish includes it
  }
}

/* ------------------------------------------------------------------ pieces */

function ColorField({ label, value, onChange }: { label: string; value: string; onChange: (v: string) => void }) {
  const [text, setText] = useState(value);
  useEffect(() => setText(value), [value]);

  return (
    <div>
      {label && (
        <span className="mb-1.5 block text-xs font-medium uppercase tracking-wide text-slate-500">{label}</span>
      )}
      <div className="flex items-center gap-2">
        <label
          className="relative size-9 shrink-0 cursor-pointer overflow-hidden rounded-lg border border-slate-200"
          style={{ backgroundColor: value }}
        >
          <input
            type="color"
            value={toInputHex(value)}
            onChange={(e) => {
              setText(e.target.value);
              onChange(e.target.value);
            }}
            className="absolute inset-0 size-full cursor-pointer opacity-0"
            aria-label={label || 'Pick colour'}
          />
        </label>
        <input
          value={text}
          onChange={(e) => {
            setText(e.target.value);
            if (isHexColor(e.target.value)) onChange(toInputHex(e.target.value));
          }}
          spellCheck={false}
          className="h-9 w-28 rounded-lg border border-slate-300 bg-white px-2.5 font-mono text-sm text-slate-700 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20"
        />
      </div>
    </div>
  );
}

function SizeStepper({ label, hint, value, onChange }: {
  label: string; hint: string; value: number; onChange: (v: number) => void;
}) {
  const step = (d: number) => onChange(Math.min(28, Math.max(10, value + d)));
  return (
    <div className="rounded-xl border border-slate-200 p-3.5">
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-slate-800">{label}</p>
          <p className="text-xs text-slate-400">{hint}</p>
        </div>
        <div className="flex items-center gap-1.5">
          <button
            type="button"
            onClick={() => step(-1)}
            className="grid size-8 place-items-center rounded-lg border border-slate-200 text-slate-500 transition-colors hover:border-slate-300 hover:bg-slate-50"
            aria-label={`Decrease ${label}`}
          >
            <Minus className="size-4" />
          </button>
          <span className="w-12 text-center font-mono text-sm font-semibold text-slate-700">{value}px</span>
          <button
            type="button"
            onClick={() => step(1)}
            className="grid size-8 place-items-center rounded-lg border border-slate-200 text-slate-500 transition-colors hover:border-slate-300 hover:bg-slate-50"
            aria-label={`Increase ${label}`}
          >
            <Plus className="size-4" />
          </button>
        </div>
      </div>
      <p className="mt-2 flex items-center gap-1.5 truncate text-slate-600" style={{ fontSize: value }}>
        <Type className="size-3.5 shrink-0 text-slate-400" /> The quick brown fox — ₹1,250.00
      </p>
    </div>
  );
}

function OrientationCard({ active, icon: Icon, title, desc, onClick }: {
  active: boolean; icon: typeof PanelLeft; title: string; desc: string; onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'flex w-52 items-center gap-3 rounded-xl border p-4 text-left transition-all',
        active ? 'border-brand-500 bg-brand-50 ring-1 ring-brand-200' : 'border-slate-200 hover:border-slate-300',
      )}
    >
      <span className={cn('grid size-10 place-items-center rounded-lg', active ? 'bg-brand-600 text-white' : 'bg-slate-100 text-slate-500')}>
        <Icon className="size-5" />
      </span>
      <span>
        <span className="block text-sm font-semibold text-slate-800">{title}</span>
        <span className="block text-xs text-slate-500">{desc}</span>
      </span>
    </button>
  );
}

/** Miniature console mock-up rendered entirely from the draft values. */
function ConsolePreview({ draft, orientation }: { draft: Draft; orientation: Orientation }) {
  const ramp = rampOf(draft);
  const horizontal = orientation === 'horizontal';

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200">
      {/* topbar */}
      <div
        className="flex h-9 items-center gap-2 border-b border-slate-200 px-3"
        style={horizontal ? { backgroundColor: draft.appbar, borderColor: 'transparent' } : { backgroundColor: '#ffffff' }}
      >
        <span className="size-4 rounded-md" style={{ backgroundColor: ramp['600'] }} />
        <span
          className="h-1.5 w-16 rounded-full"
          style={{ backgroundColor: horizontal ? draft.fore : '#cbd5e1', opacity: 0.75 }}
        />
        <span className="ml-auto size-4 rounded-full" style={{ backgroundColor: ramp['500'] }} />
      </div>

      <div className="flex">
        {/* sidebar */}
        {!horizontal && (
          <div className="flex w-14 shrink-0 flex-col gap-2 p-2" style={{ backgroundColor: draft.navbar }}>
            <span className="h-2 w-full rounded" style={{ backgroundColor: ramp['600'] }} />
            {[0.5, 0.35, 0.35, 0.25].map((op, i) => (
              <span key={i} className="h-1.5 w-full rounded-full" style={{ backgroundColor: draft.fore, opacity: op }} />
            ))}
          </div>
        )}

        {/* content */}
        <div className="min-h-36 flex-1 space-y-2.5 bg-slate-50 p-3">
          <div className="h-2 w-24 rounded-full bg-slate-300" />
          <div className="flex gap-2">
            {[ramp['600'], draft.secondary, ramp['300']].map((c, i) => (
              <div key={i} className="flex-1 rounded-lg bg-white p-2 shadow-sm">
                <span className="block h-1.5 w-8 rounded-full" style={{ backgroundColor: c }} />
                <span className="mt-1.5 block h-2 w-10 rounded-full bg-slate-200" />
              </div>
            ))}
          </div>
          <div className="rounded-lg bg-white p-2.5 shadow-sm">
            <p className="truncate font-medium text-slate-700" style={{ fontSize: Math.min(draft.reportFs, 16) }}>
              Recharge · Airtel · <span style={{ fontSize: Math.min(draft.consumerFs, 18) }}>9876543210</span>
            </p>
            <p className="truncate text-slate-400" style={{ fontSize: Math.min(draft.reportFs, 16) }}>
              ₹1,250.00 · Success
            </p>
          </div>
          <div className="flex flex-wrap gap-1.5">
            <span className="rounded-md px-2.5 py-1 text-xs font-medium text-white" style={{ backgroundColor: ramp['600'] }}>
              Primary
            </span>
            <span className="rounded-md px-2.5 py-1 text-xs font-medium" style={{ backgroundColor: ramp['50'], color: ramp['700'] }}>
              Soft
            </span>
            <span className="rounded-md px-2.5 py-1 text-xs font-medium text-white" style={{ backgroundColor: draft.secondary }}>
              Secondary
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
