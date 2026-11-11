namespace FintechApp.Requests.TaxInvoice
{
    /// <summary>What to generate, and for when.</summary>
    public class InvoiceGenerationRequest
    {
        public DateTime PeriodFrom { get; set; }

        public DateTime PeriodTo { get; set; }

        /// <summary>Our registration raising the documents.</summary>
        public string CompanyGSTIN { get; set; } = string.Empty;

        /// <summary>Our state's two-digit code, for the CGST/SGST against IGST decision.</summary>
        public string SupplierStateCode { get; set; } = string.Empty;

        /// <summary>
        /// Work out everything and report it, but write nothing.
        ///
        /// Worth having for its own sake: the first run of this over a real period is the moment
        /// somebody finds out how much was never invoiced, and that number should be readable
        /// before a few thousand documents are created from it.
        /// </summary>
        public bool DryRun { get; set; }

        public int CreatedByUserID { get; set; }

        /// <summary>Limit to one party, for re-running a single case.</summary>
        public int OnlyPartyUserID { get; set; }
    }
}
