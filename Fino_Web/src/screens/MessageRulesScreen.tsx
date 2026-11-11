import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowRight, FlaskConical, MessageSquareText, Pencil, Plus, Trash2 } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Modal } from '@/components/ui/Modal';
import { Skeleton } from '@/components/ui/Skeleton';
import { EmptyState } from '@/components/ui/EmptyState';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

const LIST_PAGE = { PageNumber: 1, PageSize: 500, IsActive: true, TakeCount: 500, SkipCount: 0 };

const MATCH_OPTIONS = [
  { value: 'CONTAINS', label: 'Contains', hint: 'The text appears anywhere in the provider’s message' },
  { value: 'EQUALS', label: 'Is exactly', hint: 'The whole message is exactly this text' },
  { value: 'STARTSWITH', label: 'Starts with', hint: 'The message begins with this text' },
  { value: 'REGEX', label: 'Pattern (regex)', hint: 'For the rare case the other three cannot express' },
];

const CHANNEL_OPTIONS = [
  { value: 'ALL', label: 'Everyone' },
  { value: 'PANEL', label: 'Panel & app' },
  { value: 'API', label: 'API partners' },
];

const MATCH_LABEL = (v: string) => MATCH_OPTIONS.find((o) => o.value === v)?.label ?? v;
const CHANNEL_LABEL = (v: string) => CHANNEL_OPTIONS.find((o) => o.value === v)?.label ?? v;

interface RuleForm {
  ResponseMessageRuleID: number; // 0 = new
  ProviderID: string;
  ServiceID: string;
  MatchType: string;
  Pattern: string;
  Replacement: string;
  Channel: string;
  Sequence: string;
  Note: string;
  IsActive: boolean;
}

const EMPTY_FORM: RuleForm = {
  ResponseMessageRuleID: 0,
  ProviderID: '0',
  ServiceID: '0',
  MatchType: 'CONTAINS',
  Pattern: '',
  Replacement: '',
  Channel: 'ALL',
  Sequence: '10',
  Note: '',
  IsActive: true,
};

interface TestResult {
  Matched?: boolean;
  MatchedRuleID?: number;
  MatchedPattern?: string;
  MatchType?: string;
  Original?: string;
  Result?: string;
  Message?: string;
}

/**
 * Response Messages — what a user is told when a provider answers in its own words.
 *
 * The case this exists for: JaiJui replies "Low balance", meaning OUR float with them has run out.
 * The retailer who reads that believes their own wallet is empty and calls support, and an outside
 * integrator learns something about how the business is funded. A rule maps that text to what the
 * user should actually see ("Server down, try again shortly"). The provider log keeps the real
 * reply, so support still sees the truth.
 *
 * The Test box is the point of the screen: an operator can see exactly what a customer would be
 * shown BEFORE a rule ever touches a live transaction.
 */
