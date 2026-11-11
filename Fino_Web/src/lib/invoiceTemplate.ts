import { useQuery } from '@tanstack/react-query';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';

/**
 * The invoice layouts the company can print its bills in. Stored as the SettingMaster key
 * `InvoiceTemplate` (no schema change: InvoiceSettings has 23 NOT NULL columns on live and a new
 * one would need an ALTER on every install), read by the Invoices screen when a bill is opened.
 */
export const INVOICE_TEMPLATE_KEY = 'InvoiceTemplate';

export const INVOICE_TEMPLATES = [
  { value: 'Classic', label: 'Classic', description: 'Black rule under the company name, boxed tables. The default.' },
  { value: 'Modern', label: 'Modern', description: 'Brand-coloured header band, light table lines, logo at the top.' },
  { value: 'Compact', label: 'Compact', description: 'Smaller type and tighter rows — fits a long annexure on one page.' },
  { value: 'Letterhead', label: 'Letterhead', description: 'Leaves the top 4 cm blank for pre-printed letterhead paper.' },
] as const;

export type InvoiceTemplate = (typeof INVOICE_TEMPLATES)[number]['value'];

export function normaliseTemplate(v: unknown): InvoiceTemplate {
  const s = String(v ?? '').trim();
  return (INVOICE_TEMPLATES.find((t) => t.value.toLowerCase() === s.toLowerCase())?.value ?? 'Classic') as InvoiceTemplate;
}

/** The saved template (and the setting row, so the picker can update rather than duplicate it). */
export function useInvoiceTemplate() {
  const { data: rows = [], isLoading } = useQuery({
    queryKey: ['crud', 'SettingMaster/GetAll'],
    queryFn: () => fetchList('SettingMaster/GetAll', { PageNumber: 1, PageSize: 500 }),
    staleTime: 60_000,
  });
  const row = rows.find((r) => String(getField(r, 'Key') ?? '') === INVOICE_TEMPLATE_KEY);
  return {
    template: normaliseTemplate(row ? getField(row, 'Value') : ''),
    settingId: row ? Number(getField(row, 'SettingMasterID') ?? 0) : 0,
    loading: isLoading,
  };
}

/** CSS appended to the bill for the chosen template. `brand` is the theme's brand colour. */
export function invoiceTemplateCss(template: InvoiceTemplate, brand = '#1d4ed8'): string {
  switch (template) {
    case 'Modern':
      return `
  .top{border-bottom:0;background:${brand};color:#fff;padding:16px 18px;border-radius:8px}
  .top .muted{color:rgba(255,255,255,.8)} .top .co{font-size:20px}
  .title{color:#fff} .top table td{color:#fff !important}
  th,td{border:0;border-bottom:1px solid #e2e8f0} th{background:${brand}14;color:${brand}}
  .box{border:0;background:#f8fafc} .grand td{border-bottom:2px solid ${brand};color:${brand}}
  .logo{max-height:52px;max-width:180px;margin-bottom:6px;display:block;background:#fff;border-radius:4px;padding:3px}`;
    case 'Compact':
      return `
  body{font-size:10.5px;padding:14px} th,td{padding:3px 6px} .co{font-size:15px} .title{font-size:13px}
  .meta{margin:8px 0} .box{padding:6px} h3{margin:10px 0 2px} .foot{margin-top:12px} .sign{margin-top:22px}
  .logo{max-height:36px;max-width:140px;margin-bottom:4px;display:block}`;
    case 'Letterhead':
      return `
  .wrap{padding-top:4cm} .top .co,.top .muted,.top .logo{display:none}
  @media print{.wrap{padding-top:4cm}}`;
    default:
      return `
  .logo{max-height:48px;max-width:170px;margin-bottom:6px;display:block}`;
  }
}
