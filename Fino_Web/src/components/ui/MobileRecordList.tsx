import { useState, type ReactNode } from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';

export interface RecordField {
  key: string;
  label: string;
  render: (row: unknown) => ReactNode;
}

/**
 * A table rendered as a list of cards, for phones.
 *
 * Every report in this console is a wide table, and a wide table on a 375px screen becomes a
 * horizontal scroll that nobody uses — the columns that matter end up off-screen. Here each
 * row is a card: the first column is the heading (it is the identifier in practically every
 * one of these reports), the next few columns sit under it as label/value pairs, and the rest
 * stay folded away until asked for.
 *
 * Shared by DataTable and DynamicTable so one fix covers every screen built on either.
 */
const ALWAYS_VISIBLE = 3;

export function MobileRecordList({
  fields,
  rows,
  rowKey,
  actions,
}: {
  fields: RecordField[];
  rows: unknown[];
  rowKey?: (row: unknown, index: number) => string | number;
  /** Per-row controls (edit, delete, …) — rendered at the foot of each card. */
  actions?: (row: unknown) => ReactNode;
}) {
  const [title, ...rest] = fields;
  const [openRow, setOpenRow] = useState<string | number | null>(null);

  return (
    <>
      {rows.map((row, i) => {
        const id = rowKey ? rowKey(row, i) : i;
        const open = openRow === id;
        const shown = open ? rest : rest.slice(0, ALWAYS_VISIBLE);
        const hidden = rest.length - ALWAYS_VISIBLE;

        return (
          <div key={id} className={cn('px-4 py-3', i > 0 && 'border-t border-slate-100')}>
            {title && (
              <div className="mb-1.5 text-sm font-semibold text-slate-800">{title.render(row)}</div>
            )}

            <dl className="grid grid-cols-[minmax(0,auto)_minmax(0,1fr)] gap-x-3 gap-y-1 text-xs">
              {shown.map((f) => (
                <div key={f.key} className="col-span-2 flex items-baseline justify-between gap-3">
                  <dt className="shrink-0 text-slate-400">{f.label}</dt>
                  <dd className="min-w-0 truncate text-right font-medium text-slate-700">
                    {f.render(row)}
                  </dd>
                </div>
              ))}
            </dl>

            <div className="mt-1.5 flex items-center justify-between gap-3">
              {hidden > 0 ? (
                <button
                  type="button"
                  onClick={() => {
                    haptic();
                    setOpenRow((cur) => (cur === id ? null : id));
                  }}
                  className="flex items-center gap-1 text-[11px] font-medium text-brand-600"
                >
                  {open ? 'Less' : `${hidden} more`}
                  <ChevronDown className={cn('size-3.5 transition-transform', open && 'rotate-180')} />
                </button>
              ) : (
                <span />
              )}
              {actions && <div className="flex shrink-0 items-center gap-2">{actions(row)}</div>}
            </div>
          </div>
        );
      })}
    </>
  );
}
