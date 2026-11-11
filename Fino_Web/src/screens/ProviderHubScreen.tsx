import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  IndianRupee, Pencil, Plus, RefreshCw, ServerCog, Settings2,
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
import { cn } from '@/lib/cn';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
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

/**
 * Coded gateways keep their credentials on this row, but each one means something different by
 * "login", "token" and "auth code" — JaiJui's auth code is a third credential it sends in every
 * body, Nexmint's is the webhook secret, which is a DIFFERENT key from the one its API calls are
 * signed with. Three generic labels and an operator pasting a secret into the wrong box is a
 * provider that silently rejects every call, or a webhook that verifies nothing.
 *
 * So the labels come from here, one entry per gateway, matched on the provider name exactly the
 * way the API matches it (JaiJuiProviderResolver / NexmintProviderResolver). Adding the next
 * coded provider is one entry, not another ternary on every Input.
 */
interface GatewayHint {
  marker: string;
  note: React.ReactNode;
  login: { label: string; placeholder: string };
  token: { label: string; placeholder: string };
  unique: { label: string; placeholder: string };
}

const GATEWAY_HINTS: GatewayHint[] = [
  {
    marker: 'jaijui',
    note: (
      <>
        JaiJui DMT gateway — credentials live here. Map: <b>Login</b> = registered mobile,
        <b> API token</b> = APIToken, <b>Auth code</b> = AuthCode. Link this provider from
        Settings → DMT Settings to use it for money transfer.
      </>
    ),
    login: { label: 'Login / member ID (JaiJui: registered mobile)', placeholder: 'Registered mobile number' },
    token: { label: 'API token / key (JaiJui: APIToken)', placeholder: 'JaiJui APIToken' },
    unique: { label: 'Auth code (JaiJui: AuthCode)', placeholder: 'JaiJui AuthCode' },
  },
  {
    marker: 'nexmint',
    note: (
      <>
        Nexmint payout gateway — credentials live here. Map: <b>Login</b> = client_id,
        <b> API token</b> = client_secret, <b>Auth code</b> = the <b>webhook secret</b> (a
        different key — it signs the callbacks, not the API calls). Leave <b>Base URL</b> blank for
        live, or end it with <code>/sandbox</code> to test without moving money. Set the
        <b> whitelisted IP</b> to Nexmint&apos;s, give them the callback URL below, and link this
        provider from Settings → DMT Settings with <b>Payout DMT</b> ticked.
      </>
    ),
    login: { label: 'Login / member ID (Nexmint: client_id)', placeholder: 'Nexmint client_id' },
    token: { label: 'API token / key (Nexmint: client_secret)', placeholder: 'Nexmint client_secret' },
    unique: { label: 'Auth code (Nexmint: webhook secret)', placeholder: 'Nexmint webhook secret' },
  },
];

const hintFor = (name: string): GatewayHint | null => {
  const normalised = name.replace(/\s+/g, '').toLowerCase();
  return GATEWAY_HINTS.find((hint) => normalised.includes(hint.marker)) ?? null;
};

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
 * Every balance a provider reported, as [label, amount] pairs.
 *
 * A provider is not one pot of money. Most keep a separate float per product — recharge, payout,
 * AEPS — and some name more than that. The response carries the two we model on the provider row
 * plus anything else the response rules extracted, so both are read: the known two first, then any
 * further "…Balance" key the provider sent, which is how a third or fourth wallet shows up without
 * needing a code change here.
 */
