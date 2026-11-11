namespace FintechApp.StaticModels
{
    /// <summary>Where a 2B snapshot stands. Only the Active one is reconciled against.</summary>
    public static class Gstr2bImportStatus
    {
        public const string Active = "Active";
        public const string Superseded = "Superseded";
    }

    /// <summary>
    /// The four buckets a purchase-vs-2B reconciliation sorts every document into. These are the
    /// GST portal's own reconciliation categories, and the reason the comparison is four-way rather
    /// than a single pass/fail: a credit is only safe to take from the Matched bucket, and each of
    /// the other three needs a different action.
    /// </summary>
    public static class ReconBucket
    {
        /// <summary>In our books and in 2B, agreeing within tolerance. The credit is safe to claim.</summary>
        public const string Matched = "Matched";

        /// <summary>In both, but the tax or taxable value disagrees beyond tolerance. Someone filed
        /// it wrong — chase the supplier to amend, or correct our capture.</summary>
        public const string Mismatch = "Mismatch";

        /// <summary>In 2B but not in our books. We have credit the supplier declared that we never
        /// recorded a purchase for — capture it, or it is credit left on the table.</summary>
        public const string MissingInBooks = "MissingInBooks";

        /// <summary>In our books but not in 2B. We recorded a purchase the supplier has not filed —
        /// the credit cannot be taken until they do, so this is the queue to chase.</summary>
        public const string MissingIn2B = "MissingIn2B";
    }

    /// <summary>The tolerance on a tax-head comparison in the 2B match, in rupees.</summary>
    public static class ReconTolerance
    {
        /// <summary>Half a paisa over five, really a paisa either way: Ezytm derives tax from the
        /// taxable value and shows a round-off, while Erech takes total minus taxable, so the same
        /// supply lands a paisa apart on the two sides. A match must not fail on that, and a real
        /// disagreement is always larger than a paisa.</summary>
        public const decimal TaxRupees = 0.05m;
    }

    /// <summary>Where an ITC-register entry stands.</summary>
    public static class ItcStatus
    {
        /// <summary>Credit exists and may be claimed (subject to its bucket being Matched).</summary>
        public const string Eligible = "Eligible";

        /// <summary>Claimed in a return.</summary>
        public const string Claimed = "Claimed";

        /// <summary>Given back — see ReversalReason for why.</summary>
        public const string Reversed = "Reversed";

        /// <summary>No claimable credit on the document at all.</summary>
        public const string Ineligible = "Ineligible";
    }

    /// <summary>The two ITC deadlines, computed one place so capture and the register agree.</summary>
    public static class Phase3Dates
    {
        /// <summary>Document date + 180 days — pay the supplier by this or reverse the credit (Rule 37).</summary>
        public static DateTime Rule37DueDate(DateTime documentDate) => documentDate.Date.AddDays(180);

        /// <summary>
        /// 30 November of the year after the supply's financial year — the last day the credit can
        /// be claimed at all (Section 16(4)). India's FY runs April–March, so Sep-2025 and Feb-2026
        /// both sit in FY 2025-26 and share a 30-Nov-2026 deadline.
        /// </summary>
        public static DateTime Section16_4Deadline(DateTime documentDate)
        {
            int fyStartYear = documentDate.Month >= 4 ? documentDate.Year : documentDate.Year - 1;
            return new DateTime(fyStartYear + 1, 11, 30);
        }

        /// <summary>The period key (yyyyMM) a date falls in.</summary>
        public static string PeriodKey(DateTime d) => d.ToString("yyyyMM");
    }

    /// <summary>Why a claimed credit was reversed.</summary>
    public static class ItcReversalReason
    {
        /// <summary>Supplier not paid within 180 days (Rule 37).</summary>
        public const string Rule37 = "Rule37";

        /// <summary>Claim window closed before it was claimed (Section 16(4)).</summary>
        public const string Section16_4 = "Section16_4";

        public const string Manual = "Manual";
    }
}
