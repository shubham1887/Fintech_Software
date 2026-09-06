import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  IndianRupee, Network, Pencil, Plus, RefreshCw, ServerCog, Settings2,
  ShieldCheck, Trash2, TriangleAlert,
} from 'lucide-react';
import { apiBaseUrl, apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { StatTile } from '@/components/ui/StatTile';
import { Spinner } from '@/components/ui/Spinner';
import { Pill, ToggleDot, type Row } from './providerShared';

const LIST_PAYLOAD = { PageNumber: 1, PageSize: 500, IsActive: false, TakeCount: 500, SkipCount: 0 };

interface ProviderForm {
  ProviderName: string;
  ProviderUrl: string;
  IPAddress: string;
  LoginID: string;
  Token: string;
  ProviderUniqueID: string;
  SupportNumber: string;
  LowBalanceAmount: string;
}

const EMPTY_FORM: ProviderForm = {
  ProviderName: '', ProviderUrl: '', IPAddress: '', LoginID: '', Token: '', ProviderUniqueID: '',
  SupportNumber: '', LowBalanceAmount: '0',
};

/** A provider whose name contains "jaijui" is the coded JaiJui DMT integration — its three
 *  credentials map onto LoginID (mobile), Token (API token) and ProviderUniqueID (auth code). */
const isJaiJui = (name: string) => name.replace(/\s+/g, '').toLowerCase().includes('jaijui');

const money = (v: unknown) =>
  `₹${Number(v ?? 0).toLocaleString('en-IN', { maximumFractionDigits: 2 })}`;

/** Balance-enquiry result shown after a live balance check. */
interface BalanceResult {
  ProviderName?: string;
  Status?: string;
  Message?: string;
  RechargeBalance?: number | null;
  DMTBalance?: number | null;
  ExtractedValues?: Record<string, string>;
  RawResponse?: string;
}

/**
 * API Providers hub — the entry point of the provider gateway suite. Each row shows the live
 * balance and carries a single "Setting" button that opens the tabbed configuration page
 * (Request / Configuration / Response).
 */
export function ProviderHubScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const navigate = useNavigate();

  const listKey = ['provider-hub'];
  const providers = useQuery({
    queryKey: listKey,
    queryFn: () => fetchList('ProviderMaster/GetAll', LIST_PAYLOAD),
  });
  const refresh = () => qc.invalidateQueries({ queryKey: listKey });

  const rows = providers.data ?? [];
  const stats = useMemo(() => {
    const active = rows.filter((r) => Boolean(getField(r, 'IsActive'))).length;
    const totalBal = rows.reduce((sum, r) => sum + Number(getField(r, 'RechargeBalance') ?? 0), 0);
    const low = rows.filter((r) => {
      const bal = Number(getField(r, 'RechargeBalance') ?? 0);
      const min = Number(getField(r, 'LowBalanceAmount') ?? 0);
      return min > 0 && bal < min;
    }).length;
    return { total: rows.length, active, totalBal, low };
  }, [rows]);

  // ── Add / edit modal ─────────────────────────────────────────────────────────
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<Row | null>(null);
  const [form, setForm] = useState<ProviderForm>(EMPTY_FORM);

  const openAdd = () => { setEditing(null); setForm(EMPTY_FORM); setFormOpen(true); };
  const openEdit = (row: Row) => {
    setEditing(row);
    setForm({
      ProviderName: String(getField(row, 'ProviderName') ?? ''),
      ProviderUrl: String(getField(row, 'ProviderUrl') ?? ''),
      IPAddress: String(getField(row, 'IPAddress') ?? ''),
      LoginID: String(getField(row, 'LoginID') ?? ''),
      Token: String(getField(row, 'Token') ?? ''),
      ProviderUniqueID: String(getField(row, 'ProviderUniqueID') ?? ''),
      SupportNumber: String(getField(row, 'SupportNumber') ?? ''),
      LowBalanceAmount: String(getField(row, 'LowBalanceAmount') ?? '0'),
    });
    setFormOpen(true);
  };

  /** The one inbound address this provider pushes to. The key in it is what identifies the
   *  provider on the shared /api/Callback/p/{key} endpoint — safe to hand out, not a credential. */
  const callbackUrl = (row: Row | null) => {
    const key = String(getField(row ?? {}, 'CallbackKey') ?? '');
    return key ? `${apiBaseUrl()}Callback/p/${key}` : '';
  };

  const rotateMut = useMutation({
    mutationFn: (id: number) =>
      apiPost('ProviderMaster/Update', { ProviderID: id, FieldName: 'CallbackKey', NewValue: 'rotate' }),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      // The old address stops working the moment this succeeds, so say so plainly.
      toast.success('New callback URL generated. Register it with the provider — the old one is dead.');
      setFormOpen(false);
      refresh();
    },
  });

  const saveMut = useMutation({
    mutationFn: async () => {
      if (!editing) {
        return apiPost('ProviderMaster/Create', {
          ...form,
          LowBalanceAmount: Number(form.LowBalanceAmount) || 0,
          IsActive: true,
        });
      }
      // ProviderMaster updates are field-level — send one call per changed field.
      const id = Number(getField(editing, 'ProviderID'));
      const fields: (keyof ProviderForm)[] = [
        'ProviderName', 'ProviderUrl', 'IPAddress', 'LoginID', 'Token', 'ProviderUniqueID', 'SupportNumber', 'LowBalanceAmount',
      ];
      for (const name of fields) {
        const before = String(getField(editing, name) ?? '');
        if (before === form[name]) continue;
        const res = await apiPost('ProviderMaster/Update', { ProviderID: id, FieldName: name, NewValue: form[name] });
        if (!res.ok) return res;
      }
      return { ok: true, status: 'SUCCESS', message: 'Provider updated.', data: null };
    },
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success(editing ? 'Provider updated.' : 'Provider added.');
      setFormOpen(false);
      refresh();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Row actions ──────────────────────────────────────────────────────────────
  const toggleMut = useMutation({
    mutationFn: (row: Row) =>
      apiPost('ProviderMaster/Update', {
        ProviderID: Number(getField(row, 'ProviderID')),
        FieldName: 'IsActive',
        NewValue: String(!getField(row, 'IsActive')),
      }),
    onSuccess: (r) => (r.ok ? refresh() : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (row: Row) => apiPost('ProviderMaster/Delete', { ProviderID: Number(getField(row, 'ProviderID')) }),
    onSuccess: (r) => (r.ok ? (toast.success('Provider deleted.'), refresh()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const [balanceOpen, setBalanceOpen] = useState(false);
  const [balance, setBalance] = useState<BalanceResult | null>(null);
  const [balanceBusy, setBalanceBusy] = useState<number | null>(null);

  const checkBalance = async (row: Row) => {
    const id = Number(getField(row, 'ProviderID'));
    setBalanceBusy(id);
    try {
      const res = await apiPost<BalanceResult>('ProviderGateway/CheckBalance', { ProviderID: id });
      if (!res.ok) return toast.error(res.message);
      setBalance(res.data);
      setBalanceOpen(true);
      refresh(); // stored balances were updated by the check
    } catch (e) {
      toast.error((e as Error).message);
    } finally {
      setBalanceBusy(null);
    }
  };

  const openSetting = (row: Row) => {
    const id = Number(getField(row, 'ProviderID'));
    const name = String(getField(row, 'ProviderName') ?? '');
    navigate(`/s/ProviderSetting?id=${id}&name=${encodeURIComponent(name)}`);
  };

  const setField = (name: keyof ProviderForm) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((p) => ({ ...p, [name]: e.target.value }));

  return (
    <>
      <PageHeader
        title="API Providers"
        subtitle="Upstream gateways — every route, endpoint and response rule is configured here, no code change needed"
      />

      {/* ── KPI strip ─────────────────────────────────────────────────────────── */}
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatTile label="Providers" value={stats.total} icon={ServerCog} tone="bg-brand-50 text-brand-600" loading={providers.isLoading} />
        <StatTile label="Active" value={stats.active} icon={ShieldCheck} tone="bg-emerald-50 text-emerald-600" loading={providers.isLoading} />
        <StatTile label="Recharge float" value={money(stats.totalBal)} icon={IndianRupee} tone="bg-sky-50 text-sky-600" loading={providers.isLoading} />
        <StatTile label="Low balance" value={stats.low} icon={TriangleAlert} tone="bg-amber-50 text-amber-600" loading={providers.isLoading} />
      </div>

      {/* ── Provider table ───────────────────────────────────────────────────── */}
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 px-5 py-4">
          <div>
            <h2 className="font-display text-base font-bold text-slate-900">Provider directory</h2>
            <p className="text-xs text-slate-500">Open Setting to configure requests, operator codes and responses</p>
          </div>
          <div className="flex gap-2">
            <Button variant="secondary" size="sm" onClick={() => refresh()}>
              <RefreshCw className="size-4" /> Refresh
            </Button>
            <Button size="sm" onClick={openAdd}>
              <Plus className="size-4" /> Add provider
            </Button>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="px-4 py-3">Provider</th>
                <th className="px-4 py-3">Balance</th>
                <th className="px-4 py-3">Support</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Setting</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {providers.isLoading ? (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-sm text-slate-400">Loading providers…</td></tr>
              ) : rows.length === 0 ? (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-sm text-slate-400">No providers yet — add your first upstream API.</td></tr>
              ) : (
                rows.map((row, i) => {
                  const id = Number(getField(row, 'ProviderID'));
                  const rechargeBal = Number(getField(row, 'RechargeBalance') ?? 0);
                  const lowAt = Number(getField(row, 'LowBalanceAmount') ?? 0);
                  const isLow = lowAt > 0 && rechargeBal < lowAt;
                  const active = Boolean(getField(row, 'IsActive'));
                  return (
                    <tr key={id || i} className="border-b border-slate-100 last:border-0 hover:bg-slate-50/50">
                      <td className="px-4 py-3">
                        <p className="font-semibold text-slate-800">{String(getField(row, 'ProviderName') ?? '—')}</p>
                        <p className="max-w-[260px] truncate text-xs text-slate-400" title={String(getField(row, 'ProviderUrl') ?? '')}>
                          {String(getField(row, 'ProviderUrl') ?? '') || 'No base URL'}
                        </p>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <div>
                            <p className="font-semibold tabular-nums text-slate-800">{money(rechargeBal)}</p>
                            <p className="tabular-nums text-xs text-slate-500">Payout {money(getField(row, 'DMTBalance'))}</p>
                            {isLow && <Pill text="Low balance" tone="bg-amber-50 text-amber-700" />}
                          </div>
                          <button
                            type="button"
                            title="Fetch live balance from the provider"
                            onClick={() => checkBalance(row)}
                            disabled={balanceBusy === id}
                            className="rounded-full border border-slate-200 p-1.5 text-brand-600 transition-colors hover:bg-brand-50 disabled:opacity-50"
                          >
                            {balanceBusy === id ? <Spinner className="size-3.5" /> : <RefreshCw className="size-3.5" />}
                          </button>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-slate-600">{String(getField(row, 'SupportNumber') ?? '') || '—'}</td>
                      <td className="px-4 py-3">
                        <ToggleDot on={active} busy={toggleMut.isPending} onClick={() => toggleMut.mutate(row)} />
                      </td>
                      <td className="px-4 py-3">
                        <Button size="sm" variant="secondary" onClick={() => openSetting(row)} title="Request / Configuration / Response">
                          <Settings2 className="size-4" /> Setting
                        </Button>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1">
                          <Button size="sm" variant="ghost" title="Edit" onClick={() => openEdit(row)}>
                            <Pencil className="size-4 text-slate-500" />
                          </Button>
                          <Button
                            size="sm" variant="ghost" title="Delete" className="text-danger-600 hover:bg-danger-50"
                            onClick={() => { if (window.confirm('Delete this provider? Its endpoints, rules and mappings stay in the database.')) deleteMut.mutate(row); }}
                          >
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

      {/* ── Add / edit provider ──────────────────────────────────────────────── */}
      <Modal open={formOpen} onClose={() => setFormOpen(false)} title={editing ? 'Edit provider' : 'Add provider'} size="lg">
        <div className="grid grid-cols-1 gap-4 p-5 sm:grid-cols-2">
          <Input label="Provider name *" value={form.ProviderName} onChange={setField('ProviderName')} placeholder="e.g. Payin, JaiJui DMT" />
          <Input label="Base URL" value={form.ProviderUrl} onChange={setField('ProviderUrl')} placeholder="https://api.example.com" />
          <Input label="Whitelisted IP" value={form.IPAddress} onChange={setField('IPAddress')} placeholder="Optional" />
          <Input label="Support number" value={form.SupportNumber} onChange={setField('SupportNumber')} placeholder="Optional helpline" />
          {isJaiJui(form.ProviderName) && (
            <div className="rounded-lg border border-brand-200 bg-brand-50 px-3 py-2 text-xs text-brand-700 sm:col-span-2">
              JaiJui DMT gateway — credentials live here. Map: <b>Login</b> = registered mobile,
              <b> API token</b> = APIToken, <b>Auth code</b> = AuthCode. Link this provider from
              Settings → DMT Settings to use it for money transfer.
            </div>
          )}
          <Input
            label={isJaiJui(form.ProviderName) ? 'Login / member ID (JaiJui: registered mobile)' : 'Login / member ID'}
            value={form.LoginID} onChange={setField('LoginID')}
            placeholder={isJaiJui(form.ProviderName) ? 'Registered mobile number' : 'Used by the [LoginID] placeholder'}
          />
          <Input
            label={isJaiJui(form.ProviderName) ? 'API token / key (JaiJui: APIToken)' : 'API token / key'}
            type="password" value={form.Token} onChange={setField('Token')}
            placeholder={isJaiJui(form.ProviderName) ? 'JaiJui APIToken' : 'Used by the [Token] placeholder'}
          />
          <Input
            label={isJaiJui(form.ProviderName) ? 'Auth code (JaiJui: AuthCode)' : 'Auth code / secondary key'}
            type="password" value={form.ProviderUniqueID} onChange={setField('ProviderUniqueID')}
            placeholder={isJaiJui(form.ProviderName) ? 'JaiJui AuthCode' : 'Optional — used by DMT integrations'}
          />
          <Input label="Low balance alert (₹)" type="number" value={form.LowBalanceAmount} onChange={setField('LowBalanceAmount')} />
          {editing && callbackUrl(editing) && (
            <div className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 sm:col-span-2">
              <p className="text-xs font-medium text-slate-700">Callback URL — give this to the provider</p>
              <p className="mt-1 break-all font-mono text-xs text-slate-900">{callbackUrl(editing)}</p>
              <p className="mt-1 text-xs text-slate-500">
                Status pushes for this provider go here. Set the whitelisted IP above so nothing else can use it.
              </p>
              <div className="mt-2 flex gap-2">
                <Button
                  variant="secondary"
                  onClick={() => {
                    navigator.clipboard.writeText(callbackUrl(editing));
                    toast.success('Callback URL copied.');
                  }}
                >
                  Copy
                </Button>
                <Button
                  variant="secondary"
                  loading={rotateMut.isPending}
                  onClick={() => rotateMut.mutate(Number(getField(editing, 'ProviderID')))}
                >
                  Regenerate
                </Button>
              </div>
            </div>
          )}
          <div className="flex items-end justify-end gap-2 sm:col-span-2">
            <Button variant="secondary" onClick={() => setFormOpen(false)}>Cancel</Button>
            <Button
              loading={saveMut.isPending}
              onClick={() => {
                if (!form.ProviderName.trim()) return toast.error('Provider name is required.');
                saveMut.mutate();
              }}
            >
              {editing ? 'Save changes' : 'Add provider'}
            </Button>
          </div>
        </div>
      </Modal>

      {/* ── Balance result ───────────────────────────────────────────────────── */}
      <Modal open={balanceOpen} onClose={() => setBalanceOpen(false)} title={`Balance — ${balance?.ProviderName ?? ''}`}>
        <div className="space-y-4 p-5">
          <div className="flex items-center gap-2">
            <Network className="size-4 text-brand-600" />
            <Pill
              text={String(balance?.Status ?? '—')}
              tone={balance?.Status === 'SUCCESS' ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-700'}
            />
            <span className="text-sm text-slate-600">{balance?.Message}</span>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="rounded-card border border-slate-200/70 p-4">
              <p className="text-xs text-slate-500">Recharge balance</p>
              <p className="font-display text-xl font-bold text-slate-900">
                {balance?.RechargeBalance != null ? money(balance.RechargeBalance) : '—'}
              </p>
            </div>
            <div className="rounded-card border border-slate-200/70 p-4">
              <p className="text-xs text-slate-500">Payout / DMT balance</p>
              <p className="font-display text-xl font-bold text-slate-900">
                {balance?.DMTBalance != null ? money(balance.DMTBalance) : '—'}
              </p>
            </div>
          </div>
          {balance?.ExtractedValues && Object.keys(balance.ExtractedValues).length > 0 && (
            <div>
              <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-500">Extracted values</p>
              <div className="rounded-lg bg-slate-50 p-3 font-mono text-xs text-slate-700">
                {Object.entries(balance.ExtractedValues).map(([k, v]) => (
                  <p key={k}>{k} = {v}</p>
                ))}
              </div>
            </div>
          )}
          {balance?.RawResponse && (
            <details>
              <summary className="cursor-pointer text-xs font-semibold text-slate-500">Raw provider reply</summary>
              <pre className="mt-2 max-h-48 overflow-auto whitespace-pre-wrap rounded-lg bg-ink-950 p-3 font-mono text-[11px] text-emerald-300">
                {balance.RawResponse}
              </pre>
            </details>
          )}
        </div>
      </Modal>
    </>
  );
}
