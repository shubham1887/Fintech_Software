namespace FintechApp.Requests.TaxInvoice
{
    /// <summary>
    /// A supply we owe the tax on under reverse charge, documented by us because the supplier
    /// cannot issue a tax invoice for it.
    /// </summary>
    public class SelfInvoiceRequest
    {
        /// <summary>The supplier. They are the other party on the document even though we raise it.</summary>
        public int SupplierUserID { get; set; }

        public int ServiceID { get; set; }

        /// <summary>What the supply cost. Reverse-charge tax is normally computed ON this rather
        /// than extracted from it, because an unregistered supplier charged no tax to begin with.</summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// True only if the figure already contains tax. The default is false, which is the usual
        /// case: an unregistered supplier billed a bare amount and the tax is ours to add on top.
        /// </summary>
        public bool IsTaxInclusive { get; set; }

        public DateTime? DocumentDate { get; set; }

        /// <summary>What the supply was. Prints on the document.</summary>
        public string Description { get; set; } = string.Empty;

        public string CompanyGSTIN { get; set; } = string.Empty;
        public string SupplierStateCode { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
        public bool DryRun { get; set; }
    }
}
