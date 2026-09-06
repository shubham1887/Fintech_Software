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
            'h-12 w-full rounded-xl border border-slate-300 bg-white px-3.5 text-[15px] text-slate-800 shadow-sm',
            'md:h-10 md:rounded-lg md:px-3 md:text-sm',
            'transition-colors placeholder:text-slate-400',
            'focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/30',
            leftIcon && 'pl-10',
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
