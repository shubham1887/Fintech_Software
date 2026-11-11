import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Building2, Landmark, MapPin, PenLine, Save } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { UserTypeCode } from '@/lib/roles';
import { useAuth } from '@/features/auth/AuthContext';
import {
  GSTIN_PATTERN, GST_STATES, IFSC_PATTERN, PAN_PATTERN,
  gstStateByName, gstStateLabel, panFromGstin, stateCodeFromGstin,
} from '@/lib/gstStates';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Spinner } from '@/components/ui/Spinner';
import { ImageField } from '@/components/ui/DynamicForm';
import { pickIcon } from '@/components/layout/iconMap';
import { INVOICE_TEMPLATES, INVOICE_TEMPLATE_KEY, useInvoiceTemplate } from '@/lib/invoiceTemplate';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

/**
 * The company's own profile: who we are on an invoice, and under which registration.
 *
 * This screen used to be a read-only list, which left the company GSTIN with no way in — and
 * with no GSTIN, `60_seed_document_series.sql` seeds nothing, so no document of any kind can be
 * issued. The tax engine and the simulator work without this row; the document side does not
 * exist without it.
 *
 * TWO THINGS TO KNOW BEFORE EDITING THIS FILE
 *
 *  1. Every one of the 23 columns on the live `dbo.InvoiceSettings` is NOT NULL and none of them
 *     carries a default, so a blank field has to go up as "" and never as null. The payload below
 *     is built from a record whose values are always strings for exactly that reason — see
 *     `payload()`. Sending null gives "Cannot insert the value NULL into column 'IsDeleted'"-class
 *     failures that pass every test on dev, where the defaults do exist.
 *
 *  2. The first two digits of a GSTIN ARE the state code. GST issues the registration per state
 *     and stamps the state into the number, so the State picked here must agree with them. If it
 *     does not, every intra-state supply is treated as inter-state (or the reverse) and the tax
 *     goes to the wrong head — on a document that looks perfectly valid. That check is the one
 *     thing on this form that blocks a save on a well-formed-looking number.
 */

/** The 23 writable columns, in the order the API's Create/Update requests declare them. */
const FIELD_NAMES = [
  'CompanyName', 'LogoURL', 'CompanyPrefix', 'Address', 'ShortAddress', 'PinCode', 'District',
  'Country', 'GSTIN', 'PAN', 'CIN', 'State', 'AccountNo', 'AccountType', 'IFSC',
  'AccountHolderName', 'BankName', 'BranchName', 'Email', 'WebsiteName', 'Note', 'FullAddress',
  'SignatureURL',
] as const;

type FieldName = (typeof FIELD_NAMES)[number];
type Profile = Record<FieldName, string>;

/** Fields the registration authorities issue in capitals — typed in any case, stored in caps. */
const UPPERCASE: FieldName[] = ['GSTIN', 'PAN', 'CIN', 'IFSC'];

function blankProfile(): Profile {
  const p = {} as Profile;
  for (const n of FIELD_NAMES) p[n] = '';
  p.Country = 'India';
  return p;
}

function profileFromRow(row: Row): Profile {
  const p = blankProfile();
  for (const n of FIELD_NAMES) p[n] = String(getField(row, n) ?? '');
  return p;
}

/** India's financial year runs April to March — the same arithmetic the seed script does. */
function financialYear(today = new Date()): { label: string; code: string } {
  const start = today.getMonth() + 1 >= 4 ? today.getFullYear() : today.getFullYear() - 1;
  const two = (y: number) => String(y % 100).padStart(2, '0');
  return { label: `${start}-${two(start + 1)}`, code: `${two(start)}${two(start + 1)}` };
}

/** What `60_seed_document_series.sql` opens once a 15-character GSTIN is saved. */
const SERIES = [
  ['SALE-TAX', 'Tax invoice', 'SI/'],
  ['SALE-BOS', 'Bill of supply', 'BS/'],
  ['SALE-CRN', 'Credit note', 'CN/'],
  ['SALE-DBN', 'Debit note', 'DN/'],
  ['SELF-INV', 'Self-invoice (RCM)', 'SV/'],
];

