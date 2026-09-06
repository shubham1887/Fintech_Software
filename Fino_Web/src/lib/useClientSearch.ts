import { useMemo, useState } from 'react';

/** Client-side, case-insensitive search across all values of each row. */
export function useClientSearch<T extends Record<string, unknown>>(rows: T[]) {
  const [query, setQuery] = useState('');
  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return rows;
    return rows.filter((r) =>
      Object.values(r).some((v) => v != null && String(v).toLowerCase().includes(q)),
    );
  }, [rows, query]);
  return { query, setQuery, filtered };
}
