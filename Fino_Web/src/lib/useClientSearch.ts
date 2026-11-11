import { useMemo, useState } from 'react';

/**
 * The search a list screen runs in the browser: case-insensitive, across every value of a row.
 *
 * Exported on its own as well as through the hook, because an export has to narrow the rows it
 * fetched by the same search the table is showing — otherwise the file holds the whole table
 * while the screen shows a filtered slice of it.
 */
export function filterRows<T extends Record<string, unknown>>(rows: T[], query: string): T[] {
  const q = query.trim().toLowerCase();
  if (!q) return rows;
  return rows.filter((r) =>
    Object.values(r).some((v) => v != null && String(v).toLowerCase().includes(q)),
  );
}

/** Client-side, case-insensitive search across all values of each row. */
export function useClientSearch<T extends Record<string, unknown>>(rows: T[]) {
  const [query, setQuery] = useState('');
  const filtered = useMemo(() => filterRows(rows, query), [rows, query]);
  return { query, setQuery, filtered };
}
