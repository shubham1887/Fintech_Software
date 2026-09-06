import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Save, ToggleRight } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');

/**
 * Key Setting = enable/disable client functions. This is NOT a raw key/value editor —
 * it shows only curated feature toggles. Charges & limits (e.g. DMTVerifyCharge, the
 * money-transfer commission) live in the commission Slab / DMT Settings, never here.
 *
 * Add a client function = add one entry below. A row that carries a number (hasValue)
 * shows an inline value box that only applies while the toggle is On.
 */
interface ToggleDef {
  key: string; // SettingMaster.Key
  label: string;
  description: string;
  hasValue?: boolean;
  valueLabel?: string;
  valuePrefix?: string;
}

const FEATURE_TOGGLES: ToggleDef[] = [
  {
    key: 'IsBeneficiaryOTP',
    label: 'Beneficiary OTP',
    description: 'Require OTP verification when a retailer adds a DMT beneficiary.',
  },
  {
    key: 'ShowLimitInDMT',
    label: 'DMT — max per transfer',
    description: 'Show and enforce a maximum amount per DMT money transfer.',
    hasValue: true,
    valueLabel: 'Limit',
    valuePrefix: '₹',
  },
  {
    key: 'eVerification',
    label: 'E-Verification (E-KYC)',
    description: 'On → users verify through E-KYC / E-Verification. Off → users complete Manual Verification by uploading the admin-configured documents.',
  },
  {
    key: 'IsKYCEnforced',
    label: 'Require KYC to transact',
    description: 'On → a user whose KYC is not approved is refused every service except the ones allowed on Users → KYC Approvals → KYC policy. Off → KYC is recorded but nothing is blocked.',
  },
  {
    key: 'IsSubscriptionEnabled',
    label: 'Service Subscription',
    description: 'On → users must subscribe to a plan to unlock its services; plans, the subscribe page and the header renewal counter appear. Off → the whole feature is hidden everywhere and no one is required to subscribe.',
  },
];

/** On-screen state for one toggle. */
interface Draft {
  on: boolean;
  value: string;
  /** Existing SettingMaster row (if the key has been seeded), for a full-object Update. */
  row?: Row;
}

/** Truthy-string parser matching the backend (true / 1 / yes / on). */
function parseOn(value: string, isOn: boolean): boolean {
  const v = value.trim().toLowerCase();
  if (v === '') return isOn;
  if (v === 'true' || v === 'false') return v === 'true';
  return v === '1' || v === 'yes' || v === 'on' || v === 'y';
}

function toDraft(def: ToggleDef, row?: Row): Draft {
  if (!row) return { on: def.hasValue ? false : false, value: def.hasValue ? '' : '', row: undefined };
  const value = str(getField(row, 'Value'));
  const isOn = Boolean(getField(row, 'IsOn'));
  // Value-bearing toggles gate on IsOn; pure on/off toggles gate on the Value string.
  const on = def.hasValue ? isOn : parseOn(value, isOn);
  return { on, value: def.hasValue ? value : '', row };
}

/** Accessible On/Off pill switch. */
function Switch({ on, onChange }: { on: boolean; onChange: (v: boolean) => void }) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={on}
      onClick={() => onChange(!on)}
      className={cn(
        'relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors',
        on ? 'bg-brand-600' : 'bg-slate-300',
      )}
    >
      <span
        className={cn(
          'inline-block size-5 transform rounded-full bg-white shadow transition-transform',
          on ? 'translate-x-5' : 'translate-x-0.5',
        )}
      />
    </button>
  );
}

