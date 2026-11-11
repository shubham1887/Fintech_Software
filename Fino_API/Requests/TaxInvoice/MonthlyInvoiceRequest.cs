namespace FintechApp.Requests.TaxInvoice
{
    /// <summary>Which month to invoice, and whether to actually issue.</summary>
    public class MonthlyInvoiceRequest
    {
        /// <summary>yyyyMM. Empty means the month that has just ended — what the schedule does.</summary>
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>
        /// Work it all out and report it, but write nothing.
        ///
        /// Worth having for its own sake: the first run over a real month is the moment somebody
        /// finds out how much was never invoiced, and that figure should be readable before any
        /// document is numbered.
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// How many documents a party's top-ups become: TRANSACTION, DAILY, MONTHLY or AUTO.
        /// Empty uses the install's standing answer.
        /// </summary>
        public string Consolidation { get; set; } = string.Empty;

        /// <summary>
        /// Remember this grouping as the install's answer, so the unattended month-end run
        /// invoices the same way the person just chose to.
        ///
        /// Separate from <see cref="Consolidation"/> on purpose: previewing a month a different
        /// way should not quietly change what happens automatically next month.
        /// </summary>
        public bool SaveAsDefault { get; set; }
    }
}
