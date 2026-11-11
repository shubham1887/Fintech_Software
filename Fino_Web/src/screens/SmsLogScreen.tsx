import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';

import { subDays } from 'date-fns';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { dateTime, apiDate } from '@/lib/format';
import { useCsvDownload } from '@/lib/exportAll';
import { PageHeader } from '@/components/ui/PageHeader';
import { ReportActions, ReportFilterBar, ReportPager } from '@/components/report';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { LogTable, type LogTableColumn } from '@/components/ui/LogTable';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');
const PAGE = 25;

type Channel = 'WhatsApp' | 'Mail' | 'SMS';
const CHANNELS: Channel[] = ['SMS', 'WhatsApp', 'Mail'];

interface Filters {
  fromDate: string;
  toDate: string;
  channel: string;
  search: string;
}

const defaults = (): Filters => ({
  fromDate: apiDate(subDays(new Date(), 7)),
  toDate: apiDate(new Date()),
  channel: '',
  search: '',
});

/** Map the stored Type / WhatsApp fields onto the three visible channels. */
function resolveChannel(type: string, whatsApp: string): Channel {
  const t = type.toLowerCase();
  if (whatsApp.trim() || t.includes('whats')) return 'WhatsApp';
  if (t.includes('mail') || t.includes('email')) return 'Mail';
  return 'SMS';
}

/**
 * Sent SMS — every outbound message, with its body and the gateway's request/response.
 *
 * The message column used to be one truncated line ending in an ellipsis, and the full text (plus
 * the gateway request and response) was only reachable through a per-row View dialog — so the
 * screen showed you that a message existed without ever showing you the message. The body and both
 * payloads are now on the row itself, wrapped rather than cut, expanding in place; and the log has
 * the date-range, channel and text filters it needed to be searchable at all.
 */
export function SmsLogScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const [draft, setDraft] = useState<Filters>(defaults);
  const [applied, setApplied] = useState<Filters>(draft);
  const [page, setPage] = useState(0);

  const upd = (patch: Partial<Filters>) => setDraft((d) => ({ ...d, ...patch }));

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['sent-sms'],
    queryFn: () => fetchList('SentSMS/GetAll', {}),
  });

  // Filtering runs over the loaded log: SentSMS/GetAll has no filtered counterpart, and unlike the
  // provider log this table is one row per outbound message rather than per provider round-trip.
  const filtered = useMemo(() => {
    const from = new Date(`${applied.fromDate}T00:00:00`).getTime();
    const to = new Date(`${applied.toDate}T23:59:59`).getTime();
    const q = applied.search.trim().toLowerCase();
    return rows.filter((r) => {
      const raw = getField(r, 'DateTime');
      const t = raw ? new Date(String(raw)).getTime() : NaN;
      if (!Number.isNaN(t) && (t < from || t > to)) return false;

      if (applied.channel) {
        const ch = resolveChannel(str(getField(r, 'Type')), str(getField(r, 'WhatsAppNumber')));
        if (ch !== applied.channel) return false;
      }

      if (!q) return true;
      return ['UserName', 'Mobile', 'WhatsAppNumber', 'TemplateID', 'Message', 'Request', 'Response']
        .some((k) => str(getField(r, k)).toLowerCase().includes(q));
    });
  }, [rows, applied]);

  const paged = useMemo(() => filtered.slice(page * PAGE, page * PAGE + PAGE), [filtered, page]);
  const total = filtered.length;

  const runSearch = () => { setPage(0); setApplied(draft); };
  const reset = () => { const d = defaults(); setDraft(d); setApplied(d); setPage(0); };

  const columns: LogTableColumn<Row>[] = [
    { label: 'Date / Time', render: (r) => <span className="whitespace-nowrap text-slate-500">{dateTime(str(getField(r, 'DateTime')))}</span> },
    {
      label: 'Channel',
      render: (r) => <ChannelChip channel={resolveChannel(str(getField(r, 'Type')), str(getField(r, 'WhatsAppNumber')))} />,
    },
    { label: 'Entity', render: (r) => <span className="font-medium text-slate-800">{str(getField(r, 'UserName')) || <Dash />}</span> },
    {
      label: 'Sent to',
      render: (r) => {
        const to = str(getField(r, 'Mobile')) || str(getField(r, 'WhatsAppNumber'));
        return to ? <span className="tabular-nums text-slate-700">{to}</span> : <Dash />;
      },
    },
    { label: 'Template', render: (r) => str(getField(r, 'TemplateID')) || <Dash /> },
  ];

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <ReportActions
            total={total}
            loading={isFetching}
            onRefresh={() => refetch()}
            onExport={() => downloadCsv(filtered, 'Sent SMS')}
            exportDisabled={filtered.length === 0}
          />
        }
      />

      <ReportFilterBar onSearch={runSearch} onReset={reset} loading={isFetching}>
            <Input label="From date" type="date" max={draft.toDate} value={draft.fromDate} onChange={(e) => upd({ fromDate: e.target.value })} />
            <Input label="To date" type="date" value={draft.toDate} onChange={(e) => upd({ toDate: e.target.value })} />
            <Select label="Channel" value={draft.channel} onChange={(e) => upd({ channel: e.target.value })}>
              <option value="">All channels</option>
              {CHANNELS.map((c) => <option key={c} value={c}>{c}</option>)}
            </Select>
            <Input
              label="Search"
              placeholder="Number, entity, template or message text…"
              value={draft.search}
              onChange={(e) => upd({ search: e.target.value })}
            />
      </ReportFilterBar>

      <LogTable
        columns={columns}
        payloads={[
          { label: 'Message', value: (r) => str(getField(r, 'Message')) },
          { label: 'Gateway request', value: (r) => str(getField(r, 'Request')) },
          { label: 'Gateway response', value: (r) => str(getField(r, 'Response')) },
        ]}
        rows={paged}
        loading={isFetching && rows.length === 0}
        rowKey={(r, i) => String(getField(r, 'SentSMSID') ?? `${page}-${i}`)}
        startIndex={page * PAGE + 1}
        emptyTitle="No messages"
        emptyDescription="Nothing matches these filters, or no message has been sent in this period."
      />

      {total > 0 && <ReportPager page={page} pageSize={PAGE} total={total} onPage={setPage} />}
    </>
  );
}

function Dash() {
  return <span className="text-slate-300">—</span>;
}

function ChannelChip({ channel }: { channel: Channel }) {
  const styles: Record<Channel, string> = {
    WhatsApp: 'bg-green-50 text-green-700 ring-green-200',
    Mail: 'bg-amber-50 text-amber-700 ring-amber-200',
    SMS: 'bg-sky-50 text-sky-700 ring-sky-200',
  };
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${styles[channel]}`}>
      {channel}
    </span>
  );
}
