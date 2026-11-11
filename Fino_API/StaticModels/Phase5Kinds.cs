namespace FintechApp.StaticModels
{
    /// <summary>Where a bank-statement import stands. Only the Active one is reconciled against —
    /// the same rule as a 2B snapshot, and for the same reason: a re-uploaded statement supersedes
    /// rather than merges, because a bank can restate a line after it clears.</summary>
    public static class BankStatementStatus
    {
        public const string Active = "Active";
        public const string Superseded = "Superseded";
    }

    /// <summary>
    /// The four buckets a bank-vs-books reconciliation sorts every movement into. Deliberately the
    /// same shape as the 2B reconciliation, because an accountant who has learnt one has learnt the
    /// other — and each bucket still means a different job.
    /// </summary>
    public static class BankReconBucket
    {
        /// <summary>In the statement and in the ledger, agreeing within tolerance. Nothing to do.</summary>
        public const string Matched = "Matched";

        /// <summary>Paired on a reference, but the amount disagrees beyond tolerance. Someone keyed
        /// a figure wrong — the entry or the payment — and one of the two has to be corrected.</summary>
        public const string Mismatch = "Mismatch";

        /// <summary>On the statement, not in the ledger. Real money moved that the books do not
        /// know about: bank charges, interest, a direct credit nobody recorded. This is the bucket
        /// that actually finds missing entries, and it is why a bank recon is done at all.</summary>
        public const string MissingInBooks = "MissingInBooks";

        /// <summary>In the ledger, not on the statement. A payment we recorded that the bank has
        /// not shown — in transit if it is recent, a mistake if it is old.</summary>
        public const string MissingInBank = "MissingInBank";
    }

    /// <summary>How a line was paired, recorded on the match so a reviewer can tell a strong match
    /// from a weak one without re-running anything.</summary>
    public static class BankMatchMethod
    {
        /// <summary>The bank's reference (UTR / cheque / RRN) was found on the voucher, and the
        /// amount agreed. The only pairing that is safe without a human looking at it.</summary>
        public const string Reference = "Reference";

        /// <summary>No reference in common; paired because exactly ONE unmatched movement of that
        /// exact amount and direction existed on each side, within the date window. Unambiguous,
        /// but circumstantial.</summary>
        public const string AmountAndDate = "AmountAndDate";

        /// <summary>Paired by a person on the screen.</summary>
        public const string Manual = "Manual";
    }

    public static class BankReconTolerance
    {
        /// <summary>Rupees. A bank moves whole paise and the ledger rounds to two places, so the
        /// only legitimate difference is rounding; anything larger is a real disagreement.</summary>
        public const decimal AmountRupees = 0.05m;

        /// <summary>Days either side a date may differ and the movement still be the same one. A
        /// NEFT posted late on Friday reaches the statement on Monday, so a same-day-only rule
        /// would report every weekend payment as missing.</summary>
        public const int DateWindowDays = 3;
    }
}
