import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Smartphone, MessageCircle, Mail, MessageSquare } from 'lucide-react';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { pickIcon } from '@/components/layout/iconMap';
import { CrudPanel } from './CrudPanel';
import type { CustomScreenProps } from './registry';
import type { CrudConfig, FieldDef } from './crudTypes';

// ── SMS + WhatsApp gateway (SmsMaster) ─────────────────────────────────────────
// Provider endpoints that deliver SMS / OTP / WhatsApp. URLs carry placeholders the backend
// substitutes at send time (e.g. [MOBILE], [MESSAGE], [OTP]).
const SMS_GATEWAY_FIELDS: FieldDef[] = [
  { name: 'APIName', label: 'Provider name', type: 'text', required: true, colSpan: 2, placeholder: 'e.g. MSG91, TextLocal, Fast2SMS' },
  { name: 'SMSString', label: 'SMS send URL', type: 'text', colSpan: 2, placeholder: 'https://api.provider.com/send?to=[MOBILE]&text=[MESSAGE]' },
  { name: 'SMSOTPURL', label: 'OTP send URL', type: 'text', colSpan: 2, placeholder: 'https://api.provider.com/otp?to=[MOBILE]&otp=[OTP]' },
  { name: 'BalanceURL', label: 'Balance check URL', type: 'text', colSpan: 2 },
  { name: 'WhatsAppURL', label: 'WhatsApp send URL', type: 'text', colSpan: 2, placeholder: 'https://api.provider.com/whatsapp?to=[MOBILE]&text=[MESSAGE]' },
  { name: 'MethodType', label: 'SMS HTTP method', type: 'select', options: [{ label: 'GET', value: 'GET' }, { label: 'POST', value: 'POST' }] },
  { name: 'MethodTypeWhatsApp', label: 'WhatsApp HTTP method', type: 'select', options: [{ label: 'GET', value: 'GET' }, { label: 'POST', value: 'POST' }] },
  { name: 'HeaderJson', label: 'SMS headers (JSON)', type: 'text', colSpan: 2, placeholder: '{"Authorization":"Bearer ..."}' },
  { name: 'HeaderJsonWhatsApp', label: 'WhatsApp headers (JSON)', type: 'text', colSpan: 2 },
  { name: 'MobileWithOrWithout91', label: 'Prefix mobile with 91', type: 'select', options: [{ label: 'No', value: 'No' }, { label: 'Yes', value: 'Yes' }] },
  { name: 'IsOTP', label: 'Supports OTP', type: 'checkbox', defaultValue: false },
  { name: 'IsActive', label: 'Active', type: 'checkbox', defaultValue: true },
];

const SMS_CRUD: CrudConfig = {
  idField: 'SMSMasterID',
  list: { endpoint: 'SmsMaster/GetAll' },
  create: { endpoint: 'SmsMaster/Create', fields: SMS_GATEWAY_FIELDS, successMessage: 'SMS gateway added.' },
  update: { endpoint: 'SmsMaster/Update', mode: 'object', fields: SMS_GATEWAY_FIELDS, successMessage: 'SMS gateway updated.' },
  remove: { endpoint: 'SmsMaster/Delete', successMessage: 'SMS gateway deleted.' },
};

// ── Email / SMTP (EmailSettingMaster) ──────────────────────────────────────────
const EMAIL_FIELDS: FieldDef[] = [
  { name: 'Email', label: 'Sender email', type: 'text', required: true, colSpan: 2, placeholder: 'noreply@yourcompany.com' },
  { name: 'Password', label: 'Password / App password', type: 'password', required: true, colSpan: 2, hint: 'For Gmail/Outlook use an app-specific password' },
  { name: 'HostName', label: 'SMTP host', type: 'text', required: true, placeholder: 'smtp.gmail.com' },
  { name: 'Port', label: 'Port', type: 'number', required: true, defaultValue: 587, placeholder: '587 / 465 / 25' },
  { name: 'IsSSL', label: 'Use SSL / TLS', type: 'checkbox', defaultValue: true },
  { name: 'IsActive', label: 'Active', type: 'checkbox', defaultValue: true },
];

