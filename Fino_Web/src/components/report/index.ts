/**
 * The common report system.
 *
 * Every report screen in the panel is built from these four pieces — header actions, filter bar,
 * summary strip, table, pager — so they behave identically no matter which report you open: the
 * filters sit in the same place, Search and Reset do the same thing, money is right-aligned and
 * formatted the same way, totals appear in the same row, and paging works the same.
 *
 * A screen still owns what is genuinely its own: which filter fields it needs, which columns it
 * shows, and what a row's actions do. Everything around that comes from here.
 */
export { ReportActions } from './ReportActions';
export { ReportFilterBar } from './ReportFilterBar';
export { ReportSummary, summaryFromRecord, type ReportSummaryItem } from './ReportSummary';
export { ReportTable, type ReportTableColumn } from './ReportTable';
export { ReportPager, REPORT_PAGE_SIZES } from './ReportPager';
export { StatRow, type StatRowItem } from './StatRow';
export { operatorsForService } from './filterOptions';
export {
  formatReportCell,
  formatSummaryValue,
  humanize,
  isNumericType,
  MONEYISH,
  type ReportCellType,
} from './reportFormat';
