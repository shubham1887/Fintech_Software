import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AlertTriangle, CheckCircle2, Download, MapPin, RefreshCw, Sparkles, UserCog, Wand2,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { useCsvDownload } from '@/lib/exportAll';
import { cn } from '@/lib/cn';
import { canRaiseDocument, periodKeyOf, periodLabel, recentPeriods, useCompanyGstin } from '@/lib/gstDocs';
import { inr } from '@/lib/format';
import { GST_STATES } from '@/lib/gstStates';
import { useAuth } from '@/features/auth/AuthContext';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface PartyRow {
  UserID: number;
  UserTypeID: number;
  PartyName: string;
  MobileNumber: string;
  GSTIN: string;
  RegistrationStatus: string;
  ProfileStateCode: string;
  ResolvedStateCode: string;
  ResolvedStateName: string;
  Source: string;
  CanBackfill: boolean;
  NeedsAttention: boolean;
  IssuedDocuments: number;
}

interface Report {
  Parties: PartyRow[];
  Total: number;
  AlreadySet: number;
  CanBackfill: number;
  NeedsAttention: number;
  DocumentsWithBadPlaceOfSupply: number;
}

interface BackfillResult {
  DryRun: boolean;
  ProfilesCreated: number;
  ProfilesUpdated: number;
  StillUnknown: number;
  Messages: string[];
}

interface RegenResult {
  DryRun: boolean;
  PeriodKey: string;
  DocumentsCancelled: number;
  TaxableValueCancelled: number;
  DocumentNumbers: string[];
  NextStep: string;
  Messages: string[];
}

const SOURCE_LABEL: Record<string, string> = {
  Profile: 'tax profile',
  UserField: 'GST state field',
  GSTIN: 'GSTIN prefix',
  OnboardingState: 'onboarding state',
  None: 'nothing on record',
};

const sourceTone = (s: string): 'success' | 'info' | 'warning' | 'danger' | 'neutral' => {
  switch (s) {
    case 'Profile': return 'success';
    case 'GSTIN': return 'info';
    case 'UserField': return 'info';
    case 'OnboardingState': return 'warning';
    default: return 'danger';
  }
};

