import type { ReactNode, ReactElement } from 'react';
import { Children, isValidElement, useMemo, useState } from 'react';
import type { LucideIcon } from 'lucide-react';
import { Building2, Fingerprint, MapPin, Network, ShieldCheck, Wallet } from 'lucide-react';
import { cn } from '@/lib/cn';
import { inr, dateOnly, dateTime } from '@/lib/format';
import { roleLabel } from '@/lib/roles';
import { Badge } from '@/components/ui/Badge';
import { StatRow } from '@/components/report/StatRow';
import type { UserRow } from '@/api/users.api';

/**
 * Shared furniture for the two user-lifecycle screens — Inactive Users and Deleted Users.
 * They ask Users/Select the same question with a different filter and show the same record,
 * so the KPI strip, the role tabs and the full-detail panel live here once.
 */

// ── Small building blocks ──────────────────────────────────────────────────────

/**
 * The user-lifecycle screens' figures bar.
 *
 * `Kpi` now just describes one metric; `KpiStrip` renders them through the shared `StatRow`, so
 * these screens carry the same compact bar as every report instead of their own taller strip with
 * a 32px icon chip per metric. Callers are unchanged — they still write
 * `<KpiStrip><Kpi .../><Kpi .../></KpiStrip>`.
 */
export function Kpi(_: { label: string; value: string; tone?: string; icon: ReactNode }) {
  // Never rendered directly — KpiStrip reads these props off its children. Declared as a component
  // so the call sites stay readable and type-checked.
  return null;
}

interface KpiProps { label: string; value: string; tone?: string; icon?: ReactNode }

export function KpiStrip({ children, columns = 4 }: { children: ReactNode; columns?: 4 | 5 }) {
  const items = Children.toArray(children)
    .filter(isValidElement)
    .map((child) => {
      const p = (child as ReactElement<KpiProps>).props;
      return { label: p.label, value: p.value, tone: p.tone, icon: p.icon };
    });

  return <StatRow items={items} columns={columns} />;
}

/** Role tabs shared by both screens. `counts` is keyed by UserTypeID; 0 = everyone. */
export const ROLE_TABS: { key: string; label: string; type: number | 'all' }[] = [
  { key: 'all', label: 'All roles', type: 'all' },
  { key: 'retailer', label: 'Retailer', type: 4 },
  { key: 'dist', label: 'Distributor', type: 3 },
  { key: 'superdist', label: 'Super Distributor', type: 2 },
  { key: 'api', label: 'API User', type: 5 },
];

export function RoleTabs({
  value, onChange, countFor,
}: { value: string; onChange: (key: string) => void; countFor: (type: number | 'all') => number }) {
  return (
    <div className="mb-4 flex flex-wrap gap-1 border-b border-slate-200">
      {ROLE_TABS.map((t) => (
        <button
          key={t.key}
          type="button"
          onClick={() => onChange(t.key)}
          className={cn(
            '-mb-px cursor-pointer border-b-2 px-4 py-2 text-sm font-medium transition-colors',
            value === t.key ? 'border-brand-600 text-brand-700' : 'border-transparent text-slate-500 hover:text-slate-700',
          )}
        >
          {t.label} <span className="tabular-nums text-slate-400">({countFor(t.type)})</span>
        </button>
      ))}
    </div>
  );
}

/** Filter rows to the selected role tab. */
export function useRoleFilter(rows: UserRow[]) {
  const [tab, setTab] = useState('all');
  const filtered = useMemo(() => {
    const t = ROLE_TABS.find((x) => x.key === tab);
    if (!t || t.type === 'all') return rows;
    return rows.filter((r) => r.UserTypeID === t.type);
  }, [rows, tab]);
  const countFor = (type: number | 'all') =>
    type === 'all' ? rows.length : rows.filter((r) => r.UserTypeID === type).length;
  return { tab, setTab, filtered, countFor };
}

// ── Detail panel ───────────────────────────────────────────────────────────────

