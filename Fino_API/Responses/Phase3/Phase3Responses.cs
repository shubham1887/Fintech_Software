namespace FintechApp.Responses.Phase3
{
    /// <summary>What a purchase capture (manual or imported) produced.</summary>
    public class PurchaseCaptureResult
    {
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierGSTIN { get; set; } = string.Empty;
        public decimal TaxableTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal EligibleITC { get; set; }

        /// <summary>True when this document was already captured — the capture is idempotent, so a
        /// re-send returns the existing document rather than a duplicate.</summary>
        public bool AlreadyExisted { get; set; }

        public string Warning { get; set; } = string.Empty;
    }

    /// <summary>The outcome of importing a GSTR-2B snapshot.</summary>
    public class Gstr2bImportResult
    {
        public int Gstr2bImportID { get; set; }
        public string PeriodKey { get; set; } = string.Empty;
        public int LinesImported { get; set; }
        public decimal TotalTaxable { get; set; }
        public decimal TotalTax { get; set; }

        /// <summary>True when a previous snapshot for this period was superseded by this import.</summary>
        public bool SupersededPrevious { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>One document in a reconciliation bucket. Both sides' figures are carried so a
    /// mismatch shows WHAT disagrees, not just that something does.</summary>
    public class ReconRow
    {
        public string Bucket { get; set; } = string.Empty;
        public string SupplierGSTIN { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }

        // Our books (the captured purchase), or 0 when the document is only in 2B.
        public int? TaxInvoiceHeaderID { get; set; }
        public decimal BooksTaxable { get; set; }
        public decimal BooksTax { get; set; }

        // GSTR-2B, or 0 when the document is only in our books.
        public int? Gstr2bLineID { get; set; }
        public decimal Portal2BTaxable { get; set; }
        public decimal Portal2BTax { get; set; }

        /// <summary>books − 2B on the total tax. Zero (within tolerance) in the Matched bucket.</summary>
        public decimal TaxVariance { get; set; }
        public bool ItcAvailableIn2B { get; set; }
    }

    public class ReconBucketSummary
    {
        public string Bucket { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal BooksTax { get; set; }
        public decimal Portal2BTax { get; set; }
    }

    /// <summary>Three-way: what we paid the provider (wallet top-ups) against what we captured as a
    /// purchase against what the supplier filed in 2B. A gap at any corner is a question to answer
    /// before the credit is trusted.</summary>
    public class ThreeWayRow
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public decimal WalletTopUps { get; set; }
        public decimal CapturedPurchaseGross { get; set; }
        public decimal CapturedPurchaseTax { get; set; }
        public decimal Matched2BTax { get; set; }

        /// <summary>wallet top-ups − captured purchase gross. Should be ~0; a gap means a payment
        /// with no bill, or a bill with no payment.</summary>
        public decimal WalletVsBooks { get; set; }
    }

    public class ReconResult
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
        public decimal Tolerance { get; set; }
        public bool Committed { get; set; }

        /// <summary>True when no active 2B snapshot exists for the period — every book document
        /// then falls into MissingIn2B, which is a "import the 2B first" signal, not a real gap.</summary>
        public bool No2BImported { get; set; }

        public List<ReconBucketSummary> Summary { get; set; } = new();
        public List<ReconRow> Rows { get; set; } = new();
        public List<ThreeWayRow> ThreeWay { get; set; } = new();
    }

    /// <summary>One ITC-register row, with both deadlines and where the credit stands.</summary>
    public class ItcRegisterRow
    {
        public int ItcRegisterEntryID { get; set; }
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public string SupplierGSTIN { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal EligibleITC { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ReconBucket { get; set; } = string.Empty;
        public string ClaimedInPeriod { get; set; } = string.Empty;
        public bool SupplierPaid { get; set; }
        public DateTime Rule37DueDate { get; set; }
        public DateTime Section16_4Deadline { get; set; }
    }

    public class ItcBuildResult
    {
        public string PeriodKey { get; set; } = string.Empty;
        public int EntriesCreated { get; set; }
        public int EntriesUpdated { get; set; }
        public decimal TotalEligibleITC { get; set; }
    }

    /// <summary>A credit running out of time, from either deadline. DeadlineKind says which rule
    /// bites, because the action differs: pay the supplier (Rule 37) versus claim now (16(4)).</summary>
    public class ItcDeadlineAlert
    {
        public int ItcRegisterEntryID { get; set; }
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal EligibleITC { get; set; }

        /// <summary>Rule37 or Section16_4.</summary>
        public string DeadlineKind { get; set; } = string.Empty;
        public DateTime DeadlineDate { get; set; }
        public int DaysLeft { get; set; }
        public bool Passed { get; set; }

        /// <summary>What to do: pay the supplier, or claim the credit.</summary>
        public string Action { get; set; } = string.Empty;
    }
}
