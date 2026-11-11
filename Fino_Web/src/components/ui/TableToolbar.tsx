import { Download, Search } from 'lucide-react';
import { Input } from './Input';
import { Button } from './Button';

export function TableToolbar({
  query,
  onQuery,
  onExport,
  count,
  exportLoading,
}: {
  query: string;
  onQuery: (v: string) => void;
  onExport: () => void;
  count: number;
  /** The export re-queries for every matching row, so it is not instant on a big list. */
  exportLoading?: boolean;
}) {
  return (
    <div className="mb-3 flex items-center gap-3">
      <div className="w-full max-w-xs">
        <Input
          placeholder="Search…"
          value={query}
          onChange={(e) => onQuery(e.target.value)}
          leftIcon={<Search className="size-4" />}
        />
      </div>
      <div className="flex-1" />
      <span className="hidden text-sm text-slate-400 sm:inline">{count} rows</span>
      <Button variant="outline" size="sm" onClick={onExport} disabled={count === 0} loading={exportLoading}>
        <Download className="size-4" />
        Export
      </Button>
    </div>
  );
}