function Field({ label, value, mono }: { label: string; value?: string | null; mono?: boolean }) {
  const shown = (value ?? '').toString().trim();
  return (
    <div className="min-w-0">
      <div className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{label}</div>
      <div className={cn('mt-0.5 break-words text-sm text-slate-800', mono && 'font-mono text-[13px]')}>
        {shown || <span className="text-slate-300">—</span>}
      </div>
    </div>
  );
}

function Section({ title, icon: Icon, children }: { title: string; icon: LucideIcon; children: ReactNode }) {
  return (
    <section className="rounded-xl border border-slate-200/80 bg-slate-50/40 p-4">
      <h4 className="mb-3 flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
        <Icon className="size-3.5 text-slate-400" />
        {title}
      </h4>
      {children}
    </section>
  );
}

/** Coloured pill for the derived KYC state. */
export function KycBadge({ status, ekyc }: { status: string; ekyc?: boolean }) {
  const s = (status || '').toLowerCase();
  const tone = s.startsWith('approved') ? 'success' : s.startsWith('rejected') ? 'danger' : s.startsWith('pending') ? 'warning' : 'neutral';
  return (
    <span className="inline-flex items-center gap-1.5">
      <Badge tone={tone}>{status || '—'}</Badge>
      {ekyc && <Badge tone="info">E-KYC</Badge>}
    </span>
  );
}

/**
 * The whole user record, laid out for reading. Both lifecycle screens open this — "show the
 * user's every detail" is the same job whether they are deactivated or deleted.
 */