export function KeySettingScreen() {
  const toast = useToast();
  const qc = useQueryClient();

  const { data: rows = [], isFetching } = useQuery({
    queryKey: ['crud', 'SettingMaster/GetAll'],
    queryFn: () => fetchList('SettingMaster/GetAll', { PageNumber: 1, PageSize: 500 }),
  });

  const rowByKey = useMemo(() => {
    const m = new Map<string, Row>();
    for (const r of rows) m.set(str(getField(r, 'Key')).toLowerCase(), r);
    return m;
  }, [rows]);

  const [drafts, setDrafts] = useState<Record<string, Draft>>({});

  // Server rows are the source of truth; local edits override per key until saved.
  const current = (def: ToggleDef): Draft =>
    drafts[def.key] ?? toDraft(def, rowByKey.get(def.key.toLowerCase()));

  const setDraft = (key: string, patch: Partial<Draft>) =>
    setDrafts((s) => {
      const def = FEATURE_TOGGLES.find((d) => d.key === key)!;
      const base = s[key] ?? toDraft(def, rowByKey.get(key.toLowerCase()));
      return { ...s, [key]: { ...base, ...patch } };
    });

  const saveMut = useMutation({
    mutationFn: async (def: ToggleDef) => {
      const d = current(def);
      const row = rowByKey.get(def.key.toLowerCase());
      // Pure toggle → Value carries true/false. Value toggle → IsOn gates, Value holds the number.
      const value = def.hasValue ? d.value.trim() : d.on ? 'true' : 'false';
      const isOn = d.on;
      if (def.hasValue && d.on && (!value || Number(value) <= 0)) {
        throw new Error(`Enter a valid ${def.valueLabel ?? 'value'} to turn this on.`);
      }
      const base = {
        Key: def.key,
        Value: value || (def.hasValue ? '0' : 'false'),
        IsOn: isOn,
        IsSecure: Boolean(getField(row ?? {}, 'IsSecure')),
        Description: str(getField(row ?? {}, 'Description')) || def.description,
        IsPassword: Boolean(getField(row ?? {}, 'IsPassword')),
      };
      const res = row
        ? await apiPost('SettingMaster/Update', { SettingMasterID: getField(row, 'SettingMasterID'), ...base })
        : await apiPost('SettingMaster/Create', base);
      if (!res.ok) throw new Error(res.message || 'Save failed.');
      return def.key;
    },
    onSuccess: (key) => {
      toast.success('Setting saved.');
      setDrafts((s) => {
        const next = { ...s };
        delete next[key]; // fall back to the freshly-refetched server row
        return next;
      });
      qc.invalidateQueries({ queryKey: ['crud', 'SettingMaster/GetAll'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <PageHeader title="Key Settings" subtitle="Enable or disable client functions" />

      <Card>
        <CardBody className="divide-y divide-slate-100 p-0">
          {isFetching && rows.length === 0 ? (
            <div className="p-6 text-sm text-slate-400">Loading…</div>
          ) : (
            FEATURE_TOGGLES.map((def) => {
              const d = current(def);
              const dirty = Boolean(drafts[def.key]);
              return (
                <div key={def.key} className="flex flex-wrap items-center gap-x-6 gap-y-3 px-5 py-4">
                  <div className="min-w-[16rem] flex-1">
                    <div className="flex items-center gap-2">
                      <ToggleRight className="size-4 text-slate-400" />
                      <span className="text-sm font-semibold text-slate-800">{def.label}</span>
                    </div>
                    <p className="mt-0.5 pl-6 text-xs text-slate-500">{def.description}</p>
                  </div>

                  {def.hasValue && (
                    <div className="w-40">
                      <Input
                        type="number"
                        min="0"
                        disabled={!d.on}
                        value={d.value}
                        placeholder="0"
                        leftIcon={<span className="text-sm text-slate-400">{def.valuePrefix ?? ''}</span>}
                        onChange={(e) => setDraft(def.key, { value: e.target.value })}
                      />
                    </div>
                  )}

                  <div className="flex items-center gap-4">
                    <Switch on={d.on} onChange={(v) => setDraft(def.key, { on: v })} />
                    <span className={cn('w-8 text-xs font-medium', d.on ? 'text-success-700' : 'text-slate-400')}>
                      {d.on ? 'On' : 'Off'}
                    </span>
                    <Button
                      size="sm"
                      variant="outline"
                      disabled={!dirty}
                      loading={saveMut.isPending && saveMut.variables?.key === def.key}
                      onClick={() => saveMut.mutate(def)}
                    >
                      <Save className="size-4" /> Save
                    </Button>
                  </div>
                </div>
              );
            })
          )}
        </CardBody>
      </Card>
    </>
  );
}
