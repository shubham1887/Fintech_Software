import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Upload, X, ImageIcon, Building2, QrCode } from 'lucide-react';
import { fetchList } from '@/api/list.api';
import { uploadImage, imageUrl } from '@/api/catalog.api';
import { cn } from '@/lib/cn';
import { Input } from './Input';
import { Select } from './Select';
import { Autocomplete, type AutocompleteOption } from './Autocomplete';
import { Button } from './Button';
import { useToast } from './Toast';
import type { FieldDef } from '@/screens/crudTypes';

type Values = Record<string, string | number | boolean>;

/** `optionsFilter` applied to a loaded option list — see FieldDef. */
function applyOptionsFilter(rows: Record<string, unknown>[], field: FieldDef): Record<string, unknown>[] {
  const f = field.optionsFilter;
  if (!f || !f.pattern) return rows;
  let re: RegExp;
  try { re = new RegExp(f.pattern, 'i'); } catch { return rows; }
  return rows.filter((r) => re.test(f.keys.map((k) => String(r[k] ?? '')).join(' ')));
}

/** Field label with a red asterisk appended when the field is required. */
function withRequired(field: FieldDef) {
  if (!field.required) return field.label;
  return (
    <>
      {field.label} <span className="text-danger-500">*</span>
    </>
  );
}

function initialValues(fields: FieldDef[], initial?: Record<string, unknown>): Values {
  const v: Values = {};
  for (const f of fields) {
    const provided = initial?.[f.name];
    if (provided !== undefined && provided !== null) {
      v[f.name] = f.type === 'checkbox' ? Boolean(provided) : (provided as string | number);
    } else if (f.defaultValue !== undefined) {
      v[f.name] = f.defaultValue;
    } else {
      v[f.name] = f.type === 'checkbox' ? false : '';
    }
  }
  return v;
}

/**
 * Converts form strings to the types the API expects (numbers, booleans).
 *
 * `skip` holds the fields that were never on screen — a cascade child whose parent has no value
 * yet, or one whose parent turned out to have no options at all (Fund Request with Mode =
 * "Others", which has no sub modes). Those are LEFT OUT of the payload entirely rather than sent
 * as "". The API's request models type those ids as `int`, and an empty string is not an int: the
 * server could only answer "An error occurred while processing your request." for a form the user
 * had filled in correctly. Omitted, the field simply takes its default and the server decides
 * whether it was needed.
 *
 * A select the user COULD see and chose to leave blank is still sent as "": on an edit form that
 * is how a value is cleared, and the API reads a blank number as "not supplied" anyway.
 */
function coerce(values: Values, fields: FieldDef[], skip?: (f: FieldDef) => boolean): Record<string, unknown> {
  const out: Record<string, unknown> = {};
  for (const f of fields) {
    if (skip?.(f)) continue;
    const raw = values[f.name];
    if (f.type === 'number') out[f.name] = raw === '' || raw === undefined ? 0 : Math.max(0, Number(raw));
    else if (f.type === 'checkbox') out[f.name] = Boolean(raw);
    else out[f.name] = raw ?? '';
  }
  return out;
}

/** Select whose options are loaded from a list endpoint (foreign-key picker). */
function AsyncSelectField({
  field,
  value,
  error,
  onChange,
  dependsValue,
  onOptionCount,
  onSelectedRow,
}: {
  field: FieldDef;
  value: string;
  error?: string;
  onChange: (v: string) => void;
  /** Current value of the field named by `field.dependsOn` (cascade selects only). */
  dependsValue?: string;
  /** Reports how many options loaded (once ready & not loading) so the parent can hide an empty cascade. */
  onOptionCount?: (count: number) => void;
  /** Reports the currently-selected option row (or null) so the parent can render its detail panel. */
  onSelectedRow?: (row: Record<string, unknown> | null) => void;
}) {
  // Cascade selects wait for their parent; the parent's value is merged into the payload.
  const gated = Boolean(field.dependsOn);
  const ready = !gated || Boolean(dependsValue);
  const payload = gated
    ? { ...(field.optionsPayload ?? {}), [field.optionsPayloadKey ?? field.dependsOn!]: dependsValue }
    : field.optionsPayload ?? {};
  const { data: rawRows = [], isLoading } = useQuery({
    queryKey: ['fk-options', field.optionsEndpoint, payload],
    queryFn: () => fetchList(field.optionsEndpoint!, payload),
    staleTime: 60_000,
    enabled: ready,
  });
  const rows = useMemo(() => applyOptionsFilter(rawRows, field), [rawRows, field]);
  const valueKey = field.optionValue ?? 'id';
  const labelKey = field.optionLabel ?? valueKey;
  const placeholder = !ready ? 'Select the previous field first…' : isLoading ? 'Loading…' : 'Select…';

  // Report the loaded option count upward (via a ref so the effect stays deps-clean).
  const countCb = useRef(onOptionCount);
  countCb.current = onOptionCount;
  useEffect(() => {
    if (ready && !isLoading) countCb.current?.(rows.length);
  }, [ready, isLoading, rows.length]);

  const selectedRow = rows.find((r) => String(r[valueKey] ?? '') === String(value)) ?? null;

  // Report the selected row upward so the parent can render its detail panel full-width.
  const rowCb = useRef(onSelectedRow);
  rowCb.current = onSelectedRow;
  useEffect(() => {
    rowCb.current?.(selectedRow);
  }, [selectedRow]);

  return (
    <Select label={withRequired(field)} value={value} error={error} disabled={!ready} onChange={(e) => onChange(e.target.value)}>
      <option value="">{placeholder}</option>
      {rows.map((r, i) => {
        const v = String(r[valueKey] ?? '');
        return (
          <option key={`${v}-${i}`} value={v}>
            {String(r[labelKey] ?? v)}
          </option>
        );
      })}
    </Select>
  );
}