export function PartyStateScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const { user } = useAuth();
  const toast = useToast();
  const qc = useQueryClient();
  const mayWrite = canRaiseDocument(user?.UserTypeID);

  const [filter, setFilter] = useState<'all' | 'attention' | 'derivable'>('all');

  const review = useQuery({
    queryKey: ['party-state'],
    queryFn: async (): Promise<Report> => {
      const res = await apiPost<Report>('PartyState/Review', {});
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not load the parties.');
      return res.data;
    },
  });

  const rows = useMemo(() => {
    const all = review.data?.Parties ?? [];
    if (filter === 'attention') return all.filter((p) => p.NeedsAttention);
    if (filter === 'derivable') return all.filter((p) => p.CanBackfill);
    return all;
  }, [review.data, filter]);

  const backfill = useMutation({
    mutationFn: async (dryRun: boolean): Promise<BackfillResult> => {
      const res = await apiPost<BackfillResult>('PartyState/Backfill', { DryRun: dryRun, OnlyUserID: 0 });
      if (!res.ok || !res.data) throw new Error(res.message || 'The derivation could not run.');
      return res.data;
    },
    onSuccess: (r) => {
      if (r.DryRun) {
        toast.info(`${r.ProfilesCreated} party profile(s) would be written. Nothing was changed.`);
      } else {
        toast.success(`${r.ProfilesCreated} created, ${r.ProfilesUpdated} updated.`);
        qc.invalidateQueries({ queryKey: ['party-state'] });
      }
      r.Messages.slice(0, 4).forEach((m) => toast.info(m));
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Cancel the documents stamped with the bad code ──────────────────────────────────
  const [regenOpen, setRegenOpen] = useState(false);
  const [regenPeriod, setRegenPeriod] = useState(periodKeyOf());
  const [regenPreview, setRegenPreview] = useState<RegenResult | null>(null);
  const { gstin } = useCompanyGstin();

  const regen = useMutation({
    mutationFn: async (dryRun: boolean): Promise<RegenResult> => {
      const res = await apiPost<RegenResult>('PartyState/CancelForRegeneration', {
        CompanyGSTIN: gstin, PeriodKey: regenPeriod, DryRun: dryRun,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'It could not run.');
      return res.data;
    },
    onSuccess: (x) => {
      setRegenPreview(x);
      if (x.DryRun) {
        toast.info(`${x.DocumentsCancelled} document(s) would be cancelled.`);
      } else {
        toast.success(`${x.DocumentsCancelled} document(s) cancelled. ${x.NextStep}`);
        qc.invalidateQueries({ queryKey: ['party-state'] });
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // ── Set one by hand ─────────────────────────────────────────────────────────────────
  const [editing, setEditing] = useState<PartyRow | null>(null);
  const [code, setCode] = useState('');

  const setOne = useMutation({
    mutationFn: async () => {
      const res = await apiPost('PartyState/SetPlaceOfSupply', {
        UserID: editing?.UserID ?? 0,
        PlaceOfSupplyStateCode: code,
      });
      if (!res.ok) throw new Error(res.message || 'It could not be saved.');
    },
    onSuccess: () => {
      toast.success(code ? 'Place of supply set.' : 'Place of supply cleared.');
      setEditing(null);
      qc.invalidateQueries({ queryKey: ['party-state'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const download = () =>
    downloadCsv(
      (review.data?.Parties ?? []).map((p) => ({
        UserID: p.UserID,
        Party: p.PartyName,
        Mobile: p.MobileNumber,
        GSTIN: p.GSTIN,
        Registration: p.RegistrationStatus,
        OnProfile: p.ProfileStateCode,
        Resolved: p.ResolvedStateCode,
        State: p.ResolvedStateName,
        Source: SOURCE_LABEL[p.Source] ?? p.Source,
        IssuedDocuments: p.IssuedDocuments,
      })),
      'party-place-of-supply.csv',
    );

  const r = review.data;

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="The state every document is stamped with — derived from what onboarding already captured"
        icon={pickIcon(def.title)}
        actions={
          mayWrite && (
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => backfill.mutate(true)} loading={backfill.isPending}>
                <Sparkles className="mr-1.5 size-4" />
                Preview
              </Button>
              <Button
                onClick={() => backfill.mutate(false)}
                loading={backfill.isPending}
                disabled={(r?.CanBackfill ?? 0) === 0}
              >
                <Wand2 className="mr-1.5 size-4" />
                Derive {r?.CanBackfill ?? 0}
              </Button>
            </div>
          )
        }
      />

      <div className="flex gap-2.5 rounded-lg border border-slate-200 bg-white p-3 text-sm text-slate-600">
        <MapPin className="mt-0.5 size-4 shrink-0 text-slate-400" />
        <p>
          The place of supply decides whether a supply is <strong>intra-state (CGST + SGST)</strong> or{' '}
          <strong>inter-state (IGST)</strong>, and the portal rejects a return carrying an invalid one.
          Most parties already have a state recorded somewhere — on the GSTIN, or from the address step at
          onboarding — it was simply never translated into the two-digit GST code. <strong>Derive</strong>{' '}
          does that translation; only the parties nothing knows about need a human.
        </p>
      </div>

      {(r?.DocumentsWithBadPlaceOfSupply ?? 0) > 0 && (
        <div className="rounded-lg border border-danger-500/30 bg-danger-50 p-3 text-sm text-danger-600">
          <div className="flex gap-2.5">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" />
            <p>
              <strong>{r!.DocumentsWithBadPlaceOfSupply} document(s) already carry no valid place of supply.</strong>{' '}
              Fixing a party here does <em>not</em> change a document already issued — the stamp is deliberately
              never recomputed. Fix the parties first, then cancel those documents and generate them again.
            </p>
          </div>
          {mayWrite && (
            <div className="mt-2.5 pl-6">
              <Button variant="danger" onClick={() => setRegenOpen(true)}>
                <RefreshCw className="mr-1.5 size-4" />
                Cancel &amp; regenerate…
              </Button>
            </div>
          )}
        </div>
      )}

      <div className={`${STAT_STRIP} grid-cols-2 lg:grid-cols-4`}>
        <StatTile label="Parties" value={r?.Total ?? 0} icon={UserCog} tone="text-brand-600" loading={review.isPending} />
        <StatTile label="Already set" value={r?.AlreadySet ?? 0} icon={CheckCircle2} tone="text-success-600" loading={review.isPending} />
        <StatTile label="Can be derived" value={r?.CanBackfill ?? 0} icon={Wand2} tone="text-warning-600" loading={review.isPending} />
        <StatTile label="Need a human" value={r?.NeedsAttention ?? 0} icon={AlertTriangle}
          tone={(r?.NeedsAttention ?? 0) > 0 ? 'text-danger-600' : 'text-success-600'} loading={review.isPending} />
      </div>

      <Card>
        <CardBody className="flex flex-wrap items-center gap-3">
          <div className="flex rounded-lg border border-slate-200 p-0.5">
            {([
              ['all', `All ${r?.Total ?? 0}`],
              ['derivable', `Derivable ${r?.CanBackfill ?? 0}`],
              ['attention', `Need a human ${r?.NeedsAttention ?? 0}`],
            ] as const).map(([k, label]) => (
              <button
                key={k}
                type="button"
                onClick={() => setFilter(k)}
                className={cn(
                  'rounded-md px-3 py-1.5 text-sm font-medium transition',
                  filter === k ? 'bg-brand-600 text-white' : 'text-slate-600 hover:bg-slate-100',
                )}
              >
                {label}
              </button>
            ))}
          </div>
          <div className="flex-1" />
          <Button variant="ghost" onClick={download} disabled={(r?.Parties.length ?? 0) === 0}>
            <Download className="mr-1.5 size-4" />
            CSV
          </Button>
        </CardBody>
      </Card>

      <Card>
        {review.isFetching ? (
          <div className="flex min-h-[30vh] items-center justify-center"><Spinner className="size-6 text-brand-500" /></div>
        ) : review.isError ? (
          <CardBody><EmptyState title="Could not load" description={(review.error as Error).message} /></CardBody>
        ) : rows.length === 0 ? (
          <CardBody>
            <EmptyState
              title={filter === 'attention' ? 'Every party has a place of supply' : 'Nothing here'}
              description={
                filter === 'attention'
                  ? 'Nothing is left for a human to set — every party resolves to a state.'
                  : 'Clear the filter to see the rest.'
              }
            />
          </CardBody>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[920px] text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-5 py-2.5 font-medium">Party</th>
                  <th className="px-3 py-2.5 font-medium">GSTIN</th>
                  <th className="px-3 py-2.5 font-medium">Registration</th>
                  <th className="px-3 py-2.5 font-medium">Place of supply</th>
                  <th className="px-3 py-2.5 font-medium">From</th>
                  <th className="px-3 py-2.5 text-right font-medium">Documents</th>
                  {mayWrite && <th className="px-3 py-2.5" />}
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rows.map((p) => (
                  <tr key={p.UserID} className="hover:bg-slate-50">
                    <td className="px-5 py-2.5">
                      <div className="max-w-[200px] truncate font-medium text-slate-800" title={p.PartyName}>
                        {p.PartyName || `#${p.UserID}`}
                      </div>
                      <div className="text-xs text-slate-400">{p.MobileNumber}</div>
                    </td>
                    <td className="px-3 py-2.5 font-mono text-xs text-slate-500">{p.GSTIN || '—'}</td>
                    <td className="px-3 py-2.5">
                      <Badge tone={p.RegistrationStatus === 'REGISTERED' ? 'info' : 'neutral'}>
                        {p.RegistrationStatus === 'REGISTERED' ? 'Registered' : 'Unregistered'}
                      </Badge>
                    </td>
                    <td className="px-3 py-2.5">
                      {p.ResolvedStateCode ? (
                        <span className="text-slate-700">
                          <span className="font-mono text-xs text-slate-500">{p.ResolvedStateCode}</span>{' '}
                          {p.ResolvedStateName}
                        </span>
                      ) : (
                        <span className="text-sm font-semibold text-danger-600">not known</span>
                      )}
                    </td>
                    <td className="px-3 py-2.5">
                      <Badge tone={sourceTone(p.Source)}>{SOURCE_LABEL[p.Source] ?? p.Source}</Badge>
                      {p.CanBackfill && <div className="text-[11px] text-warning-600">not yet on the profile</div>}
                    </td>
                    <td className="px-3 py-2.5 text-right">
                      <span className={cn('tabular-nums', p.IssuedDocuments > 0 ? 'text-slate-700' : 'text-slate-300')}>
                        {p.IssuedDocuments}
                      </span>
                    </td>
                    {mayWrite && (
                      <td className="px-3 py-2.5 text-right">
                        <button
                          type="button"
                          onClick={() => { setEditing(p); setCode(p.ProfileStateCode || p.ResolvedStateCode || ''); }}
                          className="text-xs text-brand-600 hover:underline"
                        >
                          Set
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {!mayWrite && (
        <p className="text-xs text-slate-400">
          Read-only. Deriving and setting a place of supply are limited to Admin — the value decides which
          tax heads every document stamped afterwards falls into.
        </p>
      )}

      <Modal
        open={regenOpen}
        onClose={() => { setRegenOpen(false); setRegenPreview(null); }}
        title="Cancel and regenerate documents"
        size="md"
      >
        <div className="space-y-3">
          <p className="text-sm text-slate-500">
            Documents are <strong>cancelled, never deleted</strong> — GSTR-1 table 13 declares the numbers each
            series issued, and a number that simply vanishes makes the sequence impossible to demonstrate. A
            cancelled document keeps its number and is reported as cancelled.
          </p>
          <div className="w-48">
            <Select label="Period" value={regenPeriod} onChange={(e) => { setRegenPeriod(e.target.value); setRegenPreview(null); }}>
              {recentPeriods(18).map((p) => (
                <option key={p} value={p}>{periodLabel(p)}</option>
              ))}
            </Select>
          </div>

          {regenPreview && (
            <div
              className={cn(
                'rounded-lg border p-3 text-sm',
                regenPreview.DocumentsCancelled === 0
                  ? 'border-slate-200 bg-slate-50 text-slate-600'
                  : 'border-warning-500/30 bg-warning-50 text-warning-700',
              )}
            >
              <p>
                <strong>{regenPreview.DocumentsCancelled} document(s)</strong>
                {regenPreview.TaxableValueCancelled > 0 && <> · {inr(regenPreview.TaxableValueCancelled)} taxable</>}
                {regenPreview.DryRun ? ' would be cancelled.' : ' cancelled.'}
              </p>
              {regenPreview.DocumentNumbers.length > 0 && (
                <p className="mt-1 font-mono text-xs opacity-80">
                  {regenPreview.DocumentNumbers.slice(0, 8).join(', ')}
                  {regenPreview.DocumentNumbers.length > 8 && ` … +${regenPreview.DocumentNumbers.length - 8}`}
                </p>
              )}
              {!regenPreview.DryRun && regenPreview.NextStep && (
                <p className="mt-2 font-medium">{regenPreview.NextStep}</p>
              )}
            </div>
          )}

          <p className="text-xs text-slate-400">
            Refused once GSTR-1 for the period has been filed, or the ledger has sealed it — a declaration
            already made is corrected with a credit note, not withdrawn.
          </p>

          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => { setRegenOpen(false); setRegenPreview(null); }}>Close</Button>
            <Button variant="outline" onClick={() => regen.mutate(true)} loading={regen.isPending}>
              Preview
            </Button>
            <Button
              variant="danger"
              onClick={() => regen.mutate(false)}
              loading={regen.isPending}
              disabled={!regenPreview || regenPreview.DocumentsCancelled === 0}
            >
              Cancel {regenPreview?.DocumentsCancelled ?? 0} document(s)
            </Button>
          </div>
        </div>
      </Modal>

      <Modal open={editing !== null} onClose={() => setEditing(null)} title="Set the place of supply" size="sm">
        <div className="space-y-3">
          <p className="text-sm text-slate-600">
            {editing?.PartyName}
            {editing?.GSTIN ? <span className="font-mono text-xs text-slate-400"> · {editing.GSTIN}</span> : null}
          </p>
          <Select label="State" value={code} onChange={(e) => setCode(e.target.value)}>
            <option value="">(not known)</option>
            {GST_STATES.map((s) => (
              <option key={s.code} value={s.code}>{`${s.code} — ${s.name}`}</option>
            ))}
          </Select>
          <p className="text-xs text-slate-400">
            Leaving it as "(not known)" is a truthful answer and is stored as empty — it is never written as
            "00", which would look like a real state and send every supply to IGST.
          </p>
          {(editing?.IssuedDocuments ?? 0) > 0 && (
            <div className="flex gap-2 rounded-lg border border-warning-500/30 bg-warning-50 p-2.5 text-xs text-warning-700">
              <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />
              <p>
                This party already has {editing!.IssuedDocuments} document(s). They keep the code they were
                stamped with — only documents generated after this change will use the new one.
              </p>
            </div>
          )}
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => setEditing(null)}>Cancel</Button>
            <Button onClick={() => setOne.mutate()} loading={setOne.isPending}>Save</Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
