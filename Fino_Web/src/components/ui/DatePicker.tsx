import { useEffect, useMemo, useRef, useState, type ChangeEvent, type ReactNode } from 'react';
import {
  format, parseISO, isValid, startOfMonth, endOfMonth, startOfWeek, endOfWeek,
  eachDayOfInterval, addMonths, isSameDay, isSameMonth, isToday, startOfDay,
} from 'date-fns';
import { Calendar as CalendarIcon, ChevronLeft, ChevronRight } from 'lucide-react';
import { cn } from '@/lib/cn';

interface DatePickerProps {
  label?: ReactNode;
  error?: string;
  value?: string | number | readonly string[];
  onChange?: (e: ChangeEvent<HTMLInputElement>) => void;
  min?: string | number;
  max?: string | number;
  disabled?: boolean;
  placeholder?: string;
  name?: string;
  id?: string;
  className?: string;
}

const WEEKDAYS = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];

function toDate(v: unknown): Date | null {
  const s = v == null ? '' : String(v);
  if (!s) return null;
  const d = parseISO(s);
  return isValid(d) ? d : null;
}

/**
 * Custom calendar date picker that matches the app's design system (styled trigger + popup),
 * replacing the browser-native `<input type="date">`. It keeps a hidden native date input so the
 * value posts with forms and `onChange` fires with a standards-compliant event whose
 * `target.value` is the chosen `yyyy-MM-dd`. `Input` delegates here for every `type="date"` field.
 */
export function DatePicker({ label, error, value, onChange, min, max, disabled, placeholder, name, id, className }: DatePickerProps) {
  const val = value == null ? '' : String(value);
  const selected = toDate(val);
  const minD = toDate(min);
  const maxD = toDate(max);

  const [open, setOpen] = useState(false);
  const [view, setView] = useState<Date>(() => startOfMonth(selected ?? new Date()));
  const boxRef = useRef<HTMLDivElement>(null);
  const hiddenRef = useRef<HTMLInputElement>(null);

  // When opening, jump the calendar to the selected month (or this month).
  useEffect(() => { if (open) setView(startOfMonth(selected ?? new Date())); /* eslint-disable-next-line react-hooks/exhaustive-deps */ }, [open]);

  useEffect(() => {
    const onDoc = (e: MouseEvent) => { if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false); };
    document.addEventListener('mousedown', onDoc);
    return () => document.removeEventListener('mousedown', onDoc);
  }, []);

  const days = useMemo(
    () => eachDayOfInterval({ start: startOfWeek(startOfMonth(view)), end: endOfWeek(endOfMonth(view)) }),
    [view],
  );

  const isDisabledDay = (d: Date) => {
    if (minD && startOfDay(d) < startOfDay(minD)) return true;
    if (maxD && startOfDay(d) > startOfDay(maxD)) return true;
    return false;
  };

  const emit = (v: string) => {
    const el = hiddenRef.current;
    if (el && onChange) { el.value = v; onChange({ target: el, currentTarget: el } as ChangeEvent<HTMLInputElement>); }
  };
  const pick = (d: Date) => { if (isDisabledDay(d)) return; emit(format(d, 'yyyy-MM-dd')); setOpen(false); };

  const triggerId = id ?? name;

  return (
    <div className="w-full" ref={boxRef}>
      {label && (
        <label htmlFor={triggerId ? `${triggerId}-trigger` : undefined} className="mb-1.5 block text-sm font-medium text-slate-700">
          {label}
        </label>
      )}

      {/* Hidden native input — carries the value for forms and gives onChange a valid target. */}
      <input
        ref={hiddenRef}
        type="date"
        name={name}
        id={triggerId}
        value={val}
        min={min as string | undefined}
        max={max as string | undefined}
        disabled={disabled}
        onChange={() => {}}
        tabIndex={-1}
        aria-hidden="true"
        className="sr-only"
      />

      <div className="relative">
        <button
          type="button"
          id={triggerId ? `${triggerId}-trigger` : undefined}
          disabled={disabled}
          onClick={() => !disabled && setOpen((o) => !o)}
          className={cn(
            'flex h-10 w-full items-center justify-between gap-2 rounded-lg border border-slate-300 bg-white px-3 text-left text-sm shadow-sm',
            'transition-colors focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30',
            disabled && 'cursor-not-allowed bg-slate-50 text-slate-400',
            error && 'border-danger-500',
            className,
          )}
        >
          <span className={cn('truncate', selected ? 'text-slate-800' : 'text-slate-400')}>
            {selected ? format(selected, 'dd MMM yyyy') : (placeholder || 'Select date')}
          </span>
          <CalendarIcon className="size-4 shrink-0 text-slate-400" />
        </button>

        {open && !disabled && (
          <div className="absolute z-30 mt-1 w-[17.5rem] rounded-xl border border-slate-200 bg-white p-3 shadow-lg">
            <div className="mb-2 flex items-center justify-between">
              <button type="button" onClick={() => setView((v) => addMonths(v, -1))} className="grid size-7 place-items-center rounded-md text-slate-500 transition-colors hover:bg-slate-100">
                <ChevronLeft className="size-4" />
              </button>
              <span className="text-sm font-semibold text-slate-700">{format(view, 'MMMM yyyy')}</span>
              <button type="button" onClick={() => setView((v) => addMonths(v, 1))} className="grid size-7 place-items-center rounded-md text-slate-500 transition-colors hover:bg-slate-100">
                <ChevronRight className="size-4" />
              </button>
            </div>

            <div className="mb-1 grid grid-cols-7 gap-0.5">
              {WEEKDAYS.map((w) => (
                <div key={w} className="grid h-7 place-items-center text-[11px] font-medium text-slate-400">{w}</div>
              ))}
            </div>

            <div className="grid grid-cols-7 gap-0.5">
              {days.map((d, i) => {
                const inMonth = isSameMonth(d, view);
                const sel = selected && isSameDay(d, selected);
                const today = isToday(d);
                const dis = isDisabledDay(d);
                return (
                  <button
                    key={i}
                    type="button"
                    disabled={dis}
                    onClick={() => pick(d)}
                    className={cn(
                      'grid size-8 place-items-center rounded-md text-sm tabular-nums transition-colors',
                      dis && 'cursor-not-allowed text-slate-200',
                      !dis && !sel && (inMonth ? 'text-slate-700 hover:bg-slate-100' : 'text-slate-300 hover:bg-slate-50'),
                      !dis && today && !sel && 'ring-1 ring-inset ring-brand-400',
                      sel && 'bg-brand-600 font-semibold text-white',
                    )}
                  >
                    {format(d, 'd')}
                  </button>
                );
              })}
            </div>

            <div className="mt-2 flex items-center justify-between border-t border-slate-100 pt-2">
              <button
                type="button"
                onClick={() => { const t = new Date(); if (!isDisabledDay(t)) { setView(startOfMonth(t)); pick(t); } }}
                className="text-xs font-medium text-brand-600 transition-colors hover:text-brand-700"
              >
                Today
              </button>
              {selected && (
                <button type="button" onClick={() => { emit(''); setOpen(false); }} className="text-xs font-medium text-slate-400 transition-colors hover:text-slate-600">
                  Clear
                </button>
              )}
            </div>
          </div>
        )}
      </div>

      {error && <p className="mt-1 text-xs text-danger-600">{error}</p>}
    </div>
  );
}