/** Searchable variant of the async select: a type-to-search Autocomplete backed by the same
 *  list endpoint. Used for long option sets (user pickers). Supports the same cascade / option-
 *  count / selected-row reporting as AsyncSelectField. */
function AsyncAutocompleteField({
  field,
  value,
  error,
  onChange,
  dependsValue,
  onOptionCount,
  onSelectedRow,
}: {
  field: FieldDef;
  value: string;
  error?: string;
  onChange: (v: string) => void;
  dependsValue?: string;
  onOptionCount?: (count: number) => void;
  onSelectedRow?: (row: Record<string, unknown> | null) => void;
}) {
  const gated = Boolean(field.dependsOn);
  const ready = !gated || Boolean(dependsValue);
  const payload = gated
    ? { ...(field.optionsPayload ?? {}), [field.optionsPayloadKey ?? field.dependsOn!]: dependsValue }
    : field.optionsPayload ?? {};
  const { data: rawRows = [], isLoading } = useQuery({
    queryKey: ['fk-options', field.optionsEndpoint, payload],
    queryFn: () => fetchList(field.optionsEndpoint!, payload),
    staleTime: 60_000,
    enabled: ready,
  });
  const rows = useMemo(() => applyOptionsFilter(rawRows, field), [rawRows, field]);
  const valueKey = field.optionValue ?? 'id';
  const labelKey = field.optionLabel ?? valueKey;
  const subKeys = field.optionSublabel == null ? [] : Array.isArray(field.optionSublabel) ? field.optionSublabel : [field.optionSublabel];

  const countCb = useRef(onOptionCount);
  countCb.current = onOptionCount;
  useEffect(() => {
    if (ready && !isLoading) countCb.current?.(rows.length);
  }, [ready, isLoading, rows.length]);

  const selectedRow = rows.find((r) => String(r[valueKey] ?? '') === String(value)) ?? null;
  const rowCb = useRef(onSelectedRow);
  rowCb.current = onSelectedRow;
  useEffect(() => {
    rowCb.current?.(selectedRow);
  }, [selectedRow]);

  const options: AutocompleteOption[] = rows.map((r) => {
    const sub = subKeys.map((k) => String(r[k] ?? '').trim()).filter(Boolean).join(' · ');
    return {
      value: String(r[valueKey] ?? ''),
      label: String(r[labelKey] ?? r[valueKey] ?? ''),
      sublabel: sub || undefined,
    };
  });

  return (
    <Autocomplete
      label={withRequired(field)}
      options={options}
      value={value}
      error={error}
      loading={isLoading}
      placeholder={!ready ? 'Select the previous field first…' : 'Type to search…'}
      onChange={(v) => onChange(String(v))}
    />
  );
}

/** Read-only summary card of the selected option's chosen fields (e.g. a sub-mode's bank / QR
 *  details): a titled card with aligned label→value rows. Only non-empty values are shown, and
 *  never as an image — text only. Account numbers / IFSC / UPI render monospaced for legibility. */
