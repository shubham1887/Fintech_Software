import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Save, LayoutGrid, PanelLeft, Palette } from 'lucide-react';
import { getUISettings, updateUISettingValue, type UISettingRow } from '@/api/uisettings.api';
import { useUISettings } from '@/theme/UISettingsProvider';
import { isHexColor } from '@/theme/colorRamp';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { cn } from '@/lib/cn';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

const ORIENT_KEY = 'layoutorientation';

/** Admin editor for the API-driven UI settings (colours, fonts, layout). Mirrors the legacy
 *  key/value settings grid, with inline edit + per-row save; changes apply live. */
export function UISettingsScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const { orientation, setOrientation } = useUISettings();

  const { data: rows = [], isLoading } = useQuery({ queryKey: ['ui-settings-admin'], queryFn: getUISettings });

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon('Setting')}
        actions={
          <Link
            to="/appearance"
            className="flex items-center gap-1.5 rounded-lg border border-brand-200 bg-brand-50 px-3 py-2 text-sm font-medium text-brand-700 transition-colors hover:bg-brand-100"
          >
            <Palette className="size-4" /> Open Theme Studio
          </Link>
        }
      />

      {/* Layout orientation quick toggle */}
      <Card className="mb-4">
        <CardHeader>
          <CardTitle>Layout orientation</CardTitle>
          <span className="text-xs text-slate-400">Applies instantly · also saved to the server</span>
        </CardHeader>
        <CardBody>
          <div className="flex flex-wrap gap-3">
            <OrientationCard
              active={orientation === 'vertical'}
              icon={PanelLeft}
              title="Vertical"
              desc="Left sidebar navigation"
              onClick={() => applyOrientation('vertical')}
            />
            <OrientationCard
              active={orientation === 'horizontal'}
              icon={LayoutGrid}
              title="Horizontal"
              desc="Top menu bar navigation"
              onClick={() => applyOrientation('horizontal')}
            />
          </div>
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>UI settings</CardTitle>
          <span className="flex items-center gap-1.5 text-xs text-slate-400"><Palette className="size-3.5" /> Colours &amp; fonts drive the whole app</span>
        </CardHeader>
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3 w-12">#</th>
                <th className="px-4 py-3">Key</th>
                <th className="px-4 py-3 w-72">Value</th>
                <th className="px-4 py-3">Description</th>
                <th className="px-4 py-3 w-20 text-right">Save</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-slate-400">Loading…</td></tr>
              ) : rows.length === 0 ? (
                <tr><td colSpan={5} className="px-4 py-10 text-center text-slate-400">No UI settings found.</td></tr>
              ) : (
                rows.map((r, i) => <SettingRow key={r.id} row={r} index={i + 1} onSaved={afterSave} />)
              )}
            </tbody>
          </table>
        </div>
      </Card>
    </>
  );

  function afterSave() {
    qc.invalidateQueries({ queryKey: ['ui-settings'] }); // provider re-applies live
    qc.invalidateQueries({ queryKey: ['ui-settings-admin'] });
  }

  function applyOrientation(o: 'vertical' | 'horizontal') {
    setOrientation(o); // instant
    const row = rows.find((x) => x.key.toLowerCase() === ORIENT_KEY);
    if (row && row.value !== o) {
      updateUISettingValue(row.id, row.value, o)
        .then((res) => (res.ok ? afterSave() : toast.error(res.message)))
        .catch((e: Error) => toast.error(e.message));
    }
  }
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

function SettingRow({ row, index, onSaved }: { row: UISettingRow; index: number; onSaved: () => void }) {
  const toast = useToast();
  const [value, setValue] = useState(row.value);
  useEffect(() => setValue(row.value), [row.value]);

  const isColor = /color/i.test(row.key) && isHexColor(value);
  const isOrient = row.key.toLowerCase() === ORIENT_KEY;
  const dirty = value !== row.value;

  const mut = useMutation({
    mutationFn: () => updateUISettingValue(row.id, row.value, value),
    onSuccess: (res) => {
      if (res.ok) { toast.success(`${row.key} updated.`); onSaved(); }
      else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <tr className="border-b border-slate-100 last:border-0 even:bg-slate-50/40">
      <td className="px-4 py-3 text-slate-400">{index}</td>
      <td className="px-4 py-3 font-medium text-slate-700">{row.key}</td>
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          {isColor && <span className="size-6 shrink-0 rounded-md border border-slate-200" style={{ backgroundColor: value }} />}
          {isOrient ? (
            <select
              value={value}
              onChange={(e) => setValue(e.target.value)}
              className="h-9 w-full rounded-lg border border-slate-300 bg-white px-2 text-sm"
            >
              <option value="vertical">vertical</option>
              <option value="horizontal">horizontal</option>
            </select>
          ) : (
            <input
              value={value}
              onChange={(e) => setValue(e.target.value)}
              className="h-9 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20"
            />
          )}
          {isColor && (
            <input type="color" value={value} onChange={(e) => setValue(e.target.value)} className="size-9 shrink-0 cursor-pointer rounded border border-slate-200 bg-white p-0.5" />
          )}
        </div>
      </td>
      <td className="px-4 py-3 text-slate-500">{row.description}</td>
      <td className="px-4 py-3 text-right">
        <Button size="sm" variant={dirty ? 'primary' : 'outline'} loading={mut.isPending} disabled={!dirty} onClick={() => mut.mutate()}>
          <Save className="size-4" />
        </Button>
      </td>
    </tr>
  );
}
