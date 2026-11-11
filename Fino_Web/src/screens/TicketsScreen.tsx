import { useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { subDays } from 'date-fns';
import { LifeBuoy, Plus, MessageSquareReply, Paperclip } from 'lucide-react';
import { ReportActions, ReportFilterBar, ReportTable, StatRow, type ReportTableColumn, type StatRowItem } from '@/components/report';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { loadSession } from '@/lib/storage';
import { apiDate, dateTime } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Modal } from '@/components/ui/Modal';
import { Badge } from '@/components/ui/Badge';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

const str = (r: Row, k: string) => String(getField(r, k) ?? '').trim();

/**
 * The lifecycle a ticket may hold. The backend refuses anything outside this list, so the two
 * sides cannot drift — a queue whose statuses are free text cannot be filtered or counted.
 */
const STATUSES = ['Open', 'In Progress', 'Resolved', 'Closed', 'Reopened'] as const;
type Status = (typeof STATUSES)[number];

/** Open work vs finished work — what the tone of the badge should say at a glance. */
const TONE: Record<string, 'warning' | 'info' | 'success' | 'neutral'> = {
  Open: 'warning',
  'In Progress': 'info',
  Reopened: 'warning',
  Resolved: 'success',
  Closed: 'neutral',
};

/** Normalise whatever the row carries onto one of the five statuses. */
function statusOf(row: Row): Status {
  const raw = str(row, 'Status');
  return (STATUSES.find((s) => s.toLowerCase() === raw.toLowerCase()) ?? 'Open') as Status;
}

/** Anything before this is a default/zero timestamp, not a date somebody meant. */
const EPOCH_FLOOR = new Date('2000-01-01T00:00:00').getTime();

/**
 * When the ticket was raised, or `null` when the row has no usable timestamp — a missing
 * value, an unparseable one, or the 0001-01-01 default rows written before the API stamped
 * the field.
 */
function raisedAt(row: Row): number | null {
  const raw = getField(row, 'DateTime', 'CreatedDate');
  if (!raw) return null;
  const t = new Date(String(raw)).getTime();
  return Number.isNaN(t) || t < EPOCH_FLOOR ? null : t;
}

/** Admin (1) and Support (6) work the queue; everyone else only raises and reads their own. */
const runsTheQueue = (userTypeId: number) => userTypeId === 1 || userTypeId === 6;

/**
 * Tickets.
 *
 * This was a read-only dump of the TicketMaster table: no way to raise a ticket, no way to answer
 * one, and the whole platform's tickets visible to anyone who opened the page. It is now the
 * working queue — raise, track, answer, close — with the same filters, table and figures bar as
 * every other screen, and it shows exactly what the signed-in user is entitled to see: admin and
 * support get the network's queue, everyone else gets the tickets they raised themselves.
 */
