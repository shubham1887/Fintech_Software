namespace FintechApp.Responses.TaxInvoice
{
    /// <summary>
    /// What a month-end invoicing run did, or would do.
    ///
    /// The same shape comes back from the preview and from the real run, so the screen shows the
    /// operator the figures they are about to commit to and then the figures that actually
    /// happened, rather than two different reports of the same act.
    /// </summary>
    public class MonthlyInvoiceResult
    {
        /// <summary>yyyyMM.</summary>
        public string PeriodKey { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        /// <summary>The registration the documents are raised under.</summary>
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>True when this was a preview and nothing was written.</summary>
        public bool DryRun { get; set; }

        /// <summary>True when documents were actually issued.</summary>
        public bool Issued { get; set; }

        public int DocumentsGenerated { get; set; }
        public int LinesGenerated { get; set; }
        public decimal TaxableTotal { get; set; }
        public decimal TaxTotal { get; set; }

        /// <summary>Sources already carried by a document — how a re-run reports doing nothing.</summary>
        public int SkippedAlreadyInvoiced { get; set; }

        /// <summary>
        /// True when the run stopped on purpose. Not an error: a refusal is the system declining
        /// to issue a document it knows to be wrong, and the reason is actionable.
        /// </summary>
        public bool Refused { get; set; }

        public string RefusalReason { get; set; } = string.Empty;

        /// <summary>Which grouping this run used: TRANSACTION, DAILY or MONTHLY (AUTO resolved
        /// per party). Reported back so the screen can show what it actually did rather than what
        /// was asked for.</summary>
        public string Consolidation { get; set; } = string.Empty;

        /// <summary>How many already-issued documents were withdrawn to re-issue this month.
        /// Zero on an ordinary run; only a regroup cancels anything.</summary>
        public int CancelledDocuments { get; set; }

        /// <summary>The parties that have no place of supply, named — the thing to go and fix.</summary>
        public List<string> Blockers { get; set; } = new();

        /// <summary>Everything else worth reading: sources that could not be invoiced, and why.</summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// The documents themselves — on a preview, what WOULD be issued, line by line; after an
        /// issue, what was. Party, what is billed, how many, the rate and the tax on each.
        /// </summary>
        public List<GeneratedDocument> Documents { get; set; } = new();

        /// <summary>True when the period is the month still in progress, invoiced up to yesterday.</summary>
        public bool PartialMonth { get; set; }
    }
}
