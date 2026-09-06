export type Row = Record<string, unknown>;

/**
 * Reads a field from a decrypted API row by trying several candidate names,
 * case-insensitively. API rows are PascalCase (JsonConvert) but field naming
 * varies across view models, so callers pass the likely aliases in priority order.
 */
export function getField(row: Row, ...names: string[]): unknown {
  for (const n of names) {
    if (n in row) return row[n];
    const k = Object.keys(row).find((key) => key.toLowerCase() === n.toLowerCase());
    if (k) return row[k];
  }
  return undefined;
}