const EMAIL_CRUD: CrudConfig = {
  idField: 'EmailSettingMasterID',
  list: { endpoint: 'EmailSetting/GetAll' },
  // Password is intentionally not shown in the table.
  columns: [
    { key: 'Email', label: 'Sender email' },
    { key: 'HostName', label: 'SMTP host' },
    { key: 'Port', label: 'Port' },
    { key: 'IsSSL', label: 'SSL / TLS' },
    { key: 'IsActive', label: 'Active' },
  ],
  create: { endpoint: 'EmailSetting/Create', fields: EMAIL_FIELDS, successMessage: 'Email settings saved.' },
  update: { endpoint: 'EmailSetting/Update', mode: 'object', fields: EMAIL_FIELDS, successMessage: 'Email settings updated.' },
  remove: { endpoint: 'EmailSetting/Delete', successMessage: 'Email settings deleted.' },
};

type TabKey = 'sms' | 'email';
const TABS: { key: TabKey; label: string; icon: typeof Smartphone }[] = [
  { key: 'sms', label: 'SMS & WhatsApp', icon: Smartphone },
  { key: 'email', label: 'Email (SMTP)', icon: Mail },
];

/**
 * Communication Setup — one place to configure every outbound channel: SMS, WhatsApp and Email.
 * SMS and WhatsApp share the provider-gateway record (SmsMaster); Email uses the SMTP settings
 * (EmailSettingMaster). Per-event message wording lives separately in Message Settings.
 */
export function MessagingSetupScreen({ def }: CustomScreenProps) {
  const [tab, setTab] = useState<TabKey>('sms');

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon(def.title)} />

      {/* Channel overview */}
      <div className="mb-4 grid grid-cols-1 gap-3 sm:grid-cols-3">
        <ChannelCard icon={Smartphone} title="SMS" tone="bg-sky-50 text-sky-600" desc="OTP & transactional SMS via your gateway URL" />
        <ChannelCard icon={MessageCircle} title="WhatsApp" tone="bg-green-50 text-green-600" desc="WhatsApp messages via the same gateway record" />
        <ChannelCard icon={Mail} title="Email" tone="bg-amber-50 text-amber-600" desc="Transactional mail over your SMTP server" />
      </div>

      <div className="mb-4 rounded-card border border-slate-200/70 bg-slate-50/60 px-4 py-3 text-sm text-slate-500">
        Configure the delivery credentials for each channel below. To edit the actual message wording per event
        (recharge, OTP, wallet, KYC…), open{' '}
        <Link to="/s/MessageSetting" className="inline-flex items-center gap-1 font-medium text-brand-600 hover:underline">
          <MessageSquare className="size-3.5" /> Message Settings
        </Link>
        .
      </div>

      {/* Tabs */}
      <div className="mb-4 flex gap-1 border-b border-slate-200">
        {TABS.map((t) => {
          const active = tab === t.key;
          return (
            <button
              key={t.key}
              type="button"
              onClick={() => setTab(t.key)}
              className={cn(
                'flex items-center gap-2 border-b-2 px-4 py-2.5 text-sm font-medium transition-colors',
                active ? 'border-brand-500 text-brand-700' : 'border-transparent text-slate-500 hover:text-slate-700',
              )}
            >
              <t.icon className="size-4" /> {t.label}
            </button>
          );
        })}
      </div>

      {tab === 'sms' ? (
        <CrudPanel label="SMS Gateway" crud={SMS_CRUD} addLabel="Add gateway" />
      ) : (
        <CrudPanel label="Email Settings" crud={EMAIL_CRUD} addLabel="Add SMTP" />
      )}
    </>
  );
}

function ChannelCard({ icon: Icon, title, tone, desc }: { icon: typeof Smartphone; title: string; tone: string; desc: string }) {
  return (
    <div className="flex items-start gap-3 rounded-card border border-slate-200/70 bg-white p-3.5 shadow-card">
      <span className={cn('grid size-9 shrink-0 place-items-center rounded-xl', tone)}>
        <Icon className="size-4.5" />
      </span>
      <div>
        <p className="text-sm font-semibold text-slate-800">{title}</p>
        <p className="mt-0.5 text-xs leading-relaxed text-slate-500">{desc}</p>
      </div>
    </div>
  );
}