export function UserDetailPanel({ user }: { user: UserRow }) {
  const role = user.UserType || roleLabel(user.UserTypeID) || '—';
  const wallets = user.WalletBalances ?? [];
  const totalBalance = wallets.reduce((sum, w) => sum + (w.Balance ?? 0), 0);
  const totalHold = wallets.reduce((sum, w) => sum + (w.HoldBalance ?? 0), 0);

  return (
    <div className="space-y-4">
      {/* Identity header */}
      <div className="flex flex-col gap-3 rounded-xl border border-slate-200/80 bg-white p-4 sm:flex-row sm:items-center">
        <span className="grid size-12 shrink-0 place-items-center rounded-2xl bg-brand-600 font-display text-xl font-extrabold text-white">
          {(user.FirmName || user.PersonName || 'U').trim().slice(0, 1).toUpperCase()}
        </span>
        <div className="min-w-0 flex-1">
          <p className="truncate font-display text-lg font-bold text-slate-900">{user.PersonName || '—'}</p>
          <p className="truncate text-sm text-slate-500">{user.FirmName || '—'}</p>
        </div>
        <div className="flex flex-wrap items-center gap-1.5">
          <Badge tone="brand">{role}</Badge>
          {user.IsDeleted ? (
            <Badge tone="danger">Deleted</Badge>
          ) : (
            <Badge tone={user.IsActive ? 'success' : 'danger'}>{user.IsActive ? 'Active' : 'Inactive'}</Badge>
          )}
          <KycBadge status={user.KYCStatus} ekyc={user.IseKYC} />
          {user.IsCommissionHold && <Badge tone="neutral">Commission hold</Badge>}
        </div>
      </div>

      <div className="grid gap-3 lg:grid-cols-2">
        <Section title="Contact" icon={Building2}>
          <div className="grid grid-cols-2 gap-x-4 gap-y-3">
            <Field label="Mobile" value={user.MobileNumber} />
            <Field label="WhatsApp" value={user.WhatsAppNumber} />
            <Field label="Email" value={user.EmailID} />
            <Field label="Alias" value={user.AliasName} />
          </div>
        </Section>

        <Section title="Network" icon={Network}>
          <div className="grid grid-cols-2 gap-x-4 gap-y-3">
            <Field label="Mapped under" value={user.ParentName} />
            <Field label="Upline mobile" value={user.ParentMobile} />
            <Field label="Commission slab" value={user.SlabName} />
            <Field label="Reference no." value={user.UniqueReferenceNo} mono />
          </div>
        </Section>

        <Section title="Address" icon={MapPin}>
          <div className="grid grid-cols-2 gap-x-4 gap-y-3">
            <Field label="Address" value={user.Address} />
            <Field label="Shop address" value={user.ShopAddress} />
            <Field label="City" value={user.CityName} />
            <Field label="State" value={user.StateName} />
            <Field label="Pincode" value={user.Pincode} />
          </div>
        </Section>

        <Section title="Statutory IDs" icon={Fingerprint}>
          <div className="grid grid-cols-2 gap-x-4 gap-y-3">
            <Field label="PAN" value={user.PANNo} mono />
            <Field label="Aadhaar" value={user.AadharNo} mono />
            <Field label="GST" value={user.GSTNo} mono />
          </div>
        </Section>

        <Section title="Wallets" icon={Wallet}>
          {wallets.length === 0 ? (
            <p className="text-sm text-slate-400">No wallets mapped.</p>
          ) : (
            <div className="space-y-1.5">
              {wallets.map((w) => (
                <div key={w.WalletID} className="flex items-baseline justify-between gap-3 text-sm">
                  <span className="truncate text-slate-600">{w.WalletName}</span>
                  <span className="shrink-0 text-right">
                    <span className="font-medium tabular-nums text-slate-800">{inr(w.Balance)}</span>
                    {w.HoldBalance > 0 && (
                      <span className="ml-2 text-[11px] tabular-nums text-slate-400">{inr(w.HoldBalance)} hold</span>
                    )}
                  </span>
                </div>
              ))}
              <div className="mt-2 flex items-baseline justify-between gap-3 border-t border-slate-200 pt-2 text-sm">
                <span className="font-medium text-slate-700">Total</span>
                <span className="font-semibold tabular-nums text-slate-900">{inr(totalBalance)}</span>
              </div>
              {totalHold > 0 && (
                <p className="text-[11px] text-slate-400">{inr(totalHold)} of that is on hold.</p>
              )}
            </div>
          )}
        </Section>

        <Section title="Account & security" icon={ShieldCheck}>
          <div className="grid grid-cols-2 gap-x-4 gap-y-3">
            <Field label="Registered on" value={dateTime(user.DOC)} />
            <Field label="Last updated" value={dateTime(user.LastUpdated)} />
            <Field label="KYC" value={user.KYCStatus} />
            <Field label="Verification mode" value={user.IseKYC ? 'E-KYC' : 'Manual'} />
            <Field label="Two-factor" value={user.IsTwoFA ? 'On' : 'Off'} />
            <Field label="Login OTP" value={user.IsLoginOTP ? 'On' : 'Off'} />
          </div>
        </Section>
      </div>
    </div>
  );
}

/** Flatten a user row for CSV export — shared so both screens export the same shape. */
export function userExportRow(user: UserRow): Record<string, unknown> {
  const wallets: Record<string, unknown> = {};
  for (const w of user.WalletBalances ?? []) wallets[w.WalletName] = w.Balance;
  return {
    Name: user.PersonName,
    Firm: user.FirmName,
    Mobile: user.MobileNumber,
    WhatsApp: user.WhatsAppNumber,
    Email: user.EmailID,
    Role: user.UserType || roleLabel(user.UserTypeID),
    'Mapped under': user.ParentName,
    Slab: user.SlabName,
    City: user.CityName,
    State: user.StateName,
    PAN: user.PANNo,
    Aadhaar: user.AadharNo,
    GST: user.GSTNo,
    KYC: user.KYCStatus,
    ...wallets,
    Registered: dateOnly(user.DOC),
    'Last updated': dateOnly(user.LastUpdated),
    Status: user.IsDeleted ? 'Deleted' : user.IsActive ? 'Active' : 'Inactive',
  };
}
