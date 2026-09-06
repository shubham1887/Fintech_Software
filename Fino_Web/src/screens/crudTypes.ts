export type FieldType = 'text' | 'number' | 'checkbox' | 'select' | 'date' | 'password' | 'hidden' | 'image';

export interface FieldDef {
  name: string;
  label: string;
  type?: FieldType;
  required?: boolean;
  placeholder?: string;
  /** Static select options. */
  options?: { label: string; value: string | number }[];
  /** Async select: load options from a list endpoint (e.g. 'Service/GetAll'). */
  optionsEndpoint?: string;
  optionsPayload?: Record<string, unknown>;
  optionValue?: string; // row key used as the option value (e.g. 'ServiceID')
  optionLabel?: string; // row key used as the option label (e.g. 'Name')
  optionSublabel?: string | string[]; // row key(s) shown as a secondary line under the label (searchable selects); multiple are joined with " · "
  /** Render this select as a type-to-search dropdown (Autocomplete) instead of a plain list.
   *  Best for long option sets like user pickers. */
  searchable?: boolean;
  /**
   * Dependent (cascade) select: this field's options load only once the field named
   * `dependsOn` has a value, and that value is sent in the options payload under
   * `optionsPayloadKey` (defaults to `dependsOn`). Changing the parent clears this field.
   * Used e.g. by Fund Request: SubMode depends on Mode (SubMode/GetByModeId { ModeID }).
   */
  dependsOn?: string;
  optionsPayloadKey?: string;
  defaultValue?: string | number | boolean;
  colSpan?: 1 | 2;
  hint?: string;
  /**
   * For a select: render these fields from the *selected* option as a read-only text block
   * below the control (e.g. a sub-mode's bank / QR-UPI details). Only non-empty values show,
   * and images are never rendered — text only.
   */
  detailFields?: { key: string; label: string }[];
}

/** A standalone action form (kind 'form') — one DynamicForm POSTing to one endpoint. */
export interface FormConfig {
  endpoint: string;
  fields: FieldDef[];
  submitLabel?: string;
  successMessage?: string;
  description?: string;
}

/** Explicit table column (overrides DynamicTable's auto-derived columns when provided). */
export interface ColumnDef {
  /** Data key on each row. */
  key: string;
  /** Header label shown to the user. */
  label: string;
}

export interface CrudConfig {
  /** Primary-key field on each row (used for delete + row identity). */
  idField: string;
  list: { endpoint: string; payload?: Record<string, unknown> };
  /** When set, the list table shows exactly these columns (in order) instead of auto-deriving. */
  columns?: ColumnDef[];
  create?: { endpoint: string; fields: FieldDef[]; successMessage?: string };
  /**
   * Update behaviour. `mode: 'field'` (default) mirrors masters whose Update takes
   * { idField, FieldName, OldValue, NewValue } — editing diffs changed fields and sends
   * one call per change. `mode: 'object'` posts the whole record { idField, ...values } in
   * a single call (for masters like Slabs whose Update takes the full object).
   */
  update?: { endpoint: string; fields: FieldDef[]; successMessage?: string; mode?: 'field' | 'object' };
  remove?: { endpoint: string; successMessage?: string };
  /** Per-row navigation links (e.g. Slab → Commission setup). */
  links?: { label: string; to: (row: Record<string, unknown>) => string }[];
}
