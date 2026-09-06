import {
  forwardRef,
  isValidElement,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ChangeEvent,
  type ReactNode,
  type SelectHTMLAttributes,
} from 'react';
import { Check, ChevronDown } from 'lucide-react';
import { cn } from '@/lib/cn';

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: ReactNode;
  error?: string;
}

interface Opt {
  value: string;
  label: string;
  disabled: boolean;
}

/** Flatten the `<option>` children (incl. those nested in fragments) into a plain list. */
function readOptions(children: ReactNode): Opt[] {
  const out: Opt[] = [];
  const walk = (node: ReactNode) => {
    if (node == null || node === false) return;
    if (Array.isArray(node)) {
      node.forEach(walk);
      return;
    }
    if (isValidElement(node)) {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const el = node as any;
      if (el.type === 'option') {
        const p = el.props ?? {};
        out.push({
          value: String(p.value ?? ''),
          label: typeof p.children === 'string' ? p.children : String(p.children ?? p.value ?? ''),
          disabled: Boolean(p.disabled),
        });
      } else if (el.props?.children) {
        walk(el.props.children);
      }
    }
  };
  walk(children);
  return out;
}

/**
 * Custom-styled dropdown that is a drop-in replacement for a native `<select>`: it accepts the
 * same `<option>` children, `value`, and `onChange` (called with a synthetic event whose
 * `target.value` is the chosen value). A hidden native `<select>` mirrors the value so form
 * submission, labels, and the `onChange` event object stay standards-compliant. The visible UI
 * is a fully themed button + popup with hover highlight and keyboard navigation.
 */
export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, error, className, id, name, children, value, defaultValue, onChange, disabled, ...rest },
  ref,
) {
  const selectId = id ?? name;
  const triggerId = selectId ? `${selectId}-trigger` : undefined;
  const options = useMemo(() => readOptions(children), [children]);

  const hiddenRef = useRef<HTMLSelectElement>(null);
  const setRefs = (el: HTMLSelectElement | null) => {
    hiddenRef.current = el;
    if (typeof ref === 'function') ref(el);
    else if (ref) (ref as React.MutableRefObject<HTMLSelectElement | null>).current = el;
  };

  // Controlled when `value` is supplied (all in-app callers are); otherwise track internally.
  const [internal, setInternal] = useState<string>(String(defaultValue ?? ''));
  const current = value !== undefined ? String(value) : internal;

  const [open, setOpen] = useState(false);
  const [highlight, setHighlight] = useState<number>(-1);
  const boxRef = useRef<HTMLDivElement>(null);

  const selected = options.find((o) => o.value === current);
  const displayLabel = selected?.label ?? '';
  const isPlaceholder = !selected || selected.value === '';

  // Close on outside click.
  useEffect(() => {
    const onDoc = (e: MouseEvent) => {
      if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', onDoc);
    return () => document.removeEventListener('mousedown', onDoc);
  }, []);

  // When opening, highlight the current selection so keyboard nav starts there.
  useEffect(() => {
    if (open) setHighlight(options.findIndex((o) => o.value === current));
  }, [open, options, current]);

  const commit = (opt: Opt) => {
    if (opt.disabled) return;
    if (value === undefined) setInternal(opt.value);
    const el = hiddenRef.current;
    if (el && onChange) {
      el.value = opt.value; // make e.target.value correct before firing
      onChange({ target: el, currentTarget: el } as ChangeEvent<HTMLSelectElement>);
    }
    setOpen(false);
  };

  const moveHighlight = (dir: 1 | -1) => {
    if (!options.length) return;
    let i = highlight;
    for (let step = 0; step < options.length; step++) {
      i = (i + dir + options.length) % options.length;
      if (!options[i].disabled) break;
    }
    setHighlight(i);
  };

  const onKeyDown: React.KeyboardEventHandler<HTMLButtonElement> = (e) => {
    if (disabled) return;
    if (!open) {
      if (e.key === 'ArrowDown' || e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        setOpen(true);
      }
      return;
    }
    if (e.key === 'ArrowDown') { e.preventDefault(); moveHighlight(1); }
    else if (e.key === 'ArrowUp') { e.preventDefault(); moveHighlight(-1); }
    else if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      if (highlight >= 0 && options[highlight]) commit(options[highlight]);
    } else if (e.key === 'Escape') { e.preventDefault(); setOpen(false); }
    else if (e.key === 'Tab') setOpen(false);
  };

  return (
    <div className="w-full" ref={boxRef}>
      {label && (
        <label htmlFor={triggerId} className="mb-1.5 block text-sm font-medium text-slate-700">
          {label}
        </label>
      )}

      {/* Hidden real <select> — carries the value for form posts and gives onChange a valid target. */}
      <select
        ref={setRefs}
        id={selectId}
        name={name}
        value={current}
        disabled={disabled}
        onChange={() => {}}
        tabIndex={-1}
        aria-hidden="true"
        className="sr-only"
        {...rest}
      >
        {children}
      </select>

      <div className="relative">
        <button
          type="button"
          id={triggerId}
          disabled={disabled}
          onClick={() => !disabled && setOpen((o) => !o)}
          onKeyDown={onKeyDown}
          role="combobox"
          aria-expanded={open}
          aria-haspopup="listbox"
          className={cn(
            'flex h-12 w-full items-center justify-between gap-2 rounded-xl border border-slate-300 bg-white px-3.5 text-left text-[15px] shadow-sm',
            'md:h-10 md:rounded-lg md:px-3 md:text-sm',
            'transition-colors focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30',
            disabled && 'cursor-not-allowed bg-slate-50 text-slate-400',
            error && 'border-danger-500',
            className,
          )}
        >
          <span className={cn('min-w-0 truncate', isPlaceholder ? 'text-slate-400' : 'text-slate-800')}>
            {displayLabel || 'Select…'}
          </span>
          <ChevronDown className={cn('size-4 shrink-0 text-slate-400 transition-transform', open && 'rotate-180')} />
        </button>

        {open && !disabled && (
          <ul
            role="listbox"
            className="absolute z-30 mt-1 max-h-64 w-full overflow-auto rounded-lg border border-slate-200 bg-white py-1 shadow-lg"
          >
            {options.length === 0 ? (
              <li className="px-3 py-2.5 text-[15px] text-slate-400 md:py-2 md:text-sm">No options</li>
            ) : (
              options.map((o, i) => {
                const active = o.value === current;
                const isHi = i === highlight;
                return (
                  <li key={`${o.value}-${i}`} role="option" aria-selected={active}>
                    <button
                      type="button"
                      disabled={o.disabled}
                      onMouseEnter={() => setHighlight(i)}
                      onClick={() => commit(o)}
                      className={cn(
                        'flex w-full items-center justify-between gap-2 px-3.5 py-3 text-left text-[15px] transition-colors',
                        'md:px-3 md:py-2 md:text-sm',
                        o.disabled && 'cursor-not-allowed text-slate-300',
                        !o.disabled && isHi && 'bg-slate-50',
                        active && 'bg-brand-50 text-brand-700',
                      )}
                    >
                      <span className="min-w-0 truncate">{o.label || <span className="text-slate-400">—</span>}</span>
                      {active && <Check className="size-4 shrink-0 text-brand-600" />}
                    </button>
                  </li>
                );
              })
            )}
          </ul>
        )}
      </div>

      {error && <p className="mt-1 text-xs text-danger-600">{error}</p>}
    </div>
  );
});
