import { useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Cable, Pencil, Plus, Trash2, X } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete } from '@/components/ui/Autocomplete';
import {
  KIND_BADGE, KIND_OPTIONS, PlaceholderChips, Pill, TemplateArea, ToggleDot,
  normalizeKind, useServices, type Row,
} from './providerShared';

interface EndpointForm {
  ProviderDetailID: number; // 0 = new
  ServiceID: string;
  UrlType: string;
  MethodType: string;
  ProviderURL: string;
  HeaderJson: string;
  DateFormat: string;
  TransactionIDPrefix: string;
  IsActive: boolean;
}

const EMPTY_FORM: EndpointForm = {
  ProviderDetailID: 0, ServiceID: '', UrlType: 'TRANSACTION', MethodType: 'POST',
  ProviderURL: '', HeaderJson: '{"Content-Type":"application/json"}', DateFormat: '', TransactionIDPrefix: '', IsActive: true,
};

/**
 * "Request" tab of the provider Setting page — the endpoint templates the gateway renders and
 * fires per service. Template chips are served by the engine, so what you can insert is exactly
 * what the engine can substitute.
 */
export function EndpointsPanel({ providerId }: { providerId: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const services = useServices();

  const listKey = ['provider-endpoints', providerId];
  const endpoints = useQuery({
    queryKey: listKey,
    queryFn: () => fetchList('ProviderDetails/GetByProviderID', { ProviderID: Number(providerId) }),
    enabled: Number(providerId) > 0,
  });
  const refresh = () => qc.invalidateQueries({ queryKey: listKey });

  const [formOpen, setFormOpen] = useState(false);
  const [form, setForm] = useState<EndpointForm>(EMPTY_FORM);
  const urlRef = useRef<HTMLTextAreaElement>(null);

  const openAdd = () => { setForm(EMPTY_FORM); setFormOpen(true); };
  const openEdit = (row: Row) => {
    setForm({
      ProviderDetailID: Number(getField(row, 'ProviderDetailID')),
      ServiceID: String(getField(row, 'ServiceID') ?? ''),
      UrlType: normalizeKind(getField(row, 'UrlType')),
      MethodType: String(getField(row, 'MethodType') ?? 'POST').toUpperCase(),
      ProviderURL: String(getField(row, 'ProviderURL') ?? ''),
      HeaderJson: String(getField(row, 'HeaderJson') ?? ''),
      DateFormat: String(getField(row, 'DateFormat') ?? ''),
      TransactionIDPrefix: String(getField(row, 'TransactionIDPrefix') ?? ''),
      IsActive: Boolean(getField(row, 'IsActive')),
    });
    setFormOpen(true);
  };

  const insertToken = (token: string) => {
    const el = urlRef.current;
    setForm((p) => {
      if (!el) return { ...p, ProviderURL: p.ProviderURL + token };
      const start = el.selectionStart ?? p.ProviderURL.length;
      const end = el.selectionEnd ?? start;
      return { ...p, ProviderURL: p.ProviderURL.slice(0, start) + token + p.ProviderURL.slice(end) };
    });
    el?.focus();
  };

  const saveMut = useMutation({
    mutationFn: () => {
      const payload = {
        ProviderID: Number(providerId),
        ServiceID: Number(form.ServiceID),
        UrlType: form.UrlType,
        MethodType: form.MethodType,
        ProviderURL: form.ProviderURL,
        HeaderJson: form.HeaderJson,
        DateFormat: form.DateFormat,
        TransactionIDPrefix: form.TransactionIDPrefix,
        IsActive: form.IsActive,
        Balance: 0,
      };
      return form.ProviderDetailID
        ? apiPost('ProviderDetails/Update', { ...payload, ProviderDetailID: form.ProviderDetailID })
        : apiPost('ProviderDetails/Create', payload);
    },
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success(form.ProviderDetailID ? 'Endpoint updated.' : 'Endpoint added.');
      setFormOpen(false);
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const toggleMut = useMutation({
    mutationFn: (row: Row) =>
      apiPost('ProviderDetails/Update', {
        ProviderDetailID: Number(getField(row, 'ProviderDetailID')),
        ProviderID: Number(getField(row, 'ProviderID')),
        ServiceID: Number(getField(row, 'ServiceID')),
        UrlType: String(getField(row, 'UrlType') ?? ''),
        MethodType: String(getField(row, 'MethodType') ?? 'POST'),
        ProviderURL: String(getField(row, 'ProviderURL') ?? ''),
        HeaderJson: String(getField(row, 'HeaderJson') ?? ''),
        DateFormat: String(getField(row, 'DateFormat') ?? ''),
        TransactionIDPrefix: String(getField(row, 'TransactionIDPrefix') ?? ''),
        IsActive: !getField(row, 'IsActive'),
        Balance: Number(getField(row, 'Balance') ?? 0),
      }),
    onSuccess: (r) => (r.ok ? refresh() : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (row: Row) =>
      apiPost('ProviderDetails/Delete', { ProviderDetailID: Number(getField(row, 'ProviderDetailID')) }),
    onSuccess: (r) => (r.ok ? (toast.success('Endpoint removed.'), refresh()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const onSave = () => {
    if (!providerId) return toast.error('Select a provider first.');
    if (!form.ServiceID) return toast.error('Select the service this endpoint serves.');
    if (!form.ProviderURL.trim()) return toast.error('The URL template is required.');
    if (form.HeaderJson.trim()) {
      try { JSON.parse(form.HeaderJson); } catch { return toast.error('Headers must be valid JSON, e.g. {"Content-Type":"application/json"}.'); }
    }
    saveMut.mutate();
  };

  const rows = endpoints.data ?? [];

  return (
    <>
      <div className="mb-4 flex items-center justify-between gap-3">
        <p className="text-sm text-slate-500">Where and how the gateway calls this provider — one endpoint per service and kind.</p>
        <Button size="sm" onClick={openAdd} disabled={!providerId}><Plus className="size-4" /> Add endpoint</Button>
      </div>

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">Service</th>
                <th className="px-4 py-3">Kind</th>
                <th className="px-4 py-3">Method</th>
                <th className="px-4 py-3">Template</th>
                <th className="px-4 py-3">Prefix</th>
                <th className="px-4 py-3">Active</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {endpoints.isFetching && rows.length === 0 ? (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-sm text-slate-400">Loading…</td></tr>
              ) : rows.length === 0 ? (
                <tr><td colSpan={7} className="px-4 py-10 text-center text-sm text-slate-400">No endpoints yet — add the Transaction endpoint first.</td></tr>
              ) : (
                rows.map((row, i) => {
                  const kind = normalizeKind(getField(row, 'UrlType'));
                  return (
                    <tr key={String(getField(row, 'ProviderDetailID') ?? i)} className="border-b border-slate-100 last:border-0 hover:bg-slate-50/50">
                      <td className="px-4 py-3 font-medium text-slate-700">{String(getField(row, 'ServiceName') ?? getField(row, 'ServiceID') ?? '—')}</td>
                      <td className="px-4 py-3"><Pill text={kind} tone={KIND_BADGE[kind] ?? 'bg-slate-100 text-slate-600'} /></td>
                      <td className="px-4 py-3 font-mono text-xs text-slate-600">{String(getField(row, 'MethodType') ?? '')}</td>
                      <td className="px-4 py-3">
                        <p className="max-w-[360px] truncate font-mono text-xs text-slate-600" title={String(getField(row, 'ProviderURL') ?? '')}>
                          {String(getField(row, 'ProviderURL') ?? '')}
                        </p>
                      </td>
                      <td className="px-4 py-3 font-mono text-xs text-slate-500">{String(getField(row, 'TransactionIDPrefix') ?? '') || '—'}</td>
                      <td className="px-4 py-3">
                        <ToggleDot on={Boolean(getField(row, 'IsActive'))} busy={toggleMut.isPending} onClick={() => toggleMut.mutate(row)} />
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1">
                          <Button size="sm" variant="ghost" title="Edit" onClick={() => openEdit(row)}><Pencil className="size-4 text-slate-500" /></Button>
                          <Button size="sm" variant="ghost" title="Delete" className="text-danger-600 hover:bg-danger-50"
                            onClick={() => { if (window.confirm('Remove this endpoint?')) deleteMut.mutate(row); }}>
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
              <Cable className="size-4 text-brand-600" />
              {form.ProviderDetailID ? 'Edit endpoint' : 'New endpoint'}
            </h2>
            <Button size="sm" variant="ghost" onClick={() => setFormOpen(false)}><X className="size-4" /></Button>
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <Autocomplete
              label="Service *"
              placeholder="Search service…"
              value={form.ServiceID}
              options={(services.data ?? []).map((s) => ({ value: String(getField(s, 'ServiceID')), label: String(getField(s, 'Name') ?? '') }))}
              onChange={(v) => setForm((p) => ({ ...p, ServiceID: String(v) }))}
            />
            <Select label="Endpoint kind *" value={form.UrlType} onChange={(e) => setForm((p) => ({ ...p, UrlType: e.target.value }))}>
              {KIND_OPTIONS.map((k) => <option key={k.value} value={k.value}>{k.label}</option>)}
            </Select>
            <Select label="HTTP method" value={form.MethodType} onChange={(e) => setForm((p) => ({ ...p, MethodType: e.target.value }))}>
              <option value="POST">POST</option>
              <option value="GET">GET</option>
            </Select>
            <Input label="Transaction ID prefix" value={form.TransactionIDPrefix}
              onChange={(e) => setForm((p) => ({ ...p, TransactionIDPrefix: e.target.value }))} placeholder="Optional, e.g. FP" />
          </div>

          <div className="mt-4">
            <TemplateArea
              ref={urlRef}
              label="URL / body template *"
              rows={4}
              value={form.ProviderURL}
              onChange={(e) => setForm((p) => ({ ...p, ProviderURL: e.target.value }))}
              placeholder={'GET:  https://api.example.com/pay?key=[Token]&num=[Number]&amt=[Amount]&op=[OperatorCode]&ref=[RequestID]\nPOST: https://api.example.com/pay?{"key":"[Token]","number":"[Number]","amount":"[Amount]"}'}
            />
            <p className="mt-1 text-xs text-slate-400">
              For POST, everything after <span className="font-mono">?</span> starting with <span className="font-mono">{'{'}</span> is sent as the JSON body.
            </p>
            <div className="mt-2">
              <PlaceholderChips direction="request" onInsert={insertToken} />
            </div>
          </div>

          <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            <TemplateArea
              label="Headers (JSON)"
              rows={2}
              value={form.HeaderJson}
              onChange={(e) => setForm((p) => ({ ...p, HeaderJson: e.target.value }))}
              placeholder='{"Content-Type":"application/json","Authorization":"Bearer [Token]"}'
            />
            <div className="grid grid-cols-1 gap-3">
              <Input label="Date format" value={form.DateFormat}
                onChange={(e) => setForm((p) => ({ ...p, DateFormat: e.target.value }))} placeholder="e.g. yyyyMMdd — feeds the [Date] token" />
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <input type="checkbox" checked={form.IsActive} onChange={(e) => setForm((p) => ({ ...p, IsActive: e.target.checked }))} className="size-4 accent-brand-600" />
                Endpoint active
              </label>
            </div>
          </div>

          <div className="mt-4 flex justify-end gap-2">
            <Button variant="secondary" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button loading={saveMut.isPending} onClick={onSave}>{form.ProviderDetailID ? 'Save changes' : 'Add endpoint'}</Button>
          </div>
        </div>
      )}
    </>
  );
}
