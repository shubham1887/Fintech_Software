import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Calculator, Info, Percent, Plus, Trash2 } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { inr, apiDate, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';

interface RuleRow {
  TdsRuleID: number;
  Scope: string; ScopeLabel: string;
  UserID: number; UserName: string;
  UserTypeID: number; UserTypeName: string;
  ServiceID: number; ServiceName: string;
  OperatorID: number; OperatorName: string;
  ChargeType: string; ChargeValue: number; ApplyOn: string;
  MinCommission: number; MaxTDSPerTransaction: number;
  SectionCode: string; ApplyNoPANRate: boolean;
  EffectiveFrom: string; EffectiveTo: string | null;
  Priority: number; Remark: string; IsActive: boolean;
  Summary: string;
}

interface SectionOption {
  SectionCode: string; Rate: number; NoPANRate: number; ThresholdAnnual: number;
}

interface SettingView {
  MasterSwitchOn: boolean;
  Rules: RuleRow[];
  Sections: SectionOption[];
  Note: string;
}

interface Preview {
  MasterSwitchOn: boolean; RuleFound: boolean;
  TdsRuleID: number; Scope: string; DecidedBy: string;
  CommissionAsQuoted: number; GSTInCommission: number; Base: number;
  ChargeType: string; RatePercent: number; TDSAmount: number; NetToParty: number;
  SectionCode: string; NoPANRateApplied: boolean;
  Steps: string[]; Warning: string;
}

/**
 * The six scopes, in the words the brief used. The order is the order they are matched in, which
 * is also the order somebody thinks in: start from the whole platform, carve out a service, carve
 * out a role, carve out one person.
 */
const SCOPES = [
  { value: 'GLOBAL', label: 'House default', hint: 'Everyone, every service — the fallback' },
  { value: 'SERVICE', label: 'One service', hint: 'Everyone who earns on this service' },
  { value: 'USERTYPE', label: 'One role', hint: 'Every retailer / distributor / …, all services' },
  { value: 'USERTYPE_SERVICE', label: 'One role on one service', hint: 'Every user of that role, on that service only' },
  { value: 'USER', label: 'One user', hint: 'This person, on every service' },
  { value: 'USER_SERVICE', label: 'One user on one service', hint: 'The narrowest rule there is' },
] as const;

const needsUser = (s: string) => s === 'USER' || s === 'USER_SERVICE';
const needsRole = (s: string) => s === 'USERTYPE' || s === 'USERTYPE_SERVICE';
const needsService = (s: string) => s === 'SERVICE' || s === 'USERTYPE_SERVICE' || s === 'USER_SERVICE';

interface Draft {
  TdsRuleID: number;
  Scope: string;
  UserID: string; UserTypeID: string; ServiceID: string; OperatorID: string;
  ChargeType: string; ChargeValue: string; ApplyOn: string;
  MinCommission: string; MaxTDSPerTransaction: string;
  SectionCode: string; ApplyNoPANRate: boolean;
  EffectiveFrom: string; EffectiveTo: string;
  Priority: string; Remark: string; IsActive: boolean;
}

const emptyDraft = (): Draft => ({
  TdsRuleID: 0,
  Scope: 'GLOBAL',
  UserID: '', UserTypeID: '', ServiceID: '', OperatorID: '',
  ChargeType: 'PERCENT', ChargeValue: '', ApplyOn: 'COMMISSION_NET',
  MinCommission: '', MaxTDSPerTransaction: '',
  SectionCode: '', ApplyNoPANRate: true,
  EffectiveFrom: apiDate(new Date()), EffectiveTo: '',
  Priority: '', IsActive: true, Remark: '',
});

const toDraft = (r: RuleRow): Draft => ({
  TdsRuleID: r.TdsRuleID,
  Scope: r.Scope,
  UserID: r.UserID ? String(r.UserID) : '',
  UserTypeID: r.UserTypeID ? String(r.UserTypeID) : '',
  ServiceID: r.ServiceID ? String(r.ServiceID) : '',
  OperatorID: r.OperatorID ? String(r.OperatorID) : '',
  ChargeType: r.ChargeType || 'PERCENT',
  ChargeValue: String(r.ChargeValue ?? ''),
  ApplyOn: r.ApplyOn || 'COMMISSION_NET',
  MinCommission: r.MinCommission ? String(r.MinCommission) : '',
  MaxTDSPerTransaction: r.MaxTDSPerTransaction ? String(r.MaxTDSPerTransaction) : '',
  SectionCode: r.SectionCode === '194H' ? '' : r.SectionCode || '',
  ApplyNoPANRate: r.ApplyNoPANRate,
  EffectiveFrom: dateOnly(r.EffectiveFrom) || apiDate(new Date()),
  EffectiveTo: r.EffectiveTo ? dateOnly(r.EffectiveTo) : '',
  Priority: r.Priority ? String(r.Priority) : '',
  Remark: r.Remark ?? '',
  IsActive: r.IsActive,
});

/**
 * TDS Setting — how much tax is withheld from a commission, and whose.
 *
 * Before this, TDS was one boolean on the commission tax profile at the section's statutory rate.
 * The desk's actual instruction — "0.01% on this service for every retailer, except this
 * distributor on a flat ₹2" — could not be written down anywhere, so it lived in nobody's hands.
 *
 * Two things on this screen are deliberate. The master switch is shown but NOT editable here: it
 * is a Key Setting, because it belongs with the other platform-wide switches and because somebody
 * turning TDS on should do it from the place where features are turned on, having read what it
 * does. And "Try a figure" is given the same prominence as the rules themselves, because a rate
 * typed as 0.01 meaning 1% is the one mistake here that costs real money on every transaction
 * until somebody notices.
 */
export function TdsSettingScreen() {
  const toast = useToast();
  const qc = useQueryClient();

  const key = ['tds-setting'];
  const { data, isFetching } = useQuery<SettingView>({
    queryKey: key,
    queryFn: async () => {
      const res = await apiPost<SettingView>('TdsSetting/Select', {});
      if (!res.ok) throw new Error(res.message || 'Could not load the TDS rules.');
      return res.data as SettingView;
    },
  });
  const refresh = () => qc.invalidateQueries({ queryKey: key });

  const services = useQuery({
    queryKey: ['fk', 'Service/GetAll'],
    queryFn: () => fetchList('Service/GetAll', { IsActive: true, PageNumber: 1, PageSize: 500 }),
    staleTime: 60_000,
  });
  const userTypes = useQuery({
    queryKey: ['fk', 'UserType/GetAll'],
    queryFn: () => fetchList('UserType/GetAll', { IsActive: true, PageNumber: 1, PageSize: 100 }),
    staleTime: 60_000,
  });
  const users = useQuery({
    queryKey: ['fk', 'Users/GetAll', 'tds'],
    queryFn: () => fetchList('Users/GetAll', { PageNumber: 1, PageSize: 1000 }),
    staleTime: 60_000,
  });

  const serviceOptions = useMemo(
    () => (services.data ?? []).map((s) => ({
      value: String(getField(s, 'ServiceID')), label: String(getField(s, 'Name') ?? ''),
    })),
    [services.data],
  );
  const roleOptions = useMemo(
    () => (userTypes.data ?? []).map((t) => ({
      value: String(getField(t, 'UserTypeID')), label: String(getField(t, 'UserType') ?? ''),
    })),
    [userTypes.data],
  );
  const userOptions = useMemo(
    () => (users.data ?? []).map((u) => {
      const name = String(getField(u, 'FirmName') || getField(u, 'PersonName') || '');
      const code = String(getField(u, 'AliasName') ?? '');
      return { value: String(getField(u, 'UserMasterID')), label: code ? `${name} (${code})` : name };
    }),
    [users.data],
  );

  const [draft, setDraft] = useState<Draft | null>(null);
  const patch = (p: Partial<Draft>) => setDraft((d) => (d ? { ...d, ...p } : d));

  const saveMut = useMutation({
    mutationFn: async (d: Draft) => {
      const res = await apiPost('TdsSetting/Save', {
        TdsRuleID: d.TdsRuleID,
        Scope: d.Scope,
        UserID: Number(d.UserID) || 0,
        UserTypeID: Number(d.UserTypeID) || 0,
        ServiceID: Number(d.ServiceID) || 0,
        OperatorID: Number(d.OperatorID) || 0,
        ChargeType: d.ChargeType,
        ChargeValue: Number(d.ChargeValue) || 0,
        ApplyOn: d.ApplyOn,
        MinCommission: Number(d.MinCommission) || 0,
        MaxTDSPerTransaction: Number(d.MaxTDSPerTransaction) || 0,
        SectionCode: d.SectionCode,
        ApplyNoPANRate: d.ApplyNoPANRate,
        EffectiveFrom: d.EffectiveFrom || null,
        EffectiveTo: d.EffectiveTo || null,
        Priority: Number(d.Priority) || 0,
        Remark: d.Remark,
        IsActive: d.IsActive,
      });
      if (!res.ok) throw new Error(res.message || 'Could not save this rule.');
    },
    onSuccess: () => { toast.success('TDS rule saved.'); setDraft(null); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: async (id: number) => {
      const res = await apiPost('TdsSetting/Delete', { TdsRuleID: id });
      if (!res.ok) throw new Error(res.message || 'Could not withdraw this rule.');
    },
    onSuccess: () => { toast.success('Rule withdrawn.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Try a figure ──────────────────────────────────────────────────────────
  const [tryUser, setTryUser] = useState('');
  const [tryService, setTryService] = useState('');
  const [tryCommission, setTryCommission] = useState('');
  const [preview, setPreview] = useState<Preview | null>(null);

  const previewMut = useMutation({
    mutationFn: async () => {
      const res = await apiPost<Preview>('TdsSetting/Preview', {
        UserID: Number(tryUser) || 0,
        ServiceID: Number(tryService) || 0,
        Commission: Number(tryCommission) || 0,
      });
      if (!res.ok) throw new Error(res.message || 'Could not work that figure out.');
      return res.data as Preview;
    },
    onSuccess: (p) => setPreview(p),
    onError: (e: Error) => { setPreview(null); toast.error(e.message); },
  });

  const rules = data?.Rules ?? [];
  const masterOn = data?.MasterSwitchOn ?? false;

  const draftProblem = useMemo(() => {
    if (!draft) return '';
    if (needsUser(draft.Scope) && !draft.UserID) return 'Pick the user this rule is for.';
    if (needsRole(draft.Scope) && !draft.UserTypeID) return 'Pick the role this rule is for.';
    if (needsService(draft.Scope) && !draft.ServiceID) return 'Pick the service this rule is for.';
    const v = Number(draft.ChargeValue);
    if (!draft.ChargeValue || Number.isNaN(v) || v <= 0) return 'Enter what to deduct.';
    if (draft.ChargeType === 'PERCENT' && v > 100) return 'A percentage over 100 is a typo — 0.01 means 0.01%.';
    return '';
  }, [draft]);

  return (
    <>
      <PageHeader
        title="TDS Setting"
        subtitle="How much tax is withheld from a commission, and whose — per user, per role, per service, or for the whole platform"
      />

      {/* The master switch. Shown, explained, and not editable here on purpose. */}
      <Card className={cn('mb-4', masterOn ? 'border-success-200' : 'border-warning-200')}>
        <CardBody className="flex flex-wrap items-center gap-3">
          <Badge tone={masterOn ? 'success' : 'warning'}>{masterOn ? 'TDS is ON' : 'TDS is OFF'}</Badge>
          <p className="min-w-0 flex-1 text-sm text-slate-600">
            {data?.Note ?? 'Loading…'}
          </p>
          {!masterOn && (
            <span className="text-xs text-slate-500">
              Turn it on under <span className="font-medium text-slate-700">Setting → Key Setting → Deduct TDS from commissions</span>.
            </span>
          )}
        </CardBody>
      </Card>

      {/* ── Try a figure ─────────────────────────────────────────────────── */}
      <Card className="mb-6">
        <CardBody>
          <div className="mb-3 flex items-center gap-2">
            <Calculator className="size-4 text-brand-600" />
            <h2 className="font-display text-sm font-bold text-slate-900">Try a figure</h2>
            <span className="text-xs text-slate-400">— changes nothing</span>
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <Select label="User" value={tryUser} onChange={(e) => setTryUser(e.target.value)}>
              <option value="">Pick a user…</option>
              {userOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </Select>
            <Select label="Service" value={tryService} onChange={(e) => setTryService(e.target.value)}>
              <option value="">Any service</option>
              {serviceOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </Select>
            <Input
              label="Commission earned"
              type="number"
              step="0.01"
              placeholder="e.g. 15"
              value={tryCommission}
              onChange={(e) => setTryCommission(e.target.value)}
            />
            <div className="flex items-end">
              <Button
                className="w-full"
                loading={previewMut.isPending}
                disabled={!tryUser || !tryCommission}
                onClick={() => previewMut.mutate()}
              >
                Work it out
              </Button>
            </div>
          </div>

          {preview && (
            <div className="mt-4 rounded-card border border-slate-200/70 bg-slate-50/60 p-4">
              <div className="mb-3 flex flex-wrap items-center gap-4 text-sm">
                <span>
                  Commission <span className="font-semibold tabular-nums">{inr(preview.CommissionAsQuoted)}</span>
                </span>
                <span className="text-slate-400">→</span>
                <span>
                  TDS{' '}
                  <span className="font-semibold tabular-nums text-danger-600">{inr(preview.TDSAmount)}</span>
                  {preview.ChargeType === 'FIX'
                    ? <span className="ml-1 text-xs text-slate-500">(flat)</span>
                    : <span className="ml-1 text-xs text-slate-500">({preview.RatePercent}%)</span>}
                </span>
                <span className="text-slate-400">→</span>
                <span>
                  Paid to the party{' '}
                  <span className="font-semibold tabular-nums text-success-700">{inr(preview.NetToParty)}</span>
                </span>
                {preview.SectionCode && <Badge tone="neutral">{preview.SectionCode}</Badge>}
                {preview.NoPANRateApplied && <Badge tone="warning">no-PAN rate</Badge>}
              </div>
              <ol className="space-y-1">
                {preview.Steps.map((s, i) => (
                  <li key={i} className="flex gap-2 text-xs text-slate-600">
                    <span className="shrink-0 text-slate-400">{i + 1}.</span>
                    <span>{s}</span>
                  </li>
                ))}
              </ol>
              {preview.Warning && (
                <p className="mt-3 flex items-start gap-1.5 text-xs font-medium text-warning-700">
                  <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />
                  {preview.Warning}
                </p>
              )}
            </div>
          )}
        </CardBody>
      </Card>

      {/* ── The rules ────────────────────────────────────────────────────── */}
      <div className="mb-2 flex items-center justify-between">
        <h2 className="font-display text-base font-bold text-slate-900">
          Rules {rules.length > 0 && <span className="text-sm font-normal text-slate-400">({rules.length})</span>}
        </h2>
        <Button size="sm" onClick={() => setDraft(emptyDraft())}>
          <Plus className="size-4" /> Add a rule
        </Button>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {isFetching && rules.length === 0 ? (
          <div className="flex justify-center py-12"><Spinner /></div>
        ) : rules.length === 0 ? (
          <EmptyState
            icon={Percent}
            title="No TDS rule yet"
            description="Without a rule nothing is deducted by the rules — the commission tax profile's own TDS switch decides, and it is off everywhere by default. Add the house default first, then carve out whatever differs."
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <th className="px-3 py-3">Applies to</th>
                  <th className="px-3 py-3">Deducts</th>
                  <th className="px-3 py-3">On</th>
                  <th className="px-3 py-3">Section</th>
                  <th className="px-3 py-3">From</th>
                  <th className="px-3 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rules.map((r) => (
                  <tr key={r.TdsRuleID} className={cn('border-b border-slate-100 last:border-0', !r.IsActive && 'bg-slate-50/60')}>
                    <td className="px-3 py-3 align-top">
                      <p className="font-medium text-slate-800">{r.ScopeLabel}</p>
                      <p className="mt-0.5 text-xs text-slate-500">{r.Summary}</p>
                      {r.Remark && <p className="mt-0.5 text-xs italic text-slate-400">{r.Remark}</p>}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 align-top tabular-nums">
                      {r.ChargeType === 'FIX'
                        ? <span className="font-semibold">{inr(r.ChargeValue)}</span>
                        : <span className="font-semibold">{r.ChargeValue}%</span>}
                      <span className="ml-1 text-xs text-slate-400">{r.ChargeType === 'FIX' ? 'flat' : ''}</span>
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 align-top text-xs text-slate-600">
                      {r.ApplyOn === 'COMMISSION_GROSS' ? 'commission as quoted' : 'commission net of GST'}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 align-top">
                      <Badge tone="neutral">{r.SectionCode || '194H'}</Badge>
                      {!r.ApplyNoPANRate && <p className="mt-1 text-xs text-warning-700">no-PAN rate off</p>}
                    </td>
                    <td className="whitespace-nowrap px-3 py-3 align-top text-xs text-slate-500">
                      {dateOnly(r.EffectiveFrom)}
                      {r.EffectiveTo && <> → {dateOnly(r.EffectiveTo)}</>}
                      {!r.IsActive && <p className="mt-1"><Badge tone="warning">inactive</Badge></p>}
                    </td>
                    <td className="px-3 py-3 align-top">
                      <div className="flex items-center justify-end gap-1">
                        <Button size="sm" variant="ghost" onClick={() => setDraft(toDraft(r))}>Edit</Button>
                        <Button
                          size="sm"
                          variant="ghost"
                          className="text-danger-600 hover:bg-danger-50"
                          loading={deleteMut.isPending}
                          onClick={() => {
                            if (window.confirm(`Withdraw the rule for ${r.ScopeLabel}?`)) deleteMut.mutate(r.TdsRuleID);
                          }}
                        >
                          <Trash2 className="size-4" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <p className="mt-3 flex items-start gap-1.5 text-xs text-slate-500">
        <Info className="mt-0.5 size-3.5 shrink-0 text-slate-400" />
        The most specific rule that matches a party wins: one user on one service beats that user
        everywhere, which beats a role on that service, which beats the role, which beats the
        service, which beats the house default. A rate of <span className="font-medium">0.01</span>{' '}
        means <span className="font-medium">0.01%</span>, not 1%. TDS is deducted only on a
        successful transaction, and only from a commission that was actually earned.
      </p>

      {/* ── Add / edit ───────────────────────────────────────────────────── */}
      <Modal
        open={draft !== null}
        onClose={() => setDraft(null)}
        title={draft?.TdsRuleID ? 'Edit TDS rule' : 'Add TDS rule'}
        size="lg"
      >
        {draft && (
          <div className="space-y-4">
            <div>
              <Select label="Applies to" value={draft.Scope} onChange={(e) => patch({ Scope: e.target.value })}>
                {SCOPES.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
              </Select>
              <p className="mt-1 text-xs text-slate-400">
                {SCOPES.find((s) => s.value === draft.Scope)?.hint}
              </p>
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {needsUser(draft.Scope) && (
                <Select label="User" value={draft.UserID} onChange={(e) => patch({ UserID: e.target.value })}>
                  <option value="">Pick a user…</option>
                  {userOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                </Select>
              )}
              {needsRole(draft.Scope) && (
                <Select label="Role" value={draft.UserTypeID} onChange={(e) => patch({ UserTypeID: e.target.value })}>
                  <option value="">Pick a role…</option>
                  {roleOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                </Select>
              )}
              {needsService(draft.Scope) && (
                <Select label="Service" value={draft.ServiceID} onChange={(e) => patch({ ServiceID: e.target.value })}>
                  <option value="">Pick a service…</option>
                  {serviceOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                </Select>
              )}
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Select label="Deduct" value={draft.ChargeType} onChange={(e) => patch({ ChargeType: e.target.value })}>
                <option value="PERCENT">A percentage</option>
                <option value="FIX">A flat amount</option>
              </Select>
              <Input
                label={draft.ChargeType === 'FIX' ? 'Amount per transaction (₹)' : 'Percentage (%)'}
                type="number"
                step="0.0001"
                placeholder={draft.ChargeType === 'FIX' ? 'e.g. 2' : 'e.g. 0.01'}
                value={draft.ChargeValue}
                onChange={(e) => patch({ ChargeValue: e.target.value })}
                hint={draft.ChargeType === 'FIX' ? 'Never more than the commission itself' : '0.01 means 0.01%, not 1%'}
              />
              <div>
                <Select label="Computed on" value={draft.ApplyOn} onChange={(e) => patch({ ApplyOn: e.target.value })}>
                  <option value="COMMISSION_NET">Commission net of GST</option>
                  <option value="COMMISSION_GROSS">Commission as quoted</option>
                </Select>
                <p className="mt-1 text-xs text-slate-400">
                  {draft.ApplyOn === 'COMMISSION_NET' ? 'Correct: TDS is a tax on income' : 'Only where a contract says so'}
                </p>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Input
                label="Skip below a commission of (₹)"
                type="number" step="0.01" placeholder="0 = always deduct"
                value={draft.MinCommission}
                onChange={(e) => patch({ MinCommission: e.target.value })}
              />
              <Input
                label="Cap per transaction (₹)"
                type="number" step="0.01" placeholder="0 = no cap"
                value={draft.MaxTDSPerTransaction}
                onChange={(e) => patch({ MaxTDSPerTransaction: e.target.value })}
              />
              <Select label="Section" value={draft.SectionCode} onChange={(e) => patch({ SectionCode: e.target.value })}>
                <option value="">194H — commission (default)</option>
                {(data?.Sections ?? [])
                  .filter((s) => s.SectionCode !== '194H')
                  .map((s) => (
                    <option key={s.SectionCode} value={s.SectionCode}>{`${s.SectionCode} — ${s.Rate}%`}</option>
                  ))}
              </Select>
            </div>

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <Input
                label="Effective from" type="date"
                value={draft.EffectiveFrom}
                onChange={(e) => patch({ EffectiveFrom: e.target.value })}
              />
              <Input
                label="Effective to (optional)" type="date"
                value={draft.EffectiveTo}
                onChange={(e) => patch({ EffectiveTo: e.target.value })}
              />
              <Input
                label="Priority (optional)" type="number" placeholder="0 = by scope"
                value={draft.Priority}
                onChange={(e) => patch({ Priority: e.target.value })}
                hint="Leave 0 unless two rules of the same scope must be ordered"
              />
            </div>

            <Input
              label="Note (optional)"
              placeholder="Why this rule exists — read by whoever asks in six months"
              value={draft.Remark}
              onChange={(e) => patch({ Remark: e.target.value })}
            />

            <div className="flex flex-wrap items-center gap-4">
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <input
                  type="checkbox"
                  checked={draft.ApplyNoPANRate}
                  onChange={(e) => patch({ ApplyNoPANRate: e.target.checked })}
                  className="size-4 rounded border-slate-300"
                />
                Use the section&apos;s higher rate when the party has no PAN
              </label>
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <input
                  type="checkbox"
                  checked={draft.IsActive}
                  onChange={(e) => patch({ IsActive: e.target.checked })}
                  className="size-4 rounded border-slate-300"
                />
                Active
              </label>
            </div>

            {draftProblem && (
              <p className="flex items-center gap-1.5 text-xs font-medium text-danger-600">
                <AlertTriangle className="size-3.5 shrink-0" />
                {draftProblem}
              </p>
            )}

            <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
              <Button variant="secondary" onClick={() => setDraft(null)}>Cancel</Button>
              <Button
                loading={saveMut.isPending}
                disabled={Boolean(draftProblem)}
                onClick={() => saveMut.mutate(draft)}
              >
                Save rule
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
