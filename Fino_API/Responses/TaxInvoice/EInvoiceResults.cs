namespace FintechApp.Responses.TaxInvoice
{
    /// <summary>What one pass of the queue did.</summary>
    public class EInvoiceProcessResult
    {
        public int Attempted { get; set; }
        public int Registered { get; set; }

        /// <summary>Failed for a reason worth retrying — portal down, timeout, not configured.</summary>
        public int FailedRetryable { get; set; }

        /// <summary>Rejected on the document's own merits. Retrying will not help; the document
        /// has to be corrected, which after issue means a credit note and a fresh invoice.</summary>
        public int FailedPermanent { get; set; }

        /// <summary>Past the 30-day limit, so the IRP will not take it at all.</summary>
        public int PastDeadline { get; set; }

        public List<string> Messages { get; set; } = new();
    }

    /// <summary>A document still waiting for an IRN, with its deadline.</summary>
    public class EInvoiceDeadlineAlert
    {
        public int EInvoiceLogID { get; set; }
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }

        /// <summary>Document date + 30 days.</summary>
        public DateTime ReportingDeadline { get; set; }

        public int DaysLeft { get; set; }

        /// <summary>Already past it: no IRN is obtainable for this document any more.</summary>
        public bool DeadlinePassed { get; set; }

        public int AttemptCount { get; set; }
        public string LastError { get; set; } = string.Empty;
    }
}
