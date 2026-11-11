import { useMemo, useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { AlertTriangle, Play, RotateCcw } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { inr, apiDate } from '@/lib/format';
import { getField } from '@/lib/row';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

interface RuleMatch {
  TaxRuleMasterID: number;
  RuleName: string;
  Priority: number;
  BillingBasis: string;
  Rate: number;
  SACCode: string;
  MatchedOn: string;
  IsSelected: boolean;
}

interface Result {
  TaxRuleMasterID: number;
  RuleName: string;
  BillingBasis: string;
  GrossValue: number;
  TaxableValue: number;
  SupplyNature: string;
  SupplierStateCode: string;
  PlaceOfSupplyStateCode: string;
  SACCode: string;
  CGSTRate: number; CGSTAmount: number;
  SGSTRate: number; SGSTAmount: number;
  IGSTRate: number; IGSTAmount: number;
  CessAmount: number;
  TotalTax: number;
  InvoiceValue: number;
  RoundOff: number;
  TDSSectionCode: string;
  TDSRate: number;
  TDSAmount: number;
  PartyRelationship: string;
  ITCEligible: boolean;
  ReverseCharge: boolean;
  Warning: string;
  Candidates: RuleMatch[];
}

interface Form {
  direction: string;
  serviceId: string;
  partyUserId: string;
  transactionDate: string;
  grossValue: string;
  commissionValue: string;
  chargeValue: string;
  isTaxInclusive: boolean;
  supplierStateCode: string;
  placeOfSupplyStateCode: string;
  partyRegistrationStatus: string;
  partyRelationship: string;
  partyYtdCommission: string;
  partyHasValidPAN: boolean;
  partyIs206ABFlagged: boolean;
}

function blankForm(): Form {
  return {
    direction: 'SALE',
    serviceId: '0',
    partyUserId: '0',
    transactionDate: apiDate(new Date()),
    grossValue: '1000',
    commissionValue: '0',
    chargeValue: '0',
    isTaxInclusive: true,
    supplierStateCode: '',
    placeOfSupplyStateCode: '',
    partyRegistrationStatus: '',
    partyRelationship: '',
    partyYtdCommission: '0',
    partyHasValidPAN: true,
    partyIs206ABFlagged: false,
  };
}

/** Indian state codes as GST uses them — the first two digits of a GSTIN. */
const STATES: Array<[string, string]> = [
  ['01', '01 Jammu & Kashmir'], ['02', '02 Himachal Pradesh'], ['03', '03 Punjab'],
  ['04', '04 Chandigarh'], ['05', '05 Uttarakhand'], ['06', '06 Haryana'], ['07', '07 Delhi'],
  ['08', '08 Rajasthan'], ['09', '09 Uttar Pradesh'], ['10', '10 Bihar'], ['11', '11 Sikkim'],
  ['12', '12 Arunachal Pradesh'], ['13', '13 Nagaland'], ['14', '14 Manipur'], ['15', '15 Mizoram'],
  ['16', '16 Tripura'], ['17', '17 Meghalaya'], ['18', '18 Assam'], ['19', '19 West Bengal'],
  ['20', '20 Jharkhand'], ['21', '21 Odisha'], ['22', '22 Chhattisgarh'], ['23', '23 Madhya Pradesh'],
  ['24', '24 Gujarat'], ['26', '26 Dadra & Nagar Haveli and Daman & Diu'], ['27', '27 Maharashtra'],
  ['29', '29 Karnataka'], ['30', '30 Goa'], ['31', '31 Lakshadweep'], ['32', '32 Kerala'],
  ['33', '33 Tamil Nadu'], ['34', '34 Puducherry'], ['35', '35 Andaman & Nicobar Islands'],
  ['36', '36 Telangana'], ['37', '37 Andhra Pradesh'], ['38', '38 Ladakh'],
];

async function simulate(f: Form): Promise<{ result: Result | null; error: string }> {
  const res = await apiPost<Result>('TaxEngine/Simulate', {
    Direction: f.direction,
    ServiceID: Number(f.serviceId || 0),
    PartyUserID: Number(f.partyUserId || 0),
    TransactionDate: f.transactionDate,
    GrossValue: Number(f.grossValue || 0),
    CommissionValue: Number(f.commissionValue || 0),
    ChargeValue: Number(f.chargeValue || 0),
    IsTaxInclusive: f.isTaxInclusive,
    SupplierStateCode: f.supplierStateCode,
    PlaceOfSupplyStateCode: f.placeOfSupplyStateCode,
    PartyRegistrationStatus: f.partyRegistrationStatus,
    PartyRelationship: f.partyRelationship,
    PartyYtdCommission: Number(f.partyYtdCommission || 0),
    PartyHasValidPAN: f.partyHasValidPAN,
    PartyIs206ABFlagged: f.partyIs206ABFlagged,
  });
  if (!res.ok) return { result: null, error: res.message || 'The simulation could not be run.' };
  return { result: res.data, error: '' };
}

/** A labelled figure. Muted when zero, so the line that is actually doing something stands out. */
function Figure({ label, value, hint, strong = false }: { label: string; value: number; hint?: string; strong?: boolean }) {
  return (
    <div className="flex items-baseline justify-between gap-4 py-1.5">
      <span className={`text-sm ${strong ? 'font-medium text-slate-700' : 'text-slate-500'}`}>
        {label}
        {hint ? <span className="ml-1.5 text-xs text-slate-400">{hint}</span> : null}
      </span>
      <span className={`tabular-nums ${value === 0 ? 'text-slate-300' : strong ? 'font-semibold text-slate-900' : 'text-slate-700'}`}>
        {value === 0 ? '—' : inr(value)}
      </span>
    </div>
  );
}

function Pill({ children, tone = 'slate' }: { children: React.ReactNode; tone?: 'slate' | 'green' | 'amber' }) {
  const tones = {
    slate: 'bg-slate-100 text-slate-600',
    green: 'bg-emerald-50 text-emerald-700',
    amber: 'bg-amber-50 text-amber-700',
  } as const;
  return <span className={`rounded px-1.5 py-0.5 text-xs font-medium ${tones[tone]}`}>{children}</span>;
}

/**
 * Tax Rule Simulator — put a hypothetical transaction in, see what the engine decides and WHY.
 *
 * This screen exists because a tax rule is otherwise unobservable. You change a row in the rule
 * master, and nothing visible happens until a month of invoices has been raised on it. The
 * candidate list is the part that matters: every rule that matched, in the order the engine
 * ranks them, with the one that won marked. When a rule "is not working", the answer is almost
 * always that a more specific one outranked it — and it is right here.
 *
 * Read-only. Nothing is written, nothing is stamped, no number is allocated. (TaxEngine/Simulate.)
 */
export function TaxSimulatorScreen({ def }: CustomScreenProps) {
  const [form, setForm] = useState<Form>(blankForm);
  const set = <K extends keyof Form>(k: K, v: Form[K]) => setForm((p) => ({ ...p, [k]: v }));

  const servicesQ = useQuery({
    queryKey: ['services-ddl'],
    queryFn: () => fetchList('Service/GetAll', { PageNumber: 1, PageSize: 500, IsActive: true }),
  });
  const services = useMemo(
    () =>
      (servicesQ.data ?? []).map((s: Row) => ({
        value: String(getField(s, 'ServiceID') ?? 0),
        label: String(getField(s, 'Name', 'ServiceName') ?? ''),
      })),
    [servicesQ.data],
  );

  const run = useMutation({ mutationFn: () => simulate(form) });
  const result = run.data?.result ?? null;
  const error = run.data?.error ?? '';

  const intrastate = result ? result.SupplyNature?.toUpperCase() === 'INTRASTATE' : false;

  return (
    <div className="space-y-4">
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      <div className="grid gap-4 lg:grid-cols-[minmax(0,420px)_minmax(0,1fr)]">
        {/* ── Inputs ─────────────────────────────────────────────────────── */}
        <Card>
          <CardBody className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <Select label="Direction" value={form.direction} onChange={(e) => set('direction', e.target.value)}>
                <option value="SALE">Sale (outward)</option>
                <option value="PURCHASE">Purchase (inward)</option>
              </Select>
              <Input label="Date" type="date" value={form.transactionDate} onChange={(e) => set('transactionDate', e.target.value)} />
            </div>

            <Select label="Service" value={form.serviceId} onChange={(e) => set('serviceId', e.target.value)}>
              <option value="0">Any service</option>
              {services.map((s) => (
                <option key={s.value} value={s.value}>{s.label}</option>
              ))}
            </Select>

            <div className="grid grid-cols-3 gap-3">
              <Input label="Gross" type="number" value={form.grossValue} onChange={(e) => set('grossValue', e.target.value)} />
              <Input label="Commission" type="number" value={form.commissionValue} onChange={(e) => set('commissionValue', e.target.value)} />
              <Input label="Charge" type="number" value={form.chargeValue} onChange={(e) => set('chargeValue', e.target.value)} />
            </div>
            <p className="-mt-1 text-xs text-slate-400">
              The rule decides which of the three it bills on. Recharge bills the gross top-up; DMT bills only the commission.
            </p>

            <div className="grid grid-cols-2 gap-3">
              <Select label="Supplier state" value={form.supplierStateCode} onChange={(e) => set('supplierStateCode', e.target.value)}>
                <option value="">Not set</option>
                {STATES.map(([c, l]) => <option key={c} value={c}>{l}</option>)}
              </Select>
              <Select label="Place of supply" value={form.placeOfSupplyStateCode} onChange={(e) => set('placeOfSupplyStateCode', e.target.value)}>
                <option value="">Not set</option>
                {STATES.map(([c, l]) => <option key={c} value={c}>{l}</option>)}
              </Select>
            </div>
            <p className="-mt-1 text-xs text-slate-400">
              Same state means CGST + SGST; different states means IGST. Nothing else decides it.
            </p>

            <div className="grid grid-cols-2 gap-3">
              <Input label="Party user ID" type="number" value={form.partyUserId}
                     onChange={(e) => set('partyUserId', e.target.value)} />
              <Select label="Party registration" value={form.partyRegistrationStatus}
                      onChange={(e) => set('partyRegistrationStatus', e.target.value)}>
                <option value="">From their profile</option>
                <option value="REGISTERED">Registered</option>
                <option value="UNREGISTERED">Unregistered</option>
                <option value="COMPOSITION">Composition</option>
              </Select>
            </div>
            <p className="-mt-1 text-xs text-slate-400">
              Leave the overrides empty to resolve from the party&apos;s saved profile — that is what a real transaction does.
            </p>

            <Select label="Relationship" value={form.partyRelationship} onChange={(e) => set('partyRelationship', e.target.value)}>
              <option value="">From their profile</option>
              <option value="P2P">P2P — principal to principal (a discount, no 194H)</option>
              <option value="AGENT">AGENT — commission, 194H applies</option>
            </Select>

            <div className="grid grid-cols-2 gap-3">
              <Input label="Party YTD commission" type="number" value={form.partyYtdCommission}
                     onChange={(e) => set('partyYtdCommission', e.target.value)} />
              <div className="flex flex-col justify-end gap-1.5 pb-1">
                <label className="flex items-center gap-2 text-sm text-slate-600">
                  <input type="checkbox" checked={form.isTaxInclusive} onChange={(e) => set('isTaxInclusive', e.target.checked)} />
                  Amount is tax-inclusive
                </label>
                <label className="flex items-center gap-2 text-sm text-slate-600">
                  <input type="checkbox" checked={form.partyHasValidPAN} onChange={(e) => set('partyHasValidPAN', e.target.checked)} />
                  Party has a valid PAN
                </label>
                <label className="flex items-center gap-2 text-sm text-slate-600">
                  <input type="checkbox" checked={form.partyIs206ABFlagged} onChange={(e) => set('partyIs206ABFlagged', e.target.checked)} />
                  Flagged under 206AB
                </label>
              </div>
            </div>

            <div className="flex gap-2 pt-1">
              <Button onClick={() => run.mutate()} disabled={run.isPending}>
                <Play className="mr-1.5 h-4 w-4" />
                {run.isPending ? 'Resolving…' : 'Simulate'}
              </Button>
              <Button variant="secondary" onClick={() => { setForm(blankForm()); run.reset(); }}>
                <RotateCcw className="mr-1.5 h-4 w-4" />
                Reset
              </Button>
            </div>
          </CardBody>
        </Card>

        {/* ── Result ─────────────────────────────────────────────────────── */}
        <div className="space-y-4">
          {!result && !error ? (
            <Card>
              <CardBody>
                <EmptyState
                  title="Nothing resolved yet"
                  description="Fill in a transaction and press Simulate. Nothing is written — this only asks the engine what it would decide."
                />
              </CardBody>
            </Card>
          ) : null}

          {error ? (
            <Card>
              <CardBody>
                <div className="flex items-start gap-2 text-sm text-danger-600">
                  <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
                  <span>{error}</span>
                </div>
              </CardBody>
            </Card>
          ) : null}

          {result ? (
            <>
              {result.Warning ? (
                <Card>
                  <CardBody>
                    <div className="flex items-start gap-2 text-sm text-amber-700">
                      <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
                      <span>{result.Warning}</span>
                    </div>
                  </CardBody>
                </Card>
              ) : null}

              <Card>
                <CardBody>
                  <div className="mb-3 flex flex-wrap items-center gap-2">
                    <span className="text-sm font-semibold text-slate-900">
                      {result.RuleName || 'No rule matched'}
                    </span>
                    {result.TaxRuleMasterID > 0 ? <Pill>#{result.TaxRuleMasterID}</Pill> : null}
                    {result.BillingBasis ? <Pill tone="green">bills on {result.BillingBasis}</Pill> : null}
                    {result.SACCode ? <Pill>SAC {result.SACCode}</Pill> : null}
                    {result.SupplyNature ? <Pill>{result.SupplyNature}</Pill> : null}
                    {result.ReverseCharge ? <Pill tone="amber">reverse charge</Pill> : null}
                    {result.ITCEligible ? <Pill tone="green">ITC eligible</Pill> : null}
                  </div>

                  <div className="grid gap-x-8 sm:grid-cols-2">
                    <div className="divide-y divide-slate-100">
                      <Figure label="Gross" value={result.GrossValue} />
                      <Figure label="Taxable value" value={result.TaxableValue} strong />
                      {intrastate ? (
                        <>
                          <Figure label="CGST" value={result.CGSTAmount} hint={`@ ${result.CGSTRate}%`} />
                          <Figure label="SGST" value={result.SGSTAmount} hint={`@ ${result.SGSTRate}%`} />
                        </>
                      ) : (
                        <Figure label="IGST" value={result.IGSTAmount} hint={`@ ${result.IGSTRate}%`} />
                      )}
                      <Figure label="Cess" value={result.CessAmount} />
                      <Figure label="Round off" value={result.RoundOff} />
                      <Figure label="Invoice value" value={result.InvoiceValue} strong />
                    </div>

                    <div className="divide-y divide-slate-100">
                      <Figure
                        label="TDS"
                        value={result.TDSAmount}
                        hint={result.TDSSectionCode ? `${result.TDSSectionCode} @ ${result.TDSRate}%` : 'none'}
                        strong
                      />
                      <div className="flex items-baseline justify-between gap-4 py-1.5">
                        <span className="text-sm text-slate-500">Relationship</span>
                        <span className="text-sm text-slate-700">{result.PartyRelationship || '—'}</span>
                      </div>
                      <div className="flex items-baseline justify-between gap-4 py-1.5">
                        <span className="text-sm text-slate-500">Supplier → place of supply</span>
                        <span className="tabular-nums text-sm text-slate-700">
                          {result.SupplierStateCode || '—'} → {result.PlaceOfSupplyStateCode || '—'}
                        </span>
                      </div>
                    </div>
                  </div>
                </CardBody>
              </Card>

              <Card>
                <CardBody>
                  <h3 className="mb-1 text-sm font-semibold text-slate-900">Why this rule won</h3>
                  <p className="mb-3 text-xs text-slate-400">
                    Every rule that matched, most specific first. When a rule you expected is listed but not selected,
                    a more specific one outranked it — that is the whole answer.
                  </p>
                  {result.Candidates?.length ? (
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="border-b border-slate-200 text-left text-xs uppercase tracking-wide text-slate-400">
                            <th className="py-2 pr-3 font-medium">Rule</th>
                            <th className="py-2 pr-3 font-medium">Matched on</th>
                            <th className="py-2 pr-3 text-right font-medium">Priority</th>
                            <th className="py-2 pr-3 font-medium">Basis</th>
                            <th className="py-2 pr-3 text-right font-medium">Rate</th>
                            <th className="py-2 font-medium">SAC</th>
                          </tr>
                        </thead>
                        <tbody>
                          {result.Candidates.map((c) => (
                            <tr
                              key={c.TaxRuleMasterID}
                              className={`border-b border-slate-50 ${c.IsSelected ? 'bg-emerald-50/50' : ''}`}
                            >
                              <td className="py-2 pr-3">
                                <span className={c.IsSelected ? 'font-medium text-slate-900' : 'text-slate-600'}>
                                  {c.RuleName}
                                </span>
                                {c.IsSelected ? <span className="ml-2"><Pill tone="green">selected</Pill></span> : null}
                              </td>
                              <td className="py-2 pr-3 text-xs text-slate-500">{c.MatchedOn}</td>
                              <td className="py-2 pr-3 text-right tabular-nums text-slate-600">{c.Priority}</td>
                              <td className="py-2 pr-3 text-slate-600">{c.BillingBasis}</td>
                              <td className="py-2 pr-3 text-right tabular-nums text-slate-600">{c.Rate}%</td>
                              <td className="py-2 text-slate-600">{c.SACCode}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  ) : (
                    <p className="text-sm text-slate-500">
                      No rule matched this combination. The engine fell back to its default, which is why the figures
                      above may not be what you expect — a rule is missing, not wrong.
                    </p>
                  )}
                </CardBody>
              </Card>
            </>
          ) : null}
        </div>
      </div>
    </div>
  );
}