export function TicketsScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const toast = useToast();
  const qc = useQueryClient();

  const session = loadSession();
  const userTypeId = Number(session?.user?.UserTypeID ?? 0);
  const canWorkQueue = runsTheQueue(userTypeId);

  const [fromDate, setFromDate] = useState(apiDate(subDays(new Date(), 90)));
  const [toDate, setToDate] = useState(apiDate(new Date()));
  const [categoryId, setCategoryId] = useState('');
  const [search, setSearch] = useState('');
  // Opens on the NEW tickets, not on everything: the queue's default view is the work in it.
  const [tab, setTab] = useState<Status | 'all'>('Open');

  const [raising, setRaising] = useState(false);
  const [working, setWorking] = useState<Row | null>(null);
  const [viewing, setViewing] = useState<Row | null>(null);
  const [busy, setBusy] = useState(false);

  // Raise form
  const [newCategory, setNewCategory] = useState('');
  const [newSubject, setNewSubject] = useState('');
  const [newBody, setNewBody] = useState('');
  const [newFile, setNewFile] = useState('');

  // Work form
  const [workStatus, setWorkStatus] = useState<Status>('Resolved');
  const [workNote, setWorkNote] = useState('');

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['tickets'],
    // The server pins a non-admin's UserID to their own session, so an empty filter is safe here.
    queryFn: () => fetchList('TicketMaster/GetAll', {}),
  });

  const { data: categories = [] } = useQuery({
    queryKey: ['ticket-categories'],
    staleTime: 300_000,
    queryFn: () => fetchList('TicketCategory/GetDropdown', {}),
  });

  const categoryOpts = useMemo(
    () => categories.map((c, i) => ({
      value: String(getField(c, 'TicketCategoryID') ?? i),
      label: String(getField(c, 'Name') ?? ''),
    })),
    [categories],
  );

  const dated = useMemo(() => {
    const from = new Date(fromDate + 'T00:00:00').getTime();
    const to = new Date(toDate + 'T23:59:59').getTime();
    return rows.filter((r) => {
      // A ticket raised before the API stamped DateTime carries 0001-01-01. Read as a real
      // date, every one of those falls outside any range the operator picks and the ticket
      // disappears from the queue — silently, which is the worst way for a support queue to
      // fail. An unknown timestamp is "unknown", not "very old": the row stays, and the
      // Raised column shows a dash.
      const t = raisedAt(r);
      if (t !== null && (t < from || t > to)) return false;
      if (categoryId && String(getField(r, 'TicketCategoryID') ?? '') !== categoryId) return false;
      return true;
    });
  }, [rows, fromDate, toDate, categoryId]);

  const counts = useMemo(() => {
    const c: Record<string, number> = { all: dated.length };
    for (const s of STATUSES) c[s] = 0;
    for (const r of dated) c[statusOf(r)]++;
    return c;
  }, [dated]);

  const shown = useMemo(() => {
    const q = search.trim().toLowerCase();
    return dated.filter((r) => {
      if (tab !== 'all' && statusOf(r) !== tab) return false;
      if (!q) return true;
      return ['Subject', 'Body', 'TicketCategoryName', 'UserName', 'Resolution']
        .some((k) => str(r, k).toLowerCase().includes(q));
    });
  }, [dated, tab, search]);

  // The figures bar carries the queue's health: what is waiting, what is being worked, what is done.
  const stats: StatRowItem[] = [
    { label: 'Total', value: String(counts.all) },
    { label: 'Open', value: String(counts.Open), tone: 'text-warning-600' },
    { label: 'In progress', value: String(counts['In Progress']), tone: 'text-info-600' },
    { label: 'Reopened', value: String(counts.Reopened), tone: 'text-warning-600' },
    { label: 'Resolved', value: String(counts.Resolved), tone: 'text-success-600' },
    { label: 'Closed', value: String(counts.Closed), tone: 'text-slate-600' },
  ];

  const resetFilters = () => {
    setFromDate(apiDate(subDays(new Date(), 90)));
    setToDate(apiDate(new Date()));
    setCategoryId('');
    setSearch('');
  };

  const closeRaise = () => { setRaising(false); setNewCategory(''); setNewSubject(''); setNewBody(''); setNewFile(''); };
  const closeWork = () => { setWorking(null); setWorkNote(''); setWorkStatus('Resolved'); };

  async function submitTicket() {
    if (!newCategory) { toast.error('Pick a category.'); return; }
    if (!newSubject.trim()) { toast.error('A subject is required.'); return; }
    if (!newBody.trim()) { toast.error('Describe the problem.'); return; }
    setBusy(true);
    try {
      // UserID / UserTypeID are stamped from the signed-in session server-side — not sent here.
      const res = await apiPost('TicketMaster/Create', {
        TicketCategoryID: Number(newCategory),
        Subject: newSubject.trim(),
        Body: newBody.trim(),
        UploadFilePath: newFile.trim(),
      });
      if (!res.ok) { toast.error(res.message || 'Could not raise the ticket.'); return; }
      toast.success('Ticket raised.');
      closeRaise();
      await qc.invalidateQueries({ queryKey: ['tickets'] });
      // The counts strip is on this screen too — a ticket you just raised that does not appear
      // in the count for another minute reads as "it didn't save".
      qc.invalidateQueries({ queryKey: ['work-queue'] });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not raise the ticket.');
    } finally {
      setBusy(false);
    }
  }

  async function submitWork() {
    if (!working) return;
    if (!workNote.trim()) { toast.error('Write what was done — the requester sees this.'); return; }
    setBusy(true);
    try {
      const res = await apiPost('TicketMaster/Resolve', {
        TicketMasterID: getField(working, 'TicketMasterID'),
        Status: workStatus,
        Resolution: workNote.trim(),
      });
      if (!res.ok) { toast.error(res.message || 'Could not update the ticket.'); return; }
      toast.success(`Ticket marked ${workStatus.toLowerCase()}.`);
      closeWork();
      await qc.invalidateQueries({ queryKey: ['tickets'] });
      qc.invalidateQueries({ queryKey: ['work-queue'] });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : 'Could not update the ticket.');
    } finally {
      setBusy(false);
    }
  }

  const columns: ReportTableColumn<Row>[] = [
    { key: '_serial', label: '#', type: 'serial' },
    {
      key: 'DateTime',
      label: 'Raised',
      // Same "unknown is not old" rule as the filter — a 0001-01-01 row reads as a dash rather
      // than claiming the ticket was raised in the year one.
      render: (r) => {
        const t = raisedAt(r);
        return <span className="tabular-nums">{t === null ? '—' : dateTime(str(r, 'DateTime'))}</span>;
      },
    },
    { key: 'TicketCategoryName', label: 'Category' },
    {
      key: 'Subject',
      label: 'Subject',
      render: (r) => (
        <div className="min-w-0">
          <span className="block max-w-[280px] truncate font-medium text-slate-800" title={str(r, 'Subject')}>
            {str(r, 'Subject') || '—'}
          </span>
          <span className="block max-w-[280px] truncate text-xs text-slate-400" title={str(r, 'Body')}>
            {str(r, 'Body')}
          </span>
        </div>
      ),
    },
    // Who raised it only matters to whoever works the queue — a retailer only ever sees their own.
    ...(canWorkQueue
      ? ([
          { key: 'UserName', label: 'Raised by' },
          { key: 'UserTypeName', label: 'Role' },
        ] as ReportTableColumn<Row>[])
      : []),
    {
      key: 'Status',
      label: 'Status',
      render: (r) => <Badge tone={TONE[statusOf(r)] ?? 'neutral'}>{statusOf(r)}</Badge>,
    },
    {
      key: 'Resolution',
      label: 'Answer',
      render: (r) => {
        const v = str(r, 'Resolution');
        return v
          ? <span className="block max-w-[240px] truncate text-slate-600" title={v}>{v}</span>
          : <span className="text-slate-300">—</span>;
      },
    },
    {
      key: '_actions',
      label: 'Actions',
      render: (r) => (
        <div className="flex items-center justify-end gap-1">
          <Button variant="ghost" size="sm" onClick={() => setViewing(r)} title="Open">
            View
          </Button>
          {canWorkQueue && (
            <Button
              size="sm"
              variant="secondary"
              onClick={() => { setWorking(r); setWorkStatus(statusOf(r) === 'Open' ? 'In Progress' : 'Resolved'); setWorkNote(str(r, 'Resolution')); }}
              title="Answer / change status"
            >
              <MessageSquareReply className="size-4" />
              Answer
            </Button>
          )}
        </div>
      ),
    },
  ];

  const TABS: { key: Status | 'all'; label: string }[] = [
    { key: 'all', label: 'All' },
    ...STATUSES.map((s) => ({ key: s as Status, label: s })),
  ];

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={canWorkQueue ? 'The support queue — answer, reassign status, close' : 'Your support tickets'}
        icon={LifeBuoy}
        actions={
          <ReportActions
            total={shown.length}
            loading={isFetching}
            onRefresh={() => refetch()}
            onExport={() => downloadCsv(shown, 'Tickets')}
            exportDisabled={shown.length === 0}
          >
            <Button size="sm" onClick={() => setRaising(true)}>
              <Plus className="size-4" />
              Raise ticket
            </Button>
          </ReportActions>
        }
      />

      <StatRow items={stats} loading={isFetching} columns={6} />

      <ReportFilterBar onReset={resetFilters} loading={isFetching}>
        <Input label="From date" type="date" max={toDate} value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
        <Input label="To date" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
        <Select label="Category" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
          <option value="">All categories</option>
          {categoryOpts.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
        </Select>
        <Input label="Search" placeholder="Subject / message / answer…" value={search} onChange={(e) => setSearch(e.target.value)} />
      </ReportFilterBar>

      {/* Status tabs — the same treatment as the complaint queue, so the two read alike. */}
      <div className="mb-3 flex flex-wrap gap-1 border-b border-slate-200">
        {TABS.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={cn(
              '-mb-px flex items-center gap-1.5 border-b-2 px-3.5 py-2 text-sm font-medium transition-colors',
              tab === t.key ? 'border-brand-600 text-brand-700' : 'border-transparent text-slate-500 hover:text-slate-700',
            )}
          >
            {t.label}
            <span className={cn('rounded-full px-1.5 py-0.5 text-[11px] tabular-nums', tab === t.key ? 'bg-brand-100 text-brand-700' : 'bg-slate-100 text-slate-500')}>
              {counts[t.key] ?? 0}
            </span>
          </button>
        ))}
      </div>

      <ReportTable
        columns={columns}
        rows={shown}
        loading={isFetching}
        rowKey={(r, i) => String(getField(r, 'TicketMasterID') ?? i)}
        emptyTitle="No tickets"
        emptyDescription={canWorkQueue ? 'Nothing in the queue for these filters.' : 'You have not raised a ticket in this period.'}
      />

      {/* Raise */}
      <Modal open={raising} onClose={closeRaise} title="Raise a ticket" size="md">
        <div className="space-y-4">
          {/* An empty category list used to be silent: the dropdown simply had nothing in it, the
              form could never be completed, and "Raise ticket" looked broken with no explanation.
              Categories now ship seeded, but if an admin deletes them all, say so — and say it
              differently to the person who can fix it. */}
          {categoryOpts.length === 0 ? (
            <div className="rounded-lg border border-warning-500/30 bg-warning-50 p-3 text-sm text-warning-700">
              {canWorkQueue ? (
                <>
                  No ticket categories exist yet. Add at least one under{' '}
                  <span className="font-semibold">Support → Ticket Categories</span> — until then
                  nobody on the network can raise a ticket.
                </>
              ) : (
                <>Ticket categories have not been set up yet. Please contact your administrator.</>
              )}
            </div>
          ) : (
            <Select label="Category" value={newCategory} onChange={(e) => setNewCategory(e.target.value)}>
              <option value="">Select a category…</option>
              {categoryOpts.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
            </Select>
          )}
          <Input label="Subject" value={newSubject} onChange={(e) => setNewSubject(e.target.value)} placeholder="One line — what is wrong" />
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700">Details</label>
            <textarea
              rows={5}
              value={newBody}
              onChange={(e) => setNewBody(e.target.value)}
              placeholder="Transaction ids, amounts, what you expected and what happened…"
              className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-800 shadow-sm transition-colors placeholder:text-slate-400 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30"
            />
          </div>
          <Input
            label="Attachment URL (optional)"
            leftIcon={<Paperclip className="size-4" />}
            value={newFile}
            onChange={(e) => setNewFile(e.target.value)}
            placeholder="https://…"
            hint="Link to a screenshot or receipt if you have one"
          />
          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={closeRaise} disabled={busy}>Cancel</Button>
            <Button onClick={submitTicket} loading={busy} disabled={categoryOpts.length === 0}>Raise ticket</Button>
          </div>
        </div>
      </Modal>

      {/* Answer / change status */}
      <Modal open={Boolean(working)} onClose={closeWork} title="Answer ticket" size="md">
        {working && (
          <div className="space-y-4">
            <div className="rounded-lg border border-slate-200 bg-slate-50/70 p-3">
              <p className="text-sm font-medium text-slate-800">{str(working, 'Subject')}</p>
              <p className="mt-1 whitespace-pre-wrap text-sm text-slate-600">{str(working, 'Body')}</p>
              <p className="mt-2 text-xs text-slate-400">
                {str(working, 'TicketCategoryName')} · {str(working, 'UserName')} · {dateTime(str(working, 'DateTime'))}
              </p>
            </div>
            <Select label="Status" value={workStatus} onChange={(e) => setWorkStatus(e.target.value as Status)}>
              {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
            </Select>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Answer</label>
              <textarea
                rows={5}
                value={workNote}
                onChange={(e) => setWorkNote(e.target.value)}
                placeholder="What was found, what was done, what the user should do next…"
                className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-800 shadow-sm transition-colors placeholder:text-slate-400 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30"
              />
              <p className="mt-1 text-xs text-slate-400">The requester sees this on their ticket.</p>
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button variant="secondary" onClick={closeWork} disabled={busy}>Cancel</Button>
              <Button onClick={submitWork} loading={busy}>Save answer</Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Read */}
      <Modal open={Boolean(viewing)} onClose={() => setViewing(null)} title="Ticket" size="md">
        {viewing && (
          <div className="space-y-4 text-sm">
            <div className="flex items-center gap-2">
              <Badge tone={TONE[statusOf(viewing)] ?? 'neutral'}>{statusOf(viewing)}</Badge>
              <span className="text-slate-400">{str(viewing, 'TicketCategoryName')}</span>
            </div>
            <div>
              <p className="font-medium text-slate-800">{str(viewing, 'Subject')}</p>
              <p className="mt-1 whitespace-pre-wrap text-slate-600">{str(viewing, 'Body')}</p>
            </div>
            {Boolean(str(viewing, 'UploadFilePath')) && (
              <a
                href={str(viewing, 'UploadFilePath')}
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-1.5 text-brand-600 hover:underline"
              >
                <Paperclip className="size-4" />
                Attachment
              </a>
            )}
            {Boolean(str(viewing, 'Resolution')) && (
              <div className="rounded-lg border border-success-200 bg-success-50/60 p-3">
                <p className="text-xs font-semibold uppercase tracking-wide text-success-700">Answer</p>
                <p className="mt-1 whitespace-pre-wrap text-slate-700">{str(viewing, 'Resolution')}</p>
                <p className="mt-2 text-xs text-slate-400">{dateTime(str(viewing, 'ResolutionDate'))}</p>
              </div>
            )}
            <p className="text-xs text-slate-400">
              Raised by {str(viewing, 'UserName') || 'you'} · {dateTime(str(viewing, 'DateTime'))}
            </p>
          </div>
        )}
      </Modal>
    </>
  );
}
