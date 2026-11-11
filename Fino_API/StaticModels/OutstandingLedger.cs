namespace FintechApp.StaticModels
{
    /// <summary>
    /// The vocabulary of the outstanding (cash-collection) ledger, in one place.
    ///
    /// These strings are written into <c>CashCollectionDetails.Type</c> and read back by the
    /// statement screen and the reports, so they are constants rather than literals at each call
    /// site: a typo in one of them would not fail a build, it would quietly produce a statement
    /// row that no filter matches.
    /// </summary>
    public static class OutstandingLedger
    {
        /// <summary>Marker written to the Credit column of a row that INCREASED the outstanding.</summary>
        public const string CreditMarker = "CREDIT";

        /// <summary>Marker written to the Debit column of a row that REDUCED the outstanding.</summary>
        public const string DebitMarker = "DEBIT";

        // ── Why the outstanding moved ────────────────────────────────────────────────────────
        /// <summary>A fund request approved with the Credit mode: funds given, not yet paid for.</summary>
        public const string TypeFundRequest = "FUND_REQUEST_CREDIT";

        /// <summary>A direct fund transfer sent with the Credit mode.</summary>
        public const string TypeFundTransfer = "FUND_TRANSFER_CREDIT";

        /// <summary>Cash (or a bank payment) collected against the debt — the Add button.</summary>
        public const string TypeCollection = "CASH_COLLECTION";

        /// <summary>
        /// Funds pulled back out of the downline's wallet, so the debt goes with them.
        ///
        /// A reversal is the one case where the outstanding goes DOWN without anyone paying:
        /// the upline took the money back, so there is nothing left to owe for it.
        /// </summary>
        public const string TypeFundReverse = "FUND_REVERSE_CREDIT";

        /// <summary>
        /// A collection entry undone by whoever recorded it (wrong user, wrong amount).
        ///
        /// Posted as a new, opposite row rather than by editing or deleting the original, so the
        /// statement still shows that it happened and who corrected it.
        /// </summary>
        public const string TypeCollectionReversed = "CASH_COLLECTION_REVERSE";

        /// <summary>An admin writing the figure off or correcting an opening balance.</summary>
        public const string TypeAdjustment = "OUTSTANDING_ADJUSTMENT";

        // ── Legacy types, still readable on old installs ─────────────────────────────────────
        /// <summary>What the first version of the collection screen wrote. Read-only now.</summary>
        public const string LegacyDecreased = "CASH_COLLECTION_DECREASED";

        /// <summary>
        /// The Mode master row that means "taken on credit".
        ///
        /// Modes are admin-managed data (<c>seed_mode_submode.sql</c>), so this is matched on the
        /// NAME rather than on an id: the id differs per install, and a reseller may delete and
        /// re-add the row. Matching is case-insensitive and trimmed for the same reason.
        /// </summary>
        public const string CreditModeName = "Credit";

        /// <summary>Key Settings row that switches the whole cash-collection feature on or off.</summary>
        public const string SettingKey = "CashCollection";

        // ── Message Setting event names (templates the admin can edit) ───────────────────────
        /// <summary>Raised on the downline when their outstanding increases.</summary>
        public const string EventOutstandingCredited = "Outstanding Credited";

        /// <summary>Raised on the downline when a payment reduces their outstanding.</summary>
        public const string EventOutstandingDebited = "Outstanding Debited";

        /// <summary>The daily morning reminder to everyone still carrying a balance.</summary>
        public const string EventOutstandingReminder = "Outstanding Reminder";
    }
}
