import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { Check, ChevronDown, Search } from 'lucide-react';
import { cn } from '@/lib/cn';

export interface AutocompleteOption {
  value: string | number;
  label: string;
  sublabel?: string;
}

/**
 * Reusable combobox: a text field that filters a list of options and emits the chosen
 * value. Used for the "Select Entity" user pickers in reports. Filtering is client-side.
 */
export function Autocomplete({
  label,
  placeholder = 'Type to search…',
  options,
  value,
  onChange,
  loading = false,
  error,
  className,
}: {
  label?: ReactNode;
  placeholder?: string;
  options: AutocompleteOption[];
  value: string | number | '';
  onChange: (value: string | number | '', option?: AutocompleteOption) => void;
  loading?: boolean;
  error?: string;
  className?: string;
}) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');
  const boxRef = useRef<HTMLDivElement>(null);

  const selected = useMemo(() => options.find((o) => String(o.value) === String(value)), [options, value]);

  // When a value is chosen elsewhere, mirror its label into the field.
  useEffect(() => {
    if (!open) setQuery(selected?.label ?? '');
  }, [selected, open]);

  // Close on outside click.
  useEffect(() => {
    const onDoc = (e: MouseEvent) => {
      if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', onDoc);
    return () => document.removeEventListener('mousedown', onDoc);
  }, []);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q || (selected && q === selected.label.toLowerCase())) return options.slice(0, 50);
    return options.filter((o) => o.label.toLowerCase().includes(q) || o.sublabel?.toLowerCase().includes(q)).slice(0, 50);
  }, [options, query, selected]);

  return (
    <div className={cn('w-full', className)} ref={boxRef}>
      {label && <label className="mb-1.5 block text-sm font-medium text-slate-700">{label}</label>}
      <div className="relative">
        <span className="pointer-events-none absolute inset-y-0 left-0 grid w-10 place-items-center text-slate-400">
          <Search className="size-4" />
        </span>
        <input
          value={query}
          placeholder={loading ? 'Loading…' : placeholder}
          onChange={(e) => {
            setQuery(e.target.value);
            setOpen(true);
            if (e.target.value === '') onChange('');
          }}
          onFocus={() => setOpen(true)}
          className={cn(
            'h-10 w-full rounded-lg border border-slate-300 bg-white pl-10 pr-9 text-sm text-slate-800 shadow-sm',
            'transition-colors placeholder:text-slate-400',
            'focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30',
            error && 'border-danger-500 focus:border-danger-500 focus:ring-danger-500/30',
          )}
        />
        <span className="pointer-events-none absolute inset-y-0 right-0 grid w-9 place-items-center text-slate-400">
          <ChevronDown className="size-4" />
        </span>

        {open && (
          <div className="absolute z-30 mt-1 max-h-64 w-full overflow-auto rounded-lg border border-slate-200 bg-white py-1 shadow-lg">
            {filtered.length === 0 ? (
              <p className="px-3 py-2 text-sm text-slate-400">No matches</p>
            ) : (
              filtered.map((o) => {
                const active = String(o.value) === String(value);
                return (
                  <button
                    key={String(o.value)}
                    type="button"
                    onClick={() => {
                      onChange(o.value, o);
                      setQuery(o.label);
                      setOpen(false);
                    }}
                    className={cn(
                      'flex w-full items-center justify-between gap-2 px-3 py-2 text-left text-sm transition-colors hover:bg-slate-50',
                      active && 'bg-brand-50 text-brand-700',
                    )}
                  >
                    <span className="min-w-0">
                      <span className="block truncate">{o.label}</span>
                      {o.sublabel && <span className="block truncate text-xs text-slate-400">{o.sublabel}</span>}
                    </span>
                    {active && <Check className="size-4 shrink-0 text-brand-600" />}
                  </button>
                );
              })
            )}
          </div>
        )}
      </div>
      {error && <p className="mt-1 text-xs text-danger-600">{error}</p>}
    </div>
  );
}