export function MessageRulesScreen({ def }: CustomScreenProps) {
  const title = def.title;
  const subtitle = def.subtitle;
  const toast = useToast();
  const qc = useQueryClient();

  const listKey = ['message-rules'];
  const rules = useQuery({ queryKey: listKey, queryFn: () => fetchList('ResponseMessageRule/GetAll', {}) });
  const refresh = () => qc.invalidateQueries({ queryKey: listKey });

  const providers = useQuery({
    queryKey: ['prov-opts'], staleTime: 300_000,
    queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAGE),
  });
  const services = useQuery({
    queryKey: ['svc-opts'], staleTime: 300_000,
    queryFn: () => fetchList('Service/GetAll', LIST_PAGE),
  });

  // ── add / edit ──────────────────────────────────────────────────────────────
  const [formOpen, setFormOpen] = useState(false);
  const [form, setForm] = useState<RuleForm>(EMPTY_FORM);
  const set = (patch: Partial<RuleForm>) => setForm((f) => ({ ...f, ...patch }));

  const openAdd = () => { setForm(EMPTY_FORM); setFormOpen(true); };
  const openEdit = (row: Row) => {
    setForm({
      ResponseMessageRuleID: Number(getField(row, 'ResponseMessageRuleID') ?? 0),
      ProviderID: String(getField(row, 'ProviderID') ?? 0),
      ServiceID: String(getField(row, 'ServiceID') ?? 0),
      MatchType: String(getField(row, 'MatchType') ?? 'CONTAINS'),
      Pattern: String(getField(row, 'Pattern') ?? ''),
      Replacement: String(getField(row, 'Replacement') ?? ''),
      Channel: String(getField(row, 'Channel') ?? 'ALL'),
      Sequence: String(getField(row, 'Sequence') ?? 0),
      Note: String(getField(row, 'Note') ?? ''),
      IsActive: Boolean(getField(row, 'IsActive')),
    });
    setFormOpen(true);
  };

  const save = useMutation({
    mutationFn: async () => {
      const payload = {
        ResponseMessageRuleID: form.ResponseMessageRuleID,
        ProviderID: Number(form.ProviderID) || 0,
        ServiceID: Number(form.ServiceID) || 0,
        MatchType: form.MatchType,
        Pattern: form.Pattern.trim(),
        Replacement: form.Replacement.trim(),
        Channel: form.Channel,
        Sequence: Number(form.Sequence) || 0,
        Note: form.Note.trim(),
        IsActive: form.IsActive,
      };
      const res = await apiPost(form.ResponseMessageRuleID > 0 ? 'ResponseMessageRule/Update' : 'ResponseMessageRule/Create', payload);
      if (!res.ok) throw new Error(res.message || 'Could not save this rule.');
      return res.message;
    },
    onSuccess: (m) => { toast.success(m || 'Saved.'); setFormOpen(false); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const remove = useMutation({
    mutationFn: async (id: number) => {
      const res = await apiPost('ResponseMessageRule/Delete', { ResponseMessageRuleID: id });
      if (!res.ok) throw new Error(res.message || 'Could not remove this rule.');
      return res.message;
    },
    onSuccess: (m) => { toast.success(m || 'Removed.'); refresh(); },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggleActive = useMutation({
    mutationFn: async (row: Row) => {
      const res = await apiPost('ResponseMessageRule/Update', {
        ResponseMessageRuleID: Number(getField(row, 'ResponseMessageRuleID') ?? 0),
        ProviderID: Number(getField(row, 'ProviderID') ?? 0),
        ServiceID: Number(getField(row, 'ServiceID') ?? 0),
        MatchType: String(getField(row, 'MatchType') ?? 'CONTAINS'),
        Pattern: String(getField(row, 'Pattern') ?? ''),
        Replacement: String(getField(row, 'Replacement') ?? ''),
        Channel: String(getField(row, 'Channel') ?? 'ALL'),
        Sequence: Number(getField(row, 'Sequence') ?? 0),
        Note: String(getField(row, 'Note') ?? ''),
        IsActive: !getField(row, 'IsActive'),
      });
      if (!res.ok) throw new Error(res.message || 'Could not update this rule.');
      return res.message;
    },
    onSuccess: () => refresh(),
    onError: (e: Error) => toast.error(e.message),
  });

  // ── test lab ────────────────────────────────────────────────────────────────
  const [sample, setSample] = useState('Low balance');
  const [testProvider, setTestProvider] = useState('0');
  const [testChannel, setTestChannel] = useState('ALL');
  const [result, setResult] = useState<TestResult | null>(null);

  const runTest = useMutation({
    mutationFn: async () => {
      const res = await apiPost<TestResult>('ResponseMessageRule/Test', {
        Message: sample,
        ProviderID: Number(testProvider) || 0,
        ServiceID: 0,
        Channel: testChannel,
      });
      if (!res.ok) throw new Error(res.message || 'Could not run the test.');
      return res.data as TestResult;
    },
    onSuccess: (d) => setResult(d),
    onError: (e: Error) => { setResult(null); toast.error(e.message); },
  });

  const rows = rules.data ?? [];
  const activeCount = useMemo(() => rows.filter((r) => Boolean(getField(r, 'IsActive'))).length, [rows]);

  return (
    <>
      <PageHeader
        title={title}
        subtitle={subtitle ?? 'Rewrite what a provider says before the user ever reads it'}
        actions={<Button onClick={openAdd}><Plus className="size-4" />Add rule</Button>}
      />

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
        {/* Test lab */}
        <Card className="xl:col-span-1">
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><FlaskConical className="size-4 text-brand-600" />Try a message</CardTitle>
            <span className="text-xs text-slate-400">Nothing is sent — this only shows what a user would read</span>
          </CardHeader>
          <CardBody className="space-y-3">
            <label className="block">
              <span className="mb-1 block text-xs font-medium text-slate-500">The provider&apos;s message</span>
              <textarea
                value={sample}
                onChange={(e) => setSample(e.target.value)}
                rows={3}
                placeholder="Paste exactly what came back, e.g. Low balance"
                className="w-full rounded-lg border border-slate-200 p-2.5 text-sm outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20"
              />
            </label>

            <div className="grid grid-cols-2 gap-3">
              <Select label="From provider" value={testProvider} onChange={(e) => setTestProvider(e.target.value)}>
                <option value="0">Any provider</option>
                {(providers.data ?? []).map((p) => (
                  <option key={String(getField(p, 'ProviderID'))} value={String(getField(p, 'ProviderID'))}>
                    {String(getField(p, 'ProviderName') ?? '')}
                  </option>
                ))}
              </Select>
              <Select label="Shown to" value={testChannel} onChange={(e) => setTestChannel(e.target.value)}>
                {CHANNEL_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </Select>
            </div>

            <Button onClick={() => runTest.mutate()} disabled={!sample.trim() || runTest.isPending} className="w-full">
              {runTest.isPending ? 'Checking…' : 'Check this message'}
            </Button>

            {result && (
              <div className={cn(
                'rounded-lg border p-3 text-sm',
                result.Matched ? 'border-success-200 bg-success-50' : 'border-slate-200 bg-slate-50',
              )}>
                <p className={cn('text-xs font-semibold', result.Matched ? 'text-success-700' : 'text-slate-500')}>
                  {result.Message}
                </p>
                <div className="mt-2 space-y-1.5">
                  <p className="text-xs text-slate-400">Provider said</p>
                  <p className="break-words rounded bg-white/70 p-2 text-sm text-slate-600 line-through decoration-slate-300">
                    {result.Original}
                  </p>
                  <p className="flex items-center gap-1 text-xs text-slate-400"><ArrowRight className="size-3" />User sees</p>
                  <p className="break-words rounded bg-white p-2 text-sm font-medium text-slate-800">
                    {result.Result || <span className="italic text-slate-400">(nothing — the message is hidden)</span>}
                  </p>
                </div>
                {result.Matched && (
                  <p className="mt-2 text-[11px] text-slate-400">
                    Rule #{result.MatchedRuleID} · {MATCH_LABEL(String(result.MatchType ?? ''))} “{result.MatchedPattern}”
                  </p>
                )}
              </div>
            )}
          </CardBody>
        </Card>

        {/* Rules */}
        <Card className="xl:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><MessageSquareText className="size-4 text-brand-600" />Rules</CardTitle>
            <span className="text-xs text-slate-400">
              {rows.length === 0 ? 'None yet' : `${activeCount} of ${rows.length} switched on · checked top to bottom, first match wins`}
            </span>
          </CardHeader>
          <CardBody className="p-0">
            {rules.isLoading ? (
              <div className="space-y-2 p-5">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-12 rounded" />)}</div>
            ) : rows.length === 0 ? (
              <div className="p-6">
                <EmptyState
                  icon={MessageSquareText}
                  title="No rules yet"
                  description="Add one to replace a provider's wording — for example map “Low balance” to “Server down, please try again shortly”."
                  action={<Button onClick={openAdd}><Plus className="size-4" />Add rule</Button>}
                />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="border-b border-slate-100 text-left text-xs uppercase tracking-wider text-slate-400">
                    <tr>
                      <th className="px-4 py-2.5 font-semibold">#</th>
                      <th className="px-4 py-2.5 font-semibold">When the message…</th>
                      <th className="px-4 py-2.5 font-semibold">The user is told</th>
                      <th className="px-4 py-2.5 font-semibold">Applies to</th>
                      <th className="px-4 py-2.5 font-semibold">On</th>
                      <th className="px-4 py-2.5 text-right font-semibold">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {rows.map((row) => {
                      const id = Number(getField(row, 'ResponseMessageRuleID') ?? 0);
                      const on = Boolean(getField(row, 'IsActive'));
                      const replacement = String(getField(row, 'Replacement') ?? '');
                      return (
                        <tr key={id} className={cn('border-b border-slate-50 last:border-0', !on && 'opacity-55')}>
                          <td className="px-4 py-3 tabular-nums text-slate-400">{String(getField(row, 'Sequence') ?? 0)}</td>
                          <td className="px-4 py-3">
                            <span className="text-xs uppercase tracking-wide text-slate-400">
                              {MATCH_LABEL(String(getField(row, 'MatchType') ?? ''))}
                            </span>
                            <p className="font-medium text-slate-800">{String(getField(row, 'Pattern') ?? '')}</p>
                            {String(getField(row, 'Note') ?? '') && (
                              <p className="mt-0.5 text-xs text-slate-400">{String(getField(row, 'Note'))}</p>
                            )}
                          </td>
                          <td className="px-4 py-3 text-slate-700">
                            {replacement || <span className="italic text-slate-400">(nothing — hidden)</span>}
                          </td>
                          <td className="px-4 py-3 text-xs text-slate-500">
                            <p>{String(getField(row, 'ProviderName') ?? 'All providers')}</p>
                            <p className="text-slate-400">{String(getField(row, 'ServiceName') ?? 'All services')}</p>
                          </td>
                          <td className="px-4 py-3 text-xs text-slate-500">{CHANNEL_LABEL(String(getField(row, 'Channel') ?? 'ALL'))}</td>
                          <td className="px-4 py-3">
                            <div className="flex items-center justify-end gap-1.5">
                              <button
                                type="button"
                                onClick={() => toggleActive.mutate(row)}
                                title={on ? 'Switch this rule off' : 'Switch this rule on'}
                                className={cn(
                                  'rounded-full px-2 py-0.5 text-[11px] font-semibold',
                                  on ? 'bg-success-50 text-success-700' : 'bg-slate-100 text-slate-500',
                                )}
                              >
                                {on ? 'On' : 'Off'}
                              </button>
                              <button
                                type="button"
                                onClick={() => openEdit(row)}
                                title="Edit"
                                className="grid size-8 place-items-center rounded-lg text-slate-400 hover:bg-slate-100 hover:text-slate-700"
                              >
                                <Pencil className="size-4" />
                              </button>
                              <button
                                type="button"
                                onClick={() => {
                                  if (window.confirm('Remove this rule? The provider’s own wording will go out unchanged again.')) remove.mutate(id);
                                }}
                                title="Remove"
                                className="grid size-8 place-items-center rounded-lg text-slate-400 hover:bg-danger-50 hover:text-danger-600"
                              >
                                <Trash2 className="size-4" />
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </CardBody>
        </Card>
      </div>

      <Modal open={formOpen} onClose={() => setFormOpen(false)} title={form.ResponseMessageRuleID > 0 ? 'Edit rule' : 'Add rule'} size="lg">
        <div className="space-y-4">
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Select label="When the message…" value={form.MatchType} onChange={(e) => set({ MatchType: e.target.value })}>
              {MATCH_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </Select>
            <Input
              label="Order"
              type="number"
              value={form.Sequence}
              onChange={(e) => set({ Sequence: e.target.value })}
              hint="Lower runs first. The first rule that matches wins."
            />
          </div>

          <Input
            label="Text to look for"
            value={form.Pattern}
            onChange={(e) => set({ Pattern: e.target.value })}
            placeholder="Low balance"
            hint={MATCH_OPTIONS.find((o) => o.value === form.MatchType)?.hint}
          />

          <Input
            label="What the user is told instead"
            value={form.Replacement}
            onChange={(e) => set({ Replacement: e.target.value })}
            placeholder="Server down. Please try again in a few minutes."
            hint="Leave empty to show no message at all."
          />

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <Select label="Provider" value={form.ProviderID} onChange={(e) => set({ ProviderID: e.target.value })}>
              <option value="0">All providers</option>
              {(providers.data ?? []).map((p) => (
                <option key={String(getField(p, 'ProviderID'))} value={String(getField(p, 'ProviderID'))}>
                  {String(getField(p, 'ProviderName') ?? '')}
                </option>
              ))}
            </Select>
            <Select label="Service" value={form.ServiceID} onChange={(e) => set({ ServiceID: e.target.value })}>
              <option value="0">All services</option>
              {(services.data ?? []).map((s) => (
                <option key={String(getField(s, 'ServiceID'))} value={String(getField(s, 'ServiceID'))}>
                  {String(getField(s, 'Name') ?? '')}
                </option>
              ))}
            </Select>
            <Select label="Shown to" value={form.Channel} onChange={(e) => set({ Channel: e.target.value })}>
              {CHANNEL_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </Select>
          </div>

          <Input
            label="Note (optional)"
            value={form.Note}
            onChange={(e) => set({ Note: e.target.value })}
            placeholder="Why this rule exists"
          />

          <label className="flex items-center gap-2 text-sm text-slate-600">
            <input
              type="checkbox"
              checked={form.IsActive}
              onChange={(e) => set({ IsActive: e.target.checked })}
              className="size-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500"
            />
            Switched on
          </label>

          <p className="rounded-lg bg-slate-50 p-3 text-xs text-slate-500">
            A rule changes only the words a user reads. The provider&apos;s real reply is still written to the
            provider log, so support can always see what actually came back.
          </p>

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button onClick={() => save.mutate()} disabled={!form.Pattern.trim() || save.isPending}>
              {save.isPending ? 'Saving…' : 'Save rule'}
            </Button>
          </div>
        </div>
      </Modal>
    </>
  );
}
