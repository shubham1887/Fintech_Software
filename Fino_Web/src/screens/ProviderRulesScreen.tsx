import { useMemo, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FlaskConical, Pencil, Plus, ScrollText, Trash2, X } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import {
  KIND_BADGE, KIND_OPTIONS, PlaceholderChips, Pill, RULE_STATUS_OPTIONS, STATUS_BADGE,
  TemplateArea, ToggleDot, normalizeKind, normalizeRuleStatus, type Row,
} from './providerShared';

interface RuleForm {
  ProviderResponseMasterID: number; // 0 = new
  Type: string;
  TextMustexits: string;
  Response: string;
  Status: string;
  IsActive: boolean;
}

const EMPTY_FORM: RuleForm = {
  ProviderResponseMasterID: 0, Type: 'TRANSACTION', TextMustexits: '', Response: '', Status: 'SUCCESS', IsActive: true,
};

interface TestResult {
  Matched?: boolean;
  MatchedRuleID?: number | null;
  MatchedPattern?: string;
  MappedStatus?: string;
  Message?: string;
  ExtractedValues?: Record<string, string>;
}

/**
 * "Response" tab of the provider Setting page — how the gateway reads a provider's reply.
 * A rule matches when every fragment of its match text appears in the reply; its template then
 * extracts values ([RefID]/[TxnID]/[Balance]/[Message]) and its status decides
 * SUCCESS / FAILED / PENDING. The built-in test lab dry-runs a sample reply through the engine.
 */
