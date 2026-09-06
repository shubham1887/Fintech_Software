import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Check, Copy, Eye, EyeOff, KeyRound, Link2, Plus, RefreshCw, ShieldCheck, Trash2, Webhook,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField as get } from '@/lib/row';
import { UserTypeCode } from '@/lib/roles';
import { cn } from '@/lib/cn';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { Spinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/EmptyState';
import type { ApiResult } from '@/types/api';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

/** A titled section card. */
function SectionCard({ icon: Icon, title, description, action, children }: {
  icon: typeof KeyRound; title: string; description?: string; action?: ReactNode; children: ReactNode;
}) {
  return (
    <Card className="p-5">
      <div className="mb-4 flex items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <span className="mt-0.5 grid size-9 shrink-0 place-items-center rounded-xl bg-brand-50 text-brand-600">
            <Icon className="size-5" />
          </span>
          <div>
            <h3 className="text-sm font-semibold text-slate-800">{title}</h3>
            {description && <p className="mt-0.5 text-sm text-slate-500">{description}</p>}
          </div>
        </div>
        {action}
      </div>
      {children}
    </Card>
  );
}

/**
 * Two-step OTP gate. Opens on `open`; step 1 sends an OTP to the user's mobile, step 2
 * takes the code and confirms. `send` and `confirm` return the standard ApiResult.
 */
function OtpModal({ open, onClose, title, description, send, confirm, onDone }: {
  open: boolean; onClose: () => void; title: string; description: string;
  send: () => Promise<ApiResult<unknown>>; confirm: (otp: string) => Promise<ApiResult<unknown>>;
  onDone: () => void;
}) {
  const toast = useToast();
  const [sent, setSent] = useState(false);
  const [otp, setOtp] = useState('');

  useEffect(() => {
    if (!open) { setSent(false); setOtp(''); }
  }, [open]);

  const sendMut = useMutation({
    mutationFn: send,
    onSuccess: (r) => (r.ok ? (setSent(true), toast.success('OTP sent to your registered mobile.')) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const confirmMut = useMutation({
    mutationFn: () => confirm(otp.trim()),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      onDone();
      onClose();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Modal open={open} onClose={onClose} title={title}>
      <div className="space-y-4">
        <p className="text-sm text-slate-500">{description}</p>
        {!sent ? (
          <Button className="w-full" loading={sendMut.isPending} onClick={() => sendMut.mutate()}>
            Send OTP
          </Button>
        ) : (
          <>
            <Input
              label="Enter OTP"
              inputMode="numeric"
              placeholder="6-digit code"
              value={otp}
              onChange={(e) => setOtp(e.target.value.replace(/\D/g, '').slice(0, 6))}
            />
            <div className="flex items-center justify-between">
              <button
                type="button"
                className="text-sm font-medium text-brand-600 hover:text-brand-700 disabled:opacity-50"
                disabled={sendMut.isPending}
                onClick={() => sendMut.mutate()}
              >
                Resend OTP
              </button>
              <div className="flex gap-2">
                <Button variant="outline" size="sm" onClick={onClose}>Cancel</Button>
                <Button size="sm" loading={confirmMut.isPending} disabled={otp.length < 4} onClick={() => confirmMut.mutate()}>
                  Confirm
                </Button>
              </div>
            </div>
          </>
        )}
      </div>
    </Modal>
  );
}

/** A minimal on/off switch (no dedicated component in the kit). */
function Toggle({ on, onChange, label }: { on: boolean; onChange: (v: boolean) => void; label: string }) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={on}
      aria-label={label}
      onClick={() => onChange(!on)}
      className={cn(
        'relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors',
        on ? 'bg-brand-600' : 'bg-slate-300',
      )}
    >
      <span className={cn('inline-block size-4 transform rounded-full bg-white transition-transform', on ? 'translate-x-6' : 'translate-x-1')} />
    </button>
  );
}

export function ApiAccessScreen(_props: CustomScreenProps) {
  const { user } = useAuth();
  const qc = useQueryClient();
  const toast = useToast();
  const isApiUser = user?.UserTypeID === UserTypeCode.ApiUser;
  const userId = user?.UserID ?? 0;

  // ── API token ──────────────────────────────────────────────────────────────
  const [showToken, setShowToken] = useState(false);
  const [copied, setCopied] = useState(false);
  const [tokenOtp, setTokenOtp] = useState(false);

  const tokenQ = useQuery({
    queryKey: ['api-token'],
    queryFn: () => apiPost<Row>('Users/SelectAPIToken', {}),
    enabled: isApiUser,
  });
  const token = String(get(tokenQ.data?.data ?? {}, 'APIToken') ?? '');

  const copyToken = async () => {
    if (!token) return;
    try {
      await navigator.clipboard.writeText(token);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch { toast.error('Could not copy to clipboard.'); }
  };

  // ── IP security ────────────────────────────────────────────────────────────
  const [ipSecure, setIpSecure] = useState(false);
  const [ips, setIps] = useState('');
  const [ipDirty, setIpDirty] = useState(false);
  const [ipOtp, setIpOtp] = useState(false);

  const ipQ = useQuery({
    queryKey: ['api-secure-ip'],
    queryFn: () => apiPost<Row>('Users/SelectIsSecureIP', {}),
    enabled: isApiUser,
  });
  useEffect(() => {
    if (ipQ.data?.ok && ipQ.data.data) {
      setIpSecure(Boolean(get(ipQ.data.data, 'IsIPSecure')));
      setIps(String(get(ipQ.data.data, 'WhitelistedIPs') ?? ''));
      setIpDirty(false);
    }
  }, [ipQ.data]);

  // ── Callbacks ──────────────────────────────────────────────────────────────
  const [cbService, setCbService] = useState('');
  const [cbUrl, setCbUrl] = useState('');
  const [editId, setEditId] = useState<number | null>(null);

  const servicesQ = useQuery({
    queryKey: ['services-ddl'],
    queryFn: () => fetchList('Service/GetAll', { PageNumber: 1, PageSize: 500, IsActive: true }),
  });
  const serviceName = useMemo(() => {
    const m = new Map<number, string>();
    for (const s of servicesQ.data ?? []) m.set(Number(get(s, 'ServiceID')), String(get(s, 'Name') ?? get(s, 'ServiceName') ?? ''));
    return m;
  }, [servicesQ.data]);

  const callbacksQ = useQuery({
    queryKey: ['callbacks', userId],
    queryFn: () => fetchList('CallbackMaster/GetByUserID', { UserID: userId }),
    enabled: isApiUser && userId > 0,
  });

  const saveCallback = useMutation({
    mutationFn: () => {
      const body = { UserID: userId, ServiceID: Number(cbService), CallbackURL: cbUrl.trim(), IsDelete: false };
      return editId
        ? apiPost('CallbackMaster/Update', { ...body, CallbackMasterID: editId })
        : apiPost('CallbackMaster/Create', body);
    },
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success(editId ? 'Callback updated.' : 'Callback added.');
      setCbService(''); setCbUrl(''); setEditId(null);
      qc.invalidateQueries({ queryKey: ['callbacks', userId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteCallback = useMutation({
    mutationFn: (id: number) => apiPost('CallbackMaster/Delete', { CallbackMasterID: id }),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success('Callback deleted.');
      qc.invalidateQueries({ queryKey: ['callbacks', userId] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const submitCallback = () => {
    if (!cbService) return toast.error('Select a service.');
    if (!/^https?:\/\/.+/i.test(cbUrl.trim())) return toast.error('Enter a valid callback URL (http/https).');
    saveCallback.mutate();
  };

  return (
    <>
      <PageHeader
        title="API Access"
        subtitle="Your API token, IP security and callback URLs"
        icon={KeyRound}
      />

      {!isApiUser && (
        <Card className="mb-5 p-5">
          <EmptyState
            icon={KeyRound}
            title="Available for API-user accounts"
            description="Sign in with an API-user login to view and manage the API token, IP whitelist and callback URLs."
          />
        </Card>
      )}

      <div className="space-y-5">
        {/* API TOKEN */}
        <SectionCard
          icon={KeyRound}
          title="API Token"
          description="Use this token to authenticate your API requests. Keep it secret."
          action={
            <Button variant="outline" size="sm" disabled={!isApiUser} onClick={() => setTokenOtp(true)}>
              <RefreshCw className="size-4" /> Regenerate
            </Button>
          }
        >
          {tokenQ.isFetching ? (
            <div className="flex h-12 items-center"><Spinner className="size-5 text-brand-500" /></div>
          ) : (
            <div className="flex flex-wrap items-center gap-2">
              <code className="min-w-0 flex-1 truncate rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 font-mono text-sm text-slate-800">
                {!isApiUser ? '—' : token ? (showToken ? token : '•'.repeat(Math.min(token.length, 20))) : 'No token yet — regenerate to create one'}
              </code>
              <Button variant="ghost" size="sm" disabled={!token} onClick={() => setShowToken((v) => !v)}>
                {showToken ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
                {showToken ? 'Hide' : 'Show'}
              </Button>
              <Button variant="ghost" size="sm" disabled={!token} onClick={copyToken}>
                {copied ? <Check className="size-4 text-success-600" /> : <Copy className="size-4" />}
                {copied ? 'Copied' : 'Copy'}
              </Button>
            </div>
          )}
        </SectionCard>

        {/* IP SECURITY */}
        <SectionCard
          icon={ShieldCheck}
          title="IP Security"
          description="Restrict API access to specific IP addresses."
          action={
            <div className="flex items-center gap-2">
              <span className="text-sm text-slate-500">{ipSecure ? 'On' : 'Off'}</span>
              <Toggle on={ipSecure} label="IP security" onChange={(v) => { if (!isApiUser) return; setIpSecure(v); setIpDirty(true); }} />
            </div>
          }
        >
          <div className="space-y-3">
            <label className="block text-sm font-medium text-slate-700">
              Whitelisted IPs
              <span className="ml-1 font-normal text-slate-400">(one per line)</span>
            </label>
            <textarea
              rows={4}
              disabled={!isApiUser || !ipSecure}
              value={ips}
              onChange={(e) => { setIps(e.target.value); setIpDirty(true); }}
              placeholder={ipSecure ? '203.0.113.10\n198.51.100.24' : 'Turn on IP security to add addresses'}
              className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 font-mono text-sm text-slate-800 placeholder:text-slate-300 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-100 disabled:bg-slate-50 disabled:text-slate-400"
            />
            <div className="flex items-center justify-between">
              <p className="flex items-center gap-1.5 text-xs text-slate-400">
                <Badge tone={ipSecure ? 'success' : 'neutral'}>{ipSecure ? 'Enforced' : 'Not enforced'}</Badge>
                {ipSecure && 'Only these IPs can call the API.'}
              </p>
              <Button size="sm" disabled={!isApiUser || !ipDirty} onClick={() => setIpOtp(true)}>
                Save changes
              </Button>
            </div>
          </div>
        </SectionCard>

        {/* CALLBACKS */}
        <SectionCard
          icon={Webhook}
          title="Callback URLs"
          description="Where transaction status updates are posted, per service."
        >
          {/* Add / edit row */}
          <div className="mb-4 grid grid-cols-1 gap-3 rounded-lg border border-slate-200 bg-slate-50/60 p-3 sm:grid-cols-[1fr_2fr_auto]">
            <Select value={cbService} disabled={!isApiUser} onChange={(e) => setCbService(e.target.value)}>
              <option value="">Select service</option>
              {(servicesQ.data ?? []).map((s, i) => (
                <option key={i} value={String(get(s, 'ServiceID') ?? '')}>{String(get(s, 'Name') ?? get(s, 'ServiceName') ?? '')}</option>
              ))}
            </Select>
            <Input placeholder="https://your-server.com/callback" disabled={!isApiUser} value={cbUrl} onChange={(e) => setCbUrl(e.target.value)} />
            <div className="flex gap-2">
              {editId && (
                <Button variant="outline" size="sm" onClick={() => { setEditId(null); setCbService(''); setCbUrl(''); }}>Cancel</Button>
              )}
              <Button size="sm" disabled={!isApiUser} loading={saveCallback.isPending} onClick={submitCallback}>
                <Plus className="size-4" /> {editId ? 'Update' : 'Add'}
              </Button>
            </div>
          </div>

          {/* List */}
          {callbacksQ.isFetching ? (
            <div className="flex h-16 items-center justify-center"><Spinner className="size-5 text-brand-500" /></div>
          ) : (callbacksQ.data ?? []).length === 0 ? (
            <EmptyState icon={Link2} title="No callbacks yet" description="Add a callback URL for a service above." />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="border-b border-slate-200 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-3 py-2">Service</th>
                    <th className="px-3 py-2">Callback URL</th>
                    <th className="px-3 py-2 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {(callbacksQ.data ?? []).map((c, i) => {
                    const id = Number(get(c, 'CallbackMasterID'));
                    const sid = Number(get(c, 'ServiceID'));
                    const url = String(get(c, 'CallbackURL') ?? '');
                    return (
                      <tr key={i} className="border-b border-slate-100 last:border-0 hover:bg-slate-50/60">
                        <td className="px-3 py-2 font-medium text-slate-700">{serviceName.get(sid) || `Service ${sid}`}</td>
                        <td className="px-3 py-2"><span className="break-all font-mono text-xs text-slate-600">{url}</span></td>
                        <td className="px-3 py-2 text-right">
                          <div className="flex justify-end gap-1">
                            <Button variant="ghost" size="sm" onClick={() => { setEditId(id); setCbService(String(sid)); setCbUrl(url); }}>Edit</Button>
                            <Button variant="ghost" size="sm" onClick={() => deleteCallback.mutate(id)}>
                              <Trash2 className="size-4 text-danger-500" />
                            </Button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </SectionCard>
      </div>

      {/* OTP gates */}
      <OtpModal
        open={tokenOtp}
        onClose={() => setTokenOtp(false)}
        title="Regenerate API token"
        description="Regenerating replaces your current token immediately — any integration using the old token will stop working. Confirm with the OTP sent to your registered mobile."
        send={() => apiPost('Users/SendAPITokenOTP', {})}
        confirm={(otp) => apiPost('Users/TokenGeneration', { OTP: otp })}
        onDone={() => { toast.success('New API token generated.'); qc.invalidateQueries({ queryKey: ['api-token'] }); setShowToken(true); }}
      />
      <OtpModal
        open={ipOtp}
        onClose={() => setIpOtp(false)}
        title="Update IP security"
        description="Confirm the change with the OTP sent to your registered mobile."
        send={() => apiPost('Users/SendSecureIPOTP', {})}
        confirm={(otp) => apiPost('Users/UpdateIsSecureIP', { IsIPSecure: ipSecure, WhitelistedIPs: ips.trim(), OTP: otp })}
        onDone={() => { toast.success('IP security updated.'); qc.invalidateQueries({ queryKey: ['api-secure-ip'] }); }}
      />
    </>
  );
}