function reportedBalances(data: BalanceResult | null | undefined): [string, number][] {
  if (!data) return [];
  const out: [string, number][] = [];
  const seen = new Set<string>();

  const push = (label: string, value: unknown) => {
    const n = Number(value);
    if (!Number.isFinite(n)) return;
    const key = label.toLowerCase();
    if (seen.has(key)) return;
    seen.add(key);
    out.push([label, n]);
  };

  if (data.RechargeBalance !== null && data.RechargeBalance !== undefined) push('Recharge', data.RechargeBalance);
  if (data.DMTBalance !== null && data.DMTBalance !== undefined) push('Payout', data.DMTBalance);

  // Anything else the rules pulled out: "AEPSBalance" → "AEPS", "WalletBalance" → "Wallet".
  for (const [rawKey, rawValue] of Object.entries(data.ExtractedValues ?? {})) {
    if (!/balance$/i.test(rawKey)) continue;
    const label = rawKey.replace(/balance$/i, '').trim() || 'Balance';
    if (/^(recharge|main)$/i.test(label)) { push('Recharge', rawValue); continue; }
    if (/^(dmt|payout)$/i.test(label)) { push('Payout', rawValue); continue; }
    push(label.toUpperCase() === label ? label : label.replace(/^\w/, (c) => c.toUpperCase()), rawValue);
  }

  return out;
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

  const [balanceBusy, setBalanceBusy] = useState<number | null>(null);

  /**
   * Refresh one provider's balance.
   *
   * The answer arrives as a toast, not a dialog. Checking a balance is a glance, and a modal made
   * it a three-step interruption — open, read one number, close — before you could check the next
   * provider. The refreshed figures land in the row itself, which is where you were already looking.
   */
  const checkBalance = async (row: Row) => {
    const id = Number(getField(row, 'ProviderID'));
    const name = String(getField(row, 'ProviderName') ?? 'Provider');
    setBalanceBusy(id);
    try {
      const res = await apiPost<BalanceResult>('ProviderGateway/CheckBalance', { ProviderID: id });
      if (!res.ok) return toast.error(`${name}: ${res.message}`);

      refresh(); // stored balances were updated by the check

      const reported = reportedBalances(res.data);
      if (reported.length === 0) {
        // The call went through but the provider named no balance — usually a BALANCE endpoint or
        // response rule that has not been configured. Say that rather than showing "₹0".
        toast.info(`${name}: ${res.data?.Message || 'no balance reported. Check its BALANCE endpoint and response rules.'}`);
        return;
      }

      const summary = reported.map(([label, value]) => `${label} ${money(value)}`).join(' · ');
      if (String(res.data?.Status ?? '').toUpperCase() === 'SUCCESS') toast.success(`${name} — ${summary}`);
      else toast.info(`${name} — ${summary}. ${res.data?.Message ?? ''}`.trim());
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

  // Relabels the three credential fields as soon as the name names a coded gateway, so the
  // operator is told what goes in each box while they are filling it in — not after the first
  // call fails.
  const gatewayHint = useMemo(() => hintFor(form.ProviderName), [form.ProviderName]);

  return (
    <>
      <PageHeader
        title="API Providers"
        subtitle="Upstream gateways — every route, endpoint and response rule is configured here, no code change needed"
      />

      {/* ── KPI strip ─────────────────────────────────────────────────────────── */}
      <div className={cn(STAT_STRIP, "grid-cols-2 lg:grid-cols-4")}>
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
                        <div className="flex flex-wrap items-center gap-1.5">
                          <p className="font-semibold text-slate-800">{String(getField(row, 'ProviderName') ?? '—')}</p>
                          {/* A compiled integration (Pay1, JaiJui) needs no endpoint templates.
                              "No base URL" on one of these is normal, not a missing setting. */}
                          {Boolean(getField(row, 'IsCodedIntegration')) && (
                            <Pill
                              text={`Built-in${getField(row, 'CodedFamily') ? ` · ${String(getField(row, 'CodedFamily'))}` : ''}`}
                              tone="bg-info-50 text-info-600"
                            />
                          )}
                        </div>
                        <p className="max-w-[260px] truncate text-xs text-slate-400" title={String(getField(row, 'ProviderUrl') ?? '')}>
                          {Boolean(getField(row, 'IsCodedIntegration')) && !String(getField(row, 'ProviderUrl') ?? '')
                            ? 'Library integration — no base URL needed'
                            : String(getField(row, 'ProviderUrl') ?? '') || 'No base URL'}
                        </p>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-2">
                          <div>
                            {/* A provider keeps a separate float per product. Both are labelled, so
                                a payout figure is never mistaken for a recharge one. */}
                            <p className="tabular-nums text-sm font-semibold text-slate-800">
                              <span className="mr-1 text-xs font-normal text-slate-400">Recharge</span>
                              {money(rechargeBal)}
                            </p>
                            <p className="tabular-nums text-xs text-slate-500">
                              <span className="mr-1 text-slate-400">Payout</span>
                              {money(getField(row, 'DMTBalance'))}
                            </p>
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
          <Input label="Provider name *" value={form.ProviderName} onChange={setField('ProviderName')} placeholder="e.g. Payin, JaiJui DMT, Nexmint Payout" />
          <Input label="Base URL" value={form.ProviderUrl} onChange={setField('ProviderUrl')} placeholder="https://api.example.com" />
          <Input label="Whitelisted IP" value={form.IPAddress} onChange={setField('IPAddress')} placeholder="Optional" />
          <Input label="Support number" value={form.SupportNumber} onChange={setField('SupportNumber')} placeholder="Optional helpline" />
          {gatewayHint && (
            <div className="rounded-lg border border-brand-200 bg-brand-50 px-3 py-2 text-xs text-brand-700 sm:col-span-2">
              {gatewayHint.note}
            </div>
          )}
          <Input
            label={gatewayHint?.login.label ?? 'Login / member ID'}
            value={form.LoginID} onChange={setField('LoginID')}
            placeholder={gatewayHint?.login.placeholder ?? 'Used by the [LoginID] placeholder'}
          />
          <Input
            label={gatewayHint?.token.label ?? 'API token / key'}
            type="password" value={form.Token} onChange={setField('Token')}
            placeholder={gatewayHint?.token.placeholder ?? 'Used by the [Token] placeholder'}
          />
          <Input
            label={gatewayHint?.unique.label ?? 'Auth code / secondary key'}
            type="password" value={form.ProviderUniqueID} onChange={setField('ProviderUniqueID')}
            placeholder={gatewayHint?.unique.placeholder ?? 'Optional — used by DMT integrations'}
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

    </>
  );
}
