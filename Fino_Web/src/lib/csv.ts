type Row = Record<string, unknown>;

function cell(value: unknown): string {
  const s = value == null ? '' : typeof value === 'object' ? JSON.stringify(value) : String(value);
  return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
}

/**
 * Download an array of objects as a CSV file.
 *
 * Columns default to the union of every key on every row. Pass `only` to export a fixed list
 * instead — a caller whose screen hides fields by role has to, or the export hands back exactly
 * the fields the screen withheld.
 */
export function exportToCsv(rows: Row[], filename: string, only?: string[]): void {
  if (!rows.length) return;
  const keys = only ?? Array.from(
    rows.reduce<Set<string>>((set, r) => {
      Object.keys(r).forEach((k) => set.add(k));
      return set;
    }, new Set()),
  );
  const csv = [keys.join(','), ...rows.map((r) => keys.map((k) => cell(r[k])).join(','))].join('\n');
  const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `${filename.replace(/\s+/g, '_')}.csv`;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}
