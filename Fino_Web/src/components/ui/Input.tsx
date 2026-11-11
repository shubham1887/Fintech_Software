import { forwardRef, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/lib/cn';
import { DatePicker } from './DatePicker';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: ReactNode;
  error?: string;
  hint?: string;
  leftIcon?: ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, hint, leftIcon, className, id, name, type, min, max, value, onChange, placeholder, disabled, onKeyDown, ...rest },
  ref,
) {
  const inputId = id ?? name;

  // Date fields use the app's custom calendar picker instead of the browser-native control.
  if (type === 'date') {
    return (
      <DatePicker
        label={label}
        error={error}
        className={className}
        id={inputId}
        name={name}
        value={value}
        onChange={onChange}
        min={min}
        max={max}
        disabled={disabled}
        placeholder={placeholder}
      />
    );
  }

  const isNumber = type === 'number';
  // Numeric fields are positive-only unless the caller explicitly passes a min:
  // default min to 0 and swallow the keys that would enter a negative / exponent value.
  const numberMin = isNumber ? (min ?? 0) : min;
  const handleKeyDown: InputHTMLAttributes<HTMLInputElement>['onKeyDown'] = (e) => {
    if (isNumber && (e.key === '-' || e.key === '+' || e.key === 'e' || e.key === 'E')) {
      e.preventDefault();
      return;
    }
    onKeyDown?.(e);
  };
  return (
    <div className="w-full">
      {label && (
        <label htmlFor={inputId} className="mb-1.5 block text-sm font-medium text-slate-700">
          {label}
        </label>
      )}
      <div className="relative">
        {leftIcon && (
          <span className="pointer-events-none absolute inset-y-0 left-0 grid w-11 place-items-center text-slate-400 md:w-10">
            {leftIcon}
          </span>
        )}
        <input
          id={inputId}
          name={name}
          ref={ref}
          type={type}
          min={numberMin}
          max={max}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          disabled={disabled}
          onKeyDown={handleKeyDown}
          className={cn(
            'h-12 w-full rounded-xl border border-slate-300 bg-white text-[15px] text-slate-800 shadow-sm',
            'md:h-10 md:rounded-lg md:text-sm',
            'transition-colors placeholder:text-slate-400',
            'focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30',
            // Horizontal padding is picked here rather than layered as "px-* … pl-*", because the
            // two cannot be mixed safely across breakpoints: a responsive `md:px-*` lands in a
            // media query, which the stylesheet emits AFTER the unprefixed `pl-*`, so the icon's
            // reserved space silently collapsed on desktop and the placeholder ran under the icon.
            // Keeping the cases mutually exclusive means no padding-left ever has to win a
            // specificity race. The left values match the icon box (w-11 / md:w-10) exactly.
            leftIcon ? 'pl-11 pr-3.5 md:pl-10 md:pr-3' : 'px-3.5 md:px-3',
            error && 'border-danger-500 focus:border-danger-500 focus:ring-danger-500/30',
            className,
          )}
          {...rest}
        />
      </div>
      {error ? (
        <p className="mt-1 text-xs text-danger-600">{error}</p>
      ) : hint ? (
        <p className="mt-1 text-xs text-slate-400">{hint}</p>
      ) : null}
    </div>
  );
});