export function RulesPanel({ providerId }: { providerId: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [kindFilter, setKindFilter] = useState('');

  const listKey = ['provider-rules', providerId];
  const rules = useQuery({
    queryKey: listKey,
    queryFn: () => fetchList('ProviderResponseMaster/GetByProviderID', { ProviderID: Number(providerId) }),
    enabled: Number(providerId) > 0,
  });
  const refresh = () => qc.invalidateQueries({ queryKey: listKey });

  const [formOpen, setFormOpen] = useState(false);
  const [form, setForm] = useState<RuleForm>(EMPTY_FORM);
  const templateRef = useRef<HTMLTextAreaElement>(null);

  const openAdd = () => { setForm(EMPTY_FORM); setFormOpen(true); };
  const openEdit = (row: Row) => {
    setForm({
      ProviderResponseMasterID: Number(getField(row, 'ProviderResponseMasterID')),
      Type: normalizeKind(getField(row, 'Type')),
      TextMustexits: String(getField(row, 'TextMustexits') ?? ''),
      Response: String(getField(row, 'Response') ?? ''),
      Status: normalizeRuleStatus(getField(row, 'Status')),
      IsActive: Boolean(getField(row, 'IsActive')),
    });
    setFormOpen(true);
  };

  const insertToken = (token: string) => {
    const el = templateRef.current;
    setForm((p) => {
      if (!el) return { ...p, Response: p.Response + token };
      const start = el.selectionStart ?? p.Response.length;
      const end = el.selectionEnd ?? start;
      return { ...p, Response: p.Response.slice(0, start) + token + p.Response.slice(end) };
    });
    el?.focus();
  };

  const saveMut = useMutation({
    mutationFn: () => {
      const payload = {
        ProviderID: Number(providerId),
        Type: form.Type,
        TextMustexits: form.TextMustexits,
        Response: form.Response,
        Status: form.Status,
        IsReference: false,
        IsActive: form.IsActive,
      };
      return form.ProviderResponseMasterID
        ? apiPost('ProviderResponseMaster/Update', { ...payload, ProviderResponseMasterID: form.ProviderResponseMasterID })
        : apiPost('ProviderResponseMaster/Create', payload);
    },
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success(form.ProviderResponseMasterID ? 'Rule updated.' : 'Rule added.');
      setFormOpen(false);
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggleMut = useMutation({
    mutationFn: (row: Row) =>
      apiPost('ProviderResponseMaster/Update', {
        ProviderResponseMasterID: Number(getField(row, 'ProviderResponseMasterID')),
        ProviderID: Number(getField(row, 'ProviderID')),
        Type: String(getField(row, 'Type') ?? ''),
        TextMustexits: String(getField(row, 'TextMustexits') ?? ''),
        Response: String(getField(row, 'Response') ?? ''),
        Status: String(getField(row, 'Status') ?? ''),
        IsReference: Boolean(getField(row, 'IsReference')),
        IsActive: !getField(row, 'IsActive'),
      }),
    onSuccess: (r) => (r.ok ? refresh() : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (row: Row) =>
      apiPost('ProviderResponseMaster/Delete', { ProviderResponseMasterID: Number(getField(row, 'ProviderResponseMasterID')) }),
    onSuccess: (r) => (r.ok ? (toast.success('Rule removed.'), refresh()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const onSave = () => {
    if (!providerId) return toast.error('Select a provider first.');
    if (!form.TextMustexits.trim()) return toast.error('The match text is required — fragments separated by commas.');
    saveMut.mutate();
  };

  // ── Test lab ─────────────────────────────────────────────────────────────────
  const [testKind, setTestKind] = useState('TRANSACTION');
  const [sample, setSample] = useState('');
  const [testResult, setTestResult] = useState<TestResult | null>(null);

  const testMut = useMutation({
    mutationFn: () =>
      apiPost<TestResult>('ProviderGateway/TestResponse', {
        ProviderID: Number(providerId),
        RuleKind: testKind,
        SampleText: sample,
      }),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      setTestResult(r.data);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const allRows = rules.data ?? [];
  const rows = useMemo(
    () => (kindFilter ? allRows.filter((r) => normalizeKind(getField(r, 'Type')) === kindFilter) : allRows),
    [allRows, kindFilter],
  );

  return (
    <>
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div className="w-56">
          <Select label="Filter by kind" value={kindFilter} onChange={(e) => setKindFilter(e.target.value)}>
            <option value="">All kinds</option>
            {KIND_OPTIONS.map((k) => <option key={k.value} value={k.value}>{k.label}</option>)}
          </Select>
        </div>
        <Button size="sm" onClick={openAdd} disabled={!providerId}><Plus className="size-4" /> Add rule</Button>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">Kind</th>
                <th className="px-4 py-3">Match text (all fragments must appear)</th>
                <th className="px-4 py-3">Extraction template</th>
                <th className="px-4 py-3">Maps to</th>
                <th className="px-4 py-3">Active</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {rules.isFetching && rows.length === 0 ? (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-sm text-slate-400">Loading…</td></tr>
              ) : rows.length === 0 ? (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-sm text-slate-400">No rules yet — replies will stay Pending until you add rules.</td></tr>
              ) : (
                rows.map((row, i) => {
                  const kind = normalizeKind(getField(row, 'Type'));
                  const status = normalizeRuleStatus(getField(row, 'Status'));
                  return (
                    <tr key={String(getField(row, 'ProviderResponseMasterID') ?? i)} className="border-b border-slate-100 last:border-0 hover:bg-slate-50/50">
                      <td className="px-4 py-3"><Pill text={kind} tone={KIND_BADGE[kind] ?? 'bg-slate-100 text-slate-600'} /></td>
                      <td className="px-4 py-3">
                        <p className="max-w-[260px] truncate font-mono text-xs text-slate-700" title={String(getField(row, 'TextMustexits') ?? '')}>
                          {String(getField(row, 'TextMustexits') ?? '')}
                        </p>
                      </td>
                      <td className="px-4 py-3">
                        <p className="max-w-[300px] truncate font-mono text-xs text-slate-500" title={String(getField(row, 'Response') ?? '')}>
                          {String(getField(row, 'Response') ?? '') || '—'}
                        </p>
                      </td>
                      <td className="px-4 py-3"><Pill text={status} tone={STATUS_BADGE[status] ?? 'bg-slate-100 text-slate-600'} /></td>
                      <td className="px-4 py-3">
                        <ToggleDot on={Boolean(getField(row, 'IsActive'))} busy={toggleMut.isPending} onClick={() => toggleMut.mutate(row)} />
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1">
                          <Button size="sm" variant="ghost" title="Edit" onClick={() => openEdit(row)}><Pencil className="size-4 text-slate-500" /></Button>
                          <Button size="sm" variant="ghost" title="Delete" className="text-danger-600 hover:bg-danger-50"
                            onClick={() => { if (window.confirm('Remove this rule?')) deleteMut.mutate(row); }}>
                            <Trash2 className="size-4" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {formOpen && (
        <div className="mt-6 rounded-card border border-brand-200/70 bg-white p-5 shadow-card">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="flex items-center gap-2 font-display text-base font-bold text-slate-900">
              <ScrollText className="size-4 text-brand-600" />
              {form.ProviderResponseMasterID ? 'Edit rule' : 'New rule'}
            </h2>
            <Button size="sm" variant="ghost" onClick={() => setFormOpen(false)}><X className="size-4" /></Button>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <Select label="Rule kind *" value={form.Type} onChange={(e) => setForm((p) => ({ ...p, Type: e.target.value }))}>
              {KIND_OPTIONS.map((k) => <option key={k.value} value={k.value}>{k.label}</option>)}
            </Select>
            <Select label="Maps reply to *" value={form.Status} onChange={(e) => setForm((p) => ({ ...p, Status: e.target.value }))}>
              {RULE_STATUS_OPTIONS.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
            </Select>
            <label className="flex items-end gap-2 pb-2 text-sm text-slate-700">
              <input type="checkbox" checked={form.IsActive} onChange={(e) => setForm((p) => ({ ...p, IsActive: e.target.checked }))} className="size-4 accent-brand-600" />
              Rule active
            </label>
          </div>

          <div className="mt-4">
            <Input
              label="Match text * (comma-separated fragments — every fragment must appear in the reply)"
              value={form.TextMustexits}
              onChange={(e) => setForm((p) => ({ ...p, TextMustexits: e.target.value }))}
              placeholder={'"status":"SUCCESS"'}
              className="font-mono text-xs"
            />
          </div>

          <div className="mt-4">
            <TemplateArea
              ref={templateRef}
              label="Extraction template (optional — mirror of the reply with capture tokens)"
              rows={3}
              value={form.Response}
              onChange={(e) => setForm((p) => ({ ...p, Response: e.target.value }))}
              placeholder={'{"message":"[Message]","status":"SUCCESS","transactionID":"[RefID]"}'}
            />
            <div className="mt-2">
              <PlaceholderChips direction="capture" onInsert={insertToken} />
            </div>
          </div>

          <div className="mt-4 flex justify-end gap-2">
            <Button variant="secondary" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button loading={saveMut.isPending} onClick={onSave}>{form.ProviderResponseMasterID ? 'Save changes' : 'Add rule'}</Button>
          </div>
        </div>
      )}

      {/* ── Test lab ─────────────────────────────────────────────────────────── */}
      <div className="mt-6 rounded-card border border-slate-200/70 bg-white p-5 shadow-card">
        <h2 className="mb-1 flex items-center gap-2 font-display text-base font-bold text-slate-900">
          <FlaskConical className="size-4 text-brand-600" /> Test lab
        </h2>
        <p className="mb-4 text-xs text-slate-500">
          Paste a sample provider reply and dry-run it through the live engine — see which rule matches, the mapped status and every extracted value. No transaction is created.
        </p>
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          <div>
            <Select label="Rule group" value={testKind} onChange={(e) => setTestKind(e.target.value)}>
              {KIND_OPTIONS.map((k) => <option key={k.value} value={k.value}>{k.label}</option>)}
            </Select>
            <div className="mt-3">
              <TemplateArea
                label="Sample provider reply"
                rows={6}
                value={sample}
                onChange={(e) => setSample(e.target.value)}
                placeholder='{"message":"Recharge done successfully","status":"SUCCESS","transactionID":"MH02565853234"}'
              />
            </div>
            <div className="mt-3 flex justify-end">
              <Button
                loading={testMut.isPending}
                disabled={!providerId || !sample.trim()}
                onClick={() => testMut.mutate()}
              >
                <FlaskConical className="size-4" /> Run test
              </Button>
            </div>
          </div>
          <div className="rounded-card border border-slate-200/70 bg-slate-50/60 p-4">
            {!testResult ? (
              <p className="py-10 text-center text-sm text-slate-400">Result appears here after you run a test.</p>
            ) : (
              <div className="space-y-3">
                <div className="flex items-center gap-2">
                  <Pill
                    text={testResult.Matched ? 'RULE MATCHED' : 'NO MATCH'}
                    tone={testResult.Matched ? 'bg-emerald-50 text-emerald-700' : 'bg-danger-50 text-danger-600'}
                  />
                  {testResult.Matched && testResult.MappedStatus && (
                    <Pill text={`→ ${testResult.MappedStatus}`} tone={STATUS_BADGE[normalizeRuleStatus(testResult.MappedStatus)] ?? 'bg-slate-100 text-slate-600'} />
                  )}
                </div>
                <p className="text-sm text-slate-600">{testResult.Message}</p>
                {testResult.MatchedPattern && (
                  <p className="font-mono text-xs text-slate-500">Matched rule #{testResult.MatchedRuleID}: {testResult.MatchedPattern}</p>
                )}
                {testResult.ExtractedValues && Object.keys(testResult.ExtractedValues).length > 0 && (
                  <div>
                    <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-500">Extracted values</p>
                    <div className="rounded-lg bg-white p-3 font-mono text-xs text-slate-700 shadow-inner">
                      {Object.entries(testResult.ExtractedValues).map(([k, v]) => (
                        <p key={k}><span className="text-brand-600">{k}</span> = {v}</p>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
