import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Calculator, Layers, Plus, Save, Trash2 } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { OPT_PAYLOAD, type Row } from './providerShared';
import { GST_TYPES, GST_TYPE_LABEL, gstPhrase, normalizeGstType, splitCharge } from '@/lib/chargeGst';

/** One editable amount band. `id` 0 = a row that has not been saved yet. */
interface BandDraft {
  key: string;      // stable React key, independent of the server id
  id: number;
  OperatorID: string;
  FromAmount: string;
  ToAmount: string;
  AdminComm: string;
  AdminCommType: string;           // Fix | Flexi
  AdminCommissionCategory: string; // Commission | Surcharge
  GSTType: string;                 // DEFAULT | INCLUSIVE | EXCLUSIVE | NONE
  GSTRatePercent: string;          // blank/0 = take the rate from the tax engine
}

const FIELD =
  'h-9 w-full rounded-lg border border-slate-200 bg-white px-2.5 text-sm text-slate-700 ' +
  'focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20';

let keySeq = 0;
const nextKey = () => `band-${++keySeq}`;

const emptyBand = (): BandDraft => ({
  key: nextKey(),
  id: 0,
  OperatorID: '0',
  FromAmount: '',
  ToAmount: '',
  AdminComm: '',
  AdminCommType: 'Fix',
  AdminCommissionCategory: 'Surcharge',
  // A new band decides nothing about GST until somebody says so, exactly as every band that
  // existed before the column did. The profile keeps deciding until then.
  GSTType: 'DEFAULT',
  GSTRatePercent: '',
});

const num = (v: string) => Number(v) || 0;
const isPercent = (t: string) => /flexi|percent|%/i.test(t);
const weCharge = (c: string) => !/surcharge/i.test(c); // Commission = provider pays us

/** What this band actually costs or earns on a given amount. Mirrors the server's resolver. */
function chargeOn(d: BandDraft, amount: number): number {
  const v = Math.abs(num(d.AdminComm));
  return isPercent(d.AdminCommType) ? (amount * v) / 100 : v;
}

/** Does `amount` fall in this band? `To` 0 means open-ended. */
function covers(d: BandDraft, amount: number): boolean {
  const from = num(d.FromAmount);
  const to = num(d.ToAmount);
  return amount >= from && (to <= 0 || amount <= to);
}

/** Server precedence: operator-specific first, then the narrowest band. */
function pickBand(rows: BandDraft[], amount: number, operatorId: number): BandDraft | null {
  const matches = rows
    .filter((d) => d.OperatorID === '0' || Number(d.OperatorID) === operatorId)
    .filter((d) => covers(d, amount));
  if (matches.length === 0) return null;
  return matches.sort((a, b) => {
    const specific = Number(b.OperatorID !== '0') - Number(a.OperatorID !== '0');
    if (specific !== 0) return specific;
    const widthA = num(a.ToAmount) <= 0 ? Number.MAX_SAFE_INTEGER : num(a.ToAmount) - num(a.FromAmount);
    const widthB = num(b.ToAmount) <= 0 ? Number.MAX_SAFE_INTEGER : num(b.ToAmount) - num(b.FromAmount);
    return widthA - widthB;
  })[0];
}

/** Row-level problems, in the words of the person fixing them. */
function problemOf(d: BandDraft, all: BandDraft[], index: number): string {
  const from = num(d.FromAmount);
  const to = num(d.ToAmount);
  if (d.FromAmount === '' && d.ToAmount === '' && d.AdminComm === '') return '';
  if (from < 0) return 'From cannot be negative.';
  if (to > 0 && to < from) return 'To must be greater than From (or 0 for no upper limit).';
  if (Math.abs(num(d.AdminComm)) === 0) return 'Enter what the provider charges or pays.';
  if (isPercent(d.AdminCommType) && Math.abs(num(d.AdminComm)) > 100) return 'A percentage over 100 is almost certainly a typo.';
  if (num(d.GSTRatePercent) < 0 || num(d.GSTRatePercent) > 100) return 'GST % must be between 0 and 100. Leave it blank to use the tax engine’s rate.';

  const clash = all.find((o, i) =>
    i !== index
    && o.OperatorID === d.OperatorID
    && num(o.FromAmount) <= (to > 0 ? to : Number.MAX_SAFE_INTEGER)
    && (num(o.ToAmount) <= 0 ? Number.MAX_SAFE_INTEGER : num(o.ToAmount)) >= from
    && (o.FromAmount !== '' || o.ToAmount !== ''));
  if (clash) return `Overlaps the ${inr(num(clash.FromAmount))} – ${num(clash.ToAmount) > 0 ? inr(num(clash.ToAmount)) : '∞'} band.`;
  return '';
}

