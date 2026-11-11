namespace FintechApp.StaticModels
{
    /// <summary>
    /// The kinds of GST document this system issues, and the series each one draws its number from.
    ///
    /// Separate series per kind because GSTR-1's "Documents Issued" table reports them separately,
    /// and because mixing a credit note into the invoice sequence makes "consecutive" impossible
    /// to demonstrate for either.
    /// </summary>
    public static class TaxDocumentType
    {
        /// <summary>A taxable supply.</summary>
        public const string TaxInvoice = "TaxInvoice";

        /// <summary>Reduces an earlier invoice: a reversal, a rate correction, a discount.</summary>
        public const string CreditNote = "CreditNote";

        /// <summary>Increases an earlier invoice.</summary>
        public const string DebitNote = "DebitNote";

        /// <summary>An exempt or non-GST supply. No tax, but the turnover is still reported.</summary>
        public const string BillOfSupply = "BillOfSupply";

        /// <summary>Reverse charge: we owe the tax on somebody else's supply, so we invoice
        /// ourselves for it. ServiceMaster.HSNCodeForSelfInvoice exists for exactly this.</summary>
        public const string SelfInvoice = "SelfInvoice";
    }

    /// <summary>Series codes, as seeded by 60_seed_document_series.sql.</summary>
    public static class TaxDocumentSeriesCode
    {
        public const string SaleTaxInvoice = "SALE-TAX";
        public const string SaleBillOfSupply = "SALE-BOS";
        public const string SaleCreditNote = "SALE-CRN";
        public const string SaleDebitNote = "SALE-DBN";
        public const string SelfInvoice = "SELF-INV";

        /// <summary>The series a document kind draws from.</summary>
        public static string For(string documentType) => documentType switch
        {
            TaxDocumentType.TaxInvoice => SaleTaxInvoice,
            TaxDocumentType.BillOfSupply => SaleBillOfSupply,
            TaxDocumentType.CreditNote => SaleCreditNote,
            TaxDocumentType.DebitNote => SaleDebitNote,
            TaxDocumentType.SelfInvoice => SelfInvoice,
            _ => SaleTaxInvoice,
        };
    }

    /// <summary>Where a document stands. Only Issued counts as turnover.</summary>
    public static class TaxDocumentStatus
    {
        public const string Draft = "Draft";
        public const string Issued = "Issued";
        public const string Cancelled = "Cancelled";
    }

    /// <summary>Where a document stands with the Invoice Registration Portal.</summary>
    public static class EInvoiceStatus
    {
        /// <summary>Waiting for the IRP. The document is already valid locally.</summary>
        public const string Pending = "Pending";

        public const string Registered = "Registered";

        /// <summary>The IRP rejected it. ErrorCode and ErrorMessage say why.</summary>
        public const string Failed = "Failed";

        public const string Cancelled = "Cancelled";

        /// <summary>Below the e-invoicing threshold, or a kind the portal does not take.</summary>
        public const string NotApplicable = "NotApplicable";
    }
}