const MONO_KEYS = new Set(['AccountNumber', 'IFSC', 'QRCode', 'UPI']);
function OptionDetails({ fields, row }: { fields: { key: string; label: string }[]; row: Record<string, unknown> }) {
  const items = fields
    .map((d) => ({ key: d.key, label: d.label, value: String(row[d.key] ?? '').trim() }))
    .filter((x) => x.value !== '');
  if (!items.length) return null;

  // Title + icon inferred from which details the option carries (bank vs QR/UPI).
  const isQr = !String(row['BankName'] ?? '').trim() && !String(row['AccountNumber'] ?? '').trim() && Boolean(String(row['QRCode'] ?? '').trim());
  const Icon = isQr ? QrCode : Building2;
  const heading = isQr ? 'UPI / QR details' : 'Account details';

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <div className="flex items-center gap-2 border-b border-slate-100 bg-slate-50/70 px-3.5 py-2">
        <Icon className="size-3.5 text-brand-600" />
        <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-500">{heading}</span>
      </div>
      <dl className="divide-y divide-slate-100">
        {items.map((it) => (
          <div key={it.key} className="grid grid-cols-[7rem_1fr] items-baseline gap-3 px-3.5 py-2">
            <dt className="text-xs font-medium text-slate-400">{it.label}</dt>
            <dd
              className={cn(
                'min-w-0 break-words text-sm font-semibold text-slate-800',
                MONO_KEYS.has(it.key) && 'font-mono tracking-tight tabular-nums',
              )}
            >
              {it.value}
            </dd>
          </div>
        ))}
      </dl>
    </div>
  );
}

/** Optional image / slip upload. Stores the uploaded public path as the field's string value.
 *  Failures are toasted and swallowed — the form never crashes on a bad file. */
export function ImageField({ field, value, onChange }: { field: FieldDef; value: string; onChange: (v: string) => void }) {
  const toast = useToast();
  const [busy, setBusy] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const pick = async (file?: File) => {
    if (!file) return;
    if (!file.type.startsWith('image/')) { toast.error('Please choose an image file.'); return; }
    setBusy(true);
    try {
      const path = await uploadImage(file, 'misc');
      onChange(path);
    } catch (e) {
      toast.error((e as Error).message || 'Upload failed.');
    } finally {
      setBusy(false);
      if (inputRef.current) inputRef.current.value = '';
    }
  };
  return (
    <div>
      <label className="mb-1.5 block text-sm font-medium text-slate-700">{field.label}</label>
      <div className="flex items-center gap-3">
        {value ? (
          <div className="relative">
            <img src={imageUrl(value)} alt={field.label} className="size-16 rounded-lg border border-slate-200 object-cover" />
            <button type="button" onClick={() => onChange('')}
              className="absolute -right-1.5 -top-1.5 rounded-full bg-danger-500 p-0.5 text-white shadow"><X className="size-3" /></button>
          </div>
        ) : (
          <div className="grid size-16 place-items-center rounded-lg border border-dashed border-slate-300 text-slate-300"><ImageIcon className="size-6" /></div>
        )}
        <Button type="button" variant="outline" size="sm" loading={busy} onClick={() => inputRef.current?.click()}>
          <Upload className="size-4" /> Upload
        </Button>
        <input ref={inputRef} type="file" accept="image/*" className="hidden" onChange={(e) => pick(e.target.files?.[0])} />
      </div>
      {field.hint && <p className="mt-1 text-xs text-slate-400">{field.hint}</p>}
    </div>
  );
}