/**
 * The provider's rate card: what it charges (or pays) us per transaction size.
 *
 * A provider does not price one figure per operator — JaiJui's DMT card is "1–3 = 3.54,
 * 500–1000 = 5.90, 1001–3000 = 9.44, 10001–25000 = 0.17%". This is that card, entered as it is
 * written. It is also the only commission editor that works for a provider with no operator
 * routes at all, which is every coded provider (JaiJui, Pay1): they are routed from DMT Settings,
 * so the per-operator grid has nothing to list.
 *
 * Everything on screen is about getting the card in correctly: each row says in words what it
 * will do, overlapping or impossible bands are called out before they are saved, and the checker
 * answers the only question that matters — "on a ₹2,500 transfer, what does this cost us?"
 */
export function ProviderCommissionBands({ providerId, serviceId }: { providerId: string; serviceId: string }) {
  const toast = useToast();
  const qc = useQueryClient();

  const bandsKey = ['provider-commission-bands', providerId];
  const bands = useQuery({
    queryKey: bandsKey,
    queryFn: () => fetchList('ProviderConfigurationAdminCommission/GetByProviderID', { ProviderID: Number(providerId) }),
    enabled: Number(providerId) > 0,
  });
  const refresh = () => qc.invalidateQueries({ queryKey: bandsKey });

  const operators = useQuery({
    queryKey: ['fk', 'Operator/GetAll'],
    queryFn: () => fetchList('Operator/GetAll', OPT_PAYLOAD),
    staleTime: 60_000,
  });
  const serviceOperators = useMemo(
    () => (operators.data ?? []).filter((o) => String(getField(o, 'ServiceID')) === serviceId),
    [operators.data, serviceId],
  );

  const [drafts, setDrafts] = useState<BandDraft[]>([]);
  useEffect(() => {
    if (!serviceId) { setDrafts([]); return; }
    setDrafts(
      (bands.data ?? [])
        .filter((r: Row) => String(getField(r, 'ServiceID')) === serviceId)
        .map((r: Row) => ({
          key: nextKey(),
          id: Number(getField(r, 'ProviderConfigurationAdminCommissionID') ?? 0),
          OperatorID: String(getField(r, 'OperatorID') ?? 0),
          FromAmount: String(Number(getField(r, 'FromAmount') ?? 0)),
          ToAmount: String(Number(getField(r, 'ToAmount') ?? 0)),
          AdminComm: String(Math.abs(Number(getField(r, 'AdminComm') ?? 0))),
          AdminCommType: String(getField(r, 'AdminCommType') ?? 'Fix') || 'Fix',
          AdminCommissionCategory: String(getField(r, 'AdminCommissionCategory') ?? 'Surcharge') || 'Surcharge',
          GSTType: normalizeGstType(String(getField(r, 'GSTType') ?? '')),
          GSTRatePercent: Number(getField(r, 'GSTRatePercent') ?? 0) > 0
            ? String(Number(getField(r, 'GSTRatePercent')))
            : '',
        }))
        .sort((a, b) => num(a.FromAmount) - num(b.FromAmount)),
    );
  }, [serviceId, bands.data]);

  const patch = (key: string, changes: Partial<BandDraft>) =>
    setDrafts((prev) => prev.map((d) => (d.key === key ? { ...d, ...changes } : d)));

  /**
   * A rate card is usually written with a minus ("-5.90" = it comes off our float). Typing that
   * here would mean the sign and the Direction column disagree, so the sign is read as the
   * direction and the box keeps the plain number.
   */
  const setValue = (d: BandDraft, raw: string) => {
    if (raw.trim().startsWith('-')) {
      patch(d.key, { AdminComm: raw.replace('-', ''), AdminCommissionCategory: 'Surcharge' });
      return;
    }
    patch(d.key, { AdminComm: raw });
  };

  const payloadOf = (d: BandDraft) => ({
    ProviderConfigurationAdminCommissionID: d.id,
    ProviderID: Number(providerId),
    ServiceID: Number(serviceId),
    OperatorID: num(d.OperatorID),
    FromAmount: num(d.FromAmount),
    ToAmount: num(d.ToAmount),
    // Stored unsigned; Direction carries the sign, exactly as the per-operator grid does.
    AdminComm: Math.abs(num(d.AdminComm)),
    AdminCommType: d.AdminCommType || 'Fix',
    AdminCommissionCategory: d.AdminCommissionCategory || 'Surcharge',
    GSTType: normalizeGstType(d.GSTType),
    // 0 means "ask the tax engine", which keeps a rate change effective-dated in one place.
    GSTRatePercent: num(d.GSTRatePercent),
  });

  const saveOne = async (d: BandDraft) => {
    const res = await apiPost(
      d.id > 0 ? 'ProviderConfigurationAdminCommission/Update' : 'ProviderConfigurationAdminCommission/Create',
      payloadOf(d),
    );
    if (!res.ok) throw new Error(res.message || 'Could not save this band.');
  };

  const [savingKey, setSavingKey] = useState<string | null>(null);
  const rowMut = useMutation({
    mutationFn: (d: BandDraft) => saveOne(d),
    onSuccess: () => { toast.success('Band saved.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
    onSettled: () => setSavingKey(null),
  });

  const problems = useMemo(() => drafts.map((d, i) => problemOf(d, drafts, i)), [drafts]);
  const blocking = problems.some(Boolean);

  const saveAll = useMutation({
    mutationFn: async () => {
      const rows = drafts.filter((d) => Math.abs(num(d.AdminComm)) > 0);
      for (const d of rows) await saveOne(d);
      return rows.length;
    },
    onSuccess: (n) => { toast.success(`${n} band${n === 1 ? '' : 's'} saved.`); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const removeMut = useMutation({
    mutationFn: async (d: BandDraft) => {
      const res = await apiPost('ProviderConfigurationAdminCommission/Delete', {
        ProviderConfigurationAdminCommissionID: d.id,
      });
      if (!res.ok) throw new Error(res.message || 'Could not remove this band.');
    },
    onSuccess: () => { toast.success('Band removed.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const removeRow = (d: BandDraft) => {
    if (d.id === 0) { setDrafts((prev) => prev.filter((x) => x.key !== d.key)); return; }
    const upper = num(d.ToAmount) > 0 ? inr(num(d.ToAmount)) : 'no limit';
    if (window.confirm(`Remove the ${inr(num(d.FromAmount))} – ${upper} band?`)) removeMut.mutate(d);
  };

  // ── "what does this cost us" checker ───────────────────────────────────────
  const [checkAmount, setCheckAmount] = useState('');
  const [checkOperator, setCheckOperator] = useState('0');
  const checked = useMemo(() => {
    const amount = Number(checkAmount);
    if (!amount || amount <= 0) return null;
    const band = pickBand(drafts.filter((d) => Math.abs(num(d.AdminComm)) > 0), amount, Number(checkOperator));
    if (!band) return { amount, band: null as BandDraft | null, charge: 0 };
    return { amount, band, charge: chargeOn(band, amount) };
  }, [checkAmount, checkOperator, drafts]);

  if (!serviceId) return null;

  const saved = drafts.filter((d) => d.id > 0).length;

  return (
    <Card className="mt-6">
      <CardHeader className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <CardTitle className="flex items-center gap-2">
            <Layers className="size-4 text-brand-600" />
            Provider rate card
          </CardTitle>
          <p className="mt-1 max-w-2xl text-xs text-slate-500">
            What this provider charges or pays per transaction size — enter it exactly as the provider's card is
            written. Used whenever the provider's own reply carries no commission figure.
          </p>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          <span className="hidden text-xs text-slate-400 sm:inline">
            {saved === 0 ? 'Nothing saved yet' : `${saved} band${saved === 1 ? '' : 's'} saved`}
          </span>
          <Button size="sm" variant="outline" onClick={() => setDrafts((prev) => [...prev, emptyBand()])}>
            <Plus className="size-4" /> Add band
          </Button>
          <Button
            size="sm"
            loading={saveAll.isPending}
            disabled={drafts.length === 0 || blocking}
            title={blocking ? 'Fix the highlighted rows first' : undefined}
            onClick={() => saveAll.mutate()}
          >
            <Save className="size-4" /> Save all bands
          </Button>
        </div>
      </CardHeader>

      <CardBody className="p-0">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[52rem] border-collapse text-sm">
            <thead>
              <tr className="border-y border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="whitespace-nowrap px-4 py-3">Transaction amount</th>
                <th className="whitespace-nowrap px-3 py-3">Operator</th>
                <th className="whitespace-nowrap px-3 py-3" title="Minus = the provider takes it from us (comes off admin profit); plus = the provider pays us">
                  Provider charge
                </th>
                <th className="whitespace-nowrap px-3 py-3">Type</th>
                <th
                  className="whitespace-nowrap px-3 py-3"
                  title="Whether this band's charge already contains GST. Leave it on Follow profile to use the Commission Tax setting."
                >
                  GST
                </th>
                <th className="whitespace-nowrap px-3 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {drafts.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-4 py-12 text-center">
                    <p className="text-sm font-medium text-slate-600">No rate card yet</p>
                    <p className="mx-auto mt-1 max-w-md text-sm text-slate-400">
                      Add one row per slab of the provider's card — for example 500 to 1000, value 5.90, Fix,
                      “We pay the provider”.
                    </p>
                    <Button size="sm" variant="outline" className="mt-4" onClick={() => setDrafts([emptyBand()])}>
                      <Plus className="size-4" /> Add the first band
                    </Button>
                  </td>
                </tr>
              ) : (
                drafts.map((d, i) => {
                  const problem = problems[i];
                  const percent = isPercent(d.AdminCommType);
                  const value = Math.abs(num(d.AdminComm));
                  const from = num(d.FromAmount);
                  const to = num(d.ToAmount);
                  const sample = to > 0 ? to : Math.max(from, 1) * 2;
                  const meaning = value === 0
                    ? '—'
                    : percent
                      ? `${value}% — ${inr(chargeOn(d, sample))} on a ${inr(sample)} transfer`
                      : `${inr(value)} on every transfer in this band`;

                  return (
                    <>
                    <tr key={d.key} className={cn(problem && 'bg-danger-50/40')}>
                      <td className="w-[15rem] px-4 pb-1 pt-3">
                        <div className="flex items-center gap-1.5">
                          <input
                            type="number" step="0.01" inputMode="decimal" placeholder="From"
                            aria-label="From amount"
                            className={cn(FIELD, 'tabular-nums')}
                            value={d.FromAmount}
                            onChange={(e) => patch(d.key, { FromAmount: e.target.value })}
                          />
                          <span className="text-slate-300">–</span>
                          <input
                            type="number" step="0.01" inputMode="decimal" placeholder="To"
                            aria-label="To amount"
                            className={cn(FIELD, 'tabular-nums')}
                            value={d.ToAmount}
                            onChange={(e) => patch(d.key, { ToAmount: e.target.value })}
                          />
                        </div>
                      </td>

                      <td className="w-[11rem] px-3 pb-1 pt-3">
                        <select
                          className={FIELD}
                          aria-label="Operator"
                          value={d.OperatorID}
                          onChange={(e) => patch(d.key, { OperatorID: e.target.value })}
                        >
                          <option value="0">Any operator</option>
                          {serviceOperators.map((o) => (
                            <option key={String(getField(o, 'OperatorID'))} value={String(getField(o, 'OperatorID'))}>
                              {String(getField(o, 'OperatorName') ?? '')}
                            </option>
                          ))}
                        </select>
                      </td>

                      <td className="w-[13rem] px-3 pb-1 pt-3">
                        <div className="flex items-stretch gap-1.5">
                          {/* Minus = JaiJui takes it from us, so it comes off admin profit. Plus = the
                              provider pays us and it adds to profit. One control, no reading required. */}
                          <div className="flex h-9 shrink-0 overflow-hidden rounded-lg border border-slate-200">
                            <button
                              type="button"
                              aria-label="Provider takes this from us"
                              aria-pressed={!weCharge(d.AdminCommissionCategory)}
                              title="The provider takes this from us — subtracted from admin profit"
                              onClick={() => patch(d.key, { AdminCommissionCategory: 'Surcharge' })}
                              className={cn(
                                'w-9 text-base font-bold leading-none transition-colors',
                                !weCharge(d.AdminCommissionCategory)
                                  ? 'bg-danger-500 text-white'
                                  : 'bg-white text-slate-400 hover:bg-slate-50',
                              )}
                            >
                              −
                            </button>
                            <button
                              type="button"
                              aria-label="Provider pays this to us"
                              aria-pressed={weCharge(d.AdminCommissionCategory)}
                              title="The provider pays this to us — added to admin profit"
                              onClick={() => patch(d.key, { AdminCommissionCategory: 'Commission' })}
                              className={cn(
                                'w-9 border-l border-slate-200 text-base font-bold leading-none transition-colors',
                                weCharge(d.AdminCommissionCategory)
                                  ? 'bg-success-600 text-white'
                                  : 'bg-white text-slate-400 hover:bg-slate-50',
                              )}
                            >
                              +
                            </button>
                          </div>
                          <div className="relative min-w-0 flex-1">
                            <span className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-xs text-slate-400">
                              {percent ? '%' : '₹'}
                            </span>
                            <input
                              type="number" step="0.00001" inputMode="decimal" placeholder="0.00"
                              aria-label="Value"
                              className={cn(FIELD, 'pl-6 tabular-nums')}
                              value={d.AdminComm}
                              onChange={(e) => setValue(d, e.target.value)}
                            />
                          </div>
                        </div>
                      </td>

                      <td className="w-[8.5rem] px-3 pb-1 pt-3">
                        <select
                          className={FIELD}
                          aria-label="Value type"
                          value={d.AdminCommType}
                          onChange={(e) => patch(d.key, { AdminCommType: e.target.value })}
                        >
                          <option value="Fix">Flat ₹</option>
                          <option value="Flexi">Percent %</option>
                        </select>
                      </td>

                      {/* This band's own GST decision. "Follow profile" is what every band held
                          before this column existed, and it still means the Commission Tax screen
                          decides — so a card whose bands are all taxed the same way needs nothing
                          here, while a card with one exempt band can finally say so. */}
                      <td className="w-[12rem] px-3 pb-1 pt-3">
                        <div className="flex items-stretch gap-1.5">
                          <select
                            className={cn(FIELD, 'min-w-0 flex-1')}
                            aria-label="GST treatment"
                            value={d.GSTType}
                            onChange={(e) => patch(d.key, { GSTType: e.target.value })}
                          >
                            {GST_TYPES.map((t) => (
                              <option key={t} value={t}>{GST_TYPE_LABEL[t]}</option>
                            ))}
                          </select>
                          {d.GSTType !== 'DEFAULT' && d.GSTType !== 'NONE' && (
                            <div className="relative w-[4.5rem] shrink-0">
                              <input
                                type="number" step="0.01" inputMode="decimal" placeholder="auto"
                                aria-label="GST percent"
                                title="Leave blank to take the rate from the tax engine for this service"
                                className={cn(FIELD, 'pr-5 tabular-nums')}
                                value={d.GSTRatePercent}
                                onChange={(e) => patch(d.key, { GSTRatePercent: e.target.value })}
                              />
                              <span className="pointer-events-none absolute right-2 top-1/2 -translate-y-1/2 text-xs text-slate-400">
                                %
                              </span>
                            </div>
                          )}
                        </div>
                      </td>

                      <td className="w-[6rem] px-3 pb-1 pt-3">
                        <div className="flex items-center justify-end gap-1">
                          <Button
                            size="sm" variant="ghost" title="Save this band"
                            loading={rowMut.isPending && savingKey === d.key}
                            disabled={Boolean(problem)}
                            onClick={() => { setSavingKey(d.key); rowMut.mutate(d); }}
                          >
                            <Save className="size-4 text-brand-600" />
                          </Button>
                          <Button size="sm" variant="ghost" title="Remove this band" onClick={() => removeRow(d)}>
                            <Trash2 className="size-4 text-slate-400" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                    <tr key={`${d.key}-note`} className={cn('border-b border-slate-100 last:border-0', problem && 'bg-danger-50/40')}>
                      <td colSpan={6} className="px-4 pb-3 pt-0">
                        {problem ? (
                          <p className="flex items-center gap-1.5 text-xs font-medium text-danger-600">
                            <AlertTriangle className="size-3.5 shrink-0" />
                            {problem}
                          </p>
                        ) : (
                          <p className="text-xs text-slate-500">
                            <span className={cn('font-medium', weCharge(d.AdminCommissionCategory) ? 'text-success-700' : 'text-danger-600')}>
                              {weCharge(d.AdminCommissionCategory) ? '+' : '−'}{meaning}
                            </span>
                            <span className="text-slate-400">
                              {' · '}
                              {weCharge(d.AdminCommissionCategory)
                                ? 'added to admin profit on this transaction'
                                : 'subtracted from admin profit on this transaction'}
                              {' · '}
                              {gstPhrase(chargeOn(d, sample), d.GSTType, num(d.GSTRatePercent), inr)}
                              {d.id === 0 && ' · not saved yet'}
                            </span>
                          </p>
                        )}
                      </td>
                    </tr>
                    </>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* The question an operator actually asks of a rate card. */}
        <div className="flex flex-wrap items-end gap-3 border-t border-slate-100 bg-slate-50/60 px-4 py-3">
          <div className="flex items-center gap-2">
            <Calculator className="size-4 text-slate-400" />
            <span className="text-xs font-semibold uppercase tracking-wide text-slate-500">Check an amount</span>
          </div>
          <input
            type="number" inputMode="decimal" placeholder="e.g. 2500"
            aria-label="Amount to check"
            className={cn(FIELD, 'w-32 tabular-nums')}
            value={checkAmount}
            onChange={(e) => setCheckAmount(e.target.value)}
          />
          {serviceOperators.length > 0 && (
            <select className={cn(FIELD, 'w-44')} aria-label="Operator to check" value={checkOperator} onChange={(e) => setCheckOperator(e.target.value)}>
              <option value="0">Any operator</option>
              {serviceOperators.map((o) => (
                <option key={String(getField(o, 'OperatorID'))} value={String(getField(o, 'OperatorID'))}>
                  {String(getField(o, 'OperatorName') ?? '')}
                </option>
              ))}
            </select>
          )}
          {checked && (
            checked.band ? (
              <p className="text-sm text-slate-600">
                Band{' '}
                <span className="font-medium text-slate-800">
                  {inr(num(checked.band.FromAmount))} – {num(checked.band.ToAmount) > 0 ? inr(num(checked.band.ToAmount)) : '∞'}
                </span>{' '}
                →{' '}
                <span className={cn('font-semibold', weCharge(checked.band.AdminCommissionCategory) ? 'text-success-700' : 'text-danger-600')}>
                  {weCharge(checked.band.AdminCommissionCategory) ? '+' : '−'}{inr(checked.charge)}
                </span>{' '}
                on a {inr(checked.amount)} transfer, {weCharge(checked.band.AdminCommissionCategory) ? 'added to' : 'off'} admin profit
                {' · '}
                <span className="text-slate-500">
                  {gstPhrase(checked.charge, checked.band.GSTType, num(checked.band.GSTRatePercent), inr)}
                </span>
                {normalizeGstType(checked.band.GSTType) !== 'DEFAULT'
                  && num(checked.band.GSTRatePercent) > 0
                  && (
                    <span className="text-slate-400">
                      {' · '}
                      {inr(Math.abs(splitCharge(checked.charge, checked.band.GSTType, num(checked.band.GSTRatePercent)).base))}
                      {' is the real cost; the GST is input credit'}
                    </span>
                  )}
              </p>
            ) : (
              <p className="text-sm text-slate-500">
                No band covers {inr(checked.amount)} — the provider's own reply would be used, or nothing recorded.
              </p>
            )
          )}
        </div>
      </CardBody>

      <div className="border-t border-slate-100 px-4 py-2.5 text-xs text-slate-400">
        <span className="font-medium text-slate-500">−</span> is what the provider takes from us and{' '}
        <span className="font-medium text-slate-500">+</span> is what it pays us, so admin profit on a transaction is
        what the retailer was charged minus the provider&apos;s own charge. Leave{' '}
        <span className="font-medium text-slate-500">To</span> at 0 for a band with no upper limit. An operator-specific
        band wins over “Any operator”, and the narrowest band wins when two overlap.
        <br />
        <span className="font-medium text-slate-500">GST</span>: most cards are quoted inclusive — JaiJui&apos;s DMT
        charges all divide by 1.18 cleanly (5.90 = 5.00 + 0.90) — and the GST inside a charge we pay is input credit,
        so recording it is money back. Leave a band on{' '}
        <span className="font-medium text-slate-500">Follow profile</span> to keep using the Commission Tax setting,
        and leave the <span className="font-medium text-slate-500">%</span> blank to take the rate from the tax engine
        so a rate change stays in one place. Only a successful transaction is charged GST.
      </div>
    </Card>
  );
}
