import { useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { MessageSquare, Plus, Pencil, Trash2, Save, Sparkles, Smartphone, Mail, KeyRound, X } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');

/** Placeholders the backend swaps in at send time. Click a chip to insert it into the last-focused box. */
const PLACEHOLDERS = [
  '{AppName}', '{CustomerName}', '{Number}', '{OTP}', '{Amount}', '{Balance}',
  '{TxnID}', '{Operator}', '{Date}', '{SupportNo}', '{Remark}', '{Password}',
];

/** A message-setting row as edited on screen. */
interface Draft {
  MessageSettingID: number;
  Type: string;
  IsSMS: boolean;
  IsOTP: boolean;
  IsMail: boolean;
  IsWhatsUp: boolean;
  SmsMessage: string;
  WhatsUpMessage: string;
  EmailMessage: string;
  TemplateID: string;
}

function toDraft(r: Row): Draft {
  return {
    MessageSettingID: Number(getField(r, 'MessageSettingID') ?? 0),
    Type: str(getField(r, 'Type')),
    IsSMS: Boolean(getField(r, 'IsSMS')),
    IsOTP: Boolean(getField(r, 'IsOTP')),
    IsMail: Boolean(getField(r, 'IsMail')),
    IsWhatsUp: Boolean(getField(r, 'IsWhatsUp')),
    SmsMessage: str(getField(r, 'SmsMessage')),
    WhatsUpMessage: str(getField(r, 'WhatsUpMessage')),
    EmailMessage: str(getField(r, 'EmailMessage')),
    TemplateID: str(getField(r, 'TemplateID')),
  };
}

/** Well-written default templates for every common event. Seeded on demand (admin). */
const DEFAULT_TEMPLATES: Array<Pick<Draft, 'Type' | 'IsSMS' | 'IsOTP' | 'IsWhatsUp' | 'SmsMessage'>> = [
  { Type: 'Welcome / Registration', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Welcome to {AppName}, {CustomerName}! Your account is now active. Enjoy recharges, bill payments & money transfer. Help: {SupportNo}' },
  { Type: 'Registration OTP', IsSMS: true, IsOTP: true, IsWhatsUp: false, SmsMessage: '{OTP} is your OTP to verify your mobile number with {AppName}. Valid for 10 minutes. Never share your OTP with anyone.' },
  { Type: 'Login OTP', IsSMS: true, IsOTP: true, IsWhatsUp: false, SmsMessage: '{OTP} is your {AppName} login OTP. Valid for 10 minutes. Do not share it with anyone for security reasons.' },
  { Type: 'Forgot Password OTP', IsSMS: true, IsOTP: true, IsWhatsUp: false, SmsMessage: '{OTP} is your OTP to reset your {AppName} password. Valid for 10 minutes. Do not share with anyone.' },
  { Type: 'Password Changed', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Your {AppName} password was changed on {Date}. If this was not you, contact support immediately at {SupportNo}.' },
  { Type: 'New Login Credentials', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Welcome to {AppName}! Login ID: {Number}, Password: {Password}. Please change your password after first login.' },
  { Type: 'Recharge Success', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Recharge SUCCESS. {Operator} {Number} recharged with Rs.{Amount}. Txn ID: {TxnID}. Wallet bal: Rs.{Balance}. - {AppName}' },
  { Type: 'Recharge Failed', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Recharge FAILED for {Operator} {Number} of Rs.{Amount}. Amount refunded to your wallet. Txn ID: {TxnID}. - {AppName}' },
  { Type: 'Recharge Pending', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Your recharge of Rs.{Amount} for {Operator} {Number} is PENDING. Txn ID: {TxnID}. We will update you shortly. - {AppName}' },
  { Type: 'Bill Payment Success', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Bill payment SUCCESS. {Operator} bill of Rs.{Amount} paid for {Number}. Txn ID: {TxnID}. Wallet bal: Rs.{Balance}. - {AppName}' },
  { Type: 'Money Transfer Success', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Rs.{Amount} sent successfully to A/c {Number}. UTR/Txn ID: {TxnID}. Wallet bal: Rs.{Balance}. - {AppName}' },
  { Type: 'Money Transfer Failed', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Money transfer of Rs.{Amount} to A/c {Number} FAILED. Amount refunded to your wallet. Txn ID: {TxnID}. - {AppName}' },
  { Type: 'AEPS Withdrawal', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'AEPS cash withdrawal of Rs.{Amount} successful. Txn ID: {TxnID}. Available bal: Rs.{Balance}. - {AppName}' },
  { Type: 'Fund Request Raised', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Your fund request of Rs.{Amount} has been submitted. You will be notified once it is approved. - {AppName}' },
  { Type: 'Fund Request Approved', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Your fund request of Rs.{Amount} is APPROVED. Rs.{Amount} credited to your wallet. New bal: Rs.{Balance}. - {AppName}' },
  { Type: 'Fund Request Rejected', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Your fund request of Rs.{Amount} was REJECTED. Reason: {Remark}. Please contact your upline for details. - {AppName}' },
  { Type: 'Wallet Credited', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Rs.{Amount} credited to your {AppName} wallet on {Date}. New balance: Rs.{Balance}. Ref: {TxnID}.' },
  { Type: 'Wallet Debited', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Rs.{Amount} debited from your {AppName} wallet on {Date}. New balance: Rs.{Balance}. Ref: {TxnID}.' },
  { Type: 'Low Balance Alert', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Low balance alert! Your {AppName} wallet balance is Rs.{Balance}. Please add funds to continue transactions.' },
  { Type: 'Complaint Registered', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Your complaint for Txn ID {TxnID} is registered. We will resolve it within 24-48 hours. - {AppName}' },
  { Type: 'Complaint Resolved', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Your complaint for Txn ID {TxnID} has been RESOLVED. Thank you for your patience. - {AppName}' },
  { Type: 'KYC Approved', IsSMS: true, IsOTP: false, IsWhatsUp: true, SmsMessage: 'Congratulations {CustomerName}! Your KYC is APPROVED. All services are now active on {AppName}.' },
  { Type: 'KYC Rejected', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'Your KYC was REJECTED. Reason: {Remark}. Please re-submit your documents on {AppName}.' },
  { Type: 'New User Added', IsSMS: true, IsOTP: false, IsWhatsUp: false, SmsMessage: 'A new user {CustomerName} ({Number}) has been added under your network on {AppName}.' },
];

const BLANK: Draft = {
  MessageSettingID: 0, Type: '', IsSMS: true, IsOTP: false, IsMail: false, IsWhatsUp: false,
  SmsMessage: '', WhatsUpMessage: '', EmailMessage: '', TemplateID: '',
};

export function MessageSettingScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Draft | null>(null);

  const { data: rows = [], isLoading } = useQuery({
    queryKey: ['message-settings'],
    queryFn: async () => (await fetchList('MessageSetting/GetAll', { PageNumber: 1, PageSize: 500 })).map(toDraft),
  });

  const saveMut = useMutation({
    mutationFn: (d: Draft) => apiPost(d.MessageSettingID > 0 ? 'MessageSetting/Update' : 'MessageSetting/Create', { ...d }),
    onSuccess: (_r, d) => {
      toast.success(`"${d.Type}" ${d.MessageSettingID > 0 ? 'saved' : 'added'}.`);
      setEditing(null);
      setAdding(false);
      qc.invalidateQueries({ queryKey: ['message-settings'] });
    },
    onError: () => toast.error('Could not save this template.'),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => apiPost('MessageSetting/Delete', { MessageSettingID: id }),
    onSuccess: () => { toast.success('Event removed.'); qc.invalidateQueries({ queryKey: ['message-settings'] }); },
    onError: () => toast.error('Could not remove this event.'),
  });

  const createMut = useMutation({
    mutationFn: (d: Partial<Draft>) => apiPost('MessageSetting/Create', d),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['message-settings'] }); },
  });

  // Seed any of the default events that don't exist yet (matched by Type, case-insensitive).
  const [seeding, setSeeding] = useState(false);
  const seedDefaults = async () => {
    const existing = new Set(rows.map((r) => r.Type.trim().toLowerCase()));
    const missing = DEFAULT_TEMPLATES.filter((t) => !existing.has(t.Type.toLowerCase()));
    if (missing.length === 0) { toast.info('All default templates already exist.'); return; }
    setSeeding(true);
    try {
      for (const t of missing) {
        await createMut.mutateAsync({
          Type: t.Type, IsSMS: t.IsSMS, IsOTP: t.IsOTP, IsWhatsUp: t.IsWhatsUp, IsMail: false,
          SmsMessage: t.SmsMessage, WhatsUpMessage: t.IsWhatsUp ? t.SmsMessage : '', EmailMessage: '', TemplateID: '',
        });
      }
      toast.success(`${missing.length} default template${missing.length > 1 ? 's' : ''} added.`);
    } catch {
      toast.error('Some templates could not be added.');
    } finally {
      setSeeding(false);
    }
  };

  return (
    <div className="mx-auto max-w-[1100px] space-y-4">
      <PageHeader
        title="Message Settings"
        subtitle="SMS, WhatsApp & email templates for every event — with on/off switches"
        icon={MessageSquare}
        actions={
          <>
            <Button variant="outline" onClick={seedDefaults} loading={seeding}>
              <Sparkles className="size-4" /> Load default templates
            </Button>
            <Button onClick={() => setAdding(true)}>
              <Plus className="size-4" /> Add event
            </Button>
          </>
        }
      />

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80">
                {['Event', 'Channels', 'SMS message', 'DLT template ID'].map((h) => (
                  <th key={h} className="whitespace-nowrap px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">{h}</th>
                ))}
                <th className="whitespace-nowrap px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                Array.from({ length: 6 }).map((_, i) => (
                  <tr key={i} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 5 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><div className="h-4 w-24 animate-pulse rounded bg-slate-100" /></td>
                    ))}
                  </tr>
                ))
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-4 py-14 text-center">
                    <MessageSquare className="mx-auto size-8 text-slate-300" />
                    <p className="mt-3 text-sm text-slate-500">No message templates yet.</p>
                    <Button className="mt-4" onClick={seedDefaults} loading={seeding}><Sparkles className="size-4" /> Load default templates</Button>
                  </td>
                </tr>
              ) : (
                rows.map((row) => (
                  <tr key={row.MessageSettingID} className="border-b border-slate-100 transition-colors last:border-0 even:bg-slate-50/40 hover:bg-brand-50/40">
                    <td className="whitespace-nowrap px-4 py-3 font-medium text-slate-800">{row.Type}</td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-1">
                        <ChannelTag label="SMS" on={row.IsSMS} />
                        <ChannelTag label="OTP" on={row.IsOTP} />
                        <ChannelTag label="WhatsApp" on={row.IsWhatsUp} />
                        <ChannelTag label="Email" on={row.IsMail} />
                      </div>
                    </td>
                    <td className="max-w-[26rem] px-4 py-3 text-slate-600">
                      <span className="line-clamp-1">{row.SmsMessage || <span className="text-slate-300">—</span>}</span>
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-slate-500">{row.TemplateID || <span className="font-sans text-slate-300">—</span>}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-1.5">
                        <button title="Edit" onClick={() => setEditing(row)} className="grid size-8 place-items-center rounded-lg border border-slate-200 text-slate-500 hover:border-brand-300 hover:text-brand-600"><Pencil className="size-4" /></button>
                        <button title="Remove" onClick={() => { if (confirm(`Remove "${row.Type}"?`)) deleteMut.mutate(row.MessageSettingID); }} className="grid size-8 place-items-center rounded-lg border border-slate-200 text-slate-400 hover:border-danger-300 hover:text-danger-600"><Trash2 className="size-4" /></button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        {!isLoading && rows.length > 0 && (
          <div className="flex items-center justify-between border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
            <span>Showing <span className="font-semibold text-slate-700 tabular-nums">{rows.length}</span> {rows.length === 1 ? 'event' : 'events'}</span>
          </div>
        )}
      </div>

      {(adding || editing) && (
        <EventEditor
          draft={editing ?? { ...BLANK }}
          saving={saveMut.isPending}
          onCancel={() => { setAdding(false); setEditing(null); }}
          onSave={(d) => saveMut.mutate(d)}
        />
      )}
    </div>
  );
}

function ChannelTag({ label, on }: { label: string; on: boolean }) {
  return (
    <span className={cn('inline-flex items-center gap-1 rounded-md border px-1.5 py-0.5 text-[11px] font-medium',
      on ? 'border-brand-200 bg-brand-50 text-brand-700' : 'border-slate-200 bg-white text-slate-400')}>
      <span className={cn('size-1.5 rounded-full', on ? 'bg-success-500' : 'bg-slate-300')} /> {label}
    </span>
  );
}

function Toggle({ icon: Icon, label, on, onClick }: { icon: typeof Smartphone; label: string; on: boolean; onClick: () => void }) {
  return (
    <button type="button" onClick={onClick}
      className={cn('inline-flex items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-xs font-medium transition-colors',
        on ? 'border-brand-300 bg-brand-50 text-brand-700' : 'border-slate-200 bg-white text-slate-400 hover:border-slate-300')}>
      <Icon className="size-3.5" /> {label}
      <span className={cn('ml-0.5 size-1.5 rounded-full', on ? 'bg-success-500' : 'bg-slate-300')} />
    </button>
  );
}

function TemplateBox({ label, value, rowId, field, focusedRef, onChange }: {
  label: string; value: string; rowId: number; field: string;
  focusedRef: React.MutableRefObject<HTMLTextAreaElement | null>; onChange: (v: string) => void;
}) {
  return (
    <div>
      <div className="mb-1 flex items-center justify-between">
        <label className="text-xs font-medium text-slate-500">{label}</label>
        <span className={cn('text-[11px]', value.length > 160 ? 'text-amber-600' : 'text-slate-300')}>{value.length} chars · {Math.max(1, Math.ceil(value.length / 160))} SMS</span>
      </div>
      <textarea
        value={value}
        data-id={rowId}
        data-field={field}
        onFocus={(e) => { focusedRef.current = e.currentTarget; }}
        onChange={(e) => onChange(e.target.value)}
        rows={3}
        className="w-full resize-y rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-brand-400 focus:ring-2 focus:ring-brand-500/15"
      />
    </div>
  );
}

/** Add / edit one message event — all channels, all templates, placeholder inserts. */
function EventEditor({ draft, saving, onCancel, onSave }: { draft: Draft; saving: boolean; onCancel: () => void; onSave: (d: Draft) => void }) {
  const [d, setD] = useState<Draft>(draft);
  const [err, setErr] = useState('');
  const focused = useRef<HTMLTextAreaElement | null>(null);
  const set = (p: Partial<Draft>) => setD((s) => ({ ...s, ...p }));

  const insertPlaceholder = (ph: string) => {
    const el = focused.current;
    const field = (el?.dataset.field as 'SmsMessage' | 'WhatsUpMessage' | 'EmailMessage') ?? 'SmsMessage';
    const cur = d[field];
    const pos = el ? (el.selectionStart ?? cur.length) : cur.length;
    set({ [field]: cur.slice(0, pos) + ph + cur.slice(pos) } as Partial<Draft>);
  };

  const submit = () => {
    if (!d.Type.trim()) { setErr('Event name is required'); return; }
    onSave({ ...d, Type: d.Type.trim() });
  };

  return (
    <Modal open onClose={onCancel} title={d.MessageSettingID > 0 ? `Edit — ${draft.Type}` : 'Add message event'} size="lg">
      <div className="space-y-4">
        <div className="grid gap-3 sm:grid-cols-2">
          <Input label={<>Event name <span className="text-danger-500">*</span></>} value={d.Type} error={err}
            placeholder="e.g. DTH Recharge Success" onChange={(e) => { set({ Type: e.target.value }); setErr(''); }} />
          <div className="flex flex-wrap items-end gap-1.5">
            <Toggle icon={Smartphone} label="SMS" on={d.IsSMS} onClick={() => set({ IsSMS: !d.IsSMS })} />
            <Toggle icon={KeyRound} label="OTP" on={d.IsOTP} onClick={() => set({ IsOTP: !d.IsOTP })} />
            <Toggle icon={MessageSquare} label="WhatsApp" on={d.IsWhatsUp} onClick={() => set({ IsWhatsUp: !d.IsWhatsUp })} />
            <Toggle icon={Mail} label="Email" on={d.IsMail} onClick={() => set({ IsMail: !d.IsMail })} />
          </div>
        </div>

        <TemplateBox label="SMS message" value={d.SmsMessage} rowId={d.MessageSettingID} field="SmsMessage" focusedRef={focused} onChange={(v) => set({ SmsMessage: v })} />
        <TemplateBox label="WhatsApp message" value={d.WhatsUpMessage} rowId={d.MessageSettingID} field="WhatsUpMessage" focusedRef={focused} onChange={(v) => set({ WhatsUpMessage: v })} />
        <TemplateBox label="Email message" value={d.EmailMessage} rowId={d.MessageSettingID} field="EmailMessage" focusedRef={focused} onChange={(v) => set({ EmailMessage: v })} />

        <div>
          <p className="mb-1.5 text-xs font-medium text-slate-500">Insert placeholder into the last-focused box</p>
          <div className="flex flex-wrap gap-1">
            {PLACEHOLDERS.map((p) => (
              <button key={p} type="button" onClick={() => insertPlaceholder(p)}
                className="rounded border border-slate-200 bg-white px-1.5 py-0.5 font-mono text-[11px] text-slate-500 hover:border-brand-300 hover:text-brand-600">{p}</button>
            ))}
          </div>
        </div>

        <Input label="DLT template ID" value={d.TemplateID} placeholder="Registered DLT template id (optional)" onChange={(e) => set({ TemplateID: e.target.value })} />

        <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
          <Button variant="outline" onClick={onCancel} disabled={saving}><X className="size-4" /> Cancel</Button>
          <Button onClick={submit} loading={saving}><Save className="size-4" /> {d.MessageSettingID > 0 ? 'Save changes' : 'Add event'}</Button>
        </div>
      </div>
    </Modal>
  );
}
