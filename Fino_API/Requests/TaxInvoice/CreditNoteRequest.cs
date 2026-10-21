namespace FintechApp.Requests.TaxInvoice
{
    /// <summary>Raise a credit note against one issued document.</summary>
    public class CreditNoteRequest
    {
        public int OriginalInvoiceHeaderID { get; set; }

        /// <summary>
        /// How much of the original to reverse, at its gross value. 0 means whatever is left.
        ///
        /// Partial reversals are the normal case: a failed transaction inside a consolidated
        /// invoice reverses one source, not the whole day.
        /// </summary>
        public decimal GrossAmount { get; set; }

        /// <summary>Why. A credit note with no reason cannot be defended in an audit, and this is
        /// the field the auditor reads first.</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>Defaults to today when not set.</summary>
        public DateTime? NoteDate { get; set; }

        public string CompanyGSTIN { get; set; } = string.Empty;
        public string SupplierStateCode { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
        public bool DryRun { get; set; }
    }
}
