import type { ReactNode } from 'react';
import { Download, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui/Button';

/**
 * The action cluster in a report's page header: how many rows matched, Export, Refresh, and
 * whatever else that report needs.
 *
 * Every report had these, in a different order, with different labels ("Export" / "Export page" /
 * a bare icon), and some with no row count at all. Passing the same component means a user learns
 * the controls once.
 */
export function ReportActions({
  total, loading, onRefresh, onExport, exportDisabled, exportLoading, exportLabel = 'Export', children,
}: {
  /** Rows the current filters matched — not the rows on screen. */
  total?: number;
  loading?: boolean;
  onRefresh?: () => void;
  onExport?: () => void;
  exportDisabled?: boolean;
  /** The export re-queries for every matching row, so it is not instant on a big report. */
  exportLoading?: boolean;
  exportLabel?: string;
  /** Report-specific buttons, placed before Export / Refresh. */
  children?: ReactNode;
}) {
  return (
    <>
      {typeof total === 'number' && (
        <span className="text-sm tabular-nums text-slate-400">{total} {total === 1 ? 'row' : 'rows'}</span>
      )}
      {children}
      {onExport && (
        <Button variant="outline" size="sm" onClick={onExport} disabled={exportDisabled} loading={exportLoading}>
          <Download className="size-4" />
          {exportLabel}
        </Button>
      )}
      {onRefresh && (
        <Button variant="outline" size="sm" onClick={onRefresh} loading={loading} title="Refresh">
          <RefreshCw className="size-4" />
        </Button>
      )}
    </>
  );
}