const ACCOUNT_TYPES = ['Savings', 'Current', 'Overdraft', 'Cash Credit'];

interface Checks {
  errors: Partial<Record<FieldName, string>>;
  warnings: string[];
}

/**
 * Blocking errors are only the ones that make the record unusable or actively wrong: a missing
 * company name or prefix, a GSTIN that cannot carry a document series, and a GSTIN that
 * contradicts the state. Everything else is a warning — the person doing this install is not an
 * accountant, and refusing to save a profile over a malformed CIN would leave them with no
 * document series at all, which is the worse outcome.
 */
function validate(p: Profile): Checks {
  const errors: Checks['errors'] = {};
  const warnings: string[] = [];

  if (!p.CompanyName.trim()) errors.CompanyName = 'The company name prints on every invoice.';
  if (!p.CompanyPrefix.trim()) errors.CompanyPrefix = 'Required — it prefixes the legacy invoice numbers.';

  const gstin = p.GSTIN.trim().toUpperCase();
  const state = gstStateByName(p.State);
  if (!gstin) {
    errors.GSTIN = 'Required — without it no document series exists and nothing can be invoiced.';
  } else if (gstin.length !== 15) {
    errors.GSTIN = `A GSTIN is exactly 15 characters; this one is ${gstin.length}.`;
  }
  if (!p.State.trim()) {
    errors.State = 'Required — it decides whether a supply is intra-state or inter-state.';
  }

  const gstinStateCode = stateCodeFromGstin(gstin);
  if (gstinStateCode && state && gstinStateCode !== state.code) {
    errors.GSTIN =
      `This GSTIN belongs to state ${gstinStateCode}, but the state selected is ` +
      `${state.name} (${state.code}). One of the two is wrong.`;
  } else if (gstin.length === 15 && !gstinStateCode) {
    errors.GSTIN = 'A GSTIN starts with the two-digit state code.';
  } else if (gstinStateCode && !state && p.State.trim()) {
    warnings.push(
      `The state "${p.State}" is not one of the GST states, so the GSTIN's state code ` +
      `(${gstinStateCode}) could not be checked against it.`,
    );
  }

  if (gstin.length === 15 && !GSTIN_PATTERN.test(gstin)) {
    warnings.push(
      'The GSTIN is 15 characters but does not follow the usual pattern for a regular taxpayer ' +
      '(state code, PAN, entity number, Z, checksum). A TCS or non-resident registration can ' +
      'look like this legitimately — check it against the certificate.',
    );
  }

  const pan = p.PAN.trim().toUpperCase();
  if (pan && !PAN_PATTERN.test(pan)) warnings.push('The PAN does not look like a PAN (AAAAA9999A).');
  const panInGstin = panFromGstin(gstin);
  if (pan && panInGstin && PAN_PATTERN.test(pan) && panInGstin !== pan) {
    warnings.push(
      `The GSTIN carries PAN ${panInGstin} in characters 3-12, but the PAN field says ${pan}. ` +
      'A PAN-based registration always embeds its own PAN.',
    );
  }

  const ifsc = p.IFSC.trim().toUpperCase();
  if (ifsc && !IFSC_PATTERN.test(ifsc)) warnings.push('The IFSC does not look like an IFSC (AAAA0999999).');

  const pin = p.PinCode.trim();
  if (pin && !/^[0-9]{6}$/.test(pin)) warnings.push('A PIN code is six digits.');

  const email = p.Email.trim();
  if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) warnings.push('The e-mail address does not look valid.');

  return { errors, warnings };
}

