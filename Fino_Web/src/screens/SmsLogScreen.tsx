import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { RefreshCw, Eye } from 'lucide-react';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { dateTime } from '@/lib/format';
import { useClientSearch } from '@/lib/useClientSearch';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { Skeleton } from '@/components/ui/Skeleton';
import { TableToolbar } from '@/components/ui/TableToolbar';
import { Modal } from '@/components/ui/Modal';
import { exportToCsv } from '@/lib/csv';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
const str = (v: unknown) => String(v ?? '');
const PAGE = 25;

/**
 * Sent SMS — outbound message log rendered as a clean table (channel, user, mobile, template,
 * time). The long Message / Request / Response payloads are kept out of the grid and shown in a
 * details dialog opened from the row's View button, so the table stays tidy and nothing is
 * truncated. Raw ids are hidden.
 */
export function SmsLogScreen({ def }: CustomScreenProps) {
  const [page, setPage] = useState(0);
  const [detail, setDetail] = useState<Row | null>(null);

  const { data: rows = [], isFetching, refetch } = useQuery({
    queryKey: ['sent-sms'],
    queryFn: () => fetchList('SentSMS/GetAll', {}),
  });
  const { query, setQuery, filtered } = useClientSearch(rows);

  const paged = useMemo(() => filtered.slice(page * PAGE, page * PAGE + PAGE), [filtered, page]);
  const total = filtered.length;
  const to = Math.min((page + 1) * PAGE, total);

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={pickIcon(def.title)}
        actions={
          <Button variant="outline" size="sm" onClick={() => refetch()} loading={isFetching}>
            <RefreshCw className="size-4" />
            Refresh
          </Button>
        }
      />
      <TableToolbar query={query} onQuery={(v) => { setQuery(v); setPage(0); }} count={total} onExport={() => exportToCsv(filtered, 'Sent SMS')} />

      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                <th className="w-12 px-4 py-3 text-right">#</th>
                <th className="px-4 py-3">Channel</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Mobile</th>
                <th className="px-4 py-3">Template</th>
                <th className="px-4 py-3">Message</th>
                <th className="px-4 py-3">Date / Time</th>
                <th className="px-4 py-3 text-right">Details</th>
              </tr>
            </thead>
            <tbody>
              {isFetching ? (
                Array.from({ length: 8 }).map((_, r) => (
                  <tr key={r} className="border-b border-slate-100 last:border-0">
                    {Array.from({ length: 8 }).map((__, c) => (
                      <td key={c} className="px-4 py-3"><Skeleton className="h-4 w-full max-w-[120px]" /></td>
                    ))}
                  </tr>
                ))
              ) : paged.length === 0 ? (
                <tr>
                  <td colSpan={8}>
                    <EmptyState title="No SMS logs" description="Nothing matches your search, or no SMS has been sent yet." />
                  </td>
                </tr>
              ) : (
                paged.map((r, i) => {
                  const serial = page * PAGE + i + 1;
                  const channel = resolveChannel(str(getField(r, 'Type')), str(getField(r, 'WhatsAppNumber')));
                  return (
                    <tr key={i} className="border-b border-slate-100 transition-colors last:border-0 even:bg-slate-50/40 hover:bg-brand-50/40">
                      <td className="px-4 py-3 text-right tabular-nums text-slate-400">{serial}</td>
                      <td className="px-4 py-3"><ChannelChip channel={channel} /></td>
                      <td className="px-4 py-3 font-medium text-slate-800">{str(getField(r, 'UserName')) || <Dash />}</td>
                      <td className="whitespace-nowrap px-4 py-3 tabular-nums text-slate-700">{str(getField(r, 'Mobile')) || <Dash />}</td>
                      <td className="px-4 py-3 text-slate-600">{str(getField(r, 'TemplateID')) || <Dash />}</td>
                      <td className="max-w-[24rem] truncate px-4 py-3 text-slate-600" title={str(getField(r, 'Message'))}>
                        {str(getField(r, 'Message')) || <Dash />}
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-500">{dateTime(str(getField(r, 'DateTime')))}</td>
                      <td className="whitespace-nowrap px-4 py-3 text-right">
                        <Button variant="ghost" size="sm" onClick={() => setDetail(r)}>
                          <Eye className="size-4" /> View
                        </Button>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
        {!isFetching && total > 0 && (
          <div className="flex items-center justify-between border-t border-slate-100 bg-slate-50/40 px-4 py-2.5 text-xs text-slate-500">
            <span>{page * PAGE + 1}–{to} of {total}</span>
            {total > PAGE && (
              <div className="flex gap-2">
                <Button variant="outline" size="sm" disabled={page === 0} onClick={() => setPage((p) => p - 1)}>Prev</Button>
                <Button variant="outline" size="sm" disabled={to >= total} onClick={() => setPage((p) => p + 1)}>Next</Button>
              </div>
            )}
          </div>
        )}
      </div>

      <Modal open={Boolean(detail)} onClose={() => setDetail(null)} title="SMS details" size="lg">
        {detail && (
          <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-sm">
              <ChannelChip channel={resolveChannel(str(getField(detail, 'Type')), str(getField(detail, 'WhatsAppNumber')))} />
              <Meta label="User" value={str(getField(detail, 'UserName'))} />
              <Meta label="Mobile" value={str(getField(detail, 'Mobile'))} />
              <Meta label="WhatsApp" value={str(getField(detail, 'WhatsAppNumber'))} />
              <Meta label="Template" value={str(getField(detail, 'TemplateID'))} />
              <Meta label="Date" value={dateTime(str(getField(detail, 'DateTime')))} />
            </div>
            <LogBlock label="Message" value={str(getField(detail, 'Message')) || '—'} />
            <LogBlock label="Request" value={str(getField(detail, 'Request')) || '—'} />
            <LogBlock label="Response" value={str(getField(detail, 'Response')) || '—'} />
          </div>
        )}
      </Modal>
    </>
  );
}

function Dash() {
  return <span className="text-slate-300">—</span>;
}

function Meta({ label, value }: { label: string; value: string }) {
  if (!value) return null;
  return (
    <span>
      <span className="text-slate-400">{label}:</span> <span className="font-medium text-slate-800">{value}</span>
    </span>
  );
}

/** Map the stored Type / WhatsApp fields onto the three visible channels. */
function resolveChannel(type: string, whatsApp: string): 'WhatsApp' | 'Mail' | 'SMS' {
  const t = type.toLowerCase();
  if (whatsApp.trim() || t.includes('whats')) return 'WhatsApp';
  if (t.includes('mail') || t.includes('email')) return 'Mail';
  return 'SMS';
}

function ChannelChip({ channel }: { channel: 'WhatsApp' | 'Mail' | 'SMS' }) {
  const styles: Record<string, string> = {
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

function LogBlock({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</p>
      <pre className="max-h-56 overflow-auto whitespace-pre-wrap break-all rounded-lg bg-slate-50 p-3 text-xs leading-relaxed text-slate-700">
        {value}
      </pre>
    </div>
  );
}
