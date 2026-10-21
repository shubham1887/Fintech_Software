namespace FintechApp.Responses.TaxInvoice
{
    /// <summary>
    /// An issued document whose credit-note window is closing while it still covers a transaction
    /// that did not succeed.
    /// </summary>
    public class CreditNoteWindowAlert
    {
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public int PartyUserID { get; set; }
        public string PartyName { get; set; } = string.Empty;

        public decimal GrossTotal { get; set; }

        /// <summary>Already reversed by earlier notes.</summary>
        public decimal AlreadyCredited { get; set; }

        /// <summary>Still reversible, at gross.</summary>
        public decimal RemainingCreditable { get; set; }

        /// <summary>Gross value of the sources on this document that failed or were refunded —
        /// the part that probably needs reversing.</summary>
        public decimal FailedSourceGross { get; set; }

        public int FailedSourceCount { get; set; }

        /// <summary>30 November of the year after the supply's financial year.</summary>
        public DateTime WindowClosesOn { get; set; }

        public int DaysLeft { get; set; }

        /// <summary>True when the window has already closed: a note can still be raised, but it
        /// carries no tax, so the tax on that supply is lost.</summary>
        public bool WindowClosed { get; set; }
    }
}
