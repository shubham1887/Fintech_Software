namespace FintechApp.StaticModels
{
    /// <summary>
    /// The outcome of one atomic change to a user's outstanding balance.
    ///
    /// Mirrors <see cref="WalletDeltaResult"/> — and exists for the same reason. The check ("does
    /// this stay inside the credit limit?", "is there enough debt left to collect against?") and
    /// the change have to happen in ONE statement under the database's row lock, or two
    /// simultaneous credit approvals both read the same balance, both decide there is room, and
    /// the limit is quietly breached. Because the UPDATE is the only read, the opening and closing
    /// figures here are the true ones and can be written straight onto a statement row without a
    /// second, racy SELECT.
    ///
    /// It carries more than the wallet version because a rejection has to be EXPLAINED. "Not
    /// applied" could mean the limit was reached, or the collection was for more than is owed, or
    /// the user has no outstanding record at all, and those are three different things to tell
    /// the person at the counter.
    /// </summary>
    public readonly struct OutstandingDeltaResult
    {
        /// <summary>True when the balance actually changed.</summary>
        public bool Applied { get; init; }

        /// <summary>Balance before the change (meaningful only when <see cref="Applied"/>).</summary>
        public decimal OpeningBalance { get; init; }

        /// <summary>Balance after the change (meaningful only when <see cref="Applied"/>).</summary>
        public decimal ClosingBalance { get; init; }

        /// <summary>The credit limit in force at the moment of the change.</summary>
        public decimal MaxOutstanding { get; init; }

        /// <summary>Set when the change was refused because it would exceed the credit limit.</summary>
        public bool LimitBreached { get; init; }

        /// <summary>
        /// Set when a collection was refused for being larger than the amount still owed.
        ///
        /// Over-collection is blocked rather than allowed to run the balance negative, because a
        /// negative outstanding is an advance payment — a different thing entirely, which nothing
        /// downstream (the wallet tile, the reminder sweep, the report) is built to represent.
        /// </summary>
        public bool WouldGoNegative { get; init; }

        /// <summary>Set when the user has no cash-collection record to move.</summary>
        public bool RowMissing { get; init; }

        public static OutstandingDeltaResult Ok(decimal opening, decimal closing, decimal max) => new()
        {
            Applied = true,
            OpeningBalance = opening,
            ClosingBalance = closing,
            MaxOutstanding = max,
        };

        public static OutstandingDeltaResult Missing => new() { RowMissing = true };

        public static OutstandingDeltaResult OverLimit(decimal current, decimal max) => new()
        {
            LimitBreached = true,
            OpeningBalance = current,
            ClosingBalance = current,
            MaxOutstanding = max,
        };

        public static OutstandingDeltaResult Negative(decimal current, decimal max) => new()
        {
            WouldGoNegative = true,
            OpeningBalance = current,
            ClosingBalance = current,
            MaxOutstanding = max,
        };
    }
}
