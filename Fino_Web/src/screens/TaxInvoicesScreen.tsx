import { useMemo, useState, type ReactNode } from 'react';
import { useQuery } from '@tanstack/react-query';
import { FileText, Layers, Search } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { inr, apiDate, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { exportToCsv } from '@/lib/csv';
import { useAuth } from '@/features/auth/AuthContext';
import {
  DIRECTIONS, DOCUMENT_TYPES, canRaiseDocument, documentTypeLabel, irnTone, statusTone, currentFyRange,
} from '@/lib/gstDocs';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

/** The header columns we read — a subset of the entity, named to match the API's PascalCase. */
interface DocHeader {
  TaxInvoiceHeaderID: number;
  Direction: string;
  DocumentType: string;
  DocumentNo: string;
  DocumentDate: string;
  PartyUserID: number;
  PartyName: string;
  PartyGSTIN: string;
  PartyRegistrationStatus: string;
  CompanyGSTIN: string;
  SupplyNature: string;
  GrossTotal: number;
  TaxableTotal: number;
  CGSTTotal: number;
  SGSTTotal: number;
  IGSTTotal: number;
  CessTotal: number;
  RoundOff: number;
  GrandTotal: number;
  Status: string;
  IsConsolidated: boolean;
  NoteReason: string;
  OriginalInvoiceHeaderID: number | null;
}

interface DocLine {
  TaxInvoiceLineID: number;
  LineNo: number;
  Description: string;
  SACCode: string;
  Quantity: number;
  BillingBasis: string;
  GrossValue: number;
  TaxableValue: number;
  CGSTRate: number; CGSTAmount: number;
  SGSTRate: number; SGSTAmount: number;
  IGSTRate: number; IGSTAmount: number;
  CessAmount: number;
  LineTotal: number;
}

interface AnnexureLine {
  TaxInvoiceAnnexureLineID: number;
  TransactionMasterID: number;
  FundTransactionID: number;
  TransactionDate: string;
  ReferenceNo: string;
  GrossValue: number;
  TaxableValue: number;
  TaxAmount: number;
}

interface EInvoice {
  Status: string;
  IRN: string;
  AckNo: string;
  AckDate: string | null;
  ReportingDeadline: string | null;
  ErrorMessage: string;
  AttemptCount: number;
}

interface DocumentDetail {
  Header: DocHeader;
  Lines: DocLine[];
  Annexure: AnnexureLine[];
  EInvoice: EInvoice | null;
  CreditNotes: DocHeader[];
}

interface Filters {
  from: string;
  to: string;
  direction: string;
  documentType: string;
  partyUserId: string;
}

function defaultFilters(): Filters {
  const fy = currentFyRange();
  return { from: fy.from, to: fy.to, direction: '', documentType: '', partyUserId: '' };
}

async function fetchDocuments(f: Filters): Promise<DocHeader[]> {
  const res = await apiPost<DocHeader[]>('TaxInvoice/Select', {
    PeriodFrom: f.from,
    PeriodTo: f.to,
    Direction: f.direction,
    DocumentType: f.documentType,
    PartyUserID: Number(f.partyUserId || 0),
  });
  if (!res.ok) throw new Error(res.message || 'The documents could not be loaded.');
  return res.data ?? [];
}

const money = (v: number) => inr(v);
const pct = (v: number) => (v ? `${v}%` : '—');

export function TaxInvoicesScreen({ def }: CustomScreenProps) {
  const { user } = useAuth();
  const [filters, setFilters] = useState<Filters>(defaultFilters);
  const [applied, setApplied] = useState<Filters>(filters);
  const [selectedId, setSelectedId] = useState<number | null>(null);

  const set = (k: keyof Filters, v: string) => setFilters((s) => ({ ...s, [k]: v }));

  const list = useQuery({
    queryKey: ['tax-invoices', applied],
    queryFn: () => fetchDocuments(applied),
  });
  const rows = list.data ?? [];

  const detail = useQuery({
    queryKey: ['tax-invoice-detail', selectedId],
    queryFn: async (): Promise<DocumentDetail> => {
      const res = await apiPost<DocumentDetail>('TaxInvoice/GetById', { TaxInvoiceHeaderID: selectedId });
      if (!res.ok || !res.data) throw new Error(res.message || 'The document could not be loaded.');
      return res.data;
    },
    enabled: selectedId !== null,
  });

  // The figures bar: only an Issued document is turnover, so the totals follow the API's own
  // rule rather than summing every row on screen.
  const totals = useMemo(() => {
    const issued = rows.filter((r) => r.Status.toUpperCase() === 'ISSUED');
    return {
      count: rows.length,
      issued: issued.length,
      taxable: issued.reduce((a, r) => a + (r.TaxableTotal || 0), 0),
      tax: issued.reduce((a, r) => a + (r.CGSTTotal || 0) + (r.SGSTTotal || 0) + (r.IGSTTotal || 0) + (r.CessTotal || 0), 0),
      grand: issued.reduce((a, r) => a + (r.GrandTotal || 0), 0),
    };
  }, [rows]);

  const runFilter = () => setApplied({ ...filters });

  const exportRows = () =>
    exportToCsv(
      rows.map((r) => ({
        Document: r.DocumentNo,
        Date: dateOnly(r.DocumentDate),
        Direction: r.Direction,
        Type: documentTypeLabel(r.DocumentType),
        Party: r.PartyName,
        PartyGSTIN: r.PartyGSTIN,
        Taxable: r.TaxableTotal,
        CGST: r.CGSTTotal,
        SGST: r.SGSTTotal,
        IGST: r.IGSTTotal,
        GrandTotal: r.GrandTotal,
        Status: r.Status,
      })),
      'GST documents',
    );

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="Every GST document — sale and purchase, with the transactions behind each figure"
        icon={pickIcon(def.title)}
      />

      {/* ── Filters ─────────────────────────────────────────────────────────────── */}
      <Card>
        <CardBody className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-6">
          <Input label="From" type="date" value={filters.from} max={filters.to} onChange={(e) => set('from', e.target.value)} />
          <Input label="To" type="date" value={filters.to} max={apiDate(new Date())} onChange={(e) => set('to', e.target.value)} />
          <Select label="Direction" value={filters.direction} onChange={(e) => set('direction', e.target.value)}>
            {DIRECTIONS.map((d) => (
              <option key={d.value} value={d.value}>{d.label}</option>
            ))}
          </Select>
          <Select label="Document type" value={filters.documentType} onChange={(e) => set('documentType', e.target.value)}>
            {DOCUMENT_TYPES.map((d) => (
              <option key={d.value} value={d.value}>{d.label}</option>
            ))}
          </Select>
          <Input
            label="Party user ID"
            type="number"
            placeholder="All parties"
            value={filters.partyUserId}
            onChange={(e) => set('partyUserId', e.target.value)}
          />
          <div className="flex items-end gap-2">
            <Button onClick={runFilter} loading={list.isFetching} className="flex-1">
              <Search className="mr-1.5 size-4" />
              Show
            </Button>
          </div>
        </CardBody>
      </Card>

      {/* ── Figures ─────────────────────────────────────────────────────────────── */}
      <div className={cn(STAT_STRIP, 'grid-cols-2 lg:grid-cols-5')}>
        <StatTile label="Documents" value={totals.count} icon={FileText} tone="text-brand-600" loading={list.isFetching} />
        <StatTile label="Issued" value={totals.issued} icon={FileText} tone="text-success-600" loading={list.isFetching} />
        <StatTile label="Taxable (issued)" value={money(totals.taxable)} icon={Layers} tone="text-slate-600" loading={list.isFetching} />
        <StatTile label="Tax (issued)" value={money(totals.tax)} icon={Layers} tone="text-info-600" loading={list.isFetching} />
        <StatTile label="Grand total (issued)" value={money(totals.grand)} icon={Layers} tone="text-brand-600" loading={list.isFetching} />
      </div>

      {/* ── The list ────────────────────────────────────────────────────────────── */}
      <Card>
        <div className="flex items-center justify-between gap-3 border-b border-slate-100 px-5 py-3">
          <span className="text-sm text-slate-500">{rows.length} document{rows.length === 1 ? '' : 's'}</span>
          <Button variant="outline" size="sm" onClick={exportRows} disabled={rows.length === 0}>
            Export
          </Button>
        </div>

        {list.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : list.isError ? (
          <CardBody>
            <EmptyState title="Could not load" description={(list.error as Error).message} />
          </CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title="No documents in this period"
              description="Nothing has been issued for the filters above. Generating documents is done from the generators, not here."
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[820px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Document</th>
                  <th className="px-3 py-2.5 font-medium">Date</th>
                  <th className="px-3 py-2.5 font-medium">Party</th>
                  <th className="px-3 py-2.5 text-right font-medium">Taxable</th>
                  <th className="px-3 py-2.5 text-right font-medium">Tax</th>
                  <th className="px-3 py-2.5 text-right font-medium">Grand total</th>
                  <th className="px-3 py-2.5 font-medium">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((r) => {
                  const tax = (r.CGSTTotal || 0) + (r.SGSTTotal || 0) + (r.IGSTTotal || 0) + (r.CessTotal || 0);
                  return (
                    <tr
                      key={r.TaxInvoiceHeaderID}
                      className="cursor-pointer transition-colors hover:bg-slate-50"
                      onClick={() => setSelectedId(r.TaxInvoiceHeaderID)}
                    >
                      <td className="px-5 py-2.5">
                        <div className="flex items-center gap-2">
                          <span className="font-mono text-sm font-semibold text-slate-800">{r.DocumentNo || '—'}</span>
                          {r.IsConsolidated && (
                            <Badge tone="info" className="px-1.5">
                              <Layers className="size-3" /> rolled up
                            </Badge>
                          )}
                        </div>
                        <div className="mt-0.5 flex items-center gap-1.5 text-xs text-slate-400">
                          <span>{documentTypeLabel(r.DocumentType)}</span>
                          <span>·</span>
                          <span>{r.Direction === 'PURCHASE' ? 'Purchase' : 'Sale'}</span>
                        </div>
                      </td>
                      <td className="px-3 py-2.5 text-slate-600">{dateOnly(r.DocumentDate)}</td>
                      <td className="px-3 py-2.5">
                        <div className="max-w-[200px] truncate text-slate-700" title={r.PartyName}>{r.PartyName || '—'}</div>
                        {r.PartyGSTIN && <div className="font-mono text-xs text-slate-400">{r.PartyGSTIN}</div>}
                      </td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{money(r.TaxableTotal)}</td>
                      <td className="px-3 py-2.5 text-right tabular-nums text-slate-700">{money(tax)}</td>
                      <td className="px-3 py-2.5 text-right font-semibold tabular-nums text-slate-900">{money(r.GrandTotal)}</td>
                      <td className="px-3 py-2.5">
                        <Badge tone={statusTone(r.Status)}>{r.Status || '—'}</Badge>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {/* ── Detail ──────────────────────────────────────────────────────────────── */}
      <Modal open={selectedId !== null} onClose={() => setSelectedId(null)} title="Document" size="xl">
        {detail.isFetching ? (
          <div className="flex min-h-[20vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        ) : detail.isError ? (
          <EmptyState title="Could not load" description={(detail.error as Error).message} />
        ) : detail.data ? (
          <DocumentView detail={detail.data} canRaise={canRaiseDocument(user?.UserTypeID)} />
        ) : null}
      </Modal>
    </div>
  );
}

/** One document, in full: header, lines, the annexure that answers "which transactions", IRN and
 *  any credit notes against it. */
function DocumentView({ detail, canRaise }: { detail: DocumentDetail; canRaise: boolean }) {
  const h = detail.Header;
  const tax = (h.CGSTTotal || 0) + (h.SGSTTotal || 0) + (h.IGSTTotal || 0) + (h.CessTotal || 0);
  const intrastate = h.SupplyNature.toUpperCase() === 'INTRASTATE';

  return (
    <div className="space-y-5">
      {/* Header summary */}
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <div className="flex items-center gap-2">
            <span className="font-mono text-lg font-semibold text-slate-900">{h.DocumentNo || '—'}</span>
            <Badge tone={statusTone(h.Status)}>{h.Status}</Badge>
            {h.IsConsolidated && (
              <Badge tone="info" className="px-1.5">
                <Layers className="size-3" /> consolidated
              </Badge>
            )}
          </div>
          <p className="mt-1 text-sm text-slate-500">
            {documentTypeLabel(h.DocumentType)} · {h.Direction === 'PURCHASE' ? 'Purchase' : 'Sale'} · {dateOnly(h.DocumentDate)}
          </p>
        </div>
        <div className="text-right">
          <p className="text-xs uppercase tracking-wide text-slate-400">Grand total</p>
          <p className="text-xl font-semibold tabular-nums text-slate-900">{money(h.GrandTotal)}</p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Party">
          <div className="text-slate-800">{h.PartyName || '—'}</div>
          {h.PartyGSTIN ? (
            <div className="font-mono text-xs text-slate-500">{h.PartyGSTIN}</div>
          ) : (
            <div className="text-xs text-slate-400">{h.PartyRegistrationStatus || 'Unregistered'}</div>
          )}
        </Field>
        <Field label="Supply">
          <div className="text-slate-800">
            {intrastate ? 'Intra-state — CGST + SGST' : 'Inter-state — IGST'}
          </div>
          <div className="font-mono text-xs text-slate-500">{h.CompanyGSTIN || '—'}</div>
        </Field>
      </div>

      {h.OriginalInvoiceHeaderID ? (
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
          Reverses document #{h.OriginalInvoiceHeaderID}. {h.NoteReason && <span className="italic">“{h.NoteReason}”</span>}
        </div>
      ) : null}

      {/* Lines */}
      <section>
        <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-400">Lines</h4>
        <div className="overflow-x-auto rounded-lg border border-slate-200">
          <table className="w-full min-w-[640px] text-sm">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-3 py-2 font-medium">SAC</th>
                <th className="px-3 py-2 font-medium">Description</th>
                <th className="px-3 py-2 text-right font-medium">Qty</th>
                <th className="px-3 py-2 text-right font-medium">Gross</th>
                <th className="px-3 py-2 text-right font-medium">Taxable</th>
                <th className="px-3 py-2 text-right font-medium">CGST</th>
                <th className="px-3 py-2 text-right font-medium">SGST</th>
                <th className="px-3 py-2 text-right font-medium">IGST</th>
                <th className="px-3 py-2 text-right font-medium">Total</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {detail.Lines.map((l) => (
                <tr key={l.TaxInvoiceLineID}>
                  <td className="px-3 py-2 font-mono text-xs text-slate-500">{l.SACCode || '—'}</td>
                  <td className="px-3 py-2 text-slate-700">
                    {l.Description || '—'}
                    <span className="ml-1 text-xs text-slate-400">({l.BillingBasis})</span>
                  </td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-600">{l.Quantity}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-600">{money(l.GrossValue)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-700">{money(l.TaxableValue)}</td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-600">
                    {money(l.CGSTAmount)}<span className="ml-1 text-xs text-slate-400">{pct(l.CGSTRate)}</span>
                  </td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-600">
                    {money(l.SGSTAmount)}<span className="ml-1 text-xs text-slate-400">{pct(l.SGSTRate)}</span>
                  </td>
                  <td className="px-3 py-2 text-right tabular-nums text-slate-600">
                    {money(l.IGSTAmount)}<span className="ml-1 text-xs text-slate-400">{pct(l.IGSTRate)}</span>
                  </td>
                  <td className="px-3 py-2 text-right font-semibold tabular-nums text-slate-800">{money(l.LineTotal)}</td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr className="border-t border-slate-200 bg-slate-50 text-sm font-semibold text-slate-700">
                <td className="px-3 py-2" colSpan={4}>
                  Taxable {money(h.TaxableTotal)} · Tax {money(tax)}
                  {h.RoundOff ? ` · Round-off ${money(h.RoundOff)}` : ''}
                </td>
                <td className="px-3 py-2 text-right tabular-nums">{money(h.TaxableTotal)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{money(h.CGSTTotal)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{money(h.SGSTTotal)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{money(h.IGSTTotal)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{money(h.GrandTotal)}</td>
              </tr>
            </tfoot>
          </table>
        </div>
      </section>

      {/* Annexure — the point of the whole screen: which transactions make up this figure */}
      {detail.Annexure.length > 0 && (
        <section>
          <h4 className="mb-2 flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-slate-400">
            Annexure
            <span className="font-normal normal-case text-slate-400">
              — the {detail.Annexure.length} transaction{detail.Annexure.length === 1 ? '' : 's'} behind this document
            </span>
          </h4>
          <div className="max-h-72 overflow-auto rounded-lg border border-slate-200">
            <table className="w-full min-w-[520px] text-sm">
              <thead className="sticky top-0">
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-3 py-2 font-medium">Date</th>
                  <th className="px-3 py-2 font-medium">Reference</th>
                  <th className="px-3 py-2 font-medium">Source</th>
                  <th className="px-3 py-2 text-right font-medium">Gross</th>
                  <th className="px-3 py-2 text-right font-medium">Taxable</th>
                  <th className="px-3 py-2 text-right font-medium">Tax</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {detail.Annexure.map((a) => (
                  <tr key={a.TaxInvoiceAnnexureLineID}>
                    <td className="px-3 py-2 text-slate-600">{dateOnly(a.TransactionDate)}</td>
                    <td className="px-3 py-2 font-mono text-xs text-slate-500">{a.ReferenceNo || '—'}</td>
                    <td className="px-3 py-2 text-xs text-slate-400">
                      {a.FundTransactionID > 0 ? `Wallet #${a.FundTransactionID}` : a.TransactionMasterID > 0 ? `Txn #${a.TransactionMasterID}` : '—'}
                    </td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-600">{money(a.GrossValue)}</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-700">{money(a.TaxableValue)}</td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-600">{money(a.TaxAmount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {/* IRN status */}
      <section className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="E-invoice (IRN)">
          {detail.EInvoice ? (
            <div className="space-y-1">
              <Badge tone={irnTone(detail.EInvoice.Status)}>{detail.EInvoice.Status}</Badge>
              {detail.EInvoice.IRN && <div className="break-all font-mono text-xs text-slate-500">{detail.EInvoice.IRN}</div>}
              {detail.EInvoice.ReportingDeadline && (
                <div className="text-xs text-slate-400">Deadline {dateOnly(detail.EInvoice.ReportingDeadline)}</div>
              )}
              {detail.EInvoice.ErrorMessage && (
                <div className="text-xs text-danger-600">{detail.EInvoice.ErrorMessage}</div>
              )}
            </div>
          ) : (
            <span className="text-sm text-slate-400">Not queued for an IRN.</span>
          )}
        </Field>
        <Field label="Credit notes against this">
          {detail.CreditNotes.length === 0 ? (
            <span className="text-sm text-slate-400">None.</span>
          ) : (
            <ul className="space-y-1 text-sm">
              {detail.CreditNotes.map((c) => (
                <li key={c.TaxInvoiceHeaderID} className="flex items-center justify-between gap-2">
                  <span className="font-mono text-xs text-slate-600">{c.DocumentNo}</span>
                  <span className="tabular-nums text-slate-500">{money(c.GrandTotal)}</span>
                  <Badge tone={statusTone(c.Status)}>{c.Status}</Badge>
                </li>
              ))}
            </ul>
          )}
        </Field>
      </section>

      {!canRaise && (
        <p className="border-t border-slate-100 pt-3 text-xs text-slate-400">
          Read-only. Raising, reversing or queuing a document is limited to the administrator — a
          number, once allocated, has gone into GSTR-1 and cannot be withdrawn.
        </p>
      )}
    </div>
  );
}

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-3">
      <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-400">{label}</p>
      {children}
    </div>
  );
}