/** A labelled note block — the amber one carries a warning, the slate one plain guidance. */
function Note({ tone, children }: { tone: 'warn' | 'info'; children: ReactNode }) {
  const warn = tone === 'warn';
  return (
    <div
      className={
        warn
          ? 'flex gap-2.5 rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-xs text-warning-600'
          : 'rounded-lg border border-slate-200 bg-slate-50 p-3 text-xs text-slate-500'
      }
    >
      {warn && <AlertTriangle className="mt-0.5 size-4 shrink-0" />}
      <div className="space-y-1">{children}</div>
    </div>
  );
}

export function InvoiceSettingsScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();
  const { user } = useAuth();
  // Create / Update / Delete on this controller match ApiAccessPolicy's WriteVerb regex and the
  // controller is not self-service, so they are Admin + Support. The accountant reads it (the
  // controller is granted to the Accounting set) and must not be shown a Save it cannot use.
  const roleId = user?.UserTypeID ?? 0;
  const canWrite = roleId === UserTypeCode.Admin || roleId === UserTypeCode.SupportUser;

  const { data: rows = [], isPending } = useQuery({
    queryKey: ['crud', 'InvoiceSetting/GetAll'],
    queryFn: () => fetchList('InvoiceSetting/GetAll', { PageNumber: 1, PageSize: 50 }),
  });

  // The seed script reads TOP 1 ... ORDER BY InvoiceSettingID, so the LOWEST id is the company
  // profile that matters. This screen edits that same row rather than whichever came back first.
  const existing = useMemo(() => {
    if (rows.length === 0) return null;
    return [...rows].sort((a, b) => Number(getField(a, 'InvoiceSettingID') ?? 0) - Number(getField(b, 'InvoiceSettingID') ?? 0))[0];
  }, [rows]);
  const existingId = existing ? Number(getField(existing, 'InvoiceSettingID') ?? 0) : 0;

  const [profile, setProfile] = useState<Profile>(blankProfile);
  const [touched, setTouched] = useState(false);
  const [showErrors, setShowErrors] = useState(false);

  // Load the saved row once it arrives, unless the admin has already started typing.
  useEffect(() => {
    if (existing && !touched) setProfile(profileFromRow(existing));
  }, [existing, touched]);

  const set = (name: FieldName, value: string) => {
    setTouched(true);
    setProfile((p) => ({ ...p, [name]: UPPERCASE.includes(name) ? value.toUpperCase() : value }));
  };

  const { errors, warnings } = validate(profile);
  const errorCount = Object.keys(errors).length;
  const fy = financialYear();
  const derivedStateCode = stateCodeFromGstin(profile.GSTIN);

  /**
   * Every column, always a string. The live table has no defaults on any of them, so a field
   * the admin left blank must travel as "" — omitting it or sending null is what produces a
   * NOT NULL insert failure on a server where the dev defaults are absent.
   */
  const payload = (): Record<string, unknown> => {
    const out: Record<string, unknown> = {};
    for (const n of FIELD_NAMES) out[n] = profile[n].trim();
    if (existingId > 0) out.InvoiceSettingID = existingId;
    return out;
  };

  const save = useMutation({
    mutationFn: () => apiPost(existingId > 0 ? 'InvoiceSetting/Update' : 'InvoiceSetting/Create', payload()),
    onSuccess: (res) => {
      if (!res.ok) {
        toast.error(res.message);
        return;
      }
      toast.success(existingId > 0 ? 'Company profile updated.' : 'Company profile saved.');
      setTouched(false);
      qc.invalidateQueries({ queryKey: ['crud', 'InvoiceSetting/GetAll'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const submit = () => {
    if (errorCount > 0) {
      setShowErrors(true);
      toast.error('Fix the highlighted fields first.');
      return;
    }
    save.mutate();
  };

  /** One text field. `max` is the live column width — a longer value is a truncation error. */
  const txt = (
    name: FieldName,
    label: string,
    max: number,
    opts: { hint?: string; placeholder?: string; span?: boolean; mono?: boolean; required?: boolean } = {},
  ) => (
    <div className={opts.span ? 'sm:col-span-2' : undefined}>
      <Input
        label={
          opts.required ? (
            <>
              {label} <span className="text-danger-500">*</span>
            </>
          ) : (
            label
          )
        }
        value={profile[name]}
        maxLength={max}
        placeholder={opts.placeholder}
        hint={opts.hint}
        error={showErrors ? errors[name] : undefined}
        disabled={!canWrite}
        className={opts.mono ? 'font-mono tracking-tight' : undefined}
        onChange={(e) => set(name, e.target.value)}
      />
    </div>
  );

  if (isPending) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Spinner className="size-6 text-brand-500" />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="The company's own identity on an invoice — registration, address, bank and signature"
        icon={pickIcon(def.title)}
        actions={
          canWrite && (
            <Button onClick={submit} loading={save.isPending}>
              <Save className="mr-1.5 size-4" />
              {existingId > 0 ? 'Save changes' : 'Save profile'}
            </Button>
          )
        }
      />

      {!canWrite && (
        <Note tone="info">
          <p>
            You can read the company profile but not change it. Creating or editing it is limited to
            Admin and Support, because the GSTIN saved here is stamped into every document number
            that is allocated afterwards.
          </p>
        </Note>
      )}

      {rows.length > 1 && (
        <Note tone="warn">
          <p>
            <strong>{rows.length} invoice-settings rows exist.</strong> Document numbering uses the
            one with the lowest id (#{existingId}), which is the row this form edits. The extra rows
            are ignored — delete them from the database to avoid confusion.
          </p>
        </Note>
      )}

      {existingId === 0 && (
        <Note tone="warn">
          <p>
            <strong>No company profile exists yet.</strong> Until a 15-character GSTIN is saved here,
            the document series cannot be created and no invoice, bill of supply, credit note or
            self-invoice can be issued. The tax engine and the simulator work regardless — it is the
            document side that is inert.
          </p>
        </Note>
      )}

      {showErrors && errorCount > 0 && (
        <Note tone="warn">
          <p>
            <strong>{errorCount === 1 ? '1 field needs fixing' : `${errorCount} fields need fixing`}</strong>{' '}
            before this can be saved.
          </p>
        </Note>
      )}

      {warnings.length > 0 && (
        <Note tone="warn">
          <p>
            <strong>Worth checking — these do not block a save.</strong>
          </p>
          <ul className="list-disc space-y-0.5 pl-4">
            {warnings.map((w) => (
              <li key={w}>{w}</li>
            ))}
          </ul>
        </Note>
      )}

      <div className="grid gap-4 lg:grid-cols-2">
        {/* ── Registration: the fields the document side depends on ─────────────── */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="size-4 text-brand-600" />
              Registration
            </CardTitle>
            {derivedStateCode && (
              <span className="rounded-md bg-brand-50 px-2 py-1 font-mono text-xs font-semibold text-brand-700">
                supplier state {derivedStateCode}
              </span>
            )}
          </CardHeader>
          <CardBody className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {txt('CompanyName', 'Registered company name', 500, {
              required: true,
              span: true,
              hint: 'Exactly as it appears on the GST registration certificate.',
            })}

            {txt('GSTIN', 'GSTIN', 15, {
              required: true,
              mono: true,
              placeholder: '27AAAAA0000A1Z5',
              hint: '15 characters. The first two digits must match the state below.',
            })}

            <div>
              <Select
                label={
                  <>
                    State <span className="text-danger-500">*</span>
                  </>
                }
                value={profile.State}
                error={showErrors ? errors.State : undefined}
                disabled={!canWrite}
                onChange={(e) => set('State', e.target.value)}
              >
                <option value="">Select…</option>
                {/* A state name an older install stored that is not in the GST list is kept as an
                    option of its own, so opening this form never silently drops it. */}
                {profile.State && !gstStateByName(profile.State) && (
                  <option value={profile.State}>{profile.State} (not a GST state)</option>
                )}
                {GST_STATES.map((s) => (
                  <option key={s.code} value={s.name}>
                    {gstStateLabel(s)}
                  </option>
                ))}
              </Select>
              <p className="mt-1 text-xs text-slate-400">
                Decides CGST + SGST versus IGST on every supply.
              </p>
            </div>

            {txt('PAN', 'PAN', 10, { mono: true, placeholder: 'AAAAA0000A' })}
            {txt('CIN', 'CIN', 25, { mono: true, hint: 'Companies only — leave blank for a firm or proprietorship.' })}

            {txt('CompanyPrefix', 'Company prefix', 10, {
              required: true,
              hint: 'Prefixes the legacy invoice numbers. Keep it short — 2 to 4 characters.',
            })}
            {txt('Email', 'E-mail', 100, { placeholder: 'accounts@example.com' })}
            {txt('WebsiteName', 'Website', 100, { placeholder: 'www.example.com' })}
          </CardBody>
        </Card>

        {/* ── Address ───────────────────────────────────────────────────────────── */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MapPin className="size-4 text-brand-600" />
              Address
            </CardTitle>
          </CardHeader>
          <CardBody className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {txt('Address', 'Address', 500, { span: true, hint: 'The principal place of business on the certificate.' })}
            {txt('ShortAddress', 'Short address', 200, { hint: 'Used where space is tight.' })}
            {txt('District', 'District', 100)}
            {txt('PinCode', 'PIN code', 6, { placeholder: '400001' })}
            {txt('Country', 'Country', 100)}
            {txt('FullAddress', 'Full address block', 500, {
              span: true,
              hint: 'The complete block as it should print on the invoice.',
            })}
          </CardBody>
        </Card>

        {/* ── Bank ──────────────────────────────────────────────────────────────── */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Landmark className="size-4 text-brand-600" />
              Bank details
            </CardTitle>
          </CardHeader>
          <CardBody className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {txt('AccountHolderName', 'Account holder', 100)}
            {txt('AccountNo', 'Account number', 100, { mono: true })}
            <div>
              <Select
                label="Account type"
                value={profile.AccountType}
                disabled={!canWrite}
                onChange={(e) => set('AccountType', e.target.value)}
              >
                <option value="">Select…</option>
                {profile.AccountType && !ACCOUNT_TYPES.includes(profile.AccountType) && (
                  <option value={profile.AccountType}>{profile.AccountType}</option>
                )}
                {ACCOUNT_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </Select>
            </div>
            {txt('IFSC', 'IFSC', 30, { mono: true, placeholder: 'HDFC0000123' })}
            {txt('BankName', 'Bank', 200)}
            {txt('BranchName', 'Branch', 200)}
          </CardBody>
        </Card>

        {/* ── What prints on the document ───────────────────────────────────────── */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <PenLine className="size-4 text-brand-600" />
              On the printed document
            </CardTitle>
          </CardHeader>
          <CardBody className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <ImageField
              field={{ name: 'LogoURL', label: 'Logo', hint: 'Shown in the invoice header.' }}
              value={profile.LogoURL}
              onChange={(v) => set('LogoURL', v)}
            />
            <ImageField
              field={{ name: 'SignatureURL', label: 'Authorised signature', hint: 'Printed above the signatory line.' }}
              value={profile.SignatureURL}
              onChange={(v) => set('SignatureURL', v)}
            />
            {txt('Note', 'Footer note', 500, {
              span: true,
              hint: 'Terms, declaration or jurisdiction line printed at the foot of the invoice.',
            })}
          </CardBody>
        </Card>
      </div>

      <InvoiceTemplateCard canWrite={canWrite} />

      {/* ── The step after this one ───────────────────────────────────────────────── */}
      <Card>
        <CardHeader>
          <CardTitle>Document series</CardTitle>
          <span className="text-xs text-slate-400">FY {fy.label} · stored as {fy.code}</span>
        </CardHeader>
        <CardBody className="space-y-3">
          <p className="text-sm text-slate-500">
            Saving a 15-character GSTIN does not by itself create the numbering series. Run the seed
            script once afterwards — it is idempotent, so running it again changes nothing — and these
            five open for the current financial year under that GSTIN:
          </p>
          <div className="overflow-hidden rounded-lg border border-slate-200">
            <table className="w-full text-sm">
              <tbody className="divide-y divide-slate-100">
                {SERIES.map(([code, label, prefix]) => (
                  <tr key={code}>
                    <td className="px-3 py-2 font-mono text-xs text-slate-500">{code}</td>
                    <td className="px-3 py-2 text-slate-700">{label}</td>
                    <td className="px-3 py-2 font-mono text-xs text-slate-800">
                      {prefix}
                      {fy.code}/00001
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Note tone="info">
            <p className="font-mono">
              sqlcmd -S &lt;server&gt; -d &lt;database&gt; -E -C -i
              Fino_API\Scripts\install\60_seed_document_series.sql
            </p>
            <p>
              A series for the NEXT financial year has to be opened before 1 April or invoicing stops
              that morning. The script deliberately opens the current year only — an unused open
              series still appears in GSTR-1&apos;s &ldquo;Documents Issued&rdquo; table.
            </p>
          </Note>
        </CardBody>
      </Card>

      {canWrite && (
        <div className="flex justify-end">
          <Button onClick={submit} loading={save.isPending}>
            <Save className="mr-1.5 size-4" />
            {existingId > 0 ? 'Save changes' : 'Save profile'}
          </Button>
        </div>
      )}
    </div>
  );
}

/**
 * Which layout the bills print in. One SettingMaster row (`InvoiceTemplate`) rather than a new
 * column on InvoiceSettings — see lib/invoiceTemplate.ts for why — read by the Invoices screen
 * each time a bill is opened, so a change here shows on the very next print.
 */
function InvoiceTemplateCard({ canWrite }: { canWrite: boolean }) {
  const toast = useToast();
  const qc = useQueryClient();
  const { template, settingId, loading } = useInvoiceTemplate();

  const save = useMutation({
    mutationFn: (value: string) => {
      const base = { Key: INVOICE_TEMPLATE_KEY, Value: value, IsOn: true, IsSecure: false, IsPassword: false, Description: 'Invoice print layout (Classic / Modern / Compact / Letterhead)' };
      return settingId > 0
        ? apiPost('SettingMaster/Update', { SettingMasterID: settingId, ...base })
        : apiPost('SettingMaster/Create', base);
    },
    onSuccess: (res, value) => {
      if (!res.ok) { toast.error(res.message || 'The template could not be saved.'); return; }
      toast.success(`Invoices will print in the ${value} layout.`);
      qc.invalidateQueries({ queryKey: ['crud', 'SettingMaster/GetAll'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Invoice template</CardTitle>
        <span className="text-xs text-slate-400">How the bills on the Invoices screen print</span>
      </CardHeader>
      <CardBody>
        {loading ? (
          <Spinner className="size-5 text-brand-500" />
        ) : (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {INVOICE_TEMPLATES.map((t) => {
              const active = t.value === template;
              return (
                <button
                  key={t.value}
                  type="button"
                  disabled={!canWrite || save.isPending}
                  onClick={() => save.mutate(t.value)}
                  className={`rounded-xl border p-3 text-left transition ${active ? 'border-brand-500 bg-brand-50/60 ring-1 ring-brand-500/30' : 'border-slate-200 hover:border-slate-300'} disabled:cursor-default`}
                >
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-semibold text-slate-800">{t.label}</span>
                    {active && <span className="rounded-full bg-brand-600 px-2 py-0.5 text-[11px] font-medium text-white">In use</span>}
                  </div>
                  <p className="mt-1 text-xs leading-snug text-slate-500">{t.description}</p>
                </button>
              );
            })}
          </div>
        )}
        <p className="mt-3 text-xs text-slate-400">
          The company logo above prints on the Classic, Modern and Compact layouts; Letterhead leaves the top blank for pre-printed paper.
        </p>
      </CardBody>
    </Card>
  );
}
