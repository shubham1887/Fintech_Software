namespace FintechApp.Responses.TaxInvoice
{
    /// <summary>
    /// What a generation run did, and what it refused to do.
    ///
    /// The refusals matter as much as the documents. A wallet skipped for mixed billing bases, or
    /// a party with no state code, is a configuration gap that would otherwise show up as missing
    /// turnover at the end of the quarter — so it is counted and named here rather than logged.
    /// </summary>
    public class InvoiceGenerationResult
    {
        public bool DryRun { get; set; }

        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        /// <summary>Documents created (or that would have been, on a dry run).</summary>
        public int DocumentsGenerated { get; set; }

        public int LinesGenerated { get; set; }

        public int AnnexureRowsGenerated { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public decimal GrossTotal { get; set; }

        public decimal TaxableTotal { get; set; }

        public decimal TaxTotal { get; set; }

        /// <summary>Sources passed over because they were already invoiced. Expected to be large
        /// on a re-run and zero on a first run; anything else is worth a look.</summary>
        public int SkippedAlreadyInvoiced { get; set; }

        /// <summary>Sources that could not be invoiced. Each one is named in Warnings.</summary>
        public int SkippedUnresolved { get; set; }

        public List<string> Warnings { get; set; } = new();

        public List<GeneratedDocument> Documents { get; set; } = new();
    }

    public class GeneratedDocument
    {
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public int PartyUserID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyRegistrationStatus { get; set; } = string.Empty;
        public bool IsConsolidated { get; set; }
        public int SourceCount { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal TaxableTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public string BillingBasis { get; set; } = string.Empty;
    }
}
