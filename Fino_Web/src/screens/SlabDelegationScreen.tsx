import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, ArrowLeft, CheckCircle2, Layers, Plus, Save, Trash2, UserPlus, Users, Wrench, X,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { inr } from '@/lib/format';
import { useAuth } from '@/features/auth/AuthContext';
import { UserTypeCode, roleLabel } from '@/lib/roles';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { Autocomplete } from '@/components/ui/Autocomplete';
import type { CustomScreenProps } from './registry';

/**
 * My Commission Slabs — hierarchy-based commission slab delegation.
 *
 * A super-distributor or distributor sees three things here, in the order the work happens:
 *   1. the card assigned to THEM, read-only, which is the ceiling everything below is cut from;
 *   2. the cards they own, each with how many bands are out of limit and how many users hold it;
 *   3. inside a card, every band with the owner's figure next to the ceiling it is measured
 *      against, and their own tier column marked — because that column is where whatever they
 *      shave off the tiers below actually lands.
 *
 * Validation here is a mirror, never the authority: the API re-derives the caller, re-reads the
 * parent band and re-checks every rule before it writes. The point of checking in the browser is
 * that the owner can see their headroom while they type instead of discovering it in a refusal.
 */

type Row = Record<string, unknown>;

const num = (r: Row, k: string) => Number(getField(r, k) ?? 0);
const str = (r: Row, k: string) => String(getField(r, k) ?? '');

const isPercent = (t: string) => /flexi|percent|per|%/i.test(t);
const isSurcharge = (c: string) => /sur|charge/i.test(c);

/** How a saved figure reads on a read-only card: "₹9.00" or "0.75%", minus for a surcharge. */
function moneyLabel(value: number, commType: string, category: string): string {
  const magnitude = Math.abs(value);
  const body = isPercent(commType) ? `${magnitude}%` : inr(magnitude);
  return isSurcharge(category) || value < 0 ? `-${body}` : body;
}

/**
 * The tier columns, by the name the API uses for them. Which of these a given owner may actually
 * SET comes from the server (`SettableTiers` on the slab view) — a distributor decides a retailer
 * rate and nothing else, a super-distributor decides the distributor and retailer rates. The list
 * is not worked out here: the same answer drives which columns the save endpoint will write, so a
 * column this screen should not draw is also a column the server refuses.
 */
const TIER_LABEL: Record<string, string> = {
  Retailer: 'Retailer',
  Distributor: 'Distributor',
  SuperDistributor: 'Super Distributor',
  Client: 'Client',
};

/** Every tier, for the read-only views where the whole split is the point. */
const ALL_TIERS = ['Retailer', 'Distributor', 'SuperDistributor', 'Client'];

type CatKey = 'COM' | 'SUR';
type TypeKey = 'PER' | 'FIX';

// The engine reads these columns as free text and understands Flexi|Fix and
// Commission|Surcharge — the same mapping the admin slab editor writes.
const toApiType = (t: TypeKey) => (t === 'PER' ? 'Flexi' : 'Fix');
const toApiCat = (c: CatKey) => (c === 'SUR' ? 'Surcharge' : 'Commission');
const fromApiType = (t: string): TypeKey => (/flexi|percent|per|%/i.test(t) ? 'PER' : 'FIX');
const fromApiCat = (c: string): CatKey => (/sur|charge/i.test(c) ? 'SUR' : 'COM');

interface TierInput {
  value: string;
  category: CatKey;
  type: TypeKey;
}

const emptyTier = (): TierInput => ({ value: '0', category: 'COM', type: 'FIX' });

/**
 * The four ways a tier figure can be read, as ONE choice. The row used to carry two separate
 * toggles per tier (COM/SUR and PER/FIX) beside the number, which on a card of two dozen bands
 * and three tiers was a wall of buttons nobody could read. One compact select says the same.
 */
const KIND_OPTS: { value: `${CatKey}-${TypeKey}`; label: string }[] = [
  { value: 'COM-FIX', label: 'Comm ₹' },
  { value: 'COM-PER', label: 'Comm %' },
  { value: 'SUR-FIX', label: 'Surcharge ₹' },
  { value: 'SUR-PER', label: 'Surcharge %' },
];
const kindOf = (t: TierInput): `${CatKey}-${TypeKey}` => `${t.category}-${t.type}`;
const fromKind = (k: string): Pick<TierInput, 'category' | 'type'> => {
  const [category, type] = k.split('-') as [CatKey, TypeKey];
  return { category, type };
};