export function DynamicForm({
  fields,
  initial,
  onSubmit,
  onCancel,
  submitting = false,
  submitLabel = 'Save',
}: {
  fields: FieldDef[];
  initial?: Record<string, unknown>;
  onSubmit: (values: Record<string, unknown>) => void;
  onCancel?: () => void;
  submitting?: boolean;
  submitLabel?: string;
}) {
  const [values, setValues] = useState<Values>(() => initialValues(fields, initial));
  const [errors, setErrors] = useState<Record<string, string>>({});
  // Loaded option count per async field name; used to hide a cascade select that has no options.
  const [optionCounts, setOptionCounts] = useState<Record<string, number>>({});
  // Selected option row per async field name; used to render that field's detail panel.
  const [detailRows, setDetailRows] = useState<Record<string, Record<string, unknown> | null>>({});

  const set = (name: string, value: string | number | boolean) => {
    setValues((s) => {
      const next = { ...s, [name]: value };
      // Clear any cascade child that depends on the field just changed.
      for (const child of fields) {
        if (child.dependsOn === name) next[child.name] = '';
      }
      return next;
    });
    // Forget the old option counts / detail rows of any child so it re-evaluates for the new parent.
    setOptionCounts((c) => {
      const next = { ...c };
      for (const child of fields) if (child.dependsOn === name) delete next[child.name];
      return next;
    });
    setDetailRows((d) => {
      const next = { ...d };
      for (const child of fields) if (child.dependsOn === name) delete next[child.name];
      return next;
    });
  };

  // A cascade select whose options have loaded empty is hidden (nothing to pick) and, being
  // hidden, must not block submission even if it is marked required.
  const isEmptyCascade = (f: FieldDef) => Boolean(f.dependsOn) && optionCounts[f.name] === 0;
  // A cascade child whose parent has no value yet is also not shown.
  const isParentPending = (f: FieldDef) => Boolean(f.dependsOn) && !String(values[f.dependsOn!] ?? '');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const errs: Record<string, string> = {};
    for (const f of fields) {
      if (isEmptyCascade(f) || isParentPending(f)) continue; // hidden fields aren't validated
      if (f.required && (values[f.name] === '' || values[f.name] === undefined || values[f.name] === null)) {
        errs[f.name] = `${f.label} is required`;
      }
    }
    setErrors(errs);
    if (Object.keys(errs).length === 0) {
      onSubmit(coerce(values, fields, (f) => isEmptyCascade(f) || isParentPending(f)));
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {fields.map((f) => {
          if (f.type === 'hidden') return null;
          // Cascade child stays hidden until its parent has a value (then it loads its options),
          // and stays hidden if that parent turned out to have no options to pick.
          if (isParentPending(f) || isEmptyCascade(f)) return null;
          const span = f.colSpan === 2 ? 'sm:col-span-2' : '';
          if (f.type === 'checkbox') {
            return (
              <label
                key={f.name}
                className={cn('flex cursor-pointer items-center gap-2.5 rounded-lg border border-slate-200 px-3 py-2.5', span)}
              >
                <input
                  type="checkbox"
                  checked={Boolean(values[f.name])}
                  onChange={(e) => set(f.name, e.target.checked)}
                  className="size-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500"
                />
                <span className="text-sm font-medium text-slate-700">{f.label}</span>
              </label>
            );
          }
          if (f.type === 'image') {
            return (
              <div key={f.name} className={span}>
                <ImageField field={f} value={String(values[f.name] ?? '')} onChange={(v) => set(f.name, v)} />
              </div>
            );
          }
          if (f.type === 'select') {
            const AsyncField = f.searchable ? AsyncAutocompleteField : AsyncSelectField;
            const control = (
              <div key={f.name} className={span}>
                {f.optionsEndpoint ? (
                  <AsyncField
                    field={f}
                    value={String(values[f.name] ?? '')}
                    error={errors[f.name]}
                    onChange={(v) => set(f.name, v)}
                    dependsValue={f.dependsOn ? String(values[f.dependsOn] ?? '') : undefined}
                    onOptionCount={
                      f.dependsOn
                        ? (n) => setOptionCounts((c) => (c[f.name] === n ? c : { ...c, [f.name]: n }))
                        : undefined
                    }
                    onSelectedRow={
                      f.detailFields
                        ? (row) => setDetailRows((d) => (d[f.name] === row ? d : { ...d, [f.name]: row }))
                        : undefined
                    }
                  />
                ) : (
                  <Select
                    label={withRequired(f)}
                    value={String(values[f.name] ?? '')}
                    error={errors[f.name]}
                    onChange={(e) => set(f.name, e.target.value)}
                  >
                    <option value="">Select…</option>
                    {(f.options ?? []).map((o) => (
                      <option key={String(o.value)} value={String(o.value)}>
                        {o.label}
                      </option>
                    ))}
                  </Select>
                )}
              </div>
            );
            // The selected option's detail panel renders as its own full-width row below the
            // Mode/Sub-mode pair — never squeezed into one column leaving the other blank.
            const detailRow = f.detailFields && detailRows[f.name] ? (
              <div key={`${f.name}-details`} className="sm:col-span-2">
                <OptionDetails fields={f.detailFields} row={detailRows[f.name]!} />
              </div>
            ) : null;
            return detailRow ? [control, detailRow] : control;
          }
          return (
            <div key={f.name} className={span}>
              <Input
                label={withRequired(f)}
                type={f.type === 'number' ? 'number' : f.type === 'date' ? 'date' : f.type === 'password' ? 'password' : 'text'}
                placeholder={f.placeholder}
                hint={f.hint}
                value={String(values[f.name] ?? '')}
                error={errors[f.name]}
                onChange={(e) => set(f.name, e.target.value)}
              />
            </div>
          );
        })}
      </div>

      <div className="mt-6 flex justify-end gap-2 border-t border-slate-100 pt-4">
        {onCancel && (
          <Button type="button" variant="outline" onClick={onCancel} disabled={submitting}>
            Cancel
          </Button>
        )}
        <Button type="submit" loading={submitting}>
          {submitLabel}
        </Button>
      </div>
    </form>
  );
}
