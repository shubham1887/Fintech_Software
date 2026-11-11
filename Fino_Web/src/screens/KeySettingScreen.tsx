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
import { SystemDownPanel } from './SystemDownPanel';

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
  // Fund amount bounds. Two independent rows rather than one "limits" row, because an operator
  // almost always wants a floor OR a ceiling, not both, and a single toggle would force them to
  // set a number they do not care about. Each applies to BOTH a fund request (raised by a
  // downline) and a fund transfer (pushed by an upline) — the two ends of the same movement, so
  // bounding one and not the other just moves the problem. Off, or 0, means "no bound".
  {
    key: 'FundMinAmount',
    label: 'Fund — minimum amount',
    description: 'On → a fund request or fund transfer below this amount is refused. Applies to both. Off means no lower bound.',
    hasValue: true,
    valueLabel: 'Minimum',
    valuePrefix: '₹',
  },
  {
    key: 'FundMaxAmount',
    label: 'Fund — maximum amount',
    description: 'On → a fund request or fund transfer above this amount is refused. Applies to both. Off means no upper bound. A maximum below the minimum is ignored rather than blocking every amount.',
    hasValue: true,
    valueLabel: 'Maximum',
    valuePrefix: '₹',
  },
  // The TDS master switch. It belongs here rather than on the TDS screen for two reasons: it is
  // a platform-wide feature switch like the others, and turning it ON starts taking real money
  // out of real people's commissions — which should be a deliberate act performed where features
  // are turned on, next to a description of what it does, not a toggle beside the rules.
  {
    key: 'IsTDSEnabled',
    label: 'Deduct TDS from commissions',
    description:
      'On → TDS is withheld from every commission a user earns, per the rules on Accountant → TDS Setting, '
      + 'and the withheld amount comes out of their wallet at settlement. Off → no rule is read and nothing is '
      + 'deducted. Set the rules first: with this on and no rule, the commission tax profile decides instead.',
  },
  // Cash collection / outstanding. A feature switch rather than a number, and it belongs beside
  // the other fund bounds: it decides whether funds can be given on credit at all.
  //
  // The description spells out what OFF does NOT do, because that is the part an operator will
  // assume wrongly. Switching it off stops new credit and stops the morning reminders; it does
  // not hide or forgive a debt already recorded, and it must not — the money is owed either way.
  {
    key: 'CashCollection',
    label: 'Cash collection (credit / outstanding)',
    description:
      'On → a fund request or fund transfer sent with the Credit payment mode funds the wallet AND '
      + 'records the amount as outstanding against the receiver, and everyone carrying a balance gets '
      + 'a reminder each morning. Off → no new credit can be taken. Balances already recorded stay '
      + 'visible and collectable either way. A user can only take credit up to the limit set per '
      + 'account on Funds → Outstanding; a limit of zero hides the Credit mode from them entirely.',
  },
  // The default credit limit. An amount row rather than a plain switch, like the two Fund bounds
  // above it — and it sits right after the cash-collection switch because it is meaningless
  // without it.
  //
  // Off by default on purpose: this one number silently extends real credit to every account that
  // has never been given a limit, so it has to be a deliberate act rather than something an
  // upgrade turns on.
  {
    key: 'DefaultCreditLimit',
    label: 'Credit — default limit',
    description:
      'On → any account that has never been given a credit limit of its own may take funds on '
      + 'credit up to this amount, so the feature works without visiting every account. A limit set '
      + 'for a specific account under Funds → Outstanding always wins — including a deliberate 0, '
      + 'which means that account gets no credit even while this default is on. Off means no credit '
      + 'until each account is given its own limit.',
    hasValue: true,
    valueLabel: 'Default limit',
    valuePrefix: '₹',
  },
  {
    key: 'IsMpinEnabled',
    label: 'Require MPIN',
    description: 'On → every user must set a 4–6 digit MPIN and enter it when they open the app (and on sensitive actions). Off → the MPIN is optional; a user can still turn it on for themselves from Account security.',
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

      {/*
        System Down is a panel of its own rather than another row in the list above, because it is
        not a single toggle: the switch, the two clock times and the per-service exception list are
        one decision and have to be saved as one. It sits last because turning it on stops the
        network trading.
      */}
      <SystemDownPanel />
    </>
  );
}