/** The number and its kind, side by side, in one line. */
function TierEditor({ tier, value, onChange, compact = true }: {
  tier: string;
  value: TierInput;
  onChange: (next: TierInput) => void;
  compact?: boolean;
}) {
  return (
    <div className="flex items-center gap-1">
      <input
        type="number" step="0.01" min="0"
        aria-label={`${TIER_LABEL[tier] ?? tier} commission`}
        value={value.value}
        onChange={(e) => onChange({ ...value, value: e.target.value })}
        className={cn(
          'rounded-lg border border-slate-200 px-2 text-sm tabular-nums focus:border-brand-500 focus:outline-none',
          compact ? 'h-8 w-20' : 'h-10 w-24',
        )}
      />
      <select
        aria-label={`${TIER_LABEL[tier] ?? tier} kind`}
        value={kindOf(value)}
        onChange={(e) => onChange({ ...value, ...fromKind(e.target.value) })}
        className={cn(
          'rounded-lg border border-slate-200 bg-white px-1.5 text-xs text-slate-700 focus:border-brand-500 focus:outline-none',
          compact ? 'h-8' : 'h-10',
        )}
      >
        {KIND_OPTS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
      </select>
    </div>
  );
}

/** A tier input read off a saved band. */
const tierFrom = (band: Row, tier: string): TierInput => ({
  value: String(Math.abs(num(band, `${tier}Comm`))),
  category: fromApiCat(str(band, `${tier}CommissionCategory`)),
  type: fromApiType(str(band, `${tier}CommType`)),
});

/**
 * What the tiers an owner sets pay out at one amount — the same arithmetic the server validates
 * with, kept small and local so the owner can watch their margin while they type. The server
 * stays the authority; if the two ever disagree the save is refused with the server's figure.
 */
function givenAwayAt(tiers: Record<string, TierInput>, settable: string[], amount: number): number {
  let total = 0;
  for (const tier of settable) {
    const t = tiers[tier];
    if (!t) continue;
    const magnitude = Math.abs(Number(t.value) || 0);
    const money = t.type === 'PER' ? (amount * magnitude) / 100 : magnitude;
    total += t.category === 'SUR' ? -money : money;
  }
  return total;
}

/**
 * One editable band.
 *
 * The owner sets the rates that go DOWN. Their own margin is not an input — it is whatever the
 * parent's allowance leaves after those rates — so it is shown as a computed figure, and every
 * change they make has a visible consequence before they save it.
 */
function BandRow({
  band, settable, ownTier, onSaved,
}: {
  band: Row;
  settable: string[];
  ownTier: string;
  onSaved: () => void;
}) {
  const toast = useToast();
  const detailId = num(band, 'SlabDetailID');

  const [tiers, setTiers] = useState<Record<string, TierInput>>(() =>
    Object.fromEntries(settable.map((t) => [t, tierFrom(band, t)])));
  const [fromAmount, setFromAmount] = useState(() => String(num(band, 'FromAmount')));
  const [toAmount, setToAmount] = useState(() => String(num(band, 'ToAmount')));

  // A background refetch has to reach the inputs, but keying on the row id alone would throw away
  // whatever the owner was mid-way through typing on every refresh. Tied to the SAVED figures
  // instead, so only a real change resets the form.
  const signature = [
    ...settable.map((t) => `${num(band, `${t}Comm`)}|${str(band, `${t}CommType`)}|${str(band, `${t}CommissionCategory`)}`),
    num(band, 'FromAmount'), num(band, 'ToAmount'),
  ].join('~');
  useEffect(() => {
    setTiers(Object.fromEntries(settable.map((t) => [t, tierFrom(band, t)])));
    setFromAmount(String(num(band, 'FromAmount')));
    setToAmount(String(num(band, 'ToAmount')));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [signature]);

  const from = Number(fromAmount) || 0;
  const to = Number(toAmount) || 0;
  const allowanceAtFrom = num(band, "AllowanceAtFrom");
  const allowanceAtTo = num(band, "AllowanceAtTo");

  // The margin preview, at both ends of the band. Equal on a flat band; on a percentage band the
  // two differ, and that spread is worth showing because it IS the owner's exposure.
  const marginAtFrom = allowanceAtFrom - givenAwayAt(tiers, settable, from);
  const marginAtTo = allowanceAtTo - givenAwayAt(tiers, settable, to);
  const marginSpread = Math.abs(marginAtTo - marginAtFrom) > 0.005;

  // What the card above pays the owner for this band. One figure on a flat rate; two on a
  // percentage one, where the band's own ends are what make it move.
  const allowanceSpread = Math.abs(allowanceAtTo - allowanceAtFrom) > 0.005;
  const allowanceLabel = allowanceSpread
    ? `${inr(allowanceAtFrom)} – ${inr(allowanceAtTo)}`
    : inr(allowanceAtTo);

  const error = useMemo(() => {
    if (!Number.isFinite(from) || !Number.isFinite(to) || to <= 0) return 'Enter a valid amount range.';
    if (from < 0) return 'From amount cannot be negative.';
    if (to < from) return 'To amount must be at least the From amount.';
    for (const tier of settable) {
      const v = Number(tiers[tier]?.value);
      if (!Number.isFinite(v) || v < 0) return `${TIER_LABEL[tier] ?? tier}: enter a valid figure.`;
    }
    if (marginAtFrom < -0.005 || marginAtTo < -0.005)
      return 'This hands out more than your own commission — reduce a rate.';
    return '';
  }, [tiers, settable, from, to, marginAtFrom, marginAtTo]);

  const tierPayload = () => Object.fromEntries(settable.map((t) => [t, {
    Comm: Number(tiers[t]?.value) || 0,
    CommType: toApiType(tiers[t]?.type ?? 'FIX'),
    Category: toApiCat(tiers[t]?.category ?? 'COM'),
  }]));

  const saveMut = useMutation({
    mutationFn: () => apiPost('SlabDelegation/UpdateRow', {
      SlabDetailID: detailId,
      FromAmount: from,
      ToAmount: to,
      Tiers: tierPayload(),
    }),
    onSuccess: (r) => (r.ok ? (toast.success('Commission updated.'), onSaved()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const delMut = useMutation({
    mutationFn: () => apiPost('SlabDelegation/DeleteRow', { SlabDetailID: detailId }),
    onSuccess: (r) => (r.ok ? (toast.success('Band removed.'), onSaved()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const invalid = str(band, 'Status').toUpperCase() === 'INVALID';
  const dirty = from !== num(band, 'FromAmount') || to !== num(band, 'ToAmount')
    || settable.some((t) => {
      const saved = tierFrom(band, t);
      const now = tiers[t];
      return !now || now.value !== saved.value || now.category !== saved.category || now.type !== saved.type;
    });

  return (
    <tr className={cn('border-b border-slate-100 last:border-0', invalid && 'bg-danger-50/40')}>
      <td className="whitespace-nowrap px-3 py-3 align-top text-sm">
        <p className="font-medium text-slate-800">{str(band, 'OperatorName') || '—'}</p>
        <p className="text-xs text-slate-500">{str(band, 'ServiceName')}</p>
        {invalid && (
          <p className="mt-1 max-w-[16rem] text-xs font-medium text-danger-600">
            {str(band, 'StatusReason') || 'Outside your parent slab limits.'}
          </p>
        )}
      </td>

      {/* The band itself — a reseller may split one of its parent's ranges into finer ones. */}
      <td className="px-3 py-3 align-top">
        <div className="flex items-center gap-1">
          <input
            type="number" step="1" min="0" aria-label="From amount"
            value={fromAmount}
            onChange={(e) => setFromAmount(e.target.value)}
            className="h-8 w-20 rounded-lg border border-slate-200 px-2 text-sm tabular-nums focus:border-brand-500 focus:outline-none"
          />
          <span className="text-xs text-slate-400">to</span>
          <input
            type="number" step="1" min="0" aria-label="To amount"
            value={toAmount}
            onChange={(e) => setToAmount(e.target.value)}
            className="h-8 w-20 rounded-lg border border-slate-200 px-2 text-sm tabular-nums focus:border-brand-500 focus:outline-none"
          />
        </div>
      </td>

      {/*
        What the card above pays the OWNER for this band — the whole of what they have to divide,
        and the figure every other number on the row is decided against. It was a caption under
        the amount inputs, which was too easy to miss on a card with two dozen bands.
      */}
      <td className="whitespace-nowrap bg-slate-50/60 px-3 py-3 align-top">
        <p className="text-sm font-semibold tabular-nums text-slate-800">{allowanceLabel}</p>
        <p className="text-[11px] text-slate-500">
          {allowanceSpread ? 'across this band' : 'per transaction'}
        </p>
      </td>

      {settable.map((tier) => {
        const t = tiers[tier] ?? emptyTier();
        return (
          <td key={tier} className="px-3 py-3 align-top">
            <TierEditor tier={tier} value={t} onChange={(next) => setTiers((p) => ({ ...p, [tier]: next }))} />
          </td>
        );
      })}

      {/* Not an input. Whatever the parent's allowance leaves after the rates above. */}
      {ownTier !== '' && (
        <td className="bg-brand-50/50 px-3 py-3 align-top">
          <p className={cn('text-sm font-semibold tabular-nums',
            marginAtTo < -0.005 ? 'text-danger-700' : 'text-brand-700')}>
            {inr(marginAtTo)}
          </p>
          <p className="text-[11px] text-slate-500">
            {marginSpread ? `${inr(marginAtFrom)} at ${inr(from)}` : 'you keep'}
          </p>
        </td>
      )}

      <td className="px-3 py-3 align-top">
        <div className="flex flex-col items-start gap-1">
          <div className="flex items-center gap-1">
            <Button size="sm" variant="ghost" disabled={!dirty || error !== ''}
              loading={saveMut.isPending} onClick={() => saveMut.mutate()}
              title={error !== '' ? error : (dirty ? 'Save' : 'No changes')}>
              <Save className={cn('size-4', !dirty || error !== '' ? 'text-slate-300' : 'text-brand-600')} />
            </Button>
            <Button size="sm" variant="ghost" className="text-danger-600 hover:bg-danger-50"
              loading={delMut.isPending}
              onClick={() => {
                if (window.confirm('Remove this band? These amounts will follow your upline slab instead.'))
                  delMut.mutate();
              }}
              title="Remove band">
              <Trash2 className="size-4" />
            </Button>
          </div>
          {error !== '' && <p className="max-w-[14rem] text-[11px] text-danger-600">{error}</p>}
        </div>
      </td>
    </tr>
  );
}

const OPERATOR_ANY = 'Any operator';

/** A band is identified by its service, its operator and the amount range it covers. */
const bandOperator = (b: Row) => str(b, 'OperatorName') || OPERATOR_ANY;
const bandInvalid = (b: Row) => str(b, 'Status').toUpperCase() === 'INVALID';

/**
 * The band table for one card — read-only for the parent view, editable for the owner's own.
 *
 * A real card carries one band per operator per amount step: two dozen on a small install, several
 * hundred on a busy one. Finding the row you came to change is therefore the actual work, so the
 * filters narrow by the three things a band is identified BY — service, operator, amount — plus
 * the one thing that needs attention. They apply to the read-only parent card as well, because
 * "what was I given for this operator" and "what do I pass on for it" is the same hunt from two
 * sides, and the two tables have to be readable against each other.
 *
 * The amount filter is a RANGE OVERLAP rather than an equality: typing 500 finds the band that
 * covers ₹500, which is the question someone actually has, instead of only a band that happens to
 * start there.
 */
function BandTable({
  bands, settable, ownTier, editable, onSaved,
}: {
  bands: Row[];
  settable: string[];
  ownTier: string;
  editable: boolean;
  onSaved: () => void;
}) {
  const [service, setService] = useState('');
  const [operator, setOperator] = useState('');
  const [fromFilter, setFromFilter] = useState('');
  const [toFilter, setToFilter] = useState('');
  const [status, setStatus] = useState<'ALL' | 'ACTIVE' | 'INVALID'>('ALL');

  const services = useMemo(
    () => Array.from(new Set(bands.map((b) => str(b, 'ServiceName')).filter(Boolean))).sort(),
    [bands],
  );

  // Operators are narrowed by the chosen service, so the list only ever offers pairs that exist.
  const operators = useMemo(
    () => Array.from(new Set(bands
      .filter((b) => service === '' || str(b, 'ServiceName') === service)
      .map(bandOperator))).sort(),
    [bands, service],
  );

  const invalidTotal = useMemo(() => bands.filter(bandInvalid).length, [bands]);

  const filtered = useMemo(() => {
    const lo = fromFilter.trim() === '' ? null : Number(fromFilter);
    const hi = toFilter.trim() === '' ? null : Number(toFilter);
    return bands.filter((b) => {
      if (service !== '' && str(b, 'ServiceName') !== service) return false;
      if (operator !== '' && bandOperator(b) !== operator) return false;
      if (lo !== null && Number.isFinite(lo) && num(b, 'ToAmount') < lo) return false;
      if (hi !== null && Number.isFinite(hi) && num(b, 'FromAmount') > hi) return false;
      if (status === 'ACTIVE' && bandInvalid(b)) return false;
      if (status === 'INVALID' && !bandInvalid(b)) return false;
      return true;
    }).sort((x, y) =>
      str(x, 'ServiceName').localeCompare(str(y, 'ServiceName'))
      || bandOperator(x).localeCompare(bandOperator(y))
      || num(x, 'FromAmount') - num(y, 'FromAmount'));
  }, [bands, service, operator, fromFilter, toFilter, status]);

  const filtering = service !== '' || operator !== '' || fromFilter !== '' || toFilter !== '' || status !== 'ALL';
  const clearFilters = () => {
    setService(''); setOperator(''); setFromFilter(''); setToFilter(''); setStatus('ALL');
  };

  // A card you cannot edit shows every tier: there is nothing to withhold in a rate card that is
  // already yours to read, and the whole split is the point of looking at it.
  const columns = editable ? settable : ALL_TIERS;

  const filterBar = (
    <div className="flex flex-wrap items-end gap-3 border-b border-slate-200 bg-slate-50/50 px-4 py-3">
      {/*
        Select and Input both render inside a w-full wrapper, so a width class passed to them sizes
        the control and leaves the wrapper filling the row — which stacks every field on its own
        line in a flex row. The width belongs on a wrapper here.
      */}
      <div className="w-44">
        <Select
          label="Service" value={service}
          onChange={(e) => { setService(e.target.value); setOperator(''); }}
        >
          <option value="">All services</option>
          {services.map((s) => <option key={s} value={s}>{s}</option>)}
        </Select>
      </div>

      <div className="w-52">
        <Select label="Operator" value={operator} onChange={(e) => setOperator(e.target.value)}>
          <option value="">All operators</option>
          {operators.map((o) => <option key={o} value={o}>{o}</option>)}
        </Select>
      </div>

      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700">Covers amount</label>
        <div className="flex items-center gap-1.5">
          <input
            type="number" min="0" placeholder="from" aria-label="Filter from amount"
            value={fromFilter} onChange={(e) => setFromFilter(e.target.value)}
            className="h-12 w-24 rounded-xl border border-slate-300 bg-white px-3 text-[15px] tabular-nums shadow-sm focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30 md:h-10 md:rounded-lg md:text-sm"
          />
          <span className="text-sm text-slate-400">to</span>
          <input
            type="number" min="0" placeholder="to" aria-label="Filter to amount"
            value={toFilter} onChange={(e) => setToFilter(e.target.value)}
            className="h-12 w-24 rounded-xl border border-slate-300 bg-white px-3 text-[15px] tabular-nums shadow-sm focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30 md:h-10 md:rounded-lg md:text-sm"
          />
        </div>
      </div>

      {/* Only worth offering when there is something to find. */}
      {invalidTotal > 0 && (
        <div className="w-48">
          <Select label="Status" value={status}
            onChange={(e) => setStatus(e.target.value as 'ALL' | 'ACTIVE' | 'INVALID')}>
            <option value="ALL">All bands</option>
            <option value="ACTIVE">Working</option>
            <option value="INVALID">Needs attention ({invalidTotal})</option>
          </Select>
        </div>
      )}

      <div className="ml-auto flex items-center gap-3 pb-1">
        <span className="text-xs text-slate-500">
          {filtering
            ? `${filtered.length} of ${bands.length} band${bands.length === 1 ? '' : 's'}`
            : `${bands.length} band${bands.length === 1 ? '' : 's'}`}
        </span>
        {filtering && (
          <Button size="sm" variant="ghost" onClick={clearFilters}>
            <X className="size-4" />
            Clear
          </Button>
        )}
      </div>
    </div>
  );

  if (bands.length === 0) {
    return (
      <EmptyState
        icon={Layers}
        title="No commission bands"
        description={editable
          ? 'Add a band below to start pricing, or ask your parent to set their rates up first.'
          : 'This slab has no rate rows yet.'}
      />
    );
  }

  return (
    <div>
      {filterBar}

      {filtered.length === 0 ? (
        <EmptyState
          icon={Layers}
          title="No band matches these filters"
          description="Widen the amount range, or clear the filters to see the whole card."
        />
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full min-w-[58rem] border-collapse text-left">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/70 text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-3 py-2">Service / operator</th>
                <th className="px-3 py-2">Amount band</th>
                {editable && (
                  <th className="bg-slate-100/70 px-3 py-2">
                    You get
                    <span className="ml-1 font-normal normal-case">(your commission)</span>
                  </th>
                )}
                {columns.map((tier) => (
                  <th key={tier} className="px-3 py-2">
                    {TIER_LABEL[tier] ?? tier}
                    {editable && <span className="ml-1 font-normal normal-case">(you give)</span>}
                  </th>
                ))}
                {editable && ownTier !== '' && (
                  <th className="px-3 py-2 text-brand-700">
                    {TIER_LABEL[ownTier] ?? ownTier}
                    <span className="ml-1 font-normal normal-case">(you keep)</span>
                  </th>
                )}
                {editable && <th className="px-3 py-2" />}
              </tr>
            </thead>
            <tbody>
              {filtered.map((b, i) => {
                // One heading per service/operator, so the operator's name is read once above its
                // amount bands instead of being repeated down every row of the card.
                const groupKey = `${str(b, 'ServiceName')}|${bandOperator(b)}`;
                const prevKey = i > 0 ? `${str(filtered[i - 1], 'ServiceName')}|${bandOperator(filtered[i - 1])}` : '';
                const heading = groupKey !== prevKey ? (
                  <tr key={`g-${groupKey}`} className="border-b border-slate-200 bg-slate-50/80">
                    <td colSpan={2 + (editable ? 1 : 0) + columns.length + (editable && ownTier !== '' ? 1 : 0) + (editable ? 1 : 0)} className="px-3 py-1.5 text-xs font-semibold text-slate-700">
                      {bandOperator(b)}
                      <span className="ml-2 font-normal text-slate-400">{str(b, 'ServiceName')}</span>
                    </td>
                  </tr>
                ) : null;
                return [heading,
                editable ? (
                  <BandRow key={num(b, 'SlabDetailID')} band={b} settable={settable} ownTier={ownTier} onSaved={onSaved} />
                ) : (
                  <tr key={num(b, 'SlabDetailID')} className="border-b border-slate-100 last:border-0">
                    <td className="whitespace-nowrap px-3 py-3 text-sm">
                      <p className="font-medium text-slate-800">{str(b, 'OperatorName') || '—'}</p>
                      <p className="text-xs text-slate-500">{str(b, 'ServiceName')}</p>
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 text-sm tabular-nums text-slate-500">
                      {inr(num(b, 'FromAmount'))} – {inr(num(b, 'ToAmount'))}
                    </td>
                    {columns.map((tier) => (
                      <td key={tier} className="px-3 py-3 text-sm tabular-nums text-slate-700">
                        {moneyLabel(num(b, `${tier}Comm`), str(b, `${tier}CommType`), str(b, `${tier}CommissionCategory`))}
                      </td>
                    ))}
                  </tr>
                )];
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

/**
 * Add a band to the owner's own card. The service and operator come from the bands already on the
 * card — those are the pairs the parent actually prices, and offering any other would only
 * produce a refusal.
 */
function AddBandForm({
  slabId, bands, settable, onSaved,
}: {
  slabId: number;
  bands: Row[];
  settable: string[];
  onSaved: () => void;
}) {
  const toast = useToast();
  const [pairKey, setPairKey] = useState('');
  const [fromAmount, setFromAmount] = useState('');
  const [toAmount, setToAmount] = useState('');
  const [tiers, setTiers] = useState<Record<string, TierInput>>(() =>
    Object.fromEntries(settable.map((t) => [t, emptyTier()])));

  const pairs = useMemo(() => {
    const seen = new Map<string, { label: string; serviceId: number; operatorId: number }>();
    for (const b of bands) {
      const key = `${num(b, 'ServiceID')}:${num(b, 'OperatorID')}`;
      if (!seen.has(key)) {
        seen.set(key, {
          label: `${str(b, 'ServiceName')} - ${str(b, 'OperatorName') || 'Any operator'}`,
          serviceId: num(b, 'ServiceID'),
          operatorId: num(b, 'OperatorID'),
        });
      }
    }
    return [...seen.entries()].map(([key, v]) => ({ key, ...v }));
  }, [bands]);

  const chosen = pairs.find((p) => p.key === pairKey);

  // Where the next band can legally start: just past the highest one already on this pair.
  const nextFrom = useMemo(() => {
    if (!chosen) return 1;
    const mine = bands.filter((b) =>
      num(b, 'ServiceID') === chosen.serviceId && num(b, 'OperatorID') === chosen.operatorId);
    return mine.length ? Math.max(...mine.map((b) => num(b, 'ToAmount'))) + 1 : 1;
  }, [chosen, bands]);

  const addMut = useMutation({
    mutationFn: () => apiPost('SlabDelegation/CreateRow', {
      SlabMasterID: slabId,
      ServiceID: chosen?.serviceId ?? 0,
      OperatorID: chosen?.operatorId ?? 0,
      FromAmount: Number(fromAmount) || 0,
      ToAmount: Number(toAmount) || 0,
      Tiers: Object.fromEntries(settable.map((t) => [t, {
        Comm: Number(tiers[t]?.value) || 0,
        CommType: toApiType(tiers[t]?.type ?? 'FIX'),
        Category: toApiCat(tiers[t]?.category ?? 'COM'),
      }])),
    }),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success('Band added.');
      setFromAmount('');
      setToAmount('');
      onSaved();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const ready = chosen !== undefined && fromAmount.trim() !== '' && toAmount.trim() !== ''
    && Number(toAmount) >= Number(fromAmount);

  return (
    <div className="border-t border-slate-100 p-5">
      <h3 className="mb-1 font-display text-sm font-bold text-slate-900">Add a band</h3>
      <p className="mb-4 text-sm text-slate-500">
        Split one of your parent ranges into a narrower one of your own. Whatever you do not pass
        on is what you keep on it.
      </p>
      {/* Widths go on wrappers, not on the controls — see the note in the filter bar. */}
      <div className="flex flex-wrap items-end gap-3">
        <div className="w-72">
          <Select label="Service / operator" value={pairKey}
            onChange={(e) => { setPairKey(e.target.value); setFromAmount(''); setToAmount(''); }}>
            <option value="">Select</option>
            {pairs.map((p) => <option key={p.key} value={p.key}>{p.label}</option>)}
          </Select>
        </div>
        <div className="w-28">
          <Input label="From" type="number" min="0"
            placeholder={chosen ? String(nextFrom) : ''}
            value={fromAmount} onChange={(e) => setFromAmount(e.target.value)} />
        </div>
        <div className="w-28">
          <Input label="To" type="number" min="0"
            value={toAmount} onChange={(e) => setToAmount(e.target.value)} />
        </div>
        {settable.map((tier) => {
          const t = tiers[tier] ?? emptyTier();
          return (
            <div key={tier}>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">
                {TIER_LABEL[tier] ?? tier} gets
              </label>
              <TierEditor tier={tier} value={t} compact={false} onChange={(next) => setTiers((p) => ({ ...p, [tier]: next }))} />
            </div>
          );
        })}
        <Button disabled={!ready} loading={addMut.isPending} onClick={() => addMut.mutate()}>
          <Plus className="size-4" />
          Add band
        </Button>
      </div>
    </div>
  );
}

/* The screen */

const DOWNLINE_PAYLOAD = { PageNumber: 1, PageSize: 1000, IsActive: true, TakeCount: 1000, SkipCount: 0, ShowDeleted: false };

export function SlabDelegationScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const { user } = useAuth();
  const myRole = Number(user?.UserTypeID ?? 0);
  const myUserId = Number(user?.UserID ?? 0);
  const isAdminTier = myRole === UserTypeCode.Admin || myRole === UserTypeCode.SupportUser;

  const [openSlabId, setOpenSlabId] = useState(0);
  const [showParent, setShowParent] = useState(false);
  const [newName, setNewName] = useState('');
  const [newForRole, setNewForRole] = useState('');
  const [assignUser, setAssignUser] = useState('');
  const [assignSlab, setAssignSlab] = useState('');

  const parentQ = useQuery({
    queryKey: ['slab-delegation', 'parent'],
    queryFn: () => apiPost<Row>('SlabDelegation/MyParentSlab', {}),
    enabled: !isAdminTier,
  });

  const slabsQ = useQuery({
    queryKey: ['slab-delegation', 'mine'],
    queryFn: async () => {
      const res = await apiPost<unknown>('SlabDelegation/MySlabs', {});
      return res.ok && Array.isArray(res.data) ? (res.data as Row[]) : [];
    },
  });

  const viewQ = useQuery({
    queryKey: ['slab-delegation', 'view', openSlabId],
    queryFn: () => apiPost<Row>('SlabDelegation/SlabView', { SlabMasterID: openSlabId }),
    enabled: openSlabId > 0,
  });

  const assignmentsQ = useQuery({
    queryKey: ['slab-delegation', 'assignments'],
    queryFn: async () => {
      const res = await apiPost<unknown>('SlabDelegation/Assignments', {});
      return res.ok && Array.isArray(res.data) ? (res.data as Row[]) : [];
    },
  });

  // Who this tier may hand a card to. Mirrors SlabDelegationPolicy.AssignableTo; the server
  // re-checks the target's actual role, so this only keeps the picker honest.
  const assignableRoles = useMemo<number[]>(() => {
    if (myRole === UserTypeCode.SuperDistributor) return [UserTypeCode.Distributor, UserTypeCode.Retailer];
    if (myRole === UserTypeCode.Distributor) return [UserTypeCode.Retailer];
    if (isAdminTier) return [UserTypeCode.SuperDistributor, UserTypeCode.Distributor, UserTypeCode.Retailer, UserTypeCode.Client];
    return [];
  }, [myRole, isAdminTier]);

  const downlineQ = useQuery({
    queryKey: ['slab-delegation', 'downline'],
    queryFn: () => fetchList('Users/Select', DOWNLINE_PAYLOAD),
    staleTime: 60_000,
  });

  const assignableUsers = useMemo(
    () => (downlineQ.data ?? []).filter((u) => assignableRoles.includes(num(u, 'UserTypeID'))),
    [downlineQ.data, assignableRoles],
  );

  const refreshAll = () => {
    qc.invalidateQueries({ queryKey: ['slab-delegation'] });
  };

  const createMut = useMutation({
    mutationFn: () =>
      apiPost<Row>('SlabDelegation/CreateSlab', {
        SlabName: newName.trim(),
        ForUserTypeID: Number(newForRole) || 0,
      }),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success('Slab created. Every band starts by passing your whole commission down — change what you want to keep.');
      setNewName('');
      setNewForRole('');
      refreshAll();
      const created = Number(getField((r.data ?? {}) as Row, 'SlabMasterID') ?? 0);
      if (created > 0) setOpenSlabId(created);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const assignMut = useMutation({
    mutationFn: () =>
      apiPost('SlabDelegation/AssignSlab', { SlabMasterID: Number(assignSlab), UserID: Number(assignUser) }),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success('Slab assigned.');
      setAssignUser('');
      setAssignSlab('');
      refreshAll();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const unassignMut = useMutation({
    mutationFn: (id: number) => apiPost('SlabDelegation/UnassignSlab', { UserSlabMappingID: id }),
    onSuccess: (r) => (r.ok ? (toast.success('Assignment removed.'), refreshAll()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const fixMut = useMutation({
    mutationFn: (slabId: number) => apiPost('SlabDelegation/FixToParent', { SlabMasterID: slabId }),
    onSuccess: (r) => (r.ok ? (toast.success(r.message || 'Corrected.'), refreshAll()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteSlabMut = useMutation({
    mutationFn: (slabId: number) => apiPost('SlabDelegation/DeleteSlab', { SlabMasterID: slabId }),
    onSuccess: (r, slabId) => {
      if (!r.ok) return toast.error(r.message);
      toast.success('Slab deleted.');
      // Deleting the card that is open would leave the editor on a card that no longer exists.
      if (openSlabId === slabId) setOpenSlabId(0);
      refreshAll();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  /**
   * Whose card may be removed from here: your own. Admin and support may remove a reseller's card
   * too, since they answer for the network — but never the platform's own ceiling card, which has
   * no owner and is managed from Commission Slabs.
   */
  const canDeleteSlab = (s: Row | null) => {
    if (!s) return false;
    const owner = num(s, 'OwnerUserID');
    if (owner <= 0) return false;
    return isAdminTier || owner === myUserId;
  };

  /**
   * Users on a card are earning on it, so the owner takes those assignments back first — said here
   * with the count rather than as a refusal after the click, because the fix is on this same screen.
   */
  const askDeleteSlab = (s: Row) => {
    const name = str(s, 'SlabName');
    const users = num(s, 'AssignedUserCount');
    if (users > 0) {
      toast.error(
        `“${name}” is assigned to ${users} user${users === 1 ? '' : 's'}. Remove ${users === 1 ? 'that assignment' : 'those assignments'} below, then delete it.`,
      );
      return;
    }
    if (window.confirm(`Delete “${name}”? Its commission bands go with it, and this cannot be undone.`))
      deleteSlabMut.mutate(num(s, 'SlabMasterID'));
  };

  const slabs = slabsQ.data ?? [];
  const parentView = (parentQ.data?.ok ? parentQ.data.data : null) as Row | null;
  const parentSlab = (parentView ? getField(parentView, 'Slab') : null) as Row | null;
  const parentBands = (parentView ? getField(parentView, 'Bands') : null) as Row[] | null;

  const view = (viewQ.data?.ok ? viewQ.data.data : null) as Row | null;
  const viewSlab = (view ? getField(view, 'Slab') : null) as Row | null;
  const viewBands = ((view ? getField(view, 'Bands') : null) as Row[] | null) ?? [];
  const ownTier = view ? str(view, 'OwnTier') : '';
  // Which rate columns this card's owner may set, straight from the API — a distributor gets
  // ["Retailer"], a super-distributor ["Distributor","Retailer"]. The save endpoint reads the same
  // list, so a column this screen does not draw is one the server would refuse to write anyway.
  const settableTiers = ((view ? getField(view, 'SettableTiers') : null) as string[] | null) ?? [];
  const editable = Boolean(view && getField(view, 'IsEditable'));

  /* ── One card, open ─────────────────────────────────────────────────────── */
  if (openSlabId > 0) {
    const invalidCount = viewSlab ? num(viewSlab, 'InvalidBandCount') : 0;
    return (
      <>
        <PageHeader
          title={viewSlab ? str(viewSlab, 'SlabName') : 'Slab'}
          subtitle={
            viewSlab && str(viewSlab, 'ParentSlabName')
              ? `Cut from “${str(viewSlab, 'ParentSlabName')}” · ${num(viewSlab, 'AssignedUserCount')} user(s) on this slab`
              : 'Commission bands'
          }
          icon={Layers}
          actions={
            <div className="flex items-center gap-2">
              {invalidCount > 0 && editable && (
                <Button
                  variant="outline"
                  loading={fixMut.isPending}
                  onClick={() => fixMut.mutate(openSlabId)}
                >
                  <Wrench className="size-4" />
                  Fix to parent limits
                </Button>
              )}
              {canDeleteSlab(viewSlab) && (
                <Button
                  variant="outline"
                  className="text-danger-600 hover:bg-danger-50"
                  loading={deleteSlabMut.isPending}
                  onClick={() => viewSlab && askDeleteSlab(viewSlab)}
                >
                  <Trash2 className="size-4" />
                  Delete slab
                </Button>
              )}
              <Button variant="outline" onClick={() => setOpenSlabId(0)}>
                <ArrowLeft className="size-4" />
                All slabs
              </Button>
            </div>
          }
        />

        {invalidCount > 0 && (
          <div className="mb-5 flex items-start gap-3 rounded-card border border-danger-200 bg-danger-50 p-4">
            <AlertTriangle className="mt-0.5 size-5 shrink-0 text-danger-600" />
            <div className="text-sm text-danger-700">
              <p className="font-semibold">
                {invalidCount} band{invalidCount === 1 ? '' : 's'} hand out more than your own commission now allows.
              </p>
              <p className="mt-0.5">
                Transactions on those bands are refused until they are corrected — every other band keeps working.
                {editable && ' “Fix to parent limits” puts them back to passing your whole commission down, in one step — then re-type the margin you want.'}
              </p>
            </div>
          </div>
        )}

        {!editable && viewSlab && (
          <div className="mb-5 rounded-card border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
            This slab is managed by {str(viewSlab, 'OwnerName') || 'its owner'}. You can read it here; only its owner
            changes its rates.
          </div>
        )}

        {viewQ.isLoading ? (
          <div className="flex justify-center py-16"><Spinner /></div>
        ) : (
          <div className="rounded-card border border-slate-200/70 bg-white shadow-card">
            <BandTable
              bands={viewBands}
              settable={settableTiers}
              ownTier={ownTier}
              editable={editable}
              onSaved={refreshAll}
            />
            {editable && settableTiers.length > 0 && (
              <AddBandForm slabId={openSlabId} bands={viewBands} settable={settableTiers} onSaved={refreshAll} />
            )}
          </div>
        )}
      </>
    );
  }

  /* ── The list ───────────────────────────────────────────────────────────── */
  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={Layers} />

      {/* 1. The ceiling everything below is cut from. */}
      {!isAdminTier && (
        <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h2 className="font-display text-base font-bold text-slate-900">Your slab</h2>
              <p className="mt-0.5 text-sm text-slate-500">
                {parentQ.isLoading
                  ? 'Loading…'
                  : parentSlab
                    ? `“${str(parentSlab, 'SlabName')}” from ${str(parentSlab, 'OwnerName')} — your own commission is the column for your role, and that is what you have to share out.`
                    : parentQ.data?.message || 'No slab is assigned to your account yet.'}
              </p>
            </div>
            {parentSlab && (
              <Button variant="outline" onClick={() => setShowParent((v) => !v)}>
                {showParent ? 'Hide rates' : 'View rates'}
              </Button>
            )}
          </div>
          {showParent && parentBands && (
            <div className="mt-4 border-t border-slate-100 pt-4">
              <BandTable bands={parentBands} settable={[]} ownTier="" editable={false} onSaved={refreshAll} />
            </div>
          )}
        </div>
      )}

      {/* 2. Cut a new card. */}
      {assignableRoles.length > 0 && !isAdminTier && (
        <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
          <h2 className="mb-1 font-display text-base font-bold text-slate-900">New slab</h2>
          <p className="mb-4 text-sm text-slate-500">
            Starts as a copy of your own slab — same services, operators, amount bands and GST — and every band
            begins by passing your whole commission down, so nothing changes until you decide what to keep.
          </p>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
            <Input
              label="Slab name"
              placeholder="Retailer Premium"
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              className="sm:w-72"
            />
            <Select
              label="For"
              value={newForRole}
              onChange={(e) => setNewForRole(e.target.value)}
              className="sm:w-56"
            >
              <option value="">Select role</option>
              {assignableRoles.map((r) => (
                <option key={r} value={String(r)}>{roleLabel(r)}</option>
              ))}
            </Select>
            <Button
              disabled={!newName.trim() || !newForRole || !parentSlab}
              loading={createMut.isPending}
              onClick={() => createMut.mutate()}
            >
              <Plus className="size-4" />
              Create slab
            </Button>
          </div>
        </div>
      )}

      {/* 3. The cards. */}
      <div className="mb-6 rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
          <h2 className="font-display text-base font-bold text-slate-900">
            {isAdminTier ? 'All commission slabs' : 'Your slabs'}
          </h2>
          <span className="text-sm text-slate-500">{slabs.length} slab(s)</span>
        </div>
        {slabsQ.isLoading ? (
          <div className="flex justify-center py-12"><Spinner /></div>
        ) : slabs.length === 0 ? (
          <EmptyState
            icon={Layers}
            title="No slabs yet"
            description="Create one from your own slab above, then assign it to the users in your network."
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[46rem] border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/70 text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <th className="px-5 py-2">Slab</th>
                  <th className="px-3 py-2">Owner</th>
                  <th className="px-3 py-2">Cut from</th>
                  <th className="px-3 py-2">Bands</th>
                  <th className="px-3 py-2">Users</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2" />
                </tr>
              </thead>
              <tbody>
                {slabs.map((s) => {
                  const invalid = num(s, 'InvalidBandCount');
                  return (
                    <tr key={num(s, 'SlabMasterID')} className="border-b border-slate-100 last:border-0">
                      <td className="px-5 py-3 text-sm font-medium text-slate-800">{str(s, 'SlabName')}</td>
                      <td className="px-3 py-3 text-sm text-slate-600">{str(s, 'OwnerName')}</td>
                      <td className="px-3 py-3 text-sm text-slate-500">{str(s, 'ParentSlabName') || '—'}</td>
                      <td className="px-3 py-3 text-sm tabular-nums text-slate-600">{num(s, 'BandCount')}</td>
                      <td className="px-3 py-3 text-sm tabular-nums text-slate-600">{num(s, 'AssignedUserCount')}</td>
                      <td className="px-3 py-3">
                        {invalid > 0 ? (
                          <Badge tone="danger">
                            <AlertTriangle className="size-3" />
                            {invalid} band{invalid === 1 ? '' : 's'} over limit
                          </Badge>
                        ) : (
                          <Badge tone="success">
                            <CheckCircle2 className="size-3" />
                            Active
                          </Badge>
                        )}
                      </td>
                      <td className="px-3 py-3">
                        <div className="flex items-center justify-end gap-1">
                          <Button size="sm" variant="outline" onClick={() => setOpenSlabId(num(s, 'SlabMasterID'))}>
                            Open
                          </Button>
                          {canDeleteSlab(s) && (
                            <Button
                              size="sm"
                              variant="ghost"
                              className="text-danger-600 hover:bg-danger-50"
                              loading={deleteSlabMut.isPending && deleteSlabMut.variables === num(s, 'SlabMasterID')}
                              onClick={() => askDeleteSlab(s)}
                              title="Delete this slab"
                            >
                              <Trash2 className="size-4" />
                            </Button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* 4. Who holds which card. */}
      <div className="rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="border-b border-slate-100 px-5 py-4">
          <h2 className="font-display text-base font-bold text-slate-900">Assigned users</h2>
          <p className="mt-0.5 text-sm text-slate-500">
            A user without a slab of their own earns on their upline’s. Assigning one replaces whatever they had.
          </p>
        </div>

        <div className="flex flex-col gap-3 border-b border-slate-100 p-5 sm:flex-row sm:items-end">
          <div className="sm:w-80">
            <Autocomplete
              label="User"
              placeholder="Search your network…"
              value={assignUser}
              onChange={(v) => setAssignUser(String(v))}
              options={assignableUsers.map((u) => ({
                value: String(num(u, 'UserMasterID')),
                label: `${str(u, 'FirmName') || str(u, 'PersonName') || `User ${num(u, 'UserMasterID')}`} [${str(u, 'MobileNumber')}] · ${roleLabel(num(u, 'UserTypeID'))}`,
              }))}
            />
          </div>
          <Select
            label="Slab"
            value={assignSlab}
            onChange={(e) => setAssignSlab(e.target.value)}
            className="sm:w-64"
          >
            <option value="">Select slab</option>
            {slabs
              .filter((s) => num(s, 'InvalidBandCount') === 0)
              .map((s) => (
                <option key={num(s, 'SlabMasterID')} value={String(num(s, 'SlabMasterID'))}>
                  {str(s, 'SlabName')}
                </option>
              ))}
          </Select>
          <Button
            disabled={!assignUser || !assignSlab}
            loading={assignMut.isPending}
            onClick={() => assignMut.mutate()}
          >
            <UserPlus className="size-4" />
            Assign
          </Button>
        </div>

        {assignmentsQ.isLoading ? (
          <div className="flex justify-center py-12"><Spinner /></div>
        ) : (assignmentsQ.data ?? []).length === 0 ? (
          <EmptyState
            icon={Users}
            title="Nobody on your slabs yet"
            description="Assign one of your slabs to a user in your network and they start earning on it immediately."
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[40rem] border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/70 text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <th className="px-5 py-2">User</th>
                  <th className="px-3 py-2">Role</th>
                  <th className="px-3 py-2">Slab</th>
                  <th className="px-3 py-2" />
                </tr>
              </thead>
              <tbody>
                {(assignmentsQ.data ?? []).map((a) => (
                  <tr key={num(a, 'UserSlabMappingID')} className="border-b border-slate-100 last:border-0">
                    <td className="px-5 py-3 text-sm">
                      <p className="font-medium text-slate-800">{str(a, 'UserName')}</p>
                      <p className="text-xs text-slate-500">
                        {str(a, 'UserCode')} {str(a, 'MobileNumber') && `· ${str(a, 'MobileNumber')}`}
                      </p>
                    </td>
                    <td className="px-3 py-3 text-sm text-slate-600">{str(a, 'UserType')}</td>
                    <td className="px-3 py-3 text-sm text-slate-600">{str(a, 'SlabName')}</td>
                    <td className="px-3 py-3">
                      <Button
                        size="sm"
                        variant="ghost"
                        className="text-danger-600 hover:bg-danger-50"
                        loading={unassignMut.isPending}
                        onClick={() => {
                          if (window.confirm(`Remove ${str(a, 'UserName')} from “${str(a, 'SlabName')}”? They will earn on their upline's slab instead.`))
                            unassignMut.mutate(num(a, 'UserSlabMappingID'));
                        }}
                        title="Remove"
                      >
                        <X className="size-4" />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </>
  );
}
