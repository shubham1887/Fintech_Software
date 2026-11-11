namespace FintechApp.Responses.Gst
{
    /// <summary>
    /// GST on charges, grouped by how the charge was quoted — which is the question the desk
    /// actually asks: "inclusive or exclusive, how much GST did we collect?"
    ///
    /// The side matters as much as the treatment, and it is decided by the SIGN of the charge:
    /// money we charged a user carries OUR output tax, while money we paid out (a commission to a
    /// retailer, a charge to a provider) carries GST that is our input credit. Adding the two
    /// together is how a GST report ends up declaring the wrong number.
    /// </summary>
    public class GstTreatmentRow
    {
        /// <summary>INCLUSIVE / EXCLUSIVE / NONE, or "NOT STAMPED" for transactions that settled
        /// before the breakdown existed.</summary>
        public string Treatment { get; set; } = string.Empty;

        /// <summary>OUTPUT or INPUT — ours to pay, or ours to claim.</summary>
        public string Kind { get; set; } = string.Empty;

        /// <summary>Where the figure came from, in words: "Charges collected from users",
        /// "Commission paid to the downline", "Charges paid to providers", …</summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>The charge without tax.</summary>
        public decimal BaseValue { get; set; }

        /// <summary>The GST on it. Always a positive magnitude here — the Kind says which way it
        /// goes, so a reader never has to interpret a minus sign.</summary>
        public decimal GSTAmount { get; set; }

        public int TransactionCount { get; set; }
    }

    /// <summary>Output tax exactly as the returns see it: per billing basis and supply nature.</summary>
    public class GstBasisRow
    {
        /// <summary>GROSS (the whole throughput is our sale) / COMMISSION / CHARGE.</summary>
        public string BillingBasis { get; set; } = string.Empty;

        /// <summary>INTRASTATE (CGST + SGST) or INTERSTATE (IGST).</summary>
        public string SupplyNature { get; set; } = string.Empty;

        public decimal GrossValue { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax { get; set; }
        public int TransactionCount { get; set; }
    }

    /// <summary>One service's whole picture, which is how an operator reads a month.</summary>
    public class GstServiceRow
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string BillingBasis { get; set; } = string.Empty;
        public string SACCode { get; set; } = string.Empty;

        public int TransactionCount { get; set; }
        public decimal Turnover { get; set; }

        /// <summary>Output tax stamped on the sale — what GSTR-1 would carry.</summary>
        public decimal OutputTaxStamped { get; set; }

        /// <summary>GST inside the charges we collected from users on this service.</summary>
        public decimal GSTOnChargesCollected { get; set; }

        /// <summary>GST inside what we paid out on this service — input credit.</summary>
        public decimal GSTOnAmountsPaid { get; set; }

        /// <summary>The treatments seen on this service, so a mixed service is visible as mixed.</summary>
        public List<string> Treatments { get; set; } = new();
    }

    /// <summary>A difference the report found, named, with what to do about it.</summary>
    public class GstReconNote
    {
        /// <summary>INFO / WARNING — a warning is a figure that is not being declared.</summary>
        public string Level { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    /// <summary>
    /// The GST collection report: what we collected, what we may claim, and what is left to pay —
    /// with the inclusive / exclusive split the desk asks for, and every difference named rather
    /// than netted away.
    /// </summary>
    public class GstCollectionReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;

        // ── The headline figures ────────────────────────────────────────────────────────────
        /// <summary>Output tax stamped on sales — the figure the returns are built from.</summary>
        public decimal OutputTaxStamped { get; set; }

        /// <summary>GST inside charges we collected from users (their surcharges and fees).</summary>
        public decimal GSTCollectedOnCharges { get; set; }

        /// <summary>GST inside commission a provider paid us — our output tax on that income.</summary>
        public decimal GSTOnProviderCommission { get; set; }

        /// <summary>GST inside what we paid providers — input credit.</summary>
        public decimal InputCreditOnProviderCharges { get; set; }

        /// <summary>GST inside commission we paid the downline — input credit.</summary>
        public decimal InputCreditOnDownlineCommission { get; set; }

        public decimal TotalOutput { get; set; }
        public decimal TotalInputCredit { get; set; }

        /// <summary>Output less input credit. What the period actually costs in GST.</summary>
        public decimal NetPayable { get; set; }

        // ── The breakdowns ──────────────────────────────────────────────────────────────────
        public List<GstTreatmentRow> ByTreatment { get; set; } = new();
        public List<GstBasisRow> ByBasis { get; set; } = new();
        public List<GstServiceRow> ByService { get; set; } = new();

        // ── Coverage, so a total is never read as complete when it is not ───────────────────
        public int SuccessCount { get; set; }
        public int ChargeStampedCount { get; set; }
        public int ChargeUnstampedCount { get; set; }
        public int TaxStampedCount { get; set; }
        public int TaxUnstampedCount { get; set; }

        public List<GstReconNote> Notes { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}
